using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Scrabble.Data;

namespace Scabble.Engine.Tests
{
    public class DataLoadTest
    {
        private WordList m_oWordList;
        private Words m_oWords;

        [SetUp]
        public void SetUp()
        {
            m_oWordList = new WordList();
            m_oWords = new Words(m_oWordList);
        }

        [Test]
        public void Word_count()
        {
            for (int i = 2; i < 10; i++)
            {
                var listWords = m_oWords.GetWordsOfLength(i);
                Assert.That(listWords.Count > 10, Is.True);
            }
        }

        [Test]
        public void Word_frequency()
        {
            var listFrequency = m_oWordList.GetLettersByFrequency().ToList();
            foreach (var oTup in listFrequency.OrderByDescending(x => x.Value))
            {
                Debug.WriteLine(oTup.Key + " " + 1.0 / oTup.Value);
            }
        }

        [Test]
        public void All_Words_by_frequency()
        {
            var listWordsBFrequency = m_oWordList.GetAllWordsByFrequency().ToList();
            foreach (var oTup in listWordsBFrequency)
            {
                //Debug.WriteLine(oTup.Word + " " + oTup.TrimmedWordByFrequency);
            }
        }

        [Test]
        public void When_2_valid_words_possible()
        {
            var listAvailableWords = new List<char>() {'b', 'x'};
            var listCharacterPositions = new List<FixedCharacter>() {new FixedCharacter(1, 'o')};
            var oCondition = new BoardCondition(4, listCharacterPositions);
            var listPossibleWords = m_oWords.GetValidWords(listAvailableWords, oCondition, 0).ToList();
            
            Assert.That(listPossibleWords.Count, Is.EqualTo(2));
            Assert.That(listPossibleWords.Count(x => x.Word == "box"), Is.EqualTo(1));
            Assert.That(listPossibleWords.Count(x => x.Word == "ox"), Is.EqualTo(1));
        }

        [Test]
        public void When_2_valid_words_possible_with_2_fixed()
        {
            var listAvailableWords = new List<char>() { 'a', 'u', 'a' };
            var listCharacterPositions = new List<FixedCharacter>() { new FixedCharacter(1, 'q'), new FixedCharacter(3, 'a') };
            var oCondition = new BoardCondition(4, listCharacterPositions);
            var listPossibleWords = m_oWords.GetValidWords(listAvailableWords, oCondition, 0).ToList();

            Assert.That(listPossibleWords.Count, Is.EqualTo(2));
            Assert.That(listPossibleWords.Count(x => x.Word == "aqua"), Is.EqualTo(1));
            Assert.That(listPossibleWords.Count(x => x.Word == "qua"), Is.EqualTo(1));
        }

        [Test]
        public void When_valid_words_possible_with_fixed_either_end()
        {
            var listAvailableWords = new List<char>() { 'o', 'o', 'a', 'b' };
            var listCharacterPositions = new List<FixedCharacter>() { new FixedCharacter(0, 'z'), new FixedCharacter(1, 'o'), new FixedCharacter(3, 'm'), new FixedCharacter(4, 's') };
            var oCondition = new BoardCondition(5, listCharacterPositions);
            var listPossibleWords = m_oWords.GetValidWords(listAvailableWords, oCondition, 0).ToList();

            Assert.That(listPossibleWords.Count, Is.EqualTo(1));
            Assert.That(listPossibleWords.Count(x => x.Word == "zooms"), Is.EqualTo(1));
        }

        [Test]
        public void When_no_words_possible_from_characters()
        {
            var listAvailableWords = new List<char>() { 'b', 'x' };
            var listCharacterPositions = new List<FixedCharacter>() { new FixedCharacter(1, 'q') };
            var oCondition = new BoardCondition(4, listCharacterPositions);
            var listPossibleWords = m_oWords.GetValidWords(listAvailableWords, oCondition, 0).ToList();

            Assert.That(listPossibleWords.Count, Is.EqualTo(0));
        }
    }
}
