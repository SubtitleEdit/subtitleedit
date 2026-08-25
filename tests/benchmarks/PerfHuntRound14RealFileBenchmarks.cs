using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Text;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// End-to-end passes over a real subtitle file, for the round-14 hunt. Unlike
/// <see cref="PerfHuntRound14Benchmarks"/> these call the shipping code, so the same class is run
/// once against a baseline worktree and once against the patched tree.
///
/// <para><see cref="SubRipParseControl"/> is the drift control: nothing in this round touches
/// SubRip, so if that row moves the two halves of the comparison are not trustworthy.</para>
///
/// Point <c>SE_BENCH_SUBTITLE</c> at an .srt; a synthetic corpus is used when it is unset.
/// </summary>
[MemoryDiagnoser]
public class PerfHuntRound14RealFileBenchmarks
{
    private List<string> _rawLines = new();
    private Subtitle _subtitle = new();
    private RemoveTextForHI _removeTextForHi = null!;
    private readonly StubFixCallbacks _callbacks = new();
    private readonly StubFixCallbacks _arabicCallbacks = new() { LanguageCode = "ar" };

    static PerfHuntRound14RealFileBenchmarks()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [GlobalSetup]
    public void Setup()
    {
        _rawLines = LoadLines();
        _subtitle = new Subtitle();
        new SubRip().LoadSubtitle(_subtitle, _rawLines, "movie.srt");
        _removeTextForHi = new RemoveTextForHI(new RemoveTextForHISettings(_subtitle));
    }

    private static List<string> LoadLines()
    {
        var path = Environment.GetEnvironmentVariable("SE_BENCH_SUBTITLE");
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            return File.ReadAllLines(path).ToList();
        }

        var samples = new[]
        {
            "It was the best of times, it was the worst of times.",
            "<i>Are you coming with us?</i>",
            "- No.|- Then stay here and wait.",
            "MAN: There were 3 of them,|and only 1 got away.",
            "[DOOR CREAKING]",
            "(SIGHS) i think i'm going to be sick.",
            "NARRATOR: Chapter 10: the long way home...",
            "You'll find it at 10:30, i.e. after lunch.",
        };

        var lines = new List<string>(1800 * 4);
        var ms = 1000;
        for (var i = 0; i < 1800; i++)
        {
            lines.Add((i + 1).ToString());
            lines.Add($"{new TimeCode(ms)}".Replace('.', ',') + " --> " + $"{new TimeCode(ms + 2000)}".Replace('.', ','));
            foreach (var part in samples[i % samples.Length].Split('|'))
            {
                lines.Add(part);
            }

            lines.Add(string.Empty);
            ms += 2200;
        }

        return lines;
    }

    /// <summary>Drift control - untouched by this round. A ratio far from 1.0 invalidates the pair.</summary>
    [Benchmark]
    public int SubRipParseControl()
    {
        var subtitle = new Subtitle();
        new SubRip().LoadSubtitle(subtitle, _rawLines, "movie.srt");
        return subtitle.Paragraphs.Count;
    }

    /// <summary>Save as Scenarist Closed Captions - the per-character code lookup and italic scan.</summary>
    [Benchmark]
    public int SccExport() => new ScenaristClosedCaptions().ToText(_subtitle, "movie").Length;

    /// <summary>Save as Cheetah Caption - the per-character newline probe and the cp1252 lookup.</summary>
    [Benchmark]
    public long CheetahExport()
    {
        using var stream = new MemoryStream();
        new CheetahCaption().Save("movie.cap", stream, _subtitle, true);
        return stream.Length;
    }

    /// <summary>Remove text for hearing impaired over the whole file.</summary>
    [Benchmark]
    public int RemoveTextForHearingImpaired()
    {
        var total = 0;
        for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
        {
            total += _removeTextForHi.RemoveTextFromHearImpaired(_subtitle.Paragraphs[i].Text, _subtitle, i, "en").Length;
        }

        return total;
    }

    /// <summary>Two of the fix-common-errors passes over the whole file.</summary>
    [Benchmark]
    public int FixCommonErrorsPass()
    {
        var working = new Subtitle(_subtitle);
        new FixMissingSpaces().Fix(working, _callbacks);
        new FixAloneLowercaseIToUppercaseI().Fix(working, _callbacks);
        return working.Paragraphs.Count;
    }

    /// <summary>Just the lowercase-i fix: it prepares four html needles per line.</summary>
    [Benchmark]
    public int FixAloneLowercaseIOnly()
    {
        var working = new Subtitle(_subtitle);
        new FixAloneLowercaseIToUppercaseI().Fix(working, _callbacks);
        return working.Paragraphs.Count;
    }

    /// <summary>
    /// FixSpaceAfter - and with it the two character-set probes - only runs for Arabic, so it
    /// needs a callback that reports "ar" to be exercised at all.
    /// </summary>
    [Benchmark]
    public int FixMissingSpacesArabic()
    {
        var working = new Subtitle(_subtitle);
        new FixMissingSpaces().Fix(working, _arabicCallbacks);
        return working.Paragraphs.Count;
    }

    /// <summary>How much of the deep copy the two passes above are measured on top of.</summary>
    [Benchmark]
    public int SubtitleCopyControl() => new Subtitle(_subtitle).Paragraphs.Count;

    private sealed class StubFixCallbacks : IFixCallbacks
    {
        public bool AllowFix(Paragraph p, string action) => true;
        public void AddFixToListView(Paragraph p, string action, string before, string after) { }
        public void AddFixToListView(Paragraph p, string action, string before, string after, bool isChecked) { }
        public void LogStatus(string sender, string message) { }
        public void LogStatus(string sender, string message, bool isImportant) { }
        public void UpdateFixStatus(int fixes, string message) { }
        public bool IsName(string candidate) => false;
        public HashSet<string> GetAbbreviations() => new();
        public void AddToTotalErrors(int count) { }
        public void AddToDeleteIndices(int index) { }
        public SubtitleFormat Format { get; } = new SubRip();
        public Encoding Encoding { get; } = Encoding.UTF8;
        public string LanguageCode { get; init; } = "en";
        public string Language => LanguageCode;
    }
}
