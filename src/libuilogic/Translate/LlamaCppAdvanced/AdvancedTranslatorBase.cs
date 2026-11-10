using System.Collections.ObjectModel;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

/// <summary>
/// Shared batch/context translation loop of the "advanced" local-LLM engines: sends numbered
/// batches with a rolling history of already-translated lines plus user-configured
/// synopsis/glossary/style, and requires a schema-constrained JSON reply so line alignment is
/// guaranteed. The Auto-translate loop and batch convert both call <see cref="TranslateBatchAsync"/>
/// (via <see cref="IBatchContextTranslator"/>); the plain <see cref="Translate"/> path (used for
/// "translate current line") is a context-free batch of one. Subclasses only supply the endpoint
/// and, where the server needs one, the model name; the host sets <see cref="Settings"/>.
/// </summary>
public abstract class AdvancedTranslatorBase : IAutoTranslator, IBatchContextTranslator, IDisposable
{
    private LlamaCppAdvancedClient? _client;

    public abstract string Name { get; }
    public abstract string Url { get; }
    public string Error { get; set; } = string.Empty;
    public int MaxCharacters => 1000;

    /// <summary>
    /// Batch/context options and sampling overrides, read per batch (the client takes a copy of
    /// the reference in <see cref="Initialize"/>). A headless host (seconv) sets its own; the UI
    /// engines override this to read the persisted settings live, so a settings reset or import
    /// that replaces the settings object is picked up by the next translation run.
    /// </summary>
    public virtual SeLlamaCppAdvanced Settings { get; set; } = new();

    /// <summary>The chat-completions endpoint to call (read per request, after any server startup).</summary>
    protected abstract string GetApiUrl();

    /// <summary>The "model" request field; null for single-model servers like llama-server.</summary>
    protected virtual string? GetModel() => null;

    public void Initialize()
    {
        _client?.Dispose();
        _client = new LlamaCppAdvancedClient(Settings);
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
        var stripped = StrippedLine.Strip(text.Trim());
        var lines = new List<LlamaCppAdvancedProtocol.BatchLine> { new(1, stripped.Text) };
        var map = await TranslateLinesAsync(lines, new List<LlamaCppAdvancedProtocol.HistoryPair>(), sourceLanguageCode, targetLanguageCode, cancellationToken);
        // An echo that survived the warmer retry in TranslateLinesAsync is accepted: the line may
        // legitimately read the same in both languages, and failing it would abort the translation.
        return map.TryGetValue(1, out var translation) && translation.Length > 0
            ? stripped.Restore(translation)
            : string.Empty;
    }

    /// <summary>
    /// Translates the next batch starting at <paramref name="index"/>, writing results into the
    /// rows, and returns the number of rows translated (always at least 1, or throws). On an
    /// invalid reply the batch is retried once, then halved - a model that cannot manage a full
    /// batch often still manages a smaller one.
    /// </summary>
    /// <param name="applyRowUpdate">Marshaller invoked around every write to the rows, or null -
    /// the Auto-translate window passes a UI-thread invoke for its grid-bound rows; batch convert
    /// and seconv pass null and the rows are written directly.</param>
    public async Task<int> TranslateBatchAsync(ObservableCollection<TranslateRow> rows, int index, string sourceLanguageCode, string targetLanguageCode, Action<Action>? applyRowUpdate, CancellationToken cancellationToken)
    {
        applyRowUpdate ??= action => action();
        var batchSize = Math.Clamp(Settings.BatchSize, 1, 50);
        if (MergeAndSplitHelper.IsKeptUntranslated(rows[index].Text))
        {
            var row = rows[index];
            applyRowUpdate(() => row.TranslatedText = row.Text);
            return 1;
        }

        // Stop the batch before the next music line kept in the source language (#9969).
        var count = 1;
        var maxCount = Math.Min(batchSize, rows.Count - index);
        while (count < maxCount && !MergeAndSplitHelper.IsKeptUntranslated(rows[index + count].Text))
        {
            count++;
        }

        return await TranslateChunkAsync(rows, index, count, sourceLanguageCode, targetLanguageCode, applyRowUpdate, cancellationToken);
    }

    private async Task<int> TranslateChunkAsync(ObservableCollection<TranslateRow> rows, int index, int count, string sourceLanguageCode, string targetLanguageCode, Action<Action> applyRowUpdate, CancellationToken cancellationToken)
    {
        // This path deliberately skips MergeAndSplitHelper (its merge/split heuristics would
        // break the alignment the reply schema guarantees), and with it the Formatting pass that
        // takes ASSA override blocks off every other engine's input. Strip them here instead:
        // they cost a lot of tokens, and small models "normalize" them into something that no
        // longer matches the source (#13927). A pure override/drawing line strips to empty, and
        // IsComplete then accepts the model's empty answer for it rather than failing the batch.
        var lines = new List<LlamaCppAdvancedProtocol.BatchLine>(count);
        var stripped = new StrippedLine[count];
        for (var i = 0; i < count; i++)
        {
            stripped[i] = StrippedLine.Strip(rows[index + i].Text);
            lines.Add(new LlamaCppAdvancedProtocol.BatchLine(i + 1, stripped[i].Text));
        }

        // Translation-tuned models (TranslateGemma) can echo history into a lone line's
        // translation, so the single-line case - including bisection retries - goes context-free.
        var history = count > 1 ? CollectHistory(rows, index) : new List<LlamaCppAdvancedProtocol.HistoryPair>();
        var map = await TranslateLinesAsync(lines, history, sourceLanguageCode, targetLanguageCode, cancellationToken);

        // An echoed line in a batch is bisected down to that line; a single line that still comes
        // back unchanged after the warmer retry is accepted - it may legitimately read the same in
        // both languages, and throwing would abort the whole translation.
        // Same for a reply where rows moved up by one and the freed row repeats an earlier
        // translation (#16555) or holds a placeholder like "..." (#16585); a lone line has no
        // neighbour to shift into.
        if (IsComplete(map, lines) && (count == 1 || (FindUntranslatedEcho(map, lines, sourceLanguageCode, targetLanguageCode) < 0 &&
                                                      FindDuplicatedTranslation(map, lines, history) < 0 &&
                                                      FindPlaceholderTranslation(map, lines) < 0)))
        {
            applyRowUpdate(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    var translation = map[i + 1];
                    rows[index + i].TranslatedText = translation.Length > 0
                        ? stripped[i].Restore(translation)
                        : rows[index + i].Text;
                }
            });

            return count;
        }

        if (count > 1)
        {
            // The model could not fill the full batch - translate the first half only; the outer
            // loop calls again for the rest (with the successful half now part of the history).
            return await TranslateChunkAsync(rows, index, count / 2, sourceLanguageCode, targetLanguageCode, applyRowUpdate, cancellationToken);
        }

        throw new HttpRequestException("No usable translation in " + Name + " reply" +
                                       (string.IsNullOrEmpty(Error) ? string.Empty : ": " + Error));
    }

    private async Task<Dictionary<int, string>> TranslateLinesAsync(List<LlamaCppAdvancedProtocol.BatchLine> lines, List<LlamaCppAdvancedProtocol.HistoryPair> history, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
    {
        var client = _client ?? throw new InvalidOperationException("Initialize() not called");
        var url = GetApiUrl();

        // The "codes" this engine receives are English language names (ListLanguages puts the
        // name in TranslationPair.Code), which is what the prompt expects.
        var systemPrompt = LlamaCppAdvancedProtocol.BuildSystemPrompt(sourceLanguageCode, targetLanguageCode, Settings);
        var userContent = LlamaCppAdvancedProtocol.BuildUserContent(history, lines);
        var responseFormat = LlamaCppAdvancedProtocol.BuildResponseFormatJson(lines);

        // Generous output budget for the batch (a translation is roughly source-sized; the JSON
        // wrapper adds a little per line). Only a fallback: the user's MaxTokens setting wins in
        // the client. Without any cap a grammar-cornered model generates until the server context
        // fills (#13830) - with it, the runaway becomes an incomplete reply that the normal
        // retry/bisection path handles.
        var defaultMaxTokens = 200;
        foreach (var line in lines)
        {
            defaultMaxTokens += 32 + 2 * line.Text.Length;
        }

        var map = new Dictionary<int, string>();
        Dictionary<int, string>? echoedMap = null;
        var echoed = false;
        for (var attempt = 0; attempt < 2 && !cancellationToken.IsCancellationRequested; attempt++)
        {
            try
            {
                // After an untranslated echo the retry runs a little warmer: at the same low
                // temperature the same request tends to echo again.
                var temperatureBump = echoed ? EchoRetryTemperatureBump : 0;
                var reply = await client.ChatAsync(url, systemPrompt, userContent, responseFormat, cancellationToken, GetModel(), defaultMaxTokens, temperatureBump);
                map = LlamaCppAdvancedProtocol.ParseTranslations(reply);
                if (IsComplete(map, lines))
                {
                    var echoedLine = FindUntranslatedEcho(map, lines, sourceLanguageCode, targetLanguageCode);
                    if (echoedLine < 0)
                    {
                        // The model merged two rows and padded the end with a copy of an earlier
                        // translation, so every row in between holds its neighbour's text (#16555).
                        // Retried here, then the caller bisects the batch.
                        // The same merge can also leave a placeholder like "..." in the freed row (#16585).
                        var duplicatedLine = FindDuplicatedTranslation(map, lines, history);
                        var placeholderLine = duplicatedLine < 0 ? FindPlaceholderTranslation(map, lines) : -1;
                        if ((duplicatedLine < 0 && placeholderLine < 0) || lines.Count == 1)
                        {
                            return map;
                        }

                        Error = duplicatedLine >= 0
                            ? "line " + duplicatedLine + " repeats an earlier translation (lines shifted): " + map[duplicatedLine]
                            : "line " + placeholderLine + " has no text, only \"" + map[placeholderLine] + "\" (lines shifted)";
                        continue;
                    }

                    // The model handed a line back in the source language (TranslateGemma 12B does
                    // this). Retried here, then the caller bisects the batch (a lone line is accepted).
                    echoed = true;
                    echoedMap = map;
                    Error = "line " + echoedLine + " came back untranslated (" + sourceLanguageCode + " source instead of " + targetLanguageCode + "): " + map[echoedLine];
                    continue;
                }

                Error = DescribeUnusableReply(reply, client.ReplyFromReasoning, client.LastRequestDisabledThinking);
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

        // A complete (if echoed) reply beats an unusable retry - the caller decides about echoes.
        return echoedMap != null && !IsComplete(map, lines) ? echoedMap : map;
    }

    /// <summary>
    /// What goes after "No usable translation in ... reply": the reply itself, or why there was
    /// none - an empty reply used to leave the message bare, which made a server that streams
    /// the answer somewhere else impossible to tell from a model that answered badly (#15009).
    /// </summary>
    internal static string DescribeUnusableReply(string reply, bool fromReasoning, bool requestDisabledThinking = true)
    {
        if (string.IsNullOrWhiteSpace(reply))
        {
            return "the server returned an empty reply (no \"content\" and no \"reasoning_content\")";
        }

        if (!fromReasoning)
        {
            return reply;
        }

        // Only claim the model ignored "enable_thinking" off when this request actually sent it -
        // the first reasoning-only reply is what switches it on for the following requests.
        return requestDisabledThinking
            ? "the server left \"content\" empty and answered in \"reasoning_content\" only - the model looks to be in thinking mode and kept thinking with \"enable_thinking\" off, turn thinking off on the server (e.g. \"--reasoning off\"): " + reply
            : "the server left \"content\" empty and answered in \"reasoning_content\" only - the model looks to be in thinking mode, the next requests ask it to skip thinking (\"enable_thinking\" off); if that does not help, turn thinking off on the server (e.g. \"--reasoning off\"): " + reply;
    }

    /// <summary>
    /// The rolling context: the last N already-translated rows immediately before the batch.
    /// </summary>
    private List<LlamaCppAdvancedProtocol.HistoryPair> CollectHistory(ObservableCollection<TranslateRow> rows, int index)
    {
        var maxPairs = Math.Clamp(Settings.HistoryPairs, 0, 50);
        var history = new List<LlamaCppAdvancedProtocol.HistoryPair>(maxPairs);
        for (var i = index - 1; i >= 0 && history.Count < maxPairs; i--)
        {
            if (string.IsNullOrEmpty(rows[i].TranslatedText))
            {
                continue;
            }

            // Same stripping as the batch itself: override blocks carry no context and would
            // otherwise fill the window twice per pair - once on each side (#13927).
            var source = StrippedLine.Strip(rows[i].Text).Text;
            var target = StrippedLine.Strip(rows[i].TranslatedText).Text;
            if (source.Length == 0 || target.Length == 0)
            {
                continue; // a pure override/drawing line is no use as an example pair
            }

            history.Add(new LlamaCppAdvancedProtocol.HistoryPair(source, target));
        }

        history.Reverse();
        return history;
    }

    private const double EchoRetryTemperatureBump = 0.3;

    /// <summary>
    /// The number of the first line whose translation is just its source text again (see
    /// <see cref="TranslationEchoGuard"/>), or -1 when there is none.
    /// </summary>
    internal static int FindUntranslatedEcho(Dictionary<int, string> map, List<LlamaCppAdvancedProtocol.BatchLine> lines, string sourceLanguageCode, string targetLanguageCode)
    {
        foreach (var line in lines)
        {
            if (map.TryGetValue(line.Number, out var translation) &&
                TranslationEchoGuard.IsUntranslatedEcho(line.Text, translation, sourceLanguageCode, targetLanguageCode))
            {
                return line.Number;
            }
        }

        return -1;
    }

    /// <summary>
    /// The number of the first line whose translation repeats an earlier one - in the batch or in
    /// the history - for a different source text (see <see cref="TranslationShiftGuard"/>), or -1
    /// when there is none.
    /// </summary>
    internal static int FindDuplicatedTranslation(Dictionary<int, string> map, List<LlamaCppAdvancedProtocol.BatchLine> lines, List<LlamaCppAdvancedProtocol.HistoryPair> history)
    {
        var sources = new List<string>(history.Count + lines.Count);
        var translations = new List<string>(history.Count + lines.Count);
        foreach (var pair in history)
        {
            sources.Add(pair.Source);
            translations.Add(pair.Target);
        }

        foreach (var line in lines)
        {
            sources.Add(line.Text);
            translations.Add(map.TryGetValue(line.Number, out var translation) ? translation : string.Empty);
        }

        var index = TranslationShiftGuard.FindDuplicatedTranslation(sources, translations, history.Count);
        return index < 0 ? -1 : lines[index - history.Count].Number;
    }

    /// <summary>
    /// The number of the first line whose source has text but whose translation is only a
    /// placeholder like "..." (see <see cref="TranslationShiftGuard.FindPlaceholderTranslation"/>),
    /// or -1 when there is none.
    /// </summary>
    internal static int FindPlaceholderTranslation(Dictionary<int, string> map, List<LlamaCppAdvancedProtocol.BatchLine> lines)
    {
        var sources = new List<string>(lines.Count);
        var translations = new List<string>(lines.Count);
        foreach (var line in lines)
        {
            sources.Add(line.Text);
            translations.Add(map.TryGetValue(line.Number, out var translation) ? translation : string.Empty);
        }

        var index = TranslationShiftGuard.FindPlaceholderTranslation(sources, translations);
        return index < 0 ? -1 : lines[index].Number;
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
