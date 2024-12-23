"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var Utility = /** @class */ (function () {
    function Utility() {
    }
    Utility.arrayFill = function (size, getVal) {
        var arr = [];
        for (var i = 0; i < size; i++) {
            arr.push(getVal(i));
        }
        return arr;
    };
    return Utility;
}());
exports.default = Utility;
//# sourceMappingURL=Utility.js.map