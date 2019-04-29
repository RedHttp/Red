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
        public Task<string> Parse(Request request)
        {
            using (var streamReader = new StreamReader(request.BodyStream))
            {
                return streamReader.ReadToEndAsync();
            }
        }
        /// <inheritdoc />
        public T Parse<T>(Request request)
        {
            switch (request.AspNetRequest.ContentType.ToLowerInvariant())
            {
                case "application/xml":
                case "text/xml":
                    return request.Context.Plugins.Get<IXmlConverter>().Deserialize<T>(request.BodyStream);
                case "application/json":
                case "text/json":
                    return request.Context.Plugins.Get<IJsonConverter>().Deserialize<T>(request.BodyStream);
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
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ParseBody<T>(this Request request)
        {
            var bodyParser = request.Context.Plugins.Get<IBodyParser>();
            return bodyParser.Parse<T>(request);
        }
        /// <summary>
        ///     Returns the body deserialized or parsed to specified type if possible, default if not
        /// </summary>
        /// <returns></returns>
        public static Task<string> ReadBodyAsync(this Request request)
        {
            var bodyParser = request.Context.Plugins.Get<IBodyParser>();
            return bodyParser.Parse(request);
        }
    }
}