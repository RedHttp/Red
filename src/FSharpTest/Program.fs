open Red

    
    
[<EntryPoint>]
let main argv =
    
    let app = new RedHttpServer(5000)
    
    app.Get("/", (fun (req:Request) (res:Response) -> res.SendString("Hello from F#")))
    
    app.Get("/:name", (fun (req:Request) (res:Response) ->
        let message = sprintf "Hello %s from F#" req.Parameters.["name"]
        res.SendString(message)))
    
    app.RunAsync().Wait()
    0