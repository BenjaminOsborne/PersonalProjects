module Problems
open System;
open Data_13;
open Data_22;
open Data_42;
open Data_54;

open HelperTypes;
    
    let SQRT x = int(System.Math.Sqrt(float(x)))
    let SQRT64 (x:int64) = int64(System.Math.Sqrt(float(x)))

    let SolveProblem1 nIgnore =
        let nResult = [1..999] |> Seq.filter (fun x -> (x%3 = 0 || x%5 = 0)) |> Seq.sum;
        nResult;
    
    let SolveProblem2 nIgnore =
        let mutable nFib1 = 1;
        let mutable nFib2 = 2;
        let mutable nSum = 2;
        let mutable bContinue = true;
        while(bContinue) do
            let nNext = nFib1 + nFib2;
            if(nNext > 4000000) then
                bContinue <- false;
            else
                if(nNext%2=0) then
                    nSum <- nSum + nNext;
                nFib1 <- nFib2;
                nFib2 <- nNext;
        nSum;

    let SolveProblem3 nIgnore =
        let fnGetFactors(nNum:int64, nFactorsUpper:int64) =
            {nFactorsUpper .. -1L .. 2L} |> Seq.filter (fun x -> nNum % x = 0L)

        let fnIsPrimeBrute nNum =
            let nFactorLimit = int(sqrt (float nNum));    
            ({2..nFactorLimit} |> Seq.tryFind(fun x -> nNum % x = 0)).IsNone

        let nNumber = 600851475143L;
        let nFactorLimit = int64(sqrt (float nNumber));

        let listFactors = fnGetFactors(nNumber, nFactorLimit) |> Seq.toList;
        let nMaxFactor = listFactors |> Seq.find (fun x -> fnIsPrimeBrute (int(x)))
        (int(nMaxFactor))

    let SolveProblem4 nIgnore =
        let nLowerLimit = 100 * 100;
        let nUpperLimit = 999 * 999;

        let fnIsPalemdrome nNum = 
            let arrReversed = (nNum.ToString()).ToCharArray() |> Array.rev;
            let sReversed = new string [|for c in arrReversed -> c|]
            let nReversed = System.Int32.Parse(sReversed)
            nReversed = nNum;

        let fnGetPalendromes nLow nUpper =
            {nLow .. nUpper} |> Seq.filter (fun x -> fnIsPalemdrome x)
        
        let fnGetFactors nNum nFactorsLowLim nFactorsHighLim =
            {nFactorsHighLim .. -1 .. nFactorsLowLim} |> Seq.filter (fun x -> (nNum % x = 0) && (nNum / x) <= nFactorsHighLim)

        let fnGetHighestFactor nNum nLowestFactor nHighestFactor = 
            let nHighest = fnGetFactors nNum nLowestFactor nHighestFactor |> Seq.tryFind (fun x -> true)
            if nHighest.IsSome then
                nHighest.Value
            else
                -1

        let listPalendromes = fnGetPalendromes nLowerLimit nUpperLimit |> Seq.toList |> List.rev; //1798 palendromes
        let nHighestFactored = listPalendromes |> Seq.find (fun x -> (fnGetHighestFactor x 100 999) > 0)
        let nMultiple = fnGetHighestFactor nHighestFactored 100 999;
        nHighestFactored;

    let SolveProblem5 nIgnore =
        
        let listNumbers = [1..20];

        let fnHasHigherFactor listNums nNum =
           listNums |> List.exists(fun x -> x <> nNum && x % nNum = 0)

        let listHighFactors = listNumbers |> List.filter (fun x -> (fnHasHigherFactor listNumbers x) = false)

        let fnAllMultiple listFactors nNum = listFactors |> List.forall (fun x -> nNum  % x = 0)
        
        let nLimit = List.fold (fun acc x -> acc * x) 1 listHighFactors

        let nLowestFactor = { 20..20..nLimit} |> Seq.find (fun x -> fnAllMultiple listHighFactors x)
        
        nLowestFactor;

    let SolveProblem6 nIgnore =
        let listNumbers = [1..100];
        let nSumSqures = List.fold (fun acc x -> acc + x * x) 0 listNumbers
        let nSum = List.fold (fun acc x -> acc + x) 0 listNumbers
        let nDifference = nSum * nSum - nSumSqures
        nDifference;

    let SolveProblem7 nIgnore =
        let nLastPrime = CSharpHelperFunctions.Primes.GetNthPrime(10001);
        nLastPrime;

    let SolveProblem8 nIgnore = 
        let sString =  "73167176531330624919225119674426574742355349194934
                        96983520312774506326239578318016984801869478851843
                        85861560789112949495459501737958331952853208805511
                        12540698747158523863050715693290963295227443043557
                        66896648950445244523161731856403098711121722383113
                        62229893423380308135336276614282806444486645238749
                        30358907296290491560440772390713810515859307960866
                        70172427121883998797908792274921901699720888093776
                        65727333001053367881220235421809751254540594752243
                        52584907711670556013604839586446706324415722155397
                        53697817977846174064955149290862569321978468622482
                        83972241375657056057490261407972968652414535100474
                        82166370484403199890008895243450658541227588666881
                        16427171479924442928230863465674813919123162824586
                        17866458359124566529476545682848912883142607690042
                        24219022671055626321111109370544217506941658960408
                        07198403850962455444362981230987879927244284909188
                        84580156166097919133875499200524063689912560717606
                        05886116467109405077541002256983155200055935729725
                        71636269561882670428252483600823257530420752963450";
        let arrFilter = sString.ToCharArray() |> Array.map (fun x -> let mutable nNum = 0;
                                                                     (System.Int32.TryParse(x.ToString(), &nNum), nNum))
                                              |> Array.filter (fun (x,y) -> x) |> Array.map (fun(x, y) -> y)
        let listIntArrays = [0 .. (Array.length(arrFilter) - 5)] |> List.map (fun x -> arrFilter.[x..x+4])
        let listProdcuts = listIntArrays |> List.map (fun x -> Array.fold (fun acc nInt -> acc * nInt) 1 x)
        let nMax = listProdcuts |> List.max
        nMax;

    let SolveProblem9 nIgnore = 
        let seqAllTuples = {1..1000/3} |> Seq.collect (fun a -> {a+1..(1000-a)/2} |> Seq.map (fun b -> (a,b,(1000-b-a))))
        let (a,b,c) = seqAllTuples |> Seq.find (fun (a,b,c) -> (a*a + b*b = c*c) && ((a + b + c) = 1000) && (c > b && b > a && a > 0)) //Final 2 checks after sqaures should be a given
        let nResult = a*b*c;
        nResult;

    let SolveProblem10 nIgnore = 
        let seqPrimes = CSharpHelperFunctions.Primes.GetAllPrimesToNum(2*1000*1000);
        let nResult = seqPrimes |> Seq.map (fun x -> (int64)x) |> Seq.sum;
        nResult;

    let SolveProblem11 nIgnore = 
        
        let sGrid =    "08 02 22 97 38 15 00 40 00 75 04 05 07 78 52 12 50 77 91 08
                        49 49 99 40 17 81 18 57 60 87 17 40 98 43 69 48 04 56 62 00
                        81 49 31 73 55 79 14 29 93 71 40 67 53 88 30 03 49 13 36 65
                        52 70 95 23 04 60 11 42 69 24 68 56 01 32 56 71 37 02 36 91
                        22 31 16 71 51 67 63 89 41 92 36 54 22 40 40 28 66 33 13 80
                        24 47 32 60 99 03 45 02 44 75 33 53 78 36 84 20 35 17 12 50
                        32 98 81 28 64 23 67 10 26 38 40 67 59 54 70 66 18 38 64 70
                        67 26 20 68 02 62 12 20 95 63 94 39 63 08 40 91 66 49 94 21
                        24 55 58 05 66 73 99 26 97 17 78 78 96 83 14 88 34 89 63 72
                        21 36 23 09 75 00 76 44 20 45 35 14 00 61 33 97 34 31 33 95
                        78 17 53 28 22 75 31 67 15 94 03 80 04 62 16 14 09 53 56 92
                        16 39 05 42 96 35 31 47 55 58 88 24 00 17 54 24 36 29 85 57
                        86 56 00 48 35 71 89 07 05 44 44 37 44 60 21 58 51 54 17 58
                        19 80 81 68 05 94 47 69 28 73 92 13 86 52 17 77 04 89 55 40
                        04 52 08 83 97 35 99 16 07 97 57 32 16 26 26 79 33 27 98 66
                        88 36 68 87 57 62 20 72 03 46 33 67 46 55 12 32 63 93 53 69
                        04 42 16 73 38 25 39 11 24 94 72 18 08 46 29 32 40 62 76 36
                        20 69 36 41 72 30 23 88 34 62 99 69 82 67 59 85 74 04 36 16
                        20 73 35 29 78 31 90 01 74 31 49 71 48 86 81 16 23 57 05 54
                        01 70 54 71 83 51 54 69 16 92 33 48 61 43 52 01 89 19 67 48";
        
        let arrDigits = sGrid.ToCharArray() |> Array.map (fun x -> let mutable nNum = 0;
                                                                   (System.Int32.TryParse(x.ToString(), &nNum), nNum))
                                            |> Array.filter (fun (x,y) -> x) |> Array.map (fun(x, y) -> y)
        let arrNumbers = [|0..2..798|] |> Array.map (fun x -> arrDigits.[x] * 10 + arrDigits.[x+1])
        let arrMatrix = [|0..20..380|] |> Array.map (fun x -> arrNumbers.[x..x+19])

        let fnDiagonalProductDR x y =  arrMatrix.[x].[y] * arrMatrix.[x+1].[y+1] * arrMatrix.[x+2].[y+2] * arrMatrix.[x+3].[y+3] //Down and Right
        let fnDiagonalProductUR x y =  arrMatrix.[x].[y] * arrMatrix.[x+1].[y-1] * arrMatrix.[x+2].[y-2] * arrMatrix.[x+3].[y-3] //Up and Right
        let fnRowProduct x y =  arrMatrix.[x].[y] * arrMatrix.[x+1].[y] * arrMatrix.[x+2].[y] * arrMatrix.[x+3].[y]
        let fnColumnProduct x y =  arrMatrix.[x].[y] * arrMatrix.[x].[y+1] * arrMatrix.[x].[y+2] * arrMatrix.[x].[y+3]
        
        let nMaxRow =    {0..16} |> Seq.collect (fun x -> {0..19} |> Seq.map (fun y -> fnRowProduct x y)) |> Seq.max
        let nMaxColumn = {0..19} |> Seq.collect (fun x -> {0..16} |> Seq.map (fun y -> fnColumnProduct x y)) |> Seq.max
        let nMaxDR =     {0..16} |> Seq.collect (fun x -> {0..16} |> Seq.map (fun y -> fnDiagonalProductDR x y)) |> Seq.max
        let nMaxUR =     {0..16} |> Seq.collect (fun x -> {3..19} |> Seq.map (fun y -> fnDiagonalProductUR x y)) |> Seq.max
        let nResult = [nMaxRow; nMaxColumn; nMaxDR; nMaxUR] |> List.max
        nResult;

    let SolveProblem12 nIgnore = 
        
        let fnGetNumberFactors nNum = 
            let nLimit = int(sqrt((float)nNum));
            let nHalfFactors = {1..nLimit} |> Seq.filter (fun x -> nNum % x = 0) |> Seq.length
            if(nLimit * nLimit = nNum) then
                nHalfFactors * 2 - 1; //if exactly square, remove one as otherwise double count the square
            else
                nHalfFactors * 2;
        
        let rec fnFindTriangle nCounter nAcc =
            if (fnGetNumberFactors nAcc) <= 500 then
                fnFindTriangle (nCounter+1) (nAcc+nCounter)
            else
                nAcc

        let nTriangle = fnFindTriangle 1 0
        nTriangle;

    let SolveProblem13 nIgnore = 
        let sNumbers = Data_13.fnGetNumbersProblem13
        let arrDigits = sNumbers.ToCharArray() |> Array.filter (fun x -> let mutable nNum = 0;
                                                                         System.Int32.TryParse(x.ToString(), &nNum))
        let lstNumbers = [|0..50..(50*99)|] |> Array.map (fun x -> new string(arrDigits.[x..x+11]))                                            |> Array.map (fun x -> System.Int64.Parse(x)) |> Array.toList

        let nSum = lstNumbers |> List.sum;
        let nResult = System.Int64.Parse(new string(nSum.ToString().ToCharArray().[0..9])) //5537376230
        nResult;

    let SolveProblem14 nIgnore = 
        let fnGetNextCollatz (nNum:int64) =
            match (nNum % 2L) with
            | 0L -> nNum / 2L
            | _ -> 3L*nNum + 1L

        let rec fnGetChain nNum nLength =
            let nNext = fnGetNextCollatz nNum 
            match nNext with
            |1L -> nLength
            |_ -> fnGetChain nNext nLength+1
        
        //Only consider odd starts, as evens immediately reduce!
        let lstLengthPair = [3L..2L..1000L*1000L] |> List.map (fun x -> (x,fnGetChain x 1))
        let nMaxLength = lstLengthPair |> List.map (fun (x,y) -> y) |> List.max
        let nStart,nLength = lstLengthPair |> List.find (fun (x,y) -> y = nMaxLength)
        int(nStart);

    let SolveProblem15 nIgnore = 
        let fnFactorial nNum = {1L..nNum} |> Seq.fold (fun acc x -> x * acc) 1L
        let fnMultipleRange nStart nStep nEnd = {nStart..nStep..nEnd} |> Seq.fold (fun acc x -> x * acc) 1L
        //nAnswer = 40! / (20! * 20!) //But this is too large in its parts
        let nBottom = (fnFactorial 20L);
        let nTopEven = (fnMultipleRange 21L 2L 39L)
        let nTopOdd = (fnMultipleRange 22L 2L 40L)
        let nDenom = (float(nBottom) / float(nTopEven));
        let nResult = int64(float(nTopOdd) / nDenom)
        nResult;

    let SolveProblem16 nIgnore = 
        
        let rec fnIncrement(nIndex:int, arrCalc:int[]) =
            let nCur = arrCalc.[nIndex];
            if(nCur = 9) then
                arrCalc.[nIndex] <- 0;
                fnIncrement(nIndex+1, arrCalc)
            else
                arrCalc.[nIndex] <- nCur + 1;
                ();

        let fnDouble(arrCalc:int[]) =
            for nIndex in {0..arrCalc.Length - 1} do
                arrCalc.[nIndex] <- arrCalc.[nIndex] * 2;
            for nIndex in {0..arrCalc.Length - 1} do
                if arrCalc.[nIndex] > 9 then
                    arrCalc.[nIndex] <- arrCalc.[nIndex] - 10;
                    fnIncrement(nIndex+1,arrCalc);
        
        let fnPowerOf2(nPower:int, arrCalc:int[]) = 
            for nCount in {1..nPower} do
                fnDouble(arrCalc);
                ();

        let arrCalc = [|0..399|] |> Array.map (fun x -> match x with | 0 -> 1 | _ -> 0)
        fnPowerOf2(1000, arrCalc);
        let nSum = arrCalc |> Array.sum
        nSum;

    let SolveProblem17 nIgnore = 
        let fnGet1To19 nNum =
            match nNum with
            | 1 -> "One" | 2 -> "Two" | 3 -> "Three" | 4 -> "Four"  | 5 -> "Five"
            | 6 -> "Six" | 7 -> "Seven" | 8 -> "Eight" | 9 -> "Nine" | 10 -> "Ten"
            | 11 -> "Eleven" | 12 -> "Twelve" | 13 -> "Thirteen" | 14 -> "Fourteen"  | 15 -> "Fifteen"
            | 16 -> "Sixteen" | 17 -> "Seventeen" | 18 -> "Eighteen" | 19 -> "Nineteen"
            | _ -> ""
        
        let fnGet10s nNum =
            match nNum with
            | 20 -> "Twenty" | 30 -> "Thirty" | 40 -> "Forty"  | 50 -> "Fifty"
            | 60 -> "Sixty" | 70 -> "Seventy" | 80 -> "Eighty"  | 90 -> "Ninety"
            | _ -> ""

        let rec fnGetNumber nNum = 
            if(nNum < 20) then
                fnGet1To19 nNum
            else if(nNum < 100) then
                let nRemainder = nNum % 10;
                fnGet10s (nNum - nRemainder) + " " + fnGetNumber nRemainder
            else if(nNum < 1000) then
                let nRemainder = nNum % 100;
                let nHundreds = (nNum - nRemainder)/100;
                if(nRemainder = 0) then
                    (fnGetNumber nHundreds) + " Hundred"
                else
                    (fnGetNumber nHundreds) + " Hundred and " + (fnGetNumber nRemainder)
            else if(nNum = 1000) then
                "One Thousand";
            else
                "";
        let sTest = fnGetNumber 20
        let nCount = [1..1000]  |> List.map (fun x -> (fnGetNumber x)+" ")
                                |> List.collect (fun x -> x.ToCharArray() |> Array.toList)
                                |> List.filter (fun x -> x <> ' ') |> List.length;
        nCount;

    let SolveProblem19 nIgnore = 
        
        let fnGetNextDayFromMax nMax nDay = if(nDay < nMax) then nDay + 1 else 1

        let fnGetNextDayFebruary nDay nYear = 
            if (nDay < 28) then nDay+1;
            else if (nDay = 28 && (nYear % 4 = 0 && (nYear % 100 <> 0 || nYear % 400 = 0))) then 29;
            else 1;

        let fnGetNextDay nDay nMonth nYear =
            match(nMonth) with
            | 4|6|9|11 -> fnGetNextDayFromMax 30 nDay
            | 1|3|5|7|8|10|12 -> fnGetNextDayFromMax 31 nDay
            | 2 -> fnGetNextDayFebruary nDay nYear;
            | _ -> -1;
        
        let fnGetNextDate nDay nMonth nYear nDayOfWeek = 
            let nNewDay = fnGetNextDay nDay nMonth nYear;
            let nNextDayOfWeek = (nDayOfWeek % 7) + 1;
            if(nNewDay > nDay) then (nNewDay,nMonth,nYear,nNextDayOfWeek)
            else if(nMonth < 12) then (1,nMonth+1,nYear,nNextDayOfWeek)
            else (1,1,nYear+1,nNextDayOfWeek)
        
        let rec fnGetAllDates (nDay,nMonth,nYear,nDayOfWeek) listDates = 
            if(nYear < 2001) then
                let oNextDate = fnGetNextDate nDay nMonth nYear nDayOfWeek;
                fnGetAllDates oNextDate (oNextDate :: listDates);
            else
                listDates.Tail;

        let oInitial = (1,1,1900,1)
        let nCount = (fnGetAllDates oInitial [oInitial]) |> List.filter (fun (nD,nM,nY,nDoW) -> nY>1900 && nD = 1 && nDoW=7)
                                                         |> List.length; //171
        nCount;

    let SolveProblem20 nIgnore = 
        
        let fnRectify (arrCalc:int[]) = 
            for nIndex in {0..arrCalc.Length - 1} do
                if arrCalc.[nIndex] > 9 then
                    let nRemainder = arrCalc.[nIndex] % 10
                    let nCarry = (arrCalc.[nIndex] - nRemainder) / 10
                    arrCalc.[nIndex] <- nRemainder;
                    arrCalc.[nIndex+1] <- arrCalc.[nIndex+1] + nCarry;
            ignore;
            
        let fnMultiply (arrCalc:int[], nFactor:int) =
            for nIndex in {0..arrCalc.Length - 1} do
                arrCalc.[nIndex] <- arrCalc.[nIndex] * nFactor;
            fnRectify arrCalc;

        let arrCalc = [|0..200|] |> Array.map (fun x -> match x with | 0 -> 1 | _ -> 0)
        for nFactor in [1..100] do fnMultiply(arrCalc,nFactor) |> ignore
        
        let nSum = arrCalc |> Array.sum
        nSum; //648

    let SolveProblem21 nIgnore = 
        
        let fnGetAmicableSum nNum = {1..nNum/2} |> Seq.filter (fun x -> nNum % x = 0) |> Seq.sum
        
        let fnIsPair(nNum:int, arrPairs: (int*int)[]) =
            let (nA,nB) = arrPairs.[nNum-1]
            if(nB > 0 && nB < 10000 && nA <> nB) then
                let (nC,nD) = arrPairs.[nB-1]
                nA = nD;
            else
                false;

        let arrTupPairs = {1..9999} |> Seq.map (fun x -> (x, fnGetAmicableSum x)) |> Seq.toArray

        let nSum = {1..9999} |> Seq.filter (fun x -> fnIsPair(x,arrTupPairs)) |> Seq.sum
        nSum; //31626

    let SolveProblem22 nIgnore = 
        
        let fnGetScore(cChar:char) = int(cChar)-64;
        let fnGetScore(sName:string) = sName.ToCharArray() |> Array.map (fun x -> fnGetScore(x))
                                                           |> Array.sum
        
        let arrSorted = Data_22.fnGetNamesProblem22 |> List.sort
                                                    |> List.map fnGetScore
                                                    |> List.toArray
        let nSum = {1..arrSorted.Length} |> Seq.map (fun x -> x * arrSorted.[x-1]) |> Seq.sum
        nSum;

    let SolveProblem23 nIgnore = 
        let fnGetSumDivisors nNum =
            let fnGetSum nFactor nNum = 
                let nOtherFac = nNum / nFactor;
                match (nFactor = nOtherFac) with
                | true -> nFactor
                | false -> if nFactor = 1 then 1 else nFactor + nOtherFac
            {1..SQRT(nNum)} |> Seq.filter (fun x -> nNum % x = 0) |> Seq.map (fun x -> fnGetSum x nNum) |> Seq.sum
        let fnIsAbundant x = (fnGetSumDivisors x) > x
        let arrAllAbundants = {12..28123} |> Seq.filter (fun x -> fnIsAbundant x) |> Seq.toArray
        let nMaxIndex = arrAllAbundants.Length-1;
        let seqAllSums = {0..nMaxIndex} |> Seq.collect (fun x -> {x..nMaxIndex} |> Seq.map (fun y -> arrAllAbundants.[x] + arrAllAbundants.[y]) |> Seq.takeWhile (fun x -> x <= 28123)) |> Seq.toList
        let setAllSums = new Set<int>(seqAllSums);
        let nSumMissing = {1..28123} |> Seq.filter (fun x -> setAllSums.Contains(x) = false) |> Seq.sum
        nSumMissing //4179871

    let SolveProblem24 nIgnore = 
        let arrOriginal = {0..9} |> Seq.toArray;
        let fnGetIncrementIndex(arrNums:int[]) = ({(arrNums.Length-1).. -1..1} |> Seq.filter (fun x -> arrNums.[x-1] < arrNums.[x]) |> Seq.head) - 1
        
        let fnGetNextArray(arrNums:int[]) = 
            let nIncIndex = fnGetIncrementIndex(arrNums)
            let arrOrigEnd = arrNums.[nIncIndex+1..arrNums.Length-1];

            let nIncOriginal = arrNums.[nIncIndex]
            let nIncVal = arrOrigEnd |> Seq.filter (fun x -> x > nIncOriginal) |> Seq.sort |> Seq.head
            
            let fnGetLastVal nNum = if nNum = nIncVal then nIncOriginal else nNum
            let seqEnd = arrOrigEnd |> Seq.map fnGetLastVal |> Seq.sort
            seq{yield! arrNums.[0..nIncIndex-1]; yield nIncVal; yield! seqEnd } |> Seq.toArray
        
        let nLength = 1000*1000;
        let arrArrays = {0..nLength-1} |> Seq.map (fun x -> [|0..9|]) |> Seq.toArray
        {1..nLength-1} |> Seq.iter (fun x -> arrArrays.[x] <- fnGetNextArray(arrArrays.[x-1]))
        //{0..nLength-1} |> Seq.iter (fun x -> (printfn "%A" arrArrays.[x]) |> ignore)
        let arrResult = arrArrays.[nLength-1];
        let oBuilder = new System.Text.StringBuilder();
        arrResult |> Array.iter (fun x -> oBuilder.Append(x.ToString()) |> ignore)
        System.Int64.Parse(oBuilder.ToString()) //2783915460
    
    let SolveProblem25 nIgnore = 
        let rec fnBuildList(listNums:List<BigNum*BigNum>) = 
            let oNum1,oNum2 = listNums.Head;
            let oNewNum = BigNum.Sum(oNum1,oNum2);
            let listAdded = (oNum2,oNewNum)::listNums
            match oNewNum.UsedDigits >= 1000 with
            |true -> listAdded
            |false -> fnBuildList(listAdded)
        let listResults = fnBuildList([BigNum.FromInt(1),BigNum.FromInt(1)])
        listResults.Length+1
        
    let SolveProblem26 nIgnore = 
        let fnGetNextDigit nDenom nCarry = (nCarry*10 / nDenom, (nCarry*10)%nDenom)
        let rec fnBuildList nDenom nCarry nLimit (listNums:List<int>) = 
            match nCarry = 0 || listNums.Length >= nLimit with
            | true -> listNums
            | _ ->  let (nDigit,nNextCarry) = fnGetNextDigit nDenom nCarry
                    let listNext = if nDigit > 0 || listNums.Length > 0 then (nDigit :: listNums) else []
                    fnBuildList nDenom nNextCarry nLimit listNext
            
        let fnGetDigits nDenom nLimit = fnBuildList nDenom 1 nLimit [] |> List.rev

        let fnArraysSame (arr1:int[]) (arr2:int[]) = arr1.Length = arr2.Length && {0..arr1.Length-1} |> Seq.forall (fun x -> arr1.[x] = arr2.[x])

        let fnIsCycle nDenom nLength (arrNums:int[]) =
            {0..nLength+1} |> Seq.exists (fun nOffset -> fnArraysSame arrNums.[nOffset..nOffset+nLength-1] arrNums.[nOffset+nLength..nOffset+nLength*2-1])

        let fnFindCycleLength nDenom =
            let nArrLengthLim = (nDenom+1)*3+2
            let arrNums = fnGetDigits nDenom nArrLengthLim |> List.toArray
            match arrNums.Length < nArrLengthLim with
            | true -> 0
            | false ->  let nMin = {0..4} |> Seq.filter (fun x -> System.Math.Pow(10.0, float(x)) > float(nDenom)) |> Seq.head;
                        {nMin..nDenom+1} |> Seq.filter (fun nCycLength -> fnIsCycle nDenom nCycLength arrNums) |> Seq.head

        let listCycles = HelperFunctions.BuildLoggedPrimes(1000) |> Seq.map (fun nDenom -> (nDenom,fnFindCycleLength nDenom)) |> Seq.sortBy (fun (a,b) -> b) |> Seq.toList |> List.rev
        let (nDenom,nCycles) = listCycles.Head;
        nDenom //983

    let SolveProblem27 nIgnore = 
        let oLogger = new PrimeLogger();

        let fnEvaluate n a b = n*n + a*n + b
        let fnIsPrime nNum = oLogger.IsPrime(nNum)
        let fnPrimeSequenceLength a b = Seq.initInfinite (fun x -> x) |> Seq.map (fun n -> fnEvaluate n a b) |> Seq.takeWhile fnIsPrime |> Seq.length

        let listBVals = oLogger.BuildPrimes(999) |> Seq.toList
        let seqABLengths = {-999..999} |> Seq.collect (fun a -> listBVals |> Seq.map (fun b -> (a,b, fnPrimeSequenceLength a b)))
        let (a,b,nLen) = seqABLengths |> Seq.sortBy (fun (a,b,len) -> -len) |> Seq.head
        a*b //-59231
        
    let SolveProblem28 nIgnore = 
        let fnDiagSumForLayer nLayer = 
            let nSideLength = 2*nLayer - 1;
            let nMaxForLayer = nSideLength * nSideLength
            nMaxForLayer*4 - 6*(nSideLength-1)
        let fnGetDiagSum nLayers = ({2..nLayers} |> Seq.map fnDiagSumForLayer |> Seq.sum) + 1
        fnGetDiagSum 501 //nSpiralSize = 1001; //nSpirals = 501 //669171001

    let SolveProblem29 nIgnore = 
        
        let fnAPowB a b = {1..b} |> Seq.fold (fun acc x -> acc * a) 1
        
        let fnGetSeq a = Seq.initInfinite (fun x -> x+2) |> Seq.map (fun b -> (a,b,fnAPowB a b)) |> Seq.takeWhile (fun (a,b,nTot) -> nTot <= 100)
        let listAllTo100 = {2..10} |> Seq.collect (fun a -> fnGetSeq a) |> Seq.toList
        let listABases = listAllTo100 |> Seq.distinctBy (fun (a,b,nTot) -> nTot) |> Seq.toList
        
        let fnGetBasePair nNum = 
            let nExists = listABases |> Seq.tryFind (fun (a,b,nTot) -> nTot = nNum)
            if nExists.IsSome then
                let (a,b,nToT) = nExists.Value;
                (a,b)
            else
                (nNum,1)

        let seqAs = {2..100} |> Seq.map fnGetBasePair |> Seq.toList
        let listDist = seqAs |> Seq.collect (fun (a,aPow) -> {2..100} |> Seq.map(fun b -> (a,aPow*b))) |> Seq.distinct |> Seq.toList
        listDist.Length //9183
    
    let SolveProblem30 nIgnore = 
        let nMax = 6 * int(System.Math.Pow(9.0,5.0))
        let fnGetPowerSum(nNum:int) = HelperFunctions.GetAllDigits(nNum) |> Seq.map (fun x -> int(System.Math.Pow(float(x),5.0))) |> Seq.sum
        let listResult = {10..nMax} |> Seq.filter (fun x -> fnGetPowerSum(x) = x) |> Seq.toList
        listResult |> List.sum

    type Coin = |P1|P2|P5|P10|P20|P50|P100|P200
    let SolveProblem31_obs nIgnore = 
        let fnGetCoins(nMax:int,eCoin:Coin) = {0..nMax} |> Seq.map (fun x -> x,eCoin);
        let fnGetCoinValue(eCoin:Coin) = match eCoin with |P1->1|P2->2|P5->5|P10->10 |P20->20|P50->50|P100->100|P200->200
        let fnGetValue nNum eCoin = fnGetCoinValue(eCoin) * nNum;
        let fnGetSumValue coinSet = coinSet |> Seq.map(fun(a,b) -> fnGetValue a b) |> Seq.sum

        let allCombs = fnGetCoins(100,Coin.P2) |> Seq.collect (fun x2 -> 
                       fnGetCoins(40,Coin.P5)  |> Seq.collect (fun x5 -> 
                       fnGetCoins(20,Coin.P10) |> Seq.collect (fun x10 -> 
                       fnGetCoins(10,Coin.P20) |> Seq.collect (fun x20 -> 
                       fnGetCoins(4,Coin.P50)  |> Seq.collect (fun x50 -> 
                       fnGetCoins(2,Coin.P100) |> Seq.collect (fun x100 -> 
                       fnGetCoins(1,Coin.P200) |> Seq.map (fun x200 -> [x2;x5;x10;x20;x50;x100;x200])))))))
        let allValsNon1 = allCombs |> Seq.map fnGetSumValue |> Seq.filter (fun x -> x <= 200) |> Seq.length
        allValsNon1 + 1

    let SolveProblem31 nIgnore = 
        let fnGetMoreTotals nCurrentSum coinVal = 
            let nYields = (200-nCurrentSum)/coinVal;
            {0..nYields} |> Seq.map (fun x -> nCurrentSum+x*coinVal)
        
        let rec fnAggregateAll(allCoins:List<int>, vals:List<int>) = 
            match allCoins with
            |[] -> vals
            |_ -> let newVals = vals |> Seq.collect (fun x -> fnGetMoreTotals x allCoins.Head) |> Seq.toList
                  fnAggregateAll(allCoins.Tail,newVals)

        let listAllValsTo2 = fnAggregateAll([200;100;50;20;10;5;2],[0])
        let nCount = (listAllValsTo2 |> List.length)
        nCount; //73682

    let SolveProblem32 nIgnore = 
        let fnIsPanDigital(num1:int,num2:int) = 
            let allDigits = seq { for x in [num1;num2;num1*num2] do
                                    yield! HelperFunctions.GetAllDigits(x); } |> Seq.toList
            allDigits.Length = 9 && (allDigits |> Seq.filter (fun x -> x = 0) |> Seq.isEmpty)
                                 && (allDigits |> Seq.distinct |> Seq.length = 9)

        let pairs1 = {10..99} |> Seq.collect (fun x -> {100..999} |> Seq.map (fun y -> x,y));
        let pairs2 = {1..9} |> Seq.collect (fun x -> {1000..9999} |> Seq.map (fun y -> x,y));
        let panDigitals = seq { yield! pairs1; yield! pairs2 }
                          |> Seq.filter fnIsPanDigital
                          |> Seq.map (fun (a,b) -> a*b)
        let sum = panDigitals |> Seq.distinct |> Seq.sum
        sum

    let SolveProblem33 nIgnore = 
        let fnIsCase nNumer nDenom = 
            let arrTop = HelperFunctions.GetAllDigits(nNumer) |> Seq.toArray
            let arrBot = HelperFunctions.GetAllDigits(nDenom) |> Seq.toArray
            
            let tupMatch = {0..1} |> Seq.collect (fun nT -> {0..1} |> Seq.map (fun nB -> (nT,nB)))
                           |> Seq.tryFind (fun (nT,nB) -> arrTop.[nT] <> 0 && arrTop.[nT] = arrBot.[nB])
            if tupMatch.IsSome then
                let nT,nB = tupMatch.Value;
                arrTop.[1-nT] * nDenom = arrBot.[1-nB] * nNumer
            else
                false;
        
        let listCases = {10..99} |> Seq.collect (fun nTop -> {nTop+1..99} |> Seq.filter (fun nBot -> fnIsCase nTop nBot)
                                                                          |> Seq.map (fun nBot -> (nTop,nBot))) |> Seq.toList
        let nTopAgg = listCases |> Seq.map (fun (nT,nB) -> nT) |> Seq.fold (fun agg x -> agg*x) 1
        let nBotAgg = listCases |> Seq.map (fun (nT,nB) -> nB) |> Seq.fold (fun agg x -> agg*x) 1

        let nDivide = {nTopAgg .. -1 .. 1} |> Seq.filter (fun x -> nTopAgg % x = 0 && nBotAgg % x = 0) |> Seq.head
                
        let nTopFinal = nTopAgg / nDivide
        let nBotFinal = nBotAgg / nDivide
        nBotFinal

    let SolveProblem34 nIgnore = 
        let rec fnFactorial nNum = match nNum with
                                   | 0 -> 1
                                   | _ -> nNum * fnFactorial (nNum-1)
        let arrFacs = {0..9} |> Seq.map (fun x -> fnFactorial x) |> Seq.toArray
        let fnGetMax nDigits = arrFacs.[9] * nDigits;
        let fnPow10 nDigits = int(System.Math.Pow(10.0,float(nDigits)))
        let nMaxDigits = {1..100} |> Seq.takeWhile (fun x -> fnGetMax x > fnPow10 x) |> Seq.last
        let fnIsCurious nVal = (nVal = (HelperFunctions.GetAllDigits(nVal) |> Seq.map (fun x -> arrFacs.[x]) |> Seq.sum))
        let listCurious = [10..(fnGetMax nMaxDigits)] |> List.filter fnIsCurious
        let nSum = listCurious |> List.sum
        nSum

    let SolveProblem35 nIgnore = 
        let rec fnToNum nNum remInts nPower = 
            match remInts with
            |[] -> nNum
            | _ -> fnToNum (nNum + remInts.Head*nPower) remInts.Tail (nPower*10)
        let fnBuildNum listInts = fnToNum 0 listInts 1
        
        let fnGetIndexes nStart nLength = {nStart..(nLength+nStart-1)} |> Seq.map (fun x -> x % nLength)
        let fnGetRotations nNum = 
            let arrDigits = HelperFunctions.GetAllDigits(nNum) |> Seq.toList |> List.rev |> List.toArray
            let nLength = arrDigits.Length
            {0..nLength-1} |> Seq.map (fun x -> fnGetIndexes x nLength |> Seq.map (fun i -> arrDigits.[i]) |> Seq.toList |> fnBuildNum)
        
        let listPrimes = HelperFunctions.BuildLoggedPrimes(1000*1000)
        let setNums = new Set<int>(listPrimes);
        let circularNums = listPrimes |> Seq.filter (fun x -> fnGetRotations x |> Seq.filter (fun y -> setNums.Contains(y) = false) |> Seq.isEmpty) |> Seq.toList
        circularNums.Length;

    let SolveProblem36 nIgnore = 
        let fnGetDecimal nNum = HelperFunctions.GetAllDigits(nNum) |> Seq.toArray
        let fnGetBinary (nNum:int) = System.Convert.ToString(nNum,2) |> Seq.map (fun x -> System.Int32.Parse(x.ToString()))
                                                                     |> Seq.toArray
        
        let fnAreSame (arr1:int[], arr2:int[]) = 
            arr1.Length = arr2.Length && {0..arr1.Length-1} |> Seq.forall (fun x -> arr1.[x] = arr2.[x])
        
        let fnArrayIsPalendrome (arrNums:int[]) = 
            let nLength = arrNums.Length;
            let nLowEnd = (nLength-1)/2
            let nHighStart = nLength/2
            let arrLow = arrNums.[0..nLowEnd]
            let arrHigh = arrNums.[nHighStart..nLength-1] |> Array.rev
            fnAreSame(arrLow,arrHigh)
        
        let fnIsPalendrome nNum = fnArrayIsPalendrome (fnGetDecimal nNum) && fnArrayIsPalendrome (fnGetBinary(nNum))

        let listNums = {1..999999} |> Seq.filter fnIsPalendrome |> Seq.toList
        listNums |> Seq.sum //872187

    let SolveProblem37 nIgnore = 
        let oLogger = new PrimeLogger();

        let fnIsTruncatable nPrime =
            let arrDigits = HelperFunctions.GetAllDigits(nPrime) |> Seq.toArray
            let nLen = arrDigits.Length
            {1..nLen-1} |> Seq.map(fun x -> HelperFunctions.BuildNumber(arrDigits.[0..x-1])) |> Seq.forall oLogger.IsPrime &&
            {1..nLen-1} |> Seq.map(fun x -> HelperFunctions.BuildNumber(arrDigits.[x..nLen-1])) |> Seq.forall oLogger.IsPrime
        
        let listTrunk = oLogger.AllPrimes() |> Seq.filter (fun x -> x > 7 && fnIsTruncatable x) |> Seq.take(11) |> Seq.toList
        listTrunk |> Seq.sum //748317

    let SolveProblem38 nIgnore = 
        let fnIsValid seqNums = 
            let arrNums = seqNums |> Seq.sort |> Seq.toArray;
            arrNums.Length = 9 && {0..8} |> Seq.forall (fun x -> arrNums.[x] = x+1)

        let fnGetPanDig nLeft n = 
            let seqNums = {1..n} |> Seq.collect (fun x -> HelperFunctions.GetAllDigits(x*nLeft))
            if fnIsValid seqNums then
                HelperFunctions.BuildNumberLong (seqNums |> Seq.toArray)
            else
                0L
        
        let fnPow10 nPow = {1..nPow} |> Seq.fold (fun agg x -> agg*10) 1
        let fnGetLim n = fnPow10 (9/n)

        let listVals = {2..9} |> Seq.collect (fun n -> {1..(fnGetLim n)} |> Seq.map(fun nVal -> fnGetPanDig nVal n))
                              |> Seq.filter (fun x -> x >0L) |> Seq.sortBy(fun x -> -x) |> Seq.toList
        listVals.Head //932718654

    let SolveProblem39 nIgnore = 
        let nMaxSide = 500; //1000/2
        let fnGetHypot a b = SQRT(a*a + b*b)
        let fnIsRightAngle a b = let c = fnGetHypot a b
                                 c*c-b*b = a*a
        let fnGetPerim a b = a + b + fnGetHypot a b
        let seqRightAngles = {1..nMaxSide} |> Seq.collect (fun x -> {x..nMaxSide} |> Seq.map (fun y -> x,y)) |> Seq.filter (fun (a,b) -> fnIsRightAngle a b)
        let seqPerims = seqRightAngles |> Seq.map (fun (a,b) -> fnGetPerim a b)
        let listByFreq = seqPerims |> Seq.groupBy (fun x -> x) |> Seq.map (fun (k,s) -> k, (s |> Seq.length)) |> Seq.sortBy (fun (a,b) -> b) |> Seq.toList
        let (perim,freq) = listByFreq |> Seq.last
        perim

    let SolveProblem40 nIgnore = 
        let arrNums = Seq.initInfinite (fun x -> x+1) |> Seq.collect (fun x -> HelperFunctions.GetAllDigits(x))|> Seq.mapi (fun i nV -> (i,nV)) |> Seq.takeWhile (fun (i,x) -> i < 1000000) |> Seq.map (fun (i,x) -> x) |> Seq.toArray
        let listIndexes = {0..6} |> Seq.map (fun x -> arrNums.[HelperFunctions.Pow10(x)-1]) |> Seq.toList
        let nFac = HelperFunctions.FactorSeq(listIndexes)
        nFac //210

    let SolveProblem41 nIgnore = 
        let fnIsPandigital nNum = 
            let digits = HelperFunctions.GetAllDigits(nNum) |> Seq.sortBy (fun x -> x) |> Seq.toArray
            {1..digits.Length} |> Seq.forall (fun x -> x = digits.[x-1])
        
        let listPrimes = PrimeSieve.GetPrimes(987654321, true) |> Seq.filter fnIsPandigital |> Seq.toList
        
        listPrimes |> Seq.last //7652413

    let SolveProblem42 nIgnore = 
        //let arrStrings = [|"A";"ABILITY";"ABLE";"ABOUT";"ABOVE";"ABSENCE";"ABSOLUTELY";"ACADEMIC";"ACCEPT";"ACCESS";"ACCIDENT";|];
        let arrStrings = Data_42.GetData()
        
        let arrChars = [|'A'..'Z'|];
        let fnGetValue cChar =
            (arrChars |> Array.findIndex(fun x -> x = cChar)) + 1
        
        let listVals = arrStrings |> Seq.map(fun x -> x.ToCharArray() |> Seq.map fnGetValue |> Seq.sum) |> Seq.toList
        let nMaxVal = listVals |> Seq.max

        let seqTriangles = Seq.initInfinite (fun x -> x+1) |> Seq.map(fun x -> (x * (x+1))/2) |> Seq.takeWhile(fun x -> x <= nMaxVal)
        let setTriangles = new Set<int>(seqTriangles)

        let nTriangleWords = listVals |> Seq.filter(fun x -> setTriangles.Contains(x)) |> Seq.length
        nTriangleWords //162

    let SolveProblem43 nIgnore = 
        let arrPandigitals = HelperFunctions.GetAllPandigitals();
        let arrPrimes = HelperFunctions.BuildLoggedPrimes(17) |> Seq.toArray
        let fnIsValid (arrNums:int[]) = {1..7} |> Seq.forall (fun x -> (HelperFunctions.BuildNumber(arrNums.[x..x+2]) % arrPrimes.[x-1])=0)
        let listValid = arrPandigitals |> Seq.filter fnIsValid |> Seq.toList
        let nSum = listValid |> Seq.map(fun x -> HelperFunctions.BuildNumberLong(x)) |> Seq.sum
        
        nSum //16695334890

    let SolveProblem44 nIgnore = 
        let fnGetPentag x = (x*(3*x-1))/2
        let fnIsPentag x = (x = (fnGetPentag (HelperFunctions.SolveQuadraticUpper 3 (-1) (-2*x))))
        let fnIsSumAndDiffPandig nIndexLow nIndexHigh =
            let panLow = fnGetPentag nIndexLow
            let panHigh = fnGetPentag nIndexHigh
            if fnIsPentag(panHigh-panLow) && fnIsPentag(panHigh+panLow) then
                Some(panHigh - panLow)
            else
                None

        let fnGetDifferencesTo nMax =
            {2..nMax} |> Seq.collect (fun nIndex -> {1..nIndex-1} |> Seq.map (fun x -> fnIsSumAndDiffPandig x nIndex)
                                                                  |> Seq.filter(fun x -> x.IsSome)
                                                                  |> Seq.map (fun x -> x.Value))

        let listDiff = fnGetDifferencesTo 10000 |> Seq.toList
        let nDiff = listDiff |> Seq.sort |> Seq.head
        nDiff // 5482660

    let SolveProblem45 nIgnore = 
        let fnGetTriangle x = (x*(x+1L))/2L
        let fnIsTriang x = (x = (fnGetTriangle (HelperFunctions.SolveQuadraticUpperLong 1L 1L (-2L*x))))
        
        let fnGetPentag x = (x*(3L*x-1L))/2L
        let fnIsPentag x = (x = (fnGetPentag (HelperFunctions.SolveQuadraticUpperLong 3L (-1L) (-2L*x))))

        let fnGetHexag x = x*(2L*x-1L)
        let fnIsHexag x = (x = (fnGetHexag (HelperFunctions.SolveQuadraticUpperLong 2L (-1L) (-x))))
        
        let fnIsHexIndexAllThree x = 
            let nHexag = fnGetHexag x
            fnIsTriang nHexag && fnIsPentag nHexag
        
            //40756
        let nHexIndexAllThree = Seq.initInfinite(fun x -> int64(x+144)) |> Seq.filter fnIsHexIndexAllThree |> Seq.head
        let nNum = fnGetHexag nHexIndexAllThree
        nNum //1533776805

    let SolveProblem46 nIgnore = 
        let oLogger = new PrimeLogger();
        
        let fnIsSquareDiff nOddNum nPrime = 
            let nDiff = nOddNum - nPrime
            match nDiff % 2 <> 0 with
            | true -> false
            | false -> let nTest = SQRT(nDiff/2)
                       nTest*nTest*2 = nDiff

        let fnIsNotComposite nOddNum = 
            let arrPrimes = oLogger.BuildPrimes(nOddNum) |> Seq.toArray
            {arrPrimes.Length-1 .. -1 .. 0} |> Seq.filter (fun x -> fnIsSquareDiff nOddNum arrPrimes.[x]) |> Seq.isEmpty
            
        let nNonComposite = Seq.initInfinite(fun x -> 2*x+3) |> Seq.filter (fun x -> (oLogger.IsPrime(x) = false) && fnIsNotComposite x) |> Seq.head
        nNonComposite //5777

    type FactorsAndCount = {Factors:List<int>; Count:int}
    let SolveProblem47 nIgnore = 
        let oLogger = new PrimeLogger();

        let nSize = 200*1000
        let arrFactors = {0..nSize+1} |> Seq.map(fun x -> { Factors = []; Count = 0}) |> Seq.toArray
        
        let fnBuildFactor nNum = 
            if oLogger.IsPrime(nNum) then
                arrFactors.[nNum] <- {Factors = [nNum]; Count = 1}
            else
                let nLowDenom = oLogger.BuildPrimes(nNum) |> Seq.filter(fun x -> nNum%x = 0) |> Seq.head
                let listFactors = nLowDenom :: arrFactors.[nNum/nLowDenom].Factors;
                arrFactors.[nNum] <- {Factors = listFactors; Count = listFactors |> Seq.distinct |> Seq.length}
            arrFactors.[nNum]
            
        let fn4thFound nNum = (fnBuildFactor nNum).Count = 4 && arrFactors.[nNum-1].Count = 4 && arrFactors.[nNum-2].Count = 4 && arrFactors.[nNum-3].Count = 4
            
        let nIndex4th = {2..nSize} |> Seq.filter fn4thFound |> Seq.head
        let n1st = nIndex4th - 3;
        n1st //134043

    let SolveProblem48 nIgnore = 
        let nBase = int64(System.Math.Pow(10.0,10.0))
        let fnNPowNModBase n = {1L..n} |> Seq.fold (fun acc x -> (acc*n) % nBase) 1L
        let rawSum = {1L..1000L} |> Seq.map fnNPowNModBase |> Seq.sum
        let nResult = rawSum % nBase
        nResult //9110846700

    let SolveProblem49 nIgnore = 
        let listPrimes = HelperFunctions.BuildLoggedPrimes(10000) |> Seq.filter (fun x -> x > 999) |> Seq.toList

        let fnGetOrderedNum x = HelperFunctions.BuildNumber(HelperFunctions.GetAllDigits(x) |> Seq.sort |> Seq.toArray)
        let listOrdered = listPrimes |> Seq.map (fun x -> (fnGetOrderedNum x, x))
                                     |> Seq.groupBy (fun (nLow,nPrime) -> nLow)
                                     |> Seq.filter (fun (key,nums) -> nums |> Seq.length >= 4)
                                     |> Seq.map (fun (key,nums) -> (key, nums |> Seq.map(fun (nLow,nPrime) -> nPrime) |> Seq.sort |> Seq.toList))
                                     |> Seq.toList

        let fnGetIndexesTo nLength = {0..nLength-3} |> Seq.collect (fun x1 -> {x1+1..nLength-2} |> Seq.collect(fun x2 -> {x2+1..nLength-1} |> Seq.map(fun x3 -> [|x1;x2;x3|])))

        let fnGet3SameDifferences seqNums = 
            let arrNums = seqNums |> Seq.toArray
            (fnGetIndexesTo arrNums.Length) |> Seq.map(fun x -> (x,(arrNums.[x.[2]]-arrNums.[x.[1]]),(arrNums.[x.[1]]-arrNums.[x.[0]])))
                                            |> Seq.filter (fun (arr,a,b) -> a = b) 
                                            |> Seq.map(fun (arr,a,b) -> arr)
                                            |> Seq.tryFind(fun x -> true)
        
        let (_,listResults) = listOrdered |> Seq.filter (fun(key,nums) -> (fnGet3SameDifferences nums).IsSome && key<> 1478) |> Seq.head
        let arrIndexes = (fnGet3SameDifferences listResults).Value
        let arrNums = listResults |> Seq.toArray
        let seqNums = arrIndexes |> Seq.collect(fun x -> HelperFunctions.GetAllDigits(arrNums.[x]))
        let nCombined = HelperFunctions.BuildNumberLong(seqNums |> Seq.toArray)
        nCombined; //296962999629

    let SolveProblem50 nIgnore = 
        let oSW = System.Diagnostics.Stopwatch.StartNew();
        let oLogger = new PrimeLogger();
        let nPrimeLimit = 1000*1000;
        let arrPrimes = oLogger.BuildPrimes(nPrimeLimit) |> Seq.toArray
        let nLength = arrPrimes.Length
        let fnGetPrimeSum nStart nEnd = {nStart..nEnd} |> Seq.takeWhile (fun x -> x < nLength) |> Seq.map(fun x -> arrPrimes.[x]) |> Seq.sum
        
        let fnGetLengthsFrom nStart =
            {nStart+1..nLength} |> Seq.map (fun nEnd -> nEnd,fnGetPrimeSum nStart nEnd)
                                |> Seq.takeWhile (fun (nEnd,nSum) -> nSum < nPrimeLimit)
                                |> Seq.filter (fun (nEnd,nSum) -> oLogger.IsPrime(nSum))
                                |> Seq.map (fun (nEnd,nSum) -> nEnd-nStart+1)
                                |> Seq.sortBy (fun nLen -> -nLen)
                                |> Seq.tryFind (fun x -> true)
        let seqResults = {0..nLength/2} |> Seq.map(fun x -> x,fnGetLengthsFrom x)
                                        |> Seq.filter (fun (a,b) -> b.IsSome)
                                        |> Seq.map(fun (a,b) -> (a,b.Value))
        
        let arrMaxLength = [|0|];
        let fnIsLimitReached(pair:int*int) = 
           let (nStart,nLen) = pair;
           arrMaxLength.[0] <- System.Math.Max(arrMaxLength.[0],nLen)
           arrPrimes.[nStart]*arrMaxLength.[0] > nPrimeLimit

        let (nStart,nLen) = seqResults |> Seq.takeWhile (fun x -> (fnIsLimitReached x) = false) |> Seq.maxBy (fun (a,b) -> b)
        let nPrime = fnGetPrimeSum nStart (nStart+nLen-1)
        oSW.Stop();
        printfn "%d ms" oSW.ElapsedMilliseconds
        nPrime //997651

     let SolveProblem51 nIgnore = 
        
        let nDigits = 6;
        let nSequenceLengh = 8;

        let nPow10 = int(System.Math.Pow(10.0, float(nDigits)));
        let nMin = nPow10/10
        let nMax = nPow10-1
        
        let oLogger = new PrimeLogger();
        let seqPrimes = oLogger.BuildPrimes(nMax)
        let list5DigNumbers = seqPrimes |> Seq.filter(fun x -> x >= nMin && x <= nMax) |> Seq.toList

        let fnSpawnNext(nLim:int, combs:List<int>) =
            match combs.Head >= nLim with
            | true -> Seq.empty
            | false -> {combs.Head+1..nLim} |> Seq.map(fun x -> x::combs)

        let rec fnSpawnAll(nLim:int, combsToAdd:List<List<int>>, combsAll:List<List<int>>) =
            match combsToAdd with
            | [] -> combsAll
            | _ -> let combsAllNext = combsToAdd |> List.fold (fun agg x -> x::agg) combsAll
                   let combsNext = combsToAdd |> Seq.collect(fun x -> fnSpawnNext(nLim,x)) |> Seq.toList
                   fnSpawnAll(nLim, combsNext, combsAllNext)

        let fnGetAllIndexsTo nLim =
            let listInit = {0..nLim} |> Seq.map(fun x -> [x]) |> Seq.toList
            fnSpawnAll(nLim,listInit,[[]]) |> Seq.filter(fun x -> x.Length > 0)
        
        let arrIndexCombs = {0..10} |> Seq.map(fun x -> fnGetAllIndexsTo (x-1) |> Seq.toList) |> Seq.toArray

        let fnGetPositions(listNums:int[]) = 
            let combs = arrIndexCombs.[listNums.Length];
            combs |> Seq.map (fun x -> x |> Seq.map (fun idx -> listNums.[idx]) |> Seq.toList)

        let fnGetAllPositionsSame nNum = 
            let arrDigits = HelperFunctions.GetAllDigits(nNum) |> Seq.toArray
            let fnPositionsSame nVal = {0..arrDigits.Length-1} |> Seq.filter (fun ndx -> arrDigits.[ndx] = nVal) |> Seq.toArray
            let listPossibles = arrDigits |> Seq.distinct |> Seq.map fnPositionsSame
            listPossibles |> Seq.map(fun x -> fnGetPositions x |> Seq.toList) |> Seq.collect(fun x -> x)

        let fnGetNumFromPosition(seqPrimeDigs:seq<int>, position:List<int>, nVal:int) = 
            let arrNew = seqPrimeDigs |> Seq.toArray;
            position |> Seq.iter(fun x -> arrNew.[x] <- nVal)
            HelperFunctions.BuildNumber(arrNew)

        let fnGetPrimesFromPosition(seqPrimeDigs:seq<int>, position:List<int>) =
            {0..9} |> Seq.map(fun x -> fnGetNumFromPosition(seqPrimeDigs,position,x))
                   |> Seq.filter(fun x -> oLogger.IsPrime(x))

        let fnPrimeSequenceLength nPrime = 
            let seqPositions = fnGetAllPositionsSame nPrime
            let seqPrimeDigs = HelperFunctions.GetAllDigits(nPrime)
            seqPositions |> Seq.map (fun x -> fnGetPrimesFromPosition(seqPrimeDigs,x) |> Seq.toList)
                         |> Seq.filter (fun x -> x.Length >= nSequenceLengh)

        let ordPrimes = list5DigNumbers |> Seq.map(fun x -> fnPrimeSequenceLength x) |> Seq.collect (fun x -> x)
                                        |> Seq.filter (fun x -> x.Head >= nMin)
                                        |> Seq.distinct |> Seq.sortBy (fun x -> -x.Length) |> Seq.toList
        let nFirstPrime = ordPrimes.Head.Head; //121313
        nFirstPrime

    let SolveProblem52 nIgnore = 
        let fnAreSame(arr1:int[], arr2:int[]) = arr1.Length = arr2.Length && {0..arr1.Length-1} |> Seq.forall(fun nx -> arr1.[nx] = arr2.[nx])
        let fnContainSame nNum = 
            let fnGetDigits x = HelperFunctions.GetAllDigits(x) |> Seq.sort |> Seq.toArray;
            let arrDigits = fnGetDigits (nNum*2)
            {6.. -1 .. 3} |> Seq.map(fun x -> fnGetDigits (x*nNum)) |> Seq.forall (fun x -> fnAreSame(arrDigits,x))
        let nAnswer = Seq.initInfinite(fun x -> x+1) |> Seq.filter fnContainSame |> Seq.head
        nAnswer //142857

    let SolveProblem53 nIgnore = 
        let fnFacRange nLow nHigh = {nLow..nHigh} |> Seq.filter(fun x -> x > 0L) |> Seq.fold (fun acc nFac -> acc * nFac) 1L
        let fnDivideSequences seqTop seqBot = 
            let fnElemOr1(arr:int[],nx:int) = if arr.Length > nx then float(arr.[nx]) else 1.0
            let arrTop = seqTop |> Seq.toArray
            let arrBot = seqBot |> Seq.toArray
            let nLen = System.Math.Max(arrTop.Length, arrBot.Length)
            {0..nLen-1} |> Seq.fold (fun acc nx -> acc * (fnElemOr1(arrTop,nx)) / (fnElemOr1(arrBot,nx))) 1.0
        let fnNCR n r = fnDivideSequences {(r+1)..n} {1..(n-r)}

        let listGreater = {1..100} |> Seq.collect(fun n -> {1..n} |> Seq.map(fun r -> fnNCR n r)) |> Seq.filter(fun x -> x > 1E6) |> Seq.toList
        let nCount = listGreater.Length
        nCount //4075

    type Suit = H|C|D|S
    type Card = { Suit:Suit; CardNumber:int }
    type Rank = StraightFlush=8|FourKind=7|FullHouse=6|Flush=5|Straight=4|ThreeKind=3|Pairs=2|HighCard=1
    type HandValue = { HandRank:Rank; RankScale:int }
    let SolveProblem54 nIgnore = 
        
        let sHands = Data_54.GetHands();
        let arrPairs = sHands.Split(' ') |> Seq.map(fun x -> x.Trim()) |> Seq.filter(fun x -> x.Length = 2) |> Seq.toArray;
        if arrPairs.Length <> 10000 then failwith "string load error"
        let fnMapNumber cNum = 
            match cNum with
            |'A' -> 14 |'K' -> 13 |'Q' -> 12 |'J' -> 11 |'T' -> 10
            | _ -> System.Int32.Parse(cNum.ToString())

        let fnMapCard(sPair:string) = 
            match sPair.ToCharArray() with
            |[|n;'C'|] -> { Suit = Suit.C; CardNumber = fnMapNumber n }
            |[|n;'D'|] -> { Suit = Suit.D; CardNumber = fnMapNumber n }
            |[|n;'H'|] -> { Suit = Suit.H; CardNumber = fnMapNumber n }
            |[|n;'S'|] -> { Suit = Suit.S; CardNumber = fnMapNumber n }
            |_ -> failwith "parse error" 

        let arrCards = arrPairs |> Seq.map(fun x -> fnMapCard(x)) |> Seq.toArray
        let arrPlayer1 = {0..999} |> Seq.map(fun x -> {x*10..x*10+4}   |> Seq.map(fun y -> arrCards.[y]) |> Seq.toArray) |> Seq.toArray
        let arrPlayer2 = {0..999} |> Seq.map(fun x -> {x*10+5..x*10+9} |> Seq.map(fun y -> arrCards.[y]) |> Seq.toArray) |> Seq.toArray

        let fnIsFlush seqCards = seqCards |> Seq.map(fun x -> x.Suit) |> Seq.distinct |> Seq.length = 1
        
        let fnValueStraight seqCards =
            let arrFiltered = seqCards |> Seq.map(fun x -> x.CardNumber) |> Seq.distinct |> Seq.sort |> Seq.toArray
            match arrFiltered.Length = 5 && arrFiltered.[0] = arrFiltered.[4] - 4 with
            | true -> arrFiltered.[4]
            | false -> -1
        
        let fnValueXOfKind x seqCards =
            seqCards |> Seq.groupBy(fun x -> x.CardNumber) |> Seq.filter (fun (a,b) -> b |> Seq.length = x)
                                                           |> Seq.map(fun (a,b) -> a)
                                                           |> Seq.sortBy (fun x -> -x)
                                                           |> Seq.fold(fun acc x -> acc*20 + x) 0
        
        let fnValueHand(arrCards:Card[]) =
            let isFlush = fnIsFlush arrCards
            let nValStraight = fnValueStraight arrCards
            let nVal4Kind = fnValueXOfKind 4 arrCards
            let nVal3Kind = fnValueXOfKind 3 arrCards
            let nVal2Kind = fnValueXOfKind 2 arrCards
            let nVal1Kind = fnValueXOfKind 1 arrCards
            
            if isFlush && nValStraight > 0 then
                { HandRank = Rank.StraightFlush; RankScale = nValStraight }
            else if nVal4Kind > 0 then
                { HandRank = Rank.FourKind; RankScale = nVal4Kind }
            else if nVal3Kind > 0 && nVal2Kind > 0 then
                { HandRank = Rank.FullHouse; RankScale = nVal3Kind * 20 + nVal2Kind }
            else if isFlush then
                { HandRank = Rank.Flush; RankScale = nVal1Kind }
            else if nValStraight > 0 then
                { HandRank = Rank.Straight; RankScale = nValStraight }
            else if nVal3Kind > 0 then
                { HandRank = Rank.ThreeKind; RankScale = nVal3Kind*400+nVal1Kind }
            else if nVal2Kind > 0 then
                { HandRank = Rank.Pairs; RankScale = nVal2Kind*8000+nVal1Kind }
            else if nVal1Kind > 0 then
                { HandRank = Rank.HighCard; RankScale = nVal1Kind }
            else
                failwith "rank error"

        let fnHand1Wins arrHand1 arrHand2 = 
            let val1 = fnValueHand arrHand1
            let val2 = fnValueHand arrHand2
            if val1.HandRank = val2.HandRank then
                val1.RankScale > val2.RankScale;
            else
                val1.HandRank > val2.HandRank
        
        let n1Wins = {0..999} |> Seq.filter(fun x -> fnHand1Wins arrPlayer1.[x] arrPlayer2.[x]) |> Seq.length
        n1Wins; //376
    
    let SolveProblem55 nIgnore = 
        let fnAreSame seq1 seq2 = ((seq1 |> List.length) = (seq2 |> List.length)) && List.forall2(fun x1 x2 -> x1 = x2) seq1 seq2
        let fnIsPalendrome listDigs =  fnAreSame listDigs (listDigs |> List.rev)

        let fnGetNextNum(oNum:BigNum) =
            let listDigsRev = oNum.Digits |> Seq.toList |> List.rev
            let oNumRev = BigNum.FromSeq(listDigsRev, 30)
            BigNum.Sum(oNum,oNumRev)
        
        let rec fnIncSeqLength nCount nNum =
            let oNextNum = fnGetNextNum nNum
            if fnIsPalendrome (oNextNum.Digits |> Seq.toList) then
                nCount+1
            else if nCount > 50 then
                nCount
            else
                fnIncSeqLength (nCount+1) oNextNum

        let fnGetSeqLength nNum = fnIncSeqLength 0 (BigNum.FromInt(nNum))

        let listNums = {1..9999} |> Seq.map fnGetSeqLength |> Seq.filter (fun x -> x > 50) |> Seq.toList
        listNums.Length //249

    let SolveProblem56 nIgnore = 
        let fnGetSum a b = BigNum.APowB(a,b) |> Seq.sum
        let listSums = {1..100} |> Seq.collect(fun a -> {1..100} |> Seq.map(fun b -> a,b,(fnGetSum a b)))
                                |> Seq.sortBy(fun (a,b,x) -> -x) |> Seq.toList
        let (a,b,nSum) = listSums.Head
        nSum //972

    let SolveProblem57 nIgnore = 
        //Solved but solution was wiped by computer!
        0 //153

    let SolveProblem58 nIgnore = 
        let arrPrimes = HelperFunctions.SievePrimes(700*1000*1000)
        let setPrimes = new Set<int>(arrPrimes)
        
        let fnGetLayerCorners nLayer = 
            let nSideLength = 2*nLayer - 1;
            let nMaxForLayer = nSideLength * nSideLength
            let nMinus = nSideLength-1
            seq { yield nMaxForLayer;yield nMaxForLayer-nMinus;yield nMaxForLayer-2*nMinus;yield nMaxForLayer-3*nMinus }
        
        let rec fnGetFinalLayer nLayer nPrimeCount =
            let nTot = (nLayer-1)*4+1 //4*nLayer-3 = 2*nSideLength-1
            let dPerc = double(nPrimeCount) / double(nTot)
            match dPerc < 0.1 with
            | true ->  nLayer
            | false -> let nNewPrimes = fnGetLayerCorners (nLayer+1) |> Seq.filter (fun x -> setPrimes.Contains(x)) |> Seq.length
                       fnGetFinalLayer (nLayer+1) (nPrimeCount + nNewPrimes)
        
        let nLayer = fnGetFinalLayer 2 3 //13121
        let nSideLength = 2*nLayer-1
        nLayer//26241

    let SolveProblem59 nIgnore = 
        let sText = Data_59.GetNumString();
        let arrNums = sText.Split(',') |> Seq.map(fun c -> System.Int32.Parse(c)) |> Seq.toArray

        let fnUseKey c1 c2 c3 = 
            let arrKeys = [c1;c2;c3] |> Seq.map(fun c -> (int)c) |> Seq.toArray
            {0..arrNums.Length-1} |> Seq.map(fun x -> arrKeys.[x%arrKeys.Length] ^^^ arrNums.[x])
                                  |> Seq.map(fun x -> char(x))
        
        let listChars = ['a'..'z']
        let seqKeys = listChars |> Seq.collect(fun c1 -> listChars |> Seq.collect(fun c2 -> listChars |> Seq.map(fun c3 -> new string([|c1;c2;c3|]), (fnUseKey c1 c2 c3))))
                                |> Seq.map(fun (a,b) -> a,new string(b |> Seq.toArray))
        
        let (sKey,sMess,nFreq) = seqKeys |> Seq.map(fun (xKey, xText) -> xKey,xText,((xText.Split(' ') |> Seq.length) + (xText.Split(' ') |> Seq.length)))
                                         |> Seq.sortBy (fun (_,_,x) -> -x)
                                         |> Seq.head

        let nSum = sMess.ToCharArray() |> Seq.map(fun x -> int(x)) |> Seq.sum
        nSum //107359

    let SolveProblem60 nIgnore = 
        
        let arrPrimes = HelperFunctions.SievePrimes(100*1000*1000) |> Seq.toArray
        let setPrimes = new Set<int>(arrPrimes);

        let arrRange = arrPrimes |> Seq.takeWhile (fun x -> x < 10000) |> Seq.toArray
        let nMax = arrRange.Length - 1;
        let seqAllPairs = {0..nMax} |> Seq.collect(fun x1 -> {x1+1..nMax} |> Seq.map(fun x2 -> arrRange.[x1],arrRange.[x2]))
        
        let fnConcat num1 num2 = 
            let nMag = int(System.Math.Log10(float(num2)))+1;
            let nScale = {1..nMag} |> Seq.fold(fun acc x -> acc*10) 1
            num1*nScale+num2
       
        let listValidPairs = seqAllPairs |> Seq.filter(fun (x1,x2) -> setPrimes.Contains(fnConcat x1 x2) && setPrimes.Contains(fnConcat x2 x1)) |> Seq.toList
        let dicPairs = listValidPairs |> Seq.groupBy (fun (a,b) -> a) |> Seq.map (fun (key,pairs) -> key,(pairs |> Seq.map(fun (a,b) -> b) |> Seq.toList)) |> dict
        
        let fnGetFromKey nKey = let listInPair = ref List.Empty;
                                if dicPairs.TryGetValue(nKey, listInPair) then listInPair.Value else []
        
        let fnToSet seqA = new Set<'a>(seqA)
        
        let rec fnGetSet(nKey:int, listPrimes:List<int>, listSets:List<Set<int>>) = 
            let listx0 = fnGetFromKey nKey;
            match listx0 with
            | [] -> seq { yield listPrimes }
            | _ ->  let listSetsx0 = (listx0 |> fnToSet) :: listSets
                    let listPrimesx0 = nKey :: listPrimes
                    let listx1 = listx0 |> Seq.filter(fun x -> listSets |> Seq.forall(fun setx -> setx.Contains(x))) |> Seq.toList
                    match listx1 with
                    | [] -> seq { yield listPrimesx0 }
                    | _ -> listx1|> Seq.collect (fun x1 -> fnGetSet(x1,listPrimesx0,listSetsx0))
        
        let (nLength,seqTop) = dicPairs.Keys |> Seq.collect (fun x -> fnGetSet(x,[],[])) |> Seq.groupBy(fun x -> x.Length) |> Seq.sortBy (fun (a,b) -> -a) |> Seq.head
        let listTop = seqTop |> Seq.sortBy (fun x -> x |> Seq.sum) |> Seq.head
        let nResult = listTop |> Seq.sum
        nResult //26033
    
    type PolyNum = { Order:int; Num:int; Beg2:int; End2:int }
    let SolveProblem61 nIgnore = 
        let fnGetP3 x = (x*(x+1))/2
        let fnGetP4 x = (x*x)
        let fnGetP5 x = (x*(3*x-1))/2
        let fnGetP6 x = x*(2*x-1)
        let fnGetP7 x = (x*(5*x-3))/2
        let fnGetP8 x = x*(3*x-2)

        let fnGetForOrder nOrder fnGetPX =
            Seq.initInfinite (fun x -> x+1) |> Seq.map fnGetPX |> Seq.filter (fun x -> x >= 1000) |> Seq.takeWhile (fun x -> x <= 9999)
            |> Seq.map(fun x -> let arrDigs = HelperFunctions.GetAllDigits(x) |> Seq.toArray
                                { Order = nOrder; Num = x; Beg2 = HelperFunctions.BuildNumber(arrDigs.[0..1]); End2 = HelperFunctions.BuildNumber(arrDigs.[2..3]) })
        
        let arrAllPXs = [fnGetP3;fnGetP4;fnGetP5;fnGetP6;fnGetP7;fnGetP8] |> List.mapi (fun x y -> (fnGetForOrder x y) |> Seq.toList) |> Seq.toArray
        
        let rec fnSpwanLoop(listInCycle:List<PolyNum>) = 
            match listInCycle.Length = 6 with
            | true -> match listInCycle.Head.End2 = (listInCycle |> Seq.last).Beg2 with 
                      | true -> [listInCycle]
                      | false -> []
            | false -> let setTaken = new Set<int>(listInCycle |> Seq.map(fun x -> x.Order))
                       let seqPossibles =  {0..5} |> Seq.filter (fun x -> setTaken.Contains(x) = false)
                                           |> Seq.collect (fun x -> arrAllPXs.[x])
                                           |> Seq.filter(fun x -> listInCycle.Head.End2 = x.Beg2)
                       seqPossibles |> Seq.collect (fun x -> fnSpwanLoop(x::listInCycle)) |> Seq.toList
        
        let listResult = arrAllPXs.[0] |> Seq.collect (fun x -> fnSpwanLoop([x])) |> Seq.head
        let nSum = listResult |> Seq.map (fun x -> x.Num) |> Seq.sum
        nSum //28684

    type CubeDigs = { Number:int64; OrderedDigs:int64 }
    let SolveProblem62 nIgnore = 
        let dicResults = new System.Collections.Generic.Dictionary<int64,List<CubeDigs>>();
        let fnGetFromKey nKey = let listCubes = ref List.Empty;
                                if dicResults.TryGetValue(nKey, listCubes) then listCubes.Value else []

        let fnGetCubeDig nNum =
            let nCube = nNum * nNum * nNum
            let arrDigs = HelperFunctions.GetAllDigitsLong(nCube) |> Seq.sortBy (fun x -> -x) |> Seq.toArray
            { Number = nCube; OrderedDigs = HelperFunctions.BuildNumberLong(arrDigs) }

        let fnBuildNum nNum = 
            let oCube = fnGetCubeDig nNum
            let listCubes = fnGetFromKey oCube.OrderedDigs
            let listAdded = oCube :: listCubes
            dicResults.[oCube.OrderedDigs] <- listAdded
            listAdded
        
        let nLast = Seq.initInfinite (fun x -> int64(x)+1L) |> Seq.takeWhile (fun x -> (fnBuildNum x).Length < 5) |> Seq.last
        let cubeResult = fnGetCubeDig (nLast+1L)
        let nResult = dicResults.[cubeResult.OrderedDigs] |> Seq.map (fun x -> x.Number) |> Seq.min
        nResult //127035954683

    let SolveProblem63 nIgnore = 
        let fnPowerMatched n = Seq.initInfinite(fun x -> x+1) |> Seq.map(fun x -> BigNum.APowB(x,n) |> Seq.length)
                                                              |> Seq.filter(fun x -> x >= n) |> Seq.takeWhile (fun x -> x = n) |> Seq.length
        let nSum = Seq.initInfinite(fun x -> x+1) |> Seq.map fnPowerMatched |> Seq.takeWhile (fun x -> x > 0) |> Seq.sum
        nSum

    let SolveProblem64 nIgnore = 
        //a0 + (1 / (a1 + 1/(a2 + 1/(a3 + 1/(a4 + ...)))))
        
        let fnGetA0 nSq = SQRT nSq
        
        //let fnGetA1 nSq a1 =
            //a0 +1/x => x = 1/(n-a0) = (a0 + n)/(nSq - a0*a0) = 


        0;

    let SolveProblem66 nIgnore = 
        //x^2 - Dy^2 = 1
        let fnIsSquare x = let nRoot = SQRT x;
                           nRoot*nRoot = x

        let fnFindXForDY d y = 
            let xSq = 1L + d*y*y
            let x = SQRT64 xSq
            if x*x = xSq then x else 0L
        
        let fnBestXForD d = Seq.initInfinite(fun a -> a+1)
                            |> Seq.map(fun y -> fnFindXForDY (int64(d)) (int64(y)))
                            |> Seq.filter(fun x -> x > 0L)
                            |> Seq.head
        
        let (dMax,xMax) = {2..1000} |> Seq.filter(fun d -> fnIsSquare d = false)
                                    |> Seq.map(fun d -> d,(fnBestXForD d))
                                    |> Seq.sortBy(fun (d,x) -> -x)
                                    |> Seq.head
        dMax; //???

    let SolveProblem69 nIgnore = 
        
//        let fnGetFactors n =
//            let seqFactors = {1..SQRT(n)} |> Seq.filter(fun x -> n%x=0)
//            seq { yield! seqFactors; yield n }
//
//        let a = {1..1000*1000} |> Seq.map(fun x -> fnGetFactors x |> Seq.toList) |> Seq.toList

        let nMax = 1000*1000;
        let setPrimes = new Set<int>(HelperFunctions.BuildLoggedPrimes(nMax))
        let arrFactors = {1..nMax} |> Seq.map(fun x -> [x..x..nMax]) |> Seq.toList
        
        let fnGetRelativePrimes n = 
            
            0

        0

    let SolveProblem75 nIgnore =
        let rec gcd big small =
            match small with
            | 0 -> big
            | _ -> gcd small (big % small)
        
        let perimLim = 1500000;
        // a = k(m*m-n*n), b = k*2*m*n, c = k(m*m+n*n), m>n and parity(m) != parity(n) // a+b+c = L => L = 2k(m*m + m*n) = 2km(m+n)
        let mLim = SQRT (perimLim/2)
        let nOneRATs = {2..mLim} |> Seq.collect(fun m -> {(m-1).. -2 ..1} |> Seq.filter(fun n -> gcd m n = 1)
                                                                          |> Seq.map(fun n -> 2*m*(m+n)))
                                 |> Seq.collect(fun perim -> {1..perimLim/perim} |> Seq.map(fun x -> x*perim))
                                 |> Seq.groupBy(fun x -> x)
                                 |> Seq.filter(fun (nK,seqP) -> seqP |> Seq.length = 1)
                                 |> Seq.length
        nOneRATs;