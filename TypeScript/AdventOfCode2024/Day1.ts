import { Console } from 'console';
import * as fs from 'fs';

var file = fs.readFileSync('Day1.txt','utf8');

const left = [] as number[];
const right = [] as number[];

file.split(/\s+/) //split on all whitespace (newlines AND spaces between numbers)
    .map(x => Number(x)) //cast to number
    .forEach((num, nx) => //every other number alternates which array
        (nx % 2 == 0 ? left : right).push(num));

var orderedLeft = left.sort();
var orderedRight = right.sort();
var total = orderedLeft
    .map((val, nx) => val - orderedRight[nx])
    .reduce((agg, val) => agg + Math.abs(val));

console.info("Result: " + total); //2580760 (for Day1.txt)