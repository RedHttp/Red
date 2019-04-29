using System.IO;
using Newtonsoft.Json;
using Red.Interfaces;

namespace Red.Extensions
{
    /// <summary>
    ///     Very simple JsonConverter plugin using Newtonsoft.Json generic methods
    /// </summary>
    internal sealed class NewtonsoftJsonConverter : IJsonConverter, IRedExtension
    {
        /// <inheritdoc />
        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <inheritdoc />
        public T Deserialize<T>(string jsonData)
        {
            return JsonConvert.DeserializeObject<T>(jsonData);
        }
        
        /// <inheritdoc />
        public T Deserialize<T>(Stream jsonStream)
        {
            using (var reader = new StreamReader(jsonStream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        public void Initialize(RedHttpServer server)
        {
            server.Plugins.Register<IJsonConverter, NewtonsoftJsonConverter>(this);
        }
    }
}