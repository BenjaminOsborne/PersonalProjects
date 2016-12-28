namespace Scrabble.Domain

type SequenceHelpers = 
    static member CoMap2 indW indH = indW |> Seq.map (fun w -> indH |> Seq.map (fun h -> (w,h))) |> Seq.collect (fun x -> x);
    static member CoMap ind = SequenceHelpers.CoMap2 ind ind;

type Location = { Width : int; Height: int}

type Letter = | Letter of char
              | Blank

type Tile = { Letter : Letter; Value : int }

type BoardSpace = | None
                  | LetterMultiply of int
                  | WordMultiply of int

type BoardState = | Played of Tile
                  | Free of BoardSpace
    with member this.IsSpace = match this with
                               | Free(_) -> true
                               | _ -> false

type BoardLocation = { Location : Location; State : BoardState }

type TilePlay = { Location : Location; Piece : Tile }

type Board(tileLocations : BoardLocation[,], width:int, height:int) = 
    
    let tileToString tile = match tile.State with
                            | Played(p) -> match(p.Letter) with
                                           | Letter(c) -> " " + c.ToString()
                                           | Blank -> "  "
                            | Free(t)   -> match(t) with
                                           | None -> "  "
                                           | LetterMultiply(m) -> "L" + m.ToString() + ""
                                           | WordMultiply(m) -> "W" + m.ToString() + ""

    static member Empty wdth hght =
        let array = Array2D.init wdth hght (fun w h -> { Location = { Width = w; Height = h }; State = BoardState.Free(None) })
        new Board(array, wdth, hght)

    member this.Width = width
    member this.Height = height

    member this.Play(tiles : TilePlay list) =
        let state = tileLocations |> Array2D.copy
        for tile in tiles do
            state.[tile.Location.Width, tile.Location.Height] <- { Location = tile.Location; State = BoardState.Played tile.Piece }
        new Board(state, width, height)

    member this.TileAt wght hght = tileLocations.[wght, hght]
    
    /// <summary> Location (or any either side or above/below), are a tile </summary>
    member this.IsTouchingTile wdth hght = 
        let locations = [ (wdth, hght); (wdth-1, hght); (wdth+1, hght); (wdth, hght-1); (wdth, hght+1) ]
        locations |> Seq.filter (fun (w,h) -> w >= 0 && w < this.Width && h >=0 && h < this.Height)
                  |> Seq.exists (fun (w,h) -> (this.TileAt w h).State.IsSpace <> true)
    
    member this.IsMiddleTile wdth hght = (wdth = width/2) && (hght = height/2)

    override this.ToString() = 
        let rows = {height-1 .. -1 .. 0} |> Seq.map
                    (fun h -> {0 .. width-1} |> Seq.map (fun w -> tileLocations.[w,h] |> tileToString)
                                             |> Seq.fold (fun acc x -> acc + x + "|") "|");
        rows |> Seq.fold (fun acc x -> acc + "\n" + x) ""

type BoardCreator = 
    
    static member Default =
        let size = 15
        let create w h space = { Location = { Width = w; Height = h }; State = BoardState.Free(space) }
        let array = Array2D.init size size (fun x y -> create x y None)
        let apply(tiles: seq<BoardLocation>) = tiles |> Seq.iter (fun x -> array.[x.Location.Width, x.Location.Height] <- x);
        
        let coMap2 = SequenceHelpers.CoMap2
        let coMap = SequenceHelpers.CoMap
        let coMapRev indA indB = Seq.append (coMap2 indA indB) (coMap2 indB indA);
        
        let createFromRange indexes state = indexes |> Seq.map (fun (w,h) -> create w h state)

        let tripWrd = createFromRange (coMap {0..7..size}) (WordMultiply 3)
        
        let dubWrdSeq = {1..4} |> Seq.map (fun n -> [ (n,n); (n, size-1-n); (size-1-n,n); (size-1-n,size-1-n)] ) |> Seq.collect (fun x -> x);
        let dubWrd = createFromRange dubWrdSeq (WordMultiply 2)

        let start = [7] |> Seq.map (fun x -> create x x (WordMultiply 2))

        let trpLet = createFromRange (coMap [1;5;9;13]) (LetterMultiply 3)
        
        let dubLetSeq = Seq.append (coMapRev [0;7;14] [3;11]) (coMap [2;6;8;12]);
        let dubLet = createFromRange dubLetSeq (LetterMultiply 2)
        
        apply ([trpLet; dubLet; tripWrd; dubWrd; start] |> Seq.collect (fun x -> x))
        
        new Board(array, size, size);