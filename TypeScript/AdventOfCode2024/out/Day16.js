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
    Operation["Rotate"] = "R";
    Operation["Step"] = "S";
})(Operation || (Operation = {}));
var cells = FileHelper_1.default.LoadFileLinesAndCharacters('Inputs\\Day16.txt') //Day16_Test2
    .map(function (row, v) { return row.map(function (c, h) { return ({ LocV: v, LocH: h, Type: c }); }); });
var routesToLocations = cells.map(function (r) { return r.map(function (v) { return []; }); });
var start = cells.flatMap(function (x) { return x; }).single(function (x) { return x.Type == CellType.Start; });
var end = cells.flatMap(function (x) { return x; }).single(function (x) { return x.Type == CellType.End; });
var initialStep = { Previous: undefined, Operation: undefined, NewLoc: start, NewDir: Direction.Right, Score: 0 };
var nextToWalk = generateNextOptions(initialStep).Options;
var routesToEnd = [];
var loop = 0;
while (nextToWalk.length > 0) {
    console.info("Loop: " + ++loop + "\tHeads: " + nextToWalk.length);
    var next = nextToWalk.map(generateNextOptions);
    routesToEnd.pushRange(next.filter(function (a) { return a.End !== undefined; }).map(function (b) { return b.End; }));
    //filter/update "routesToLocations" with best score to get to that point
    nextToWalk = [];
    next.flatMap(function (x) { return x.Options; })
        .sort(function (a, b) { return a.Score - b.Score; }) //sort ascending on score (so if multiple routes to same point, lowest score kept!)
        .forEach(function (r) {
        var routesAtLoc = routesToLocations[r.NewLoc.LocV][r.NewLoc.LocH];
        var existRoute = routesAtLoc.first(function (x) { return x.NewDir == r.NewDir; });
        //NOTE: could use "existRoute.Score <= r.Score" if JUST cared about best score. But part2 wants all walked cells, so must also take equal on score
        if (existRoute !== undefined && existRoute.Score < r.Score) {
            return;
        }
        var updated = routesAtLoc.filter(function (x) { return x != existRoute; });
        updated.push(r);
        routesToLocations[r.NewLoc.LocV][r.NewLoc.LocH] = updated;
        nextToWalk.push(r);
    });
}
var routes = routesToEnd
    .sort(function (a, b) { return a.Score - b.Score; }); //ascending
//.sort((a,b) => b.score - a.score) //descending
var best = routes[0];
console.info("Route:" + displayRoute(best));
console.info("Score: " + best.Score); //Part1: 90440
var equalBestRouteCells = routesToEnd
    .filter(function (x) { return x.Score == best.Score; })
    .flatMap(function (x) { return getAllCellsOnRoute(x); })
    .distinct()
    .length;
console.info("Cells on best routes: " + equalBestRouteCells); //Part2: 479
function getAllCellsOnRoute(route) {
    var cells = [];
    var pre = route;
    while (pre !== undefined) {
        cells.push(pre.NewLoc);
        pre = pre.Previous;
    }
    return cells;
}
function displayRoute(route) {
    var _a;
    var display = "";
    var prev = route;
    var steps = 0;
    while (prev !== undefined) {
        display = ((_a = prev.Operation) !== null && _a !== void 0 ? _a : "") + display;
        prev = prev.Previous;
        steps += 1;
    }
    return "Length: [" + steps + "]. Path: " + display;
}
function generateNextOptions(prev) {
    var nextDirs = prev.Operation == Operation.Rotate
        ? [prev.NewDir] //If rotated before, can now only step, otherwise would turn around!
        : allDirs.filter(function (x) { return x != oppositeDir(prev.NewDir); });
    var nextOptions = nextDirs
        .map(function (d) { return ({ cell: getCellAt(prev.NewLoc, d), dir: d }); })
        .filter(function (x) { return x.cell.Type != CellType.Wall; });
    var end = nextOptions.first(function (x) { return x.cell.Type == CellType.End; });
    if (end !== undefined) {
        var endRoute = { Previous: prev, Operation: Operation.Step, NewLoc: end.cell, NewDir: prev.NewDir, Score: prev.Score + 1 };
        return { End: endRoute, Options: [] };
    }
    var nextSteps = nextOptions.map(function (x) {
        var isStep = x.dir == prev.NewDir;
        return ({
            Previous: prev,
            Operation: isStep ? Operation.Step : Operation.Rotate,
            NewLoc: isStep ? x.cell : prev.NewLoc, //only move if not rotating
            NewDir: x.dir,
            Score: prev.Score + (isStep ? 1 : 1000)
        });
    });
    if (nextSteps.length == 1) {
        return generateNextOptions(nextSteps[0]); //if only 1 option - just walk immediately and short-cut "best route" analysis in caller
    }
    return { End: undefined, Options: nextSteps };
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