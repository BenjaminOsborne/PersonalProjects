module Seq

let distinctByField (fieldSelect: 'T -> 'U) (items: seq<'T>) =
    let set = new System.Collections.Generic.HashSet<'U>()
    items |> Seq.filter (fun x -> set.Add (fieldSelect x))

let someValues (items : seq<Option<'T>>) =
    items |> Seq.filter (fun x -> x.IsSome)
          |> Seq.map (fun x -> x.Value)
