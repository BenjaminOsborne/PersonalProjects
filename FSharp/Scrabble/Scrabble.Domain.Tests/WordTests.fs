module WordTests

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
    let words = WordLoader.LoadAllWords
    words.Count |> should equal 267752