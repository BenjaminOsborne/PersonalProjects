"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
Array.prototype.sum = function () {
    return this.reduce(function (acc, x) { return acc + x; }, 0);
};
Array.prototype.any = function (pred) {
    for (var nx = 0; nx < this.length; nx++) {
        if (pred(this[nx])) {
            return true;
        }
    }
    return false;
};
Array.prototype.popOffFirst = function () {
    return { first: this[0], rest: this.slice(1) };
};
//# sourceMappingURL=Globals.js.map