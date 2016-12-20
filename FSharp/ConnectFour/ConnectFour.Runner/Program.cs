using System;
using System.Collections.Generic;
using System.Linq;
using ConnectFour.Engine;

namespace ConnectFour.Runner
{
    class Program
    {
        private static int m_nComputerAnalysisDepth;

        static void Main(string[] args)
        {
            var lastPlayed = CounterColour.Red;
            m_nComputerAnalysisDepth = 3;
            var firstPlay = NextPlayType.Manual;

            var board = new BoardState(RunnerHelpers.InitialiseCellState(), lastPlayed);

            _PlayMove(new Stack<BoardState>(new [] { board }), new Calculator(), firstPlay);

            Console.ReadLine();
        }

        public enum NextPlayType
        {
            Manual,
            Computer
        }

        private static void _PlayMove(Stack<BoardState> boardStack, Calculator calculator, NextPlayType playType)
        {
            if (boardStack.Count == 0)
            {
                return;
            }

            var board = boardStack.Peek();
            if (board == null)
            {
                Console.WriteLine("Cannot play on board");
                return;
            }

            _PrintBoard(board);
            
            if (board.AvailableSlots.Any() == false)
            {
                Console.WriteLine("No more plays");
                return;
            }

            if (_NotifyIfPlayerHasWon(board))
            {
                return;
            }

            var nextBoard = playType == NextPlayType.Manual ? _GetManualMove(boardStack)
                                                            : _GetComputerBoard(calculator, board);
            boardStack.Push(nextBoard);
            _PlayMove(boardStack, calculator, _GetNext(playType));
        }

        private static void _PrintBoard(BoardState board)
        {
            foreach (var letter in board.Print().ToCharArray())
            {
                Console.ForegroundColor = _GetConsoleColour(letter);
                Console.Write(letter);
            }
        }

        private static ConsoleColor _GetConsoleColour(char cChar)
        {
            if (cChar == 'R')
            {
                return ConsoleColor.Red;
            }
            if (cChar == 'Y')
            {
                return ConsoleColor.Yellow;
            }
            return ConsoleColor.White;
        }

        private static NextPlayType _GetNext(NextPlayType playType)
        {
            return playType == NextPlayType.Manual ? NextPlayType.Computer : NextPlayType.Manual;
        }
        
        private static BoardState _GetComputerBoard(Calculator calculator, BoardState nextBoard)
        {
            Console.WriteLine("Waiting for next move...\n");

            var bestPlay = calculator.GetNextBestPlay(nextBoard, m_nComputerAnalysisDepth);
            return bestPlay != null ? bestPlay.Value.BestPlay.NextPlay.Board : null;
        }

        private static BoardState _GetManualMove(Stack<BoardState> boardStack)
        {
            BoardState nextBoard = null;
            while (nextBoard == null)
            {
                Console.WriteLine("\nEnter Slot Index (0-6) -OR- 'U' to Undo last move");
                var sRead = Console.ReadLine();

                if (sRead == null)
                {
                    continue;
                }

                if (sRead.ToUpper() == "U")
                {
                    if (boardStack.Count < 3)
                    {
                        Console.WriteLine("Cannot undo - no moves");
                        continue;
                    }
                    var thisBoard = boardStack.Pop();
                    var previousBoard = boardStack.Pop();
                    _PrintBoard(boardStack.Peek());
                    continue;
                }

                int slotParse;
                var bSucess = int.TryParse(sRead, out slotParse);
                if (bSucess && boardStack.Peek().AvailableSlots.Contains(slotParse))
                {
                    nextBoard = boardStack.Peek().PlayAtSlot(slotParse);
                }
            }

            return nextBoard;
        }

        private static bool _NotifyIfPlayerHasWon(BoardState board)
        {
            if (board == null || board.GetFourInARow(board.LastPlayed) == null)
            {
                return false;
            }
            var sColour = board.LastPlayed.IsRed ? "Red" : "Yellow";
            Console.WriteLine(sColour + " is Winner!");
            return true;
        }
    }

    public static class RunnerHelpers
    {
        public static CellState[,] InitialiseCellState()
        {
            var arrState = new CellState[7, 6];
            0.To(6).ForEach(x => 0.To(5).ForEach(y => arrState[x, y] = CellStateHelper.EmptyCellState()));
            return arrState;
        }
    }
}
