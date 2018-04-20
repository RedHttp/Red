using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using Newtonsoft.Json;
using Red;
using Red.Interfaces;

namespace JwtSessions
{
    public class JwtSessions<TSession> : IRedMiddleware, IRedWebSocketMiddleware
    {
        /// <summary>
        /// Constructor for JwtSessions Middleware
        /// </summary>
        /// <param name="settings">Settings object</param>
        public JwtSessions(JwtSessionSettings settings)
        {
            _settings = settings;
        }
        
 
        private readonly JwtSessionSettings _settings;

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
            
            string token = null;
            string auth = req.Headers["Authorization"];

            if (string.IsNullOrEmpty(auth))
            {
                await Response.SendStatus(res, HttpStatusCode.Unauthorized);
                return false;
            }

            if (auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = auth.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(token))
            {
                await Response.SendStatus(res, HttpStatusCode.Unauthorized);
                return false;
            }
          
            if (!TryAuthenticateToken(token, out var session))
            {
                await Response.SendStatus(res, HttpStatusCode.Unauthorized);
                return false;
            }
            
            req.SetData(session.Data);
            return true;
        }

        /// <summary>
        ///     Do not invoke. Is invoked by the server with every request
        /// </summary>
        public async Task<bool> Process(string path, HttpMethodEnum method, Request req, Response res)
        {
            if (_settings.Excluded.Contains(path))
                return true;
            string token = null;
            string auth = req.Headers["Authorization"];

            if (string.IsNullOrEmpty(auth))
            {
                await res.SendStatus(HttpStatusCode.Unauthorized);
                return false;
            }

            if (auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = auth.Substring("Bearer ".Length).Trim();
            }
          
            if (string.IsNullOrEmpty(token) || !TryAuthenticateToken(token, out var session))
            {
                await res.SendStatus(HttpStatusCode.Unauthorized);
                return false;
            }
            
            req.SetData(session.Data);
            return true;
        }
        
        private bool TryAuthenticateToken(string authorization, out JwtSession data)
        {
            try
            {
                var json = new JwtBuilder()
                    .WithSecret(_settings.Secret)
                    .MustVerifySignature()
                    .Decode(authorization);
                data = JsonConvert.DeserializeObject<JwtSession>(json);
                return true;
            }
            catch (SignatureVerificationException)
            {
                data = null;
                return false;
            }
            catch (JsonSerializationException e)
            {
                data = null;
                return false;
            }
        }
        
        internal string NewSession(TSession sessionData)
        {
            var token = new JwtBuilder()
                .WithAlgorithm(_settings.Algoritm)
                .WithSecret(_settings.Secret)
                .AddClaim("exp", DateTimeOffset.UtcNow.Add(_settings.SessionLength).ToUnixTimeSeconds())
                .AddClaim("data", sessionData)
                .Build();
            
            return $"Bearer {token}";
        }

        internal class JwtSession
        {
            public TSession Data;
        }

        
    }
}