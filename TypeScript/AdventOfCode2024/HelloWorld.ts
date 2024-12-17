let message: string = 'Hello World';
message += " More";
console.log(message);

const numbers = [1, 2, 6]; // inferred to type number[]
numbers.push(4); // no error
let head: number = numbers[0]; // no error
let tail: number = numbers[3]; // no error
console.log(head);
console.log(tail);
console.log(numbers);