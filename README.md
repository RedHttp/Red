# RedHttpServer
### Cross-platform http server framework with websocket support
[![GitHub](https://img.shields.io/github/license/redhttp/red)](https://github.com/RedHttp/Red/blob/master/LICENSE.md)
[![Nuget](https://img.shields.io/nuget/v/rhttpserver)](https://www.nuget.org/packages/RHttpServer/)
[![Nuget](https://img.shields.io/nuget/dt/rhttpserver)](https://www.nuget.org/packages/RHttpServer/)


A .NET Standard web application framework built on ASP.NET Core w/ Kestrel and inspired by the simplicity of Express.js

- [Homepage](https://redhttp.github.io/Red/)
- [Documentation](https://redhttp.github.io/Red/doxygen/)

### Installation
RedHttpServer can be installed from [NuGet](https://www.nuget.org/packages/RHttpServer/): `Install-Package RHttpServer`

## Middleware and plugins
RedHttpServer is created to be easy to build on top of. 
The server supports both middleware modules and extension modules, and offers a method to register these. 

I have created a couple already; three are inbuilt, but can easily be replaced:

* JsonConverter - uses System.Text.Json
* XmlConverter - uses System.Xml.Serialization
* BodyParser - uses both the Json- and Xml converter to parse request body to an object, depending on content-type.

And five more split out into separate projects:
- [CookieSessions](https://github.com/RedHttp/Red.CookieSessions) simple session management middleware that uses cookies with authentication tokens.
- [JwtSessions](https://github.com/RedHttp/Red.JwtSessions) simple session management middleware that uses JWT tokens - uses [Jwt.Net](https://github.com/jwt-dotnet/jwt)
- [EcsRenderer](https://github.com/RedHttp/Red.EcsRenderer) simple template rendering extension. See more info about the format by clicking the link.
- [CommonMarkRenderer](https://github.com/RedHttp/Red.CommonMarkRenderer) simple CommonMark/Markdown renderer extension - uses [CommonMark.NET](https://github.com/Knagis/CommonMark.NET)
- [HandlebarsRenderer](https://github.com/RedHttp/Red.HandlebarsRenderer) simple Handlebars renderer extension - uses [Handlebars.Net](https://github.com/rexm/Handlebars.Net)


### Example
```csharp
// We create a server instance using port 5000 which serves static files, such as index.html from the 'public' directory
var server = new RedHttpServer(5000, "public");

// We register the needed middleware:

// We use the Red.EscRenderer plugin in this example
server.Use(new EcsRenderer());

// We use Red.CookieSessions as authentication in this example
server.Use(new CookieSessions<MySess>(TimeSpan.FromDays(1))
{
    Secure = false // for development
});

// Middleware function that closes requests that does not have a valid session associated
async Task<HandlerType> Auth(Request req, Response res)
{
    if (req.GetSession<MySess>() != null)
    {
        return HandlerType.Continue;
    }
    await res.SendStatus(HttpStatusCode.Unauthorized);
    return HandlerType.Final;
}

var startTime = DateTime.UtcNow;

// We register our endpoint handlers:

// We can use url params, much like in express.js
server.Get("/:param1/:paramtwo/:somethingthird", Auth, (req, res) =>
{
    var context = req.Context;
    return res.SendString(
        $"you entered:\n" +
        context.ExtractUrlParameter("param1") + "\n" +
        context.ExtractUrlParameter("paramtwo") + "\n" +
        context.ExtractUrlParameter("somethingthird") + "\n");
});

// The clients can post to this endpoint to authenticate
server.Post("/login", async (req, res) =>
{
    // To make it easy to test the session system only using the browser and no credentials
    await req.OpenSession(new MySess {Username = "benny"});
    return await res.SendStatus(HttpStatusCode.OK);
});

// The client can post to this endpoint to close their current session
// Note that we require the client the be authenticated using the Auth-function we created above
server.Get("/logout", Auth, async (req, res) =>
{
    await req.GetSession<MySess>().Close(req);
    return await res.SendStatus(HttpStatusCode.OK);
});


// We redirect authenticated clients to some other page
server.Get("/redirect", Auth, (req, res) => res.Redirect("/redirect/test/here"));

// We save the files contained in the POST request from authenticated clients in a directory called 'uploads'
Directory.CreateDirectory("uploads");
server.Post("/upload", async (req, res) =>
{
    if (await req.SaveFiles("uploads"))
        return await res.SendString("OK");
    else
        return await res.SendString("Error", status: HttpStatusCode.NotAcceptable);
});

// We can also serve files outside of the public directory, if we wish to
// This can be handy when serving "dynamic files" - files which the client identify using an ID instead of the actual path on the server
server.Get("/file", (req, res) => res.SendFile("testimg.jpeg"));

// We can easily handle POST requests containing FormData
server.Post("/formdata", async (req, res) =>
{
    var form = await req.GetFormDataAsync();
    return await res.SendString("Hello " + form["firstname"]);
});

// In a similar way, we can also handle url queries for a given request easily, in the example only for authenticated clients
server.Get("/hello", Auth, async (req, res) =>
{
    var session = req.GetData<MySess>();
    var queries = req.Queries;
    return await res.SendString(
        $"Hello {queries["firstname"]} {queries["lastname"]}, you are logged in as {session.Username}");
});

// We can render MarkDown/CommonMark using the Red.CommonMarkRenderer plugin
server.Get("/markdown", (req, res) => res.RenderFile("markdown.md"));

// We use the Red.EcsRenderer plugin to render a simple template
server.Get("/serverstatus", async (req, res) => await res.RenderPage("pages/statuspage.ecs", new RenderParams
{
    {"uptime", (int) DateTime.UtcNow.Subtract(startTime).TotalSeconds},
    {"version", Red.RedHttpServer.Version}
}));

// We can also handle WebSocket requests, without any plugin needed
// In this example we just have a simple WebSocket echo server
server.WebSocket("/echo", async (req, wsd) =>
{
    wsd.SendText("Welcome to the echo test server");
    wsd.OnTextReceived += (sender, eventArgs) => { wsd.SendText("you sent: " + eventArgs.Text); };
    return HandlerType.Final;
});

// Then we start the server as an awaitable task
// This is practical for C# 7.1 and up, since the Main-method of a program can be async and thus kept open by awaiting this call
await server.RunAsync();
```

## Why?
Because I like C# and .NET Core, but very often need a simple yet powerful web server for some project. Express.js is concise and simple to work with, so the API is inspired by that.

## License
RedHttpServer is released under MIT license, so it is free to use, even in commercial projects.
