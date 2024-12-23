import './Globals'
import FileHelper from './FileHelper';

enum CellType { Wall = '#', Space = '.', Start = "S", End = "E" }
type Cell = { LocV: number, LocH: number, Type: CellType };
enum Operation { RotClock =  "RotClock", RotAntiClock = "RotAntiClock", Step = "Step" }
type RouteStep = { Previous: RouteStep, Location: Cell, Operation: Operation }

var cells = FileHelper.LoadFileLinesAndCharacters('Inputs\\Day16_Test1.txt')
    .map((row, v) => row.map((c, h) => ({ LocV: v, LocH: h, Type: c } as Cell)));

const start = cells.flatMap(x => x).single(x => x.Type == CellType.Start);
const end = cells.flatMap(x => x).single(x => x.Type == CellType.End);

