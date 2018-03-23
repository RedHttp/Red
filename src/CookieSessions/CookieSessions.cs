using System;
using System.Net;
using System.Threading.Tasks;
using Red;
using Red.Interfaces;

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
}