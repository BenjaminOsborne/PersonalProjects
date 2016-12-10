module TestArea

    let TestArea nIgnore = 
        let IsPrimeMultipleTest nFactor x =
           x = nFactor || x % nFactor <> 0

        let rec RemoveAllMultiples listFactors listPrimes =
           match listFactors with
           | head :: tail -> RemoveAllMultiples tail (listPrimes |> List.filter (fun x -> IsPrimeMultipleTest head x))
           | [] -> listPrimes

        let GetPrimesUpTo n =
            let max = int (sqrt (float n))
            let listPrimesTo = RemoveAllMultiples [ 2 .. max ] [ 2 .. n ]
            listPrimesTo;

        let oStopwatch = System.Diagnostics.Stopwatch.StartNew();

        let listPrimes = (GetPrimesUpTo (1000 * 1000));
        let nPrimes = listPrimes |> Seq.length
        printfn "Primes Up To %d:\n %A" (1000 * 1000) listPrimes
        
        oStopwatch.Stop();
        let nElapsed = oStopwatch.ElapsedMilliseconds;
        printfn "Elapsed ms: %d" nElapsed; 
        printfn "Number Primes: %d" nPrimes; 
        
        0;

    let PrimeTestArea nIgnore = 
        let fnXIsPrime seqAllPrimesToX nX =
            (seqAllPrimesToX |> Seq.tryFind(fun x -> nX % x = 0)).IsNone;
        
        let fnGetAllPrimesToNum listAllPrimesBefore nNum =
            let nFactorLimit = int(sqrt (float nNum));
            let seqAllPossibleFactors = listAllPrimesBefore |> List.filter (fun x -> x < nFactorLimit)
            let bIsPrime = fnXIsPrime seqAllPossibleFactors nNum;
            if bIsPrime then
                nNum :: listAllPrimesBefore
            else
                listAllPrimesBefore

        let fnBuildPrimesTo nLimit =
            let mutable listPrimes = [];

            for nInt in 2..nLimit do
                listPrimes <- fnGetAllPrimesToNum listPrimes nInt
                        
            listPrimes;
        
        
        let oStopwatch = System.Diagnostics.Stopwatch.StartNew();

        let listPrimesTest = fnBuildPrimesTo (int 1000*1000)
        
        oStopwatch.Stop();

        let nElapsed = oStopwatch.ElapsedMilliseconds;
        printfn "Elapsed ms: %d" nElapsed; 

        0;

    let PrimesTest2 ignore =
        let fnNumIsPrime seqAllPrimesToNum nNum =
            let max = int (sqrt (float nNum)) 
            (seqAllPrimesToNum |> Seq.takeWhile(fun x -> x <= max) |> Seq.tryFind (fun x -> nNum % x = 0)).IsNone

        let fnFilterList listAllNums nCurrent =
            listAllNums

        let nTarget = 1000*1000;
        let listAllNumbers = [2..nTarget]

        0;

    let PrimesMod nIgnore = 
        let IsPrimeMultipleTest nFactor x =
           x = nFactor || x % nFactor <> 0

        let rec RemoveAllMultiples listFactors listPrimes =
           match listFactors with
           | head :: tail -> RemoveAllMultiples tail (listPrimes |> List.filter (fun x -> IsPrimeMultipleTest head x))
           | [] -> listPrimes

        let GetPrimesUpTo n =
            let max = int (sqrt (float n))
            let listPrimesTo = RemoveAllMultiples [ 2 .. max ] [ 2 .. n ]
            listPrimesTo;

        let oStopwatch = System.Diagnostics.Stopwatch.StartNew();

        let listPrimes = (GetPrimesUpTo 1000000);
        let nPrimes = listPrimes |> Seq.length
        printfn "Primes Up To %d:\n %A" 1000000 listPrimes
        
        oStopwatch.Stop();
        let nElapsed = oStopwatch.ElapsedMilliseconds;
        printfn "Elapsed ms: %d" nElapsed; 
        printfn "Number Primes: %d" nPrimes; 
        

        let a = [1..3]
        let b = 7::a
        let c = b @ [7]
        0;

    let PrimesRevisit nIgnore =
        let fnBuildSequence seqStart nNew = 
            seq {   for n in seqStart do
                        yield n;
                    yield nNew;} |> Seq.toList

        let fnSetIndex(nIndex:int, nValue:int, arrPrimes:int[]) = arrPrimes.[nIndex] <- nValue;

        let fnIsPrime nNum seqExisting = 
            let nLimit = int(sqrt(float(nNum)))
            seqExisting |> Seq.takeWhile (fun x -> x <= nLimit) |> Seq.forall (fun x -> nNum % x <> 0)

        let fnAddIfPrime nNew arrExisting nIndex =
            if fnIsPrime nNew arrExisting then
                fnSetIndex(nIndex, nNew, arrExisting)
                (arrExisting,nIndex+1)
            else
                (arrExisting,nIndex)

        let rec fnBuildPrimesFromTo nNum arrExisting nIndex nLimit = 
            if nNum=nLimit then
                let arrNew,nIndex = fnAddIfPrime nNum arrExisting nIndex
                arrNew;
            else
                let arrNew,nNewIndex = (fnAddIfPrime nNum arrExisting nIndex);
                fnBuildPrimesFromTo (nNum+1) arrNew nNewIndex nLimit
        
        let fnBuildPrimesTo nLimit =
            let arrInitial = {1..nLimit} |> Seq.map (fun x -> nLimit+1) |> Seq.toArray;
            fnBuildPrimesFromTo 2 arrInitial 0 nLimit |> Array.filter (fun x -> x <= nLimit) |> Array.toList

        let lstBuildTest = fnBuildPrimesTo 10

        let listBigTest = fnBuildPrimesTo (1000*1000)

        0;
