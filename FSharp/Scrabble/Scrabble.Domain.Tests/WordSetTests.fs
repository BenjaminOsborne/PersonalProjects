module WordSetTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

[<Test>]
let ``Word Walking 1 letter``() =
    let createWordSet strings = new WordSet(strings |> Set)
    let wordSet = createWordSet ["a"; "big"; "hello"; "to"; "my"; "friends"; "again"; "bin"; "gib"]
    
    let find_a = wordSet.WordsForLengthWithLetters 1 ['b'; 'a'] Map.empty
    find_a |> should equal ["a"]
    
    let find_a_pin = wordSet.WordsForLengthWithLetters 1 ['b'; 'a'] ([(0, 'a')] |> Map)
    find_a_pin |> should equal ["a"]

    let miss_a = wordSet.WordsForLengthWithLetters 1 ['b'; 'c'] Map.empty
    miss_a |> should equal []
    
    let miss_a_pin = wordSet.WordsForLengthWithLetters 1 ['a';] ([(0, 'x')] |> Map)
    miss_a_pin |> should equal []
