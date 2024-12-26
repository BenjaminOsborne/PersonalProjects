"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g = Object.create((typeof Iterator === "function" ? Iterator : Object).prototype);
    return g.next = verb(0), g["throw"] = verb(1), g["return"] = verb(2), typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (g && (g = 0, op[0] && (_ = 0)), _) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var a = Array(4).fill(8);
console.info(a);
var lines = FileHelper_1.default.LoadFileLines('Inputs\\Day17.txt');
var regInit = {
    A: loadRegister("A", lines[0]),
    B: loadRegister("B", lines[1]),
    C: loadRegister("C", lines[2])
};
var instructions = lines[3]
    .replace("Program: ", "")
    .split(',')
    .map(function (x) { return Number(x); });
console.info("Inputs: " + instructions);
run().then(function (d) { return console.info("RegA: " + d); });
function run() {
    return __awaiter(this, void 0, void 0, function () {
        var perLoop, start, tasks, waited, found;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    perLoop = 10 * 1000 * 1000;
                    start = 7074000000;
                    _a.label = 1;
                case 1:
                    if (!true) return [3 /*break*/, 3];
                    console.info("Running from: " + start);
                    tasks = [0, 1, 2, 3, 4, 5, 6, 7]
                        .map(function (x) { return start + (x * perLoop); })
                        .map(function (s) { return runForRange(s, s + perLoop); });
                    return [4 /*yield*/, Promise.all(tasks)];
                case 2:
                    waited = _a.sent();
                    found = waited.first(function (x) { return x !== undefined; });
                    if (found > 0) {
                        return [2 /*return*/, found];
                    }
                    start += tasks.length * perLoop;
                    return [3 /*break*/, 1];
                case 3: return [2 /*return*/];
            }
        });
    });
}
function runForRange(startVal, endValExc) {
    return __awaiter(this, void 0, void 0, function () {
        var regA, output;
        return __generator(this, function (_a) {
            regA = startVal;
            while (regA < endValExc) {
                output = outputMatchesInput(cloneWithA(regInit, regA));
                if (output) {
                    return [2 /*return*/, regA];
                }
                regA += 1;
            }
            return [2 /*return*/, undefined];
        });
    });
}
function cloneWithA(reg, valA) {
    return ({ A: valA, B: reg.B, C: reg.C });
}
function outputMatchesInput(reg) {
    var _a;
    var pointer = 0;
    var output = [];
    while (pointer < (instructions.length - 1)) {
        var result = process(instructions[pointer], instructions[pointer + 1], reg);
        var out = result === null || result === void 0 ? void 0 : result.Output;
        if (out !== undefined) {
            output.push(out);
            if (output.length > instructions.length ||
                instructions[output.length - 1] != out) {
                return false;
            }
        }
        pointer = (_a = result === null || result === void 0 ? void 0 : result.JumpTo) !== null && _a !== void 0 ? _a : (pointer + 2);
    }
    return (output.length == instructions.length);
}
function process(opCode, opLiteral, reg) {
    var comboOp = fromComboOperand(opLiteral, reg);
    switch (opCode) {
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
            if (reg.A == 0) {
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