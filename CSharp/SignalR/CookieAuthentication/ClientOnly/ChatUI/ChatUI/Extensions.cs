using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using ChatServiceLayer;
using JetBrains.Annotations;

namespace ChatUI
{
    public static class Extensions
    {
        /// <remarks> Minimises change events on <see cref="ObservableCollection{T}"/> by only replacing / adding / removing at the minimum number of indexes </remarks>
        public static void SetState<T>(this ObservableCollection<T> collection, [CanBeNull] ImmutableList<T> newItems) where T : class
        {
            SetState(collection, newItems, (a, b) => a == b);
        }

        public static void SetState<T>(this ObservableCollection<T> collection, [CanBeNull] ImmutableList<T> newItems, Func<T, T, bool> fnAreSame) where T : class
        {
            var items = newItems ?? ImmutableList<T>.Empty;

            //Remove excess
            while (collection.Count > items.Count)
            {
                collection.RemoveAt(collection.Count - 1);
            }

            //Assign items at existing collection indexes
            var limit = items.Count.Min(collection.Count);
            for (var nx = 0; nx < limit; nx++)
            {
                var item = items[nx];

                // ReSharper disable once RedundantCheckBeforeAssignment - this is not redundant: stops the collection change event being raised if item same
                if (fnAreSame(collection[nx], item) == false)
                {
                    collection[nx] = item;
                }
            }

            //Add missing items at end of collection
            for (var nx = collection.Count; nx < items.Count; nx++)
            {
                collection.Add(items[nx]);
            }
        }
    }
}
