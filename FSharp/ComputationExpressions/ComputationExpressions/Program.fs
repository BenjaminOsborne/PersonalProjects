// Learn more about F# at http://fsharp.org

open System

[<EntryPoint>]
let main argv =
    
    ExpressionBuilder.runExpression |> ignore
    ActivePattern.runPattern |> ignore
    0 // return an integer exit code
