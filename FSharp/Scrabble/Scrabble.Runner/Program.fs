// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open Scrabble.Domain;
open System;

let play w h c v = { Location = { Width = w; Height = h }; Piece = { Letter = c; Value = v } };

let somePlay() =
    let board = BoardCreator.Default.Play [ (play 7 7 'a' 1)
                                            (play 8 7 'b' 2)
                                            (play 9 7 'c' 1) ]
    printfn "%s" (board.ToString())

[<EntryPoint>]
let main argv = 
    
    //somePlay
    let words = WordLoader.LoadAllWords()

    let mutable tileBag = TileBagCreator.Default
    let mutable board = BoardCreator.Default
    let mutable shouldContinue = true
    while shouldContinue do
        let (dts, bag) = tileBag.Draw 7
        tileBag <- bag
        let tiles = dts |> List.map (fun x -> let letter = match x.TileLetter with | Blank -> 'a' | Letter(c) -> c
                                              { Letter = letter; Value = x.Value })
        let tileHand = new TileHand(tiles)
        let possible = (new BoardSpaceAnalyser()).GetPossibleScoredPlays board tileHand words
        
        let played = match possible with
                     | head :: tail -> head.BoardPlay //WiP...
                                       true
                     | _ -> false

        shouldContinue <- shouldContinue && (tileBag.Tiles.IsEmpty = false)

        Console.WriteLine(board);

    let ignore = Console.ReadLine()
    0 // return an integer exit code
