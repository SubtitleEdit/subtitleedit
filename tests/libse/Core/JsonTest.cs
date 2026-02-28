using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Core;

public class JsonTest
{
    [Fact]
    public void TestUnicodeFirst()
    {
        var result = Json.DecodeJsonText("\u05d1 ");
        Assert.Equal("ב ", result);
    }

    [Fact]
    public void TestUnicodeLast()
    {
        var result = Json.DecodeJsonText(" \u05d1");
        Assert.Equal(" ב", result);
    }
}
