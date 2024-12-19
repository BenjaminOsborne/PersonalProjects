export { }

declare global
{
   type Predicate<T> = (val: T) => boolean;

   type Selector<T1,T2> = (input: T1) => T2;

   type GroupedItem<TKey, T> = { Key: TKey, Items: T[] }

   interface Array<T> {
       sum(): number;
       any(pred: Predicate<T>): boolean;
       groupBy<TKey>(fnSelect: Selector<T, TKey>): GroupedItem<TKey, T>[];
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

Array.prototype.groupBy = function<TKey, T>(fnSelect: Selector<T, TKey>): GroupedItem<TKey, T>[]
{
   var map = new Map<TKey, T[]>();
   var results = [];
   for(var nx = 0; nx < this.length; nx++)
   {
      var item = this[nx];
      var key = fnSelect(item);
      var current = map.get(key);
      if(current === undefined)
      {
         var arr = [item];
         map.set(key, arr)
         results.push({ Key: key, Items: arr })
      }
      else
      {
         current.push(item);
      }
   }

   return results;
}

Array.prototype.popOffFirst = function()
{
   return { first: this[0], rest: this.slice(1) };
}