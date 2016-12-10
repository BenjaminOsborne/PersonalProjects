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
    public class GameTest
    {
        private Words m_oWords;
        private Board m_oBoard;
        private Optimiser m_oOptimiser;
        private LetterTileRepository m_oTileRepository;
        private Stopwatch m_oSW;
        private Game m_oGame;
        private List<String> m_listPlayerNames;
        
        [SetUp]
        public void SetUp()
        {
            m_oWords = new Words(new WordList());
            m_oBoard = new Board();
            m_oOptimiser = new Optimiser(m_oWords, m_oBoard);
            m_oTileRepository = new LetterTileRepository();
            m_oSW = new Stopwatch();

            m_listPlayerNames = new List<string>(){"Ben", "Lizzie"};

            m_oGame = new Game(m_oTileRepository, m_oBoard, m_listPlayerNames);
        }

        [Test]
        public void First_play_test()
        {
            var oPlayer = m_oGame.GetNextPlayerToMove;
            Assert.That(oPlayer.Name, Is.EqualTo("Ben"));

            oPlayer.DrawTilesIntoHand();
            var listAllPlays = m_oOptimiser.GetPossiblePlaysByScore(oPlayer.CurrentHand).ToList();
            var oPlay = listAllPlays[0];
            Assert.That(oPlay.Score, Is.EqualTo(listAllPlays.Max(x => x.Score)));

            m_oGame.MakePlay(oPlayer, oPlay);

            var nMidHeight = 7;
            var listBoardTiles = new List<BoardTile>();
            for (int i = 0; i < 15; i++) { listBoardTiles.Add(m_oBoard[i, nMidHeight]); }

            var listBoardWord = listBoardTiles.Where(x => x.HasLetter).Select(x => x.LetterTile).ToList();
            Assert.That(listBoardWord.GetWord(), Is.EqualTo(oPlay.Word));

            Assert.That(oPlayer.Score, Is.EqualTo(oPlay.Score));

            Assert.That(m_oGame.ScoreLog.PlayerScore(oPlayer), Is.EqualTo(oPlay.Score));

            var listLogs = m_oGame.ScoreLog.GetPlayerLogs(oPlayer).ToList();
            Assert.That(listLogs.Count, Is.EqualTo(1));
            Assert.That(listLogs[0].AllTilesInWord.GetWord(), Is.EqualTo(oPlay.Word));
            Assert.That(listLogs[0].Score, Is.EqualTo(oPlay.Score));

            Debug.WriteLine("Word: " + oPlay.Word + ". Score: " + oPlay.Score);
        }

        [Test]
        public void Multiple_play_test()
        {
            //Assert.IsTrue(false); //this is not solving!
            for (int i = 0; i < 1; i++)
            {
                _MakeNextPlay("Ben");
                _MakeNextPlay("Lizzie");
            }
            m_oBoard.DebugPrint();
        }

        [Test]
        public void AutoGame_test()
        {
            var oAutoGame = new AutoGame(m_listPlayerNames);
            m_oSW.Start();
            oAutoGame.PlayFullGame();
            m_oSW.Stop();
            Debug.WriteLine("Total Game Time (s): " + (double)m_oSW.ElapsedMilliseconds / 1000.0);
        }

        private void _MakeNextPlay(string sAssertName)
        {
            var oPlayer = m_oGame.GetNextPlayerToMove;
            Assert.That(oPlayer.Name, Is.EqualTo(sAssertName));

            oPlayer.DrawTilesIntoHand();
            var listAllPlays = m_oOptimiser.GetPossiblePlaysByScore(oPlayer.CurrentHand).ToList();
            var oPlay = listAllPlays[0];
            m_oGame.MakePlay(oPlayer, oPlay);

            Debug.WriteLine("Word: " + oPlay.Word + ". Score: " + oPlay.Score);
        }
    }
}