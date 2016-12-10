module HelperTypes
open System;

type BigNum(nDigits:int) = 
    let m_numArray = Array.zeroCreate nDigits
    
    static member FromInt(nNum:int) = 
        match nNum > 0 with
        | true -> let bigNum = new BigNum(int(System.Math.Log10(float(nNum))) + 1)
                  bigNum.Add(nNum) |> ignore
                  bigNum
        | false -> new BigNum(1)

    static member FromSeq(seqDigs:seq<int>, nSize:int) = 
        let oNum = new BigNum(nSize)
        seqDigs |> Seq.toList |> List.rev |> Seq.iteri(fun nx x -> oNum.SetIndex(x,nx))
        oNum;

    static member APowB(a:int, b:int) = 
        let nBase = int(System.Math.Log10(float(a)))+1
        let oNum = new BigNum(b*nBase)
        oNum.Add(a) |> ignore
        {2..b} |> Seq.iter(fun x -> oNum.Multiply(a) |> ignore)
        oNum.Digits

    member this.CopyIncrease(nAddDigits:int) = 
        let oClone = new BigNum(nDigits+nAddDigits)
        {0..nDigits-1} |> Seq.iter (fun nx -> oClone.SetIndex(m_numArray.[nx],nx))
        oClone

    member this.Add(nNum:int) = 
        let rec fnAddToIndex(nIndex:int, nNum:int) =
            let nSum = m_numArray.[nIndex] + nNum;
            match nSum > 9 with
            |true  -> m_numArray.[nIndex] <- nSum / 10
                      fnAddToIndex(nIndex+1, 1)
            |false -> m_numArray.[nIndex] <- nSum
        
        let rec fnAdd(nPow:int, nNum:int) =
            let nNext = nNum / 10
            let nRemainder = nNum - nNext*10
            fnAddToIndex(nPow-1, nRemainder)
            match nNext with
            |0 -> ignore
            |_ -> fnAdd(nPow+1,nNext)
        
        fnAdd(1,nNum)
    
    member this.Multiply(nScale:int) = 
        {0..nDigits-1} |> Seq.iter(fun x -> m_numArray.[x] <- m_numArray.[x]*nScale)
        let fnBalance nIndex = 
            let nVal = m_numArray.[nIndex];
            let nThis = nVal%10;
            this.SetIndex(nThis, nIndex);
            if nIndex < nDigits-1 then
                let nNext = nVal/10 + m_numArray.[nIndex+1];
                this.SetIndex(nNext, nIndex+1)

        {0..nDigits-1} |> Seq.iter (fun x -> fnBalance x |> ignore)

    member this.MaxDigits = nDigits;

    member this.UsedDigits =
        let nZeroDigits = {nDigits-1..-1..0} |> Seq.takeWhile (fun x -> m_numArray.[x] = 0) |> Seq.length
        (nDigits - nZeroDigits);

    member this.Digits = {this.UsedDigits-1.. -1 .. 0} |> Seq.map(fun x -> m_numArray.[x])

    member this.Number = m_numArray;
    
    member this.PrintNumber =
        let builder = System.Text.StringBuilder();
        {this.UsedDigits-1..-1..0} |> Seq.iter (fun x -> builder.Append(m_numArray.[x].ToString()) |> ignore);
        builder.ToString();

    member this.AtIndex(nIndex:int) = match nIndex < nDigits with | true -> m_numArray.[nIndex] | false -> 0

    member this.SetIndex(nVal:int,nIndex:int) = m_numArray.[nIndex] <- nVal

    static member Sum(num1:BigNum, num2:BigNum) = 
        let fnGetNum(oNum:BigNum,nIndex:int) = 
            match oNum.MaxDigits <= nIndex with
            | true -> 0
            | false -> oNum.AtIndex(nIndex)
        let nMax = System.Math.Max(num1.MaxDigits, num2.MaxDigits);
        let nMaxPost = match (fnGetNum(num1,nMax-1) + fnGetNum(num2,nMax-1)) > 8 with
                       | true -> nMax + 1 | false -> nMax
        let oNewNum = new BigNum(nMaxPost);
        
        let rec fnCombineAtIndex(nVal:int, nIndex:int) = 
            let nNext = nVal / 10;
            let nThis = nVal - nNext*10;
            oNewNum.SetIndex(nThis,nIndex)
            match nNext>0 with
            |true -> fnCombineAtIndex(oNewNum.AtIndex(nIndex+1)+nNext, nIndex+1)
            |false -> ignore

        let fnCombineAtIndex(nIndex:int) = 
            let nVal = num1.AtIndex(nIndex) + num2.AtIndex(nIndex) + oNewNum.AtIndex(nIndex);
            fnCombineAtIndex(nVal,nIndex)

        {0..nMaxPost-1} |> Seq.iter (fun x -> fnCombineAtIndex(x) |> ignore)
        oNewNum

type PrimeLogger () = 
    let listPrimes = new System.Collections.Generic.List<int>([2]);
    let setPrimes = new System.Collections.Generic.HashSet<int>([2])

    let fnAdd nPrime = 
        listPrimes.Add(nPrime)
        setPrimes.Add(nPrime) |> ignore
        nPrime

    let fnIsPrime nNum existPrimes =
        let nMax = int(System.Math.Sqrt(float(nNum)))
        existPrimes |> Seq.takeWhile (fun x -> x <= nMax) |> Seq.filter (fun x -> nNum % x = 0) |> Seq.isEmpty
    
    let fnBuildFromSeq seqNums = seqNums |> Seq.filter (fun x -> fnIsPrime x listPrimes) |> Seq.map(fun x -> fnAdd x)
    
    member this.BuildPrimes(nMaxPrime:int) = 
        let nLast = listPrimes.[listPrimes.Count-1];
        if nLast < nMaxPrime then
            fnBuildFromSeq {(nLast+1)..nMaxPrime} |> Seq.toList |> ignore
        listPrimes |> Seq.filter (fun x -> x <= nMaxPrime)

    member this.AllPrimes() = 
        let nLast = listPrimes.[listPrimes.Count-1];
        seq{ yield! this.LoggedPrimes; 
             yield! fnBuildFromSeq (Seq.initInfinite (fun x -> x+nLast+1))}
    
    member this.IsPrime(nNum:int) =
        let nLast = listPrimes.[listPrimes.Count-1];
        if nLast < nNum then
            this.BuildPrimes(nNum) |> ignore
        setPrimes.Contains nNum

    member this.LoggedPrimes = listPrimes |> Seq.map (fun x -> x)

type PrimeSieve = 
    static member GetPrimes(nNum:int, bPrint:bool) = 

        let oBuilder = new System.Text.StringBuilder();
        let oSW = System.Diagnostics.Stopwatch.StartNew();
        let fnPrint sMessage =
            if bPrint then
                let sPrint = (sMessage + " Time (s): " + (oSW.ElapsedMilliseconds/1000L).ToString())
                oBuilder.AppendLine(sPrint) |> ignore
                printfn "%s" sPrint
        
        let nSqrtMax = int(System.Math.Sqrt(float(nNum)))
        let nMemBlockSize = 1000*1000;
        let nInitialPrimes = 100
        let nParallelBatch = System.Math.Min(1000,nSqrtMax/4);

        let nBlocks = nNum / nMemBlockSize + 1
        fnPrint "Size Array"
        let arrNums = {0..nBlocks-1} |> Seq.map(fun block -> Array.init(nMemBlockSize+1)(fun x -> true) ) |> Seq.toArray
        fnPrint "End Array"
            
        let fnGetVal nNum = 
            let nBlock = nNum / nMemBlockSize;
            let nRemainder = nNum - (nMemBlockSize*nBlock)
            arrNums.[nBlock].[nRemainder]

        let fnSetFalse nNum = 
            let nBlock = nNum / nMemBlockSize;
            let nRemainder = nNum - (nMemBlockSize*nBlock)
            arrNums.[nBlock].[nRemainder] <- false
            
        let fnCancelMultiples nVal =
            if fnGetVal nVal then
                fnPrint ("Processing " + nVal.ToString() + " of " + nSqrtMax.ToString())
                {2..(nNum / nVal)} |> Seq.iter (fun x -> fnSetFalse(x*nVal))

        let fnProcessSeq seqNums = seqNums |> Seq.iter (fun x -> fnCancelMultiples x)

        let fnParrallelProcess (seqNums:seq<List<int>>) = 
            System.Threading.Tasks.Parallel.ForEach(seqNums, (fun x -> fnProcessSeq x)) |> ignore

        let fnGetBatches nLow nHigh = 
            let nNum = nHigh-nLow;
            let nBatches = nNum/nParallelBatch;
            seq { yield! {0..nBatches-1} |> Seq.map(fun x -> {nLow+x..nBatches..nHigh} |> Seq.toList)     
                  yield {nParallelBatch*nBatches..nHigh} |> Seq.toList }

        let listBatches = fnGetBatches (nInitialPrimes+1) nSqrtMax |> Seq.toList
        let nCheckCount = listBatches |> Seq.collect (fun x -> x) |> Seq.distinct |> Seq.length
        if nCheckCount <> (nSqrtMax - nInitialPrimes) then failwith "Batching failed to capture all integers" 
            
        fnPrint "Initial Primes Start"
        let seqInitialPrimes = (new PrimeLogger()).BuildPrimes(nInitialPrimes)
        fnParrallelProcess(seqInitialPrimes |> Seq.map (fun x -> [x])) //Process initial block of primes 1 per thread
            
        fnPrint "Batch Start"
        fnParrallelProcess listBatches
            
        fnPrint "Final Filter:"
        {2..nNum} |> Seq.filter (fun x -> fnGetVal x)

type HelperFunctions () = 
    static member GetAllDigitsLong(nNum:int64) = 
        let rec fnGetDigits(nNum:int64,allNums:List<int>) = 
            match nNum with
            | 0L -> allNums;
            | _ -> let nNext = nNum/10L;
                   fnGetDigits(nNext, int(nNum - nNext*10L)::allNums);
        fnGetDigits(nNum,[]);

    static member GetAllDigits(nNum:int) = HelperFunctions.GetAllDigitsLong(int64(nNum))

    static member NumDigsLong(nNum:int64) = 
        let rec fnDivide10 nVal nIter = match nVal with | 0L -> nIter | _ -> fnDivide10 (nVal/10L) (nIter+1)
        fnDivide10 nNum 0

    static member FactorSeq(seqNums:seq<int>) = seqNums |> Seq.fold (fun agg x -> agg*x) 1
    
    static member Pow10(nPow:int) = {1..nPow} |> Seq.fold (fun agg x -> agg*10) 1
    
    static member BuildNumber(arrNums:int[]) = 
        arrNums |> Seq.mapi (fun i x -> x * HelperFunctions.Pow10(arrNums.Length-i-1)) |> Seq.sum
    
    static member BuildNumberLong(arrNums:int[]) = 
        let fnPow10 nPow = {1..nPow} |> Seq.fold (fun agg x -> agg*10L) 1L
        arrNums |> Seq.mapi (fun i x -> int64(x) * fnPow10 (arrNums.Length-i-1)) |> Seq.sum

    static member AllLoggedPrimes() = (new PrimeLogger()).AllPrimes()

    static member BuildLoggedPrimes(nMaxPrime:int) =  (new PrimeLogger()).BuildPrimes(nMaxPrime) |> Seq.toList

    static member SievePrimes(nMaxPrime:int) = PrimeSieve.GetPrimes(nMaxPrime, false)

    static member SolveQuadratic a b c = 
        let nInter = int(System.Math.Sqrt(float(b*b - 4*a*c)));
        ((-b - nInter)/(2*a),(-b + nInter)/(2*a))
    static member SolveQuadraticUpper a b c = (-b + int(System.Math.Sqrt(float(b*b - 4*a*c))))/(2*a)

    static member SolveQuadraticUpperLong a b c = (-b + int64(System.Math.Sqrt(float(b*b - 4L*a*c))))/(2L*a)

    static member GetAllPandigitals() = 
        let fnGetIncrementIndex(arrNums:int[]) = ({(arrNums.Length-1).. -1..1} |> Seq.filter (fun x -> arrNums.[x-1] < arrNums.[x]) |> Seq.head) - 1
        
        let fnGetNextArray(arrNums:int[]) = 
            let nIncIndex = fnGetIncrementIndex(arrNums)
            let arrOrigEnd = arrNums.[nIncIndex+1..arrNums.Length-1];

            let nIncOriginal = arrNums.[nIncIndex]
            let nIncVal = arrOrigEnd |> Seq.filter (fun x -> x > nIncOriginal) |> Seq.sort |> Seq.head
            
            let fnGetLastVal nNum = if nNum = nIncVal then nIncOriginal else nNum
            let seqEnd = arrOrigEnd |> Seq.map fnGetLastVal |> Seq.sort
            seq { yield! arrNums.[0..nIncIndex-1]; yield nIncVal; yield! seqEnd } |> Seq.toArray
        
        let rec fnFac x = match x with | 0 -> 1 | _ -> x * fnFac(x-1)
        let nLength = fnFac 10;
        let arrOriginal = {0..9} |> Seq.toArray;
        let arrArrays = Array.init (nLength) (fun x -> arrOriginal)
        {1..nLength-1} |> Seq.iter (fun x -> arrArrays.[x] <- fnGetNextArray(arrArrays.[x-1]))
        arrArrays
