using System;
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
        private static readonly Type StringType = typeof(string);
        
        public void Initialize(RedHttpServer server)
        {
            server.Plugins.Register<IBodyParser>(this);
        }

        public async Task<T> Parse<T>(Request request)
        {
            var t = typeof(T);
            using (var sr = new StreamReader(request.UnderlyingRequest.Body))
            {
                if (t == StringType)
                    return (T) (object) await sr.ReadToEndAsync();
                switch (request.UnderlyingRequest.ContentType)
                {
                    case "application/xml":
                    case "text/xml":
                        return request.ServerPlugins.Get<IXmlConverter>().Deserialize<T>(await sr.ReadToEndAsync());
                    case "application/json":
                    case "text/json":
                        return request.ServerPlugins.Get<IJsonConverter>().Deserialize<T>(await sr.ReadToEndAsync());
                    default:
                        return default(T);
                }
            }
        }
    }
}