using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace UITests.Features.Options.Settings;

public class ContinuationSettingsRegressionTests
{
    [Fact]
    public void ApplyResetSelections_ResetRules_RestoresContinuationSettingsDefaults()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            var general = Se.Settings.General;
            general.ContinuationPause = 1450;
            general.CustomContinuationStyleSuffix = "!!";
            general.CustomContinuationStyleSuffixApplyIfComma = true;
            general.CustomContinuationStyleSuffixAddSpace = true;
            general.CustomContinuationStyleSuffixReplaceComma = true;
            general.CustomContinuationStylePrefix = "--";
            general.CustomContinuationStylePrefixAddSpace = true;
            general.CustomContinuationStyleUseDifferentStyleGap = false;
            general.CustomContinuationStyleGapSuffix = "??";
            general.CustomContinuationStyleGapSuffixApplyIfComma = false;
            general.CustomContinuationStyleGapSuffixAddSpace = true;
            general.CustomContinuationStyleGapSuffixReplaceComma = false;
            general.CustomContinuationStyleGapPrefix = "__";
            general.CustomContinuationStyleGapPrefixAddSpace = true;
            general.FixContinuationStyleUncheckInsertsAllCaps = false;
            general.FixContinuationStyleUncheckInsertsItalic = false;
            general.FixContinuationStyleUncheckInsertsLowercase = false;
            general.FixContinuationStyleHideContinuationCandidatesWithoutName = false;
            general.FixContinuationStyleIgnoreLyrics = false;

            var reset = new SettingsResetViewModel
            {
                ResetRules = true,
            };
            var applyResetSelections = typeof(SettingsViewModel).GetMethod(
                "ApplyResetSelections",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(applyResetSelections);
            applyResetSelections!.Invoke(null, [reset]);

            var defaults = new SeGeneral();
            Assert.Equal(defaults.ContinuationPause, general.ContinuationPause);
            Assert.Equal(defaults.CustomContinuationStyleSuffix, general.CustomContinuationStyleSuffix);
            Assert.Equal(defaults.CustomContinuationStyleSuffixApplyIfComma, general.CustomContinuationStyleSuffixApplyIfComma);
            Assert.Equal(defaults.CustomContinuationStyleSuffixAddSpace, general.CustomContinuationStyleSuffixAddSpace);
            Assert.Equal(defaults.CustomContinuationStyleSuffixReplaceComma, general.CustomContinuationStyleSuffixReplaceComma);
            Assert.Equal(defaults.CustomContinuationStylePrefix, general.CustomContinuationStylePrefix);
            Assert.Equal(defaults.CustomContinuationStylePrefixAddSpace, general.CustomContinuationStylePrefixAddSpace);
            Assert.Equal(defaults.CustomContinuationStyleUseDifferentStyleGap, general.CustomContinuationStyleUseDifferentStyleGap);
            Assert.Equal(defaults.CustomContinuationStyleGapSuffix, general.CustomContinuationStyleGapSuffix);
            Assert.Equal(defaults.CustomContinuationStyleGapSuffixApplyIfComma, general.CustomContinuationStyleGapSuffixApplyIfComma);
            Assert.Equal(defaults.CustomContinuationStyleGapSuffixAddSpace, general.CustomContinuationStyleGapSuffixAddSpace);
            Assert.Equal(defaults.CustomContinuationStyleGapSuffixReplaceComma, general.CustomContinuationStyleGapSuffixReplaceComma);
            Assert.Equal(defaults.CustomContinuationStyleGapPrefix, general.CustomContinuationStyleGapPrefix);
            Assert.Equal(defaults.CustomContinuationStyleGapPrefixAddSpace, general.CustomContinuationStyleGapPrefixAddSpace);
            Assert.Equal(defaults.FixContinuationStyleUncheckInsertsAllCaps, general.FixContinuationStyleUncheckInsertsAllCaps);
            Assert.Equal(defaults.FixContinuationStyleUncheckInsertsItalic, general.FixContinuationStyleUncheckInsertsItalic);
            Assert.Equal(defaults.FixContinuationStyleUncheckInsertsLowercase, general.FixContinuationStyleUncheckInsertsLowercase);
            Assert.Equal(defaults.FixContinuationStyleHideContinuationCandidatesWithoutName, general.FixContinuationStyleHideContinuationCandidatesWithoutName);
            Assert.Equal(defaults.FixContinuationStyleIgnoreLyrics, general.FixContinuationStyleIgnoreLyrics);
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    [Theory]
    [InlineData("Hungarian.json", "A folytatási stílus javításának beállításai", "Megjegyzés: Az egyéni folytatási stílus a profilok között meg van osztva.")]
    [InlineData("Portuguese.json", "Definições da fixação do estilo de continuação", "Nota: o estilo de continuação personalizado é partilhado entre perfis.")]
    public void ContinuationSettingsLocalization_UsesTranslatedStrings(string fileName, string expectedSettingsLabel, string expectedNote)
    {
        var repoRoot = FindRepoRoot();
        var json = File.ReadAllText(Path.Combine(repoRoot, "src/ui/Assets/Languages", fileName));
        using var document = JsonDocument.Parse(json);

        var settings = document.RootElement
            .GetProperty("options")
            .GetProperty("settings");

        Assert.Equal(expectedSettingsLabel, settings.GetProperty("fixContinuationStyleSettings").GetString());
        Assert.Equal(expectedNote, settings.GetProperty("customContinuationStyleNote").GetString());
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "SubtitleEdit.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
