using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Red.Extensions;
using Red.Interfaces;

namespace Red
{
    /// <summary>
    /// An Http server based on ASP.NET Core with Kestrel, with use-patterns inspired by express.js
    /// </summary>
    public partial class RedHttpServer
    {
        
        /// <summary>
        ///     Constructs a server instance with given port and using the given path as public folder.
        ///     Set path to null or empty string if none wanted
        /// </summary>
        /// <param name="port">The port that the server should listen on</param>
        /// <param name="publicDir">Path to use as public dir. Set to null or empty string if none wanted</param>
        public RedHttpServer(int port = 5000, string publicDir = "")
        {
            Port = port;
            _publicRoot = publicDir;
            
            Use(new NewtonsoftJsonConverter());
            Use(new XmlConverter());
            Use(new BodyParser());
        }

        /// <summary>
        /// The plugin collection containing all plugins registered to this server instance.
        /// </summary>
        public PluginCollection Plugins { get; } = new PluginCollection();

        /// <summary>
        /// The version of the library
        /// </summary>
        public static string Version { get; } = typeof(RedHttpServer).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

        /// <summary>
        /// Whether details about an exception should be sent together with the code 500 response. For debugging
        /// </summary>
        public bool RespondWithExceptionDetails { get; set; } = false;
        /// <summary>
        ///     The port that the server is listening on
        /// </summary>
        public int Port { get; }

        /// <summary>
        ///     Cross-Origin Resource Sharing (CORS) policy
        /// </summary>
        public CorsPolicy CorsPolicy { get; set; }

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
                    if (!string.IsNullOrWhiteSpace(_publicRoot) && Directory.Exists(_publicRoot))
                        app.UseFileServer(new FileServerOptions { FileProvider = new PhysicalFileProvider(Path.GetFullPath(_publicRoot)) });
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
        public Action<IServiceCollection> ConfigureServices { private get; set; }

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
        ///     Register extension modules and middleware
        /// </summary>
        /// <param name="extension"></param>
        public void Use(IRedExtension extension)
        {
            if (extension is IRedWebSocketMiddleware wsMiddleware)
                _wsMiddlewareStack.Add(wsMiddleware);
            if (extension is IRedMiddleware middleware)
                _middlewareStack.Add(middleware);
            _plugins.Add(extension);
        }


        
        /// <summary>
        ///     Add action to handle GET requests to a given route
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="handlers">The handlers that wil respond to the request</param>
        public void Get(string route, params Func<Request, Response, Task>[] handlers)
        {
            AddHandlers(route, GetMethod, handlers);
        }

        /// <summary>
        ///     Add action to handle POST requests to a given route
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="handlers">The handlers that wil respond to the request</param>
        public void Post(string route, params Func<Request, Response, Task>[] handlers)
        {
            AddHandlers(route, PostMethod, handlers);
        }


        /// <summary>
        ///     Add action to handle PUT requests to a given route.
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="handlers">The handlers that wil respond to the request</param>
        public void Put(string route, params Func<Request, Response, Task>[] handlers)
        {
            AddHandlers(route, PutMethod, handlers);
        }


        /// <summary>
        ///     Add action to handle DELETE requests to a given route.
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="handlers">The handlers that wil respond to the request</param>
        public void Delete(string route, params Func<Request, Response, Task>[] handlers)
        {
            AddHandlers(route, DeleteMethod, handlers);
        }
        
        /// <summary>
        ///     Add action to handle WEBSOCKET requests to a given route. <para/>
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="handlers">The handlers that wil respond to the request</param>
        public void WebSocket(string route, params Func<Request, Response, WebSocketDialog, Task>[] handlers)
        {
            if (handlers.Length == 0)
                throw new RedHttpServerException("A route requires at least one handler");
            
            if (_wsHandlers == null)
                throw new RedHttpServerException("Cannot add route handlers after server is started");
            
            _wsHandlers.Add(new WsHandlerWrapper(route, handlers));
        }
        /// <summary>
        ///     Add action to handle WEBSOCKET requests to a given route. <para/>
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="handlers">The handlers that wil respond to the request</param>
        public void WebSocket(string route, params Func<Request, WebSocketDialog, Task>[] handlers)
        {
            if (handlers.Length == 0)
                throw new RedHttpServerException("A route requires at least one handler");
            
            if (_wsHandlers == null)
                throw new RedHttpServerException("Cannot add route handlers after server is started");
            
            _wsHandlers.Add(new WsHandlerWrapper(route, handlers));
        }
    }
}