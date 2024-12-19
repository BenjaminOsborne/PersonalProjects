import './Globals'
import FileHelper from './FileHelper';

const rawLengths = [];
const gaps = [];
FileHelper.LoadFile('Day9.txt')
    .trim()
    .split('')
    .forEach((val, nx) =>
        (nx % 2 == 0 ? rawLengths : gaps).push(Number(val)));

type FileBlock = { FileId: number, OriginalLength: number, Remaining: number }

const lengths = rawLengths
    .map((x,nx) => ({ FileId: nx, OriginalLength: x, Remaining: x } as FileBlock));
const lengthsReverse = lengths.slice().sort((a,b) => b.FileId - a.FileId);

const finalDisk: number[] = [];

//Add first
addFullFileBlock(lengths[0])

//Enumerate gaps
for(var gNx = 0; gNx < gaps.length; gNx++)
{
    //Fill in gap
    processNextGap(gaps[gNx]);

    //Then take the next item
    var nextFillIn = lengths.filter(x => x.Remaining > 0)[0];
    if(nextFillIn !== undefined)
    {
        addFullFileBlock(nextFillIn);
    }
    else
    {
        break; //If no blocks left with any, exit loop
    }
}

var result = finalDisk
    .map((x,nx) => x * nx)
    .sum();
console.info("Result: " + result); //Part1: 6242766523059

function processNextGap(gap: number)
{
    var remainingGap = gap;
    while(true)
    {
        var usable = lengthsReverse.filter(x => x.Remaining > 0);
        if(usable.length == 0)
        {
            return;
        }
        var took = addFileBlockUpTo(usable[0], remainingGap);
        remainingGap -= took;
        if(remainingGap == 0)
        {
            return;
        }
    }
}

function addFullFileBlock(block: FileBlock)
{
    addFileBlockUpTo(block, block.Remaining);
}

function addFileBlockUpTo(block: FileBlock, maxTake: number) : number
{
    var take = Math.min(maxTake, block.Remaining);
    for(var nx = 0; nx < take; nx++)
    {
        finalDisk.push(block.FileId)
    }
    block.Remaining = block.Remaining - take;
    return take;
}