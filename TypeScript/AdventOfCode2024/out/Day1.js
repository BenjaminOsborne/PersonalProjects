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
var orderedLeft = left.sort();
var orderedRight = right.sort();
var total = orderedLeft
    .map(function (val, nx) { return val - orderedRight[nx]; })
    .reduce(function (agg, val) { return agg + Math.abs(val); });
console.info("Result: " + total); //2580760 (for Day1.txt)
//# sourceMappingURL=Day1.js.map