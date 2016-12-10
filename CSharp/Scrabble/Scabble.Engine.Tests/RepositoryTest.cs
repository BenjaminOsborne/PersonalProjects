using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Scrabble.Engine;

namespace Scabble.Engine.Tests
{
    public class RepositoryTest
    {
        private LetterTileRepository m_oLetterRepository;
        [SetUp]
        public void SetUp()
        {
            m_oLetterRepository = new LetterTileRepository();
        }

        [Test]
        public void When_getting_every_letter()
        {
            foreach (var oTile in m_oLetterRepository.GetNextLetterTiles(100))
            {
                Debug.WriteLine(oTile.Letter + ": " + oTile.Value);
            }
        }

        [Test]
        public void When_getting_too_many_letters()
        {
            var listTiles = m_oLetterRepository.GetNextLetterTiles(200).ToList();
            Assert.That(listTiles.Count, Is.EqualTo(100));
        }
    }
}
