"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var fs = require("fs");
var FileHelper = /** @class */ (function () {
    function FileHelper() {
    }
    FileHelper.LoadFile = function (location) {
        return fs.readFileSync(location, 'utf8');
    };
    FileHelper.LoadFileLines = function (location) {
        return fs.readFileSync(location, 'utf8').split("\r\n");
    };
    FileHelper.LoadFileLinesWithMap = function (location, fnMap) {
        return FileHelper.LoadFileLines(location).map(fnMap);
    };
    return FileHelper;
}());
exports.default = FileHelper;
//# sourceMappingURL=FileHelper.js.map