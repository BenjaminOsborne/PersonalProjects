import './Globals'
import FileHelper from './FileHelper';

const rawLengths = [];
const rawGaps = [];
FileHelper.LoadFile('Day9.txt')
    .trim()
    .split('')
    .forEach((val, nx) =>
        (nx % 2 == 0 ? rawLengths : rawGaps).push(Number(val)));

type FileBlock = { StartIndex: number, FileId: number, FileLength: number }
type Gap = { StartIndex: number, GapLength: number }

const lengths: FileBlock[] = [];
const gaps: Gap[] = [];

//Build data
var currentIndex = 0;
for(var nx = 0; nx < rawLengths.length; nx++)
{
    var fileLen = rawLengths[nx];
    lengths.push({ StartIndex: currentIndex, FileId: nx, FileLength: fileLen });
    currentIndex += fileLen;

    var gap = rawGaps[nx];
    if(gap !== undefined) //Final element, will be no gap, so handle missing!
    {
        gaps.push({ StartIndex: currentIndex, GapLength: gap });
        currentIndex += gap;
    }
}

const finalDisk: number[] = [];

lengths
    .sort((a,b) => b.FileId - a.FileId) //Enumerate files in reverse
    .forEach(file =>
    {
        var gap = gaps.first(x => x.StartIndex < file.StartIndex && x.GapLength >= file.FileLength);
        if(gap !== undefined)
        {
            addFullFileBlock(file, gap.StartIndex);
            gap.GapLength -= file.FileLength;
            gap.StartIndex += file.FileLength;
        }
        else
        {
            addFullFileBlock(file, file.StartIndex);
        }
    });

var result = finalDisk
    .map((x,nx) => x > 0 ? x * nx : 0) //(> 0) filter also handles "undefined" gaps (which)
    .sum();
console.info("Result: " + result); //Part2: 6272188244509

function addFullFileBlock(block: FileBlock, startIndex: number)
{
    for(var nx = 0; nx < block.FileLength; nx++)
    {
        finalDisk[startIndex+nx] = block.FileId;
    }
}
