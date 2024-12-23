import './Globals'
import FileHelper from './FileHelper';

var cells = FileHelper.LoadFileLinesAndCharacters('Inputs\\Day15.txt')

enum Item { Empty = ".", Wall = "#", Box = "O", Robot = "@" }
type Location = { LocV: number, LocH: number }
type Cell = Location & { Item: Item }

enum Direction { Up = "^", Down = "v", Left = "<", Right = ">" }

type MoveResult = { DidMove: boolean, Updates: Cell[], Robot: Cell } 

var grid = cells
    .filter(x => x.length > 0 && x[0] == '#')
    .map((row, nV) => row.map((c,nH) => ({ LocV: nV, LocH: nH, Item: c as Item } as Cell)));
const locVSize = grid.length;
const locHSize = grid[0].length;

var directions = cells
    .filter(x => x.length > 0 && x[0] != '#')
    .flatMap(x => x)
    .map(x => x as Direction);

var robot = grid.flatMap(x => x).single(x => x.Item == Item.Robot);

directions
    .forEach((d, nx) =>
    {
        var r = processMove(robot, d);
        if(r.DidMove == false)
        {
            return;
        }
        robot = r.Robot;
        r.Updates.forEach(u => grid[u.LocV][u.LocH] = u)
    });

var result = grid
    .flatMap(x => x)
    .filter(x => x.Item == Item.Box)
    .map(x => 100 * x.LocV+ x.LocH)
    .sum();
console.info("Result: " + result);

grid.forEach(r => console.info(r.map(a => a.Item).join("")))

function processMove(robot: Cell, direction: Direction) : MoveResult
{
    var cells: Cell[] = [];
    var current: Location = robot;
    while(true)
    {
        var next = getLocForStep(current, direction);
        if(next == undefined)
        {
            throw new Error("Should not be able to go off map")
        }
        const cell = grid[next.LocV][next.LocH];
        if(cell.Item == Item.Wall)
        {
            return { DidMove: false, Updates: undefined, Robot: undefined };
        }
        if(cell.Item == Item.Robot)
        {
            throw new Error("Should not find robot on step!")
        }

        if(cell.Item == Item.Empty)
        {
            const updates = []
            updates.push(({ LocV: robot.LocV, LocH: robot.LocH, Item: Item.Empty })); //Space where robot was must be empty
            
            const nextRobot = move(robot, direction);
            updates.push(nextRobot);

            cells.forEach(c => updates.push(move(c, direction)))
            
            return { DidMove: true, Updates: updates, Robot: nextRobot };
        }

        cells.push(cell);
        current = next;
    }
}

function move(cell: Cell, direction: Direction) : Cell
{
    var next = getLocForStep(cell, direction);
    return { LocV: next.LocV, LocH: next.LocH, Item: cell.Item };
}

function getLocForStep(location: Location, dir: Direction) : Location
{
    var locV = location.LocV;
    var locH = location.LocH;
    var nextV = locV + (dir == Direction.Up
        ? - 1
        : dir == Direction.Down ? 1 : 0);
        
    var nextH = locH + (dir == Direction.Left
        ? - 1
        : dir == Direction.Right ? 1 : 0);
    return (nextV < 0 || nextH < 0 || nextV >= locVSize || nextH >= locHSize)
        ? undefined
        : { LocV: nextV, LocH: nextH };
}