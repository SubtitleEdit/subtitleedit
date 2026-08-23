using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

public class UtilitiesCountTagInTextTest
{
    // The char overload has two implementations: MemoryExtensions.Count on net8+ and an
    // IndexOf loop on netstandard2.1. Both must agree on these, in particular on a hit that
    // lands on the very last index - the loop returns early from inside its body there.
    [Theory]
    [InlineData("", '"', 0)]
    [InlineData("no quotes here", '"', 0)]
    [InlineData("\"", '"', 1)]
    [InlineData("say \"this\"", '"', 2)]
    [InlineData("\"\"\"", '"', 3)]
    [InlineData("- No.\r\n- Then stay.", '-', 2)]
    [InlineData("{\\an8}{\\pos(10,20)}Hi", '{', 2)]
    [InlineData("aaa", 'a', 3)]
    public void CountsEveryOccurrenceOfChar(string text, char tag, int expected)
    {
        Assert.Equal(expected, Utilities.CountTagInText(text, tag));
    }

    // The char and string overloads are separate implementations; for a single-character tag
    // they must not drift apart.
    [Theory]
    [InlineData("\"Are you coming?\" she asked.", '"')]
    [InlineData("- Yes.\r\n- No.", '-')]
    [InlineData("nothing to find", 'z')]
    [InlineData("trailing hit-", '-')]
    public void CharOverloadAgreesWithStringOverload(string text, char tag)
    {
        Assert.Equal(Utilities.CountTagInText(text, tag.ToString()), Utilities.CountTagInText(text, tag));
    }
}
