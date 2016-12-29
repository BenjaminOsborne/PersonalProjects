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

type ValidWordPlays = { BoardPlay : BoardPlay; Words : Word list }

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
        
        let distinct = Seq.distinctByField(allValid, (fun x -> Seq.EqualitySet x.Spaces)) |> Seq.toList
        distinct
    
    member this.GetPossibleScoredPlays (board:Board) (tileHand:TileHand) (wordSet: WordSet) =
        
        let pinnedLettersMatch (play:BoardPlay) (word : Word) =
            play.Locations |> Seq.mapi (fun nx l -> match l.State with
                                                    | Free(_) -> true
                                                    | Played(t) -> t.Letter = word.Word.[nx])
                           |> Seq.forall (fun x -> x)
        
        let wordMatches (play:BoardPlay) (word : Word) =
            (pinnedLettersMatch play word) && word.CanMakeWordFromSet tileHand.LetterSet

        let isWordValid (location:Location) (letter:char) (direction:Direction) =
            let walkWhileTiles init getLocation =
                Seq.initInfinite init
                |> Seq.map (fun x -> getTile board (getLocation x))
                |> Seq.takeWhile (fun x -> x.IsSome)
                |> Seq.map (fun x -> x.Value.Letter)
                |> Seq.toList
            
            let getTiles start getLocation = 
                let forward = walkWhileTiles (fun x -> start + x + 1) getLocation
                let backward = walkWhileTiles (fun x -> start - x - 1) getLocation
                (backward |> List.rev) |> List.append (letter::forward) 

            let chars = match direction with
                        | Across -> getTiles location.Width (fun x -> { Width = x; Height = location.Height })
                        | Down   -> getTiles location.Height (fun x -> { Width = location.Width; Height = x })
            if (chars.Length <= 1) then
                (true)
            else
                let word = LetterHelpers.CharListToString chars
                let isWord = wordSet.IsWord word
                (isWord)

        let areSecondaryWordsValid (word:Word) (play:BoardPlay) =
            play.Locations |> Seq.mapi (fun nx bp -> (word.Word.[nx], bp))
                           |> Seq.filter (fun (c,bp) -> bp.State.IsSpace)
                           |> Seq.map (fun (c, bp) -> isWordValid bp.Location c play.Direction.Flip)
                           |> Seq.forall (fun x -> x)

        let boardPlays = this.GenerateSpaces board
        let getPossibleWords (play:BoardPlay) =
            let words = wordSet.WordsForLength play.Locations.Length
            words |> Seq.filter (fun w -> (wordMatches play w) && (areSecondaryWordsValid w play))
                  |> Seq.toList
        
        let possiblePlays = boardPlays |> Seq.map (fun bp -> { BoardPlay = bp; Words = getPossibleWords bp }) |> Seq.toList
        possiblePlays
