import './Globals'
import FileHelper from './FileHelper';

type Location = { PosVer: number, PosHor: number }

const lines = FileHelper.LoadFileLinesWithMap('Inputs\\Day18.txt', l =>
    {
        const arr = l.split(',').map(x => Number(x));
        return { PosHor: arr[0], PosVer: arr[1] } as Location
    });

const size = 71;
const take = 1024;

var grid: string[][] = Array(size).fill([]).map(_ => Array(size).fill('.'));
console.info(grid.length)
console.info(grid[0].length)

lines.slice(0, take).forEach(l => grid[l.PosVer][l.PosHor] = '#')

grid.forEach(x => console.info(x.join('')));