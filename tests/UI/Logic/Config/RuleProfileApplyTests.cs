using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

public class RuleProfileApplyTests
{
    private static RulesProfile MakeProfile()
    {
        return new RulesProfile
        {
            Name = "Test profile",
            SubtitleLineMaximumLength = 42,
            SubtitleOptimalCharactersPerSeconds = 15,
            SubtitleMaximumCharactersPerSeconds = 20,
            SubtitleMaximumWordsPerMinute = 240,
            SubtitleMinimumDisplayMilliseconds = 833,
            SubtitleMaximumDisplayMilliseconds = 7007,
            MinimumMillisecondsBetweenLines = 83,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 43,
            DialogStyle = DialogType.DashBothLinesWithoutSpace,
            ContinuationStyle = ContinuationStyle.NoneLeadingTrailingEllipsis,
            CpsLineLengthStrategy = nameof(CalcCjk),
        };
    }

    // The profile picker used to apply these fields inline and silently dropped the two duration
    // limits, so picking e.g. "Netflix (English)" left min/max duration untouched.
    [Fact]
    public void ApplyRuleProfile_CopiesEveryRuleIntoGeneralSettings()
    {
        var savedSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.SubtitleMinimumDisplayMilliseconds = 1;
            Se.Settings.General.SubtitleMaximumDisplayMilliseconds = 2;

            Se.ApplyRuleProfile(MakeProfile());

            var g = Se.Settings.General;
            Assert.Equal("Test profile", g.CurrentProfile);
            Assert.Equal(42, g.SubtitleLineMaximumLength);
            Assert.Equal(15, g.SubtitleOptimalCharactersPerSeconds);
            Assert.Equal(20, g.SubtitleMaximumCharactersPerSeconds);
            Assert.Equal(240, g.SubtitleMaximumWordsPerMinute);
            Assert.Equal(833, g.SubtitleMinimumDisplayMilliseconds);
            Assert.Equal(7007, g.SubtitleMaximumDisplayMilliseconds);
            Assert.Equal(83, g.MinimumBetweenLines.Milliseconds);
            Assert.Equal(2, g.MaxNumberOfLines);
            Assert.Equal(43, g.UnbreakLinesShorterThan);
            Assert.Equal(nameof(DialogType.DashBothLinesWithoutSpace), g.DialogStyle);
            Assert.Equal(nameof(ContinuationStyle.NoneLeadingTrailingEllipsis), g.ContinuationStyle);
            Assert.Equal(nameof(CalcCjk), g.CpsLineLengthStrategy);
        }
        finally
        {
            Se.Settings = savedSettings;
        }
    }

    // The gap setting is ms-or-frames and frame mode reads the frame value, so a profile that
    // only carries milliseconds has to populate both.
    [Fact]
    public void ApplyRuleProfile_DerivesTheGapInFramesFromMilliseconds()
    {
        var savedSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.MinimumBetweenLines.Frames = 99;

            Se.ApplyRuleProfile(MakeProfile());

            Assert.Equal(SubtitleFormat.MillisecondsToFrames(83), Se.Settings.General.MinimumBetweenLines.Frames);
            Assert.NotEqual(99, Se.Settings.General.MinimumBetweenLines.Frames);
        }
        finally
        {
            Se.Settings = savedSettings;
        }
    }

    // libse enforces duration limits from its own Configuration, which defaults to 1000/8000.
    // Without this bridge the user's limits never reached RecalculateDisplayTimes and friends.
    [Fact]
    public void ApplyRuleSettingsToLibSe_BridgesEveryRuleIncludingTheDurationLimits()
    {
        var savedSettings = Se.Settings;
        var savedLibSe = new GeneralSettings();
        CopyRuleSettings(Configuration.Settings.General, savedLibSe);
        try
        {
            Se.Settings = new Se();
            var g = Se.Settings.General;
            g.SubtitleLineMaximumLength = 42;
            g.SubtitleOptimalCharactersPerSeconds = 15;
            g.SubtitleMaximumCharactersPerSeconds = 20;
            g.SubtitleMaximumWordsPerMinute = 240;
            g.SubtitleMinimumDisplayMilliseconds = 833;
            g.SubtitleMaximumDisplayMilliseconds = 7007;
            g.MinimumBetweenLines.Milliseconds = 83;
            g.UseFrameMode = false;
            g.MaxNumberOfLines = 2;
            g.UnbreakLinesShorterThan = 43;
            g.CpsLineLengthStrategy = nameof(CalcCjk);
            g.DialogStyle = nameof(DialogType.DashBothLinesWithoutSpace);

            Se.ApplyRuleSettingsToLibSe();

            var libSe = Configuration.Settings.General;
            Assert.Equal(42, libSe.SubtitleLineMaximumLength);
            Assert.Equal(15, libSe.SubtitleOptimalCharactersPerSeconds);
            Assert.Equal(20, libSe.SubtitleMaximumCharactersPerSeconds);
            Assert.Equal(240, libSe.SubtitleMaximumWordsPerMinute);
            Assert.Equal(833, libSe.SubtitleMinimumDisplayMilliseconds);
            Assert.Equal(7007, libSe.SubtitleMaximumDisplayMilliseconds);
            Assert.Equal(83, libSe.MinimumMillisecondsBetweenLines);
            Assert.Equal(2, libSe.MaxNumberOfLines);
            Assert.Equal(43, libSe.MergeLinesShorterThan);
            Assert.Equal(nameof(CalcCjk), libSe.CpsLineLengthStrategy);
            Assert.Equal(DialogType.DashBothLinesWithoutSpace, libSe.DialogStyle);
        }
        finally
        {
            Se.Settings = savedSettings;
            CopyRuleSettings(savedLibSe, Configuration.Settings.General);
        }
    }

    /// <summary>Snapshot/restore helper - the bridge writes global libse state that later tests read.</summary>
    private static void CopyRuleSettings(GeneralSettings from, GeneralSettings to)
    {
        to.SubtitleLineMaximumLength = from.SubtitleLineMaximumLength;
        to.SubtitleOptimalCharactersPerSeconds = from.SubtitleOptimalCharactersPerSeconds;
        to.SubtitleMaximumCharactersPerSeconds = from.SubtitleMaximumCharactersPerSeconds;
        to.SubtitleMaximumWordsPerMinute = from.SubtitleMaximumWordsPerMinute;
        to.SubtitleMinimumDisplayMilliseconds = from.SubtitleMinimumDisplayMilliseconds;
        to.SubtitleMaximumDisplayMilliseconds = from.SubtitleMaximumDisplayMilliseconds;
        to.MinimumMillisecondsBetweenLines = from.MinimumMillisecondsBetweenLines;
        to.MaxNumberOfLines = from.MaxNumberOfLines;
        to.MergeLinesShorterThan = from.MergeLinesShorterThan;
        to.CpsLineLengthStrategy = from.CpsLineLengthStrategy;
        to.DialogStyle = from.DialogStyle;
        to.ContinuationStyle = from.ContinuationStyle;
        CustomContinuationStyle.FromGeneralSettings(from).ApplyToGeneralSettings(to);
    }
}
