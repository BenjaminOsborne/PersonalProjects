module ReactiveExtensions

open NUnit.Framework
open FsUnit
open System
open System.Threading

/// create a timer and register an event handler, then run the timer for five seconds
let createTimer timerInterval eventHandler =
    let timer = new System.Timers.Timer(float timerInterval) // setup a timer
    timer.AutoReset <- true
    
    timer.Elapsed.Add eventHandler // add an event handler

    async { // return an async task
        timer.Start() // start timer...
        do! Async.Sleep 1000 // ...run for five seconds...
        timer.Stop() } //...and stop

let createTimerAndObservableFor timerInterval totalLength =
    let timer = new System.Timers.Timer(float timerInterval)
    timer.AutoReset <- true
        
    let observable = timer.Elapsed  // events are automatically IObservable

    let task = async { // return an async task
        timer.Start()
        do! Async.Sleep totalLength
        timer.Stop() }
    (task,observable) // return a async task and the observable

let createTimerAndObservable timerInterval = createTimerAndObservableFor timerInterval 1000

[<Test>]
let ``Timer Events``() = 
    
    let start = DateTime.Now;
    let basicHandler _ = printfn "tick %A" (DateTime.Now - start).TotalMilliseconds // create a handler. The event args are ignored
    let basicTimer1 = createTimer 200 basicHandler // register the handler
    Async.RunSynchronously basicTimer1  // run the task now

[<Test>]
let ``Timer Events Observable``() = 

    //Example 1
    let basicTimer, timerEventStream = createTimerAndObservable 200 // create the timer and the corresponding observable

    let start = DateTime.Now
    timerEventStream |> Observable.subscribe (fun _ -> printfn "tick %A" (DateTime.Now - start).TotalMilliseconds) |> ignore // register that everytime something happens on the  event stream, print the time.

    Async.RunSynchronously basicTimer // run the task now

    //Example 3
    let timerCount2, timerEventStream2 = createTimerAndObservable 200 // create the timer and the corresponding observable

    // set up the transformations on the event stream
    timerEventStream2 |> Observable.scan (fun count eventArgs -> count + 1) 0
                      |> Observable.subscribe (fun count -> printfn "timer ticked with count %i" count)
                      |> ignore

    // run the task now
    Async.RunSynchronously timerCount2


type FizzBuzzEvent = { label:int; time: double }

[<Test>]
let ``Fizz Buzz``() =
    
    let fizzId = 3;
    let buzzId = 5;

    let timer3, timerEventStream3 = createTimerAndObservableFor 300 5000 // create the event streams and raw observables
    let timer5, timerEventStream5 = createTimerAndObservableFor 500 5000

    // convert the time events into FizzBuzz events with the appropriate id
    let start = DateTime.Now
    let eventStream3  = timerEventStream3 |> Observable.map (fun _ -> { label = fizzId; time = (DateTime.Now - start).TotalMilliseconds })
    let eventStream5  = timerEventStream5 |> Observable.map (fun _ -> { label = buzzId; time = (DateTime.Now - start).TotalMilliseconds })

    let combinedStream = Observable.merge eventStream3 eventStream5 // combine the two streams
 
    let pairwiseStream = combinedStream |> Observable.pairwise // make pairs of events (n and n-1 from 2nd event onwards)
 
    let simultaneousStream, nonSimultaneousStream = pairwiseStream |> Observable.partition (fun (pre,nxt) -> (nxt.time - pre.time) < 50.0) // split the stream based on whether the pairs are simultaneous

    // split the non-simultaneous stream based on the id
    let fizzStream, buzzStream  = nonSimultaneousStream |> Observable.map (fun (pre,nxt) -> pre) // convert pair of events to the first event
                                                        |> Observable.partition (fun x -> x.label=3) // split on whether the event id is three

    //print events from the combinedStream
    combinedStream |> Observable.subscribe (fun {label=id;time=t} -> printf "\n[%i] %03i " id ((int)t))|> ignore
 
    //print events from the simultaneous, fizz and buzz streams
    simultaneousStream |> Observable.subscribe (fun _ -> printf "FizzBuzz") |> ignore
    fizzStream |> Observable.subscribe (fun _ -> printf "Fizz") |> ignore
    buzzStream |> Observable.subscribe (fun _ -> printf "Buzz") |> ignore

    let u = [timer3;timer5] |> Async.Parallel |> Async.RunSynchronously // run the two timers at the same time
    u |> ignore
