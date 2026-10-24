namespace Nikse.SubtitleEdit.UiLogic.Translate.Llm;

public interface ILlmClient : IDisposable
{
    string Error { get; }
    bool ReplyFromReasoning { get; }
    bool LastRequestDisabledThinking { get; }
    Task<string> ChatAsync(string url, string systemPrompt, string userContent, string? responseFormatJson,
        CancellationToken cancellationToken, string? model = null, int defaultMaxTokens = -1, double temperatureBump = 0);
}
