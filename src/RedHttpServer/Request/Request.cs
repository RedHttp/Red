using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Red.Plugins.Interfaces;

namespace Red
{
    /// <summary>
    ///     Class representing a request from a client
    /// </summary>
    public sealed class Request
    {
        private readonly Dictionary<Type, object> _data = new Dictionary<Type, object>();
        private static readonly Type StringType = typeof(string);
        private static readonly IFormCollection EmptyFormCol =
            new FormCollection(new Dictionary<string, StringValues>());

        internal Request(HttpRequest req, PluginCollection plugins)
        {
            UnderlyingRequest = req;
            Parameters = new RequestParameters(req.HttpContext);
            ServerPlugins = plugins;
        }

        /// <summary>
        /// The available plugins
        /// </summary>
        public PluginCollection ServerPlugins { get; set; }

        /// <summary>
        ///     The underlying HttpRequest
        ///     <para />
        ///     The implementation of RRequest is leaky, to avoid limiting you
        /// </summary>
        public HttpRequest UnderlyingRequest { get; }

        /// <summary>
        ///     The url parameters of the request
        /// </summary>
        public RequestParameters Parameters { get; }

        /// <summary>
        ///     The query elements of the request
        /// </summary>
        public IQueryCollection Queries => UnderlyingRequest.Query;

        /// <summary>
        ///     The headers contained in the request
        /// </summary>
        public IHeaderDictionary Headers => UnderlyingRequest.Headers;

        /// <summary>
        ///     The cookies contained in the request
        /// </summary>
        public IRequestCookieCollection Cookies => UnderlyingRequest.Cookies;

        /// <summary>
        ///     Returns the body stream of the request
        ///     and null if the request does not contain a body
        /// </summary>
        /// <returns></returns>
        public Stream GetBodyStream() => UnderlyingRequest.Body;

        /// <summary>
        ///     Returns form-data from post request, if any
        /// </summary>
        /// <returns></returns>
        public async Task<IFormCollection> GetFormDataAsync()
        {
            if (!UnderlyingRequest.HasFormContentType)
                return EmptyFormCol;
            return await UnderlyingRequest.ReadFormAsync();
        }
        
        /// <summary>
        ///     Get data attached to request by middleware
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <returns></returns>
        public TData GetData<TData>()
        {
            if (_data.TryGetValue(typeof(TData), out var data))
                return (TData) data;
            return default(TData);
        }
        public void SetData<TData>(TData data)
        {
            _data[typeof(TData)] = data;
        }
        
        
        /// <summary>
        ///     Returns the body deserialized or parsed to specified type, if possible, default if not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> ParseBodyAsync<T>()
        {
            var t = typeof(T);
            using (var sr = new StreamReader(UnderlyingRequest.Body))
            {
                if (t == StringType)
                    return (T) (object) await sr.ReadToEndAsync();
                switch (UnderlyingRequest.ContentType)
                {
                    case "application/xml":
                    case "text/xml":
                        return ServerPlugins.Get<IXmlConverter>().Deserialize<T>(await sr.ReadToEndAsync());
                    case "application/json":
                    case "text/json":
                        return ServerPlugins.Get<IJsonConverter>().Deserialize<T>(await sr.ReadToEndAsync());
                    default:
                        return default(T);
                }
            }
        }
        

        /// <summary>
        ///     Save multipart form-data from request body, if any, to a file in the specified directory
        /// </summary>
        /// <param name="saveDir">The directory to place the file(s) in</param>
        /// <param name="fileRenamer">Function to rename the file(s)</param>
        /// <param name="maxSizeKb">The max total filesize allowed</param>
        /// <returns>Whether the file was saved succesfully</returns>
        public async Task<bool> SaveFiles(string saveDir, Func<string, string> fileRenamer = null,
            long maxSizeKb = 4096)
        {
            if (UnderlyingRequest.HasFormContentType)
            {
                var form = await UnderlyingRequest.ReadFormAsync();
                if (form.Files.Sum(file => file.Length) > (maxSizeKb << 10))
                    return false;
                
                foreach (var formFile in form.Files)
                {
                    var filename = fileRenamer == null ? formFile.FileName : fileRenamer(formFile.FileName);
                    var filepath = Path.Combine(saveDir, filename);
                    using (var filestream = File.Create(filepath))
                    {
                        await formFile.CopyToAsync(filestream);
                    }
                }

                return true;
            }

            return false;
        }
    }
}