# RedHttpServer
### Cross-platform http server with websocket support


A C# alternative to nodejs and similar server bundles

Some of the use patterns has been inspired by nodejs and expressjs

### Documentation
[Documentation for .NET Core version](https://rosenbjerg.dk/rhscore/docs/)

[Documentation for .NET Framework version](https://rosenbjerg.dk/rhs/docs/)

### Installation
RedHttpServer can be installed from [NuGet](https://www.nuget.org/packages/RHttpServer/): Install-Package RHttpServer

### Example
```csharp
// We serve static files, such as index.html from the 'public' directory
var server = new RedHttpServer(5000, "public");
var startTime = DateTime.UtcNow;

// We log to terminal here
var logger = new TerminalLogging();
server.Plugins.Register<ILogging, TerminalLogging>(logger);

// URL param demo
server.Get("/:param1/:paramtwo/:somethingthird", async (req, res) =>
{
    await res.SendString($"URL: {req.Params["param1"]} / {req.Params["paramtwo"]} / {req.Params["somethingthird"]}");
});

// Redirect to page on same host
server.Get("/redirect", async (req, res) =>
{
    await res.Redirect("/redirect/test/here");
});


server.Post("/register", async (req, res) =>
{
    var registerForm = await req.GetFormDataAsync();
    CreateUser(registerForm["username"][0], registerForm["password"][0]);
    SaveUserImage(registerForm["username"][0], registerForm.Files[0]);
    await res.SendString("User created!");
});

// Save uploaded file from request body
Directory.CreateDirectory("./uploads");
server.Post("/upload", async (req, res) =>
{
    if (await req.SaveBodyToFile("./uploads"))
    {
        await res.SendString("OK");
        // We can use logger reference directly
        logger.Log("UPL", "File uploaded");
    }
    else
        await res.SendString("Error", status: 413);
});

server.Get("/file", async (req, res) =>
{
    await res.SendFile("testimg.jpeg");
});


// Using url queries to generate an answer
server.Get("/hello", async (req, res) =>
{
    var queries = req.Queries;
    var firstname = queries["firstname"][0];
    var lastname = queries["lastname"][0];
    await res.SendString($"Hello {firstname} {lastname}, have a nice day");
});

// Rendering a page for dynamic content
server.Get("/serverstatus", async (req, res) =>
{
    await res.RenderPage("./pages/statuspage.ecs", new RenderParams
    {
        { "uptime", DateTime.UtcNow.Subtract(startTime).TotalHours },
        { "versiom", RedHttpServer.Version }
    });
});

// WebSocket echo server
server.WebSocket("/echo", (req, wsd) =>
{
    // We can also use the logger from the plugin collection 
    wsd.ServerPlugins.Use<ILogging>().Log("WS", "Echo server visited");

    wsd.SendText("Welcome to the echo test server");
    wsd.OnTextReceived += (sender, eventArgs) =>
    {
        wsd.SendText("you sent: " + eventArgs.Text);
    };
});

server.Start();
```
### Static files
When serving static files, it not required to add a route action for every static file.
If no route action is provided for the requested route, a lookup will be performed, determining whether the route matches a file in the public file directory specified when creating an instance of the RedHttpServer class.

## Plug-ins
RedHttpServer is created to be easy to build on top of. 
The server supports plug-ins, and offer a method to easily add new functionality.
The plugin system works by registering plug-ins before starting the server, so all plug-ins are ready when serving requests.
Some of the default functionality is implemented through plug-ins, and can easily customized or changed entirely.
The server comes with default handlers for json and xml (ServiceStack.Text), page renderering (ecs).
You can easily replace the default plugins with your own, just implement the interface of the default plugin you want to replace, and 
register it before initializing default plugins and/or starting the server.

## The .ecs file format
The .ecs file format is merely an extension used for html pages with ecs-tags.

#### Tags
- <% foo %> will get replaced with the text data in the RenderParams object passed to the renderer

- <%= foo =%> will get replaced with a HTML encoded version of the text data in the RenderParams object passed to the renderer

- <¤ files/style.css ¤> will get replaced with the content of the file with the specified path. Must be absolute or relative to the server executable. Only html, ecs, js, css and txt is supported for now, but if you have a good reason to include another filetype, please create an issue regarding that.


The file extension is enforced by the default page renderer to avoid confusion with regular html files without tags.

The format is inspired by the ejs format, though you cannot embed JavaScript or C# for that matter, in the pages.


Embed your dynamic content using RenderParams instead of embedding the code for generation of the content in the html.

## Why?
Because i like C#, the .NET framework and type-safety, but i also like the use-patterns of nodejs, with expressjs especially.

## License
RedHttpServer is released under MIT license, so it is free to use, even in commercial projects.

Buy me a beer? 
```
17c5b8n9LJxXg32EWSBQWABLgQEqAYfsMq
```
