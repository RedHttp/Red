namespace Red.CookieSessions
{
    /// <summary>
    ///     Represents a currently active session.
    /// </summary>
    public class Session
    {
        private readonly SessionManager<Session> _manager;
        public object Data;

        public Session(object sessionData, SessionManager<Session> manager)
        {
            Data = sessionData;
            _manager = manager;
        }

        /// <summary>
        ///    Renews the session expiry time and updates the cookie
        /// </summary>
        /// <param name="request"></param>
        public void Renew(Request request)
        {
            var cookie = _manager.RenewSession(request.Cookies[_manager.TokenName]);
            if (cookie != "")
                request.UnderlyingRequest.HttpContext.Response.Headers.Add("Set-Cookie", cookie);
        }

        /// <summary>
        ///    Closes the session and updates the cookie
        /// </summary>
        /// <param name="request"></param>
        public void Close(Request request)
        {
            if (_manager.CloseSession(request.Cookies[_manager.TokenName], out var cookie))
                request.UnderlyingRequest.HttpContext.Response.Headers.Add("Set-Cookie", cookie);
        }
    }
}