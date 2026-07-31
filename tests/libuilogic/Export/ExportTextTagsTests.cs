using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

public class ExportTextTagsTests
{
    private static ImageParameter MakeParameter(ExportAlignment alignment, int bitmapWidth = 200, int bitmapHeight = 50)
    {
        return new ImageParameter
        {
            Alignment = alignment,
            ScreenWidth = 1920,
            ScreenHeight = 1080,
            Bitmap = new SKBitmap(bitmapWidth, bitmapHeight),
        };
    }

    [Theory]
    [InlineData("{\\an1}Hello", ExportAlignment.BottomLeft)]
    [InlineData("{\\an2}Hello", ExportAlignment.BottomCenter)]
    [InlineData("{\\an3}Hello", ExportAlignment.BottomRight)]
    [InlineData("{\\an4}Hello", ExportAlignment.MiddleLeft)]
    [InlineData("{\\an5}Hello", ExportAlignment.MiddleCenter)]
    [InlineData("{\\an6}Hello", ExportAlignment.MiddleRight)]
    [InlineData("{\\an7}Hello", ExportAlignment.TopLeft)]
    [InlineData("{\\an8}Hello", ExportAlignment.TopCenter)]
    [InlineData("{\\an9}Hello", ExportAlignment.TopRight)]
    public void GetAlignment_AlignmentTag_MapsToExportAlignment(string text, ExportAlignment expected)
    {
        Assert.Equal(expected, ExportTextTags.GetAlignment(text, ExportAlignment.BottomCenter));
    }

    [Theory]
    [InlineData("{\\an8\\i1}Hello")] // multi tag, alignment first
    [InlineData("{\\i1\\an8}Hello")] // multi tag, alignment last
    [InlineData("{\\pos(10,20)\\an8}Hello")]
    [InlineData("{an8\\i1}Hello")] // malformed variant that RemoveAssAlignmentTags also strips
    [InlineData("  {\\an8}Hello")]
    public void GetAlignment_TagInsideBlock_IsFound(string text)
    {
        Assert.Equal(ExportAlignment.TopCenter, ExportTextTags.GetAlignment(text, ExportAlignment.BottomCenter));
    }

    [Theory]
    [InlineData("Hello")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("{\\i1}Hello")]
    [InlineData("{\\an8Hello")] // no closing brace, not a tag
    [InlineData("Hello {\\an8}")] // not leading, ignored like the export window always did
    public void GetAlignment_NoLeadingAlignmentTag_UsesFallback(string? text)
    {
        Assert.Equal(ExportAlignment.MiddleRight, ExportTextTags.GetAlignment(text, ExportAlignment.MiddleRight));
    }

    [Theory]
    [InlineData("{\\an8}Hello", "Hello")]
    [InlineData("{\\an8\\an8}Hello", "Hello")]
    [InlineData("Hello", "Hello")]
    [InlineData(null, "")]
    public void ToRenderableText_AlignmentTags_AreRemoved(string? text, string expected)
    {
        Assert.Equal(expected, ExportTextTags.ToRenderableText(text));
    }

    [Theory]
    [InlineData("{\\i1}Hello{\\i0}", "<i>Hello</i>")]
    [InlineData("{\\b1}Hello{\\b0}", "<b>Hello</b>")]
    [InlineData("{\\an8\\i1}Hello{\\i0}", "<i>Hello</i>")]
    [InlineData("{\\i1}Hello", "<i>Hello</i>")] // unclosed tag is closed for the renderer
    public void ToRenderableText_AssaStyleTags_BecomeHtml(string text, string expected)
    {
        Assert.Equal(expected, ExportTextTags.ToRenderableText(text));
    }

    [Fact]
    public void ToRenderableText_AssaColorTag_BecomesFontColor()
    {
        // ASSA colors are &HBBGGRR& - blue-red-green order flipped for HTML
        Assert.Equal("<font color=\"#ff0000\">Red</font>", ExportTextTags.ToRenderableText("{\\c&H0000FF&}Red{\\c}"));
    }

    [Theory]
    [InlineData("{\\pos(10,20)}Hello", "Hello")] // "too complex" for GetFormattedText, must not be drawn
    [InlineData("{\\fad(200,200)}Hello", "Hello")]
    [InlineData("{\\k50}Hello", "Hello")]
    [InlineData("{\\an8\\pos(10,20)}Hello", "Hello")]
    public void ToRenderableText_UntranslatableAssaTags_AreDroppedNotDrawn(string text, string expected)
    {
        Assert.Equal(expected, ExportTextTags.ToRenderableText(text));
    }

    [Theory]
    [InlineData("<u>Hello</u>", "Hello")] // the renderer cannot underline
    [InlineData("{\\u1}Hello{\\u0}", "Hello")]
    public void ToRenderableText_UnderlineTags_AreRemoved(string text, string expected)
    {
        Assert.Equal(expected, ExportTextTags.ToRenderableText(text));
    }

    [Theory]
    [InlineData("{\\pos(300,200)}Hello", 300f, 200f)]
    [InlineData("{\\an8\\pos(300,200)}Hello", 300f, 200f)]
    [InlineData("{\\pos( 300 , 200 )}Hello", 300f, 200f)]
    [InlineData("{\\pos(300.5,200.25)}Hello", 300.5f, 200.25f)]
    public void TryGetPosition_PosTag_IsRead(string text, float expectedX, float expectedY)
    {
        Assert.True(ExportTextTags.TryGetPosition(text, out var x, out var y));
        Assert.Equal(expectedX, x);
        Assert.Equal(expectedY, y);
    }

    [Theory]
    [InlineData("Hello")]
    [InlineData("{\\an8}Hello")]
    [InlineData("{\\move(10,20,30,40)}Hello")]
    [InlineData(null)]
    public void TryGetPosition_NoPosTag_ReturnsFalse(string? text)
    {
        Assert.False(ExportTextTags.TryGetPosition(text, out _, out _));
    }

    [Theory]
    // \pos gives the anchor point for the current alignment, the bitmap is 200x50
    [InlineData(ExportAlignment.TopLeft, 300, 200)]
    [InlineData(ExportAlignment.TopCenter, 200, 200)]
    [InlineData(ExportAlignment.TopRight, 100, 200)]
    [InlineData(ExportAlignment.MiddleLeft, 300, 175)]
    [InlineData(ExportAlignment.MiddleCenter, 200, 175)]
    [InlineData(ExportAlignment.BottomLeft, 300, 150)]
    [InlineData(ExportAlignment.BottomCenter, 200, 150)]
    [InlineData(ExportAlignment.BottomRight, 100, 150)]
    public void ApplyPositionTag_AnchorsByAlignment(ExportAlignment alignment, int expectedLeft, int expectedTop)
    {
        var ip = MakeParameter(alignment);

        ExportTextTags.ApplyPositionTag(ip, "{\\pos(300,200)}Hello");

        Assert.NotNull(ip.OverridePosition);
        Assert.Equal(expectedLeft, ip.OverridePosition!.Value.X);
        Assert.Equal(expectedTop, ip.OverridePosition.Value.Y);
    }

    [Fact]
    public void ApplyPositionTag_ScriptResolutionDiffers_ScalesToCanvas()
    {
        var ip = MakeParameter(ExportAlignment.TopLeft);

        // 384x288 script on a 1920x1080 canvas: x*5, y*3.75
        ExportTextTags.ApplyPositionTag(ip, "{\\pos(100,100)}Hello", 384, 288);

        Assert.NotNull(ip.OverridePosition);
        Assert.Equal(500, ip.OverridePosition!.Value.X);
        Assert.Equal(375, ip.OverridePosition.Value.Y);
    }

    [Fact]
    public void ApplyPositionTag_OutsideTheFrame_IsClampedInside()
    {
        var ip = MakeParameter(ExportAlignment.TopLeft);

        ExportTextTags.ApplyPositionTag(ip, "{\\pos(-100,5000)}Hello");

        Assert.NotNull(ip.OverridePosition);
        Assert.Equal(0, ip.OverridePosition!.Value.X);
        Assert.Equal(1080 - 50, ip.OverridePosition.Value.Y);
    }

    [Fact]
    public void ApplyPositionTag_NoPosTag_LeavesOverridePositionUnset()
    {
        var ip = MakeParameter(ExportAlignment.BottomCenter);

        ExportTextTags.ApplyPositionTag(ip, "{\\an8}Hello");

        Assert.Null(ip.OverridePosition);
    }

    [Theory]
    [InlineData("[Script Info]\r\nPlayResX: 384\r\nPlayResY: 288\r\n", 384, 288)]
    [InlineData("[Script Info]\r\nPlayResX: 0\r\nPlayResY: 288\r\n", 0, 0)]
    [InlineData("[Script Info]\r\nTitle: none\r\n", 0, 0)]
    [InlineData(null, 0, 0)]
    public void GetScriptResolution_ReadsPlayRes(string? header, int expectedWidth, int expectedHeight)
    {
        var (width, height) = ExportTextTags.GetScriptResolution(header);
        Assert.Equal(expectedWidth, width);
        Assert.Equal(expectedHeight, height);
    }

    [Theory]
    [InlineData("<i>Hello</i>")]
    [InlineData("<b>Hello</b>")]
    [InlineData("<font color=\"#ff4040\">Hello</font>")]
    [InlineData("Plain text")]
    [InlineData("C:\\new folder")] // a lone backslash in plain text must survive
    [InlineData("50% > 40%")]
    public void ToRenderableText_TextWithoutAssaTags_IsUnchanged(string text)
    {
        Assert.Equal(text, ExportTextTags.ToRenderableText(text));
    }
}
