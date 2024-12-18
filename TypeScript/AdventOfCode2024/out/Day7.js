"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var fs = require("fs");
var file = fs.readFileSync('Day7.txt', 'utf8');
var Operator;
(function (Operator) {
    Operator["Add"] = "+";
    Operator["Multiply"] = "*";
    Operator["Concat"] = "||";
})(Operator || (Operator = {}));
var summed = file
    .split('\r\n')
    .map(toLoadedInputs)
    .filter(canMakeValid)
    .reduce(function (agg, x) { return agg + x.Result; }, 0);
console.info("Result: " + summed);
function canMakeValid(input) {
    var start = input.Inputs[0];
    var remain = input.Inputs.slice(1);
    var possible = spawn([{ Start: start, Next: [] }], remain);
    for (var nx = 0; nx < possible.length; nx++) {
        var eq = possible[nx];
        var first = eq.Next[0];
        var initial = apply(eq.Start, first.Operator, first.Value);
        var result = eq.Next.slice(1).reduce(function (agg, x) { return apply(agg, x.Operator, x.Value); }, initial);
        if (result == input.Result) {
            return true;
        }
    }
    return false;
    function spawn(current, remain) {
        if (remain.length == 0) {
            return current;
        }
        var next = remain[0];
        var withNext = current.flatMap(function (e) {
            return [Operator.Add, Operator.Multiply, Operator.Concat].map(function (op) {
                return ({ Start: e.Start, Next: e.Next.concat([{ Operator: op, Value: next }]) });
            });
        });
        return spawn(withNext, remain.slice(1));
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