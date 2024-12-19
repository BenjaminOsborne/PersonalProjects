import './Globals'
import FileHelper from './FileHelper';

const rawLengths = [];
const gaps = [];
FileHelper.LoadFile('Day9_Test.txt')
    .trim()
    .split('')
    .forEach((val, nx) =>
        (nx % 2 == 0 ? rawLengths : gaps).push(Number(val)));

type FileBlock = { FileId: number, OriginalLength: number, Handled: boolean, CanMove: boolean }

const lengths = rawLengths
    .map((x,nx) => ({ FileId: nx, OriginalLength: x, Handled: false, CanMove: true } as FileBlock));
const lengthsReverse = lengths
    .slice() //shallow clone as require 2 distinct arrays.
    .sort((a,b) => b.FileId - a.FileId);

const finalDisk: number[] = [];

//Add first
var currentIndex = addFullFileBlock(lengths[0], 0);

//Enumerate gaps
for(var gNx = 0; gNx < gaps.length; gNx++)
{
    //Try fill in gap
    var gap = gaps[gNx];
    while(true)
    {
        var firstFillIn = scanForFileFromEndToFillGap(gap);
        if(firstFillIn !== undefined)
        {
            currentIndex = addFullFileBlock(firstFillIn, currentIndex);
            gap -= firstFillIn.OriginalLength;
            if(gap == 0)
            {
                break;
            }
        }
        else
        {
            currentIndex += gap;
            break;
        }
    }

    //Then take the next item
    var nextFillIn = lengths.filter(x => !x.Handled)[0];
    if (nextFillIn !== undefined)
    {
        currentIndex = addFullFileBlock(nextFillIn, currentIndex);
    }
    else
    {
        break; //If no blocks left with any, exit loop
    }
}

console.info("FinalDisk: " + finalDisk)

var result = finalDisk
    .map((x,nx) => x > 0 ? x * nx : 0) //Also handles "undefined" when gaps
    .sum();
console.info("Result: " + result); //Part1: 6242766523059

function scanForFileFromEndToFillGap(gap: number) : FileBlock
{
    for(var nx = 0; nx < lengthsReverse.length; nx++)
    {
        var item = lengthsReverse[nx];
        if( item.Handled)
        {
            continue;
        }
        if (item.CanMove && item.OriginalLength <= gap)
        {
            return item;
        }
        item.CanMove = false;
    }
    return undefined;
}

//Returns next index
function addFullFileBlock(block: FileBlock, startIndex: number)
{
    for(var nx = 0; nx < block.OriginalLength; nx++)
    {
        finalDisk[startIndex+nx] = block.FileId;
    }
    block.Handled = true;
    block.CanMove = false;
    return startIndex + block.OriginalLength;
}
