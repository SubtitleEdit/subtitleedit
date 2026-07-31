using Avalonia;
using Avalonia.Headless;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Edit.ModifySelection;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System.Reflection;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// The Statistics window measures every line of the subtitle three times over
/// (line lengths + two GetIndicesWithSingleLineWidth passes), and batch convert measures
/// every line of every file. Each call did a font-manager typeface lookup + SKFont alloc.
/// </summary>
[MemoryDiagnoser]
public class TextMeasurerBenchmarks
{
    private List<string> _lines = null!;

    [GlobalSetup]
    public void Setup()
    {
        var sentences = new[]
        {
            "It was the best of times, it was the worst of times.",
            "Are you coming with us?",
            "- No.\n- Then stay here and wait.",
            "I told you already, this is not going to work out the way you think it will.",
            "Hello.",
            "Somewhere in Denmark",
        };

        _lines = new List<string>(600);
        for (var i = 0; i < 600; i++)
        {
            _lines.Add(sentences[i % sentences.Length]);
        }
    }

    [Benchmark]
    public float MeasureAllLines()
    {
        var sum = 0f;
        foreach (var line in _lines)
        {
            sum += TextMeasurer.MeasureString(line, "Arial", 17f).Width;
        }

        return sum;
    }
}

/// <summary>
/// The binary-edit adjust dialogs run these over a full-frame bitmap on every debounced
/// slider tick, and batch convert runs them once per subtitle image. The per-pixel
/// brightness/contrast/gamma chain is a pure function of one input byte per channel.
/// </summary>
[MemoryDiagnoser]
public class SubtitleImageAdjusterBenchmarks
{
    private SKBitmap _bitmap = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Subtitle-like full-frame image: mostly transparent, an opaque "text band" of
        // antialiased blobs near the bottom (premultiplied, like the dialogs' input).
        var info = new SKImageInfo(1920, 1080, SKColorType.Bgra8888, SKAlphaType.Premul);
        _bitmap = new SKBitmap(info);
        using var canvas = new SKCanvas(_bitmap);
        canvas.Clear(SKColors.Transparent);

        using var fill = new SKPaint { Color = new SKColor(255, 255, 240), IsAntialias = true };
        using var shadow = new SKPaint { Color = new SKColor(0, 0, 0, 160), IsAntialias = true };
        for (var i = 0; i < 60; i++)
        {
            var x = 80 + i % 30 * 60;
            var y = 880 + i / 30 * 90;
            canvas.DrawRoundRect(x + 4, y + 4, 48, 64, 8, 8, shadow);
            canvas.DrawRoundRect(x, y, 48, 64, 8, 8, fill);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _bitmap.Dispose();
    }

    [Benchmark]
    public int AdjustBrightnessContrastGamma()
    {
        using var result = SubtitleImageAdjuster.AdjustBrightness(_bitmap, 20f, 15f, 1.2f);
        return result.Width;
    }

    [Benchmark]
    public int AdjustAlpha()
    {
        using var result = SubtitleImageAdjuster.AdjustAlpha(_bitmap, -40f, 32);
        return result.Width;
    }

    [Benchmark]
    public int Colorize()
    {
        using var result = SubtitleImageAdjuster.Colorize(_bitmap, 220, 180, 60);
        return result.Width;
    }
}

/// <summary>
/// During "Apply selected fixes", every libse fix rule calls AllowFix once per paragraph
/// (78 call sites across 42 rules), and each call scanned the whole Fixes list. This
/// reproduces one apply pass worth of probes: every paragraph x every action type.
/// </summary>
[MemoryDiagnoser]
public class FixCommonErrorsAllowFixBenchmarks
{
    private static readonly string[] Actions =
    {
        "Fix empty lines", "Fix overlapping display times", "Fix short display times",
        "Fix long display times", "Fix invalid italic tags", "Fix unneeded spaces",
    };

    private static bool _avaloniaInitialized;

    private FixCommonErrorsViewModel _viewModel = null!;
    private List<Paragraph> _paragraphs = null!;

    [GlobalSetup]
    public void Setup()
    {
        if (!_avaloniaInitialized)
        {
            _avaloniaInitialized = true;
            AppBuilder.Configure<Application>()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                .UseSkia()
                .WithInterFont()
                .SetupWithoutStarting();
        }

        _viewModel = new FixCommonErrorsViewModel(null!, null!, null!);

        _paragraphs = new List<Paragraph>(2000);
        for (var i = 0; i < 2000; i++)
        {
            var p = new Paragraph($"Line {i} text goes here.", i * 2000, i * 2000 + 1500) { Number = i + 1 };
            _paragraphs.Add(p);

            // ~1.5 fixes per paragraph across the action types, most of them ticked -
            // the shape of a real scan result.
            _viewModel.Fixes.Add(new FixDisplayItem(p, p.Number, Actions[i % Actions.Length], "before", "after", i % 10 != 0));
            if (i % 2 == 0)
            {
                _viewModel.Fixes.Add(new FixDisplayItem(p, p.Number, Actions[(i + 3) % Actions.Length], "before", "after", true));
            }
        }

        // Put the view model in apply mode, the way DoApplyFixes does before calling ApplyFixes.
        var previewModeField = typeof(FixCommonErrorsViewModel)
            .GetField("_previewMode", BindingFlags.NonPublic | BindingFlags.Instance)!;
        previewModeField.SetValue(_viewModel, false);
    }

    [Benchmark]
    public int AllowFixApplyPassProbes()
    {
        var allowed = 0;
        foreach (var p in _paragraphs)
        {
            foreach (var action in Actions)
            {
                if (_viewModel.AllowFix(p, action))
                {
                    allowed++;
                }
            }
        }

        return allowed;
    }
}

/// <summary>
/// The modify-selection window re-evaluates the selected rule against every line of the
/// subtitle on a 250 ms dirty timer while the user types. Regex rules went through the
/// static Regex cache per line, line-count rules allocated a string[] per line, and
/// style rules ran a LINQ Any with a closure per line.
/// </summary>
[MemoryDiagnoser]
public class ModifySelectionRuleBenchmarks
{
    private List<SubtitleLineViewModel> _lines = null!;
    private ModifySelectionRule _regexRule = null!;
    private ModifySelectionRule _lineCountRule = null!;
    private ModifySelectionRule _styleRule = null!;

    [GlobalSetup]
    public void Setup()
    {
        _lines = SubtitleFactory.Make(2000);
        var styles = new[] { "Default", "Sign", "Song", "Top", "Narrator" };
        for (var i = 0; i < _lines.Count; i++)
        {
            _lines[i].Style = styles[i % styles.Length];
        }

        _regexRule = new ModifySelectionRule
        {
            RuleType = RuleType.RegEx,
            Text = @"\b[A-Z][a-z]+\b",
            HasMatchCase = true,
            MatchCase = true,
        };

        _lineCountRule = new ModifySelectionRule
        {
            RuleType = RuleType.ExactlyTwoLines,
        };

        _styleRule = new ModifySelectionRule
        {
            RuleType = RuleType.Style,
            MultiSelectItems = styles.Select(s => new MultiSelectItem(s) { Apply = s is "Sign" or "Top" }).ToList(),
        };
    }

    [Benchmark]
    public int RegexRuleFullScan() => CountMatches(_regexRule);

    [Benchmark]
    public int LineCountRuleFullScan() => CountMatches(_lineCountRule);

    [Benchmark]
    public int StyleRuleFullScan() => CountMatches(_styleRule);

    private int CountMatches(ModifySelectionRule rule)
    {
        var count = 0;
        foreach (var line in _lines)
        {
            if (rule.IsMatch(line))
            {
                count++;
            }
        }

        return count;
    }
}
