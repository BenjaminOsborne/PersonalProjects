import * as fs from 'fs';
var file = fs.readFileSync('Day5.txt','utf8');
var allLines = file.split('\r\n');

type Rule = { left: number; right: number };
type ReorderResult = { update: number[]; isReordered: boolean };

var rules = allLines
    .filter(l => l.includes('|'))
    .map(l => {
        var rule = l.split('|').map(x => Number(x));
        return { left: rule[0], right: rule[1] } as Rule;
    });

var updates = allLines
    .filter(l => l.includes(','))
    .map(l => l.split(',').map(x => Number(x)));

    var total = updates
    .map(arr => getReorderResult(arr, false, 1)) //startIndex:1 as first element cannot break a rule!
    .filter(x => x.isReordered)
    .map(x => x.update[Math.floor(x.update.length / 2)])
    .reduce((agg,x) => agg + x, 0);

console.info("Result: " + total); //Part2: 5448

function getReorderResult(update: number[], isReordered: boolean, startIndex: number) : ReorderResult
{
    for(var nx = 1; nx < update.length; nx++)
    {
        var currentVal = update[nx];
        var matchedRules = rules.filter(x => x.left == currentVal);
        if(matchedRules.length == 0)
        {
            continue;
        }
        var previous = update.slice(0, nx); //everything up to index (not including!)
        var failedRules = matchedRules
            .filter(rule => previous.filter(x => x == rule.right).length > 0);
        if(failedRules.length == 0)
        {
            continue;
        }
        var firstFailed = failedRules[0];
        var start = previous.filter(x => x != firstFailed.right); //everything except the item ("rule.right") to bump forwards
        var middle = [currentVal, firstFailed.right]; //the current value with the "rule.right" after it
        var end = update.slice(nx+1); //everything left in the array after the current value
        var next = start.concat(middle).concat(end); //combine to next call
        return getReorderResult(next, true, nx); //startIndex current (NOT nx+1), as element shuffled forwards so "next val" will now be at "nx"

    }
    return { update, isReordered } as ReorderResult;
}