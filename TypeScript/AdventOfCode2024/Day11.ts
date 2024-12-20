import './Globals'
import FileHelper from './FileHelper';
import { error } from 'console';

type Link = { val: number, next: Link };

var loaded = FileHelper.LoadFile('Inputs\\Day11.txt')
    .split(' ');

var headLink: Link = undefined;
var totalNums = loaded.length;

for(var n = loaded.length-1; n >= 0; n--)
{
    var sNum = loaded[n];
    headLink = { val: Number(sNum), next: headLink } as Link;
}

for(var n = 0; n < 75; n++)
{
    var local = headLink;
    while(local !== undefined)
    {
        local = blink(local);
    }
    console.info("Loop: " + (n+1) + ". Count: " + totalNums)
}

console.info("Result: " + totalNums); //Part1: 186424 (25 loops)

function blink(link : Link) : Link
{
    if(link === undefined)
    {
        return undefined;
    }

    if(link.val == 0)
    {
        link.val = 1;
        return link.next;
    }
    const numLen = getDigits(link.val);
    if(numLen %2 == 0)
    {
        var str = link.val.toString();
        var num1 = Number(str.slice(0, numLen / 2));
        var num2 = Number(str.slice(numLen / 2));
        
        link.val = num1;
        var rePoint = link.next;
        link.next = { val: num2, next: rePoint } as Link;
        totalNums += 1;
        return rePoint;
    }

    link.val *= 2024;
    return link.next;
}

function getDigits(n: number)
{
    if (n < 10) return 1;
    if (n < 100) return 2;
    if (n < 1000) return 3;
    if (n < 10000) return 4;
    if (n < 100000) return 5;
    if (n < 1000000) return 6;
    if (n < 10000000) return 7;
    if (n < 100000000) return 8;
    if (n < 1000000000) return 9;
    if (n < 10000000000) return 10;
    if (n < 100000000000) return 11;
    if (n < 1000000000000) return 12;
    if (n < 10000000000000) return 13;
    if (n < 100000000000000) return 14;
    if (n < 1000000000000000) return 15;
    if (n < 10000000000000000) return 16;
    if (n < 100000000000000000) return 17;
    if (n < 1000000000000000000) return 18;
    if (n < 10000000000000000000) return 19;
    
    throw new Error("UNHANDLED NUMBER: " + n);
}