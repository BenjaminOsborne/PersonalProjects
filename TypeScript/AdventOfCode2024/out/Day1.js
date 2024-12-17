"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var fs = require("fs");
var file = fs.readFileSync('Day1.txt', 'utf8');
var left = [];
var right = [];
;
file.split("\r\n")
    .filter(function (l) { return l.length > 0; })
    .forEach(function (l) {
    var nums = l.split(/\s/).filter(function (t) { return t.length > 0; }).map(function (t) { return Number(t); });
    left.push(nums[0]);
    right.push(nums[1]);
});
var orderedLeft = left.sort();
var orderedRight = right.sort();
var total = 0;
for (var nx = 0; nx < orderedLeft.length; nx++) {
    total += Math.abs(orderedLeft[nx] - orderedRight[nx]);
}
console.info(total); //2580760 (for text file)
//# sourceMappingURL=Day1.js.map