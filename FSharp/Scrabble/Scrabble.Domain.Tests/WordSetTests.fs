module WordSetTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let canMakeWord word set expected = (new Word(word)).CanMakeWordFromSet (LetterSet.FromLetters(set)) |> should equal expected

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

    let count1 = words.WordsForLength 1 |> Seq.length
    let count9 = words.WordsForLength 9 |> Seq.length
    let count25 = words.WordsForLength 25 |> Seq.length

    let count9a = words.WordsForLengthWithStart 9 ['a'] |> Seq.length
    let count9b = words.WordsForLengthWithStart 9 ['b'] |> Seq.length
    let count9c = words.WordsForLengthWithStart 9 ['c'] |> Seq.length
    let count9abc = words.WordsForLengthWithStart 9 ['a'; 'b'; 'c'] |> Seq.length

    words.Count |> should equal 267752
    isWord |> should equal true
    isNotWord |> should equal false
    
    count1 |> should equal 0
    count9 |> should equal 40727
    count25 |> should equal 0
    
    count9a |> should equal 2471
    count9b |> should equal 2356
    count9c |> should equal 3737
    count9abc |> should equal (2471+2356+3737)
