"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var gridSizeX = 101;
var gridSizeY = 103;
var step = 100;
var midX = Math.floor(gridSizeX / 2);
var midY = Math.floor(gridSizeY / 2);
var robotStart = FileHelper_1.default.LoadFileLines('Inputs\\Day14.txt')
    .map(function (l) {
    var lines = l.replace("p=", "")
        .replace("v=", "")
        .split(' ')
        .map(function (a) { return a.split(",").map(function (b) { return Number(b); }); });
    return { PosX: lines[0][0], PosY: lines[0][1], VelocityX: lines[1][0], VelocityY: lines[1][1] };
});
var postStep = robotStart
    .map(performStep);
var score = postStep
    .map(function (r) { return ({ Robot: r, Quadrant: getQuadrant(r) }); })
    .filter(function (x) { return x.Quadrant != undefined; })
    .groupBy(function (x) { return x.Quadrant; })
    .map(function (x) { return x.Items.length; })
    .reduce(function (agg, x) { return agg * x; }, 1);
console.info("Result: " + score);
function performStep(r) {
    return ({
        PosX: normalise(r.PosX + step * r.VelocityX, gridSizeX),
        PosY: normalise(r.PosY + step * r.VelocityY, gridSizeY),
        VelocityX: r.VelocityX,
        VelocityY: r.VelocityY
    });
    function normalise(pos, size) {
        var raw = pos % size;
        return raw < 0 ? size + raw : raw;
    }
}
function getQuadrant(robot) {
    if (robot.PosX == midX || robot.PosY == midY) {
        return undefined;
    }
    return (robot.PosX < midX ? "0" : "1") + "|" + (robot.PosY < midY ? "0" : "1");
}
//# sourceMappingURL=Day14.js.map