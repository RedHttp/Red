using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace Red.CookieSessions
{
    public class CookieSessionSettings
    {
        public TimeSpan SessionLength;
        public string Domain = "";
        public string Path = "";
        public bool HttpOnly = true;
        public bool Secure = true;
        public SameSiteSetting SameSite = SameSiteSetting.Strict;
        public string TokenName = "session_token";

        /// <summary>
        /// Renew session on each authenticated request
        /// </summary>
        public bool AutoRenew = false;

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

        /// <summary>
        /// Whether the list property should be used as a whitelist. As opposed to a blacklist
        /// </summary>
        public bool Whitelist { get; set; } = false;

        public CookieSessionSettings(TimeSpan sessionLength)
        {
            SessionLength = sessionLength;
        }
    }
}