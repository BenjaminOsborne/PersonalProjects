"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var loaded = FileHelper_1.default.LoadFile('Inputs\\Day11.txt')
    .split(' ');
//.map(x => ());
var headLink = undefined;
var totalNums = loaded.length;
for (var n = loaded.length - 1; n >= 0; n--) {
    var sNum = loaded[n];
    headLink = { val: { num: Number(sNum), asString: sNum }, next: headLink };
}
for (var n = 0; n < 75; n++) {
    var local = headLink;
    while (local !== undefined) {
        local = blink(local);
    }
    console.info("Loop: " + (n + 1) + ". Count: " + totalNums);
}
console.info("Result: " + totalNums); //Par1: 186424
function blink(link) {
    if (link === undefined) {
        return undefined;
    }
    var num = link.val;
    if (num.num == 0) {
        link.val = { num: 1, asString: "1" };
        return link.next;
    }
    var numLen = num.asString.length;
    if (numLen % 2 == 0) {
        var num1 = Number(num.asString.slice(0, numLen / 2));
        var num2 = Number(num.asString.slice(numLen / 2));
        var tNum1 = { num: num1, asString: num1.toString() };
        var tNum2 = { num: num2, asString: num2.toString() };
        link.val = tNum1;
        link.next = { val: tNum2, next: link.next };
        totalNums += 1;
        return link.next.next;
    }
    else {
        var next = num.num * 2024;
        link.val = { num: next, asString: next.toString() };
        return link.next;
    }
}
//# sourceMappingURL=Day11.js.map