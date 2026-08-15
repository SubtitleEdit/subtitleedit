using Nikse.SubtitleEdit.Core.Common;
using System.Globalization;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

// seconv --multiple-replace accepts the legacy SE4 XML plus the SE5 GUI's exported
// .template JSON and .csv (issue #12544). These pin that all three shapes produce identical
// replacements, including the CaseInsensitive/CaseSensitive/RegularExpression rule types.
public class MultipleReplaceLoaderTest : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "seconv_mr_" + Guid.NewGuid().ToString("N"));

    public MultipleReplaceLoaderTest() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch { /* best effort */ }
    }

    private const string Xml = """
        <?xml version="1.0" encoding="utf-8"?>
        <MultipleSearchAndReplaceGroups>
          <Group>
            <Name>Demo</Name>
            <IsActive>true</IsActive>
            <Rules>
              <Rule><Active>true</Active><FindWhat>colour</FindWhat><ReplaceWith>color</ReplaceWith><SearchType>Normal</SearchType></Rule>
              <Rule><Active>true</Active><FindWhat>\bteh\b</FindWhat><ReplaceWith>the</ReplaceWith><SearchType>RegularExpression</SearchType></Rule>
              <Rule><Active>true</Active><FindWhat>HELLO</FindWhat><ReplaceWith>Hi</ReplaceWith><SearchType>CaseSensitive</SearchType></Rule>
              <Rule><Active>false</Active><FindWhat>sky</FindWhat><ReplaceWith>SKY</ReplaceWith><SearchType>Normal</SearchType></Rule>
            </Rules>
          </Group>
        </MultipleSearchAndReplaceGroups>
        """;

    private const string Csv =
        "Category,Find,ReplaceWith,Description,Active,Type\r\n" +
        "Demo,colour,color,,true,CaseInsensitive\r\n" +
        "Demo,\"\\bteh\\b\",the,,true,RegularExpression\r\n" +
        "Demo,HELLO,Hi,,true,CaseSensitive\r\n" +
        "Demo,sky,SKY,,false,CaseInsensitive\r\n";

    private const string Json = """
        {
          "categories": [
            { "name": "Demo", "rules": [
              { "find": "colour", "replaceWith": "color", "description": "", "isActive": true, "type": "CaseInsensitive" },
              { "find": "\\bteh\\b", "replaceWith": "the", "description": "", "isActive": true, "type": "RegularExpression" },
              { "find": "HELLO", "replaceWith": "Hi", "description": "", "isActive": true, "type": "CaseSensitive" },
              { "find": "sky", "replaceWith": "SKY", "description": "", "isActive": false, "type": "CaseInsensitive" }
            ]}
          ]
        }
        """;

    private static Subtitle NewSubtitle()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("The colour of teh sky", 0, 3000));
        sub.Paragraphs.Add(new Paragraph("HELLO world", 4000, 6000));
        sub.Paragraphs.Add(new Paragraph("hello again", 7000, 9000)); // lower-case: CaseSensitive HELLO must NOT match
        return sub;
    }

    private int Apply(string content, string ext)
    {
        var path = Path.Combine(_dir, "rules" + ext);
        File.WriteAllText(path, content);
        return MultipleReplaceLoader.Apply(_sub, path);
    }

    private Subtitle _sub = NewSubtitle();

    [Theory]
    [InlineData(Xml, ".xml")]
    [InlineData(Csv, ".csv")]
    [InlineData(Json, ".template")]
    public void AllFormatsProduceSameResult(string content, string ext)
    {
        _sub = NewSubtitle();
        var modified = Apply(content, ext);

        Assert.Equal(2, modified); // lines 1 and 2 change; line 3 unchanged
        Assert.Equal("The color of the sky", _sub.Paragraphs[0].Text); // colour->color, teh->the; inactive "sky" rule ignored
        Assert.Equal("Hi world", _sub.Paragraphs[1].Text);             // case-sensitive HELLO->Hi
        Assert.Equal("hello again", _sub.Paragraphs[2].Text);          // lower-case hello left alone
    }

    [Fact]
    public void JsonAndCsvDetectedByContentWithoutExtensionHint()
    {
        _sub = NewSubtitle();
        Assert.Equal(2, Apply(Json, ".txt")); // sniffed as JSON by leading '{'
        _sub = NewSubtitle();
        Assert.Equal(2, Apply(Xml, ".txt"));  // sniffed as XML by leading '<'
    }

    // Replacement text of a non-regex rule is literal: '$1', '$&' and '$$' are not expanded.
    // The escaping that guarantees this is done once per rule rather than per paragraph, so pin
    // it across several paragraphs — a stale or shared escape would show up on the later ones.
    private const string DollarXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <MultipleSearchAndReplaceGroups>
          <Group>
            <Name>Dollars</Name>
            <IsActive>true</IsActive>
            <Rules>
              <Rule><Active>true</Active><FindWhat>PRICE</FindWhat><ReplaceWith>$1 and $&amp; and $$</ReplaceWith><SearchType>Normal</SearchType></Rule>
              <Rule><Active>true</Active><FindWhat>COST</FindWhat><ReplaceWith>$9</ReplaceWith><SearchType>CaseSensitive</SearchType></Rule>
            </Rules>
          </Group>
        </MultipleSearchAndReplaceGroups>
        """;

    [Fact]
    public void ReplacementDollarsAreLiteralOnEveryParagraph()
    {
        _sub = new Subtitle();
        _sub.Paragraphs.Add(new Paragraph("a PRICE here", 0, 3000));
        _sub.Paragraphs.Add(new Paragraph("another price there", 4000, 6000)); // Normal rule is case-insensitive
        _sub.Paragraphs.Add(new Paragraph("a COST too", 7000, 9000));

        Assert.Equal(3, Apply(DollarXml, ".xml"));
        Assert.Equal("a $1 and $& and $$ here", _sub.Paragraphs[0].Text);
        Assert.Equal("another $1 and $& and $$ there", _sub.Paragraphs[1].Text);
        Assert.Equal("a $9 too", _sub.Paragraphs[2].Text);
    }

    // A rule whose pattern will not compile is dropped, and the remaining rules still apply.
    private const string BadRegexXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <MultipleSearchAndReplaceGroups>
          <Group>
            <Name>Bad</Name>
            <IsActive>true</IsActive>
            <Rules>
              <Rule><Active>true</Active><FindWhat>[unclosed</FindWhat><ReplaceWith>X</ReplaceWith><SearchType>RegularExpression</SearchType></Rule>
              <Rule><Active>true</Active><FindWhat>colour</FindWhat><ReplaceWith>color</ReplaceWith><SearchType>Normal</SearchType></Rule>
            </Rules>
          </Group>
        </MultipleSearchAndReplaceGroups>
        """;

    [Fact]
    public void UncompilableRuleIsSkippedAndOthersStillApply()
    {
        _sub = new Subtitle();
        _sub.Paragraphs.Add(new Paragraph("the colour [unclosed", 0, 3000));

        Assert.Equal(1, Apply(BadRegexXml, ".xml"));
        Assert.Equal("the color [unclosed", _sub.Paragraphs[0].Text);
    }

    private const string IstanbulXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <MultipleSearchAndReplaceGroups>
          <Group>
            <Name>Turkish</Name>
            <IsActive>true</IsActive>
            <Rules>
              <Rule><Active>true</Active><FindWhat>istanbul</FindWhat><ReplaceWith>Constantinople</ReplaceWith><SearchType>Normal</SearchType></Rule>
            </Rules>
          </Group>
        </MultipleSearchAndReplaceGroups>
        """;

    /// <summary>
    /// A case-insensitive rule must match the same text on every machine. Matching through a
    /// culture-sensitive RegexOptions.IgnoreCase made this depend on CurrentCulture: under
    /// tr-TR, "I" lower-cases to "ı", so the plain-ASCII rule "istanbul" stopped matching the
    /// plain-ASCII text "ISTANBUL" — the same rules file quietly produced different output for
    /// a Turkish user. Ordinal matching (what the GUI uses) is locale-independent.
    /// </summary>
    [Theory]
    [InlineData("en-US")]
    [InlineData("tr-TR")]
    [InlineData("de-DE")]
    [InlineData("")] // invariant
    public void CaseInsensitiveMatchingIsLocaleIndependent(string cultureName)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = cultureName.Length == 0
                ? CultureInfo.InvariantCulture
                : new CultureInfo(cultureName);

            _sub = new Subtitle();
            _sub.Paragraphs.Add(new Paragraph("ISTANBUL", 0, 3000));
            _sub.Paragraphs.Add(new Paragraph("Istanbul", 4000, 6000));
            _sub.Paragraphs.Add(new Paragraph("istanbul", 7000, 9000));

            Assert.Equal(3, Apply(IstanbulXml, ".xml"));
            Assert.Equal("Constantinople", _sub.Paragraphs[0].Text);
            Assert.Equal("Constantinople", _sub.Paragraphs[1].Text);
            Assert.Equal("Constantinople", _sub.Paragraphs[2].Text);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    /// <summary>
    /// Ordinal matching compares code points, so a dotted capital I (U+0130) is not an "i" —
    /// matching the GUI, which walks IndexOf(..., OrdinalIgnoreCase).
    /// </summary>
    [Fact]
    public void CaseInsensitiveMatchingIsOrdinalNotLinguistic()
    {
        _sub = new Subtitle();
        _sub.Paragraphs.Add(new Paragraph("İstanbul", 0, 3000));

        Assert.Equal(0, Apply(IstanbulXml, ".xml"));
        Assert.Equal("İstanbul", _sub.Paragraphs[0].Text);
    }

    // "^(a|aa)+$" against a run of 'a's that cannot match backtracks catastrophically.
    private const string CatastrophicXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <MultipleSearchAndReplaceGroups>
          <Group>
            <Name>Slow</Name>
            <IsActive>true</IsActive>
            <Rules>
              <Rule><Active>true</Active><FindWhat>^(a|aa)+$</FindWhat><ReplaceWith>X</ReplaceWith><SearchType>RegularExpression</SearchType></Rule>
              <Rule><Active>true</Active><FindWhat>colour</FindWhat><ReplaceWith>color</ReplaceWith><SearchType>Normal</SearchType></Rule>
            </Rules>
          </Group>
        </MultipleSearchAndReplaceGroups>
        """;

    /// <summary>
    /// A user pattern that backtracks catastrophically must not hang the conversion: the regex
    /// carries the UI's match timeout, and a rule that trips it is retired for the rest of the
    /// file rather than costing the timeout again on every remaining paragraph. Other rules keep
    /// working. This test necessarily waits out one timeout, so it takes about five seconds.
    /// </summary>
    [Fact(Timeout = 60_000)]
    public void CatastrophicPatternTimesOutAndIsRetiredWithoutStoppingOtherRules()
    {
        var slowText = new string('a', 46) + "!";
        _sub = new Subtitle();
        for (var i = 0; i < 4; i++)
        {
            _sub.Paragraphs.Add(new Paragraph(slowText, i * 4000, i * 4000 + 3000));
        }
        _sub.Paragraphs.Add(new Paragraph("the colour", 40000, 43000));

        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        var modified = Apply(CatastrophicXml, ".xml");
        elapsed.Stop();

        // Only the last paragraph changes; the slow rule never gets to match anything.
        Assert.Equal(1, modified);
        Assert.Equal("the color", _sub.Paragraphs[^1].Text);
        Assert.Equal(slowText, _sub.Paragraphs[0].Text);

        // Retirement means one timeout for the file, not one per paragraph.
        Assert.True(
            elapsed.Elapsed < RegexUtils.UserPatternMatchTimeout * 2,
            $"expected roughly one timeout, took {elapsed.Elapsed.TotalSeconds:0.0}s");
    }
}
