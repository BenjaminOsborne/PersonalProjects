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

type Board(tileLocations : TileLocation[,]) = 
    
    member this.X = "F#"

    static member Default =
        let array = Array2D.init 15 15 (fun x y -> { Location = { Width = x; Height = y }; State = TileState.Free(None) })
        new Board(array); 
