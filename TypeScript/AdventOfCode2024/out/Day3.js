"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var FileHelper_1 = require("./FileHelper");
var fileLines = FileHelper_1.default.LoadFileLines('Day3.txt');
var oneLine = fileLines
    .reduce(function (a, b) { return a + b; });
var forDoBlocks = oneLine
    .replace(/don't\(\).+?(?=do\(\))/g, "|"); //Match from "don't()" up to next "do()" - replace with | (i.e. rip out and ensure invalid for mul)
var matches = forDoBlocks
    .match(/mul\(\d+,\d+\)/g); //"/g" moves the pointer along to find all matches
var result = matches
    .map(function (line) { return line
    .replace("mul(", "")
    .replace(")", "")
    .split(",")
    .map(function (x) { return Number(x); })
    .reduce(function (a, b) { return a * b; }); })
    .reduce(function (a, b) { return a + b; }, 0);
console.info("Result (part2): " + result); //Is: 89349241
//# sourceMappingURL=Day3.js.map