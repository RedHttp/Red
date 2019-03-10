# RedHttpServer

## Features
- Extensible - support for middleware and plugins
- WebSocket support - no plugins required
- JSON & XML converters come as standard, but can be replaced
- Parse JSON, XML and even FormData request bodies without any plugins
- API inspired by the simplicity of [express.js](https://github.com/expressjs/express)


### _Hello world_ example
```csharp
var server = new RedHttpServer(5000, "public");
server.Get("/", async (req, res) => await res.SendString("Hello world!"));
await server.RunAsync();
```


### Authentication example - using [Red.CookieSessions](https://www.nuget.org/packages/Red.CookieSessions/) middleware
```csharp
var server = new RedHttpServer(5000, "public");
server.Use(new CookieSessions<MySession>(TimeSpan.FromDays(5)));

server.Get("/friends", Auth, async (req, res) => 
{
  var session = req.GetSession<MySession>();
  var friends = await db.FindFriends(session.Username);
  await res.SendJson(friends);
}
await server.RunAsync();
```


### Find it all at NuGet
- [RedHttpServer](https://www.nuget.org/packages/RHttpServer/)
  - Authentication middleware
    - [CookieSessions](https://www.nuget.org/packages/Red.CookieSessions/)
    - [JwtSessions](https://www.nuget.org/packages/Red.JwtSessions/)
  - Rendering plugins
    - [HandlebarsRenderer](https://www.nuget.org/packages/Red.HandlebarsRenderer/)
    - [CommonMarkRenderer](https://www.nuget.org/packages/Red.CommonMarkRenderer/)
    - [EcsRenderer](https://www.nuget.org/packages/Red.EcsRenderer/)

### Cool projects used 
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [Jwt.Net](https://github.com/jwt-dotnet/jwt)
- [CommonMark.NET](https://github.com/Knagis/CommonMark.NET)
- [Handlebars.Net](https://github.com/rexm/Handlebars.Net)



### MIT Licensed
##### _So use it for whatever you want to!_
