module BoardTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let coMap2 a b = SequenceHelpers.CoMap2 a b

[<Test>]
let ``When empty board``() = 
    let board = Board.Empty 2 3
    coMap2 [0..1] [0..2]
        |> Seq.iter (fun (w, h) ->
            let tile = board.TileAt w h
            tile.State |> should equal (Free None))
    

