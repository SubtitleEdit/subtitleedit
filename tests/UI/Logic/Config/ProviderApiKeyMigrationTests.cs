using System.Text.Json;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

#pragma warning disable CS0618 // the legacy per-feature keys are what is being migrated
public class ProviderApiKeyMigrationTests
{
    [Fact]
    public void FirstNonEmptyLegacyKeyIsCopied()
    {
        var settings = new Se();
        settings.Ocr.MistralApiKey = "ocr-mistral";
        settings.Video.TextToSpeech.MistralApiKey = "tts-mistral";
        settings.Video.TextToSpeech.OpenRouterTtsApiKey = "tts-openrouter";
        settings.Tools.OpenRouterSttApiKey = "stt-openrouter";

        Se.MigrateProviderApiKeys(settings);

        // Translate comes first, but it is empty here - so OCR / TTS win.
        Assert.Equal("ocr-mistral", settings.Providers.MistralApiKey);
        Assert.Equal("tts-openrouter", settings.Providers.OpenRouterApiKey);
        Assert.Equal(SeProviders.CurrentMigrationVersion, settings.Providers.MigrationVersion);
    }

    [Fact]
    public void TranslateKeyWinsAndIsTrimmed()
    {
        var settings = new Se();
        settings.AutoTranslate.MistralApiKey = "  translate-mistral ";
        settings.Ocr.MistralApiKey = "ocr-mistral";
        settings.AutoTranslate.OpenRouterApiKey = "translate-openrouter";
        settings.Tools.OpenRouterSttApiKey = "stt-openrouter";

        Se.MigrateProviderApiKeys(settings);

        Assert.Equal("translate-mistral", settings.Providers.MistralApiKey);
        Assert.Equal("translate-openrouter", settings.Providers.OpenRouterApiKey);
    }

    [Fact]
    public void LegacyKeysAreKeptForDowngrade()
    {
        var settings = new Se();
        settings.Ocr.MistralApiKey = "ocr-mistral";

        Se.MigrateProviderApiKeys(settings);

        Assert.Equal("ocr-mistral", settings.Ocr.MistralApiKey);
    }

    [Fact]
    public void ExistingSharedKeyIsNotOverwritten()
    {
        var settings = new Se();
        settings.Providers.MistralApiKey = "shared";
        settings.AutoTranslate.MistralApiKey = "legacy";

        Se.MigrateProviderApiKeys(settings);

        Assert.Equal("shared", settings.Providers.MistralApiKey);
    }

    [Fact]
    public void MigrationRunsOnlyOnce()
    {
        var settings = new Se();
        Se.MigrateProviderApiKeys(settings);
        Assert.Equal(string.Empty, settings.Providers.MistralApiKey);

        // A user who later clears the shared key must not get an old per-feature key back.
        settings.AutoTranslate.MistralApiKey = "legacy";
        Se.MigrateProviderApiKeys(settings);

        Assert.Equal(string.Empty, settings.Providers.MistralApiKey);
    }

    [Fact]
    public void MissingProvidersSectionIsCreated()
    {
        var settings = new Se { Providers = null! };
        settings.Tools.OpenRouterSttApiKey = "stt-openrouter";

        Se.MigrateProviderApiKeys(settings);

        Assert.NotNull(settings.Providers);
        Assert.Equal("stt-openrouter", settings.Providers.OpenRouterApiKey);
    }

    [Fact]
    public void OldSettingsJsonWithoutProvidersSectionMigrates()
    {
        // A settings file written before the Providers section existed.
        const string json = """
        {
          "AutoTranslate": { "MistralApiKey": "translate-mistral" },
          "Video": { "TextToSpeech": { "OpenRouterTtsApiKey": "tts-openrouter" } }
        }
        """;
        var settings = JsonSerializer.Deserialize(json, SeJsonContext.Default.Se)!;
        Assert.Null(settings.Providers.MigrationVersion);

        Se.MigrateProviderApiKeys(settings);

        Assert.Equal("translate-mistral", settings.Providers.MistralApiKey);
        Assert.Equal("tts-openrouter", settings.Providers.OpenRouterApiKey);

        // Round-trips through the source-generated context.
        var roundTripped = JsonSerializer.Deserialize(
            JsonSerializer.Serialize(settings, SeJsonContext.Default.Se), SeJsonContext.Default.Se)!;
        Assert.Equal("translate-mistral", roundTripped.Providers.MistralApiKey);
        Assert.Equal(SeProviders.CurrentMigrationVersion, roundTripped.Providers.MigrationVersion);
    }
}
#pragma warning restore CS0618
