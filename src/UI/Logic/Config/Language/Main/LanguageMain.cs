using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Main;

public class LanguageMain
{
    public LanguageMainMenu Menu { get; set; } = new();
    public LanguageMainToolbar Toolbar { get; set; } = new();
    public LanguageMainWaveform Waveform { get; set; } = new();

    public string AudioTrackIsNowX { get; set; }
    public string AudioTrackX { get; set; }
    public string AutoBreakHint { get; set; }
    public string CharactersPerSecond { get; set; }
    public string ChooseColumn { get; set; }
    public string ColumnPaste { get; set; }
    public string CreatedEmptyTranslation { get; set; }
    public string DeleteText { get; set; }
    public string DeleteTextAndShiftCellsUp { get; set; }
    public string EndTimeMustBeAfterStartTime { get; set; }
    public string ErrorLoad7Zip { get; set; }
    public string ErrorLoadBinaryZeroes { get; set; }
    public string ErrorLoadGZip { get; set; }
    public string ErrorLoadJpg { get; set; }
    public string ErrorLoadPng { get; set; }
    public string ErrorLoadRar { get; set; }
    public string ErrorLoadSrr { get; set; }
    public string ErrorLoadTorrent { get; set; }
    public string ErrorLoadZip { get; set; }
    public string ExtractingWaveInfo { get; set; }
    public string ExtractingShotChanges { get; set; }
    public string FailedToExtractWaveInfo { get; set; }
    public string FixedRightToLeftUsingUnicodeControlCharactersX { get; set; }
    public string GeneratingSpectrogramDotDotDot { get; set; }
    public string GeneratingWaveformDotDotDot { get; set; }
    public string InsertEmptyTextAndShiftCellsDown { get; set; }
    public string InsertTextFromSubtitleDotDotDot { get; set; }
    public string InsertedXTextsFromSubtitleY { get; set; }
    public string ItalicHint { get; set; }
    public string JoinedSubtitleLoaded { get; set; }
    public string LineXTextAndTimingChanged { get; set; }
    public string LineXTextChangedFromYToZ { get; set; }
    public string LineXTimingChanged { get; set; }
    public string LoadingWaveInfoFromCache { get; set; }
    public string NoTextInClipboard { get; set; }
    public string OneLineCopiedFromOriginal { get; set; }
    public string OneLineMerged { get; set; }
    public string OneLineSwitched { get; set; }
    public string OverwriteExistingCells { get; set; }
    public string OverwriteOrShiftCellsDown { get; set; }
    public string ParsingMatroskaFile { get; set; }
    public string PasteFromClipboardDotDotDot { get; set; }
    public string RedoPerformed { get; set; }
    public string RedoPerformedXActionLeft { get; set; }
    public string RemovedUnicodeControlCharactersX { get; set; }
    public string RemovedXBlankLines { get; set; }
    public string ReplacedXWithYCountZ { get; set; }
    public string ReplacedXWithYInLineZ { get; set; }
    public string ReversedStartAndEndingsForRightToLeftX { get; set; }
    public string RuleProfileIsX { get; set; }
    public string SaveLanguageFile { get; set; }
    public string SaveXFileAs { get; set; }
    public string ShiftTextCellsDown { get; set; }
    public string SingleLineLength { get; set; }
    public string SpeedIsNowX { get; set; }
    public string SpellCheckResult { get; set; }
    public string SubtitleImportedFromMatroskaFile { get; set; }
    public string TextDown { get; set; }
    public string TextOnly { get; set; }
    public string TextUp { get; set; }
    public string TimeCodesOnly { get; set; }
    public string XPropertiesDotDotDot { get; set; }
    public string TotalCharacters { get; set; }
    public string UnbreakHint { get; set; }
    public string UndoPerformed { get; set; }
    public string UndoPerformedXActionLeft { get; set; }
    public string XLinesCopiedFromOriginal { get; set; }
    public string XLinesMerged { get; set; }
    public string XLinesSelectedOfY { get; set; }
    public string XLinesSwitched { get; set; }
    public string XShotChangedLoaded { get; set; }
    public string YoutubeDlDownloadedSuccessfully { get; set; }
    public string YoutubeDlNotInstalledDownloadNow { get; set; }
    public string InsertUnicodeSymbol { get; set; }
    public string TrimmedXLines { get; set; }
    public string OpenOriginalDifferentNumberOfSubtitlesXY { get; set; }
    public string ImportXMatchingOriginalLines { get; set; }
    public string VideoOpenedChangeLayoutQuestion { get; set; }
    public string SortedByStartTime { get; set; }
    public string SortedByEndTime { get; set; }
    public string ColorHint { get; set; }
    public string RemoveFormattingHint { get; set; }
    public string AssaResolutionResamplerDone { get; set; }
    public string LanguageFileSavedToX { get; set; }
    public string FileExportedInFormatXToY { get; set; }
    public string FileExportedInFormatXToFileY { get; set; }
    public string FixedXLines { get; set; }
    public string TranscriptionCompletedWithXLines { get; set; }
    public string ReplacedXOccurrences { get; set; }
    public string FfmpegDownloadedAndInstalledToX { get; set; }
    public string NothingToSave { get; set; }
    public string NothingToSaveOriginal { get; set; }
    public string LiveSpellCheckLanguageXLoaded { get; set; }

    public LanguageMain()
    {
        AudioTrackIsNowX = "Audio track is now \"{0}\"";
        AudioTrackX = "Audio track {0}";
        AutoBreakHint = "Auto-break selected lines";
        CharactersPerSecond = "Chars/second: {0}";
        ChooseColumn = "Choose column";
        ColumnPaste = "Column paste";
        CreatedEmptyTranslation = "Created empty translation from current subtitle";
        DeleteText = "Delete text";
        DeleteTextAndShiftCellsUp = "Delete text and shift cells up";
        EndTimeMustBeAfterStartTime = "End time must be after start time.";
        ErrorLoad7Zip = "This file seems to be a compressed 7-Zip file.\n\nSubtitle Edit cannot open compressed files.";
        ErrorLoadBinaryZeroes = "Sorry, this file contains only binary zeroes!\n\nIf you have edited this file with Subtitle Edit you might be able to find a backup via the menu item File -&gt; Restore auto-backup...";
        ErrorLoadGZip = "This file seems to be a compressed GZip file.\n\nSubtitle Edit cannot open compressed files.";
        ErrorLoadJpg = "This file seems to be a JPG image file.\n\nSubtitle Edit cannot open image files.";
        ErrorLoadPng = "This file seems to be a PNG image file.\n\nSubtitle Edit cannot open image files.";
        ErrorLoadRar = "This file seems to be a compressed 7-Zip file.\n\nSubtitle Edit cannot open compressed files.";
        ErrorLoadSrr = "This file seems to be a ReScene SRR file.\n\nSubtitle Edit cannot open SRR files.";
        ErrorLoadTorrent = "This file seems to be a BitTorrent file.\n\nSubtitle Edit cannot open torrent files.";
        ErrorLoadZip = "This file seems to be a compressed ZIP file.\n\nSubtitle Edit cannot open compressed files.";
        ExtractingWaveInfo = "Extracting wave info...";
        ExtractingShotChanges = "Extracting shot changes...";
        FailedToExtractWaveInfo = "Failed to extract wave info.";
        FixedRightToLeftUsingUnicodeControlCharactersX = "Fixed right-to-left using Unicode control characters in {0} lines";
        GeneratingSpectrogramDotDotDot = "Generating spectrogram...";
        GeneratingWaveformDotDotDot = "Generating waveform...";
        InsertEmptyTextAndShiftCellsDown = "Insert empty text and shift cells down";
        InsertTextFromSubtitleDotDotDot = "Insert text from subtitle...";
        InsertedXTextsFromSubtitleY = "Inserted {0} texts from subtitle file \"{1}\"";
        ItalicHint = "Italic selected lines/text";
        JoinedSubtitleLoaded = "Joined subtitle loaded";
        LineXTextAndTimingChanged = "Line {0}: Text and timing changed";
        LineXTextChangedFromYToZ = "Line {0}: Text changed from \"{1}\" to \"{2}\"";
        LineXTimingChanged = "Line {0}: Timing changed";
        LoadingWaveInfoFromCache = "Loading wave info from cache...";
        NoTextInClipboard = "No text in clipboard";
        OneLineCopiedFromOriginal = "One line copied from original subtitle";
        OneLineMerged = "One line merged";
        OneLineSwitched = "One line switched";
        OverwriteExistingCells = "Overwrite existing cells";
        OverwriteOrShiftCellsDown = "Overwrite/shift cells down";
        ParsingMatroskaFile = "Parsing Matroska file...";
        PasteFromClipboardDotDotDot = "Paste from clipboard...";
        RedoPerformed = "Redo performed";
        RedoPerformedXActionLeft = "Redo performed (actions left: {0})";
        RemovedUnicodeControlCharactersX = "Removed Unicode control characters from {0} lines";
        RemovedXBlankLines = "Removed {0} blank lines";
        ReplacedXWithYCountZ = "Replaced \"{0}\" with \"{1}\" ({2} occurrences)";
        ReplacedXWithYInLineZ = "Replaced \"{0}\" with \"{1}\" in line {2}";
        ReversedStartAndEndingsForRightToLeftX = "Reversed start and endings for right-to-left in {0} lines";
        RuleProfileIsX = "Rule profile is now \"{0}\"";
        SaveLanguageFile = "Save language file";
        SaveXFileAs = "Save {0} file as";
        ShiftTextCellsDown = "Shift text cells down";
        SingleLineLength = "Line length: ";
        SpeedIsNowX = "Speed is now \"{0}\"";
        SpellCheckResult = "Spell check completed. \n\n• Changed words: {0}\n• Skipped words: {1}";
        SubtitleImportedFromMatroskaFile = "Subtitle imported from Matroska file";
        TextDown = "Text down";
        TextOnly = "Text only";
        TextUp = "Text up";
        TimeCodesOnly = "Time codes only";
        XPropertiesDotDotDot = "{0} properties...";
        TotalCharacters = "Total chars: {0}";
        UnbreakHint = "Unbreak selected lines";
        UndoPerformed = "Undo performed";
        UndoPerformedXActionLeft = "Undo performed (actions left: {0})";
        XLinesCopiedFromOriginal = "{0} lines copied from original subtitle";
        XLinesMerged = "X lines merged";
        XLinesSelectedOfY = "{0} lines selected of {1}";
        XLinesSwitched = "{0} lines switched";
        XShotChangedLoaded = "{0} shot changes loaded";
        YoutubeDlDownloadedSuccessfully = "\"yt-dlp\" downloaded successfully.";
        YoutubeDlNotInstalledDownloadNow = "\"yt-dlp\" is not installed and is required for playing online videos.\n\nDownload now?";
        InsertUnicodeSymbol = "Insert Unicode symbol";
        TrimmedXLines = "Trimmed {0} subtitle lines";
        OpenOriginalDifferentNumberOfSubtitlesXY = "The original subtitle file does not have the same number of subtitles as the current subtitle file.\n\n• Original subtitles: {0}\n• Current subtitles: {1}";
        ImportXMatchingOriginalLines = "Import {0} matching original subtitles?";
        VideoOpenedChangeLayoutQuestion = "A video file has been opened.\n\nDo you want to change the layout to show the video panel?";
        SortedByStartTime = "Sorted by \"Show\" time";
        SortedByEndTime = "Sorted by \"Hide\" time";
        ColorHint = "Color selected lines";
        RemoveFormattingHint = "Remove formatting from selected lines";
        AssaResolutionResamplerDone = "ASSA resolution changed.";
        LanguageFileSavedToX = "Language file saved to {0}";
        FileExportedInFormatXToY = "File exported in format {0} to {1}";
        FileExportedInFormatXToFileY = "File exported in format \"{0}\" to file \"{1}\"";
        FixedXLines = "Fixed {0} lines";
        TranscriptionCompletedWithXLines = "Transcription completed with {0} lines";
        ReplacedXOccurrences = "Replaced {0} occurrences";
        FfmpegDownloadedAndInstalledToX = "ffmpeg downloaded and installed to {0}";
        NothingToSave = "Nothing to save";
        NothingToSaveOriginal = "Nothing to save (original)";
        LiveSpellCheckLanguageXLoaded = "Live spell check language {0} loaded";
    }
}