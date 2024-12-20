import './Globals'
import FileHelper from './FileHelper';

type Num = { num: number, asString: string };
var loaded = FileHelper.LoadFile('Inputs\\Day11.txt')
    .split(' ')
    .map(x => ({ num: Number(x), asString: x } as Num));

var input = loaded;

for(var n = 0; n < 75; n++)
{
    input = input.flatMap(getNext);
    console.info("Loop: " + (n+1) + ". Count: " + input.length)
}

console.info("Result: " + input.length)

function getNext(num : Num) : Num[]
{
    if(num.num == 0)
    {
        return [{ num: 1, asString: "1" }]
    }
    const numLen = num.asString.length;
    if(numLen %2 == 0)
    {
        var num1 = Number(num.asString.slice(0, numLen / 2));
        var num2 = Number(num.asString.slice(numLen / 2));
        return [
            { num: num1, asString: num1.toString() },
            { num: num2, asString: num2.toString() }
        ]
    }
    var next = num.num * 2024;
    return [{ num: next, asString: next.toString() },]
}