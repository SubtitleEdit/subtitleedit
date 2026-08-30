using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// The shared one-line progress used by the OCR loops and auto-translate (issue #14267).
/// </summary>
public class ProgressLineTest
{
    [Theory]
    [InlineData(0, 100, 0)]
    [InlineData(1, 100, 1)]
    [InlineData(50, 100, 50)]
    [InlineData(100, 100, 100)]
    [InlineData(1, 3, 33)]
    [InlineData(2, 3, 66)]
    [InlineData(3, 3, 100)]
    public void Percent_ReportsWholePercent(int done, int total, int expected)
    {
        Assert.Equal(expected, ProgressLine.Percent(done, total));
    }

    [Fact]
    public void Percent_FloorsSoOnlyTheLastItemShows100()
    {
        // 999/1000 must not round up to a misleading "100%".
        Assert.Equal(99, ProgressLine.Percent(999, 1000));
        Assert.Equal(100, ProgressLine.Percent(1000, 1000));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(5, 0)]
    [InlineData(5, -1)]
    public void Percent_NothingToDo_IsZeroNotDivideByZero(int done, int total)
    {
        Assert.Equal(0, ProgressLine.Percent(done, total));
    }

    [Fact]
    public void Percent_OutOfRangeCountsAreClamped()
    {
        Assert.Equal(100, ProgressLine.Percent(11, 10));
        Assert.Equal(0, ProgressLine.Percent(-1, 10));
    }

    [Fact]
    public void Percent_LargeCountsDoNotOverflow()
    {
        // done * 100 overflows int at ~21.5M; the multiply is done in long.
        Assert.Equal(50, ProgressLine.Percent(50_000_000, 100_000_000));
    }

    /// <summary>
    /// Captures what ProgressLine writes for one run, in terminal or redirected mode.
    /// Console.SetOut alone cannot drive that choice: the test runner has already redirected
    /// the process's stdout, so Console.IsOutputRedirected is true here no matter what.
    /// </summary>
    private static string Capture(bool interactive, Action<Action<int, int>> run)
    {
        var original = Console.Out;
        var buffer = new StringWriter();
        try
        {
            Console.SetOut(buffer);
            ProgressLine.InteractiveOverride = interactive;
            run((done, total) => ProgressLine.Report("OCR", done, total));
        }
        finally
        {
            ProgressLine.InteractiveOverride = null;
            Console.SetOut(original);
        }

        return buffer.ToString();
    }

    [Fact]
    public void Report_Terminal_RewritesOneLineWithCountAndPercent()
    {
        var output = Capture(interactive: true, report => report(12, 345));

        Assert.Equal("\r  OCR 12/345 (3%)...", output);
    }

    [Fact]
    public void Report_Terminal_RewritesOnEveryItem()
    {
        var output = Capture(interactive: true, report =>
        {
            for (var done = 0; done <= 200; done++)
            {
                report(done, 200);
            }
        });

        // One \r-prefixed rewrite per call, no newlines to scroll the terminal.
        Assert.Equal(201, output.Split('\r').Length - 1);
        Assert.DoesNotContain('\n', output);
    }

    [Fact]
    public void Report_Terminal_RenderedTextNeverShrinks()
    {
        // The \r rewrite only works because the line grows: a shorter line would leave
        // characters of the previous one behind.
        var previous = 0;
        for (var done = 0; done <= 250; done++)
        {
            var length = Capture(interactive: true, report => report(done, 250)).Length;
            Assert.True(length >= previous, $"line shrank at {done}: {previous} -> {length}");
            previous = length;
        }
    }

    [Fact]
    public void Report_Redirected_WritesWholeLinesAtEveryTenPercent()
    {
        var output = Capture(interactive: false, report =>
        {
            for (var done = 0; done <= 5000; done++)
            {
                report(done, 5000);
            }
        });

        // A pipe or log file gets ten milestones, not 5000 fragments of one endless line.
        var lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(10, lines.Length);
        Assert.Equal("  OCR 500/5000 (10%)...", lines[0]);
        Assert.Equal("  OCR 5000/5000 (100%)...", lines[^1]);
        Assert.DoesNotContain('\r', output);
    }

    [Fact]
    public void Report_Redirected_ShortRunStillReportsEveryItemAndEndsAt100()
    {
        var output = Capture(interactive: false, report =>
        {
            for (var done = 0; done <= 3; done++)
            {
                report(done, 3);
            }
        });

        var lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(
            ["  OCR 1/3 (33%)...", "  OCR 2/3 (66%)...", "  OCR 3/3 (100%)..."],
            lines);
    }

    [Fact]
    public void Finish_OnlyEndsTheLineOnATerminal()
    {
        // Redirected output is already newline-terminated; a second newline would leave a
        // blank line in the log.
        Assert.Equal(Environment.NewLine, Capture(interactive: true, _ => ProgressLine.Finish()));
        Assert.Equal(string.Empty, Capture(interactive: false, _ => ProgressLine.Finish()));
    }
}
