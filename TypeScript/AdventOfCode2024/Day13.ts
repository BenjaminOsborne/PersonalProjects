import './Globals'
import FileHelper from './FileHelper';

var lines = FileHelper.LoadFileLines('Inputs\\Day13.txt');

type Button = { MoveX: number, MoveY: number };
type Setup = { PrizeX: number, PrizeY: number, ButA: Button, ButB : Button };

var tokens = lines
    .map((x,nx) => ({ Line: x, Block: Math.floor(nx / 3) }))
    .groupBy(x => x.Block)
    .map(x =>
    {
        const bA = loadButton(x.Items[0].Line);
        const bB = loadButton(x.Items[1].Line);
        const split = x.Items[2].Line.replace(", Y", "").split('=');
        return { PrizeX: Number(split[1]), PrizeY: Number(split[2]), ButA: bA, ButB: bB } as Setup
    })
    .map(getMinTokens)
    .sum();
console.log("Result: " + tokens)

function getMinTokens(s: Setup) : number
{
    var maxA = Math.floor(Math.min(s.PrizeX / s.ButA.MoveX, s.PrizeY / s.ButA.MoveY));

    var possible = 0;
    for(var nA = 0; nA <= maxA; nA++)
    {
        var gapX = s.PrizeX - nA * s.ButA.MoveX;
        var gapY = s.PrizeY - nA * s.ButA.MoveY;
        if(gapX % s.ButB.MoveX == 0)
        {
            var pressB = gapX / s.ButB.MoveX;
            if(gapY == pressB * s.ButB.MoveY)
            {
                var score = 3 * nA + pressB;
                possible = possible == 0 ? score : Math.min(possible, score);
            }
        }
    }

    return possible;
}

function loadButton(line: string) : Button
{
    const split = line.replace(", Y", "").split('+');
    return { MoveX: Number(split[1]), MoveY: Number(split[2]) }
}