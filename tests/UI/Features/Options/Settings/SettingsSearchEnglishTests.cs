using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic.Config.Language;

namespace UITests.Features.Options.Settings;

/// <summary>
/// With a translated UI the Settings search also matches the English names used in docs and
/// tutorials (#15945).
/// </summary>
public class SettingsSearchEnglishTests
{
    [Fact]
    public void Build_MapsTranslatedStringsToEnglish()
    {
        var english = new SeLanguage();
        var translated = new SeLanguage();
        translated.Options.Settings.MinGapMs = "Mindestabstand (ms)";
        translated.General.Profiles = "Profile";

        var map = EnglishLabelLookup.Build(translated, english);

        Assert.Equal(english.Options.Settings.MinGapMs, map["Mindestabstand (ms)"]);
        Assert.Equal(english.General.Profiles, map["Profile"]);
        Assert.DoesNotContain(english.Options.Settings.MaxLines, map.Keys); // untranslated = same text
    }

    [Fact]
    public void Build_JoinsSeveralEnglishStringsWithSameTranslation()
    {
        var english = new SeLanguage();
        var translated = new SeLanguage();
        translated.Options.Settings.MinGapMs = "X";
        translated.Options.Settings.MaxLines = "X";

        var map = EnglishLabelLookup.Build(translated, english);

        Assert.Contains(english.Options.Settings.MinGapMs, map["X"].Split('\n'));
        Assert.Contains(english.Options.Settings.MaxLines, map["X"].Split('\n'));
    }

    [Fact]
    public void Filter_MatchesExplicitEnglishLabel()
    {
        var item = new SettingsItem("Mindestabstand", () => null!) { EnglishLabel = "Min gap (ms)" };

        item.Filter("gap");
        Assert.True(item.IsVisible);

        item.Filter("abstand");
        Assert.True(item.IsVisible);

        item.Filter("duration");
        Assert.False(item.IsVisible);
    }
}
