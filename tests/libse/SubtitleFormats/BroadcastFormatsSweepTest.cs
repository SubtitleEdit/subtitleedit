using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Round-trip regression tests from the broadcast format sweep: each test covers a
/// write→read defect found by saving a battery of subtitles with every broadcast
/// format and reading them back.
/// </summary>
public class BroadcastFormatsSweepTest
{
    private static Subtitle SaveAndReloadBinary(IBinaryPersistableSubtitle format, Subtitle subtitle, string extension = ".bin")
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + extension);
        try
        {
            using (var fs = File.Create(path))
            {
                format.Save(path, fs, subtitle, batchMode: true);
            }

            var loaded = new Subtitle();
            ((SubtitleFormat)format).LoadSubtitle(loaded, new List<string>(), path);
            return loaded;
        }
        finally
        {
            File.Delete(path);
        }
    }

    // The reader's record-bounds check used '<' where the last record ends exactly at the
    // end of file, so the final cue of every Ayato file was dropped.
    [Fact]
    public void AyatoKeepsLastCue()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("First cue.", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("Second cue.", 4000, 6000));
        subtitle.Paragraphs.Add(new Paragraph("Last cue.", 7000, 9000));

        var loaded = SaveAndReloadBinary(new Ayato(), subtitle, extension: ".aya");

        Assert.Equal(3, loaded.Paragraphs.Count);
        Assert.Equal("Last cue.", loaded.Paragraphs[2].Text);
    }

    // A zero time code normally marks a header placeholder, but a real cue can start at
    // 00:00:00:00 - the reader silently dropped it.
    [Fact]
    public void CapMakerPlusKeepsCueAtTimeZero()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Cue at time zero.", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("Second cue.", 2000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("Third cue.", 4000, 5000));

        var loaded = SaveAndReloadBinary(new CapMakerPlus(), subtitle, extension: ".cap");

        Assert.Equal(3, loaded.Paragraphs.Count);
        Assert.Equal("Cue at time zero.", loaded.Paragraphs[0].Text);
        Assert.Equal(0, loaded.Paragraphs[0].StartTime.TotalMilliseconds, 3);
    }

    private static Subtitle CheetahSaveAndReload(Subtitle subtitle)
    {
        var old = Configuration.Settings.SubtitleSettings.CheetahCaptionAlwayWriteEndTime;
        Configuration.Settings.SubtitleSettings.CheetahCaptionAlwayWriteEndTime = true;
        try
        {
            return SaveAndReloadBinary(new CheetahCaption(), subtitle, extension: ".cap");
        }
        finally
        {
            Configuration.Settings.SubtitleSettings.CheetahCaptionAlwayWriteEndTime = old;
        }
    }

    // Characters without a code in the Cheetah charset were written as raw cp1252 bytes,
    // which the reader either drops (>= 0xC0) or maps to the wrong character via the
    // special-character table ('…' is 0x85 = 'ó').
    [Fact]
    public void CheetahCaptionTransliteratesUnsupportedCharacters()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Über schön: äöü ÄÖÜ ß.", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("Æblegrød på Ærø.", 4000, 6000));
        subtitle.Paragraphs.Add(new Paragraph("Almost done…", 7000, 9000));

        var loaded = CheetahSaveAndReload(subtitle);

        Assert.Equal(3, loaded.Paragraphs.Count);
        Assert.Equal("Uber schon: aou AOU ss.", loaded.Paragraphs[0].Text);
        Assert.Equal("AEblegrod pa AEro.", loaded.Paragraphs[1].Text);
        Assert.Equal("Almost done...", loaded.Paragraphs[2].Text);
    }

    // Cheetah charset characters (music note, accented vowels, £) must still round-trip.
    [Fact]
    public void CheetahCaptionCharsetRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("♪ Café niño ça £5 ½ ® ¿qué?", 1000, 3000));

        var loaded = CheetahSaveAndReload(subtitle);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("♪ Café niño ça £5 ½ ® ¿qué?", loaded.Paragraphs[0].Text);
    }

    // The 0x1E right-align style byte sits where the reader's back-scan may pull it into
    // the text run; 0x15-0x1F must be treated as style bytes, not cp1252 text.
    [Fact]
    public void CheetahCaptionRightAlignDoesNotLeakControlBytes()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("{\\an3}Bottom right text.", 1000, 3000));

        var loaded = CheetahSaveAndReload(subtitle);

        Assert.Single(loaded.Paragraphs);
        foreach (var ch in loaded.Paragraphs[0].Text)
        {
            Assert.True(ch >= 0x20, $"Control character 0x{(int)ch:X2} leaked into text");
        }
    }

    private static Subtitle Cavena890SaveAndReload(Subtitle subtitle, int languageId)
    {
        var oldLanguageId = Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId;
        Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = languageId;
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".890");
        try
        {
            new Cavena890().Save(path, subtitle, batchMode: true);
            var loaded = new Subtitle();
            new Cavena890().LoadSubtitle(loaded, new List<string>(), path);
            return loaded;
        }
        finally
        {
            Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = oldLanguageId;
            File.Delete(path);
        }
    }

    // 'å' was written as 0x1D and decoded correctly - and then the raw-byte replace for
    // 0xE5 ('[') ran on the decoded string and turned every 'å' into '['.
    [Fact]
    public void Cavena890LatinScandinavianRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Æblegrød på Ærø, øl så øm.", 1000, 3000));

        var loaded = Cavena890SaveAndReload(subtitle, 0);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("Æblegrød på Ærø, øl så øm.", loaded.Paragraphs[0].Text);
    }

    // The latin fallback encoder used Encoding.Default (UTF-8 on .NET Core) and wrote only
    // the first UTF-8 byte - ß became 'Ã', ç became 'Ã', £ became 'Â'.
    [Fact]
    public void Cavena890LatinFallbackCharactersRoundTrip()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Garçon, ça va? ß £5 ♪", 1000, 3000));

        var loaded = Cavena890SaveAndReload(subtitle, 0);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("Garçon, ça va? ß £5 ♪", loaded.Paragraphs[0].Text);
    }

    // An <i> spanning both lines was written as a bare marker on line one only; the reader
    // auto-closes per line, leaving a stray "</i>" on line two.
    [Fact]
    public void Cavena890TwoLineItalicRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("<i>Two italic lines" + Environment.NewLine + "both of them.</i>", 1000, 3000));

        var loaded = Cavena890SaveAndReload(subtitle, 0);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("<i>Two italic lines</i>" + Environment.NewLine + "<i>both of them.</i>", loaded.Paragraphs[0].Text);
    }

    // There was no Russian encoder at all - Russian text was written as the first bytes of
    // its UTF-8 sequences and read back as mojibake.
    [Fact]
    public void Cavena890RussianRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Привет, как дела?", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("Это тестовый субтитр.", 4000, 6000));

        var loaded = Cavena890SaveAndReload(subtitle, Cavena890.LanguageIdRussian);

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("Привет, как дела?", loaded.Paragraphs[0].Text);
        Assert.Equal("Это тестовый субтитр.", loaded.Paragraphs[1].Text);
    }

    [Fact]
    public void Cavena890ArabicRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("مرحبا كيف حالك", 1000, 3000));

        var loaded = Cavena890SaveAndReload(subtitle, Cavena890.LanguageIdArabic);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("مرحبا كيف حالك", loaded.Paragraphs[0].Text);
    }

    [Fact]
    public void Cavena890GreekRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Γεια σου, τι κάνεις;", 1000, 3000));

        var loaded = Cavena890SaveAndReload(subtitle, Cavena890.LanguageIdGreek);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("Γεια σου, τι κάνεις;", loaded.Paragraphs[0].Text);
    }

    private static Subtitle PacSaveAndReload(Subtitle subtitle, int codePage)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pac");
        try
        {
            var pac = new Pac { BatchMode = true, CodePage = codePage };
            pac.Save(path, subtitle);
            var loaded = new Subtitle();
            var pacIn = new Pac { BatchMode = true, CodePage = codePage };
            pacIn.LoadSubtitle(loaded, new List<string>(), path);
            return loaded;
        }
        finally
        {
            File.Delete(path);
        }
    }

    // The Hebrew code table had a duplicate mapping for '.' whose byte (0x2B) the decoder
    // reads as '+' via its ASCII fast path, so every Hebrew period became '+'.
    [Fact]
    public void PacHebrewPeriodRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("זהו מבחן כתוביות.", 1000, 3000));

        var loaded = PacSaveAndReload(subtitle, Pac.CodePageHebrew);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("זהו מבחן כתוביות.", loaded.Paragraphs[0].Text);
    }

    // The W16 writer emitted bare single-byte ASCII inside double-byte text, shifting the
    // byte pairing after every odd-length ASCII run (e.g. the space between Hangul words).
    [Fact]
    public void PacKoreanWithSpacesRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("안녕하세요, 어떻게 지내세요?", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("이것은 자막 테스트입니다.", 4000, 6000));

        var loaded = PacSaveAndReload(subtitle, Pac.CodePageKorean);

        Assert.Equal(2, loaded.Paragraphs.Count);
        Assert.Equal("안녕하세요, 어떻게 지내세요?", loaded.Paragraphs[0].Text);
        Assert.Equal("이것은 자막 테스트입니다.", loaded.Paragraphs[1].Text);
    }

    private static Subtitle SccSaveAndReload(Subtitle subtitle)
    {
        var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
        Configuration.Settings.General.CurrentFrameRate = 29.97;
        try
        {
            var format = new ScenaristClosedCaptions();
            var text = format.ToText(subtitle, "test");
            var loaded = new Subtitle();
            format.LoadSubtitle(loaded, text.SplitToLines(), null);
            return loaded;
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
        }
    }

    // CEA-608 has ß in the extended charset (0x13 0x34); the decode table had an empty
    // string in that slot, making ß unencodable and undecodable.
    [Fact]
    public void SccSharpSRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Über schön: äöü ÄÖÜ ß.", 1000, 3000));

        var loaded = SccSaveAndReload(subtitle);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("Über schön: äöü ÄÖÜ ß.", loaded.Paragraphs[0].Text);
    }

    // Characters CEA-608 cannot encode were written as spaces and vanished - they are now
    // transliterated instead.
    [Fact]
    public void SccTransliteratesUnsupportedCharacters()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Æblegrød… fine", 1000, 3000));

        var loaded = SccSaveAndReload(subtitle);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("AEblegrød... fine", loaded.Paragraphs[0].Text);
    }

    // Tab Offset codes (97a1/97a2/9723) are positioning-only; decoding them as spaces put a
    // spurious leading space on centered italic continuation lines.
    [Fact]
    public void SccTwoLineItalicHasNoSpuriousSpace()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("<i>Two italic lines" + Environment.NewLine + "both of them.</i>", 1000, 3000));

        var loaded = SccSaveAndReload(subtitle);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("<i>Two italic lines" + Environment.NewLine + "both of them.</i>", loaded.Paragraphs[0].Text);
    }

    // Right alignment was lost whenever line length modulo 4 left a 2-3 column tab offset,
    // because the tab offset was not counted into the x position used for detection.
    [Fact]
    public void SccRightAlignmentRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("{\\an3}Bottom right text.", 1000, 3000));

        var loaded = SccSaveAndReload(subtitle);

        Assert.Single(loaded.Paragraphs);
        Assert.StartsWith("{\\an3}", loaded.Paragraphs[0].Text);
    }

    private static Subtitle MacCaptionSaveAndReload(Subtitle subtitle)
    {
        var format = new MacCaption10();
        var text = format.ToText(subtitle, "test");
        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, text.SplitToLines(), null);
        return loaded;
    }

    // The G1 table mapped bytes 0xA2/0xA3/0xA5/0xAC/0xAF to fullwidth forms (￡ etc.), so
    // '£' failed the reverse lookup on write and was dropped.
    [Fact]
    public void MacCaptionPoundSignRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Fish & chips for £5.", 1000, 3000));

        var loaded = MacCaptionSaveAndReload(subtitle);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("Fish & chips for £5.", loaded.Paragraphs[0].Text);
    }

    // '…' lives in the CEA-708 G2 set behind the EXT1 prefix; the EXT1 decode had an
    // operator precedence bug and no G2 table, and the encoder dropped G2 characters.
    [Fact]
    public void MacCaptionEllipsisRoundTrips()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Almost done…", 1000, 3000));

        var loaded = MacCaptionSaveAndReload(subtitle);

        Assert.Single(loaded.Paragraphs);
        Assert.Equal("Almost done…", loaded.Paragraphs[0].Text);
    }

    // An <i> opened on line one lost its italics on line two because the segment splitter
    // reset its tag state on every line.
    [Fact]
    public void EbuTtDTwoLineItalicKeepsSecondLineItalic()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("<i>Two italic lines" + Environment.NewLine + "both of them.</i>", 1000, 3000));

        var format = new EbuTtD();
        var text = format.ToText(subtitle, "test");
        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, text.SplitToLines(), null);

        Assert.Single(loaded.Paragraphs);
        var normalized = loaded.Paragraphs[0].Text.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);
        Assert.Equal("<i>Two italic lines" + Environment.NewLine + "both of them.</i>", normalized);
    }
}
