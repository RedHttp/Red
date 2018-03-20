using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Red.Plugins.Interfaces
{
    /// <summary>
    ///     Interface for classes used to parse the request body data stream
    /// </summary>
    public interface IBodyParser
    {
        /// <summary>
        ///     The method that must handle the body stream async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> ParseBodyAsync<T>(HttpRequest underlyingRequest);
    }
}