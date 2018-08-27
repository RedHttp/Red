# RedHttpServer
### Cross-platform http server framework with websocket support

A Http web server framework built on ASP.NET Core and Kestrel, but with an API inspired by the simplicity of express.js

[Documentation](https://rosenbjerg.dk/red/docs)

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
- [JwtSessions](https://github.com/rosenbjerg/Red.JwtSessions) simple session management middleware that uses cookies with authentication tokens.
- [EcsRenderer](https://github.com/rosenbjerg/Red.EcsRenderer) simple template rendering extension. See more info about the format by clicking the link.
- [CommonMarkRenderer](https://github.com/rosenbjerg/Red.CommonMarkRenderer) simple CommonMark/Markdown renderer extension - uses [CommonMark.NET](https://github.com/Knagis/CommonMark.NET)
- [HandlebarsRenderer](https://github.com/rosenbjerg/Red.HandlebarsRenderer) simple Handlebars renderer extension - uses [Handlebars.Net](https://github.com/rexm/Handlebars.Net)


### Example
```csharp
    // We serve static files, such as index.html from the 'public' directory
    var server = new RedHttpServer(5000, "public");
    server.Use(new EcsRenderer());

    var sessions = new CookieSessions<MySess>(new CookieSessionSettings(TimeSpan.FromDays(1))
    {
	Secure = false // To be able to use cookies without https in development
    });

    server.Use(sessions);

    // Middleware that closes requests without a valid session
    async Task Auth(Request req, Response res)
    {
	if (req.GetSession<MySess>() == null)
	{
	    await res.SendStatus(HttpStatusCode.Unauthorized);
	}
    }

    var startTime = DateTime.UtcNow;

    // URL param demo
    server.Get("/:param1/:paramtwo/:somethingthird", Auth,
	async (req, res) =>
	{
	    await res.SendString(
		$"URL: {req.Parameters["param1"]} / {req.Parameters["paramtwo"]} / {req.Parameters["somethingthird"]}");
	});

    server.Post("/login", async (req, res) =>
    {
	// Here we could authenticate the user properly, with credentials sent in a form, or similar
	req.OpenSession(new MySess {Username = "benny"});
	await res.SendStatus(HttpStatusCode.OK);
    });
    server.Get("/login", async (req, res) =>
    {
	// To make it easy to test the session system only using the browser and no credentials
	req.OpenSession(new MySess {Username = "benny"});
	await res.SendStatus(HttpStatusCode.OK);
    });

    server.Get("/jwtlogin", async (req, res) =>
    {
	// To make it easy to test the session system only using the browser and no credentials
	await res.SendJwtToken(new MySess {Username = "benny"});
    });
    server.Get("/logout", Auth, async (req, res) =>
    {
	req.GetSession<MySess>().Close(req);
	await res.SendStatus(HttpStatusCode.OK);
    });

    // Redirect to page on same host, but only if authenticated
    server.Get("/redirect", Auth, async (req, res) => { await res.Redirect("/redirect/test/here"); });

    // Save uploaded file from request body, but only if authenticated
    Directory.CreateDirectory("uploads");
    server.Post("/upload", Auth, async (req, res) =>
    {
	if (await req.SaveFiles("uploads"))
	    await res.SendString("OK");
	else
	    await res.SendString("Error", status: HttpStatusCode.NotAcceptable);
    });

    server.Get("/file", async (req, res) => { await res.SendFile("testimg.jpeg"); });

    // Handling formdata from client
    server.Post("/formdata", async (req, res) =>
    {
	var form = await req.GetFormDataAsync();
	await res.SendString("Hello " + form["firstname"]);
    });

    // Using url queries to generate an answer, but only if authenticated
    server.Get("/hello", Auth, async (req, res) =>
    {
	var session = req.GetData<MySess>();
	var queries = req.Queries;
	await res.SendString(
	    $"Hello {queries["firstname"]} {queries["lastname"]}, you are logged in as {session.Username} - have a nice day");
    });

    // Render a markdown file to test 
    server.Get("/markdown", async (req, res) => { await res.RenderFile("markdown.md"); });

    // Rendering a page for dynamic content
    server.Get("/serverstatus", async (req, res) =>
    {
	await res.RenderPage("pages/statuspage.ecs", new RenderParams
	{
	    {"uptime", (int) DateTime.UtcNow.Subtract(startTime).TotalSeconds},
	    {"version", RedHttpServer.Version}
	});
    });

    // WebSocket echo server
    server.WebSocket("/echo", async (req, wsd) =>
    {
	await wsd.SendText("Welcome to the echo test server");
	wsd.OnTextReceived += (sender, eventArgs) => { wsd.SendText("you sent: " + eventArgs.Text); };
    });
    server.Start();
```

## Why?
Because I like C# and .NET Core, but very often need a simple yet powerful web server for some project. Express.js is concise and simple to work with, so the API is inspired by that.

## License
RedHttpServer is released under MIT license, so it is free to use, even in commercial projects.
