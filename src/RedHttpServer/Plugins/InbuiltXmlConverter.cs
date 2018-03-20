using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Red.Plugins.Interfaces;

namespace Red.Plugins
{
    /// <summary>
    ///     Very simple XmlConverter plugin using System.Xml.Serialization
    /// </summary>
    internal sealed class InbuiltXmlConverter : IXmlConverter, IRedExtension
    {
        public string Serialize<T>(T obj)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var xml = new XmlTextWriter(stream, new UTF8Encoding(false)))
                    {
                        var xs = new XmlSerializer(typeof(T));
                        xs.Serialize(xml, obj);
                        var reader = new StreamReader(stream, Encoding.UTF8);
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            { 
                return string.Empty; 
            }
        }
        
        public T Deserialize<T>(string xmlData)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                using (var stringreader = new StringReader(xmlData))
                {
                    return (T)xmlSerializer.Deserialize(stringreader);
                }
            }
            catch (Exception)
            { 
                return default(T); 
            }
        }

        public void Initialize(RedHttpServer server)
        {
            // Nothing to init
        }
    }
}