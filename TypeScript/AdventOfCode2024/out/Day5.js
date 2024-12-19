"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var FileHelper_1 = require("./FileHelper");
var allLines = FileHelper_1.default.LoadFileLines('Day5.txt');
var rules = allLines
    .filter(function (l) { return l.includes('|'); })
    .map(function (l) {
    var rule = l.split('|').map(function (x) { return Number(x); });
    return { left: rule[0], right: rule[1] };
});
var updates = allLines
    .filter(function (l) { return l.includes(','); })
    .map(function (l) { return l.split(',').map(function (x) { return Number(x); }); });
var total = updates
    .map(function (arr) { return getReorderResult(arr, false, 1); }) //startIndex:1 as first element cannot break a rule!
    .filter(function (x) { return x.isReordered; })
    .map(function (x) { return x.update[Math.floor(x.update.length / 2)]; })
    .reduce(function (agg, x) { return agg + x; }, 0);
console.info("Result: " + total); //Part2: 5448
function getReorderResult(update, isReordered, startIndex) {
    for (var nx = 1; nx < update.length; nx++) {
        var currentVal = update[nx];
        var matchedRules = rules.filter(function (x) { return x.left == currentVal; });
        if (matchedRules.length == 0) {
            continue;
        }
        var previous = update.slice(0, nx); //everything up to index (not including!)
        var failedRules = matchedRules
            .filter(function (rule) { return previous.filter(function (x) { return x == rule.right; }).length > 0; });
        if (failedRules.length == 0) {
            continue;
        }
        var firstFailed = failedRules[0];
        var start = previous.filter(function (x) { return x != firstFailed.right; }); //everything except the item ("rule.right") to bump forwards
        var middle = [currentVal, firstFailed.right]; //the current value with the "rule.right" after it
        var end = update.slice(nx + 1); //everything left in the array after the current value
        var next = start.concat(middle).concat(end); //combine to next call
        return getReorderResult(next, true, nx); //startIndex current (NOT nx+1), as element shuffled forwards so "next val" will now be at "nx"
    }
    return { update: update, isReordered: isReordered };
}
//# sourceMappingURL=Day5.js.map