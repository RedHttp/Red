# RedHttpServer
### Cross-platform http server framework with websocket support

A Http web server framework built on ASP.NET Core and Kestrel, but with an API inspired by the simplicity of express.js

[Homepage](https://rosenbjerg.github.io/Red/)
[Documentation](https://rosenbjerg.github.io/Red/doxygen/)

### Installation
RedHttpServer can be installed from [NuGet](https://www.nuget.org/packages/RHttpServer/): Install-Package RHttpServer

## Middleware and plugins
RedHttpServer is created to be easy to build on top of. 
The server supports both middleware modules and extension modules, and offers a method to register these. 

I have created a couple already; three are inbuilt, but can easily be replaced:

* NewtonsoftJsonConverter - uses [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
* XmlConverter - uses System.Xml.Serialization
* BodyParser - uses both the Json- and Xml converter to parse request body to an object, depending on content-type.

And five more split out into separate projects:
- [CookieSessions](https://github.com/rosenbjerg/Red.CookieSessions) simple session management middleware that uses cookies with authentication tokens.
- [JwtSessions](https://github.com/rosenbjerg/Red.JwtSessions) simple session management middleware that uses JWT tokens - uses [Jwt.Net](https://github.com/jwt-dotnet/jwt)
- [EcsRenderer](https://github.com/rosenbjerg/Red.EcsRenderer) simple template rendering extension. See more info about the format by clicking the link.
- [CommonMarkRenderer](https://github.com/rosenbjerg/Red.CommonMarkRenderer) simple CommonMark/Markdown renderer extension - uses [CommonMark.NET](https://github.com/Knagis/CommonMark.NET)
- [HandlebarsRenderer](https://github.com/rosenbjerg/Red.HandlebarsRenderer) simple Handlebars renderer extension - uses [Handlebars.Net](https://github.com/rexm/Handlebars.Net)


### Example
```csharp
// We create a server instance using port 5000 which serves static files, such as index.html from the 'public' directory
var server = new RedHttpServer(5000, "public");

// We register the needed middleware:

// We use the Red.EscRenderer plugin in this example
server.Use(new EcsRenderer());

// We use Red.CookieSessions as authentication in this example
server.Use(new CookieSessions<MySess>(new CookieSessionSettings(TimeSpan.FromDays(1))
{
	Secure = false // To be able to use cookies without https in development
}));
// Middleware function that closes requests that does not have a valid session associated
async Task Auth(Request req, Response res)
{
	if (req.GetSession<MySess>() == null)
	{
		await res.SendStatus(HttpStatusCode.Unauthorized);
	}
}

var startTime = DateTime.UtcNow;

// We then register our endpoint handlers:

// We can use url params, much like in express.js
server.Get("/:param1/:paramtwo/:somethingthird", Auth, async (req, res) =>
{
	await res.SendString($"URL: {req.Parameters["param1"]} / {req.Parameters["paramtwo"]} / {req.Parameters["somethingthird"]}");
});

// The clients can post to this endpoint to authenticate
server.Post("/login", async (req, res) =>
{
	// You should authenticate the client properly, but here we just log the client in - no matter what..
	req.OpenSession(new MySess {Username = "benny"});
	await res.SendStatus(HttpStatusCode.OK);
});

// The client can post to this endpoint to close their current session
// Note that we require the client the be authenticated using the Auth-function we created above
server.Get("/logout", Auth, async (req, res) =>
{
	req.GetSession<MySess>().Close(req);
	await res.SendStatus(HttpStatusCode.OK);
});

// We redirect authenticated clients to some other page
server.Get("/redirect", Auth, async (req, res) => { await res.Redirect("/redirect/user/here"); });

// We save the files contained in the POST request from authenticated clients in a directory called 'uploads'
Directory.CreateDirectory("uploads");
server.Post("/upload", Auth, async (req, res) =>
{
	if (await req.SaveFiles("uploads"))
		await res.SendString("OK");
	else
		await res.SendString("Error", status: HttpStatusCode.NotAcceptable);
});

// We can also serve files outside of the public directory, if we wish to
// This can be handy when serving "dynamic files" - files which the client identify using an ID instead of the actual path on the server
server.Get("/file", async (req, res) => { await res.SendFile("/notpublic/testimg.jpeg"); });

// We can easily handle POST requests containing FormData
server.Post("/formdata", async (req, res) =>
{
	var form = await req.GetFormDataAsync();
	await res.SendString("Hello " + form["firstname"]);
});

// In a similar way, we can also handle url queries for a given request easily, in the example only for authenticated clients
server.Get("/hello", Auth, async (req, res) =>
{
	var session = req.GetSession<MySess>();
	var queries = req.Queries;
	await res.SendString($"Hello {queries["firstname"]} {queries["lastname"]}, you are logged in as {session.Username} - have a nice day");
});

// We can render MarkDown/CommonMark using the Red.CommonMarkRenderer plugin
server.Get("/markdown", async (req, res) => { await res.RenderFile("markdown.md"); });

// We use the Red.EcsRenderer plugin to render a simple template
server.Get("/serverstatus", async (req, res) =>
{
	await res.RenderPage("pages/statuspage.ecs", new RenderParams
	{
		{"uptime", (int) DateTime.UtcNow.Subtract(startTime).TotalSeconds},
		{"version", RedHttpServer.Version}
	});
});

// We can also handle WebSocket requests, without any plugin needed
// In this example we just have a simple WebSocket echo server
server.WebSocket("/echo", async (req, wsd) =>
{
	wsd.SendText("Welcome to the echo test server");
	wsd.OnTextReceived += (sender, eventArgs) => { wsd.SendText("you sent: " + eventArgs.Text); };
});

// Then we start the server as an awaitable task
// This is practical for C# 7.1 and up, since the Main-method of a program can be async and thus kept open by awaiting this call
await server.RunAsync();
```

## Why?
Because I like C# and .NET Core, but very often need a simple yet powerful web server for some project. Express.js is concise and simple to work with, so the API is inspired by that.

## License
RedHttpServer is released under MIT license, so it is free to use, even in commercial projects.
