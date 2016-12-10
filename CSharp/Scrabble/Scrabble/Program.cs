using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Scrabble.Data;
using Scrabble.Engine;

namespace Scrabble.Program
{
    class Program
    {
        private Words m_oWords;
        private Board m_oBoard;
        private Optimiser m_oOptimiser;
        private LetterTileRepository m_oTileRepository;

        static void Main(string[] args)
        {
            OutputPrinter.Mode = PrintMode.Console;

            //_RunTest();
            _RunAutoGame();

            Console.ReadLine();
        }

        private static void _RunAutoGame()
        {
            var oRunner = new AutoGameRunner();
            oRunner.RunAutoGame();
        }

        private static void _RunTest()
        {
            var oRunner = new ConsoleTestRunner();
            oRunner.RunTest();
        }
    }

    public class AutoGameRunner
    {
        public void RunAutoGame()
        {
            var m_oSW = new Stopwatch();
            var m_listPlayerNames = new List<string>() { "Ben", "Lizzie" };
            
            var oAutoGame = new AutoGame(m_listPlayerNames);
            m_oSW.Start();
            oAutoGame.PlayFullGame();
            m_oSW.Stop();

            OutputPrinter.WriteLine("Total Game Time (s): " + (double)m_oSW.ElapsedMilliseconds / 1000.0);
        }
    }

    public class ConsoleTestRunner
    {
        private Words m_oWords;
        private Board m_oBoard;
        private Optimiser m_oOptimiser;
        private LetterTileRepository m_oTileRepository;

        public ConsoleTestRunner()
        {
            m_oWords = new Words(new WordList());
            m_oBoard = new Board();
            m_oOptimiser = new Optimiser(m_oWords, m_oBoard);
            m_oTileRepository = new LetterTileRepository();
        }

        public void RunTest()
        {
            _SetBoard(07, ' ', ' ', ' ', 'w', 'a', 'i', 'l', 'e', 'r', ' ', ' ', ' ', ' ', ' ', ' ');//7
            var enTiles = _GetTiles("audien*".ToCharArray());
            _RunOptimisation(enTiles, 5); //2811
        }

        protected void _RunOptimisation(IEnumerable<LetterTile> enTiles, int nMax)
        {
            var ordPlays = m_oOptimiser.GetPossiblePlaysByScore(enTiles).ToList();
            var m_oSW = new Stopwatch();
            m_oSW.Start();
            for (var i = 0; i < nMax; i++)
            {
                ordPlays = m_oOptimiser.GetPossiblePlaysByScore(enTiles).ToList();
                //OutputPrinter.WriteLine(ordPlays[0].Word);
            }
            m_oSW.Stop();
            OutputPrinter.WriteLine("Average time (ms): " + m_oSW.ElapsedMilliseconds / nMax);
        }

        private void _SetBoard(int nRowIndex, params char[] arrLetters)
        {
            var listLetters = arrLetters.ToList();
            for (int nWidth = 0; nWidth < 15; nWidth++)
            {
                var cChar = listLetters[nWidth];
                if (cChar != ' ')
                {
                    m_oBoard[nWidth, nRowIndex].LetterTile = _GetTile(cChar);
                }
            }
        }

        private IEnumerable<LetterTile> _GetTiles(params char[] enLetters)
        {
            return enLetters.Select(_GetTile);
        }

        private LetterTile _GetTile(char cChar)
        {
            var oTile = m_oTileRepository.GetLetterTile(cChar, false);
            return oTile;
        }

        private void _PrintPossiblePlays(IEnumerable<LetterTile> enTiles, int nMax = 100000)
        {
            var ordPlays = m_oOptimiser.GetPossiblePlaysByScore(enTiles);
            var nCount = 0;
            foreach (var oPossiblePlay in ordPlays)
            {
                OutputPrinter.WriteLine(oPossiblePlay.Score + "pts - " +
                                oPossiblePlay.Direction + " - " +
                                oPossiblePlay.StartPosition.StartWidthPosition + ", " +
                                oPossiblePlay.StartPosition.StartHeightPosition + " - " +
                                oPossiblePlay.Word);
                nCount++;
                if (nCount > nMax)
                {
                    break;
                }
            }
        }
    }
}
