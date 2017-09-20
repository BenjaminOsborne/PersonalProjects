open JSONTypeProvider

// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

[<EntryPoint>]
let main argv = 
    let a = new JSONExperiment();
    let b = a.TestThis();
    let c = a.Guff();
    0 // return an integer exit code
