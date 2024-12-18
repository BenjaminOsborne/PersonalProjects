import * as fs from 'fs';
var file = fs.readFileSync('Day3.txt','utf8');
var oneLine = file.split('\r\n')
     .reduce((a,b) => a + b);

var forDoBlocks = oneLine
    .replace(/don't\(\).+?(?=do\(\))/g, "|"); //Match from "don't()" up to next "do()" - replace with | (i.e. rip out and ensure invalid for mul)
var matches = forDoBlocks
    .match(/mul\(\d+,\d+\)/g); //"/g" moves the pointer along to find all matches
var result = matches
    .map(line => line
        .replace("mul(", "")
        .replace(")", "")
        .split(",")
        .map(x => Number(x))
        .reduce((a,b) => a * b))
    .reduce((a,b) => a + b, 0);
console.info("Result (part2): " + result); //Is: 89349241