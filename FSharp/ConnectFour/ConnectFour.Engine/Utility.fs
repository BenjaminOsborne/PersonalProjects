namespace ConnectFour.Engine
open System.Diagnostics

type Ensure() = 
    
    static member IsTrue(bIsTrue, message) = Debug.Assert(bIsTrue, message)

    static member Action(fnIsTrue, message) = Debug.Assert(fnIsTrue(), message)
