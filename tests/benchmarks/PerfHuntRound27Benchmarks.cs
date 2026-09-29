using System.Text;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Benchmarks;

// Round 27: opening a large (1-5 MB) file that is not a subtitle. Every format is asked
// IsMine and all of them must say no, so the whole list runs. Each class calls the real
// IsMine / converter, so the same file is run before and after the change.

/// <summary>Non-subtitle documents, generated once (deterministic).</summary>
internal static class NonSubtitleFiles
{
    private static readonly string[] Dict = "the of and to in is was that for it with as his on be at by had are but from or have an they which one you were her all she there would their we him been has when who will more no if out so said what up its about into than them can only other new some could time these two may then do first any my now such like our over man me even most made after also did many before must through back years where much your way well down should because each just those people".Split(' ');

    private static string Words(Random r, int n) => string.Join(' ', Enumerable.Range(0, n).Select(_ => Dict[r.Next(Dict.Length)]));

    private static string Repeat(int size, Func<int, string> f)
    {
        var sb = new StringBuilder(size + 1000);
        for (var i = 0; sb.Length < size; i++)
        {
            sb.Append(f(i));
        }

        return sb.ToString();
    }

    internal static string Prose(int size, int seed = 1)
    {
        var r = new Random(seed);
        return Repeat(size, i => Words(r, 12 + r.Next(12)) + ".\r\n" + (i % 5 == 0 ? "\r\n" : ""));
    }

    /// <summary>Pretty-printed array of objects that have a "start" key but no "end".</summary>
    internal static string Json(int size)
    {
        var r = new Random(2);
        return "[" + Repeat(size, i => (i > 0 ? ",\n" : "\n") + $"  {{\"id\": {i}, \"name\": \"{Words(r, 2)}\", \"start\": \"00:{i / 60 % 60:00}:{i % 60:00}\", \"tags\": [\"a\", \"b\"], \"value\": {r.NextDouble():F4}}}") + "\n]\n";
    }

    /// <summary>Minified JSON: the whole file is one line.</summary>
    internal static string JsonMinified(int size)
    {
        var r = new Random(3);
        return "[" + Repeat(size, i => (i > 0 ? "," : "") + $"{{\"id\":{i},\"text\":\"{Words(r, 5)}\",\"t\":{i * 1.5}}}") + "]";
    }

    internal static string Xml(int size)
    {
        var r = new Random(4);
        return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<catalog>\n" + Repeat(size, i => $"  <item id=\"{i}\" begin=\"00:00:{i % 60:00}\">\n    <title>{Words(r, 4)}</title>\n    <price>{r.Next(100)}.99</price>\n  </item>\n") + "</catalog>\n";
    }

    internal static string Rtf(int size, int seed)
    {
        var r = new Random(seed);
        return "{\\rtf1\\ansi\\deff0{\\fonttbl{\\f0 Times New Roman;}}\\f0\\fs24\n" + Repeat(size, i => "\\par " + Words(r, 14) + (i % 3 == 0 ? " {\\b bold} \\'e9t\\'e9 \\u8212? " : " ") + "\n") + "}";
    }

    internal static List<string> Lines(string s) => s.SplitToLines();
}

/// <summary>The RTF-to-text converter that ~20 RTF based formats share (two different documents,
/// so the last-conversion cache never answers).</summary>
[MemoryDiagnoser]
public class Round27RtfBenchmarks
{
    private string _a = string.Empty;
    private string _b = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _a = NonSubtitleFiles.Rtf(1024 * 1024, 5);
        _b = NonSubtitleFiles.Rtf(1024 * 1024, 6);
    }

    [Benchmark]
    public int ConvertTwo1MbRtf() => RichTextToPlainText.ConvertToText(_a).Length + RichTextToPlainText.ConvertToText(_b).Length;
}

/// <summary>Single formats asked IsMine on 5 MB non-subtitle files.</summary>
[MemoryDiagnoser]
public class Round27FormatIsMineBenchmarks
{
    private const int Size = 5 * 1024 * 1024;
    private List<string> _prose = null!;
    private List<string> _json = null!;
    private List<string> _jsonMin = null!;
    private List<string> _xml = null!;

    [GlobalSetup]
    public void Setup()
    {
        _prose = NonSubtitleFiles.Lines(NonSubtitleFiles.Prose(Size));
        _json = NonSubtitleFiles.Lines(NonSubtitleFiles.Json(Size));
        _jsonMin = NonSubtitleFiles.Lines(NonSubtitleFiles.JsonMinified(Size));
        _xml = NonSubtitleFiles.Lines(NonSubtitleFiles.Xml(Size));
    }

    [Benchmark]
    public bool CsvNuendoMinifiedJson() => new CsvNuendo().IsMine(_jsonMin, "data.json");

    [Benchmark]
    public bool JsonType9Json() => new JsonType9().IsMine(_json, "data.json");

    [Benchmark]
    public bool CsvDaVinciProse() => new CsvDaVinci().IsMine(_prose, "notes.txt");

    [Benchmark]
    public bool MacCaptionProse() => new MacCaption10().IsMine(_prose, "notes.txt");

    [Benchmark]
    public bool SamiFamilyProse() =>
        new Sami().IsMine(_prose, "notes.txt") | new SamiModern().IsMine(_prose, "notes.txt") |
        new SamiYouTube().IsMine(_prose, "notes.txt") | new SamiAvDicPlayer().IsMine(_prose, "notes.txt");

    [Benchmark]
    public bool SsaAssaProse() => new SubStationAlpha().IsMine(_prose, "notes.txt") | new AdvancedSubStationAlpha().IsMine(_prose, "notes.txt");

    [Benchmark]
    public bool FinalCutPro7Xml() =>
        new FinalCutProXml().IsMine(_xml, "data.xml") | new FinalCutProTest2Xml().IsMine(_xml, "data.xml") |
        new FinalCutProTestXml().IsMine(_xml, "data.xml");

    [Benchmark]
    public bool FinalCutProXFamilyXml() =>
        new FinalCutProXCM().IsMine(_xml, "data.xml") | new FinalCutProXmlGap().IsMine(_xml, "data.xml") |
        new FinalCutProXXml().IsMine(_xml, "data.xml") | new FinalCutProXmlName().IsMine(_xml, "data.xml");

    [Benchmark]
    public bool TimedTextNoNamespaceXml() => new TimedTextNoNs().IsMine(_xml, "data.xml");

    /// <summary>Drift control - untouched by this round.</summary>
    [Benchmark]
    public bool ControlSubRipProse() => new SubRip().IsMine(_prose, "notes.txt");
}

/// <summary>
/// The whole auto-detect pass Subtitle.LoadSubtitle runs for a file no format claims: every
/// format's IsMine, in list order, on 5 MB files (the RTF one alternates two 1 MB documents).
/// </summary>
[MemoryDiagnoser]
public class Round27AutoDetectBenchmarks
{
    private List<string> _prose = null!;
    private List<string> _json = null!;
    private List<string> _jsonMin = null!;
    private List<string> _xml = null!;
    private List<string> _rtfA = null!;
    private List<string> _rtfB = null!;

    [GlobalSetup]
    public void Setup()
    {
        const int size = 5 * 1024 * 1024;
        _prose = NonSubtitleFiles.Lines(NonSubtitleFiles.Prose(size));
        _json = NonSubtitleFiles.Lines(NonSubtitleFiles.Json(size));
        _jsonMin = NonSubtitleFiles.Lines(NonSubtitleFiles.JsonMinified(size));
        _xml = NonSubtitleFiles.Lines(NonSubtitleFiles.Xml(size));
        _rtfA = NonSubtitleFiles.Lines(NonSubtitleFiles.Rtf(1024 * 1024, 7));
        _rtfB = NonSubtitleFiles.Lines(NonSubtitleFiles.Rtf(1024 * 1024, 8));
    }

    [Benchmark]
    public int Prose5Mb() => DetectAll(_prose, "notes.txt");

    [Benchmark]
    public int Json5Mb() => DetectAll(_json, "data.json");

    [Benchmark]
    public int JsonMinified5Mb() => DetectAll(_jsonMin, "data.json");

    [Benchmark]
    public int Xml5Mb() => DetectAll(_xml, "data.xml");

    [Benchmark]
    public int TwoRtf1Mb() => DetectAll(_rtfA, "a.rtf") + DetectAll(_rtfB, "b.rtf");

    private static int DetectAll(List<string> lines, string fileName)
    {
        var ext = Path.GetExtension(fileName);
        var claimed = 0;
        foreach (var format in SubtitleFormat.AllSubtitleFormats.Where(p => p.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase) && !p.Name.StartsWith("Unknown", StringComparison.Ordinal))
                     .Concat(SubtitleFormat.AllSubtitleFormats.Where(p => !p.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase) || p.Name.StartsWith("Unknown", StringComparison.Ordinal))))
        {
            try
            {
                if (format.IsMine(lines, fileName))
                {
                    claimed++;
                }
            }
            catch
            {
                // as Subtitle.IsFormatMine
            }
        }

        return claimed;
    }
}
