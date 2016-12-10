module SudokuDefinition

open System.Diagnostics

type Tile (listValues : List<int>) = 
    
    let m_listValueOptions = listValues
    
    //Constructors
    new(tileValue:int) = new Tile([tileValue])
    new() = new Tile([1..9])
    
    //Methods
    member this.HasKnownValue = m_listValueOptions.Length = 1
    
    member this.Value = 
        match this.HasKnownValue with
        | true -> Some(m_listValueOptions.Head)
        | false -> None

    member this.PossibleValues = m_listValueOptions

type TilePosition (boardTile : Tile, nRowIndex : int, nColIndex : int) = 
    member this.BoardTile = boardTile;
    member this.RowIndex = nRowIndex;
    member this.ColIndex = nColIndex;

type SudokuBoard(seqInitialPositions : seq<TilePosition>) =
    
    let nSize = 9;
    let arrBoardTiles = Array2D.init nSize nSize (fun i j -> new TilePosition(new Tile(), i, j))
    do
        seqInitialPositions |> Seq.iter (fun x -> arrBoardTiles.[x.RowIndex, x.ColIndex] <- x)

    let fnTileToValue (enTiles : seq<TilePosition>) = enTiles |> Seq.filter (fun x -> x.BoardTile.HasKnownValue) |> Seq.map (fun x -> x.BoardTile.Value.Value)

    let fnFindMissing(enKnowValues : seq<int>) = 
        let listKnown = enKnowValues |> Seq.toList
        {1..9} |> Seq.filter (fun x -> listKnown |> List.forall (fun y -> y <> x))
    
    let SubSet(seq1 : seq<int>, seq2 : seq<int>) =  seq1 |> Seq.where(fun x -> seq2 |> Seq.where (fun y -> x = y) |> Seq.isEmpty = false)

    let fnOnlyIn1(seq1:seq<int>, seq2:seq<int>) = seq1 |> Seq.where(fun x -> seq2 |> Seq.where(fun y -> x = y) |> Seq.isEmpty)

    member this.Width = nSize
    member this.Height = nSize

    member this.TileAt(nRowIndex : int, nColIndex : int) = arrBoardTiles.[nRowIndex, nColIndex]

    member this.SetValue(nRow:int, nCol:int, nValue:int) = this.SetTile(new TilePosition(new Tile(nValue), nRow, nCol))
    member this.SetTile(tilePos:TilePosition) = arrBoardTiles.[tilePos.RowIndex, tilePos.ColIndex] <- tilePos

    member this.KnownValue(nRowIndex:int, nColIndex:int) = this.TileAt(nRowIndex,nColIndex).BoardTile.HasKnownValue
    member this.ValueAt(nRowIndex:int, nColIndex:int) = this.TileAt(nRowIndex,nColIndex).BoardTile.Value

    member this.RowKnownValues(nRowIndex : int) = {0..8} |> Seq.map (fun x -> arrBoardTiles.[nRowIndex, x]) |> fnTileToValue

    member this.ColKnownValues(nColIndex : int) = {0..8} |> Seq.map (fun x -> arrBoardTiles.[x, nColIndex]) |> fnTileToValue

    member this.SecKnownValues(nSecIndex : int) = //From 0 to 8
        Debug.Assert(nSecIndex >= 0 && nSecIndex <= 8)
        let nRowStart = (nSecIndex - nSecIndex % 3)
        let nColStart = (nSecIndex % 3) * 3
        {nRowStart..nRowStart+2} |> Seq.collect (fun nRow -> {nColStart..nColStart+2} |> Seq.map (fun nCol -> arrBoardTiles.[nRow, nCol]))
                                 |> fnTileToValue

    member this.RowAvailableValues(nRowIndex : int) = fnFindMissing (this.RowKnownValues nRowIndex)
    member this.ColAvailableValues(nColIndex : int) = fnFindMissing (this.ColKnownValues nColIndex)
    member this.SecAvailableValues(nSecIndex : int) = fnFindMissing (this.SecKnownValues nSecIndex)

    member this.RowComplete(nRowIndex : int) = this.RowKnownValues(nRowIndex) |> Seq.length = nSize
    member this.ColComplete(nColIndex : int) = this.ColKnownValues(nColIndex) |> Seq.length = nSize
    member this.SecComplete(nSecIndex : int) = this.SecKnownValues(nSecIndex) |> Seq.length = nSize
    
    member this.SecIndexFromRowAndCol(nRowIndex : int, nColIndex : int) = (nRowIndex/3) * 3 + nColIndex/3

    member this.CellOnlyAvailableValues(nRowIndex : int, nColIndex : int) =
        let existing = this.TileAt(nRowIndex,nColIndex)
        if existing.BoardTile.HasKnownValue then 
            Seq.singleton existing.BoardTile.Value.Value
        else
            let rowAvailable = this.RowAvailableValues nRowIndex
            let colAvailable = this.ColAvailableValues nColIndex |> Seq.toList
            let secAvailable = this.SecAvailableValues (this.SecIndexFromRowAndCol(nRowIndex,nColIndex)) |> Seq.toList
            SubSet(SubSet(rowAvailable,colAvailable), secAvailable)
    
    member this.CellExtendedAvailableValues(nRowIndex : int, nColIndex : int) =
        let nSecIndex = this.SecIndexFromRowAndCol(nRowIndex, nColIndex)
        let simpleAvailable = this.CellOnlyAvailableValues(nRowIndex, nColIndex) |> Seq.toList

        let indexesExclude n = {0..8} |> Seq.where (fun x -> x <> n)
        let rowOthers = indexesExclude nRowIndex |> Seq.collect (fun othRow -> this.CellOnlyAvailableValues(othRow,nColIndex)) |> Seq.distinct
        let colOthers = indexesExclude nColIndex |> Seq.collect (fun othCol -> this.CellOnlyAvailableValues(nRowIndex,othCol)) |> Seq.distinct
        
        let secOthers = indexesExclude nRowIndex |> Seq.collect (fun nR ->
                        indexesExclude nColIndex |> Seq.where (fun nC -> nSecIndex = this.SecIndexFromRowAndCol(nR,nC))
                                                 |> Seq.collect (fun nC -> this.CellOnlyAvailableValues(nR,nC)))
                                                 |> Seq.distinct
        
        let rowFiltered = fnOnlyIn1(simpleAvailable,rowOthers)
        let colFiltered = fnOnlyIn1(simpleAvailable,colOthers)
        let secFiltered = fnOnlyIn1(simpleAvailable,secOthers)
        let result = SubSet(SubSet(rowFiltered,colFiltered),secFiltered) |> Seq.toList
        if result.Length = 0 then
            simpleAvailable
        else
            result

type BoardProcessor() = 
    member this.ResolveOrder1(board:SudokuBoard, nCount:int) =

        let toSet = {0..8} |> Seq.collect(fun nR -> {0..8} |> Seq.where (fun nC -> board.KnownValue(nR,nC) = false)
                                                           |> Seq.map (fun nC -> nR, nC, board.CellOnlyAvailableValues(nR,nC) |> Seq.toList))
                                                           |> Seq.where (fun (r,c,vals) -> vals.Length = 1)
                                                           |> Seq.map (fun (r,c,vals) -> r,c,vals.Head)
                                                           |> Seq.toList

        toSet |> Seq.iter(fun (r,c,tileVal) -> board.SetValue(r,c,tileVal))
        
        match toSet.Length with
        | 0 -> nCount
        | _ -> this.ResolveOrder1(board,nCount+1)

    member this.PrintBoard(board:SudokuBoard) = 
        let builder = new System.Text.StringBuilder()
        let addText(c:string) = ignore(builder.Append(c))
        for nR in {0..8} do
            for nC in {0..8} do
                match board.KnownValue(nR,nC) with
                | true -> addText(board.ValueAt(nR,nC).Value.ToString())
                | false -> addText(" ")
                addText(" ")
            ignore(builder.AppendLine());
        builder.ToString()

//    let fnProcessAcross(board : SudokuBoard) = 
//    
//        let fnProcessRow nRowIndex = 
//            let possibleValues = //write method...