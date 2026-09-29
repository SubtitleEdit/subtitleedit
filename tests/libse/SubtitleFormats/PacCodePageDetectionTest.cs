using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// A PAC file does not say which code page its text uses, so it is detected: from the "Lang:"
/// story header when there is one, else by decoding with each code page and checking the language.
/// </summary>
public class PacCodePageDetectionTest
{
    public static IEnumerable<object[]> Languages => new List<object[]>
    {
        new object[] { Pac.CodePageLatin, new[] { "Hej, jeg kommer tilbage.", "Måske var han der, men jeg vidste det ikke.", "Undskyld, det er længere væk." } },
        new object[] { Pac.CodePageLatinTurkish, new[] { "Tamam, benim için daha önce.", "Hayır, bu benim.", "Tamam, daha sonra." } },
        new object[] { Pac.CodePageCyrillic, new[] { "Да, это меня.", "Нет, я не знаю.", "Он как всё за нас." } },
        new object[] { Pac.CodePageGreek, new[] { "Είναι καλά.", "Αυτό μου αρέσει.", "Είναι καλά, ξερεις." } },
        new object[] { Pac.CodePageHebrew, new[] { "אתה בסדר?", "הוא יודע טוב.", "אולי הוא יודע." } },
        new object[] { Pac.CodePageArabic, new[] { "هل أنت بخير؟", "لا، أنا في البيت.", "ماذا هذا؟" } },
        new object[] { Pac.CodePageThai, new[] { "เพื่อน เข้าใจมั้ย", "คุณตำรวจ ผู้กอง", "เพื่อน คุณตำรวจ" } },
        new object[] { Pac.CodePageKorean, new[] { "엄마, 괜찮아?", "그리고 우리가 거야.", "하지만 뭐라고 있어요." } },
        new object[] { Pac.CodePageChineseSimplified, new[] { "你好吗？我是你的朋友。", "好的，谢谢。", "走吧，晚上好。" } },
        new object[] { Pac.CodePageJapanese, new[] { "シンジ、だいじょうぶ？", "ありがとう、シュン。", "インチキだ！" } },
    };

    private static string SavePac(Subtitle subtitle, int codePage, string extension = ".pac")
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + extension);
        new Pac { BatchMode = true, CodePage = codePage }.Save(path, subtitle);
        return path;
    }

    private static Subtitle MakeSubtitle(IEnumerable<string> lines)
    {
        var subtitle = new Subtitle();
        var start = 1000;
        foreach (var line in lines)
        {
            subtitle.Paragraphs.Add(new Paragraph(line, start, start + 2000));
            start += 3000;
        }

        return subtitle;
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void BatchModeDetectsCodePage(int codePage, string[] lines)
    {
        var path = SavePac(MakeSubtitle(lines), codePage);
        try
        {
            Assert.Equal(codePage, Pac.AutoDetectEncoding(path));

            var pac = new Pac { BatchMode = true };
            var loaded = new Subtitle();
            pac.LoadSubtitle(loaded, null, path);
            Assert.Equal(codePage, pac.CodePage);
            Assert.Equal(lines[0], loaded.Paragraphs[0].Text);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Subtitle Edit 5 registers no code page picker, and opening a PAC decoded every file with
    /// the fallback Latin (Czech) code page - "skæbne" came out as "skćbne", Chinese as Big5.
    /// </summary>
    [Theory]
    [MemberData(nameof(Languages))]
    public void OpenWithoutCodePagePickerDetectsCodePage(int codePage, string[] lines)
    {
        Assert.Null(Pac.GetPacEncodingImplementation);
        var path = SavePac(MakeSubtitle(lines), codePage);
        try
        {
            var pac = new Pac();
            var loaded = new Subtitle();
            pac.LoadSubtitle(loaded, null, path);
            Assert.Equal(codePage, pac.CodePage);
            Assert.Equal(lines[0], loaded.Paragraphs[0].Text);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>The "Lang:" header decides - Screen's code, e.g. SIM = Simplified Chinese.</summary>
    [Fact]
    public void LanguageHeaderDecides()
    {
        var path = SavePac(MakeSubtitle(new[] { "Lang:SIM", "Hello there.", "How are you?" }), Pac.CodePageLatin);
        try
        {
            Assert.Equal(Pac.CodePageChineseSimplified, Pac.AutoDetectEncoding(File.ReadAllBytes(path)));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Icelandic has no keyword list match in the Latin language set before, and read with the
    /// Greek table (which remaps ASCII letters) it "detected" as Greek.
    /// </summary>
    [Fact]
    public void IcelandicIsNotDetectedAsGreek()
    {
        var lines = new[] { "Góðan daginn, frú.", "Áttu herbergi við ströndina?", "Við erum hér." };
        var path = SavePac(MakeSubtitle(lines), Pac.CodePageLatin);
        try
        {
            Assert.Equal(Pac.CodePageLatin, Pac.AutoDetectEncoding(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Ordinary Greek sentences have few Greek keyword hits, and read as Latin they got a couple
    /// of random Estonian word hits - the Greek decoding (all Greek letters) must win.
    /// </summary>
    [Fact]
    public void GreekSentencesAreNotDetectedAsLatin()
    {
        var lines = new[] { "Δεν ξέρω τι εννοείς.", "Πρέπει να πάμε σπίτι τώρα." };
        AssertDetectsAndDecodes(Pac.CodePageGreek, lines);
    }

    [Fact]
    public void ThaiIsNotDetectedAsHebrew()
    {
        var lines = new[] { "ฉันไม่รู้ว่าคุณต้องการอะไร" };
        AssertDetectsAndDecodes(Pac.CodePageThai, lines);
    }

    /// <summary>
    /// The Greek and Cyrillic tables remap ASCII letters, so short all-ASCII text could "detect"
    /// as Greek or Cyrillic from letter statistics - plain ASCII without dictionary hits is Latin.
    /// </summary>
    [Theory]
    [InlineData("OK.")]
    [InlineData("Yes.")]
    [InlineData("Hello")]
    [InlineData("Subtitles by ACME Studios")]
    [InlineData("Paris, 1944")]
    public void ShortAsciiTextIsLatin(string line)
    {
        AssertDetectsAndDecodes(Pac.CodePageLatin, new[] { line });
    }

    private static void AssertDetectsAndDecodes(int codePage, string[] lines)
    {
        var path = SavePac(MakeSubtitle(lines), codePage);
        try
        {
            Assert.Equal(codePage, Pac.AutoDetectEncoding(path));

            var pac = new Pac();
            var loaded = new Subtitle();
            pac.LoadSubtitle(loaded, null, path);
            Assert.Equal(codePage, pac.CodePage);
            Assert.Equal(lines, loaded.Paragraphs.Select(p => p.Text));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// A ".rac" file has the PAC subtitle records without the PAC file header, with unrelated
    /// bytes in front of (and between) them.
    /// </summary>
    [Fact]
    public void RacFileLoadsPacRecords()
    {
        var lines = new[] { "Hej, jeg kommer tilbage.", "Måske var han der.", "Undskyld." };
        var pacPath = SavePac(MakeSubtitle(lines), Pac.CodePageLatin);
        var racPath = Path.ChangeExtension(pacPath, ".rac");
        var disguisedPacPath = pacPath + "2.pac";
        try
        {
            var garbage = Enumerable.Range(0, 64).Select(i => (byte)(0x9B ^ i)).ToArray();
            var content = garbage.Concat(File.ReadAllBytes(pacPath)).ToArray();
            File.WriteAllBytes(racPath, content);
            File.WriteAllBytes(disguisedPacPath, content);

            var pac = new Pac();
            Assert.True(pac.IsMine(null, racPath));
            var loaded = new Subtitle();
            pac.LoadSubtitle(loaded, null, racPath);
            Assert.Equal(lines, loaded.Paragraphs.Select(p => p.Text));

            // without the header it is not a .pac
            Assert.False(new Pac().IsMine(null, disguisedPacPath));
        }
        finally
        {
            File.Delete(pacPath);
            File.Delete(racPath);
            File.Delete(disguisedPacPath);
        }
    }
}
