using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// The hover details on a tinted grid cell (#17188) must exist exactly when the cell is tinted -
/// a red cell with no explanation, or an explanation on a clean cell, is the bug.
/// </summary>
public class SubtitleGridErrorCellDetailsTests
{
    private static SubtitleLineViewModel Line(string text, double startMs, double endMs)
        => new()
        {
            Text = text,
            StartTime = TimeSpan.FromMilliseconds(startMs),
            EndTime = TimeSpan.FromMilliseconds(endMs),
        };

    private static List<SubtitleLineViewModel> MakeLines()
    {
        var lines = new List<SubtitleLineViewModel>
        {
            Line("Hello there.", 1000, 3000),
            Line("Hi.", 3100, 3200),
            Line("A very long single line that is well past forty-three characters.", 4000, 12000),
            Line("Way too much text to read in this little time, honestly.", 12100, 12900),
            Line("Line one" + Environment.NewLine + "Line two" + Environment.NewLine + "Line three", 13000, 16000),
            Line("<i>Overlapping.</i>", 15500, 17000),
            Line("Barely a gap.", 17005, 19000),
            Line("In range gap.", 19300, 21000),
            Line(string.Empty, 22000, 24000),
        };
        SubtitleTextInfoHelper.UpdateGaps(lines);
        return lines;
    }

    private static bool IsTinted(IBrush brush)
        => brush is ISolidColorBrush solid && solid.Color.A != 0;

    private static IBrush BrushFor(SubtitleLineViewModel line, SubtitleGridErrorCell cell) => cell switch
    {
        SubtitleGridErrorCell.StartTime => line.StartTimeBackgroundBrush,
        SubtitleGridErrorCell.EndTime => line.EndTimeBackgroundBrush,
        SubtitleGridErrorCell.Duration => line.DurationBackgroundBrush,
        SubtitleGridErrorCell.Text => line.TextBackgroundBrush,
        SubtitleGridErrorCell.Gap => line.GapBackgroundBrush,
        SubtitleGridErrorCell.Cps => line.CpsBackgroundBrush,
        SubtitleGridErrorCell.Wpm => line.WpmBackgroundBrush,
        SubtitleGridErrorCell.ShotIn => line.ShotInBackgroundBrush,
        SubtitleGridErrorCell.ShotOut => line.ShotOutBackgroundBrush,
        _ => throw new ArgumentOutOfRangeException(nameof(cell)),
    };

    private static int AssertDetailsMatchTint()
    {
        var tinted = 0;
        foreach (var line in MakeLines())
        {
            foreach (var cell in Enum.GetValues<SubtitleGridErrorCell>())
            {
                var isTinted = IsTinted(BrushFor(line, cell));
                var details = line.GetCellErrorDetails(cell);
                Assert.True(isTinted == (details.Length > 0),
                    $"'{line.Text}' {cell}: tinted={isTinted}, details='{details}'");
                if (isTinted)
                {
                    tinted++;
                }
            }
        }

        return tinted;
    }

    [AvaloniaFact]
    public void Details_ExistExactlyWhenCellIsTinted_GapColumnHidden()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            var general = Se.Settings.General;
            general.ColorTextTooWide = false;
            general.ColorGapInRange = true;
            general.ShowColumnGap = false;
            general.ShowColumnCps = true;

            Assert.True(AssertDetailsMatchTint() >= 8);
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    [AvaloniaFact]
    public void Details_ExistExactlyWhenCellIsTinted_GapColumnShown_CpsHidden()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            var general = Se.Settings.General;
            general.ColorTextTooWide = false;
            general.ColorGapInRange = true;
            general.ColorTimeCodeOverlap = false;
            general.ShowColumnGap = true;
            general.ShowColumnCps = false;

            Assert.True(AssertDetailsMatchTint() >= 5);
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    [AvaloniaFact]
    public void Details_ExplainTheRule()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.ShowColumnGap = false;
            var lines = MakeLines();

            Assert.Contains(Se.Language.General.TooShort, lines[1].GetCellErrorDetails(SubtitleGridErrorCell.Duration));
            Assert.Contains(Se.Language.ErrorList.LineTooLong, lines[2].GetCellErrorDetails(SubtitleGridErrorCell.Text));
            Assert.Contains(Se.Language.General.Overlapping, lines[5].GetCellErrorDetails(SubtitleGridErrorCell.StartTime));
            Assert.Contains(Se.Language.General.Overlapping, lines[4].GetCellErrorDetails(SubtitleGridErrorCell.EndTime));
            Assert.Contains(Se.Language.ErrorList.GapTooShort, lines[6].GetCellErrorDetails(SubtitleGridErrorCell.StartTime));
            Assert.Equal(string.Empty, lines[0].GetCellErrorDetails(SubtitleGridErrorCell.Text));
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    [AvaloniaFact]
    public void Details_ShotChangeWarnings()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.UseFrameMode = true;
            var line = Line("Text", 1000, 2000);
            line.SetShotChangeOffsets(40, 1, true, double.NaN, 0, false);

            var details = line.GetCellErrorDetails(SubtitleGridErrorCell.ShotIn);
            Assert.Contains(Se.Language.ErrorList.StartTooCloseToShotChange, details);
            Assert.Contains("+1", details);
            Assert.Equal(string.Empty, line.GetCellErrorDetails(SubtitleGridErrorCell.ShotOut));
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    private static (bool cancelled, object? tip) OpenToolTip(Border cell)
    {
        var args = new CancelRoutedEventArgs(ToolTip.ToolTipOpeningEvent, cell);
        cell.RaiseEvent(args);
        return (args.Cancel, ToolTip.GetTip(cell));
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void GridCell_ToolTip_ShowsDetailsOnlyForErrorsAndWhenEnabled(bool enabled)
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.ShowErrorDetailsOnHover = enabled;

            var shortLine = Line("Hi.", 1000, 1100);
            var cell = InitListViewAndEditBox.MakeErrorCell(nameof(SubtitleLineViewModel.DurationBackgroundBrush), SubtitleGridErrorCell.Duration);
            cell.DataContext = shortLine;
            var (cancelled, tip) = OpenToolTip(cell);
            Assert.Equal(!enabled, cancelled);
            if (enabled)
            {
                Assert.Contains(Se.Language.General.TooShort, Assert.IsType<string>(tip));
            }

            var cleanCell = InitListViewAndEditBox.MakeErrorCell(nameof(SubtitleLineViewModel.DurationBackgroundBrush), SubtitleGridErrorCell.Duration);
            cleanCell.DataContext = Line("Hello there.", 1000, 3000);
            Assert.True(OpenToolTip(cleanCell).cancelled);
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }
}
