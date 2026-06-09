using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;

namespace LibSETests.Common;

public class CustomContinuationStyleTest
{
    private static CustomContinuationStyle MakeNonDefault()
    {
        return new CustomContinuationStyle
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
        };
    }

    [Fact]
    public void DefaultsMatchLegacyGeneralSettings()
    {
        var style = new CustomContinuationStyle();
        var general = new GeneralSettings();

        Assert.Equal(general.ContinuationPause, style.Pause);
        Assert.Equal(general.CustomContinuationStyleSuffix, style.Suffix);
        Assert.Equal(general.CustomContinuationStyleUseDifferentStyleGap, style.UseDifferentStyleGap);
        Assert.Equal(general.CustomContinuationStyleGapSuffix, style.GapSuffix);
        Assert.Equal(general.CustomContinuationStyleGapSuffixApplyIfComma, style.GapSuffixApplyIfComma);
        Assert.Equal(general.CustomContinuationStyleGapPrefix, style.GapPrefix);
    }

    [Fact]
    public void CopyConstructorCopiesEveryField()
    {
        var original = MakeNonDefault();

        var copy = new CustomContinuationStyle(original);

        Assert.Equal(original.Pause, copy.Pause);
        Assert.Equal(original.Suffix, copy.Suffix);
        Assert.Equal(original.SuffixApplyIfComma, copy.SuffixApplyIfComma);
        Assert.Equal(original.SuffixAddSpace, copy.SuffixAddSpace);
        Assert.Equal(original.SuffixReplaceComma, copy.SuffixReplaceComma);
        Assert.Equal(original.Prefix, copy.Prefix);
        Assert.Equal(original.PrefixAddSpace, copy.PrefixAddSpace);
        Assert.Equal(original.UseDifferentStyleGap, copy.UseDifferentStyleGap);
        Assert.Equal(original.GapSuffix, copy.GapSuffix);
        Assert.Equal(original.GapSuffixApplyIfComma, copy.GapSuffixApplyIfComma);
        Assert.Equal(original.GapSuffixAddSpace, copy.GapSuffixAddSpace);
        Assert.Equal(original.GapSuffixReplaceComma, copy.GapSuffixReplaceComma);
        Assert.Equal(original.GapPrefix, copy.GapPrefix);
        Assert.Equal(original.GapPrefixAddSpace, copy.GapPrefixAddSpace);
    }

    [Fact]
    public void CopyConstructorWithNullYieldsDefaults()
    {
        var copy = new CustomContinuationStyle(null);
        var defaults = new CustomContinuationStyle();

        Assert.Equal(defaults.Pause, copy.Pause);
        Assert.Equal(defaults.GapSuffix, copy.GapSuffix);
        Assert.Equal(defaults.UseDifferentStyleGap, copy.UseDifferentStyleGap);
    }

    [Fact]
    public void ApplyToAndFromGeneralSettingsRoundTrips()
    {
        var original = MakeNonDefault();
        var general = new GeneralSettings();

        original.ApplyToGeneralSettings(general);
        var read = CustomContinuationStyle.FromGeneralSettings(general);

        // verify it actually landed on the flat fields the engine reads
        Assert.Equal(555, general.ContinuationPause);
        Assert.Equal("..", general.CustomContinuationStyleSuffix);
        Assert.Equal("…", general.CustomContinuationStyleGapSuffix);
        Assert.Equal("—", general.CustomContinuationStyleGapPrefix);
        Assert.False(general.CustomContinuationStyleUseDifferentStyleGap);

        // and the round-trip is loss-free
        Assert.Equal(original.Pause, read.Pause);
        Assert.Equal(original.Suffix, read.Suffix);
        Assert.Equal(original.GapSuffix, read.GapSuffix);
        Assert.Equal(original.GapPrefix, read.GapPrefix);
        Assert.Equal(original.UseDifferentStyleGap, read.UseDifferentStyleGap);
        Assert.Equal(original.GapSuffixReplaceComma, read.GapSuffixReplaceComma);
    }
}
