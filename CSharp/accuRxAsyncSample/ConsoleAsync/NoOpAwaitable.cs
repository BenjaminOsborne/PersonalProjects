using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAsync
{
    public readonly struct NoOpAwaitable
    {
        public NoOpAwaiter GetAwaiter() { return new NoOpAwaiter(); }

        public readonly struct NoOpAwaiter : ICriticalNotifyCompletion
        {
            public bool IsCompleted => false; // yielding is always required for Awaiter, hence false
            
            /// <summary>Ends the await operation.</summary>
            public void GetResult() { } // Nop. It exists purely because the compiler pattern demands it.

            public void OnCompleted(Action continuation) => QueueContinuation(continuation, flowContext: true);

            public void UnsafeOnCompleted(Action continuation) => QueueContinuation(continuation, flowContext: false);

            private static void QueueContinuation(Action continuation, bool flowContext)
            {
                var syncCtx = SynchronizationContext.Current;
                if (syncCtx != null && syncCtx.GetType() != typeof(SynchronizationContext))
                {
                    syncCtx.Post(s_sendOrPostCallbackRunAction, continuation);
                }
                else
                {
                    var scheduler = TaskScheduler.Current;
                    if (scheduler == TaskScheduler.Default)
                    {
                        if (flowContext)
                        {
                            ThreadPool.QueueUserWorkItem(s_waitCallbackRunAction, continuation);
                        }
                        else
                        {
                            ThreadPool.UnsafeQueueUserWorkItem(s_waitCallbackRunAction, continuation);
                        }
                    }
                    else // We're targeting a custom scheduler, so queue a task.
                    {
                        Task.Factory.StartNew(continuation, default, TaskCreationOptions.PreferFairness, scheduler);
                    }
                }
            }

            private static readonly WaitCallback s_waitCallbackRunAction = RunAction;
            private static readonly SendOrPostCallback s_sendOrPostCallbackRunAction = RunAction;

            private static void RunAction(object? state) => ((Action)state!).Invoke();
        }
    }
}
