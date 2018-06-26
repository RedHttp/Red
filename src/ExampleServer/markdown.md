# RedHttpServer
### Cross-platform http server with websocket support


A C# alternative to nodejs and similar server bundles

Some of the use patterns has been inspired by nodejs and expressjs

### Documentation
[Documentation for .NET Core version](https://rosenbjerg.dk/rhscore/docs/)

[Documentation for .NET Framework version](https://rosenbjerg.dk/rhs/docs/)

### Installation
RedHttpServer can be installed from [NuGet](https://www.nuget.org/packages/RHttpServer/): Install-Package RHttpServer

### Examples
```csharp
// Serving static files, such as index.html, from the directory './public' and listens for requests on port 5000
var server = new RedHttpServer(5000, "./public");
var startTime = DateTime.UtcNow;


// Using url queries to generate an answer
// Fx. the request "/hello?firstname=John&lastname=Doe"
// will get the text reponse "Hello John Doe, have a nice day"
server.Get("/hello", async (req, res) =>
{
    var queries = req.Queries;
    var firstname = queries["firstname"][0];
    var lastname = queries["lastname"][0];
    await res.SendString($"Hello {firstname} {lastname}, have a nice day");
});


// URL parameter demonstration
server.Get("/:param1/:paramtwo/:somethingthird", async (req, res) =>
{
    await res.SendString($"URL: {req.Params["param1"]} / {req.Params["paramtwo"]} / {req.Params["somethingthird"]}");
});


// Redirect request to another page
// In this example the client is redirected to
// another site on the same domain
server.Get("/redirect", async (req, res) =>
{
    await res.Redirect("/redirect/test/here");
});


// Handling data sent as forms (FormData)
// This example shows how a simple user registration
// with profile picture can be handled
server.Post("/register", async (req, res) =>
{
    var form = await req.GetFormDataAsync();
    
    var username = form["username"][0];
    var password = form["password"][0];
    var profilePicture = form.Files[0];
    
    CreateUser(username, password);
    SaveUserImage(username, profilePicture);
    
    await res.SendString("User registered!");
});


// Save uploaded file from request body
// In this example the file is saved in the './uploads' 
// directory using the filename it was uploaded with
server.Post("/upload", async (req, res) =>
{
    if (await req.SaveBodyToFile("./uploads"))
    {
        await res.SendString("OK");
    }
    else
    {
        await res.SendString("Error", status: 413);
    }
});


// Save file uploaded in FormData object
server.Post("/formupload", async (req, res) =>
{
    var form = await req.GetFormDataAsync();
    var file = form.Files[0];
    
    using (var outputfile = File.Create("./uploads/" + file.FileName))
    {
        await file.CopyToAsync(outputfile);
    }
    await res.SendString("OK");
});


// Rendering a page with dynamic content
// In this example we create RenderParams for a very simple
// server status page, which only shows uptime in hours and
// and which version of the server framework is used
server.Get("/serverstatus", async (req, res) =>
{
    await res.RenderPage("./pages/statuspage.ecs", new RenderParams
    {
        { "uptime", DateTime.UtcNow.Subtract(startTime).TotalHours },
        { "version", RedHttpServer.Version }
    });
});


// WebSocket echo server
// Clients can connects to "/echo" using the WebSocket protocol
// This example simply echoes any text message received from a 
// client back with "You sent: " prepended to the message
server.WebSocket("/echo", (req, wsd) =>
{
    // We can also use the logger from the plugin collection 
    wsd.ServerPlugins.Use<ILogging>().Log("WS", "Echo server visited");

    wsd.SendText("Welcome to the echo test server");
    wsd.OnTextReceived += (sender, eventArgs) =>
    {
        wsd.SendText("You sent: " + eventArgs.Text);
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
16B6bzSgvBBprQteahougoDpbRHf8PnHvD (BTC)
0x63761494aAf03141bDea42Fb1e519De0c01CcF10 (ETH)
```
