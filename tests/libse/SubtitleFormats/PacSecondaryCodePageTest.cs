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
}
