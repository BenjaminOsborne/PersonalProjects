export { }

declare global
{
   type Predicate<T> = (val: T) => boolean;

   interface Array<T> {
       sum(): number;
       any(pred: Predicate<T>): boolean;
       popOffFirst(): { first: T, rest: T[]};
    }
}
 
Array.prototype.sum = function(): number {
   return this.reduce((acc, x) => acc + x, 0);
}

Array.prototype.any = function<T>(pred: Predicate<T>): boolean
{
   for(var nx = 0; nx < this.length; nx++)
   {
      if(pred(this[nx]))
      {
         return true;
      }
   }
   return false;
}

Array.prototype.popOffFirst = function()
{
   return { first: this[0], rest: this.slice(1) };
}