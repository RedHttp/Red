using System.IO;
using System.Threading.Tasks;

namespace Red.Interfaces
{
    /// <summary>
    ///     Interface for body converters
    /// </summary>
    public interface IBodyConverter
    {
        /// <summary>
        ///     Serialize data to a string
        /// </summary>
        string? Serialize<T>(T obj);

        /// <summary>
        ///     Deserialize data to specified type
        /// </summary>
        T? Deserialize<T>(string jsonData)
            where T : class;

        /// <summary>
        ///     Deserialize data from a stream to specified type
        /// </summary>
        Task<T?> DeserializeAsync<T>(Stream jsonStream)
            where T : class;

        /// <summary>
        ///     Serialize data to a stream
        /// </summary>
        Task SerializeAsync<T>(T obj, Stream jsonStream);
    }
}