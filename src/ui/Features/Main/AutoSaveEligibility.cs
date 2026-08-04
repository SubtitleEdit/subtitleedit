namespace Nikse.SubtitleEdit.Features.Main;

/// <summary>
/// Pure eligibility gate for the write-back auto-save in <see cref="MainViewModel"/>: decides whether the
/// current document may be auto-saved straight back to its open file, or whether persisting it would need a
/// "Save as" (so auto-save must stay out of the way and leave it to the user).
///
/// Kept side-effect free so it can be unit tested without spinning up the view model, mirroring
/// <see cref="AutoSaveDebounce"/>.
/// </summary>
public static class AutoSaveEligibility
{
    /// <summary>
    /// True only when the document can be written straight back to its existing file in its current format.
    /// </summary>
    /// <param name="isEmpty">No subtitle is loaded.</param>
    /// <param name="hasFileName">The document is backed by a named file on disk.</param>
    /// <param name="converted">
    /// The document came from a conversion/import (OCR, Matroska, Transport Stream, transcription, ...) and has
    /// no committed on-disk save target in its current format yet - saving it would open a "Save as" dialog.
    /// </param>
    /// <param name="lastOpenSaveFormatName">Format name the file was last opened/saved as, or null if never.</param>
    /// <param name="currentFormatName">Currently selected subtitle format name.</param>
    public static bool CanWriteBack(
        bool isEmpty,
        bool hasFileName,
        bool converted,
        string? lastOpenSaveFormatName,
        string currentFormatName)
    {
        // Only auto-save real, already-named files in their current format. Anything that would open a
        // "Save as" dialog (empty, untitled, converted) is left to the user.
        if (isEmpty || !hasFileName || converted)
        {
            return false;
        }

        // A format change turns a plain save into a "Save as"; auto-save never triggers that.
        return lastOpenSaveFormatName != null && lastOpenSaveFormatName == currentFormatName;
    }
}
