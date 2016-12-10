using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Scrabble.Engine
{
    public class LetterTileRepository
    {
        private Dictionary<char, LetterTile> m_dicLetterTileTypes = new Dictionary<char, LetterTile>();
        private List<LetterTile> m_listTileRepository = new List<LetterTile>();
        private Random m_oRandom = new Random();

        public LetterTileRepository()
        {
            _InitialiseTileTypes();
            _InitialiseRepositoryList();

            Debug.Assert(m_dicLetterTileTypes.Keys.Count == 27, "Should be 27 tile types (26 letters + blank)");
            Debug.Assert(m_listTileRepository.Count == 100, "Not 100 tiles in repository");
        }

        public LetterTile GetLetterTile(char cLetter, bool bRemoveFromRepository = true)
        {
            if (bRemoveFromRepository == false)
            {
                LetterTile oLetterTile;
                m_dicLetterTileTypes.TryGetValue(cLetter, out oLetterTile);
                return oLetterTile;
            }

            var oTile = m_listTileRepository.FirstOrDefault(x => x.Letter == cLetter);
            if (oTile == null)
            {
                return null;
            }

            m_listTileRepository.Remove(oTile);
            return oTile;
        }

        public LetterTile GetNextLetterTile()
        {
            if (m_listTileRepository.Any() == false)
            {
                return null;
            }

            var nIndex = m_oRandom.Next(0, m_listTileRepository.Count);
            var oNextTile = m_listTileRepository[nIndex];
            m_listTileRepository.RemoveAt(nIndex);
            return oNextTile;
        }

        public IEnumerable<LetterTile> GetNextLetterTiles(int nTiles)
        {
            for (int i = 0; i < nTiles; i++)
            {
                var oTile = GetNextLetterTile();
                if (oTile != null)
                {
                    yield return oTile;
                }
            }
        }

        public IEnumerable<LetterTile> RemainingTiles
        {
            get { return m_listTileRepository; }
        }

        #region Helpers

        private void _InitialiseTileTypes()
        {
            m_dicLetterTileTypes.Add('*', new LetterTile('*',0));

            m_dicLetterTileTypes.Add('e', new LetterTile('e',1));
            m_dicLetterTileTypes.Add('a', new LetterTile('a',1));
            m_dicLetterTileTypes.Add('i', new LetterTile('i',1));
            m_dicLetterTileTypes.Add('o', new LetterTile('o',1));
            m_dicLetterTileTypes.Add('n', new LetterTile('n',1));
            m_dicLetterTileTypes.Add('r', new LetterTile('r',1));
            m_dicLetterTileTypes.Add('t', new LetterTile('t',1));
            m_dicLetterTileTypes.Add('l', new LetterTile('l',1));
            m_dicLetterTileTypes.Add('s', new LetterTile('s',1));
            m_dicLetterTileTypes.Add('u', new LetterTile('u',1));

            m_dicLetterTileTypes.Add('d', new LetterTile('d',2));
            m_dicLetterTileTypes.Add('g', new LetterTile('g',2));

            m_dicLetterTileTypes.Add('b', new LetterTile('b',3));
            m_dicLetterTileTypes.Add('c', new LetterTile('c',3));
            m_dicLetterTileTypes.Add('m', new LetterTile('m',3));
            m_dicLetterTileTypes.Add('p', new LetterTile('p',3));

            m_dicLetterTileTypes.Add('f', new LetterTile('f',4));
            m_dicLetterTileTypes.Add('h', new LetterTile('h',4));
            m_dicLetterTileTypes.Add('v', new LetterTile('v',4));
            m_dicLetterTileTypes.Add('w', new LetterTile('w',4));
            m_dicLetterTileTypes.Add('y', new LetterTile('y',4));

            m_dicLetterTileTypes.Add('k', new LetterTile('k',5));

            m_dicLetterTileTypes.Add('j', new LetterTile('j',8));
            m_dicLetterTileTypes.Add('x', new LetterTile('x',8));

            m_dicLetterTileTypes.Add('q', new LetterTile('q',10));
            m_dicLetterTileTypes.Add('z', new LetterTile('z',10));

        }

        private void _InitialiseRepositoryList()
        {
            _AddNumberToRepository(2, '*');

            _AddNumberToRepository(12, 'e');
            _AddNumberToRepository(9, 'a');
            _AddNumberToRepository(9, 'i');
            _AddNumberToRepository(8, 'o');
            _AddNumberToRepository(6, 'n');
            _AddNumberToRepository(6, 'r');
            _AddNumberToRepository(6, 't');
            _AddNumberToRepository(4, 'l');
            _AddNumberToRepository(4, 's');
            _AddNumberToRepository(4, 'u');

            _AddNumberToRepository(4, 'd');
            _AddNumberToRepository(3, 'g');

            _AddNumberToRepository(2, 'b');
            _AddNumberToRepository(2, 'c');

            _AddNumberToRepository(2, 'm');
            _AddNumberToRepository(2, 'p');

            _AddNumberToRepository(2, 'f');
            _AddNumberToRepository(2, 'h');
            _AddNumberToRepository(2, 'v');
            _AddNumberToRepository(2, 'w');
            _AddNumberToRepository(2, 'y');

            _AddNumberToRepository(1, 'k');

            _AddNumberToRepository(1, 'j');
            _AddNumberToRepository(1, 'x');

            _AddNumberToRepository(1, 'q');
            _AddNumberToRepository(1, 'z');
        }

        private void _AddNumberToRepository(int nNumber, char cLetter)
        {
            for (int i = 0; i < nNumber; i++)
            {
                m_listTileRepository.Add(m_dicLetterTileTypes[cLetter].Clone());
            }
        }

        #endregion
    }

    public class LetterTile
    {
        public LetterTile(char cLetter, int nValue)
        {
            Letter = cLetter;
            Value = nValue;
        }

        public char Letter { get; private set; }
        public bool IsBlankTile { get { return Letter == '*' || Value == 0; } }

        public int Value { get; private set; }

        public LetterTile Clone()
        {
            return new LetterTile(this.Letter, this.Value);
        }
    }

    public static class LetterTileExtensions
    {
        public static string GetWord(this IEnumerable<LetterTile> enTiles)
        {
            return new string(enTiles.Select(x => x.Letter).ToArray());
        }

        public static int GetScore(this IEnumerable<LetterTile> enExistingTiles, BoardTile oBoardTile)
        {
            if (oBoardTile.HasLetter == false)
            {
                Debug.Assert(false, "Board tile does not have letter score");
                return -1;
            }

            var nExistingScore = enExistingTiles.Sum(x => x.Value);

            var nWordMultiply = 1; var nLetterScale = 1;
            if (oBoardTile.TileBonus == TileBonus.DoubleLetter) { nLetterScale = 2; }
            else if (oBoardTile.TileBonus == TileBonus.TripleLetter) { nLetterScale = 3; }
            else if (oBoardTile.TileBonus == TileBonus.DoubleWord) { nWordMultiply *= 2; }
            else if (oBoardTile.TileBonus == TileBonus.TripleWord) { nWordMultiply *= 3; }

            return (nExistingScore + oBoardTile.LetterTile.Value*nLetterScale)*nWordMultiply;
        }

        public static int GetScore(this IEnumerable<BoardTile> enTiles)
        {
            var listTiles = enTiles.ToList();
            
            if (listTiles.Any(x => x.HasLetter == false))
            {
                Debug.Assert(false, "Board tile does not have letter score");
                return -1;
            }

            var nletterScore = 0;
            var nWordMultiply = 1;
            foreach (var oLetterTile in listTiles)
            {
                var nLetterScale = 1;

                if (oLetterTile.TileBonus == TileBonus.DoubleLetter) { nLetterScale = 2; }
                else if (oLetterTile.TileBonus == TileBonus.TripleLetter){nLetterScale = 3;}
                else if(oLetterTile.TileBonus == TileBonus.DoubleWord){nWordMultiply *= 2;}
                else if(oLetterTile.TileBonus == TileBonus.TripleWord){nWordMultiply *= 3;}

                nletterScore += oLetterTile.LetterTile.Value * nLetterScale;
            }
            return nletterScore * nWordMultiply;
        }
    }
}
