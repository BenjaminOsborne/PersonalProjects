export default class Utility
{
    static arrayFill<T>(size: number, getVal: (index: number) => T) : T[]
    {
        var arr = [];
        for(var i = 0; i < size; i++)
        {
            arr.push(getVal(i));
        }
        return arr;
    }
}