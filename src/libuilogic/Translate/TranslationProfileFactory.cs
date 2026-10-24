using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate.Llm;

namespace Nikse.SubtitleEdit.UiLogic.Translate;

public record TranslationParameter(string Property, string LegacyProperty, string Label, bool Secret = false);
public record TraditionalEngineDefinition(string Id, string Name, Func<ToolsSettings, IAutoTranslator> Create, params TranslationParameter[] Fields)
{
    public override string ToString() => Name;
}

public static class TranslationProfileFactory
{
    public static IReadOnlyList<TraditionalEngineDefinition> TraditionalEngines { get; } = new TraditionalEngineDefinition[]
    {
        new("GoogleTranslateV1", "Google Translate V1", _ => new GoogleTranslateV1()),
        new("GoogleTranslateV2", "Google Translate V2", s => new GoogleTranslateV2(s), new TranslationParameter("GoogleApiV2Key", "GoogleApiV2Key", "API key", true)),
        new("MicrosoftTranslator", "Microsoft Translator", s => new MicrosoftTranslator(s), new TranslationParameter("MicrosoftTranslatorApiKey", "MicrosoftTranslatorApiKey", "API key", true), new TranslationParameter("MicrosoftTranslatorTokenEndpoint", "MicrosoftTranslatorTokenEndpoint", "Token endpoint"), new TranslationParameter("MicrosoftTranslatorCategory", "MicrosoftTranslatorCategory", "Category")),
        new("DeepLTranslate", "DeepL", s => new DeepLTranslate(s), new TranslationParameter("AutoTranslateDeepLUrl", "DeepLUrl", "API URL"), new TranslationParameter("AutoTranslateDeepLApiKey", "DeepLApiKey", "API key", true), new TranslationParameter("AutoTranslateDeepLFormality", "DeepLFormality", "Formality")),
        new("LibreTranslate", "LibreTranslate", s => new LibreTranslate(s), new TranslationParameter("AutoTranslateLibreUrl", "LibreTranslateUrl", "API URL"), new TranslationParameter("AutoTranslateLibreApiKey", "LibreTranslateApiKey", "API key", true)),
        new("MyMemoryApi", "MyMemory", s => new MyMemoryApi(s), new TranslationParameter("AutoTranslateMyMemoryApiKey", "MyMemoryApiKey", "API key", true)),
        new("LaraTranslate", "Lara", s => new LaraTranslate(s), new TranslationParameter("LaraUrl", "LaraUrl", "API URL"), new TranslationParameter("LaraApiId", "LaraApiId", "API ID"), new TranslationParameter("LaraApiSecret", "LaraApiSecret", "API secret", true)),
        new("PapagoTranslate", "Papago", s => new PapagoTranslate(s), new TranslationParameter("AutoTranslatePapagoApiKeyId", "PapagoApiKeyId", "Client ID"), new TranslationParameter("AutoTranslatePapagoApiKey", "PapagoApiKey", "API key", true)),
        new("BaiduTranslate", "Baidu", s => new BaiduTranslate(s), new TranslationParameter("BaiduUrl", "BaiduUrl", "API URL"), new TranslationParameter("BaiduApiKey", "BaiduApiKey", "App ID | API secret", true)),
        new("NoLanguageLeftBehindApi", "NLLB API", s => new NoLanguageLeftBehindApi(s), new TranslationParameter("AutoTranslateNllbApiUrl", "NllbApiUrl", "API URL")),
        new("NoLanguageLeftBehindServe", "NLLB Serve", s => new NoLanguageLeftBehindServe(s), new TranslationParameter("AutoTranslateNllbServeUrl", "NllbServeUrl", "API URL")),
        new("CrispAsrMadladTranslate", "CrispASR MADLAD", s => new CrispAsrMadladTranslate(s), new TranslationParameter("AutoTranslateCrispAsrExe", "CrispAsrExe", "Executable"), new TranslationParameter("AutoTranslateCrispAsrModel", "CrispAsrModel", "Model file")),
    };

    public static IAutoTranslator Create(TranslationProfile profile, Action<Action>? applyResults = null)
    {
        var snapshot = profile.Snapshot();
        Validate(snapshot);
        if (snapshot.Kind == TranslationEngineKind.Llm)
        {
            if (snapshot.NeedsReview) throw new InvalidOperationException("Review this migrated connection's API URL and choose a chat-compatible model before translating.");
            if (snapshot.Mode == LlmTranslationMode.LineByLine) return new ProfileLlmTranslator(snapshot);
            return new ProfileAdvancedLlmTranslator(snapshot) { ApplyResults = applyResults ?? (action => action()) };
        }
        var engine = TraditionalEngines.FirstOrDefault(e => e.Id == snapshot.EngineId)
            ?? throw new InvalidOperationException("Unknown translation engine: " + snapshot.EngineId);
        var settings = new ToolsSettings();
        foreach (var field in engine.Fields)
            if (snapshot.Parameters.TryGetValue(field.Property, out var value))
                typeof(ToolsSettings).GetProperty(field.Property)!.SetValue(settings, value);
        return engine.Create(settings);
    }

    public static void ApplyModelDefaults(AdvancedTranslationSettings settings, LlamaCppModel? model)
    {
        if (model == null) return;
        if (settings.Temperature < 0) settings.Temperature = model.Temperature;
        if (settings.TopP < 0) settings.TopP = model.TopP;
        if (settings.TopK < 0) settings.TopK = model.TopK;
        if (settings.RepeatPenalty < 0) settings.RepeatPenalty = model.RepeatPenalty;
    }

    public static void Validate(TranslationProfile profile)
    {
        if (profile.Kind == TranslationEngineKind.Llm)
        {
            if (profile.NeedsReview) throw new InvalidOperationException("Review the migrated connection URL and choose a chat-compatible model before translating.");
            if (!Uri.TryCreate(profile.ApiUrl, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
                throw new InvalidOperationException("Enter a valid HTTP or HTTPS API URL in the translation profile.");
            return;
        }
        var engine = TraditionalEngines.FirstOrDefault(e => e.Id == profile.EngineId)
            ?? throw new InvalidOperationException("Unknown translation engine: " + profile.EngineId);
        foreach (var field in engine.Fields)
        {
            var value = profile.Parameters.GetValueOrDefault(field.Property) ?? "";
            if (field.Secret && profile.EngineId is not ("LibreTranslate" or "MyMemoryApi") && string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(engine.Name + " requires " + field.Label + ". Edit the translation profile.");
            if (field.Label is "API URL" or "Token endpoint" &&
                (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https")))
                throw new InvalidOperationException(engine.Name + " requires a valid HTTP or HTTPS " + field.Label + ".");
        }
    }

    public static string UniqueName(IEnumerable<TranslationProfile> profiles, string name)
    {
        var used = profiles.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var result = name;
        for (var i = 2; used.Contains(result); i++) result = name + " (" + i + ")";
        return result;
    }
}
