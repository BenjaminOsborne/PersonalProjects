module WordSetTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let canMakeWord word set expected = (new Word(word)).CanMakeWordFromSet (new LetterSet(set)) |> should equal expected

[<Test>]
let ``Can make word``() =
    let pairs = [("aabbccdd","cab");
                 ("hello", "hell");
                 ("nnbaaa", "banana");
                 ("a", "a")]
    pairs |> Seq.iter (fun (set,word) -> canMakeWord word set true)

[<Test>]
let ``Cannot make word``() =
    let pairs = [("back","backs");
                 ("hell", "hello");
                 ("a", "b");
                 ("llyon", "nylon")]
    pairs |> Seq.iter (fun (set,word) -> canMakeWord word set false)

[<Test>]
let ``Load all words``() =
    let words = WordLoader.LoadAllWords()
    
    let isWord = words.IsWord "ambiguous"
    let isNotWord = words.IsWord "blaaah"

    let count1 = words.WordsForLength 1 |> List.length
    let count9 = words.WordsForLength 9 |> List.length
    let count25 = words.WordsForLength 25 |> List.length

    words.Count |> should equal 267752
    isWord |> should equal true
    isNotWord |> should equal false
    
    count1 |> should equal 0
    count9 |> should equal 40727
    count25 |> should equal 0
