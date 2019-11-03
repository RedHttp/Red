using System.IO;
using System.Threading.Tasks;
using Red.Interfaces;

namespace Red.Extensions
{
    /// <summary>
    ///     Very simple JsonConverter plugin using Newtonsoft.Json generic methods
    /// </summary>
    internal sealed class BodyParser : IBodyParser, IRedExtension
    {
        public void Initialize(RedHttpServer server)
        {
            server.Plugins.Register<IBodyParser, BodyParser>(this);
        }

        /// <inheritdoc />
        public Task<string> ReadAsync(Request request)
        {
            using (var streamReader = new StreamReader(request.BodyStream))
            {
                return streamReader.ReadToEndAsync();
            }
        }
        /// <inheritdoc />
        public async Task<T?> ParseAsync<T>(Request request)
            where T : class
        {
            switch (request.AspNetRequest.ContentType.ToLowerInvariant())
            {
                case "application/xml":
                case "text/xml":
                    return await request.Context.Plugins.Get<IXmlConverter>().DeserializeAsync<T>(request.BodyStream);
                case "application/json":
                case "text/json":
                    return await request.Context.Plugins.Get<IJsonConverter>().DeserializeAsync<T>(request.BodyStream);
                default:
                    return default;
            }
        }
    }
    
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
            return bodyParser.ParseAsync<T>(request);
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