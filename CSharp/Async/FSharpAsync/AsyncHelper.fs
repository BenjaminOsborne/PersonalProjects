module FSharpAsync

type AsyncHelper() = 
    
    member this.Perform() = 
        0;

open System.Net
open Microsoft.FSharp.Control.WebExtensions

let DownloadHelper() =

    let urlList = [ "Microsoft.com", "http://www.microsoft.com/" 
                    "MSDN", "http://msdn.microsoft.com/" 
                    "Bing", "http://www.bing.com"
                  ]

    let fetchAsync(name, url:string) =
        async { 
            try 
                let uri = new System.Uri(url)
                let webClient = new WebClient()
                let! html = webClient.AsyncDownloadString(uri)
                printfn "Read %d characters for %s" html.Length name
            with
                | ex -> printfn "%s" (ex.Message);
        }

    let runAll() =
        urlList
        |> Seq.map fetchAsync
        |> Async.Parallel 
        |> Async.RunSynchronously
        |> ignore

    //runAll()

    let singleRun = fetchAsync("Single Test", "http://www.bbc.co.uk/news/")
    Async.RunSynchronously singleRun
    let a = System.Console.ReadLine();

    0;