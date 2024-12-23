"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var cells = FileHelper_1.default.LoadFileLinesAndCharacters('Inputs\\Day15.txt');
var Item;
(function (Item) {
    Item["Empty"] = ".";
    Item["Wall"] = "#";
    Item["Box"] = "O";
    Item["Robot"] = "@";
})(Item || (Item = {}));
var Direction;
(function (Direction) {
    Direction["Up"] = "^";
    Direction["Down"] = "v";
    Direction["Left"] = "<";
    Direction["Right"] = ">";
})(Direction || (Direction = {}));
var grid = cells
    .filter(function (x) { return x.length > 0 && x[0] == '#'; })
    .map(function (row, nV) { return row.map(function (c, nH) { return ({ LocV: nV, LocH: nH, Item: c }); }); });
var locVSize = grid.length;
var locHSize = grid[0].length;
var directions = cells
    .filter(function (x) { return x.length > 0 && x[0] != '#'; })
    .flatMap(function (x) { return x; })
    .map(function (x) { return x; });
var robot = grid.flatMap(function (x) { return x; }).single(function (x) { return x.Item == Item.Robot; });
directions
    .forEach(function (d, nx) {
    var r = processMove(robot, d);
    if (r.DidMove == false) {
        return;
    }
    robot = r.Robot;
    r.Updates.forEach(function (u) { return grid[u.LocV][u.LocH] = u; });
});
var result = grid
    .flatMap(function (x) { return x; })
    .filter(function (x) { return x.Item == Item.Box; })
    .map(function (x) { return 100 * x.LocV + x.LocH; })
    .sum();
console.info("Result: " + result);
grid.forEach(function (r) { return console.info(r.map(function (a) { return a.Item; }).join("")); });
function processMove(robot, direction) {
    var cells = [];
    var current = robot;
    var _loop_1 = function () {
        next = getLocForStep(current, direction);
        if (next == undefined) {
            throw new Error("Should not be able to go off map");
        }
        var cell = grid[next.LocV][next.LocH];
        if (cell.Item == Item.Wall) {
            return { value: { DidMove: false, Updates: undefined, Robot: undefined } };
        }
        if (cell.Item == Item.Robot) {
            throw new Error("Should not find robot on step!");
        }
        if (cell.Item == Item.Empty) {
            var updates_1 = [];
            updates_1.push(({ LocV: robot.LocV, LocH: robot.LocH, Item: Item.Empty })); //Space where robot was must be empty
            var nextRobot = move(robot, direction);
            updates_1.push(nextRobot);
            cells.forEach(function (c) { return updates_1.push(move(c, direction)); });
            return { value: { DidMove: true, Updates: updates_1, Robot: nextRobot } };
        }
        cells.push(cell);
        current = next;
    };
    var next;
    while (true) {
        var state_1 = _loop_1();
        if (typeof state_1 === "object")
            return state_1.value;
    }
}
function move(cell, direction) {
    var next = getLocForStep(cell, direction);
    return { LocV: next.LocV, LocH: next.LocH, Item: cell.Item };
}
function getLocForStep(location, dir) {
    var locV = location.LocV;
    var locH = location.LocH;
    var nextV = locV + (dir == Direction.Up
        ? -1
        : dir == Direction.Down ? 1 : 0);
    var nextH = locH + (dir == Direction.Left
        ? -1
        : dir == Direction.Right ? 1 : 0);
    return (nextV < 0 || nextH < 0 || nextV >= locVSize || nextH >= locHSize)
        ? undefined
        : { LocV: nextV, LocH: nextH };
}
//# sourceMappingURL=Day15.js.map