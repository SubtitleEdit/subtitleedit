using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

public class UtilitiesCasingProbeTest
{
    // IsAllUppercase / HasUppercase replace "s == s.ToUpperInvariant()" and
    // "s != s.ToLowerInvariant()" in the per-line hearing-impaired and casing rules, so they
    // must answer identically for every character - including the ones where the invariant
    // case mapping does something surprising (Turkish dotless i, Cherokee, Deseret, ...).
    [Fact]
    public void CasingProbes_MatchStringComparisonForEveryChar()
    {
        for (var i = 0; i <= char.MaxValue; i++)
        {
            var s = ((char)i).ToString();

            Assert.True(s == s.ToUpperInvariant() == Utilities.IsAllUppercase(s), $"IsAllUppercase U+{i:X4}");
            Assert.True(s != s.ToLowerInvariant() == Utilities.HasUppercase(s), $"HasUppercase U+{i:X4}");
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("HELLO THERE!")]
    [InlineData("Hello there!")]
    [InlineData("hello there!")]
    [InlineData("123 - ?!")]
    [InlineData("MAN:")]
    [InlineData("ÆØÅ ÖÜ")]
    [InlineData("æøå öü")]
    [InlineData("ΑΘΗΝΑ")]
    [InlineData("Ω ω")]
    [InlineData("ß STRASSE")]
    public void CasingProbes_MatchStringComparisonForLines(string s)
    {
        Assert.Equal(s == s.ToUpperInvariant(), Utilities.IsAllUppercase(s));
        Assert.Equal(s != s.ToLowerInvariant(), Utilities.HasUppercase(s));
    }
}
