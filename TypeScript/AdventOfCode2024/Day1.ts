import { Console } from 'console';
import * as fs from 'fs';

var file = fs.readFileSync('Day1.txt','utf8');

const left = [] as number[];
const right = [] as number[];
file.split("\r\n")
    .filter(line => line.length > 0)
    .forEach(line => {
        var nums = line
            .split(/\s+/) //match all whitespace
            .map(t => Number(t));
        left.push(nums[0]);
        right.push(nums[1]);
    });

var orderedLeft = left.sort();
var orderedRight = right.sort();
var total = orderedLeft
    .map((val, nx) => val - orderedRight[nx])
    .reduce((agg, val) => agg + Math.abs(val));

console.info(total); //2580760 (for Day1.txt)