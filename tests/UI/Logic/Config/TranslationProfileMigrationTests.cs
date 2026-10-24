using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace UITests.Logic.Config;

public class TranslationProfileMigrationTests
{
    [Fact]
    public void FreshSettingsCreateOneDefaultAndDoNotDuplicateOnReload()
    {
        var settings = new Se();
        TranslationProfileMigration.EnsureMigrated(settings);
        Assert.Single(settings.AutoTranslate.Profiles);
        Assert.Equal("Default", settings.AutoTranslate.Profiles[0].Name);
        var id = settings.AutoTranslate.Profiles[0].Id;
        TranslationProfileMigration.EnsureMigrated(settings);
        Assert.Single(settings.AutoTranslate.Profiles);
        Assert.Equal(id, settings.AutoTranslate.SelectedProfileId);
    }

    [Fact]
    public void PreviouslyMigratedProfileKeepsNameAndIdentity()
    {
        var profile = new TranslationProfile { Name = "Google Translate" };
        profile.Parameters["TranslateStrategy"] = "Default";
        var settings = new Se();
        settings.AutoTranslate.ProfileMigrationVersion = 1;
        settings.AutoTranslate.Profiles.Add(profile);
        settings.AutoTranslate.SelectedProfileId = profile.Id;
        TranslationProfileMigration.EnsureMigrated(settings);
        Assert.Equal("Google Translate", profile.Name);
        Assert.Equal(profile.Id, settings.AutoTranslate.SelectedProfileId);
        Assert.Equal(2, settings.AutoTranslate.ProfileMigrationVersion);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ExistingProfilesKeepCustomNamesAndDoNotCreateDuplicates(int version)
    {
        var settings = new Se();
        var custom = new TranslationProfile { Name = "My Google account" };
        var generated = new TranslationProfile { Name = "Google Translate" };
        settings.AutoTranslate.ProfileMigrationVersion = 1;
        settings.AutoTranslate.ProfileMigrationVersion = version;
        settings.AutoTranslate.Profiles.AddRange(new[] { custom, generated, new TranslationProfile() });
        TranslationProfileMigration.EnsureMigrated(settings);
        Assert.Equal("My Google account", custom.Name);
        Assert.Equal("Google Translate", generated.Name);
        Assert.Single(settings.AutoTranslate.Profiles, p => p.Name == "Default");
    }

    [Fact]
    public void EveryTraditionalEngineMigratesWithItsOriginalDisplayName()
    {
        foreach (var engine in TranslationProfileFactory.TraditionalEngines)
        {
            var name = engine.Create(new()).Name;
            var settings = new Se();
            settings.AutoTranslate.AutoTranslateLastName = name;
            TranslationProfileMigration.EnsureMigrated(settings);
            var selected = Assert.Single(settings.AutoTranslate.Profiles, p => p.Id == settings.AutoTranslate.SelectedProfileId);
            Assert.Equal(name, selected.Name);
            Assert.Equal(engine.Id, selected.EngineId);
        }
    }

    [Fact]
    public void ConfiguredLegacyAccountsAndFallbackKeepEngineNames()
    {
        var settings = new Se();
        settings.AutoTranslate.DeepLApiKey = "secret:fx";
        TranslationProfileMigration.EnsureMigrated(settings);
        Assert.Contains(settings.AutoTranslate.Profiles, p => p.Name == new DeepLTranslate().Name);
        Assert.Contains(settings.AutoTranslate.Profiles, p => p.Name == new GoogleTranslateV1().Name);
        Assert.DoesNotContain(settings.AutoTranslate.Profiles, p => p.Name == "Default");
    }

    [Fact]
    public void MigrationAddsSuffixForExistingNamesAndPreservesSelectionById()
    {
        var name = new GoogleTranslateV1().Name;
        var existing = new TranslationProfile { Name = name };
        var settings = new Se();
        settings.AutoTranslate.Profiles.Add(existing);
        settings.AutoTranslate.AutoTranslateLastName = name;
        settings.Tools.BatchConvert.AutoTranslateEngine = name;
        TranslationProfileMigration.EnsureMigrated(settings);
        var migrated = Assert.Single(settings.AutoTranslate.Profiles, p => p.Id == settings.AutoTranslate.SelectedProfileId);
        Assert.Equal(name + " (2)", migrated.Name);
        Assert.NotEqual(existing.Id, migrated.Id);
        Assert.Equal(migrated.Id, settings.AutoTranslate.BatchProfileId);
        Assert.Equal(name, existing.Name);
        TranslationProfileMigration.EnsureMigrated(settings);
        Assert.Equal(2, settings.AutoTranslate.Profiles.Count);
    }

    [Fact]
    public void MigratedCloudAccountGetsIndependentKeyAndAdvancedPolicy()
    {
        var settings = new Se();
        settings.AutoTranslate.AutoTranslateLastName = new OpenRouterTranslate().Name;
        settings.Providers.OpenRouterApiKey = "shared-key";
        settings.AutoTranslate.LlamaCppAdvanced.Glossary = "Alice = 艾丽丝";
        TranslationProfileMigration.EnsureMigrated(settings);
        var profile = Assert.Single(settings.AutoTranslate.Profiles, p => p.Id == settings.AutoTranslate.SelectedProfileId);
        Assert.Equal(new OpenRouterTranslate().Name, profile.Name);
        Assert.Equal("shared-key", profile.ApiKey);
        Assert.Equal(LlmTranslationMode.Advanced, profile.Mode);
        profile.ApiKey = "other-account";
        profile.Policy.Glossary = "another glossary";
        Assert.Equal("shared-key", settings.Providers.OpenRouterApiKey);
        Assert.Equal("Alice = 艾丽丝", settings.AutoTranslate.LlamaCppAdvanced.Glossary);
    }

    [Fact]
    public void CustomCompletionEndpointIsPreservedForReview()
    {
        var settings = new Se();
        settings.AutoTranslate.AutoTranslateLastName = new LlamaCppTranslate().Name;
        settings.AutoTranslate.LlamaCppApiUrl = "http://gpu-box/custom/completion";
        TranslationProfileMigration.EnsureMigrated(settings);
        var profile = settings.AutoTranslate.Profiles.Single(p => p.Id == settings.AutoTranslate.SelectedProfileId);
        Assert.Equal("http://gpu-box/custom/completion", profile.ApiUrl);
        Assert.True(profile.NeedsReview);
    }

    [Fact]
    public void TraditionalCredentialsAreStoredPerProfile()
    {
        var settings = new Se();
        settings.AutoTranslate.DeepLApiKey = "secret:fx";
        TranslationProfileMigration.EnsureMigrated(settings);
        var profile = settings.AutoTranslate.Profiles.Single(p => p.EngineId == "DeepLTranslate");
        Assert.Equal(new DeepLTranslate().Name, profile.Name);
        Assert.Equal("secret:fx", profile.Parameters["AutoTranslateDeepLApiKey"]);
        Assert.IsType<DeepLTranslate>(TranslationProfileFactory.Create(profile));
    }
}
