import { Console } from 'console';
import * as fs from 'fs';

var file = fs.readFileSync('Day1.txt','utf8');

const left = [] as number[];
const right = [] as number[];;
file.split("\r\n")
    .filter(l => l.length > 0)
    .forEach(l => {
    var nums = l.split(/\s/).filter(t => t.length > 0).map(t => Number(t));
    left.push(nums[0]);
    right.push(nums[1]);
    });

var orderedLeft = left.sort();
var orderedRight = right.sort();

var total = 0;
for(var nx = 0; nx < orderedLeft.length; nx++)
{
    total += Math.abs(orderedLeft[nx] - orderedRight[nx]);
}
console.info(total); //2580760 (for text file)