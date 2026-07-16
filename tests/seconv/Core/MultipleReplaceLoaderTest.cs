using Nikse.SubtitleEdit.Core.Common;
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
}
