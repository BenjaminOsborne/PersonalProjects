module GameMoveProviderTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let empty3_3 = Board.Empty 3 3

let tileBag = TileBagCreator.Default

let getTile char = match char with
                   | '_' -> { TileLetter = Blank; Value = 0 }
                   | _ -> let (_, t) = tileBag.DrawFromLetter (Letter(char))
                          t

let isExpected words board (tileChars : char list) finalWord finalScore (usedTileChars: char list) =
    let tiles = tileChars |> List.map getTile
    let usedTiles = usedTileChars |> List.map getTile
    let data = { WordSet = new WordSet(words |> Set); Board = board; Player = { Name = "Test" }; Tiles = tiles }
    let gmp = new GameMoveProvider() :> IGameMoveProvider
    let result = gmp.GetNextMove data

    match result with
    | Play(p) -> p.WordScore.Word.Word |> should equal finalWord
                 p.WordScore.Score |> should equal finalScore
                 p.UsedTiles |> should equal usedTiles
    | _ -> failwith "add tests in new file" |> ignore

[<Test>]
let ``Game has letter tiles only``() =
    isExpected ["bad"] empty3_3 ['a';'d';'b']
               "bad" 6 ['b';'a';'d']

    isExpected ["poo"; "zoo"; "pod"] empty3_3 ['p';'o';'o';'z']
               "zoo" 12 ['z';'o';'o']

[<Test>]
let ``Game has blank tiles``() =
    //1 blank
    isExpected ["bad"] empty3_3 ['_';'d';'a']
               "bad" 3 ['_';'a';'d']
    
    isExpected ["bad"] empty3_3 ['b';'_';'a']
               "bad" 4 ['b';'a';'_']
    
    isExpected ["bad"] empty3_3 ['b';'d';'_']
               "bad" 5 ['b';'_';'d']
    
    //2 blank
    isExpected ["bad"] empty3_3 ['_';'_';'a']
               "bad" 1 ['_';'a';'_']

    isExpected ["bad"] empty3_3 ['_';'b';'_']
               "bad" 3 ['b';'_';'_']

    isExpected ["bad"] empty3_3 ['d';'_';'_']
               "bad" 2 ['_';'_';'d']
    
    //3 blanks
    isExpected ["bad"] empty3_3 ['_';'_';'_']
               "bad" 0 ['_';'_';'_']
