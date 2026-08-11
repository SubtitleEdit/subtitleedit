using System.Linq;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// <c>StringExtensions.CountWords</c>: the words-per-minute column calls it once per visible row
/// on every grid repaint, and the statistics window calls it once per paragraph.
/// <c>Old</c> is the tag-stripping implementation it replaced, kept here so the comparison can be
/// re-run without a stash dance.
/// </summary>
[MemoryDiagnoser]
public class CountWordsBenchmarks
{
    public static readonly string[] Lines =
    {
        "Hello there, how are you?",
        "<i>Hello there,</i> how are you?",
        "{\\an8}Hello there, how are you?",
        "Hello there,\r\nhow are you today my friend?",
        "- What are you doing in there?\r\n- Nothing much, really.",
        "<font color=\"red\">Wow!</font> {\\i1}nice{\\i0} one",
        "The quick brown fox jumps over the lazy dog while the tired cat watches from the window " +
        "and nobody in the house says a single word about any of it.",
    };

    [Params(0, 1, 2, 3, 4, 5, 6)]
    public int LineIndex { get; set; }

    private string _line;

    [GlobalSetup]
    public void Setup() => _line = Lines[LineIndex];

    [Benchmark(Baseline = true)]
    public int Old() => Old(_line);

    [Benchmark]
    public int Current() => _line.CountWords();

    /// <summary>
    /// One statistics-window pass / one full words-per-minute recompute: every line of a
    /// subtitle. Set <c>SE_BENCH_SUBTITLE</c> to a subtitle file to run over that file instead of
    /// the built-in corpus.
    /// </summary>
    [MemoryDiagnoser]
    public class WholeFile
    {
        private string[] _lines;

        [GlobalSetup]
        public void Setup()
        {
            var path = Environment.GetEnvironmentVariable("SE_BENCH_SUBTITLE");
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                _lines = Subtitle.Parse(path).Paragraphs.Select(p => p.Text).ToArray();
                return;
            }

            var lines = new List<string>();
            for (var i = 0; i < 200; i++)
            {
                lines.AddRange(Lines);
            }

            _lines = lines.ToArray();
        }

        [Benchmark(Baseline = true)]
        public int Old()
        {
            var total = 0;
            foreach (var line in _lines)
            {
                total += CountWordsBenchmarks.Old(line);
            }

            return total;
        }

        [Benchmark]
        public int Current()
        {
            var total = 0;
            foreach (var line in _lines)
            {
                total += line.CountWords();
            }

            return total;
        }
    }

    /// <summary>The implementation this replaced: strip tags into a new string, then count runs.</summary>
    private static int Old(string source)
    {
        var text = HtmlUtil.RemoveHtmlTags(source, true);
        var count = 0;
        var inWord = false;
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (ch == ' ' || ch == '\n' || ch == '\r')
            {
                inWord = false;
            }
            else if (!inWord)
            {
                inWord = true;
                count++;
            }
        }

        return count;
    }
}
