using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Red;

namespace Test
{
    
    public class TestObject
    {
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
    }
    class MySess
    {
        public string Name { get; set; }
    }
    
    class Program
    {
        static async Task<HandlerType> Auth(Request req, Response res)
        {
            return HandlerType.Final;
        }
        static async Task<HandlerType> Auth(Request req, Response res, WebSocketDialog wsd)
        {
            return HandlerType.Final;
        }

        
        static async Task Main(string[] args)
        {
            var server = new RedHttpServer(5001);
            server.RespondWithExceptionDetails = true;
            server.Get("/exception", (req, res) => throw new Exception("oh no!"));
            server.Get("/index", Auth, (req, res) => res.SendFile("./index.html"));
            server.Get("/webm", (req, res) => res.SendFile("./Big_Buck_Bunny_alt.webm"));
            server.Get("/files/*", Utils.SendFiles("public/files"));
            
            var testObj = new TestObject{Prop1 = "Hello", Prop2 = 42};
            
            server.Get("/json", (req, res) => res.SendJson(testObj));
            server.Get("/xml", (req, res) => res.SendXml(testObj));
            
            TestRoutes.Register(server.CreateRouter("/test"));

            server.OnHandlerException += (e, sender) =>
            {
                Console.WriteLine(e);
            };
            
            server.WebSocket("/echo", Auth, async (req, res, wsd) =>
            {
                await wsd.SendText("Welcome to the echo test server");
                wsd.OnTextReceived += (sender, eventArgs) => { wsd.SendText("you sent: " + eventArgs.Text); };
            
                return wsd.Final();
            });
            await server.RunAsync();
        }
    }
}