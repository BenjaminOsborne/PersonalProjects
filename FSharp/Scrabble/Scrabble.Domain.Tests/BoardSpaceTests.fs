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

    assertSpaces 3 27 //Horizontal: 3 x (3 + 2 + 1) = 18. //Vertical = Horizontal - (3 * 3) = 9 // Total = 18 + 9 = 27
    assertSpaces 4 64 //Horizontal: 4 x (4 + 3 + 2 + 1) = 40. //Vertical = Horizontal - (4 * 4) = 24 // Total = 40 + 24 = 64
