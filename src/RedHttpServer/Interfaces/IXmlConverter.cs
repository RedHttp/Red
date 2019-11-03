using System.IO;
using System.Threading.Tasks;

namespace Red.Interfaces
{
    /// <summary>
    ///     Interface for classes used for XML serialization and deserialization
    /// </summary>
    public interface IXmlConverter
    {
        /// <summary>
        ///     Method to serialize data to a XML string
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        string? Serialize<T>(T obj);

        /// <summary>
        ///     Method to serialize data to a XML string
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="outputStream"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task SerializeAsync<T>(T obj, Stream outputStream);

        /// <summary>
        ///     Method to deserialize a XML-containing string to specified type
        /// </summary>
        /// <param name="xmlData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T? Deserialize<T>(string xmlData)
            where T : class;
        
        /// <summary>
        ///     Method to deserialize a XML-containing stream to specified type
        /// </summary>
        /// <param name="xmlStream"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T?> DeserializeAsync<T>(Stream xmlStream)
            where T : class;
    }
}