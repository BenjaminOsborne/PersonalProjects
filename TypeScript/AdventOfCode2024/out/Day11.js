"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var loaded = FileHelper_1.default.LoadFile('Inputs\\Day11.txt')
    .split(' ')
    .map(function (x) { return ({ num: toNum(Number(x)), multiplier: 1 }); });
var input = loaded;
for (var n = 0; n < 75; n++) {
    input = blink(input);
    console.info("Loop: " + (n + 1) + ". UniqueNumbers: " + input.length + ". Total: " + input.sumFrom(function (x) { return x.multiplier; }));
}
console.info("Result: " + input.sumFrom(function (x) { return x.multiplier; })); //Part2: 219838428124832
function blink(current) {
    return current
        .flatMap(function (x) {
        return processNumberRules(x.num)
            .map(function (n) {
            return ({ num: n, multiplier: x.multiplier });
        });
    })
        .groupBy(function (x) { return x.num.val; })
        .map(function (x) {
        var first = x.Items[0];
        return x.Items.length == 1
            ? first
            : ({ num: first.num, multiplier: x.Items.sumFrom(function (i) { return i.multiplier; }) });
    });
}
function processNumberRules(num) {
    if (num.val == 0) {
        return [toNum(1)];
    }
    var numLen = num.asString.length;
    if (numLen % 2 == 0) {
        return [
            toNum(Number(num.asString.slice(0, numLen / 2))),
            toNum(Number(num.asString.slice(numLen / 2)))
        ];
    }
    return [toNum(num.val * 2024)];
}
function toNum(num) { return { val: num, asString: num.toString() }; }
//# sourceMappingURL=Day11.js.map