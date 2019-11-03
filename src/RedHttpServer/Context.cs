using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Red
{
    /// <summary>
    /// Class that encapsulates data relevant to both Requests and Responses, and the HttpContext
    /// </summary>
    public sealed class Context
    {
        private readonly Lazy<HybridDictionary> _data = new Lazy<HybridDictionary>();
        private readonly Lazy<StringDictionary> _strings = new Lazy<StringDictionary>();
        /// <summary>
        ///    The Red.Request for this context
        /// </summary>
        public readonly Request Request;
        
        /// <summary>
        ///    The Red.Response for this context
        /// </summary>
        public readonly Response Response;
        
        /// <summary>
        ///     The ASP.NET HttpContext that is wrapped
        /// </summary>
        public readonly HttpContext AspNetContext;
        
        /// <summary>
        ///    The 
        /// </summary>
        public readonly PluginCollection Plugins;
        
        internal Context(HttpContext aspNetContext, PluginCollection plugins)
        {
            Plugins = plugins;
            AspNetContext = aspNetContext;
            Request = new Request(this, aspNetContext.Request);
            Response = new Response(this, aspNetContext.Response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
        public string ParseLanguageHeader(string defaultLanguage = "en-UK")
        {
            return Request.TypedHeaders.AcceptLanguage.FirstOrDefault()?.Value.Value ?? defaultLanguage;
        }

        /// <summary>
        ///     Returns the value of a named URL parameter
        /// </summary>
        /// <param name="paramId"></param>
        public string ExtractUrlParameter(string paramId)
        {
            return AspNetContext.GetRouteValue(paramId.TrimStart(':')).ToString();
        }

        /// <summary>
        ///     Returns all the values embedded in the route using the names of the URL parameters as keys
        /// </summary>
        public Dictionary<string, string?> ExtractAllUrlParameters() => AspNetContext.GetRouteData().Values.ToDictionary(x => x.Key, x => x.Value?.ToString());   
        
        
        /// <summary>
        ///     Get data attached to request by middleware. The middleware should specify the type to lookup
        /// </summary>
        /// <param name="key">the data key</param>
        public string? GetData(string key)
        {
            return key != null && _strings.Value.ContainsKey(key) ? _strings.Value[key] : default;
        }
        /// <summary>
        ///     Get data attached to request by middleware. The middleware should specify the type to lookup
        /// </summary>
        /// <typeparam name="TData">the type key</typeparam>
        /// <returns>Object of specified type, registered to request. Otherwise default</returns>
        public TData? GetData<TData>()
            where TData : class
        {
            return (TData) _data.Value[typeof(TData)];
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
        ///     Function that middleware can use to attach string values to the request, so the next handlers has access to the data
        /// </summary>
        /// <param name="key">the data key</param>
        /// <param name="value">the data value</param>
        public void SetData(string key, string value)
        {
            _strings.Value[key] = value;
        }

    }
}