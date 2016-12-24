namespace Scrabble.Domain

type Location = { Width : int; Height: int}

type Letter = | Letter of char
              | Blank

type PlayedPiece = { Letter : Letter; Value : int }

type TileSpace = | None
                 | LetterMultiply of int
                 | WordMultiply of int

type TileState = | Played of PlayedPiece
                 | Free of TileSpace

type TileLocation = { Location : Location; State : TileState }

type TilePlay = { Location : Location; Piece : PlayedPiece }

type Board(tileLocations : TileLocation[,], Width:int, Height:int) = 
    
    let tileToString tile = match tile.State with
                            | Played(p) -> match(p.Letter) with
                                           | Letter(c) -> " " + c.ToString()
                                           | Blank -> "  "
                            | Free(t)   -> match(t) with
                                           | None -> "  "
                                           | LetterMultiply(m) -> "L" + m.ToString() + ""
                                           | WordMultiply(m) -> "W" + m.ToString() + ""

    member this.Play(tiles : TilePlay list) =
        let state = tileLocations |> Array2D.copy
        for tile in tiles do
            state.[tile.Location.Width, tile.Location.Height] <- { Location = tile.Location; State = TileState.Played tile.Piece }
        new Board(state, Width, Height)

    override this.ToString() = 
        let rows = {Height-1 .. -1 .. 0} |> Seq.map
                    (fun h -> {0 .. Width-1} |> Seq.map (fun w -> tileLocations.[w,h] |> tileToString)
                                             |> Seq.fold (fun acc x -> acc + x + "|") "|");
        rows |> Seq.fold (fun acc x -> acc + "\n" + x) ""

type BoardCreator = 
    static member Default =
        let size = 13
        let create w h space = { Location = { Width = w; Height = h }; State = TileState.Free(space) }
        let array = Array2D.init size size (fun x y -> create x y None)
        let apply(tiles: seq<TileLocation>) = tiles |> Seq.iter (fun x -> array.[x.Location.Width, x.Location.Height] <- x);
        
        let createFromRange indexes state = indexes |> Seq.collect (fun x -> x) |> Seq.map (fun (w,h) -> create w h state)

        let tripple = createFromRange ({0..6..size} |> Seq.map (fun w -> {0..6..size} |> Seq.map (fun h -> (w,h))))
                                      (WordMultiply 3)
        let double = createFromRange ({1..4} |> Seq.map (fun n -> [ (n,n); (n, size-1-n); (size-1-n,n); (size-1-n,size-1-n)] ))
                                     (WordMultiply 2)
        let start = [ create 6 6 (WordMultiply 2) ]

        apply tripple
        apply double
        apply start
         
        new Board(array, 13, 13);