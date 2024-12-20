using NUnit.Framework;
using AccountProcessor.Components.Services;
using MatchType = AccountProcessor.Components.Services.MatchType;

namespace AccountProcessor.Tests
{
    public class MatchTests
    {
        //Simple exact
        [TestCase("Test", "Test", true)]
        [TestCase(" Test Yeah", " Test Yeah", true)]
        //Wilcard at end
        [TestCase("Test Today", "Test", false)]
        [TestCase("Test Today", "Test*", true)]
        //Wilcard at start
        [TestCase("Today Test", "Test", false)]
        [TestCase("Today Test", "*Test", true)]
        //Wilcard either end
        [TestCase("Today Test Tomorrow", "Test", false)]
        [TestCase("Today Test Tomorrow", "Test*", false)]
        [TestCase("Today Test Tomorrow", "*Test", false)]
        [TestCase("Today Test Tomorrow", "*Test*", true)]
        //Wildcard in middle
        [TestCase("Test", "Te*s", false)]
        [TestCase("Test", "Te*t", true)]
        [TestCase("Test", "T**t", true)]
        [TestCase("Teeeesssst", "T*t", true)]
        //Many wildcards
        [TestCase("This is a test", "This *s a* est", false)]
        [TestCase("This is a test", "This *s a *es*t", true)]
        [TestCase("This is a test", "*is i* a tes*", true)]
        //Whitespace Tests
        [TestCase("Test ", "Test", false)]
        [TestCase("Test ", "Test*", true)]
        [TestCase("Test ", "Test *", true)]
        //Special character Tests
        [TestCase("Test (Some", "Test *", true)]
        [TestCase("Test (Some", "Test (Some", true)]
        
        //* Literal tests with "*" in transaction as well! [To literally match a "*" escape with "\*"]
        [TestCase("Test*", "Test*", true)]
        [TestCase("Test*", "Test\\*", true)]
        [TestCase("Test*", "Test\\*a", false)] //Is now literal, so should not match
        [TestCase("Test**", "Test\\*\\*", true)]
        [TestCase("Test*Some", "Test\\**", true)]
        
        [TestCase("Test*Multiple*s", "Test*", true)]
        [TestCase("Test*Multiple*s", "Test\\*Multiple\\*s", true)]
        [TestCase("Test*Multiple*s", "Test\\*Mul*e\\*s", true)]
        [TestCase("AMAZON* PN3VT6DC5", "Amazon\\**", true)] //real example where want to 
        public void Match(string transaction, string pattern, bool isMatch) =>
            _PatternAssert(pattern, transaction, isMatch ? MatchType.MatchExact : MatchType.NoMatch);

        [TestCase(0, MatchType.MatchExact)]
        [TestCase(-1, MatchType.MatchPreviousTransaction)]
        [TestCase(1, MatchType.MatchPreviousTransaction)]
        public void Previous_date(int offsetDays, MatchType match) =>
            _AssertMatchIs(
                _CreateMatch("Test", exactDate: _Now().AddDays(offsetDays)),
                _CreateTransaction("Test"),
                match);

        [Test, Explicit]
        public void RegexPlay()
        {
            var regex = new System.Text.RegularExpressions.Regex("^.*Test.*$");
            Console.WriteLine(regex.IsMatch("Test"));
            Console.WriteLine(regex.IsMatch("Test Yeah"));
            Console.WriteLine(regex.IsMatch("Some Test"));
            Console.WriteLine(regex.IsMatch("Some Test Yeah"));
        }

        private void _PatternAssert(string pattern, string transaction, MatchType type) =>
            _AssertMatchIs(
                _CreateMatch(pattern),
                _CreateTransaction(transaction),
                type);

        private static void _AssertMatchIs(Match match, Transaction trans, MatchType type) =>
            Assert.That(match.Matches(trans), Is.EqualTo(type));

        private static Match _CreateMatch(string pattern, string? overrideDescription = null, DateOnly? exactDate = null) =>
            new Match(pattern, overrideDescription, exactDate);

        private static Transaction _CreateTransaction(string pattern, DateOnly? overrideDate = null) =>
            new Transaction(overrideDate ?? _Now(), pattern, 0);

        private static DateOnly _Now() => DateOnly.FromDateTime(DateTime.Now);
    }
}
