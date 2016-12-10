using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrabble.Data
{
    public class ValidWord
    {
        public ValidWord(int nStartIndex, string sWord, int nBlankCount, IEnumerable<char> enCharsFromBlanks)
        {
            StartIndex = nStartIndex;
            Word = sWord;

            BlankCount = nBlankCount;
            CharsFromBlanks = enCharsFromBlanks;
        }
        public int StartIndex { get; private set; }
        public string Word { get; private set; }

        public int BlankCount { get; private set; }
        public IEnumerable<char> CharsFromBlanks { get; private set; }
    }

    public class BoardCondition
    {
        public BoardCondition(int nMaxLength, IEnumerable<FixedCharacter> enCharacterPositions)
        {
            MaxLength = nMaxLength;
            FixedCharacters = enCharacterPositions;
        }

        public int MaxLength { get; private set; }

        public IEnumerable<FixedCharacter> FixedCharacters { get; private set; }
    }

    public struct FixedCharacter
    {
        public FixedCharacter(int nOffset, char cLetter) : this()
        {
            Offset = nOffset;
            Letter = cLetter;
        }
        public int Offset { get; private set; }
        public char Letter { get; private set; }
    }

    public struct WordAndBlanks
    {
        public WordAndBlanks(string sWord, int nBlankCount, IEnumerable<char> enCharsFromBlanks)
            : this()
        {
            Word = sWord;
            BlankCount = nBlankCount;
            CharsFromBlanks = enCharsFromBlanks;
        }

        public string Word { get; private set; }
        public int BlankCount { get; private set; }
        public IEnumerable<char> CharsFromBlanks { get; private set; }
    }

    public class Words
    {
        private IWordsList m_oWordsList;
        private readonly List<string> m_listAllWords; 
        public Words(IWordsList oWordsList)
        {
            m_oWordsList = oWordsList;
            m_listAllWords = oWordsList.GetAllWords().ToList() ?? new List<string>();
            
            _PopulateWordsByLengthDictionary();
        }

        public bool WordValid(string sWord)
        {
            return m_oWordsList.WordValid(sWord);
        }

        public IEnumerable<char> AllCharacters
        {
            get { return m_oWordsList.GetAllCharacters(); }
        }

        private Dictionary<int, List<string>> m_dicWordsByLength;
        public List<string> GetWordsOfLength(int nLength)
        {
            if (m_dicWordsByLength == null || m_dicWordsByLength.Any() == false)
            {
                _PopulateWordsByLengthDictionary();
            }

            List<string> listWordsOfLength;
            if (m_dicWordsByLength.TryGetValue(nLength, out listWordsOfLength) == false)
            {
                listWordsOfLength = new List<string>();
                m_dicWordsByLength.Add(nLength, listWordsOfLength);
            }
            return listWordsOfLength;
        }

        public List<string> GetWordsUpToLength(int nLength)
        {
            var listString = new List<string>();
            if (nLength < 0)
            {
                return listString;
            }

            for (int i = 0; i <= nLength; i++)
            {
                listString.AddRange(GetWordsOfLength(i));
            }

            return listString;
        }

        public IList<char> _AddNextCharFromBlank(IList<char> listChar, int nRequired, char cLetter)
        {
            if (listChar == null)
            {
                listChar = new List<char>();
            }
            for (int i = 0; i < nRequired; i++)
            {
                listChar.Add(cLetter);
            }
            return listChar;
        }

        public IEnumerable<WordAndBlanks> GetWordsMadeBy(IEnumerable<char> enLetters, int nBlanks)
        {
            var listLetters = enLetters.ToList();
            var dicLetterCounts = m_oWordsList.GetAllCharacters().Select(x => Tuple.Create(x, listLetters.Count(y => y == x))).ToDictionary(z => z.Item1, z => z.Item2);

            foreach (var oWordWithFreq in m_oWordsList.GetWordsByFrequencyUpToLength(listLetters.Count + nBlanks))
            {
                IList<char> listCharsFromBlanks = null; //gets created if required
                var nBlanksUsed = 0;
                var bValid = true;
                foreach (var x in oWordWithFreq.CharsByFrequency)
                {
                    var nRequired = oWordWithFreq.CharCount[x] - dicLetterCounts[x];
                    if (nRequired > 0)
                    {
                        nBlanksUsed += nRequired;
                        if (nBlanksUsed > nBlanks)
                        {
                            bValid = false; break;
                        }
                        listCharsFromBlanks = _AddNextCharFromBlank(listCharsFromBlanks, nRequired, x);
                    }
                }
                if (bValid)
                {
                    yield return new WordAndBlanks(oWordWithFreq.Word, nBlanksUsed, listCharsFromBlanks);
                }
            }
        }

        /// <summary>
        /// Gets all valid words for a collection of letters (not including blanks), the board condition and the number of blank tiles available
        /// </summary>
        public IEnumerable<ValidWord> GetValidWords(IEnumerable<char> enFreeLetters, BoardCondition oCondition, int nBlanks)
        {
            var listValidWords = new List<ValidWord>();
            var listFreeLetters = enFreeLetters.ToList();
            var enAllLetters = listFreeLetters.Concat(oCondition.FixedCharacters.Select(x => x.Letter));

            foreach (var oWordAndBlanks in GetWordsMadeBy(enAllLetters, nBlanks))
            {
                var sWord = oWordAndBlanks.Word;
                var listFreeLettersAndBlanks = (oWordAndBlanks.BlankCount > 0) ? listFreeLetters.Concat(oWordAndBlanks.CharsFromBlanks).ToList() : listFreeLetters;

                var listRequiredFixedCharacters = new List<char>();
                foreach (var cChar in sWord.Distinct()) //For each distinct letter, add as many extra letters as required 
                {
                    var nRequired = sWord.Count(y => y == cChar) - listFreeLettersAndBlanks.Count(y => y == cChar);
                    for (int i = 0; i < nRequired; i++) { listRequiredFixedCharacters.Add(cChar); }
                }

                var nWordLength = sWord.Length;
                for (var i = 0; i <= oCondition.MaxLength - nWordLength; i++)
                {
                    var bValid = true;
                    var listUsedFixedCharacters = new List<char>();
                    foreach (var oFixedCharacter in oCondition.FixedCharacters)
                    {
                        var nWordCharIndex = oFixedCharacter.Offset - i;
                        if (oFixedCharacter.Offset == i-1 || oFixedCharacter.Offset == nWordLength + i) //Fixed character at start or end
                        {
                            bValid = false; break;
                        }
                        if (nWordCharIndex >= 0 && nWordCharIndex <= nWordLength)
                        {
                            if (sWord[nWordCharIndex] != oFixedCharacter.Letter)
                            {
                                bValid = false; break;
                            }
                            listUsedFixedCharacters.Add(oFixedCharacter.Letter);
                        }
                    }

                    if(bValid && listRequiredFixedCharacters.Any(x => listRequiredFixedCharacters.Count(y => y == x) > listUsedFixedCharacters.Count(y => y == x)) == false)
                    {
                        listValidWords.Add(new ValidWord(i, sWord, oWordAndBlanks.BlankCount, oWordAndBlanks.CharsFromBlanks));
                    }
                }
            }

            return listValidWords;
        }

        #region Private Helpers

        private void _PopulateWordsByLengthDictionary()
        {
            m_dicWordsByLength = new Dictionary<int, List<string>>();
            foreach (var sWord in m_listAllWords)
            {
                var nLength = sWord.Length;
                List<string> listWordsOfLength;
                if (m_dicWordsByLength.TryGetValue(nLength, out listWordsOfLength) == false)
                {
                    listWordsOfLength = new List<string>();
                    m_dicWordsByLength.Add(nLength, listWordsOfLength);
                }
                listWordsOfLength.Add(sWord);
            }
        }
        
        #endregion
    }

}
