namespace Scrabble.Domain

type BoardPlay(locations : BoardLocation list) =
    let spaces = new System.Lazy<int>(fun _ -> locations |> Seq.filter (fun x -> x.State.IsSpace) |> Seq.length);
    member this.Locations = locations
    member this.Spaces = spaces.Value

    override this.ToString() =
        match locations.Length with
        | 0 -> "<EMPTY>"
        | 1 -> locations.Head.Location.ToString()
        | _ -> let last = locations |> Seq.last
               locations.Head.Location.ToString() + " - " + last.Location.ToString()

type BoardSpaceAnalyser =

    static member GenerateSpaces(board:Board) =        

        let allSpaces outer inner hasSpaceOrEdge tileAt =
            let manySeqs = {0 .. outer-1} |> Seq.map (fun o ->
                {0 .. inner-1} |> Seq.filter (fun inS -> hasSpaceOrEdge o (inS-1))
                               |> Seq.map (fun inS ->
                    {inS .. inner-1} |> Seq.filter (fun inE -> hasSpaceOrEdge o (inE+1))
                                     |> Seq.map (fun inE ->
                        let spaceList = {inS .. inE} |> Seq.map(fun i -> tileAt o i) |> Seq.toList
                        BoardPlay(spaceList))))
            manySeqs |> Seq.collect (fun x -> x) |> Seq.collect (fun x -> x)
        
        let width = board.Width
        let height = board.Height
        let hasSpaceOrEdge w h = w < 0 || w >= width || h < 0 || h >= height || (board.TileAt w h).State.IsSpace
        
        let horiSpaces = allSpaces height width (fun h w ->  hasSpaceOrEdge w h) (fun h w -> board.TileAt w h)
        let vertSpaces = allSpaces width height (fun w h -> hasSpaceOrEdge w h) (fun w h -> board.TileAt w h)
        let allSpaces = (Seq.append horiSpaces vertSpaces)
        
        let isTouching loc = board.IsTouchingTile loc.Width loc.Height || board.IsMiddleTile loc.Width loc.Height
        let allValid = allSpaces |> Seq.filter (fun x -> x.Locations |> Seq.exists (fun t -> isTouching t.Location))
        
        let distinct = Seq.distinctByField(allValid, (fun x -> Seq.EqualitySet x.Locations)) |> Seq.toList
        distinct
    