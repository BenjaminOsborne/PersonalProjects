import './Globals'
import FileHelper from './FileHelper';

type Robot = { PosX: number, PosY: number, VelocityX: number, VelocityY: number };

const gridSizeX = 101;
const gridSizeY = 103;
const step = 100;
const midX = Math.floor(gridSizeX / 2);
const midY = Math.floor(gridSizeY / 2);

var robotStart = FileHelper.LoadFileLines('Inputs\\Day14.txt')
    .map(l =>
    {
        const lines = l.replace("p=", "")
            .replace("v=", "")
            .split(' ')
            .map(a => a.split(",").map(b => Number(b)));
        return { PosX: lines[0][0], PosY: lines[0][1], VelocityX: lines[1][0], VelocityY: lines[1][1] } as Robot;
    });

var postStep = robotStart
    .map(performStep);
var score = postStep
    .map(r => ({ Robot: r, Quadrant: getQuadrant(r) }))
    .filter(x => x.Quadrant != undefined)
    .groupBy(x => x.Quadrant)
    .map(x => x.Items.length)
    .reduce((agg, x) => agg * x, 1);
console.info("Result: " + score);

function performStep(r: Robot) : Robot
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
