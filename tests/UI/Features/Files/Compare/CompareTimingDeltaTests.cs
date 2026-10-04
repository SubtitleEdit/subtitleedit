using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace UITests.Features.Files.Compare;

/// <summary>
/// Differing times are shown as how far the reference is off, not as filled cells that looked
/// like a text change (#15622).
/// </summary>
public class CompareTimingDeltaTests : IDisposable
{
    private readonly SeCompare _savedSettings = Se.Settings.File.Compare;
    private readonly CultureInfo _savedCulture = CultureInfo.CurrentCulture;

    public CompareTimingDeltaTests()
    {
        Se.Settings.File.Compare = new SeCompare();
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    public void Dispose()
    {
        Se.Settings.File.Compare = _savedSettings;
        CultureInfo.CurrentCulture = _savedCulture;
    }

    [AvaloniaFact]
    public void SameTextDifferentTimes_ShowsTheDeltaOfStartAndEnd()
    {
        var row = CompareOne(("Same", 1000, 3000), ("Same", 1047, 2988));

        Assert.Equal(CompareRowKind.Changed, row.Kind);
        Assert.True(row.HasTimingDelta);
        Assert.Equal("Δ +47ms → −12ms", row.TimingDeltaDisplay);
    }

    [AvaloniaFact]
    public void OnlyTheTextDiffers_ShowsNoDelta()
    {
        var row = CompareOne(("One", 1000, 3000), ("Two", 1000, 3000));

        Assert.Equal(CompareRowKind.Changed, row.Kind);
        Assert.False(row.HasTimingDelta);
        Assert.Equal(string.Empty, row.TimingDeltaDisplay);
    }

    [AvaloniaFact]
    public void SameLine_ShowsNoDelta()
    {
        var row = CompareOne(("Same", 1000, 3000), ("Same", 1000, 3000));

        Assert.False(row.HasTimingDelta);
    }

    [AvaloniaTheory]
    [InlineData(0, "0ms")]
    [InlineData(5, "+5ms")]
    [InlineData(-999, "−999ms")]
    [InlineData(1470, "+1.470s")]
    [InlineData(-2005, "−2.005s")]
    public void FormatDelta_UsesMillisecondsUnderASecond(int milliseconds, string expected)
    {
        Assert.Equal(expected, CompareRow.FormatDelta(TimeSpan.FromMilliseconds(milliseconds)));
    }

    private static CompareRow CompareOne((string Text, int Start, int End) left, (string Text, int Start, int End) right)
    {
        var vm = new CompareViewModel(new FileHelper(), new FolderHelper());
        vm.Initialize(MakeLine(left), "left.srt", MakeLine(right), "right.srt", false);
        for (var pump = 0; pump < 12; pump++)
        {
            Dispatcher.UIThread.RunJobs();
        }

        return vm.Rows.Single();
    }

    private static ObservableCollection<SubtitleLineViewModel> MakeLine((string Text, int Start, int End) line)
    {
        return new ObservableCollection<SubtitleLineViewModel>
        {
            new(new Paragraph(line.Text, line.Start, line.End), null!) { Number = 1 },
        };
    }
}
