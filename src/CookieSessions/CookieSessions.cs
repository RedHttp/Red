using System;
using System.Net;
using System.Threading.Tasks;
using Red;
using Red.Interfaces;

namespace Red.CookieSessions
{
    /// <summary>
    ///     RedMiddleware for CookieSessions
    /// </summary>
    public class CookieSessions : IRedMiddleware, IRedWebSocketMiddleware
    {
        private readonly CookieSessionSettings _settings;

        /// <summary>
        /// Constructor for CookieSession Middleware
        /// </summary>
        /// <param name="settings"></param>
        public CookieSessions(CookieSessionSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        ///     Do not invoke. Is invoked by the server when it starts. 
        /// </summary>
        public void Initialize(RedHttpServer server)
        {
            var manager = new SessionManager<Session>(_settings);
            server.Plugins.Register(manager);
        }

        /// <summary>
        ///     Do not invoke. Is invoked by the server with every websocket request
        /// </summary>
        public async Task<bool> Process(string path, Request req, WebSocketDialog wsd)
        {
            if (_settings.Excluded.Contains(path))
                return true;
            var res = req.UnderlyingRequest.HttpContext.Response;
            if (!req.Cookies.ContainsKey(_settings.TokenName) || req.Cookies[_settings.TokenName] == "")
            {
                await Response.SendStatus(res, HttpStatusCode.Unauthorized);
                return false;
            }

            var manager = req.ServerPlugins.Get<SessionManager<Session>>();
            if (!manager.TryAuthenticateToken(req.Cookies[_settings.TokenName], out var sessionData))
            {
                res.Headers.Add("Set-Cookie", manager.ExpiredCookie);
                await Response.SendStatus(res, HttpStatusCode.Unauthorized);
                return false;
            }
            req.SetData(sessionData);
            return true;
        }

        /// <summary>
        ///     Do not invoke. Is invoked by the server with every request
        /// </summary>
        public async Task<bool> Process(string path, HttpMethodEnum method, Request req, Response res)
        {
            if (_settings.Excluded.Contains(path))
                return true;
            if (!req.Cookies.ContainsKey(_settings.TokenName) || req.Cookies[_settings.TokenName] == "")
            {
                await res.SendStatus(HttpStatusCode.Unauthorized);
                return false;
            }

            var manager = req.ServerPlugins.Get<SessionManager<Session>>();
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