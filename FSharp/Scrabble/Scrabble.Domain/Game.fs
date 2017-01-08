namespace Scrabble.Domain

type Player = { Name : string }

type PlayerState =
    { Player : Player; Tiles : BagTile list; Plays : WordScore list }
    member this.TotalScore = this.Plays |> Seq.sumBy (fun x -> x.Score)

type GameState =
    { Board: Board; TileBag : TileBag; PlayerStates : PlayerState list }
