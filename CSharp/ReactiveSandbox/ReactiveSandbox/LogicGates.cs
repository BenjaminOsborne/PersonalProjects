using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace ReactiveSandbox
{
    public static class LogicGates
    {
        public static void Invoke()
        {
            Console.WriteLine("Single...");
            InvokeSingle();

            Console.WriteLine("\nPause...");
            Console.ReadLine();

            Console.WriteLine("\nStreams..");
            InvokeStreams();
        }

        public static void InvokeSingle()
        {
            var board = new LogicBoard();

            board.LeftStream.OnNext(Bit.High);
            board.RightStream.OnNext(Bit.High);

            board.LeftStream.OnNext(Bit.Low);
            board.RightStream.OnNext(Bit.Low);

            board.LeftStream.OnNext(Bit.High);
            board.LeftStream.OnNext(Bit.High);
            board.RightStream.OnNext(Bit.High);
        }

        public static void InvokeStreams()
        {
            var board = new LogicBoard();

            _RandomStream().ToObservable(NewThreadScheduler.Default).Sample(TimeSpan.FromSeconds(1)).Subscribe(board.RightStream);
            _RandomStream().ToObservable(NewThreadScheduler.Default).Sample(TimeSpan.FromSeconds(1)).Subscribe(board.LeftStream);
        }

        public static IEnumerable<Bit> _RandomStream()
        {
            var random = new Random();
            while (true)
            {
                var sleep = random.Next(100, 2000);
                //Thread.Sleep(sleep);
                yield return sleep % 2 == 0 ? Bit.Low : Bit.High;
            }
        } 

        public class LogicBoard
        {
            public LogicBoard()
            {
                LeftStream = new Subject<Bit>();
                RightStream = new Subject<Bit>();
                AndGate = new AndGate(LeftStream, RightStream);
                OrGate = new OrGate(LeftStream, RightStream);

                LeftStream.Subscribe(b => Console.WriteLine("Left Stream: " + b));
                RightStream.Subscribe(b => Console.WriteLine("Right Stream: " + b));
                AndGate.OutputStream.Subscribe(b => Console.WriteLine("AND Stream: " + b));
                OrGate.OutputStream.Subscribe(b => Console.WriteLine("OR Stream: " + b));
            }

            public Subject<Bit> LeftStream { get; }
            public Subject<Bit> RightStream { get; }

            public AndGate AndGate { get; }
            public OrGate OrGate { get; }
        }

        /// <summary>
        /// On request of Dr Ben.
        /// </summary>
        public enum Bit
        {
            Low = 0,
            High
        }

        public interface ILogicElement
        {
            Bit Evaluate(Bit leftIn, Bit rightIn);
        }

        public abstract class DualInputGate : ILogicElement
        {
            protected DualInputGate(IObservable<Bit> leftIn, IObservable<Bit> rightIn)
            {
                OutputStream = leftIn.CombineLatest(rightIn, Evaluate);
            }

            public IObservable<Bit> OutputStream { get; }

            public abstract Bit Evaluate(Bit leftIn, Bit rightIn);
        }

        public class AndGate : DualInputGate
        {
            public AndGate(IObservable<Bit> leftIn, IObservable<Bit> rightIn) : base(leftIn, rightIn)
            {
            }

            public override Bit Evaluate(Bit leftIn, Bit rightIn)
            {
                return leftIn == Bit.High && rightIn == Bit.High ? Bit.High : Bit.Low;
            }
        }

        public class OrGate : DualInputGate
        {
            public OrGate(IObservable<Bit> leftIn, IObservable<Bit> rightIn) : base(leftIn, rightIn)
            {
            }

            public override Bit Evaluate(Bit leftIn, Bit rightIn)
            {
                return leftIn == Bit.High || rightIn == Bit.High ? Bit.High : Bit.Low;
            }
        }
    }
}
