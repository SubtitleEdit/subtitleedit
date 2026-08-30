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

    [Fact]
    public void Report_RewritesOneLineWithCountAndPercent()
    {
        var original = Console.Out;
        var buffer = new StringWriter();
        try
        {
            Console.SetOut(buffer);
            ProgressLine.Report("OCR", 12, 345);
        }
        finally
        {
            Console.SetOut(original);
        }

        Assert.Equal("\r  OCR 12/345 (3%)...", buffer.ToString());
    }

    [Fact]
    public void Report_RenderedTextNeverShrinks()
    {
        // The \r rewrite only works because the line grows: a shorter line would leave
        // characters of the previous one behind.
        var original = Console.Out;
        var lengths = new List<int>();
        try
        {
            for (var done = 0; done <= 250; done++)
            {
                var buffer = new StringWriter();
                Console.SetOut(buffer);
                ProgressLine.Report("OCR", done, 250);
                lengths.Add(buffer.ToString().Length);
            }
        }
        finally
        {
            Console.SetOut(original);
        }

        for (var i = 1; i < lengths.Count; i++)
        {
            Assert.True(lengths[i] >= lengths[i - 1], $"line shrank at {i}: {lengths[i - 1]} -> {lengths[i]}");
        }
    }
}
