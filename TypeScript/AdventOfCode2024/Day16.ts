import './Globals'
import FileHelper from './FileHelper';

enum CellType { Wall = '#', Space = '.', Start = "S", End = "E" }

enum Direction { Up = "^", Down = "v", Left = "<", Right = ">" }
const allDirs = [ Direction.Up, Direction.Down, Direction.Left, Direction.Right ] as ReadonlyArray<Direction>;

type Location = { LocV: number, LocH: number };
type Cell = Location & { Type: CellType };

enum Operation { Rotate =  "R", Step = "S" }

type RouteStep = { Previous: RouteStep, Operation: Operation, NewLoc: Cell, NewDir: Direction, Score: number }

type NextOptions = { End: RouteStep, Options: RouteStep[] }

const cells = FileHelper.LoadFileLinesAndCharacters('Inputs\\Day16.txt') //Day16_Test2
    .map((row, v) => row.map((c, h) => ({ LocV: v, LocH: h, Type: c } as Cell)));

const routesToLocations = cells.map(r => r.map(v => [] as RouteStep[]));

const start = cells.flatMap(x => x).single(x => x.Type == CellType.Start);
const end = cells.flatMap(x => x).single(x => x.Type == CellType.End);

const initialStep = { Previous: undefined, Operation: undefined, NewLoc: start, NewDir: Direction.Right, Score: 0 } as RouteStep
var nextToWalk = generateNextOptions(initialStep).Options;

var routesToEnd: RouteStep[] = [];
var loop = 0;
while(nextToWalk.length > 0)
{
    console.info("Loop: " + ++loop + "\tHeads: " + nextToWalk.length)
    var next = nextToWalk.map(generateNextOptions);
    routesToEnd.pushRange(next.filter(a => a.End !== undefined).map(b => b.End))
    
    //filter/update "routesToLocations" with best score to get to that point
    nextToWalk = [];
    next.flatMap(x => x.Options)
        .sort((a,b) => a.Score - b.Score) //sort ascending on score (so if multiple routes to same point, lowest score kept!)
        .forEach(r =>
        {
            var routesAtLoc = routesToLocations[r.NewLoc.LocV][r.NewLoc.LocH];
            var existRoute = routesAtLoc.first(x => x.NewDir == r.NewDir);

            //NOTE: could use "existRoute.Score <= r.Score" if JUST cared about best score. But part2 wants all walked cells, so must also take equal on score
            if(existRoute !== undefined && existRoute.Score < r.Score)
            {
                return;
            }
            var updated = routesAtLoc.filter(x => x != existRoute);
            updated.push(r);
            routesToLocations[r.NewLoc.LocV][r.NewLoc.LocH] = updated;
            nextToWalk.push(r);
        })
}

var routes = routesToEnd
    .sort((a,b) => a.Score - b.Score) //ascending
    //.sort((a,b) => b.score - a.score) //descending

var best = routes[0];   
console.info("Route:" + displayRoute(best))
console.info("Score: " + best.Score) //Part1: 90440

var equalBestRouteCells = routesToEnd
    .filter(x => x.Score == best.Score)
    .flatMap(x => getAllCellsOnRoute(x))
    .distinct()
    .length;
console.info("Cells on best routes: " + equalBestRouteCells) //Part2: 479

function getAllCellsOnRoute(route: RouteStep) : Cell[]
{
    var cells: Cell[] = [];
    var pre = route;
    while(pre !== undefined)
    {
        cells.push(pre.NewLoc);
        pre = pre.Previous;
    }
    return cells;
}

function displayRoute(route: RouteStep)
{
    var display = "";
    var prev = route;
    var steps = 0;
    while(prev !== undefined)
    {
        display = (prev.Operation ?? "") + display;
        prev = prev.Previous;
        steps += 1;
    }
    return "Length: [" + steps + "]. Path: " + display;
}

function generateNextOptions(prev: RouteStep) : NextOptions
{
    const nextDirs = prev.Operation == Operation.Rotate
        ? [prev.NewDir] //If rotated before, can now only step, otherwise would turn around!
        : allDirs.filter(x => x != oppositeDir(prev.NewDir));

    const nextOptions = nextDirs
        .map(d => ({ cell: getCellAt(prev.NewLoc, d), dir: d }))
        .filter(x => x.cell.Type != CellType.Wall);
    const end = nextOptions.first(x => x.cell.Type == CellType.End);
    if(end !== undefined)
    {
        var endRoute = { Previous: prev, Operation: Operation.Step, NewLoc: end.cell, NewDir: prev.NewDir, Score: prev.Score +1 } as RouteStep;
        return { End: endRoute, Options: [] };
    }
    var nextSteps = nextOptions.map(x =>
        {
            const isStep = x.dir == prev.NewDir;
            return ({
                Previous: prev,
                Operation: isStep ? Operation.Step : Operation.Rotate,
                NewLoc: isStep ? x.cell : prev.NewLoc, //only move if not rotating
                NewDir: x.dir,
                Score: prev.Score + (isStep ? 1 : 1000) 
            }) as RouteStep;
        });
    if(nextSteps.length == 1)
    {
        return generateNextOptions(nextSteps[0]); //if only 1 option - just walk immediately and short-cut "best route" analysis in caller
    }
    return { End: undefined, Options: nextSteps };
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