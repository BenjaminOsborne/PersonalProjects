using System.Collections.Concurrent;
using AccountProcessor.Core;

namespace AccountProcessor.Client.ClientServices;

public class TaskTracker : IAsyncDisposable
{
    /// <remarks> Value irrelevant as just using this as a concurrent set </remarks>
    private readonly ConcurrentDictionary<Task, Unit> _tasks = new();

    public void TrackTask(Task task)
    {
        _tasks[task] = Unit.Instance;
        task.ContinueWith((t, o) =>
            _tasks.TryRemove(t, out var found), null); //Task reference in the continuation is identical
    }

    public async ValueTask DisposeAsync()
    {
        if (_tasks.Any())
        {
            await Task.WhenAll(_tasks.Keys);
        }
    }
}