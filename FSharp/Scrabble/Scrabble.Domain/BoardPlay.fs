namespace Scrabble.Domain

type BoardPlay(locations : BoardLocation list, direction : Direction) =
    let lazySpaces = new System.Lazy<BoardLocation list>(fun _ -> locations |> Seq.filter (fun x -> x.State.IsSpace) |> Seq.toList)
    member this.Locations = locations
    member this.Spaces = lazySpaces.Value
    member this.AnySpaces = this.Spaces.Length > 0
    member this.Direction = direction

    override this.ToString() =
        match locations.Length with
        | 0 -> "<EMPTY>"
        | 1 -> locations.Head.Location.ToString()
        | _ -> let last = locations |> Seq.last
               locations.Head.Location.ToString() + " - " + last.Location.ToString()

type WordScore = { Word : Word; Locations: (Location*Tile) list; Score : int }
type ValidWordPlays = { BoardPlay : BoardPlay; WordScores : WordScore list }

type ScoreData =
    { MainScore : int; MainScoreMultiplier : int; SideScores : int; RemainingTileHand : TileHand;  Locations: (Location*Tile) list }

    static member Create ms msm ss th lcs = { MainScore = ms; MainScoreMultiplier = msm; SideScores = ss; RemainingTileHand = th; Locations = lcs; }
    static member Initial th = ScoreData.Create 0 1 0 th []
    member this.CalcScore = this.MainScore * this.MainScoreMultiplier + this.SideScores
    member this.WithNextLocationScore location getItem =
        let getSideScore (tiles : Tile list) tileValue letterMult wordMult =
            match tiles.Length with
            | 0 -> 0
            | _ -> let incomingScore = tiles |> Seq.sumBy (fun x -> x.Value)
                   (incomingScore + (tileValue * letterMult)) * wordMult
        
        let (ms,msm,ss,th,lcs) = match location.State with
                                 | Played(t) ->  (t.Value, 1, 0, this.RemainingTileHand, [])
                                 | Free(bs) ->   let (c,tiles) = getItem (location.Location.Width, location.Location.Height)
                                                 let (t,th) = this.RemainingTileHand.PopNextTileFor c
                                                 let (lm,wm) = (bs.GetLetterMultiply, bs.GetWordMultiply)
                                                 let sideScore = getSideScore tiles t.Value lm wm
                                                 (t.Value * lm, wm, sideScore, th, [(location.Location, t)])
        ScoreData.Create (this.MainScore + ms) (this.MainScoreMultiplier * msm) (this.SideScores + ss) th (List.append this.Locations lcs)

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
        
        let distinct = allValid |> Seq.distinctByField (fun x -> Seq.EqualitySet x.Spaces) |> Seq.toList
        distinct
    
    member this.GetPossibleScoredPlays (board:Board) (tileHand:TileHand) (wordSet: WordSet) =

        let pinnedLettersMatch (play:BoardPlay) (word : Word) =
            play.Locations |> Seq.mapi (fun nx l -> match l.State with
                                                    | Free(_) -> true
                                                    | Played(t) -> t.Letter = word.Word.[nx])
                           |> Seq.forall (fun x -> x)
        
        let wordMatches (play:BoardPlay) (word : Word) =
            (pinnedLettersMatch play word) && word.CanMakeWordFromSet tileHand.LetterSet

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

        let getSecondaryWordsValidScore (word:Word) (play:BoardPlay) =
            play.Locations |> Seq.mapi (fun nx bp -> (word.Word.[nx], bp))
                           |> Seq.filter (fun (c,bp) -> bp.State.IsSpace)
                           |> Seq.map (fun (c, bp) -> getWordValidData bp.Location c play.Direction.Flip)
                           |> Seq.toList
        
        let scoreWordPlay (boardPlay:BoardPlay) (word:Word) (data: (Location*char*(Tile list)) list) = 
            let orderedLocs = boardPlay.Locations |> Seq.sortBy (fun x -> match x.State with //Biggest letter then biggest word
                                                                          | Free(t) -> -t.GetLetterMultiply, -t.GetWordMultiply
                                                                          | _ -> 0,0) |> Seq.toList
            let mapData = data |> Seq.map (fun (a,b,c) -> (a.Width, a.Height), (b,c)) |> Map
            let getItem = (fun (w,h) -> mapData.Item (w,h))
            let aggData = orderedLocs|> Seq.fold (fun (agg : ScoreData) location -> agg.WithNextLocationScore location getItem) (ScoreData.Initial tileHand)
            { Word = word; Locations = aggData.Locations; Score = aggData.CalcScore }

        let getPossibleWords (play:BoardPlay) =
            let words = wordSet.WordsForLength play.Locations.Length
            let scoredWords = words |> Seq.filter (fun w -> (wordMatches play w))
                                    |> Seq.map (fun w -> (w, (getSecondaryWordsValidScore w play)))
                                    |> Seq.filter (fun (_, data) -> data |> Seq.forall (fun (valid, _) -> valid))
                                    |> Seq.map (fun (wrd, data) -> let scoreData = data |> Seq.map (fun (_,x) -> x) |> Seq.toList
                                                                   scoreWordPlay play wrd scoreData)
                                    |> Seq.toList
            scoredWords
        
        let boardPlays = this.GenerateSpaces board
        let possiblePlays = boardPlays |> Seq.map (fun bp -> { BoardPlay = bp; WordScores = getPossibleWords bp })
                                       |> Seq.filter (fun x -> x.WordScores.IsEmpty = false)
                                       |> Seq.toList
        possiblePlays
