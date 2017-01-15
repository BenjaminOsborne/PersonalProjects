module GameMoveProviderTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

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
    | Play(p) -> p.WordScore.Word |> should equal finalWord
                 p.WordScore.Score |> should equal finalScore
                 p.UsedTiles |> should equal usedTiles
    | _ -> failwith "add tests in new file" |> ignore

[<Test>]
let ``Game has letter tiles only``() =
    isExpected ["bad"] (Board.Empty 3 3) ['a';'d';'b']
               "bad" 6 ['b';'a';'d']

    isExpected ["poo"; "zoo"; "pod"] (Board.Empty 3 3) ['p';'o';'o';'z']
               "zoo" 12 ['z';'o';'o']

[<Test>]
let ``Game has blank tiles``() =
    isExpected ["bad"] (Board.Empty 3 3) ['_';'d';'b']
               "bad" 5 ['b';'_';'d']
    
    isExpected ["bad"] (Board.Empty 3 3) ['_';'_';'b']
               "bad" 2 ['b';'_';'_']
    
    isExpected ["bad"] (Board.Empty 3 3) ['_';'_';'_']
               "bad" 2 ['_';'_';'_']
