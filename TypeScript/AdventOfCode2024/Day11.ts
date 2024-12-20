import './Globals'
import FileHelper from './FileHelper';

type Num = { val: number, asString: string };
type NumScale = { num: Num, multiplier: number };

var loaded = FileHelper.LoadFile('Inputs\\Day11.txt')
    .split(' ')
    .map(x => ({ num: toNum(Number(x)), multiplier: 1 } as NumScale));

var input = loaded;
for(var n = 0; n < 75; n++)
{
    input = blink(input);
    console.info("Loop: " + (n+1) + ". UniqueNumbers: " + input.length + ". Total: " + input.sumFrom(x => x.multiplier));
}

console.info("Result: " + input.sumFrom(x => x.multiplier)) //Part2: 219838428124832

function blink(current: NumScale[]) : NumScale[]
{
    return current
        .flatMap(x =>
            processNumberRules(x.num)
            .map(n =>
                ({ num: n, multiplier: x.multiplier } as NumScale)))
        .groupBy(x => x.num.val)
        .map(x =>
        {
            var first = x.Items[0];
            return x.Items.length == 1
                ? first
                : ({ num: first.num, multiplier: x.Items.sumFrom(i => i.multiplier) });
        });
}

function processNumberRules(num : Num) : Num[]
{
    if(num.val == 0)
    {
        return [ toNum(1) ]
    }
    const numLen = num.asString.length;
    if(numLen %2 == 0)
    {
        return [
            toNum(Number(num.asString.slice(0, numLen / 2))),
            toNum(Number(num.asString.slice(numLen / 2)))
        ];
    }
    return [ toNum(num.val * 2024)]
}

function toNum(num: number) : Num { return { val: num, asString: num.toString() } }