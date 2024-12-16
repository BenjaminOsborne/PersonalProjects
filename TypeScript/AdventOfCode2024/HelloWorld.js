var message = 'Hello World';
message += " More";
console.log(message);
var numbers = [1, 2, 3]; // inferred to type number[]
numbers.push(4); // no error
var head = numbers[0]; // no error
var tail = numbers[3]; // no error
console.log(head);
console.log(tail);
console.log(numbers);
