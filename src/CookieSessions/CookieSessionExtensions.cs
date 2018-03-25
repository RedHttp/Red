namespace Red.CookieSessions
{
    public static class CookieSessionExtensions
    {
        /// <summary>
        ///     Opens a new session and adds cookie for authentication. 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sessionData"></param>
        public static void OpenSession(this Request request, object sessionData)
        {
            var manager = request.ServerPlugins.Get<SessionManager<Session>>();
            var session = new Session(sessionData, manager);
            var cookie = manager.OpenSession(session);
            request.SetData(session);
            request.UnderlyingRequest.HttpContext.Response.Headers.Add("Set-Cookie", cookie);
        }
        /// <summary>
        ///     Gets the session-object attached to the request, added by the CookieSessions middleware
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Session GetSession(this Request request)
        {
            return request.GetData<Session>();
        }
    }
}