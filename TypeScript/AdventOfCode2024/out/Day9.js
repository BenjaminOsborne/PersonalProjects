"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var rawLengths = [];
var rawGaps = [];
FileHelper_1.default.LoadFile('Inputs\\Day9.txt')
    .trim()
    .split('')
    .forEach(function (val, nx) {
    return (nx % 2 == 0 ? rawLengths : rawGaps).push(Number(val));
});
var lengths = [];
var gaps = [];
//Build data
var currentIndex = 0;
for (var nx = 0; nx < rawLengths.length; nx++) {
    var fileLen = rawLengths[nx];
    lengths.push({ StartIndex: currentIndex, FileId: nx, FileLength: fileLen });
    currentIndex += fileLen;
    var gap = rawGaps[nx];
    if (gap !== undefined) //Final element, will be no gap, so handle missing!
     {
        gaps.push({ StartIndex: currentIndex, GapLength: gap });
        currentIndex += gap;
    }
}
var finalDisk = [];
lengths
    .sort(function (a, b) { return b.FileId - a.FileId; }) //Enumerate files in reverse
    .forEach(function (file) {
    var gap = gaps.first(function (x) { return x.StartIndex < file.StartIndex && x.GapLength >= file.FileLength; });
    if (gap !== undefined) {
        addFullFileBlock(file, gap.StartIndex);
        gap.GapLength -= file.FileLength;
        gap.StartIndex += file.FileLength;
    }
    else {
        addFullFileBlock(file, file.StartIndex);
    }
});
var result = finalDisk
    .map(function (x, nx) { return x > 0 ? x * nx : 0; }) //(> 0) filter also handles "undefined" gaps (which)
    .sum();
console.info("Result: " + result); //Part2: 6272188244509
function addFullFileBlock(block, startIndex) {
    for (var nx = 0; nx < block.FileLength; nx++) {
        finalDisk[startIndex + nx] = block.FileId;
    }
}
//# sourceMappingURL=Day9.js.map