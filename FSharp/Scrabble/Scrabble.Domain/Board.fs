namespace Scrabble.Domain

type SequenceHelpers = 
    static member CoMap2 indW indH = indW |> Seq.map (fun w -> indH |> Seq.map (fun h -> (w,h))) |> Seq.collect (fun x -> x);
    static member CoMap ind = SequenceHelpers.CoMap2 ind ind;

type Direction = | Across
                 | Down
    with member this.Flip = match(this) with | Across -> Down | Down -> Across

[<CustomEqualityAttribute>]
[<NoComparisonAttribute>]
type Location =
    { Width : int; Height: int}
    override this.Equals(obj) =
        match obj with
        | :? Location as other -> other.Width = this.Width && other.Height = this.Height
        | _ -> false
    override this.GetHashCode() = (this.Width * 397) ^^^ this.Height  
    override this.ToString() = "(" + this.Width.ToString() + " " + this.Height.ToString() + ")"

type BagTileLetter = | Letter of char
                     | Blank
type BagTile = { TileLetter : BagTileLetter; Value : int }

type Tile = { Letter : char; Value : int }

type BoardSpace =
    | Normal
    | LetterMultiply of int
    | WordMultiply of int
    member this.GetLetterMultiply = match this with | LetterMultiply(v) -> v  | _ -> 1
    member this.GetWordMultiply = match this with | WordMultiply(v) -> v  | _ -> 1

type BoardState =
    | Played of Tile
    | Free of BoardSpace
    member this.IsSpace = match this with | Free(_) -> true | _ -> false
    member this.Tile = match this with | Free(_) -> None | Played(t) -> Some(t)

type BoardLocation = { Location : Location; State : BoardState }

type TilePlay = { Location : Location; Piece : Tile }

type Board(tileLocations : BoardLocation[,], width:int, height:int) = 
    
    static member Empty wdth hght =
        let array = Array2D.init wdth hght (fun w h -> { Location = { Width = w; Height = h }; State = BoardState.Free(Normal) })
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
                  |> Seq.exists (fun (w,h) -> (this.TileAt w h).State.IsSpace = false)
    
    member this.IsMiddleTile wdth hght = (wdth = width/2) && (hght = height/2)

    override this.ToString() =
        let scoreToString score = if score < 10 then " " + score.ToString() else score.ToString()

        let tileToString tile = match tile.State with
                                | Played(p) -> " " + p.Letter.ToString()
                                | Free(t)   -> match(t) with
                                               | Normal -> "  "
                                               | LetterMultiply(m) -> "L" + m.ToString()
                                               | WordMultiply(m) -> "W" + m.ToString()

        let rows = {0 .. height-1} |> Seq.map
                    (fun h -> {0 .. width-1} |> Seq.map (fun w -> tileLocations.[w,h] |> tileToString)
                                             |> Seq.fold (fun acc x -> acc + x + "|") "|");
        rows |> Seq.fold (fun acc x -> acc + "\n" + x) ""
