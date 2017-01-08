namespace Scrabble.Domain

type Player = { Name : string }

type GameMoveData = { WordSet : WordSet; Board : Board; Player : Player; Tiles : BagTile list }

type GameMovePlay = { WordScore : WordScore; UsedTiles: BagTile list; }

type GameMove = | Play of GameMovePlay
                | Switch of BagTile list
                | Complete

type PlayerState = { Player : Player; Tiles : BagTile list; Plays : GameMove list }

type GameState = { Board: Board; TileBag : TileBag; PlayerStates : PlayerState list; }

type IGameMoveProvider =
    abstract member GetNextMove: GameMoveData -> GameMove

type ScrabbleGame (words : WordSet, handSize:int, initialState : GameState ) =
    
    let removeFrom initial remove =
        let removeSet = remove |> Set
        initial |> List.filter (fun x -> removeSet.Contains x = false)

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
        let getNext state = getNextState state moveProvider

        //Seq.initInfinite (fun x -> x) |> Seq.scan ()
        
        //let getNext (tileBag:TileBag) (states: PlayerState list) player =
        //    let (dts,nxtBag) = tileBag.Draw 7
        //    let state = { Player = player; Tiles = dts; Plays = []}
        //    (nxtBag, state::states)

        let mutable gameState = initialState
        while true do
            gameState <- getNext gameState
        0
