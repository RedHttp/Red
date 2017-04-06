using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using RHttpServer.Default;
using RHttpServer.Logging;

namespace RHttpServer
{
    /// <summary>
    ///     The base for the http servers
    /// </summary>
    public abstract class BaseHttpServer : IDisposable
    {
        private static readonly RequestParams EmptyReqParams = new RequestParams(new Dictionary<string, string>());
        internal static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        internal static bool ThrowExceptions;

        /// <summary>
        ///     Constructs a server instance with given port and using the given path as public folder.
        ///     Set path to null or empty string if none wanted
        /// </summary>
        /// <param name="path">Path to use as public dir. Set to null or empty string if none wanted</param>
        /// <param name="port">The port that the server should listen on</param>
        /// <param name="throwExceptions">Whether exceptions should be suppressed and logged, or thrown (for debugging)</param>
        protected BaseHttpServer(int port, string path, bool throwExceptions)
        {
            ThrowExceptions = throwExceptions;
            PublicDir = path;
            _publicFiles = !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
            Port = port;
            StopEvent = new ManualResetEventSlim(false);
            _listener = new HttpListener();
            _listenerThread = new Thread(HandleRequests) {Name = "ListenerThread"};
        }

        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private readonly bool _publicFiles;
        private readonly RPluginCollection _rPluginCollection = new RPluginCollection();
        private readonly RouteTreeManager _rtman = new RouteTreeManager();
        protected readonly ManualResetEventSlim StopEvent;
        protected IHttpSecurityHandler SecMan;
        private IFileCacheManager _cacheMan;
        private bool _defPluginsReady;
        private ResponseHandler _resHandler;
        private bool _securityOn;

        /// <summary>
        ///     The publicly available folder
        /// </summary>
        public string PublicDir { get; }

        /// <summary>
        ///     Whether public files should be cached if size and extension is set to be cached
        /// </summary>
        public bool CachePublicFiles { get; set; }

        /// <summary>
        ///     The port that the server is listening on
        /// </summary>
        public int Port { get; }

        /// <summary>
        ///     Whether security is turned on
        /// </summary>
        public bool SecurityOn
        {
            get { return _securityOn; }
            set
            {
                if (_securityOn == value) return;
                _securityOn = value;
                if (_securityOn)
                    _rPluginCollection.Use<IHttpSecurityHandler>()?.Start();
                else
                    _rPluginCollection.Use<IHttpSecurityHandler>()?.Stop();
            }
        }


        /// <summary>
        ///     Whether the server should respond to http requests
        ///     <para />
        ///     Defaults to true
        /// </summary>
        public bool HttpEnabled { get; set; } = true;

        /// <summary>
        ///     Whether the server should respond to https requests
        ///     <para />
        ///     You must have a (ssl) certificate setup to the specified port for it to respond.
        /// </summary>
        public bool HttpsEnabled { get; set; }

        /// <summary>
        ///     The port that https requests are handled through, if enabled
        /// </summary>
        public int HttpsPort { get; set; } = 5443;

        /// <summary>
        ///     Register a plugin to be used in the server.
        ///     <para />
        ///     You can replace the default plugins by registering your plugin using the same interface as key before starting the
        ///     server
        /// </summary>
        /// <typeparam name="TPluginInterface">The type the plugin implements</typeparam>
        /// <typeparam name="TPlugin">The type of the plugin instance</typeparam>
        /// <param name="plugin">The instance of the plugin that will be registered</param>
        public void RegisterPlugin<TPluginInterface, TPlugin>(TPlugin plugin)
            where TPlugin : RPlugin, TPluginInterface
        {
            plugin.SetPlugins(_rPluginCollection);
            _rPluginCollection.Add(typeof(TPluginInterface), plugin);
        }

        /// <summary>
        ///     Add action to handle GET requests to a given route
        ///     <para />
        ///     Should always be idempotent.
        ///     (Receiving the same GET request one or multiple times should yield same result)
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Get(string route, Action<RRequest, RResponse> action)
            => _rtman.AddRoute(new RHttpAction(route, action), HttpMethod.GET);

        /// <summary>
        ///     Add action to handle POST requests to a given route
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Post(string route, Action<RRequest, RResponse> action)
            => _rtman.AddRoute(new RHttpAction(route, action), HttpMethod.POST);

        /// <summary>
        ///     Add action to handle PUT requests to a given route.
        ///     <para />
        ///     Should always be idempotent.
        ///     (Receiving the same PUT request one or multiple times should yield same result)
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Put(string route, Action<RRequest, RResponse> action)
            => _rtman.AddRoute(new RHttpAction(route, action), HttpMethod.PUT);

        /// <summary>
        ///     Add action to handle DELETE requests to a given route.
        ///     <para />
        ///     Should always be idempotent.
        ///     (Receiving the same DELETE request one or multiple times should yield same result)
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Delete(string route, Action<RRequest, RResponse> action)
            => _rtman.AddRoute(new RHttpAction(route, action), HttpMethod.DELETE);

        /// <summary>
        ///     Add action to handle HEAD requests to a given route
        ///     You should only send the headers of the route as response
        ///     <para />
        ///     Should always be idempotent.
        ///     (Receiving the same HEAD request one or multiple times should yield same result)
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="action">The action that wil respond to the request</param>
        public void Head(string route, Action<RRequest, RResponse> action)
            => _rtman.AddRoute(new RHttpAction(route, action), HttpMethod.HEAD);

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
        public void Options(string route, Action<RRequest, RResponse> action)
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
        ///     Protocol and port will be added automatically
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
                InitializeDefaultPlugins();
                foreach (var listeningPrefix in listeningPrefixes)
                {
                    if (HttpEnabled) _listener.Prefixes.Add($"http://{listeningPrefix}:{Port}/");
                    if (HttpsEnabled) _listener.Prefixes.Add($"https://{listeningPrefix}:{HttpsPort}/");
                }
                _listener.Start();
                _listenerThread.Start();
                OnStart();

                Console.WriteLine("RHttpServer v. {0} started", Version);
                if (_listener.Prefixes.First() == "localhost")
                    Logger.Log("Server visibility", "Listening on localhost only");
                RenderParams.Converter = _rPluginCollection.Use<IJsonConverter>();
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
        ///     Initializes any default plugin if no other plugin is registered to same interface
        ///     <para />
        ///     Also used for changing the default security settings
        ///     <para />
        ///     Should be called after you have registered all your non-default plugins
        /// </summary>
        public void InitializeDefaultPlugins(bool renderCaching = true, bool securityOn = false,
            SimpleHttpSecuritySettings securitySettings = null)
        {
            if (_defPluginsReady) return;

            if (!_rPluginCollection.IsRegistered<IJsonConverter>())
                RegisterPlugin<IJsonConverter, ServiceStackJsonConverter>(new ServiceStackJsonConverter());

            if (!_rPluginCollection.IsRegistered<IXmlConverter>())
                RegisterPlugin<IXmlConverter, ServiceStackXmlConverter>(new ServiceStackXmlConverter());

            if (!_rPluginCollection.IsRegistered<IHttpSecurityHandler>())
                RegisterPlugin<IHttpSecurityHandler, SimpleServerProtection>(new SimpleServerProtection());

            if (!_rPluginCollection.IsRegistered<IBodyParser>())
                RegisterPlugin<IBodyParser, SimpleBodyParser>(new SimpleBodyParser());

            if (!_rPluginCollection.IsRegistered<IFileCacheManager>())
                RegisterPlugin<IFileCacheManager, SimpleFileCacheManager>(new SimpleFileCacheManager());

            if (!_rPluginCollection.IsRegistered<IPageRenderer>())
                RegisterPlugin<IPageRenderer, EcsPageRenderer>(new EcsPageRenderer());

            _defPluginsReady = true;

            if (securitySettings == null) securitySettings = new SimpleHttpSecuritySettings();
            SecMan = _rPluginCollection.Use<IHttpSecurityHandler>();
            SecMan.Settings = securitySettings;
            _rPluginCollection.Use<IPageRenderer>().CachePages = renderCaching;
            _cacheMan = _rPluginCollection.Use<IFileCacheManager>();
            if (CachePublicFiles && _publicFiles)
                _resHandler = new CachePublicFileRequestHander(PublicDir, _cacheMan);
            else if (_publicFiles)
                _resHandler = new PublicFileRequestHander(PublicDir);
            else
                _resHandler = new ActionOnlyResponseHandler();
            SecurityOn = securityOn;
        }

        /// <summary>
        ///     Stops the server thread and all request handling threads.
        /// </summary>
        public void Stop()
        {
            StopEvent.Set();
            _listenerThread.Join();
            _listener.Stop();
            _rPluginCollection.Use<IHttpSecurityHandler>().Stop();
            OnStop();
        }

        private void HandleRequests()
        {
            while (_listener.IsListening)
                try
                {
                    var context = _listener.BeginGetContext(ContextReady, null);
                    if (0 == WaitHandle.WaitAny(new[] {StopEvent.WaitHandle, context.AsyncWaitHandle}))
                        return;
                }
                catch (Exception ex)
                {
                    if (ThrowExceptions) throw;
                    Logger.Log(ex);
                }
        }

        private void ContextReady(IAsyncResult ar)
        {
            try
            {
                ProcessContext(_listener.EndGetContext(ar));
            }
            catch
            {
                if (ThrowExceptions) throw;
            }
        }

        protected abstract void ProcessContext(HttpListenerContext context);

        protected void Process(HttpListenerContext context)
        {
            var route = context.Request.Url.AbsolutePath.Trim('/');
            var hm = GetMethod(context.Request.HttpMethod);
            if (hm == HttpMethod.UNSUPPORTED)
            {
                Logger.Log("Unsupported HTTP method",
                    $"{context.Request.HttpMethod} from {context.Request.RemoteEndPoint}");
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }

            if (context.Request.IsWebSocketRequest)
            {
                bool gf;
                var wsact = _rtman.SearchInTree(route, HttpMethod.WEBSOCKET, out gf);
                if (wsact != null)
                {
                    PerformWSAction(context, GetParams(wsact, route), _rPluginCollection, wsact);
                    return;
                }
            }

            bool generalFallback;
            var act = _rtman.SearchInTree(route, hm, out generalFallback);

            if (generalFallback && _resHandler.Handle(route, context))
                return;
            if (act != null)
            {
                PerformAction(context, GetParams(act, route), _rPluginCollection, act);
            }
            else
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
            }
        }

        private static async void PerformWSAction(HttpListenerContext context, RequestParams reqPar,
            RPluginCollection plugins, RHttpAction wsact)
        {
            var wsc = await context.AcceptWebSocketAsync(wsact.WSProtocol);
            wsact.WSAction(new RRequest(context.Request, reqPar, plugins), new WebSocketDialog(wsc));
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
                case "HEAD":
                    return HttpMethod.HEAD;
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
        ///     Returns the plugin registered to type T, if any
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetPlugin<T>()
        {
            return _rPluginCollection.Use<T>();
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnStop()
        {
        }

        public void Dispose()
        {
            Stop();
        }
    }
}