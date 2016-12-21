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

// Lists
    type ListWorkflowBuilder() =
        member this.Bind(list, f) =  list |> List.collect f 
        member this.Return(x) = [x]
        member this.Yield(x) = [x]
        member this.For(m,f) = this.Bind(m,f)

    let listWorkflow = new ListWorkflowBuilder()

    let added = 
        listWorkflow {
            let! i = [1;2;3]
            let! j = [10;11;12]
            return i+j
            }
    //For (similar to bind in simple workflows)
    let added2 = 
        listWorkflow { 
            for x in [1..3] do
            for y in [10;20;30] do
            return x + y
            }

//TraceBuilder
    type TraceBuilder() =
        member this.Bind(m, f) = match m with 
                                 | None ->   printfn "Binding with None. Exiting."
                                 | Some a -> printfn "Binding with Some(%A). Continuing" a
                                 Option.bind f m

        member this.Return(x) = printfn "Returning a unwrapped %A as an option" x
                                Some x

        member this.ReturnFrom(m) = printfn "Returning an option (%A) directly" m
                                    m

        member this.Yield(x) = printfn "Yield an unwrapped %A as an option" x
                               Some x
        
        member this.Zero() = printfn "Zero"
                             None

        member this.Combine (a,b) = match a,b with
                                       | Some a', Some b' -> printfn "combining %A and %A" a' b' 
                                                             Some (a' + b')
                                       | Some a', None ->    printfn "combining %A with None" a' 
                                                             Some a'
                                       | None, Some b' ->    printfn "combining None with %A" b' 
                                                             Some b'
                                       | None, None ->       printfn "combining None with None"
                                                             None
        
        member this.Delay(f) = printfn "Delay"
                               f()
        

    let trace = new TraceBuilder()

    //Straight to return
    trace { return 1 } |> printfn "Result 1: %A" 

    //Let
    trace { return! Some 2 } |> printfn "Result 2: %A" 

    //Let2
    trace { let! x = Some 1
            let! y = Some 2
            return x + y }
          |> printfn "Result 3: %A" 

    //Short out on let
    trace { let! x = None
            let! y = Some 1
            return x + y }
          |> printfn "Result 4: %A"
    
    //Zero
    trace { printfn "hello world" } |> printfn "Result for simple expression: %A"

    //Yield
    trace { yield 1 } |> printfn "Result for yield: %A" 

    //Yield many and Delay
    trace {
            yield 1
            yield 2
          } |> printfn "Result for yield: %A"

//Trace Builder lazy
    type TraceBuilderLazy() =
        member this.Bind(m, f) = match m with 
                                 | None ->   printfn "Binding with None. Exiting."
                                 | Some a -> printfn "Binding with Some(%A). Continuing" a
                                 Option.bind f m

        member this.Return(x) = printfn "Return: An unwrapped %A as an option" x
                                Some x

        member this.Zero() = printfn "Zero"
                             None
        
        member this.Combine (a,b) = printfn "Combine: Returning early with %A. Ignoring second part: %A" a b 
                                    a

        member this.Delay (f) = printfn "Delay"
                                f
        
        member this.Run (f) = printfn "%A - Run: Start..." f
                              let runResult = f()
                              printfn "%A - Run End. Result is %A" f runResult
                              runResult // return the result of running the delayed function
    
    let traceLazy = new TraceBuilderLazy();

    printfn "\nBegin Trace Lazy..."
    let a = traceLazy { 
                        printfn "Part 1: about to return 1"
                        return 1
                        printfn "Part 2: after return has happened"
                      }
    let b = a |> printfn "Result for TraceLazy return 1: %A"  
    
    let c = traceLazy {
                        return 1
                        return 2
                        return 3
                      }
    let d = c |> printfn "Result for TraceLazy 3 returns: %A"


//Trace Build Typed
    type Internal = Internal of int option
    type Delayed = Delayed of (unit -> Internal)

    type TraceBuilderTyped() =
        member this.Bind(m, f) = 
            match m with 
            | None -> 
                printfn "Binding with None. Exiting."
            | Some a -> 
                printfn "Binding with Some(%A). Continuing" a
            Option.bind f m

        member this.Return(x) = 
            printfn "Returning a unwrapped %A as an option" x
            Internal (Some x) 

        member this.ReturnFrom(m) = 
            printfn "Returning an option (%A) directly" m
            Internal m

        member this.Zero() = 
            printfn "Zero"
            Internal None

        member this.Combine (Internal x, Delayed g) : Internal = 
            printfn "Combine. Starting %A" g
            let (Internal y) = g()
            printfn "Combine. Finished %A. Result is %A" g y
            let o = match x,y with
                    | Some a, Some b -> printfn "Combining %A and %A" a b 
                                        Some (a + b)
                    | Some a, None -> printfn "combining %A with None" a 
                                      Some a
                    | None, Some b -> printfn "combining None with %A" b 
                                      Some b
                    | None, None -> printfn "combining None with None"
                                    None
            // return the new value wrapped in a Internal
            Internal o                

        member this.Delay(funcToDelay) = 
            let delayed = fun () ->
                printfn "%A - Starting Delayed Fn." funcToDelay
                let delayedResult = funcToDelay()
                printfn "%A - Finished Delayed Fn. Result is %A" funcToDelay delayedResult
                delayedResult  // return the result 

            printfn "%A - Delaying using %A" funcToDelay delayed
            Delayed delayed // return the new function wrapped in a Delay

        member this.Run(Delayed funcToRun) = 
            printfn "%A - Run Start." funcToRun
            let (Internal runResult) = funcToRun()
            printfn "%A - Run End. Result is %A" funcToRun runResult
            runResult // return the result of running the delayed function

    // make an instance of the workflow                
    let traceTyped = new TraceBuilderTyped()

//MaybeBuilderAgain
    type MaybeBuilderAgain() =

        member this.Bind(m, f) = Option.bind f m
        member this.Return(x) = Some x
        member this.ReturnFrom(x) = x
        member this.Zero() = None
        member this.Combine (a,b) =  match a with
                                     | Some _ -> a  // if a is good, skip b
                                     | None -> b()  // if a is bad, run b
        member this.Delay(f) = f
        member this.Run(f) = f()

    // make an instance of the workflow                
    let maybeAgain = new MaybeBuilderAgain()

    printfn "\nBegin Maybe Again..."

    let maybeAgainResult = 
        maybeAgain { 
            printfn "Part 1: about to return 1"
            return 1
            printfn "Part 2: after return has happened"
            } 

    maybeAgainResult |> printfn "Result for Part1 but not Part2: %A"

    printfn "\nBegin Maybe Again2..."

    let childWorkflow =  maybeAgain { 
                            printfn "Child workflow"
                            return! Option<int>.None
                            } 
    let maybeAgainResult2 =
        maybeAgain { 
            printfn "Part 1: about to return 1"
            return 1
            return! childWorkflow 
            }

    maybeAgainResult2 |> printfn "Result for Part1 but not childWorkflow: %A"

//MaybeBuilderDeferred
    
    type Maybe<'a> = Maybe of (unit -> 'a option)
        
    type MaybeBuilderDeferred() =
        member this.Bind(m, f) = Option.bind f m
        member this.Return(x) = Some x
        //member this.ReturnFrom(x) = x
        member this.ReturnFrom(Maybe f) = f()
        member this.Zero() = None
        member this.Combine (a,b) =  match a with
                                     | Some _ -> a  // if a is good, skip b
                                     | None -> b()  // if a is bad, run b
        member this.Delay(f) = f
        member this.Run(f) = Maybe f

    let maybeDeferred = new MaybeBuilderDeferred()
    printfn "\nBegin Maybe Deferred..."

    let childWorkflow2 = maybeDeferred {printfn "Child workflow"} 

    let m3 = maybeDeferred { 
        printfn "Part 1: about to return 1"
        return 1
        return! childWorkflow2
        } 

    let run (Maybe f) = f()
    let evaluated = run m3;
    evaluated |> printfn "Result for Part1 but not Part2: %A" 

//MaybeBuilderLazy
    
    type MaybeLazy<'a> = Maybe of Lazy<'a option>
        
    type MaybeBuilderLazy() =
        member this.Bind(m, f) = Option.bind f m
        member this.Return(x) = Some x
        member this.ReturnFrom(Maybe f) = f.Force()
        member this.Zero() = None
        member this.Combine (a,b) =  match a with
                                     | Some _ -> a  // if a is good, skip b
                                     | None -> b()  // if a is bad, run b
        member this.Delay(f) = f
        member this.Run(f) = Maybe (lazy f())

    let maybeLazy = new MaybeBuilderLazy()
    printfn "\nBegin Lazy..."

    let m4: Maybe<unit> = maybeLazy { 
                                        let childWorkflow3 = maybeLazy { printfn "Child workflow Lazy" }
                                        return! maybeLazy { printfn "Part 1: about to return None" }
                                        return! childWorkflow3
                                        return! childWorkflow3
                                    } 

    let runLazy (Maybe f) = f.Force()
    let evaluatedLazy = runLazy m4;
    evaluatedLazy |> printfn "Result for Part1 but not Part2: %A" 

