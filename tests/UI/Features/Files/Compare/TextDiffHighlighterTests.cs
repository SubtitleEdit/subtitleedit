using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Files.Compare;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UITests.Features.Files.Compare;

public class TextDiffHighlighterTests
{
    // "He goes home" and "He went home". The verb differs only in its last two letters, and
    // Persian writes it with a zero width non-joiner inside the word, so a character level
    // diff cuts the word in two - which breaks the cursive joining when each half is shaped
    // as its own run (#13435).
    private const string PersianGoesHome = "او به خانه می‌رود";
    private const string PersianWentHome = "او به خانه می‌رفت";
    private const string PersianCommonPart = "او به خانه ";
    private const string PersianGoesVerb = "می‌رود";
    private const string PersianWentVerb = "می‌رفت";

    private static string JoinRuns(InlineCollection? inlines)
        => string.Concat(inlines!.Cast<Run>().Select(r => r.Text));

    private static string[] RunTexts(InlineCollection? inlines)
        => inlines!.Cast<Run>().Select(r => r.Text!).ToArray();

    [Fact]
    public void CompareReplacement_CrLfBeforeVsLfAfter_NormalizesInsteadOfDiffingTheCr()
    {
        // Windows subtitle text carries \r\n while a regex replacement comes back \n-normalized
        // (RegexUtils.ReplaceNewLineSafe). Diffing the raw strings isolated the \r in its own
        // one-character run, and a \r/\n pair split across two runs renders as two line breaks -
        // a phantom empty line (plus a red mark for the invisible \r) in the Multiple Replace
        // preview's Before column (#12622).
        var (before, after) = TextDiffHighlighter.CompareReplacement(
            "Just don't move out\r\nof my school district, okay?",
            "Just don't move out\nof my school district, ok?");

        var beforeText = JoinRuns(before.Inlines);
        var afterText = JoinRuns(after.Inlines);

        Assert.Equal("Just don't move out\nof my school district, okay?", beforeText);
        Assert.Equal("Just don't move out\nof my school district, ok?", afterText);
        Assert.All(before.Inlines!.Cast<Run>(), r => Assert.DoesNotContain('\r', r.Text!));
    }

    [Fact]
    public void Compare_CrLfVsLf_TreatedAsIdenticalText()
    {
        // The compare view gets one text per file: a CRLF file against an LF file must not paint
        // every line break as a difference (same normalization as CompareReplacement, #12622).
        var (left, right) = TextDiffHighlighter.Compare("First line\r\nSecond line", "First line\nSecond line");

        var leftRun = Assert.IsType<Run>(Assert.Single(left.Inlines!));
        var rightRun = Assert.IsType<Run>(Assert.Single(right.Inlines!));

        // The identical-texts path adds a single uncolored run per side.
        Assert.Equal("First line\nSecond line", leftRun.Text);
        Assert.Equal("First line\nSecond line", rightRun.Text);
        Assert.Null(leftRun.Background);
        Assert.Null(rightRun.Background);
    }

    [Fact]
    public void FindCommonParts_WhenContentIsReordered_MiddleCommon2IsSortedByText2Positions()
    {
        // Regression test for: <i>[KITT] text</i> → [KITT] <i>text</i>
        // "<i>" (pos 0 in text1, pos 7 in text2) and "[KITT] " (pos 3 in text1,
        // pos 0 in text2) are the two common middle substrings. Before the fix,
        // middleCommon2 was sorted by text1 positions → [(7,3),(0,7)], causing
        // BuildDiffRuns to walk backwards and emit duplicate text.
        var method = typeof(TextDiffHighlighter).GetMethod(
            "FindCommonParts", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var result = method!.Invoke(null, [
            "<i>[KITT] Why is that, Michael?</i>",
            "[KITT] <i>Why is that, Michael?</i>"
        ])!;

        var middleCommon2 = (List<(int start, int length)>)result.GetType()
            .GetField("Item4")!.GetValue(result)!;

        Assert.True(middleCommon2.Count >= 2,
            "Expected at least 2 common middle substrings for this input");

        for (var i = 1; i < middleCommon2.Count; i++)
        {
            Assert.True(middleCommon2[i - 1].start <= middleCommon2[i].start,
                $"middleCommon2[{i - 1}].start ({middleCommon2[i - 1].start}) " +
                $"must be <= middleCommon2[{i}].start ({middleCommon2[i].start})");
        }
    }

    [Fact]
    public void Compare_RightToLeftText_BothSidesFlowRightToLeft()
    {
        // A left to right text block lays its runs out left to right, so the chunks of a Persian
        // line come out in reverse reading order - the line reads backwards (#13435).
        var (left, right) = TextDiffHighlighter.Compare(PersianGoesHome, PersianWentHome);

        Assert.Equal(FlowDirection.RightToLeft, left.FlowDirection);
        Assert.Equal(FlowDirection.RightToLeft, right.FlowDirection);
    }

    [Fact]
    public void Compare_LeftToRightText_BothSidesFlowLeftToRight()
    {
        var (left, right) = TextDiffHighlighter.Compare("Hello there", "Hello world");

        Assert.Equal(FlowDirection.LeftToRight, left.FlowDirection);
        Assert.Equal(FlowDirection.LeftToRight, right.FlowDirection);
    }

    [Fact]
    public void Compare_RightToLeftAgainstLeftToRight_EachSideFollowsItsOwnContent()
    {
        // An untranslated original next to a Persian translation: the two columns do not have
        // to agree on a direction.
        var (left, right) = TextDiffHighlighter.Compare("He goes home", PersianGoesHome);

        Assert.Equal(FlowDirection.LeftToRight, left.FlowDirection);
        Assert.Equal(FlowDirection.RightToLeft, right.FlowDirection);
    }

    [Fact]
    public void CompareReplacement_RightToLeftText_BothSidesFlowRightToLeft()
    {
        var (before, after) = TextDiffHighlighter.CompareReplacement(PersianGoesHome, PersianWentHome);

        Assert.Equal(FlowDirection.RightToLeft, before.FlowDirection);
        Assert.Equal(FlowDirection.RightToLeft, after.FlowDirection);
    }

    [Fact]
    public void Compare_RightToLeftText_DifferenceCoversTheWholeWord()
    {
        // The verbs share everything up to and including the "ر", so a character level diff
        // splits the word: Avalonia shapes each run on its own and the halves fall back to
        // isolated letter forms. The difference has to grow out to the word boundaries.
        var (left, right) = TextDiffHighlighter.Compare(PersianGoesHome, PersianWentHome);

        Assert.Equal(new[] { PersianCommonPart, PersianGoesVerb }, RunTexts(left.Inlines));
        Assert.Equal(new[] { PersianCommonPart, PersianWentVerb }, RunTexts(right.Inlines));
    }

    [Fact]
    public void Compare_RightToLeftText_NoRunEndsInsideAWord()
    {
        var (left, right) = TextDiffHighlighter.Compare(
            "سلام دنیا، حال شما چطور است؟",
            "سلام دنیای زیبا، حال شما چطور بود؟");

        AssertNoRunSplitsAWord(left.Inlines);
        AssertNoRunSplitsAWord(right.Inlines);
    }

    [Fact]
    public void Compare_RightToLeftText_RunsStillCoverTheWholeText()
    {
        var (left, right) = TextDiffHighlighter.Compare(
            "سلام دنیا، حال شما چطور است؟",
            "سلام دنیای زیبا، حال شما چطور بود؟");

        Assert.Equal("سلام دنیا، حال شما چطور است؟", JoinRuns(left.Inlines));
        Assert.Equal("سلام دنیای زیبا، حال شما چطور بود؟", JoinRuns(right.Inlines));
    }

    [Fact]
    public void Compare_LeftToRightText_KeepsCharacterLevelGranularity()
    {
        // Word snapping is only for cursive right to left scripts; Latin text keeps the finer
        // "colo[u]r" style highlighting it has always had.
        var (left, _) = TextDiffHighlighter.Compare("colour", "color");

        Assert.Equal(new[] { "colo", "u", "r" }, RunTexts(left.Inlines));
    }

    [Fact]
    public void Compare_UnchangedRuns_DoNotOverrideTheInheritedForeground()
    {
        // Assigning Foreground = null is not the same as leaving it alone: a local null value
        // overrides the foreground inherited from the theme, and Avalonia draws a run with a
        // null brush as nothing at all. That made every unchanged stretch of text invisible in
        // the diff previews (Fix common errors, Compare, Multiple replace) in 5.2.0-beta10 (#13501).
        var (left, right) = TextDiffHighlighter.Compare("Goodbye cruel world", "Goodbye kind world");
        var (before, after) = TextDiffHighlighter.CompareReplacement("Goodbye cruel world", "Goodbye kind world");

        foreach (var inlines in new[] { left.Inlines, right.Inlines, before.Inlines, after.Inlines })
        {
            var runs = inlines!.Cast<Run>().ToArray();
            Assert.True(runs.Length > 1); // the diff must produce both changed and unchanged runs

            foreach (var run in runs)
            {
                if (run.Foreground == null)
                {
                    Assert.False(run.IsSet(TextElement.ForegroundProperty),
                        $"Unchanged run '{run.Text}' has a local null Foreground, which renders it invisible");
                }
            }

            Assert.Contains(runs, r => !r.IsSet(TextElement.ForegroundProperty));
            Assert.Contains(runs, r => r.Foreground != null);
        }
    }

    private static void AssertNoRunSplitsAWord(InlineCollection? inlines)
    {
        var isWordChar = typeof(TextDiffHighlighter).GetMethod(
            "IsWordChar", BindingFlags.NonPublic | BindingFlags.Static)!;

        var runs = inlines!.Cast<Run>().Select(r => r.Text!).ToArray();
        for (var i = 1; i < runs.Length; i++)
        {
            var lastOfPrevious = runs[i - 1][^1];
            var firstOfCurrent = runs[i][0];
            var splitsWord = (bool)isWordChar.Invoke(null, [lastOfPrevious])!
                             && (bool)isWordChar.Invoke(null, [firstOfCurrent])!;

            Assert.False(splitsWord,
                $"Run boundary {i} splits a word: '{runs[i - 1]}' | '{runs[i]}'");
        }
    }
}
