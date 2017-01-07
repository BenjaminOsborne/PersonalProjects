module List

let sequenceEqual (list1 : 'T list) (list2 : 'T list) =
    let rec matchDeep (x : 'T list) (y : 'T list) =
        match x.Length with
        | 0 -> y.Length = 0
        | _ -> x.Head.Equals(y.Head) && matchDeep x.Tail y.Tail
    
    list1.Length = list2.Length && matchDeep list1 list2
