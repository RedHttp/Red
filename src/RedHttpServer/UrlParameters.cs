using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Red
{
    /// <summary>
    /// The url parameters in the path of the request
    /// </summary>
    public sealed class UrlParameters
    {
        private readonly HttpContext _context;

        internal UrlParameters(HttpContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Extract all url parameters as a dictionary of parameter ids and parameter values
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string?> All() => _context.GetRouteData().Values.ToDictionary(x => x.Key, x => x.Value?.ToString());   
        /// <summary>
        /// Returns the value of a given parameter id
        /// </summary>
        /// <param name="parameterId"></param>
        public string? this[string parameterId] => _context.GetRouteValue(parameterId.TrimStart(':'))?.ToString();
    }
}