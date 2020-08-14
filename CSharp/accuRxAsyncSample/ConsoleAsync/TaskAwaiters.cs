using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ConsoleAsync
{
    public static class TaskAwaitersTime
    {
        public static async Task Time()
        {
            await 500;
        }

        public static TaskAwaiter GetAwaiter(this int timeSpan) =>
            Task.Delay(timeSpan).GetAwaiter();
    }

    public static class TaskAwaitersProcess
    {
        public static async Task WaitForProcessStart()
        {
            await Process.Start("Foo.exe");
        }

        public static TaskAwaiter<int> GetAwaiter(this Process process)
        {
            var tcs = new TaskCompletionSource<int>();

            process.EnableRaisingEvents = true;
            process.Exited += (s, e) => tcs.TrySetResult(process.ExitCode);

            if (process.HasExited)
            {
                tcs.TrySetResult(process.ExitCode);
            }
            return tcs.Task.GetAwaiter();
        }
    }
}
