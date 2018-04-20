# Handlebars template renderer extension for RedHttpServer
Handlebars renderer is an extension for RedHttpServer, to render Handlebars template files using [Handlebars.Net](https://github.com/rexm/Handlebars.Net)

### Usage
After installing and referencing this library, the `Red.Response` has the extension method 
`RenderTemplate(filePath, renderParams, ..)` 


`RenderTemplate(filePath, renderParams, ..)` takes the path of a Handlebars template file and sends the html, rendered using the render parameter object, as response.