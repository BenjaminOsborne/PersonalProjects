import * as fs from 'fs';
var file = fs.readFileSync('Day5.txt','utf8');
var allLines = file.split('\r\n');

type Rule = { left: number; right: number };

var rules = allLines
    .filter(l => l.includes('|'))
    .map(l => {
        var rule = l.split('|').map(x => Number(x));
        return { left: rule[0], right: rule[1] } as Rule;
    });

var total = allLines
    .filter(l => l.includes(','))
    .map(l => l.split(',').map(x => Number(x)))
    .filter(arr => isValidUpdate(arr))
    .map(arr => arr[Math.floor(arr.length/2)])
    .reduce((agg,x) => agg + x, 0);

console.info("Result: " + total);

function isValidUpdate(update: number[])
{
    for(var nx = 1; nx < update.length; nx++)
    {
        var matchedRules = rules.filter(x => x.left == update[nx]);
        if(matchedRules.length == 0)
        {
            continue;
        }
        var previous = update.slice(0, nx);
        var brokenRules = matchedRules
            .filter(rule => previous.filter(x => x == rule.right).length > 0)
            .length > 0;
        if(brokenRules)
        {
            return false;
        }

    }
    return true;
}