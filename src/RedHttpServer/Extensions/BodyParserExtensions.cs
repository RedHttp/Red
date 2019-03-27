using System.Threading.Tasks;
using Red.Interfaces;

namespace Red.Extensions
{
    /// <summary>
    ///     Extension to RedRequests, to parse body to object of specified type
    /// </summary>
    public static class BodyParserExtensions
    {
        /// <summary>
        ///     Returns the body deserialized or parsed to specified type, if possible, default if not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> ParseBodyAsync<T>(this Request request)
        {
            var bodyParser = request.ServerPlugins.Get<IBodyParser>();
            return await bodyParser.Parse<T>(request);
        }
    }
}