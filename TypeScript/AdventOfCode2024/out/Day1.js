"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var FileHelper_1 = require("./FileHelper");
var file = FileHelper_1.default.LoadFile('Day1.txt');
var left = [];
var right = [];
file.split(/\s+/) //split on all whitespace (newlines AND spaces between numbers)
    .map(function (x) { return Number(x); }) //cast to number
    .forEach(function (num, nx) {
    return (nx % 2 == 0 ? left : right).push(num);
});
var orderedLeft = left.sort(function (a, b) { return a - b; });
var orderedRight = right.sort(function (a, b) { return a - b; });
var totalPart1 = orderedLeft
    .map(function (val, nx) { return val - orderedRight[nx]; })
    .reduce(function (agg, val) { return agg + Math.abs(val); }, 0);
console.info("Result (Part1): " + totalPart1); //2580760 (for Day1.txt)
//Part 2
var rightMap = new Map();
orderedRight.forEach(function (val) {
    return rightMap.set(val, (rightMap.get(val) || 0) + 1);
});
var totalPart2 = orderedLeft
    .reduce(function (agg, val) { return agg + (val * (rightMap.get(val) || 0)); }, 0);
console.info("Result (Part2): " + totalPart2); //25358365
//# sourceMappingURL=Day1.js.map