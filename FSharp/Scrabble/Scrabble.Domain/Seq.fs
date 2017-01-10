module Seq

let distinctByField (fieldSelect: 'T -> 'U) (items: seq<'T>) =
    let set = new System.Collections.Generic.HashSet<'U>()
    items |> Seq.filter (fun x -> set.Add (fieldSelect x))

let someValues (items : seq<Option<'T>>) =
    items |> Seq.filter (fun x -> x.IsSome)
          |> Seq.map (fun x -> x.Value)

let takeWhileAndNext (predicate: 'T -> bool) (items: seq<'T>) =
    let mutable shouldContinue = true
    items |> Seq.takeWhile (fun x -> match shouldContinue with
                                     | true -> shouldContinue <- predicate x
                                               true
                                     | false -> false)

let single item = [item] :> seq<'a>
