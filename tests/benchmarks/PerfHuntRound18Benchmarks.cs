using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Text;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Round 18 performance hunt. These benchmarks drive the public entry points (LoadSubtitle,
/// ToText, the converters and fix helpers) so the same file can be run against a baseline
/// checkout and the branch; <see cref="SubRipControl"/> is the untouched drift control.
///
/// Themes: (a) TTML readers that re-ran a document-wide XPath (or, for Rosetta, re-parsed
/// its whole header template) per span / per attribute; (b) the copy-pasted TTML writer loop
/// that allocated <c>line.Substring(i)</c> for every tag probe of every character and grew
/// the node text one character at a time through <c>InnerText +=</c>; (c) small per-paragraph
/// helpers that rebuilt constant tables or linearly scanned a large name list.
///
/// Default job, in-process, Apple M4, .NET 10 (SubRipControl 86.3 us -> 84.4 us, identical
/// allocations, so the before/after pair is trustworthy):
///
///   Rosetta load (400 cues)          36,127 us -> 734 us    49x   56.2 MB -> 1.5 MB allocated
///   Netflix Japanese load (300)      58,003 us -> 1,498 us  39x   23.8 MB -> 2.1 MB
///   TimedText10 load (400)           55,855 us -> 2,064 us  27x   6.6 MB -> 2.5 MB
///   Netflix Timed Text save (400)     4,272 us -> 1,574 us  2.7x  24.2 MB -> 2.3 MB
///   SanitizeString, plain lines          88 us -> 41.8 us   2.1x
///   FixDialogsOnOneLine                 105 us -> 58.4 us   1.8x  356 KB -> 77 KB
///   SMPTE-TT 2052 save (400)          9,164 us -> 6,919 us  1.32x 21.9 MB -> 2.7 MB
///   iTunes Timed Text save (400)      9,237 us -> 7,060 us  1.31x 21.3 MB -> 2.1 MB
///   DFXP save (400)                   8,741 us -> 6,701 us  1.30x 21.4 MB -> 2.3 MB
///   FixHyphensRemoveDashSingleLine      271 us -> 217 us    1.25x
///   Netflix Japanese -> ASSA (300)      781 us -> 663 us    1.18x 2.2 MB -> 1.3 MB
///   ActorConverter.IsActor (7 probes)   594 us -> 0.29 us   ~2000x (self-contained old/new)
/// </summary>
[MemoryDiagnoser]
public class PerfHuntRound18Benchmarks
{
    private Subtitle _taggedSubtitle = new();
    private Subtitle _japaneseSubtitle = new();
    private Subtitle _dashSubtitle = new();
    private List<string> _ttmlLines = new();
    private List<string> _rosettaLines = new();
    private List<string> _netflixJapaneseLines = new();
    private string[] _hyphenTexts = Array.Empty<string>();
    private string[] _plainTexts = Array.Empty<string>();
    private List<string> _names = new();
    private HashSet<string> _nameSet = new(StringComparer.OrdinalIgnoreCase);
    private string[] _actorProbes = Array.Empty<string>();

    [GlobalSetup]
    public void Setup()
    {
        _taggedSubtitle = BuildTaggedSubtitle(400);
        _japaneseSubtitle = BuildJapaneseSubtitle(300);
        _dashSubtitle = BuildDashSubtitle(400);
        _ttmlLines = BuildTtmlDocument(400).SplitToLines();
        _rosettaLines = BuildRosettaDocument(400).SplitToLines();
        _netflixJapaneseLines = BuildNetflixJapaneseDocument(300).SplitToLines();
        _hyphenTexts = _dashSubtitle.Paragraphs.Select(p => p.Text).ToArray();
        _plainTexts = _taggedSubtitle.Paragraphs.Select(p => p.Text).ToArray();
        BuildNames();
        AssertActorEquivalence();
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
                1 => $"He said: <b>no</b>.{Environment.NewLine}<font color=\"#ffff00\">Yellow {i}</font> and more.",
                2 => $"{Sentence(i)}{Environment.NewLine}<i>Whispered</i> reply.",
                _ => Sentence(i),
            };
            s.Paragraphs.Add(new Paragraph(text, i * 3000, i * 3000 + 2500));
        }

        return s;
    }

    private static Subtitle BuildJapaneseSubtitle(int count)
    {
        var s = new Subtitle();
        for (var i = 0; i < count; i++)
        {
            var text = (i % 3) switch
            {
                0 => $"<ruby-container><ruby-base>漢字</ruby-base><ruby-text>かんじ</ruby-text></ruby-container>を読む{i}。{Environment.NewLine}<i>そして続く</i>",
                1 => $"<bouten-filled-circle>強調</bouten-filled-circle>された言葉と<horizontalDigit>{i % 100}</horizontalDigit>時。",
                _ => $"普通の行です{i}。{Environment.NewLine}二行目の文章。",
            };
            s.Paragraphs.Add(new Paragraph(text, i * 3000, i * 3000 + 2500));
        }

        return s;
    }

    private static Subtitle BuildDashSubtitle(int count)
    {
        var s = new Subtitle();
        for (var i = 0; i < count; i++)
        {
            var text = (i % 4) switch
            {
                0 => $"- Where are you going, number {i}?",
                1 => $"- Out. - Fine, go then {i}!",
                2 => $"<i>- Out.</i> - Fine {i}.",
                _ => $"Nothing to fix in cue {i}.",
            };
            s.Paragraphs.Add(new Paragraph(text, i * 3000, i * 3000 + 2500));
        }

        return s;
    }

    private static string BuildTtmlDocument(int count)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xml:lang=\"en\">");
        sb.AppendLine("  <head>");
        sb.AppendLine("    <styling>");
        for (var i = 0; i < 40; i++)
        {
            sb.AppendLine($"      <style xml:id=\"s{i}\" tts:fontFamily=\"Arial\" tts:fontSize=\"100%\" tts:color=\"#ffffff\" />");
        }

        sb.AppendLine("      <style xml:id=\"italic\" tts:fontStyle=\"italic\" />");
        sb.AppendLine("      <style xml:id=\"bold\" tts:fontWeight=\"bold\" />");
        sb.AppendLine("    </styling>");
        sb.AppendLine("    <layout>");
        for (var i = 0; i < 10; i++)
        {
            sb.AppendLine($"      <region xml:id=\"r{i}\" tts:origin=\"10% {10 + i * 5}%\" tts:extent=\"80% 20%\" tts:displayAlign=\"after\" />");
        }

        sb.AppendLine("    </layout>");
        sb.AppendLine("  </head>");
        sb.AppendLine("  <body style=\"s0\">");
        sb.AppendLine("    <div>");
        for (var i = 0; i < count; i++)
        {
            var begin = TimeSpan.FromMilliseconds(i * 3000).ToString(@"hh\:mm\:ss\.fff");
            var end = TimeSpan.FromMilliseconds(i * 3000 + 2500).ToString(@"hh\:mm\:ss\.fff");
            sb.AppendLine($"      <p begin=\"{begin}\" end=\"{end}\" region=\"r{i % 10}\" style=\"s{i % 40}\"><span style=\"italic\">Cue {i}</span> says <span style=\"s{(i + 1) % 40}\">something</span><br/><span style=\"bold\">and more {i}</span></p>");
        }

        sb.AppendLine("    </div>");
        sb.AppendLine("  </body>");
        sb.AppendLine("</tt>");
        return sb.ToString();
    }

    private static string BuildRosettaDocument(int count)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" xmlns:ebutts=\"urn:ebu:tt:style\" xmlns:itts=\"http://www.w3.org/ns/ttml/profile/imsc1#styling\" ttp:timeBase=\"media\" xml:lang=\"en\">");
        sb.AppendLine("  <head>");
        sb.AppendLine("    <styling>");
        sb.AppendLine("      <style xml:id=\"_r_default\" style=\"s_fg_white p_al_center\" tts:fontSize=\"100%\" />");
        sb.AppendLine("      <style xml:id=\"s_fg_white\" tts:color=\"#FFFFFF\" />");
        sb.AppendLine("      <style xml:id=\"s_fg_yellow\" tts:color=\"#FFFF00\" />");
        sb.AppendLine("      <style xml:id=\"s_fg_cyan\" tts:color=\"#00FFFF\" />");
        sb.AppendLine("      <style xml:id=\"s_italic\" tts:fontStyle=\"italic\" />");
        sb.AppendLine("      <style xml:id=\"p_al_center\" tts:textAlign=\"center\" />");
        sb.AppendLine("    </styling>");
        sb.AppendLine("    <layout>");
        sb.AppendLine("      <region xml:id=\"r_bottom\" tts:origin=\"10% 80%\" tts:extent=\"80% 10%\" tts:displayAlign=\"after\" />");
        sb.AppendLine("    </layout>");
        sb.AppendLine("  </head>");
        sb.AppendLine("  <body>");
        sb.AppendLine("    <div>");
        for (var i = 0; i < count; i++)
        {
            var begin = TimeSpan.FromMilliseconds(i * 3000).ToString(@"hh\:mm\:ss\.fff");
            var end = TimeSpan.FromMilliseconds(i * 3000 + 2500).ToString(@"hh\:mm\:ss\.fff");
            var color = i % 2 == 0 ? "s_fg_yellow" : "s_fg_cyan";
            sb.AppendLine($"      <p begin=\"{begin}\" end=\"{end}\" region=\"r_bottom\" style=\"_r_default\"><span style=\"s_italic\">Cue {i}</span> says <span style=\"{color}\">something</span><br/>and more {i}</p>");
        }

        sb.AppendLine("    </div>");
        sb.AppendLine("  </body>");
        sb.AppendLine("</tt>");
        return sb.ToString();
    }

    /// <summary>A "real" Netflix document: numbered regions and style references, not the names SE writes.</summary>
    private static string BuildNetflixJapaneseDocument(int count)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<tt xmlns=\"http://www.w3.org/ns/ttml\" xmlns:tts=\"http://www.w3.org/ns/ttml#styling\" xmlns:ttp=\"http://www.w3.org/ns/ttml#parameter\" xmlns:ebutts=\"urn:ebu:tt:style\" ttp:timeBase=\"media\" xml:lang=\"ja\">");
        sb.AppendLine("  <head>");
        sb.AppendLine("    <styling>");
        sb.AppendLine("      <initial tts:color=\"white\" tts:fontFamily=\"proportionalSansSerif\" tts:displayAlign=\"after\" />");
        for (var i = 0; i < 30; i++)
        {
            sb.AppendLine($"      <style xml:id=\"style{i}\" tts:fontSize=\"{80 + i}%\" tts:textAlign=\"center\" />");
        }

        sb.AppendLine("      <style xml:id=\"italic\" tts:shear=\"16.67%\" />");
        sb.AppendLine("      <style xml:id=\"rubyContainer\" tts:ruby=\"container\" />");
        sb.AppendLine("      <style xml:id=\"rubyBase\" tts:ruby=\"base\" />");
        sb.AppendLine("      <style xml:id=\"rubyText\" tts:ruby=\"text\" tts:rubyPosition=\"before\" />");
        sb.AppendLine("      <style xml:id=\"bouten\" tts:textEmphasis=\"filled circle before\" />");
        sb.AppendLine("      <style xml:id=\"tcy\" tts:textCombine=\"all\" />");
        sb.AppendLine("    </styling>");
        sb.AppendLine("    <layout>");
        for (var i = 0; i < 10; i++)
        {
            var mode = i % 3 == 0 ? " tts:writingMode=\"tbrl\"" : string.Empty;
            sb.AppendLine($"      <region xml:id=\"region{i}\" style=\"style{i}\" tts:origin=\"10% {10 + i * 5}%\" tts:extent=\"80% 80%\"{mode} />");
        }

        sb.AppendLine("    </layout>");
        sb.AppendLine("  </head>");
        sb.AppendLine("  <body>");
        sb.AppendLine("    <div>");
        for (var i = 0; i < count; i++)
        {
            var begin = TimeSpan.FromMilliseconds(i * 3000).ToString(@"hh\:mm\:ss\.fff");
            var end = TimeSpan.FromMilliseconds(i * 3000 + 2500).ToString(@"hh\:mm\:ss\.fff");
            var content = (i % 3) switch
            {
                0 => $"<span style=\"rubyContainer\"><span style=\"rubyBase\">漢字</span><span style=\"rubyText\">かんじ</span></span>を読む{i}。<br/><span style=\"italic style{i % 30}\">そして続く</span>",
                1 => $"<span style=\"bouten\">強調</span>された言葉と<span style=\"tcy\">{i % 100}</span>時。",
                _ => $"普通の行です{i}。<br/>二行目の文章。",
            };
            sb.AppendLine($"      <p begin=\"{begin}\" end=\"{end}\" region=\"region{i % 10}\" style=\"style{i % 30}\">{content}</p>");
        }

        sb.AppendLine("    </div>");
        sb.AppendLine("  </body>");
        sb.AppendLine("</tt>");
        return sb.ToString();
    }

    private void BuildNames()
    {
        // NameList.GetAllNames() is ~30k entries for English; model that size.
        var rnd = new Random(42);
        for (var i = 0; i < 30000; i++)
        {
            var len = 4 + rnd.Next(6);
            var chars = new char[len];
            for (var j = 0; j < len; j++)
            {
                chars[j] = (char)('a' + rnd.Next(26));
            }

            chars[0] = char.ToUpperInvariant(chars[0]);
            _names.Add(new string(chars));
        }

        _names.Add("John");
        _names.Add("Mary Ann");
        _nameSet = new HashSet<string>(_names, StringComparer.OrdinalIgnoreCase);
        _actorProbes = new[] { "John", "MARY ANN", "Mr. Nobody", "Zzzz", _names[15000], _names[29990] + " " + _names[100], "It's 12:30" };
    }

    // ------------------------------------------------------------------ TTML readers

    [Benchmark]
    public int TimedText10Load()
    {
        var s = new Subtitle();
        new TimedText10().LoadSubtitle(s, _ttmlLines, "in.ttml");
        return s.Paragraphs.Count;
    }

    [Benchmark]
    public int RosettaLoad()
    {
        var s = new Subtitle();
        new TimedTextImscRosetta().LoadSubtitle(s, _rosettaLines, "in.ttml");
        return s.Paragraphs.Count;
    }

    [Benchmark]
    public int NetflixJapaneseLoad()
    {
        var s = new Subtitle();
        new NetflixImsc11Japanese().LoadSubtitle(s, _netflixJapaneseLines, "in.ttml");
        return s.Paragraphs.Count;
    }

    // ------------------------------------------------------------------ TTML writers

    [Benchmark]
    public int DfxpBasicSave() => new DfxpBasic().ToText(_taggedSubtitle, "t").Length;

    [Benchmark]
    public int NetflixTimedTextSave() => new NetflixTimedText().ToText(_taggedSubtitle, "t").Length;

    [Benchmark]
    public int SmpteTt2052Save() => new SmpteTt2052().ToText(_taggedSubtitle, "t").Length;

    [Benchmark]
    public int ItunesTimedTextSave() => new ItunesTimedText().ToText(_taggedSubtitle, "t").Length;

    // ------------------------------------------------------------------ converters / fixes

    [Benchmark]
    public int NetflixJapaneseToAss() => NetflixImsc11JapaneseToAss.Convert(_japaneseSubtitle, 1920, 1080).Length;

    [Benchmark]
    public int FixDialogsOnOneLine()
    {
        var total = 0;
        for (var i = 0; i < _hyphenTexts.Length; i++)
        {
            total += Helper.FixDialogsOnOneLine(_hyphenTexts[i], "en").Length;
        }

        return total;
    }

    [Benchmark]
    public int FixHyphensRemoveDashSingleLineFix()
    {
        var copy = new Subtitle(_dashSubtitle);
        new FixHyphensRemoveDashSingleLine().Fix(copy, new EmptyFixCallback());
        return copy.Paragraphs.Count;
    }

    [Benchmark]
    public int SanitizeStringPlain()
    {
        var total = 0;
        foreach (var t in _plainTexts)
        {
            total += ContinuationUtilities.SanitizeString(t).Length;
        }

        return total;
    }

    // ------------------------------------------------------------------ ActorConverter.IsActor (self-contained old vs new)

    private static readonly HashSet<string> CommonTitles = new(StringComparer.OrdinalIgnoreCase) { "Mr.", "Mrs.", "Ms.", "Dr." };

    private void AssertActorEquivalence()
    {
        foreach (var probe in _actorProbes)
        {
            if (IsActorOld(probe) != IsActorNew(probe))
            {
                throw new Exception("IsActor old/new differ for " + probe);
            }
        }
    }

    private bool IsActorOld(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        var words = s.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return false;
        }

        if (_names.Contains(s, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var word in words)
        {
            if (word.Length < 2)
            {
                return false;
            }

            if (CommonTitles.Contains(word))
            {
                continue;
            }

            if (word.Any(c => char.IsDigit(c) || (!char.IsLetter(c) && c != '-' && c != '\'')))
            {
                return false;
            }

            if (!_names.Contains(word, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsActorNew(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        var words = s.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return false;
        }

        if (_nameSet.Contains(s))
        {
            return true;
        }

        foreach (var word in words)
        {
            if (word.Length < 2)
            {
                return false;
            }

            if (CommonTitles.Contains(word))
            {
                continue;
            }

            if (word.Any(c => char.IsDigit(c) || (!char.IsLetter(c) && c != '-' && c != '\'')))
            {
                return false;
            }

            if (!_nameSet.Contains(word))
            {
                return false;
            }
        }

        return true;
    }

    [Benchmark]
    public int IsActorOldList()
    {
        var hits = 0;
        foreach (var probe in _actorProbes)
        {
            if (IsActorOld(probe))
            {
                hits++;
            }
        }

        return hits;
    }

    [Benchmark]
    public int IsActorNewSet()
    {
        var hits = 0;
        foreach (var probe in _actorProbes)
        {
            if (IsActorNew(probe))
            {
                hits++;
            }
        }

        return hits;
    }

    // ------------------------------------------------------------------ drift control

    [Benchmark]
    public int SubRipControl() => new SubRip().ToText(_taggedSubtitle, "t").Length;
}
