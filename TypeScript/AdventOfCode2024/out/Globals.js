"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
Array.prototype.sum = function () {
    return this.reduce(function (acc, x) { return acc + x; }, 0);
};
Array.prototype.sumFrom = function (select) {
    return this.map(select).reduce(function (acc, x) { return acc + x; }, 0);
};
Array.prototype.any = function (pred) {
    for (var nx = 0; nx < this.length; nx++) {
        if (pred(this[nx])) {
            return true;
        }
    }
    return false;
};
Array.prototype.first = function (pred) {
    for (var nx = 0; nx < this.length; nx++) {
        var item = this[nx];
        if (pred(item)) {
            return item;
        }
    }
    return undefined;
};
Array.prototype.groupBy = function (fnSelect) {
    //GroupItems tracked locally in map for (O(1)) lookup in loop.
    //Results in array to ensure returned stably in order of first encounter.
    var map = new Map();
    var results = [];
    for (var nx = 0; nx < this.length; nx++) {
        var item = this[nx];
        var key = fnSelect(item);
        var current = map.get(key);
        if (current === undefined) {
            var grpItem = { Key: key, Items: [item] };
            map.set(key, grpItem);
            results.push(grpItem);
        }
        else {
            current.Items.push(item); //push into existing grpResult
        }
    }
    return results;
};
Array.prototype.popOffFirst = function () {
    return { first: this[0], rest: this.slice(1) };
};
Array.prototype.distinct = function () {
    if (this.length <= 1) {
        return this.slice(); //if 0 or 1, can just return shallow copy. (MUST copy as caller expects new reference)
    }
    var results = [];
    this.forEach(function (item) {
        if (results.indexOf(item) < 0) {
            results.push(item);
        }
    });
    return results;
};
//# sourceMappingURL=Globals.js.map