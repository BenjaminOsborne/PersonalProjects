module EqualitySet

type EqualitySet<'T when 'T : equality>(items : 'T list) = 
    member this.Items = items
    
    override this.Equals obj =
        match obj with
        | :? EqualitySet<'T> as other -> items |> List.sequenceEqual other.Items
        | _ -> false

    override this.GetHashCode() = items |> Seq.fold (fun agg x -> (agg * 397) ^^^ x.GetHashCode()) 0