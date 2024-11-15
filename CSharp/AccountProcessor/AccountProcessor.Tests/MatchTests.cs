using NUnit.Framework;
using AccountProcessor.Components.Services;
using MatchType = AccountProcessor.Components.Services.MatchType;

namespace AccountProcessor.Tests
{
    public class MatchTests
    {
        [Test]
        public void Match_exact() =>
            _AssertMatchIs(
                _CreateMatch("Test"),
                _CreateTransaction("Test"),
                MatchType.MatchExact);

        [Test]
        public void Match_partial_is_no_match() =>
            _AssertMatchIs(
                _CreateMatch("Test"),
                _CreateTransaction("Test Today"),
                MatchType.NoMatch);

        private static void _AssertMatchIs(Match match, Transaction trans, MatchType type)
        {
            Assert.That(match.Matches(trans), Is.EqualTo(type));
        }

        private static Match _CreateMatch(string pattern, string? overrideDescription = null, DateOnly? exactDate = null) =>
            new Match(pattern, overrideDescription, exactDate);

        private static Transaction _CreateTransaction(string pattern, DateOnly? overrideDate = null) =>
            new Transaction(overrideDate ?? DateOnly.FromDateTime(DateTime.Now), pattern, 0);
    }
}
