"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var lines = FileHelper_1.default.LoadFileLines('Inputs\\Day13.txt');
var tokens = lines
    .map(function (x, nx) { return ({ Line: x, Block: Math.floor(nx / 3) }); })
    .groupBy(function (x) { return x.Block; })
    .map(function (x) {
    var bA = loadButton(x.Items[0].Line);
    var bB = loadButton(x.Items[1].Line);
    var split = x.Items[2].Line.replace(", Y", "").split('=');
    return { PrizeX: Number(split[1]), PrizeY: Number(split[2]), ButA: bA, ButB: bB };
})
    .map(getMinTokens)
    .sum();
console.log("Result: " + tokens);
function getMinTokens(s) {
    var maxA = Math.floor(Math.min(s.PrizeX / s.ButA.MoveX, s.PrizeY / s.ButA.MoveY));
    var possible = 0;
    for (var nA = 0; nA <= maxA; nA++) {
        var gapX = s.PrizeX - nA * s.ButA.MoveX;
        var gapY = s.PrizeY - nA * s.ButA.MoveY;
        if (gapX % s.ButB.MoveX == 0) {
            var pressB = gapX / s.ButB.MoveX;
            if (gapY == pressB * s.ButB.MoveY) {
                var score = 3 * nA + pressB;
                possible = possible == 0 ? score : Math.min(possible, score);
            }
        }
    }
    return possible;
}
function loadButton(line) {
    var split = line.replace(", Y", "").split('+');
    return { MoveX: Number(split[1]), MoveY: Number(split[2]) };
}
//# sourceMappingURL=Day13.js.map