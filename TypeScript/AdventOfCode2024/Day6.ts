import * as fs from 'fs';

enum Direction { Up = '^', Right = '>', Down = 'v', Left = '<' }
type LocationCoords = { verticalIndex: number, horizontalIndex: number }
enum LocationState { Vacant, Visited, Blocked };
type LocationInfo = { location: LocationCoords, state: LocationState, history: Direction[] }

type StepResult = { isFinished: boolean, isLoop: boolean, grid: LocationInfo[][], nextLoc: LocationCoords, direction: Direction }

var file = fs.readFileSync('Day6.txt','utf8');

var originalGrid = file
    .split('\r\n')
    .map(arr => arr.split(''))
    .map((row, verNx) =>
        row.map((val, horNx) => (
            {
                location: { verticalIndex: verNx, horizontalIndex: horNx },
                state: parseCell(val),
                history: [],
                currentDirection: parseDirection(val),
            }
        )));

var verticalCount = originalGrid.length;
var horizontalCount = originalGrid[0].length;

var initial = originalGrid.flatMap(row => row.filter(c => c.state == LocationState.Visited))[0];

var totalLoopsCount = 0;
for(var nv = 0; nv < verticalCount; nv++)
{
    for(var nh = 0; nh < horizontalCount; nh++)
    {
        var current = originalGrid[nv][nh];
        if(current.state != LocationState.Vacant)
        {
            continue;
        }

        //Clone grid as mutates state in nextStep routine
        var copyGrid = originalGrid.map(row => row.map(r => ({ location: r.location, state: r.state, history: [] } )));
        copyGrid[nv][nh].state = LocationState.Blocked;

        var nextResult = nextStep(copyGrid, initial.location, initial.currentDirection); //initialise
        while(!nextResult.isFinished && !nextResult.isLoop)
        {
            nextResult = nextStep(nextResult.grid, nextResult.nextLoc, nextResult.direction);
        }
        totalLoopsCount += nextResult.isLoop ? 1 : 0;
    }    
}

console.info("Result: " + totalLoopsCount); //Part1: 5461. Part2: 1836

function parseCell(val: string) : LocationState
{
    switch(val)
    {
        case '.':
            return LocationState.Vacant;
        case '#':
            return LocationState.Blocked;
        default:
            return LocationState.Visited;
    }
}

function parseDirection(val: string) : Direction
{
    switch(val)
    {
        case Direction.Up:
            return Direction.Up;
        case Direction.Down:
            return Direction.Down;
        case Direction.Left:
            return Direction.Left;
        case Direction.Right:
            return Direction.Right;
        default:
            return undefined;
    }
}

function nextStep(grid: LocationInfo[][], current: LocationCoords, direction: Direction) : StepResult
{
    var nextLoc = getNextLocation(current, direction);
    if (isOutOfRange(nextLoc)) //if next is out of range, then finished!
    {
        return { isFinished: true, isLoop: false, grid: grid, nextLoc: current, direction }
    }

    //mark visited
    var currentLocInfo = grid[current.verticalIndex][current.horizontalIndex];
    var nxtLocInfo = grid[nextLoc.verticalIndex][nextLoc.horizontalIndex];
    if (isBlocked(grid, nextLoc)) //If blocked, rotate at current
    {
        var rotated = rotate(direction);
        currentLocInfo.history.push(rotated); //track rotating at current position
        return { isFinished: false, isLoop: isLoop(currentLocInfo.history), grid, nextLoc: current, direction: rotated }
    }
    
    nxtLocInfo.state = LocationState.Visited;
    nxtLocInfo.history.push(direction);
    return { isFinished: false, isLoop: isLoop(nxtLocInfo.history), grid, nextLoc: nextLoc, direction: direction };
}

function isOutOfRange(current: LocationCoords) : boolean
{
    return current.verticalIndex < 0 || current.verticalIndex >= verticalCount ||
        current.horizontalIndex < 0 || current.horizontalIndex >= horizontalCount;
}

function isBlocked(grid: LocationInfo[][], current: LocationCoords) : boolean
{
    var locInfo = grid[current.verticalIndex][current.horizontalIndex];
    return locInfo.state == LocationState.Blocked;
}

function isLoop(history: Direction[])
{
    if (history.length <= 1)
    {
        return false;
    }

    var map = new Map<Direction, number>();
    for(var nx = 0; nx < history.length; nx++)
    {
        var dir = history[nx];
        var current = map.get(dir) || 0;
        if(current > 0)
        {
            return true;
        }
        map.set(dir, 1);
    }
    return false;
}

function getNextLocation(current: LocationCoords, direction: Direction) : LocationCoords
{
    switch(direction)
    {
        case Direction.Up:
            return { verticalIndex: current.verticalIndex - 1, horizontalIndex: current.horizontalIndex };
        case Direction.Down:
            return { verticalIndex: current.verticalIndex + 1, horizontalIndex: current.horizontalIndex };
        case Direction.Left:
            return { verticalIndex: current.verticalIndex, horizontalIndex: current.horizontalIndex - 1 };
        case Direction.Right:
            return { verticalIndex: current.verticalIndex, horizontalIndex: current.horizontalIndex + 1 };
        default:
            const exhaustiveCheck: never = direction;
            return undefined;
    }
}

function rotate(direction: Direction) : Direction
{
    switch(direction)
    {
        case Direction.Up:
            return Direction.Right;
        case Direction.Down:
            return Direction.Left;
        case Direction.Left:
            return Direction.Up;
        case Direction.Right:
            return Direction.Down;
        default:
            const exhaustiveCheck: never = direction;
            return undefined;
    }
}