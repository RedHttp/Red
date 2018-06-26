﻿using System;
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
        
      
        public JwtSessionSettings(TimeSpan sessionLength, string secret)
        {
            SessionLength = sessionLength;
            Secret = secret;
        }
    }
}