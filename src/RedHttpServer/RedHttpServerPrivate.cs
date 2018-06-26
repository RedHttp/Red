using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.Internal.Networking;
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

        private readonly string _publicRoot;
        private List<HandlerWrapper> _handlers = new List<HandlerWrapper>();
        
        private List<WsHandlerWrapper> _wsHandlers = new List<WsHandlerWrapper>();
        
        private void ConfigurePolicy(CorsPolicyBuilder builder)
        {
            builder = CorsPolicy.AllowedOrigins.All(d => d == "*")
                ? builder.AllowAnyOrigin()
                : builder.WithOrigins(CorsPolicy.AllowedOrigins.ToArray());
            
            builder = CorsPolicy.AllowedMethods.All(d => d == "*")
                ? builder.AllowAnyMethod()
                : builder.WithOrigins(CorsPolicy.AllowedMethods.ToArray());
            
            builder = CorsPolicy.AllowedHeaders.All(d => d == "*")
                ? builder.AllowAnyHeader()
                : builder.WithOrigins(CorsPolicy.AllowedHeaders.ToArray());
        }
        
        private void Initialize()
        {
            foreach (var plugin in _plugins)
            {
                plugin.Initialize(this);
            }
        }
        
        private void SetRoutes(IRouteBuilder rb)
        {
            var urlParam = new Regex(":[\\w-]+", RegexOptions.Compiled);
            var generalParam = new Regex("\\*", RegexOptions.Compiled);

            foreach (var handlerWrapper in _handlers)
            {
                var path = ConvertParameter(handlerWrapper.Path, urlParam, generalParam);
                rb.MapVerb(handlerWrapper.Method, path, ctx => WrapHandler(ctx, handlerWrapper));
            }
            _handlers = null;

            foreach (var handlerWrapper in _wsHandlers)
            {
                var path = ConvertParameter(handlerWrapper.Path, urlParam, generalParam);
                rb.MapGet(path, ctx => WrapWebsocketHandler(ctx, handlerWrapper));
            }
            _wsHandlers = null;
        }
        private async Task WrapHandler(HttpContext context, HandlerWrapper handlerWrapper)
        {
            var req = new Request(context.Request, Plugins);
            var res = new Response(context.Response, Plugins);
            try
            {
                foreach (var middleware in _middlewareStack)
                {
                    if (res.Closed) return;
                    await middleware.Process(req, res);
                }
                handlerWrapper.Process(req, res);
            }
            catch (Exception)
            {
                if (!res.Closed)
                    await res.SendStatus(HttpStatusCode.InternalServerError);
            }
        }
        private async Task WrapWebsocketHandler(HttpContext context, WsHandlerWrapper handlerWrapper)
        {
            var req = new Request(context.Request, Plugins);
            var res = new Response(context.Response, Plugins);
            try
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var wsd = new WebSocketDialog(context, webSocket, Plugins);
                    foreach (var middleware in _wsMiddlewareStack)
                    {
                        if (res.Closed) break;
                        await middleware.Process(req, wsd, res);
                    }
                    handlerWrapper.Process(req, wsd, res);
                    await wsd.ReadFromWebSocket();
                }
                else
                {
                    if (!res.Closed)
                        await res.SendStatus(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception e)
            {
                if (!res.Closed)
                    await res.SendStatus(HttpStatusCode.InternalServerError);
            }
        }

        private void AddHandlers(string route, string method, Func<Request, Response, Task>[] handlers)
        {
            if (_handlers == null) // Handlers are set to null after they have been loaded
                throw new RedHttpServerException("Cannot add route handlers after server is started");
            
            if (handlers.Length == 0)
                throw new RedHttpServerException("A route requires at least one handler");
            
            _handlers.Add(new HandlerWrapper(route, method, handlers));
        }
        private static string ConvertParameter(string parameter, Regex urlParam, Regex generalParam)
        {
            parameter = parameter.Trim('/');
            if (parameter.Contains("*"))
                parameter = generalParam.Replace(parameter, "{*any}");
            if (parameter.Contains(":"))
                parameter = urlParam.Replace(parameter, match => "{" + match.Value.TrimStart(':') + "}");
            return parameter;
        }
    }
}