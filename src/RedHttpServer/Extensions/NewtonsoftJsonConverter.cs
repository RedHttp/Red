using Newtonsoft.Json;
using Red.Interfaces;

namespace Red.Extensions
{
    /// <summary>
    ///     Very simple JsonConverter plugin using Newtonsoft.Json generic methods
    /// </summary>
    internal sealed class NewtonsoftJsonConverter : IJsonConverter, IRedExtension
    {
        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public T Deserialize<T>(string jsonData)
        {
            return JsonConvert.DeserializeObject<T>(jsonData);
        }

        public void Initialize(RedHttpServer server)
        {
            server.Plugins.Register<IJsonConverter, NewtonsoftJsonConverter>(this);
        }
    }
}