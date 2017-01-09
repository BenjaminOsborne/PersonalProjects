module ScrabbleGameTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

type TestProvider (getMove : int -> GameMoveData -> GameMove) =
    let mutable count = 0
    interface IGameMoveProvider with
        member x.GetNextMove data =
            count <- count+1
            getMove count data

let words = new WordSet(["one"; "two"; "tre";] |> Set)
let initStates_2 = [ { Name = "1" }; { Name = "2" }] |> List.map (fun p -> PlayerState.Empty p)
let board_3_3 = Board.Empty 3 3
let emptyBag = new TileBag([])

[<Test>]
let ``Game has no plays``() =
    
    let initial = { Board = board_3_3; TileBag = emptyBag; PlayerStates = initStates_2 }
    let game = new ScrabbleGame(words, 3, initial)
    
    let testProvider = new TestProvider (fun count gmd -> Complete)
    let result = game.PlayGame testProvider
    
    result.PlayerStates.Length |> should equal 2
    result.PlayerStates |> Seq.iter (fun x ->
        x.Plays.Length |> should equal 1
        let allComplete = x.Plays |> Seq.forall (fun p -> p = Complete)
        allComplete |> should equal true)

[<Test>]
let ``Game has 4 plays``() =
    let initial = { Board = board_3_3; TileBag = emptyBag; PlayerStates = initStates_2 }
    let game = new ScrabbleGame(words, 3, initial)
    
    let emptyPlay = { WordScore = { Word = ""; Locations = []; Score = 0 }; UsedTiles = [] }
    let testProvider = new TestProvider (fun count gmd -> 
        match count with
        | 1|2|3|4 -> Play(emptyPlay)
        | _       -> Complete)

    let result = game.PlayGame testProvider
    
    result.PlayerStates.Length |> should equal 2
    result.PlayerStates |> Seq.iter (fun x ->
        x.Plays.Length |> should equal 1
        let allComplete = x.Plays |> Seq.forall (fun p -> p = Complete)
        allComplete |> should equal true)