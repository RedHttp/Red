using System;
using System.Collections.Generic;
using System.Linq;
using Red;
using Red.Request;
using Red.Response;

namespace CookieSessions
{
    public class Session<TSess>
    {
        public Session(TSess tsess, DateTime exp)
        {
            SessionData = tsess;
            Expires = exp;
        }

        public TSess SessionData { get; }
        public DateTime Expires { get; set; }
    }
    
    class CookieSessions : IRedMiddleware
    {
        private readonly HashSet<string> _excluded;

        public CookieSessions(IEnumerable<string> excluded)
        {
            _excluded = excluded.ToHashSet();
            _manager = new Sess
        }

        public void Initialize(RedHttpServer server)
        {
            throw new NotImplementedException();
        }

        public bool Handle(string path, HttpMethodEnum method, RRequest req, RResponse res)
        {
            if (_excluded.Contains(path) || !req.Cookies.ContainsKey())
                return true;
            var token = 
            
            var sessionData = 
            req.SetData();
            return true;
        }
    }
    
    public static class SessionManagerExtension
    {
        
        public static bool Authenticate(this RRequest instance, out TSess)
        {
            var token = instance.Cookies[]
                
                
            if (!_sessions.TryGetValue(token, out var s) || s.Expires <= DateTime.UtcNow)
            {
                data = default(TSess);
                return false;
            }
            data = s.SessionData;
            return true;
        }
    }
}