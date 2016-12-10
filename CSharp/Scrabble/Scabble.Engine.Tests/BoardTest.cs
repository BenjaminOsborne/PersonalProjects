using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Scrabble.Data;
using Scrabble.Engine;

namespace Scabble.Engine.Tests
{
    public class BoardTest
    {
        private Words m_oWords;
        private Board m_oBoard;

        [SetUp]
        public void SetUp()
        {
            m_oWords = new Words(new WordList());
            m_oBoard = new Board();
        }

        [Test]
        public void Default_board_size_correct_is_blank_and_bonus()
        {
            Assert.That(m_oBoard.Width, Is.EqualTo(15));
            Assert.That(m_oBoard.Height, Is.EqualTo(15));

            var oTile = m_oBoard[0, 0];
            Assert.That(oTile.TileBonus, Is.EqualTo(TileBonus.TripleWord));
            oTile = m_oBoard[7, 7];
            Assert.That(oTile.TileBonus, Is.EqualTo(TileBonus.DoubleWord));

            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    Assert.That(m_oBoard[i,j].HasLetter, Is.EqualTo(false));
                }
            }
        }

        [Test]
        public void Assign_single_value()
        {
            m_oBoard[1,2].LetterTile = new LetterTile('e', 1);

            var oTile = m_oBoard[1, 2];
            Assert.That(oTile.HasLetter, Is.EqualTo(true));
            Assert.That(oTile.LetterTile.Letter, Is.EqualTo('e'));
            Assert.That(oTile.LetterTile.Value, Is.EqualTo(1));
        }

        [Test]
        public void When_getting_possible_position_single_tile_top_corner()
        {
            m_oBoard[0, 0].LetterTile = new LetterTile('e', 1);

            var listPositions = m_oBoard.AvailableBoardPositions.ToList();

            Assert.That(listPositions.Count, Is.EqualTo(4)); //2 width, 2 height

            var listColPositions = listPositions.Where(x => x.Direction == Direction.Down).ToList();
            Assert.That(listColPositions.Count, Is.EqualTo(2));

            Assert.That(listColPositions[0].BoardTileSpaces.Count(x => x.IsEmpty == false), Is.EqualTo(1));
            Assert.That(listColPositions[0].BoardTileSpaces.First().Letter.Letter, Is.EqualTo('e'));

            Assert.That(listColPositions[1].BoardTileSpaces.Count(x => x.IsEmpty == false), Is.EqualTo(0));
        }

        [Test]
        public void When_getting_possible_position_single_tile_in_middle()
        {
            m_oBoard[7, 7].LetterTile = new LetterTile('e', 1);

            var listPositions = m_oBoard.AvailableBoardPositions.ToList();
            Assert.That(listPositions.Count, Is.EqualTo(6)); //3 each way

            var listColPositions = listPositions.Where(x => x.Direction == Direction.Down).ToList();

            Assert.That(listColPositions.Count, Is.EqualTo(3));

            Assert.That(listColPositions[0].BoardTileSpaces.Count(x => x.IsEmpty == false), Is.EqualTo(0));
            Assert.That(listColPositions[0].BoardTileSpaces.Count(x => x.IsEmpty), Is.EqualTo(15));

            Assert.That(listColPositions[1].BoardTileSpaces.Count(x => x.IsEmpty == false), Is.EqualTo(1));
            Assert.That(listColPositions[1].BoardTileSpaces.Count(x => x.IsEmpty), Is.EqualTo(14));
            Assert.That(listColPositions[1].BoardTileSpaces.ElementAt(7).Letter.Letter, Is.EqualTo('e'));

            Assert.That(listColPositions[2].BoardTileSpaces.Count(x => x.IsEmpty == false), Is.EqualTo(0));
            Assert.That(listColPositions[2].BoardTileSpaces.Count(x => x.IsEmpty), Is.EqualTo(15));
        }

        [Test]
        public void When_getting_possible_position_multiple_tiles_in_middle()
        {
            m_oBoard[7, 7].LetterTile = new LetterTile('e', 1);
            m_oBoard[7, 8].LetterTile = new LetterTile('a', 1);

            var listPositions = m_oBoard.AvailableBoardPositions.ToList();
            Assert.That(listPositions.Count, Is.EqualTo(7)); //3 height, 4 width

            var listColPositions = listPositions.Where(x => x.Direction == Direction.Down).ToList();

            Assert.That(listColPositions.Count, Is.EqualTo(3)); 

            Assert.That(listColPositions[0].BoardTileSpaces.Count(x => x.IsEmpty == false), Is.EqualTo(0));

            Assert.That(listColPositions[1].BoardTileSpaces.Count(x => x.IsEmpty == false), Is.EqualTo(2));
            Assert.That(listColPositions[1].BoardTileSpaces.Count(x => x.IsEmpty), Is.EqualTo(13));
            Assert.That(listColPositions[1].BoardTileSpaces.ElementAt(7).Letter.Letter, Is.EqualTo('e'));
            Assert.That(listColPositions[1].BoardTileSpaces.ElementAt(8).Letter.Letter, Is.EqualTo('a'));

            Assert.That(listColPositions[2].BoardTileSpaces.Count(x => x.IsEmpty == false), Is.EqualTo(0));
        }

        [Test]
        public void When_getting_possible_from_2_blocks_of_multiple_tiles_in_middle()
        {
            m_oBoard[7, 7].LetterTile = new LetterTile('e', 1);
            m_oBoard[7, 8].LetterTile = new LetterTile('a', 1);

            m_oBoard[7, 11].LetterTile = new LetterTile('f', 2);
            m_oBoard[7, 12].LetterTile = new LetterTile('g', 2);

            var listPositions = m_oBoard.AvailableBoardPositions.ToList();
            Assert.That(listPositions.Count, Is.EqualTo(11)); //3 height, 8 width

            var listColPositions = listPositions.Where(x => x.Direction == Direction.Down).ToList();

            Assert.That(listColPositions.Count, Is.EqualTo(3)); 

            Assert.That(listColPositions[0].BoardTileSpaces.Count(x => x.IsEmpty == false), Is.EqualTo(0));

            Assert.That(listColPositions[1].BoardTileSpaces.Count(x => x.IsEmpty == false), Is.EqualTo(4));
            Assert.That(listColPositions[1].BoardTileSpaces.Count(x => x.IsEmpty), Is.EqualTo(11));
            Assert.That(listColPositions[1].BoardTileSpaces.ElementAt(7).Letter.Letter, Is.EqualTo('e'));
            Assert.That(listColPositions[1].BoardTileSpaces.ElementAt(8).Letter.Letter, Is.EqualTo('a'));
            Assert.That(listColPositions[1].BoardTileSpaces.ElementAt(11).Letter.Letter, Is.EqualTo('f'));
            Assert.That(listColPositions[1].BoardTileSpaces.ElementAt(12).Letter.Letter, Is.EqualTo('g'));

            Assert.That(listColPositions[2].BoardTileSpaces.Count(x => x.IsEmpty == false), Is.EqualTo(0));
        }

        [Test]
        public void When_getting_constraints_from_single_tile_either_side()
        {
            m_oBoard[7, 7].LetterTile = new LetterTile('e', 1);
            m_oBoard[5, 7].LetterTile = new LetterTile('a', 1);

            var listPositions = m_oBoard.AvailableBoardPositions.ToList();
            Assert.That(listPositions.Count, Is.EqualTo(8)); //5 height, 3 width

            var listColPositions = listPositions.Where(x => x.Direction == Direction.Down).ToList();

            Assert.That(listColPositions.Count, Is.EqualTo(5));

            Assert.That(listColPositions[2].BoardTileSpaces.Count(x => x.IsEmpty == false), Is.EqualTo(0)); //middle column between letters
            Assert.That(listColPositions[2].ExernalConstraints.Count(), Is.EqualTo(1));
            Assert.That(listColPositions[2].ExernalConstraints.ElementAt(0).FollowingLetters.Count(), Is.EqualTo(1));
            Assert.That(listColPositions[2].ExernalConstraints.ElementAt(0).FollowingLetters.ElementAt(0).Letter, Is.EqualTo('e'));
            Assert.That(listColPositions[2].ExernalConstraints.ElementAt(0).PreceedingLetters.Count(), Is.EqualTo(1));
            Assert.That(listColPositions[2].ExernalConstraints.ElementAt(0).PreceedingLetters.ElementAt(0).Letter, Is.EqualTo('a'));
        }

        [Test]
        public void When_getting_constraints_from_multiple_tiles_on_one_side()
        {
            m_oBoard[7, 7].LetterTile = new LetterTile('e', 1);
            m_oBoard[4, 7].LetterTile = new LetterTile('b', 3);
            m_oBoard[5, 7].LetterTile = new LetterTile('a', 1);

            var listPositions = m_oBoard.AvailableBoardPositions.ToList();
            Assert.That(listPositions.Count, Is.EqualTo(9)); //6 height, 3 width

            var listColPositions = listPositions.Where(x => x.Direction == Direction.Down).ToList();

            Assert.That(listColPositions.Count, Is.EqualTo(6));

            Assert.That(listColPositions[3].BoardTileSpaces.Count(x => x.IsEmpty == false), Is.EqualTo(0)); //middle column between letters
            Assert.That(listColPositions[3].ExernalConstraints.Count(), Is.EqualTo(1));
            Assert.That(listColPositions[3].ExernalConstraints.ElementAt(0).FollowingLetters.Count(), Is.EqualTo(1));
            Assert.That(listColPositions[3].ExernalConstraints.ElementAt(0).FollowingLetters.ElementAt(0).Letter, Is.EqualTo('e'));
            Assert.That(listColPositions[3].ExernalConstraints.ElementAt(0).PreceedingLetters.Count(), Is.EqualTo(2));
            Assert.That(listColPositions[3].ExernalConstraints.ElementAt(0).PreceedingLetters.ElementAt(0).Letter, Is.EqualTo('b'));
            Assert.That(listColPositions[3].ExernalConstraints.ElementAt(0).PreceedingLetters.ElementAt(1).Letter, Is.EqualTo('a'));
        }
    }
}
