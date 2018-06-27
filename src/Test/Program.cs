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