module ShortExamples

    let tupleTest () = 
        let stringTuple = ( "one", "two", "three" )
        let fst3 (a,_,__) = a
        let oneFrom3 = fst3 stringTuple
        System.Console.WriteLine(oneFrom3)
        0