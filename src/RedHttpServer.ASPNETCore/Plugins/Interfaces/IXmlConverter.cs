namespace RedHttpServerCore.Plugins.Interfaces
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
    }
}