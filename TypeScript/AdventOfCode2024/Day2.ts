import * as fs from 'fs';
var file = fs.readFileSync('Day2.txt','utf8');

var countSafe = file.split('\r\n') //split on new line
    .filter(x => x.length > 0)
    .filter(line =>{
        var nums = line.split(/\s+/).map(x => Number(x));
        
        var isUp = (nums[1] - nums[0]) > 0;
        for(var nx = 0; nx < nums.length - 1; nx++)
        {
            if(((nums[nx+1] - nums[nx]) > 0) != isUp)
            {
                return false;
            }
            var gap = Math.abs(nums[nx+1] - nums[nx]);
            if(gap < 1 || gap > 3)
            {
                return false;
            }
        }
        return true;
    })
    .length;
console.log("Result (part1): " + countSafe)