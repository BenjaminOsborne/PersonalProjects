module TestRunner

open SudokuTests

[<EntryPoint>]
let main argv = 

    let basicTest = new SudokuTests.``Given a Basic Sudoku Board``();
    let test_b1 = basicTest.``when getting orders``()
    let test_b2 = basicTest.``when getting known values``()
    let test_b3 = basicTest.``when getting missing values``()
    let test_b4 = basicTest.``when getting possible values``()

    let simpleTest = new SudokuTests.``Given a Simple Sudoku Board``();
    let test1_s1 = simpleTest.``when getting possible values``()
    let test1_s2 = simpleTest.``when running processing``()
    
    0 // return an integer exit code

