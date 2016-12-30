module Seq

let distinctByField (fieldSelect: 'T -> 'U) (items: seq<'T>) =
    let set = new System.Collections.Generic.HashSet<'U>()
    items |> Seq.filter (fun x -> set.Add (fieldSelect x))

let removeFirstWith (select : 'T -> bool) (items : 'T list) =
    let index = items |> Seq.findIndex select
    let remaining = {0 .. items.Length-1} |> Seq.filter (fun n -> n <> index)
                                          |> Seq.map (fun n -> items.[n]) |> Seq.toList
    (items.[index], remaining)

type EqualitySet<'T when 'T : equality>(items : 'T list) = 
    member this.Items = items
    
    override this.Equals(obj) =
        let rec matchDeep(x: 'T list, y : 'T list) =
            match x.Length with
            | 0 -> y.Length = 0
            | _ -> x.Head.Equals(y.Head) && matchDeep(x.Tail, y.Tail)
        match obj with
        | :? EqualitySet<'T> as other ->
            items.Length = other.Items.Length && matchDeep(items, other.Items)
        | _ -> false

    override this.GetHashCode() = items |> Seq.fold (fun agg x -> (agg * 397) ^^^ x.GetHashCode()) 0