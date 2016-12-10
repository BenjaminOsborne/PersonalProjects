using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Scrabble.Data;

namespace Scrabble.Engine
{
    public class Game
    {
        private readonly LetterTileRepository m_oRepository;

        public Game(LetterTileRepository oRepository, Board oBoard, IEnumerable<string> enPlayers)
        {
            m_oRepository = oRepository;
            Board = oBoard;
            
            var i = 0;
            foreach (var sPlayer in enPlayers)
            {
                m_listPlayerOrder.Add(Tuple.Create(i++, new Player(oRepository, sPlayer)));
            }
            
            ScoreLog = new ScoreLog(Players);
        }

        public Board Board { get; private set; }

        public void DebugPrintResult()
        {
            Board.DebugPrint();

            OutputPrinter.WriteLine("");

            foreach (var oPlayer in ScoreLog.Players)
            {
                var listLogs = ScoreLog.GetPlayerLogs(oPlayer).ToList();
                OutputPrinter.WriteLine("Player: " + oPlayer.Name + ". Score: " + oPlayer.Score + " (" + oPlayer.Score / listLogs.Count +" avg.)");
                var nScoreTally = 0;
                foreach (var oLogItem in listLogs)
                {
                    OutputPrinter.WriteLine(oLogItem.AllTilesInWord.GetWord() + " (" + oLogItem.Score + ")");
                    nScoreTally += oLogItem.Score;
                }
                Debug.Assert(oPlayer.Score == nScoreTally, "scores not equal");
                OutputPrinter.WriteLine("");
            }
        }

        private readonly List<Tuple<int, Player>> m_listPlayerOrder = new List<Tuple<int, Player>>();
        public IEnumerable<Player> Players { get { return m_listPlayerOrder.Select(x => x.Item2); } } 

        public ScoreLog ScoreLog { get; private set; }

        public IEnumerable<LetterTile> RemainingTilesInBag { get { return m_oRepository.RemainingTiles; } } 

        private int m_nNextPlayer = 0;
        public Player GetNextPlayerToMove
        {
            get
            {
                var tupPlayer = m_listPlayerOrder.FirstOrDefault(x => x.Item1 == m_nNextPlayer);
                Debug.Assert(tupPlayer != null);
                
                m_nNextPlayer++;
                if (m_nNextPlayer == m_listPlayerOrder.Count)
                {
                    m_nNextPlayer = 0;
                }
                
                return tupPlayer.Item2;
            }
        }

        public void MakePlay(Player oPlayer, PossiblePlay oPlay)
        {
            if (oPlay == null || oPlayer == null)
            {
                return;
            }

            var listUsedFromHand = _UpdateBoardWithPlay(oPlayer, oPlay);

            //Update player
            oPlayer.MakePlayAndDraw(listUsedFromHand, oPlay.Score);

            ScoreLog.LogPlacement(new SinglePlacementLog(oPlayer, oPlay.AllLetterTiles, oPlay.LetterTilesFromHand, oPlay.Score));
        }

        private IEnumerable<LetterTile> _UpdateBoardWithPlay(Player oPlayer, PossiblePlay oPlay)
        {
            var nWidth = oPlay.StartPosition.StartWidthPosition;
            var nheight = oPlay.StartPosition.StartHeightPosition;

            var listUsedFromHand = new List<LetterTile>();
            var listLeftInHand = oPlayer.CurrentHand.ToList();
            var nIndex = 0;
            foreach (var oTile in oPlay.AllLetterTiles)
            {
                Debug.Assert(oPlay.Word[nIndex++] == oTile.Letter);

                var oBoardTile = Board[nWidth, nheight];
                if (oBoardTile.HasLetter == false)
                {
                    var cLetter = (oTile.Value == 0) ? '*' : oTile.Letter; //Play doesn't have blanks, but letters with 0 value
                    var oTileFromHand = listLeftInHand.FirstOrDefault(x => x.Letter == cLetter);
                    Debug.Assert(oTileFromHand != null);

                    oBoardTile.LetterTile = oTileFromHand;

                    listUsedFromHand.Add(oTileFromHand);
                    listLeftInHand.Remove(oTileFromHand);
                }

                if (oPlay.Direction == Direction.Across) { nWidth++; }
                else                                     {nheight++;}
            }

            Debug.Assert(listUsedFromHand.Count == oPlay.LetterTilesFromHand.Count());
            return listUsedFromHand;
        }
    }

    public class AutoGame
    {
        private Optimiser m_oOptimiser;
        public AutoGame(IEnumerable<string> enPlayers)
        {
            var oBoard = new Board();
            Game = new Game(new LetterTileRepository(), oBoard, enPlayers);
            m_oOptimiser = new Optimiser(new Words(new WordList()), oBoard);
        }

        public Game Game { get; private set; }

        public void PlayFullGame()
        {
            var nPlayersWhoCantPlay = 0;
            var nPlayers = Game.Players.Count();
            while (nPlayersWhoCantPlay < nPlayers)
            {
                var oPlayer = Game.GetNextPlayerToMove;
                var oPlay = m_oOptimiser.GetPossiblePlaysByScore(oPlayer.CurrentHand).FirstOrDefault();
                if (oPlay == null)
                {
                    OutputPrinter.WriteLine(oPlayer.Name + " could not make a play");
                    nPlayersWhoCantPlay++;
                }
                else
                {
                    Game.MakePlay(oPlayer, oPlay);
                    nPlayersWhoCantPlay = 0;
                }

                if (Game.RemainingTilesInBag.Any() == false && oPlayer.CurrentHand.Any() == false)
                {
                    break;
                }
            }

            Game.DebugPrintResult();
        }
    }
}
