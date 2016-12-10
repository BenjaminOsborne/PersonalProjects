module FunctionalFlow

    type MaybeBuilder() =  
        member this.Return(x) = Some  x
        member this.Bind(option, func) = 
            match option with
            | None -> None
            | Some a -> func a


    let maybe = new MaybeBuilder()

    let divide x y =  
        if y = 0 then None
        else Some(x/y)

    let chainedDivisionSuccess =  
        maybe {
            let inter = (divide 8 2)
            let! four = inter
            let! two = (divide four 2)
            return two
        }

//Alternative...
    let divideBy bottom top =
        if bottom = 0 then None
        else Some(top/bottom)

    let (>>=) m f = Option.bind f m

    let divideByWorkflow x y w z = 
        x |> divideBy y 
        >>= divideBy w 
        >>= divideBy z 

    // test
    let good = divideByWorkflow 12 3 2 1
    let bad = divideByWorkflow 12 3 0 1