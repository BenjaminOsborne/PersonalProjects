"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var fs = require("fs");
var file = fs.readFileSync('Day1.txt', 'utf8');
var left = [];
var right = [];
file.split(/\s+/) //split on all whitespace (newlines AND spaces between numbers)
    .map(function (x) { return Number(x); }) //cast to number
    .forEach(function (num, nx) {
    return (nx % 2 == 0 ? left : right).push(num);
});
console.info(left);
var orderedLeft = left.sort();
var orderedRight = right.sort();
var total = orderedLeft
    .map(function (val, nx) { return val - orderedRight[nx]; })
    .reduce(function (agg, val) { return agg + Math.abs(val); });
console.info("Result (Part1): " + total); //2580760 (for Day1.txt)
//Part 2
var rightMap = new Map();
orderedRight.forEach(function (x) {
    var count = rightMap.get(x);
    //console.info("Count: " + count)
    //console.info("Count2: " + (count || 0))
    rightMap.set(x, (count || 0) + 1);
});
var totalPart2 = orderedLeft
    .reduce(function (agg, x) { return agg + (x * (rightMap.get(x) || 0)); });
var totalPart2_2 = orderedLeft
    .reduce(function (agg5, x) { return agg5 + (x * (orderedRight.filter(function (r) { return r == x; }).length)); });
console.info("Result (Part2): " + totalPart2); //Initial guess: 25368459
console.info("Result (Part2_2): " + totalPart2_2); //Initial guess: 25368459
//# sourceMappingURL=Day1.js.map