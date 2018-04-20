namespace Red.CookieSessions
{
    public static class CookieSessionExtensions
    {
        /// <summary>
        ///     Opens a new session and adds cookie for authentication. 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sessionData"></param>
        public static void OpenSession<TSession>(this Request request, TSession sessionData)
        {
            var manager = request.ServerPlugins.Get<CookieSessions<TSession>>();
            var cookie = manager.OpenSession(sessionData);
            request.UnderlyingRequest.HttpContext.Response.Headers.Add("Set-Cookie", cookie);
        }
        /// <summary>
        ///     Gets the session-object attached to the request, added by the CookieSessions middleware
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static CookieSessions<TSession>.CookieSession GetSession<TSession>(this Request request)
        {
            return request.GetData<CookieSessions<TSession>.CookieSession>();
        }
    }
}