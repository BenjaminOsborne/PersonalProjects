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
    public class OptimiserTest : OptimisationTestBase
    {
        [SetUp]
        public void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void With_no_possible_play()
        {
            m_oBoard[7, 7].LetterTile = new LetterTile('e', 1);

            var listLetters = new List<LetterTile>();
            listLetters.Add(new LetterTile('z', 10));

            var listPossiblePlays = m_oOptimiser.GetPossiblePlaysByScore(listLetters).ToList();
            Assert.That(listPossiblePlays.Count, Is.EqualTo(0));
        }

        [Test]
        public void With_single_tile_and_two_possible()
        {
            m_oBoard[7,7].LetterTile = new LetterTile('e', 1);

            var listLetters = new List<LetterTile>();
            listLetters.Add(new LetterTile('b', 3));
            
            var listPossiblePlays = m_oOptimiser.GetPossiblePlaysByScore(listLetters).ToList();
            Assert.That(listPossiblePlays.Count, Is.EqualTo(2));
        }

        [Test]
        public void With_two_tiles_and_1_possible()
        {
            m_oBoard[7, 7].LetterTile = new LetterTile('b', 1);
            m_oBoard[7, 9].LetterTile = new LetterTile('t', 1);

            var listLetters = new List<LetterTile>();
            listLetters.Add(new LetterTile('u', 1));

            var listPossiblePlays = m_oOptimiser.GetPossiblePlaysByScore(listLetters).ToList();
            Assert.That(listPossiblePlays.Count, Is.EqualTo(1));
        }

        [Test]
        public void With_many_tiles_and_1_possible()
        {
            m_oBoard[3, 0].LetterTile = new LetterTile('l', 1);
            m_oBoard[5, 0].LetterTile = new LetterTile('t', 1);
            m_oBoard[6, 0].LetterTile = new LetterTile('t', 1);
            m_oBoard[7, 0].LetterTile = new LetterTile('e', 1);
            m_oBoard[8, 0].LetterTile = new LetterTile('r', 1);

            var listLetters = new List<LetterTile>();
            listLetters.Add(new LetterTile('i', 1));
            listLetters.Add(new LetterTile('z', 1));

            var listPossiblePlays = m_oOptimiser.GetPossiblePlaysByScore(listLetters).ToList();
            Assert.That(listPossiblePlays.Count, Is.EqualTo(1));
            Assert.That(listPossiblePlays[0].AllLetterTiles.GetWord(), Is.EqualTo("litter"));
            Assert.That(listPossiblePlays[0].StartPosition.StartHeightPosition, Is.EqualTo(0));
            Assert.That(listPossiblePlays[0].StartPosition.StartWidthPosition, Is.EqualTo(3));
        }

        [Test]
        public void Single_word_with_blank_tile_test()
        {
            var oSW = new Stopwatch();

            _SetBoard(07, ' ', ' ', ' ', 'w', 'a', 'i', 'l', 'e', 'r', ' ', ' ', ' ', ' ', ' ', ' ');
            var enTiles = _GetTiles('a', 'u', 'd', 'i', 'e', 'n', 'c');
            
            oSW.Start();
            var ordPlays = m_oOptimiser.GetPossiblePlaysByScore(enTiles).ToList();
            oSW.Stop();
            OutputPrinter.WriteLine("Time (ms): " + oSW.ElapsedMilliseconds);
            oSW.Reset();
            
            //Assert.That(ordPlays.Count, Is.EqualTo(324));
            Assert.That(ordPlays[0].Word, Is.EqualTo("audience")); //880ms
            Assert.That(ordPlays[0].Score, Is.EqualTo(86));

            enTiles = _GetTiles('a', 'u', 'd', 'i', 'e', 'n', '*');

            oSW.Start();
            ordPlays = m_oOptimiser.GetPossiblePlaysByScore(enTiles).ToList();
            oSW.Stop();
            OutputPrinter.WriteLine("Time (ms): " + oSW.ElapsedMilliseconds);
            oSW.Reset();

            Assert.That(ordPlays[0].Word, Is.EqualTo("unpaired")); //2706
            Assert.That(ordPlays[0].Score, Is.EqualTo(86));
        }

        [Test]
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
            _SetBoard(07, ' ', ' ', ' ', 'w', 'a', 'i', 'l', 'e', 'r', ' ', ' ', ' ', ' ', ' ', ' ');//7
            _SetBoard(08, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//8
            _SetBoard(09, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//9
            _SetBoard(10, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//10
            _SetBoard(11, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//11
            _SetBoard(12, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//12
            _SetBoard(13, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//13
            _SetBoard(14, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//14
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            //                                                              '1', '1', '1', '1', '1'
            _PrintPossiblePlays(_GetTiles('a', 'u', 'd', 'i', 'e', 'n', '*')); ////c = * Audience //wailer in the middle only
        }

        [Test]
        public void PerformanceTestNoBlanks()
        {
            _SetBoard(07, ' ', ' ', ' ', 'w', 'a', 'i', 'l', 'e', 'r', ' ', ' ', ' ', ' ', ' ', ' ');//7
            var enTiles = _GetTiles("audienc".ToCharArray());
            _RunOptimisation(enTiles, 5); //190 (post dic. change)
        }

        [Test]
        public void PerformanceTestWith1Blank()
        {
            _SetBoard(07, ' ', ' ', ' ', 'w', 'a', 'i', 'l', 'e', 'r', ' ', ' ', ' ', ' ', ' ', ' ');//7
            var enTiles = _GetTiles("audien*".ToCharArray());
            _RunOptimisation(enTiles, 3); //542
        }

        [Test]
        public void PerformanceTestWith2Blanks()
        {
            _SetBoard(07, ' ', ' ', ' ', 'w', 'a', 'i', 'l', 'e', 'r', ' ', ' ', ' ', ' ', ' ', ' ');//7
            var enTiles = _GetTiles("audie**".ToCharArray());
            _RunOptimisation(enTiles, 3); //3600
        }

        [Test]
        public void When_1_blank_tile_in_hand()
        {
            _SetBoard(07, ' ', ' ', ' ', 'w', 'a', 'i', 'l', 'e', 'r', ' ', ' ', ' ', ' ', ' ', ' ');//7
            var enTiles = _GetTiles("audien*".ToCharArray());
            var ordPlays = m_oOptimiser.GetPossiblePlaysByScore(enTiles).ToList();
            Assert.That(ordPlays[0].Word, Is.EqualTo("unpaired"));
        }

        [Test]
        public void Full_completed_board_test()
        {
            //                                                              '1', '1', '1', '1', '1'
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            _SetBoard(00, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'f', 'o', 'c', 'i');//0
            _SetBoard(01, ' ', ' ', ' ', ' ', ' ', 'v', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'u', ' ');//1
            _SetBoard(02, ' ', ' ', 't', 'r', 'i', 'o', ' ', ' ', ' ', ' ', ' ', 'b', 'o', 'p', ' ');//2
            _SetBoard(03, ' ', ' ', ' ', ' ', ' ', 'm', ' ', ' ', 'q', 'u', 'e', 'e', 'n', 's', ' ');//3
            _SetBoard(04, ' ', ' ', ' ', ' ', 'b', 'i', 'j', 'o', 'u', ' ', ' ', ' ', ' ', ' ', ' ');//4
            _SetBoard(05, ' ', ' ', ' ', 'f', 'e', 't', 'a', ' ', 'a', 'm', 'p', 'l', 'y', ' ', ' ');//5
            _SetBoard(06, ' ', ' ', 'a', ' ', ' ', ' ', 'n', ' ', 'y', 'e', ' ', ' ', ' ', ' ', 'r');//6
            _SetBoard(07, ' ', ' ', 'e', 'x', 'i', 'l', 'i', 'c', ' ', 't', 'o', 'd', 'd', 'l', 'e');//7
            _SetBoard(08, 's', ' ', 'o', ' ', ' ', ' ', 't', ' ', ' ', 'r', ' ', ' ', 'u', ' ', 'v');//8
            _SetBoard(09, 'e', ' ', 'n', ' ', ' ', 'h', 'o', 'd', ' ', 'o', 'z', ' ', 'e', ' ', ' ');//9
            _SetBoard(10, 'a', ' ', 'i', ' ', 'w', 'a', 'r', 'e', ' ', ' ', 'e', ' ', 'n', ' ', ' ');//10
            _SetBoard(11, 'w', 'h', 'a', 'r', 'e', ' ', 's', ' ', ' ', ' ', 't', 'o', 'n', 'g', ' ');//11
            _SetBoard(12, 'a', 'i', 'n', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'a', ' ', 'a', ' ', ' ');//12
            _SetBoard(13, 'r', ' ', ' ', ' ', ' ', ' ', ' ', 'i', 'n', 'k', 's', ' ', ' ', ' ', ' ');//13
            _SetBoard(14, 'd', ' ', ' ', 'l', 'e', 'g', 'i', 't', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//14
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            //                                                              '1', '1', '1', '1', '1'

            _PrintPossiblePlays(_GetTiles("i".ToCharArray())); //double a in row 5 should be filtered
        }

        [Test]
        public void Distinct_letter_aorta_test()
        {
            //                                                              '1', '1', '1', '1', '1'
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            _SetBoard(00, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//0
            _SetBoard(01, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//1
            _SetBoard(02, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//2
            _SetBoard(03, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'q', ' ', ' ', ' ', ' ', ' ', ' ');//3
            _SetBoard(04, ' ', ' ', ' ', ' ', 'b', 'i', 'j', 'o', 'u', ' ', ' ', ' ', ' ', ' ', ' ');//4
            _SetBoard(05, ' ', ' ', ' ', ' ', ' ', ' ', 'a', ' ', 'a', ' ', ' ', ' ', ' ', ' ', ' ');//5
            _SetBoard(06, ' ', ' ', 'a', ' ', ' ', ' ', 'n', ' ', 'y', ' ', ' ', ' ', ' ', ' ', ' ');//6
            _SetBoard(07, ' ', ' ', 'e', 'x', 'i', 'l', 'i', 'c', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//7
            _SetBoard(08, 's', ' ', 'o', ' ', ' ', ' ', 't', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//8
            _SetBoard(09, 'e', ' ', 'n', ' ', ' ', ' ', 'o', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//9
            _SetBoard(10, 'a', ' ', 'i', ' ', 'w', 'a', 'r', 'e', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//10
            _SetBoard(11, 'w', 'h', 'a', 'r', 'e', ' ', 's', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//11
            _SetBoard(12, 'a', ' ', 'n', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//12
            _SetBoard(13, 'r', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//13
            _SetBoard(14, 'd', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//14
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            //                                                              '1', '1', '1', '1', '1'

            _PrintPossiblePlays(_GetTiles("mesortp".ToCharArray())); //double a in row 5 should be filtered
        }

        [Test]
        public void Empty_board_test()
        {
            _PrintPossiblePlays(_GetTiles('o', 'z', 'a', 'b', 'l', 'p', 'm'));
        }

        [Test]
        public void StMartin()
        {
            //                                                              '1', '1', '1', '1', '1'
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            _SetBoard(00, ' ', ' ', ' ', ' ', ' ', 'i', 'r', 'e', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//0
            _SetBoard(01, ' ', ' ', 'w', 'a', 'r', 'n', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//1
            _SetBoard(02, ' ', ' ', 'i', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//2
            _SetBoard(03, ' ', ' ', 'p', 'i', 'k', 'e', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//3
            _SetBoard(04, ' ', ' ', 'e', ' ', ' ', 'g', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//4
            _SetBoard(05, ' ', ' ', 'd', ' ', ' ', 'o', 'f', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//5
            _SetBoard(06, ' ', ' ', ' ', ' ', ' ', ' ', 'a', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//6
            _SetBoard(07, ' ', ' ', ' ', ' ', 'b', 'a', 'r', 'n', 's', ' ', ' ', ' ', ' ', ' ', ' ');//7
            _SetBoard(08, ' ', ' ', ' ', ' ', 'o', ' ', 'm', ' ', 'h', ' ', ' ', ' ', ' ', ' ', ' ');//8
            _SetBoard(09, ' ', ' ', ' ', ' ', 'o', ' ', ' ', 't', 'a', 'b', 'l', 'e', 't', ' ', ' ');//9
            _SetBoard(10, ' ', ' ', ' ', ' ', 'm', ' ', ' ', ' ', 'r', ' ', ' ', ' ', 'e', ' ', ' ');//10
            _SetBoard(11, ' ', ' ', ' ', 'f', ' ', ' ', ' ', ' ', 'e', ' ', ' ', ' ', 'a', ' ', ' ');//11
            _SetBoard(12, ' ', ' ', 'p', 'a', 'i', 'n', 't', 'e', 'd', ' ', ' ', ' ', 'r', ' ', ' ');//12
            _SetBoard(13, ' ', ' ', ' ', 'x', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//13
            _SetBoard(14, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//14
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            //                                                              '1', '1', '1', '1', '1'
            _PrintPossiblePlays(_GetTiles('g','a','i','w','y','u','r'));
        }
    }
}
