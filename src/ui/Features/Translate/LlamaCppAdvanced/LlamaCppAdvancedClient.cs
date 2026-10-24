using Nikse.SubtitleEdit.Logic.Config;
namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;

public class LlamaCppAdvancedClient : UiLogic.Translate.Llm.OpenAiLlmClient
{
    public LlamaCppAdvancedClient() : base(Se.Settings.AutoTranslate.LlamaCppAdvanced) { }
    internal static string BuildRequestJson(string systemPrompt, string userContent, string? responseFormatJson, string? model, int defaultMaxTokens, double temperatureBump = 0, bool disableThinking = false) =>
        UiLogic.Translate.Llm.OpenAiLlmClient.BuildRequestJson(systemPrompt, userContent, responseFormatJson, model, defaultMaxTokens, temperatureBump, disableThinking, Se.Settings.AutoTranslate.LlamaCppAdvanced);
}
