using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Text;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Saving: SubtitleFormat.ToText for the two most-used formats. ASSA gets a styled header
/// (styles.Contains per paragraph) and SubRip the plain number/timecode/text path.
/// </summary>
[MemoryDiagnoser]
public class ToTextBenchmarks
{
    private Subtitle _srtSubtitle = new();
    private Subtitle _assaSubtitle = new();
    private readonly SubRip _subRip = new();
    private readonly AdvancedSubStationAlpha _assa = new();

    [Params(1000, 10000)]
    public int Paragraphs { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var texts = new[]
        {
            "It was the best of times, it was the worst of times.",
            "<i>Are you coming with us?</i>",
            "- No." + Environment.NewLine + "- Then stay here and wait.",
            "1999 was a very long time ago.",
            "Hello.",
        };

        _srtSubtitle = new Subtitle();
        _assaSubtitle = new Subtitle();
        var startMs = 1000.0;
        for (var i = 0; i < Paragraphs; i++)
        {
            var p = new Paragraph(texts[i % texts.Length], startMs, startMs + 2000);
            _srtSubtitle.Paragraphs.Add(p);

            var assaParagraph = new Paragraph(texts[i % texts.Length], startMs, startMs + 2000)
            {
                Extra = "S" + (i % 100),
            };
            _assaSubtitle.Paragraphs.Add(assaParagraph);
            startMs += 2100;
        }

        // A header in the shape anime/karaoke files have: many styles.
        var header = new StringBuilder();
        header.AppendLine("[Script Info]");
        header.AppendLine("Title: Bench");
        header.AppendLine("ScriptType: v4.00+");
        header.AppendLine();
        header.AppendLine("[V4+ Styles]");
        header.AppendLine(SsaStyle.DefaultAssStyleFormat);
        header.AppendLine("Style: Default,Arial,20,&H00FFFFFF,&H000000FF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,2,2,10,10,10,1");
        for (var i = 0; i < 100; i++)
        {
            header.AppendLine($"Style: S{i},Arial,20,&H00FFFFFF,&H000000FF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,2,2,10,10,10,1");
        }

        header.AppendLine();
        header.AppendLine("[Events]");
        header.AppendLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");
        _assaSubtitle.Header = header.ToString();
    }

    [Benchmark]
    public string SubRipToText() => _subRip.ToText(_srtSubtitle, "title");

    [Benchmark]
    public string AssaToText() => _assa.ToText(_assaSubtitle, "title");
}
