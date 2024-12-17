import * as fs from 'fs';

var file = fs.readFileSync('Day1.txt','utf8');

const left = [] as number[];
const right = [] as number[];

file.split(/\s+/) //split on all whitespace (newlines AND spaces between numbers)
    .map(x => Number(x)) //cast to number
    .forEach((num, nx) => //every other number alternates which array
        (nx % 2 == 0 ? left : right).push(num));

console.info(left);

var orderedLeft = left.sort();
var orderedRight = right.sort();
var total = orderedLeft
    .map((val, nx) => val - orderedRight[nx])
    .reduce((agg, val) => agg + Math.abs(val));

console.info("Result (Part1): " + total); //2580760 (for Day1.txt)

//Part 2
var rightMap = new Map<number, number>();
orderedRight.forEach(x => {
    var count = rightMap.get(x);
    rightMap.set(x, (count || 0) + 1);
})
var totalPart2 = orderedLeft
    .reduce((agg, x) => agg + (x * (rightMap.get(x) || 0)));

var totalPart2_2 = orderedLeft
    .reduce((agg5, x) => agg5 + (x * (orderedRight.filter(r => r == x).length)));

console.info("Result (Part2): " + totalPart2); //Initial guess: 25368459

console.info("Result (Part2_2): " + totalPart2_2); //Initial guess: 25368459
