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
    
    while tileBag.Tiles.IsEmpty = false do
        let (dts, bag) = tileBag.Draw 7
        tileBag <- bag
        let tiles = dts |> List.map (fun x -> let letter = match x.TileLetter with | Blank -> 'a' | Letter(c) -> c
                                              { Letter = letter; Value = x.Value })
        let tileHand = new TileHand(tiles)
        let a = (new BoardSpaceAnalyser()).GetPossibleScoredPlays board tileHand words
        
        Console.WriteLine(board);

    let ignore = Console.ReadLine()
    0 // return an integer exit code
