using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Round 4 of the micro-perf hunt: helpers that run once per subtitle line, per word or per
/// character. Everything here goes through the public API that exists on both sides of the
/// change, so the same benchmarks can be run against a stashed baseline for before/after numbers.
/// </summary>
internal static class BenchmarkSubtitles
{
    internal static string FindDataDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Dictionaries", "names.xml")))
            {
                return dir.FullName + Path.DirectorySeparatorChar;
            }

            dir = dir.Parent;
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Subtitle Edit") + Path.DirectorySeparatorChar;
    }

    internal static readonly string[] Sentences =
    {
        "It was the best of times, it was the worst of times.",
        "MAN: Are you coming with us, John?",
        "- No, Mrs. smith.\r\n- Then stay here and wait for peter.",
        "<i>I told you already, this is not going to work out the way you think.</i>",
        "WOMAN 2: Hello there, dr. jones.",
        "Somewhere in Denmark,\r\na quiet evening begins.",
        "[DOOR SLAMS]",
        "THIS WHOLE LINE IS SHOUTED.",
    };

    internal static Subtitle Build(int lineCount)
    {
        var subtitle = new Subtitle();
        for (var i = 0; i < lineCount; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph(
                Sentences[i % Sentences.Length],
                i * 2000,
                i * 2000 + 1800));
        }

        subtitle.Renumber();
        return subtitle;
    }
}

/// <summary>
/// "Change casing" runs StrippableText over every paragraph with the full name list (~8000
/// entries for English). The name loop lower-cased every name on every paragraph.
/// </summary>
[MemoryDiagnoser]
public class ChangeCasingBenchmarks
{
    private Subtitle _subtitle = null!;

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        _subtitle = BenchmarkSubtitles.Build(200);
    }

    [Benchmark]
    public int FixCasingNormal()
    {
        var subtitle = new Subtitle(_subtitle);
        var fixCasing = new FixCasing("en") { FixNormal = true };
        fixCasing.Fix(subtitle);
        return subtitle.Paragraphs.Count;
    }
}

/// <summary>
/// The CJK length calculators ask IsCjk once per character; the grid re-reads CPS and line
/// length on every repaint. IsCjk used to allocate a one-char string and run a regex on it.
/// </summary>
[MemoryDiagnoser]
public class CjkLengthBenchmarks
{
    private const string Latin = "It was the best of times, it was the worst of times.";
    private const string Japanese = "彼は静かな夕暮れの中を歩いていた。それは長い一日の終わりだった。";
    private readonly CalcCjk _calc = new CalcCjk();

    [Benchmark]
    public decimal CountLatin() => _calc.CountCharacters(Latin, true);

    [Benchmark]
    public decimal CountJapanese() => _calc.CountCharacters(Japanese, true);
}

/// <summary>
/// Auto-break runs per line on split/merge and per keystroke with "auto break while typing".
/// Re-inserting the html tags did two dictionary probes per character.
/// </summary>
[MemoryDiagnoser]
public class AutoBreakBenchmarks
{
    private const string Tagged = "<i>It was the best of times,</i> it was the <b>worst</b> of times, and nobody knew it yet.";
    private const string Plain = "It was the best of times, it was the worst of times, and nobody knew it yet.";

    [Benchmark]
    public string AutoBreakTagged() => Utilities.AutoBreakLine(Tagged);

    [Benchmark]
    public string AutoBreakPlain() => Utilities.AutoBreakLine(Plain);
}

/// <summary>
/// "Remove text for hearing impaired" over a whole file. The all-uppercase probes allocated an
/// uppercased copy of the line, and the whitelist set was rebuilt per line.
/// </summary>
[MemoryDiagnoser]
public class RemoveTextForHiBenchmarks
{
    private Subtitle _subtitle = null!;
    private RemoveTextForHI _lib = null!;

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        _subtitle = BenchmarkSubtitles.Build(500);
        _lib = new RemoveTextForHI(new RemoveTextForHISettings(_subtitle));
    }

    [Benchmark]
    public int RemoveHearingImpaired()
    {
        var count = 0;
        for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
        {
            var text = _lib.RemoveTextFromHearImpaired(_subtitle.Paragraphs[i].Text, _subtitle, i, "en");
            count += text.Length;
        }

        return count;
    }
}

/// <summary>
/// Loading a SAMI file: the cue loop uppercased whole cues, rebuilt a character set per
/// character of a class name and grew the milliseconds string one character at a time.
/// </summary>
[MemoryDiagnoser]
public class SamiLoadBenchmarks
{
    private List<string> _lines = null!;

    [GlobalSetup]
    public void Setup()
    {
        var subtitle = BenchmarkSubtitles.Build(500);
        var text = new Sami().ToText(subtitle, "benchmark");
        _lines = text.SplitToLines().ToList();
    }

    [Benchmark]
    public int LoadSami()
    {
        var subtitle = new Subtitle();
        new Sami().LoadSubtitle(subtitle, _lines, null);
        return subtitle.Paragraphs.Count;
    }
}

/// <summary>
/// Writing SubStation Alpha and MicroDVD. SSA scanned the style list per paragraph and copied
/// the finished output twice; MicroDVD counted tags over the whole line before the cheap
/// prefix test that rejects it.
/// </summary>
[MemoryDiagnoser]
public class FormatWriteBenchmarks
{
    private Subtitle _styled = null!;
    private Subtitle _plain = null!;

    [GlobalSetup]
    public void Setup()
    {
        _styled = BenchmarkSubtitles.Build(500);
        var header = new System.Text.StringBuilder();
        header.AppendLine("[V4 Styles]");
        header.AppendLine("Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding");
        for (var i = 0; i < 120; i++)
        {
            header.AppendLine($"Style: Style{i},Arial,20,&H00FFFFFF,&H0000FFFF,&H00000000,&H80000000,-1,0,1,2,2,2,10,10,10,0,1");
        }

        _styled.Header = header.ToString();
        for (var i = 0; i < _styled.Paragraphs.Count; i++)
        {
            _styled.Paragraphs[i].Extra = "Style" + i % 120;
        }

        _plain = BenchmarkSubtitles.Build(500);
    }

    [Benchmark]
    public int SubStationAlphaToText() => new SubStationAlpha().ToText(_styled, "benchmark").Length;

    [Benchmark]
    public int MicroDvdToText() => new MicroDvd().ToText(_plain, "benchmark").Length;
}
