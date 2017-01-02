module WordScoreTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let getPossible board tileHand wordSet = (new BoardSpaceAnalyser()).GetPossibleScoredPlays board tileHand wordSet

let assertScoreTiles board (word:string) (letters:seq<char>) score =
    let tileBag = TileBagCreator.Default
    let tiles = letters |> Seq.map (fun c -> let (_,t) = tileBag.DrawFromLetter(BagTileLetter.Letter(c))
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

let assertScore board (word:string) score = assertScoreTiles board word word score

let assertPlayArray array word tiles score =
    let initial = BoardCreator.Default
    let board = BoardCreator.PlayArray initial array
    assertScoreTiles board word tiles score

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

[<Test>]
let ``Puzzle 1``() =
    let array = [[' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; 'r'; 'a'; 'd'; ' '; ' '; 'e'; 'y'; 'e'; 'd'; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; 'd'; 'i'; 'v'; 'a'; 'n'; ' '; ' '; 'a'; ' '; ' '; 'a'; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; 'x'; ' '; ' '; ' '; 'n'; 'e'; 'w'; 't'; ' '];
                 [' '; ' '; ' '; ' '; ' '; 'p'; 'e'; 'w'; ' '; ' '; ' '; ' '; 'o'; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; 's'; 'a'; 'f'; 'e'; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; 't'; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; 'c'; 'a'; 'm'; 'e'; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; 'h'; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' ']]
    assertPlayArray array "pipework" "rockpil" 38

[<Test>]
let ``Puzzle 2``() =
    let array = [[' '; ' '; ' '; ' '; ' '; 'c'; 'o'; 'x'; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; 'h'; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 ['d'; 'j'; 'i'; 'n'; 'n'; 'i'; ' '; ' '; ' '; 'r'; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; 'l'; ' '; ' '; 'f'; 'a'; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; 'l'; 'o'; 'v'; 'i'; 'n'; 'g'; ' '; ' '; ' '; ' '];
                 [' '; 'b'; 'e'; ' '; ' '; 'i'; ' '; ' '; 't'; ' '; ' '; ' '; ' '; ' '; ' '];
                 ['w'; 'o'; 'r'; 'k'; ' '; ' '; ' '; ' '; 'l'; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; 'o'; 'e'; ' '; ' '; 'r'; 'o'; 'p'; 'y'; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; 'z'; ' '; ' '; ' '; ' '; 'u'; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; 'e'; 'a'; 'r'; 'i'; 'n'; 'g'; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; 'h'; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; 't'; 'e'; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' ']]
    assertPlayArray array "workmen" "enflomb" 65 //Next: menfolk, 43

[<Test>]
let ``Puzzle 3``() =
    let array = [[' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; 'd'; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; 'q'; 'u'; 'i'; 't'; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; 'l'; 'u'; 'g'; ' '; 'r'; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; 'e'; ' '; ' '; 'i'; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; 'a'; 's'; 'k'; ' '; 'c'; 'o'; 'y'; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; 't'; 'i'; 'm'; 'e'; 'd'; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; 'f'; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' ']]
    assertPlayArray array "questioned" "odepain" 27

[<Test>]
let ``Puzzle 4``() =
    let array = [[' '; ' '; ' '; 'l'; 'i'; 'o'; 'n'; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; 'v'; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; 'e'; ' '; ' '; 'p'; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; 'r'; ' '; ' '; 'a'; 'v'; 'e'; 's'; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; 'a'; ' '; ' '; ' '; 'e'; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; 'c'; ' '; 'w'; 'i'; 's'; 'e'; ' '; ' '; ' '; ' '];
                 [' '; ' '; 'l'; 'o'; 'f'; 't'; ' '; 'a'; ' '; 't'; 'h'; 'a'; 'n'; ' '; ' '];
                 [' '; ' '; 'o'; ' '; ' '; 'e'; 'r'; 'r'; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; 't'; ' '; ' '; 'd'; ' '; 'e'; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '];
                 [' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' '; ' ']]
    assertPlayArray array "livestock" "koolcig" 34 //Next: clockwise 33
    