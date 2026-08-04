using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace UITests.Features.Main;

/// <summary>
/// <see cref="SubtitleLineViewModel.TextBackgroundBrush"/> memoizes its verdict across cell
/// repaints, so these tests pin down that the memo still notices a changed setting (and a
/// changed error colour) for an otherwise unchanged line.
/// </summary>
public class SubtitleLineViewModelCacheTests
{
    private static Color ColorOf(IBrush brush) => Assert.IsType<SolidColorBrush>(brush).Color;

    [AvaloniaFact]
    public void TextBackgroundBrush_FollowsMaxLineLengthChange()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.ColorTextTooLong = true;
            Se.Settings.General.SubtitleLineMaximumLength = 100;

            var vm = new SubtitleLineViewModel
            {
                Text = "Hello there, world.",
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromSeconds(2),
            };

            Assert.Equal(Colors.Transparent, ColorOf(vm.TextBackgroundBrush));

            Se.Settings.General.SubtitleLineMaximumLength = 5;

            Assert.NotEqual(Colors.Transparent, ColorOf(vm.TextBackgroundBrush));
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    [AvaloniaFact]
    public void TextBackgroundBrush_FollowsMaxNumberOfLinesChange()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.ColorTextTooLong = false;
            Se.Settings.General.ColorTextTooWide = false;
            Se.Settings.General.ColorTextTooManyLines = true;
            Se.Settings.General.MaxNumberOfLines = 3;

            var vm = new SubtitleLineViewModel
            {
                Text = "Line one" + Environment.NewLine + "Line two",
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromSeconds(2),
            };

            Assert.Equal(Colors.Transparent, ColorOf(vm.TextBackgroundBrush));

            Se.Settings.General.MaxNumberOfLines = 1;

            Assert.NotEqual(Colors.Transparent, ColorOf(vm.TextBackgroundBrush));
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    [AvaloniaFact]
    public void TextBackgroundBrush_FollowsErrorColorChange()
    {
        var originalSettings = Se.Settings;
        var originalErrorColor = SubtitleLineViewModel.ErrorColor;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.ColorTextTooLong = true;
            Se.Settings.General.SubtitleLineMaximumLength = 5;
            SubtitleLineViewModel.ErrorColor = Colors.Red;

            var vm = new SubtitleLineViewModel
            {
                Text = "Hello there, world.",
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromSeconds(2),
            };

            Assert.Equal(Colors.Red, ColorOf(vm.TextBackgroundBrush));

            SubtitleLineViewModel.ErrorColor = Colors.Green;

            Assert.Equal(Colors.Green, ColorOf(vm.TextBackgroundBrush));
        }
        finally
        {
            SubtitleLineViewModel.ErrorColor = originalErrorColor;
            Se.Settings = originalSettings;
        }
    }

    [AvaloniaFact]
    public void TextBackgroundBrush_FollowsTextChange()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.ColorTextTooLong = true;
            Se.Settings.General.SubtitleLineMaximumLength = 10;

            var vm = new SubtitleLineViewModel
            {
                Text = "Short",
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromSeconds(2),
            };

            Assert.Equal(Colors.Transparent, ColorOf(vm.TextBackgroundBrush));

            vm.Text = "A line that is clearly longer than ten characters.";

            Assert.NotEqual(Colors.Transparent, ColorOf(vm.TextBackgroundBrush));
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }
}
