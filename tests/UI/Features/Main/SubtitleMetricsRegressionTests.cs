using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace UITests.Features.Main;

public class SubtitleMetricsRegressionTests
{
    [Fact]
    public void CpsOnlyStrategy_AffectsCpsButNotLineLength()
    {
        var originalSettings = Se.Settings;
        var originalStrategy = Configuration.Settings.General.CpsLineLengthStrategy;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.CpsLineLengthStrategy = nameof(CalcNoSpaceOrPunctuationCpsOnly);
            Configuration.Settings.General.CpsLineLengthStrategy = Se.Settings.General.CpsLineLengthStrategy;

            const string text = "A, B";
            var stripped = SubtitleTextInfoHelper.StripHtml(text);

            Assert.Equal(4, SubtitleTextInfoHelper.GetLineLength(stripped));
            Assert.Equal(2.0, SubtitleTextInfoHelper.GetCharactersPerSecond(text, TimeSpan.Zero, TimeSpan.FromSeconds(1)));
        }
        finally
        {
            Configuration.Settings.General.CpsLineLengthStrategy = originalStrategy;
            Se.Settings = originalSettings;
        }
    }

    [Fact]
    public void CpsError_IsGatedOnColorCharactersPerSecond_NotColorDurationTooShort()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.SubtitleMaximumCharactersPerSeconds = 1;
            Se.Settings.General.ColorCharactersPerSecond = true;
            Se.Settings.General.ColorDurationTooShort = false;

            var vm = new SubtitleLineViewModel
            {
                Text = "Hello world",
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromSeconds(1),
            };

            Assert.Contains("Cps:", vm.GetErrors(null, null), StringComparison.Ordinal);

            Se.Settings.General.ColorCharactersPerSecond = false;
            Se.Settings.General.ColorDurationTooShort = true;

            Assert.DoesNotContain("Cps:", vm.GetErrors(null, null), StringComparison.Ordinal);
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    [Fact]
    public void StrategyAwareLineLength_IsConsistentBetweenHighlightAndErrors()
    {
        var originalSettings = Se.Settings;
        var originalStrategy = Configuration.Settings.General.CpsLineLengthStrategy;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.CpsLineLengthStrategy = nameof(CalcNoSpaceOrPunctuation);
            Se.Settings.General.ColorTextTooLong = true;
            Se.Settings.General.SubtitleLineMaximumLength = 3;
            Configuration.Settings.General.CpsLineLengthStrategy = Se.Settings.General.CpsLineLengthStrategy;

            var vm = new SubtitleLineViewModel
            {
                Text = "A, B",
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromSeconds(2),
            };

            var brush = Assert.IsType<SolidColorBrush>(vm.TextBackgroundBrush);

            Assert.Equal(Colors.Transparent, brush.Color);
            Assert.DoesNotContain("Max line length", vm.GetErrors(null, null), StringComparison.Ordinal);
        }
        finally
        {
            Configuration.Settings.General.CpsLineLengthStrategy = originalStrategy;
            Se.Settings = originalSettings;
        }
    }
}
