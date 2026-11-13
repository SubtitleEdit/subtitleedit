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
    private readonly BatchSizeGovernor _batchSizeGovernor = new();

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
        _batchSizeGovernor.Reset();
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
        var stripped = StrippedLine.StripForTranslation(text.Trim());
        var lines = new List<LlamaCppAdvancedProtocol.BatchLine> { new(1, stripped.Text) };
        var map = await TranslateLinesAsync(lines, new List<LlamaCppAdvancedProtocol.HistoryPair>(), sourceLanguageCode, targetLanguageCode, cancellationToken);
        // An echo that survived the warmer retry in TranslateLinesAsync is accepted: the line may
        // legitimately read the same in both languages, and failing it would abort the translation.
        // A broken value (leaked JSON, the prompt's example) is never written into the subtitle.
        if (map.TryGetValue(1, out var broken) && LlamaCppAdvancedProtocol.IsBrokenValue(stripped.Text, broken))
        {
            throw new HttpRequestException("No usable translation in " + Name + " reply: " + broken);
        }

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
        // After a batch had to be cut, later batches start at the size that worked instead of
        // failing at the full size again every time; the governor grows back towards the setting.
        var batchSize = _batchSizeGovernor.Limit(Math.Clamp(Settings.BatchSize, 1, 50));
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
        // Whole-line "<i>...</i>" and "♪ ... ♪" come off too and go back on afterwards - local
        // models drop them often (#16869).
        var lines = new List<LlamaCppAdvancedProtocol.BatchLine>(count);
        var stripped = new StrippedLine[count];
        for (var i = 0; i < count; i++)
        {
            stripped[i] = StrippedLine.StripForTranslation(rows[index + i].Text);
            lines.Add(new LlamaCppAdvancedProtocol.BatchLine(i + 1, stripped[i].Text));
        }

        // Translation-tuned models (TranslateGemma) can echo history into a lone line's
        // translation, so the single-line case - including bisection retries - goes context-free.
        var history = count > 1 ? CollectHistory(rows, index) : new List<LlamaCppAdvancedProtocol.HistoryPair>();
        var map = await TranslateLinesAsync(lines, history, sourceLanguageCode, targetLanguageCode, cancellationToken);

        // A few lines handed back in the source language in an otherwise good batch are asked for
        // again one at a time; a batch where many came back that way is cut to end before the
        // first of them. A single line that still comes
        // back unchanged after the warmer retry is accepted - it may legitimately read the same in
        // both languages, and throwing would abort the whole translation.
        // Same for a reply where rows moved up by one and the freed row repeats an earlier
        // translation (#16555) or holds a placeholder like "..." (#16585); a lone line has no
        // neighbour to shift into.
        var failure = IsComplete(map, lines)
            ? count == 1
                ? LlamaCppAdvancedProtocol.IsBrokenValue(lines[0].Text, map[lines[0].Number]) ? new BatchFailure(1, IsShift: false) : BatchFailure.None
                : FindFirstFailure(map, lines, history, sourceLanguageCode, targetLanguageCode)
            : new BatchFailure(FindFirstMissingLine(map, lines), IsShift: false);
        if (failure.Line < 0)
        {
            _batchSizeGovernor.OnSuccess();
            if (count > 1)
            {
                await RetranslateEchoedLinesAsync(map, lines, sourceLanguageCode, targetLanguageCode, cancellationToken);
            }

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
            // The model could not fill the full batch - translate a shorter chunk that ends before
            // the line that went wrong; the outer loop calls again for the rest (with the
            // successful chunk now part of the history).
            var nextCount = NextChunkCount(count, failure);
            // The run keeps at least half the failed size: a cut to a single bad first line says
            // little about how many lines the model can handle.
            _batchSizeGovernor.OnCut(Math.Max(nextCount, count / 2));
            return await TranslateChunkAsync(rows, index, nextCount, sourceLanguageCode, targetLanguageCode, applyRowUpdate, cancellationToken);
        }

        throw new HttpRequestException("No usable translation in " + Name + " reply" +
                                       (string.IsNullOrEmpty(Error) ? string.Empty : ": " + Error));
    }

    /// <summary>
    /// Asks again, one line at a time and without context, for the few lines a usable batch
    /// handed back in the source language. The lone-line answer wins when it is a translation; if
    /// the line comes back unchanged again it is kept, as for any single line.
    /// </summary>
    private async Task RetranslateEchoedLinesAsync(Dictionary<int, string> map, List<LlamaCppAdvancedProtocol.BatchLine> lines, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
    {
        foreach (var number in FindUntranslatedEchoes(map, lines, sourceLanguageCode, targetLanguageCode))
        {
            var line = lines.First(l => l.Number == number);
            Dictionary<int, string> single;
            try
            {
                single = await TranslateLinesAsync(new List<LlamaCppAdvancedProtocol.BatchLine> { new(1, line.Text) }, new List<LlamaCppAdvancedProtocol.HistoryPair>(), sourceLanguageCode, targetLanguageCode, cancellationToken);
            }
            catch (HttpRequestException)
            {
                continue; // the batch itself was fine - keep its answer for this line
            }

            if (single.TryGetValue(1, out var translation) && translation.Length > 0 && !LlamaCppAdvancedProtocol.IsBrokenValue(line.Text, translation))
            {
                map[number] = translation;
            }
        }
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
        for (var attempt = 0; attempt < 2 && !cancellationToken.IsCancellationRequested; attempt++)
        {
            try
            {
                // The retry runs a little warmer: at a low temperature (0 for the translation-tuned
                // curated models) the same request against the same cached prompt mostly brings
                // back the same echo, shifted rows or truncated reply.
                var temperatureBump = attempt > 0 ? RetryTemperatureBump : 0;
                var reply = await client.ChatAsync(url, systemPrompt, userContent, responseFormat, cancellationToken, GetModel(), defaultMaxTokens, temperatureBump);
                map = LlamaCppAdvancedProtocol.ParseTranslations(reply);
                if (IsComplete(map, lines))
                {
                    // A few lines kept in the source language in an otherwise translated batch are
                    // usually right (call signs, names, titles: "Mission Day, Foxtrot.") - the caller
                    // asks for those lines on their own instead of throwing the batch away.
                    var echoedLines = FindUntranslatedEchoes(map, lines, sourceLanguageCode, targetLanguageCode);
                    var echoedLine = echoedLines.Count > 0 && !IsFewEchoes(echoedLines.Count, lines.Count) ? echoedLines[0] : -1;
                    if (echoedLine < 0)
                    {
                        // The model merged two rows and padded the end with a copy of an earlier
                        // translation, so every row in between holds its neighbour's text (#16555).
                        // Retried here, then the caller cuts the batch.
                        // The same merge can also leave a placeholder like "..." in the freed row (#16585).
                        var duplicatedLine = FindDuplicatedTranslation(map, lines, history);
                        var placeholderLine = duplicatedLine < 0 ? FindPlaceholderTranslation(map, lines) : -1;
                        if ((duplicatedLine < 0 && placeholderLine < 0) || (lines.Count == 1 && !LlamaCppAdvancedProtocol.IsBrokenValue(lines[0].Text, map[lines[0].Number])))
                        {
                            return map;
                        }

                        Error = duplicatedLine >= 0
                            ? "line " + duplicatedLine + " repeats an earlier translation (lines shifted): " + map[duplicatedLine]
                            : "line " + placeholderLine + " has no text, only \"" + map[placeholderLine] + "\" (lines shifted)";
                        continue;
                    }

                    // The model handed a line back in the source language (TranslateGemma 12B does
                    // this to most of a batch). Retried here, then the caller cuts the batch (a lone line is accepted).
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
            var source = StrippedLine.StripForTranslation(rows[i].Text).Text;
            var target = StrippedLine.StripForTranslation(rows[i].TranslatedText).Text;
            if (source.Length == 0 || target.Length == 0)
            {
                continue; // a pure override/drawing line is no use as an example pair
            }

            history.Add(new LlamaCppAdvancedProtocol.HistoryPair(source, target));
        }

        history.Reverse();
        return history;
    }

    private const double RetryTemperatureBump = 0.3;

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

    /// <summary>The numbers of all lines whose translation is just their source text again.</summary>
    internal static List<int> FindUntranslatedEchoes(Dictionary<int, string> map, List<LlamaCppAdvancedProtocol.BatchLine> lines, string sourceLanguageCode, string targetLanguageCode)
    {
        var result = new List<int>();
        foreach (var line in lines)
        {
            if (map.TryGetValue(line.Number, out var translation) &&
                TranslationEchoGuard.IsUntranslatedEcho(line.Text, translation, sourceLanguageCode, targetLanguageCode))
            {
                result.Add(line.Number);
            }
        }

        return result;
    }

    /// <summary>
    /// True when so few lines of a multi-line batch came back in the source language that they
    /// are more likely names or call signs than a model that stopped translating (TranslateGemma
    /// 12B echoed most of a batch): at most one line in five, and at least one.
    /// </summary>
    internal static bool IsFewEchoes(int echoCount, int lineCount)
    {
        return lineCount > 1 && echoCount <= Math.Max(1, lineCount / 5);
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
            // The prompt's own example value, copied instead of a translation (Index-Nailong 9B
            // filled a whole 50-line batch with "<translation of line 1>", "<translation of line 2>"...
            // - each one different, so neither the duplicate nor the "..." check saw it), or a
            // value that swallowed the following lines' JSON and the model's own notes (#16856).
            if (map.TryGetValue(line.Number, out var value) && LlamaCppAdvancedProtocol.IsBrokenValue(line.Text, value))
            {
                return line.Number;
            }

            sources.Add(line.Text);
            translations.Add(map.TryGetValue(line.Number, out var translation) ? translation : string.Empty);
        }

        var index = TranslationShiftGuard.FindPlaceholderTranslation(sources, translations);
        return index < 0 ? -1 : lines[index].Number;
    }

    /// <summary>
    /// Where a complete batch reply went wrong: <see cref="Line"/> is the 1-based number of the
    /// first bad line (-1 when the reply is usable). <see cref="IsShift"/> marks merged/shifted
    /// rows, where the merge itself happened somewhere <i>before</i> the reported line.
    /// </summary>
    internal readonly record struct BatchFailure(int Line, bool IsShift)
    {
        public static BatchFailure None => new(-1, false);
    }

    /// <summary>The first problem in a complete multi-line reply: an echo, a duplicate or a placeholder.</summary>
    internal static BatchFailure FindFirstFailure(Dictionary<int, string> map, List<LlamaCppAdvancedProtocol.BatchLine> lines, List<LlamaCppAdvancedProtocol.HistoryPair> history, string sourceLanguageCode, string targetLanguageCode)
    {
        var failure = BatchFailure.None;
        var echoes = FindUntranslatedEchoes(map, lines, sourceLanguageCode, targetLanguageCode);
        if (echoes.Count > 0 && !IsFewEchoes(echoes.Count, lines.Count))
        {
            failure = new BatchFailure(echoes[0], false);
        }

        var shift = FindDuplicatedTranslation(map, lines, history);
        if (shift < 0)
        {
            shift = FindPlaceholderTranslation(map, lines);
        }

        if (shift >= 0 && (failure.Line < 0 || shift < failure.Line))
        {
            failure = new BatchFailure(shift, true);
        }

        return failure;
    }

    /// <summary>
    /// The first line number missing from the reply or left empty for a non-empty source; 0 when
    /// the reply held no lines at all (unparseable, a stall), so there is no position to cut at.
    /// </summary>
    internal static int FindFirstMissingLine(Dictionary<int, string> map, List<LlamaCppAdvancedProtocol.BatchLine> lines)
    {
        if (map.Count == 0)
        {
            return 0;
        }

        foreach (var line in lines)
        {
            if (!map.TryGetValue(line.Number, out var translation) ||
                (translation.Length == 0 && !string.IsNullOrWhiteSpace(line.Text)))
            {
                return line.Number;
            }
        }

        return 0;
    }

    /// <summary>
    /// The size of the next, shorter attempt after a failed batch of <paramref name="count"/>
    /// lines: it ends right before the bad line, so a slip near the end of a long batch no longer
    /// throws away half of it. A shift is only noticed where the padding shows up, after the
    /// merged row, so it is cut at least in half too - otherwise the retry would include the same
    /// merge and shrink one line at a time. A bad first line is retried on its own; an unusable
    /// reply with no position (line 0) falls back to halving. Always at least 1 and below
    /// <paramref name="count"/>.
    /// </summary>
    internal static int NextChunkCount(int count, BatchFailure failure)
    {
        var beforeBadLine = failure.Line switch
        {
            > 1 => failure.Line - 1,
            1 => 1,
            _ => count / 2,
        };
        var next = failure.IsShift ? Math.Min(beforeBadLine, count / 2) : beforeBadLine;
        return Math.Clamp(next, 1, Math.Max(1, count - 1));
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
