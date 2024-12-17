"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var fs = require("fs");
var file = fs.readFileSync('Day3.txt', 'utf8');
var regex = /mul\(\d+,\d+\)/g; //"/g" moves the pointer along to find all matches
var matches = file.match(regex);
var result = matches
    .map(function (line) { return line
    .replace("mul(", "")
    .replace(")", "")
    .split(",")
    .map(function (x) { return Number(x); })
    .reduce(function (a, b) { return a * b; }); })
    .reduce(function (a, b) { return a + b; }, 0);
console.info("Result (part1): " + result);
//# sourceMappingURL=Day3.js.map