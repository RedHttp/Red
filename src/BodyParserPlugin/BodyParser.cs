using Red;
using Red.Plugins.Interfaces;

namespace BodyParserPlugin
{
    /// <summary>
    ///     Simple body parser that can be used to parse JSON and XML body , or just return the input stream
    /// </summary>
    public sealed class BodyParser : IRedPlugin
    {
        public void Initialize(RedHttpServer server)
        {
            BodyParserExtension.JsonConverter = server.Plugins.Get<IJsonConverter>();
            BodyParserExtension.XmlConverter = server.Plugins.Get<IXmlConverter>();
        }
    }
}