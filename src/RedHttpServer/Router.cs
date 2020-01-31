using System;
using System.Threading.Tasks;
using Red.Interfaces;

namespace Red
{
    /// <summary>
    ///     A Router class that can be used to separate the handlers of the server in a more teamwork (git) friendly way
    /// </summary>
    public class Router : IRouter
    {
        private readonly string _baseRoute;
        private readonly IRouter _router;

        /// <summary>
        ///     Creates a Router from a baseRoute, that will be prepended all handlers registered through it,
        ///     and add them to the router
        /// </summary>
        /// <param name="baseRoute">The base route of the router</param>
        /// <param name="router">The router (or server) to add handlers to</param>
        public Router(string baseRoute, IRouter router)
        {
            _baseRoute = baseRoute.Trim('/');
            _router = router;
        }

        /// <inheritdoc />
        public IRouter CreateRouter(string baseRoute)
        {
            return new Router(baseRoute, this);
        }

        /// <inheritdoc />
        public void Get(string route, params Func<Request, Response, Task<HandlerType>>[] handlers)
        {
            _router.Get(CombinePartialRoutes(_baseRoute, route), handlers);
        }

        /// <inheritdoc />
        public void Post(string route, params Func<Request, Response, Task<HandlerType>>[] handlers)
        {
            _router.Post(CombinePartialRoutes(_baseRoute, route), handlers);
        }

        /// <inheritdoc />
        public void Put(string route, params Func<Request, Response, Task<HandlerType>>[] handlers)
        {
            _router.Put(CombinePartialRoutes(_baseRoute, route), handlers);
        }

        /// <inheritdoc />
        public void Delete(string route, params Func<Request, Response, Task<HandlerType>>[] handlers)
        {
            _router.Delete(CombinePartialRoutes(_baseRoute, route), handlers);
        }

        /// <inheritdoc />
        public void WebSocket(string route,
            params Func<Request, Response, WebSocketDialog, Task<HandlerType>>[] handlers)
        {
            _router.WebSocket(CombinePartialRoutes(_baseRoute, route), handlers);
        }

        private static string CombinePartialRoutes(string baseRoute, string route)
        {
            return $"{baseRoute}/{route.TrimStart('/')}";
        }
    }
}