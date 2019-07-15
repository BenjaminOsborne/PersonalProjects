using System;
using System.Collections.Immutable;
using System.Linq;

namespace Processor
{
    public class WordModel
    {
        private readonly ImmutableDictionary<int, ImmutableList<DictionaryWord>> _wordsByLength;

        public WordModel()
        {
            var allWords = WordLoader.BuildWordSet();
            _wordsByLength = allWords
                .GroupBy(x => x.Length)
                .ToImmutableDictionary(x => x.Key, x => x.Select(a => new DictionaryWord(a)).ToImmutableList());
        }

        public string Suggest(string wordsBlock, string lettersBlock)
        {
            var words = _ToWords(wordsBlock);
            var letters = _ToLetters(lettersBlock);
            var available = letters.GroupBy(x => x).ToImmutableDictionary(x => x.Key, x => x.Count());

            //From letters only
            var fromLetters = _Suggest(string.Empty, letters.Count, available);

            //From existing words
            var foundSet = words
                .Select(x => new { input = x, suggested = _Suggest(x, letters.Count, available) })
                .ToImmutableList();

            var all = fromLetters.Concat(foundSet.SelectMany(x => x.suggested))
                .Distinct()
                .OrderByDescending(x => x.Length)
                .ToArray();
            return string.Join("\n", all);
        }

        private ImmutableList<string> _Suggest(string word, int lettersLength, ImmutableDictionary<char, int> letters) =>
            _wordsByLength
                .Where(x => x.Key > 3 && x.Key > word.Length && x.Key <= (word.Length + lettersLength))
                .SelectMany(x => x.Value)
                .Where(x => x.CanSuggest(word, letters))
                .Select(x => x.Word)
                .ToImmutableList();

        private ImmutableList<string> _ToWords(string wordsBlock) =>
            (wordsBlock ?? string.Empty)
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Split(' ')
            .Select(x => x.Trim().ToLower())
            .Where(x => string.IsNullOrEmpty(x) == false)
            .ToImmutableList();

        private ImmutableList<char> _ToLetters(string lettersBlock) =>
            lettersBlock
                .SelectMany(x => x.ToString().ToLower())
                .Where(WordLoader.ValidChars.Contains)
                .ToImmutableList();

        private class DictionaryWord
        {
            private readonly ImmutableDictionary<char, int> _countByChar;

            public DictionaryWord(string word)
            {
                Word = word;

                _countByChar = word.GroupBy(x => x).ToImmutableDictionary(x => x.Key, x => x.Count());
            }

            public string Word { get; }

            public bool CanSuggest(string existing, ImmutableDictionary<char, int> letters)
            {
                var remain = string.IsNullOrEmpty(existing)
                    ? _countByChar 
                    : _GetRemainingChars(existing);
                if (remain == null)
                {
                    return false;
                }
                foreach (var needed in remain.Where(x => x.Value > 0))
                {
                    if (letters.TryGetValue(needed.Key, out var available))
                    {
                        if (available < needed.Value)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }

            private ImmutableDictionary<char, int> _GetRemainingChars(string existing)
            {
                var map = _countByChar;
                foreach (var c in existing)
                {
                    if (map.TryGetValue(c, out var count))
                    {
                        if (count == 0)
                        {
                            return null;
                        }
                        map = map.SetItem(c, count - 1);
                    }
                    else
                    {
                        return null;
                    }
                }
                return map;
            }
        }
    }
}
