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
Array.prototype.groupBy = function (fnSelect) {
    var map = new Map();
    var results = [];
    for (var nx = 0; nx < this.length; nx++) {
        var item = this[nx];
        var key = fnSelect(item);
        var current = map.get(key);
        if (current === undefined) {
            var arr = [item];
            map.set(key, arr);
            results.push({ Key: key, Items: arr });
        }
        else {
            current.push(item);
        }
    }
    return results;
};
Array.prototype.popOffFirst = function () {
    return { first: this[0], rest: this.slice(1) };
};
//# sourceMappingURL=Globals.js.map