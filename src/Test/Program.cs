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
            server.RespondWithExceptionDetails = true;

            async Task Auth(Request req, Response res, WebSocketDialog wsd = null)
            {
                
            }

            server.Get("/exception", async (req, res) => { throw new Exception("oh no!"); });

            server.WebSocket("/echo", Auth, async (req, res, wsd) =>
            {
                await wsd.SendText("Welcome to the echo test server");
                wsd.OnTextReceived += (sender, eventArgs) => { wsd.SendText("you sent: " + eventArgs.Text); };
            }, async (req, res, wsd) =>
            {
                
            });

            server.Start();
            Console.WriteLine("Hello World!");
            Console.Read();
        }
    }
}