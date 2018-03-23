using System.Threading.Tasks;

namespace Red.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRedMiddleware : IRedExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="method"></param>
        /// <param name="req"></param>
        /// <param name="res"></param>
        /// <returns>Whether to continue through the middleware stack</returns>
        Task<bool> Process(string path, HttpMethodEnum method, Request req, Response res, PluginCollection plugins);
    }
}