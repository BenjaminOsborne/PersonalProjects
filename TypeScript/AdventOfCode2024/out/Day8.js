"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var fileCells = FileHelper_1.default.LoadFileLinesAndCharacters('Day8.txt');
var grpAntenna = fileCells
    .flatMap(function (row, v) { return row
    .map(function (val, h) { return ({ Freq: val, vLoc: v, hLoc: h }); })
    .filter(function (x) { return x.Freq != '.'; }); } //remove empty locations
)
    .groupBy(function (x) { return x.Freq; });
var nodeGrid = fileCells.map(function (v) { return v.map(function (h) { return false; }); });
var vLen = nodeGrid.length;
var hLen = nodeGrid[0].length;
grpAntenna.forEach(function (grp) {
    return grp.Items.forEach(function (ant1) {
        return grp.Items.forEach(function (ant2) {
            return applyNodes(ant1, ant2);
        } //Same one ignored in method
        );
    });
});
var result = nodeGrid.flatMap(function (x) { return x; }).filter(function (x) { return x; }).length;
console.info("Result (Part 2): " + result); //Part1: 361. Part2: 1249.
function applyNodes(ant1, ant2) {
    if (ant1 === ant2) //Ignore if same node
     {
        return;
    }
    var hGap = ant2.hLoc - ant1.hLoc;
    var vGap = ant2.vLoc - ant1.vLoc;
    applyWithOperator(ant1, function (loc, dist) { return loc - dist; });
    applyWithOperator(ant2, function (loc, dist) { return loc + dist; });
    function applyWithOperator(ant, op) {
        var cont = true;
        var antH = ant.hLoc;
        var antV = ant.vLoc;
        while (cont) {
            cont = markNode(antH, antV); //Also applies at antennas themselves now
            antH = op(antH, hGap);
            antV = op(antV, vGap);
        }
    }
    function markNode(v, h) {
        if (v < 0 || h < 0 || v >= vLen || h >= hLen) {
            return false;
        }
        nodeGrid[v][h] = true;
        return true;
    }
}
//# sourceMappingURL=Day8.js.map