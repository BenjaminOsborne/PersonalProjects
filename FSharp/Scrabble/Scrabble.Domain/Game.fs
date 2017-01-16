namespace Scrabble.Domain

type Player = { Name : string }

type GameMoveData = { WordSet : WordSet; Board : Board; Player : Player; Tiles : BagTile list }

type GameMovePlay = { WordScore : WordScore; UsedTiles: BagTile list; }

type GameMove = | Play of GameMovePlay
                | Switch of BagTile list
                | Complete

type PlayerState =
    { Player : Player; Tiles : BagTile list; Plays : GameMove list }
    static member Empty p = { Player = p; Tiles = []; Plays = [] }

type GameState = { Board: Board; TileBag : TileBag; PlayerStates : PlayerState list; }

type IGameMoveProvider =
    abstract member GetNextMove: GameMoveData -> GameMove

type ScrabbleGame (words : WordSet, handSize:int, initialState : GameState ) =
    
    let removeFrom initial remove =
        let mapInit = initial |> Seq.groupBy (fun x -> x) |> Seq.map (fun (key,vals) -> key, vals |> Seq.length) |> Map
        
        let finalMap = remove |> Seq.fold (fun (agg:Map<'a,int>) x -> let count = agg.Item x
                                                                      agg.Add(x,count-1)) mapInit
        let valid = finalMap |> Seq.forall (fun kvp -> kvp.Value >= 0)
        if valid then
            finalMap |> Seq.map (fun kvp -> [1 .. kvp.Value] |> Seq.map (fun _ -> kvp.Key))
                     |> Seq.collect (fun x -> x) |> Seq.toList
        else
            failwith "Could not find tile"

    let getNextState gameState (moveProvider : IGameMoveProvider) = 
        let playerState = gameState.PlayerStates.Head
        let player = playerState.Player
        let board = gameState.Board

        let drawCount = (handSize - playerState.Tiles.Length)
        let (bts, newBag) = gameState.TileBag.Draw drawCount
        let fullTileList = List.append playerState.Tiles bts

        let moveData = { WordSet = words; Board = board; Player = player; Tiles = fullTileList}
        let moveResult = moveProvider.GetNextMove moveData
        let nextData = match moveResult with
                       | Play(move) ->      let plays = move.WordScore.Locations |> List.map (fun (loc, tile) -> { Location = loc; Piece = tile })
                                            let updateBoard = board.Play plays
                                            let remaining = removeFrom fullTileList move.UsedTiles
                                            (updateBoard, remaining)
                       | Switch(switch) ->  let remaining = removeFrom fullTileList switch
                                            (board, remaining)
                       | Complete ->        (board, fullTileList)

        let (nextBoard, nextTiles) = nextData
        let newState = { Player = player; Tiles = nextTiles; Plays = moveResult :: playerState.Plays }
        let newStates = List.append gameState.PlayerStates.Tail [newState]
        { Board = nextBoard; TileBag = newBag; PlayerStates = newStates; }

    member this.PlayGame (moveProvider : IGameMoveProvider) =
        
        let playerCount = initialState.PlayerStates.Length

        let shouldContinue (state : GameState) =
            let latest = state.PlayerStates |> Seq.map (fun x -> match x.Plays with | head::tail -> [head] | [] -> [])
                                            |> Seq.collect (fun x -> x) |> Seq.toList
            if latest.Length < playerCount then
                true
            else
                let shouldStop = latest |> Seq.forall (fun x -> match x with
                                                                | Play(p) -> false
                                                                | Switch(s) -> true //For now, if everyone switches -> game over
                                                                | Complete -> true)
                (shouldStop = false)
        
        let states = Seq.initInfinite (fun x -> x) |> Seq.scan (fun state _ -> getNextState state moveProvider) initialState
        let gameStates = states |> Seq.takeWhileAndNext shouldContinue |> Seq.toList
        gameStates |> Seq.last


type GameMoveProvider() =
    let buildNext (current:(BagTile * Tile) list list) (bagTile : BagTile) =
        let popLetterOnCurrent c =
            let nxtPair = (bagTile, { Letter = c; Value = bagTile.Value })
            match current with
            | [] -> [[nxtPair]]
            | _  -> current |> List.map (fun cur -> (nxtPair :: cur))
        
        match bagTile.TileLetter with
        | Letter(c) -> popLetterOnCurrent c
        | Blank     -> ['a'..'z'] |> List.map (fun c -> popLetterOnCurrent c)
                                  |> Seq.collect (fun x -> x) |> Seq.toList

    let getAllTileSets (tiles:BagTile list) =
        tiles |> List.fold (fun agg bt -> buildNext agg bt) []
    
    let getBestPlay (data:GameMoveData) (bagTilePairs:(BagTile * Tile) list) =
        let tileHand = new TileHand(bagTilePairs |> List.map (fun (_,t) -> t))
        let possible = (new BoardSpaceAnalyser()).GetPossibleScoredPlays data.Board tileHand data.WordSet
        match possible with
           | head :: tail -> let topScore = head.WordScores.Head
                             let usedTiles = topScore.Locations |> List.map (fun (loc,tle) -> tle)
                             let usedBagTiles = usedTiles |> List.map (fun ut -> bagTilePairs |> List.find (fun (_,tl) -> ut = tl))
                                                          |> List.map (fun (bt,_) -> bt)
                             Play({ WordScore = topScore; UsedTiles = usedBagTiles })
           | _ -> Complete

    interface IGameMoveProvider with
        member this.GetNextMove data =
            let allSets = getAllTileSets data.Tiles
            match allSets with
            | [] -> Complete
            | _ -> allSets |> List.map (fun set -> getBestPlay data set)
                           |> Seq.sortBy (fun x -> match x with | Play(a) -> -a.WordScore.Score | _ -> 1)
                           |> Seq.head
            