using System.Collections;
using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

public class StrippableTextFixCasingTest
{
    private static string FixNames(string text, IEnumerable<string> names, bool changeNameCases = true)
    {
        var st = new StrippableText(text);
        st.FixCasing(names, changeNameCases, false, false, string.Empty);
        return st.MergedString;
    }

    [Fact]
    public void FixCasing_FixesLowercaseName()
    {
        Assert.Equal("hello Bob", FixNames("hello bob", new List<string> { "Bob" }));
    }

    [Fact]
    public void FixCasing_FixesNameAtStartOfText()
    {
        Assert.Equal("Bob went home", FixNames("bob went home", new List<string> { "Bob" }));
    }

    [Fact]
    public void FixCasing_FixesNameFollowedByPunctuation()
    {
        Assert.Equal("Hi, Bob!", FixNames("Hi, bob!", new List<string> { "Bob" }));
    }

    [Fact]
    public void FixCasing_FixesUppercaseNameToProperCase()
    {
        Assert.Equal("hello Bob", FixNames("hello BOB", new List<string> { "Bob" }));
    }

    [Fact]
    public void FixCasing_LeavesOriginalCasingWhenChangeNameCasesIsFalse()
    {
        Assert.Equal("hello BOB", FixNames("hello BOB", new List<string> { "Bob" }, changeNameCases: false));
    }

    [Fact]
    public void FixCasing_FixesMultipleNames()
    {
        Assert.Equal("hello Bob and Alice", FixNames("hello bob and alice", new List<string> { "Bob", "Alice" }));
    }

    [Fact]
    public void FixCasing_DoesNotReplaceNameEmbeddedInsideAnotherWord()
    {
        // "bob" inside "bobsled" is not a whole word, so it must be left untouched.
        Assert.Equal("the bobsled race", FixNames("the bobsled race", new List<string> { "Bob" }));
    }

    [Fact]
    public void FixCasing_DoesNotTreatDontAsTheNameDon()
    {
        // English contraction "don't" must not be capitalised to "Don't" for the name "Don".
        Assert.Equal("I don't know", FixNames("I don't know", new List<string> { "Don" }));
    }

    [Fact]
    public void FixCasing_FixesDonWhenItReallyIsTheName()
    {
        Assert.Equal("Hi Don", FixNames("Hi don", new List<string> { "Don" }));
    }

    [Fact]
    public void FixCasing_PreservesLeadingAssaTag()
    {
        // The {\an8} block is stripped into Pre and must survive verbatim.
        Assert.Equal("{\\an8}Bob", FixNames("{\\an8}bob", new List<string> { "Bob" }));
    }

    [Fact]
    public void FixCasing_PreservesInlineAssaTag()
    {
        // Inline ASSA override tags are protected while the name after them is fixed.
        Assert.Equal("hello {\\i1} Bob", FixNames("hello {\\i1} bob", new List<string> { "Bob" }));
    }

    [Fact]
    public void FixCasing_NoNamesLeavesTextUnchanged()
    {
        Assert.Equal("nothing to do here", FixNames("nothing to do here", new List<string>()));
    }

    [Theory]
    [InlineData("bob", "Bob")]
    [InlineData("hello bob", "hello Bob")]
    [InlineData("\"bob\"", "\"Bob\"")]
    [InlineData("(bob)", "(Bob)")]
    [InlineData("bob, come here", "Bob, come here")]
    public void FixCasing_HandlesWordBoundaries(string input, string expected)
    {
        Assert.Equal(expected, FixNames(input, new List<string> { "Bob" }));
    }

    [Fact]
    public void FixCasing_CapitalizesAfterSentenceBreakWhenRequested()
    {
        var st = new StrippableText("hello. world");
        st.FixCasing(new List<string>(), true, makeUppercaseAfterBreak: true, false, string.Empty);
        Assert.Equal("hello. World", st.MergedString);
    }

    [Fact]
    public void FixCasing_CapitalizesFirstLetterWhenPreviousLineWasClosed()
    {
        var st = new StrippableText("hello");
        st.FixCasing(new List<string>(), true, false, checkLastLine: true, "Previous sentence.");
        Assert.Equal("Hello", st.MergedString);
    }

    [Fact]
    public void FixCasing_DoesNotCapitalizeFirstLetterWhenPreviousLineWasOpen()
    {
        var st = new StrippableText("hello");
        st.FixCasing(new List<string>(), true, false, checkLastLine: true, "an unfinished line");
        Assert.Equal("hello", st.MergedString);
    }

    // --- IEnumerable<string> signature change ---------------------------------

    public static IEnumerable<object[]> EnumerableVariants()
    {
        yield return new object[] { new List<string> { "Bob", "Alice" } };
        yield return new object[] { new[] { "Bob", "Alice" } };
        yield return new object[] { new HashSet<string> { "Bob", "Alice" } };
        yield return new object[] { new[] { "Bob", "Alice" }.Where(n => n.Length > 0) };
        yield return new object[] { YieldNames("Bob", "Alice") };
    }

    [Theory]
    [MemberData(nameof(EnumerableVariants))]
    public void FixCasing_ProducesSameResultForAnyEnumerableType(IEnumerable<string> names)
    {
        Assert.Equal("hello Bob and Alice", FixNames("hello bob and alice", names));
    }

    [Fact]
    public void FixCasing_EnumeratesNameListExactlyOnce()
    {
        var counting = new CountingEnumerable(new[] { "Bob" });
        var st = new StrippableText("hello bob");
        st.FixCasing(counting, true, false, false, string.Empty);

        Assert.Equal("hello Bob", st.MergedString);
        Assert.Equal(1, counting.EnumerationCount);
    }

    [Fact]
    public void FixCasing_WorksWithSingleUseEnumerable()
    {
        // The refactor iterates the name list once; a source that can only be
        // enumerated a single time must therefore not throw.
        var singleUse = new SingleUseEnumerable(new[] { "Bob" });
        var st = new StrippableText("hello bob");
        st.FixCasing(singleUse, true, false, false, string.Empty);
        Assert.Equal("hello Bob", st.MergedString);
    }

    private static IEnumerable<string> YieldNames(params string[] names)
    {
        foreach (var name in names)
        {
            yield return name;
        }
    }

    private sealed class CountingEnumerable : IEnumerable<string>
    {
        private readonly IEnumerable<string> _inner;
        public int EnumerationCount { get; private set; }

        public CountingEnumerable(IEnumerable<string> inner) => _inner = inner;

        public IEnumerator<string> GetEnumerator()
        {
            EnumerationCount++;
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class SingleUseEnumerable : IEnumerable<string>
    {
        private readonly IEnumerable<string> _inner;
        private bool _enumerated;

        public SingleUseEnumerable(IEnumerable<string> inner) => _inner = inner;

        public IEnumerator<string> GetEnumerator()
        {
            if (_enumerated)
            {
                throw new InvalidOperationException("This enumerable can only be enumerated once.");
            }

            _enumerated = true;
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
