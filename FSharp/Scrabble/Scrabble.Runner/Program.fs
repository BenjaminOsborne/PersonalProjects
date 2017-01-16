open Scrabble.Domain;
open System;

let play w h c v = { Location = { Width = w; Height = h }; Piece = { Letter = c; Value = v } };

let somePlay() =
    let board = BoardCreator.Default.Play [ (play 7 7 'a' 1)
                                            (play 8 7 'b' 2)
                                            (play 9 7 'c' 1) ]
    printfn "%s" (board.ToString())

let printTiles context (tiles:BagTile list) = 
    let tileText = LetterHelpers.CharListToString (tiles |> List.map (fun x -> match x.TileLetter with Letter(c) -> c | Blank -> '_'))
    Console.WriteLine("\nRemaining " + context + " Tiles: " + tiles.Length.ToString() + " (" + tileText + ")")

let gameRoutine () =
    let words = WordLoader.LoadAllWords()
    let initial = GameStateCreator.InitialiseGameFor [ { Name = "Auto 1" }; { Name = "Auto 2" }]
    let game = new ScrabbleGame(words, 7, initial)
    
    let sw = System.Diagnostics.Stopwatch.StartNew();
    let moveProvider = new GameMoveProvider (fun state -> Console.WriteLine(sw.Elapsed.TotalSeconds.ToString())
                                                          sw.Reset(); sw.Start()
                                                          Console.WriteLine("\n" + state.Board.ToString())
                                                          printTiles "" state.Tiles
                                                          )
    let result = game.PlayGame (moveProvider :> IGameMoveProvider)
    
    Console.WriteLine(result.Board.ToString())
    Console.WriteLine("\nFinal Scores...")
    
    result.PlayerStates |> Seq.iter (fun ps ->
        Console.WriteLine("\n" + ps.Player.Name)
        ps.Plays |> List.rev
                 |> Seq.iter (fun p -> let print = match p with
                                                   | Play(a) -> let w = a.WordScore.Word
                                                                let s = a.WordScore.Score
                                                                let space = LetterHelpers.CharListToString ([w.Length..15] |> List.map (fun _ -> ' '))
                                                                w + space + s.ToString()
                                                   | Switch(_) ->  "Switch"
                                                   | Complete -> "Complete"
                                       Console.WriteLine(print))
        printTiles "Hand" ps.Tiles)

    printTiles "Bag" result.TileBag.Tiles

    let score = result.PlayerStates |> List.collect (fun x -> x.Plays)
                                    |> List.map (fun p -> match p with | Play(a) -> a.WordScore.Score | _ -> 0)
                                    |> List.sum
    Console.WriteLine("Total Score: " + score.ToString())

    let ignore = Console.ReadLine()
    0 // return an integer exit code

[<EntryPoint>]
let main argv = 
    gameRoutine()