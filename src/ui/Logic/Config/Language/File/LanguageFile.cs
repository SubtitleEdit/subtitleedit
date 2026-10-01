using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.File;

public class LanguageFile
{
    public LanguageEbuSaveOptions EbuSaveOptions { get; set; } = new();
    public LanguageImport Import { get; set; } = new();
    public LanguageExport Export { get; set; } = new();
    public LanguageStatistics Statistics { get; set; } = new();
    public LanguageManualChosenEncoding ManualChosenEncoding { get; set; } = new();
    public LanguageRestoreAutoBackup RestoreAutoBackup { get; set; } = new();
    public LanguageFilePropertiesDCinema PropertiesDCinema { get; set; } = new();
    public LanguageWebVtt WebVtt { get; set; } = new();
    public string Compare { get; set; }
    public string PreviousDifference { get; set; }
    public string NextDifference { get; set; }
    public string SubtitlesNotAlike { get; set; }
    public string XNumberOfDifference { get; set; }
    public string XNumberOfDifferenceAndPercentChanged { get; set; }
    public string XNumberOfDifferenceAndPercentLettersChanged { get; set; }
    public string ShowOnlyDifferences { get; set; }
    public string CompareOnlyInOneFile { get; set; }
    public string CompareTextOrTimeDifference { get; set; }
    public string CompareNumberDifference { get; set; }
    public string IgnoreWhitespace { get; set; }
    public string IgnoreWhitespaceHint { get; set; }
    public string IgnoreFormatting { get; set; }
    public string IgnoreFormattingHint { get; set; }
    public string ShowOnlyDifferencesInText { get; set; }
    public string IgnoreNumbering { get; set; }
    public string IgnoreNumberingHint { get; set; }
    public string CompareDifferences { get; set; }
    public string CompareTextDifferences { get; set; }
    public string CompareCurrent { get; set; }
    public string CompareReference { get; set; }
    public string CompareEditable { get; set; }
    public string CompareEditableHint { get; set; }
    public string CompareReadOnly { get; set; }
    public string CompareTakeFromReference { get; set; }
    public string CompareInsertFromReference { get; set; }
    public string CompareDeleteFromCurrent { get; set; }
    public string CompareTakeText { get; set; }
    public string CompareTakeTiming { get; set; }
    public string CompareEditLine { get; set; }
    public string CompareEditHint { get; set; }
    public string CompareEdited { get; set; }
    public string CompareOnePendingChange { get; set; }
    public string CompareXPendingChanges { get; set; }
    public string CompareDiscardXChanges { get; set; }
    public string CompareXChangesApplied { get; set; }
    public string CompareChangeTextX { get; set; }
    public string CompareChangeTimingX { get; set; }
    public string CompareChangeTextAndTimingX { get; set; }
    public string CompareChangeEditedX { get; set; }
    public string CompareChangeInsertedX { get; set; }
    public string CompareChangeDeletedX { get; set; }
    public string CompareSyncPoint { get; set; }
    public string CompareSyncPointHint { get; set; }
    public string CompareSyncPickCurrent { get; set; }
    public string CompareSyncPickReference { get; set; }
    public string CompareSyncRemove { get; set; }
    public string CompareClearXSyncPoints { get; set; }
    public string CompareSyncCurrentPickedX { get; set; }
    public string CompareSyncReferencePickedX { get; set; }
    public string CompareSyncWithCurrentX { get; set; }
    public string CompareSyncWithReferenceX { get; set; }
    public string CompareSyncApply { get; set; }
    public string CompareSyncPointSetXY { get; set; }
    public string CompareSyncAlreadyPairedXY { get; set; }
    public string CompareSyncPointShiftedXYZW { get; set; }
    public string CompareChangeSyncShiftXYZ { get; set; }
    public string LoadXFromFile { get; set; }
    public string SaveCompareHtmlTitle { get; set; }
    public string PickMatroskaTrackX { get; set; }
    public string PickTransportStreamTrackX { get; set; }
    public string Chapters { get; set; }
    public string DvdVobFilesMissingX { get; set; }
    public string PickMp4TrackX { get; set; }
    public string PickMxfTrackX { get; set; }
    public string PickMpegTrackX { get; set; }
    public string RosettaProperties { get; set; }
    public string RosettaFontSize { get; set; }
    public string PropertyTimeBase { get; set; }
    public string PropertyFrameRateMultiplier { get; set; }
    public string PropertyDropMode { get; set; }
    public string PropertyDefaultStyle { get; set; }
    public string PropertyDefaultRegion { get; set; }
    public string PropertyStyleAttributeName { get; set; }
    public string PropertyTimeCodeFormat { get; set; }
    public string PropertyTopOrigin { get; set; }
    public string PropertyTopExtent { get; set; }
    public string PropertyBottomOrigin { get; set; }
    public string PropertyBottomExtent { get; set; }
    public string XProperties { get; set; }

    public LanguageFile()
    {
        Compare = "Compare";
        PreviousDifference = "Previous difference";
        NextDifference = "Next difference";
        SubtitlesNotAlike = "Subtitles have no similarities";
        XNumberOfDifference = "Number of differences: {0}";
        XNumberOfDifferenceAndPercentChanged = "Number of differences: {0} ({1:0.##}% of words changed)";
        XNumberOfDifferenceAndPercentLettersChanged = "Number of differences: {0} ({1:0.##}% of letters changed)";
        ShowOnlyDifferences = "Only differences";
        CompareOnlyInOneFile = "Only in one file";
        CompareTextOrTimeDifference = "Text/time difference";
        CompareNumberDifference = "Number difference";
        ShowOnlyDifferencesInText = "Only differences in text";
        IgnoreNumbering = "Ignore numbering";
        IgnoreNumberingHint = "Lines that differ only in their number do not count as different";
        CompareDifferences = "Differences";
        CompareTextDifferences = "Text differences";
        CompareCurrent = "Current";
        CompareReference = "Reference";
        CompareEditable = "Editable";
        CompareEditableHint = "Changes made here go back to the subtitle when you click Apply";
        CompareReadOnly = "Read-only";
        CompareTakeFromReference = "Take text and timing from the reference";
        CompareInsertFromReference = "Insert this line into the current subtitle";
        CompareDeleteFromCurrent = "Delete this line from the current subtitle";
        CompareTakeText = "Take text";
        CompareTakeTiming = "Take timing";
        CompareEditLine = "Edit line";
        CompareEditHint = "Ctrl+Enter to save, Esc to cancel";
        CompareEdited = "edited";
        CompareOnePendingChange = "1 pending change";
        CompareXPendingChanges = "{0} pending changes";
        CompareDiscardXChanges = "Discard the {0} change(s) made in Compare?";
        CompareXChangesApplied = "Compare: {0} change(s) applied";
        CompareChangeTextX = "#{0} text from reference";
        CompareChangeTimingX = "#{0} timing from reference";
        CompareChangeTextAndTimingX = "#{0} text and timing from reference";
        CompareChangeEditedX = "#{0} edited";
        CompareChangeInsertedX = "#{0} inserted";
        CompareChangeDeletedX = "#{0} deleted";
        CompareSyncPoint = "Sync point";
        CompareSyncPointHint = "Sync point - these two lines are always shown as a pair, and the lines above and below are lined up on their own";
        CompareSyncPickCurrent = "Sync point: use this current line";
        CompareSyncPickReference = "Sync point: use this reference line";
        CompareSyncRemove = "Remove sync point";
        CompareClearXSyncPoints = "Clear sync points ({0})";
        CompareSyncCurrentPickedX = "Current #{0} picked as sync point - select the matching reference line and click Sync";
        CompareSyncReferencePickedX = "Reference #{0} picked as sync point - select the matching current line and click Sync";
        CompareSyncWithCurrentX = "Sync with current #{0}";
        CompareSyncWithReferenceX = "Sync with reference #{0}";
        CompareSyncApply = "Sync";
        CompareSyncPointSetXY = "Sync point set: current #{0} and reference #{1} are now shown as a pair";
        CompareSyncAlreadyPairedXY = "Current #{0} and reference #{1} are already shown as a pair with the same start - nothing to sync";
        CompareSyncPointShiftedXYZW = "Sync point set: current #{0}-#{1} shifted {2} to start with reference #{3}";
        CompareChangeSyncShiftXYZ = "#{0}-#{1} shifted {2} (sync point)";
        IgnoreWhitespace = "Ignore whitespace";
        IgnoreWhitespaceHint = "Lines that differ only in spaces, tabs or line breaks do not count as different";
        IgnoreFormatting = "Ignore formatting";
        IgnoreFormattingHint = "Lines that differ only in formatting tags, like <i> or {\\an8}, do not count as different";
        LoadXFromFile = "Load \"{0}\" from file";
        SaveCompareHtmlTitle = "Save compare HTML file";
        PickMatroskaTrackX = "Pick Matroska track - {0}";
        PickTransportStreamTrackX = "Pick transport stream track - {0}";
        Chapters = "Chapters";
        DvdVobFilesMissingX = "VOB files missing ({0}% found)";
        PickMp4TrackX = "Pick MP4 track - {0}";
        PickMxfTrackX = "Pick MXF track - {0}";
        PickMpegTrackX = "Pick MPEG track - {0}";
        RosettaProperties = "Timed Text Rosetta IMSC properties";
        RosettaFontSize = "Font size (row height)";
        PropertyTimeBase = "Time base";
        PropertyFrameRateMultiplier = "Frame rate multiplier";
        PropertyDropMode = "Drop mode";
        PropertyDefaultStyle = "Default style";
        PropertyDefaultRegion = "Default region";
        PropertyStyleAttributeName = "Style attribute name";
        PropertyTimeCodeFormat = "Time code format";
        PropertyTopOrigin = "Top origin";
        PropertyTopExtent = "Top extent";
        PropertyBottomOrigin = "Bottom origin";
        PropertyBottomExtent = "Bottom extent";
        XProperties = "{0} properties";
    }
}