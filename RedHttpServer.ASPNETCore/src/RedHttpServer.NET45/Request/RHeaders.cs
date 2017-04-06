using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace RHttpServer
{
    /// <summary>
    ///     Ease-of-use wrapper for request headers
    /// </summary>
    public sealed class RHeaders
    {
        internal RHeaders(NameValueCollection headers)
        {
            _headers = headers;
        }

        internal RHeaders()
        {
        }

        private readonly NameValueCollection _headers;

        /// <summary>
        ///     Tries to retrieve the content of a given header
        /// </summary>
        /// <param name="headerName"></param>
        /// <returns></returns>
        public string this[string headerName]
        {
            get { return _headers.Get(headerName); }
        }

        /// <summary>
        ///     Returns all name-value pairs as tuples with the name of the header first.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tuple<string, string>> GetAll()
        {
            var h = _headers.AllKeys;
            foreach (var s in h)
                yield return new Tuple<string, string>(s, _headers[s]);
        }
    }
}