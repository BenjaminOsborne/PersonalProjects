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

let manualRoutine () = 
    
    //somePlay
    let words = WordLoader.LoadAllWords()

    let mutable tileBag = TileBagCreator.Default
    let mutable tileList = List.empty<BagTile>;
    let mutable board = BoardCreator.Default
    let mutable wordPlays = List.empty<string*int>
    let mutable shouldContinue = true

    let getLetter bt = match bt.TileLetter with | Blank -> 'a' | Letter(c) -> c

    while shouldContinue do
        Console.WriteLine(board);
        
        let (bts, bag) = tileBag.Draw (7-tileList.Length)
        tileList <- List.append tileList bts
        tileBag <- bag

        let tiles = tileList |> List.map (fun bt-> { Letter = (getLetter bt); Value = bt.Value })
                             |> Seq.sortBy (fun x -> -x.Value) |> Seq.toList

        let tileHand = new TileHand(tiles)
        let possible = (new BoardSpaceAnalyser()).GetPossibleScoredPlays board tileHand words
        
        let played = match possible with
                     | head :: tail -> let topScore = head.WordScores.Head
                                       let locationPlays = topScore.Locations;
                                       let plays = locationPlays |> List.map (fun (loc, tile) -> { Location = loc; Piece = tile })
                                       let updateBoard = board.Play plays
                                       
                                       board <- updateBoard
                                       wordPlays <- (topScore.Word,topScore.Score) :: wordPlays

                                       let remain = locationPlays |> Seq.fold (fun (agg : BagTile list) (l,t) -> let (a,b) = agg |> List.removeFirstWith (fun x -> t.Letter = (getLetter x))
                                                                                                                 b) tileList
                                       tileList <- remain
                                       true
                     | _ -> false
        
        shouldContinue <- shouldContinue && played && (tileBag.Tiles.IsEmpty = false)
    
    Console.WriteLine("\nFinal Scores...")

    wordPlays |> Seq.iter (fun (w,s) -> let space = LetterHelpers.CharListToString ([w.Length..15] |> List.map (fun _ -> ' '))
                                        Console.WriteLine(w + space + s.ToString()))

    let printTiles context (tiles:BagTile list) = 
        let tileText = LetterHelpers.CharListToString (tiles |> List.map (fun x -> getLetter x))
        Console.WriteLine("Remaining " + context + " Tiles: " + tiles.Length.ToString() + " (" + tileText + ")")
    
    printTiles "Hand" tileList
    printTiles "Bag" tileBag.Tiles

    Console.WriteLine("Total Score: " + (wordPlays |> Seq.sumBy (fun (_,s) -> s)).ToString())

    let ignore = Console.ReadLine()
    0 // return an integer exit code

[<EntryPoint>]
let main argv = 
    manualRoutine()