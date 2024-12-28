"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var lines = FileHelper_1.default.LoadFileLinesWithMap('Inputs\\Day18.txt', function (l) {
    var arr = l.split(',').map(function (x) { return Number(x); });
    return { PosHor: arr[0], PosVer: arr[1] };
});
var size = 71;
var take = 1024;
var grid = Array(size).fill([]).map(function (_) { return Array(size).fill('.'); });
console.info(grid.length);
console.info(grid[0].length);
lines.slice(0, take).forEach(function (l) { return grid[l.PosVer][l.PosHor] = '#'; });
grid.forEach(function (x) { return console.info(x.join('')); });
//# sourceMappingURL=Day18.js.map