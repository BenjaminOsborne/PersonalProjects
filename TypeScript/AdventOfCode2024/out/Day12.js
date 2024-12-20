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
        var left = tryGetRegion(vLoc, hLoc - 1);
        var aboveMatches = above !== undefined && above.Id == val;
        var leftMatches = left !== undefined && left.Id == val;
        if (aboveMatches && leftMatches && above != left) {
            mergeRegionIn(loc, above, left);
        }
        else if (aboveMatches) {
            joinRegion(above, loc, leftMatches ? 0 : 2);
        }
        else if (leftMatches) {
            joinRegion(left, loc, aboveMatches ? 0 : 2);
        }
        else //create new region
         {
            setRegion(loc, { Id: val, Locations: [loc], Perimiter: 4 });
        }
    });
});
var result = regionMap
    .flatMap(function (x) { return x; })
    .distinct()
    .map(function (x) { return x.Locations.length * x.Perimiter; })
    .sum();
console.info("Result: " + result);
function mergeRegionIn(loc, main, mergeIn) {
    //Add locations
    mergeIn.Locations.forEach(function (l) {
        main.Locations.push(l);
        setRegion(l, main); //update map reference
    });
    main.Perimiter += mergeIn.Perimiter; //take permiter
    joinRegion(main, loc, 0); //0 as 2 extra already from joining in other block
}
function joinRegion(region, loc, extendPerim) {
    region.Locations.push(loc);
    region.Perimiter += extendPerim;
    setRegion(loc, region);
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