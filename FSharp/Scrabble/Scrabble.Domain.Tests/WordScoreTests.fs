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
    possible.Length |> should greaterThanOrEqualTo 1
    
    let best = possible.Head.WordScores.Head
    best.Word.Word |> should equal word
    best.Score |> should equal score
    
    possible |> Seq.iter (fun t ->
        t.WordScores.Length |> should equal 1
        t.WordScores.Head.Score |> should lessThanOrEqualTo best.Score
        )

[<Test>]
let ``With empty board``() =
    let board = Board.Empty 3 1
    assertScore board "tie" 3 // (1+1+1)

[<Test>]
let ``With default board``() =
    let board = BoardCreator.Default
    
    assertScore board "a" 2 //(1*2)
    assertScore board "avise" 18 //((1*2) + 4 + 1 + 1 + 1) * 2
    assertScore board "aaeeiioo" 54 //(7 + 1*2) * 2 * 3 -> 54
    assertScore board "farces" 30 //((4*2) + 1 + 1 + 3 + 1 + 1) * 2 -> 30