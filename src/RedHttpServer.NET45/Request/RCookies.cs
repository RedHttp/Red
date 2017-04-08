using System;
using System.Net;
using RedHttpServer.Logging;

namespace RedHttpServer.Request
{
    /// <summary>
    ///     Ease-of-use wrapper for request cookies
    /// </summary>
    public sealed class RCookies
    {
        internal RCookies(CookieCollection cocol)
        {
            _cookies = cocol;
        }

        internal RCookies()
        {
        }

        private readonly CookieCollection _cookies;

        /// <summary>
        ///     Returns the cookie with the given tag if any
        /// </summary>
        /// <param name="cookieId"></param>
        public Cookie this[string cookieId]
        {
            get
            {
                try
                {
                    return _cookies[cookieId];
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return null;
                }
            }
        }
    }
}