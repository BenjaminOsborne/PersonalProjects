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
        return fs.readFileSync(location, 'utf8')
            .split("\r\n")
            .filter(function (x) { return x.length > 0; }); //removes empty lines
    };
    FileHelper.LoadFileLinesAndCharacters = function (location) {
        return FileHelper.LoadFileLinesWithMap(location, function (x) { return x.split(''); });
    };
    FileHelper.LoadFileLinesWithMap = function (location, fnMap) {
        return FileHelper.LoadFileLines(location).map(fnMap);
    };
    FileHelper.writeFile = function (location, data) {
        fs.writeFile(location, data, function (_) { });
    };
    FileHelper.appendFile = function (location, data) {
        fs.appendFile(location, data, function (_) { });
    };
    return FileHelper;
}());
exports.default = FileHelper;
//# sourceMappingURL=FileHelper.js.map