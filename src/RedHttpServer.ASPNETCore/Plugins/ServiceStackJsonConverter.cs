using RedHttpServer.Plugins.Interfaces;
using ServiceStack.Text;

namespace RedHttpServer.Plugins
{
    /// <summary>
    ///     Very simple JsonConverter plugin using ServiceStact.Text generic methods
    /// </summary>
    internal sealed class ServiceStackJsonConverter : IJsonConverter
    {
        public string Serialize<T>(T obj)
        {
            return JsonSerializer.SerializeToString(obj);
        }

        public T Deserialize<T>(string jsonData)
        {
            return JsonSerializer.DeserializeFromString<T>(jsonData);
        }
    }
}