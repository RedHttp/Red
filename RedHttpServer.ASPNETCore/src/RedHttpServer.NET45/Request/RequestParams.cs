using System.Collections.Generic;

namespace RHttpServer
{
    /// <summary>
    ///     Object containing the parameters for a request
    /// </summary>
    public sealed class RequestParams
    {
        internal RequestParams(Dictionary<string, string> dict)
        {
            _dict = dict;
        }

        internal RequestParams()
        {
        }

        private readonly Dictionary<string, string> _dict;

        /// <summary>
        ///     Get the request data fora given parameter
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public string this[string paramId]
        {
            get
            {
                string v;
                return _dict.TryGetValue(paramId, out v) ? v : "";
            }
        }
    }
}