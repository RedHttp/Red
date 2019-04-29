using System;
using System.Threading.Tasks;

namespace Red.Interfaces
{
    /// <summary>
    /// The interface for Routers
    /// </summary>
    public interface IRouter
    {
        /// <summary>
        ///     Create a 
        /// </summary>
        /// <param name="baseRoute"></param>
        /// <returns></returns>
        IRouter CreateRouter(string baseRoute);
        
        /// <summary>
        ///     Add action to handle GET requests to a given route
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="handlers">The handlers that wil respond to the request</param>
        void Get(string route, params Func<Request, Response, Task<HandlerType>>[] handlers);
        /// <summary>
        ///     Add action to handle POST requests to a given route
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="handlers">The handlers that wil respond to the request</param>
        void Post(string route, params Func<Request, Response, Task<HandlerType>>[] handlers);
        /// <summary>
        ///     Add action to handle PUT requests to a given route.
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="handlers">The handlers that wil respond to the request</param>
        void Put(string route, params Func<Request, Response, Task<HandlerType>>[] handlers);
        /// <summary>
        ///     Add action to handle DELETE requests to a given route.
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="handlers">The handlers that wil respond to the request</param>
        void Delete(string route, params Func<Request, Response, Task<HandlerType>>[] handlers);
        
        /// <summary>
        ///     Add action to handle WEBSOCKET requests to a given route. <para/>
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="handlers">The handlers that wil respond to the request</param>
        void WebSocket(string route, params Func<Request, Response, WebSocketDialog, Task<HandlerType>>[] handlers);
        /// <summary>
        ///     Add action to handle WEBSOCKET requests to a given route. <para/>
        /// </summary>
        /// <param name="route">The route to respond to</param>
        /// <param name="handlers">The handlers that wil respond to the request</param>
        void WebSocket(string route, params Func<Request, WebSocketDialog, Task<HandlerType>>[] handlers);
    }
}