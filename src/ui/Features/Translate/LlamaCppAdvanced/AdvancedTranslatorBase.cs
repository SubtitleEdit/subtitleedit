using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;

/// <summary>
/// Shared batch/context translation loop of the "advanced" local-LLM engines: sends numbered
/// batches with a rolling history of already-translated lines plus user-configured
/// synopsis/glossary/style, and requires a schema-constrained JSON reply so line alignment is
/// guaranteed. The Auto-translate loop calls <see cref="TranslateBatchAsync"/> directly; the
/// plain <see cref="Translate"/> path (used for "translate current line") is a context-free
/// batch of one. Subclasses only supply the endpoint and, where the server needs one, the
/// model name.
/// </summary>
public abstract class AdvancedTranslatorBase : IAutoTranslator, IDisposable
{
    private LlamaCppAdvancedClient? _client;

    public abstract string Name { get; }
    public abstract string Url { get; }
    public string Error { get; set; } = string.Empty;
    public int MaxCharacters => 1000;

    /// <summary>The chat-completions endpoint to call (read per request, after any server startup).</summary>
    protected abstract string GetApiUrl();

    /// <summary>The "model" request field; null for single-model servers like llama-server.</summary>
    protected virtual string? GetModel() => null;

    public void Initialize()
    {
        _client?.Dispose();
        _client = new LlamaCppAdvancedClient();
    }

    public List<TranslationPair> GetSupportedSourceLanguages()
    {
        return ChatGptTranslate.ListLanguages();
    }

    public List<TranslationPair> GetSupportedTargetLanguages()
    {
        return ChatGptTranslate.ListLanguages();
    }

    public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
    {
        var lines = new List<LlamaCppAdvancedProtocol.BatchLine> { new(1, text.Trim()) };
        var map = await TranslateLinesAsync(lines, new List<LlamaCppAdvancedProtocol.HistoryPair>(), sourceLanguageCode, targetLanguageCode, cancellationToken);
        return map.TryGetValue(1, out var translation) ? translation : string.Empty;
    }

    /// <summary>
    /// Translates the next batch starting at <paramref name="index"/>, writing results into the
    /// rows, and returns the number of rows translated (always at least 1, or throws). On an
    /// invalid reply the batch is retried once, then halved - a model that cannot manage a full
    /// batch often still manages a smaller one.
    /// </summary>
    public async Task<int> TranslateBatchAsync(ObservableCollection<TranslateRow> rows, int index, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
    {
        var settings = Se.Settings.AutoTranslate.LlamaCppAdvanced;
        var batchSize = Math.Clamp(settings.BatchSize, 1, 50);
        var count = Math.Min(batchSize, rows.Count - index);
        return await TranslateChunkAsync(rows, index, count, sourceLanguageCode, targetLanguageCode, cancellationToken);
    }

    private async Task<int> TranslateChunkAsync(ObservableCollection<TranslateRow> rows, int index, int count, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
    {
        var lines = new List<LlamaCppAdvancedProtocol.BatchLine>(count);
        for (var i = 0; i < count; i++)
        {
            lines.Add(new LlamaCppAdvancedProtocol.BatchLine(i + 1, rows[index + i].Text));
        }

        // Translation-tuned models (TranslateGemma) can echo history into a lone line's
        // translation, so the single-line case - including bisection retries - goes context-free.
        var history = count > 1 ? CollectHistory(rows, index) : new List<LlamaCppAdvancedProtocol.HistoryPair>();
        var map = await TranslateLinesAsync(lines, history, sourceLanguageCode, targetLanguageCode, cancellationToken);
        if (IsComplete(map, lines))
        {
            // Runs on the background translation loop; the rows are DataGrid-bound, so the
            // writes must happen on the UI thread.
            Dispatcher.UIThread.Invoke(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    var translation = map[i + 1];
                    rows[index + i].TranslatedText = translation.Length > 0 ? translation : rows[index + i].Text;
                }
            });

            return count;
        }

        if (count > 1)
        {
            // The model could not fill the full batch - translate the first half only; the outer
            // loop calls again for the rest (with the successful half now part of the history).
            return await TranslateChunkAsync(rows, index, count / 2, sourceLanguageCode, targetLanguageCode, cancellationToken);
        }

        throw new HttpRequestException("No usable translation in " + Name + " reply" +
                                       (string.IsNullOrEmpty(Error) ? string.Empty : ": " + Error));
    }

    private async Task<Dictionary<int, string>> TranslateLinesAsync(List<LlamaCppAdvancedProtocol.BatchLine> lines, List<LlamaCppAdvancedProtocol.HistoryPair> history, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
    {
        var client = _client ?? throw new InvalidOperationException("Initialize() not called");
        var settings = Se.Settings.AutoTranslate.LlamaCppAdvanced;
        var url = GetApiUrl();

        // The "codes" this engine receives are English language names (ListLanguages puts the
        // name in TranslationPair.Code), which is what the prompt expects.
        var systemPrompt = LlamaCppAdvancedProtocol.BuildSystemPrompt(sourceLanguageCode, targetLanguageCode, settings);
        var userContent = LlamaCppAdvancedProtocol.BuildUserContent(history, lines);
        var responseFormat = LlamaCppAdvancedProtocol.BuildResponseFormatJson(lines);

        var map = new Dictionary<int, string>();
        for (var attempt = 0; attempt < 2 && !cancellationToken.IsCancellationRequested; attempt++)
        {
            try
            {
                var reply = await client.ChatAsync(url, systemPrompt, userContent, responseFormat, cancellationToken, GetModel());
                map = LlamaCppAdvancedProtocol.ParseTranslations(reply);
                if (IsComplete(map, lines))
                {
                    return map;
                }

                Error = reply;
            }
            catch (HttpRequestException)
            {
                Error = client.Error;
                if (attempt == 1)
                {
                    throw;
                }
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        return map;
    }

    /// <summary>
    /// The rolling context: the last N already-translated rows immediately before the batch.
    /// </summary>
    private static List<LlamaCppAdvancedProtocol.HistoryPair> CollectHistory(ObservableCollection<TranslateRow> rows, int index)
    {
        var settings = Se.Settings.AutoTranslate.LlamaCppAdvanced;
        var maxPairs = Math.Clamp(settings.HistoryPairs, 0, 50);
        var history = new List<LlamaCppAdvancedProtocol.HistoryPair>(maxPairs);
        for (var i = index - 1; i >= 0 && history.Count < maxPairs; i--)
        {
            if (!string.IsNullOrEmpty(rows[i].TranslatedText))
            {
                history.Add(new LlamaCppAdvancedProtocol.HistoryPair(rows[i].Text, rows[i].TranslatedText));
            }
        }

        history.Reverse();
        return history;
    }

    /// <summary>Every requested line number must be present, and non-empty for non-empty sources.</summary>
    private static bool IsComplete(Dictionary<int, string> map, List<LlamaCppAdvancedProtocol.BatchLine> lines)
    {
        foreach (var line in lines)
        {
            if (!map.TryGetValue(line.Number, out var translation))
            {
                return false;
            }

            if (translation.Length == 0 && !string.IsNullOrWhiteSpace(line.Text) && lines.Count > 1)
            {
                return false;
            }
        }

        return true;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
