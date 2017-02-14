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

let createPlayerStates count =
    [1..count] |> List.map (fun num -> PlayerState.Empty { Name = num.ToString() })

let emptyWords = new WordSet(Set.empty)
let initStates_2 = createPlayerStates 2
let board_3_3 = Board.Empty 3 3
let emptyBag = new TileBag([])

let bagTile_a = { TileLetter = Letter('a'); Value = 1 };
let bagTile_e = { TileLetter = Letter('e'); Value = 1 };

let playGame (game : ScrabbleGame) provider =
    let observer = game.PlayGame provider
    let mutable results = [];
    observer.Subscribe(fun x -> results <- x :: results) |> ignore
    results.Head

let createResult word = { Word = word; UsedTiles = [] }

[<Test>]
let ``Game has no plays different player count``() =
    let assertPlayerCount playerCount =
        let initStates = createPlayerStates playerCount
        let initial = { Board = board_3_3; TileBag = emptyBag; PlayerStates = initStates }
        let game = new ScrabbleGame(emptyWords, 3, initial)
    
        let testProvider = new TestProvider (fun count gmd -> Complete)
        let result = playGame game testProvider
    
        result.PlayerStates.Length |> should equal playerCount
        result.PlayerStates |> Seq.iter (fun x ->
            x.Plays.Length |> should equal 1
            x.Plays.Head |> should equal Complete)
    
    [1..10] |> Seq.iter assertPlayerCount

[<Test>]
let ``Game has 4 plays``() =
    let initial = { Board = board_3_3; TileBag = emptyBag; PlayerStates = initStates_2 }
    let game = new ScrabbleGame(emptyWords, 3, initial)
    
    let testProvider = new TestProvider (fun count gmd -> 
        match count with
        | c when c <= 8 -> let emptyPlay = { WordScore = { Word = createResult ""; Locations = []; Score = 0 }; UsedTiles = [] }
                           Play(emptyPlay)
        | _             -> Complete)

    let result = playGame game testProvider
    
    result.PlayerStates.Length |> should equal 2
    result.PlayerStates |> Seq.iter (fun x ->
        x.Plays.Length |> should equal (4+1)
        let first4Plays = x.Plays |> Seq.skip 1 |> Seq.take 4 |> Seq.forall (fun p -> match p with Play(_) -> true | _ -> false)
        let lastComplete = (x.Plays |> Seq.head) = Complete
        first4Plays |> should equal true
        lastComplete |> should equal true)


let playGameWith1Word bagTiles =
    let tileBag = new TileBag(bagTiles) 
    let initial = { Board = board_3_3; TileBag = tileBag; PlayerStates = initStates_2 }
    let words = new WordSet(["eee"] |> Set)
    let handSize = 4
    let game = new ScrabbleGame(words, handSize, initial)
    
    let testProvider = new TestProvider (fun count gmd -> 
        match gmd.Tiles.Length with
        | 0 -> Complete
        | _ -> let eCount = gmd.Tiles |> Seq.filter (fun x -> x.TileLetter = Letter('e')) |> Seq.length
               match eCount with
               | 0 -> Complete
               | _ -> let max = System.Math.Min(2, eCount-1)
                      let locs = [0..max] |> List.map (fun w -> { Location = { Width = w; Height = 0 }; Piece = { Letter = 'e'; Value = 1 } })
                      let usedTiles = locs |> List.map (fun _ -> bagTile_e)
                      let emptyPlay = { WordScore = { Word = createResult "eee"; Locations = locs; Score = 0 }; UsedTiles = usedTiles }
                      Play(emptyPlay))
    
    playGame game testProvider

[<Test>]
let ``Game has limited tiles - e only``() =
    let bagTiles = [0..9] |> List.map (fun _ -> bagTile_e); //10 tiles
    let result = playGameWith1Word bagTiles

    result.TileBag.Tiles.Length |> should equal 0

    result.PlayerStates.Length |> should equal 2
    let first = result.PlayerStates |> List.index 0
    let second = result.PlayerStates |> List.index 1
    first.Plays.Length |> should equal 3
    second.Plays.Length |> should equal 3

    let hasTileCount tileCount play = 
        match play with
        | Play(a) -> a.UsedTiles.Length = tileCount
        | _ -> false

    let assertCountThenComplete element count1 count2 =
        element.Plays |> List.index 0 |> should equal Complete
        element.Plays |> List.index 1 |> hasTileCount count2 |> should equal true
        element.Plays |> List.index 2 |> hasTileCount count1 |> should equal true

    assertCountThenComplete first 3 3
    assertCountThenComplete second 3 1

[<Test>]
let ``Game has limited tiles - e and a only``() =
    let aTiles = [0..0] |> List.map (fun _ -> bagTile_a); //1 tile
    let eTiles = [0..9] |> List.map (fun _ -> bagTile_e); //10 tiles
    
    let result = playGameWith1Word (List.append aTiles eTiles)
    result.TileBag.Tiles.Length |> should equal 0
    
    result.PlayerStates.Length |> should equal 2
    let first = result.PlayerStates |> List.index 0
    let second = result.PlayerStates |> List.index 1
    (List.append first.Tiles second.Tiles) |> should equal [bagTile_a]
