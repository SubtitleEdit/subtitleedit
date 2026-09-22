using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using System.Linq;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// The styles dialog writes its styles back with GetHeaderAndStylesFromAdvancedSubStationAlpha.
/// A header that did not spell "[Script Info]" and "ScriptType: v4.00+" exactly was replaced by
/// the stock header on OK, which threw away every style in the file (#15126).
/// </summary>
public class AdvancedSubStationAlphaHeaderRebuildTest
{
    private static List<SsaStyle> TwoStyles() =>
    [
        new SsaStyle { Name = "Default", FontName = "cinema", FontSize = 40 },
        new SsaStyle { Name = "Gothic", FontName = "CinemaGothic", FontSize = 39 },
    ];

    private static string Rebuild(string scriptInfo)
    {
        var header = scriptInfo + "\r\n\r\n[V4+ Styles]\r\n" + SsaStyle.DefaultAssStyleFormat + "\r\n" +
                     "Style: Default,cinema,40,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,1,1,2,10,10,7,1\r\n" +
                     "Style: Gothic,CinemaGothic,35,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,-1,0,0,0,100,100,0,0,1,1,1,2,10,10,7,1\r\n\r\n[Events]";
        return AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(header, TwoStyles());
    }

    private static string[] StyleNames(string header) =>
        AdvancedSubStationAlpha.GetStylesFromHeader(header).ToArray();

    [Fact]
    public void StandardHeader_KeepsScriptInfoAndStyles()
    {
        var header = Rebuild("[Script Info]\r\nTitle: my film\r\nScriptType: v4.00+\r\nPlayResX: 1234\r\nPlayResY: 516");

        Assert.Equal(new[] { "Default", "Gothic" }, StyleNames(header));
        Assert.Contains("Title: my film", header);
        Assert.Contains("PlayResX: 1234", header);
        Assert.Contains("ScriptType: v4.00+", header);
        Assert.Contains("Style: Gothic,CinemaGothic,39,", header);
    }

    [Fact]
    public void HeaderWithoutScriptTypeLine_KeepsStylesAndAddsTheLine()
    {
        var header = Rebuild("[Script Info]\r\nTitle: my film\r\nPlayResX: 1234\r\nPlayResY: 516");

        Assert.Equal(new[] { "Default", "Gothic" }, StyleNames(header));
        Assert.Contains("Title: my film", header);
        Assert.Contains("PlayResX: 1234", header);
        Assert.Single(header.SplitToLines().Where(l => l.StartsWith("ScriptType:")));
        Assert.Contains("ScriptType: v4.00+", header);
    }

    [Theory]
    [InlineData("ScriptType: V4.00+")]
    [InlineData("ScriptType:v4.00+")]
    [InlineData("ScriptType: v4.00")]
    [InlineData("scripttype: v4.00+")]
    public void ScriptTypeSpelledDifferently_KeepsStylesAndNormalisesTheLine(string scriptTypeLine)
    {
        var header = Rebuild("[Script Info]\r\nTitle: my film\r\n" + scriptTypeLine + "\r\nPlayResY: 516");

        Assert.Equal(new[] { "Default", "Gothic" }, StyleNames(header));
        Assert.Contains("Title: my film", header);
        Assert.Contains("PlayResY: 516", header);
        Assert.Single(header.SplitToLines().Where(l => l.StartsWith("ScriptType:", System.StringComparison.OrdinalIgnoreCase)));
        Assert.Contains("ScriptType: v4.00+", header);
    }

    [Fact]
    public void LowercaseScriptInfoSection_KeepsStyles()
    {
        var header = Rebuild("[script info]\r\nTitle: my film\r\nScriptType: v4.00+\r\nPlayResY: 516");

        Assert.Equal(new[] { "Default", "Gothic" }, StyleNames(header));
        Assert.Contains("[Script Info]", header);
        Assert.Contains("Title: my film", header);
        Assert.Contains("PlayResY: 516", header);
    }

    [Fact]
    public void HeaderWithoutScriptInfoSection_StillKeepsTheStyles()
    {
        var header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            "[V4+ Styles]\r\n" + SsaStyle.DefaultAssStyleFormat + "\r\nStyle: Default,cinema,40,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,1,1,2,10,10,7,1\r\n\r\n[Events]",
            TwoStyles());

        Assert.Equal(new[] { "Default", "Gothic" }, StyleNames(header));
        Assert.Contains("[Script Info]", header);
        Assert.Contains("ScriptType: v4.00+", header);
    }

    [Fact]
    public void NoStyles_FallsBackToTheStockHeader()
    {
        var header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            "[Script Info]\r\nScriptType: v4.00+", new List<SsaStyle>());

        Assert.Equal(new[] { "Default" }, StyleNames(header));
    }
}
