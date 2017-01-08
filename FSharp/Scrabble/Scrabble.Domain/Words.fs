namespace Scrabble.Domain

type LetterHelpers = 
    static member CharsToString chars (len:int) =
            let builder = new System.Text.StringBuilder(len)
            chars |> Seq.iter (fun c -> builder.Append(c.ToString()) |> ignore)
            builder.ToString()
    static member CharListToString (chars: char list) = LetterHelpers.CharsToString chars chars.Length

type TileHand(tiles : Tile list) =
    
    let orderTiles = Lazy.Create (fun _ -> tiles |> Seq.sortBy (fun x -> x.Letter, -x.Value) |> Seq.toList)
    let letters = Lazy.Create (fun _ -> tiles |> Seq.map (fun x -> x.Letter) |> Seq.toList)
    
    member this.Tiles = tiles
    member this.Letters = letters.Value
    member this.PopNextTileFor c =
        let (tile, remaining) = orderTiles.Value |> List.removeFirstWith (fun x -> x.Letter = c)
        (tile, new TileHand(remaining))

type WordNode = | Leaf of string
                | Branch of WordBranch

and WordBranch (length:int, words: (string * string) list) =
    
    let doSome (tree: Map<char, Lazy<WordNode>>) c walkBranch = 
        match tree.TryFind c with
        | Some(node) -> match node.Value with
                        | Leaf(word) -> Seq.single word
                        | Branch(branch) -> walkBranch branch
        | None -> Seq.empty

    let rec walkWith (chrs: char list) (pinned : Map<int,char>) (currentIndex : int) (tree: Map<char, Lazy<WordNode>>) =
        match pinned.TryFind currentIndex with
        | Some(c) -> doSome tree c (fun branch -> walkWith chrs pinned (currentIndex+1) branch.memberTree)
        | None ->    chrs |> Seq.mapi (fun nx c -> doSome tree c (fun branch -> let nextChrs = chrs |> List.removeIndex nx
                                                                                walkWith nextChrs pinned (currentIndex+1) branch.memberTree))
                          |> Seq.collect (fun x -> x)
        
    let makeNode (grp: seq<string*string>) =
        match length with 
        | 1 -> let word = grp |> Seq.map (fun (_,w) -> w) |> Seq.head
               Leaf(word)
        | _ -> let nextStrings = grp |> Seq.map (fun (s,w) -> (s.Substring(1, s.Length-1),w)) |> Seq.toList
               Branch(new WordBranch(length-1, nextStrings))

    let treeField = words |> Seq.groupBy (fun (s,w) -> s.[0])
                          |> Seq.map (fun (c, grp) -> c, Lazy.Create(fun _ -> makeNode grp))
                          |> Map
    
    member private this.memberTree = treeField

    member this.Length = length

    member this.WalkWith (chars: char list) (pinned : Map<int,char>) =
        (walkWith chars pinned 0 this.memberTree) |> Seq.distinct |> Seq.toList

type WordSet(words : Set<string>) = 
    
    let wordsByLength = words |> Seq.groupBy (fun x -> x.Length)
                              |> Seq.map(fun (key, items) -> key, Lazy.Create(fun _ -> new WordBranch(key, items |> Seq.map (fun s -> (s, s)) |> Seq.toList)))
                              |> Map

    member this.Count = words.Count

    member this.AllWords = words

    member this.WordsForLengthWithLetters wordLength (letters: char list) (pinned: Map<int,char>) =
        match wordsByLength.TryFind wordLength with
        | Some(wb) -> wb.Value.WalkWith letters pinned
        | None -> []

    member this.IsWord word = words.Contains word
    
    