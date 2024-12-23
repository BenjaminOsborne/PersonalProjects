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
var Operation;
(function (Operation) {
    Operation["RotClock"] = "RotClock";
    Operation["RotAntiClock"] = "RotAntiClock";
    Operation["Step"] = "Step";
})(Operation || (Operation = {}));
var cells = FileHelper_1.default.LoadFileLinesAndCharacters('Inputs\\Day16_Test1.txt')
    .map(function (row, v) { return row.map(function (c, h) { return ({ LocV: v, LocH: h, Type: c }); }); });
var start = cells.flatMap(function (x) { return x; }).single(function (x) { return x.Type == CellType.Start; });
var end = cells.flatMap(function (x) { return x; }).single(function (x) { return x.Type == CellType.End; });
//# sourceMappingURL=Day16.js.map