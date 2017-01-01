module BoardPlayTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let coMap2 a b = SequenceHelpers.CoMap2 a b
let play w h c v = { Location = { Width = w; Height = h }; Piece = { Letter = c; Value = v } };

[<Test>]
let ``When empty board``() =
    let board = Board.Empty 2 3
    coMap2 [0..1] [0..2]
        |> Seq.iter (fun (w, h) ->
            let tile = board.TileAt w h
            tile.State |> should equal (Free Normal))

[<Test>]
let ``When play tile 2``() =
    let board = (Board.Empty 4 4).Play [ play 2 2 'a' 4]
    let tile = board.TileAt 2 2
    tile.State |> should equal (Played { Letter = 'a'; Value = 4 })
