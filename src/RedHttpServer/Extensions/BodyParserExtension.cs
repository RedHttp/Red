using System.Threading.Tasks;
using Red.Interfaces;

namespace Red.Extensions
{
    /// <summary>
    ///     Extension to RedRequests, to parse body to object of specified type
    /// </summary>
    public static class BodyParserExtension
    {
        /// <summary>
        ///     Returns the body deserialized or parsed to specified type if possible, default if not
        /// </summary>
        public static Task<T?> ParseBodyAsync<T>(this Request request)
            where T : class
        {
            var bodyParser = request.Context.Plugins.Get<IBodyParser>();
            return bodyParser.DeserializeAsync<T>(request);
        }

        /// <summary>
        ///     Returns the body deserialized or parsed to specified type if possible, default if not
        /// </summary>
        public static Task<string> ReadBodyAsync(this Request request)
        {
            var bodyParser = request.Context.Plugins.Get<IBodyParser>();
            return bodyParser.ReadAsync(request);
        }
    }
}