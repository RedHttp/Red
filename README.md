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

And three more split out into separate projects:
- [CookieSessions](https://github.com/rosenbjerg/RedHttpServer.CSharp/tree/master/src/CookieSessions) simple session management middleware that uses cookies with authentication tokens.
- [EcsRenderer](https://github.com/rosenbjerg/RedHttpServer.CSharp/tree/master/src/EcsRenderer) simple template rendering extension. See more info about the format by clicking the link.
- [CommonMarkRenderer](https://github.com/rosenbjerg/RedHttpServer.CSharp/tree/master/src/CommonMarkRenderer) simple CommonMark/Markdown renderer extension - uses [CommonMark.NET](https://github.com/Knagis/CommonMark.NET)

### Example
```csharp
// We serve static files, such as index.html from the 'public' directory
var server = new RedHttpServer(5000, "public");

// URL param demo
server.Get("/:param1/:paramtwo/:somethingthird", async (req, res) =>
{
	await res.SendString($"URL: {req.Parameters["param1"]} / {req.Parameters["paramtwo"]} / {req.Parameters["somethingthird"]}");
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
```

## Why?
Because I like C# and .NET Core, but very often need a simple yet powerful web server for some project. Express.js is concise and simple to work with, so the API is inspired by that.

## License
RedHttpServer is released under MIT license, so it is free to use, even in commercial projects.