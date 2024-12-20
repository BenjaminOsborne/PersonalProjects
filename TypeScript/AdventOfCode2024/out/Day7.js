"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./Globals");
var FileHelper_1 = require("./FileHelper");
var fileLines = FileHelper_1.default.LoadFileLines('Inputs\\Day7.txt');
var Operator;
(function (Operator) {
    Operator["Add"] = "+";
    Operator["Multiply"] = "*";
    Operator["Concat"] = "||";
})(Operator || (Operator = {}));
var summed = fileLines
    .map(toLoadedInputs)
    .filter(canMakeValid)
    .reduce(function (agg, x) { return agg + x.Result; }, 0);
console.info("Result: " + summed); //Part2: 92148721834692
function canMakeValid(input) {
    var inputPop = input.Inputs.popOffFirst();
    var possible = spawnCombinations([{ Start: inputPop.first, Next: [] }], inputPop.rest);
    return possible.any(function (eq) {
        var eqPop = eq.Next.popOffFirst();
        var initial = apply(eq.Start, eqPop.first.Operator, eqPop.first.Value);
        var result = eqPop.rest //skip first as taken out for initial value
            .reduce(function (agg, x) { return apply(agg, x.Operator, x.Value); }, initial);
        return result == input.Result;
    });
    function spawnCombinations(current, remaining) {
        if (remaining.length == 0) {
            return current;
        }
        var next = remaining[0];
        var withNext = current.flatMap(function (e) {
            return [Operator.Add, Operator.Multiply, Operator.Concat].map(function (op) {
                return ({ Start: e.Start, Next: e.Next.concat([{ Operator: op, Value: next }]) });
            });
        });
        return spawnCombinations(withNext, remaining.slice(1)); //remove first from remaining (as taken in this loop)
    }
}
function apply(left, op, right) {
    switch (op) {
        case Operator.Add:
            return left + right;
        case Operator.Multiply:
            return left * right;
        case Operator.Concat:
            return Number(left.toString() + right.toString()); //13 || 985 => 13985
    }
}
function toLoadedInputs(line) {
    var parts = line.split(':');
    var inputs = parts[1]
        .split(' ')
        .filter(function (x) { return x.length > 0; })
        .map(function (x) { return Number(x); });
    return { Result: Number(parts[0]), Inputs: inputs };
}
//# sourceMappingURL=Day7.js.map