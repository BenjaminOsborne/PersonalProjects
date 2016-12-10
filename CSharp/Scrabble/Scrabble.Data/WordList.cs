using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;

namespace Scrabble.Data
{
    public interface IWordsList
    {
        IEnumerable<string> GetAllWords();
        Dictionary<char, double> GetLettersByFrequency();
        IEnumerable<WordsByFrequency> GetAllWordsByFrequency();
        IEnumerable<WordsByFrequency> GetWordsByFrequencyUpToLength(int nLength);
        IEnumerable<char> GetAllCharacters();

        bool WordValid(string sWord);
    }

    public class WordsByFrequency
    {
        public WordsByFrequency(string sWord, Dictionary<char, double> dicCharsWithFrequency)
        {
            Word = sWord;
            CharsByFrequency = sWord.OrderBy(y => dicCharsWithFrequency[y]).ToList();
            CharCount = sWord.Distinct().ToDictionary(x => x, x => sWord.Count(y => y == x));
        }

        public string Word { get; private set; }
        public IEnumerable<char> CharsByFrequency { get; private set; }
        public Dictionary<char, int> CharCount { get; private set; } 
    }

    public class WordList : IWordsList
    {
        private static List<char> m_listChars = new List<char>() { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        
        private Dictionary<char, double> m_dicCharacterFrequency;
        
        private Dictionary<string, bool> m_dicWords = null;
        private readonly IEnumerable<string> m_enAllWords;

        private Dictionary<int, List<WordsByFrequency>> m_dicWordsByFrequencyForLength;

        public WordList(string sFileName = "ShortWords.txt")
        {
            var oSW = Stopwatch.StartNew();
            _PopulateWordList(sFileName);
            _PopulateCharacterFrequency();
            _PopulateWordsByFrequencyForLength();

            m_enAllWords = m_dicWords.Keys.ToList();

            oSW.Stop();
            Debug.WriteLine("Total data construction time (ms): " + oSW.ElapsedMilliseconds);
        }

        public IEnumerable<char> GetAllCharacters()
        {
            return m_listChars;
        }
        
        public IEnumerable<string> GetAllWords()
        {
            return m_enAllWords;
        }

        public bool WordValid(string sWord)
        {
            return m_dicWords.ContainsKey(sWord);
        }

        public Dictionary<char, double> GetLettersByFrequency()
        {
            return m_dicCharacterFrequency;
        }

        public IEnumerable<WordsByFrequency> GetWordsByFrequencyUpToLength(int nLength)
        {
            for (var i = 0; i <= nLength; i++)
            {
                List<WordsByFrequency> listWords;
                if (m_dicWordsByFrequencyForLength.TryGetValue(i, out listWords))
                {
                    foreach (var oWordByFreq in listWords)
                    {
                        yield return oWordByFreq;
                    }
                }
            }
        }
        
        public IEnumerable<WordsByFrequency> GetAllWordsByFrequency()
        {
            return m_dicWordsByFrequencyForLength.SelectMany(x => x.Value);
        }

        private Dictionary<char, long> _GetBlankCharCountDictionary()
        {
            var dicCharCount = new Dictionary<char, long>();

            foreach (var cChar in m_listChars)
            {
                dicCharCount.Add(cChar, 0);
            }
            return dicCharCount;
        }

        private void _PopulateWordsByFrequencyForLength()
        {
            m_dicWordsByFrequencyForLength = new Dictionary<int, List<WordsByFrequency>>();
            foreach (var sWord in m_dicWords.Select(x => x.Key))
            {
                var oWordByFreq = new WordsByFrequency(sWord, GetLettersByFrequency());
                List<WordsByFrequency> listWordsByFreq;
                if (m_dicWordsByFrequencyForLength.TryGetValue(sWord.Length, out listWordsByFreq))
                {
                    listWordsByFreq.Add(oWordByFreq);
                }
                else
                {
                    m_dicWordsByFrequencyForLength.Add(sWord.Length, new List<WordsByFrequency>() { oWordByFreq });
                }
            }
        }

        private void _PopulateCharacterFrequency()
        {
            var dicCount = _GetBlankCharCountDictionary();
            foreach (var sWord in m_dicWords.Select(x => x.Key.ToLower()))
            {
                foreach (var cCharacter in sWord)
                {
                    dicCount[cCharacter] += 1;
                }
            }
            var nMax = dicCount.Select(x => x.Value).Max();
            m_dicCharacterFrequency = dicCount.ToDictionary(x => x.Key, x => (double) x.Value/nMax);
        }

        private void _PopulateWordList(string sFileName)
        {
            var oSW = new Stopwatch();
            oSW.Start();

            m_dicWords = new Dictionary<string, bool>();
            var listIgnoredWords = new List<string>();

            var oReader = new StreamReader(sFileName);
            var sLine = "";
            while (sLine != null)
            {
                sLine = oReader.ReadLine();
                if (string.IsNullOrEmpty(sLine) == false && m_dicWords.ContainsKey(sLine) == false)
                {
                    if (sLine.All(x => m_listChars.Contains(x)) == false)
                    {
                        listIgnoredWords.Add(sLine);
                        continue;
                    }

                    if (m_dicWords.ContainsKey(sLine) == false)
                    {
                        m_dicWords.Add(sLine, true);
                    }
                }
            }
            oReader.Close();

            oSW.Stop();
            Debug.WriteLine("File read (ms): " + oSW.ElapsedMilliseconds);
        }
    }
}
