module SudokuTests

open NUnit.Framework
open FsUnit
open SudokuDefinition

type CharParseRecord = {Symb:char; Row:int; Col:int }

let fnShouldContain (actualVals : seq<int>, expectedVals : seq<int>) =
    actualVals |> Seq.length |> should equal (expectedVals |> Seq.length)
    for x in expectedVals do
        actualVals |> should contain x

let fnMapBoard(arrInitials:char[][]) =  
    let listTiles = {0..8} |> Seq.collect (fun nRow -> {0..8} |> Seq.map (fun nCol -> { Symb = arrInitials.[nRow].[nCol]; Row = nRow; Col = nCol }))
                           |> Seq.filter (fun (x:CharParseRecord) -> x.Symb <> ' ')
                           |> Seq.map (fun x -> new TilePosition(new Tile(System.Int32.Parse(x.Symb.ToString())), x.Row, x.Col))
                           |> Seq.filter (fun tile -> tile.BoardTile.Value.Value <> 0)
                           |> Seq.toList
    let myBoard = SudokuBoard(listTiles)
    myBoard

[<TestFixture>] 
type ``Given a Basic Sudoku Board`` () =
    let arrInitials = [|[|' ';' ';'3';  '5';' ';' ';  ' ';' ';' ';|];
                        [|' ';' ';'4';  ' ';' ';' ';  ' ';' ';' ';|];
                        [|' ';' ';' ';  '6';' ';' ';  ' ';' ';' ';|];
                                                          
                        [|' ';' ';' ';  ' ';'8';' ';  ' ';' ';' ';|];
                        [|' ';' ';' ';  '9';' ';' ';  ' ';' ';' ';|];
                        [|' ';' ';' ';  ' ';' ';' ';  ' ';' ';' ';|];
                                                          
                        [|'1';' ';' ';  ' ';' ';' ';  '9';' ';' ';|];
                        [|' ';'8';' ';  ' ';' ';' ';  ' ';'7';' ';|];
                        [|'2';' ';' ';  ' ';' ';' ';  ' ';' ';'3';|]|];
    let myBoard = fnMapBoard arrInitials

    [<Test>]
    member this.``when getting orders`` () =
        myBoard.SecIndexFromRowAndCol(0,0) |> should equal 0
        myBoard.SecIndexFromRowAndCol(8,8) |> should equal 8
        myBoard.SecIndexFromRowAndCol(1,2) |> should equal 0
        myBoard.SecIndexFromRowAndCol(3,5) |> should equal 4
        myBoard.SecIndexFromRowAndCol(4,7) |> should equal 5

    [<Test>]
    member this.``when getting known values`` () =
        fnShouldContain(myBoard.SecKnownValues 0, [3;4])
        fnShouldContain(myBoard.SecKnownValues 1, [5;6])
        fnShouldContain(myBoard.SecKnownValues 5, [])
        fnShouldContain(myBoard.SecKnownValues 8, [9;7;3])
        
        fnShouldContain(myBoard.RowKnownValues 0, [3;5])
        fnShouldContain(myBoard.RowKnownValues 3, [8])
        fnShouldContain(myBoard.RowKnownValues 4, [9])

        fnShouldContain(myBoard.ColKnownValues 0, [1;2])
        fnShouldContain(myBoard.ColKnownValues 3, [5;6;9])
        fnShouldContain(myBoard.ColKnownValues 8, [3])

    [<Test>]
    member this.``when getting missing values`` () =
        fnShouldContain(myBoard.RowAvailableValues 0, [1;2;  4;  6;7;8;9;])
        fnShouldContain(myBoard.RowAvailableValues 6, [  2;3;4;5;6;7;8;  ])
        fnShouldContain(myBoard.RowAvailableValues 8, [1;    4;5;6;7;8;9;])
        
        fnShouldContain(myBoard.ColAvailableValues 0, [    3;4;5;6;7;8;9;])
        fnShouldContain(myBoard.ColAvailableValues 3, [1;2;3;4;    7;8;  ])
        
        fnShouldContain(myBoard.SecAvailableValues 3, [1;2;3;4;5;6;7;8;9;])
        fnShouldContain(myBoard.SecAvailableValues 8, [1;2;  4;5;6;  8;  ])

    [<Test>]
    member this.``when getting possible values`` () =
        fnShouldContain(myBoard.CellOnlyAvailableValues(0,0), [6;7;8;9;])
        fnShouldContain(myBoard.CellOnlyAvailableValues(4,3), [9;])
        fnShouldContain(myBoard.CellOnlyAvailableValues(4,7), [1;2;3;4;5;6;8;])
        fnShouldContain(myBoard.CellOnlyAvailableValues(7,2), [5;6;9])

[<TestFixture>] 
type ``Given a Simple Sudoku Board`` () =
    let arrInitials = [|[|' ';'4';'6';' ';' ';'2';'3';'5';'7';|];
                        [|' ';' ';' ';'7';'4';' ';'2';'9';' ';|];
                        [|' ';' ';'2';' ';' ';' ';'6';' ';'8';|];
                          
                        [|' ';' ';'5';' ';'7';'9';'1';' ';'3';|];
                        [|' ';' ';' ';' ';' ';' ';' ';' ';' ';|];
                        [|'4';' ';'1';'2';'6';' ';'8';' ';' ';|];
                          
                        [|'5';' ';'4';' ';' ';' ';'9';' ';' ';|];
                        [|' ';'8';'9';' ';'5';'3';' ';' ';' ';|];
                        [|'6';'2';'3';'1';' ';' ';'7';'8';' ';|]|];
    let myBoard = fnMapBoard arrInitials
    let processor = new BoardProcessor()

    [<Test>]
    member this.``when getting possible values`` () =
        fnShouldContain(myBoard.CellOnlyAvailableValues(0,0), [1;8;9;])
        
        fnShouldContain(myBoard.CellOnlyAvailableValues(0,3), [8;9;])
        fnShouldContain(myBoard.CellExtendedAvailableValues(0,3), [8;9;])

        fnShouldContain(myBoard.CellOnlyAvailableValues(8,4), [9;])

    [<Test>]
    member this.``when running processing`` () =
        let result = processor.ResolveOrder1(myBoard,0)
        printfn "%s" (processor.PrintBoard(myBoard))
        0

//let solution =    [|[|'1';'4';'6';'9';'8';'2';'3';'5';'7';|];
//	                  [|'3';'5';'8';'7';'4';'6';'2';'9';'1';|];
//	                  [|'9';'7';'2';'5';'3';'1';'6';'4';'8';|];
//                      
//	                  [|'8';'6';'5';'4';'7';'9';'1';'2';'3';|];
//	                  [|'2';'9';'7';'3';'1';'8';'5';'6';'4';|];
//	                  [|'4';'3';'1';'2';'6';'5';'8';'7';'9';|];
//                      
//	                  [|'5';'1';'4';'8';'2';'7';'9';'3';'6';|];
//	                  [|'7';'8';'9';'6';'5';'3';'4';'1';'2';|];
//	                  [|'6';'2';'3';'1';'9';'4';'7';'8';'5';|]|];