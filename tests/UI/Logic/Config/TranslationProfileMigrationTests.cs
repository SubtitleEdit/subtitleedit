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
        settings.Tools.BatchConvert.AutoTranslateEngine = "";
        TranslationProfileMigration.EnsureMigrated(settings);
        Assert.Single(settings.AutoTranslate.Profiles);
        var id = settings.AutoTranslate.Profiles[0].Id;
        TranslationProfileMigration.EnsureMigrated(settings);
        Assert.Single(settings.AutoTranslate.Profiles);
        Assert.Equal(id, settings.AutoTranslate.SelectedProfileId);
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
        Assert.Equal("secret:fx", profile.Parameters["AutoTranslateDeepLApiKey"]);
        Assert.IsType<DeepLTranslate>(TranslationProfileFactory.Create(profile));
    }
}
