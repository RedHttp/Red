# RedHttpServer
### Cross-platform http server with websocket support


A C# alternative to nodejs and similar server bundles

Some of the use patterns has been inspired by nodejs and expressjs

### Documentation
[Documentation for .NET Core version](https://rosenbjerg.dk/rhscore/docs/)
[Documentation for .NET Framework version](https://rosenbjerg.dk/rhs/docs/)

### Example
In this example, we listen locally on port 5000 and handles the responses as tasks.

This example only handles GET http requests and the public folder is placed in the same folder as the server executable and named public.

```csharp
var server = new RedHttpServer(5000, "./public");

server.Get("/", (req, res) =>
{
    res.SendString("Welcome");
});

// Sends the file secret.html as response
server.Get("/file", (req, res) =>
{
    res.SendFile("./notpublic/somepage.html");
});

server.Get("/download", (req, res) =>
{
    res.Download("./notpublic/video.mp4");
});

server.Get("/:name", (req, res) =>
{
    var name = req.Params["name"];
    
    var pars = new RenderParams
    {
        {"nametag", name},
        {"foo", "bar"},
        {"answer", 42}
    };

    res.RenderPage("./public/index.ecs", pars);
});

// Handle websocket requests
// This example is a simple echo server that replies with what has been send
server.WebSocket("/ws", (req, wsd) =>
{
    wsd.OnTextReceived += (sender, eventArgs) =>
    {
        wsd.SendText(eventArgs.Text);
    };
    
    wsd.OnClosed += (sender, eventArgs) =>
    {
        // Do stuff when websocket connection is closed
    };
});


// Saves the body of post requests to the Uploads folder
// and prepends the current date and time to the filename
server.Post("/upload", async (req, res) =>
{
    await req.SaveBodyToFile("./Uploads", fname => DateTime.Now + "-" + fname);
    res.SendString("saved");
});

// The asterisk (*) is a weak wildcard
// here it is used as a fallback when visitors requests an unknown route
server.Get("/*", (req, res) =>
{
    res.Redirect("/404");
});

server.Get("/404", (req, res) =>
{
    res.SendString("Nothing found", 404);
});

server.Start(true);
```
### Static files
When serving static files, it not required to add a route action for every static file.
If no route action is provided for the requested route, a lookup will be performed, determining whether the route matches a file in the public file directory specified when creating an instance of the HttpServer class.

## Plug-ins
RHttpServer is created to be easy to build on top of. 
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
