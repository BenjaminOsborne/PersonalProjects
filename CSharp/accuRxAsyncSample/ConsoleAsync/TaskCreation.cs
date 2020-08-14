using System;
using System.Threading;
using System.Threading.Tasks;
using Zoo;

namespace ConsoleAsync
{
    public static class TaskCreation
    {
        public static void Start()
        {
            var a = Task.CompletedTask; //Completed
            var b = Task.FromResult(new { prop = 5 }); //With result
            var ex = Task.FromException(new Exception("Not again Brian...")); //With exception

            var c = Task.Run(() => "Hello"); //With result on ThreadPool
            var d = Task.Factory.StartNew(() => 7, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default); //As above!

            var e = new Task(() => Console.WriteLine("Callback")); //Manually bound and started
            e.Start(TaskScheduler.Default);
        }

        public static Task Yield()
        {
            Task yld = _GetNoOp();
            return yld;
        }

        private static async Task _GetYield() => await Task.Yield();
        private static async Task _GetNoOp() => await new NoOpAwaitable();

        public static async Task<Animal> GetAnimal() => await _GetCatAsync();

        private static async Task<Cat> _GetCatAsync()
        {
            await Task.Yield();
            return new Cat();
        }
    }
}

namespace Zoo
{
    public class Animal { }
    public class Cat : Animal { }
}
