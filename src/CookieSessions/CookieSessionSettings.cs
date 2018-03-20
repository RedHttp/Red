using System;
using System.Collections.Generic;

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
        public readonly HashSet<string> Excluded = new HashSet<string>();

        public CookieSessionSettings(TimeSpan sessionLength)
        {
            SessionLength = sessionLength;
        }
    }
}