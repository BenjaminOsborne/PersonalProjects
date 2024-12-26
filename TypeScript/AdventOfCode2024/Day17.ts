import './Globals'
import FileHelper from './FileHelper';

const lines = FileHelper.LoadFileLines('Inputs\\Day17.txt')

type Register = { A: number, B: number, C: number };
type Result = { Output: number, JumpTo: number }

const register =
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

var pointer = 0;
const output: number[] = [];
while(pointer < (instructions.length-1))
{
    var result = process(instructions[pointer], instructions[pointer+1], register);
    var out = result?.Output;
    if(out !== undefined)
    {
        output.push(out);
    }
    pointer = result?.JumpTo ?? (pointer + 2);
}
console.info("Output: " + output.join(",")) //Wrong: 6,5,6,0,2,3,0,3,3

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