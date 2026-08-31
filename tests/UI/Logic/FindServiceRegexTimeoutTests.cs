using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UITests.Logic;

// A user-entered pattern with catastrophic backtracking used to run on the UI thread with no
// timeout at all, so find/replace hung the program until it was killed. SE 4 gave the same
// patterns five seconds; the timeout now comes back as "no match" instead of an exception.
public class FindServiceRegexTimeoutTests : IDisposable
{
    private readonly ShortRegexTimeout _shortRegexTimeout = new();

    public void Dispose() => _shortRegexTimeout.Dispose();

    // 30 a's and no "b": "(a+)+b" has to try every way of splitting them before giving up.
    private const string EvilPattern = "(a+)+b";
    private static readonly string EvilLine = new string('a', 30) + "c";

    // Well above the five second timeout, but far below "never returns" - the point is that the
    // call comes back at all, not how quickly.
    private const int MaxSeconds = 60;

    [Fact]
    public void FindNext_CatastrophicPattern_TimesOutAsNotFound()
    {
        var lines = new List<string> { EvilLine };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.RegularExpression);

        var stopwatch = Stopwatch.StartNew();
        var lineIndex = service.FindNext(EvilPattern, lines, 0, 0);
        stopwatch.Stop();

        Assert.Equal(-1, lineIndex);
        Assert.True(stopwatch.Elapsed.TotalSeconds < MaxSeconds, $"FindNext took {stopwatch.Elapsed.TotalSeconds:0.0}s");
    }

    [Fact]
    public void ReplaceAll_CatastrophicPattern_TimesOutAndLeavesTextUnchanged()
    {
        var lines = new List<string> { EvilLine };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.RegularExpression);

        var stopwatch = Stopwatch.StartNew();
        var count = service.ReplaceAll(EvilPattern, "x");
        stopwatch.Stop();

        Assert.Equal(0, count);
        Assert.Equal(EvilLine, lines[0]);
        Assert.True(stopwatch.Elapsed.TotalSeconds < MaxSeconds, $"ReplaceAll took {stopwatch.Elapsed.TotalSeconds:0.0}s");
    }

    [Fact]
    public void Count_CatastrophicPattern_TimesOutAsZero()
    {
        var lines = new List<string> { EvilLine };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.RegularExpression);

        var stopwatch = Stopwatch.StartNew();
        var count = service.Count(EvilPattern, lines, false, FindService.FindMode.RegularExpression);
        stopwatch.Stop();

        Assert.Equal(0, count);
        Assert.True(stopwatch.Elapsed.TotalSeconds < MaxSeconds, $"Count took {stopwatch.Elapsed.TotalSeconds:0.0}s");
    }

    [Fact]
    public void FindAll_CatastrophicPattern_TimesOutAsNoMatches()
    {
        var lines = new List<string> { EvilLine };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.RegularExpression);

        var stopwatch = Stopwatch.StartNew();
        var matches = service.FindAll(EvilPattern);
        stopwatch.Stop();

        Assert.Empty(matches);
        Assert.True(stopwatch.Elapsed.TotalSeconds < MaxSeconds, $"FindAll took {stopwatch.Elapsed.TotalSeconds:0.0}s");
    }

    // A pattern that is merely long, not pathological, must still work - the timeout is a
    // backstop, not a length limit.
    [Fact]
    public void FindNext_HeavyButFinitePattern_StillMatches()
    {
        var lines = new List<string> { new string('a', 5000) + "b" };
        var service = new FindService();
        service.Initialize(lines, 0, false, FindService.FindMode.RegularExpression);

        Assert.Equal(0, service.FindNext("a+b", lines, 0, 0));
    }
}
