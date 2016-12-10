module HelperTypes
open System;

type BigNum(nDigits:int) = 
    let m_numArray = Array.zeroCreate nDigits
    
    static member FromInt(nNum:int) = 
        match nNum > 0 with
        | true -> let bigNum = new BigNum(int(Math.Log10(float(nNum))) + 1)
                  bigNum.Add(nNum) |> ignore
                  bigNum
        | false -> new BigNum(1)

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
    
    member this.MaxDigits = nDigits;

    member this.UsedDigits =
        let nZeroDigits = {nDigits-1..-1..0} |> Seq.takeWhile (fun x -> m_numArray.[x] = 0) |> Seq.length
        (nDigits - nZeroDigits);

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

type HelperFunctions () = 
    static member GetAllDigits(nNum:int) = 
        let rec fnGetDigits(nNum:int,allNums:List<int>) = 
            match nNum with
            | 0 -> allNums;
            | _ -> let nNext = nNum/10;
                   fnGetDigits(nNext, (nNum - nNext*10)::allNums);
        fnGetDigits(nNum,[]);

    static member FactorSeq(seqNums:seq<int>) = seqNums |> Seq.fold (fun agg x -> agg*x) 1
    
    static member Pow10(nPow:int) = {1..nPow} |> Seq.fold (fun agg x -> agg*10) 1
    
    static member BuildNumber(arrNums:int[]) = 
        arrNums |> Seq.mapi (fun i x -> x * HelperFunctions.Pow10(arrNums.Length-i-1)) |> Seq.sum
    
    static member BuildNumberLong(arrNums:int[]) = 
        let fnPow10 nPow = {1..nPow} |> Seq.fold (fun agg x -> agg*10L) 1L
        arrNums |> Seq.mapi (fun i x -> int64(x) * fnPow10 (arrNums.Length-i-1)) |> Seq.sum

    static member AllPrimes() = (new PrimeLogger()).AllPrimes()

    static member BuildPrimes(nMaxPrime:int) =  (new PrimeLogger()).BuildPrimes(nMaxPrime) |> Seq.toList

    static member GetAllPandigitals() = 
        let fnGetIncrementIndex(arrNums:int[]) = ({(arrNums.Length-1).. -1..1} |> Seq.filter (fun x -> arrNums.[x-1] < arrNums.[x]) |> Seq.head) - 1
        
        let fnGetNextArray(arrNums:int[]) = 
            let nIncIndex = fnGetIncrementIndex(arrNums)
            let arrOrigEnd = arrNums.[nIncIndex+1..arrNums.Length-1];

            let nIncOriginal = arrNums.[nIncIndex]
            let nIncVal = arrOrigEnd |> Seq.filter (fun x -> x > nIncOriginal) |> Seq.sort |> Seq.head
            
            let fnGetLastVal nNum = if nNum = nIncVal then nIncOriginal else nNum
            let seqEnd = arrOrigEnd |> Seq.map fnGetLastVal |> Seq.sort
            seq{yield! arrNums.[0..nIncIndex-1]; yield nIncVal; yield! seqEnd } |> Seq.toArray
        
        let rec fnFac x = match x with | 0 -> 1 | _ -> x * fnFac(x-1)
        let nLength = fnFac 10;
        let arrOriginal = {0..9} |> Seq.toArray;
        let arrArrays = Array.init (nLength) (fun x -> arrOriginal)
        {1..nLength-1} |> Seq.iter (fun x -> arrArrays.[x] <- fnGetNextArray(arrArrays.[x-1]))
        arrArrays
