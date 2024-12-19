import * as fs from 'fs';

var file = fs.readFileSync('Day7.txt','utf8');

type LoadedInputs = { Result: number, Inputs: number[] }
enum Operator { Add = "+", Multiply = "*", Concat = "||" }
type OpPair = { Operator: Operator, Value: number }
type Equation = { Start: number, Next: OpPair[] }

var summed = file
    .split('\r\n')
    .map(toLoadedInputs)
    .filter(canMakeValid)
    .reduce((agg, x) => agg + x.Result, 0);
console.info("Result: " + summed); //Part2: 92148721834692

function canMakeValid(input: LoadedInputs) : boolean
{
    var start = input.Inputs[0];
    var remain = input.Inputs.slice(1);
    var possible = spawnCombinations([ { Start: start, Next: [] }], remain);
    
    for(var nx = 0; nx < possible.length; nx++)
    {
        var eq = possible[nx];
        var first = eq.Next[0];
        var initial = apply(eq.Start, first.Operator, first.Value);
        var result = eq.Next.slice(1) //skip 1 as first taken out for initial value
            .reduce((agg, x) => apply(agg, x.Operator, x.Value), initial)
        if (result == input.Result)
        {
            return true; //early exit when find first match
        }
    }
    return false;

    function spawnCombinations(current: Equation[], remaining: number[]) : Equation[]
    {
        if (remaining.length == 0)
        {
            return current;
        }
        var next = remaining[0];
        var withNext = current.flatMap(e =>
            [Operator.Add, Operator.Multiply, Operator.Concat].map(op =>
                ({ Start: e.Start, Next: e.Next.concat([{ Operator: op, Value: next }]) } as Equation)
            ));
        return spawnCombinations(withNext, remaining.slice(1)); //remove first from remaining (as taken in this loop)
    } 
}

function apply(left: number, op: Operator, right: number) : number
{
    switch(op)
    {
        case Operator.Add:
            return left + right;
        case Operator.Multiply:
            return left * right;
        case Operator.Concat:
            return Number(left.toString() + right.toString()); //13 || 985 => 13985
    }
}

function toLoadedInputs(line: string) : LoadedInputs
{
    var parts = line.split(':');
    var inputs = parts[1]
        .split(' ')
        .filter(x => x.length > 0)
        .map(x => Number(x));
    return { Result: Number(parts[0]), Inputs: inputs };
}