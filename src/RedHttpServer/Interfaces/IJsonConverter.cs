using System.IO;

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
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        string Serialize<T>(T obj);

        /// <summary>
        ///     Method to deserialize JSON data to specified type
        /// </summary>
        /// <param name="jsonData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Deserialize<T>(string jsonData);
        
        /// <summary>
        ///     Method to deserialize JSON data to specified type
        /// </summary>
        /// <param name="jsonStream"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Deserialize<T>(Stream jsonStream);
    }
}