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

    member this.Draw count = 
        let draw = min count tiles.Length
        let tileArray = tiles |> List.toArray
        shuffleIndexes tileArray draw
        let drawnTiles = tileArray.[0..draw-1] |> Array.toList
        let bag = new ItemBag<'a>(tileArray.[draw..(tileArray.Length-1)] |> Array.toList)
        (drawnTiles, bag)

type ItemBag = static member Create<'a> a = new ItemBag<'a>(a)

type TileBag = ItemBag<Tile>