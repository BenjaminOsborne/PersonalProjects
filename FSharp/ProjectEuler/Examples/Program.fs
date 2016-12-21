namespace Ben.Example
open System
open Ben.Example.PersonModule
open FunctionalFlow

module Main =

    type ePeople = 
    | None = 0
    | Me = 1
    | You = 2

    //[<EntryPoint>]
    let mainOld argv = 
        let isDisivisibleBy num elem = elem % num = 0
        let fIsDivisibleBy31 n = isDisivisibleBy 31 n

        let result = Seq.find fIsDivisibleBy31 [1..100]
        printfn "%d " result

        let fConcatArray x = x |> Seq.fold (fun acc n -> acc + " " + n.ToString()) ""
        let sConcatArray = fConcatArray {1..10}
        Console.WriteLine sConcatArray

        let fSquare x = x |> Seq.map (fun n -> n * n)
        Console.WriteLine (fConcatArray (fSquare {1..20}))

        let fAppFirst x = List.append x [5..8]
        printfn "%A" (fAppFirst [1..3])

        let eMe = ePeople.Me

        let fun0 x = x + 3
        let fun1 x = x * 4

        let g_f = fun0 >> fun1
        let g_b = fun1 >> fun0

        let result2 = g_f 4
        printfn "%d" result2

        let result2 = g_b 4
        printfn "%d" result2

        let swap a b = b,a
        printfn "%s" ((swap 3 4).ToString())

        0

    [<EntryPoint>]
    let main argv = 
        
        let chainDiv = FunctionalFlow.chainedDivisionSuccess
        
        let (+) a b = a + b + 1
        let (+-+) a b = a + b - 10
        let a = 5 +-+ 6;

        do Console.ReadLine();

        0

//[<EntryPoint>]
//let main argv = 
//    printfn "%A" argv
//    0 // return an integer exit code

//let anInt = 5
//let aString = "Hello" 
//// Perform a simple calculation and bind anIntSquared to the result. 
//let anIntSquared = anInt * anInt
//
//System.Console.WriteLine(anInt)
//System.Console.WriteLine(aString)
//System.Console.WriteLine(anIntSquared)
//
//System.Console.WriteLine("\nBreak\n")
//let rec factorial n = 
//    if n = 0 
//    then 1 
//    else
//        let nNext = n-1 
//        n * factorial nNext
//System.Console.WriteLine(factorial anInt)

//let turnChoices = ("right", "left")
//System.Console.WriteLine(turnChoices)
//// Output: (right, left) 
//
//let square n = n * n
//// Call the function to calculate the square of anInt, which has the value 5. 
//let result = square anInt
//// Display the result.
//System.Console.WriteLine(result)
//
//let intAndSquare = (anInt, square anInt)
//System.Console.WriteLine(intAndSquare)
//// Output: (5,25)

//// List of best friends. 
//let bffs = [ "Susan"; "Kerry"; "Linda"; "Maria" ] 
//// Bind newBffs to a new list that has "Katie" as its first element.
//let newBffs = "Katie" :: bffs
//
//printfn "%A" bffs
//// Output: ["Susan"; "Kerry"; "Linda"; "Maria"]
//printfn "%A" newBffs
//// Output: ["Katie"; "Susan"; "Kerry"; "Linda"; "Maria"]
//
//System.Console.WriteLine("END")