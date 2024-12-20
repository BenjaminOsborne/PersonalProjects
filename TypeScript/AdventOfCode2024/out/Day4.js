"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var FileHelper_1 = require("./FileHelper");
var map = FileHelper_1.default.LoadFileLinesWithMap('Inputs\\Day4.txt', function (l) { return l.split(''); });
var xRange = map.length;
var yRange = map[0].length;
var counter = 0;
for (var nx = 0; nx < xRange; nx++) {
    for (var ny = 0; ny < yRange; ny++) {
        if (map[nx][ny] != "A") {
            continue;
        }
        var isX = (isMS(nx - 1, ny + 1, nx + 1, ny - 1) || isMS(nx + 1, ny - 1, nx - 1, ny + 1)) &&
            (isMS(nx - 1, ny - 1, nx + 1, ny + 1) || isMS(nx + 1, ny + 1, nx - 1, ny - 1));
        counter += isX ? 1 : 0;
    }
}
console.log("Result: " + counter);
function isMS(xM, yM, xS, yS) {
    return equalsSafe(xM, yM, "M") &&
        equalsSafe(xS, yS, "S");
}
function equalsSafe(x, y, c) {
    if (x < 0 || y < 0) {
        return false;
    }
    if (x >= map.length) {
        return false;
    }
    var yArr = map[x];
    if (y >= yArr.length) {
        return false;
    }
    return yArr[y] == c;
}
//# sourceMappingURL=Day4.js.map