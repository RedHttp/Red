using System.IO;

namespace RHttpServer
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
        ///     Method to serialize object to output stream
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="outputStream"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        void SerializeToStream<T>(T obj, Stream outputStream);

        /// <summary>
        ///     Method to deserialize JSON data to specified type from stream
        /// </summary>
        /// <param name="jsonStream"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T DeserializeFromStream<T>(Stream jsonStream);
    }
}