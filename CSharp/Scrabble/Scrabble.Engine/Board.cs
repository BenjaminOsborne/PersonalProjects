using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Scrabble.Data;

namespace Scrabble.Engine
{
    public enum TileBonus
    {
        None = 0,
        DoubleLetter,
        TripleLetter,
        DoubleWord,
        TripleWord,
    }

    public static class EnumExtensions
    {
        public static int GetLetterScale(this TileBonus eTileBonus)
        {
            if (eTileBonus == TileBonus.DoubleLetter)
            {
                return 2;
            }
            else if (eTileBonus == TileBonus.TripleLetter)
            {
                return 3;
            }
            return 1;
        }
        public static int GetWordScale(this TileBonus eTileBonus)
        {
            if (eTileBonus == TileBonus.DoubleWord)
            {
                return 2;
            }
            else if (eTileBonus == TileBonus.TripleWord)
            {
                return 3;
            }
            return 1;
        }
    
    }

    public class BoardTile
    {
        public BoardTile(TileBonus eTileBonus = TileBonus.None)
        {
            TileBonus = eTileBonus;
        }

        public TileBonus TileBonus { get; private set; }

        public LetterTile LetterTile { get; set; }
        public bool HasLetter { get { return LetterTile != null; } }
    }

    public class Board
    {
        public Board()
        {
            Width = 15;
            Height = 15;
            
            m_arrTiles = new BoardTile[Width, Height];
            _PopulateDefaultTileBonusMap();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    m_arrTiles[x, y] = new BoardTile(m_arrTileBonusMap[x, y]);
                }
            }

            TilesPerHand = 7;
            AllTilesUsedBonus = 50;
        }

        public int TilesPerHand { get; private set; }
        public int AllTilesUsedBonus { get; private set; }

        private TileBonus[,] m_arrTileBonusMap; //width, height
        private void _PopulateDefaultTileBonusMap()
        {
            m_arrTileBonusMap = new TileBonus[15,15];

            m_arrTileBonusMap[0, 0] = TileBonus.TripleWord;
            m_arrTileBonusMap[0, 7] = TileBonus.TripleWord;
            m_arrTileBonusMap[0, 14] = TileBonus.TripleWord;
            m_arrTileBonusMap[7, 0] = TileBonus.TripleWord;
            m_arrTileBonusMap[7, 14] = TileBonus.TripleWord;
            m_arrTileBonusMap[14, 0] = TileBonus.TripleWord;
            m_arrTileBonusMap[14, 7] = TileBonus.TripleWord;
            m_arrTileBonusMap[14, 14] = TileBonus.TripleWord;
            
            m_arrTileBonusMap[1, 5] = TileBonus.TripleLetter;
            m_arrTileBonusMap[1, 9] = TileBonus.TripleLetter;
            m_arrTileBonusMap[5, 1] = TileBonus.TripleLetter;
            m_arrTileBonusMap[5, 5] = TileBonus.TripleLetter;
            m_arrTileBonusMap[5, 9] = TileBonus.TripleLetter;
            m_arrTileBonusMap[5, 13] = TileBonus.TripleLetter;
            m_arrTileBonusMap[9, 1] = TileBonus.TripleLetter;
            m_arrTileBonusMap[9, 5] = TileBonus.TripleLetter;
            m_arrTileBonusMap[9, 9] = TileBonus.TripleLetter;
            m_arrTileBonusMap[9, 13] = TileBonus.TripleLetter;
            m_arrTileBonusMap[13, 5] = TileBonus.TripleLetter;
            m_arrTileBonusMap[13, 9] = TileBonus.TripleLetter;
            
            m_arrTileBonusMap[1, 1] = TileBonus.DoubleWord;
            m_arrTileBonusMap[1, 13] = TileBonus.DoubleWord;
            m_arrTileBonusMap[2, 2] = TileBonus.DoubleWord;
            m_arrTileBonusMap[2, 12] = TileBonus.DoubleWord;
            m_arrTileBonusMap[3, 3] = TileBonus.DoubleWord;
            m_arrTileBonusMap[3, 11] = TileBonus.DoubleWord;
            m_arrTileBonusMap[4, 4] = TileBonus.DoubleWord;
            m_arrTileBonusMap[4, 10] = TileBonus.DoubleWord;
            m_arrTileBonusMap[6, 6] = TileBonus.DoubleWord;
            m_arrTileBonusMap[6, 8] = TileBonus.DoubleWord;
            m_arrTileBonusMap[7, 7] = TileBonus.DoubleWord;
            m_arrTileBonusMap[8, 6] = TileBonus.DoubleWord;
            m_arrTileBonusMap[8, 8] = TileBonus.DoubleWord;
            m_arrTileBonusMap[10, 4] = TileBonus.DoubleWord;
            m_arrTileBonusMap[10, 10] = TileBonus.DoubleWord;
            m_arrTileBonusMap[11, 3] = TileBonus.DoubleWord;
            m_arrTileBonusMap[11, 11] = TileBonus.DoubleWord;
            m_arrTileBonusMap[12, 2] = TileBonus.DoubleWord;
            m_arrTileBonusMap[12, 12] = TileBonus.DoubleWord;
            m_arrTileBonusMap[13, 1] = TileBonus.DoubleWord;
            m_arrTileBonusMap[13, 13] = TileBonus.DoubleWord;
            
            m_arrTileBonusMap[0, 3] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[0, 11] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[2, 6] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[2, 8] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[3, 0] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[3, 7] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[3, 14] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[6, 2] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[6, 12] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[7, 3] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[7, 11] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[8, 2] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[8, 12] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[11, 0] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[11, 7] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[11, 14] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[12, 6] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[12, 8] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[14, 3] = TileBonus.DoubleLetter;
            m_arrTileBonusMap[14, 11] = TileBonus.DoubleLetter;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        private BoardTile[,] m_arrTiles;
        public BoardTile this[int nWidth, int nHeight]
        {
            get
            {
                return _RangeValid(nWidth, nHeight) ? m_arrTiles[nWidth, nHeight] : null;
            }
            private set
            {
                if (_RangeValid(nWidth, nHeight))
                {
                    m_arrTiles[nWidth, nHeight] = value;
                }
            }
        }

        public bool HasLetters
        {
            get
            {
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        if (m_arrTiles[i, j].HasLetter)
                            return true;
                    }
                }
                return false;
            }
        }

        private bool _RangeValid(int nWidth, int nHeight)
        {
            return nWidth >= 0 && nWidth < Width && nHeight < Height && nHeight >= 0;
        }

        public IEnumerable<PossiblePosition> AvailableBoardPositions
        {
            get
            {
                var listPossiblePositions = new List<PossiblePosition>();

                //_AddPossiblePositionsSplitOnMultiple(Direction.Down, listPossiblePositions);
                //_AddPossiblePositionsSplitOnMultiple(Direction.Across, listPossiblePositions);

                _AddPossiblePositions(Direction.Down, listPossiblePositions);
                _AddPossiblePositions(Direction.Across, listPossiblePositions);
                
                return listPossiblePositions;
            }
        }

        private void _AddPossiblePositions(Direction eDirection, List<PossiblePosition> listPossiblePositions)
        {
            var nLoopMax = (eDirection == Direction.Down) ? Width : Height;
            for (var nLoop = 0; nLoop < nLoopMax; nLoop++)
            {
                if (_LineOrAdjacentLinesContainTile(nLoop, eDirection))
                {
                    var oPosition = _GetPositionFromLine(eDirection, nLoop);
                    if (oPosition != null)
                    {
                        listPossiblePositions.Add(oPosition);
                    }
                }
            }
        }

        private PossiblePosition _GetPositionFromLine(Direction eDirection, int nOuterIndex)
        {
            var nWidth = 0; var nHeight = 0;
            if (eDirection == Direction.Down) { nWidth = nOuterIndex;}
            else                              { nHeight = nOuterIndex; }

            var oStartPosition = new BoardPosition(nWidth, nHeight);
            var nLoopMax = (eDirection == Direction.Down) ? Height : Width;

            var listSpaces = new List<BoardSpace>(); var listConstraints = new List<Constraint>(); //To fill in
            for (var nCurrentLoop = 0; nCurrentLoop < nLoopMax; nCurrentLoop++)
            {
                var oCurrentTile = (eDirection == Direction.Down) ? m_arrTiles[nWidth, nCurrentLoop] : m_arrTiles[nCurrentLoop, nHeight];
                if (oCurrentTile.HasLetter == false)
                {
                    var oConstraint = _GetConstraint(nOuterIndex, nCurrentLoop, eDirection, nCurrentLoop);
                    if (oConstraint != null) { listConstraints.Add(oConstraint); }
                }

                listSpaces.Add(new BoardSpace(nCurrentLoop, oCurrentTile.LetterTile));
            }

            return listSpaces.Any(x => x.IsEmpty) ? new PossiblePosition(oStartPosition, eDirection, listSpaces, listConstraints) : null; //Handle case where all tiles in a row filled!
        }

        #region Deprecated: Used to split possible positions if going over blocks of multiple tiles

        private void _AddPossiblePositionsSplitOnMultiple(Direction eDirection, List<PossiblePosition> listPossiblePositions)
        {
            var nOuterLoopMax = (eDirection == Direction.Down) ? Width : Height;
            for (var nCurrentOuterLoop = 0; nCurrentOuterLoop < nOuterLoopMax; nCurrentOuterLoop++)
            {
                var nCurrentInnerLoop = 0;
                var nInnerLoopMax = (eDirection == Direction.Down) ? Height : Width;
                if (_LineOrAdjacentLinesContainTile(nCurrentOuterLoop, eDirection))
                {
                    while (nCurrentInnerLoop < nInnerLoopMax)
                    {
                        int nNextStartPosition;
                        if (eDirection == Direction.Down) {_AddNextPossiblePosition(nCurrentOuterLoop, nCurrentInnerLoop, eDirection, listPossiblePositions, out nNextStartPosition);}
                        else                              {_AddNextPossiblePosition(nCurrentInnerLoop, nCurrentOuterLoop, eDirection, listPossiblePositions, out nNextStartPosition);}
                        nCurrentInnerLoop = nNextStartPosition;
                    }
                }
            }
        }

        private void _AddNextPossiblePosition(int nCurrentWidth, int nCurrentHeight, Direction eDirection, List<PossiblePosition> listPossiblePositions, out int nNextStartPosition)
        {
            var oPosition = _GetNextPossiblePositionSplitOnMultiple(nCurrentWidth, nCurrentHeight, eDirection, out nNextStartPosition);
            if (oPosition != null)
            {
                listPossiblePositions.Add(oPosition);
            }

            Debug.Assert(nNextStartPosition >= ((eDirection == Direction.Down) ? nCurrentHeight : nCurrentWidth), "Next position should be greater than start.");
        }

        /// <summary>
        /// Splits positions up if goes over multiple positions
        /// </summary>
        private PossiblePosition _GetNextPossiblePositionSplitOnMultiple(int nCurrentWidth, int nCurrentHeight, Direction eDirection, out int nNextStartPosition)
        {
            var oStartPosition = new BoardPosition(nCurrentWidth, nCurrentHeight);
            
            var nLoopStart = (eDirection == Direction.Down) ? nCurrentHeight : nCurrentWidth;
            var nLoopMax = (eDirection == Direction.Down) ? Height : Width;
            nNextStartPosition = nLoopStart;

            var listSpaces = new List<BoardSpace>();    var listConstraints = new List<Constraint>(); //To fill in
            BoardTile oPreviousTile = new BoardTile();  BoardTile oCurrentTile;
            var bBlankHit = false; var bMultipleTilesHit = false;

            var nCurrentLoop = nLoopStart;
            for (; nCurrentLoop < nLoopMax; nCurrentLoop++)
            {
                var nOffset = nCurrentLoop - nLoopStart;

                oCurrentTile = (eDirection == Direction.Down) ? m_arrTiles[nCurrentWidth, nCurrentLoop] : m_arrTiles[nCurrentLoop, nCurrentHeight];

                if (oCurrentTile.HasLetter == false) { bBlankHit = true; } //Set blank hit flag

                if (oCurrentTile.HasLetter == false && bMultipleTilesHit) { break; } //Break on hitting second multiple letter group

                if (oCurrentTile.HasLetter && oPreviousTile.HasLetter == false) { nNextStartPosition = nCurrentLoop; } //Update start of letter group

                if (bBlankHit && oCurrentTile.HasLetter && oPreviousTile.HasLetter) { bMultipleTilesHit = true; }

                if (oCurrentTile.HasLetter == false)
                {
                    var nAdjacentStartIndex = (eDirection == Direction.Down) ? nCurrentWidth : nCurrentHeight;
                    var oConstraint = _GetConstraint(nAdjacentStartIndex, nCurrentLoop, eDirection, nOffset);
                    if (oConstraint != null) { listConstraints.Add(oConstraint); }
                }

                listSpaces.Add(new BoardSpace(nOffset, oCurrentTile.LetterTile));
                oPreviousTile = oCurrentTile;
            }

            if (nCurrentLoop == nLoopMax) { nNextStartPosition = nLoopMax; } //Set last letter group to edge of board if reach end

            return listSpaces.Any(x => x.IsEmpty) ? new PossiblePosition(oStartPosition, eDirection, listSpaces, listConstraints) : null; //Handle case where all tiles in a row filled!
        }
        
        #endregion

        #region Constraints

        private Constraint _GetConstraint(int nAdjacentStartIndex, int nCurrentLoop, Direction eDirection, int nOffset)
        {
            var oCurrentTile = (eDirection == Direction.Down) ? m_arrTiles[nAdjacentStartIndex, nCurrentLoop] : m_arrTiles[nCurrentLoop, nAdjacentStartIndex];
            Debug.Assert(oCurrentTile.HasLetter == false, "Current tile should not have letter.");

            var listLettersBefore = _GetAdjacentLetters(nCurrentLoop, nAdjacentStartIndex, eDirection, x => x - 1); //Back
            listLettersBefore.Reverse();
            var listLettersAfter = _GetAdjacentLetters(nCurrentLoop, nAdjacentStartIndex, eDirection, x => x + 1); //Forwards
            
            if (listLettersBefore.Any() || listLettersAfter.Any())
            {
                return new Constraint(nOffset, listLettersBefore, listLettersAfter);
            }
            return null;
        }

        private List<LetterTile> _GetAdjacentLetters(int nCurrentLoop, int nStartAdjacentIndex, Direction eDirection, Func<int, int> fnGetNextIndex)
        {
            var listAdjacentLetters = new List<LetterTile>();
            var nAdjacentMoveMax = (eDirection == Direction.Down) ? Width : Height;

            var nNextAdjacentIndex = fnGetNextIndex(nStartAdjacentIndex);
            while (nNextAdjacentIndex >= 0 && nNextAdjacentIndex < nAdjacentMoveMax)
            {
                var oAdjacentTile = (eDirection == Direction.Down) ? m_arrTiles[nNextAdjacentIndex, nCurrentLoop] : m_arrTiles[nCurrentLoop, nNextAdjacentIndex];
                if (oAdjacentTile.HasLetter)
                {
                    listAdjacentLetters.Add(oAdjacentTile.LetterTile);
                }
                else
                {
                    break;
                }
                nNextAdjacentIndex = fnGetNextIndex(nNextAdjacentIndex);
            }
            return listAdjacentLetters;
        }

        #endregion

        private bool _LineOrAdjacentLinesContainTile(int nLineIndex, Direction eDirection)
        {
            var bContains = _LineContainsTile(nLineIndex - 1, eDirection);
            bContains = bContains || _LineContainsTile(nLineIndex, eDirection);
            bContains = bContains || _LineContainsTile(nLineIndex + 1, eDirection);
            return bContains;
        }

        private bool _LineContainsTile(int nLineIndex, Direction eDirection)
        {
            var nAccrossDirectionMax = (eDirection == Direction.Down) ? Width : Height;
            var nInLineDirectionMax = (eDirection == Direction.Down) ? Height : Width;

            if (nLineIndex < 0 || nLineIndex >= nAccrossDirectionMax)
            {
                return false;
            }

            for (int i = 0; i < nInLineDirectionMax; i++)
            {
                var oTile = (eDirection == Direction.Down) ? m_arrTiles[nLineIndex, i] : m_arrTiles[i, nLineIndex];
                if (oTile.HasLetter)
                {
                    return true;
                }
            }
            
            return false;
        }

        public int GetScore(PossiblePlay oPossiblePlay)
        {
            var nWordScore = 0;
            var nConstraintAdded = 0;
            var nWordMultiply = 1;
            var listLetters = oPossiblePlay.AllLetterTiles.ToList();
            var nFreeLetterCount = 0;
            for (int nIndex = 0; nIndex < listLetters.Count; nIndex++)
            {
                var oLetter = listLetters[nIndex];

                var nWidth = oPossiblePlay.StartPosition.StartWidthPosition;
                var nHeight = oPossiblePlay.StartPosition.StartHeightPosition;
                if (oPossiblePlay.Direction == Direction.Down) { nHeight += nIndex; }
                else                                           { nWidth += nIndex; }
                var oBoardTile = this[nWidth, nHeight];

                if (oBoardTile.HasLetter)
                {
                    nWordScore += oBoardTile.LetterTile.Value;
                }
                else
                {
                    nFreeLetterCount++;
                    nWordScore += oLetter.Value*oBoardTile.TileBonus.GetLetterScale();
                    nWordMultiply *= oBoardTile.TileBonus.GetWordScale();
                }

                var oConstraint = oPossiblePlay.Constraints.FirstOrDefault(x => x.Offset == nIndex);
                if (oConstraint != null)
                {
                    Debug.Assert(oBoardTile.HasLetter == false, "should not have letter tile if in constraint logic");
                    var nExistingScore = oConstraint.FollowingLetters.Concat(oConstraint.PreceedingLetters).Sum(x => x.Value);
                    var nUnscaledScore = nExistingScore + oLetter.Value * oBoardTile.TileBonus.GetLetterScale();
                    nConstraintAdded += (nUnscaledScore * oBoardTile.TileBonus.GetWordScale());
                }
            }

            var nTotalScore = nWordScore * nWordMultiply + nConstraintAdded;
            Debug.Assert(nFreeLetterCount <= this.TilesPerHand, "should not have more free tiles than available per hand");
            if (nFreeLetterCount == this.TilesPerHand)
            {
                nTotalScore += this.AllTilesUsedBonus;
            }
            return nTotalScore;
        }

        public void DebugPrint()
        {
            _PerformActionPerTile((nWidth, nHeight, oBoardTile) =>
            {
                if (nWidth == 0){ OutputPrinter.WriteLine(""); }
                var cChar = (oBoardTile.HasLetter) ? oBoardTile.LetterTile.Letter : ' ';
                OutputPrinter.Write("|" + cChar);
                if(nWidth == Width - 1) { OutputPrinter.Write("|");}
            });
            OutputPrinter.WriteLine("");
        }

        /// <summary>
        /// Action on: Width, Height, BoardTile
        /// </summary>
        private void _PerformActionPerTile(Action<int, int, BoardTile> fnAction)
        {
            for (int nHeight = 0; nHeight < Height; nHeight++)
            {
                for (int nWidth = 0; nWidth < Width; nWidth++)
                {
                    var oBoardTile = this[nWidth, nHeight];
                    fnAction(nWidth, nHeight, oBoardTile);
                }
            }
        }
    }

    public class PossiblePosition
    {
        public PossiblePosition(BoardPosition oStartPosition, Direction eDirection, IEnumerable<BoardSpace> enBoardSpace, IEnumerable<Constraint> enConstraints)
        {
            StartPosition = oStartPosition;
            Direction = eDirection;
            BoardTileSpaces = enBoardSpace;
            ExernalConstraints = enConstraints;
        }

        public BoardPosition StartPosition { get; private set; }

        public Direction Direction { get; private set; }

        public IEnumerable<BoardSpace> BoardTileSpaces { get; private set; }
        
        public IEnumerable<Constraint> ExernalConstraints { get; private set; }
    }

    public enum Direction
    {
        Across,
        Down
    }

    public class BoardSpace
    {
        public BoardSpace(int nPositionOffset, LetterTile oTile = null)
        {
            Offset = nPositionOffset;
            Letter = oTile;
        }

        public LetterTile Letter { get; private set; }
        public bool IsEmpty { get { return Letter == null; } }

        public int Offset { get; private set; }
    }

    public class Constraint
    {
        public Constraint(int nOffsetIndex, IEnumerable<LetterTile> enPreceedingLetters, IEnumerable<LetterTile> enFollowingLetters)
        {
            Offset = nOffsetIndex;
            PreceedingLetters = enPreceedingLetters;
            FollowingLetters = enFollowingLetters;
        }

        public int Offset { get; private set; }
        public IEnumerable<LetterTile> PreceedingLetters { get; private set; }
        public IEnumerable<LetterTile> FollowingLetters { get; private set; } 

        public string GetWord(char cLetter)
        {
            return new string(PreceedingLetters.Select(x => x.Letter).Concat(new[] { cLetter }).Concat(FollowingLetters.Select(x => x.Letter)).ToArray());
        }
    }

    public struct BoardPosition
    {
        #region Equals Override
        public bool Equals(BoardPosition other)
        {
            return StartWidthPosition == other.StartWidthPosition && StartHeightPosition == other.StartHeightPosition;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BoardPosition && Equals((BoardPosition) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StartWidthPosition*397) ^ StartHeightPosition;
            }
        }

        public static bool operator ==(BoardPosition left, BoardPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoardPosition left, BoardPosition right)
        {
            return !left.Equals(right);
        }
        #endregion

        public BoardPosition(int nWidth, int nHeight) : this()
        {
            StartWidthPosition = nWidth;
            StartHeightPosition = nHeight;
        }
        public int StartWidthPosition { get; private set; }
        public int StartHeightPosition { get; private set; }

    }
}
