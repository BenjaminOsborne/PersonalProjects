module PossibleWordPlayTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let getPossible board tileHand wordSet = (new BoardSpaceAnalyser()).GetPossibleScoredPlays board tileHand wordSet

let assertSome board words letters (assertData: int list) =
    let tiles = letters |> Seq.map (fun c -> { Letter = c; Value = 0}) |> Seq.toList
    let tileHand = new TileHand(tiles)
    let wordSet = new WordSet(words |> Set)
    let possible = getPossible board tileHand wordSet |> Seq.toArray
    possible.Length |> should equal assertData.Length
    assertData |> Seq.iteri (fun nx x -> possible.[nx].Words.Length |> should equal x)

[<Test>]
let ``With empty board``() =
    let board = Board.Empty 3 3
    assertSome board ["tie"] [] []
    assertSome board ["tie"] ['t'; 'i'; 'f'] []

    assertSome board ["tie"] ['t'; 'i'; 'e'] [1; 1]
    
    assertSome board ["dog"; "god"] ['o'; 'd'; 'g'] [2; 2]