module BoardSpaceTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let getSpaces board = BoardSpaceAnalyser.GenerateSpaces board

[<Test>]
let ``When empty board``() = 
    let assertSpaces size expected =
        let spaces = getSpaces (Board.Empty size size)
        spaces.Length |> should equal expected

    assertSpaces 1 1
    assertSpaces 2 3
    assertSpaces 3 7 //H: 4, V: 4 (1 same) -> 7
    assertSpaces 4 11 //H: 6, V: 6 (1 same) -> 11
    assertSpaces 5 17 //H: 9, V: 9 (1 same) -> 17