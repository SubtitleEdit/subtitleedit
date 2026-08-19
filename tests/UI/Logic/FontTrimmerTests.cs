using HarfBuzzSharp;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System.Runtime.InteropServices;
using Buffer = HarfBuzzSharp.Buffer;
using Font = HarfBuzzSharp.Font;

namespace UITests.Logic;

/// <summary>
/// Tests for the font trimming behind "trim fonts to used characters" (attachments window,
/// font collector and batch convert). The fixture font <c>SeTrimTest.ttf</c> is an original
/// font built for these tests with glyph order
/// <c>.notdef(0), space(1), A(2), B(3), C_lig(4), D(5), E(6), F(7)</c>, where a GSUB 'liga'
/// feature substitutes "AB" with C_lig and E is a composite glyph referencing D.
/// </summary>
public class FontTrimmerTests
{
    private static byte[] LoadFixture(string name) =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Files", name));

    private static (List<uint> Glyphs, List<int> Advances) Shape(byte[] fontBytes, string text)
    {
        var handle = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
        using var blob = new Blob(handle.AddrOfPinnedObject(), fontBytes.Length, MemoryMode.ReadOnly, () => handle.Free());
        using var face = new Face(blob, 0);
        using var font = new Font(face);
        using var buffer = new Buffer();
        buffer.AddUtf16(text);
        buffer.GuessSegmentProperties();
        font.Shape(buffer);

        var glyphs = new List<uint>();
        foreach (var info in buffer.GlyphInfos)
        {
            glyphs.Add(info.Codepoint);
        }

        var advances = new List<int>();
        foreach (var position in buffer.GlyphPositions)
        {
            advances.Add(position.XAdvance);
        }

        return (glyphs, advances);
    }

    private static void AssertShapesEqual(byte[] originalFont, byte[] trimmedFont, string text)
    {
        var (originalGlyphs, originalAdvances) = Shape(originalFont, text);
        var (trimmedGlyphs, trimmedAdvances) = Shape(trimmedFont, text);
        Assert.Equal(originalGlyphs, trimmedGlyphs);
        Assert.Equal(originalAdvances, trimmedAdvances);
    }

    /// <summary>True when the glyph still has an outline (a trimmed-away glyph has none).</summary>
    private static bool HasInk(byte[] fontBytes, char character)
    {
        var handle = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
        using var blob = new Blob(handle.AddrOfPinnedObject(), fontBytes.Length, MemoryMode.ReadOnly, () => handle.Free());
        using var face = new Face(blob, 0);
        using var font = new Font(face);
        Assert.True(font.TryGetGlyph(character, out var glyph)); // cmap must stay intact
        return font.TryGetGlyphExtents(glyph, out var extents) && (extents.Width != 0 || extents.Height != 0);
    }

    [Fact]
    public void Trim_KeepsUsedAndLigatureGlyphs_EmptiesTheRest()
    {
        var original = LoadFixture("SeTrimTest.ttf");
        var result = FontTrimmer.Trim(original, new[] { "AB" });

        Assert.True(result.Trimmed);
        Assert.Equal(FontTrimmer.TrimSkipReason.None, result.SkipReason);
        Assert.Equal(8, result.TotalGlyphs);
        // .notdef + A + B + the shaped "AB" -> C_lig ligature
        Assert.Equal(4, result.KeptGlyphs);
        Assert.True(result.Bytes.Length < original.Length);

        // "AB" must shape exactly as before, to the C_lig ligature (same ids, same advances)
        Assert.Equal(new uint[] { 4 }, Shape(original, "AB").Glyphs);
        AssertShapesEqual(original, result.Bytes, "AB");

        // kept glyphs still have outlines, trimmed glyphs are empty - but still mapped
        Assert.True(HasInk(result.Bytes, 'A'));
        Assert.True(HasInk(result.Bytes, 'B'));
        Assert.True(HasInk(original, 'F'));
        Assert.False(HasInk(result.Bytes, 'F'));
        Assert.False(HasInk(result.Bytes, 'D'));
    }

    // A lone surrogate (corrupt UTF-16 input) must not abort the trim - the invalid unit
    // is skipped and the rest of the line still drives the used-glyph set.
    [Theory]
    [InlineData("AB\uD800")]
    [InlineData("\uDC00AB")]
    [InlineData("A\uD800B")]
    public void Trim_LoneSurrogateInTextIsIgnored(string usedLine)
    {
        var original = LoadFixture("SeTrimTest.ttf");
        var result = FontTrimmer.Trim(original, new[] { usedLine });

        Assert.True(result.Trimmed);
        Assert.Equal(FontTrimmer.TrimSkipReason.None, result.SkipReason);
        Assert.True(HasInk(result.Bytes, 'A'));
        Assert.True(HasInk(result.Bytes, 'B'));
        Assert.False(HasInk(result.Bytes, 'F'));
    }

    [Fact]
    public void Trim_KeepsComponentsOfUsedCompositeGlyphs()
    {
        var original = LoadFixture("SeTrimTest.ttf");

        // E is a composite referencing D; using only "E" must keep D's outline alive.
        var result = FontTrimmer.Trim(original, new[] { "E" });

        Assert.True(result.Trimmed);
        Assert.True(HasInk(result.Bytes, 'E'));
        Assert.False(HasInk(result.Bytes, 'F'));
        AssertShapesEqual(original, result.Bytes, "E");
    }

    [Fact]
    public void Trim_ResultIsAValidFontForSkia()
    {
        var result = FontTrimmer.Trim(LoadFixture("SeTrimTest.ttf"), new[] { "AB" });

        using var typeface = SKTypeface.FromData(SKData.CreateCopy(result.Bytes));
        Assert.NotNull(typeface);
        Assert.Equal("SeTrimTest", typeface.FamilyName);
    }

    [Fact]
    public void Trim_TwiceIsNoSavings()
    {
        var once = FontTrimmer.Trim(LoadFixture("SeTrimTest.ttf"), new[] { "AB" });
        Assert.True(once.Trimmed);

        var twice = FontTrimmer.Trim(once.Bytes, new[] { "AB" });
        Assert.False(twice.Trimmed);
        Assert.Equal(FontTrimmer.TrimSkipReason.NoSavings, twice.SkipReason);
        Assert.Same(once.Bytes, twice.Bytes);
    }

    [Fact]
    public void Trim_CffFontIsSkipped()
    {
        var original = LoadFixture("SeTrimTestCff.otf");
        var result = FontTrimmer.Trim(original, new[] { "A" });

        Assert.False(result.Trimmed);
        Assert.Equal(FontTrimmer.TrimSkipReason.NotTrueType, result.SkipReason);
        Assert.Same(original, result.Bytes);
    }

    [Fact]
    public void Trim_FontCollectionIsSkipped()
    {
        var bytes = new byte[] { (byte)'t', (byte)'t', (byte)'c', (byte)'f', 0, 1, 0, 0, 0, 0, 0, 2 };
        var result = FontTrimmer.Trim(bytes, new[] { "A" });

        Assert.False(result.Trimmed);
        Assert.Equal(FontTrimmer.TrimSkipReason.FontCollection, result.SkipReason);
    }

    [Fact]
    public void Trim_GarbageIsSkipped()
    {
        var result = FontTrimmer.Trim(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, new[] { "A" });

        Assert.False(result.Trimmed);
        Assert.Equal(FontTrimmer.TrimSkipReason.CouldNotParse, result.SkipReason);
    }

    [Fact]
    public void SplitToScriptRuns_SplitsMixedScripts_KeepsJoinersAndMarks()
    {
        Assert.Equal(new[] { "Hello" }, FontTrimmer.SplitToScriptRuns("Hello"));
        Assert.Equal(new[] { "Hello ", "مرحبا" }, FontTrimmer.SplitToScriptRuns("Hello مرحبا"));
        // ZWJ (200D) must stay inside the Arabic run, combining acute inside the Latin run
        Assert.Equal(new[] { "é ", "با‍ب" }, FontTrimmer.SplitToScriptRuns("é با‍ب"));
        Assert.Equal(new[] { "क्क ", "abc" }, FontTrimmer.SplitToScriptRuns("क्क abc"));
    }

    [Fact]
    public void GetUsedTextLines_StripsTagsAndSplitsLines()
    {
        var text =
            "[Script Info]\r\nScriptType: v4.00+\r\n\r\n" +
            "[V4+ Styles]\r\n" +
            "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\r\n" +
            "Style: Default,Arial,20,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,2,2,10,10,10,1\r\n\r\n" +
            "[Events]\r\n" +
            "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\r\n" +
            "Dialogue: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,{\\an8\\fnFoo}Hello\\Nworld\r\n" +
            "Dialogue: 0,0:00:04.00,0:00:06.00,Default,,0,0,0,,Hello\r\n" +
            "Dialogue: 0,0:00:07.00,0:00:09.00,Default,,0,0,0,,{\\p1}m 0 0 l 100 0{\\p0}\r\n";
        var subtitle = new Subtitle();
        new AdvancedSubStationAlpha().LoadSubtitle(subtitle, text.SplitToLines(), string.Empty);

        var lines = AssaFontEmbedder.GetUsedTextLines(subtitle);

        Assert.Contains("Hello", lines);
        Assert.Contains("world", lines);
        Assert.Equal(lines.Count, lines.Distinct().Count()); // "Hello" only once
        Assert.DoesNotContain(lines, l => l.Contains('\\') || l.Contains('{'));
        Assert.DoesNotContain(lines, l => l.Contains("m 0 0")); // drawing commands are not text
    }

    /// <summary>
    /// Sanity check against a real installed font when one is available (skipped silently
    /// on machines without any glyf-flavored .ttf) - the fixture font is tiny, a real font
    /// exercises long loca offsets, hinting instructions and big cmaps.
    /// </summary>
    [Fact]
    public void Trim_RealSystemFontShapesIdenticallyAndShrinks()
    {
        foreach (var file in FindSystemTtfFiles().Take(20))
        {
            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(file);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            var lines = new[] { "Hello World!", "The quick brown fox 0123456789", "office flift" };
            if (Shape(bytes, lines[0]).Glyphs.Contains(0u))
            {
                continue; // a font without Latin coverage proves nothing here
            }

            var result = FontTrimmer.Trim(bytes, lines);
            if (!result.Trimmed)
            {
                continue; // CFF, collection, bitmap-only... find another candidate
            }

            Assert.True(result.Bytes.Length < bytes.Length);
            Assert.True(result.KeptGlyphs < result.TotalGlyphs);
            foreach (var line in lines)
            {
                AssertShapesEqual(bytes, result.Bytes, line);
            }

            using var typeface = SKTypeface.FromData(SKData.CreateCopy(result.Bytes));
            Assert.NotNull(typeface);
            return; // one successfully trimmed real font is enough
        }
    }

    private static IEnumerable<string> FindSystemTtfFiles()
    {
        var folders = new List<string>();
        if (OperatingSystem.IsWindows())
        {
            folders.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts"));
        }
        else if (OperatingSystem.IsMacOS())
        {
            folders.Add("/System/Library/Fonts/Supplemental");
            folders.Add("/Library/Fonts");
        }
        else
        {
            folders.Add("/usr/share/fonts");
        }

        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                continue;
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(folder, "*.ttf", SearchOption.AllDirectories);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var file in files)
            {
                yield return file;
            }
        }
    }
}
