using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using RedHttpServer.Logging;

namespace RedHttpServer.Plugins.Default
{
    /// <summary>
    ///     Simple body parser that can be used to parse JSON objects and C# primitives, or just return the input stream
    /// </summary>
    internal sealed class SimpleBodyParser : IBodyParser
    {
        private static readonly Type Char = typeof(char);
        private static readonly Type Decimal = typeof(decimal);
        private static readonly Type Double = typeof(double);
        private static readonly Type Float = typeof(float);
        private static readonly Type Int = typeof(int);
        private static readonly Type Stream = typeof(Stream);
        private static readonly Type String = typeof(string);

        public async Task<T> ParseBodyAsync<T>(HttpListenerRequest underlyingRequest)
        {
            using (var sr = new StreamReader(underlyingRequest.InputStream))
            {
                switch (UnderlyingRequest.ContentType)
                {
                    case "application/xml":
                    case "text/xml":
                        return XmlSerializer.DeserializeFromString<T>(await sr.ReadToEndAsync());
                    case "application/json":
                    case "text/json":
                        return XmlSerializer.DeserializeFromString<T>(await sr.ReadToEndAsync());
                    default:
                        return default(T);
                }
            }

            if (!underlyingRequest.HasEntityBody) return default(T);
            var t = typeof(T);
            if (t == Stream) return (T) (object) underlyingRequest.InputStream;
            using (var stream = underlyingRequest.InputStream)
            {
                if (t.IsPrimitive || t == String)
                    using (var reader = new StreamReader(stream, underlyingRequest.ContentEncoding))
                    {
                        if (t == Int)
                        {
                            int i;
                            if (int.TryParse(await reader.ReadToEndAsync(), out i)) return (T) (object) i;
                            return default(T);
                        }
                        if (t == Double)
                        {
                            double d;
                            if (double.TryParse(await reader.ReadToEndAsync(), out d)) return (T) (object) d;
                            return default(T);
                        }
                        if (t == Decimal)
                        {
                            decimal d;
                            if (decimal.TryParse(await reader.ReadToEndAsync(), out d)) return (T) (object) d;
                            return default(T);
                        }
                        if (t == Float)
                        {
                            float f;
                            if (float.TryParse(await reader.ReadToEndAsync(), out f)) return (T) (object) f;
                            return default(T);
                        }
                        if (t == Char)
                        {
                            char c;
                            if (char.TryParse(await reader.ReadToEndAsync(), out c)) return (T) (object) c;
                            return default(T);
                        }
                        if (t == String) return (T) (object) reader.ReadToEnd();
                    }

                if (underlyingRequest.ContentType.Contains("application/xml") ||
                    underlyingRequest.ContentType.Contains("text/xml"))
                    try
                    {
                        return UsePlugin<IXmlConverter>().DeserializeFromStream<T>(stream);
                    }
                    catch (FormatException ex)
                    {
                        Logger.Log(ex);
                        return default(T);
                    }

                if (underlyingRequest.ContentType.Contains("application/json") ||
                    underlyingRequest.ContentType.Contains("text/json"))
                    try
                    {
                        return UsePlugin<IJsonConverter>().DeserializeFromStream<T>(stream);
                    }
                    catch (FormatException ex)
                    {
                        Logger.Log(ex);
                        return default(T);
                    }
                return default(T);
            }
        }

        public T ParseBody<T>(HttpListenerRequest underlyingRequest)
        {
            if (!underlyingRequest.HasEntityBody) return default(T);
            var t = typeof(T);
            if (t == Stream) return (T) (object) underlyingRequest.InputStream;
            using (var stream = underlyingRequest.InputStream)
            {
                if (t.IsPrimitive || t == String)
                    using (var reader = new StreamReader(stream, underlyingRequest.ContentEncoding))
                    {
                        if (t == Int)
                        {
                            int i;
                            if (int.TryParse(reader.ReadToEnd(), out i)) return (T) (object) i;
                            return default(T);
                        }
                        if (t == Double)
                        {
                            double d;
                            if (double.TryParse(reader.ReadToEnd(), out d)) return (T) (object) d;
                            return default(T);
                        }
                        if (t == Decimal)
                        {
                            decimal d;
                            if (decimal.TryParse(reader.ReadToEnd(), out d)) return (T) (object) d;
                            return default(T);
                        }
                        if (t == Float)
                        {
                            float f;
                            if (float.TryParse(reader.ReadToEnd(), out f)) return (T) (object) f;
                            return default(T);
                        }
                        if (t == Char)
                        {
                            char c;
                            if (char.TryParse(reader.ReadToEnd(), out c)) return (T) (object) c;
                            return default(T);
                        }
                        if (t == String) return (T) (object) reader.ReadToEnd();
                    }

                if (underlyingRequest.ContentType.Contains("application/xml") ||
                    underlyingRequest.ContentType.Contains("text/xml"))
                    try
                    {
                        return UsePlugin<IXmlConverter>().DeserializeFromStream<T>(stream);
                    }
                    catch (FormatException ex)
                    {
                        Logger.Log(ex);
                        return default(T);
                    }

                if (underlyingRequest.ContentType.Contains("application/json") ||
                    underlyingRequest.ContentType.Contains("text/json"))
                    try
                    {
                        return UsePlugin<IJsonConverter>().DeserializeFromStream<T>(stream);
                    }
                    catch (FormatException ex)
                    {
                        Logger.Log(ex);
                        return default(T);
                    }
                return default(T);
            }
        }
    }
}