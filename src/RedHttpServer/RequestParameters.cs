using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Red
{
    /// <summary>
    ///     Object containing the parameters for a request
    /// </summary>
    public sealed class RequestParameters
    {
        internal RequestParameters(HttpContext context)
        {
            _context = context;
        }

        private readonly HttpContext _context;

        /// <summary>
        ///     Get the request data fora given parameter
        /// </summary>
        /// <param name="paramId"></param>
        public string this[string paramId] => _context.GetRouteValue(paramId.TrimStart(':')).ToString();

        /// <summary>
        ///     Returns the request route parameters as a dictionary
        /// </summary>
        public Dictionary<string, string> Values => _context.GetRouteData().Values.ToDictionary(x => x.Key, x => x.Value?.ToString());       
    }
}