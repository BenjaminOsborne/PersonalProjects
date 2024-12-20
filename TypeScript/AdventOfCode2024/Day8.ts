import './Globals'
import FileHelper from './FileHelper';

type Antenna = { Freq: string, vLoc: number, hLoc: number }

var fileCells = FileHelper.LoadFileLinesAndCharacters('Day8.txt');

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
            applyNodes(ant1, ant2) //Same one ignored in method
    )))

var result = nodeGrid.flatMap(x => x).filter(x => x).length;
console.info("Result (Part 2): " + result); //Part1: 361. Part2: 1249.

function applyNodes(ant1: Antenna, ant2: Antenna)
{
    if(ant1 === ant2) //Ignore if same node
    {
        return;
    }
    var hGap = ant2.hLoc - ant1.hLoc;
    var vGap = ant2.vLoc - ant1.vLoc;

    applyWithOperator(ant1, (loc, dist) => loc - dist);
    applyWithOperator(ant2, (loc, dist) => loc + dist);
    
    function applyWithOperator(ant: Antenna, op: (a: number, b: number) => number)
    {
        var cont = true;
        var antH = ant.hLoc;
        var antV = ant.vLoc;
        while(cont)
        {
            cont = markNode(antH, antV); //Also applies at antennas themselves now
            antH = op(antH, hGap);
            antV = op(antV, vGap);
        }
    }

    function markNode(v: number, h: number)
    {
        if(v < 0 || h < 0 || v >= vLen || h >= hLen)
        {
            return false;
        }
        nodeGrid[v][h] = true;
        return true;
    }
}