using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Scrabble.Data;
using Scrabble.Engine;

namespace Scabble.Engine.Tests
{
    public class OptimisationTestBase
    {
        protected Words m_oWords;
        protected Board m_oBoard;
        protected Optimiser m_oOptimiser;
        protected LetterTileRepository m_oTileRepository;
        protected Stopwatch m_oSW;

        [SetUp]
        public void SetUp()
        {
            m_oWords = new Words(new WordList());
            m_oBoard = new Board();
            m_oOptimiser = new Optimiser(m_oWords, m_oBoard);
            m_oTileRepository = new LetterTileRepository();
            m_oSW = new Stopwatch();
        }

        protected void _RunOptimisation(IEnumerable<LetterTile> enTiles, int nMax)
        {
            var ordPlays = m_oOptimiser.GetPossiblePlaysByScore(enTiles).ToList();

            m_oSW.Start();
            for (var i = 0; i < nMax; i++)
            {
                ordPlays = m_oOptimiser.GetPossiblePlaysByScore(enTiles).ToList();
                //Debug.WriteLine(ordPlays[0].Word);
            }
            m_oSW.Stop();
            Debug.WriteLine("Average time (ms): " + m_oSW.ElapsedMilliseconds / nMax); //177
        }

        protected IEnumerable<PossiblePlay> _PrintPossiblePlays(IEnumerable<LetterTile> enTiles)
        {
            var ordPlays = m_oOptimiser.GetPossiblePlaysByScore(enTiles);
            
            foreach (var oPossiblePlay in ordPlays)
            {
                Debug.WriteLine(oPossiblePlay.Score + "pts - " +
                                oPossiblePlay.Direction + " - " +
                                oPossiblePlay.StartPosition.StartWidthPosition + ", " +
                                oPossiblePlay.StartPosition.StartHeightPosition + " - " +
                                oPossiblePlay.Word);
            }

            return ordPlays;
        }

        protected void _SetBoard(int nRowIndex, params char[] arrLetters)
        {
            var listLetters = arrLetters.ToList();
            Assert.That(listLetters.Count, Is.EqualTo(15));
            Assert.That(nRowIndex >= 0 && nRowIndex < 15, Is.True);

            for (int nWidth = 0; nWidth < 15; nWidth++)
            {
                var cChar = listLetters[nWidth];
                if (cChar != ' ')
                {
                    m_oBoard[nWidth, nRowIndex].LetterTile = _GetTile(cChar);
                }
            }
        }

        protected IEnumerable<LetterTile> _GetTiles(params char[] enLetters)
        {
            return enLetters.Select(_GetTile);
        }

        protected LetterTile _GetTile(char cChar)
        {
            var oTile = m_oTileRepository.GetLetterTile(cChar, false);
            Assert.That(oTile != null, Is.True);
            return oTile;
        }

        public void TemplateBoard()
        {
            //                                                              '1', '1', '1', '1', '1'
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            _SetBoard(00, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//0
            _SetBoard(01, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//1
            _SetBoard(02, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//2
            _SetBoard(03, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//3
            _SetBoard(04, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//4
            _SetBoard(05, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//5
            _SetBoard(06, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//6
            _SetBoard(07, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//7
            _SetBoard(08, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//8
            _SetBoard(09, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//9
            _SetBoard(10, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//10
            _SetBoard(11, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//11
            _SetBoard(12, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//12
            _SetBoard(13, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//13
            _SetBoard(14, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//14
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            //                                                              '1', '1', '1', '1', '1'
        }
    }
}
