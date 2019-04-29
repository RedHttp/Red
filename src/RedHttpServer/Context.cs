using System.Collections.Generic;
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
        public Dictionary<string, string> ExtractAllUrlParameters() => AspNetContext.GetRouteData().Values.ToDictionary(x => x.Key, x => x.Value?.ToString());   
    }
}