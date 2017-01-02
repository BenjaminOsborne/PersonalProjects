module PossibleWordPlayTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let getPossible board tileHand wordSet = (new BoardSpaceAnalyser()).GetPossibleScoredPlays board tileHand wordSet

let assertPossible board words letters (assertData: int list) =
    let tiles = letters |> Seq.map (fun c -> { Letter = c; Value = 0}) |> Seq.toList
    let tileHand = new TileHand(tiles)
    let wordSet = new WordSet(words |> Set)
    let possible = getPossible board tileHand wordSet |> Seq.toArray
    possible.Length |> should equal assertData.Length
    assertData |> Seq.iteri (fun nx x -> possible.[nx].WordScores.Length |> should equal x)

[<Test>]
let ``With empty board``() =
    let board = Board.Empty 3 3
    assertPossible board ["tie"] [] []
    assertPossible board ["tie"] ['t'; 'i'; 'f'] []

    assertPossible board ["tie"] ['t'; 'i'; 'e'] [1; 1]
    
    assertPossible board ["dog"; "god"] ['o'; 'd'; 'g'] [2; 2]

[<Test>]
let ``Board with singe letter``() =
    let board = BoardCreator.FromArray [[' '; 'b'; ' '; ' ';' ']]

    assertPossible board ["bad"] ['a';'d'] [1]
    assertPossible board ["abate"] ['a';'a';'t'; 'e'] [1]
    assertPossible board ["bib"] ['i';'b'] [1]

[<Test>]
let ``Board with letters 1``() =
    let board = BoardCreator.FromArray [[' '; ' '; ' '; ' ';' '];
                                        [' '; ' '; ' '; ' ';' '];
                                        ['a'; 'b'; 'c'; 'd';'e'];
                                        [' '; ' '; ' '; ' ';' '];
                                        [' '; ' '; ' '; ' ';' '];]

    assertPossible board ["bad"] ['a';'d'] [1]
    assertPossible board ["bad"] ['b';'a';'d'] [1;1;1]
    assertPossible board ["at";"to";"bo"] ['t';'o'] [1]

[<Test>]
let ``Board with letters 2``() =
    let board = BoardCreator.FromArray [[' '; ' '; ' '; ' ';' '];
                                        [' '; ' '; ' '; ' ';' '];
                                        ['d'; 'o'; 'n'; 'u';'t'];
                                        [' '; ' '; ' '; ' ';' '];
                                        [' '; ' '; ' '; ' ';' '];]
    assertPossible board ["donut"; "dot"; "dug"] ['d';'o';'t'; 'u'; 'g'] [2;1;1;1;1]
