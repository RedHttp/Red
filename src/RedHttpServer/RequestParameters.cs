using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;

namespace Red
{
    /// <summary>
    ///     Object containing the parameters for a request
    /// </summary>
    public sealed class RequestParameters
    {
        internal RequestParameters(HttpContext context)
        {
            _ctx = context;
        }

        private readonly HttpContext _ctx;

        /// <summary>
        ///     Get the request data fora given parameter
        /// </summary>
        /// <param name="paramId"></param>
        public string this[string paramId] => _ctx.GetRouteValue(paramId).ToString();

        public Dictionary<string, string> Values => _ctx.GetRouteData().Values.ToDictionary(x => x.Key, x => x.Value?.ToString());       
    }
}