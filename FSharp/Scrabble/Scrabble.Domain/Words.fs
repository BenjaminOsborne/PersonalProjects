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
    let thisSet = LetterSet.FromLetters(word)
    member this.Word = word
    member this.CanMakeWordFromSet (letters : LetterSet) =
        letters.ContainsAtLeastAllFrom thisSet

type WordSet(words : Set<string>) = 
    
    let groupByFirstLetter (items:seq<string>) =
        items |> Seq.groupBy (fun x -> x.[0]) |> Seq.map (fun (key, items) -> key, items |> Seq.map (fun x -> Word(x)) |> Seq.toList) |> Map

    let wordsByLength = Lazy.Create (fun _ -> words |> Seq.groupBy (fun x -> x.Length)
                                                    |> Seq.map(fun (key, items) -> key, Lazy.Create (fun _ -> groupByFirstLetter items))
                                                    |> Map)
    
    let mapForLength wordLength selectValue =
        let someMap = wordsByLength.Value.TryFind wordLength
        match someMap with
        | Some(map) -> selectValue map
        | _ -> Seq.empty

    member this.Count = words.Count

    member this.WordsForLength wordLength =
        mapForLength wordLength (fun map -> map.Value |> Seq.map (fun x -> x.Value)
                                                      |> Seq.collect (fun x -> x))
    
    member this.WordsForLengthWithStart (wordLength:int) (startLetters: seq<char>) =
        mapForLength wordLength (fun map -> startLetters |> Seq.map (fun c -> let someList = map.Value.TryFind c
                                                                              match someList with
                                                                              | Some(l) -> l
                                                                              | _ -> [])
                                                         |> Seq.collect (fun x -> x))

    member this.IsWord word = words.Contains word
    
