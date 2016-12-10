using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrabble.Engine
{
    public class ScoreLog
    {
        public ScoreLog(IEnumerable<Player> enPlayers)
        {
            foreach (var oPlayer in enPlayers.Where(x => m_listPlayers.Any(y => y.ID == x.ID) == false))
            {
                m_listPlayers.Add(oPlayer);
            }

            foreach (var oPlayer in m_listPlayers)
            {
                m_dicPlayerPlacements.Add(oPlayer, new List<SinglePlacementLog>());
            }
        }

        private readonly List<Player> m_listPlayers = new List<Player>(); 
        public IEnumerable<Player> Players { get { return m_listPlayers; } } 

        private Dictionary<Player, List<SinglePlacementLog>> m_dicPlayerPlacements = new Dictionary<Player, List<SinglePlacementLog>>(); 
        public void LogPlacement(SinglePlacementLog oPlacement)
        {
            if (oPlacement == null)
            {
                return;
            }

            _GetPlayerLogs(oPlacement.Player).Add(oPlacement);
        }

        public int PlayerScore(Player oPlayer)
        {
            return _GetPlayerLogs(oPlayer).Sum(x => x.Score);
        }

        public IEnumerable<SinglePlacementLog> GetPlayerLogs(Player oPlayer)
        {
            return _GetPlayerLogs(oPlayer);
        }

        private IList<SinglePlacementLog> _GetPlayerLogs(Player oPlayer)
        {
            List<SinglePlacementLog> listPlacements;
            if (m_dicPlayerPlacements.TryGetValue(oPlayer, out listPlacements))
            {
                return listPlacements;
            }
            return new List<SinglePlacementLog>();
        }
    }

    public class SinglePlacementLog
    {
        public SinglePlacementLog(Player oPlayer, IEnumerable<LetterTile> enAllTilesInWord, IEnumerable<LetterTile> enTilesFromHand, int nScore)
        {
            Player = oPlayer;
            AllTilesInWord = enAllTilesInWord.ToList();
            TilesFromHand = enTilesFromHand.ToList();
            Score = nScore;
        }

        public Player Player { get; private set; }
        public IEnumerable<LetterTile> AllTilesInWord { get; private set; }
        public IEnumerable<LetterTile> TilesFromHand { get; private set; }
        public int Score { get; private set; }


    }
}
