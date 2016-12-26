namespace Scrabble.Domain

type BoardPlay(locations : BoardLocation list) =
    let spaces = new System.Lazy<int>(fun _ -> locations |> Seq.filter (fun x -> x.State.IsSpace) |> Seq.length);
    member this.Locations = locations
    member this.Spaces = spaces.Value

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
                            |> Seq.filter (fun x -> x.Locations.Length > 1) //1 length spaces will be duplicated in both directions
        


        let allSpaces = (Seq.append horiSpaces vertSpaces) |> Seq.toList
        allSpaces
    