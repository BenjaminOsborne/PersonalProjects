namespace Scrabble.Domain

type ItemBag<'a>(tiles : 'a list) = 
    
    let random = new System.Random()

    let switchIndex (arr: _[]) x1 x2 =
        let local = arr.[x1]
        arr.[x1] <- arr.[x2]
        arr.[x2] <- local
    
    let min (a:int) (b:int) = System.Math.Min(a,b)

    let shuffleIndexes arr count =
        {0..(count-1)} |> Seq.iter (fun n -> switchIndex arr n (random.Next(0, arr.Length)))

    member this.Tiles = tiles

    member this.DrawCreate count create = 
        let draw = min count tiles.Length
        let tileArray = tiles |> List.toArray
        shuffleIndexes tileArray draw
        let drawnTiles = tileArray.[0..draw-1] |> Array.toList
        let bag = create (tileArray.[draw..(tileArray.Length-1)] |> Array.toList)
        (drawnTiles, bag)

type ItemBag = static member Create<'a> a = new ItemBag<'a>(a)

type TileBag(items) =
    inherit ItemBag<BagTile>(items)

    member this.DrawFromLetter (letter:BagTileLetter) =
        let (item, remaining) = items |> List.removeFirstWith (fun x -> x.TileLetter.Equals(letter))
        (new TileBag(remaining), item)

    member this.Draw count = this.DrawCreate count (fun l -> new TileBag(l))
