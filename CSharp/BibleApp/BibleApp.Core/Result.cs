namespace BibleApp.Core;

public static class Result
{
    public static IResultDetail Success { get; } = SuccessResult.Create(Unit.Instance);

    public static Result<T> CreateSuccess<T>(T value) => SuccessResult.Create(value);

    public static FailedResult Fail(string error) => new(error);

    public static Result<T> Fail<T>(string error) => new FailedResult(error);
}

/// <remarks> Both "IResult" and "IActionResult" already taken by .NET library types - so named with "Detail" suffix to avoid type clash! </remarks>
public interface IResultDetail
{
    bool IsSuccess { get; }
    string? Error { get; }
    bool IsFail => !IsSuccess;
}

public interface IResultDetail<out T> : IResultDetail
{
    T? Value { get; }
}

public static class ResultDetailExtensions
{
    /// <summary> <see cref="result"/> MUST be a failed result - otherwise will throw. </summary>
    public static FailedResult AsFailure(this IResultDetail result) =>
        result.IsFail
            ? new FailedResult(result.Error!)
            : throw new InvalidOperationException($"Cannot map Success result to fail. Type: {result.GetType().FullName}");

    /// <summary> <see cref="result"/> MUST be a failed result - otherwise will throw. </summary>
    public static FailedResult AsFailure<T>(this IResultDetail<T> result) =>
        result.IsFail
            ? new FailedResult(result.Error!)
            : throw new InvalidOperationException($"Cannot map Success result to fail. Type: {typeof(T).FullName}");

    public static Result<T> OnSuccess<T>(this Result<T> result, Action action)
    {
        if (result.IsSuccess)
        {
            action.Invoke();
        }
        return result;
    }

    /// <summary> De-nests a Result{Result{T}} to Result{T} </summary>
    /// <returns></returns>
    public static Result<T> UnWrap<T>(this Result<Result<T>> result) =>
        result.IsSuccess
            ? result.Value ?? Result.Fail("Inner Result null")
            : result.AsFailure();

    /// <summary> De-nests a Result{IResultDetail} to IResultDetail </summary>
    public static IResultDetail UnWrap(this Result<IResultDetail> result) =>
        result.IsSuccess
            ? result.Value ?? Result.Fail("Inner Result null")
            : result.AsFailure();

    /// <summary> De-nests Result and confirms all inner results are Success to return Success </summary>
    public static IResultDetail UnWrap(this Result<IReadOnlyList<IResultDetail>> result) =>
        result.IsSuccess
            ? result.Value?.Combine() ?? Result.Fail("Inner Result null")
            : result.AsFailure();

    public static Result<TOut> MapOnSuccess<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> fnMapOnSuccess) =>
        result.IsSuccess
            ? Result.CreateSuccess(fnMapOnSuccess(result.Value!))
            : result.AsFailure();

    public static async Task<Result<TOut>> MapOnSuccessAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> fnMapOnSuccessAsync) =>
        result.IsSuccess
            ? Result.CreateSuccess(await fnMapOnSuccessAsync(result.Value!))
            : result.AsFailure();

    public static Result<TOut> MapOnAllSuccess<TIn, TOut>(this IReadOnlyList<Result<TIn>> results, Func<IReadOnlyList<TIn>, TOut> fnMapOnSuccess) =>
        results.FirstOrDefault(x => x.IsFail)?.AsFailure() ?? //Fail if any failed
        Result.CreateSuccess(fnMapOnSuccess(results.MaterialiseMap(x => x.Value!)));

    public static async Task<Result<TOut>> MapOnAllSuccessAsync<TIn, TOut>(this IReadOnlyList<Result<TIn>> results, Func<IReadOnlyList<TIn>, Task<TOut>> fnMapOnSuccessAsync) =>
        results.FirstOrDefault(x => x.IsFail)?.AsFailure() ?? //Fail if any failed
        Result.CreateSuccess(await fnMapOnSuccessAsync(results.MaterialiseMap(x => x.Value!)));

    public static Result<T> MapOnSuccess<T>(this IResultDetail result, Func<T> fnOnSuccess) =>
        result.IsSuccess
            ? Result.CreateSuccess(fnOnSuccess())
            : result.AsFailure();

    public static async Task<Result<T>> MapOnSuccessAsync<T>(this IResultDetail result, Func<Task<T>> fnOnSuccessAsync) =>
        result.IsSuccess
            ? Result.CreateSuccess(await fnOnSuccessAsync())
            : result.AsFailure();

    /// <summary> Success if both succeed, else returns first failure </summary>
    /// <remarks> Special case of params Combine (doesn't require Array alloc) </remarks>
    public static IResultDetail Combine(this IResultDetail first, IResultDetail second) =>
        first.IsFail
            ? first.AsFailure()
            : second.IsFail
                ? second.AsFailure()
                : Result.Success;

    /// <summary> Success if all success, else returns first failure </summary>
    public static IResultDetail Combine(this IResultDetail first, params IResultDetail[] others) =>
        first.Combine(others.AsReadOnlyList()); //pipe to next

    public static IResultDetail Combine(this IResultDetail first, IReadOnlyList<IResultDetail> others) =>
        first.IsFail
            ? first.AsFailure()
            : others.Combine();

    /// <summary> Success if all success, else returns first failure </summary>
    public static IResultDetail Combine(this IEnumerable<IResultDetail> results) =>
        results.FirstOrDefault(x => x.IsFail)
            ?.AsFailure()
        ?? Result.Success; //Return Unit success here, so that runtime type is not the underyling type of either first or others.

    /// <summary> Success with results if all succeeded, else returns first failure </summary>
    public static Result<IReadOnlyList<T>> FlattenResults<T>(this IReadOnlyList<Result<T>> results) =>
        results.FirstOrDefault(x => x.IsFail)
            ?.AsFailure()
        ?? Result.CreateSuccess(results.MaterialiseMap(x => x.Value!));
}

/// <summary> Useful for providing implicit type conversion from failure to <see cref="Result{T}"/> without needing to stamp the type in </summary>
public class FailedResult(string error) : IResultDetail
{
    public bool IsSuccess => false;
    public string? Error { get; } = error;
}

public class Result<T> : IResultDetail<T>
{
    protected Result(bool isSuccess, string? error, T? value)
    {
        IsSuccess = isSuccess;
        Error = error;
        Value = value;
    }

    public bool IsSuccess { get; }
    public bool IsFail => !IsSuccess;

    public string? Error { get; }
    public T? Value { get; }

    public static implicit operator Result<T>(T successItem) =>
        new(isSuccess: true, error: null, value: successItem);

    public static implicit operator Result<T>(FailedResult fail) =>
        new(isSuccess: false, error: fail.Error, value: default);
}

public static class SuccessResult
{
    public static SuccessResult<T> Create<T>(T value) => new(value);
}

public class SuccessResult<T>(T value) : Result<T>(true, null, value)
{
    public static implicit operator SuccessResult<T>(T successItem) => new(successItem);
}

public class Result<T, TFailure> : Result<T>
{
    protected Result(bool isSuccess, string? error, T? value, TFailure? failure)
        : base(isSuccess, error, value)
    {
        Failure = failure;
    }

    public TFailure? Failure { get; }
}