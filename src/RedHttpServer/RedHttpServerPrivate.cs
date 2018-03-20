using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Red
{
    public partial class RedHttpServer
    {
        
        private readonly List<IRedMiddleware> _middlewareStack = new List<IRedMiddleware>();
        private readonly List<IRedExtension> _plugins = new List<IRedExtension>();
        
        private List<HandlerWrapper> _handlers = new List<HandlerWrapper>();
        
        private List<Tuple<string, Action<Request, WebSocketDialog>>> _wsHandlers =
            new List<Tuple<string, Action<Request, WebSocketDialog>>>();
        
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
        
        
        private void SetRoutes(IRouteBuilder rb)
        {
            var urlParam = new Regex(":[\\w-]+", RegexOptions.Compiled);
            var generalParam = new Regex("\\*", RegexOptions.Compiled);

            foreach (var handlerWrapper in _handlers)
            {
                var path = ConvertParameter(handlerWrapper.Path, urlParam, generalParam);
                rb.MapVerb(handlerWrapper.Method.ToString(), path, ctx => WrapHandler(ctx, handlerWrapper.Handler));
            }
            _handlers = null;
            
            foreach (var method in _wsHandlers)
                rb.MapGet(ConvertParameter(method.Item1, urlParam, generalParam), ctx => WrapWebsocketHandler(ctx, method.Item2));
            _wsHandlers = null;
        }
        private async Task WrapHandler(HttpContext context, Func<Request, Response, Task> handler)
        {
            var req = new Request(context.Request, Plugins);
            var res = new Response(context.Response, Plugins);
            try
            {
                var url = context.Request.Path.Value;
                var method = ParseHttpMethod(context.Request.Method);
                foreach (var middleware in _middlewareStack)
                    if (!await middleware.Process(url, method, req, res, Plugins) || res.Closed) return;
                await handler(req, res);
            }
            catch (Exception e)
            {
                if (!res.Closed)
                    await res.SendStatus(HttpStatusCode.InternalServerError);
            }
        }
        private async Task WrapWebsocketHandler(HttpContext context, Action<Request, WebSocketDialog> handler)
        {
            var req = new Request(context.Request, Plugins);
            try
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var wsd = new WebSocketDialog(context, webSocket, Plugins);
                    handler(req, wsd);
                    await wsd.ReadFromWebSocket();
                }
                else
                    await Response.SendStatus(context.Response, HttpStatusCode.BadRequest);
                
            }
            catch (Exception e)
            {
                await Response.SendStatus(context.Response, HttpStatusCode.InternalServerError);
            }
        }

        private static HttpMethodEnum ParseHttpMethod(string str)
        {
            switch (str.ToUpperInvariant())
            {
                case "GET":
                    return HttpMethodEnum.GET;
                case "POST":
                    return HttpMethodEnum.POST;
                case "PUT":
                    return HttpMethodEnum.PUT;
                case "DELETE":
                    return HttpMethodEnum.DELETE;
                default:
                    throw new ArgumentException();
            }
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