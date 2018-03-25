using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Red;
using Red.Interfaces;

namespace Red.CookieSessions
{
    public class SessionManager<TSess>
    {
        private readonly Random _random = new Random();
        private readonly string _cookie;
        private readonly TimeSpan _sessionLength;
        private readonly ConcurrentDictionary<string, Session> _sessions = new ConcurrentDictionary<string, Session>();
        private readonly RandomNumberGenerator _tokenGenerator = RandomNumberGenerator.Create();
        
        public readonly string TokenName;
        public readonly string ExpiredCookie;

        /// <summary>
        ///     Constructor for SessionManager
        /// </summary>
        /// <param name="settings">The settings for the cookie sessions</param>
        public SessionManager(CookieSessionSettings settings)
        {
            _sessionLength = settings.SessionLength;
            TokenName = settings.TokenName;
            var d = settings.Domain == "" ? "" : $" Domain={settings.Domain};";
            var p = settings.Path == "" ? "" : $" Path={settings.Path};";
            var h = settings.HttpOnly ? " HttpOnly;" : "";
            var s = settings.Secure ? " Secure;" : "";
            var ss = settings.SameSite == SameSiteSetting.None ? "" : $" SameSite={settings.SameSite};";
            _cookie = d + p + h + s + ss;
            ExpiredCookie = $"{settings.TokenName}=;{_cookie} Expires=Thu, 01 Jan 1970 00:00:00 GMT";
            Maintain();
        }

        // Simple maintainer loop
        private async void Maintain()
        {
            var delay = TimeSpan.FromMinutes(_sessionLength.TotalMinutes * 0.26);
            while (true)
            {
                await Task.Delay(delay);
                var now = DateTime.UtcNow;
                var expired = _sessions.Where(kvp => kvp.Value.Expires < now).ToList();
                foreach (var kvp in expired)
                    _sessions.TryRemove(kvp.Key, out var s);
            }
        }

        /// <summary>
        ///     Determines if a token is a valid authentication token. Returns session data object through out paramenter if token is valid.
        /// </summary>
        /// <param name="token">Token to authenticate</param>
        /// <param name="data">Session data if token is valid</param>
        /// <returns>True if token is valid</returns>
        public bool TryAuthenticateToken(string token, out TSess data)
        {
            if (!_sessions.TryGetValue(token, out var s) || s.Expires <= DateTime.UtcNow)
            {
                data = default(TSess);
                return false;
            }

            data = s.SessionData;
            return true;
        }

        /// <summary>
        /// Method for creating reasonably secure tokens
        /// </summary>
        /// <returns>A cryptographically strong token</returns>
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


        /// <summary>
        ///     Creates a new session and returns the cookie to send the client with 'Set-Cookie' header.
        /// </summary>
        /// <param name="sessionData">Object that represents the session data</param>
        /// <returns>The string to send with 'Set-Cookie' header</returns>
        public string OpenSession(TSess sessionData)
        {
            var id = GenerateToken();
            var exp = DateTime.UtcNow.Add(_sessionLength);
            _sessions.TryAdd(id, new Session(sessionData, exp));
            return $"{TokenName}={id};{_cookie} Expires={exp:R}";
        }

        /// <summary>
        ///     Renews the expiration and token of an active session and returns the cookie to send the client with 'Set-Cookie' header. 
        ///     Returns empty string if token invalid
        /// </summary>
        /// <param name="existingToken">The existing authentication token to replace</param>
        /// <returns>The string to send with 'Set-Cookie' header</returns>
        public string RenewSession(string existingToken)
        {
            if (!_sessions.TryRemove(existingToken, out var sess))
                return "";
            var newToken = GenerateToken();
            sess.Expires = DateTime.UtcNow.Add(_sessionLength);
            _sessions.TryAdd(newToken, sess);
            return $"{TokenName}={newToken};{_cookie} Expires={sess.Expires:R}";
        }

        /// <summary>
        ///     Closes an active session so the token becomes invalid. Returns true if an active session was found
        /// </summary>
        /// <param name="token">The authentication token to invalidate</param>
        /// <param name="cookie">The cookie to return, to invalidate the existing cookie</param>
        /// <returns>Whether the session was found and closed</returns>
        public bool CloseSession(string token, out string cookie)
        {
            cookie = ExpiredCookie;
            return _sessions.TryRemove(token, out var s);
        }

        private class Session
        {
            internal Session(TSess tsess, DateTime exp)
            {
                SessionData = tsess;
                Expires = exp;
            }

            public TSess SessionData { get; }
            public DateTime Expires { get; set; }
        }
    }
}