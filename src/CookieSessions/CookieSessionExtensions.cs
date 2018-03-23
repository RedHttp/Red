namespace Red.CookieSessions
{
    public static class CookieSessionExtensions
    {
        public static void OpenSession(this Request request, object sessionData)
        {
            var manager = request.ServerPlugins.Get<SessionManager<Session>>();
            var session = new Session(sessionData, manager);
            var cookie = manager.OpenSession(session);
            request.SetData(session);
            request.UnderlyingRequest.HttpContext.Response.Headers.Add("Set-Cookie", cookie);
        }
        public static Session GetSession(this Request request)
        {
            return request.GetData<Session>();
        }
    }
}