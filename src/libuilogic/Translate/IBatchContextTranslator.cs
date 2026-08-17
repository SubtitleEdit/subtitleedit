using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.UiLogic.Translate;

/// <summary>
/// An engine that translates whole batches of lines at once with surrounding context, instead of
/// the line-by-line <see cref="AutoTranslate.IAutoTranslator.Translate"/> call. Implemented by the
/// "advanced" local-LLM engines, whose numbered-batch protocol guarantees line alignment on its
/// own - so callers must skip MergeAndSplitHelper's merge/split heuristics (which would break that
/// alignment) and drive <see cref="TranslateBatchAsync"/> instead.
/// </summary>
public interface IBatchContextTranslator
{
    /// <summary>
    /// Translates the next batch starting at <paramref name="index"/>, writing the results into
    /// the rows, and returns the number of rows translated (always at least 1, or throws).
    /// </summary>
    Task<int> TranslateBatchAsync(ObservableCollection<TranslateRow> rows, int index, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken);
}
