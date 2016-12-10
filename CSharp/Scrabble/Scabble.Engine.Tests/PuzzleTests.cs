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
    public class PuzzleTests : OptimisationTestBase
    {
        [SetUp]
        public void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void PUZ_001_Should_score_38_with_word_pipework()
        {
            //http://www.onwords.info/puz001.pdf

            //                                                              '1', '1', '1', '1', '1'
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            _SetBoard(00, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//0
            _SetBoard(01, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//1
            _SetBoard(02, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//2
            _SetBoard(03, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//3
            _SetBoard(04, ' ', ' ', 'r', 'a', 'd', ' ', ' ', 'e', 'y', 'e', 'd', ' ', ' ', ' ', ' ');//4
            _SetBoard(05, ' ', ' ', ' ', 'd', 'i', 'v', 'a', 'n', ' ', ' ', 'a', ' ', ' ', 'a', ' ');//5
            _SetBoard(06, ' ', ' ', ' ', ' ', ' ', ' ', 'x', ' ', ' ', ' ', 'n', 'e', 'w', 't', ' ');//6
            _SetBoard(07, ' ', ' ', ' ', ' ', ' ', 'p', 'e', 'w', ' ', ' ', ' ', ' ', 'o', ' ', ' ');//7
            _SetBoard(08, ' ', ' ', ' ', ' ', ' ', ' ', 's', 'a', 'f', 'e', ' ', ' ', ' ', ' ', ' ');//8
            _SetBoard(09, ' ', ' ', ' ', ' ', ' ', ' ', ' ', 't', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//9
            _SetBoard(10, ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'c', 'a', 'm', 'e', ' ', ' ', ' ', ' ');//10
            _SetBoard(11, ' ', ' ', ' ', ' ', ' ', ' ', ' ', 'h', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//11
            _SetBoard(12, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//12
            _SetBoard(13, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//13
            _SetBoard(14, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//14
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            //                                                              '1', '1', '1', '1', '1'

            var ordPlays = _PrintPossiblePlays(_GetTiles("rockpil".ToCharArray())); //double a in row 5 should be filtered
            Assert.That(ordPlays.First().Score, Is.EqualTo(38));
            Assert.That(ordPlays.First().Word, Is.EqualTo("pipework"));
        }

        [Test]
        public void PUZ_003_Should_score_65_with_word_workmen()
        {
            //http://www.onwords.info/puz003.pdf //This only guesses the second one!

            //                                                              '1', '1', '1', '1', '1'
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            _SetBoard(00, ' ', ' ', ' ', ' ', ' ', 'c', 'o', 'x', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//0
            _SetBoard(01, ' ', ' ', ' ', ' ', ' ', 'h', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//1
            _SetBoard(02, 'd', 'j', 'i', 'n', 'n', 'i', ' ', ' ', ' ', 'r', ' ', ' ', ' ', ' ', ' ');//2
            _SetBoard(03, ' ', ' ', ' ', ' ', ' ', 'l', ' ', ' ', 'f', 'a', ' ', ' ', ' ', ' ', ' ');//3
            _SetBoard(04, ' ', ' ', ' ', ' ', ' ', 'l', 'o', 'v', 'i', 'n', 'g', ' ', ' ', ' ', ' ');//4
            _SetBoard(05, ' ', 'b', 'e', ' ', ' ', 'i', ' ', ' ', 't', ' ', ' ', ' ', ' ', ' ', ' ');//5
            _SetBoard(06, 'w', 'o', 'r', 'k', ' ', ' ', ' ', ' ', 'l', ' ', ' ', ' ', ' ', ' ', ' ');//6
            _SetBoard(07, ' ', 'o', 'e', ' ', ' ', 'r', 'o', 'p', 'y', ' ', ' ', ' ', ' ', ' ', ' ');//7
            _SetBoard(08, ' ', 'z', ' ', ' ', ' ', ' ', 'u', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//8
            _SetBoard(09, ' ', 'e', 'a', 'r', 'i', 'n', 'g', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//9
            _SetBoard(10, ' ', ' ', ' ', ' ', ' ', ' ', 'h', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//10
            _SetBoard(11, ' ', ' ', ' ', ' ', ' ', ' ', 't', 'e', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//11
            _SetBoard(12, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//12
            _SetBoard(13, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//13
            _SetBoard(14, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//14
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            //                                                              '1', '1', '1', '1', '1'

            var ordPlays = _PrintPossiblePlays(_GetTiles("enflomb".ToCharArray())); //double a in row 5 should be filtered
            Assert.That(ordPlays.First().Score, Is.EqualTo(65));
            Assert.That(ordPlays.First().Word, Is.EqualTo("workmen"));
            Assert.That(ordPlays.ElementAt(1).Score, Is.EqualTo(43));
            Assert.That(ordPlays.ElementAt(1).Word, Is.EqualTo("menfolk"));
        }

        [Test]
        public void PUZ_010_Should_score_27_with_word_questioned()
        {
            //http://www.onwords.info/puz010.pdf

            //                                                              '1', '1', '1', '1', '1'
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            _SetBoard(00, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//0
            _SetBoard(01, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//1
            _SetBoard(02, ' ', ' ', ' ', ' ', ' ', ' ', 'd', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//2
            _SetBoard(03, ' ', ' ', ' ', ' ', ' ', 'q', 'u', 'i', 't', ' ', ' ', ' ', ' ', ' ', ' ');//3
            _SetBoard(04, ' ', ' ', ' ', ' ', 'l', 'u', 'g', ' ', 'r', ' ', ' ', ' ', ' ', ' ', ' ');//4
            _SetBoard(05, ' ', ' ', ' ', ' ', ' ', 'e', ' ', ' ', 'i', ' ', ' ', ' ', ' ', ' ', ' ');//5
            _SetBoard(06, ' ', ' ', ' ', ' ', 'a', 's', 'k', ' ', 'c', 'o', 'y', ' ', ' ', ' ', ' ');//6
            _SetBoard(07, ' ', ' ', ' ', ' ', ' ', 't', 'i', 'm', 'e', 'd', ' ', ' ', ' ', ' ', ' ');//7
            _SetBoard(08, ' ', ' ', ' ', ' ', ' ', ' ', 'f', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//8
            _SetBoard(09, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//9
            _SetBoard(10, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//10
            _SetBoard(11, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//11
            _SetBoard(12, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//12
            _SetBoard(13, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//13
            _SetBoard(14, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//14
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            //                                                              '1', '1', '1', '1', '1'

            var ordPlays = _PrintPossiblePlays(_GetTiles("odepain".ToCharArray())); //double a in row 5 should be filtered
            Assert.That(ordPlays.First().Score, Is.EqualTo(27));
            Assert.That(ordPlays.First().Word, Is.EqualTo("questioned"));
        }

        [Test]
        public void PUZ_030_Should_score_34_with_word_livestock()
        {
            //http://www.onwords.info/puz030.pdf //test only gets second one...

            //                                                              '1', '1', '1', '1', '1'
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            _SetBoard(00, ' ', ' ', ' ', 'l', 'i', 'o', 'n', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//0
            _SetBoard(01, ' ', ' ', ' ', ' ', ' ', 'v', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//1
            _SetBoard(02, ' ', ' ', ' ', ' ', ' ', 'e', ' ', ' ', 'p', ' ', ' ', ' ', ' ', ' ', ' ');//2
            _SetBoard(03, ' ', ' ', ' ', ' ', ' ', 'r', ' ', ' ', 'a', 'v', 'e', 's', ' ', ' ', ' ');//3
            _SetBoard(04, ' ', ' ', ' ', ' ', ' ', 'a', ' ', ' ', ' ', 'e', ' ', ' ', ' ', ' ', ' ');//4
            _SetBoard(05, ' ', ' ', ' ', ' ', ' ', 'c', ' ', 'w', 'i', 's', 'e', ' ', ' ', ' ', ' ');//5
            _SetBoard(06, ' ', ' ', 'l', 'o', 'f', 't', ' ', 'a', ' ', 't', 'h', 'a', 'n', ' ', ' ');//6
            _SetBoard(07, ' ', ' ', 'o', ' ', ' ', 'e', 'r', 'r', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//7
            _SetBoard(08, ' ', ' ', 't', ' ', ' ', 'd', ' ', 'e', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//8
            _SetBoard(09, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//9
            _SetBoard(10, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//10
            _SetBoard(11, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//11
            _SetBoard(12, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//12
            _SetBoard(13, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//13
            _SetBoard(14, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ');//14
            //            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2', '3', '4'
            //                                                              '1', '1', '1', '1', '1'

            var ordPlays = _PrintPossiblePlays(_GetTiles("koolcig".ToCharArray())); //double a in row 5 should be filtered
            Assert.That(ordPlays.First().Score, Is.EqualTo(34));
            Assert.That(ordPlays.First().Word, Is.EqualTo("livestock"));
            Assert.That(ordPlays.ElementAt(1).Score, Is.EqualTo(33));
            Assert.That(ordPlays.ElementAt(1).Word, Is.EqualTo("clockwise"));
        }
    }
}
