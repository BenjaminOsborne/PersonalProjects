import * as fs from 'fs';

var file = fs.readFileSync('Day1.txt','utf8');

const left = [] as number[];
const right = [] as number[];

file.split(/\s+/) //split on all whitespace (newlines AND spaces between numbers)
    .map(x => Number(x)) //cast to number
    .forEach((num, nx) => //every other number alternates which array
        (nx % 2 == 0 ? left : right).push(num));

var orderedLeft = left.sort((a, b) => a - b);
var orderedRight = right.sort((a, b) => a - b);

var totalPart1 = orderedLeft
    .map((val, nx) => val - orderedRight[nx])
    .reduce((agg, val) => agg + Math.abs(val), 0);
console.info("Result (Part1): " + totalPart1); //2580760 (for Day1.txt)

//Part 2
var rightMap = new Map<number, number>();
orderedRight.forEach(val =>
    rightMap.set(val, (rightMap.get(val) || 0) + 1));
var totalPart2 = orderedLeft
    .reduce((agg, val) => agg + (val * (rightMap.get(val) || 0)), 0);
console.info("Result (Part2): " + totalPart2); //25358365
