using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Headless;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Video.VideoOcr;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.NetflixQualityCheck;
using Nikse.SubtitleEdit.UiLogic.Ocr.AppleVision;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Benchmarks;

// Round 32. Every benchmark calls the real code, so the classes are run once against the commit
// before the change and once after; ControlAutoBreak is the drift control.

/// <summary>Shot changes, video OCR, find/replace, language detect, spell check.</summary>
[MemoryDiagnoser]
public class Round32Benchmarks
{
    private Subtitle _subtitle = null!;
    private string _folder = null!;
    private string _videoFileName = null!;
    private string _frameFileName = null!;
    private string _maskedFileName = null!;
    private List<string> _groupFrames = null!;
    private SKBitmap _decodedFrame = null!;
    private List<AppleVisionObservation> _observations = null!;
    private List<double> _shotChanges = null!;
    private List<string> _lines = null!;
    private FindService _findService = null!;
    private SpellChecker _spellChecker = null!;
    private string[] _words = null!;

    [Params(3000)]
    public int Lines { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        _subtitle = BenchmarkSubtitles.Build(Lines);

        _folder = Path.Combine(Path.GetTempPath(), "se-round32-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);

        // A fake "video" (the shot-change file is keyed on a content hash) with a shot change
        // roughly every 2.3 seconds over the whole subtitle.
        _videoFileName = Path.Combine(_folder, "video.mp4");
        var bytes = new byte[256 * 1024];
        new Random(32).NextBytes(bytes);
        File.WriteAllBytes(_videoFileName, bytes);
        _shotChanges = new List<double>();
        var end = _subtitle.Paragraphs[^1].EndTime.TotalSeconds;
        for (var s = 0.7; s < end; s += 2.3)
        {
            _shotChanges.Add(Math.Round(s, 3));
        }

        ShotChangeHelper.SaveShotChanges(_videoFileName, _folder, _shotChanges);
        NetflixCheckShotChange.ShotChangeDirectory = _folder;

        // 1080p frames: a bright sky over the top third (a bright background is the expensive
        // case for the mask dilation), a dark scene below, and white subtitle text.
        _frameFileName = Path.Combine(_folder, "frame.jpg");
        _maskedFileName = Path.Combine(_folder, "masked.jpg");
        WriteFrame(_frameFileName, "Subtitle text for the masked copy benchmark");
        _groupFrames = new List<string>();
        for (var i = 0; i < 40; i++)
        {
            var fileName = Path.Combine(_folder, $"group{i:000}.jpg");
            WriteFrame(fileName, "Subtitle number " + (i / 8));
            _groupFrames.Add(fileName);
        }

        // As the Apple Vision path has it: the decoded frame and a few text boxes - the two
        // subtitle lines and some darker scene text (normalized, bottom-left origin).
        _decodedFrame = SKBitmap.Decode(_frameFileName);
        _observations = new List<AppleVisionObservation>
        {
            new("Subtitle text", 0.15, 0.85, 0.16, 0.10),
            new("second line", 0.21, 0.79, 0.08, 0.03),
            new("SHIRT PRINT", 0.40, 0.55, 0.55, 0.50),
        };

        _lines = _subtitle.Paragraphs.Select(p => p.Text).ToList();
        _findService = new FindService();

        _spellChecker = new SpellChecker();
        _spellChecker.Initialize(Path.Combine(BenchmarkSubtitles.FindDataDirectory(), "Dictionaries", "en_US.dic"), "en");
        _words = _subtitle.Paragraphs
            .SelectMany(p => HtmlUtil.RemoveHtmlTags(p.Text, true).Split(new[] { ' ', '\r', '\n', ',', '.', '?', '!', '"', '-' }, StringSplitOptions.RemoveEmptyEntries))
            .ToArray();
    }

    private static void WriteFrame(string fileName, string text)
    {
        using var bitmap = new SKBitmap(1920, 1080);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(new SKColor(30, 30, 40));
        using var sky = new SKPaint { Color = new SKColor(220, 225, 235) };
        canvas.DrawRect(0, 0, 1920, 360, sky);
        using var font = new SKFont(SKTypeface.Default, 64);
        using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        canvas.DrawText(text, 300, 950, font, paint);
        canvas.DrawText("and a second line of white glyphs", 420, 1030, font, paint);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 92);
        File.WriteAllBytes(fileName, data.ToArray());
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _decodedFrame?.Dispose();
        try
        {
            Directory.Delete(_folder, true);
        }
        catch
        {
            // temp folder
        }
    }

    [Benchmark]
    public int NetflixShotChangeCheck()
    {
        // Set here, not in Setup: touching Se.Settings later in Setup resets the static folder.
        NetflixCheckShotChange.ShotChangeDirectory = _folder;
        var controller = new NetflixQualityController { VideoFileName = _videoFileName, FrameRate = 25 };
        new NetflixCheckShotChange("Shot changes").Check(_subtitle, controller);
        return controller.Records.Count;
    }

    [Benchmark]
    public bool VideoOcrWriteMaskedCopy()
    {
        return VideoOcrFrameGrouper.WriteMaskedCopy(_frameFileName, _maskedFileName, 180);
    }

    [Benchmark]
    public int VideoOcrGroupFrames()
    {
        return VideoOcrFrameGrouper.Group(_groupFrames, 180, 90, null, CancellationToken.None).Count;
    }

    [Benchmark]
    public int VideoOcrFilterObservations()
    {
        return VideoOcrObservationFilter.FilterByBrightness(_observations, _decodedFrame, 180).Count;
    }

    [Benchmark]
    public double ExtendToShotChangeAllLines()
    {
        var total = 0.0;
        var paragraphs = _subtitle.Paragraphs;
        for (var i = 0; i < paragraphs.Count; i++)
        {
            var p = paragraphs[i];
            double? next = i + 1 < paragraphs.Count ? paragraphs[i + 1].StartTime.TotalMilliseconds : null;
            double? previous = i > 0 ? paragraphs[i - 1].EndTime.TotalMilliseconds : null;
            total += ShotChangesHelper.GetExtendedEndMs(_shotChanges, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds, next, 84, 0, 8000) ?? 0;
            total += ShotChangesHelper.GetExtendedStartMs(_shotChanges, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds, previous, 0, 0, 8000) ?? 0;
        }

        return total;
    }

    [Benchmark]
    public int FindReplaceAllWholeWord()
    {
        // Replacing with the same text keeps the lines unchanged across iterations.
        _findService.Initialize(_lines, 0, true, FindService.FindMode.CaseInsensitive);
        return _findService.ReplaceAll("times", "times") + _findService.ReplaceAll("Denmark", "Denmark");
    }

    [Benchmark]
    public string? LanguageAutoDetect()
    {
        return Nikse.SubtitleEdit.Core.Common.LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(_subtitle);
    }

    [Benchmark]
    public int SpellCheckWords()
    {
        // Twice per word, like the OCR fix engine's line check plus its word check - per
        // document, so the answer memo (when there is one) starts empty each time.
        DictionaryAnswers?.Clear(_spellChecker);
        var correct = 0;
        for (var pass = 0; pass < 2; pass++)
        {
            foreach (var word in _words)
            {
                if (_spellChecker.IsWordCorrect(word))
                {
                    correct++;
                }
            }
        }

        return correct;
    }

    private static readonly ClearableField? DictionaryAnswers = ClearableField.Find(typeof(SpellChecker), "_dictionaryAnswers");

    [Benchmark]
    public bool SpellCheckerInitialize()
    {
        // A new spell checker per file, like seconv's fix common errors and each spell check run.
        return new SpellChecker().Initialize(Path.Combine(BenchmarkSubtitles.FindDataDirectory(), "Dictionaries", "en_US.dic"), "en");
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

/// <summary>Binary containers: VobSub writer, XSub fallback scan.</summary>
[MemoryDiagnoser]
public class Round32BinaryBenchmarks
{
    private byte[] _avi = null!;
    private byte[] _subPictureUnit = null!;
    private string _folder = null!;

    [GlobalSetup]
    public void Setup()
    {
        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        _folder = Path.Combine(Path.GetTempPath(), "se-round32b-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);

        // A broken AVI: no RIFF structure, so the parser falls back to scanning every byte.
        _avi = new byte[64 * 1024 * 1024];
        new Random(32).NextBytes(_avi);
        var packet = Encoding.ASCII.GetBytes("[00:01:02.003-00:01:04.005]");
        for (var i = 1; i < 8; i++)
        {
            packet.CopyTo(_avi, i * 8 * 1024 * 1024);
        }

        _subPictureUnit = new byte[4000];
        new Random(7).NextBytes(_subPictureUnit);
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
            // temp folder
        }
    }

    [Benchmark]
    public long VobSubWriteUnits()
    {
        var fileName = Path.Combine(_folder, "out.sub");
        var palette = Enumerable.Range(0, 16).Select(i => new SKColor((byte)(i * 16), (byte)(i * 16), (byte)(i * 16))).ToList();
        using (var writer = new VobSubWriter(fileName, 720, 576, 32, DvdSubtitleLanguage.English, palette))
        {
            for (var i = 0; i < 3000; i++)
            {
                writer.WriteSubPictureUnit(new TimeCode(i * 3000), _subPictureUnit);
            }
        }

        return new FileInfo(fileName).Length;
    }

    [Benchmark]
    public int XSubFallbackScan()
    {
        using var ms = new MemoryStream(_avi);
        return XSubParser.ParseAvi(ms).Tracks.Count;
    }
}

/// <summary>Statistics / batch convert line width.</summary>
[MemoryDiagnoser]
public class Round32StatisticsBenchmarks
{
    private static bool _avaloniaInitialized;
    private Func<string, int> _getSingleLineWidth = null!;
    private string[] _lines = null!;

    [GlobalSetup]
    public void Setup()
    {
        if (!_avaloniaInitialized)
        {
            _avaloniaInitialized = true;
            try
            {
                AppBuilder.Configure<Application>()
                    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                    .UseSkia()
                    .WithInterFont()
                    .SetupWithoutStarting();
            }
            catch (InvalidOperationException)
            {
                // already set up in this process
            }
        }

        Configuration.DataDirectory = BenchmarkSubtitles.FindDataDirectory();
        var method = typeof(BatchStatics).GetMethod("GetSingleLineWidth", BindingFlags.NonPublic | BindingFlags.Static)!;
        _getSingleLineWidth = method.CreateDelegate<Func<string, int>>();
        _lines = BenchmarkSubtitles.Build(3000).Paragraphs.SelectMany(p => p.Text.SplitToLines()).ToArray();
    }

    [Benchmark]
    public int BatchStatisticsLineWidths()
    {
        var total = 0;
        foreach (var line in _lines)
        {
            total += _getSingleLineWidth(line);
        }

        return total;
    }
}

/// <summary>A private field that may not exist in the commit being measured.</summary>
internal sealed class ClearableField
{
    private readonly FieldInfo _field;

    private ClearableField(FieldInfo field)
    {
        _field = field;
    }

    public static ClearableField? Find(Type type, string name)
    {
        var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        return field == null ? null : new ClearableField(field);
    }

    public void Clear(object instance)
    {
        ((System.Collections.IDictionary)_field.GetValue(instance)!).Clear();
    }
}
