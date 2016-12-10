namespace ConnectFour.Engine

open System.Diagnostics

type CounterColour = Red | Yellow
    with member this.Flip() = match this with
                              | CounterColour.Red -> CounterColour.Yellow | CounterColour.Yellow -> CounterColour.Red;

type CellState = | OutOfRange
                 | Empty
                 | Full of CounterColour

type CellStateHelper() = 
    static member CellState(colour) = CellState.Full(colour);
    static member EmptyCellState() = CellState.Empty;

type Location = { SlotIndex : int; HeightIndex : int; }
type BoardLocation = { Location : Location; State : CellState }
type CounterLocation = { Location : Location; Colour : CounterColour }

type Direction = Right | Down | DownRight | DownLeft

type CounterChain = { StartLocation : Location; Length : int; Direction : Direction; Colour : CounterColour }

type BoardState(arrCellState : CellState[,], lastPlayedColour : CounterColour) = 
    let SIxMax = 6;
    let HIxMax = 5;

    let fnGetCellState slot height = 
        if(slot < 0 || slot > SIxMax || height < 0 || height > HIxMax) then
            CellState.OutOfRange
        else
            arrCellState.[slot,height];
    
    let fnIsEmpty slot height = fnGetCellState slot height = CellState.Empty
    let fnIsColour colour slot height = fnGetCellState slot height = CellState.Full(colour)
    let fnIsFilled slot height = match fnGetCellState slot height with | CellState.Full(_) -> true | _ -> false

    let fnIsEmptyOrColour colour slot height = (fnIsEmpty slot height) || (fnIsColour colour slot height )
    
    let fnLengthFrom fnCondition = {0..4} |> Seq.takeWhile(fun x -> fnCondition x) |> Seq.length
    
    let fnRightLen startSlot startHeight fnSelect = fnLengthFrom (fun x -> fnSelect (startSlot + x) startHeight)
    let fnLeftLen startSlot startHeight fnSelect = fnLengthFrom (fun x -> fnSelect (startSlot-x) startHeight)
    
    let fnDownLen startSlot startHeight fnSelect = fnLengthFrom (fun x -> fnSelect startSlot (startHeight-x))
    let fnUpLen startSlot startHeight fnSelect = fnLengthFrom (fun x -> fnSelect startSlot (startHeight+x))
    
    let fnDownRightLen startSlot startHeight fnSelect = fnLengthFrom (fun x -> fnSelect (startSlot+x) (startHeight-x))
    let fnUpLeftLen startSlot startHeight fnSelect = fnLengthFrom (fun x -> fnSelect (startSlot-x) (startHeight+x))
    
    let fnDownLeftLen startSlot startHeight fnSelect = fnLengthFrom (fun x -> fnSelect (startSlot-x) (startHeight-x))
    let fnUpRightLen startSlot startHeight fnSelect = fnLengthFrom (fun x -> fnSelect (startSlot+x) (startHeight+x))
    
    let fnGetFourInARow(startSlot,startHeight,colour) =
        if fnRightLen startSlot startHeight (fnIsColour colour)>= 4 then Some(Direction.Right)
        else if fnDownLen startSlot startHeight (fnIsColour colour) >= 4 then Some(Direction.Down)
        else if fnDownRightLen startSlot startHeight (fnIsColour colour) >= 4 then Some(Direction.DownRight)
        else if fnDownLeftLen startSlot startHeight (fnIsColour colour) >= 4 then Some(Direction.DownLeft)
        else None

    let fnCanPlayInSlot slot = ({HIxMax ..(-1).. 0} |> Seq.tryFind(fun x -> arrCellState.[slot, x] = CellState.Empty))
    let fnGetHeightForSlot slot = ({0..1.. HIxMax} |> Seq.tryFind(fun x -> arrCellState.[slot, x] = CellState.Empty))

    new() = BoardState(Array2D.init 7 6 (fun x y -> CellState.Empty), CounterColour.Red)

    member this.PlayAtSlot(play : CounterLocation) = 
        Ensure.IsTrue(fnIsEmpty play.Location.SlotIndex play.Location.HeightIndex, "Cell Should be empty")
        Ensure.IsTrue(play.Location.HeightIndex = 0 || (fnIsFilled play.Location.SlotIndex (play.Location.HeightIndex - 1)), "Cell Below Should be bottom or filled")
        Ensure.IsTrue(play.Colour = this.NextPlay, "Expecting next play Colour")
        
        let arrNewState = arrCellState |> Array2D.copy
        arrNewState.[play.Location.SlotIndex, play.Location.HeightIndex] <- CellState.Full(play.Colour);
        new BoardState(arrNewState, play.Colour)

    member this.PlayAtSlot(slot:int) = 
        let height = fnGetHeightForSlot slot;
        if(height.IsNone) then
            failwith "Cannot play at this location"
        else
            this.PlayAtSlot({Location = {SlotIndex = slot; HeightIndex = height.Value}; Colour = this.NextPlay})

    member this.LastPlayed = lastPlayedColour;
    member this.NextPlay = this.LastPlayed.Flip()
    member this.SlotCount = SIxMax;

    member this.CanPlayInSlot(slot) = (fnCanPlayInSlot slot).IsSome;

    member this.AvailableSlots = {0..SIxMax} |> Seq.filter this.CanPlayInSlot
    
    member this.GetFourInARow(colour) = 
        let seqPositions = {0..SIxMax} |> Seq.collect(fun slot -> {0..HIxMax} |> Seq.filter(fun height -> arrCellState.[slot, height] <> CellState.Empty)
                                                                              |> Seq.map(fun height -> slot, height,fnGetFourInARow(slot,height,colour)))
        let found4 = seqPositions |> Seq.tryFind(fun (_,_,a) -> a.IsSome)
        if found4.IsSome then
            let (index,height,direction) = found4.Value;
            Some({ StartLocation = { SlotIndex = index; HeightIndex = height; }; Length = 4; Direction = direction.Value; Colour = colour })
        else
            None
    
    member this.GetBoardPositionsForColour(playColour : CounterColour) =
        
        let fnGetHeightIndexForSlotIndex slotIndex = {HIxMax..(-1)..0} |> Seq.tryFind(fun x -> arrCellState.[slotIndex, x] <> CellState.Empty)

        let arrLastPlayHeightIndexs = {0..SIxMax} |> Seq.map (fun slot -> let height = fnGetHeightIndexForSlotIndex slot;
                                                                          if height.IsSome then height.Value else -1;  )
                                                  |> Seq.toArray

        //let fnSeqNextHeightIxs slot = { (arrLastPlayHeightIndexs.[slot] + 1) ..(-1).. 0 };
        let fnGetLocation slot height direction length = { StartLocation = { SlotIndex = slot; HeightIndex = height; }; Length = length; Direction = direction; Colour = playColour }
        
        let fnGetScoreIfPlayAt slot height = //This location will be empty
            
            Ensure.IsTrue(fnIsEmpty slot height, "Expecting Empty")
            
            let fnGetLocationPlayAt direction length = fnGetLocation slot height direction length
            
            [|   
                    //Right
                    let rightSpace = fnRightLen slot height (fnIsEmptyOrColour playColour);
                    let leftSpace = fnLeftLen slot height (fnIsEmptyOrColour playColour);

                    if ((rightSpace + leftSpace - 1) >= 4) then //If possible space at least 4
                        let rightLength = fnRightLen (slot+1) height (fnIsColour playColour);
                        let leftLength = fnLeftLen (slot-1) height (fnIsColour playColour);
                        yield fnGetLocationPlayAt Direction.Right (rightLength + leftLength + 1)
                    
                    //Down
                    let downLength = (fnDownLen slot height (fnIsEmptyOrColour playColour));
                    let upSpace = (fnUpLen slot (height+1) (fnIsEmptyOrColour playColour));
                    if ((downLength + upSpace) >= 4) then
                        yield fnGetLocationPlayAt Direction.Down downLength //No Condition -> always yield one option if there is space!

                    //DownRight
                    let downRightSpace = (fnDownRightLen slot height (fnIsEmptyOrColour playColour));
                    let upLeftSpace = (fnUpLeftLen slot height (fnIsEmptyOrColour playColour));
                    if ((downRightSpace + upLeftSpace - 1) >= 4) then
                        let downRightLen = (fnDownRightLen (slot+1) (height-1) (fnIsColour playColour));
                        let upLeftLen = (fnUpLeftLen (slot-1) (height+1) (fnIsColour playColour));
                        yield fnGetLocationPlayAt Direction.DownRight (downRightLen + upLeftLen + 1)

                    //Down Left
                    let downLeftSpace = (fnDownLeftLen slot height (fnIsEmptyOrColour playColour));
                    let upRightSpace = (fnUpRightLen slot height (fnIsEmptyOrColour playColour));
                    if ((downLeftSpace + upRightSpace - 1) >= 4) then
                        let downLeftLen = (fnDownLeftLen (slot-1) (height-1) (fnIsColour playColour));
                        let upRightLen = (fnUpRightLen (slot+1) (height+1) (fnIsColour playColour));
                        yield fnGetLocationPlayAt Direction.DownLeft (downLeftLen + upRightLen + 1)
        |]

        let fnGetScoresForSlot slot =
            let gapheight = arrLastPlayHeightIndexs.[slot] + 1;
            if gapheight <= HIxMax then
                let arrScores = fnGetScoreIfPlayAt slot gapheight
                match arrScores.Length with //If array empty -> create 1 option down at that height (so that branch can be analysed)
                | 0 -> [|(fnGetLocation slot gapheight Direction.Down 1)|]
                | _ -> arrScores
            else
                Array.empty

        {0..SIxMax} |> Seq.collect fnGetScoresForSlot

    member this.GetBoardPositionsForNextPlay() = this.GetBoardPositionsForColour(this.NextPlay)

    member this.Print() = 
        let builder = new System.Text.StringBuilder();

        let fnGetChar ind hei = match arrCellState.[ind,hei] with
                                | CellState.Empty -> '_'
                                | CellState.Full(CounterColour.Red) -> 'R'
                                | CellState.Full(CounterColour.Yellow) -> 'Y'
                                | _ -> 'X'

        let fnPerformForHeight(fnGetFromSlot : int -> char)=
            {0..SIxMax} |> Seq.iter(fun slot -> builder.Append(fnGetFromSlot(slot)) |> ignore
                                                builder.Append(" ") |> ignore)
            builder.Append("\n") |> ignore
        
        {HIxMax..(-1).. 0} |> Seq.iter (fun height -> let fnGetFromSlot slot = fnGetChar slot height;
                                                      fnPerformForHeight(fnGetFromSlot))
        fnPerformForHeight((fun x -> x.ToString().Chars(0)))
        builder.ToString()

    