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
    let assertBoard (board:Board) w h c v =
        let board = board.Play [ play w h c v]
        let tile = board.TileAt w h
        tile.State |> should equal (Played { Letter = c; Value = v })
        board

    let next0 = (Board.Empty 4 4);
    let next1 = assertBoard next0 2 1 'a' 4
    let next2 = assertBoard next1 2 2 'b' 3
    let next3 = assertBoard next2 2 3 'c' 2
    
    let next4 = assertBoard next3 0 0 't' 1
    let next5 = assertBoard next4 1 0 'o' 0
    let next6 = assertBoard next5 2 0 'p' 6
    
    next6 |> ignore

