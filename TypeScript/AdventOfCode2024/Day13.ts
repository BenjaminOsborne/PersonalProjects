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
        var offset = 10000000000000;
        return { PrizeX: Number(split[1]) + offset, PrizeY: Number(split[2]) + offset, ButA: bA, ButB: bB } as Setup
    })
    .map(getMinTokens)
    .reduce((agg,x) => agg + x, Number(0));
console.log("Result: " + tokens)

function getMinTokens(s: Setup) : number
{
    var numAPress = (s.PrizeY * s.ButB.MoveX - s.PrizeX * s.ButB.MoveY) / (s.ButA.MoveY * s.ButB.MoveX - s.ButA.MoveX * s.ButB.MoveY)
    var numBPress = (s.PrizeY * s.ButA.MoveX - s.PrizeX * s.ButA.MoveY) / (s.ButB.MoveY * s.ButA.MoveX - s.ButB.MoveX * s.ButA.MoveY);
    
    console.info("A Press: " + numAPress);
    console.info("B Press: " + numBPress);

    if(numAPress < 0 || numBPress < 0) //negative solutions not valid
    {
        return 0;
    }
    if(numAPress != Math.floor(numAPress) || numBPress != Math.floor(numBPress)) //must be discrete number of moves
    {
        return 0;
    }
    return numAPress * 3 + numBPress; //tokens cost "3" for an A press, just "1" for a B press.
}

function loadButton(line: string) : Button
{
    const split = line.replace(", Y", "").split('+');
    return { MoveX: Number(split[1]), MoveY: Number(split[2]) }
}

/*
2 initial equations:
numAPress * s.ButA.MoveX + numBPress * s.ButB.MoveX = s.PrizeX
numAPress * s.ButA.MoveY + numBPress * s.ButB.MoveY = s.PrizeY

**Calculate A by substituting for B**

numBPress = (s.PrizeX - numAPress * s.ButA.MoveX) / s.ButB.MoveX
numBPress =(s.PrizeY - numAPress * s.ButA.MoveY) / s.ButB.MoveY

(s.PrizeX - numAPress * s.ButA.MoveX) / s.ButB.MoveX = (s.PrizeY - numAPress * s.ButA.MoveY) / s.ButB.MoveY
(s.PrizeX - numAPress * s.ButA.MoveX) * s.ButB.MoveY = (s.PrizeY - numAPress * s.ButA.MoveY) * s.ButB.MoveX
s.PrizeX * s.ButB.MoveY - numAPress * s.ButA.MoveX * s.ButB.MoveY = s.PrizeY * s.ButB.MoveX - numAPress * s.ButA.MoveY * s.ButB.MoveX

numAPress * s.ButA.MoveY * s.ButB.MoveX - numAPress * s.ButA.MoveX * s.ButB.MoveY = s.PrizeY * s.ButB.MoveX - s.PrizeX * s.ButB.MoveY
numAPress = (s.PrizeY * s.ButB.MoveX - s.PrizeX * s.ButB.MoveY) / (s.ButA.MoveY * s.ButB.MoveX - s.ButA.MoveX * s.ButB.MoveY)

**Calculate B by substituting for A**

numAPress = (s.PrizeX - numBPress * s.ButB.MoveX) / s.ButA.MoveX
numAPress = (s.PrizeY - numBPress * s.ButB.MoveY) / s.ButA.MoveY

(s.PrizeX - numBPress * s.ButB.MoveX) / s.ButA.MoveX = (s.PrizeY - numBPress * s.ButB.MoveY) / s.ButA.MoveY
(s.PrizeX - numBPress * s.ButB.MoveX) * s.ButA.MoveY = (s.PrizeY - numBPress * s.ButB.MoveY) * s.ButA.MoveX
s.PrizeX * s.ButA.MoveY - numBPress * s.ButB.MoveX * s.ButA.MoveY = s.PrizeY * s.ButA.MoveX - numBPress * s.ButB.MoveY * s.ButA.MoveX
numBPress * s.ButB.MoveY * s.ButA.MoveX - numBPress * s.ButB.MoveX * s.ButA.MoveY = s.PrizeY * s.ButA.MoveX - s.PrizeX * s.ButA.MoveY
numBPress = (s.PrizeY * s.ButA.MoveX - s.PrizeX * s.ButA.MoveY) / (s.ButB.MoveY * s.ButA.MoveX - s.ButB.MoveX * s.ButA.MoveY)
*/