using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Round 14 performance hunt: a random sweep over the Netflix quality checkers, the
/// hearing-impaired / fix-common-errors text passes and a few subtitle format writers.
///
/// Every candidate is measured as a *whole-file pass* over a real movie subtitle so the input
/// distribution (tag density, line lengths, dialog dashes, digits) is the one users actually hit.
/// Point <c>SE_BENCH_SUBTITLE</c> at an .srt to use it; otherwise a synthetic corpus is built.
///
/// Both the current shape and the candidate shape live in this file, each with its own direct
/// call site (never behind a shared delegate — that goes megamorphic and lies), and
/// <see cref="Setup"/> asserts the two produce identical output before any timing runs.
/// </summary>
[MemoryDiagnoser]
public class PerfHuntRound14Benchmarks
{
    private string[] _texts = Array.Empty<string>();
    private Subtitle _subtitle = new();

    static PerfHuntRound14Benchmarks()
    {
        // Program.cs does this at start-up; the benchmark host does not.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Cp1252 = Encoding.GetEncoding(1252);
    }

    [GlobalSetup]
    public void Setup()
    {
        _subtitle = LoadCorpus();
        _texts = _subtitle.Paragraphs.Select(p => p.Text).ToArray();
        AssertEquivalence();
    }

    private static Subtitle LoadCorpus()
    {
        var path = Environment.GetEnvironmentVariable("SE_BENCH_SUBTITLE");
        var subtitle = new Subtitle();
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            var lines = File.ReadAllLines(path).ToList();
            new SubRip().LoadSubtitle(subtitle, lines, path);
            if (subtitle.Paragraphs.Count > 0)
            {
                return subtitle;
            }
        }

        // Fallback corpus with the same shape mix as a real movie subtitle.
        var samples = new[]
        {
            "It was the best of times, it was the worst of times.",
            "<i>Are you coming with us?</i>",
            "- No.\r\n- Then stay here and wait.",
            "MAN: There were 3 of them,\r\nand only 1 got away.",
            "[DOOR CREAKING]",
            "(SIGHS) i think i'm going to be sick.",
            "Chapter 10: the long way home...",
            "You'll find it at 10:30, i.e. after lunch.",
            "♪ Somewhere over there ♪",
            "{\\an8}Somewhere in Denmark",
        };
        var ms = 1000.0;
        for (var i = 0; i < 1800; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph(samples[i % samples.Length], ms, ms + 2000));
            ms += 2200;
        }

        return subtitle;
    }

    private void AssertEquivalence()
    {
        foreach (var t in _texts)
        {
            Check("C01", C01_Current(t), C01_Candidate(t));
            Check("C02", C02_Current(t), C02_Candidate(t));
            Check("C03", C03_Current(t), C03_Candidate(t));
            Check("C04", C04_Current(t), C04_Candidate(t));
            Check("C07", C07_Current(t), C07_Candidate(t));
            Check("C10", C10_Current(t), C10_Candidate(t));
            Check("C12", C12_Current(t), C12_Candidate(t));
            Check("C13", C13_Current(t), C13_Candidate(t));
            Check("C14", C14_Current(t), C14_Candidate(t));
            Check("C15", C15_Current(t), C15_Candidate(t));
            Check("C16", C16_Current(t), C16_Candidate(t));
            Check("C17", C17_Current(t), C17_Candidate(t));
            Check("C18", C18_Current(t), C18_Candidate(t));
            Check("C19", C19_Current(t), C19_Candidate(t));
            Check("C22", C22_Current(t), C22_Candidate(t));
            Check("C23", C23_Current(t), C23_Candidate(t));
            Check("C24", C24_Current(t), C24_Candidate(t));
            Check("C25", C25_Current(t), C25_Candidate(t));
        }

        Check("C05", C05_Current().ToString(CultureInfo.InvariantCulture), C05_Candidate().ToString(CultureInfo.InvariantCulture));
        Check("C08", C08_Current().ToString(CultureInfo.InvariantCulture), C08_Candidate().ToString(CultureInfo.InvariantCulture));
        Check("C09", C09_Current().ToString(CultureInfo.InvariantCulture), C09_Candidate().ToString(CultureInfo.InvariantCulture));
        Check("C11", C11_Current().ToString(CultureInfo.InvariantCulture), C11_Candidate().ToString(CultureInfo.InvariantCulture));
        Check("C20", C20_Current(), C20_Candidate());
        Check("C21", C21_Current(), C21_Candidate());
        Check("C06", C06_Current().ToString(CultureInfo.InvariantCulture), C06_Candidate().ToString(CultureInfo.InvariantCulture));
    }

    private static void Check(string name, string current, string candidate)
    {
        if (!string.Equals(current, candidate, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{name}: candidate diverges.\ncurrent  : {current}\ncandidate: {candidate}");
        }
    }

    private static string Fold(string[] texts, Func<string, string> f)
    {
        var sb = new StringBuilder();
        foreach (var t in texts)
        {
            sb.Append(f(t));
        }

        return sb.ToString();
    }

    // ---------------------------------------------------------------------------------------
    // C01 - NetflixCheckNumbersOneToTenSpellOut: `new Regex(@",\d")` constructed inside the
    //       per-match while loop.
    // ---------------------------------------------------------------------------------------
    private static readonly Regex NumberOneToNine = new Regex(@"\b\d\b", RegexOptions.Compiled);
    private static readonly Regex CommaDigitCached = new Regex(@",\d", RegexOptions.Compiled);

    private static string C01_Current(string text)
    {
        var hits = 0;
        var m = NumberOneToNine.Match(text);
        while (m.Success)
        {
            if (m.Index + m.Length < text.Length && text.Substring(m.Index + m.Length).StartsWith(","))
            {
                var rest = text.Substring(m.Index + 1);
                var regex = new Regex(@",\d");
                if (regex.IsMatch(rest))
                {
                    hits++;
                }
            }

            m = NumberOneToNine.Match(text, m.Index + 1);
        }

        return hits.ToString(CultureInfo.InvariantCulture);
    }

    private static string C01_Candidate(string text)
    {
        var hits = 0;
        var m = NumberOneToNine.Match(text);
        while (m.Success)
        {
            if (m.Index + m.Length < text.Length && text[m.Index + m.Length] == ',')
            {
                if (CommaDigitCached.IsMatch(text, m.Index + 1))
                {
                    hits++;
                }
            }

            m = NumberOneToNine.Match(text, m.Index + 1);
        }

        return hits.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C01_RegexInLoop_Current() => Fold(_texts, C01_Current);
    [Benchmark] public string C01_RegexInLoop_Candidate() => Fold(_texts, C01_Candidate);

    // ---------------------------------------------------------------------------------------
    // C02 - NetflixCheckNumbersOneToTenSpellOut: `":.".Contains(text[i].ToString())` (3 sites)
    // ---------------------------------------------------------------------------------------
    private static string C02_Current(string text)
    {
        var n = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (":.".Contains(text[i].ToString()))
            {
                n++;
            }
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    private static string C02_Candidate(string text)
    {
        var n = 0;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == ':' || c == '.')
            {
                n++;
            }
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C02_CharToStringContains_Current() => Fold(_texts, C02_Current);
    [Benchmark] public string C02_CharToStringContains_Candidate() => Fold(_texts, C02_Candidate);

    // ---------------------------------------------------------------------------------------
    // C03 - `text.Substring(i).StartsWith(",")` - allocates the whole tail to test one char.
    // ---------------------------------------------------------------------------------------
    private static string C03_Current(string text)
    {
        var n = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text.Substring(i).StartsWith(","))
            {
                n++;
            }
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    private static string C03_Candidate(string text)
    {
        var n = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == ',')
            {
                n++;
            }
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C03_SubstringStartsWith_Current() => Fold(_texts, C03_Current);
    [Benchmark] public string C03_SubstringStartsWith_Candidate() => Fold(_texts, C03_Candidate);

    // ---------------------------------------------------------------------------------------
    // C04 - the "rest" tail Substring compared against 12 literals.
    // ---------------------------------------------------------------------------------------
    private static readonly string[] RestLiterals =
    {
        ".", "?", "!",
        ".</i>", "?</i>", "!</i>",
        "." + "\r\n", "?" + "\r\n", "!" + "\r\n",
        ".</i>" + "\r\n", "?</i>" + "\r\n", "!</i>" + "\r\n",
    };

    private static string C04_Current(string text)
    {
        var n = 0;
        var m = NumberOneToNine.Match(text);
        while (m.Success)
        {
            var rest = text.Substring(m.Index + m.Length);
            if (rest == "." || rest == "?" || rest == "!" ||
                rest == ".</i>" || rest == "?</i>" || rest == "!</i>" ||
                rest == "." + "\r\n" || rest == "?" + "\r\n" || rest == "!" + "\r\n" ||
                rest == ".</i>" + "\r\n" || rest == "?</i>" + "\r\n" || rest == "!</i>" + "\r\n")
            {
                n++;
            }

            m = NumberOneToNine.Match(text, m.Index + 1);
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    private static string C04_Candidate(string text)
    {
        var n = 0;
        var m = NumberOneToNine.Match(text);
        while (m.Success)
        {
            var rest = text.AsSpan(m.Index + m.Length);
            foreach (var lit in RestLiterals)
            {
                if (rest.SequenceEqual(lit))
                {
                    n++;
                    break;
                }
            }

            m = NumberOneToNine.Match(text, m.Index + 1);
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C04_TailCompare_Current() => Fold(_texts, C04_Current);
    [Benchmark] public string C04_TailCompare_Candidate() => Fold(_texts, C04_Candidate);

    // ---------------------------------------------------------------------------------------
    // C05 - NetflixCheckMaxCps: `new Paragraph(p)` deep clone for every paragraph, needed only
    //       for the Japanese tag-stripping branch.
    // ---------------------------------------------------------------------------------------
    private int C05_Current()
    {
        var calc = CalcFactory.MakeCalculator(nameof(CalcAll));
        var n = 0;
        foreach (var p in _subtitle.Paragraphs)
        {
            var jp = new Paragraph(p);
            if (jp.GetCharactersPerSecond(calc) > 20)
            {
                n++;
            }
        }

        return n;
    }

    private int C05_Candidate()
    {
        var calc = CalcFactory.MakeCalculator(nameof(CalcAll));
        var n = 0;
        foreach (var p in _subtitle.Paragraphs)
        {
            // language != "ja": no tag stripping needed, so measure the original paragraph.
            if (p.GetCharactersPerSecond(calc) > 20)
            {
                n++;
            }
        }

        return n;
    }

    [Benchmark] public int C05_MaxCpsClone_Current() => C05_Current();
    [Benchmark] public int C05_MaxCpsClone_Candidate() => C05_Candidate();

    // ---------------------------------------------------------------------------------------
    // C06 - NetflixCheckMaxCps: CalcFactory.MakeCalculator() resolved inside the paragraph loop.
    // ---------------------------------------------------------------------------------------
    private int C06_Current()
    {
        var n = 0;
        foreach (var p in _subtitle.Paragraphs)
        {
            var calc = CalcFactory.MakeCalculator(nameof(CalcCjk));
            if (p.GetCharactersPerSecond(calc) > 20)
            {
                n++;
            }
        }

        return n;
    }

    private int C06_Candidate()
    {
        var calc = CalcFactory.MakeCalculator(nameof(CalcCjk));
        var n = 0;
        foreach (var p in _subtitle.Paragraphs)
        {
            if (p.GetCharactersPerSecond(calc) > 20)
            {
                n++;
            }
        }

        return n;
    }

    [Benchmark] public int C06_CalcFactoryInLoop_Current() => C06_Current();
    [Benchmark] public int C06_CalcFactoryInLoop_Candidate() => C06_Candidate();

    // ---------------------------------------------------------------------------------------
    // C07 - NetflixCheckTextForHiUseBrackets: SplitToLines() for every paragraph, used only in
    //       the rare two-line-dash branch.
    // ---------------------------------------------------------------------------------------
    private static string C07_Current(string newText)
    {
        var arr = newText.SplitToLines();
        if (newText.StartsWith('(') && newText.EndsWith(')'))
        {
            return "[" + newText.Substring(1, newText.Length - 2) + "]";
        }

        if (arr.Count == 2 && arr[0].StartsWith('-') && arr[1].StartsWith('-'))
        {
            return arr[0] + "\r\n" + arr[1];
        }

        return newText;
    }

    private static string C07_Candidate(string newText)
    {
        if (newText.StartsWith('(') && newText.EndsWith(')'))
        {
            return "[" + newText.Substring(1, newText.Length - 2) + "]";
        }

        if (newText.StartsWith('-'))
        {
            var arr = newText.SplitToLines();
            if (arr.Count == 2 && arr[0].StartsWith('-') && arr[1].StartsWith('-'))
            {
                return arr[0] + "\r\n" + arr[1];
            }
        }

        return newText;
    }

    [Benchmark] public string C07_LazySplitToLines_Current() => Fold(_texts, C07_Current);
    [Benchmark] public string C07_LazySplitToLines_Candidate() => Fold(_texts, C07_Candidate);

    // ---------------------------------------------------------------------------------------
    // C08 - NetflixCheckMaxLineLength: `controller.Language` / `controller.SingleLineMaxLength`
    //       are switch-per-access properties evaluated inside the per-line loop.
    // ---------------------------------------------------------------------------------------
    private sealed class FakeController
    {
        public string Language { get; set; } = "en";

        public int SingleLineMaxLength
        {
            get
            {
                switch (Language)
                {
                    case "ja": return 23;
                    case "th": return 35;
                    case "ko":
                    case "zh": return 16;
                    default: return 42;
                }
            }
        }
    }

    private readonly FakeController _controller = new();

    private int C08_Current()
    {
        var n = 0;
        foreach (var t in _texts)
        {
            foreach (var line in t.SplitToLines())
            {
                if (_controller.Language == "ja")
                {
                    n++;
                }
                else if (_controller.Language == "ko" && line.CountCharacters(nameof(CalcCjk), false) > _controller.SingleLineMaxLength)
                {
                    n++;
                }
                else if (line.CountCharacters(false) > _controller.SingleLineMaxLength)
                {
                    n++;
                }
            }
        }

        return n;
    }

    private int C08_Candidate()
    {
        var n = 0;
        var language = _controller.Language;
        var isJa = language == "ja";
        var isKo = language == "ko";
        var maxLen = _controller.SingleLineMaxLength;
        foreach (var t in _texts)
        {
            foreach (var line in t.SplitToLines())
            {
                if (isJa)
                {
                    n++;
                }
                else if (isKo && line.CountCharacters(nameof(CalcCjk), false) > maxLen)
                {
                    n++;
                }
                else if (line.CountCharacters(false) > maxLen)
                {
                    n++;
                }
            }
        }

        return n;
    }

    [Benchmark] public int C08_HoistControllerProps_Current() => C08_Current();
    [Benchmark] public int C08_HoistControllerProps_Candidate() => C08_Candidate();

    // ---------------------------------------------------------------------------------------
    // C09 - NetflixCheckGlyph: HashSet<int> probe per code point vs a BMP bitmap.
    // ---------------------------------------------------------------------------------------
    private static readonly HashSet<int> GlyphSet = BuildGlyphSet();
    private static readonly bool[] GlyphBmp = BuildGlyphBmp();

    private static HashSet<int> BuildGlyphSet()
    {
        var set = new HashSet<int> { 10, 13 };
        for (var i = 0x20; i <= 0x7E; i++)
        {
            set.Add(i);
        }

        foreach (var cp in new[] { 0x2026, 0x266A, 0x266B, 0xA1, 0xBF, 0xE9, 0xE8, 0xF6, 0xFC, 0x2019, 0x201C, 0x201D })
        {
            set.Add(cp);
        }

        return set;
    }

    private static bool[] BuildGlyphBmp()
    {
        var table = new bool[0x10000];
        foreach (var cp in GlyphSet)
        {
            if (cp < table.Length)
            {
                table[cp] = true;
            }
        }

        return table;
    }

    private int C09_Current()
    {
        var bad = 0;
        foreach (var text in _texts)
        {
            for (int pos = 0; pos < text.Length; pos += char.IsSurrogatePair(text, pos) ? 2 : 1)
            {
                var cp = char.ConvertToUtf32(text, pos);
                if (!GlyphSet.Contains(cp))
                {
                    bad++;
                }
            }
        }

        return bad;
    }

    private int C09_Candidate()
    {
        var bad = 0;
        foreach (var text in _texts)
        {
            for (var pos = 0; pos < text.Length; pos++)
            {
                var c = text[pos];
                if (!char.IsSurrogate(c))
                {
                    if (!GlyphBmp[c])
                    {
                        bad++;
                    }

                    continue;
                }

                var cp = char.ConvertToUtf32(text, pos);
                if (!GlyphSet.Contains(cp))
                {
                    bad++;
                }

                pos++; // consume the low surrogate
            }
        }

        return bad;
    }

    [Benchmark] public int C09_GlyphLookup_Current() => C09_Current();
    [Benchmark] public int C09_GlyphLookup_Candidate() => C09_Candidate();

    // ---------------------------------------------------------------------------------------
    // C10 - NetflixCheckWhiteSpace: two anchored compiled regexes vs direct char tests.
    // ---------------------------------------------------------------------------------------
    private static readonly Regex LineEndingSpaceBefore = new Regex(@"^( |\n|\r\n)[^\s]", RegexOptions.Compiled);
    private static readonly Regex LineEndingSpaceAfter = new Regex(@"[^\s]( |\n|\r\n)$", RegexOptions.Compiled);

    private static string C10_Current(string text)
    {
        var n = 0;
        if (LineEndingSpaceBefore.IsMatch(text))
        {
            n++;
        }

        if (LineEndingSpaceAfter.IsMatch(text))
        {
            n += 2;
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    // What actually shipped. A hand-rolled equivalent of these two patterns is a trap: "$" in
    // .NET also matches *before* a trailing newline, so "a \n" matches LineEndingSpaceAfter.
    // The guard below is provably implied by both patterns instead - each can only match when
    // the first / last character is white space - so it changes nothing but keeps the regex
    // engine out of the loop for ordinary lines.
    private static string C10_Candidate(string text)
    {
        var n = 0;
        if (text.Length > 1 && char.IsWhiteSpace(text[0]) && LineEndingSpaceBefore.IsMatch(text))
        {
            n++;
        }

        if (text.Length > 1 && char.IsWhiteSpace(text[text.Length - 1]) && LineEndingSpaceAfter.IsMatch(text))
        {
            n += 2;
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C10_AnchoredRegex_Current() => Fold(_texts, C10_Current);
    [Benchmark] public string C10_AnchoredRegex_Candidate() => Fold(_texts, C10_Candidate);

    // ---------------------------------------------------------------------------------------
    // C11 - RemoveTextForHI.ShouldRemoveNarrator: three arrays allocated on every call.
    // ---------------------------------------------------------------------------------------
    private static readonly string[] NarratorSkipShort = { "http", ", " };

    private static readonly string[] NarratorSkipEnglish =
    {
        "Previously on", "Improved by", " is ", " are ", " were ", " was ", " think ",
        " guess ", " will ", " believe ", " say ", " said ", " do ", " want ", "That's ",
    };

    private static readonly char[] NarratorSkipChars = { '!', '?', '¿', '¡' };

    private int C11_Current()
    {
        var n = 0;
        foreach (var pre in _texts)
        {
            if (pre.Length > 30 || pre.IndexOfAny(new[] { "http", ", " }, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            if (pre.Length > 15 && pre.IndexOfAny(new[]
                {
                    "Previously on", "Improved by", " is ", " are ", " were ", " was ", " think ",
                    " guess ", " will ", " believe ", " say ", " said ", " do ", " want ", "That's ",
                }, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            if (pre.IndexOfAny(new[] { '!', '?', '¿', '¡' }) < 0)
            {
                n++;
            }
        }

        return n;
    }

    private int C11_Candidate()
    {
        var n = 0;
        foreach (var pre in _texts)
        {
            if (pre.Length > 30 || pre.IndexOfAny(NarratorSkipShort, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            if (pre.Length > 15 && pre.IndexOfAny(NarratorSkipEnglish, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            if (pre.IndexOfAny(NarratorSkipChars) < 0)
            {
                n++;
            }
        }

        return n;
    }

    // Split of the same site: which half of the fix pays? The two string[] literals are pure
    // allocation, but `IndexOfAny(new[] { '!', ... })` hands the JIT a known-length char[] that
    // a static readonly field does not, so the char set is measured on its own.
    private static readonly SearchValues<char> NarratorSkipSearch = SearchValues.Create("!?¿¡");

    private int C11b_Current()
    {
        var n = 0;
        foreach (var pre in _texts)
        {
            if (pre.IndexOfAny(new[] { '!', '?', '¿', '¡' }) < 0)
            {
                n++;
            }
        }

        return n;
    }

    private int C11b_StaticArray()
    {
        var n = 0;
        foreach (var pre in _texts)
        {
            if (pre.IndexOfAny(NarratorSkipChars) < 0)
            {
                n++;
            }
        }

        return n;
    }

    private int C11b_SearchValues()
    {
        var n = 0;
        foreach (var pre in _texts)
        {
            if (pre.AsSpan().IndexOfAny(NarratorSkipSearch) < 0)
            {
                n++;
            }
        }

        return n;
    }

    private int C11c_Current()
    {
        var n = 0;
        foreach (var pre in _texts)
        {
            if (pre.IndexOfAny(new[] { "http", ", " }, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            if (pre.IndexOfAny(new[]
                {
                    "Previously on", "Improved by", " is ", " are ", " were ", " was ", " think ",
                    " guess ", " will ", " believe ", " say ", " said ", " do ", " want ", "That's ",
                }, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            n++;
        }

        return n;
    }

    private int C11c_Static()
    {
        var n = 0;
        foreach (var pre in _texts)
        {
            if (pre.IndexOfAny(NarratorSkipShort, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            if (pre.IndexOfAny(NarratorSkipEnglish, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            n++;
        }

        return n;
    }

    // .NET 9+ multi-substring search: one Aho-Corasick-style pass instead of 15 IndexOf sweeps.
    // ShouldRemoveNarrator only asks "any of these present?", so leftmost-vs-array-order does
    // not matter here.
    private static readonly SearchValues<string> NarratorSkipShortSv =
        SearchValues.Create(new[] { "http", ", " }, StringComparison.OrdinalIgnoreCase);

    private static readonly SearchValues<string> NarratorSkipEnglishSv =
        SearchValues.Create(NarratorSkipEnglish, StringComparison.OrdinalIgnoreCase);

    private int C11c_SearchValues()
    {
        var n = 0;
        foreach (var pre in _texts)
        {
            if (pre.AsSpan().IndexOfAny(NarratorSkipShortSv) >= 0)
            {
                continue;
            }

            if (pre.AsSpan().IndexOfAny(NarratorSkipEnglishSv) >= 0)
            {
                continue;
            }

            n++;
        }

        return n;
    }

    [Benchmark] public int C11c_StringSets_SearchValues() => C11c_SearchValues();

    [Benchmark] public int C11_NarratorArrays_Current() => C11_Current();
    [Benchmark] public int C11_NarratorArrays_Candidate() => C11_Candidate();
    [Benchmark] public int C11b_CharSet_Current() => C11b_Current();
    [Benchmark] public int C11b_CharSet_StaticArray() => C11b_StaticArray();
    [Benchmark] public int C11b_CharSet_SearchValues() => C11b_SearchValues();
    [Benchmark] public int C11c_StringSets_Current() => C11c_Current();
    [Benchmark] public int C11c_StringSets_Static() => C11c_Static();

    // ---------------------------------------------------------------------------------------
    // C12 - RemoveTextForHI.RemovePartialBeforeColon: Substring(i).StartsWith("  ")
    // ---------------------------------------------------------------------------------------
    private static string C12_Current(string text)
    {
        var n = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text.Substring(i).StartsWith("  "))
            {
                n++;
            }
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    private static string C12_Candidate(string text)
    {
        var n = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (i + 1 < text.Length && text[i] == ' ' && text[i + 1] == ' ')
            {
                n++;
            }
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C12_DoubleSpaceProbe_Current() => Fold(_texts, C12_Current);
    [Benchmark] public string C12_DoubleSpaceProbe_Candidate() => Fold(_texts, C12_Candidate);

    // ---------------------------------------------------------------------------------------
    // C13 - RemoveTextForHI: line.Substring(idx + 1).StartsWith(Environment.NewLine)
    // ---------------------------------------------------------------------------------------
    private static string C13_Current(string text)
    {
        var n = 0;
        var idx = text.IndexOf(':');
        while (idx >= 0 && idx < text.Length - 1)
        {
            if (text.Substring(idx + 1).StartsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                n++;
            }

            idx = text.IndexOf(':', idx + 1);
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    private static string C13_Candidate(string text)
    {
        var n = 0;
        var idx = text.IndexOf(':');
        while (idx >= 0 && idx < text.Length - 1)
        {
            if (text.AsSpan(idx + 1).StartsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                n++;
            }

            idx = text.IndexOf(':', idx + 1);
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C13_TailStartsWithNewLine_Current() => Fold(_texts, C13_Current);
    [Benchmark] public string C13_TailStartsWithNewLine_Candidate() => Fold(_texts, C13_Candidate);

    // ---------------------------------------------------------------------------------------
    // C14 - FixMissingSpaces.FixSpaceAfter: "0123456789".Contains(c.ToString())
    // ---------------------------------------------------------------------------------------
    private static string C14_Current(string text)
    {
        var n = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if ("0123456789".Contains(text[i].ToString()))
            {
                n++;
            }
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    private static string C14_Candidate(string text)
    {
        var n = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (char.IsAsciiDigit(text[i]))
            {
                n++;
            }
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C14_DigitProbe_Current() => Fold(_texts, C14_Current);
    [Benchmark] public string C14_DigitProbe_Candidate() => Fold(_texts, C14_Candidate);

    // ---------------------------------------------------------------------------------------
    // C15 - FixMissingSpaces.FixSpaceAfter: separator-set membership via char.ToString()
    // ---------------------------------------------------------------------------------------
    private static readonly SearchValues<char> SpaceAfterSkipLookup =
        SearchValues.Create(" \r\n\":;()[]<>.؟!،");

    private static string C15_Current(string text)
    {
        var n = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (!" \r\n\":;()[]<>.؟!،".Contains(text[i].ToString()))
            {
                n++;
            }
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    private static string C15_Candidate(string text)
    {
        var n = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (!SpaceAfterSkipLookup.Contains(text[i]))
            {
                n++;
            }
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C15_SeparatorSet_Current() => Fold(_texts, C15_Current);
    [Benchmark] public string C15_SeparatorSet_Candidate() => Fold(_texts, C15_Candidate);

    // ---------------------------------------------------------------------------------------
    // C16 - FixAloneLowercaseIToUppercaseLine: `">" + target + "</"` built on every call.
    // ---------------------------------------------------------------------------------------
    private static string C16_Current(string input)
    {
        const char target = 'i';
        return input.Replace(">" + target + "</", ">I</")
                    .Replace(">" + target + " ", ">I ")
                    .Replace(">" + target + "​" + Environment.NewLine, ">I" + Environment.NewLine)
                    .Replace(">" + target + "﻿" + Environment.NewLine, ">I" + Environment.NewLine);
    }

    private static readonly string ILtSlash = ">i</";
    private static readonly string ISpace = ">i ";
    private static readonly string IZwsp = ">i​" + "\r\n";
    private static readonly string IZwnbsp = ">i﻿" + "\r\n";
    private static readonly string INewLine = ">I" + "\r\n";

    private static string C16_Candidate(string input)
    {
        return input.Replace(ILtSlash, ">I</")
                    .Replace(ISpace, ">I ")
                    .Replace(IZwsp, INewLine)
                    .Replace(IZwnbsp, INewLine);
    }

    [Benchmark] public string C16_ReplaceLiterals_Current() => Fold(_texts, C16_Current);
    [Benchmark] public string C16_ReplaceLiterals_Candidate() => Fold(_texts, C16_Candidate);

    // ---------------------------------------------------------------------------------------
    // C17 - FixAloneLowercaseIToUppercaseLine: s.Substring(i).StartsWith("i.e."/"i-")
    // ---------------------------------------------------------------------------------------
    private static readonly Regex LittleI = new Regex(@"\bi\b", RegexOptions.Compiled);

    private static string C17_Current(string s)
    {
        var n = 0;
        var match = LittleI.Match(s);
        while (match.Success)
        {
            if (!s.Substring(match.Index).StartsWith("i.e.", StringComparison.Ordinal) &&
                !s.Substring(match.Index).StartsWith("i-", StringComparison.Ordinal))
            {
                n++;
            }

            match = match.NextMatch();
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    private static string C17_Candidate(string s)
    {
        var n = 0;
        var match = LittleI.Match(s);
        while (match.Success)
        {
            var tail = s.AsSpan(match.Index);
            if (!tail.StartsWith("i.e.", StringComparison.Ordinal) &&
                !tail.StartsWith("i-", StringComparison.Ordinal))
            {
                n++;
            }

            match = match.NextMatch();
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C17_LittleITail_Current() => Fold(_texts, C17_Current);
    [Benchmark] public string C17_LittleITail_Candidate() => Fold(_texts, C17_Candidate);

    // ---------------------------------------------------------------------------------------
    // C18 - FixAloneLowercaseIToUppercaseLine: wholePrev = s.Substring(0, i-1) then
    //       .TrimEnd().EndsWith("...") - two allocations to look at three characters.
    // ---------------------------------------------------------------------------------------
    private static string C18_Current(string s)
    {
        var n = 0;
        var match = LittleI.Match(s);
        while (match.Success)
        {
            var wholePrev = string.Empty;
            if (match.Index > 1)
            {
                wholePrev = s.Substring(0, match.Index - 1);
            }

            if (!wholePrev.TrimEnd().EndsWith("...", StringComparison.Ordinal))
            {
                n++;
            }

            match = match.NextMatch();
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    private static string C18_Candidate(string s)
    {
        var n = 0;
        var match = LittleI.Match(s);
        while (match.Success)
        {
            var end = match.Index > 1 ? match.Index - 1 : 0;
            var prev = s.AsSpan(0, end).TrimEnd();
            if (!prev.EndsWith("...", StringComparison.Ordinal))
            {
                n++;
            }

            match = match.NextMatch();
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C18_WholePrevTrim_Current() => Fold(_texts, C18_Current);
    [Benchmark] public string C18_WholePrevTrim_Candidate() => Fold(_texts, C18_Candidate);

    // ---------------------------------------------------------------------------------------
    // C19 - FixAloneLowercaseIToUppercaseLine: (Environment.NewLine + @" <>!.?:;,").Contains(c)
    //       concatenates a fresh string on every evaluation.
    // ---------------------------------------------------------------------------------------
    private static readonly string LittleIStopChars = Environment.NewLine + @" <>!.?:;,";

    private static string C19_Current(string s)
    {
        var n = 0;
        for (var i = 0; i < s.Length; i++)
        {
            if (!(Environment.NewLine + @" <>!.?:;,").Contains(s[i]))
            {
                n++;
            }
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    private static string C19_Candidate(string s)
    {
        var n = 0;
        for (var i = 0; i < s.Length; i++)
        {
            if (!LittleIStopChars.Contains(s[i]))
            {
                n++;
            }
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C19_StopCharConcat_Current() => Fold(_texts, C19_Current);
    [Benchmark] public string C19_StopCharConcat_Candidate() => Fold(_texts, C19_Candidate);

    // ---------------------------------------------------------------------------------------
    // C20 - ScenaristClosedCaptions.GetCodeFromLetter: LINQ FirstOrDefault over a 339-entry
    //       KeyValuePair list, once per character written.
    // ---------------------------------------------------------------------------------------
    private static readonly List<KeyValuePair<string, string>> SccLetters = BuildSccLetters();
    private static readonly Dictionary<string, string> SccReverse = BuildSccReverse();

    private static List<KeyValuePair<string, string>> BuildSccLetters()
    {
        // Same shape/size as the real table: 339 entries, printable ASCII early, accents late.
        var list = new List<KeyValuePair<string, string>>(339);
        for (var c = 0x20; c <= 0x7E; c++)
        {
            list.Add(new KeyValuePair<string, string>(c.ToString("x2"), ((char)c).ToString()));
        }

        var extra = 0x100;
        while (list.Count < 339)
        {
            list.Add(new KeyValuePair<string, string>(extra.ToString("x4"), ((char)extra).ToString()));
            extra++;
        }

        return list;
    }

    private static Dictionary<string, string> BuildSccReverse()
    {
        var d = new Dictionary<string, string>(SccLetters.Count, StringComparer.Ordinal);
        foreach (var kv in SccLetters)
        {
            if (!d.ContainsKey(kv.Value))
            {
                d.Add(kv.Value, kv.Key);
            }
        }

        return d;
    }

    private static string GetCodeFromLetter_Current(string letter)
    {
        var code = SccLetters.FirstOrDefault(x => x.Value == letter);
        if (code.Equals(new KeyValuePair<string, string>()))
        {
            return null;
        }

        return code.Key;
    }

    private static string GetCodeFromLetter_Candidate(string letter)
    {
        return SccReverse.TryGetValue(letter, out var code) ? code : null;
    }

    private string C20_Current()
    {
        var sb = new StringBuilder();
        foreach (var text in _texts)
        {
            for (var i = 0; i < text.Length; i++)
            {
                sb.Append(GetCodeFromLetter_Current(text.Substring(i, 1)) ?? "--");
            }
        }

        return sb.Length.ToString(CultureInfo.InvariantCulture);
    }

    private string C20_Candidate()
    {
        var sb = new StringBuilder();
        foreach (var text in _texts)
        {
            for (var i = 0; i < text.Length; i++)
            {
                sb.Append(GetCodeFromLetter_Candidate(text.Substring(i, 1)) ?? "--");
            }
        }

        return sb.Length.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C20_SccCodeLookup_Current() => C20_Current();
    [Benchmark] public string C20_SccCodeLookup_Candidate() => C20_Candidate();

    // ---------------------------------------------------------------------------------------
    // C21 - ScenaristClosedCaptions.ToText: text.Substring(i, 1) allocated for every character.
    // ---------------------------------------------------------------------------------------
    private string C21_Current()
    {
        var sb = new StringBuilder();
        foreach (var text in _texts)
        {
            for (var i = 0; i < text.Length; i++)
            {
                var s = text.Substring(i, 1);
                sb.Append(GetCodeFromLetter_Candidate(s) ?? "--");
            }
        }

        return sb.Length.ToString(CultureInfo.InvariantCulture);
    }

    private static readonly Dictionary<char, string> SccReverseChar = BuildSccReverseChar();

    private static Dictionary<char, string> BuildSccReverseChar()
    {
        var d = new Dictionary<char, string>(SccLetters.Count);
        foreach (var kv in SccLetters)
        {
            if (kv.Value.Length == 1 && !d.ContainsKey(kv.Value[0]))
            {
                d.Add(kv.Value[0], kv.Key);
            }
        }

        return d;
    }

    private string C21_Candidate()
    {
        var sb = new StringBuilder();
        foreach (var text in _texts)
        {
            for (var i = 0; i < text.Length; i++)
            {
                sb.Append(SccReverseChar.TryGetValue(text[i], out var code) ? code : "--");
            }
        }

        return sb.Length.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C21_SccPerCharSubstring_Current() => C21_Current();
    [Benchmark] public string C21_SccPerCharSubstring_Candidate() => C21_Candidate();

    // ---------------------------------------------------------------------------------------
    // C22 - ScenaristClosedCaptions.ToText: text.Substring(i).StartsWith("<i>"/"</i>") inside
    //       the per-character loop (quadratic in line length).
    // ---------------------------------------------------------------------------------------
    private static string C22_Current(string text)
    {
        var italic = 0;
        var i = 0;
        while (i < text.Length)
        {
            if (text.Substring(i).StartsWith("<i>", StringComparison.Ordinal))
            {
                i += 3;
                italic++;
            }
            else if (text.Substring(i).StartsWith("</i>", StringComparison.Ordinal) && italic > 0)
            {
                i += 4;
                italic--;
            }
            else
            {
                i++;
            }
        }

        return italic.ToString(CultureInfo.InvariantCulture);
    }

    private static string C22_Candidate(string text)
    {
        var italic = 0;
        var i = 0;
        while (i < text.Length)
        {
            var tail = text.AsSpan(i);
            if (tail.StartsWith("<i>", StringComparison.Ordinal))
            {
                i += 3;
                italic++;
            }
            else if (tail.StartsWith("</i>", StringComparison.Ordinal) && italic > 0)
            {
                i += 4;
                italic--;
            }
            else
            {
                i++;
            }
        }

        return italic.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C22_SccItalicScan_Current() => Fold(_texts, C22_Current);
    [Benchmark] public string C22_SccItalicScan_Candidate() => Fold(_texts, C22_Candidate);

    // ---------------------------------------------------------------------------------------
    // C23 - FinalCutProXmlCaptions.SplitStyledRuns: four Substring(i).StartsWith per '<'.
    // ---------------------------------------------------------------------------------------
    private static string C23_Current(string input)
    {
        var sb = new StringBuilder();
        var runs = 0;
        var i = 0;
        while (i < input.Length)
        {
            if (input[i] == '<')
            {
                var toggled = true;
                if (input.Substring(i).StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                {
                    runs++;
                    i += 3;
                }
                else if (input.Substring(i).StartsWith("</i>", StringComparison.OrdinalIgnoreCase))
                {
                    runs++;
                    i += 4;
                }
                else if (input.Substring(i).StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                {
                    runs++;
                    i += 3;
                }
                else if (input.Substring(i).StartsWith("</b>", StringComparison.OrdinalIgnoreCase))
                {
                    runs++;
                    i += 4;
                }
                else
                {
                    toggled = false;
                }

                if (toggled)
                {
                    continue;
                }
            }

            sb.Append(input[i]);
            i++;
        }

        return runs.ToString(CultureInfo.InvariantCulture) + ":" + sb.Length.ToString(CultureInfo.InvariantCulture);
    }

    private static string C23_Candidate(string input)
    {
        var sb = new StringBuilder();
        var runs = 0;
        var i = 0;
        while (i < input.Length)
        {
            if (input[i] == '<')
            {
                var toggled = true;
                var tail = input.AsSpan(i);
                if (tail.StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                {
                    runs++;
                    i += 3;
                }
                else if (tail.StartsWith("</i>", StringComparison.OrdinalIgnoreCase))
                {
                    runs++;
                    i += 4;
                }
                else if (tail.StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                {
                    runs++;
                    i += 3;
                }
                else if (tail.StartsWith("</b>", StringComparison.OrdinalIgnoreCase))
                {
                    runs++;
                    i += 4;
                }
                else
                {
                    toggled = false;
                }

                if (toggled)
                {
                    continue;
                }
            }

            sb.Append(input[i]);
            i++;
        }

        return runs.ToString(CultureInfo.InvariantCulture) + ":" + sb.Length.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C23_FcpStyledRuns_Current() => Fold(_texts, C23_Current);
    [Benchmark] public string C23_FcpStyledRuns_Candidate() => Fold(_texts, C23_Candidate);

    // ---------------------------------------------------------------------------------------
    // C24 - CheetahCaption.ToText: Substring(j).StartsWith(NewLine) per character, plus
    //       Encoding.GetEncoding(1252) resolved for every paragraph.
    // ---------------------------------------------------------------------------------------
    private static string C24_Current(string text)
    {
        var encoding = Encoding.GetEncoding(1252);
        var bytes = 0;
        var j = 0;
        while (j < text.Length)
        {
            if (text.Substring(j).StartsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                j += Environment.NewLine.Length;
                bytes += 4;
            }
            else
            {
                bytes += encoding.GetByteCount(text.AsSpan(j, 1));
                j++;
            }
        }

        return bytes.ToString(CultureInfo.InvariantCulture);
    }

    private static readonly Encoding Cp1252;

    private static string C24_Candidate(string text)
    {
        var encoding = Cp1252;
        var bytes = 0;
        var j = 0;
        while (j < text.Length)
        {
            if (text.AsSpan(j).StartsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                j += Environment.NewLine.Length;
                bytes += 4;
            }
            else
            {
                bytes += encoding.GetByteCount(text.AsSpan(j, 1));
                j++;
            }
        }

        return bytes.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C24_CheetahScan_Current() => Fold(_texts, C24_Current);
    [Benchmark] public string C24_CheetahScan_Candidate() => Fold(_texts, C24_Candidate);

    // ---------------------------------------------------------------------------------------
    // C25 - DialogSplitMerge: l.TrimStart().StartsWith(dash) allocates a trimmed copy of every
    //       line only to look at its first non-space character.
    // ---------------------------------------------------------------------------------------
    private static string C25_Current(string text)
    {
        var n = 0;
        foreach (var l in text.SplitToLines())
        {
            if (l.TrimStart().StartsWith('-'))
            {
                n++;
            }
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    private static string C25_Candidate(string text)
    {
        var n = 0;
        foreach (var l in text.SplitToLines())
        {
            var span = l.AsSpan().TrimStart();
            if (span.Length > 0 && span[0] == '-')
            {
                n++;
            }
        }

        return n.ToString(CultureInfo.InvariantCulture);
    }

    [Benchmark] public string C25_DialogTrimStart_Current() => Fold(_texts, C25_Current);
    [Benchmark] public string C25_DialogTrimStart_Candidate() => Fold(_texts, C25_Candidate);
}
