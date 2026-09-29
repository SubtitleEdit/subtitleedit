using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// A PAC text line can be flagged "secondary code page" (alignment bit 0x08). The file was
/// treated as secondary-code-page-first when ANY line had the flag, and then every other line
/// was decoded as Latin - one flagged line (or an 0xFE time code byte mistaken for a line
/// marker) turned a whole Greek or Cyrillic file into Latin gibberish.
/// </summary>
public class PacSecondaryCodePageTest
{
    private static readonly string[] GreekLines =
    {
        "Γεια σου, τι κάνεις;",
        "Είναι καλά.",
        "Αυτό μου αρέσει.",
        "Πού είναι το σπίτι;",
        "Ευχαριστώ πολύ.",
    };

    private static byte[] SaveGreek()
    {
        var subtitle = new Subtitle();
        var start = 1000;
        foreach (var line in GreekLines)
        {
            subtitle.Paragraphs.Add(new Paragraph(line, start, start + 2000));
            start += 3000;
        }

        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pac");
        try
        {
            new Pac { BatchMode = true, CodePage = Pac.CodePageGreek }.Save(path, subtitle);
            return File.ReadAllBytes(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static Subtitle LoadGreek(byte[] bytes)
    {
        var subtitle = new Subtitle();
        new Pac { BatchMode = true, CodePage = Pac.CodePageGreek }.LoadSubtitle(subtitle, bytes);
        return subtitle;
    }

    /// <summary>Offsets of the alignment byte after each text line marker (0xFE, alignment, 0x03).</summary>
    private static List<int> LineAlignmentOffsets(byte[] bytes)
    {
        var offsets = new List<int>();
        for (var i = 0; i < bytes.Length - 2; i++)
        {
            if (bytes[i] == 0xFE && bytes[i + 2] == 0x03 && (bytes[i + 1] & 0x08) == 0)
            {
                offsets.Add(i + 1);
            }
        }

        return offsets;
    }

    [Fact]
    public void OneSecondaryLineDoesNotTurnTheFileLatin()
    {
        var bytes = SaveGreek();
        Assert.Equal(GreekLines, LoadGreek(bytes).Paragraphs.Select(p => p.Text));

        var offsets = LineAlignmentOffsets(bytes);
        Assert.True(offsets.Count >= GreekLines.Length);
        bytes[offsets[offsets.Count - 1]] |= 0x08; // flag the last line only

        var loaded = LoadGreek(bytes);

        Assert.Equal(GreekLines.Take(GreekLines.Length - 1), loaded.Paragraphs.Take(GreekLines.Length - 1).Select(p => p.Text));
    }

    /// <summary>When most lines are flagged, the flagged lines are the main text - as before.</summary>
    [Fact]
    public void MostlySecondaryLinesKeepTheCodePageForTheFlaggedLines()
    {
        var bytes = SaveGreek();
        foreach (var offset in LineAlignmentOffsets(bytes))
        {
            bytes[offset] |= 0x08;
        }

        var loaded = LoadGreek(bytes);

        Assert.Equal(GreekLines, loaded.Paragraphs.Select(p => p.Text));
    }

    /// <summary>Hebrew subtitles with a Russian translation line each - two scripts, two code pages.</summary>
    private static readonly string[] HebrewRussian =
    {
        "אתה בסדר?" + Environment.NewLine + "Да, это меня.",
        "הוא יודע טוב." + Environment.NewLine + "Нет, я не знаю.",
        "אולי הוא יודע." + Environment.NewLine + "Он как всё за нас.",
        "אתה בסדר, אולי." + Environment.NewLine + "Да, я знаю.",
    };

    private static byte[] SavePac(string[] texts, int codePage, int secondaryCodePage)
    {
        var subtitle = new Subtitle();
        var start = 1000;
        foreach (var text in texts)
        {
            subtitle.Paragraphs.Add(new Paragraph(text, start, start + 2000));
            start += 3000;
        }

        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pac");
        try
        {
            new Pac { BatchMode = true, CodePage = codePage, SecondaryCodePage = secondaryCodePage }.Save(path, subtitle);
            return File.ReadAllBytes(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SecondaryCodePageRoundTripsWithExplicitCodePages()
    {
        var bytes = SavePac(HebrewRussian, Pac.CodePageHebrew, Pac.CodePageCyrillic);

        var loaded = new Subtitle();
        new Pac { CodePage = Pac.CodePageHebrew, SecondaryCodePage = Pac.CodePageCyrillic }.LoadSubtitle(loaded, bytes);

        Assert.Equal(HebrewRussian, loaded.Paragraphs.Select(p => p.Text));
    }

    /// <summary>Opening detects both: Hebrew for the plain lines, Cyrillic for the flagged ones.</summary>
    [Fact]
    public void SecondaryCodePageIsDetectedWhenOpening()
    {
        var bytes = SavePac(HebrewRussian, Pac.CodePageHebrew, Pac.CodePageCyrillic);

        var pac = new Pac { BatchMode = true };
        var loaded = new Subtitle();
        pac.LoadSubtitle(loaded, bytes);

        Assert.Equal(Pac.CodePageHebrew, pac.CodePage);
        Assert.Equal(HebrewRussian, loaded.Paragraphs.Select(p => p.Text));
    }

    /// <summary>
    /// A flagged line in the same script as the rest (e.g. a large-font line) keeps the code page
    /// the caller set - it was decoded with a "detected" secondary code page ("Γιάννη" as "Ciámmg").
    /// </summary>
    [Theory]
    [InlineData(Pac.CodePageGreek, new[] { "Δεν ξέρω τι εννοείς.", "Πρέπει να πάμε σπίτι τώρα.", "Γιάννη" })]
    [InlineData(Pac.CodePageLatin, new[] { "I don't know what you mean.", "We have to go home now.", "OK." })]
    public void FlaggedLineUsesExplicitCodePage(int codePage, string[] lines)
    {
        var bytes = SavePac(lines, codePage, -1);
        var offsets = new List<int>();
        for (var i = 0; i < bytes.Length - 2; i++)
        {
            if (bytes[i] == 0xFE && bytes[i + 2] == 0x03)
            {
                offsets.Add(i + 1);
            }
        }

        Assert.Equal(lines.Length, offsets.Count);
        bytes[offsets[offsets.Count - 1]] |= 0x08; // flag the last line only

        var loaded = new Subtitle();
        new Pac { CodePage = codePage }.LoadSubtitle(loaded, bytes);

        Assert.Equal(lines, loaded.Paragraphs.Select(p => p.Text));
    }

    [Fact]
    public void OnlyLinesThatFitTheSecondaryCodePageAreFlagged()
    {
        var bytes = SavePac(HebrewRussian, Pac.CodePageHebrew, Pac.CodePageCyrillic);

        // every line marker (0xFE, alignment, 0x03): the Hebrew lines plain, the Russian ones flagged
        var flags = new List<bool>();
        for (var i = 0; i < bytes.Length - 2; i++)
        {
            if (bytes[i] == 0xFE && bytes[i + 2] == 0x03)
            {
                flags.Add((bytes[i + 1] & 0x08) != 0);
            }
        }

        Assert.Equal(Enumerable.Range(0, HebrewRussian.Length * 2).Select(i => i % 2 == 1), flags);
    }

    /// <summary>No secondary code page - the bytes are exactly what they were before the option existed.</summary>
    [Fact]
    public void WithoutSecondaryCodePageNothingIsFlagged()
    {
        var withoutOption = SavePac(HebrewRussian, Pac.CodePageHebrew, -1);
        var sameAsPrimary = SavePac(HebrewRussian, Pac.CodePageHebrew, Pac.CodePageHebrew);

        Assert.Equal(withoutOption, sameAsPrimary);
        Assert.DoesNotContain(Enumerable.Range(0, withoutOption.Length - 2), i => withoutOption[i] == 0xFE && withoutOption[i + 2] == 0x03 && (withoutOption[i + 1] & 0x08) != 0);
    }
}
