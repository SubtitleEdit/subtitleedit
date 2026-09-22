namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageCheckArteErrors
{
    public string Title { get; set; }
    public string RunChecks { get; set; }
    public string ApplyCorrections { get; set; }
    public string SelectAllFixable { get; set; }
    public string ClearSelection { get; set; }
    public string UndoLastChange { get; set; }
    public string DownloadErrorReport { get; set; }
    public string HeaderInfo { get; set; }
    public string AcceptShortDurations { get; set; }
    public string Preset { get; set; }
    public string ShiftWholeFileTo { get; set; }
    public string Language { get; set; }
    public string FrameRate { get; set; }
    public string TeletextMaxCells { get; set; }
    public string MinimumGapFrames { get; set; }
    public string ReadingDurationTolerance { get; set; }
    public string Minimum { get; set; }
    public string FramesShort { get; set; }
    public string Checks { get; set; }
    public string NoErrorsDetected { get; set; }
    public string NoChecksRunYet { get; set; }
    public string SelectProfileFirst { get; set; }
    public string NoSnapshotAvailable { get; set; }
    public string AlarmTitle { get; set; }
    public string SaveErrorReport { get; set; }
    public string ErrorReportSaved { get; set; }
    public string ErrorReportSavedToX { get; set; }
    public string LastCorrectionPassUndone { get; set; }
    public string CorrectionsAppliedX { get; set; }
    public string Flow { get; set; }
    public string MaxCells { get; set; }
    public string ArtePreset { get; set; }
    public string GoTo { get; set; }
    public string Go { get; set; }
    public string ForceTeletextBoxColorSdh { get; set; }
    public string GapBefore { get; set; }
    public string GapAfter { get; set; }
    public string CompatibilityTitle { get; set; }
    public string CompatibilityMessageX { get; set; }
    public string SubtitlesLoadedX { get; set; }
    public string HeaderValidNoOptionalChecks { get; set; }
    public string NoIssuesFoundX { get; set; }
    public string CorrectionsAndAlarmsX { get; set; }
    public string NoLanguage { get; set; }
    public string SdhLanguageNotConfigured { get; set; }
    public string SourceHasNoEbuHeader { get; set; }
    public string ApplyTargetHeader { get; set; }
    public string CreateTargetHeader { get; set; }
    public string StartSubtitleMustBeBlank { get; set; }
    public string Missing { get; set; }
    public string CreateStartBlank { get; set; }
    public string SetStartBlankDuration { get; set; }
    public string StartBlankNoRoomAlarm { get; set; }
    public string FrameRoundingRemovesDurationAlarm { get; set; }
    public string RoundTimeCodesToFrames { get; set; }
    public string DurationBelowShortMinimumX { get; set; }
    public string DurationBelowToleratedMinimumX { get; set; }
    public string DurationAboveMaximumX { get; set; }
    public string OptionalTcOutAdjustment { get; set; }
    public string NoSafeTcOutAdjustmentAlarm { get; set; }
    public string CannotCreateGapAlarmX { get; set; }
    public string NotSet { get; set; }
    public string BlankStartsOnRow22 { get; set; }
    public string FileVerticallyShifted { get; set; }
    public string DoubleHeightBottomRowsX { get; set; }
    public string NoTeletextPositionX { get; set; }
    public string ColorMapped { get; set; }
    public string NormalNoSdhBoxing { get; set; }
    public string NormalYellowOrNoColor { get; set; }
    public string UnsupportedColor { get; set; }
    public string ItalicNotAllowed { get; set; }
    public string NothingToReport { get; set; }
    public string FrameRateAndCorrectionsAppliedX { get; set; }
    public string FitSelectedToTimeRange { get; set; }
    public string FitSelectedToTimeRangeDots { get; set; }
    public string StartTimeCode { get; set; }
    public string EndTimeCode { get; set; }
    public string SelectedSubtitles { get; set; }
    public string AvailableDuration { get; set; }
    public string TimeRangeTooShort { get; set; }

    public LanguageCheckArteErrors()
    {
        Title = "Check and fix ARTE errors";
        RunChecks = "Run checks";
        ApplyCorrections = "Apply corrections";
        SelectAllFixable = "Select all fixable";
        ClearSelection = "Clear selection";
        UndoLastChange = "Undo last change";
        DownloadErrorReport = "Download error report";
        HeaderInfo = "Header Info";
        AcceptShortDurations = "Accept short durations";
        Preset = "Preset 37 / 5 / 15%";
        ShiftWholeFileTo = "Shift whole file to";
        Language = "Language";
        FrameRate = "Frame rate";
        TeletextMaxCells = "Teletext max cells";
        MinimumGapFrames = "Minimum GAP (frames)";
        ReadingDurationTolerance = "Reading duration tolerance (%)";
        Minimum = "Min";
        FramesShort = "fr";
        Checks = "Checks";
        NoErrorsDetected = "No ARTE errors detected.";
        NoChecksRunYet = "No ARTE checks have been run yet.";
        SelectProfileFirst = "Select an ARTE profile first.";
        NoSnapshotAvailable = "No subtitle snapshot is available.";
        AlarmTitle = "ARTE alarm";
        SaveErrorReport = "Save ARTE error report";
        ErrorReportSaved = "ARTE error report saved";
        ErrorReportSavedToX = "ARTE error report saved to {0}";
        LastCorrectionPassUndone = "Last correction pass undone.";
        CorrectionsAppliedX = "{0} ARTE fix(es) applied.";
        Flow = "Flow";
        MaxCells = "Max cells:";
        ArtePreset = "ARTE Preset";
        GoTo = "Go to:";
        Go = "Go";
        ForceTeletextBoxColorSdh = "Force Teletext box color (SDH)";
        GapBefore = "Gap before";
        GapAfter = "Gap after";
        CompatibilityTitle = "EBU STL compatibility";
        CompatibilityMessageX = "The ARTE preset uses 37 available Teletext cells. You selected {0}. You can continue working with this value, but subtitles created with it may exceed the 40-cell EBU STL row limit.\n\nEBU STL export will check the actually encoded Teletext cells and will only block saving if a row really exceeds 40 cells.";
        SubtitlesLoadedX = "{0} subtitle(s) loaded for ARTE analysis.";
        HeaderValidNoOptionalChecks = "ARTE target header is valid; no optional checks are selected.";
        NoIssuesFoundX = "No issues found by the {0} selected check(s) currently implemented.";
        CorrectionsAndAlarmsX = "{0} correction(s), {1} unresolved issue(s)/alarm(s).";
        NoLanguage = "No language";
        SdhLanguageNotConfigured = "SDH language code is not configured for this language.";
        SourceHasNoEbuHeader = "Source subtitle has no EBU STL header.";
        ApplyTargetHeader = "Apply ARTE target header. Programme start and informative fields are preserved; timecodes are not converted.";
        CreateTargetHeader = "Create an ARTE EBU STL target header from this subtitle. Timecodes and text are retained.";
        StartSubtitleMustBeBlank = "The subtitle at the ARTE start time code must be an empty five-frame blank/control subtitle.";
        Missing = "Missing";
        CreateStartBlank = "Create a five-frame blank/control subtitle at the start time code before the first normal subtitle.";
        SetStartBlankDuration = "Set the ARTE blank/control subtitle duration to exactly five frames.";
        StartBlankNoRoomAlarm = "ALARM: The ARTE blank/control subtitle must last exactly five frames, but there is not enough room before the next subtitle.";
        FrameRoundingRemovesDurationAlarm = "ALARM: Rounding to 25-fps frames would remove the subtitle duration.";
        RoundTimeCodesToFrames = "TC In and TC Out are rounded to whole 25-fps frames.";
        DurationBelowShortMinimumX = "Duration {0} is below the accepted short-duration minimum of {1} frames.";
        DurationBelowToleratedMinimumX = "Duration {0} is below the tolerated minimum {1} ({2}% tolerance; configured requirement {3}).";
        DurationAboveMaximumX = "Duration {0} exceeds the configured maximum {1}.";
        OptionalTcOutAdjustment = "Optional TC Out adjustment.";
        NoSafeTcOutAdjustmentAlarm = "ALARM: No safe TC Out adjustment is possible.";
        CannotCreateGapAlarmX = "ALARM: Cannot create {0}-frame gap between UT {1} and UT {2} without reducing one of the subtitles below its accepted minimum duration.";
        NotSet = "Not set";
        BlankStartsOnRow22 = "ARTE blank/control subtitle starts on Teletext row 22 (occupying rows 22+23).";
        FileVerticallyShifted = "File appears vertically shifted by one Teletext row; relative position is preserved.";
        DoubleHeightBottomRowsX = "ARTE double-height Teletext starts on row {0}: {1}-line subtitle occupies the bottom rows.";
        NoTeletextPositionX = "No Teletext position is set; propose bottom position for {0}-line subtitle.";
        ColorMapped = "Color is mapped to the nearest Teletext standard color.";
        NormalNoSdhBoxing = "Normal ARTE subtitles do not use SDH boxing; boxing is removed and color is normalized.";
        NormalYellowOrNoColor = "Normal ARTE subtitles use yellow or no color; color is changed to Yellow.";
        UnsupportedColor = "Color is not a hexadecimal or Teletext standard color and cannot be mapped automatically.";
        ItalicNotAllowed = "Italic is not allowed. Remove italic tags.";
        NothingToReport = "Nothing to report.";
        FrameRateAndCorrectionsAppliedX = "Frame rate {0} → 25 fps and {1} ARTE correction(s) applied.";
        FitSelectedToTimeRange = "Fit selected subtitles to time range";
        FitSelectedToTimeRangeDots = "Fit selected subtitles to time range...";
        StartTimeCode = "Start time code:";
        EndTimeCode = "End time code:";
        SelectedSubtitles = "Selected subtitles:";
        AvailableDuration = "Available duration:";
        TimeRangeTooShort = "The selected time range is too short for the subtitles and the requested gaps.";
    }
}
