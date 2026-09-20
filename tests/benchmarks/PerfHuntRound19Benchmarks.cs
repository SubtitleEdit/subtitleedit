using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Text;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Round 19 performance hunt - the per-character <c>Substring</c> loops left over from round 18
/// (ESub-XF, TextST, Ultech 130, DVD Studio Pro, Unknown 81, the right-to-left reverser,
/// Cavena 890) plus per-paragraph allocations (Cavena's colour markers and its byte-to-string
/// replace chain, "add missing period"'s full word split).
/// Public entry points only, so the file runs unchanged against a baseline checkout;
/// <see cref="SubRipControl"/> is the drift control.
///
/// Default job, in-process, Apple M4, .NET 10 (SubRipControl 51.5 us -> 51.5 us, identical
/// allocations):
///
///   DVD Studio Pro load (400)       1,049 us -> 311 us   3.4x   12.2 MB -> 736 KB allocated
///   Cavena 890 load (400)           3,895 us -> 1,206 us 3.2x   16.2 MB -> 1.0 MB
///   Unknown 81 save (400)             736 us -> 228 us   3.2x   8.9 MB -> 786 KB
///   TextST dialog segments (400)      484 us -> 163 us   3.0x   4.7 MB -> 814 KB
///   Ultech 130 save (400)             533 us -> 337 us   1.58x  3.5 MB -> 184 KB
///   ESub-XF save (400)              8,653 us -> 6,660 us 1.30x  17.8 MB -> 1.9 MB
///   RTL start/end reverse (400)      94.9 us -> 84.0 us  1.13x  639 KB -> 521 KB
///   FixMissingPeriodsAtEndOfLine      294 us -> 263 us   1.12x  612 KB -> 496 KB
///   Cavena 890 save (400)           6,808 us -> 6,638 us 1.03x  3.8 MB -> 2.5 MB
/// </summary>
[MemoryDiagnoser]
public class PerfHuntRound19Benchmarks
{
    private Subtitle _taggedSubtitle = new();
    private Subtitle _periodSubtitle = new();
    private List<string> _dvdStudioProLines = new();
    private string[] _rtlTexts = Array.Empty<string>();
    private string _cavenaFile = string.Empty;
    private string _ultechFile = string.Empty;
    private TextST.RegionStyle _regionStyle = new();

    [GlobalSetup]
    public void Setup()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _taggedSubtitle = BuildTaggedSubtitle(400);
        _periodSubtitle = BuildPeriodSubtitle(400);
        _dvdStudioProLines = new DvdStudioPro().ToText(BuildTaggedSubtitle(400), "t").SplitToLines();
        _rtlTexts = BuildRtlTexts(400);

        var dir = Path.Combine(Path.GetTempPath(), "se-perf-round19");
        Directory.CreateDirectory(dir);
        _cavenaFile = Path.Combine(dir, "in.890");
        var cavena = new Subtitle();
        for (var i = 0; i < 400; i++)
        {
            cavena.Paragraphs.Add(new Paragraph($"<i>Cue {i}</i> says hello{Environment.NewLine}and then some more text {i}.", i * 3000, i * 3000 + 2500));
        }

        using (var fs = File.Create(_cavenaFile))
        {
            new Cavena890().Save(_cavenaFile, fs, cavena, true);
        }

        _ultechFile = Path.Combine(dir, "out.uld");
        _regionStyle = new TextST.RegionStyle();
    }

    private static string Sentence(int i) =>
        $"The quick brown fox number {i} jumps over the lazy dog, twice, and then rests.";

    private static Subtitle BuildTaggedSubtitle(int count)
    {
        var s = new Subtitle();
        for (var i = 0; i < count; i++)
        {
            var text = (i % 4) switch
            {
                0 => $"<i>{Sentence(i)}</i>{Environment.NewLine}Second line of cue {i}.",
                1 => $"He said: <b>no</b>.{Environment.NewLine}<font color=\"#ffff00\">Yellow {i}</font> and <u>more</u>.",
                2 => $"{Sentence(i)}{Environment.NewLine}<i>Whispered</i> reply.",
                _ => Sentence(i),
            };
            s.Paragraphs.Add(new Paragraph(text, i * 3000, i * 3000 + 2500));
        }

        return s;
    }

    private static Subtitle BuildPeriodSubtitle(int count)
    {
        var s = new Subtitle();
        for (var i = 0; i < count; i++)
        {
            // No end punctuation and the next cue starts upper-case, so the rule asks whether its first word is a name.
            s.Paragraphs.Add(new Paragraph($"we went to the market {i}", i * 3000, i * 3000 + 1500));
            s.Paragraphs.Add(new Paragraph($"And bought {i} apples, pears and more", i * 3000 + 1600, i * 3000 + 2900));
        }

        return s;
    }

    private static string[] BuildRtlTexts(int count)
    {
        var list = new string[count];
        for (var i = 0; i < count; i++)
        {
            list[i] = (i % 3) switch
            {
                0 => $"<i>- שלום {i}, מה שלומך?</i>{Environment.NewLine}♪ טוב מאוד! ♪",
                1 => $"{{\\an8}}<font color=\"#ffff00\">(אני {i})</font> ...",
                _ => $"שורה רגילה {i}.",
            };
        }

        return list;
    }

    [Benchmark]
    public int ESubXfSave() => new ESubXf().ToText(_taggedSubtitle, "t").Length;

    [Benchmark]
    public int TextStDialogSegments()
    {
        var total = 0;
        foreach (var p in _taggedSubtitle.Paragraphs)
        {
            total += new TextST.DialogPresentationSegment(p, _regionStyle).Regions[0].Content.Count;
        }

        return total;
    }

    [Benchmark]
    public long UltechSave()
    {
        Ultech130.Save(_ultechFile, _taggedSubtitle);
        return new FileInfo(_ultechFile).Length;
    }

    [Benchmark]
    public int DvdStudioProLoad()
    {
        var s = new Subtitle();
        new DvdStudioPro().LoadSubtitle(s, _dvdStudioProLines, "in.stl");
        return s.Paragraphs.Count;
    }

    [Benchmark]
    public int UnknownSubtitle81Save() => SubtitleFormat.AllSubtitleFormats.First(f => f.Name == "Unknown 81").ToText(_taggedSubtitle, "t").Length;

    [Benchmark]
    public int ReverseStartAndEndingForRightToLeft()
    {
        var total = 0;
        foreach (var t in _rtlTexts)
        {
            total += Utilities.ReverseStartAndEndingForRightToLeft(t).Length;
        }

        return total;
    }

    [Benchmark]
    public int Cavena890Load()
    {
        var s = new Subtitle();
        new Cavena890().LoadSubtitle(s, new List<string>(), _cavenaFile);
        return s.Paragraphs.Count;
    }

    [Benchmark]
    public long Cavena890Save()
    {
        using var ms = new MemoryStream();
        new Cavena890().Save("x.890", ms, _taggedSubtitle, true);
        return ms.Length;
    }

    [Benchmark]
    public int FixMissingPeriodsAtEndOfLine()
    {
        var copy = new Subtitle(_periodSubtitle);
        new FixMissingPeriodsAtEndOfLine().Fix(copy, new EmptyFixCallback());
        return copy.Paragraphs.Count;
    }

    [Benchmark]
    public int SubRipControl() => new SubRip().ToText(_taggedSubtitle, "t").Length;
}
