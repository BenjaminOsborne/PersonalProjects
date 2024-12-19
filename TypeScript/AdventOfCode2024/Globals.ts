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
   //GroupItems tracked locally in map for (O(1)) lookup in loop.
   //Results in array to ensure returned stably in order of first encounter.
   var map = new Map<TKey, GroupedItem<TKey, T>>();
   var results = [] as GroupedItem<TKey, T>[];
   for(var nx = 0; nx < this.length; nx++)
   {
      var item = this[nx];
      var key = fnSelect(item);
      var current = map.get(key);
      if(current === undefined)
      {
         var grpItem = { Key: key, Items: [item] };
         map.set(key, grpItem)
         results.push(grpItem)
      }
      else
      {
         current.Items.push(item); //push into existing grpResult
      }
   }

   return results;
}

Array.prototype.popOffFirst = function()
{
   return { first: this[0], rest: this.slice(1) };
}