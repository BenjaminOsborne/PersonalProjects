"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var Utility_1 = require("./Utility");
var FileHelper_1 = require("./FileHelper");
var robotStart = FileHelper_1.default.LoadFileLines('Inputs\\Day14.txt')
    .map(function (l) {
    var lines = l.replace("p=", "")
        .replace("v=", "")
        .split(' ')
        .map(function (a) { return a.split(",").map(function (b) { return Number(b); }); });
    return { PosX: lines[0][0], PosY: lines[0][1], VelocityX: lines[1][0], VelocityY: lines[1][1] };
});
var gridSizeX = 101;
var gridSizeY = 103;
var midX = Math.floor(gridSizeX / 2);
var midY = Math.floor(gridSizeY / 2);
var outputFile = 'Inputs\\Day14_Output.txt';
FileHelper_1.default.writeFile(outputFile, "");
var startStep = 9877; //gridSizeX * gridSizeY
for (var step = 0; step < gridSizeX * gridSizeY; step++) {
    var stepped = robotStart
        .map(function (r) { return performStep(r, step); });
    var inLine = stepped
        .groupBy(function (x) { return x.PosX; })
        .map(function (x) { return getLongestContinuous(x.Items.map(function (i) { return i.PosY; })); })
        .filter(function (l) { return l > 4; });
    if (inLine.length == 0) {
        continue;
    }
    console.info("Candidate: " + step);
    printContext(Utility_1.default.arrayFill(gridSizeX, function () { return '.'; }).join(""));
    printContext("STEP: " + step);
    var display = Utility_1.default.arrayFill(gridSizeY, function () { return Utility_1.default.arrayFill(gridSizeX, function () { return '.'; }); });
    stepped
        .groupBy(function (x) { return x.PosX + "|" + x.PosY; })
        .forEach(function (x) { return display[x.Items[0].PosY][x.Items[0].PosX] = x.Items.length.toString(); });
    display.forEach(function (r) { return printContext(r.join("")); });
}
function printContext(data) {
    console.info(data);
    FileHelper_1.default.appendFile(outputFile, data + "\n");
}
function getLongestContinuous(pos) {
    pos.sort();
    var longest = 0;
    var previous = -2;
    for (var n = 0; n < pos.length; n++) {
        var val = pos[n];
        if (val == previous + 1) {
            longest += 1;
        }
        else {
            longest = 1;
        }
        previous = val;
    }
    return longest;
}
function performStep(r, step) {
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