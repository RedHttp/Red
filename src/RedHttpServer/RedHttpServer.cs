using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Red.Extensions;
using Red.Interfaces;

namespace Red
{
    /// <summary>
    /// A HTTP server based on Kestrel, with use-patterns inspired by express.js
    /// </summary>
    public partial class RedHttpServer
    {
        /// <summary>
        /// Build version of RedHttpServer
        /// </summary>
        public const string Version = "3.0.0";

        /// <summary>
        ///     Constructs a server instance with given port and using the given path as public folder.
        ///     Set path to null or empty string if none wanted
        /// </summary>
        /// <param name="port">The port that the server should listen on</param>
        /// <param name="publicDir">Path to use as public dir. Set to null or empty string if none wanted</param>
        public RedHttpServer(int port = 5000, string publicDir = "")
        {
            Port = port;
            PublicRoot = publicDir;
            
            Use(new NewtonsoftJsonConverter());
            Use(new InbuiltXmlConverter());
            Use(new BodyParser());
        }

        /// <summary>
        /// The plugin collection containing all plugins registered to this server instance.
        /// </summary>
        public PluginCollection Plugins { get; } = new PluginCollection();

        /// <summary>
        ///     The port that the server is listening on
        /// </summary>
        public int Port { get; }

        /// <summary>
        ///     The publicly available folder root
        /// </summary>
        private string PublicRoot { get; }

        /// <summary>
        ///     Cross-Origin Resource Sharing (CORS) policy
        /// </summary>
        public RCorsPolicy CorsPolicy { get; set; }

        /// <summary>
        ///     Starts the server
        ///     The server will handle requests for hostnames
        ///     Protocol and port will be added automatically
        /// </summary>
        /// <param name="hostnames">The host names the server is handling requests for</param>
        public void Start(params string[] hostnames)
        {
            Initialize();
            var urls = hostnames.Select(url => $"http://{url}:{Port}").ToArray();
            var host = new WebHostBuilder()
                .UseKestrel()
                .ConfigureServices(s =>
                {
                    if (CorsPolicy != null)
                        s.AddCors(options => options.AddPolicy("CorsPolicy", ConfigurePolicy));
                    s.AddRouting();
                    ConfigureServices?.Invoke(s);
                })
                .Configure(app =>
                {
                    if (CorsPolicy != null)
                        app.UseCors("CorsPolicy");
                    if (!string.IsNullOrWhiteSpace(PublicRoot) && Directory.Exists(PublicRoot))
                        app.UseFileServer(new FileServerOptions { FileProvider = new PhysicalFileProvider(Path.GetFullPath(PublicRoot)) });
                    if (_wsHandlers.Any())
                        app.UseWebSockets();
                    app.UseRouter(SetRoutes);
                })
                .UseUrls(urls)
                .Build();
            host.Start();
            Console.WriteLine($"RedHttpServer/{Version} running on port " + Port);
        }

        /// <summary>
        ///     Method to register ASP.NET Core Services, such as DbContext etc.
        /// </summary>
        public Action<IServiceCollection> ConfigureServices { get; set; }

        /// <summary>
        ///     Starts the server
        ///     <para />
        /// </summary>
        /// <param name="localOnly">Whether to only listn locally</param>
        public void Start(bool localOnly = false)
        {
            Start(localOnly ? "localhost" : "*");
        }

        /// <summary>
        ///     Register middleware module
        /// </summary>
        /// <param name="middleware"></param>
        public void Use(IRedMiddleware middleware)
        {
            _middlewareStack.Add(middleware);
        }
        /// <summary>
        ///     Register extension module
        /// </summary>
        /// <param name="extension"></param>
        public void Use(IRedExtension extension)
        {
            _plugins.Add(extension);
        }
        

        /// <summary>
        ///     Add action to handle GET requests to a given route
        ///     <para />
        ///     Should always be idempotent.
        ///     (Receiving the same GET request one or multiple times should yield same result)
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Get(string route, Func<Request, Response, Task> action)
        {
            if (_handlers == null)
                throw new RedHttpServerException("Cannot add route handlers after server is started");
            _handlers.Add(new HandlerWrapper(route, HttpMethodEnum.GET, action));
        }

        /// <summary>
        ///     Add action to handle POST requests to a given route
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Post(string route, Func<Request, Response, Task> action)
        {
            if (_handlers == null)
                throw new RedHttpServerException("Cannot add route handlers after server is started");
            _handlers.Add(new HandlerWrapper(route, HttpMethodEnum.POST, action));
        }


        /// <summary>
        ///     Add action to handle PUT requests to a given route.
        ///     <para />
        ///     Should always be idempotent.
        ///     (Receiving the same PUT request one or multiple times should yield same result)
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Put(string route, Func<Request, Response, Task> action)
        {
            if (_handlers == null)
                throw new RedHttpServerException("Cannot add route handlers after server is started");
            _handlers.Add(new HandlerWrapper(route, HttpMethodEnum.PUT, action));
        }


        /// <summary>
        ///     Add action to handle DELETE requests to a given route.
        ///     <para />
        ///     Should always be idempotent.
        ///     (Receiving the same DELETE request one or multiple times should yield same result)
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Delete(string route, Func<Request, Response, Task> action)
        {
            if (_handlers == null)
                throw new RedHttpServerException("Cannot add route handlers after server is started");
            _handlers.Add(new HandlerWrapper(route, HttpMethodEnum.DELETE, action));
        }


        /// <summary>
        ///     Add action to handle WEBSOCKET requests to a given route. <para/>
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void WebSocket(string route, Action<Request, WebSocketDialog> action)
        {
            if (_wsHandlers == null)
                throw new RedHttpServerException("Cannot add route handlers after server is started");
            _wsHandlers.Add(new Tuple<string, Action<Request, WebSocketDialog>>(route, action));
        }
    }
}