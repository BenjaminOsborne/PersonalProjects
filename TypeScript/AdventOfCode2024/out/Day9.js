"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var rawLengths = [];
var gaps = [];
FileHelper_1.default.LoadFile('Day9_Test.txt')
    .trim()
    .split('')
    .forEach(function (val, nx) {
    return (nx % 2 == 0 ? rawLengths : gaps).push(Number(val));
});
var lengths = rawLengths
    .map(function (x, nx) { return ({ FileId: nx, OriginalLength: x, Handled: false, CanMove: true }); });
var lengthsReverse = lengths
    .slice() //shallow clone as require 2 distinct arrays.
    .sort(function (a, b) { return b.FileId - a.FileId; });
var finalDisk = [];
//Add first
var currentIndex = addFullFileBlock(lengths[0], 0);
//Enumerate gaps
for (var gNx = 0; gNx < gaps.length; gNx++) {
    //Try fill in gap
    var gap = gaps[gNx];
    while (true) {
        var firstFillIn = scanForFileFromEndToFillGap(gap);
        if (firstFillIn !== undefined) {
            currentIndex = addFullFileBlock(firstFillIn, currentIndex);
            gap -= firstFillIn.OriginalLength;
            if (gap == 0) {
                break;
            }
        }
        else {
            currentIndex += gap;
            break;
        }
    }
    //Then take the next item
    var nextFillIn = lengths.filter(function (x) { return !x.Handled; })[0];
    if (nextFillIn !== undefined) {
        currentIndex = addFullFileBlock(nextFillIn, currentIndex);
    }
    else {
        break; //If no blocks left with any, exit loop
    }
}
console.info("FinalDisk: " + finalDisk);
var result = finalDisk
    .map(function (x, nx) { return x > 0 ? x * nx : 0; }) //Also handles "undefined" when gaps
    .sum();
console.info("Result: " + result); //Part1: 6242766523059
function scanForFileFromEndToFillGap(gap) {
    for (var nx = 0; nx < lengthsReverse.length; nx++) {
        var item = lengthsReverse[nx];
        if (item.Handled) {
            continue;
        }
        if (item.CanMove && item.OriginalLength <= gap) {
            return item;
        }
        item.CanMove = false;
    }
    return undefined;
}
//Returns next index
function addFullFileBlock(block, startIndex) {
    for (var nx = 0; nx < block.OriginalLength; nx++) {
        finalDisk[startIndex + nx] = block.FileId;
    }
    block.Handled = true;
    block.CanMove = false;
    return startIndex + block.OriginalLength;
}
//# sourceMappingURL=Day9.js.map