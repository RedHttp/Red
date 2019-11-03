using System.IO;
using System.Threading.Tasks;

namespace Red.Interfaces
{
    /// <summary>
    ///     Interface for classes used for Json serialization and deserialization
    /// </summary>
    public interface IJsonConverter
    {
        /// <summary>
        ///     Method to serialize data to JSON
        /// </summary>
        string? Serialize<T>(T obj);

        /// <summary>
        ///     Method to deserialize JSON data to specified type
        /// </summary>
        T? Deserialize<T>(string jsonData)
            where T : class;
        
        /// <summary>
        ///     Method to deserialize JSON data to specified type
        /// </summary>
        Task<T?> DeserializeAsync<T>(Stream jsonStream)
            where T : class;

        /// <summary>
        ///     Method to serialize data to JSON, to a stream
        /// </summary>
        Task SerializeAsync<T>(T obj, Stream jsonStream);
    }
}