export { }

declare global
{
   type GroupedItem<TKey, T> = { Key: TKey, Items: T[] }

   interface Array<T> {
       sum(): number;
       any(pred: (val: T) => boolean): boolean;
       first(pred: (val: T) => boolean): T;
       groupBy<TKey>(fnSelect: (input: T) => TKey): GroupedItem<TKey, T>[];
       popOffFirst(): { first: T, rest: T[]};
       distinct(): T[];
    }
}
 
Array.prototype.sum = function(): number {
   return this.reduce((acc, x) => acc + x, 0);
}

Array.prototype.any = function<T>(pred: (val: T) => boolean): boolean
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

Array.prototype.first = function<T>(pred: (val: T) => boolean): T
{
   for(var nx = 0; nx < this.length; nx++)
   {
      var item = this[nx];
      if(pred(item))
      {
         return item;
      }
   }
   return undefined;
}

Array.prototype.groupBy = function<TKey, T>(fnSelect: (input: T) => TKey): GroupedItem<TKey, T>[]
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

Array.prototype.distinct = function()
{
   if(this.length <= 1)
   {
      return this.slice(); //if 0 or 1, can just return shallow copy. (MUST copy as caller expects new reference)
   }
   var results = [];
   this.forEach(item =>
   {
      if(results.indexOf(item) < 0)
      {
         results.push(item);
      }
   });
   return results;
}