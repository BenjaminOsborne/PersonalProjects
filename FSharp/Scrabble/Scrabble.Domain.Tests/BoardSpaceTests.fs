module BoardSpaceTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let getSpaces board = BoardSpaceAnalyser.GenerateSpaces board

let assertBoardSpaces board expected =
        let spaces = getSpaces board
        spaces.Length |> should equal expected

let play w h c v = { Location = { Width = w; Height = h }; Piece = { Letter = Letter c; Value = v } };

[<Test>]
let ``When empty board``() = 
    let assertSpaces size expected = assertBoardSpaces (Board.Empty size size) expected

    assertSpaces 1 1
    assertSpaces 2 3
    assertSpaces 3 7 //H: 4, V: 4 (1 same) -> 7
    assertSpaces 4 11 //H: 6, V: 6 (1 same) -> 11
    assertSpaces 5 17 //H: 9, V: 9 (1 same) -> 17

[<Test>]
let ``When full board``() = 
    let fullBoard size = 
        let plays = SequenceHelpers.CoMap {0 .. size-1} |> Seq.map (fun (w,h) -> play w h 'f' 4) |> Seq.toList
        (Board.Empty size size).Play plays
    {1..15} |> Seq.iter (fun size -> assertBoardSpaces (fullBoard size) 0)

[<Test>]
let ``When middle piece``() = 
    let board = (Board.Empty 3 3).Play [play 1 1 'a' 1]
    //All: H: 3 x (3 + 2 + 1) = 18. //V = H - (3 * 3) = 9 // Total = 18 + 9 = 27
    //Excluded: Middle + 4 corners -> 27 - 5 = 22
    assertBoardSpaces board 22

