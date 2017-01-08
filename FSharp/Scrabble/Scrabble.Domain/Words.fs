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
    
    member this.Letters = letterSet |> Seq.map (fun x -> [1..x.Value] |> Seq.map (fun _ -> x.Key))
                                    |> Seq.collect (fun x -> x)

type TileHand(tiles : Tile list) =
    
    let orderTiles = Lazy.Create (fun _ -> tiles |> Seq.sortBy (fun x -> x.Letter, -x.Value) |> Seq.toList)
    let letters = Lazy.Create (fun _ -> LetterSet.FromTiles tiles)
    
    member this.Tiles = tiles
    member this.LetterSet = letters.Value
    member this.PopNextTileFor c =
        let (tile, remaining) = orderTiles.Value |> List.removeFirstWith (fun x -> x.Letter = c)
        (tile, new TileHand(remaining))

type Word(word : string) =
    let thisSet = Lazy.Create(fun _ -> LetterSet.FromLetters(word))
    member this.Word = word
    member this.CanMakeWordFromSet (letters : LetterSet) =
        letters.ContainsAtLeastAllFrom thisSet.Value

    interface System.IComparable with
        member this.CompareTo other = word.CompareTo (other :?> Word).Word

type WordSetAtLength = { Length : int; Words : Map<string, Word>; IndexMap : Map<char, Set<Word>> list }

type WordSetOld(words : Set<string>) = 
    
    let groupByFirstLetter (items:seq<string>) =
        items |> Seq.groupBy (fun x -> x.[0]) |> Seq.map (fun (key, items) -> key, items |> Seq.map (fun x -> Word(x)) |> Seq.toList) |> Map
    
    let groupByLetterIndex length (items:seq<string>) =
        let mapWords = items |> Seq.map (fun x -> x, new Word(x)) |> Map
        let indexMap = [0 .. length-1] |> List.map (fun nx -> mapWords |> Seq.groupBy (fun kvp -> kvp.Key.[nx])
                                                                       |> Seq.map (fun (k, items) -> k, items |> Seq.map (fun i -> i.Value) |> Set)
                                                                       |> Map)
        { Length = length; Words = mapWords; IndexMap = indexMap }
        
    let wordsByLength = words |> Seq.groupBy (fun x -> x.Length)
                              |> Seq.map(fun (key, items) -> key, groupByLetterIndex key items)
                              |> Map
    
    let mapForLength wordLength selectValue =
        let someMap = wordsByLength.TryFind wordLength
        match someMap with
        | Some(map) -> selectValue map
        | _ -> Seq.empty

    let wordsForLengthWithPinned wordLength (pinnedLetters: (int*char) list) =
        let sets = mapForLength wordLength (fun map -> pinnedLetters |> Seq.map (fun (nx,c) -> let someSet = map.IndexMap.[nx].TryFind c
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

type WordNode = | Leaf of Word
                | Branch of WordBranch

and WordBranch (length:int, words: (string * Word) list) =
    
    let doSome (tree: Map<char, WordNode>) c walkBranch = 
        match tree.TryFind c with
        | Some(node) -> match node with
                        | Leaf(word) -> Seq.single word
                        | Branch(branch) -> walkBranch branch
        | None -> Seq.empty

    let rec walkWith (chrs: char list) (pinned : Map<int,char>) (currentIndex : int) (tree: Map<char, WordNode>) =
        match pinned.TryFind currentIndex with
        | Some(c) -> doSome tree c (fun branch -> walkWith chrs pinned (currentIndex+1) branch.memberTree)
        | None ->    chrs |> Seq.mapi (fun nx c -> doSome tree c (fun branch -> let nextChrs = chrs |> List.removeIndex nx
                                                                                walkWith nextChrs pinned (currentIndex+1) branch.memberTree))
                          |> Seq.collect (fun x -> x)
        
    
    let treeField = words |> Seq.groupBy (fun (s,w) -> s.[0])
                          |> Seq.map (fun (c, grp) -> c, match length with 
                                                         | 1 -> let word = grp |> Seq.map (fun (_,w) -> w) |> Seq.head
                                                                Leaf(word)
                                                         | _ -> let nextStrings = grp |> Seq.map (fun (s,w) -> (s.Substring(1, s.Length-1),w)) |> Seq.toList
                                                                Branch(new WordBranch(length-1, nextStrings)))
                          |> Map
    
    member private this.memberTree = treeField

    member this.Length = length

    member this.WalkWith (chars: char list) (pinned : Map<int,char>) =
        (walkWith chars pinned 0 this.memberTree) |> Seq.distinct |> Seq.toList

type WordSet(words : Set<string>) = 
    
    let wordsByLength = words |> Seq.groupBy (fun x -> x.Length)
                              |> Seq.map(fun (key, items) -> key, new WordBranch(key, items |> Seq.map (fun s -> (s, new Word(s))) |> Seq.toList))
                              |> Map

    member this.Count = words.Count

    member this.WordsForLengthWithLetters wordLength (letters: char list) (pinned: Map<int,char>) =
        match wordsByLength.TryFind wordLength with
        | Some(wb) -> wb.WalkWith letters pinned
        | None -> []

    member this.IsWord word = words.Contains word

    member this.WordsForLength wordLength =
        List.empty<Word>
    
    member this.WordsForLengthWithPinned wordLength (pinnedLetters: (int*char) list) =
        List.empty<Word>

    member this.WordsForLengthWithStarting wordLength (startLetters: seq<char>) =
        List.empty<Word>
    
    