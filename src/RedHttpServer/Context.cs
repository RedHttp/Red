using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Red
{
    /// <summary>
    ///     Class that encapsulates data relevant to both Requests and Responses, and the HttpContext
    /// </summary>
    public sealed class Context
    {
        private readonly Lazy<Dictionary<Type, object>> _data = new Lazy<Dictionary<Type, object>>();
        private readonly Lazy<Dictionary<string, string>> _strings = new Lazy<Dictionary<string, string>>();

        /// <summary>
        ///     The ASP.NET HttpContext that is wrapped
        /// </summary>
        public readonly HttpContext AspNetContext;

        /// <summary>
        ///     Represent the url parameters and theirs values, contained in the path of the current request.
        /// </summary>
        public readonly UrlParameters Params;

        /// <summary>
        ///     The
        /// </summary>
        public readonly PluginCollection Plugins;

        /// <summary>
        ///     The Red.Request for this context
        /// </summary>
        public readonly Request Request;

        /// <summary>
        ///     The Red.Response for this context
        /// </summary>
        public readonly Response Response;

        internal Context(HttpContext aspNetContext, PluginCollection plugins)
        {
            Plugins = plugins;
            AspNetContext = aspNetContext;
            Request = new Request(this, aspNetContext.Request);
            Response = new Response(this, aspNetContext.Response);
            Params = new UrlParameters(aspNetContext);
        }

        /// <summary>
        /// </summary>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public string ParseLanguageHeader(string defaultLanguage = "en-UK")
        {
            return Request.TypedHeaders.AcceptLanguage.FirstOrDefault()?.Value.Value ?? defaultLanguage;
        }

        /// <summary>
        ///     Get data attached to request by middleware. The middleware should specify the type to lookup
        /// </summary>
        /// <param name="key">the data key</param>
        public string? GetData(string key)
        {
            return _strings.Value.TryGetValue(key, out var value) ? value : default;
        }

        /// <summary>
        ///     Get data attached to request by middleware. The middleware should specify the type to lookup
        /// </summary>
        /// <typeparam name="TData">the type key</typeparam>
        /// <returns>Object of specified type, registered to request. Otherwise default</returns>
        public TData? GetData<TData>()
            where TData : class
        {
            return _data.Value.TryGetValue(typeof(TData), out var value) ? (TData) value : default;
        }

        /// <summary>
        ///     Function that middleware can use to attach data to the request, so the next handlers has access to the data
        /// </summary>
        /// <typeparam name="TData">the type of the data object (implicitly)</typeparam>
        /// <param name="data">the data object</param>
        public void SetData<TData>(TData data)
            where TData : class
        {
            _data.Value[typeof(TData)] = data;
        }

        /// <summary>
        ///     Function that middleware can use to attach string values to the request, so the next handlers has access to the
        ///     data
        /// </summary>
        /// <param name="key">the data key</param>
        /// <param name="value">the data value</param>
        public void SetData(string key, string value)
        {
            _strings.Value[key] = value;
        }
    }
}