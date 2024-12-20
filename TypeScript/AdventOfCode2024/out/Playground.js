"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var LocationState2;
(function (LocationState2) {
    LocationState2[LocationState2["Vacant"] = 0] = "Vacant";
    LocationState2[LocationState2["Visited"] = 1] = "Visited";
    LocationState2[LocationState2["Blocked"] = 2] = "Blocked";
})(LocationState2 || (LocationState2 = {}));
;
function parseCell(val) {
    console.info(val);
    switch (val) {
        case '.':
            return LocationState2.Vacant;
        case '#':
            return LocationState2.Blocked;
        default:
            return LocationState2.Visited;
    }
}
console.info(parseCell('.').toString());
var s = "190: 10 19".split(':');
console.info("Num: " + Number(s[0]));
console.info("Sum: " + [1, 2, 3].sum());
console.info("Any: " + [1, 2, 3].any(function (x) { return x > 3; }));
[17, 1, 2, 3, 1, 5, 5, 17]
    .groupBy(function (x) { return x; })
    .forEach(function (grp) { return console.info("Group Key: " + grp.Key + " | Items: " + grp.Items); });
var arr = [];
arr[7] = "Hey";
console.info("Array: " + arr);
console.info("Filled: " + (new Array(5)).fill(0, 0, 5));
console.info("Reversed: " + [0, 1, 2].sort(function (a, b) { return b - a; }));
console.info("Distinct: " + [17, 1, 2, 3, 1, 5, 5, 17].distinct());
var i1 = { a: 1, b: "Hey" };
var i2 = { a: 1, b: "Diff" };
var i3 = { a: 2, b: "Hey" };
console.info("Distinct 2: " + [i1, i2, i1, i3, i2].distinct().map(function (x) { return x.a + "|" + x.b; }));
//# sourceMappingURL=Playground.js.map