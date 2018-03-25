using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Red
{
    /// <summary>
    ///     Class representing a request from a client
    /// </summary>
    public sealed class Request
    {
        private readonly Dictionary<Type, object> _data = new Dictionary<Type, object>();

        internal Request(HttpRequest req, PluginCollection plugins)
        {
            UnderlyingRequest = req;
            Parameters = new RequestParameters(req.HttpContext);
            ServerPlugins = plugins;
        }

        /// <summary>
        /// The available plugins
        /// </summary>
        public PluginCollection ServerPlugins { get; }

        /// <summary>
        ///     The underlying HttpRequest
        ///     <para />
        ///     The implementation of Request is leaky, to avoid limiting you
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
        /// </summary>
        public Stream BodyStream => UnderlyingRequest.Body;

        /// <summary>
        ///     Returns form-data from request, if any, null otherwise. 
        /// </summary>
        public async Task<IFormCollection> GetFormDataAsync()
        {
            if (!UnderlyingRequest.HasFormContentType)
                return null;
            return await UnderlyingRequest.ReadFormAsync();
        }
        
        /// <summary>
        ///     Get data attached to request by middleware. The middleware should specify the type to lookup
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <returns>Object of specified type, registered to request. Otherwise default</returns>
        public TData GetData<TData>()
        {
            if (_data.TryGetValue(typeof(TData), out var data))
                return (TData) data;
            return default(TData);
        }
        /// <summary>
        ///     Function that middleware can use to attach data to the request, so the next handlers has access to the data
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="data"></param>
        public void SetData<TData>(TData data)
        {
            _data[typeof(TData)] = data;
        }
        
        /// <summary>
        ///     Save all files in requests to specified directory.
        /// </summary>
        /// <param name="saveDir">The directory to place the file(s) in</param>
        /// <param name="fileRenamer">Function to rename the file(s)</param>
        /// <param name="maxSizeKb">The max total filesize allowed</param>
        /// <returns>Whether the file(s) was saved succesfully</returns>
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