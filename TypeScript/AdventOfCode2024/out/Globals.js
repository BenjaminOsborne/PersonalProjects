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
Array.prototype.all = function (pred) {
    for (var nx = 0; nx < this.length; nx++) {
        if (pred(this[nx]) == false) {
            return false;
        }
    }
    return true;
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
Array.prototype.last = function (pred) {
    for (var nx = this.length - 1; nx >= 0; nx--) {
        var item = this[nx];
        if (pred(item)) {
            return item;
        }
    }
    return undefined;
};
Array.prototype.lastItem = function () {
    return this[this.length - 1];
};
Array.prototype.single = function (pred) {
    var found = undefined;
    for (var nx = 0; nx < this.length; nx++) {
        var item = this[nx];
        if (pred(item)) {
            if (found != undefined) {
                throw new Error("Single: Matched multiple items");
            }
            found = item;
        }
    }
    if (found == undefined) {
        throw new Error("Single did not find any items");
    }
    return found;
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
Array.prototype.pushRange = function (items) {
    var _this = this;
    items.forEach(function (i) { return _this.push(i); });
};
Map.prototype.getOrAdd = function (key, getVal) {
    var found = this.get(key);
    if (found !== undefined) {
        return found;
    }
    var created = getVal(key);
    this[key] = created;
    return created;
};
//# sourceMappingURL=Globals.js.map