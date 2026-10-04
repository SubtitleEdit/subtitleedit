namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// Credentials for AI/cloud providers that more than one feature talks to. A provider account
/// has one API key no matter whether it is used for translation, OCR, speech-to-text or
/// text-to-speech, so the key lives here once instead of per feature. URLs and models stay with
/// each feature, since those really do differ per use.
/// </summary>
public class SeProviders
{
    internal const int CurrentMigrationVersion = 1;

    /// <summary>Mistral (La Plateforme) API key - Translate, OCR and text-to-speech.</summary>
    public string MistralApiKey { get; set; } = string.Empty;

    /// <summary>OpenRouter API key - Translate, speech-to-text and text-to-speech.</summary>
    public string OpenRouterApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Null until <see cref="Se.MigrateProviderApiKeys"/> has copied the old per-feature keys
    /// in. Deliberately not defaulted to the current version: an old settings file has no
    /// "Providers" section at all, and a defaulted version would skip its migration.
    /// </summary>
    public int? MigrationVersion { get; set; }
}
