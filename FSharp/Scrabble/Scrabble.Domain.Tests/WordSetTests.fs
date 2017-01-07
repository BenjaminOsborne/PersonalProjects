module WordSetTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain
open System.Collections.Generic

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

    let count9a = words.WordsForLengthWithStarting 9 ['a'] |> Seq.length
    let count9b = words.WordsForLengthWithPinned 9 [1, 'b'] |> Seq.length
    let count9c = words.WordsForLengthWithPinned 9 [2, 'c'] |> Seq.length

    let count9ing = words.WordsForLengthWithPinned 9 [(6, 'i'); (7, 'n'); (8, 'g')] |> Set
    let manualCount9ing = words.WordsForLength 9 |> Seq.filter (fun x -> x.Word.EndsWith("ing")) |> Set
    let count9SetsEqual = (new HashSet<Word>(count9ing)).SetEquals(new HashSet<Word>(manualCount9ing))

    words.Count |> should equal 267752
    isWord |> should equal true
    isNotWord |> should equal false
    
    count1 |> should equal 0
    count9 |> should equal 40727
    count25 |> should equal 0
    
    count9a |> should equal 2471
    count9b |> should equal 255
    count9c |> should equal 2179

    count9SetsEqual |> should equal true
    count9ing.Count |> should equal 3276

[<Test>]
let ``Build stats``() =
    
    let words = WordLoader.LoadAllWords()
    
    let write (s:string) = System.Console.WriteLine s

    let printMap (map:Map<char,int>) =
        ['a'..'z'] |> Seq.map (fun c -> let len = match map.TryFind c with | Some(n) -> n | None -> 0
                                        (c,len))
                   |> Seq.sortBy (fun (ch,len) -> -len)
                   |> Seq.iter (fun (ch,len) -> write (ch.ToString() + "\t" + len.ToString()))

    let buildStats length =
        let byLength = words.WordsForLength length
        let data = [0 .. length-1] |> List.map (fun index -> byLength |> Seq.groupBy (fun g -> g.Word.[index])
                                                                      |> Seq.map (fun (key,items) -> key, items |> Seq.length)
                                                                      |> Map)
        write ("Word Length " + length.ToString())
        data |> List.iteri (fun nx map -> write "\n"
                                          write ("Index " + nx.ToString())
                                          printMap map)
        write "\n"

    buildStats 5
