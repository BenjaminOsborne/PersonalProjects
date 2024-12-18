"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var fs = require("fs");
var file = fs.readFileSync('Day5.txt', 'utf8');
var allLines = file.split('\r\n');
var rules = allLines
    .filter(function (l) { return l.includes('|'); })
    .map(function (l) {
    var rule = l.split('|').map(function (x) { return Number(x); });
    return { left: rule[0], right: rule[1] };
});
var total = allLines
    .filter(function (l) { return l.includes(','); })
    .map(function (l) { return l.split(',').map(function (x) { return Number(x); }); })
    .filter(function (arr) { return isValidUpdate(arr); })
    .map(function (arr) { return arr[Math.floor(arr.length / 2)]; })
    .reduce(function (agg, x) { return agg + x; }, 0);
console.info("Result: " + total);
function isValidUpdate(update) {
    for (var nx = 1; nx < update.length; nx++) {
        var matchedRules = rules.filter(function (x) { return x.left == update[nx]; });
        if (matchedRules.length == 0) {
            continue;
        }
        var previous = update.slice(0, nx);
        var brokenRules = matchedRules
            .filter(function (rule) { return previous.filter(function (x) { return x == rule.right; }).length > 0; })
            .length > 0;
        if (brokenRules) {
            return false;
        }
    }
    return true;
}
//# sourceMappingURL=Day5.js.map