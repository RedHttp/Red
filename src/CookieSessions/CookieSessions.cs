using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Red;
using Red.Interfaces;

namespace Red.CookieSessions
{
    /// <summary>
    ///     RedMiddleware for CookieSessions
    /// </summary>
    public class CookieSessions<TSession> : IRedMiddleware, IRedWebSocketMiddleware
    {
        /// <summary>
        /// Constructor for CookieSession Middleware
        /// </summary>
        /// <param name="settings">Settings object</param>
        public CookieSessions(CookieSessionSettings settings)
        {
            _settings = settings;
            _tokenName = settings.TokenName;
            var d = settings.Domain == "" ? "" : $" Domain={settings.Domain};";
            var p = settings.Path == "" ? "" : $" Path={settings.Path};";
            var h = settings.HttpOnly ? " HttpOnly;" : "";
            var s = settings.Secure ? " Secure;" : "";
            var ss = settings.SameSite == SameSiteSetting.None ? "" : $" SameSite={settings.SameSite};";
            _cookie = d + p + h + s + ss;
            _expiredCookie = $"{settings.TokenName}=;{_cookie} Expires=Thu, 01 Jan 1970 00:00:00 GMT; Max-Age={(int)settings.SessionLength.TotalSeconds};";
            Maintain();
        }
        
        private readonly Random _random = new Random();
        private readonly string _cookie;
        private readonly ConcurrentDictionary<string, CookieSession> _sessions = new ConcurrentDictionary<string, CookieSession>();
        private readonly RandomNumberGenerator _tokenGenerator = RandomNumberGenerator.Create();

        private readonly string _tokenName;
        private readonly string _expiredCookie;
        private CookieSessionSettings _settings;

        /// <summary>
        ///     Do not invoke. Is invoked by the server when it starts. 
        /// </summary>
        public void Initialize(RedHttpServer server)
        {
            server.Plugins.Register(this);
        }

        /// <summary>
        ///     Do not invoke. Is invoked by the server with every websocket request
        /// </summary>
        public async Task<bool> Process(string path, Request req, WebSocketDialog wsd)
        {
            if (_settings.Excluded.Contains(path))
                return true;
            var res = req.UnderlyingRequest.HttpContext.Response;
            if (!req.Cookies.ContainsKey(_tokenName) || req.Cookies[_tokenName] == "")
            {
                await Response.SendStatus(res, HttpStatusCode.Unauthorized);
                return false;
            }

            if (!TryAuthenticateToken(req.Cookies[_tokenName], out var session))
            {
                res.Headers.Add("Set-Cookie", _expiredCookie);
                await Response.SendStatus(res, HttpStatusCode.Unauthorized);
                return false;
            }
            req.SetData(session.Data);
            if (_settings.AutoRenew) 
                session.Renew(req);
            return true;
        }

        /// <summary>
        ///     Do not invoke. Is invoked by the server with every request
        /// </summary>
        public async Task<bool> Process(string path, HttpMethodEnum method, Request req, Response res)
        {
            if (_settings.Excluded.Contains(path))
                return true;
            if (!req.Cookies.ContainsKey(_tokenName) || req.Cookies[_tokenName] == "")
            {
                await res.SendStatus(HttpStatusCode.Unauthorized);
                return false;
            }

            if (!TryAuthenticateToken(req.Cookies[_tokenName], out var session))
            {
                res.AddHeader("Set-Cookie", _expiredCookie);
                await res.SendStatus(HttpStatusCode.Unauthorized);
                return false;
            }
            if (_settings.AutoRenew)
                session.Renew(req);
            req.SetData(session);
            return true;
        }
        
        // Simple maintainer loop
        private async void Maintain()
        {
            var delay = TimeSpan.FromSeconds(_settings.SessionLength.TotalSeconds * 0.20);
            while (true)
            {
                await Task.Delay(delay);
                var now = DateTime.UtcNow;
                var expired = _sessions.Where(kvp => kvp.Value.Expires < now).ToList();
                foreach (var kvp in expired)
                    _sessions.TryRemove(kvp.Key, out var s);
            }
        }

        private bool TryAuthenticateToken(string token, out CookieSession data)
        {
            if (!_sessions.TryGetValue(token, out var s) || s.Expires <= DateTime.UtcNow)
            {
                data = null;
                return false;
            }

            data = s;
            return true;
        }
        
        private string GenerateToken()
        {
            var data = new byte[32];
            _tokenGenerator.GetBytes(data);
            var b64 = Convert.ToBase64String(data);
            var id = new StringBuilder(b64, 46);
            id.Replace('+', (char) _random.Next(97, 122));
            id.Replace('=', (char) _random.Next(97, 122));
            id.Replace('/', (char) _random.Next(97, 122));
            return id.ToString();
        }

        internal string OpenSession(TSession sessionData)
        {
            var id = GenerateToken();
            var exp = DateTime.UtcNow.Add(_settings.SessionLength);
            _sessions.TryAdd(id, new CookieSession(sessionData, exp, this));
            return $"{_tokenName}={id};{_cookie} Expires={exp:R}";
        }

        private string RenewSession(string existingToken)
        {
            if (!_sessions.TryRemove(existingToken, out var sess))
                return "";
            var newToken = GenerateToken();
            sess.Expires = DateTime.UtcNow.Add(_settings.SessionLength);
            _sessions.TryAdd(newToken, sess);
            return $"{_tokenName}={newToken};{_cookie} Expires={sess.Expires:R}";
        }

        private bool CloseSession(string token, out string cookie)
        {
            cookie = _expiredCookie;
            return _sessions.TryRemove(token, out var s);
        }

        public class CookieSession
        {
            private readonly CookieSessions<TSession> _manager;

            internal CookieSession(TSession tsess, DateTime exp, CookieSessions<TSession> manager)
            {
                _manager = manager;
                Data = tsess;
                Expires = exp;
            }

            public TSession Data { get; }
            public DateTime Expires { get; internal set; }
            
            
            /// <summary>
            ///    Renews the session expiry time and updates the cookie
            /// </summary>
            /// <param name="request"></param>
            public void Renew(Request request)
            {
                var cookie = _manager.RenewSession(request.Cookies[_manager._tokenName]);
                if (cookie != "")
                    request.UnderlyingRequest.HttpContext.Response.Headers.Add("Set-Cookie", cookie);
            }

            /// <summary>
            ///    Closes the session and updates the cookie
            /// </summary>
            /// <param name="request"></param>
            public void Close(Request request)
            {
                if (_manager.CloseSession(request.Cookies[_manager._tokenName], out var cookie))
                    request.UnderlyingRequest.HttpContext.Response.Headers.Add("Set-Cookie", cookie);
            }
        }
        
    }
}