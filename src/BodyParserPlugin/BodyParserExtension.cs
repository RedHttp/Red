using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Red.Plugins.Interfaces;
using Red.Request;

namespace BodyParserPlugin
{
    public static class BodyParserExtension
    {
        private static readonly Type StringType = typeof(string);
        public static IJsonConverter JsonConverter;
        public static IXmlConverter XmlConverter;

        /// <summary>
        ///     Returns the body deserialized or parsed to specified type, if possible, default if not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> ParseBodyAsync<T>(this RRequest request)
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
                        return XmlConverter.Deserialize<T>(await sr.ReadToEndAsync());
                    case "application/json":
                    case "text/json":
                        return JsonConverter.Deserialize<T>(await sr.ReadToEndAsync());
                    default:
                        return default(T);
                }
            }
        }
    }
}