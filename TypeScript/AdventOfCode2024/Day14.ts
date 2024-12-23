import './Globals'
import Utility from './Utility'
import FileHelper from './FileHelper';

type Robot = { PosX: number, PosY: number, VelocityX: number, VelocityY: number };

var robotStart = FileHelper.LoadFileLines('Inputs\\Day14.txt')
    .map(l =>
    {
        const lines = l.replace("p=", "")
            .replace("v=", "")
            .split(' ')
            .map(a => a.split(",").map(b => Number(b)));
        return { PosX: lines[0][0], PosY: lines[0][1], VelocityX: lines[1][0], VelocityY: lines[1][1] } as Robot;
    });

const gridSizeX = 101;
const gridSizeY = 103;

const midX = Math.floor(gridSizeX / 2);
const midY = Math.floor(gridSizeY / 2);

const outputFile = 'Inputs\\Day14_Output.txt';
FileHelper.writeFile(outputFile, "");

for(var step = 0; step < gridSizeX * gridSizeY; step++)
{
    var stepped = robotStart
        .map(r => performStep(r, step));
    var inLine = stepped
        .groupBy(x => x.PosX)
        .map(x => getLongestContinuous(x.Items.map(i => i.PosY).sort())) //Finds longest in a given X position with no gaps
        .filter(l => l > 10); //Assume at least 11 in a line with no gaps
    if(inLine.length == 0)
    {
        continue;
    }

    console.info("Candidate: " + step)

    printContext(Utility.arrayFill(gridSizeX, () => '.').join(""))
    printContext("STEP: " + step);
    
    var display = Utility.arrayFill(gridSizeY, () => Utility.arrayFill(gridSizeX, () => '.'));
    stepped
        .groupBy(x => x.PosX + "|" + x.PosY)
        .forEach(x => display[x.Items[0].PosY][x.Items[0].PosX] = x.Items.length.toString());
    display.forEach(r => printContext(r.join("")))
}

function printContext(data: string)
{
    console.info(data);
    FileHelper.appendFile(outputFile, data + "\n");
}

function getLongestContinuous(pos: number[]) : number
{
    var longest = 0;
    var previous = -2;
    for(var n = 0; n < pos.length; n++)
    {
        var val = pos[n]
        if(val == previous + 1)
        {
            longest += 1;
        }
        else
        {
            longest = 1;
        }
        previous = val;
    }

    return longest;
}

function performStep(r: Robot, step: number) : Robot
{
    return (
    {
        PosX: normalise(r.PosX + step * r.VelocityX, gridSizeX),
        PosY: normalise(r.PosY + step * r.VelocityY, gridSizeY),
        VelocityX: r.VelocityX,
        VelocityY: r.VelocityY
    });
    function normalise(pos: number, size: number)
    {
        var raw = pos % size;
        return raw < 0 ? size + raw : raw;
    }
}

function getQuadrant(robot: Robot) : string
{
    if(robot.PosX == midX || robot.PosY == midY)
    {
        return undefined;
    }
    return (robot.PosX < midX ? "0" : "1") + "|" + (robot.PosY < midY ? "0" : "1");
}
