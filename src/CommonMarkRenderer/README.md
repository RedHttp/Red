# CommonMark renderer extension for RedHttpServer
CommonMark renderer is an extension for RedHttpServer, to render CommonMark (and Markdown) using [CommonMark.NET](https://github.com/Knagis/CommonMark.NET)

### Usage
After installing and referencing this library, the `Red.Response` has the extension methods 
`RenderFile(filePath, ..)` and `RenderString(commonMarkText, fileName, ..)`

`RenderFile(filePath, ..)` takes the path of a CommonMark or Markdown file, renders it to html and sends it as response.

`RenderString(commonMarkText, fileName, ..)` renders a string containing CommonMark or Markdown text, renders it and sends it as reponse.
The `fileName` parameter is optional and is used to specify a filename attached through a `Content-Disposition`-header.
