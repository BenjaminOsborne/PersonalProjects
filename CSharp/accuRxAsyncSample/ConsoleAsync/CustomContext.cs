using System;
using System.Threading;

namespace ConsoleAsync
{
    public class CustomContext : SynchronizationContext
    {
        public override void Send(SendOrPostCallback d, object? state)
        {
            _Intercept();

            base.Send(d, state);
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _Intercept();

            base.Post(d, state);
        }

        public override void OperationStarted()
        {
            _Intercept();

            base.OperationStarted();
        }

        public override void OperationCompleted()
        {
            _Intercept();

            base.OperationCompleted();
        }

        public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            _Intercept();

            return base.Wait(waitHandles, waitAll, millisecondsTimeout);
        }

        public override SynchronizationContext CreateCopy()
        {
            return new CustomContext();
        }

        private static void _Intercept()
        {
        }
    }
}
