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