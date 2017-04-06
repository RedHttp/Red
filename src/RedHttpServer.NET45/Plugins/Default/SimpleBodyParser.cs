using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using RHttpServer.Logging;

namespace RHttpServer.Default
{
    /// <summary>
    ///     Simple body parser that can be used to parse JSON objects and C# primitives, or just return the input stream
    /// </summary>
    internal sealed class SimpleBodyParser : RPlugin, IBodyParser
    {
        private readonly Type _char = typeof(char);
        private readonly Type _decimal = typeof(decimal);
        private readonly Type _double = typeof(double);
        private readonly Type _float = typeof(float);
        private readonly Type _int = typeof(int);
        private readonly Type _stream = typeof(Stream);
        private readonly Type _string = typeof(string);

        public async Task<T> ParseBodyAsync<T>(HttpListenerRequest underlyingRequest)
        {
            if (!underlyingRequest.HasEntityBody) return default(T);
            var t = typeof(T);
            if (t == _stream) return (T) (object) underlyingRequest.InputStream;
            using (var stream = underlyingRequest.InputStream)
            {
                if (t.IsPrimitive || t == _string)
                    using (var reader = new StreamReader(stream, underlyingRequest.ContentEncoding))
                    {
                        if (t == _int)
                        {
                            int i;
                            if (int.TryParse(await reader.ReadToEndAsync(), out i)) return (T) (object) i;
                            return default(T);
                        }
                        if (t == _double)
                        {
                            double d;
                            if (double.TryParse(await reader.ReadToEndAsync(), out d)) return (T) (object) d;
                            return default(T);
                        }
                        if (t == _decimal)
                        {
                            decimal d;
                            if (decimal.TryParse(await reader.ReadToEndAsync(), out d)) return (T) (object) d;
                            return default(T);
                        }
                        if (t == _float)
                        {
                            float f;
                            if (float.TryParse(await reader.ReadToEndAsync(), out f)) return (T) (object) f;
                            return default(T);
                        }
                        if (t == _char)
                        {
                            char c;
                            if (char.TryParse(await reader.ReadToEndAsync(), out c)) return (T) (object) c;
                            return default(T);
                        }
                        if (t == _string) return (T) (object) reader.ReadToEnd();
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
            if (t == _stream) return (T) (object) underlyingRequest.InputStream;
            using (var stream = underlyingRequest.InputStream)
            {
                if (t.IsPrimitive || t == _string)
                    using (var reader = new StreamReader(stream, underlyingRequest.ContentEncoding))
                    {
                        if (t == _int)
                        {
                            int i;
                            if (int.TryParse(reader.ReadToEnd(), out i)) return (T) (object) i;
                            return default(T);
                        }
                        if (t == _double)
                        {
                            double d;
                            if (double.TryParse(reader.ReadToEnd(), out d)) return (T) (object) d;
                            return default(T);
                        }
                        if (t == _decimal)
                        {
                            decimal d;
                            if (decimal.TryParse(reader.ReadToEnd(), out d)) return (T) (object) d;
                            return default(T);
                        }
                        if (t == _float)
                        {
                            float f;
                            if (float.TryParse(reader.ReadToEnd(), out f)) return (T) (object) f;
                            return default(T);
                        }
                        if (t == _char)
                        {
                            char c;
                            if (char.TryParse(reader.ReadToEnd(), out c)) return (T) (object) c;
                            return default(T);
                        }
                        if (t == _string) return (T) (object) reader.ReadToEnd();
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