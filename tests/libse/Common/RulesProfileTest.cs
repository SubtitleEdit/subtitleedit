using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;

namespace LibSETests.Common;

public class RulesProfileTest
{
    [Fact]
    public void DefaultConstructorHasCustomContinuationStyle()
    {
        var p = new RulesProfile();

        Assert.NotNull(p.CustomContinuationStyle);
        Assert.Equal(300, p.CustomContinuationStyle.Pause);
        Assert.True(p.CustomContinuationStyle.UseDifferentStyleGap);
        Assert.Equal("...", p.CustomContinuationStyle.GapSuffix);
    }

    [Fact]
    public void CopyConstructorDeepCopiesCustomContinuationStyle()
    {
        var original = new RulesProfile();
        original.CustomContinuationStyle.Suffix = "..";
        original.CustomContinuationStyle.Pause = 555;

        var copy = new RulesProfile(original);

        // values copied
        Assert.Equal("..", copy.CustomContinuationStyle.Suffix);
        Assert.Equal(555, copy.CustomContinuationStyle.Pause);

        // and it is a deep copy: mutating the copy must not affect the original
        copy.CustomContinuationStyle.Suffix = "changed";
        Assert.NotSame(original.CustomContinuationStyle, copy.CustomContinuationStyle);
        Assert.Equal("..", original.CustomContinuationStyle.Suffix);
    }

    [Fact]
    public void SerializeDeserializeRoundTripsBaseFields()
    {
        var original = new RulesProfile
        {
            Name = "P1",
            MaxNumberOfLines = 3,
            CpsLineLengthStrategy = string.Empty,
            MergeLinesShorterThan = 25,
            MinimumMillisecondsBetweenLines = 24,
            SubtitleLineMaximumLength = 43,
            SubtitleMaximumCharactersPerSeconds = 25,
            SubtitleMaximumWordsPerMinute = 400,
            SubtitleMaximumDisplayMilliseconds = 10000,
            SubtitleMinimumDisplayMilliseconds = 500,
            SubtitleOptimalCharactersPerSeconds = 20,
            DialogStyle = DialogType.DashBothLinesWithSpace,
            ContinuationStyle = ContinuationStyle.LeadingTrailingDash,
        };

        var json = RulesProfile.Serialize(new List<RulesProfile> { original });
        var roundTripped = RulesProfile.Deserialize(json);

        Assert.Single(roundTripped);
        Assert.Equal("P1", roundTripped[0].Name);
        Assert.Equal(3, roundTripped[0].MaxNumberOfLines);
        Assert.Equal(ContinuationStyle.LeadingTrailingDash, roundTripped[0].ContinuationStyle);
    }
}
