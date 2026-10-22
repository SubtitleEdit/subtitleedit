using System.Text.Json;
using Nikse.SubtitleEdit.UiLogic.Translate.Llm;

namespace Nikse.SubtitleEdit.UiLogic.Translate;

public enum TranslationEngineKind { Traditional, Llm }
public enum LlmLocation { Local, Cloud }
public enum LlmApiProtocol { OpenAi, Anthropic }
public enum LlmTranslationMode { Advanced, LineByLine }

/// <summary>A named, self-contained connection and translation policy. References use Id, never Name.</summary>
public sealed class TranslationProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Default";
    public TranslationEngineKind Kind { get; set; }
    public string EngineId { get; set; } = "GoogleTranslateV1";
    public LlmLocation Location { get; set; } = LlmLocation.Cloud;
    public LlmApiProtocol Protocol { get; set; }
    public string ApiUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "";
    public string LinePrompt { get; set; } = "Translate from {0} to {1}. Return only the translation, preserving formatting and line breaks.";
    public LlmTranslationMode Mode { get; set; } = LlmTranslationMode.Advanced;
    public AdvancedTranslationSettings Policy { get; set; } = new();
    public Dictionary<string, string> Parameters { get; set; } = new();
    public bool NeedsReview { get; set; }
    public TranslationProfile Snapshot() => JsonSerializer.Deserialize<TranslationProfile>(JsonSerializer.Serialize(this))!;
    public TranslationProfile Copy(string name)
    {
        var copy = Snapshot();
        copy.Id = Guid.NewGuid().ToString("N");
        copy.Name = name;
        return copy;
    }
    public override string ToString() => Name;
}
