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
        return fs.readFileSync(location,'utf8')
            .split("\r\n")
            .filter(x => x.length > 0); //removes empty lines
    }

    static LoadFileLinesAndCharacters(location: string) : string[][]
    {
        return FileHelper.LoadFileLinesWithMap(location, x => x.split(''));
    }

    static LoadFileLinesWithMap<T>(location: string, fnMap: MapLambda<T>) : T[]
    {
        return FileHelper.LoadFileLines(location).map(fnMap);
    }

    static writeFile(location: string, data: string)
    {
        fs.writeFile(location, data, _ => { });
    }

    static appendFile(location: string, data: string)
    {
        fs.appendFile(location, data, _ => { });
    }
}