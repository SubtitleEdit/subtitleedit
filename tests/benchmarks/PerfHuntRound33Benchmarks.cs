using System.Globalization;
using System.Text;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Benchmarks;

// Round 33. Every benchmark calls the real code, so the class is run once against the commit
// before the change and once after; ControlAutoBreak is the drift control.
[MemoryDiagnoser]
public class Round33Benchmarks
{
    private Subtitle _subtitle = null!;
    private Subtitle _diarized = null!;
    private FixCasing _fixCasing = null!;
    private string _folder = null!;
    private string _unknownFileName = null!;
    private NikseBitmap2 _ocrImage = null!;
    private List<SubtitleLineViewModel> _rows = null!;
    private DialogSplitMerge _dialogSplitMerge = null!;

    [GlobalSetup]
    public void Setup()
    {
        // The Danish UI culture - the culture-aware string calls take the slow path there.
        var culture = CultureInfo.GetCultureInfo("da-DK");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.CurrentCulture = culture;

        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        _subtitle = BenchmarkSubtitles.Build(1500);
        _fixCasing = new FixCasing("en") { FixNormal = true };
        _fixCasing.Fix(new Subtitle(_subtitle)); // the name index is built on first use

        // A diarized transcript: 400 speaker turns of three lines.
        _diarized = new Subtitle();
        for (var i = 0; i < 1200; i++)
        {
            var text = $"(speaker {i / 3 % 2}) " + BenchmarkSubtitles.Sentences[i % BenchmarkSubtitles.Sentences.Length].Replace("\r\n", " ");
            _diarized.Paragraphs.Add(new Paragraph(text, i * 2000, i * 2000 + 1800));
        }

        _folder = Path.Combine(Path.GetTempPath(), "se-round33-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);

        // A plain text file that no format matches (so the open falls through every format).
        _unknownFileName = Path.Combine(_folder, "notes.txt");
        var sb = new StringBuilder();
        for (var i = 0; i < 4000; i++)
        {
            sb.AppendLine("Meeting notes item " + i + ": " + BenchmarkSubtitles.Sentences[i % BenchmarkSubtitles.Sentences.Length].Replace("\r\n", " "));
        }

        File.WriteAllText(_unknownFileName, sb.ToString(), new UTF8Encoding(false));
        var format = new Subtitle().LoadSubtitle(_unknownFileName, out _, null);
        if (format != null)
        {
            throw new InvalidOperationException("Unknown file matched " + format.Name);
        }

        _ocrImage = RenderSubtitleImage();
        var letters = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(new NikseBitmap2(_ocrImage), 10, false, true, 20, true);
        if (letters.Count < 40)
        {
            throw new InvalidOperationException("OCR split found " + letters.Count + " parts");
        }

        _rows = new List<SubtitleLineViewModel>();
        string[] texts = { "{\\an8}Øjeblik, hvor er du på vej hen?", "Det ved jeg ikke, måske til København.", "- Hej.\r\n- Godt at se dig igen.", "{\\an1}Hvad så?" };
        for (var i = 0; i < 3000; i++)
        {
            _rows.Add(new SubtitleLineViewModel { Text = texts[i % texts.Length], MarginV = i % 2 == 0 ? "20" : string.Empty });
        }

        _dialogSplitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace, TwoLetterLanguageCode = "en" };
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch
        {
            // ignore
        }
    }

    private static NikseBitmap2 RenderSubtitleImage()
    {
        // A Blu-ray sized two-line subtitle: white text with a black outline on transparent.
        using var bitmap = new SKBitmap(new SKImageInfo(1500, 150, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);
        using var font = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 46);
        using var outline = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 4, IsAntialias = false };
        using var fill = new SKPaint { Color = SKColors.White, IsAntialias = false };
        foreach (var (line, y) in new[] { ("You told me you would be home by nine tonight,", 60f), ("but the train was late and nobody called me back.", 125f) })
        {
            canvas.DrawText(line, 20, y, SKTextAlign.Left, font, outline);
            canvas.DrawText(line, 20, y, SKTextAlign.Left, font, fill);
        }

        return new NikseBitmap2(bitmap);
    }

    [Benchmark]
    public int FixCasingFix()
    {
        var subtitle = new Subtitle(_subtitle);
        _fixCasing.Fix(subtitle);
        return subtitle.Paragraphs.Count;
    }

    [Benchmark]
    public int SpeechToTextPostProcessDiarized()
    {
        var processor = new SpeechToTextPostProcessor("en");
        var result = processor.Fix(SpeechToTextPostProcessor.Engine.Whisper, new Subtitle(_diarized), true, false, false, true, false, false, false, default);
        return result.Paragraphs.Count;
    }

    [Benchmark]
    public int OcrSplitLetters()
    {
        return NikseBitmapImageSplitter2.SplitBitmapToLettersNew(new NikseBitmap2(_ocrImage), 10, false, true, 20, true).Count;
    }

    [Benchmark]
    public int TeletextAlignment3000Rows()
    {
        var total = 0;
        foreach (var row in _rows)
        {
            total += (int)row.TeletextTextAlignment + row.TeletextDisplay.Length;
        }

        return total;
    }

    [Benchmark]
    public bool LoadUnknownTextFile()
    {
        return new Subtitle().LoadSubtitle(_unknownFileName, out _, null) == null;
    }

    [Benchmark]
    public int DialogFixDashesAndSpaces()
    {
        var total = 0;
        foreach (var p in _subtitle.Paragraphs)
        {
            total += _dialogSplitMerge.FixDashesAndSpaces(p.Text).Length;
            total += _dialogSplitMerge.AddSpaces(p.Text).Length;
        }

        return total;
    }

    [Benchmark]
    public int ControlAutoBreak()
    {
        var total = 0;
        for (var i = 0; i < 2000; i++)
        {
            total += Utilities.AutoBreakLine(BenchmarkSubtitles.Sentences[i % BenchmarkSubtitles.Sentences.Length]).Length;
        }

        return total;
    }
}
