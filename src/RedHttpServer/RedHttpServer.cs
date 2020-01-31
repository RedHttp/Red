using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Red.Extensions;
using Red.Interfaces;

namespace Red
{
    /// <summary>
    /// An Http server based on ASP.NET Core with Kestrel, with use-patterns inspired by express.js
    /// </summary>
    public partial class RedHttpServer : IRouter
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
            
            Use(new JsonConverter());
            Use(new XmlConverter());
            Use(new BodyParser());
        }

        /// <summary>
        /// The plugin collection containing all plugins registered to this server instance.
        /// </summary>
        public readonly PluginCollection Plugins = new PluginCollection();

        /// <summary>
        /// The version of the library
        /// </summary>
        public static readonly string Version = typeof(RedHttpServer).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

        /// <summary>
        /// Whether details about an exception should be sent together with the code 500 response. For debugging
        /// </summary>
        public bool RespondWithExceptionDetails = false;

        /// <summary>
        ///    Event that is raised when an exception is thrown from a handler
        /// </summary>
        public event EventHandler<HandlerExceptionEventArgs>? OnHandlerException; 

        /// <summary>
        ///     The port that the server is listening on
        /// </summary>
        public readonly int Port;

        /// <summary>
        ///     Starts the server.
        ///     If no hostnames are provided, localhost will be used.
        /// </summary>
        /// <param name="hostnames">The host names the server is handling requests for. Protocol and port will be added automatically</param>
        public void Start(params string[] hostnames)
        {
            _host = Build(hostnames);
            _host.Start();
            Console.WriteLine($"Red/{Version} running on port " + Port);
        }
        
        /// <summary>
        ///     Attempts to stop the running server using IWebHost.StopAsync
        /// </summary>
        public Task? StopAsync()
        {
            return _host?.StopAsync();
        }

        /// <summary>
        ///     Run the server using IWebHost.RunAsync and return the task so it can be awaited.
        ///     If no hostnames are provided, localhost will be used.
        /// </summary>
        /// <param name="hostnames">The host names the server is handling requests for. Protocol and port will be added automatically</param>
        /// <returns></returns>
        public Task RunAsync(params string[] hostnames)
        {
            _host = Build(hostnames);
            Console.WriteLine($"Starting Red/{Version} on port " + Port);
            return _host.StartAsync();
        }
        
        /// <summary>
        ///     Method to register additional ASP.NET Core Services
        /// </summary>
        public Action<IServiceCollection>? ConfigureServices { private get; set; }
        
        /// <summary>
        ///     Method to configure the ASP.NET Core application additionally
        /// </summary>
        public Action<IApplicationBuilder>? ConfigureApplication { private get; set; }

        /// <summary>
        ///     Register extension modules and middleware
        /// </summary>
        /// <param name="extension"></param>
        public void Use(IRedExtension extension)
        {
            if (extension is IRedWebSocketMiddleware wsMiddleware)
                _wsMiddle.Add(wsMiddleware.Invoke);
            if (extension is IRedMiddleware middleware)
                _middle.Add(middleware.Invoke);
            _plugins.Add(extension);
        }

        private List<Func<Request, Response, WebSocketDialog, Task<HandlerType>>> _wsMiddle = new List<Func<Request, Response, WebSocketDialog, Task<HandlerType>>>();
        private List<Func<Request, Response, Task<HandlerType>>> _middle = new List<Func<Request, Response, Task<HandlerType>>>();
        /// <inheritdoc />
        public IRouter CreateRouter(string baseRoute)
        {
            return new Router(baseRoute, this);
        }

        /// <inheritdoc />
        public void Get(string route, params Func<Request, Response, Task<HandlerType>>[] handlers)
        {
            AddHandlers(route, GetMethod, handlers);
        }
        /// <inheritdoc />
        public void Post(string route, params Func<Request, Response, Task<HandlerType>>[] handlers)
        {
            AddHandlers(route, PostMethod, handlers);
        }
        /// <inheritdoc />
        public void Put(string route, params Func<Request, Response, Task<HandlerType>>[] handlers)
        {
            AddHandlers(route, PutMethod, handlers);
        }
        /// <inheritdoc />
        public void Delete(string route, params Func<Request, Response, Task<HandlerType>>[] handlers)
        {
            AddHandlers(route, DeleteMethod, handlers);
        }
        
        /// <inheritdoc />
        public void WebSocket(string route, params Func<Request, Response, WebSocketDialog, Task<HandlerType>>[] handlers)
        {
            _useWebSockets = true;
            AddHandlers(route, handlers);
        }
    }
}