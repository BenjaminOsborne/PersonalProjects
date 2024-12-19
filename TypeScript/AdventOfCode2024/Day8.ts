import './Globals'
import FileHelper from './FileHelper';

type Antenna = { Freq: string, vLoc: number, hLoc: number }

var fileCells = FileHelper.LoadFileLinesWithMap('Day8.txt', x => x.split(''));

var grpAntenna = fileCells
    .flatMap((row, v) => row
        .map((val, h) => ({ Freq: val, vLoc: v, hLoc: h }))
        .filter(x => x.Freq != '.') //remove empty locations
    )
    .groupBy(x => x.Freq);

var nodeGrid = fileCells.map(v => v.map(h => false));
var vLen = nodeGrid.length;
var hLen = nodeGrid[0].length;

grpAntenna.forEach(grp =>
    grp.Items.forEach(ant1 =>
        grp.Items.forEach(ant2 =>
            applyNode(ant1, ant2)
    )))

var result = nodeGrid.flatMap(x => x).filter(x => x).length;
console.info("Result: " + result); //Part1: 361

function applyNode(ant1: Antenna, ant2: Antenna)
{
    if(ant1 === ant2)
    {
        return;
    }
    var hGap = ant2.hLoc - ant1.hLoc;
    var vGap = ant2.vLoc - ant1.vLoc;

    var hNode1 = ant1.hLoc - hGap;
    var vNode1 = ant1.vLoc - vGap;
    apply(hNode1, vNode1);

    var hNode2 = ant2.hLoc + hGap;
    var vNode2 = ant2.vLoc + vGap;
    apply(hNode2, vNode2);
    
    function apply(v: number, h: number)
    {
        if(v < 0 || h < 0 || v >= vLen || h >= hLen)
        {
            return;
        }
        nodeGrid[v][h] = true;
    }
}