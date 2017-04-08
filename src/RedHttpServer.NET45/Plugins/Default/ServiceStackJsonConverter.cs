using System.IO;
using ServiceStack.Text;

namespace RedHttpServer.Plugins.Default
{
    /// <summary>
    ///     Very simple JsonConverter plugin using ServiceStact.Text generic methods
    /// </summary>
    internal sealed class ServiceStackJsonConverter : RPlugin, IJsonConverter
    {
        public string Serialize<T>(T obj)
        {
            return JsonSerializer.SerializeToString(obj);
        }

        public T Deserialize<T>(string jsonData)
        {
            return JsonSerializer.DeserializeFromString<T>(jsonData);
        }

        public void SerializeToStream<T>(T obj, Stream outputStream)
        {
            JsonSerializer.SerializeToStream(obj, outputStream);
        }

        public T DeserializeFromStream<T>(Stream jsonStream)
        {
            return JsonSerializer.DeserializeFromStream<T>(jsonStream);
        }
    }
}