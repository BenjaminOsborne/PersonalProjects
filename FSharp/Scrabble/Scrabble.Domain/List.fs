module List

let removeIndex (index : int) (items : 'T list) =
    {0 .. items.Length-1} |> Seq.filter (fun n -> n <> index)
                          |> Seq.map (fun n -> items.[n]) |> Seq.toList

let removeFirstWith (select : 'T -> bool) (items : 'T list) =
    let index = items |> Seq.findIndex select
    let remaining = removeIndex index items
    (items.[index], remaining)

let sequenceEqual (list1 : 'T list) (list2 : 'T list) =
    let rec matchDeep (x : 'T list) (y : 'T list) =
        match x.Length with
        | 0 -> y.Length = 0
        | _ -> x.Head.Equals(y.Head) && matchDeep x.Tail y.Tail
    
    list1.Length = list2.Length && matchDeep list1 list2
