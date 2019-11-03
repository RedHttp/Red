using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Red.Interfaces;

namespace Red
{
    public partial class RedHttpServer
    {
        private const string GetMethod = "GET";
        private const string PostMethod = "POST";
        private const string PutMethod = "PUT";
        private const string DeleteMethod = "DELETE";
        
        private readonly List<IRedMiddleware> _middlewareStack = new List<IRedMiddleware>();
        private readonly List<IRedWebSocketMiddleware> _wsMiddlewareStack = new List<IRedWebSocketMiddleware>();
        private readonly List<IRedExtension> _plugins = new List<IRedExtension>();

        private IWebHost? _host;
        private readonly string? _publicRoot;
        private List<RequestHandler>? _handlers = new List<RequestHandler>();
        
        private List<WebsocketRequestHandler>? _wsHandlers = new List<WebsocketRequestHandler>();
        
        private IWebHost Build(IReadOnlyCollection<string> hostnames)
        {
            if (_host != default)
            {
                throw new RedHttpServerException("The server is already running");
            }
            Initialize();
            var urls = hostnames.Count != 0 
                ? hostnames.Select(url => $"http://{url}:{Port}").ToArray()
                : new []{ $"http://localhost:{Port}" };
            return new WebHostBuilder()
                .UseKestrel()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    ConfigureServices?.Invoke(services);
                })
                .Configure(app =>
                {
                    if (!string.IsNullOrWhiteSpace(_publicRoot) && Directory.Exists(_publicRoot))
                        app.UseFileServer(new FileServerOptions { FileProvider = new PhysicalFileProvider(Path.GetFullPath(_publicRoot)) });
                    if (_wsHandlers.Any())
                        app.UseWebSockets();
                    app.UseRouter(SetRoutes);
                    ConfigureApplication?.Invoke(app);
                })
                .UseUrls(urls)
                .Build();
        }
        
        private void Initialize()
        {
            foreach (var plugin in _plugins)
            {
                plugin.Initialize(this);
            }
        }
        
        private void SetRoutes(IRouteBuilder routeBuilder)
        {
            var namePathParameterRegex = new Regex(":[\\w-]+", RegexOptions.Compiled);

            if (_handlers == null || _wsHandlers == null)
            {
                throw new InvalidOperationException();
            }
            
            foreach (var handler in _handlers)
            {
                var path = ConvertPathParameters(handler.Path, namePathParameterRegex);
                routeBuilder.MapVerb(handler.Method, path, ctx => ExecuteHandler(ctx, handler));
            }
            _handlers = default;

            foreach (var handlerWrapper in _wsHandlers)
            {
                var path = ConvertPathParameters(handlerWrapper.Path, namePathParameterRegex);
                routeBuilder.MapGet(path, ctx => ExecuteHandler(ctx, handlerWrapper));
            }
            _wsHandlers = default;
        }
        private async Task<HandlerType> ExecuteHandler(HttpContext aspNetContext, RequestHandler requestHandler)
        {
            var context = new Context(aspNetContext, Plugins);
            
            var status = HandlerType.Continue;
            try
            {
                foreach (var middleware in _middlewareStack)
                {
                    status = await middleware.Invoke(context.Request, context.Response);
                    if (status != HandlerType.Continue) return status;
                }
                if (status == HandlerType.Continue) 
                    status = await requestHandler.Invoke(context.Request, context.Response);
                return status;
            }
            catch (Exception e)
            { 
                return await HandleException(context, status, e);
            }
        }
        private async Task<HandlerType> ExecuteHandler(HttpContext aspNetContext, WebsocketRequestHandler requestHandler)
        {
            var context = new Context(aspNetContext, Plugins);
            
            var status = HandlerType.Continue;
            try
            {
                if (aspNetContext.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await aspNetContext.WebSockets.AcceptWebSocketAsync();
                    var webSocketDialog = new WebSocketDialog(context, webSocket);
                    foreach (var middleware in _wsMiddlewareStack)
                    {
                        status = await middleware.Invoke(context.Request, webSocketDialog, context.Response);
                        if (status != HandlerType.Continue) return status;
                    }
                    if (status == HandlerType.Continue) 
                        status = await requestHandler.Invoke(context.Request, webSocketDialog, context.Response);
                    await webSocketDialog.ReadFromWebSocket();
                    return status;
                }
                else
                {
                    await context.Response.SendStatus(HttpStatusCode.BadRequest);
                    return HandlerType.Error;
                }
            }
            catch (Exception e)
            {
                return await HandleException(context, status, e);
            }
        }

        private async Task<HandlerType> HandleException(Context context, HandlerType status, Exception e)
        {
            var path = context.Request.AspNetRequest.Path.ToString();
            OnHandlerException?.Invoke(this, new HandlerExceptionEventArgs(path, e));
            
            if (status != HandlerType.Continue)
            {
                return HandlerType.Error;
            }
                
            if (RespondWithExceptionDetails)
            {
                await context.Response.SendString(e.ToString(), status: HttpStatusCode.InternalServerError);
            }
            else
            {
                await context.Response.SendStatus(HttpStatusCode.InternalServerError);
            }
            return HandlerType.Error;
        }

        private void AddHandlers(string route, string method, Func<Request, Response, Task<HandlerType>>[] handlers)
        {
            if (_handlers == null) // Handlers are set to null after they have been loaded
                throw new RedHttpServerException("Cannot add route handlers after server is started");
            
            if (handlers.Length == 0)
                throw new RedHttpServerException("A route requires at least one handler");
            
            _handlers.Add(new RequestHandler(route, method, handlers));
        }
        private static string ConvertPathParameters(string parameter, Regex urlParam)
        {
            return urlParam
                .Replace(parameter, match => "{" + match.Value.TrimStart(':') + "}")
                .Replace("*", "{*any}")
                .Trim('/');
        }

    }
}