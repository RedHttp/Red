
## The .ecs file format
The .ecs file format is merely an extension used for html pages with ecs-tags.

#### Tags
- <% foo %> will get replaced with the text data in the RenderParams object passed to the renderer

- <%= foo =%> will get replaced with a HTML encoded version of the text data in the RenderParams object passed to the renderer

- <¤ files/style.css ¤> will get replaced with the content of the file with the specified path. Must be absolute or relative to the server executable. Only html, ecs, js, css and txt is supported for now, but if you have a good reason to include another filetype, please create an issue regarding that.


The file extension is enforced by the default page renderer to avoid confusion with regular html files without tags.

The format is inspired by the ejs format, though you cannot embed JavaScript or C# for that matter, in the pages.


Embed your dynamic content using RenderParams instead of embedding the code for generation of the content in the html.