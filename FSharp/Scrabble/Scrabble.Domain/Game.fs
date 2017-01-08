namespace Scrabble.Domain

type Player = { Name : string }

type PlayerState =
    { Player : Player; Tiles : BagTile list; Plays : WordScore list }
    member this.TotalScore = this.Plays |> Seq.sumBy (fun x -> x.Score)

type GameState = { Board: Board; TileBag : TileBag; PlayerStates : PlayerState list; }

type GameMoveData = { WordSet : WordSet; Board : Board; Player : Player; Tiles : BagTile list }

type GameMoveResult = { WordScore : WordScore; UsedTiles: BagTile list; }

type IGameMoveProvider =
    abstract member GetNextMove: GameMoveData -> Option<GameMoveResult>

type ScrabbleGame (words : WordSet, handSize:int, initialState : GameState ) =
    
    let getNextState gameState (moveProvider : IGameMoveProvider) = 
        let playerState = gameState.PlayerStates.Head
        let player = playerState.Player
        let board = gameState.Board

        let drawCount = (handSize - playerState.Tiles.Length)
        let (bts, newBag) = gameState.TileBag.Draw drawCount
        let fullTileList = List.append playerState.Tiles bts

        let moveData = { WordSet = words; Board = board; Player = player; Tiles = fullTileList}
        let optMove = moveProvider.GetNextMove moveData
        let nextData = match optMove with
                       | Some(move) -> let plays = move.WordScore.Locations |> List.map (fun (loc, tile) -> { Location = loc; Piece = tile })
                                       let updateBoard = board.Play plays
                                       let playedTiles = move.UsedTiles |> Set
                                       let remaining = fullTileList |> List.filter (fun x -> playedTiles.Contains x = false)
                                       (move.WordScore, updateBoard, remaining)
                       | None -> let emptyScore = { Word = ""; Locations = []; Score = 0 }
                                 (emptyScore, board, fullTileList)

        let (play, nextBoard, nextTiles) = nextData
        let newState = { Player = player; Tiles = nextTiles; Plays = play :: playerState.Plays }
        let newStates = List.append gameState.PlayerStates.Tail [newState]
        { Board = nextBoard; TileBag = newBag; PlayerStates = newStates; }

    member this.PlayGame (moveProvider : IGameMoveProvider) =
        //Seq.initInfinite (fun x -> x) |> Seq.scan ()
        
        //let getNext (tileBag:TileBag) (states: PlayerState list) player =
        //    let (dts,nxtBag) = tileBag.Draw 7
        //    let state = { Player = player; Tiles = dts; Plays = []}
        //    (nxtBag, state::states)

        let mutable gameState = initialState
        while true do
            gameState <- getNextState gameState moveProvider
        0
