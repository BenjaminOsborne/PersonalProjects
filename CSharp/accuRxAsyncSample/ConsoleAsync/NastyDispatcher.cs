using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleAsync
{
    public sealed class NastyDispatcher : SynchronizationContext, IDisposable
    {
        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> _queue = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        public NastyDispatcher()
        {
            var thread = new Thread(ThreadWorkerDelegate);
            thread.Start(this);
            Console.WriteLine($"Creating thread with Id: {thread.ManagedThreadId}");
        }

        public void Dispose() => _queue.CompleteAdding();

        public override void Post(SendOrPostCallback d, object state) =>
            _queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));

        public override void Send(SendOrPostCallback d, object state)
        {
            using var handledEvent = new ManualResetEvent(false);
            var valueTuple = (d, state, handledEvent);
            Post(SendOrPostCallback_BlockingWrapper, valueTuple);
            handledEvent.WaitOne();
        }

        private static void SendOrPostCallback_BlockingWrapper(object state)
        {
            var innerCallback = (((SendOrPostCallback cb, object obj, ManualResetEvent mre)?) state).Value;
            try
            {
                innerCallback.cb(innerCallback.obj);
            }
            finally
            {
                innerCallback.mre.Set();
            }
        }

        private void ThreadWorkerDelegate(object obj)
        {
            SetSynchronizationContext((NastyDispatcher)obj);
            try
            {
                foreach (var workItem in _queue.GetConsumingEnumerable())
                {
                    workItem.Key(workItem.Value);
                }
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
