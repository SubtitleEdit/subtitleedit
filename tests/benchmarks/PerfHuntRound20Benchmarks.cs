using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Cea708;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Text;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Round 20 performance hunt: the quadratic same-text merge scan, WebVTT colour regex loops
/// restarting at 0, per-paragraph LINQ lookups (WebVTT to ASSA styles),
/// and per-character string keys / encoder objects (Smart Titler, Magic Video Titler, CEA-708,
/// Captions Inc, Projection Subtitle List, unknown-format importer, fix casing).
/// Public entry points only; <see cref="SubRipControl"/> is the drift control.
///
/// Default job, in-process, Apple M4, .NET 10 (SubRipControl 48.2 us -> 48.3 us, identical
/// allocations):
///
///   Merge lines with same text (2000)   24,530 us -> 543 us   45x
///   Captions Inc save (400)              1,149 us -> 454 us   2.5x   2.6 MB -> 381 KB allocated
///   CEA-708 encode text                    271 us -> 134 us   2.0x   620 KB -> 85 KB
///   Smart Titler round trip (400)          616 us -> 317 us   1.9x   2.7 MB -> 1.0 MB
///   Projection Subtitle List save (400)    291 us -> 152 us   1.9x   1.6 MB -> 714 KB
///   Magic Video Titler round trip (400)    663 us -> 371 us   1.8x   2.7 MB -> 1.1 MB
///   Fix casing, titles (400)            34,709 us -> 28,210 us 1.23x  3.0 MB -> 2.7 MB
///   WebVTT save with colours (400)         527 us -> 484 us   1.09x
///   WebVTT -> ASSA convert (400)           743 us -> 722 us   1.03x  3.0 MB -> 2.7 MB
///   Unknown-format import (400)          1,467 us -> 1,429 us 1.03x  2.19 MB -> 2.12 MB
/// </summary>
[MemoryDiagnoser]
public class PerfHuntRound20Benchmarks
{
    private Subtitle _taggedSubtitle = new();
    private Subtitle _sameTextSubtitle = new();
    private Subtitle _webVttColorSubtitle = new();
    private Subtitle _webVttClassSubtitle = new();
    private Subtitle _titleSubtitle = new();
    private Subtitle _unicodeSubtitle = new();
    private List<WebVttStyle> _webVttStyles = new();
    private List<string> _smartTitlerLines = new();
    private List<string> _magicLines = new();
    private List<string> _importLines = new();
    private string _captionsIncFile = string.Empty;
    private string _cea708Text = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _taggedSubtitle = BuildTaggedSubtitle(400);
        _sameTextSubtitle = BuildSameTextSubtitle(2000);
        _webVttColorSubtitle = BuildWebVttColorSubtitle(400);
        (_webVttClassSubtitle, _webVttStyles) = BuildWebVttClassSubtitle(400);
        _titleSubtitle = BuildTitleSubtitle(400);
        _unicodeSubtitle = BuildUnicodeSubtitle(400);
        _smartTitlerLines = new SmartTitler().ToText(_unicodeSubtitle, "t").SplitToLines();
        _magicLines = new MagicVideoTitler().ToText(_unicodeSubtitle, "t").SplitToLines();
        _importLines = BuildImportLines(400);
        _cea708Text = string.Join(" ", _unicodeSubtitle.Paragraphs.Select(p => p.Text));

        var dir = Path.Combine(Path.GetTempPath(), "se-perf-round20");
        Directory.CreateDirectory(dir);
        _captionsIncFile = Path.Combine(dir, "out.cin");
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

    /// <summary>Fade-style repeats: every third cue is the same text as the one before it.</summary>
    private static Subtitle BuildSameTextSubtitle(int count)
    {
        var s = new Subtitle();
        for (var i = 0; i < count; i++)
        {
            var text = i % 3 == 1 ? $"<i>{Sentence(i - 1)}</i>" : Sentence(i);
            s.Paragraphs.Add(new Paragraph(text, i * 2000, i * 2000 + 1900));
        }

        return s;
    }

    private static Subtitle BuildWebVttColorSubtitle(int count)
    {
        var s = new Subtitle();
        for (var i = 0; i < count; i++)
        {
            var text = (i % 3) switch
            {
                0 => $"<font color=\"#ffff00\">Yellow {i}</font> and <font color=\"cyan\">cyan</font>{Environment.NewLine}<font color=\"#ff0000\">red</font> <font color=\"#00ff00\">green</font>",
                1 => $"<font color=\"white\">{Sentence(i)}</font>",
                _ => Sentence(i),
            };
            s.Paragraphs.Add(new Paragraph(text, i * 3000, i * 3000 + 2500));
        }

        return s;
    }

    private static (Subtitle, List<WebVttStyle>) BuildWebVttClassSubtitle(int count)
    {
        var header = new StringBuilder();
        header.AppendLine("WEBVTT");
        header.AppendLine();
        for (var i = 0; i < 30; i++)
        {
            header.AppendLine($"STYLE{Environment.NewLine}::cue(.style{i}) {{ color: #{(i * 8) % 256:x2}{(i * 16) % 256:x2}{(i * 32) % 256:x2}; font-weight: bold; }}");
        }

        var s = new Subtitle { Header = header.ToString() };
        for (var i = 0; i < count; i++)
        {
            var text = (i % 3) switch
            {
                0 => $"<c.style{i % 30}>{Sentence(i)}</c>",
                1 => $"<c.style{i % 30}.style{(i + 1) % 30}>Two styles {i}</c>{Environment.NewLine}<c.style3>and <i>more</i></c>",
                _ => Sentence(i),
            };
            s.Paragraphs.Add(new Paragraph(text, i * 3000, i * 3000 + 2500));
        }

        return (s, WebVttHelper.GetStyles(s.Header));
    }

    private static Subtitle BuildTitleSubtitle(int count)
    {
        var s = new Subtitle();
        for (var i = 0; i < count; i++)
        {
            var text = (i % 2) switch
            {
                0 => $"mr. smith and Mrs. jones went to dr. brown, number {i}.",
                _ => $"Nothing here for cue {i}, MS. anderson.",
            };
            s.Paragraphs.Add(new Paragraph(text, i * 3000, i * 3000 + 2500));
        }

        return s;
    }

    private static Subtitle BuildUnicodeSubtitle(int count)
    {
        var s = new Subtitle();
        for (var i = 0; i < count; i++)
        {
            var text = (i % 3) switch
            {
                0 => $"Đorđe i Ljiljana, Žarko, Šćepan {i}{Environment.NewLine}Čović dž nj Dž",
                1 => $"Café crème – très bien {i}, señor ¿qué? ©",
                _ => Sentence(i),
            };
            s.Paragraphs.Add(new Paragraph(text, i * 3000, i * 3000 + 2500));
        }

        return s;
    }

    private static List<string> BuildImportLines(int count)
    {
        var lines = new List<string>();
        for (var i = 0; i < count; i++)
        {
            var start = TimeSpan.FromMilliseconds(i * 3000).ToString(@"hh\:mm\:ss\.fff");
            var end = TimeSpan.FromMilliseconds(i * 3000 + 2500).ToString(@"hh\:mm\:ss\.fff");
            lines.Add($"[{start}] --> [{end}]");
            lines.Add(Sentence(i));
            lines.Add(string.Empty);
        }

        return lines;
    }

    [Benchmark]
    public int MergeLinesWithSameText() => MergeLinesSameTextUtils.MergeLinesWithSameTextInSubtitle(_sameTextSubtitle, true, 250).Paragraphs.Count;

    [Benchmark]
    public int WebVttSaveColors() => new WebVTT().ToText(_webVttColorSubtitle, "t").Length;

    [Benchmark]
    public int WebVttToAssaConvert() => WebVttToAssa.Convert(_webVttClassSubtitle, new SsaStyle(), 1920, 1080).Paragraphs.Count;

    [Benchmark]
    public int FixCasingTitles()
    {
        var copy = new Subtitle(_titleSubtitle);
        new FixCasing("en") { FixNormal = true }.Fix(copy);
        return copy.Paragraphs.Count;
    }

    [Benchmark]
    public int SmartTitlerRoundTrip()
    {
        var s = new Subtitle();
        new SmartTitler().LoadSubtitle(s, _smartTitlerLines, "in.txt");
        return new SmartTitler().ToText(s, "t").Length;
    }

    [Benchmark]
    public int MagicVideoTitlerRoundTrip()
    {
        var s = new Subtitle();
        new MagicVideoTitler().LoadSubtitle(s, _magicLines, "in.txt");
        return new MagicVideoTitler().ToText(s, "t").Length;
    }

    [Benchmark]
    public int Cea708EncodeText() => Cea708.EncodeText(_cea708Text).Length;

    [Benchmark]
    public long CaptionsIncSave()
    {
        CaptionsInc.Save(_captionsIncFile, _taggedSubtitle);
        return new FileInfo(_captionsIncFile).Length;
    }

    [Benchmark]
    public int ProjectionSubtitleListSave() => new ProjectionSubtitleList().ToText(_unicodeSubtitle, "t").Length;

    [Benchmark]
    public int UnknownFormatImport() => new UnknownFormatImporter().AutoGuessImport(_importLines, "in.txt").Paragraphs.Count;

    [Benchmark]
    public int SubRipControl() => new SubRip().ToText(_taggedSubtitle, "t").Length;
}
