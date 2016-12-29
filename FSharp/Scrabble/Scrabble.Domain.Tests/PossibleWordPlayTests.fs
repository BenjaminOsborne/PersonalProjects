module PossibleWordPlayTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let getPossible board tileHand wordSet = (new BoardSpaceAnalyser()).GetPossibleScoredPlays board tileHand wordSet

[<Test>]
let ``Word is possible``() =
    let assertSome words letters (assertData: int list) =
        let board = Board.Empty 3 3
        let tiles = letters |> Seq.map (fun c -> { Letter = c; Value = 0}) |> Seq.toList
        let tileHand = new TileHand(tiles)
        let wordSet = new WordSet(words |> Set)
        let possible = getPossible board tileHand wordSet |> Seq.toArray
        possible.Length |> should equal assertData.Length
        assertData |> Seq.iteri (fun nx x -> possible.[nx].Words.Length |> should equal x)

    assertSome ["tie"] ['t'; 'i'; 'e'] [1; 1]