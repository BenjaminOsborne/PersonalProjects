namespace Scrabble.Domain

type LetterHelpers = 
    static member CharsToString chars (len:int) =
            let builder = new System.Text.StringBuilder(len)
            chars |> Seq.iter (fun c -> builder.Append(c.ToString()) |> ignore)
            builder.ToString()
    static member CharListToString (chars: char list) = LetterHelpers.CharsToString chars chars.Length

type TileHand(tiles : BagTile list) =
    
    let orderTiles = Lazy.Create (fun _ -> tiles |> Seq.sortBy (fun x -> x.TileLetter, -x.Value) |> Seq.toList)
    let letters = Lazy.Create (fun _ -> tiles |> Seq.map (fun x -> x.TileLetter) |> Seq.toList)
    
    member this.Length = tiles.Length
    member this.Letters = letters.Value
    member this.PopNextTileFor tileLetter =
        let (tile, remaining) = orderTiles.Value |> List.removeFirstWith (fun x -> x.TileLetter = tileLetter)
        (tile, new TileHand(remaining))

type BagTileLetterMap = { BagTileLetter : BagTileLetter; Letter : char; WordLetterIndex : int }

type WordWalkResult = { Word : string; UsedTiles : BagTileLetterMap list }

type WordNode = | Leaf of string
                | Branch of WordBranch

and WordBranch (length:int, words: (string * string) list) =
    
    let tryWalkNode (tree: Map<char, Lazy<WordNode>>) c usedTiles walkIfBranch = 
        match tree.TryFind c with
        | Some(node) -> match node.Value with
                        | Leaf(word) -> Seq.single { Word = word; UsedTiles = usedTiles }
                        | Branch(branch) -> walkIfBranch branch
        | None -> Seq.empty

    let rec walkWith (tiles: BagTileLetter list) (pinned : Map<int,char>) (currentIndex : int) (tree: Map<char, Lazy<WordNode>>) (usedTiles: BagTileLetterMap list) =
        
        let walkWithNexChars nextTiles used (branch:WordBranch) =
            walkWith nextTiles pinned (currentIndex+1) branch.treeMember used
        
        let walkForCharAtIndex chr tile index =
            let used = { BagTileLetter = tile; Letter = chr; WordLetterIndex = currentIndex } :: usedTiles
            tryWalkNode tree chr used (fun branch -> let nextTiles = tiles |> List.removeIndex index
                                                     walkWithNexChars nextTiles used branch)

        match pinned.TryFind currentIndex with
        | Some(c) -> tryWalkNode tree c usedTiles (fun branch -> walkWithNexChars tiles usedTiles branch)
        | None ->    tiles |> Seq.mapi (fun nx t -> match t with
                                                    | Letter(c) -> walkForCharAtIndex c t nx
                                                    | Blank -> ['a'..'z'] |> Seq.map (fun c -> walkForCharAtIndex c t nx)
                                                                          |> Seq.collect (fun x -> x))
                           |> Seq.collect (fun x -> x)
        
    let makeNode (grp: seq<string*string>) =
        match length with 
        | 1 -> let word = grp |> Seq.map (fun (_, w) -> w) |> Seq.head
               Leaf(word)
        | _ -> let nextStrings = grp |> Seq.map (fun (s,w) -> (s.Substring(1, s.Length-1), w)) |> Seq.toList
               Branch(new WordBranch(length-1, nextStrings))

    let treeField = words |> Seq.groupBy (fun (s,w) -> s.[0])
                          |> Seq.map (fun (c, grp) -> c, Lazy.Create(fun _ -> makeNode grp))
                          |> Map
    
    member private this.treeMember = treeField

    member this.WalkWith (chars: BagTileLetter list) (pinned : Map<int,char>) =
        (walkWith chars pinned 0 this.treeMember []) |> Seq.distinct

type WordSet(words : Set<string>) = 
    
    let wordsByLength = words |> Seq.groupBy (fun x -> x.Length)
                              |> Seq.map(fun (key, items) -> key, Lazy.Create(fun _ -> new WordBranch(key, items |> Seq.map (fun s -> (s, s)) |> Seq.toList)))
                              |> Map

    member this.Count = words.Count

    member this.AllWords = words

    member this.WordsForLengthWithLetters wordLength (letters: BagTileLetter list) (pinned: Map<int,char>) =
        match wordsByLength.TryFind wordLength with
        | Some(wb) -> wb.Value.WalkWith letters pinned
        | None -> Seq.empty

    member this.IsWord word = words.Contains word
    
    