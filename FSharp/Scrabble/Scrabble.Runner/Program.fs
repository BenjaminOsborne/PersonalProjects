// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open Scrabble.Domain;
open System;

[<EntryPoint>]
let main argv = 
    
    let board = BoardCreator.Default;
    
    let play w h c v = { Location = { Width = w; Height = h }; Piece = { Letter = Letter c; Value = v } };
    let board2 = board.Play [ (play 7 7 'a' 1)
                              (play 8 7 'b' 2)
                              (play 9 7 'c' 1)
                            ]
    
    printfn "%s" (board2.ToString())

    let a = Console.ReadLine();

    0 // return an integer exit code
