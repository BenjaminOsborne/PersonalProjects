namespace ConnectFour.Engine

open ConnectFour.Engine

type CounterPlay = { SlotIndex : int; HeightIndex : int; Colour : CounterColour}

type PlayValue = | Win | Value of int

type PlayValueHelper() = 
    static member Win() = PlayValue.Win;
    static member Value(value) = PlayValue.Value(value);

type BoardPlay = { Board : BoardState; PlayPosition:Location; PlayValue : PlayValue }

type PlayLeaf = { FinalPlay : BoardPlay; Depth : int }
type PlayBranch = { LastPlay : BoardPlay; Depth : int; Tree : PlayTree list; }
and PlayTree = | Leaf of PlayLeaf
               | Branch of PlayBranch

type PlayTreeDepth = { Depth:int; Play:BoardPlay; PreviousPlay:BoardPlay }
    with member this.Colour = this.Play.Board.LastPlayed
    
type ColourScore = { Mine : int; Theirs : int;}
type LeafState = | Win
                 | Lose
                 | Score of ColourScore
        with member this._IsScore = match this with | Score(_) -> true | _ -> false
             member this.CombinePrefer ls2 p unP = //p = Prefered, unP = Unprefered
                    match (this,ls2) with
                    | (LeafState.Score(sc1),LeafState.Score(sc2)) -> LeafState.Score({Mine = sc1.Mine + sc2.Mine; Theirs = sc1.Theirs + sc2.Theirs})
                    | (a,_) when a = p -> p;
                    | (_,a) when a = p -> p;
                    | (_, Score(sc2)) -> LeafState.Score(sc2);
                    | (Score(sc1),_) -> LeafState.Score(sc1);
                    | _ -> unP

type LeafStateHelper() = 
    static member Win = LeafState.Win
    static member Lose = LeafState.Lose

type LeafValue = { State : LeafState; Depth : int }
        with member this.CombinePreferWin lv2 =     let picked = this.State.CombinePrefer lv2.State LeafState.Win LeafState.Lose
                                                    if (picked = this.State) then this else lv2
             member this.CombinePreferLose lv2 =    let picked = this.State.CombinePrefer lv2.State LeafState.Lose LeafState.Win
                                                    if (picked = this.State) then this else lv2

type BoardPlayScore = { NextPlay : BoardPlay; Score : LeafValue }
type NextBoardPlay = { BestPlay : BoardPlayScore; AllPossible : BoardPlayScore[]}

type PlayBranchHelper() = 
    static member FlattenBoardPlays(tree:PlayTree, previousPlay) = 
        let rec fnGetPlays tr depth prevPlay =
            match tr with
            | PlayTree.Leaf(a) -> seq { yield { Depth = depth; Play = a.FinalPlay; PreviousPlay = prevPlay} }
            | PlayTree.Branch(b) -> b.Tree |> Seq.collect(fun x -> fnGetPlays x (depth+1) b.LastPlay)
        fnGetPlays tree 1 previousPlay

type Calculator() = 
    let fnGetPlayValue length = if (length >=4) then PlayValue.Win else PlayValue.Value(length)

    let fnGetNextPlay(board:BoardState, chain : CounterChain) = 
        let newBoard = board.PlayAtSlot({ Location = chain.StartLocation; Colour = chain.Colour })
        { Board = newBoard; PlayPosition = chain.StartLocation; PlayValue = fnGetPlayValue chain.Length }

    let fnSelectFromBoardPlays(seqPlays : seq<CounterChain>, fnSelect)= 
        let arrNextPositions = seqPlays |> Seq.toArray;
        let winPosition = arrNextPositions |> Seq.tryFind (fun x -> x.Length >= 4)
        if (winPosition.IsSome) then
            seq { yield fnSelect winPosition.Value }
        else
            arrNextPositions |> Seq.groupBy(fun x -> x.StartLocation.SlotIndex)
                             |> Seq.map(fun (key,vals) -> let best = vals |> Seq.sortBy (fun x -> -x.Length) |> Seq.head
                                                          fnSelect best)

    member this.GetNextPlays(board : BoardState) = fnSelectFromBoardPlays(board.GetBoardPositionsForNextPlay(), (fun x -> fnGetNextPlay(board,x)))
    
    member this.GetPlayTree(board : BoardState, numberPlays : int) =
        let rec fnGetPlays brd totMoves depth =
            let seqPlays = this.GetNextPlays(brd);
            seqPlays |> Seq.map(fun play -> match (totMoves, play.PlayValue) with
                                            | a,b when a <= 1 || b = PlayValue.Win -> PlayTree.Leaf({ FinalPlay = play; Depth = depth })
                                            | _ -> let seqNextPlays = fnGetPlays play.Board (totMoves-1) (depth+1);
                                                   PlayTree.Branch({ LastPlay = play; Depth = depth; Tree = seqNextPlays |> Seq.toList }))
        fnGetPlays board ((numberPlays * 2) - 1) 1

    //Approach:
    //Walk to end of tree.
    //Collapse final leaves into 1 score -> step back.
    //Collapse next level leaves into score and compare scores. Collapse to 1 score -> step back...
        //Where "collapse" means -> crunch all slot values into 1 value for that layer colour

     member this.GetNextPlayScores(board : BoardState, numberPlays : int) = 
        let myColour = board.NextPlay;
        let theirColour = board.LastPlayed;
        
        let fnScoreBoardForColour(brd : BoardState, col) = 
            let arrSome = fnSelectFromBoardPlays(brd.GetBoardPositionsForColour(col), (fun x -> x)) |> Seq.toArray
            let weightedSum = arrSome
                              |> Seq.sumBy(fun x -> x.Length * x.Length * x.Length * x.Length)
            (10000 * weightedSum) / (brd.SlotCount * 4 * 4 * 4 * 4); //Percentage of max possible

        let fnGetLeafState brdPlay = 
            match brdPlay.PlayValue with
            | PlayValue.Win when brdPlay.Board.LastPlayed = myColour -> LeafState.Win
            | PlayValue.Win -> LeafState.Lose
            | _ -> LeafState.Score({ Mine = fnScoreBoardForColour(brdPlay.Board,myColour); Theirs = fnScoreBoardForColour(brdPlay.Board,theirColour) })
        
        let fnGetLeafValue brdPlay depth = { State = fnGetLeafState brdPlay; Depth = depth }

        let fnPickCombine(lv:LeafValue, colour) = if (colour = theirColour) then lv.CombinePreferWin else lv.CombinePreferLose

        let rec fnValueTree tree =
            let rec fnCombineList(currentValue:LeafValue, listPlayTree, lpColour) =
                match listPlayTree with
                | [] -> currentValue
                | _ -> let combined = fnPickCombine(currentValue, lpColour)(fnValueTree listPlayTree.Head)
                       fnCombineList(combined, listPlayTree.Tail, lpColour)
                
            match tree with
            | Leaf(bp) -> let fin = fnGetLeafValue bp.FinalPlay bp.Depth
                          fin
            | Branch(bch) -> let slot = bch.LastPlay.PlayPosition.SlotIndex
                             let lpColour = bch.LastPlay.Board.LastPlayed
                             if (bch.Tree.Length > 0) then
                                let fin = fnCombineList(fnValueTree bch.Tree.Head, bch.Tree.Tail, lpColour)
                                fin
                             else
                                { State = LeafState.Score({ Mine = 0; Theirs = 0 }); Depth = System.Int32.MaxValue; }   

        let fnGetBoardPlay tree = match tree with | Leaf(bp) -> bp.FinalPlay | Branch(bch) -> bch.LastPlay
        let fnGetSlotIndex tree = (fnGetBoardPlay tree).PlayPosition.SlotIndex
        
        let arrPlayTrees = this.GetPlayTree(board, numberPlays) |> Seq.toArray
        let arrResult = arrPlayTrees |> Seq.map(fun x -> { NextPlay = (fnGetBoardPlay x); Score = (fnValueTree x)}) |> Seq.toArray
        arrResult;

    member this.GetNextBestPlay(board : BoardState, numberPlays : int) = 
        
        let fnRankSlot boardScore = 
            let slot = boardScore.NextPlay.PlayPosition.SlotIndex;
            let mid = boardScore.NextPlay.Board.SlotCount / 2
            -System.Math.Abs(slot - mid)

        let fnGetScore boardScore =
            let slotScore = fnRankSlot boardScore
            match boardScore.Score.State with
            | LeafState.Win -> (3,1,slotScore)
            | LeafState.Score(a) -> (2, a.Mine - a.Theirs,slotScore)
            | LeafState.Lose -> (1,boardScore.Score.Depth,slotScore)

        let fnCompare boardScore1 boardScore2 =
            let (a1,b1,c1) = fnGetScore boardScore1
            let (a2,b2,c2) = fnGetScore boardScore2
            if (a1 > a2) then -1 else if(a1 < a2) then 1
            else if (b1 > b2) then -1 else if(b1 < b2) then 1
            else if (c1 > c2) then -1 else if(c1 < c2) then 1
            else 0
        
        let arrPlayScores = this.GetNextPlayScores(board, numberPlays);
        match arrPlayScores.Length with
        | 0 -> None
        | _ -> let best = arrPlayScores |> Array.sortWith fnCompare |> Seq.head
               Some({ BestPlay = best; AllPossible = arrPlayScores})