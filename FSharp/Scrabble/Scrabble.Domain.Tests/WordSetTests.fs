module WordSetTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let createWordSet strings = new WordSet(strings |> Set)
let wordSet = createWordSet ["a"; "big"; "hello"; "to"; "my"; "friends"; "again"; "bin"; "gib"; "bag"]

let findWords length letters map =
    let bagTiles = letters |> List.map (fun c -> Letter(c))
    wordSet.WordsForLengthWithLetters length bagTiles (map |> Map) |> Seq.map (fun w -> w.Word)
                                                                   |> Seq.toList

[<Test>]
let ``Word Walking 1 letter``() =
    
    let find_a = findWords 1 ['b'; 'a'] []
    find_a |> should equal ["a"]
    
    let find_a_pin = findWords 1 ['b'; 'a'] [0, 'a']
    find_a_pin |> should equal ["a"]

    let miss_a = findWords 1 ['b'; 'c'] []
    miss_a |> should equal []
    
    let miss_a_pin = findWords 1 ['a';] [0, 'x']
    miss_a_pin |> should equal []


[<Test>]
let ``Word Walking 3 letters``() =
    
    let find_big = findWords 3 ['b'; 'i'; 'g'] []
    find_big |> should equal ["big"; "gib"]

    let find_big_pin_i = findWords 3 ['b'; 'g'; 'n'] [1,'i']
    find_big_pin_i |> should equal ["big"; "bin"; "gib"]

    let find_big_pin_gi = findWords 3 ['b'; 'i'; ] [(1,'i');(0,'g')]
    find_big_pin_gi |> should equal ["gib"]

[<Test>]
let ``Word Walking many letters``() =
    
    let find_no_pinned = findWords 7 ['f';'r';'i';'e';'n';'d';'s'] []
    find_no_pinned |> should equal ["friends"]

    let find_some_pinned = findWords 7 ['f';'r';'i';'e';'n';'d';] [6,'s']
    find_some_pinned |> should equal ["friends"]

    let find_wrong_pinned = findWords 7 ['f';'r';'i';'e';'n';'d';] [(6,'s');(2,'r')]
    find_wrong_pinned |> should equal []

    let find_all_pinned = findWords 7 [] [(0,'f');(1,'r');(2,'i');(3,'e');(4,'n');(5,'d');(6,'s')]
    find_all_pinned |> should equal ["friends"]

    
    
