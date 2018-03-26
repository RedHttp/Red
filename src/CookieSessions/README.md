# Cookie Sessions for RedHttpServer
Simple session management middleware for Red. 

### Usage
After installing and referencing this library, the `Red.Request` has the extension methods `OpenSession(sessionData)` and `GetSession()`.

`OpenSession(sessionData)` will open a new session and add a header to the response associated with the request.

`GetSession()` will return then `Session` object which has two methods: `Renew()` and `Close()`, and the field `Data`, which holds the session-data object


### Example
```csharp
server.Use(new CookieSessions(new CookieSessionSettings(TimeSpan.FromDays(1))
{   // We allow unauthenticated users to send requests to /login, so we can authenticate them
    Excluded = { "/login" }
}));
server.Post("/login", async (req, res) =>
{
    var form = await res.GetFormDataAsync();
    if (ValidForm(form) && Authenticate(form["username"], form["password"]))
    {
        req.OpenSession(form["username"]); // Here we just have the username as session-data
        await res.SendStatus(HttpStatusCode.OK);
    }
    else 
        await res.SendStatus(HttpStatusCode.BadRequest);
});
// Only authenticated users are allowed to /friends
server.Get("/friends", async (req, res) => 
{
    var username = (string) req.GetSession().Data; // We know which user, by looking at session-data
    var friends = database.GetFriendsOfUser(username);
    await res.SendJson(friends);
});
server.Post("/logout", async (req, res) => 
{
    req.GetSession().Close();
    await res.SendStatus(HttpStatusCode.OK);
});
```

#### Implementation
`OpenSession` will open a new session and attach a `Set-Cookie` header to the associated response. 
This header's value contains the token used for authentication. 
The token is generated using the `RandomNumberGenerator` from `System.Security.Cryptography`, 
so it shouldn't be too easy to "guess" other tokens, even with knowledge of some tokens.

