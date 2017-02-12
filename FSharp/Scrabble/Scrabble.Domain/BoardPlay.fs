namespace Scrabble.Domain

type BoardPlay(locations : BoardLocation list, direction : Direction) =
    let lazySpaces = Lazy.Create (fun _ -> locations |> List.filter (fun x -> x.State.IsSpace))
    member this.Locations = locations
    member this.Spaces = lazySpaces.Value
    member this.AnySpaces = this.Spaces.Length > 0
    member this.Direction = direction

    member this.StartLocation = 
        match locations.Length with
        | 0 -> { Width = -1; Height = -1 }
        | _ -> locations.Head.Location

    override this.ToString() =
        match locations.Length with
        | 0 -> "<EMPTY>"
        | 1 -> locations.Head.Location.ToString()
        | _ -> let last = locations |> Seq.last
               locations.Head.Location.ToString() + " - " + last.Location.ToString()

[<System.Diagnostics.DebuggerDisplayAttribute("{Word.Word} - {Score}")>]
type WordScore = { Word : WordWalkResult; Locations: TilePlay list; Score : int }
type ValidWordPlays = { BoardPlay : BoardPlay; WordScores : WordScore list }

type ScoreData =
    { MainScore : int; MainScoreMultiplier : int; SideScores : int; RemainingTileHand : TileHand;  Locations: TilePlay list }

    static member Create ms msm ss th lcs = { MainScore = ms; MainScoreMultiplier = msm; SideScores = ss; RemainingTileHand = th; Locations = lcs; }
    
    static member Initial th = ScoreData.Create 0 1 0 th []
    
    member this.CalcScore = this.MainScore * this.MainScoreMultiplier + this.SideScores
    
    member this.WithNextLocationScore location getItem =
        let getSideScore (tiles : Tile list) tileValue letterMult wordMult =
            match tiles.Length with
            | 0 | 1 -> 0
            | _ -> let incomingScore = tiles |> Seq.sumBy (fun x -> x.Value)
                   (incomingScore + (tileValue * letterMult)) * wordMult
        
        let (ms,msm,ss,th,lcs) = match location.State with
                                 | Played(t) -> (t.Value, 1, 0, this.RemainingTileHand, [])
                                 | Free(bs) ->  let (btl,chr,tiles) = getItem (location.Location.Width, location.Location.Height)
                                                let (t,th) = this.RemainingTileHand.PopNextTileFor btl
                                                let (lm,wm) = (bs.GetLetterMultiply, bs.GetWordMultiply)
                                                let sideScore = getSideScore tiles t.Value lm wm
                                                let tile = { Letter = chr; Value = t.Value }
                                                (t.Value * lm, wm, sideScore, th, [{ Location = location.Location; Piece = tile }])
        ScoreData.Create (this.MainScore + ms) (this.MainScoreMultiplier * msm) (this.SideScores + ss) th (List.append this.Locations lcs)

[<CustomEqualityAttribute>]
[<NoComparisonAttribute>]
type BoardPlayTileData =
    { TotalLength : int; PinnedIndexLetters : Map<int, char> }
    
    static member private mapsEqual (map1 : Map<int, char>) (map2 : Map<int, char>) =
        map1.Count = map2.Count && map1 |> Seq.forall (fun kvp -> match map2.TryFind kvp.Key with 
                                                                  | Some(v) -> kvp.Value = v
                                                                  | None -> false)

    member this.FreeSpaces = this.TotalLength - this.PinnedIndexLetters.Count

    override this.Equals obj =
        match obj with
        | :? BoardPlayTileData as other -> this.TotalLength = other.TotalLength &&
                                           BoardPlayTileData.mapsEqual this.PinnedIndexLetters other.PinnedIndexLetters
        | _ -> false

    override this.GetHashCode() = this.PinnedIndexLetters |> Seq.fold (fun agg x -> (agg * 397) ^^^ x.GetHashCode()) (this.TotalLength.GetHashCode())
    
    static member FromPlay (play:BoardPlay) =
        let pinnedIndexLetters = play.Locations |> Seq.mapi (fun nx l -> match l.State with
                                                                         | Free(_) -> None
                                                                         | Played(t) -> Some(nx, t.Letter))
                                                |> Seq.someValues
                                                |> Map
        { TotalLength = play.Locations.Length; PinnedIndexLetters = pinnedIndexLetters}

type BoardSpaceAnalyser() =
    
    let isOutOfRange (board:Board) w h =
        let width = board.Width
        let height = board.Height
        w < 0 || w >= width || h < 0 || h >= height

    let hasSpaceOrEdge (board:Board) w h =
        isOutOfRange board w h || (board.TileAt w h).State.IsSpace
    
    let getTile (board:Board) location =
        let w = location.Width
        let h = location.Height
        match isOutOfRange board w h with
        | true -> None
        | false -> (board.TileAt w h).State.Tile

    member this.GenerateSpaces (board:Board) =        

        let allSpaces direction outer inner hasSpaceOrEdge tileAt =
            let manySeqs = {0 .. outer-1} |> Seq.map (fun o ->
                {0 .. inner-1} |> Seq.filter (fun inS -> hasSpaceOrEdge o (inS-1))
                               |> Seq.map (fun inS ->
                    {inS .. inner-1} |> Seq.filter (fun inE -> hasSpaceOrEdge o (inE+1))
                                     |> Seq.map (fun inE ->
                        let spaceList = {inS .. inE} |> Seq.map(fun i -> tileAt o i) |> Seq.toList
                        BoardPlay(spaceList, direction))))
            manySeqs |> Seq.collect (fun x -> x) |> Seq.collect (fun x -> x)
        
        let hasSpaceOrEdge w h = hasSpaceOrEdge board w h
        
        let horiSpaces = allSpaces Across board.Height board.Width (fun h w -> hasSpaceOrEdge w h) (fun h w -> board.TileAt w h)
        let vertSpaces = allSpaces Down board.Width board.Height (fun w h -> hasSpaceOrEdge w h) (fun w h -> board.TileAt w h)
        let allSpaces = (Seq.append horiSpaces vertSpaces)
        
        let isTouchTile loc = board.IsTouchingTile loc.Width loc.Height || board.IsMiddleTile loc.Width loc.Height
        let allValid = allSpaces |> Seq.filter (fun x -> x.AnySpaces && x.Locations |> Seq.exists (fun t -> isTouchTile t.Location)) //Must be at least 1 space and touching at least 1 existing tile
        
        let distinct = allValid |> Seq.distinctByField (fun x -> EqualitySet.EqualitySet x.Spaces) |> Seq.toList
        distinct
    
    member this.GetPossibleScoredPlays (board:Board) (tileHand:TileHand) (wordSet: WordSet) =
        
        let getPossibleWords (play:BoardPlayTileData) =
            let wordsLen = wordSet.WordsForLengthWithLetters play.TotalLength tileHand.Letters play.PinnedIndexLetters
            wordsLen

        let getWordValidData (location:Location) (letter:char) (direction:Direction) =
            let walkWhileTiles init getLocation =
                Seq.initInfinite init
                |> Seq.map (fun x -> getTile board (getLocation x))
                |> Seq.takeWhile (fun x -> x.IsSome)
                |> Seq.map (fun x -> x.Value)
                |> Seq.toList
            
            let getTiles start getLocation = 
                let forward = walkWhileTiles (fun x -> start + x + 1) getLocation
                let backward = walkWhileTiles (fun x -> start - x - 1) getLocation
                let fakeTile = { Letter = letter; Value = 0}
                List.append (backward |> List.rev) (fakeTile::forward) 

            let tiles = match direction with
                        | Across -> getTiles location.Width (fun x -> { Width = x; Height = location.Height })
                        | Down   -> getTiles location.Height (fun x -> { Width = location.Width; Height = x })
            let isValid = match tiles.Length with
                          | 1 -> true
                          | _ -> let chars = tiles |> Seq.map (fun x -> x.Letter) |> Seq.toList
                                 let word = LetterHelpers.CharListToString chars
                                 wordSet.IsWord word
            (isValid, (location, letter, tiles))

        let getSecondaryWordsValidScore (word:string) (play:BoardPlay) =
            play.Locations |> Seq.mapi (fun nx bp -> (word.[nx], bp))
                           |> Seq.filter (fun (c,bp) -> bp.State.IsSpace)
                           |> Seq.map (fun (c, bp) -> getWordValidData bp.Location c play.Direction.Flip)
                           |> Seq.toList
        
        let scoreWordPlay (boardPlay:BoardPlay) (word:WordWalkResult) (data: (Location*char*(Tile list)) list) = 
            let startLoc = boardPlay.StartLocation
            let getLocation index = match boardPlay.Direction with
                                    | Across -> (startLoc.Width + index, startLoc.Height)
                                    | Down -> (startLoc.Width, startLoc.Height + index)
                
            let mapBagTiles = word.UsedTiles |> Seq.map (fun ut -> getLocation ut.WordLetterIndex, ut) |> Map
            let mapData = data |> Seq.map (fun (loc,chr,tiLst) -> let key = (loc.Width, loc.Height)
                                                                  let tileMap = mapBagTiles.Item key
                                                                  Exception.failIf (tileMap.Letter <> chr) "Expecting char to match"
                                                                  key, (tileMap.BagTileLetter, chr, tiLst)) |> Map
            let getItem = (fun (w,h) -> mapData.Item (w,h))
            let aggData = boardPlay.Locations|> Seq.fold (fun (agg : ScoreData) location -> agg.WithNextLocationScore location getItem) (ScoreData.Initial tileHand)
            
            let bonusScore = match boardPlay.Spaces.Length >= board.Rules.HandSize with
                             | true -> board.Rules.UseAllTileScore | false -> 0

            { Word = word; Locations = aggData.Locations; Score = aggData.CalcScore + bonusScore }

        let getPossibleWordScore (words : seq<WordWalkResult>) (play:BoardPlay) =
            let scoredWords = words |> Seq.map (fun w -> (w, (getSecondaryWordsValidScore w.Word play)))
                                    |> Seq.filter (fun (_, data) -> data |> Seq.forall (fun (valid, _) -> valid))
                                    |> Seq.map (fun (wrd, data) -> let scoreData = data |> Seq.map (fun (_,x) -> x) |> Seq.toList
                                                                   scoreWordPlay play wrd scoreData)
                                    |> Seq.sortBy (fun x -> -x.Score)
                                    |> Seq.toList
            scoredWords
        
        let boardPlays = this.GenerateSpaces board
        let groupPlayData = boardPlays |> Seq.map (fun bp -> (bp, BoardPlayTileData.FromPlay bp))
                                       |> Seq.filter (fun (_, td) -> td.FreeSpaces <= tileHand.Length)
                                       |> Seq.groupBy (fun (_, td) -> td)
        let possiblePlays = groupPlayData |> Seq.map (fun (key, items) -> let words = getPossibleWords key
                                                                          items |> Seq.map (fun (bp, _) -> { BoardPlay = bp; WordScores = getPossibleWordScore words bp }))
                                          |> Seq.collect (fun x -> x)
                                          |> Seq.filter (fun x -> x.WordScores.IsEmpty = false)
                                          |> Seq.sortBy (fun x -> -x.WordScores.Head.Score)
                                          |> Seq.toList
        possiblePlays
