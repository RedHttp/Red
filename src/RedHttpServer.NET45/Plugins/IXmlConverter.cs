using System.IO;

namespace RedHttpServer.Plugins
{
    /// <summary>
    ///     Interface for classes used for XML serialization and deserialization
    /// </summary>
    public interface IXmlConverter
    {
        /// <summary>
        ///     Method to serialize data to XML
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        string Serialize<T>(T obj);

        /// <summary>
        ///     Method to deserialize XML data to specified type
        /// </summary>
        /// <param name="xmlData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Deserialize<T>(string xmlData);

        /// <summary>
        ///     Method to serialize object to output stream
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="outputStream"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        void SerializeToStream<T>(T obj, Stream outputStream);

        /// <summary>
        ///     Method to deserialize XML data to specified type from stream
        /// </summary>
        /// <param name="xmlStream"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T DeserializeFromStream<T>(Stream xmlStream);
    }
}