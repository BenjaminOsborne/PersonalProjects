"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var fs = require("fs");
var Direction;
(function (Direction) {
    Direction["Up"] = "^";
    Direction["Right"] = ">";
    Direction["Down"] = "v";
    Direction["Left"] = "<";
})(Direction || (Direction = {}));
var LocationState;
(function (LocationState) {
    LocationState[LocationState["Vacant"] = 0] = "Vacant";
    LocationState[LocationState["Visited"] = 1] = "Visited";
    LocationState[LocationState["Blocked"] = 2] = "Blocked";
})(LocationState || (LocationState = {}));
;
var file = fs.readFileSync('Day6.txt', 'utf8');
var grid = file
    .split('\r\n')
    .map(function (arr) { return arr.split(''); })
    .map(function (row, verNx) {
    return row.map(function (val, horNx) { return ({
        location: { verticalIndex: verNx, horizontalIndex: horNx },
        state: parseCell(val),
        currentDirection: parseDirection(val),
    }); });
});
var verticalCount = grid.length;
var horizontalCount = grid[0].length;
var initial = grid.flatMap(function (row) { return row.filter(function (c) { return c.state == LocationState.Visited; }); })[0];
var nextResult = nextStep(grid, initial.location, initial.currentDirection); //initialise
while (!nextResult.isFinished) {
    nextResult = nextStep(nextResult.grid, nextResult.nextLoc, nextResult.direction);
}
var visitedCount = nextResult.grid
    .flatMap(function (v) { return v.filter(function (r) { return r.state == LocationState.Visited; }); }).length;
console.info("Result: " + visitedCount); //Part1: 5461
function parseCell(val) {
    switch (val) {
        case '.':
            return LocationState.Vacant;
        case '#':
            return LocationState.Blocked;
        default:
            return LocationState.Visited;
    }
}
function parseDirection(val) {
    switch (val) {
        case Direction.Up:
            return Direction.Up;
        case Direction.Down:
            return Direction.Down;
        case Direction.Left:
            return Direction.Left;
        case Direction.Right:
            return Direction.Right;
        default:
            return undefined;
    }
}
function nextStep(grid, current, direction) {
    var nextLoc = getNextLocation(current, direction);
    if (isOutOfRange(nextLoc)) //if next is out of range, then finished!
     {
        return { isFinished: true, grid: grid, nextLoc: current, direction: direction };
    }
    if (isBlocked(grid, nextLoc)) //If blocked, rotate at current
     {
        return { isFinished: false, grid: grid, nextLoc: current, direction: rotate(direction) };
    }
    //mark visited
    grid[nextLoc.verticalIndex][nextLoc.horizontalIndex] = { location: nextLoc, state: LocationState.Visited };
    return { isFinished: false, grid: grid, nextLoc: nextLoc, direction: direction };
}
function isOutOfRange(current) {
    return current.verticalIndex < 0 || current.verticalIndex >= verticalCount ||
        current.horizontalIndex < 0 || current.horizontalIndex >= horizontalCount;
}
function isBlocked(grid, current) {
    var locInfo = grid[current.verticalIndex][current.horizontalIndex];
    return locInfo.state == LocationState.Blocked;
}
function getNextLocation(current, direction) {
    switch (direction) {
        case Direction.Up:
            return { verticalIndex: current.verticalIndex - 1, horizontalIndex: current.horizontalIndex };
        case Direction.Down:
            return { verticalIndex: current.verticalIndex + 1, horizontalIndex: current.horizontalIndex };
        case Direction.Left:
            return { verticalIndex: current.verticalIndex, horizontalIndex: current.horizontalIndex - 1 };
        case Direction.Right:
            return { verticalIndex: current.verticalIndex, horizontalIndex: current.horizontalIndex + 1 };
        default:
            var exhaustiveCheck = direction;
            return undefined;
    }
}
function rotate(direction) {
    switch (direction) {
        case Direction.Up:
            return Direction.Right;
        case Direction.Down:
            return Direction.Left;
        case Direction.Left:
            return Direction.Up;
        case Direction.Right:
            return Direction.Down;
        default:
            var exhaustiveCheck = direction;
            return undefined;
    }
}
//# sourceMappingURL=Day6.js.map