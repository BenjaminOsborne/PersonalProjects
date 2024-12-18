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
//# sourceMappingURL=Playground.js.map