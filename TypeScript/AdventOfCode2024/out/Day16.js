"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var CellType;
(function (CellType) {
    CellType["Wall"] = "#";
    CellType["Space"] = ".";
    CellType["Start"] = "S";
    CellType["End"] = "E";
})(CellType || (CellType = {}));
var Direction;
(function (Direction) {
    Direction["Up"] = "^";
    Direction["Down"] = "v";
    Direction["Left"] = "<";
    Direction["Right"] = ">";
})(Direction || (Direction = {}));
var allDirs = [Direction.Up, Direction.Down, Direction.Left, Direction.Right];
var Operation;
(function (Operation) {
    Operation["Rotate"] = "Rotate";
    Operation["Step"] = "Step";
})(Operation || (Operation = {}));
var cells = FileHelper_1.default.LoadFileLinesAndCharacters('Inputs\\Day16_Test1.txt')
    .map(function (row, v) { return row.map(function (c, h) { return ({ LocV: v, LocH: h, Type: c }); }); });
var start = cells.flatMap(function (x) { return x; }).single(function (x) { return x.Type == CellType.Start; });
var end = cells.flatMap(function (x) { return x; }).single(function (x) { return x.Type == CellType.End; });
var initialStep = { Previous: undefined, Operation: undefined, NewLoc: start, NewDir: Direction.Right };
var nextToWalk = generateNextOptions(initialStep).Options;
var routesToEnd = [];
while (nextToWalk.length > 0) {
    var next = nextToWalk.map(generateNextOptions);
    routesToEnd.pushRange(next.filter(function (a) { return a.End !== undefined; }).map(function (b) { return b.End; }));
    nextToWalk = next.flatMap(function (x) { return x.Options; });
}
var routes = routesToEnd
    .map(function (r) { return ({ route: r, score: scoreRoute(r) }); })
    .sort(function (a, b) { return b.score - a.score; });
routes.forEach(function (x) { return console.info("Score: " + x.score + ".\n" + displayRoute(x.route)); });
//console.info("Result: " + routes[0].score);
function displayRoute(route) {
    var display = "";
    var prev = route;
    var steps = 0;
    while (prev !== undefined) {
        display = prev.NewDir + display;
        prev = prev.Previous;
        steps += 1;
    }
    return "[" + steps + "]: " + display;
}
function scoreRoute(init) {
    var loc = init;
    var score = 0;
    while (loc !== undefined) {
        score += loc.Operation == Operation.Step
            ? 1
            : loc.Operation == Operation.Rotate
                ? 1000
                : 0;
        loc = loc.Previous;
    }
    return score;
}
function generateNextOptions(prev) {
    var nextDirs = prev.Operation == Operation.Rotate
        ? [prev.NewDir] //If rotated before, can now only step, otherwise would turn around!
        : allDirs.filter(function (x) { return x != oppositeDir(prev.NewDir); });
    var nextOptions = nextDirs
        .map(function (d) { return ({ cell: getCellAt(prev.NewLoc, d), dir: d }); })
        .filter(function (x) { return x.cell.Type != CellType.Wall && hasNotVisited(x.cell, prev); });
    var end = nextOptions.first(function (x) { return x.cell.Type == CellType.End; });
    if (end !== undefined) {
        return { End: { Previous: prev, Operation: Operation.Step, NewLoc: end.cell, NewDir: prev.NewDir }, Options: [] };
    }
    var nextSteps = nextOptions.map(function (x) { return ({
        Previous: prev,
        Operation: x.dir == prev.NewDir ? Operation.Step : Operation.Rotate,
        NewLoc: x.cell,
        NewDir: x.dir
    }); });
    return { End: undefined, Options: nextSteps };
}
function hasNotVisited(cell, route) {
    var prev = route;
    while (prev !== undefined) {
        if (cell === prev.NewLoc) {
            return false;
        }
        prev = prev.Previous;
    }
    return true;
}
function oppositeDir(dir) {
    switch (dir) {
        case Direction.Up:
            return Direction.Down;
        case Direction.Down:
            return Direction.Up;
        case Direction.Left:
            return Direction.Right;
        case Direction.Right:
            return Direction.Left;
        default:
            throw new Error("Unreachable");
    }
}
function getCellAt(loc, dir) {
    switch (dir) {
        case Direction.Up:
            return cells[loc.LocV - 1][loc.LocH];
        case Direction.Down:
            return cells[loc.LocV + 1][loc.LocH];
        case Direction.Left:
            return cells[loc.LocV][loc.LocH - 1];
        case Direction.Right:
            return cells[loc.LocV][loc.LocH + 1];
        default:
            throw new Error("Unreachable");
    }
}
//# sourceMappingURL=Day16.js.map