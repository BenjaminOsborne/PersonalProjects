import './Globals'
import FileHelper from './FileHelper';

var cells = FileHelper.LoadFileLinesAndCharacters('Inputs\\Day12.txt');

type Location = { VLoc: number, HLoc: number };
type Region = { Id: string, Locations: Location[], Perimiter: number };

const vLocMax = cells.length;
const hLocMax = cells[0].length;

var regionMap: Region[][] = [];

cells.forEach((row, vLoc) =>
    row.forEach((val, hLoc) =>
    {
        var loc = { VLoc: vLoc, HLoc: hLoc } as Location;
        var above = tryGetRegion(vLoc-1, hLoc);
        var left = tryGetRegion(vLoc, hLoc-1);
        var aboveMatches = above !== undefined && above.Id == val;
        var leftMatches = left !== undefined && left.Id == val;
        if(aboveMatches && leftMatches && above != left)
        {
            mergeRegionIn(loc, above, left);
        }
        else if(aboveMatches)
        {
            joinRegion(above, loc, leftMatches ? 0 : 2);
        }
        else if(leftMatches)
        {
            joinRegion(left, loc, aboveMatches ? 0 : 2);
        }
        else //create new region
        {
            setRegion(loc, { Id: val, Locations: [loc], Perimiter: 4 });
        }
    }));

var result = regionMap
    .flatMap(x => x)
    .distinct()
    .map(x => x.Locations.length * x.Perimiter)
    .sum();
console.info("Result: " + result);

function mergeRegionIn(loc: Location, main: Region, mergeIn: Region)
{
    //Add locations
    mergeIn.Locations.forEach(l =>
    {
        main.Locations.push(l);
        setRegion(l, main); //update map reference
    });
    main.Perimiter += mergeIn.Perimiter; //take permiter
    joinRegion(main, loc, 0); //0 as 2 extra already from joining in other block
}

function joinRegion(region: Region, loc: Location, extendPerim: number)
{
    region.Locations.push(loc);
    region.Perimiter += extendPerim;
    setRegion(loc, region);
}

function tryGetRegion(vLoc: number, hLoc: number) : Region
{
    if(vLoc < 0 || hLoc < 0 || vLoc >= vLocMax || hLoc >= hLocMax)
    {
        return undefined;
    }
    var v = regionMap[vLoc];
    return v !== undefined ? v[hLoc] : undefined;
}

function setRegion(location: Location, region: Region)
{
    var v = regionMap[location.VLoc];
    if(v === undefined)
    {
        regionMap[location.VLoc] = (v = []);
    }
    v[location.HLoc] = region;
}