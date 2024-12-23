"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var cells = FileHelper_1.default.LoadFileLinesAndCharacters('Inputs\\Day15.txt');
var Item;
(function (Item) {
    Item["Empty"] = ".";
    Item["Wall"] = "#";
    Item["BoxLeft"] = "[";
    Item["BoxRight"] = "]";
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
    .map(function (r) { return r.flatMap(function (c) { return part2ExpandCell(c); }); })
    .map(function (row, nV) { return row.map(function (c, nH) { return ({ LocV: nV, LocH: nH, Item: c }); }); });
//grid.forEach(r => console.info(r.map(c => c.Item).join(''))); //print grid
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
    //console.info("Loop: " + nx + " D: " + d + " Move: " + r.DidMove); //print out progress
    if (r.DidMove == false) {
        return;
    }
    robot = r.Robot;
    r.Updates.forEach(function (u) { return grid[u.LocV][u.LocH] = u; });
});
//Print grid
grid.forEach(function (r) { return console.info(r.map(function (a) { return a.Item; }).join("")); });
var result = grid
    .flatMap(function (x) { return x; })
    .filter(function (x) { return x.Item == Item.BoxLeft; })
    .map(function (x) { return 100 * x.LocV + x.LocH; })
    .sum();
console.info("Result: " + result); //Part2: 1446175
function part2ExpandCell(c) {
    return c.replace("#", "##")
        .replace("O", "[]")
        .replace(".", "..")
        .replace("@", "@.")
        .split('');
}
function processMove(robot, direction) {
    var boxesToMove = [];
    var currentHead = [robot];
    while (true) {
        var next = currentHead.map(function (l) { return getLocForStep(l, direction); });
        if (next.any(function (x) { return x === undefined; })) {
            throw new Error("Should not be able to go off map");
        }
        var nextCells = next.map(function (n) { return grid[n.LocV][n.LocH]; });
        if (nextCells.any(function (c) { return c.Item == Item.Wall; })) {
            return { DidMove: false, Updates: undefined, Robot: undefined };
        }
        if (nextCells.any(function (c) { return c.Item == Item.Robot; })) {
            throw new Error("Should not find robot on step!");
        }
        if (nextCells.all(function (x) { return x.Item == Item.Empty; })) {
            var updates = [];
            //start with empty spots where existing items are (any where boxes now go will be overwritten as updates processed in order)
            updates.pushRange(boxesToMove.concat(robot)
                .map(function (e) { return ({ LocV: e.LocV, LocH: e.LocH, Item: Item.Empty }); })); //Space where robot was must be empty
            var nextRobot = move(robot, direction);
            updates.push(nextRobot);
            updates.pushRange(boxesToMove
                .map(function (c) { return move(c, direction); }));
            return { DidMove: true, Updates: updates, Robot: nextRobot };
        }
        var nextBoxes = nextCells
            .filter(function (x) { return x.Item == Item.BoxLeft || x.Item == Item.BoxRight; })
            //bring in other side of box
            .flatMap(function (b) { return b.Item == Item.BoxLeft
            ? [b, grid[b.LocV][b.LocH + 1]]
            : [grid[b.LocV][b.LocH - 1], b]; })
            .distinct(); //remove duplicates (as if box straight ahead, will already have in!)
        boxesToMove.pushRange(nextBoxes);
        currentHead = direction == Direction.Left
            ? nextBoxes.filter(function (x) { return x.Item == Item.BoxLeft; })
            : direction == Direction.Right
                ? nextBoxes.filter(function (x) { return x.Item == Item.BoxRight; })
                : nextBoxes; //up or down is both
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