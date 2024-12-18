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