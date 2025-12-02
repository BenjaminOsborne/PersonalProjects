using System.Collections.Immutable;

namespace BibleApp.Core;

public static partial class CollectionExtensions
{
    public static IEnumerable<IReadOnlyList<T>> GroupIntoBatches<T>(this IReadOnlyList<T> items, int batchSizeLimit) =>
        items.Count > 0
            ? items.Count <= batchSizeLimit
                ? [items] //if total at/below limit, return as single block
                : GroupIntoBatches(items.AsEnumerable(), batchSizeLimit) //inner impl. handles special cases of batchSizeLimit of 0 or 1.
            : []; //If no items, empty

    public static IEnumerable<IReadOnlyList<T>> GroupIntoBatches<T>(this IEnumerable<T> items, int batchSizeLimit)
    {
        if (batchSizeLimit <= 0)
        {
            var materialised = items.Materialise();
            return materialised.Any() //ONLY yield if any. Batch-size of zero with zero items should yield nothing (NOT a single empty collection!)
                ? [materialised]
                : [];
        }
        return batchSizeLimit == 1
            ? items.Select(ImmutableList.Create) //Each item as batch of 1
            : items.GroupIntoBatches(fnIsBatchFull: (batch, head, next) => batch.Count >= batchSizeLimit); //Batch full when reaches batchSizeLimit
    }

    /// <summary> Note; <see cref="fnIsBatchFull"/> will ONLY be called when there is at least 1 item in the batch already (i.e. never invoked with an empty batch). </summary>
    public static IEnumerable<IReadOnlyList<T>> GroupIntoBatches<T>(this IEnumerable<T> items, Func<IReadOnlyList<T>, T, T, bool> fnIsBatchFull)
    {
        static ImmutableList<T>.Builder CreateBuilderWithItem(T item) =>
            ImmutableListBuilder.Create(item);

        ImmutableList<T>.Builder? current = null;
        T? batchHead = default;
        foreach (var item in items)
        {
            if (current == null) //Construction deferred until first item hit (could be empty items!)
            {
                current = CreateBuilderWithItem(item); //Each batch initialised with 1 element
            }
            else if (!fnIsBatchFull(current, batchHead!, item)) //batchHead will always be set if current not null
            {
                current.Add(item);
            }
            else
            {
                yield return current.ToImmutable();
                current = CreateBuilderWithItem(item); //Reset and initialise with item
            }
            batchHead = item;
        }
        if (current is { Count: > 0 })
        {
            yield return current.ToImmutable();
        }
    }
}