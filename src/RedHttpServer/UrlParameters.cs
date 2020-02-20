using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Red
{
    /// <summary>
    ///     The url parameters in the path of the request
    /// </summary>
    public sealed class UrlParameters
    {
        private readonly Func<string, object> _getRouteValue;
        private readonly Func<RouteData> _getRouteData;

        internal UrlParameters(HttpContext context)
        {
            _getRouteValue = context.GetRouteValue;
            _getRouteData = context.GetRouteData;
        }

        /// <summary>
        ///     Returns the value of a given parameter id
        /// </summary>
        /// <param name="parameterId"></param>
        public string? this[string parameterId] => _getRouteValue(parameterId.TrimStart(':'))?.ToString();

        /// <summary>
        ///     Extract all url parameters as a dictionary of parameter ids and parameter values
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string?> All()
        {
            return _getRouteData().Values.ToDictionary(x => x.Key, x => x.Value?.ToString());
        }
    }
}