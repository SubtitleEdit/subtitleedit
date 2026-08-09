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
        Assert.Equal(25, roundTripped[0].MergeLinesShorterThan);
        Assert.Equal(24, roundTripped[0].MinimumMillisecondsBetweenLines);
        Assert.Equal(43, roundTripped[0].SubtitleLineMaximumLength);
        Assert.Equal(25, roundTripped[0].SubtitleMaximumCharactersPerSeconds);
        Assert.Equal(400, roundTripped[0].SubtitleMaximumWordsPerMinute);
        Assert.Equal(10000, roundTripped[0].SubtitleMaximumDisplayMilliseconds);
        Assert.Equal(500, roundTripped[0].SubtitleMinimumDisplayMilliseconds);
        Assert.Equal(20, roundTripped[0].SubtitleOptimalCharactersPerSeconds);
        Assert.Equal(DialogType.DashBothLinesWithSpace, roundTripped[0].DialogStyle);
    }

    // The custom continuation style used to be dropped entirely: Serialize never wrote it and
    // Deserialize never read it, so a "Custom" profile came back with the built-in defaults.
    [Fact]
    public void SerializeDeserializeRoundTripsCustomContinuationStyle()
    {
        var original = new RulesProfile
        {
            Name = "P1",
            ContinuationStyle = ContinuationStyle.Custom,
            CustomContinuationStyle = new CustomContinuationStyle
            {
                Pause = 555,
                Suffix = "..",
                SuffixApplyIfComma = true,
                SuffixAddSpace = true,
                SuffixReplaceComma = true,
                Prefix = "-",
                PrefixAddSpace = true,
                UseDifferentStyleGap = false,
                GapSuffix = "…",
                GapSuffixApplyIfComma = false,
                GapSuffixAddSpace = true,
                GapSuffixReplaceComma = false,
                GapPrefix = "—",
                GapPrefixAddSpace = true,
            },
        };

        var roundTripped = RulesProfile.Deserialize(RulesProfile.Serialize(new List<RulesProfile> { original }));

        var ccs = roundTripped[0].CustomContinuationStyle;
        Assert.Equal(555, ccs.Pause);
        Assert.Equal("..", ccs.Suffix);
        Assert.True(ccs.SuffixApplyIfComma);
        Assert.True(ccs.SuffixAddSpace);
        Assert.True(ccs.SuffixReplaceComma);
        Assert.Equal("-", ccs.Prefix);
        Assert.True(ccs.PrefixAddSpace);
        Assert.False(ccs.UseDifferentStyleGap);
        Assert.Equal("…", ccs.GapSuffix);
        Assert.False(ccs.GapSuffixApplyIfComma);
        Assert.True(ccs.GapSuffixAddSpace);
        Assert.False(ccs.GapSuffixReplaceComma);
        Assert.Equal("—", ccs.GapPrefix);
        Assert.True(ccs.GapPrefixAddSpace);
    }

    [Fact]
    public void SerializeDeserializeRoundTripsSeveralProfiles()
    {
        var profiles = new List<RulesProfile>
        {
            new() { Name = "A", SubtitleMinimumDisplayMilliseconds = 833, DialogStyle = DialogType.DashBothLinesWithoutSpace },
            new() { Name = "B", SubtitleMinimumDisplayMilliseconds = 1400, DialogStyle = DialogType.DashSecondLineWithoutSpace },
        };

        var roundTripped = RulesProfile.Deserialize(RulesProfile.Serialize(profiles));

        Assert.Equal(2, roundTripped.Count);
        Assert.Equal("A", roundTripped[0].Name);
        Assert.Equal(833, roundTripped[0].SubtitleMinimumDisplayMilliseconds);
        Assert.Equal(DialogType.DashBothLinesWithoutSpace, roundTripped[0].DialogStyle);
        Assert.Equal("B", roundTripped[1].Name);
        Assert.Equal(1400, roundTripped[1].SubtitleMinimumDisplayMilliseconds);
        Assert.Equal(DialogType.DashSecondLineWithoutSpace, roundTripped[1].DialogStyle);
    }

    // A missing dialogStyle used to throw out of Enum.Parse and take the whole profile list with it.
    [Fact]
    public void DeserializeUsesDefaultsForMissingTagsInsteadOfThrowing()
    {
        var json = "{\"profiles\":[{\"name\":\"Sparse\"}]}";

        var profiles = RulesProfile.Deserialize(json);

        Assert.Single(profiles);
        Assert.Equal("Sparse", profiles[0].Name);
        Assert.Equal(DialogType.DashBothLinesWithSpace, profiles[0].DialogStyle);
        Assert.Equal(ContinuationStyle.None, profiles[0].ContinuationStyle);
        Assert.NotNull(profiles[0].CustomContinuationStyle);
        Assert.Equal(300, profiles[0].CustomContinuationStyle.Pause);
    }

    // Convert.ToInt32(null) returns 0, so a missing duration used to mean "flag every line".
    [Fact]
    public void DeserializeDoesNotTurnMissingNumbersIntoZero()
    {
        var json = "{\"profiles\":[{\"name\":\"Sparse\"}]}";

        var profiles = RulesProfile.Deserialize(json);

        Assert.NotEqual(0, profiles[0].SubtitleMaximumDisplayMilliseconds);
        Assert.NotEqual(0, profiles[0].SubtitleMinimumDisplayMilliseconds);
        Assert.NotEqual(0, profiles[0].SubtitleLineMaximumLength);
        Assert.NotEqual(0, profiles[0].SubtitleMaximumCharactersPerSeconds);
        Assert.NotEqual(0, profiles[0].MaxNumberOfLines);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("not json at all")]
    [InlineData("{\"profiles\":[]}")]
    public void DeserializeReturnsEmptyForUnusableInput(string? input)
    {
        Assert.Empty(RulesProfile.Deserialize(input));
    }
}
