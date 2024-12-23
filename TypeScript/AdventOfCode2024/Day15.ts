import './Globals'
import FileHelper from './FileHelper';

var cells = FileHelper.LoadFileLinesAndCharacters('Inputs\\Day15.txt')

enum Item { Empty = ".", Wall = "#", BoxLeft = "[", BoxRight = "]", Robot = "@" }
type Location = { LocV: number, LocH: number }
type Cell = Location & { Item: Item }

enum Direction { Up = "^", Down = "v", Left = "<", Right = ">" }

type MoveResult = { DidMove: boolean, Updates: Cell[], Robot: Cell } 

var grid = cells
    .filter(x => x.length > 0 && x[0] == '#')
    .map(r => r.flatMap(c => part2ExpandCell(c)))
    .map((row, nV) => row.map((c,nH) => ({ LocV: nV, LocH: nH, Item: c as Item } as Cell)));
//grid.forEach(r => console.info(r.map(c => c.Item).join(''))); //print grid

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
        //console.info("Loop: " + nx + " D: " + d + " Move: " + r.DidMove); //print out progress
        if(r.DidMove == false)
        {
            return;
        }
        robot = r.Robot;
        r.Updates.forEach(u => grid[u.LocV][u.LocH] = u)
    });

//Print grid
grid.forEach(r => console.info(r.map(a => a.Item).join("")))

var result = grid
    .flatMap(x => x)
    .filter(x => x.Item == Item.BoxLeft)
    .map(x => 100 * x.LocV + x.LocH)
    .sum();
console.info("Result: " + result); //Part2: 1446175

function part2ExpandCell(c: string) : string[]
{
    return c.replace("#", "##")
            .replace("O", "[]")
            .replace(".", "..")
            .replace("@", "@.")
            .split('');
}

function processMove(robot: Cell, direction: Direction) : MoveResult
{
    const boxesToMove: Cell[] = [];
    var currentHead: Location[] = [robot];
    while(true)
    {
        const next = currentHead.map(l => getLocForStep(l, direction));
        if(next.any(x => x === undefined))
        {
            throw new Error("Should not be able to go off map")
        }

        const nextCells = next.map(n => grid[n.LocV][n.LocH]);
        if(nextCells.any(c => c.Item == Item.Wall))
        {
            return { DidMove: false, Updates: undefined, Robot: undefined };
        }
        if(nextCells.any(c => c.Item == Item.Robot))
        {
            throw new Error("Should not find robot on step!")
        }
        if(nextCells.all(x => x.Item == Item.Empty))
        {
            const updates = []

            //start with empty spots where existing items are (any where boxes now go will be overwritten as updates processed in order)
            updates.pushRange(
                boxesToMove.concat(robot)
                    .map(e => ({ LocV: e.LocV, LocH: e.LocH, Item: Item.Empty }))); //Space where robot was must be empty
            
            const nextRobot = move(robot, direction);
            updates.push(nextRobot);

            updates.pushRange(boxesToMove
                .map(c => move(c, direction)));
            
            return { DidMove: true, Updates: updates, Robot: nextRobot };
        }

        const nextBoxes = nextCells
            .filter(x => x.Item == Item.BoxLeft || x.Item == Item.BoxRight)
            //bring in other side of box
            .flatMap(b => b.Item == Item.BoxLeft
                    ? [b, grid[b.LocV][b.LocH+1]]
                    : [grid[b.LocV][b.LocH-1], b])
            .distinct(); //remove duplicates (as if box straight ahead, will already have in!)
        boxesToMove.pushRange(nextBoxes);

        currentHead = direction == Direction.Left
                        ? nextBoxes.filter(x => x.Item == Item.BoxLeft)
                        : direction == Direction.Right
                            ? nextBoxes.filter(x => x.Item == Item.BoxRight)
                            : nextBoxes; //up or down is both
    }
}

function move(cell: Cell, direction: Direction) : Cell
{
    const next = getLocForStep(cell, direction);
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