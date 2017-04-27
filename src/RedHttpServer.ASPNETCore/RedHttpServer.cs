using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using RedHttpServerCore.Plugins;
using RedHttpServerCore.Plugins.Interfaces;
using RedHttpServerCore.Request;
using RedHttpServerCore.Response;

namespace RedHttpServerCore
{
    /// <summary>
    /// A HTTP server based on Kestrel, with use-patterns inspired by express.js
    /// </summary>
    public class RedHttpServer
    {
        /// <summary>
        /// Build version of RedHttpServer
        /// </summary>
        public const string Version = "2.0.2";

        /// <summary>
        ///     Constructs a server instance with given port and using the given path as public folder.
        ///     Set path to null or empty string if none wanted
        /// </summary>C:\Users\Malte\Documents\GitHub\RedHttpServer.CSharp\src\RedHttpServer.ASPNETCore\RedHttpServer.cs
        /// <param name="port">The port that the server should listen on</param>
        /// <param name="publicDir">Path to use as public dir. Set to null or empty string if none wanted</param>
        public RedHttpServer(int port = 5000, string publicDir = "")
        {
            Port = port;
            PublicRoot = publicDir;
        }

        /// <summary>
        /// The plugin collection containing all plugins registered to this server instance.
        /// </summary>
        public RPluginCollection Plugins { get; } = new RPluginCollection();

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
        public RCorsPolicy RCorsPolicy { get; set; }

        /// <summary>
        ///     Starts the server
        ///     The server will handle requests for hostnames
        ///     Protocol and port will be added automatically
        /// </summary>
        /// <param name="hostnames">The host names the server is handling requests for</param>
        public void Start(params string[] hostnames)
        {
            InitializePlugins();
            var urls = hostnames.Select(url => $"http://{url}:{Port}").ToArray();
            var host = new WebHostBuilder()
                .UseKestrel()
                .ConfigureServices(s =>
                {
                    if (RCorsPolicy != null)
                        s.AddCors(options => options.AddPolicy("CorsPolicy", ConfigurePolicy));
                    s.AddRouting();
                })
                .Configure(app =>
                {
                    if (RCorsPolicy != null)
                        app.UseCors("CorsPolicy");
                    if (!string.IsNullOrWhiteSpace(PublicRoot) && Directory.Exists(PublicRoot))
                        app.UseFileServer(new FileServerOptions { FileProvider = new PhysicalFileProvider(Path.GetFullPath(PublicRoot)) });
                    if (_wsMethods.Any())
                        app.UseWebSockets();
                    app.UseRouter(SetRoutes);
                })
                .UseUrls(urls)
                .Build();
            host.Start();
            Console.WriteLine($"RedHttpServer/{Version} running on port " + Port);
        }

        private void ConfigurePolicy(CorsPolicyBuilder builder)
        {
            builder = RCorsPolicy.AllowedOrigins.All(d => d == "*")
                ? builder.AllowAnyOrigin()
                : builder.WithOrigins(RCorsPolicy.AllowedOrigins.ToArray());
            builder = RCorsPolicy.AllowedMethods.All(d => d == "*")
                ? builder.AllowAnyMethod()
                : builder.WithOrigins(RCorsPolicy.AllowedMethods.ToArray());
            if (RCorsPolicy.AllowedHeaders.All(d => d == "*"))
                builder.AllowAnyHeader();
            else
                builder.WithOrigins(RCorsPolicy.AllowedHeaders.ToArray());
        }

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
        ///     Initializes any default plugin if no other plugin is registered to same interface
        ///     <para />
        ///     Should be called after you have registered all your non-default plugins
        /// </summary>
        public void InitializePlugins(bool renderCaching = true)
        {
            if (_defPluginsReady) return;
            if (!Plugins.IsRegistered<IJsonConverter>())
                Plugins.Register<IJsonConverter, ServiceStackJsonConverter>(new ServiceStackJsonConverter());

            if (!Plugins.IsRegistered<IXmlConverter>())
                Plugins.Register<IXmlConverter, ServiceStackXmlConverter>(new ServiceStackXmlConverter());

            if (!Plugins.IsRegistered<IPageRenderer>())
                Plugins.Register<IPageRenderer, EcsPageRenderer>(new EcsPageRenderer());

            if (!Plugins.IsRegistered<IBodyParser>())
                Plugins.Register<IBodyParser, SimpleBodyParser>(new SimpleBodyParser(Plugins.Use<IJsonConverter>(),
                    Plugins.Use<IXmlConverter>()));

            if (!Plugins.IsRegistered<ILogging>())
                Plugins.Register<ILogging, NoLogging>(new NoLogging());

            Plugins.Use<IPageRenderer>().CachePages = renderCaching;
            RenderParams.Converter = Plugins.Use<IJsonConverter>();
            _defPluginsReady = true;
        }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        private void SetRoutes(IRouteBuilder rb)
        {
            var urlParam = new Regex(":[a-zA-Z0-9_-]+");
            var generalParam = new Regex("\\*{1}");
            foreach (var method in _getMethods)
                rb.MapGet(ConvertParameter(method.Item1, urlParam, generalParam),
                    async context =>
                        await method.Item2(new RRequest(context.Request, Plugins), new RResponse(context.Response, Plugins)));
            _getMethods.Clear();
            foreach (var method in _postMethods)
                rb.MapPost(ConvertParameter(method.Item1, urlParam, generalParam),
                    async context =>
                        await method.Item2(new RRequest(context.Request, Plugins), new RResponse(context.Response, Plugins)));
            _postMethods.Clear();
            foreach (var method in _putMethods)
                rb.MapPut(ConvertParameter(method.Item1, urlParam, generalParam),
                    async context =>
                        await method.Item2(new RRequest(context.Request, Plugins), new RResponse(context.Response, Plugins)));
            _putMethods.Clear();
            foreach (var method in _deleteMethods)
                rb.MapDelete(ConvertParameter(method.Item1, urlParam, generalParam),
                    async context =>
                        await method.Item2(new RRequest(context.Request, Plugins), new RResponse(context.Response, Plugins)));
            _deleteMethods.Clear();
            foreach (var wsMethod in _wsMethods)
                rb.MapGet(ConvertParameter(wsMethod.Item1, urlParam, generalParam), async context =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var wsd = new WebSocketDialog(context, webSocket, Plugins);
                        wsMethod.Item2(new RRequest(context.Request, Plugins), wsd);
                        await wsd.ReadFromWebSocket();
                    }
                    else
                        context.Response.StatusCode = 400;
                });
            _wsMethods.Clear();
        }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        /// <summary>
        ///     Converts from url parameter format used in previous versions, to one supported by ASP.NET Core
        /// </summary>
        /// <param name="parameter">original parameter</param>
        /// <param name="urlParam"></param>
        /// <param name="generalParam"></param>
        private static string ConvertParameter(string parameter, Regex urlParam, Regex generalParam)
        {
            parameter = parameter.Trim('/');
            if (parameter.Contains("*"))
                parameter = generalParam.Replace(parameter, "{*any}");
            if (parameter.Contains(":"))
                parameter = urlParam.Replace(parameter, match => "{" + match.Value.TrimStart(':') + "}");
            return parameter;
        }

        #region Adding route handlers

        /// <summary>
        ///     Add action to handle GET requests to a given route
        ///     <para />
        ///     Should always be idempotent.
        ///     (Receiving the same GET request one or multiple times should yield same result)
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Get(string route, Func<RRequest, RResponse, Task> action)
            => _getMethods.Add(new Tuple<string, Func<RRequest, RResponse, Task>>(route, action));

        /// <summary>
        ///     Add action to handle POST requests to a given route
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Post(string route, Func<RRequest, RResponse, Task> action)
            => _postMethods.Add(new Tuple<string, Func<RRequest, RResponse, Task>>(route, action));

        /// <summary>
        ///     Add action to handle PUT requests to a given route.
        ///     <para />
        ///     Should always be idempotent.
        ///     (Receiving the same PUT request one or multiple times should yield same result)
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Put(string route, Func<RRequest, RResponse, Task> action)
            => _putMethods.Add(new Tuple<string, Func<RRequest, RResponse, Task>>(route, action));

        /// <summary>
        ///     Add action to handle DELETE requests to a given route.
        ///     <para />
        ///     Should always be idempotent.
        ///     (Receiving the same DELETE request one or multiple times should yield same result)
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Delete(string route, Func<RRequest, RResponse, Task> action)
            => _deleteMethods.Add(new Tuple<string, Func<RRequest, RResponse, Task>>(route, action));

        /// <summary>
        ///     Add action to handle WEBSOCKET requests to a given route. <para/>
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void WebSocket(string route, Action<RRequest, WebSocketDialog> action)
            => _wsMethods.Add(new Tuple<string, Action<RRequest, WebSocketDialog>>(route, action));


        private readonly List<Tuple<string, Func<RRequest, RResponse, Task>>> _deleteMethods =
            new List<Tuple<string, Func<RRequest, RResponse, Task>>>();

        private readonly List<Tuple<string, Func<RRequest, RResponse, Task>>> _getMethods =
            new List<Tuple<string, Func<RRequest, RResponse, Task>>>();

        private readonly List<Tuple<string, Func<RRequest, RResponse, Task>>> _postMethods =
            new List<Tuple<string, Func<RRequest, RResponse, Task>>>();

        private readonly List<Tuple<string, Func<RRequest, RResponse, Task>>> _putMethods =
            new List<Tuple<string, Func<RRequest, RResponse, Task>>>();

        private readonly List<Tuple<string, Action<RRequest, WebSocketDialog>>> _wsMethods =
            new List<Tuple<string, Action<RRequest, WebSocketDialog>>>();

        private bool _defPluginsReady;

        #endregion
    }
}