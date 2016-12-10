using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Scrabble.Engine
{
    public class Player
    {
        private readonly LetterTileRepository m_oRepository;

        public Player(LetterTileRepository oRepository, string sName, int nHandSize = 7)
        {
            m_oRepository = oRepository;
            Name = sName;
            ID = Guid.NewGuid();
            HandSize = nHandSize;
            
            DrawTilesIntoHand();
        }

        public string Name { get; private set; }
        public Guid ID { get; private set; }
        public int Score { get; private set; }
        public int HandSize { get; private set; }

        private List<LetterTile> m_listTilesInHand = new List<LetterTile>();
        public IEnumerable<LetterTile> CurrentHand
        {
            get { return m_listTilesInHand; }
        }

        public void DrawTilesIntoHand()
        {
            Debug.Assert(m_listTilesInHand.Count >= 0);
            var nDraw = HandSize - m_listTilesInHand.Count;
            if (nDraw > 0)
            {
                m_listTilesInHand.AddRange(m_oRepository.GetNextLetterTiles(nDraw));
            }
        }

        public bool MakePlayAndDraw(IEnumerable<LetterTile> enPlayedTiles, int nPlayScore)
        {
            if (MakePlay(enPlayedTiles, nPlayScore))
            {
                DrawTilesIntoHand();
                return true;
            }
            return false;
        }

        public bool MakePlay(IEnumerable<LetterTile> enPlayedTiles, int nPlayScore)
        {
            var listOriginalHand = m_listTilesInHand.ToList();
            var bSucess = true;
            foreach (var oPlayedTile in enPlayedTiles)
            {
                if (oPlayedTile.IsBlankTile || oPlayedTile.Value == 0)
                {
                    bSucess &= m_listTilesInHand.Remove(m_listTilesInHand.FirstOrDefault(x => x.IsBlankTile));
                }
                else
                {
                    bSucess &= m_listTilesInHand.Remove(m_listTilesInHand.FirstOrDefault(x => x.Letter == oPlayedTile.Letter));
                }
                if (bSucess == false) { break; }
            }

            if (bSucess)
            {
                Score += nPlayScore;
                return true;
            }
            else
            {
                m_listTilesInHand = listOriginalHand;
                return false;
            }
        }
    }
}
