module WordScoreTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let getPossible board tileHand wordSet = (new BoardSpaceAnalyser()).GetPossibleScoredPlays board tileHand wordSet

let assertScore board (word:string) score =
    let tileBag = TileBagCreator.Default
    let tiles = word |> Seq.map (fun c -> let (_,t) = tileBag.DrawFromLetter(BagTileLetter.Letter(c))
                                          { Letter = c; Value = t.Value })
                     |> Seq.toList
    let tileHand = new TileHand(tiles)
    let wordSet = new WordSet([word] |> Set)
    let possible = getPossible board tileHand wordSet |> Seq.toList
    possible.Length |> should equal 1
    let head = possible.Head;
    head.WordScores.Length |> should equal 1
    head.WordScores.Head.Score |> should equal score

[<Test>]
let ``With empty board``() =
    let board = Board.Empty 3 1
    assertScore board "tie" 6