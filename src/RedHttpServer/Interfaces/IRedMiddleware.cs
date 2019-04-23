using System.Threading.Tasks;

namespace Red.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public interface IRedMiddleware : IRedExtension
    {
        /// <summary>
        ///     Method called for every get, post, put and delete request to the server
        /// </summary>
        /// <param name="req"></param>
        /// <param name="res"></param>
        Task<HandlerType> Invoke(Request req, Response res);
    }
}