using System.IO;
using ServiceStack.Text;

namespace RedHttpServer.Plugins.Default
{
    /// <summary>
    ///     Very simple XmlConverter plugin using ServiceStact.Text generic methods
    /// </summary>
    internal sealed class ServiceStackXmlConverter : RPlugin, IXmlConverter
    {
        public string Serialize<T>(T obj)
        {
            return XmlSerializer.SerializeToString(obj);
        }

        public T Deserialize<T>(string jsonData)
        {
            return XmlSerializer.DeserializeFromString<T>(jsonData);
        }

        public void SerializeToStream<T>(T obj, Stream outputStream)
        {
            XmlSerializer.SerializeToStream(obj, outputStream);
        }

        public T DeserializeFromStream<T>(Stream jsonStream)
        {
            return XmlSerializer.DeserializeFromStream<T>(jsonStream);
        }
    }
}