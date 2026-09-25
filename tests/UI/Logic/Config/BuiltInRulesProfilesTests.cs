using System.Linq;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

public class BuiltInRulesProfilesTests
{
    // Issue #15295: a built-in profile for the 1-3 words per cue Shorts/TikTok style.
    [Fact]
    public void TikTokShortWordsProfile_IsOneShortLine()
    {
        var profile = new SeGeneral().Profiles.Single(p => p.Name == "TikTok/YouTube-shorts (1-3 words)");

        Assert.Equal(1, profile.MaxNumberOfLines);
        Assert.Equal(15, profile.SubtitleLineMaximumLength);
        Assert.Equal(300, profile.SubtitleMinimumDisplayMilliseconds);

        // Text at or under the line length must stay on one line when auto-breaking.
        Assert.True(profile.MergeLinesShorterThan > profile.SubtitleLineMaximumLength);
    }
}
