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
        let size = 15
        let create w h space = { Location = { Width = w; Height = h }; State = TileState.Free(space) }
        let array = Array2D.init size size (fun x y -> create x y None)
        let apply(tiles: seq<TileLocation>) = tiles |> Seq.iter (fun x -> array.[x.Location.Width, x.Location.Height] <- x);
        
        let coMap2 indW indH = indW |> Seq.map (fun w -> indH |> Seq.map (fun h -> (w,h)));
        let coMap ind = coMap2 ind ind;
        let coMapRev indA indB = Seq.append (coMap2 indA indB) (coMap2 indB indA);
        
        let createFromRange indexes state = indexes |> Seq.collect (fun x -> x) |> Seq.map (fun (w,h) -> create w h state)

        let tripWrd = createFromRange (coMap {0..7..size}) (WordMultiply 3)
        
        let dubWrdSeq = {1..4} |> Seq.map (fun n -> [ (n,n); (n, size-1-n); (size-1-n,n); (size-1-n,size-1-n)] );
        let dubWrd = createFromRange dubWrdSeq (WordMultiply 2)

        let start = [7] |> Seq.map (fun x -> create x x (WordMultiply 2))

        let trpLet = createFromRange (coMap [1;5;9;13]) (LetterMultiply 3)
        
        let dubLetSeq = Seq.append (coMapRev [0;7;14] [3;11]) (coMap [2;6;8;12]);
        let dubLet = createFromRange dubLetSeq (LetterMultiply 2)
        
        apply ([trpLet; dubLet; tripWrd; dubWrd; start] |> Seq.collect (fun x -> x))
        
        new Board(array, size, size);