namespace Scrabble.Domain

type LetterHelpers = 
    static member CharsToString chars (len:int) =
            let builder = new System.Text.StringBuilder(len)
            chars |> Seq.iter (fun c -> builder.Append(c.ToString()) |> ignore)
            builder.ToString()
    static member CharListToString (chars: char list) = LetterHelpers.CharsToString chars chars.Length

type LetterSet(letters : string) = 
    
    let letterSet = letters |> Seq.groupBy (fun x -> x) |> Seq.map (fun (key, items) -> key, items |> Seq.length) |> Map

    static member FromTiles (tiles : Tile list) =
        new LetterSet(LetterHelpers.CharsToString (tiles |> Seq.map (fun x -> x.Letter)) tiles.Length)
    
    member this.LetterSet = letterSet
    member this.InputLetters = letters
    member this.ContainsAtLeastAllFrom (other:LetterSet) = 
        other.LetterSet |> Seq.forall (fun kvp -> let someVal = this.LetterSet.TryFind kvp.Key
                                                  match someVal with
                                                  | Some(count) -> count >= kvp.Value
                                                  | _ -> false)

type TileHand(tiles : Tile list) =
    
    let mapTiles = tiles |> Seq.groupBy (fun x -> x.Letter) |> Seq.map (fun (key,vals) -> (key, vals |> Seq.toList)) |> Map
    let letters = LetterSet.FromTiles tiles
    
    member this.Tiles = tiles
    member this.LetterSet = letters
    member this.GetTiles c = match mapTiles.TryFind c with | Some(lst) -> lst | None -> []

type Word(word : string) =
    let thisSet = LetterSet(word)
    member this.Word = word
    member this.CanMakeWordFromSet(letters : LetterSet) = letters.ContainsAtLeastAllFrom thisSet

type WordSet(words : Set<string>) = 
    let wordsByLength = words |> Seq.groupBy (fun x -> x.Length)
                              |> Seq.map(fun (key, items) -> key, items |> Seq.map (fun x -> Word(x)) |> Seq.toList)
                              |> Map
    
    member this.WordsForLength len = let somelist = wordsByLength.TryFind len
                                     match somelist with | Some(l) -> l | _ -> []
    
    member this.IsWord word = words.Contains word
    
