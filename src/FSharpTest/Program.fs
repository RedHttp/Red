open Red

    
[<EntryPoint>]
let main argv =
    
    
    let app = new RedHttpServer(5000)
    
    app.Get("/", (fun (req:Request) (res:Response) -> res.SendString("Hello from F#")))
    
    app.Get("/:name", (fun (req:Request) (res:Response) ->
        let message = sprintf "Hello from F#, %s" (req.Context.ExtractUrlParameter "name")
        res.SendString(message)))
    
    app.RunAsync() |> Async.AwaitTask |> Async.RunSynchronously
    0