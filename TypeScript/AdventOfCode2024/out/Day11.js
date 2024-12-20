"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
console.time('Run');
var loaded = FileHelper_1.default.LoadFile('Inputs\\Day11.txt')
    .split(' ')
    .map(function (sNum) { return ({ num: Number(sNum), multiplier: 1 }); });
var input = loaded;
var blinkMap = new Map();
for (var n = 0; n < 75; n++) {
    input = blink(input);
    console.info("Loop: " + (n+1) + ". UniqueNumbers: " + input.length + ". Total: " + input.sumFrom(x => x.multiplier));
}
console.info("Result: " + input.sumFrom(function (x) { return x.multiplier; })); //Part2: 219838428124832
console.timeEnd('Run');

function blink(current)
{
    return current
        .flatMap(function (x) {
        return blinkMap.getOrAdd(x.num, function (k) { return processNumberRules(k); })
            .map(function (n) {
            return ({ num: n, multiplier: x.multiplier });
        });
    })
        .groupBy(function (x) { return x.num; })
        .map(function (x) {
        var first = x.Items[0];
        return x.Items.length == 1
            ? first
            : ({ num: first.num, multiplier: x.Items.sumFrom(function (i) { return i.multiplier; }) });
    });
}

function processNumberRules(num) {
    if (num == 0) {
        return [1];
    }
    var strNum = num.toString();
    var numLen = strNum.length;
    if (numLen % 2 == 0) {
        var splitAt = numLen / 2;
        return [
            new Number(strNum.slice(0, splitAt)),
            new Number(strNum.slice(splitAt))
        ];
    }
    return [num * 2024];
}
//# sourceMappingURL=Day11.js.map