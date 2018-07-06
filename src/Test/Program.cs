using System;
using System.Net;
using System.Threading.Tasks;
using Red;

namespace Test
{
    class MySess
    {
        public string Name { get; set; }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var server = new RedHttpServer(5000);
            var s = new CookieSessions<MySess>();
            server.Use(new CookieSessions<TSession>());;
            
            Func<Request, Response, Task> auth = async (req, res) =>
            {
                if (true)
                {
                    await res.SendStatus(HttpStatusCode.Unauthorized);
                    res.Closed = true;
                }
            };
            
            server.Get("/login", async (req, res) =>
            {
                req
            });
            
            server.Get("/user", auth, async (req, res) =>
            {
                
            });
            server.WebSocket("/serser", async (req, wsd, res) =>
            {
                
            });
            
            Console.WriteLine("Hello World!");
        }
    }
}