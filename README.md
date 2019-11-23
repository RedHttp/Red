# RedHttpServer
### Cross-platform http server framework with websocket support
[![GitHub](https://img.shields.io/github/license/redhttp/red)](https://github.com/RedHttp/Red/blob/master/LICENSE.md)
[![Nuget](https://img.shields.io/nuget/v/rhttpserver)](https://www.nuget.org/packages/RHttpServer/)
[![Nuget](https://img.shields.io/nuget/dt/rhttpserver)](https://www.nuget.org/packages/RHttpServer/)
![Dependent repos (via libraries.io)](https://img.shields.io/librariesio/dependent-repos/nuget/rhttpserver)

A .NET Standard web application framework built on ASP.NET Core w/ Kestrel and inspired by the simplicity of Express.js

- [Homepage](https://redhttp.github.io/Red/)
- [Documentation](https://redhttp.github.io/Red/doxygen/)

### Installation
RedHttpServer can be installed from [NuGet](https://www.nuget.org/packages/RHttpServer/): `Install-Package RHttpServer`

## Middleware and plugins
RedHttpServer is created to be easy to build on top of. 
The server supports both middleware modules and extension modules

* JsonConverter - uses System.Text.Json
* XmlConverter - uses System.Xml.Serialization
* BodyParser - uses both the Json- and Xml converter to parse request body to an object, depending on content-type.

More extensions and middleware
- [CookieSessions](https://github.com/RedHttp/Red.CookieSessions) simple session management middleware that uses cookies with authentication tokens.
- [JwtSessions](https://github.com/RedHttp/Red.JwtSessions) simple session management middleware that uses JWT tokens - uses [Jwt.Net](https://github.com/jwt-dotnet/jwt)
- [Validation](https://github.com/RedHttp/Red.Validation) build validators for forms and queries using a fluent API
- [EcsRenderer](https://github.com/RedHttp/Red.EcsRenderer) simple template rendering extension. See more info about the format by clicking the link.
- [CommonMarkRenderer](https://github.com/RedHttp/Red.CommonMarkRenderer) simple CommonMark/Markdown renderer extension - uses [CommonMark.NET](https://github.com/Knagis/CommonMark.NET)
- [HandlebarsRenderer](https://github.com/RedHttp/Red.HandlebarsRenderer) simple Handlebars renderer extension - uses [Handlebars.Net](https://github.com/rexm/Handlebars.Net)


### Example
```csharp
var server = new RedHttpServer(5000, "public");
server.Use(new EcsRenderer());
server.Use(new CookieSessions<MySession>(TimeSpan.FromDays(1)));

// Authentication middleware
async Task<HandlerType> Auth(Request req, Response res)
{
    if (req.GetData<MySession>() != null)
    {
        return HandlerType.Continue;
    }

    await res.SendStatus(HttpStatusCode.Unauthorized);
    return HandlerType.Final;
}

var startTime = DateTime.UtcNow;

// Url parameters
server.Get("profile/:username", Auth, (req, res) =>
{
    var username = req.Context.ExtractUrlParameter("username");
    // ... lookup username in database or similar and fetch profile ...
    var user = new { FirstName = "John", LastName = "Doe", Username = username };
    return res.SendJson(user);
});

// Using forms
server.Post("/login", async (req, res) =>
{
    var form = await req.GetFormDataAsync();
    // ... some validation and authentication ...
    await res.OpenSession(new MySession { Username = form["username"] });
    return await res.SendStatus(HttpStatusCode.OK);
});

server.Post("/logout", Auth, async (req, res) =>
{
    var session = req.GetData<MySession>();
    await res.CloseSession(session);
    return await res.SendStatus(HttpStatusCode.OK);
});

// Simple redirects
server.Get("/redirect", Auth, (req, res) => res.Redirect("/redirect/test/here"));

// File uploads
Directory.CreateDirectory("uploads");
server.Post("/upload", async (req, res) =>
{
    if (await req.SaveFiles("uploads"))
        return await res.SendString("OK");
    else
        return await res.SendString("Error", status: HttpStatusCode.NotAcceptable);
});

server.Get("/file", (req, res) => res.SendFile("somedirectory/animage.jpg"));

// Using url queries
server.Get("/search", Auth, (req, res) =>
{
    string searchQuery = req.Queries["query"];
    string format = req.Queries["format"];
    // ... perform search using searchQuery and return results ...
    var results = new[] { "Apple", "Pear" };

    if (format == "xml")
        return res.SendXml(results);
    else
        return res.SendJson(results);
});

// Markdown rendering
server.Get("/markdown", (req, res) => res.RenderFile("markdown.md"));

// Esc rendering
server.Get("/serverstatus", async (req, res) => await res.RenderPage("pages/statuspage.ecs",
    new RenderParams
    {
        { "uptime", (int) DateTime.UtcNow.Subtract(startTime).TotalSeconds },
        { "version", Red.RedHttpServer.Version }
    }));

// Using websockets
server.WebSocket("/echo", async (req, wsd) =>
{
    wsd.SendText("Welcome to the echo test server");
    wsd.OnTextReceived += (sender, eventArgs) => { wsd.SendText("you sent: " + eventArgs.Text); };
    return HandlerType.Final;
});

// Keep the program running easily (async Program.Main - C# 7.1+)
await server.RunAsync();
```

## Why?
Because I like C# and .NET Core, but very often need a simple yet powerful web server for some project. Express.js is concise and simple to work with, so the API is inspired by that.

## License
RedHttpServer is released under MIT license, so it is free to use, even in commercial projects.
