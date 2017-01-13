module List

let removeIndex (index : int) (items : 'T list) =
    let rec skipIfIndex list currentIndex =
        match list with
        | head::tail -> if currentIndex = index then tail
                        else head :: (skipIfIndex tail (currentIndex+1))
        | [] -> []
    let final = skipIfIndex items 0
    final

let removeFirstWith (select : 'T -> bool) (items : 'T list) =
    let index = items |> Seq.findIndex select
    let remaining = removeIndex index items
    (items.[index], remaining)

let index n (items : 'T list) =
    if n < 0 then
        failwith "negative index"
    else
        let remain = [1 .. n] |> List.fold (fun (agg : 'T list) _ -> agg.Tail) items
        remain.Head

let sequenceEqual (list1 : 'T list) (list2 : 'T list) =
    let rec matchDeep (x : 'T list) (y : 'T list) =
        match x.Length with
        | 0 -> y.Length = 0
        | _ -> x.Head.Equals(y.Head) && matchDeep x.Tail y.Tail
    
    list1.Length = list2.Length && matchDeep list1 list2
