using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Red.Interfaces;

namespace Red.Extensions
{
    /// <summary>
    ///     Very simple XmlConverter plugin using System.Xml.Serialization
    /// </summary>
    internal sealed class XmlConverter : IXmlConverter, IRedExtension
    {
        public void Initialize(RedHttpServer server)
        {
            server.Plugins.Register<IXmlConverter, XmlConverter>(this);
        }

        /// <inheritdoc />
        public string? Serialize<T>(T obj)
        {
            try
            {
                using var stream = new MemoryStream();
                using var xml = XmlWriter.Create(stream);
                var xs = new XmlSerializer(typeof(T));
                xs.Serialize(xml, obj);
                var reader = new StreamReader(stream, Encoding.UTF8);
                return reader.ReadToEnd();
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <inheritdoc />
        public T? Deserialize<T>(string xmlData)
            where T : class
        {
            try
            {
                using var stringReader = new StringReader(xmlData);
                using var xml = XmlReader.Create(stringReader);
                return (T) xml.ReadContentAs(typeof(T), null);
            }
            catch (FormatException)
            {
                return default;
            }
            catch (InvalidCastException)
            {
                return default;
            }
        }

        /// <inheritdoc />
        public async Task<T?> DeserializeAsync<T>(Stream xmlStream)
            where T : class
        {
            try
            {
                using var xmlReader = XmlReader.Create(xmlStream);
                return (T) await xmlReader.ReadContentAsAsync(typeof(T), null);
            }
            catch (FormatException)
            {
                return default;
            }
            catch (InvalidCastException)
            {
                return default;
            }
        }

        /// <inheritdoc />
        public async Task SerializeAsync<T>(T obj, Stream output)
        {
            try
            {
                using var xmlWriter = XmlWriter.Create(output, new XmlWriterSettings
                {
                    Async = true
                });
                var xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(xmlWriter, obj);
                await xmlWriter.FlushAsync();
            }
            catch (Exception)
            {
            }
        }
    }
}