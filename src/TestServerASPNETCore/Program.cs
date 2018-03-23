using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Red.CookieSessions;
using EcsRendererPlugin;
using Red;

namespace TestServerASPNETCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // We serve static files, such as index.html from the 'public' directory
            var server = new RedHttpServer(5000, "public");
            server.Use(new EcsRenderer());
            server.Use(new CookieSessions(new CookieSessionSettings(TimeSpan.FromDays(1))
            {
                HttpOnly = false,
                Secure = false,
                Excluded = { "/login" }
            }));
            var startTime = DateTime.UtcNow;

            // URL param demo
            server.Get("/:param1/:paramtwo/:somethingthird", async (req, res) =>
            {
                await res.SendString($"URL: {req.Parameters["param1"]} / {req.Parameters["paramtwo"]} / {req.Parameters["somethingthird"]}");
            });

            server.Get("/login", async (req, res) =>
            {
                req.OpenSession(new {Name = "benny"});
                await res.SendString("ok");
            });
            
            // Redirect to page on same host
            server.Get("/redirect", async (req, res) =>
            {
                await res.Redirect("/redirect/test/here");
            });

            // Save uploaded file from request body 
            Directory.CreateDirectory("uploads");
            server.Post("/upload", async (req, res) =>
            {
                if (await req.SaveFiles("uploads"))
                    await res.SendString("OK");
                else
                    await res.SendString("Error", status: HttpStatusCode.NotAcceptable);
            });

            server.Get("/file", async (req, res) =>
            {
                await res.SendFile("testimg.jpeg");
            });

            // Handling formdata from client
            server.Post("/formdata", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                await res.SendString("Hello " + form["firstname"]);
            });

            // Using url queries to generate an answer
            server.Get("/hello", async (req, res) =>
            {
                Console.WriteLine(req.GetSession().Data);
                var queries = req.Queries;
                await res.SendString($"Hello {queries["firstname"]} {queries["lastname"]}, have a nice day");
            });

            // Rendering a page for dynamic content
            server.Get("/serverstatus", async (req, res) =>
            {
                try
                {
                    await res.RenderPage("pages/statuspage.ecs", new RenderParams
                    {
                        { "uptime", DateTime.UtcNow.Subtract(startTime).TotalMinutes },
                        { "version", RedHttpServer.Version }
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            });

            // WebSocket echo server
            server.WebSocket("/echo", async (req, wsd) =>
            {
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
