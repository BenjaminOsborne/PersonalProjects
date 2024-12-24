import './Globals'
import FileHelper from './FileHelper';

enum CellType { Wall = '#', Space = '.', Start = "S", End = "E" }

enum Direction { Up = "^", Down = "v", Left = "<", Right = ">" }
const allDirs = [ Direction.Up, Direction.Down, Direction.Left, Direction.Right ] as ReadonlyArray<Direction>;

type Location = { LocV: number, LocH: number };
type Cell = Location & { Type: CellType };

enum Operation { Rotate =  "Rotate", Step = "Step" }

type RouteStep = { Previous: RouteStep, Operation: Operation, NewLoc: Cell, NewDir: Direction }

type NextOptions = { End: RouteStep, Options: RouteStep[] }

var cells = FileHelper.LoadFileLinesAndCharacters('Inputs\\Day16_Test1.txt')
    .map((row, v) => row.map((c, h) => ({ LocV: v, LocH: h, Type: c } as Cell)));

const start = cells.flatMap(x => x).single(x => x.Type == CellType.Start);
const end = cells.flatMap(x => x).single(x => x.Type == CellType.End);

const initialStep = { Previous: undefined, Operation: undefined, NewLoc: start, NewDir: Direction.Right } as RouteStep
var nextToWalk = generateNextOptions(initialStep).Options;

var routesToEnd: RouteStep[] = [];
while(nextToWalk.length > 0)
{
    var next = nextToWalk.map(generateNextOptions);
    routesToEnd.pushRange(next.filter(a => a.End !== undefined).map(b => b.End))
    nextToWalk = next.flatMap(x => x.Options);
}

var routes = routesToEnd
    .map(r => ({ route: r, score: scoreRoute(r)}))
    .sort((a,b) => b.score - a.score)

routes.forEach(x => console.info("Score: " + x.score + ".\n" + displayRoute(x.route)))

//console.info("Result: " + routes[0].score);

function displayRoute(route: RouteStep)
{
    var display = "";
    var prev = route;
    var steps = 0;
    while(prev !== undefined)
    {
        display = prev.NewDir + display;
        prev = prev.Previous;
        steps += 1;
    }
    return "[" + steps + "]: " + display;
}

function scoreRoute(init: RouteStep)
{
    var loc = init;
    var score = 0;
    while(loc !== undefined)
    {
        score += loc.Operation == Operation.Step
            ? 1
            : loc.Operation == Operation.Rotate
                ? 1000
                : 0;
        loc = loc.Previous;
    }
    return score;
}

function generateNextOptions(prev: RouteStep) : NextOptions
{
    const nextDirs = prev.Operation == Operation.Rotate
        ? [prev.NewDir] //If rotated before, can now only step, otherwise would turn around!
        : allDirs.filter(x => x != oppositeDir(prev.NewDir));

    const nextOptions = nextDirs
        .map(d => ({ cell: getCellAt(prev.NewLoc, d), dir: d }))
        .filter(x => x.cell.Type != CellType.Wall && hasNotVisited(x.cell, prev));
    const end = nextOptions.first(x => x.cell.Type == CellType.End);
    if(end !== undefined)
    {
        return { End: { Previous: prev, Operation: Operation.Step, NewLoc: end.cell, NewDir: prev.NewDir }, Options: [] };
    }
    var nextSteps = nextOptions.map(x => (
        {
            Previous: prev,
            Operation: x.dir == prev.NewDir ? Operation.Step : Operation.Rotate,
            NewLoc: x.cell,
            NewDir: x.dir
        } as RouteStep));
    return { End: undefined, Options: nextSteps };
}

function hasNotVisited(cell: Cell, route: RouteStep)
{
    var prev = route;
    while(prev !== undefined)
    {
        if(cell === prev.NewLoc)
        {
            return false;
        }
        prev = prev.Previous;
    }
    return true;
}

function oppositeDir(dir: Direction)
{
    switch (dir)
    {
        case Direction.Up:
            return Direction.Down;
        case Direction.Down:
            return Direction.Up;
        case Direction.Left:
            return Direction.Right;
        case Direction.Right:
            return Direction.Left;
        default:
            throw new Error("Unreachable");
    }
}

function getCellAt(loc: Location, dir: Direction)
{
    switch (dir)
    {
        case Direction.Up:
            return cells[loc.LocV-1][loc.LocH];
        case Direction.Down:
            return cells[loc.LocV+1][loc.LocH];
        case Direction.Left:
            return cells[loc.LocV][loc.LocH-1];
        case Direction.Right:
            return cells[loc.LocV][loc.LocH+1];
        default:
            throw new Error("Unreachable");
    }
}