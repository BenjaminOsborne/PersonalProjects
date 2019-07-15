using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Processor
{
    public static class WordLoader
    {
        public static readonly ImmutableHashSet<char> ValidChars =
            new [] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' }
                .ToImmutableHashSet();

        public static ImmutableHashSet<string> BuildWordSet(string fileName = "ShortWords.txt")
        {
            var allWords = new HashSet<string>();
            var ignoredWords = new List<string>(); //Filters hyphens and apostraphes

            using (var reader = new StreamReader(fileName))
            {
                var next = "";
                while (next != null)
                {
                    next = reader.ReadLine();
                    if (string.IsNullOrEmpty(next) || allWords.Contains(next))
                    {
                        continue;
                    }

                    if (next.All(x => ValidChars.Contains(x)) == false)
                    {
                        ignoredWords.Add(next);
                        continue;
                    }
                    allWords.Add(next);
                }
            }
            return allWords.ToImmutableHashSet();
        }
    }
}
