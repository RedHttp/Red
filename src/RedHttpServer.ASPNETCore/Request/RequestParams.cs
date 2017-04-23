using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace RedHttpServerCore.Request
{
    /// <summary>
    ///     Object containing the parameters for a request
    /// </summary>
    public sealed class RequestParams
    {
        internal RequestParams(HttpContext context)
        {
            _ctx = context;
        }

        private readonly HttpContext _ctx;

        /// <summary>
        ///     Get the request data fora given parameter
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public string this[string paramId] => _ctx.GetRouteValue(paramId).ToString();
    }
}