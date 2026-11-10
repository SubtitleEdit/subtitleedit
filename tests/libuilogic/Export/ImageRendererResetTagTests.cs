using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// "&lt;r&gt;" (ASSA "{\r}") goes back to how the line started - the parameter's own
/// italic/underline/strikeout and font colour - whatever tags are open.
/// </summary>
public class ImageRendererResetTagTests
{
    private static SKBitmap Render(string text, bool italic = false)
    {
        return ImageRenderer.GenerateBitmap(new ImageParameter
        {
            Text = text,
            FontName = "Arial",
            FontSize = 40,
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            OutlineWidth = 2,
            ScreenWidth = 1280,
            ScreenHeight = 720,
            ContentAlignment = ExportContentAlignment.Center,
            IsItalic = italic,
        });
    }

    private static bool SamePixels(SKBitmap a, SKBitmap b) =>
        a.Width == b.Width && a.Height == b.Height && a.Bytes.AsSpan().SequenceEqual(b.Bytes);

    private static bool HasRed(SKBitmap bitmap)
    {
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y) is { Alpha: > 200, Red: > 200, Green: < 60, Blue: < 60 })
                {
                    return true;
                }
            }
        }

        return false;
    }

    [Fact]
    public void Reset_ClosesEveryOpenTag()
    {
        using var reset = Render("<i><b><font color=\"#ff0000\">A</font></b></i><r>Hello world");
        using var plain = Render("<i><b><font color=\"#ff0000\">A</font></b></i>Hello world");
        using var resetMidway = Render("<i><b><font color=\"#ff0000\">A<r>Hello world");

        Assert.True(SamePixels(reset, plain));
        Assert.True(SamePixels(resetMidway, plain));
    }

    [Fact]
    public void Reset_RemovesTheColour()
    {
        using var bitmap = Render("<font color=\"#ff0000\"><r>Hello world");

        Assert.False(HasRed(bitmap));
    }

    [Fact]
    public void Reset_ComesBackToTheLinesItalic()
    {
        // An italic style line: "{\i0}A{\r}B" - B is italic again.
        using var reset = Render("</i>Hello<r> world", italic: true);
        using var expected = Render("</i>Hello<i> world</i>", italic: false);

        Assert.True(SamePixels(reset, expected));
    }

    [Fact]
    public void ItalicLine_IsTheSameAsAnItalicTag()
    {
        using var line = Render("Hello world", italic: true);
        using var tag = Render("<i>Hello world</i>");

        Assert.True(SamePixels(line, tag));
    }

    // "{\rDefault}" on an italic "Sign" line: the reset's "</i>" came after the "<font>" of the
    // Default style and took it off again - the text stayed italic in the Sign colour.
    [Fact]
    public void ResetToUprightStyle_FromItalicStyle_DrawsTheTargetStyle()
    {
        const string header = "[Script Info]\nPlayResX: 1280\nPlayResY: 720\n\n[V4+ Styles]\n" +
            "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\n" +
            "Style: Default,Arial,40,&H000000FF,&H000000FF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,0,2,10,10,10,1\n" +
            "Style: Sign,Arial,40,&H00FFFFFF,&H000000FF,&H00000000,&H00000000,0,-1,0,0,100,100,0,0,1,2,0,2,10,10,10,1\n";
        var styles = ExportAssaStyles.FromHeader(header)!;

        using var reset = RenderStyled(styles, "Sign", "{\\rDefault}Hello world");
        using var plain = RenderStyled(styles, "Default", "Hello world");

        Assert.True(HasRed(reset));
        Assert.True(SamePixels(reset, plain));
    }

    private static SKBitmap RenderStyled(ExportAssaStyles styles, string styleName, string rawText)
    {
        var ip = new ImageParameter
        {
            Text = ExportTextTags.ToRenderableText(rawText, styles, styleName),
            ScreenWidth = 1280,
            ScreenHeight = 720,
        };
        styles.Apply(ip, styleName, rawText);
        ip.ContentAlignment = ExportContentAlignment.Center;
        return ImageRenderer.GenerateBitmap(ip);
    }
}
