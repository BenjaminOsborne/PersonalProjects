module PossibleWordPlayTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let getPossible board tileHand wordSet = (new BoardSpaceAnalyser()).GetPossibleScoredPlays board tileHand wordSet

let play w h c v = { Location = { Width = w; Height = h }; Piece = { Letter = c; Value = v } };

let assertPossible board words letters (assertData: int list) =
    let tiles = letters |> Seq.map (fun c -> { Letter = c; Value = 0}) |> Seq.toList
    let tileHand = new TileHand(tiles)
    let wordSet = new WordSet(words |> Set)
    let possible = getPossible board tileHand wordSet |> Seq.toArray
    possible.Length |> should equal assertData.Length
    assertData |> Seq.iteri (fun nx x -> possible.[nx].Words.Length |> should equal x)

[<Test>]
let ``With empty board``() =
    let board = Board.Empty 3 3
    assertPossible board ["tie"] [] []
    assertPossible board ["tie"] ['t'; 'i'; 'f'] []

    assertPossible board ["tie"] ['t'; 'i'; 'e'] [1; 1]
    
    assertPossible board ["dog"; "god"] ['o'; 'd'; 'g'] [2; 2]

[<Test>]
let ``With board with letters``() =
    let board = (Board.Empty 5 5).Play [(play 0 2 'a' 1);
                                        (play 1 2 'b' 1);
                                        (play 2 2 'c' 1);
                                        (play 3 2 'd' 1);
                                        (play 4 2 'e' 1)]
    assertPossible board ["bad"] ['b';'a';'d'] [1;1;1]
    assertPossible board ["at";"to";"bo"] ['t';'o'] [1]
