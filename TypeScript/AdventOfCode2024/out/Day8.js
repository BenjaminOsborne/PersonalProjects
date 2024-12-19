"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var fileCells = FileHelper_1.default.LoadFileLinesWithMap('Day8.txt', function (x) { return x.split(''); });
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
        });
    });
});
var result = nodeGrid.flatMap(function (x) { return x; }).filter(function (x) { return x; }).length;
console.info("Result: " + result); //Part1: 361
function applyNodes(ant1, ant2) {
    if (ant1 === ant2) //Ignore if same node
     {
        return;
    }
    var hGap = ant2.hLoc - ant1.hLoc;
    var vGap = ant2.vLoc - ant1.vLoc;
    apply(ant1.hLoc - hGap, ant1.vLoc - vGap);
    apply(ant2.hLoc + hGap, ant2.vLoc + vGap);
    function apply(v, h) {
        if (v < 0 || h < 0 || v >= vLen || h >= hLen) {
            return;
        }
        nodeGrid[v][h] = true;
    }
}
//# sourceMappingURL=Day8.js.map