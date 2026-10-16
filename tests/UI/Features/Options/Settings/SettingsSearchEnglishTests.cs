using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic.Config.Language;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

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
        Assert.DoesNotContain(english.General.MaxNumberOfLines, map.Keys); // untranslated = same text
    }

    [Fact]
    public void Build_JoinsSeveralEnglishStringsWithSameTranslation()
    {
        var english = new SeLanguage();
        var translated = new SeLanguage();
        translated.Options.Settings.MinGapMs = "X";
        translated.General.MaxNumberOfLines = "X";

        var map = EnglishLabelLookup.Build(translated, english);

        Assert.Contains(english.Options.Settings.MinGapMs, map["X"].Split('\n'));
        Assert.Contains(english.General.MaxNumberOfLines, map["X"].Split('\n'));
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

    /// <summary>
    /// The lookup round-trips a loaded translation through <see cref="SeLanguageJsonContext"/>.
    /// A non-string language property (an <c>object</c> deserializes to a JsonElement the
    /// source-generated context can't write) made that throw, so the map was silently empty and
    /// English names never matched with any shipped translation.
    /// </summary>
    [Theory]
    [MemberData(nameof(TranslationFiles))]
    public void Build_WorksForShippedTranslation(string fileName)
    {
        using var stream = File.OpenRead(Path.Combine(LanguagesFolder(), fileName));
        var translated = JsonSerializer.Deserialize(stream, SeLanguageJsonContext.Default.SeLanguage)!;

        JsonSerializer.SerializeToElement(translated, SeLanguageJsonContext.Default.SeLanguage); // throws naming the bad property
        var map = EnglishLabelLookup.Build(translated, new SeLanguage());

        Assert.NotEmpty(map);
    }

    public static TheoryData<string> TranslationFiles()
    {
        var data = new TheoryData<string>();
        foreach (var path in Directory.GetFiles(LanguagesFolder(), "*.json").Order())
        {
            var fileName = Path.GetFileName(path);
            if (fileName != "English.json")
            {
                data.Add(fileName);
            }
        }

        return data;
    }

    private static string LanguagesFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "src", "ui")))
        {
            dir = dir.Parent;
        }

        return Path.Combine(dir?.FullName ?? throw new DirectoryNotFoundException("Could not find repo root"),
            "src", "ui", "Assets", "Languages");
    }
}
