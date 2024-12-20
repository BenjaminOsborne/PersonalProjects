"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var cells = FileHelper_1.default.LoadFileLinesAndCharacters('Inputs\\Day12.txt');
var vLocMax = cells.length;
var hLocMax = cells[0].length;
var regionMap = [];
cells.forEach(function (row, vLoc) {
    return row.forEach(function (val, hLoc) {
        var loc = { VLoc: vLoc, HLoc: hLoc };
        var above = tryGetRegion(vLoc - 1, hLoc);
        var aboveRight = tryGetRegion(vLoc - 1, hLoc + 1);
        var aboveLeft = tryGetRegion(vLoc - 1, hLoc - 1);
        var left = tryGetRegion(vLoc, hLoc - 1);
        var aboveMatches = above !== undefined && above.Id == val;
        var leftMatches = left !== undefined && left.Id == val;
        if (aboveMatches && leftMatches) {
            var addSides = aboveRight === above ? 0 : -2;
            if (above != left) {
                mergeRegionIn(loc, above, left, addSides);
            }
            else {
                joinRegion(above, loc, 0, addSides);
            }
        }
        else if (aboveMatches) {
            var addSides = aboveLeft === above && aboveRight === above
                ? 4
                : (aboveRight === above || aboveLeft == above)
                    ? 2
                    : 0;
            joinRegion(above, loc, leftMatches ? 0 : 2, addSides);
        }
        else if (leftMatches) {
            var addSides = aboveLeft === left ? 2 : 0;
            joinRegion(left, loc, aboveMatches ? 0 : 2, addSides);
        }
        else //create new region
         {
            var reg = { Id: val, Locations: [loc], Perimiter: 4, Sides: 4 };
            setRegion(loc, reg);
            printRegion(reg);
        }
    });
});
console.info("----");
regionMap
    .flatMap(function (x) { return x; })
    .distinct()
    .forEach(function (x) { return console.info("Region: " + x.Id + " Area: " + x.Locations.length + " Sides: " + x.Sides); });
console.info("----");
var result = regionMap
    .flatMap(function (x) { return x; })
    .distinct()
    .map(function (x) { return x.Locations.length * x.Sides; })
    .sum();
console.info("Result: " + result);
function printRegion(reg) {
    if (reg.Id != "C") {
        return;
    }
    console.info("Region: " + reg.Id + " sides: " + reg.Sides);
}
function mergeRegionIn(loc, main, mergeIn, addSides) {
    //Add locations
    mergeIn.Locations.forEach(function (l) {
        main.Locations.push(l);
        setRegion(l, main); //update map reference
    });
    main.Perimiter += mergeIn.Perimiter; //take permiter
    main.Sides += mergeIn.Sides; //same number of sides
    joinRegion(main, loc, 0, addSides); //0 as 2 extra already from joining in other block
}
function joinRegion(region, loc, extendPerim, addSides) {
    region.Locations.push(loc);
    region.Perimiter += extendPerim;
    region.Sides += addSides;
    setRegion(loc, region);
    printRegion(region);
}
function tryGetRegion(vLoc, hLoc) {
    if (vLoc < 0 || hLoc < 0 || vLoc >= vLocMax || hLoc >= hLocMax) {
        return undefined;
    }
    var v = regionMap[vLoc];
    return v !== undefined ? v[hLoc] : undefined;
}
function setRegion(location, region) {
    var v = regionMap[location.VLoc];
    if (v === undefined) {
        regionMap[location.VLoc] = (v = []);
    }
    v[location.HLoc] = region;
}
//# sourceMappingURL=Day12.js.map