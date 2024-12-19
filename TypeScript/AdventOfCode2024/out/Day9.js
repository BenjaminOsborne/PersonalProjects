"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var rawLengths = [];
var gaps = [];
FileHelper_1.default.LoadFile('Day9.txt')
    .trim()
    .split('')
    .forEach(function (val, nx) {
    return (nx % 2 == 0 ? rawLengths : gaps).push(Number(val));
});
var lengths = rawLengths
    .map(function (x, nx) { return ({ FileId: nx, OriginalLength: x, Remaining: x }); });
var lengthsReverse = lengths.slice().sort(function (a, b) { return b.FileId - a.FileId; });
var finalDisk = [];
//Add first
addFullFileBlock(lengths[0]);
//Enumerate gaps
for (var gNx = 0; gNx < gaps.length; gNx++) {
    //Fill in gap
    processNextGap(gaps[gNx]);
    //Then take the next item
    var nextFillIn = lengths.filter(function (x) { return x.Remaining > 0; })[0];
    if (nextFillIn !== undefined) {
        addFullFileBlock(nextFillIn);
    }
    else {
        break; //If no blocks left with any, exit loop
    }
}
var result = finalDisk
    .map(function (x, nx) { return x * nx; })
    .sum();
console.info("Result: " + result); //Part1: 6242766523059
function processNextGap(gap) {
    var remainingGap = gap;
    while (true) {
        var usable = lengthsReverse.filter(function (x) { return x.Remaining > 0; });
        if (usable.length == 0) {
            return;
        }
        var took = addFileBlockUpTo(usable[0], remainingGap);
        remainingGap -= took;
        if (remainingGap == 0) {
            return;
        }
    }
}
function addFullFileBlock(block) {
    addFileBlockUpTo(block, block.Remaining);
}
function addFileBlockUpTo(block, maxTake) {
    var take = Math.min(maxTake, block.Remaining);
    for (var nx = 0; nx < take; nx++) {
        finalDisk.push(block.FileId);
    }
    block.Remaining = block.Remaining - take;
    return take;
}
//# sourceMappingURL=Day9.js.map