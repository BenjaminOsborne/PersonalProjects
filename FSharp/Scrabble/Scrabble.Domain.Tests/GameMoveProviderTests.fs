module GameMoveProviderTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let tileBag = TileBagCreator.Default
let letterOf char =
    let (_, t) = tileBag.DrawFromLetter (Letter(char))
    t

let blank = { TileLetter = Blank; Value = 0 }
let (a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z) =
    ((letterOf 'a'), (letterOf 'b'), (letterOf 'c'), (letterOf 'd'), (letterOf 'e'), (letterOf 'f'),
     (letterOf 'g'), (letterOf 'h'), (letterOf 'i'), (letterOf 'j'), (letterOf 'k'), (letterOf 'l'),
     (letterOf 'm'), (letterOf 'n'), (letterOf 'o'), (letterOf 'p'), (letterOf 'q'), (letterOf 'r'),
     (letterOf 's'), (letterOf 't'), (letterOf 'u'), (letterOf 'v'), (letterOf 'w'), (letterOf 'x'),
     (letterOf 'y'), (letterOf 'z'))

let isExpected words board tiles finalWord finalScore usedTiles =
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
    isExpected ["bad"] (Board.Empty 3 3) [a;d;b]
               "bad" 6 [b;a;d]

    isExpected ["poo"; "zoo"; "pod"] (Board.Empty 3 3) [p;o;o;z]
               "zoo" 12 [z;o;o]

[<Test>]
let ``Game has blank tiles``() =
    isExpected ["bad"] (Board.Empty 3 3) [blank;d;b]
               "bad" 5 [b;blank;d]
    
    isExpected ["bad"] (Board.Empty 3 3) [blank;blank;b]
               "bad" 2 [b;blank;blank]
    
    isExpected ["bad"] (Board.Empty 3 3) [blank;blank;blank]
               "bad" 2 [blank;blank;blank]
