"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var cells = FileHelper_1.default.LoadFileLinesAndCharacters('Inputs\\Day10.txt')
    .map(function (row, vLoc) {
    return row.map(function (c, hLoc) { return ({ Value: Number(c), VLoc: vLoc, HLoc: hLoc }); });
});
var vMax = cells.length;
var hMax = cells[0].length;
var result = cells
    .flatMap(function (x) { return x; })
    .filter(function (x) { return x.Value == 0; })
    .map(findDistinctPeaks)
    .sum();
console.log("Result: " + result); //Part1: 535. Part2: 1186
function findDistinctPeaks(start) {
    return walkToEnd(start)
        //.distinct() <-- single line change to go from part1 to part2
        .length;
    function walkToEnd(current) {
        return current.Value < 9
            ? [
                getCellSafe(current.VLoc - 1, current.HLoc), //Up
                getCellSafe(current.VLoc, current.HLoc + 1), //Right
                getCellSafe(current.VLoc + 1, current.HLoc), //Down
                getCellSafe(current.VLoc, current.HLoc - 1) //Left
            ]
                .filter(function (x) { return x !== undefined && x.Value == current.Value + 1; })
                .flatMap(walkToEnd)
            : [current];
    }
    function getCellSafe(vLoc, hLoc) {
        return vLoc < 0 || hLoc < 0 || vLoc >= vMax || hLoc >= hMax
            ? undefined
            : cells[vLoc][hLoc];
    }
}
//# sourceMappingURL=Day10.js.map