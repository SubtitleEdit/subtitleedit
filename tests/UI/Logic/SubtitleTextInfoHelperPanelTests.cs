using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Logic;

/// <summary>
/// The single-line-length panel is refilled on every keystroke in the edit box, so it reuses the
/// text blocks that are already there instead of clearing and rebuilding the panel. These tests
/// pin down that reuse produces the same children as a rebuild would - in particular when the
/// line count shrinks (leftover labels must go) and when an error highlight has to be dropped.
/// </summary>
public class SubtitleTextInfoHelperPanelTests
{
    private static List<string> LabelTexts(StackPanel panel)
        => panel.Children.Cast<TextBlock>().Select(t => t.Text ?? string.Empty).ToList();

    [AvaloniaFact]
    public void Refill_WithFewerLines_DropsLeftoverLabels()
    {
        var panel = new StackPanel();
        var header = Se.Language.Main.SingleLineLength;

        SubtitleTextInfoHelper.FillLineLengthPanel(panel, new List<string> { "abc", "de", "f" }, false, 100);
        Assert.Equal(new List<string> { header, "3", "/", "2", "/", "1" }, LabelTexts(panel));

        SubtitleTextInfoHelper.FillLineLengthPanel(panel, new List<string> { "abcd" }, false, 100);
        Assert.Equal(new List<string> { header, "4" }, LabelTexts(panel));

        SubtitleTextInfoHelper.FillLineLengthPanel(panel, new List<string> { "ab", "cde" }, false, 100);
        Assert.Equal(new List<string> { header, "2", "/", "3" }, LabelTexts(panel));
    }

    [AvaloniaFact]
    public void Refill_ClearsErrorBackground_WhenLineIsNoLongerTooLong()
    {
        var panel = new StackPanel();

        SubtitleTextInfoHelper.FillLineLengthPanel(panel, new List<string> { "abcdef" }, true, 3);
        var lengthLabel = (TextBlock)panel.Children[1];
        Assert.NotNull(lengthLabel.Background);

        SubtitleTextInfoHelper.FillLineLengthPanel(panel, new List<string> { "ab" }, true, 3);
        Assert.Same(lengthLabel, panel.Children[1]);
        Assert.Null(lengthLabel.Background);
    }

    [AvaloniaFact]
    public void Refill_ReusesTheSameTextBlockInstances()
    {
        var panel = new StackPanel();

        SubtitleTextInfoHelper.FillLineLengthPanel(panel, new List<string> { "abc" }, false, 100);
        var before = panel.Children.ToList();

        SubtitleTextInfoHelper.FillLineLengthPanel(panel, new List<string> { "abcd" }, false, 100);

        Assert.Equal(before, panel.Children.ToList());
        Assert.Equal("4", ((TextBlock)panel.Children[1]).Text);
    }

    [AvaloniaFact]
    public void Refill_ReplacesAForeignControl()
    {
        var panel = new StackPanel();
        panel.Children.Add(new Border { Background = Brushes.Red });

        SubtitleTextInfoHelper.FillLineLengthPanel(panel, new List<string> { "abc" }, false, 100);

        Assert.Equal(new List<string> { Se.Language.Main.SingleLineLength, "3" }, LabelTexts(panel));
    }
}
