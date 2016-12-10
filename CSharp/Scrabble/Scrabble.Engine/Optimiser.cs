using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scrabble.Data;

namespace Scrabble.Engine
{
    public class Optimiser
    {
        private readonly Words m_oWords;
        private readonly Board m_oBoard;

        public Optimiser(Words oWords, Board oBoard)
        {
            m_oWords = oWords;
            m_oBoard = oBoard;
        }

        public IEnumerable<PossiblePlay> GetPossiblePlaysByScore(IEnumerable<LetterTile> enAvailableLetterTiles)
        {
            var listAvailableLetterTiles = enAvailableLetterTiles.ToList();

            IEnumerable<PossiblePlay> enPossiblePlays;
            if (m_oBoard.HasLetters == false)
            {
                enPossiblePlays = _GetStartPlay(listAvailableLetterTiles);
            }
            else
            {
                var listPositions = m_oBoard.AvailableBoardPositions.ToList();
                var nIndexMax = listPositions.Count - 1;
                var dicResults = 0.Through(nIndexMax).ToDictionary(x => x, x => Enumerable.Empty<PossiblePlay>());
                Parallel.For(0, nIndexMax, x => { dicResults[x] = _GetPlaysForPosition(listPositions[x], listAvailableLetterTiles); });
                enPossiblePlays = dicResults.Values.SelectMany(x => x);
            }

            var dicUniquePlays = new Dictionary<Tuple<int, string>, List<PossiblePlay>>();
            foreach (var oPlay in enPossiblePlays)
            {
                var oTup = Tuple.Create(oPlay.Score, oPlay.Word);
                List<PossiblePlay> listPlays;
                if (dicUniquePlays.TryGetValue(oTup, out listPlays) && listPlays.IsUniquePosition(oPlay))
                {
                    listPlays.Add(oPlay);
                }
                else
                {
                    dicUniquePlays.Add(oTup, new List<PossiblePlay>() {oPlay});
                }
            }

            return dicUniquePlays.Keys.OrderByDescending(x => x.Item1).SelectMany(x => dicUniquePlays[x]); //return dicUniquePlays; //dicUniquePlays.Keys.OrderByDescending(x => x).SelectMany(oKey => dicUniquePlays[oKey]);
        }

        private IEnumerable<PossiblePlay> _GetPlaysForPosition(PossiblePosition oPosition, List<LetterTile> listAvailableLetterTiles)
        {
            var listBlanks = listAvailableLetterTiles.Where(x => x.IsBlankTile).ToList();
            var listNonBlanks = listAvailableLetterTiles.Where(x => x.IsBlankTile == false).ToList();
            var listAvailableCharacters = listNonBlanks.Select(x => x.Letter).ToList();

            var listBoardSpaces = oPosition.BoardTileSpaces.ToList();
            var listExistingBoardLetters = listBoardSpaces.Where(x => x.IsEmpty == false).ToList();
            var listFixedCharacters = listExistingBoardLetters.Select(y => new FixedCharacter(y.Offset, y.Letter.Letter)).ToList();

            var oCondition = new BoardCondition(listBoardSpaces.Count, listFixedCharacters);
            foreach (var oValidWord in m_oWords.GetValidWords(listAvailableCharacters, oCondition, listBlanks.Count))
            {
                var bValid = true;

                //Check constraints not violated
                var listAppliedConstraints = new List<Constraint>();
                foreach (var oConstraint in oPosition.ExernalConstraints.Where(x => x.Offset >= oValidWord.StartIndex && x.Offset < oValidWord.StartIndex + oValidWord.Word.Length))
                {
                    var nOffsetIndexInWord = oConstraint.Offset - oValidWord.StartIndex;
                    var cChar = oValidWord.Word[nOffsetIndexInWord];
                    if (m_oWords.WordValid(oConstraint.GetWord(cChar)) == false) { bValid = false; break; }
                    listAppliedConstraints.Add(new Constraint(nOffsetIndexInWord, oConstraint.PreceedingLetters, oConstraint.FollowingLetters));
                }

                if (bValid == false) { continue; }

                //Find used letters
                var bUsedExistingLetter = false;

                var listRemainingNonBlankTiles = listNonBlanks.ToList();
                var listRemainingBlankTiles = listBlanks.ToList();

                var listTilesUsedFromHand = new List<LetterTile>();
                var listAllUsedLetters = new List<LetterTile>();
                
                for (var i = 0; i < oValidWord.Word.Length; i++)
                {
                    var cLetter = oValidWord.Word[i];
                    var oBoardTile = oPosition.BoardTileSpaces.ElementAt(oValidWord.StartIndex + i);
                    if (oBoardTile.IsEmpty == false)
                    {
                        bUsedExistingLetter = true;
                        listAllUsedLetters.Add(oBoardTile.Letter);
                    }
                    else
                    {
                        var oLetterTile = listRemainingNonBlankTiles.FirstOrDefault(x => x.Letter == cLetter);
                        if (oLetterTile != null) 
                        {
                            listRemainingNonBlankTiles.Remove(oLetterTile);
                        }
                        else //try use blank
                        {
                            var oBlank = listRemainingBlankTiles.FirstOrDefault();
                            if (oBlank != null)
                            {
                                listRemainingBlankTiles.Remove(oBlank);
                                oLetterTile = new LetterTile(cLetter, 0);
                            }
                            else
                            {
                                Debug.Assert(false, "no remaining blanks");
                            }
                        }
                        listTilesUsedFromHand.Add(oLetterTile);
                        listAllUsedLetters.Add(oLetterTile);
                    }
                }

                var enValidCheck = listTilesUsedFromHand.Concat(listRemainingNonBlankTiles).Where(x => x.IsBlankTile == false);
                Debug.Assert(enValidCheck.OrderBy(x => x.Letter).GetWord() == listAvailableLetterTiles.Where(x => x.IsBlankTile == false).OrderBy(x => x.Letter).GetWord());

                Debug.Assert(listAllUsedLetters.OrderBy(x => x.Letter).GetWord() == new string(oValidWord.Word.OrderBy(x => x).ToArray()));

                //Check word connects to a least 1 existing tile
                if (listTilesUsedFromHand.Any() == false || (bUsedExistingLetter == false && listAppliedConstraints.Any() == false)) { continue; } //Must use a tile and Cannot have word floating not bound to the board

                Debug.Assert(listAllUsedLetters.Count == oValidWord.Word.Length);
                var oBoardPosition = (oPosition.Direction == Direction.Down) ? new BoardPosition(oPosition.StartPosition.StartWidthPosition, oPosition.StartPosition.StartHeightPosition + oValidWord.StartIndex)
                                                                             : new BoardPosition(oPosition.StartPosition.StartWidthPosition + oValidWord.StartIndex, oPosition.StartPosition.StartHeightPosition);
                
                var nBlanksUsed = oValidWord.BlankCount;
                Debug.Assert(nBlanksUsed <= listBlanks.Count);

                yield return PossiblePlay.CreatePossiblePlay(m_oBoard, oPosition.Direction, oBoardPosition, listAllUsedLetters, listTilesUsedFromHand, listAppliedConstraints, nBlanksUsed, (nBlanksUsed > 0) ? oValidWord.CharsFromBlanks.Select(x => new LetterTile(x, 0)) : null);
            }
        }

        private IEnumerable<PossiblePlay> _GetStartPlay(IEnumerable<LetterTile> enAvailableLetterTiles)
        {
            var nBlanks = enAvailableLetterTiles.Count(x => x.IsBlankTile);
            var nWidth = (int)Math.Round((m_oBoard.Width-1) / 2.0, MidpointRounding.AwayFromZero);
            var nHeight = (int)Math.Round((m_oBoard.Height-1) / 2.0, MidpointRounding.AwayFromZero);
            foreach (var oWordAndBlanks in m_oWords.GetWordsMadeBy(enAvailableLetterTiles.Where(x => x.IsBlankTile == false).Select(x => x.Letter), nBlanks))
            {
                var sWord = oWordAndBlanks.Word;
                var listAllLetters = enAvailableLetterTiles.ToList();
                var listUsedLetterTiles = sWord.Select(x =>
                {
                    var oLetter = listAllLetters.FirstOrDefault(y => y.Letter == x);
                    Debug.Assert(oLetter != null);
                    listAllLetters.Remove(oLetter);
                    return oLetter;
                }).ToList();

                Debug.Assert(oWordAndBlanks.BlankCount <= nBlanks);

                for (var nIndex = 0; nIndex < sWord.Length; nIndex ++)
                {
                    var nWidthIndex = nWidth - (sWord.Length - 1) + nIndex;
                    yield return PossiblePlay.CreatePossiblePlay(m_oBoard, Direction.Across, new BoardPosition(nWidthIndex, nHeight), listUsedLetterTiles, listUsedLetterTiles, Enumerable.Empty<Constraint>(), oWordAndBlanks.BlankCount, (oWordAndBlanks.BlankCount > 0) ? oWordAndBlanks.CharsFromBlanks.Select(x => new LetterTile(x, 0)) : null);
                }
            }
        }
    }

    public class PossiblePlay
    {
        public static PossiblePlay CreatePossiblePlay(Board oBoard, Direction eDirection, BoardPosition oStartPosition, IEnumerable<LetterTile> enLetterTiles, IEnumerable<LetterTile> enLetterTilesFromHand, IEnumerable<Constraint> enConstraints, int nBlanks, IEnumerable<LetterTile> enBlanks)
        {
            var oPlay = new PossiblePlay(eDirection, oStartPosition, enLetterTiles, enLetterTilesFromHand, enConstraints, nBlanks, enBlanks);
            oPlay.Score = oBoard.GetScore(oPlay);
            return oPlay;
        }

        private PossiblePlay(Direction eDirection, BoardPosition oStartPosition, IEnumerable<LetterTile> enAllLetterTiles, IEnumerable<LetterTile> enLetterTilesFromHand, IEnumerable<Constraint> enConstraints, int nBlanks, IEnumerable<LetterTile> enBlanks)
        {
            Direction = eDirection;
            StartPosition = oStartPosition;
            AllLetterTiles = enAllLetterTiles;
            LetterTilesFromHand = enLetterTilesFromHand;
            Constraints = enConstraints;

            Word = AllLetterTiles.GetWord();

            Blanks = nBlanks;
            BlankTiles = enBlanks;
        }
        public Direction Direction { get; private set; }
        public BoardPosition StartPosition { get; private set; }
        public IEnumerable<LetterTile> AllLetterTiles { get; private set; }
        public IEnumerable<LetterTile> LetterTilesFromHand { get; private set; }
        public IEnumerable<Constraint> Constraints { get; private set; }

        public int Blanks { get; private set; }
        public IEnumerable<LetterTile> BlankTiles { get; private set; }

        public string Word { get; private set; }

        public int Score { get; private set; }
    }

    public static class OptimiserExtensions
    {
        public static IEnumerable<int> Through(this int nstart, int nEnd)
        {
            for (var i = nstart; i <= nEnd; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<PossiblePlay> OrderByScoreDesc(this Dictionary<int, List<PossiblePlay>> dicPossiblePlays) 
        {
            return dicPossiblePlays.Keys.OrderByDescending(x => x).SelectMany(x => dicPossiblePlays[x]);
        }

        //public static IEnumerable<PossiblePlay> Unique(this IEnumerable<PossiblePlay> enPossiblePlays)
        //{
        //    var listPossiblePlays = enPossiblePlays.ToList();
        //    return listPossiblePlays.Where(listPossiblePlays.IsUnique);
        //}

        //public static bool IsUnique(this IEnumerable<PossiblePlay> enPossiblePlays, PossiblePlay oPossiblePlay)
        //{
        //    return enPossiblePlays.Any(x =>
        //                               x.Score == oPossiblePlay.Score &&
        //                               x.Word == oPossiblePlay.Word &&
        //                               x.Direction == oPossiblePlay.Direction &&
        //                               x.StartPosition == oPossiblePlay.StartPosition) == false;
        //}

        public static bool IsUniquePosition(this IEnumerable<PossiblePlay> enPossiblePlays, PossiblePlay oPossiblePlay)
        {
            return enPossiblePlays.Any(x => x.Direction == oPossiblePlay.Direction && x.StartPosition == oPossiblePlay.StartPosition) == false;
        }
    }

    public class OptimiserOld
    {
        #region Old Blank Replacement
        /*
        private Dictionary<int, List<PossiblePlay>> _GetAllPossiblePlaysWithBlank(List<LetterTile> listAvailableLetterTiles)
        {
            var oBlank = listAvailableLetterTiles.FirstOrDefault(x => x.IsBlankTile);
            Debug.Assert(oBlank != null);

            var dicResults = m_oWords.AllCharacters.ToDictionary(x => x, x => new Dictionary<int, List<PossiblePlay>>());
            Parallel.ForEach(m_oWords.AllCharacters.Select(x => new LetterTile(x, 0)), //new ParallelOptions(){MaxDegreeOfParallelism = 1},
                                                                (oNewTile) => dicResults[oNewTile.Letter] = _GetPlaysWithReplacedBlankTile(listAvailableLetterTiles, oBlank, oNewTile));

            var dicCombined = new Dictionary<int, List<PossiblePlay>>();
            foreach (var dicByChar in dicResults.Values)
            {
                foreach (var oKeyValPair in dicByChar)
                {
                    List<PossiblePlay> listPlays;
                    if (dicCombined.TryGetValue(oKeyValPair.Key, out listPlays))
                    {
                        foreach (var oPlay in oKeyValPair.Value.Where(x => listPlays.IsUnique(x)))
                        {
                            listPlays.Add(oPlay);
                        }
                    }
                    else
                    {
                        dicCombined.Add(oKeyValPair.Key, oKeyValPair.Value);
                    }
                }
            }
            return dicCombined;
        }

        private Dictionary<int, List<PossiblePlay>> _GetPlaysWithReplacedBlankTile(IEnumerable<LetterTile> enAvailableLetterTiles, LetterTile oBlank, LetterTile oNewTile)
        {
            var listBlankSubList = enAvailableLetterTiles.ToList();
            listBlankSubList.Remove(oBlank);
            listBlankSubList.Add(oNewTile);
            return _GetPossiblePlaysByScore(listBlankSubList);
        }
        */
        #endregion
    }
}
