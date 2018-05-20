# Cookie Sessions for RedHttpServer
Simple session management middleware for Red. 

### Usage
After installing and referencing this library, the `Red.Request` has the extension methods `OpenSession(sessionData)` and `GetSession()`.

`OpenSession(sessionData)` will open a new session and add a header to the response associated with the request.

`GetSession<TSession>()` will return the `CookieSession` object wrapping the `TSession`-data, which has two methods: `Renew()` and `Close()`, and the field `Data`, which holds the session-data object


### Example
```csharp
class MySession 
{
    public string Username;
}
...

server.Use(new CookieSessions<MySession>(new CookieSessionSettings(TimeSpan.FromDays(1))
{   // We allow unauthenticated users to send requests to /login, so we can authenticate them
    ShouldAuthenticate = path => path != "/login" // We allow people to send requests without a valid Authorization to /login, where we can authenticate them
}));
server.Post("/login", async (req, res) =>
{
    var form = await res.GetFormDataAsync();
    if (ValidForm(form) && Authenticate(form["username"], form["password"]))
    {
        req.OpenSession(new MySession {Username = form["username"]}); // Here we just have the username as session-data
        await res.SendStatus(HttpStatusCode.OK);
    }
    else 
        await res.SendStatus(HttpStatusCode.BadRequest);
});
// Only authenticated users are allowed to /friends
server.Get("/friends", async (req, res) => 
{
    var session = req.GetSession<MySession>();
    var friends = database.GetFriendsOfUser(session.Username);
    await res.SendJson(friends);
});
server.Post("/logout", async (req, res) => 
{
    req.GetSession<MySession>().Close();
    await res.SendStatus(HttpStatusCode.OK);
});
```

#### Implementation
`OpenSession` will open a new session and attach a `Set-Cookie` header to the associated response. 
This header's value contains the token used for authentication. 
The token is generated using the `RandomNumberGenerator` from `System.Security.Cryptography`, 
so it shouldn't be too easy to "guess" other tokens, even with knowledge of some tokens.

