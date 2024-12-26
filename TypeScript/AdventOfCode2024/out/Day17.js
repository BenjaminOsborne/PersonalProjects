"use strict";
var _a;
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var lines = FileHelper_1.default.LoadFileLines('Inputs\\Day17_Test.txt');
var register = {
    A: loadRegister("A", lines[0]),
    B: loadRegister("B", lines[1]),
    C: loadRegister("C", lines[2])
};
var instructions = lines[3]
    .replace("Program: ", "")
    .split(',')
    .map(function (x) { return Number(x); });
console.info("Inputs: " + instructions);
var pointer = 0;
var output = [];
while (pointer < (instructions.length - 1)) {
    var result = process(instructions[pointer], instructions[pointer + 1], register);
    var out = result === null || result === void 0 ? void 0 : result.Output;
    if (out !== undefined) {
        output.push(out);
    }
    pointer = (_a = result === null || result === void 0 ? void 0 : result.JumpTo) !== null && _a !== void 0 ? _a : (pointer + 2);
}
console.info("Output: " + output.join(","));
function process(opCode, operand, reg) {
    var fromCombOp = fromComboOperand(operand, reg);
    switch (opCode) {
        case 0: //adv
            reg.A = Math.floor(reg.A / (Math.pow(2, fromCombOp)));
            return undefined;
        case 1: //bxl
            reg.B = reg.B ^ operand;
            return undefined;
        case 2: //bst
            reg.B = operand % 8;
            return undefined;
        case 3: //jnz
            if (reg.A == 0) {
                return undefined;
            }
            return { JumpTo: operand, Output: undefined };
        case 4: //bxc
            reg.B = reg.B ^ reg.C;
            return undefined;
        case 5: //out
            return { Output: fromCombOp % 8, JumpTo: undefined };
        case 6: //bdv
            reg.B = Math.floor(reg.A / (Math.pow(2, fromCombOp)));
            return undefined;
        case 7:
            reg.C = Math.floor(reg.A / (Math.pow(2, fromCombOp)));
            return undefined;
    }
}
function fromComboOperand(operand, reg) {
    switch (operand) {
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
function loadRegister(letter, line) {
    var split = line.split("Register " + letter + ": ");
    if (split.length == 2) {
        return Number(split[1]);
    }
    throw new Error("Unexpected register: " + line);
}
//# sourceMappingURL=Day17.js.map