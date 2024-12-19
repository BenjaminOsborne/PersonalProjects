import * as fs from 'fs';

type MapLambda<T> = (input: string) => T;

export default class FileHelper
{
    static LoadFile(location: string) : string
    {
        return fs.readFileSync(location,'utf8');
    }

    static LoadFileLines(location: string) : string[]
    {
        return fs.readFileSync(location,'utf8').split("\r\n");
    }

    static LoadFileLinesWithMap<T>(location: string, fnMap: MapLambda<T>) : T[]
    {
        return FileHelper.LoadFileLines(location).map(fnMap);
    }
    
}