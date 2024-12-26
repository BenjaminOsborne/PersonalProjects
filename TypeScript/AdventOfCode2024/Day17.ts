import './Globals'
import FileHelper from './FileHelper';

var a = Array(4).fill(8);
console.info(a);

const lines = FileHelper.LoadFileLines('Inputs\\Day17.txt')

type Register = { A: number, B: number, C: number };
type Result = { Output: number, JumpTo: number }

const regInit =
{
    A: loadRegister("A", lines[0]),
    B: loadRegister("B", lines[1]),
    C: loadRegister("C", lines[2])
} as Register;

const instructions = lines[3]
    .replace("Program: ", "")
    .split(',')
    .map(x => Number(x))

console.info("Inputs: " + instructions)

run().then(d => console.info("RegA: " + d))

async function run() : Promise<number>
{
    const perLoop = 10*1000*1000;
    var start = 7074000000;

    while(true)
    {
        console.info("Running from: " + start);

        var tasks = [0,1,2,3,4,5,6,7]
            .map(x => start + (x * perLoop))
            .map(s => runForRange(s, s + perLoop))
        var waited = await Promise.all(tasks);
        var found = waited.first(x => x !== undefined);
        if(found > 0)
        {
            return found;
        }
        start += tasks.length * perLoop;
    }
}

async function runForRange(startVal: number, endValExc: number) : Promise<number>
{
    var regA = startVal;
    while(regA < endValExc)
    {
        var output = outputMatchesInput(cloneWithA(regInit, regA));
        if(output)
        {
            return regA;
        }
        regA += 1;
    }
    return undefined;
}

function cloneWithA(reg: Register, valA: number) : Register
{
    return ({ A: valA, B: reg.B, C: reg.C }) as Register;
}

function outputMatchesInput(reg: Register) : boolean
{
    var pointer = 0;
    const output: number[] = [];
    while(pointer < (instructions.length-1))
    {
        var result = process(instructions[pointer], instructions[pointer+1], reg);
        var out = result?.Output;
        if(out !== undefined)
        {
            output.push(out);
            if(output.length > instructions.length ||
                instructions[output.length-1] != out)
            {
                return false;
            }
        }
        pointer = result?.JumpTo ?? (pointer + 2);
    }
    return (output.length == instructions.length);
}

function process(opCode: number, opLiteral: number, reg: Register) : Result
{
    var comboOp = fromComboOperand(opLiteral, reg);
    switch(opCode)
    {
        case 0: //adv
            reg.A = Math.floor(reg.A / (Math.pow(2, comboOp)));
            return undefined;
        case 1: //bxl
            reg.B = reg.B ^ opLiteral;
            return undefined;
        case 2: //bst
            reg.B = comboOp % 8;
            return undefined;
        case 3: //jnz
            if(reg.A == 0)
            {
                return undefined;
            }
            return { JumpTo: opLiteral, Output: undefined };
        case 4: //bxc
            reg.B = reg.B ^ reg.C;
            return undefined;
        case 5: //out
            return { Output: comboOp % 8, JumpTo: undefined };
        case 6: //bdv
            reg.B = Math.floor(reg.A / (Math.pow(2, comboOp)));
            return undefined;
        case 7:
            reg.C = Math.floor(reg.A / (Math.pow(2, comboOp)));
            return undefined;
    }
}

function fromComboOperand(operand: number, reg: Register) : number
{
    switch(operand)
    {
        case 0:
        case 1:
        case 2:
        case 3:
            return operand;
        case 4:
            return reg.A;
        case 5:
            return reg.B;
        case 6:
            return reg.C;
        case 7:
            return undefined;
    }
}

function loadRegister(letter: string, line: string) : number
{
    var split = line.split("Register " + letter + ": ");
    if(split.length == 2)
    {
        return Number(split[1])
    }
    throw new Error("Unexpected register: " + line);
}