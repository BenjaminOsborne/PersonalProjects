"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var fs = require("fs");
var file = fs.readFileSync('Day1.txt', 'utf8');
var left = [];
var right = [];
file.split("\r\n")
    .filter(function (line) { return line.length > 0; })
    .forEach(function (line) {
    var nums = line
        .split(/\s+/) //match all whitespace
        .map(function (t) { return Number(t); });
    left.push(nums[0]);
    right.push(nums[1]);
});
var orderedLeft = left.sort();
var orderedRight = right.sort();
var total = orderedLeft
    .map(function (val, nx) { return val - orderedRight[nx]; })
    .reduce(function (agg, val) { return agg + Math.abs(val); });
console.info(total); //2580760 (for Day1.txt)
//# sourceMappingURL=Day1.js.map