using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Translate;

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
            var forceSingleLineMode = TranslateEachLineSeparately ||
                                      translator.Name ==
                                      NoLanguageLeftBehindApi.StaticName || // NLLB seems to miss some text...
                                      translator.Name == NoLanguageLeftBehindServe.StaticName ||
                                      translator.Name == CrispAsrMadladTranslate.StaticName; // one CLI process per line

            var index = start;
            var linesTranslated = 0;
            var errorCount = 0;
            var noErrorCount = 0;

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

                    if (noErrorCount > 7)
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
                    Progress?.Invoke(Math.Min(index, subtitle.Paragraphs.Count), subtitle.Paragraphs.Count);
                }
                else
                {
                    forceSingleLineMode = true;
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
