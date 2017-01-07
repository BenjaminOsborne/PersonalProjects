module Seq

let distinctByField (fieldSelect: 'T -> 'U) (items: seq<'T>) =
    let set = new System.Collections.Generic.HashSet<'U>()
    items |> Seq.filter (fun x -> set.Add (fieldSelect x))

let removeFirstWith (select : 'T -> bool) (items : 'T list) =
    let index = items |> Seq.findIndex select
    let remaining = {0 .. items.Length-1} |> Seq.filter (fun n -> n <> index)
                                          |> Seq.map (fun n -> items.[n]) |> Seq.toList
    (items.[index], remaining)

let someValues (items : seq<Option<'T>>) =
    items |> Seq.filter (fun x -> x.IsSome)
          |> Seq.map (fun x -> x.Value)
