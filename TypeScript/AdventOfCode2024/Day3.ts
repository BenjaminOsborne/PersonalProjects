import * as fs from 'fs';
var file = fs.readFileSync('Day3.txt','utf8');

var regex = /mul\(\d+,\d+\)/g; //"/g" moves the pointer along to find all matches
var matches = file.match(regex);
var result = matches
    .map(line => line
        .replace("mul(", "")
        .replace(")", "")
        .split(",")
        .map(x => Number(x))
        .reduce((a,b) => a * b))
    .reduce((a,b) => a + b, 0);
console.info("Result (part1): " + result);