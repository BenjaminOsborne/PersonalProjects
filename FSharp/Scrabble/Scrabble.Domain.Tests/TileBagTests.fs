module TileBagTests

open NUnit.Framework
open FsUnit
open Scrabble.Domain

let bagDraw (bag:ItemBag<'a>) draw = bag.DrawCreate draw (fun l -> ItemBag.Create(l))

[<Test>]
let ``When draw from empty bag``() =
    let bag = ItemBag.Create([])
    
    let assertDrawEmpty draw =
        let (tl,b) = bagDraw bag draw
        tl.Length |> should equal 0
        b.Tiles |> should equal bag.Tiles

    assertDrawEmpty 0
    assertDrawEmpty 1
    assertDrawEmpty 10

[<Test>]
let ``When draw all tiles from bag``() =
    let bag = ItemBag.Create([""; "Hello"; "More"])
    
    let assertDrawAll draw =
        let (tl,b) = bagDraw bag draw
        tl.Length |> should equal bag.Tiles.Length
        (tl |> List.sortBy (fun x -> x)) |> should equal (bag.Tiles |> List.sortBy (fun x -> x))
        b.Tiles.Length |> should equal 0

    assertDrawAll 3
    assertDrawAll 4
    assertDrawAll 10

[<Test>]
let ``When draw some tiles from bag``() =
    let bag = ItemBag.Create(["1"; "2"; "3"; "4"])

    let assertDraw draw =
        let (tl,b) = bagDraw bag draw
        tl.Length |> should equal draw
        b.Tiles.Length |> should equal (bag.Tiles.Length - draw)

    {0..4} |> Seq.iter assertDraw

    //Print...
    {0..10} |> Seq.iter (fun _ ->
        {0..4} |> Seq.iter (fun n ->
            let (tl,b) = bagDraw bag n
            let print = tl |> Seq.map (fun x -> x) |> Seq.fold (fun agg x -> agg + " " + x) ""
            printfn "%s" print))