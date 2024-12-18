import * as fs from 'fs';

enum Direction { Up = '^', Right = '>', Down = 'v', Left = '<' }
type LocationCoords = { verticalIndex: number, horizontalIndex: number }
enum LocationState { Vacant, Visited, Blocked };
type LocationInfo = { location: LocationCoords, state: LocationState }

type StepResult = { isFinished: boolean, grid: LocationInfo[][], nextLoc: LocationCoords, direction: Direction }

var file = fs.readFileSync('Day6.txt','utf8');

var grid = file
    .split('\r\n')
    .map(arr => arr.split(''))
    .map((row, verNx) =>
        row.map((val, horNx) => (
            {
                location: { verticalIndex: verNx, horizontalIndex: horNx },
                state: parseCell(val),
                currentDirection: parseDirection(val),
            }
        )));

var verticalCount = grid.length;
var horizontalCount = grid[0].length;

var initial = grid.flatMap(row => row.filter(c => c.state == LocationState.Visited))[0];

var nextResult = nextStep(grid, initial.location, initial.currentDirection); //initialise
while(!nextResult.isFinished)
{
    nextResult = nextStep(nextResult.grid, nextResult.nextLoc, nextResult.direction);
}

var visitedCount = nextResult.grid
    .flatMap(v => v.filter(r => r.state == LocationState.Visited)).length;
console.info("Result: " + visitedCount); //Part1: 5461

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
        return { isFinished: true, grid: grid, nextLoc: current, direction }
    }
    if (isBlocked(grid, nextLoc)) //If blocked, rotate at current
    {
        return { isFinished: false, grid, nextLoc: current, direction: rotate(direction) }
    }
    
    //mark visited
    grid[nextLoc.verticalIndex][nextLoc.horizontalIndex] = { location: nextLoc, state: LocationState.Visited };
    return { isFinished: false, grid, nextLoc: nextLoc, direction: direction };
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