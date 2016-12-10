using System;
using System.Collections.Generic;
using System.Linq;
using ConnectFour.Engine;
using ConnectFour.Runner;
using Microsoft.FSharp.Core;
using NUnit.Framework;

namespace ConnectFour.Test
{
    [TestFixture]
    public class EngineTestBase
    {
        protected CellState m_redState;
        protected CellState m_yellowState;

        protected CounterColour Y = CounterColour.Yellow;
        protected CounterColour R = CounterColour.Red;
        protected CounterColour _ = null;
        
        protected Calculator m_calculator;

        [SetUp]
        public virtual void SetUp()
        {
            m_redState = CellStateHelper.CellState(CounterColour.Red);
            m_yellowState = CellStateHelper.CellState(CounterColour.Yellow);

            m_calculator = new Calculator();
        }

        protected BoardState _BuildBoard(CounterColour lastPlayed, params CounterColour[][] arrColours)
        {
            var arrInitial = RunnerHelpers.InitialiseCellState();

            var listColours = arrColours.ToList();

            if (listColours.Any() == false)
            {
                return new BoardState(arrInitial, lastPlayed);
            }

            Assert.That(listColours.First().Length, Is.LessThanOrEqualTo(7));
            Assert.That(listColours.All(x => x.Length == listColours.First().Length), "Expecting Same length");

            listColours.Reverse();

            for (int height = 0; height < listColours.Count; height++)
            {
                var row = listColours[height];
                for (int slot = 0; slot < row.Length; slot++)
                {
                    var cell = row[slot];
                    if (cell != null)
                    {
                        arrInitial[slot, height] = (cell == CounterColour.Red) ? m_redState : m_yellowState;
                    }
                }
            }
            return new BoardState(arrInitial, lastPlayed);
        }
    }

    public class When_getting_four_in_a_row_Tests : EngineTestBase
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void When_creating_empty_board()
        {
            var board = new BoardState();

            var redFourInRow = board.GetFourInARow(CounterColour.Red);
            Assert.IsNull(redFourInRow);

            var yellowFourInRow = board.GetFourInARow(CounterColour.Yellow);
            Assert.IsNull(yellowFourInRow);
        }

        [Test]
        public void When_four_in_a_row_right()
        {
            var colour = CounterColour.Red;
            var board = _BuildBoard(colour, new[] { R, R, R, R });
            
            _AssertFourInARow(board, colour, Direction.Right, 0, 0);
        }

        [Test]
        public void When_four_in_a_row_up()
        {
            var colour = CounterColour.Red;
            var board = _BuildBoard(colour, new[] { R },
                                            new[] { R },
                                            new[] { R },
                                            new[] { R });
            _AssertFourInARow(board, colour, Direction.Down, 0, 3);
        }

        [Test]
        public void When_four_in_a_row_up_left()
        {
            var colour = CounterColour.Red;
            var board = _BuildBoard(colour, new[] { R,_,_,_ },
                                            new[] { Y,R,_,_ },
                                            new[] { Y,Y,R,_ },
                                            new[] { Y,Y,Y,R });

            _AssertFourInARow(board, colour, Direction.DownRight, 0, 3);
        }

        [Test]
        public void When_four_in_a_row_up_right()
        {
            //First row irrelevant
            var colour = CounterColour.Yellow;
            var board = _BuildBoard(colour, new[] { _,_,_,_,_,Y },
                                            new[] { _,_,_,_,Y,R },
                                            new[] { _,_,_,Y,R,R },
                                            new[] { _,_,Y,R,R,R },
                                            new[] { _,_,Y,R,R,Y });

            _AssertFourInARow(board, colour, Direction.DownLeft, 5, 4);
        }

        private static void _AssertFourInARow(BoardState board, CounterColour colour, Direction direction, int index, int height)
        {
            var fourAcross = board.GetFourInARow(colour);
            Assert.IsNotNull(fourAcross);
            Assert.That(fourAcross.Value.Colour, Is.EqualTo(colour));
            Assert.That(fourAcross.Value.Direction, Is.EqualTo(direction));
            Assert.That(fourAcross.Value.StartLocation.SlotIndex, Is.EqualTo(index));
            Assert.That(fourAcross.Value.StartLocation.HeightIndex, Is.EqualTo(height));
            Assert.That(fourAcross.Value.Length, Is.EqualTo(4));
        }
    }

    public class When_getting_next_position_Tests : EngineTestBase
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void When_empty_board()
        {
            var colour = CounterColour.Red;
            var board = _BuildBoard(colour);

            var groupPositions = _GetPlaysByPosition(board);
            Assert.That(groupPositions.SelectMany(x => x).All(x => x.StartLocation.HeightIndex == 0));
            Assert.That(groupPositions.All(x => x.All(y => y.Length == 1)));
        }

        [Test]
        public void When_one_counter_in_middle()
        {
            var colour = CounterColour.Red;
            var board = _BuildBoard(colour, new[] {_,_,_,R});

            var groupByLocation = _GetPlaysByPosition(board);

            var middlePosition = groupByLocation.Single(x => x.Key == 3);
            _AssertPosition(middlePosition.First().StartLocation, 3, 1);
            Assert.That(middlePosition.All(x => x.Length == 1));
        }

        [Test]
        public void When_two_counters_stacked_in_middle()
        {
            var board = _BuildBoard(CounterColour.Red, new[] { _, _, _, R },
                                                       new[] { _, _, _, Y });

            var groupByLocation = _GetPlaysByPosition(board);

            var middlePosition = groupByLocation.Single(x => x.Key == 3);
            _AssertPosition(middlePosition.First().StartLocation, 3, 2);
            Assert.That(middlePosition.All(x => x.Length == 1));
        }

        [Test]
        public void When_two_counters_side_by_side()
        {
            var board = _BuildBoard(CounterColour.Red, new[] { _, _, R, Y });

            var groupByLocation = _GetPlaysByPosition(board);

            var yellowPosition = groupByLocation.Single(x => x.Key == 3);
            _AssertPosition(yellowPosition.First().StartLocation, 3, 1);

            Assert.That(yellowPosition.Single(x => x.Direction == Direction.Down).Length, Is.EqualTo(2));
            Assert.That(yellowPosition.Where(x => x.Direction != Direction.Down).All(x => x.Length == 1));
        }

        [Test]
        public void When_some_spaces_cannot_go_right()
        {
            var board = _BuildBoard(CounterColour.Red,  new[] { _, _, R, _, _, _, R },
                                                        new[] { Y, _, Y, R, Y, R, Y });

            var groupByLocation = _GetPlaysByPosition(board);

            //Only slot 2 and 6 can possibly build acrosss for Yellow
            Assert.That(groupByLocation.Where(x => x.Key != 2 && x.Key != 6).SelectMany(x => x).Any(x => x.Direction == Direction.Right) == false);
        }

        [Test]
        public void When_down_right_at_right_edge_and_down_left_at_left_edge()
        {
            var board = _BuildBoard(CounterColour.Red,  new[] { _, _, _, _, _, Y, R },
                                                        new[] { R, _, _, _, Y, R, Y },
                                                        new[] { Y, _, _, _, R, Y, R });
            var groupByLocation = _GetPlaysByPosition(board);
            
            var slot0 = groupByLocation.Single(x => x.Key == 0);
            Assert.That(slot0.Any(x => x.Direction == Direction.DownRight) == false);
            
            var slot6 = groupByLocation.Single(x => x.Key == 6);
            var downLeft = slot6.Single(x => x.Direction == Direction.DownLeft);
            Assert.That(downLeft.Length == 3);
        }

        [Test]
        public void When_right_with_gap()
        {
            var board = _BuildBoard(CounterColour.Red, new[] {Y, _, _, Y});

            var groupByLocation = _GetPlaysByPosition(board);
            var slot1 = groupByLocation.Single(x => x.Key == 1);
            var slot2 = groupByLocation.Single(x => x.Key == 2);
            Assert.That(slot1.Single(x => x.Direction == Direction.Right).Length, Is.EqualTo(2));
            Assert.That(slot2.Single(x => x.Direction == Direction.Right).Length, Is.EqualTo(2));
        }

        [Test]
        public void When_on_limits_at_height()
        {
            var board = _BuildBoard(CounterColour.Red,  new[] { _, _, _, _, _, _, _ },
                                                        new[] { _, _, _, _, _, Y, _ },
                                                        new[] { _, _, _, _, _, Y, _ },
                                                        new[] { R, _, _, _, _, Y, _ },
                                                        new[] { Y, _, _, _, _, R, _ },
                                                        new[] { Y, _, _, _, _, R, _ });
            var groupByLocation = _GetPlaysByPosition(board);

            //Yellow
            var slot0 = groupByLocation.Single(x => x.Key == 0);
            Assert.That(slot0.Any(x => x.Direction.Equals(Direction.Down)) == false);

            //Can go down
            var slot5 = groupByLocation.Single(x => x.Key == 5);
            Assert.That(slot5.Single(x => x.Direction == Direction.Down).Length == 4);
        }

        private static void _AssertPosition(Location position, int slot, int height)
        {
            Assert.That(position.SlotIndex, Is.EqualTo(slot));
            Assert.That(position.HeightIndex, Is.EqualTo(height));
        }

        private static List<IGrouping<int, CounterChain>> _GetPlaysByPosition(BoardState board, int positionCount = 7)
        {
            var listGroup = board.GetBoardPositionsForNextPlay().GroupBy(x => x.StartLocation.SlotIndex).ToList();
            Assert.That(listGroup.Count, Is.EqualTo(positionCount));
            return listGroup;
        }
    }

    public class When_getting_next_play_Tests : EngineTestBase
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void When_four_in_a_row_across()
        {
            _Assert1PlayWin(CounterColour.Yellow, new[] { R, R, _, R });
        }

        [Test]
        public void When_four_in_a_row_up()
        {
            _Assert1PlayWin(CounterColour.Yellow,   new[] { R },
                                                    new[] { R },
                                                    new[] { R });
        }

        [Test]
        public void When_four_in_a_row_diagonal()
        {
            _Assert1PlayWin(CounterColour.Red,  new[] { _, _, _, _, _, Y },
                                                new[] { _, _, _, _, Y, R },
                                                new[] { _, _, _, _, R, R },
                                                new[] { _, _, Y, R, R, R },
                                                new[] { _, _, Y, R, R, Y });
        }

        [Test]
        public void When_all_slots_possible()
        {
            var listPlays = _AssertAllSlotsPossible(CounterColour.Red, new[] { R, Y, _, Y });

            _AssertSlotPlay(listPlays, 0, 1, 1);
            _AssertSlotPlay(listPlays, 1, 1, 2);
            _AssertSlotPlay(listPlays, 2, 0, 3);
            _AssertSlotPlay(listPlays, 3, 1, 2);
            _AssertSlotPlay(listPlays, 4, 0, 2);
            _AssertSlotPlay(listPlays, 5, 0, 1);
        }

        private static void _AssertSlotPlay(IEnumerable<BoardPlay> enPlays, int slot, int height, int length)
        {
            var slotPlay = enPlays.Single(x => x.PlayPosition.SlotIndex == slot);
            Assert.That(slotPlay.PlayPosition.HeightIndex, Is.EqualTo(height));
            Assert.That(slotPlay.PlayValue, Is.EqualTo(PlayValueHelper.Value(length)));
        }

        protected IList<BoardPlay> _AssertAllSlotsPossible(CounterColour lastPlayed, params CounterColour[][] arrColours)
        {
            var listPlays = _GetNextPlay(lastPlayed, arrColours);
            Assert.That(listPlays.Count, Is.EqualTo(7));
            Assert.That(listPlays.Select(x => x.PlayPosition.SlotIndex).Distinct().Count(), Is.EqualTo(7));
            Assert.That(listPlays.All(x => x.PlayValue != PlayValue.Win));
            Assert.That(listPlays.All(x => x.Board.LastPlayed != lastPlayed), "New Board should be opposite colour");
            return listPlays;
        }

        protected void _Assert1PlayWin(CounterColour lastPlayed, params CounterColour[][] arrColours)
        {
            var listPlays = _GetNextPlay(lastPlayed, arrColours);
            Assert.That(listPlays.Count, Is.EqualTo(1));
            Assert.That(listPlays.Single().PlayValue, Is.EqualTo(PlayValue.Win));
        }

        protected IList<BoardPlay> _GetNextPlay(CounterColour lastPlayed, params CounterColour[][] arrColours)
        {
            var board = _BuildBoard(lastPlayed, arrColours);
            return m_calculator.GetNextPlays(board).ToList();
        }
    }

    public class When_getting_play_tree_Tests : EngineTestBase
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void When_1_play_to_win()
        {
            var board = _BuildBoard(CounterColour.Yellow,   new[] {Y, Y, Y, _,},
                                                            new[] {R, R, R, _,});

            var listBranches = m_calculator.GetPlayTree(board, 2).ToList();
            Assert.That(listBranches.Count == 1);
        }

        [Test]
        public void When_red_can_always_win_after_2_rounds()
        {
            var board = _BuildBoard(CounterColour.Red,  new[] { _, Y, Y, Y, },
                                                        new[] { _, R, R, R, });

            var listPlays = _GetBoardPlayDepths(board, 2);
            Assert.That(listPlays.Count == 7);
            foreach (var play in listPlays)
            {
                Assert.That(play.Depth, Is.EqualTo(2));
                Assert.That(play.Play.PlayValue, Is.EqualTo(PlayValueHelper.Win()));
                Assert.That(play.Play.Board.LastPlayed, Is.EqualTo(CounterColour.Red));

                var slot = play.Play.PlayPosition.SlotIndex;
                var height = play.Play.PlayPosition.HeightIndex;
                Assert.That(slot == 0 || slot == 4);
                Assert.That(height == 0);
            }
        }

        [Test]
        public void When_yellow_can_win_in_3_moves()
        {
            var board = _BuildBoard(CounterColour.Red, new[] { Y, Y, _, _, },
                                                       new[] { R, R, Y, R, });

            var listPlays = _GetBoardPlayDepths(board, 2);
            Assert.That(listPlays.All(x => x.Play.Board.LastPlayed == CounterColour.Yellow));
            Assert.That(listPlays.All(x => x.Depth == 3));

            var listWins = listPlays.Where(x => x.Play.PlayValue == PlayValueHelper.Win()).ToList();
            Assert.That(listWins.All(x => x.Play.PlayPosition.HeightIndex == 1));
            Assert.That(listWins.All(x => x.Play.PlayPosition.SlotIndex == 2 || x.Play.PlayPosition.SlotIndex == 3));
        }

        [Test]
        public void When_either_can_win_in_3_moves()
        {
            var board = _BuildBoard(CounterColour.Red, new[] { _, _, _, _, _, _, _, },
                                                       new[] { Y, Y, _, _, _, R, R, });

            var listPlays = _GetBoardPlayDepths(board, 2);

            Assert.That(listPlays.All(x => x.Play.Board.LastPlayed == CounterColour.Yellow));
            Assert.That(listPlays.All(x => x.Depth == 3));

            var listWins = listPlays.Where(x => x.Play.PlayValue == PlayValueHelper.Win()).ToList();
            Assert.That(listWins.All(x => x.Play.PlayPosition.HeightIndex == 0));
            Assert.That(listWins.All(x =>
            {
                var slot = x.Play.PlayPosition.SlotIndex;
                var height = x.Play.PlayPosition.HeightIndex;
                return (slot >=2 && slot <= 4) && height == 0;
            }));
        }

        private IList<PlayTreeDepth> _GetBoardPlayDepths(BoardState board, int numColourPlays)
        {
            var listBranches = m_calculator.GetPlayTree(board, numColourPlays).ToList();
            return _GetBoardPlays(listBranches);
        }

        private IList<PlayTreeDepth> _GetBoardPlays(IEnumerable<PlayTree> enTrees)
        {
            var listPlays = enTrees.SelectMany(x => PlayBranchHelper.FlattenBoardPlays(x, null)).ToList();
            foreach (var groupDepth in listPlays.GroupBy(x => x.Depth))
            {
                Assert.That(groupDepth.All(x => x.Colour == groupDepth.First().Colour));
            }
            return listPlays;
        }
    }

    public class When_selecting_best_move_Tests : EngineTestBase
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void When_empty_board()
        {
            var board = _BuildBoard(CounterColour.Yellow);

            var bestPlay = m_calculator.GetNextBestPlay(board, 3).Value;

            Assert.That(bestPlay.BestPlay.NextPlay.PlayPosition.SlotIndex, Is.EqualTo(3));
            Assert.That(bestPlay.AllPossible.Count(), Is.EqualTo(7));
            Assert.That(bestPlay.AllPossible.All(x => x.Score.State.IsScore));
        }

        [Test]
        public void When_yellow_wins_in_1_move()
        {
            var board = _BuildBoard(CounterColour.Yellow,   new[] { _, _, _, _, _, _, _, },
                                                            new[] { _, Y, Y, Y, _, R, R, });
            var listRedPlays = _GetNextPlays(board, 2);

            Assert.That(listRedPlays.All(x => x.Score.State.IsLose));

            var bestPlay = m_calculator.GetNextBestPlay(board, 2);
            Assert.That(bestPlay != null && bestPlay.Value != null);
            var nextPlay = bestPlay.Value.BestPlay.NextPlay;
            Assert.That(nextPlay.PlayPosition.SlotIndex, Is.EqualTo(3));

            var listYellowPlays = _GetNextPlays(nextPlay.Board, 2);
            Assert.That(listYellowPlays.Count, Is.EqualTo(1));
            Assert.That(listYellowPlays.Single().Score.State.IsWin);
        }

        [Test]
        public void When_yellow_wins_in_2()
        {
            var board = _BuildBoard(CounterColour.Yellow,   new[] { R, R, _, _, _, _, _, },
                                                            new[] { Y, Y, Y, _, _, R, R, },
                                                            new[] { Y, Y, Y, _, _, Y, Y, },
                                                            new[] { R, R, R, _, _, R, R, },
                                                            new[] { Y, Y, Y, _, _, Y, R, });
            var listRedPlays1 = _GetNextPlays(board, 3);
            Assert.That(listRedPlays1.All(x => x.Score.State.IsLose));

            var redBoard1 = _AssertNextPlayAndGetBoard(board, 3, 7, 0, LeafStateHelper.Lose, 4, 3);

            var yellowBoard2 = _AssertNextPlayAndGetBoard(redBoard1, 3, 7, 1, LeafStateHelper.Win, 3, 3);

            var redBoard2 = _AssertNextPlayAndGetBoard(yellowBoard2, 3, 7, 0, LeafStateHelper.Lose, 2, 3);

            var yellowBoard3 = _AssertNextPlayAndGetBoard(redBoard2, 3, 1, 1, LeafStateHelper.Win, 1, 3);
        }

        [Test]
        public void When_red_should_block_slot_4_or_6()
        {
            var board = _BuildBoard(CounterColour.Yellow,   new[] { _, _, R, R, _, _, _ },
                                                            new[] { _, _, R, Y, _, _, _ },
                                                            new[] { Y, Y, R, Y, R, _, _ },
                                                            new[] { R, R, Y, R, R, Y, Y },
                                                            new[] { R, Y, Y, R, Y, R, Y },
                                                            new[] { R, R, Y, Y, Y, R, Y });

            var listPlays = _GetNextPlays(board, 2);
            Assert.That(listPlays.Count, Is.EqualTo(5), "Sould have 5 options");

            var bestPlay = m_calculator.GetNextBestPlay(board, 3).Value;
            var slot = bestPlay.BestPlay.NextPlay.PlayPosition.SlotIndex;
            Assert.That(slot == 4 || slot == 6); //Both can win for yellow!
        }

        [Test]
        public void When_red_should_play_0_or_6()
        {
            var board = _BuildBoard(CounterColour.Yellow,   new[] { _, R, Y, R, R, _, _ },
                                                            new[] { R, Y, R, Y, R, _, _ },
                                                            new[] { R, R, R, Y, Y, _, _ },
                                                            new[] { Y, Y, Y, R, Y, _, _ },
                                                            new[] { R, R, Y, R, Y, _, Y },
                                                            new[] { R, Y, Y, Y, R, _, Y });

            var listPlays = _GetNextPlays(board, 3);
            Assert.That(listPlays.Count, Is.EqualTo(3), "Sould have 3 options");

            var bestPlay = m_calculator.GetNextBestPlay(board, 3).Value;
            var slot = bestPlay.BestPlay.NextPlay.PlayPosition.SlotIndex;
            Assert.That(slot == 0 || slot == 6); //Any but slot 5!
        }

        private BoardState _AssertNextPlayAndGetBoard(BoardState board, int playCount,
                                                      int possiblePlayCount, int possibleWins,
                                                      LeafState state, int depth, int slot)
        {
            var listPlays = _GetNextPlays(board, playCount);
            Assert.That(listPlays.Count, Is.EqualTo(possiblePlayCount));
            Assert.That(listPlays.Count(x => x.Score.State.IsWin), Is.EqualTo(possibleWins));

            var bestPlay1Wrap = m_calculator.GetNextBestPlay(board, playCount);
            var bestPlay1 = bestPlay1Wrap.Value.BestPlay;
            Assert.That(bestPlay1.Score.State, Is.EqualTo(state));
            Assert.That(bestPlay1.Score.Depth, Is.EqualTo(depth));
            Assert.That(bestPlay1.NextPlay.PlayPosition.SlotIndex, Is.EqualTo(slot));
            return bestPlay1Wrap.Value.BestPlay.NextPlay.Board;
        }

        private List<BoardPlayScore> _GetNextPlays(BoardState board, int nPlays)
        {
            var listNextPlays = m_calculator.GetNextPlayScores(board, nPlays).ToList();
            Assert.That(listNextPlays.Select(x => x.NextPlay.PlayPosition.SlotIndex).Distinct().Count(), Is.EqualTo(listNextPlays.Count), "Multiple plays for same slot");
            Assert.That(listNextPlays.All(x => x.NextPlay.Board.NextPlay == board.LastPlayed), "Next plays for next board should match start board last play");
            return listNextPlays;
        }
    }
}
