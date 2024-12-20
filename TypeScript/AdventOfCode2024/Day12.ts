import './Globals'
import FileHelper from './FileHelper';

var cells = FileHelper.LoadFileLinesAndCharacters('Inputs\\Day12.txt');

type Location = { VLoc: number, HLoc: number };
type Region = { Id: string, Locations: Location[], Perimiter: number, Sides: number };

const vLocMax = cells.length;
const hLocMax = cells[0].length;

var regionMap: Region[][] = [];

cells.forEach((row, vLoc) =>
    row.forEach((val, hLoc) =>
    {
        var loc = { VLoc: vLoc, HLoc: hLoc } as Location;
        var above = tryGetRegion(vLoc-1, hLoc);
        var aboveRight = tryGetRegion(vLoc-1, hLoc+1);
        var aboveLeft = tryGetRegion(vLoc-1, hLoc-1);
        var left = tryGetRegion(vLoc, hLoc-1);
        var aboveMatches = above !== undefined && above.Id == val;
        var leftMatches = left !== undefined && left.Id == val;
        if(aboveMatches && leftMatches)
        {
            var addSides = aboveRight === above ? 0 : -2;
            if(above != left)
            {
                mergeRegionIn(loc, above, left, addSides);
            }
            else
            {
                joinRegion(above, loc, 0, addSides);
            }
        }
        else if(aboveMatches)
        {
            var addSides =
                aboveLeft === above && aboveRight === above
                    ? 4
                    : (aboveRight === above || aboveLeft == above)
                        ? 2
                        : 0;
            joinRegion(above, loc, leftMatches ? 0 : 2, addSides);
        }
        else if(leftMatches)
        {
            var addSides = aboveLeft === left ? 2 : 0;
            joinRegion(left, loc, aboveMatches ? 0 : 2, addSides);
        }
        else //create new region
        {
            var reg = { Id: val, Locations: [loc], Perimiter: 4, Sides: 4 };
            setRegion(loc, reg);
            printRegion(reg);
        }
    }));

console.info("----")
regionMap
    .flatMap(x => x)
    .distinct()
    .forEach(x => console.info("Region: " + x.Id + " Area: " + x.Locations.length + " Sides: " + x.Sides));
console.info("----")

var result = regionMap
    .flatMap(x => x)
    .distinct()
    .map(x => x.Locations.length * x.Sides)
    .sum();
console.info("Result: " + result);

function printRegion(reg: Region)
{
    if(reg.Id != "C")
    {
        return;
    }
    console.info("Region: " + reg.Id + " sides: " + reg.Sides);
}

function mergeRegionIn(loc: Location, main: Region, mergeIn: Region, addSides: number)
{
    //Add locations
    mergeIn.Locations.forEach(l =>
    {
        main.Locations.push(l);
        setRegion(l, main); //update map reference
    });
    main.Perimiter += mergeIn.Perimiter; //take permiter
    main.Sides += mergeIn.Sides; //same number of sides
    joinRegion(main, loc, 0, addSides); //0 as 2 extra already from joining in other block
}

function joinRegion(region: Region, loc: Location, extendPerim: number, addSides: number)
{
    region.Locations.push(loc);
    region.Perimiter += extendPerim;
    region.Sides += addSides;
    setRegion(loc, region);

    printRegion(region);
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