using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

public class MacOsFontMigrationTests
{
    [Theory]
    [InlineData(".AppleSystemUIFont")]
    [InlineData("Default")]
    public void LegacyDefaultFontMigratesToHelveticaNeue(string fontName)
    {
        var appearance = new SeAppearance
        {
            FontName = fontName,
            MacOsFontMigrationVersion = null,
        };

        Se.MigrateMacOsFontSettings(appearance, true, true);

        Assert.Equal("Helvetica Neue", appearance.FontName);
        Assert.Equal(Se.CurrentMacOsFontMigrationVersion, appearance.MacOsFontMigrationVersion);
    }

    [Fact]
    public void LegacyCustomFontIsPreservedAndMarkedMigrated()
    {
        var appearance = new SeAppearance
        {
            FontName = "Avenir Next",
            MacOsFontMigrationVersion = null,
        };

        Se.MigrateMacOsFontSettings(appearance, true, true);

        Assert.Equal("Avenir Next", appearance.FontName);
        Assert.Equal(Se.CurrentMacOsFontMigrationVersion, appearance.MacOsFontMigrationVersion);
    }

    [Fact]
    public void ExplicitSystemFontAfterMigrationIsPreserved()
    {
        var appearance = new SeAppearance
        {
            FontName = ".AppleSystemUIFont",
            MacOsFontMigrationVersion = Se.CurrentMacOsFontMigrationVersion,
        };

        Se.MigrateMacOsFontSettings(appearance, true, true);

        Assert.Equal(".AppleSystemUIFont", appearance.FontName);
    }

    [Fact]
    public void FreshSettingsAreMarkedWithoutChangingFont()
    {
        var appearance = new SeAppearance
        {
            FontName = ".AppleSystemUIFont",
            MacOsFontMigrationVersion = null,
        };

        Se.MigrateMacOsFontSettings(appearance, true, false);

        Assert.Equal(".AppleSystemUIFont", appearance.FontName);
        Assert.Equal(Se.CurrentMacOsFontMigrationVersion, appearance.MacOsFontMigrationVersion);
    }

    [Fact]
    public void NonMacOsSettingsAreNotChanged()
    {
        var appearance = new SeAppearance
        {
            FontName = ".AppleSystemUIFont",
            MacOsFontMigrationVersion = null,
        };

        Se.MigrateMacOsFontSettings(appearance, false, true);

        Assert.Equal(".AppleSystemUIFont", appearance.FontName);
        Assert.Null(appearance.MacOsFontMigrationVersion);
    }

    [Fact]
    public void LegacyDefaultDisplaysAsHelveticaNeueInSettings()
    {
        var fontNames = new List<string> { "System Font", "Helvetica Neue", "Avenir Next" };

        var selectedFont = SettingsViewModel.MapMacOsFontNameForDisplay("Default", fontNames);

        Assert.Equal("Helvetica Neue", selectedFont);
    }

    [Fact]
    public void ExplicitSystemFontDisplaysAsSystemFontInSettings()
    {
        var fontNames = new List<string> { "System Font", "Helvetica Neue", "Avenir Next" };

        var selectedFont = SettingsViewModel.MapMacOsFontNameForDisplay(".AppleSystemUIFont", fontNames);

        Assert.Equal("System Font", selectedFont);
    }
}
