using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

public class ExportAssaStylesTests
{
    private const string Header = @"[Script Info]
ScriptType: v4.00+
PlayResX: 1280
PlayResY: 720

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Arial,40,&H00FFFFFF,&H000000FF,&H00000000,&H80000000,0,0,0,0,100,100,0,0,1,2,1,2,20,30,40,1
Style: Sign,Impact,60,&H0000FFFF,&H000000FF,&H00FF0000,&H00000000,-1,-1,-1,-1,120,90,2,10,1,3,0,8,10,10,10,1
Style: Boxed,Arial,40,&H00FFFFFF,&H000000FF,&H00202020,&H00000000,0,0,0,0,100,100,0,0,3,4,2,1,50,60,70,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
";

    // 1920x1080 canvas from a 1280x720 script: everything scales by 1.5
    private static ImageParameter MakeParameter(string text = "Hello")
    {
        return new ImageParameter
        {
            Text = text,
            ScreenWidth = 1920,
            ScreenHeight = 1080,
            FontName = "Dialog font",
            FontSize = 99,
            IsBold = true,
            OutlineWidth = 9,
            ShadowWidth = 9,
            BoxType = ExportBoxType.OneBox,
            Alignment = ExportAlignment.MiddleCenter,
            ContentAlignment = ExportContentAlignment.Left,
        };
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("[Script Info]\nPlayResX: 1280\nPlayResY: 720\n")]
    public void FromHeader_NoStyles_IsNull(string? header)
    {
        Assert.Null(ExportAssaStyles.FromHeader(header));
    }

    [Fact]
    public void Apply_DefaultStyle_ReplacesTheWindowSettings()
    {
        var ip = MakeParameter();

        ExportAssaStyles.FromHeader(Header)!.Apply(ip, "Default", "Hello");

        Assert.Equal("Arial", ip.FontName);
        Assert.Equal(60f, ip.FontSize);
        Assert.False(ip.IsBold);
        Assert.Equal(SKColors.White, ip.FontColor);
        Assert.Equal(SKColors.Black, ip.OutlineColor);
        Assert.Equal(new SKColor(0, 0, 0, 0x7F), ip.ShadowColor); // &H80 transparency
        Assert.Equal(3.0, ip.OutlineWidth);
        Assert.Equal(1.5, ip.ShadowWidth);
        Assert.Equal(ExportBoxType.None, ip.BoxType);
        Assert.Equal(ExportAlignment.BottomCenter, ip.Alignment);
        Assert.Equal(ExportContentAlignment.FromAlignment, ip.ContentAlignment);
        Assert.Equal(60, ip.BottomTopMargin);
        Assert.Equal(38, ip.LeftRightMargin); // centered: the average of 30 and 45
        Assert.Equal("Hello", ip.Text);
        Assert.Equal(1.5f, ip.TagFontSizeScale);
    }

    [Fact]
    public void Apply_SignStyle_ReadsEveryProperty()
    {
        var ip = MakeParameter();

        ExportAssaStyles.FromHeader(Header)!.Apply(ip, "Sign", "Hello");

        Assert.Equal("Impact", ip.FontName);
        Assert.Equal(90f, ip.FontSize);
        Assert.True(ip.IsBold);
        Assert.Equal(new SKColor(255, 255, 0), ip.FontColor); // &H0000FFFF is yellow
        Assert.Equal(new SKColor(0, 0, 255), ip.OutlineColor); // &H00FF0000 is blue
        Assert.True(ip.IsItalic);
        Assert.True(ip.IsUnderline);
        Assert.True(ip.IsStrikeout);
        Assert.Equal("Hello", ip.Text);
        Assert.Equal(ExportAlignment.TopCenter, ip.Alignment);
        Assert.Equal(120f, ip.TagScaleX);
        Assert.Equal(90f, ip.TagScaleY);
        Assert.Equal(3f, ip.TagLetterSpacing);
        Assert.Equal(10f, ip.TagRotation);
    }

    [Fact]
    public void Apply_OpaqueBoxStyle_DrawsABoxPerLineInTheOutlineColour()
    {
        var ip = MakeParameter();

        ExportAssaStyles.FromHeader(Header)!.Apply(ip, "Boxed", "Hello");

        Assert.Equal(ExportBoxType.BoxPerLine, ip.BoxType);
        Assert.Equal(new SKColor(0x20, 0x20, 0x20), ip.BackgroundColor);
        Assert.Equal(6, ip.BoxPaddingLeft);
        Assert.Equal(6, ip.BoxPaddingTop);
        Assert.Equal(0.0, ip.OutlineWidth);
        Assert.Equal(ExportAlignment.BottomLeft, ip.Alignment);
        Assert.Equal(75, ip.LeftRightMargin); // left aligned: MarginL
    }

    [Theory]
    [InlineData("Sign", "Sign")]
    [InlineData("*Sign", "Sign")]
    [InlineData("sign", "Sign")]
    [InlineData("Unknown", "Default")]
    [InlineData(null, "Default")]
    public void GetStyle_FindsTheStyleOrFallsBackToDefault(string? name, string expected)
    {
        Assert.Equal(expected, ExportAssaStyles.FromHeader(Header)!.GetStyle(name).Name);
    }

    [Fact]
    public void Apply_AlignmentTag_WinsOverTheStyle()
    {
        var ip = MakeParameter();

        ExportAssaStyles.FromHeader(Header)!.Apply(ip, "Sign", "{\\an1}Hello");

        Assert.Equal(ExportAlignment.BottomLeft, ip.Alignment);
    }

    [Fact]
    public void Apply_LineMargins_WinOverTheStyle()
    {
        var ip = MakeParameter();

        ExportAssaStyles.FromHeader(Header)!.Apply(ip, "Boxed", "Hello", "100", "0", "200");

        Assert.Equal(150, ip.LeftRightMargin);
        Assert.Equal(300, ip.BottomTopMargin);
    }

    [Fact]
    public void Apply_ThenOverrideTags_TagsWin()
    {
        var ip = MakeParameter();
        var styles = ExportAssaStyles.FromHeader(Header)!;
        const string text = "{\\bord5\\3c&H00FF00&\\fscx50}Hello";

        styles.Apply(ip, "Default", text);
        ExportTextTags.ApplyStyleOverrideTags(ip, text, styles.ScriptResolution.Height);

        Assert.Equal(7.5, ip.OutlineWidth);
        Assert.Equal(new SKColor(0, 255, 0), ip.OutlineColor);
        Assert.Equal(50f, ip.TagScaleX);
    }

    [Fact]
    public void Apply_NoPlayRes_UsesTheLibassDefaultResolution()
    {
        var header = Header.Replace("PlayResX: 1280\n", string.Empty).Replace("PlayResY: 720\n", string.Empty)
            .Replace("PlayResX: 1280\r\n", string.Empty).Replace("PlayResY: 720\r\n", string.Empty);
        var ip = MakeParameter();

        ExportAssaStyles.FromHeader(header)!.Apply(ip, "Default", "Hello");

        Assert.Equal(150f, ip.FontSize); // 40 * 1080 / 288
    }

    [Theory]
    [InlineData("A{\\r}B", "A<r>B")]
    [InlineData("{\\i1}A{\\r}B", "<i>A<r>B</i>")]
    [InlineData("{\\i1\\r\\b1}A", "<i><r><b>A</i></b>")] // a reset only undoes the tags before it
    [InlineData("A{\\rDefault}B", "A<r>B")] // the line's own style: a plain reset
    [InlineData("A{\\rNoSuchStyle}B", "A<r>B")] // libass falls back to a plain reset too
    public void ToRenderableText_Reset_BecomesTheRendererReset(string text, string expected)
    {
        Assert.Equal(expected, ExportTextTags.ToRenderableText(text, ExportAssaStyles.FromHeader(Header), "Default"));
    }

    [Fact]
    public void ToRenderableText_ResetToAnotherStyle_SwitchesToItsLook()
    {
        var text = ExportTextTags.ToRenderableText("A{\\rSign}B", ExportAssaStyles.FromHeader(Header), "Default");

        // Sign: Impact 60, yellow, bold, italic, underline, strikeout (the closing tags at the end
        // are GetFormattedText balancing what it sees open, and change nothing)
        Assert.Equal("A<r><font face=\"Impact\" size=\"60\" color=\"#ffff00\"><b><i><u><s>B</i></u></b>", text);
    }

    [Fact]
    public void ToRenderableText_ResetFromAnItalicStyle_TurnsItalicOff()
    {
        var text = ExportTextTags.ToRenderableText("A{\\rDefault}B", ExportAssaStyles.FromHeader(Header), "Sign");

        Assert.Equal("A<r><font face=\"Arial\" size=\"40\" color=\"#ffffff\"></i></u></s>B", text);
    }

    [Fact]
    public void ToRenderableText_ResetWithoutStyles_IsAPlainReset()
    {
        Assert.Equal("A<r>B", ExportTextTags.ToRenderableText("A{\\rSign}B"));
    }

    [Fact]
    public void Apply_ItalicStyle_CanBeTurnedOffByATag()
    {
        var ip = MakeParameter(ExportTextTags.ToRenderableText("{\\i0}Upright"));

        ExportAssaStyles.FromHeader(Header)!.Apply(ip, "Sign", "{\\i0}Upright");

        Assert.True(ip.IsItalic);
        Assert.Equal("</i>Upright", ip.Text);
    }
}
