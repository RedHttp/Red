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
        public async Task Process(Request req, WebSocketDialog wsd, Response res)
        {
            string token = null;
            string auth = req.Headers["Authorization"];

            if (string.IsNullOrEmpty(auth))
            {
                return;
            }

            if (auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = auth.Substring("Bearer ".Length).Trim();
            }
            
            if (string.IsNullOrEmpty(token) || !TryAuthenticateToken(token, out var session))
            {
                return;
            }
            
            req.SetData(session.Data);
        }

        /// <summary>
        ///     Do not invoke. Is invoked by the server with every request
        /// </summary>
        public async Task Process(Request req, Response res)
        {
            string token = null;
            string auth = req.Headers["Authorization"];

            if (string.IsNullOrEmpty(auth))
            {
                return;
            }

            if (auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = auth.Substring("Bearer ".Length).Trim();
            }
          
            if (string.IsNullOrEmpty(token) || !TryAuthenticateToken(token, out var session))
            {
                return;
            }
            
            req.SetData(session.Data);
        }
        
        private bool TryAuthenticateToken(string authorization, out JwtSession data)
        {
            try
            {
                var json = new JwtBuilder()
                    .WithSecret(_settings.Secret)
                    .WithAlgorithm(_settings.Algoritm)
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
                .WithSecret(_settings.Secret)
                .WithAlgorithm(_settings.Algoritm)
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