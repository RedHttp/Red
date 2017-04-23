using RedHttpServerCore.Plugins.Interfaces;
using ServiceStack.Text;

namespace RedHttpServerCore.Plugins
{
    /// <summary>
    ///     Very simple XmlConverter plugin using ServiceStact.Text generic methods
    /// </summary>
    internal sealed class ServiceStackXmlConverter : IXmlConverter
    {
        public string Serialize<T>(T obj)
        {
            return XmlSerializer.SerializeToString(obj);
        }

        public T Deserialize<T>(string jsonData)
        {
            return XmlSerializer.DeserializeFromString<T>(jsonData);
        }
    }
}