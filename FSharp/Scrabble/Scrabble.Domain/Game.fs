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
    interface IGameMoveProvider with
        member this.GetNextMove (data:GameMoveData) =
            let tileHand = new TileHand([]) //Todo: Fill in and handle blank tiles!
            let possible = (new BoardSpaceAnalyser()).GetPossibleScoredPlays data.Board tileHand data.WordSet
            let played = match possible with
                         | head :: tail -> let topScore = head.WordScores.Head
                                           let locationPlays = topScore.Locations;
                                           let plays = locationPlays |> List.map (fun (loc, tile) -> { Location = loc; Piece = tile })
                                           let usedTiles = [] //Todo: Pull out used tiles
                                           Play({ WordScore = topScore; UsedTiles = usedTiles })
                         | _ -> Complete
            Complete