// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open HelperTypes;
[<EntryPoint>]
let main argv = 

    let fnPrintProblem nNumber nResult =
        printfn "Problem %d: %d" nNumber nResult;
    
    let fnPrintProblemLong nNumber nResult =
        let nCase = nResult + 0L;
        printfn "Problem %d: %d" nNumber nResult;        

//    let listPrimes = CSharpHelperFunctions.Primes.GetAllPrimesToNum(1000000) |> Seq.toList;
//    printfn "%d" listPrimes.Length

//    let a = new BigNum(1000)
//    let ig = a.Add(129874)
//    printfn "%d" a.UsedDigits;
//    printfn "%s" a.PrintNumber;
//    
//    let num1 = new BigNum(300)
//    num1.Add(99901);
//    let num2 = new BigNum(300)
//    num2.Add(98432)
//    let num3 = BigNum.Sum(num1,num2);
//    printfn "Expect %d" (99901 + 98432)
//    printfn "Actual %s" num3.PrintNumber
//    let aIn = System.Console.ReadLine();

//    fnPrintProblem 1 (Problems.SolveProblem1 -1) //233168
//    fnPrintProblem 2 (Problems.SolveProblem2 -1) //4613732
//    fnPrintProblem 3 (Problems.SolveProblem3 -1) //6857
//    fnPrintProblem 4 (Problems.SolveProblem4 -1) //906609
//    fnPrintProblem 5 (Problems.SolveProblem5 -1) //232792560
//    fnPrintProblem 6 (Problems.SolveProblem6 -1) //25164150
//    fnPrintProblem 7 (Problems.SolveProblem7 -1) //104743
//    fnPrintProblem 8 (Problems.SolveProblem8 -1) //40824
//    fnPrintProblem 9 (Problems.SolveProblem9 -1) //31875000
//    fnPrintProblemLong 10 (Problems.SolveProblem10 -1)//142913828922
//    fnPrintProblem 11 (Problems.SolveProblem11 -1) //70600674
//    fnPrintProblem 12 (Problems.SolveProblem12 -1) //76576500
//    fnPrintProblemLong 13 (Problems.SolveProblem13 -1) //5537376230
//    fnPrintProblem 14 (Problems.SolveProblem14 -1) //837799
//    fnPrintProblemLong 15 (Problems.SolveProblem15 -1) //137846528820
//    fnPrintProblem 16 (Problems.SolveProblem16 -1) //1366
//    fnPrintProblem 17 (Problems.SolveProblem17 -1) //21124
//    fnPrintProblem 19 (Problems.SolveProblem19 -1) //171
//    fnPrintProblem 20 (Problems.SolveProblem20 -1) //648
//    fnPrintProblem 21 (Problems.SolveProblem21 -1) //31626
//    fnPrintProblem 22 (Problems.SolveProblem22 -1) //871198282
//    fnPrintProblem 23 (Problems.SolveProblem23 -1) //4179871
//    fnPrintProblemLong 24 (Problems.SolveProblem24 -1) //2783915460
//    fnPrintProblem 25 (Problems.SolveProblem25 -1) //4782
//    fnPrintProblem 26 (Problems.SolveProblem26 -1) //983
//    fnPrintProblem 27 (Problems.SolveProblem27 -1) //-59231
//    fnPrintProblem 28 (Problems.SolveProblem28 -1) //669171001
//    fnPrintProblem 29 (Problems.SolveProblem29 -1) //9183
//    fnPrintProblem 30 (Problems.SolveProblem30 -1) //443839
//    fnPrintProblem 31 (Problems.SolveProblem31 -1) //73682
//    fnPrintProblem 32 (Problems.SolveProblem32 -1) //45228
//    fnPrintProblem 33 (Problems.SolveProblem33 -1) //100
//    fnPrintProblem 34 (Problems.SolveProblem34 -1) //40730
//    fnPrintProblem 35 (Problems.SolveProblem35 -1) //55
//    fnPrintProblem 36 (Problems.SolveProblem36 -1) //872187
//    fnPrintProblem 37 (Problems.SolveProblem37 -1) //748317
//    fnPrintProblem 38 (Problems.SolveProblem38 -1) ////932718654
//    fnPrintProblem 39 (Problems.SolveProblem39 -1) //840
//    fnPrintProblem 40 (Problems.SolveProblem40 -1) //210
    fnPrintProblem 41 (Problems.SolveProblem41 -1) //

    printfn "%s" "Pause"
    let sPause = System.Console.ReadLine();
    0 // return an integer exit code
