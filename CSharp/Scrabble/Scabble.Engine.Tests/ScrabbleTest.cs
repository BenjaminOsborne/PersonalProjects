using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Scrabble.Data;
using System.Diagnostics;

namespace Scabble.Engine.Tests
{
    public class ScrabbleTest
    {
        private Words m_oWords;

        [SetUp]
        public void SetUp()
        {
            m_oWords = new Words(new WordList());
        }

        [Test]
        public void ToBeTested()
        {
            var listWords = m_oWords.GetWordsMadeBy("testing", 0);
        }

        [Test]
        public void ListTest() //ToList takes a element by element copy
        {
            var listInts = new List<int>() {1, 2, 3, 4, 5, 6};

            listInts.ForEach(x => Debug.WriteLine(x));
            Debug.WriteLine("___");

            var listNewInts = listInts;
            listNewInts.Remove(3);

            
            Debug.WriteLine("After reference copy...");
            listInts.ForEach(x => Debug.WriteLine(x));
            Debug.WriteLine("___");
            listNewInts.ForEach(x => Debug.WriteLine(x));
            Debug.WriteLine("___");

            var listToListCopy = listInts.ToList();
            listToListCopy.Remove(2);

            Debug.WriteLine("After to list copy...");
            listInts.ForEach(x => Debug.WriteLine(x));
            Debug.WriteLine("___");
            listToListCopy.ForEach(x => Debug.WriteLine(x));
        }
    }
}
