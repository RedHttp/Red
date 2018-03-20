using System.Net;
using System.Threading.Tasks;
using Red;

namespace Red.CookieSessions
{
    public class CookieSessions : IRedMiddleware
    {
        private readonly CookieSessionSettings _settings;

        public CookieSessions(CookieSessionSettings settings)
        {
            _settings = settings;
        }

        public void Initialize(RedHttpServer server)
        {
            var manager = new SessionManager<Session>(_settings);
            server.Plugins.Register(manager);
        }

        public async Task<bool> Process(string path, HttpMethodEnum method, Request req, Response res, PluginCollection plugins)
        {
            if (_settings.Excluded.Contains(path))
                return true;
            if (!req.Cookies.ContainsKey(_settings.TokenName) || req.Cookies[_settings.TokenName] == "")
            {
                await res.SendStatus(HttpStatusCode.Unauthorized);
                return false;
            }

            var manager = plugins.Get<SessionManager<Session>>();
            if (!manager.TryAuthenticateToken(req.Cookies[_settings.TokenName], out var sessionData))
            {
                res.AddHeader("Set-Cookie", manager.ExpiredCookie);
                await res.SendStatus(HttpStatusCode.Unauthorized);
                return false;
            }
            req.SetData(sessionData);
            return true;
        }
        
    }
    public class Session
    {
        private readonly SessionManager<Session> _manager;
        public object Data;

        public Session(object sessionData, SessionManager<Session> manager)
        {
            Data = sessionData;
            _manager = manager;
        }

        public void Renew(Request request)
        {
            var cookie = _manager.RenewSession(request.Cookies[_manager.TokenName]);
            if (cookie != "")
                request.UnderlyingRequest.HttpContext.Response.Headers.Add("Set-Cookie", cookie);
        }

        public void Close(Request request)
        {
            if (_manager.CloseSession(request.Cookies[_manager.TokenName], out var cookie))
                request.UnderlyingRequest.HttpContext.Response.Headers.Add("Set-Cookie", cookie);
        }
    }

    public static class CookieSessionExtensions
    {
        public static void OpenSession(this Request request, object sessionData)
        {
            var manager = request.ServerPlugins.Get<SessionManager<Session>>();
            var session = new Session(sessionData, manager);
            var cookie = manager.OpenSession(session);
            request.UnderlyingRequest.HttpContext.Response.Headers.Add("Set-Cookie", cookie);
        }
        public static Session GetSession(this Request request)
        {
            return request.GetData<Session>();
        }
    }
    
}