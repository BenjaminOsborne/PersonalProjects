namespace Scrabble.Domain

type LetterHelpers = 
    static member CharsToString chars (len:int) =
            let builder = new System.Text.StringBuilder(len)
            chars |> Seq.iter (fun c -> builder.Append(c.ToString()) |> ignore)
            builder.ToString()
    static member CharListToString (chars: char list) = LetterHelpers.CharsToString chars chars.Length

type LetterSet private (letterSet : Map<char,int>) = 
    
    static member private toLetterSet (letters:seq<char>)  = letters |> Seq.groupBy (fun x -> x) |> Seq.map (fun (key, items) -> key, items |> Seq.length) |> Map

    static member FromTiles (tiles : Tile list) = new LetterSet(LetterSet.toLetterSet (tiles |> Seq.map (fun x -> x.Letter)))
    
    static member FromLetters (letters : string) = new LetterSet(LetterSet.toLetterSet letters)
    
    member this.WithNewLetters (letters : seq<char>) =
        let finalMap = letters |> Seq.fold (fun (agg:Map<char,int>) c -> let existing = agg.TryFind c
                                                                         match existing with
                                                                         | None -> agg.Add (c,1)
                                                                         | Some(count) -> agg.Add (c, count+1)) letterSet
        new LetterSet(finalMap)

    member private this.LetterSet = letterSet
    
    member this.ContainsAtLeastAllFrom (other:LetterSet) = 
        other.LetterSet |> Seq.forall (fun kvp -> let someVal = this.LetterSet.TryFind kvp.Key
                                                  match someVal with
                                                  | Some(count) -> count >= kvp.Value
                                                  | _ -> false)

type TileHand(tiles : Tile list) =
    
    let orderTiles = Lazy.Create (fun _ -> tiles |> Seq.sortBy (fun x -> x.Letter, -x.Value) |> Seq.toList)
    let letters = Lazy.Create (fun _ -> LetterSet.FromTiles tiles)
    
    member this.Tiles = tiles
    member this.LetterSet = letters.Value
    member this.PopNextTileFor c =
        let (tile, remaining) = orderTiles.Value |> Seq.removeFirstWith (fun x -> x.Letter = c)
        (tile, new TileHand(remaining))

type Word(word : string) =
    let thisSet = Lazy.Create(fun _ -> LetterSet.FromLetters(word))
    member this.Word = word
    member this.CanMakeWordFromSet (letters : LetterSet) =
        letters.ContainsAtLeastAllFrom thisSet.Value

    interface System.IComparable with
        member this.CompareTo other = word.CompareTo (other :?> Word).Word

type WordSetAtLength = { Length : int; Words : Map<string, Word>; IndexMap : Lazy<Map<char, Set<Word>>> list }

type WordSet(words : Set<string>) = 
    
    let groupByFirstLetter (items:seq<string>) =
        items |> Seq.groupBy (fun x -> x.[0]) |> Seq.map (fun (key, items) -> key, items |> Seq.map (fun x -> Word(x)) |> Seq.toList) |> Map
    
    let groupByLetterIndex length (items:seq<string>) =
        let mapWords = items |> Seq.map (fun x -> x, new Word(x)) |> Map
        let indexMap = [0 .. length-1] |> List.map (fun nx -> Lazy.Create(fun _ -> mapWords |> Seq.groupBy (fun kvp -> kvp.Key.[nx])
                                                                                            |> Seq.map (fun (k, items) -> k, items |> Seq.map (fun i -> i.Value) |> Set)
                                                                                            |> Map))
        { Length = length; Words = mapWords; IndexMap = indexMap }
        
    let wordsByLength = Lazy.Create (fun _ -> words |> Seq.groupBy (fun x -> x.Length)
                                                    |> Seq.map(fun (key, items) -> key, Lazy.Create (fun _ -> groupByLetterIndex key items))
                                                    |> Map)
    
    let mapForLength wordLength selectValue =
        let someMap = wordsByLength.Value.TryFind wordLength
        match someMap with
        | Some(map) -> selectValue map.Value
        | _ -> Seq.empty

    let wordsForLengthWithPinned wordLength (pinnedLetters: (int*char) list) =
        let sets = mapForLength wordLength (fun map -> pinnedLetters |> Seq.map (fun (nx,c) -> let someSet = map.IndexMap.[nx].Value.TryFind c
                                                                                               match someSet with
                                                                                               | Some(l) -> l
                                                                                               | _ -> Set.empty))
        sets

    member this.Count = words.Count

    member this.WordsForLength wordLength =
        mapForLength wordLength (fun map -> map.Words |> Seq.map (fun x -> x.Value))
    
    member this.WordsForLengthWithPinned wordLength (pinnedLetters: (int*char) list) =
        let orderedSets = wordsForLengthWithPinned wordLength pinnedLetters
                          |> Seq.sortBy (fun x -> x.Count) |> Seq.toList
        match orderedSets with
        | head::[] -> head :> seq<Word>
        | head::tail -> head |> Seq.filter (fun h -> tail |> Seq.forall(fun t -> t.Contains(h)))
        | [] -> Seq.empty

    member this.WordsForLengthWithStarting wordLength (startLetters: seq<char>) =
        let pinnedLetters = startLetters |> Seq.map (fun c -> (0,c)) |> Seq.toList
        let sets = wordsForLengthWithPinned wordLength pinnedLetters
        sets |> Seq.collect (fun x -> x)

    member this.IsWord word = words.Contains word
    
