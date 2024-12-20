import './Globals'
import FileHelper from './FileHelper';

type Cell = { Value: number, VLoc: number, HLoc: number };

var cells = FileHelper.LoadFileLinesAndCharacters('Inputs\\Day10.txt')
    .map((row, vLoc) =>
        row.map((c, hLoc) => (
            { Value: Number(c), VLoc: vLoc, HLoc: hLoc } as Cell
        )));

var vMax = cells.length;
var hMax = cells[0].length;

var result = cells
    .flatMap(x => x)
    .filter(x => x.Value == 0)
    .map(findDistinctPeaks)
    .sum();
console.log("Result: " + result) //Part1: 535. Part2: 1186

function findDistinctPeaks(start: Cell) : number
{
    return walkToEnd(start)
        //.distinct() <-- single line change to go from part1 to part2
        .length;

    function walkToEnd(current: Cell) : Cell[]
    {
        return current.Value < 9
            ? [
                getCellSafe(current.VLoc-1, current.HLoc), //Up
                getCellSafe(current.VLoc, current.HLoc+1), //Right
                getCellSafe(current.VLoc+1, current.HLoc), //Down
                getCellSafe(current.VLoc, current.HLoc-1) //Left
                ]
                .filter(x => x !== undefined && x.Value == current.Value + 1)
                .flatMap(walkToEnd)
            : [current];
    }

    function getCellSafe(vLoc: number, hLoc: number) : Cell
    {
        return vLoc < 0 || hLoc < 0 || vLoc >= vMax || hLoc >= hMax
            ? undefined
            : cells[vLoc][hLoc];
    }
}
