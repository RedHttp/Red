using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ServiceStack.Text;

namespace RedHttpServer.Plugins.Default
{
    /// <summary>
    ///     Simple body parser that can be used to parse JSON objects and C# primitives, or just return the input stream
    /// </summary>
    internal sealed class SimpleBodyParser : IBodyParser
    {
        internal SimpleBodyParser(IJsonConverter jsonconv, IXmlConverter xmlconv)
        {
            _jsonConv = jsonconv;
            _xmlConv = xmlconv;
        }
        private static readonly Type StringType = typeof(string);
        private readonly IJsonConverter _jsonConv;
        private readonly IXmlConverter _xmlConv;

        public async Task<T> ParseBodyAsync<T>(HttpRequest underlyingRequest)
        {
            var t = typeof(T);
            using (var sr = new StreamReader(underlyingRequest.Body))
            {
                if (t == StringType)
                    return (T) (object) await sr.ReadToEndAsync();
                switch (underlyingRequest.ContentType)
                {
                    case "application/xml":
                    case "text/xml":
                        return _xmlConv.Deserialize<T>(await sr.ReadToEndAsync());
                    case "application/json":
                    case "text/json":
                        return _jsonConv.Deserialize<T> (await sr.ReadToEndAsync());
                    default:
                        return default(T);
                }
            }
        }
    }
}