"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var FileHelper_1 = require("./FileHelper");
var fileLines = FileHelper_1.default.LoadFileLines('Day2.txt');
var countSafe = fileLines
    .filter(function (line) {
    var nums = line.split(/\s+/).map(function (x) { return Number(x); });
    if (isValidReport(nums)) {
        return true;
    }
    //part2 handling...
    for (var nx = 0; nx < nums.length; nx++) {
        var withRemovedIndex = isValidReport(nums.filter(function (x, localNx) { return localNx != nx; }));
        if (withRemovedIndex) {
            return true;
        }
    }
    return false;
})
    .length;
console.log("Result (part2): " + countSafe); //Part1: 332
function isValidReport(nums) {
    var isUp = (nums[1] - nums[0]) > 0;
    for (var nx = 0; nx < nums.length - 1; nx++) {
        if (((nums[nx + 1] - nums[nx]) > 0) != isUp) {
            return false;
        }
        var gap = Math.abs(nums[nx + 1] - nums[nx]);
        if (gap < 1 || gap > 3) {
            return false;
        }
    }
    return true;
}
//# sourceMappingURL=Day2.js.map