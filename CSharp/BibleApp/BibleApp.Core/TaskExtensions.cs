using System.Collections.Immutable;

namespace BibleApp.Core;

public static class TaskExtensions
{
    public static async Task<Unit> ToUnitTask(this Task t)
    {
        await t;
        return Unit.Instance;
    }

    public static async Task ToTask(this ValueTask t) => await t;

    public static async ValueTask ToValueTask(this Task t) => await t;

    /// <summary> Enables caller to pipe through fluently </summary>
    public static async Task<T> AsNotNullAsync<T>(this Task<T?> task) =>
        (await task)!;

    public static async Task<IReadOnlyList<T>> AsReadonly<T>(this Task<ImmutableList<T>> task) =>
        await task;

    public static Task<T> UnWrapAsync<T>(this Task<Task<T>> task) =>
        task.Unwrap();

    public static async Task<TResult> UnWrapAsync<T, TResult>(this Task<T> task, Func<T, TResult> fnUnwrap) =>
        fnUnwrap(await task);

    public static async Task<T> TouchAsync<T>(this Task<T> task, Action<T> fnTouch)
    {
        var result = await task;
        fnTouch(result);
        return result;
    }

    /// <remarks> Helpful to have extension on concrete Immutable type as generics on Task of collection is more complex </remarks>
    public static Task<IReadOnlyList<TResult>> UnWrapCollectionAsync<T, TResult>(this Task<ImmutableList<T>> task, Func<T, TResult> fnUnwrap) =>
        task.AsReadonly().UnWrapCollectionAsync(fnUnwrap);

    public static async Task<IReadOnlyList<TResult>> UnWrapCollectionAsync<T, TResult>(this Task<IReadOnlyList<T>> task, Func<T, TResult> fnUnwrap) =>
        (await task)
        .MaterialiseMap(fnUnwrap);

    public static async Task<TResult> UnWrapAsync<T, TResult>(this Task<T> task, Func<T, Task<TResult>> fnUnwrap) =>
        await fnUnwrap(await task);

    public static async Task<IReadOnlyList<T>> UnWrapToReadOnlyListAsync<T>(this Task<IEnumerable<T>> task) =>
        (await task).Materialise();

    public static async Task UnWrapAsync<T>(this Task<T> task, Action<T> fnUnwrap) =>
        fnUnwrap(await task);

    public static async Task UnWrapAsync<T>(this Task<T> task, Func<T, Task> fnUnwrap) =>
        await fnUnwrap(await task);
}