import './Globals'
import FileHelper from './FileHelper';

var lines = FileHelper.LoadFileLines('Inputs\\Day13_Test.txt');

type Button = { MoveX: bigint, MoveY: bigint };
type Setup = { PrizeX: bigint, PrizeY: bigint, ButA: Button, ButB : Button };

var tokens = lines
    .map((x,nx) => ({ Line: x, Block: Math.floor(nx / 3) }))
    .groupBy(x => x.Block)
    .map(x =>
    {
        const bA = loadButton(x.Items[0].Line);
        const bB = loadButton(x.Items[1].Line);
        const split = x.Items[2].Line.replace(", Y", "").split('=');
        var offset: bigint = BigInt(10000000000000);
        offset = BigInt(0);
        return { PrizeX: BigInt(split[1]) + offset, PrizeY: BigInt(split[2]) + offset, ButA: bA, ButB: bB } as Setup
    })
    .map(getMinTokens)
    .reduce((agg,x) => agg + x, BigInt(0));
console.log("Result: " + tokens)

function getMinTokens(s: Setup) : bigint
{
    var unit: bigint = BigInt(1);
    var numAPress = ((s.PrizeX / s.ButA.MoveX) -
        (s.PrizeY * (s.ButB.MoveX / (s.ButA.MoveY * s.ButA.MoveX))))
        /
        (unit - (s.ButA.MoveY * s.ButB.MoveX) / (s.ButB.MoveY * s.ButA.MoveX));
    var numBPress = ((s.PrizeY / s.ButB.MoveY) - 
        (s.PrizeX * (s.ButA.MoveY / (s.ButB.MoveX * s.ButB.MoveY))))
        /
        (unit - (s.ButB.MoveX * s.ButA.MoveY) / (s.ButA.MoveX * s.ButB.MoveY));

    console.info("A Press: " + numAPress);
    console.info("B Press: " + numBPress);

    if(numAPress < 0 || numBPress < 0)
    {
        return BigInt(0);
    }
    var three: bigint = BigInt(3);
    return numAPress * three + numBPress;
}

function loadButton(line: string) : Button
{
    const split = line.replace(", Y", "").split('+');
    return { MoveX: BigInt(split[1]), MoveY: BigInt(split[2]) }
}

/*
Button A: X+94, Y+34
Button B: X+22, Y+67
Prize: X=10000000008400, Y=10000000005400

A * 94 + B * 22 = PX
A * 34 + B * 67 = PY

A = PX / 94 - B * 22 / 94
B = PY / 67 - A * 34 / 67

B = PY / 67 - (PX / 94 - B * 22 / 94) * 34 / 67
B = PY / 67 - (PX / 94) * (34 / 67) + B * (22 / 94) * (34 / 67)
B = (PY / 67 - (PX * 34) / (94 * 67)) / (1 - (22 * 34) / (94 * 67)

A = PX / 94 - ((PY / 67) - A * (34 / 67)) * 22 / 94
A = PX / 94 - (PY / 67) * (22 / 94) + A * (34 / 67) * (22 / 94)
A = (PX / 94 - (PY * 22) / (67 * 94)) / (1 - (34 * 22) / (67 * 94))

*/