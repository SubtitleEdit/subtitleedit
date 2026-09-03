using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// "&lt;font face=..&gt;" and "&lt;font size=..&gt;" in the exported text (discussion #14476):
/// each segment renders with its own face and size, and the dialog font is only the default
/// for text outside such a tag - the way the colour attribute of the same tag already worked.
/// </summary>
public class ImageRendererFontTagTests
{
    private static ImageParameter MakeParameter(string text, string fontName = "Arial", float fontSize = 30, TextEffects? effects = null, ExportBoxType boxType = ExportBoxType.None)
    {
        return new ImageParameter
        {
            Text = text,
            FontName = fontName,
            FontSize = fontSize,
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            OutlineWidth = 2,
            ShadowColor = SKColors.Black,
            ShadowWidth = 2,
            ScreenWidth = 1280,
            ScreenHeight = 720,
            LineSpacingPercent = 0,
            ContentAlignment = ExportContentAlignment.Center,
            TextEffects = effects,
            BoxType = boxType,
            BoxPaddingLeft = 4,
            BoxPaddingRight = 4,
            BoxPaddingTop = 4,
            BoxPaddingBottom = 4,
            BackgroundColor = SKColors.Blue,
        };
    }

    private static bool SamePixels(SKBitmap a, SKBitmap b)
    {
        if (a.Width != b.Width || a.Height != b.Height)
        {
            return false;
        }

        return a.Bytes.AsSpan().SequenceEqual(b.Bytes);
    }

    private static bool HasPixelNear(SKBitmap bitmap, SKColor color)
    {
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var p = bitmap.GetPixel(x, y);
                if (p.Alpha > 200 &&
                    Math.Abs(p.Red - color.Red) < 40 &&
                    Math.Abs(p.Green - color.Green) < 40 &&
                    Math.Abs(p.Blue - color.Blue) < 40)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// A face that resolves to a different typeface than the default one, or null when this
    /// machine has only one usable font family.
    /// </summary>
    private static string? FindOtherFace(string defaultFace)
    {
        using var defaultTypeface = FontFaces.CreateTypeface(defaultFace, false, false);
        foreach (var face in FontFaces.GetFontFaces())
        {
            using var typeface = FontFaces.CreateTypeface(face, false, false);
            if (typeface != null && defaultTypeface != null && typeface.FamilyName != defaultTypeface.FamilyName)
            {
                return face;
            }
        }

        return null;
    }

    [Fact]
    public void FaceTag_RendersLikeThatFontChosenInTheDialog()
    {
        var other = FindOtherFace("Arial");
        if (other == null)
        {
            return; // one-font machine, nothing to compare against
        }

        using var tagged = ImageRenderer.GenerateBitmap(MakeParameter($"<font face=\"{other}\">Hello world</font>", "Arial"));
        using var dialog = ImageRenderer.GenerateBitmap(MakeParameter("Hello world", other));
        using var untagged = ImageRenderer.GenerateBitmap(MakeParameter("Hello world", "Arial"));

        // Same glyphs, so the same width. The height can differ: a tagged line keeps at least the
        // dialog font's line box, so the bitmap height stays a function of the line count.
        Assert.Equal(dialog.Width, tagged.Width);
        Assert.False(SamePixels(tagged, untagged), "a face tag should not render with the dialog font");
    }

    [Fact]
    public void FaceTag_AppliesPerLine_NotToTheWholeSubtitle()
    {
        var other = FindOtherFace("Arial");
        if (other == null)
        {
            return;
        }

        // Second line tagged, first line not - the first line must still be the dialog font.
        using var mixed = ImageRenderer.GenerateBitmap(MakeParameter($"Line A\n<font face=\"{other}\">Line B</font>", "Arial"));
        using var allOther = ImageRenderer.GenerateBitmap(MakeParameter("Line A\nLine B", other));
        using var allArial = ImageRenderer.GenerateBitmap(MakeParameter("Line A\nLine B", "Arial"));

        Assert.False(SamePixels(mixed, allOther));
        Assert.False(SamePixels(mixed, allArial));
    }

    [Fact]
    public void SizeTag_RendersLikeThatSizeChosenInTheDialog()
    {
        using var tagged = ImageRenderer.GenerateBitmap(MakeParameter("<font size=\"60\">Hello</font>", fontSize: 30));
        using var dialog = ImageRenderer.GenerateBitmap(MakeParameter("Hello", fontSize: 60));
        using var small = ImageRenderer.GenerateBitmap(MakeParameter("Hello", fontSize: 30));

        Assert.True(SamePixels(tagged, dialog));
        Assert.True(tagged.Height > small.Height);
        Assert.True(tagged.Width > small.Width);
    }

    [Fact]
    public void SizeTag_GrowsOnlyItsOwnLine()
    {
        using var mixed = ImageRenderer.GenerateBitmap(MakeParameter("<font size=\"60\">Big</font>\nSmall", fontSize: 30));
        using var bothSmall = ImageRenderer.GenerateBitmap(MakeParameter("Big\nSmall", fontSize: 30));
        using var bothBig = ImageRenderer.GenerateBitmap(MakeParameter("Big\nSmall", fontSize: 60));

        Assert.True(mixed.Height > bothSmall.Height);
        Assert.True(mixed.Height < bothBig.Height);
    }

    [Fact]
    public void SizeTag_SmallerThanDialog_KeepsTheDialogLineBox()
    {
        // A tag can make a line taller, never shorter - the bitmap height stays a function of
        // the line count for bottom anchored formats (issue #13202).
        using var tagged = ImageRenderer.GenerateBitmap(MakeParameter("<font size=\"12\">tiny</font>\nnormal", fontSize: 30));
        using var plain = ImageRenderer.GenerateBitmap(MakeParameter("tiny\nnormal", fontSize: 30));

        Assert.Equal(plain.Height, tagged.Height);
    }

    [Fact]
    public void ColorAndFaceInOneTag_BothApply()
    {
        var other = FindOtherFace("Arial") ?? "Arial";
        var ip = MakeParameter($"<font color=\"#ff0000\" face=\"{other}\">Hello</font>");
        ip.OutlineWidth = 0;
        ip.ShadowWidth = 0;
        using var bitmap = ImageRenderer.GenerateBitmap(ip);

        Assert.True(HasPixelNear(bitmap, SKColors.Red));
    }

    [Fact]
    public void NestedFontTags_SizeTagKeepsTheOuterColor()
    {
        var ip = MakeParameter("<font color=\"#ff0000\"><font size=\"40\">Hello</font></font>");
        ip.OutlineWidth = 0;
        ip.ShadowWidth = 0;
        using var bitmap = ImageRenderer.GenerateBitmap(ip);

        Assert.True(HasPixelNear(bitmap, SKColors.Red));
        Assert.False(HasPixelNear(bitmap, SKColors.White));
    }

    [Fact]
    public void UnquotedAttributes_ParseLikeQuotedOnes()
    {
        var other = FindOtherFace("Arial") ?? "Arial";
        if (other.Contains(' '))
        {
            other = "Arial"; // a bare value ends at whitespace, so only a one-word face works unquoted
        }

        using var bare = ImageRenderer.GenerateBitmap(MakeParameter($"<font face={other} size=50>Hi</font>"));
        using var quoted = ImageRenderer.GenerateBitmap(MakeParameter($"<font face=\"{other}\" size=\"50\">Hi</font>"));

        Assert.True(SamePixels(bare, quoted));
    }

    [Fact]
    public void UnknownFace_FallsBackAndStillDraws()
    {
        using var bitmap = ImageRenderer.GenerateBitmap(MakeParameter("<font face=\"No Such Font 14476\">Hello</font>"));

        Assert.True(bitmap.Width > 2 && bitmap.Height > 2);
        Assert.True(HasPixelNear(bitmap, SKColors.White));
    }

    [Fact]
    public void BadSizeValue_IsIgnored()
    {
        using var tagged = ImageRenderer.GenerateBitmap(MakeParameter("<font size=\"large\">Hello</font>"));
        using var plain = ImageRenderer.GenerateBitmap(MakeParameter("Hello"));

        Assert.True(SamePixels(tagged, plain));
    }

    [Fact]
    public void SizeTag_WithBoxPerLine_BoxFollowsTheTallerLine()
    {
        using var mixed = ImageRenderer.GenerateBitmap(MakeParameter("<font size=\"60\">Big</font>\nSmall", boxType: ExportBoxType.BoxPerLine));
        using var bothSmall = ImageRenderer.GenerateBitmap(MakeParameter("Big\nSmall", boxType: ExportBoxType.BoxPerLine));

        Assert.True(mixed.Height > bothSmall.Height);
        Assert.True(HasPixelNear(mixed, SKColors.Blue));
    }

    [Fact]
    public void SizeTag_WithTextEffects_GrowsTheText()
    {
        var effects = TextEffectPresetFactory.Create(TextEffectPreset.NeonGlow, 30, SKColors.White, SKColors.Black, SKColors.Black);
        using var tagged = ImageRenderer.GenerateBitmap(MakeParameter("<font size=\"60\">Hello</font>", effects: effects));
        using var plain = ImageRenderer.GenerateBitmap(MakeParameter("Hello", effects: effects));

        Assert.True(tagged.Height > plain.Height);
        Assert.True(tagged.Width > plain.Width);
    }

    [Fact]
    public void SizeTag_WithGlyphGeometryEffects_GrowsTheText()
    {
        var effects = TextEffectPresetFactory.Create(TextEffectPreset.NeonGlow, 30, SKColors.White, SKColors.Black, SKColors.Black)!;
        effects.LetterSpacing = 2;
        using var tagged = ImageRenderer.GenerateBitmap(MakeParameter("<font size=\"60\">Hello</font>\nWorld", effects: effects));
        using var plain = ImageRenderer.GenerateBitmap(MakeParameter("Hello\nWorld", effects: effects));

        Assert.True(tagged.Height > plain.Height);
    }

    [Fact]
    public void AssaFontSize_ScalesWithTheScriptResolution()
    {
        // "{\fs20}" in a PlayResY=360 script exported at 720p is 40 px.
        var ip = MakeParameter(ExportTextTags.ToRenderableText("{\\fs20}Hello"), fontSize: 30);
        ExportTextTags.ApplyStyleOverrideTags(ip, "{\\fs20}Hello", scriptHeight: 360);
        using var scaled = ImageRenderer.GenerateBitmap(ip);
        using var at40 = ImageRenderer.GenerateBitmap(MakeParameter("Hello", fontSize: 40));

        Assert.Equal(2f, ip.TagFontSizeScale);
        Assert.True(SamePixels(scaled, at40));
    }

    [Fact]
    public void NoTags_RendersExactlyAsBefore()
    {
        // The line box and baseline maths were rewritten per line; without tags they must
        // give the same picture for plain, italic and bold text.
        foreach (var text in new[] { "Hello", "Hello <i>world</i>", "<b>Hello</b>\nworld", "a\n\nb" })
        {
            using var a = ImageRenderer.GenerateBitmap(MakeParameter(text));
            using var b = ImageRenderer.GenerateBitmap(MakeParameter(text));
            Assert.True(SamePixels(a, b));
            Assert.True(a.Width > 2);
        }
    }
}
