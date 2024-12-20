import './Globals'
import FileHelper from './FileHelper';

console.time('Run');

type NumScale = { num: number, multiplier: number };

var loaded = FileHelper.LoadFile('Inputs\\Day11.txt')
    .split(' ')
    .map(sNum => ({ num: Number(sNum), multiplier: 1 } as NumScale));

var input = loaded;
var blinkMap = new Map<number, number[]>();

for(var n = 0; n < 75; n++)
{
    input = blink(input);
    //console.info("Loop: " + (n+1) + ". UniqueNumbers: " + input.length + ". Total: " + input.sumFrom(x => x.multiplier));
}

console.info("Result: " + input.sumFrom(x => x.multiplier)) //Part2: 219838428124832

console.timeEnd('Run');

function blink(current: NumScale[]) : NumScale[]
{
    return current
        .flatMap(x =>
            blinkMap.getOrAdd(x.num, k => processNumberRules(k))
            .map(n =>
                ({ num: n, multiplier: x.multiplier } as NumScale)))
        .groupBy(x => x.num)
        .map(x =>
        {
            var first = x.Items[0];
            return x.Items.length == 1
                ? first
                : ({ num: first.num, multiplier: x.Items.sumFrom(i => i.multiplier) });
        });
}

function processNumberRules(num : number) : number[]
{
    if(num == 0)
    {
        return [ 1 ]
    }
    const strNum = num.toString();
    const numLen = strNum.length;
    if(numLen %2 == 0)
    {
        var splitAt = numLen / 2;
        return [
            new Number(strNum.slice(0, splitAt)) as number,
            new Number(strNum.slice(splitAt)) as number
        ];
    }
    return [ num * 2024 ]
}