using System;
using System.IO;
using RedHttpServerCore;
using RedHttpServerCore.Plugins;
using RedHttpServerCore.Plugins.Interfaces;
using RedHttpServerCore.Response;

namespace TestServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // We serve static files, such as index.html from the 'public' directory
            var server = new RedHttpServer(5000, "public");
            var startTime = DateTime.UtcNow;

            // We log to terminal here
            var logger = new TerminalLogging();
            server.Plugins.Register<ILogging, TerminalLogging>(logger);

            // URL param demo
            server.Get("/:param1/:paramtwo/:somethingthird", (req, res) =>
            {
                res.SendString($"URL: {req.Params["param1"]} / {req.Params["paramtwo"]} / {req.Params["somethingthird"]}");
            });

            // Redirect to page on same host
            server.Get("/redirect", (req, res) =>
            {
                res.Redirect("/redirect/test/here");
            });
            
            // Save uploaded file from request body 
            Directory.CreateDirectory("./uploads");
            server.Post("/upload", async (req, res) =>
            {
                if (await req.SaveBodyToFile("./uploads"))
                {
                    res.SendString("OK");
                    // We can use logger reference directly
                    logger.Log("UPL", "File uploaded");
                }
                else
                    res.SendString("Error", status: 413);
            });

            // Using url queries to generate an answer
            server.Get("/hello", (req, res) =>
            {
                var queries = req.Queries;
                var firstname = queries["firstname"];
                var lastname = queries["lastname"];
                res.SendString($"Hello {firstname} {lastname}, have a nice day");
            });

            // Rendering a page for dynamic content
            server.Get("/serverstatus", (req, res) =>
            {
                res.RenderPage("./pages/serverstates.ecs", new RenderParams
                {
                    { "uptime", DateTime.UtcNow.Subtract(startTime).TotalHours },
                    { "versiom", RedHttpServer.Version }
                });
            });

            // WebSocket echo server
            server.WebSocket("/echo", async (req, wsd) =>
            {
                // Or we can use the logger from the plugin collection 
                wsd.ServerPlugins.Use<ILogging>().Log("WS", "Echo server visited");

                await wsd.SendText("Welcome to the echo test server");
                wsd.OnTextReceived += (sender, eventArgs) =>
                {
                    wsd.SendText("you sent: " + eventArgs.Text);
                };
            });


            server.Start();
            Console.ReadKey();
        }
    }
}
