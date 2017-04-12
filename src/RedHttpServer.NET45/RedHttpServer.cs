using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RedHttpServer.Handling;
using RedHttpServer.Plugins;
using RedHttpServer.Plugins.Interfaces;
using RedHttpServer.Request;
using RedHttpServer.Response;
using ILogger = RedHttpServer.Plugins.Interfaces.ILogger;

namespace RedHttpServer
{
    /// <summary>
    /// A HTTP server based on HttpListener, with use-patterns inspired by express.js
    /// </summary>
    public sealed class RedHttpServer : IDisposable
    {
        private static readonly RequestParams EmptyReqParams = new RequestParams(new Dictionary<string, string>());
        internal static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        ///     Constructs a server instance with given port and using the given path as public folder.
        ///     Set path to null or empty string if none wanted
        /// </summary>
        /// <param name="path">Path to use as public dir. Set to null or empty string if none wanted</param>
        /// <param name="port">The port that the server should listen on</param>
        public RedHttpServer(int port = 5000, string path = "")
        {
            PublicRoot = path;
            _publicFiles = !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
            Port = port;
            _stopEvent = new ManualResetEventSlim(false);
            _listener = new HttpListener();
            _listenerThread = new Thread(HandleRequests) {Name = "ListenerThread"};
        }

        private readonly CorsHandler _cors = new CorsHandler();
        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private readonly bool _publicFiles;
        private readonly RouteTreeManager _rtman = new RouteTreeManager();
        private readonly ManualResetEventSlim _stopEvent;
        private bool _defPluginsReady;
        private ResponseHandler _resHandler;

        /// <summary>
        ///     The publicly available folder
        /// </summary>
        public string PublicRoot { get; }
        
        /// <summary>
        ///     The port that the server is listening on
        /// </summary>
        public int Port { get; }
        
        /// <summary>
        /// The plugin collection containing all plugins registered to this server instance.
        /// </summary>
        public RPluginCollection Plugins { get; } = new RPluginCollection();
        
        #region Adding route handlers

        /// <summary>
        ///     Add action to handle GET requests to a given route
        ///     <para />
        ///     Should always be idempotent.
        ///     (Receiving the same GET request one or multiple times should yield same result)
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Get(string route, Action<RRequest, RResponse> action)
        {
            _rtman.AddRoute(new RHttpAction(route, action), HttpMethod.GET);
        }


        /// <summary>
        ///     Add action to handle POST requests to a given route
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Post(string route, Action<RRequest, RResponse> action)
        {
            _rtman.AddRoute(new RHttpAction(route, action), HttpMethod.POST);
            _cors.Register(route, "POST");
        }

        /// <summary>
        ///     Add action to handle PUT requests to a given route.
        ///     <para />
        ///     Should always be idempotent.
        ///     (Receiving the same PUT request one or multiple times should yield same result)
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Put(string route, Action<RRequest, RResponse> action)
        {
            _rtman.AddRoute(new RHttpAction(route, action), HttpMethod.PUT);
            _cors.Register(route, "PUT");
        }

        /// <summary>
        ///     Add action to handle DELETE requests to a given route.
        ///     <para />
        ///     Should always be idempotent.
        ///     (Receiving the same DELETE request one or multiple times should yield same result)
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Delete(string route, Action<RRequest, RResponse> action)
        {
            _rtman.AddRoute(new RHttpAction(route, action), HttpMethod.DELETE);
            _cors.Register(route, "DELETE");
        }

        /// <summary>
        ///     Add action to handle OPTIONS requests to a given route
        ///     You should respond only using headers.
        ///     <para />
        ///     Should always be idempotent.
        ///     (Receiving the same HEAD request one or multiple times should yield same result)
        ///     <para />
        ///     Should contain one header with the id "Allow", and the content should contain the HTTP methods the route
        ///     allows.
        ///     <para />
        ///     (f.x. "Allow": "GET, POST, OPTIONS")
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        internal void Options(string route, Action<RRequest, RResponse> action)
            => _rtman.AddRoute(new RHttpAction(route, action), HttpMethod.OPTIONS);

        /// <summary>
        ///     Add action to handle WEBSOCKET requests to a given route. <para/>
        ///     Please beware that this relies on HttpListenerWebSocketContext, which is not implemented in Mono
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the requestsud</param>
        /// <param name="protocol"></param>
        public void WebSocket(string route, Action<RRequest, WebSocketDialog> action, string protocol = null)
        {
            _rtman.AddRoute(new RHttpAction(route, action, protocol), HttpMethod.WEBSOCKET);
        }

        #endregion

        /// <summary>
        ///     Starts the server
        ///     <para />
        /// </summary>
        /// <param name="localOnly">Whether to only listn locally</param>
        public void Start(bool localOnly = false)
        {
            Start(localOnly ? "localhost" : "+");
        }

        /// <summary>
        ///     Starts the server, and all request handling threads
        ///     <para />
        ///     Only answers to requests with specified prefixes
        ///     <para />
        ///     Specify in following format:
        ///     <para />
        ///     "+" , "*" , "localhost" , "example.com", "123.12.34.56"
        ///     <para />
        /// </summary>
        /// <param name="listeningPrefixes">The prefixes the server will listen for requests with</param>
        public void Start(params string[] listeningPrefixes)
        {
            try
            {
                if (!listeningPrefixes.Any())
                {
                    Console.WriteLine(
                        "You must listen for either http or https (or both) requests for the server to do anything");
                    return;
                }
                InitializePlugins();
                _cors.Bind(CorsPolicy, _rtman);
                foreach (var listeningPrefix in listeningPrefixes)
                {
                    _listener.Prefixes.Add($"http://{listeningPrefix}:{Port}/");
                }
                _listener.Start();
                _listenerThread.Start();

                Console.WriteLine("RHttpServer v. {0} started", Version);
                if (listeningPrefixes.All(a => a == "localhost"))
                    Plugins.Use<ILogger>().Log("Server visibility", "Listening on localhost only");
            }
            catch (SocketException)
            {
                Console.WriteLine("Unable to bind to port, the port may already be in use.");
                Environment.Exit(0);
            }
            catch (HttpListenerException)
            {
                Console.WriteLine("Could not obtain permission to listen for '{0}' on selected port\n" +
                                  "Please aquire the permission or start the server as local-only",
                    string.Join(", ", listeningPrefixes));
                Environment.Exit(0);
            }
        }
        

        /// <summary>
        ///     Cross-Origin Resource Sharing (CORS) policy
        /// </summary>
        public CorsPolicy CorsPolicy { get; set; }

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
            
            if (!Plugins.IsRegistered<IBodyParser>())
                Plugins.Register<IBodyParser, SimpleBodyParser>(new SimpleBodyParser(Plugins.Use<IJsonConverter>(), Plugins.Use<IXmlConverter>()));
            
            if (!Plugins.IsRegistered<IPageRenderer>())
                Plugins.Register<IPageRenderer, EcsPageRenderer>(new EcsPageRenderer());

            if (!Plugins.IsRegistered<ILogger>())
                Plugins.Register<ILogger, NoLogger>(new NoLogger());

            Plugins.Use<IPageRenderer>().CachePages = renderCaching;
            RenderParams.Converter = Plugins.Use<IJsonConverter>();
            _defPluginsReady = true;
            if (_publicFiles)
                _resHandler = new PublicFileRequestHander(PublicRoot);
            else
                _resHandler = new ActionOnlyResponseHandler();
        }

        /// <summary>
        ///     Stops the server thread and all request handling threads.
        /// </summary>
        public void Stop()
        {
            _stopEvent.Set();
            _listenerThread.Join();
            _listener.Stop();
        }

        private void HandleRequests()
        {
            while (_listener.IsListening)
            {
                try
                {
                    var context = _listener.BeginGetContext(ContextReady, null);
                    if (0 == WaitHandle.WaitAny(new[] {_stopEvent.WaitHandle, context.AsyncWaitHandle}))
                        return;
                }
                catch (Exception ex)
                {
                    Plugins.Use<ILogger>().Log(ex);
                }}
        }

        private void ContextReady(IAsyncResult ar)
        {
            Task.Run(() =>
            {
                Process(_listener.EndGetContext(ar));
            });
        }
        
        private async void Process(HttpListenerContext context)
        {
            var route = context.Request.Url.AbsolutePath.Trim('/');
            var hm = GetMethod(context.Request.HttpMethod);
            if (hm == HttpMethod.UNSUPPORTED)
            {
                Plugins.Use<ILogger>().Log("Unsupported HTTP method",
                    $"{context.Request.HttpMethod} from {context.Request.RemoteEndPoint}");
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }

            if (context.Request.IsWebSocketRequest)
            {
                var wsact = _rtman.SearchInTree(route, HttpMethod.WEBSOCKET, out bool gf);
                if (wsact != null)
                {
                    await PerformWsAction(context, GetParams(wsact, route), Plugins, wsact);
                    return;
                }
            }

            var act = _rtman.SearchInTree(route, hm, out bool generalFallback);

            if (generalFallback && _resHandler.Handle(route, context))
                return;
            if (act != null)
            {
                PerformAction(context, GetParams(act, route), Plugins, act);
            }
            else
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
            }
        }

        private static async Task PerformWsAction(HttpListenerContext context, RequestParams reqPar,
            RPluginCollection plugins, RHttpAction wsact)
        {
            var wsc = await context.AcceptWebSocketAsync(wsact.WSProtocol);
            var wsd = new WebSocketDialog(wsc);
            wsact.WSAction(new RRequest(context.Request, reqPar, plugins), wsd);
            await wsd.ReadFromWebSocket();
        }

        private static HttpMethod GetMethod(string input)
        {
            switch (input)
            {
                case "GET":
                    return HttpMethod.GET;
                case "POST":
                    return HttpMethod.POST;
                case "PUT":
                    return HttpMethod.PUT;
                case "DELETE":
                    return HttpMethod.DELETE;
                case "OPTIONS":
                    return HttpMethod.OPTIONS;
                default:
                    return HttpMethod.UNSUPPORTED;
            }
        }

        private static void PerformAction(HttpListenerContext context, RequestParams reqPar, RPluginCollection plugins,
            RHttpAction act)
        {
            var req = new RRequest(context.Request, reqPar, plugins);
            var res = new RResponse(context.Response, plugins);
            act.Action(req, res);
        }


        private static RequestParams GetParams(RHttpAction act, string route)
        {
            if (!act.Params.Any()) return EmptyReqParams;
            var rt = route.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            var dict = act.Params
                .Where(kvp => kvp.Key < rt.Length)
                .ToDictionary(kvp => kvp.Value, kvp => rt[kvp.Key]);
            return new RequestParams(dict);
        }

        /// <summary>
        /// Makes sure the server is correctly stopped when this intance is disposed
        /// </summary>
        public void Dispose()
        {
            Stop();
            _stopEvent.Dispose();
            _listener.Close();
        }
    }
}