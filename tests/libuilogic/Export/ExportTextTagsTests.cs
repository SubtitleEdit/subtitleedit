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
    [InlineData("{\\i1\\pos(10,20)}Hello", "<i>Hello</i>")]
    [InlineData("{\\pos(10,20)\\i1}Hello", "<i>Hello</i>")]
    [InlineData("{\\an8\\fad(300,300)\\b1}Hello", "<b>Hello</b>")]
    [InlineData("{\\fad(300,300}{\\i1}Hello", "<i>Hello</i>")] // missing ")" as SE's own effects write it
    [InlineData("{\\fade(255,0,255,0,500,2000,2200)\\i1}Hello", "<i>Hello</i>")]
    [InlineData("{\\alpha&H80&\\i1}Hello", "<i>Hello</i>")]
    [InlineData("{\\1a&H80&\\3a&HFF&}{\\i1}Hello", "<i>Hello</i>")]
    [InlineData("{\\pos(10,20)\\c&H0000FF&}Red{\\c}", "<font color=\"#ff0000\">Red</font>")]
    [InlineData("{\\bord0\\i1}Hello", "<i>Hello</i>")]
    [InlineData("{\\bord2.5\\shad0\\b1}Hello", "<b>Hello</b>")]
    [InlineData("{\\3c&H0000FF&\\4c&H00FF00&\\i1}Hello", "<i>Hello</i>")]
    public void ToRenderableText_ConsumedTags_DoNotCostTheLineItsFormatting(string text, string expected)
    {
        // "\pos", "\fad", "\alpha", "\3c"/"\4c" and "\bord"/"\shad" are read off the text
        // before rendering (ApplyPositionTag/ApplyTransparencyTags/ApplyStyleOverrideTags),
        // so they must not make GetFormattedText treat the line as too complex and drop the
        // formatting tags next to them.
        Assert.Equal(expected, ExportTextTags.ToRenderableText(text));
    }

    [Theory]
    [InlineData("{\\move(10,20,30,40)\\i1}Hello", "<i>Hello</i>")]
    [InlineData("{\\xbord4\\i1}Hello", "<i>Hello</i>")]
    [InlineData("{\\t(0,200,\\bord0)\\i1}Hello", "<i>Hello</i>")]
    [InlineData("{\\blur2\\i1}Hello", "<i>Hello</i>")]
    [InlineData("{\\be1}{\\b1}Hello", "<b>Hello</b>")]
    [InlineData("{\\k50}Hel{\\k30\\i1}lo", "Hel<i>lo</i>")]
    [InlineData("{\\fscx120\\fscy80\\fs40}Hello", "<font size=\"40\">Hello</font>")]
    [InlineData("{\\clip(0,0,100,100)\\c&H0000FF&}Red", "<font color=\"#ff0000\">Red</font>")]
    [InlineData("{\\frz10\\fnArial}Hello", "<font face=\"Arial\">Hello</font>")]
    [InlineData("{\\t(0,200,\\i1)}Hello", "Hello")] // an italic inside \t is an animation target, not the line's look
    [InlineData("{\\p1}m 0 0 l 100 0{\\p0}Hello", "Hello")] // drawing commands are not text
    [InlineData("{\\i1}A{\\p2}m 0 0 l 100 0", "<i>A</i>")]
    public void ToRenderableText_UndrawableTags_DoNotCostTheLineItsFormatting(string text, string expected)
    {
        // A tag the renderer cannot draw made GetFormattedText treat the whole line as too
        // complex, which dropped the italic/bold/colour/font tags next to it as well.
        Assert.Equal(expected, ExportTextTags.ToRenderableText(text));
    }

    [Fact]
    public void ToRenderableText_CommentBlockNextToTags_IsKept()
    {
        // "{note}" has no backslash - a comment, not a tag block - and was never touched here
        // (the export window removes comment blocks itself when asked to, #15584).
        Assert.Equal("{note}<i>Hello</i>", ExportTextTags.ToRenderableText("{note}{\\blur1\\i1}Hello"));
    }

    [Theory]
    [InlineData("<u>Hello</u>", "<u>Hello</u>")]
    [InlineData("{\\u1}Hello{\\u0}", "<u>Hello</u>")]
    [InlineData("{\\s1}Hello{\\s0}", "<s>Hello</s>")]
    [InlineData("{\\s1\\i1}Hel{\\i0}lo", "<i><s>Hel</i>lo")]
    [InlineData("{\\s1\\s0}Hello", "</s>Hello")] // the last one in a block wins
    [InlineData("{\\shad2\\s1}Hello", "<s>Hello")] // "\shad" is not "\s"
    public void ToRenderableText_UnderlineAndStrikeoutTags_BecomeHtml(string text, string expected)
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

    private static ImageParameter MakeStyledParameter()
    {
        var ip = MakeParameter(ExportAlignment.BottomCenter);
        ip.OutlineColor = new SKColor(1, 2, 3);
        ip.ShadowColor = new SKColor(4, 5, 6, 128); // half transparent - a "\4c" must keep that
        ip.OutlineWidth = 3;
        ip.ShadowWidth = 2;
        return ip;
    }

    [Fact]
    public void ApplyStyleOverrideTags_OutlineColorTag_SetsOutlineColor()
    {
        var ip = MakeStyledParameter();

        // ASSA colors are &HBBGGRR& - "&H0000FF&" is red
        ExportTextTags.ApplyStyleOverrideTags(ip, "{\\3c&H0000FF&}Hello");

        Assert.Equal(new SKColor(255, 0, 0), ip.OutlineColor);
        Assert.Equal(new SKColor(4, 5, 6, 128), ip.ShadowColor); // untouched
    }

    [Fact]
    public void ApplyStyleOverrideTags_ShadowColorTag_KeepsTheParameterAlpha()
    {
        var ip = MakeStyledParameter();

        ExportTextTags.ApplyStyleOverrideTags(ip, "{\\4c&HFF0000&}Hello");

        // "&HFF0000&" is blue; the transparency stays with the parameter ("\4a" owns it)
        Assert.Equal(new SKColor(0, 0, 255, 128), ip.ShadowColor);
    }

    [Fact]
    public void ApplyStyleOverrideTags_EightDigitColor_ReadsOnlyTheLowSixDigits()
    {
        var ip = MakeStyledParameter();

        // Some tools write "&HAABBGGRR&" - the alpha up front belongs to "\3a", not the color
        ExportTextTags.ApplyStyleOverrideTags(ip, "{\\3c&H800000FF&}Hello");

        Assert.Equal(new SKColor(255, 0, 0), ip.OutlineColor);
    }

    [Theory]
    [InlineData("{\\bord0}Hello", 0.0, 2.0)] // how ASSA turns the outline off for one line
    [InlineData("{\\bord2.5}Hello", 2.5, 2.0)]
    [InlineData("{\\shad0}Hello", 3.0, 0.0)]
    [InlineData("{\\bord4\\shad1}Hello", 4.0, 1.0)]
    public void ApplyStyleOverrideTags_WidthTags_OverrideTheDialogWidths(string text, double expectedOutline, double expectedShadow)
    {
        var ip = MakeStyledParameter();

        ExportTextTags.ApplyStyleOverrideTags(ip, text);

        Assert.Equal(expectedOutline, ip.OutlineWidth);
        Assert.Equal(expectedShadow, ip.ShadowWidth);
    }

    [Fact]
    public void ApplyStyleOverrideTags_ScriptResolutionDiffers_ScalesTheWidths()
    {
        var ip = MakeStyledParameter();

        // 288 line script on a 1080 line canvas: 2 * 1080/288 = 7.5
        ExportTextTags.ApplyStyleOverrideTags(ip, "{\\bord2}Hello", scriptHeight: 288);

        Assert.Equal(7.5, ip.OutlineWidth);
    }

    [Theory]
    [InlineData("{\\fsp4}Hello", 4f)]
    [InlineData("{\\fsp-1.5}Hello", -1.5f)]
    [InlineData("{\\fsp0}Hello", 0f)] // explicit 0 still overrides the dialog's spacing
    [InlineData("{\\fsp2}Hel{\\fsp6}lo", 6f)] // last one wins
    public void ApplyStyleOverrideTags_LetterSpacingTag_SetsTagLetterSpacing(string text, float expected)
    {
        var ip = MakeStyledParameter();

        ExportTextTags.ApplyStyleOverrideTags(ip, text);

        Assert.Equal(expected, ip.TagLetterSpacing);
    }

    [Theory]
    [InlineData("{\\blur3}Hello", 3.0)]
    [InlineData("{\\blur0.5}Hello", 0.5)]
    [InlineData("{\\be2}Hello", 1.0)] // two [1 2 1] passes: sqrt(2 / 2)
    [InlineData("{\\blur3\\be32}Hello", 5.0)] // sqrt(3^2 + 32 / 2)
    [InlineData("{\\blur0}Hello", 0.0)]
    public void ApplyStyleOverrideTags_BlurTags_SetTagBlur(string text, double expected)
    {
        var ip = MakeStyledParameter();

        ExportTextTags.ApplyStyleOverrideTags(ip, text);

        Assert.NotNull(ip.TagBlur);
        Assert.Equal(expected, ip.TagBlur!.Value, 3);
    }

    [Fact]
    public void ApplyStyleOverrideTags_ScriptResolutionDiffers_ScalesSpacingAndBlur()
    {
        var ip = MakeStyledParameter();

        ExportTextTags.ApplyStyleOverrideTags(ip, "{\\fsp2\\blur2}Hello", scriptHeight: 288);

        Assert.Equal(7.5f, ip.TagLetterSpacing!.Value, 3);
        Assert.Equal(7.5f, ip.TagBlur!.Value, 3);
    }

    [Theory]
    [InlineData("{\\fscx150}Hello", 150f, 100f)]
    [InlineData("{\\fscy80}Hello", 100f, 80f)]
    [InlineData("{\\fscx120\\fscy90.5}Hello", 120f, 90.5f)]
    [InlineData("Hello", 100f, 100f)]
    [InlineData("{\\fs40\\fsp2}Hello", 100f, 100f)] // not "\fsc"
    [InlineData("{\\t(0,200,\\fscx200)}Hello", 100f, 100f)] // an animation target, not the line's look
    public void ApplyStyleOverrideTags_ScaleTags_SetTagScale(string text, float expectedX, float expectedY)
    {
        var ip = MakeStyledParameter();

        // Percentages: the script resolution must not change them.
        ExportTextTags.ApplyStyleOverrideTags(ip, text, scriptHeight: 288);

        Assert.Equal(expectedX, ip.TagScaleX);
        Assert.Equal(expectedY, ip.TagScaleY);
    }

    [Theory]
    [InlineData("{\\frz15}Hello", 15f)]
    [InlineData("{\\fr-7.5}Hello", -7.5f)]
    [InlineData("{\\frx30\\fry20}Hello", 0f)] // 3D rotations are not a flat turn
    [InlineData("{\\fn Arial}Hello", 0f)]
    [InlineData("{\\t(0,200,\\frz90)}Hello", 0f)] // an animation target, not the line's look
    [InlineData("{\\frz10}Hel{\\frz20}lo", 20f)] // last one wins
    public void ApplyStyleOverrideTags_RotationTag_SetsTagRotation(string text, float expected)
    {
        var ip = MakeStyledParameter();

        ExportTextTags.ApplyStyleOverrideTags(ip, text);

        Assert.Equal(expected, ip.TagRotation);
    }

    [Theory]
    [InlineData("Hello")]
    [InlineData("{\\b1\\bord2}Hello")] // "\b1"/"\bord" are not "\be"
    [InlineData("{\\fs40\\fscx120}Hello")] // "\fs"/"\fscx" are not "\fsp"
    [InlineData("{\\t(0,200,\\blur4\\fsp3)}Hello")] // an animation target, not the line's look
    public void ApplyStyleOverrideTags_NoSpacingOrBlurTag_LeavesThemUnset(string text)
    {
        var ip = MakeStyledParameter();

        ExportTextTags.ApplyStyleOverrideTags(ip, text);

        Assert.Null(ip.TagLetterSpacing);
        Assert.Null(ip.TagBlur);
    }

    [Theory]
    [InlineData("Hello")]
    [InlineData(null)]
    [InlineData("{\\i1}Hello")]
    [InlineData("{\\xbord4\\yshad3}Hello")] // one axis variants are not consumed
    [InlineData("{\\t(0,200,\\bord0)}Hello")] // an animation target, not the line's look
    [InlineData("{\\t(0,200,\\3c&H0000FF&)}Hello")]
    public void ApplyStyleOverrideTags_NoConsumableTag_LeavesTheParameterAlone(string? text)
    {
        var ip = MakeStyledParameter();

        ExportTextTags.ApplyStyleOverrideTags(ip, text);

        Assert.Equal(new SKColor(1, 2, 3), ip.OutlineColor);
        Assert.Equal(new SKColor(4, 5, 6, 128), ip.ShadowColor);
        Assert.Equal(3.0, ip.OutlineWidth);
        Assert.Equal(2.0, ip.ShadowWidth);
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
