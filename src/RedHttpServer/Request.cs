using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Red
{
    /// <summary>
    ///     Class representing a request from a client
    /// </summary>
    public sealed class Request
    {
        private readonly Lazy<Dictionary<Type, object>> _data = new Lazy<Dictionary<Type, object>>();
        private readonly Lazy<Dictionary<string, string>> _strings = new Lazy<Dictionary<string, string>>();
        private IFormCollection _form;

        internal Request(HttpContext context, PluginCollection plugins)
        {
            Context = context;
            Parameters = new RequestParameters(context);
            ServerPlugins = plugins;
        }


        /// <summary>
        ///     The underlying HttpContext
        ///     <para />
        ///     The implementation of Request is leaky, to avoid limiting you
        /// </summary>
        public readonly HttpContext Context;

        /// <summary>
        /// The available plugins
        /// </summary>
        public readonly PluginCollection ServerPlugins;


        /// <summary>
        ///     The url parameters of the request
        /// </summary>
        public readonly RequestParameters Parameters;

        /// <summary>
        ///     The query elements of the request
        /// </summary>
        public IQueryCollection Queries => Context.Request.Query;

        /// <summary>
        ///     The headers contained in the request
        /// </summary>
        public IHeaderDictionary Headers => Context.Request.Headers;

        /// <summary>
        ///     The cookies contained in the request
        /// </summary>
        public IRequestCookieCollection Cookies => Context.Request.Cookies;

        /// <summary>
        ///     Returns the body stream of the request
        /// </summary>
        public Stream BodyStream => Context.Request.Body;

        /// <summary>
        ///     Returns form-data from request, if any, null otherwise. 
        /// </summary>
        public async Task<IFormCollection> GetFormDataAsync()
        {
            if (!Context.Request.HasFormContentType)
                return null;

            if (_form != null)
                return _form;

            _form =  await Context.Request.ReadFormAsync();
            return _form;
        }
        
        /// <summary>
        ///     Get data attached to request by middleware. The middleware should specify the type to lookup
        /// </summary>
        /// <param name="key">the data key</param>
        public string GetData(string key)
        {
            return _strings.Value.TryGetValue(key, out var data) ? data : default;
        }
        /// <summary>
        ///     Get data attached to request by middleware. The middleware should specify the type to lookup
        /// </summary>
        /// <typeparam name="TData">the type key</typeparam>
        /// <returns>Object of specified type, registered to request. Otherwise default</returns>
        public TData GetData<TData>()
        {
            if (_data.Value.TryGetValue(typeof(TData), out var data))
                return (TData) data;
            return default;
        }
        /// <summary>
        ///     Function that middleware can use to attach data to the request, so the next handlers has access to the data
        /// </summary>
        /// <typeparam name="TData">the type of the data object (implicitly)</typeparam>
        /// <param name="data">the data object</param>
        public void SetData<TData>(TData data)
        {
            _data.Value[typeof(TData)] = data;
        }
        /// <summary>
        ///     Function that middleware can use to attach string values to the request, so the next handlers has access to the data
        /// </summary>
        /// <param name="key">the data key</param>
        /// <param name="value">the data value</param>
        public void SetData(string key, string value)
        {
            _strings.Value[key] = value;
        }
        
        /// <summary>
        ///     Save all files in requests to specified directory.
        /// </summary>
        /// <param name="saveDir">The directory to place the file(s) in</param>
        /// <param name="fileRenamer">Function to rename the file(s)</param>
        /// <param name="maxSizeKb">The max total filesize allowed</param>
        /// <returns>Whether the file(s) was saved successfully</returns>
        public async Task<bool> SaveFiles(string saveDir, Func<string, string> fileRenamer = null,
            long maxSizeKb = 50000)
        {
            if (!Context.Request.HasFormContentType) return false;
            var form = await Context.Request.ReadFormAsync();
            if (form.Files.Sum(file => file.Length) > maxSizeKb << 10)
                return false;

            var fullSaveDir = Path.GetFullPath(saveDir);
            foreach (var formFile in form.Files)
            {
                var filename = fileRenamer == null ? formFile.FileName : fileRenamer(formFile.FileName);
                filename = Path.GetFileName(filename);
                if (string.IsNullOrWhiteSpace(filename))
                {
                    continue;
                }
                var filepath = Path.Combine(fullSaveDir, filename);
                using (var fileStream = File.Create(filepath))
                {
                    await formFile.CopyToAsync(fileStream);
                }
            }

            return true;

        }
    }
}