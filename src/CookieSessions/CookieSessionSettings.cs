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

   

        public CookieSessionSettings(TimeSpan sessionLength)
        {
            SessionLength = sessionLength;
        }
    }
}