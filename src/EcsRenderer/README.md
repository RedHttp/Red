# Ecs Renderer for RedHttpServer
The ecs renderer is a simple templating engine extension for Red 

### Usage
After installing and referencing this library, the `Red.Response` has the extension method `RenderPage(pageFilePath, parameters, ..)`.

The `pageFilePath` must be the path of a `.ecs` file.

### The .ecs file format
The .ecs file format is an extension used for html pages with ecs-tags.

### Tags
- `<% foo %>` will get replaced with the text data in the RenderParams object passed to the renderer

- `<%= foo =%>` will get replaced with a HTML encoded version of the text data in the RenderParams object passed to the renderer

- `<%- files/footer.html -%>` will get replaced with the content of the file with the specified path. Can be both absolute or relative. Only html, ecs, js, css and txt is supported for now, but if you have a good reason to include another filetype, please create an issue regarding that.


### Example
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Status page</title>
</head>
<body>
    <b>Uptime in minutes: <% uptime %></b></br>
    <b>Version: <% version %></b>
</body>
</html>
```
```csharp
server.Get("/statuspage", async (req, res) =>
{
    await res.RenderPage("pages/statuspage.ecs", new RenderParams
    {
        { "uptime", DateTime.UtcNow.Subtract(startTime).TotalMinutes },
        { "version", RedHttpServer.Version }
    });
});
```
Could result in:
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Status page</title>
</head>
<body>
    <b>Uptime in minutes: 42</b></br>
    <b>Version: 3.0.0</b>
</body>
</html>
```

The file extension is enforced to avoid confusion with regular html files without tags.

The format is somewhat inspired by the ejs format, though you cannot embed JavaScript, or C# for that matter, in the pages.

Embed your dynamic content using RenderParams instead of embedding the code for generation of the content in the html.