using System.Threading.Tasks;

namespace Red.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    ///     Interface for middleware that handle websocket middleware
    /// </summary>
    public interface IRedWebSocketMiddleware : IRedExtension
    {
        /// <summary>
        ///     Method called for every websocket request to the server
        /// </summary>
        /// <param name="req">The request object</param>
        /// <param name="wsd">The websocket dialog object</param>
        /// <param name="res">The response object</param>
        Task<HandlerType> Invoke(Request req, Response res, WebSocketDialog wsd);
    }
}