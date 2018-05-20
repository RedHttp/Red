using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using JWT.Algorithms;
using Red;

namespace JwtSessions
{
    public class JwtSessionSettings
    {
        /// <summary>
        /// Algorithm used by Jwt-library. Defaults to HMAC-SHA256-A1
        /// </summary>
        public IJwtAlgorithm Algoritm { get; set; } = new HMACSHA256Algorithm();
        
        public TimeSpan SessionLength { get; }
        public string Secret { get; }
        
        /// <summary>
        /// Function that is passed the requested path string for every request, so it can determine whether the request should be authenticated.
        /// Defaults to requiring authentication on all paths but '/login'.
        /// </summary>
        public Func<string, bool> ShouldAuthenticate { get; set; } = path => path != "/login";


        /// <summary>
        /// Function that is called for a request without valid authentication.
        /// Defaults to 
        /// </summary>
        public Func<Request, Response, Task> OnNotAuthenticated { get; set; } = (req, res) =>
        {
            res.Closed = true;
            return res.SendStatus(HttpStatusCode.Unauthorized);
        };

        public JwtSessionSettings(TimeSpan sessionLength, string secret)
        {
            SessionLength = sessionLength;
            Secret = secret;
        }
    }
}