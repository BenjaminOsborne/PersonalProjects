import * as fs from 'fs';

var file = fs.readFileSync('Day7.txt','utf8');

type LoadedInputs = { Result: number, Inputs: number[] }
enum Operator { Add = "+", Multiply = "*" }
type OpPair = { Operator: Operator, Value: number }
type Equation = { Start: number, Next: OpPair[] }

var summed = file
    .split('\r\n')
    .map(toLoadedInputs)
    .filter(canMakeValid)
    .reduce((agg, x) => agg + x.Result, 0);
console.info("Result: " + summed);

function canMakeValid(input: LoadedInputs) : boolean
{
    var start = input.Inputs[0];
    var remain = input.Inputs.slice(1);
    var possible = spawn([ { Start: start, Next: [] }], remain);
    
    for(var nx = 0; nx < possible.length; nx++)
    {
        var eq = possible[nx];
        
        var first = eq.Next[0];
        var initial = apply(eq.Start, first.Operator, first.Value);
        var result = eq.Next.slice(1).reduce((agg, x) => apply(agg, x.Operator, x.Value), initial)
        if(result == input.Result)
        {
            return true;
        }
    }
    return false;

    function spawn(current: Equation[], remain: number[]) : Equation[]
    {
        if (remain.length == 0)
        {
            return current;
        }
        var next = remain[0];
        var withNext = current.flatMap(e =>
            [Operator.Add, Operator.Multiply].map(op =>
                ({ Start: e.Start, Next: e.Next.concat([{ Operator: op, Value: next }]) } as Equation)
            ));
        return spawn(withNext, remain.slice(1));
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