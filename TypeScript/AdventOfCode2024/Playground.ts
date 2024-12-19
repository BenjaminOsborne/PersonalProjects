import './Globals'

enum LocationState2 { Vacant, Visited, Blocked };

function parseCell(val: string) : LocationState2
{
    console.info(val);
    switch(val)
    {
        case '.':
            return LocationState2.Vacant;
        case '#':
            return LocationState2.Blocked;
        default:
            return LocationState2.Visited;
    }
}

console.info(parseCell('.').toString());

var s = "190: 10 19".split(':')
console.info("Num: " + Number(s[0]))
console.info("Sum: " + [1, 2, 3].sum());
console.info("Any: " + [1, 2, 3].any(x => x > 3));

[17, 1, 2, 3, 1, 5, 5, 17]
    .groupBy(x => x)
    .forEach(grp => console.info("Group Key: " + grp.Key + " | Items: " + grp.Items));

var arr = [];
arr[7] = "Hey";
console.info("Array: " + arr);

console.info("Filled: " + (new Array<number>(5)).fill(0, 0, 5))

console.info("Reversed: " + [0, 1, 2].sort((a,b) => b - a));