using System.Collections.ObjectModel;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.UiLogic.Translate;

public class DoAutoTranslate
{
    /// <summary>
    /// Force one-line-at-a-time translation from the start. The UI wires this to the
    /// per-engine "translate each line separately" user setting; headless callers
    /// (seconv) leave it off and rely on the built-in per-engine defaults below.
    /// </summary>
    public bool TranslateEachLineSeparately { get; set; }

    /// <summary>Optional progress callback: (lines done, total lines). Invoked as translation advances.</summary>
    public Action<int, int>? Progress { get; set; }

    public async Task<List<TranslateRow>> DoTranslate(Subtitle subtitle, TranslationPair sourceLanguage, TranslationPair targetLanguage, IAutoTranslator translator, CancellationToken cancellationToken)
    {
        try
        {
            translator.Initialize();
            var start = 0;
            // Single-line mode forced by the user setting or the engine itself is permanent;
            // forceSingleLineMode may additionally be turned on temporarily after errors and
            // is only allowed to fall back to merged mode when the permanent flag is off.
            var alwaysSingleLineMode = TranslateEachLineSeparately ||
                                       translator.Name ==
                                       NoLanguageLeftBehindApi.StaticName || // NLLB seems to miss some text...
                                       translator.Name == NoLanguageLeftBehindServe.StaticName ||
                                       translator.Name == CrispAsrMadladTranslate.StaticName; // one CLI process per line
            var forceSingleLineMode = alwaysSingleLineMode;

            var index = start;
            var linesTranslated = 0;
            var errorCount = 0;
            var noErrorCount = 0;
            var noProgressCount = 0;

            var rows = new ObservableCollection<TranslateRow>();
            foreach (var p in subtitle.Paragraphs)
            {
                rows.Add(new TranslateRow { Number = p.Number, Show = p.StartTime.TimeSpan, Hide = p.EndTime.TimeSpan, Duration = p.Duration.ToShortDisplayString(), Text = p.Text });
            }

            while (index < subtitle.Paragraphs.Count)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var linesMergedAndTranslated = 0;

                linesMergedAndTranslated = await MergeAndSplitHelper.MergeAndTranslateIfPossible(rows, sourceLanguage, targetLanguage, index, translator, forceSingleLineMode,
                    cancellationToken);

                if (linesMergedAndTranslated > 0)
                {
                    noErrorCount++;
                    index += linesMergedAndTranslated;
                    Progress?.Invoke(Math.Min(index, subtitle.Paragraphs.Count), subtitle.Paragraphs.Count);

                    var index1 = index;
                    linesTranslated += linesMergedAndTranslated;
                    errorCount = 0;

                    noProgressCount = 0;
                    if (noErrorCount > 7 && !alwaysSingleLineMode)
                    {
                        forceSingleLineMode = false;
                    }

                    continue;
                }

                errorCount++;
                noErrorCount = 0;
                if (errorCount > 3)
                {
                    forceSingleLineMode = true;
                }


                var translateCount = await MergeAndSplitHelper.MergeAndTranslateIfPossible(
                    rows,
                    sourceLanguage,
                    targetLanguage,
                    index,
                    translator,
                    forceSingleLineMode,
                    cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    return rows.ToList();
                }

                if (translateCount > 0)
                {
                    index += translateCount;
                    noProgressCount = 0;
                    Progress?.Invoke(Math.Min(index, subtitle.Paragraphs.Count), subtitle.Paragraphs.Count);
                }
                else
                {
                    forceSingleLineMode = true;

                    // The engine keeps returning nothing for this line without throwing -
                    // without a cap the loop would retry the same line forever.
                    noProgressCount++;
                    if (noProgressCount > 3)
                    {
                        throw new Exception($"Translation engine {translator.Name} returned no translation for line {index + 1} after {noProgressCount} attempts");
                    }
                }
            }

            return rows.ToList();
        }
        catch (Exception exception)
        {
            SeLogger.Error(exception, "Auto-translate failed");
            throw;
        }
    }
}
