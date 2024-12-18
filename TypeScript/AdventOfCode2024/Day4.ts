import * as fs from 'fs';
var file = fs.readFileSync('Day4.txt','utf8');

var map = file.split('\r\n')
    .map(l => l.split(''));

var xRange = map.length;
var yRange = map[0].length;

var counter = 0;
for(var nx = 0; nx < xRange; nx++)
{
    for(var ny = 0; ny < yRange; ny++)
    {
        if(map[nx][ny] != "X")
        {
            continue;
        }

        //hours going round clock
        counter += isMAS(nx,   ny-1, nx,   ny-2, nx,   ny-3); //12:00
        counter += isMAS(nx+1, ny-1, nx+2, ny-2, nx+3, ny-3); //01:30
        counter += isMAS(nx+1, ny,   nx+2, ny,   nx+3, ny  ); //03:00
        counter += isMAS(nx+1, ny+1, nx+2, ny+2, nx+3, ny+3); //04:30
        counter += isMAS(nx,   ny+1, nx,   ny+2, nx,   ny+3); //06:00
        counter += isMAS(nx-1, ny+1, nx-2, ny+2, nx-3, ny+3); //07:30
        counter += isMAS(nx-1, ny,   nx-2, ny,   nx-3, ny  ); //09:00
        counter += isMAS(nx-1, ny-1, nx-2, ny-2, nx-3, ny-3); //10:30
    }
}

console.log("Result: " + counter)

function isMAS(xM: number, yM: number, xA: number, yA: number, xS: number, yS: number)
{
    return equalsSafe(xM, yM, "M") &&
        equalsSafe(xA, yA, "A") &&
        equalsSafe(xS, yS, "S") ? 1 : 0;
}

function equalsSafe(x: number, y: number, c: string)
{
    if(x < 0 || y < 0)
    {
        return false;
    }
    if(x >= map.length)
    {
        return false;
    }
    var yArr = map[x];
    if(y >= yArr.length)
    {
        return false;
    }
    return yArr[y] == c;
}