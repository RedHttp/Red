using System;
using System.Collections.Generic;
using RedHttpServer.Request;
using RedHttpServer.Response;

namespace RedHttpServer
{
    internal class CorsHandler
    {
        private readonly Dictionary<string, List<string>> _corsRoutes = new Dictionary<string, List<string>>();

        internal void Register(string route, string method)
        {
            route = route.Trim('/');
            if (!_corsRoutes.TryGetValue(route, out List<string> list))
            {
                list = new List<string>();
                _corsRoutes.Add(route, list);
            }
            list.Add(method);
        }

        public void Bind(CorsPolicy corsPolicy, RouteTreeManager rtman)
        {
            if (corsPolicy == null)
                return;
            var headers = string.Join(", ", corsPolicy.AllowedHeaders);
            var origins = string.Join(", ", corsPolicy.AllowedOrigins);
            foreach (var route in _corsRoutes.Keys)
            {
                var methods = string.Join(", ", _corsRoutes[route]);
                rtman.AddRoute(new RHttpAction(route, (request, response) =>
                {
                    response.AddHeader("Access-Control-Allow-Origin", origins);
                    response.AddHeader("Access-Control-Allow-Methods", methods);
                    response.AddHeader("Access-Control-Allow-Headers", headers);
                    response.SendString("");
                }), HttpMethod.OPTIONS);
            }
        }
    }
}