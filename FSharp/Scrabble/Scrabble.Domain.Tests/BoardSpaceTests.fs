module BoardSpaceTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let getSpaces board = (new BoardSpaceAnalyser()).GenerateSpaces board

let assertBoardSpaces board expectedAcross expectedDown =
        let spaces = getSpaces board
        let filter direction = spaces |> Seq.filter (fun x -> x.Direction = direction) |> Seq.toList
        (filter Across).Length |> should equal expectedAcross
        (filter Down).Length |> should equal expectedDown
        spaces.Length |> should equal (expectedAcross + expectedDown)

let play w h c v = { Location = { Width = w; Height = h }; Piece = { Letter = c; Value = v } };

let playAll (board:Board) locations =
    let plays = locations |> Seq.map (fun (w,h) -> play w h 'f' 4) |> Seq.toList
    board.Play plays

let fullBoard size = playAll (Board.Empty size size) (SequenceHelpers.CoMap {0 .. size-1})

[<Test>]
let ``When empty board``() = 
    let assertSpaces size expected = assertBoardSpaces (Board.Empty size size) expected

    assertSpaces 1 1 0
    assertSpaces 2 2 1
    assertSpaces 3 4 3 //H: 4, V: 4 (1 same) -> 7
    assertSpaces 4 6 5 //H: 6, V: 6 (1 same) -> 11
    assertSpaces 5 9 8 //H: 9, V: 9 (1 same) -> 17

[<Test>]
let ``When full board``() = 
    {1..15} |> Seq.iter (fun size -> assertBoardSpaces (fullBoard size) 0 0)

[<Test>]
let ``When 1 space``() = 
    let create size =
        let locs = (SequenceHelpers.CoMap {0.. size-1}) |> Seq.filter (fun (w, h) -> w <> size/2 || h <> size/2)
        playAll (Board.Empty size size) locs
    {1..15} |> Seq.iter (fun size -> assertBoardSpaces (create size) 1 0) //Should be 1 play if only 1 space

[<Test>]
let ``When single middle piece``() = 
    let board = (Board.Empty 3 3).Play [play 1 1 'a' 1]
    //All: H: 3 x (3 + 2 + 1) = 18. //V = H - (3 * 3) = 9 // Total = 18 + 9 = 27
    //Excluded: Middle + 4 corners -> 27 - 5 = 22
    //Identical: Singles and single + middle -> 22 - 4 = 18
    assertBoardSpaces board 11 7

[<Test>]
let ``When middle row or column``() =
    let playRow size filter =
        let locs = (SequenceHelpers.CoMap {0.. size-1}) |> Seq.filter filter
        playAll (Board.Empty size size) locs
    
    let assertExpected size expAcross expDown expBothAcross expBothDown = 
        let rowBoard = playRow size (fun (w,h) -> h = size/2)
        let colBoard = playRow size (fun (w,h) -> w = size/2)
        let rowColBoard = playRow size (fun (w,h) -> w = size/2 || h = size/2)
        assertBoardSpaces rowBoard expAcross expDown
        assertBoardSpaces rowColBoard expBothAcross expBothDown
    
    assertExpected 3 12 3 6 2
    assertExpected 5 30 30 36 24 //(V: (8 * 5) + H: (10 * 2) = 60), (V: 8*4 + H:V - 4 repeats = 60)
