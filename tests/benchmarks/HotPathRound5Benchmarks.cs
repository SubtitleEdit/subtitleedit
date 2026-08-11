using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using BenchmarkDotNet.Attributes;
using Nikse.SubtitleEdit.Controls.SyntaxTextEditorControl;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace Nikse.SubtitleEdit.Benchmarks;

/// <summary>
/// Shared test data for round 5: the words of a handful of ordinary subtitle lines, which is
/// what the per-word spell-check helpers actually see. Almost none of them are names, so the
/// "no match" path is the one that matters.
/// </summary>
internal static class WordSamples
{
    public const string SampleText =
        "We should probably head back before it gets dark." + "\r\n" +
        "I told you the bridge was out.";

    public static readonly string[] Words =
    {
        "We", "should", "probably", "head", "back", "before", "it", "gets", "dark",
        "I", "told", "you", "the", "bridge", "was", "out",
        "Nothing", "here", "matters", "much", "anymore",
        "Let's", "go", "home", "and", "forget", "this", "ever", "happened",
    };

    /// <summary>The repo's Dictionaries folder (names.xml + en_names.xml), found from the test binary.</summary>
    public static string DictionaryFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "Dictionaries");
            if (File.Exists(Path.Combine(candidate, "names.xml")))
            {
                return candidate + Path.DirectorySeparatorChar;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repo Dictionaries folder");
    }
}

/// <summary>
/// <see cref="NameList.IsInNamesMultiWordList"/> runs once per word of every line during spell
/// check (from SpellChecker.IsWordCorrect -> IsName -> HasNameExtended), and up to four times
/// per word from the OCR fix engine.
/// </summary>
[MemoryDiagnoser]
public class NameListMultiWordBenchmarks
{
    private NameList _nameList = null!;
    private SeNamesList _seNamesList = null!;

    [GlobalSetup]
    public void Setup()
    {
        var folder = WordSamples.DictionaryFolder();
        _nameList = new NameList(folder, "en_US", false, string.Empty);
        _seNamesList = new SeNamesList();
        _seNamesList.Load(folder, "en_US");
    }

    [Benchmark]
    public int LibSeIsInNamesMultiWordList()
    {
        var hits = 0;
        foreach (var word in WordSamples.Words)
        {
            if (_nameList.IsInNamesMultiWordList(WordSamples.SampleText, word))
            {
                hits++;
            }
        }

        return hits;
    }

    [Benchmark]
    public int UiIsInNamesMultiWordList()
    {
        var hits = 0;
        foreach (var word in WordSamples.Words)
        {
            if (_seNamesList.IsInNamesMultiWordList(WordSamples.SampleText, word))
            {
                hits++;
            }
        }

        return hits;
    }
}

/// <summary>
/// <see cref="SpellCheckWordLists.HasNameExtended"/> is the whole per-word name check: the
/// uppercase/apostrophe sets, the multi-word list and the dash/period part scan.
/// </summary>
[MemoryDiagnoser]
public class SpellCheckWordListsBenchmarks
{
    private sealed class AlwaysWrongSpell : IDoSpell
    {
        public bool DoSpell(string word) => false;
    }

    private SpellCheckWordLists _wordLists = null!;

    [GlobalSetup]
    public void Setup()
    {
        var folder = WordSamples.DictionaryFolder();
        SpellCheckConfig.DictionariesFolder = () => folder;
        _wordLists = new SpellCheckWordLists("en_US", new AlwaysWrongSpell());
    }

    [Benchmark]
    public int HasNameExtended()
    {
        var hits = 0;
        foreach (var word in WordSamples.Words)
        {
            if (_wordLists.HasNameExtended(word, WordSamples.SampleText))
            {
                hits++;
            }
        }

        return hits;
    }
}

/// <summary>
/// <see cref="Utilities.NormalizeUserDictionaryWord"/> runs once per word during spell check
/// (SpellChecker.IsWordCorrect -> HasUserWord) and from the OCR fix engine.
/// </summary>
[MemoryDiagnoser]
public class NormalizeUserDictionaryWordBenchmarks
{
    [Benchmark]
    public int Normalize()
    {
        var total = 0;
        foreach (var word in WordSamples.Words)
        {
            total += Utilities.NormalizeUserDictionaryWord(word).Length;
        }

        return total;
    }
}

/// <summary>
/// RTF import (drag/drop of an .rtf transcript, paste from a word processor) walks every RTF
/// control word through the destination list.
/// </summary>
[MemoryDiagnoser]
public class RichTextToPlainTextBenchmarks
{
    private string _rtf = null!;

    [GlobalSetup]
    public void Setup()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat{\fonttbl{\f0\fnil\fcharset0 Calibri;}}");
        sb.Append(@"{\*\generator Riched20 10.0.19041}\viewkind4\uc1 ");
        for (var i = 0; i < 400; i++)
        {
            sb.Append(@"\pard\sa200\sl276\slmult1\f0\fs22\lang9 ");
            sb.Append("We should probably head back before it gets dark.");
            sb.Append(@"\par ");
        }

        sb.Append('}');
        _rtf = sb.ToString();
    }

    [Benchmark]
    public int ConvertToText() => RichTextToPlainText.ConvertToText(_rtf).Length;
}

/// <summary>
/// One render pass of the source-view line number gutter, the way the editor repaints it while
/// scrolling or typing: only the visible lines are drawn, but every one of them is drawn on
/// every frame.
/// </summary>
[MemoryDiagnoser]
public class LineNumberGutterRenderBenchmarks
{
    private const double LineHeightPx = 16;

    private static bool _avaloniaInitialized;
    private LineNumberGutter _gutter = null!;
    private double _verticalOffset;

    /// <summary>Lines visible in the source view at once - that is how many numbers are drawn per frame.</summary>
    [Params(40)]
    public int VisibleLines { get; set; }

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

        var height = VisibleLines * LineHeightPx;
        _gutter = new LineNumberGutter
        {
            LineCount = 5000,
            LineHeight = LineHeightPx,
            CurrentLine = 12,
            FontSize = 13,
            Foreground = Brushes.Gray,
            Background = Brushes.Black,
        };

        _gutter.Measure(new Size(80, height));
        _gutter.Arrange(new Rect(0, 0, 50, height));
        if (_gutter.Bounds.Width <= 0 || _gutter.Bounds.Height <= 0)
        {
            throw new InvalidOperationException("gutter was not arranged");
        }

        _verticalOffset = 0;
    }

    /// <summary>Scrolling: the visible line numbers change every frame.</summary>
    [Benchmark]
    public void ScrollFrame()
    {
        _verticalOffset += 3;
        if (_verticalOffset > 4000)
        {
            _verticalOffset = 0;
        }

        _gutter.VerticalOffset = _verticalOffset;
        RenderFrame();
    }

    /// <summary>Typing/caret moves: the same line numbers are re-drawn every frame.</summary>
    [Benchmark]
    public void StaticFrame()
    {
        _gutter.VerticalOffset = 320;
        RenderFrame();
    }

    private void RenderFrame()
    {
        var drawingGroup = new DrawingGroup();
        using var context = drawingGroup.Open();
        _gutter.Render(context);
    }
}
