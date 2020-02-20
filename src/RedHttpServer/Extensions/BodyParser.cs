using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Red.Interfaces;

namespace Red.Extensions
{
    /// <summary>
    ///     Extendable bodyparser
    /// </summary>
    internal sealed class BodyParser : IBodyParser, IRedExtension
    {
        private static readonly Dictionary<string, Type> ConverterMappings = new Dictionary<string, Type>
        {
            {"application/xml", typeof(IXmlConverter)},
            {"text/xml", typeof(IXmlConverter)},
            {"application/json", typeof(IJsonConverter)},
            {"text/json", typeof(IJsonConverter)}
        };

        /// <inheritdoc />
        public Task<string> ReadAsync(Request request)
        {
            using var streamReader = new StreamReader(request.BodyStream);
            return streamReader.ReadToEndAsync();
        }

        /// <inheritdoc />
        public async Task<T?> DeserializeAsync<T>(Request request)
            where T : class
        {
            string contentType = request.Headers["Content-Type"];
            if (!ConverterMappings.TryGetValue(contentType, out var converterType))
                return default;

            var converter = request.Context.Plugins.Get<IBodyConverter>(converterType);
            return await converter.DeserializeAsync<T>(request.BodyStream);
        }

        public void Initialize(RedHttpServer server)
        {
            server.Plugins.Register<IBodyParser, BodyParser>(this);
        }

        /// <summary>
        ///     Set body converter for content-type
        /// </summary>
        public static void SetContentTypeConverter<TBodyConverter>(string contentType)
            where TBodyConverter : IBodyConverter
        {
            ConverterMappings[contentType] = typeof(TBodyConverter);
        }
    }
}