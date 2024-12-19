export { }

declare global
{
   interface Array<T> {
       sum(): number;
    }
}
 
Array.prototype.sum = function(): number {
   return this.reduce((acc, x) => acc + x, 0);
}