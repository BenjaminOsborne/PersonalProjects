module WordSetTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain
open System.Collections.Generic

let wordsByLength (words:WordSet) = words.AllWords |> Seq.groupBy (fun x -> x.Length)
                                                   |> Seq.map (fun (k,vals) -> k, vals |> Seq.toList)
                                                   |> Map
let wordsForLength (wordsByLength:Map<int, string list>) n = match wordsByLength.TryFind n with | Some(l) -> l | None -> []

[<Test>]
let ``Load all words``() =
    let words = WordLoader.LoadAllWords()
    
    let isWord = words.IsWord "ambiguous"
    let isNotWord = words.IsWord "blaaah"
    
    let map = wordsByLength words
    let count1 = wordsForLength map 1 |> Seq.length
    let count9 = wordsForLength map 9 |> Seq.length
    let count25 = wordsForLength map 25 |> Seq.length

    let count_add = words.WordsForLengthWithLetters 3 ['a'; 'd'; 'd'] Map.empty |> Seq.length
    let count_add_pin = words.WordsForLengthWithLetters 3 ['a'; 'd'; 'd'] ([(1,'a')] |> Map) |> Seq.length

    words.Count |> should equal 267752
    isWord |> should equal true
    isNotWord |> should equal false
    
    count1 |> should equal 0
    count9 |> should equal 40727
    count25 |> should equal 0
    
    count_add |> should equal 2
    count_add_pin |> should equal 1

[<Test>]
let ``Build stats``() =
    
    let words = WordLoader.LoadAllWords()
    let map = wordsByLength words

    let write (s:string) = System.Console.WriteLine s

    let printMap (map:Map<char,int>) =
        ['a'..'z'] |> Seq.map (fun c -> let len = match map.TryFind c with | Some(n) -> n | None -> 0
                                        (c,len))
                   |> Seq.sortBy (fun (ch,len) -> -len)
                   |> Seq.iter (fun (ch,len) -> write (ch.ToString() + "\t" + len.ToString()))

    let buildStats length =
        let byLength = wordsForLength map length
        let data = [0 .. length-1] |> List.map (fun index -> byLength |> Seq.groupBy (fun s -> s.[index])
                                                                      |> Seq.map (fun (key,items) -> key, items |> Seq.length)
                                                                      |> Map)
        write ("Word Length " + length.ToString())
        data |> List.iteri (fun nx map -> write "\n"
                                          write ("Index " + nx.ToString())
                                          printMap map)
        write "\n"

    buildStats 5
