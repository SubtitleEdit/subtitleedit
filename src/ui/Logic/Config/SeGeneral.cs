using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeGeneral
{
    public string Version { get; set; }
    public string Language { get; set; }
    public int LayoutNumber { get; set; } = 0;

    public string CurrentProfile { get; set; }
    public List<RulesProfile> Profiles { get; set; }
    public int SubtitleLineMaximumPixelWidth { get; set; }
    public int SubtitleLineMaximumLength { get; set; } = 43;
    public int MaxNumberOfLines { get; set; }
    public bool SubtitleTextBoxLimitNewLines { get; set; } = true;
    public double SubtitleMaximumCharactersPerSeconds { get; set; }
    public double SubtitleOptimalCharactersPerSeconds { get; set; }
    public double SubtitleMaximumWordsPerMinute { get; set; }
    public int UnbreakLinesShorterThan { get; set; }
    public string DialogStyle { get; set; }
    public string ContinuationStyle { get; set; }
    public string CpsLineLengthStrategy { get; set; }

    public CustomContinuationStyle CustomContinuationStyle { get; set; }
    public bool FixContinuationStyleUncheckInsertsAllCaps { get; set; }
    public bool FixContinuationStyleUncheckInsertsItalic { get; set; }
    public bool FixContinuationStyleUncheckInsertsLowercase { get; set; }
    public bool FixContinuationStyleHideContinuationCandidatesWithoutName { get; set; }
    public bool FixContinuationStyleIgnoreLyrics { get; set; }


    /// <summary>
    /// The user's persisted frame-mode choice, stored as "UseFrameMode" in the settings file.
    /// Read <see cref="UseFrameMode"/> instead, so the temporary override is honored.
    /// </summary>
    [JsonPropertyName("UseFrameMode")]
    public bool UseFrameModePersisted { get; set; }

    /// <summary>
    /// Session-only frame mode, forced while a frame-based format (EBU STL) is the active
    /// format (#14076). Managed by the main view when the subtitle format changes; never saved.
    /// </summary>
    [JsonIgnore]
    public bool? UseFrameModeOverride { get; set; }

    [JsonIgnore]
    public bool UseFrameMode
    {
        get => UseFrameModeOverride ?? UseFrameModePersisted;
        set => UseFrameModePersisted = value;
    }

    public double DefaultFrameRate { get; set; }
    public double CurrentFrameRate { get; set; }
    public string DefaultSubtitleFormat { get; set; }
    public string DefaultSaveAsFormat { get; set; }
    public string FavoriteSubtitleFormats { get; set; }
    public string FavoriteLanguages { get; set; }
    public string DefaultEncoding { get; set; }
    public string SubtitleEnterKeyAction { get; set; }
    public string SubtitleSingleClickAction { get; set; }
    public string SubtitleDoubleClickAction { get; set; }
    public bool SubtitleGridCenterSelectedRow { get; set; }
    public string SaveAsBehavior { get; set; }
    public string SaveAsAppendLanguageCode { get; set; }
    public string DefaultSaveLocation { get; set; }
    public string DefaultSaveLocationCustomFolder { get; set; }
    public bool AutoConvertToUtf8 { get; set; }
    public bool AutoGuessAnsiEncoding { get; set; }
    public int MaxNumberOfLinesPlusAbort { get; set; }
    public int SubtitleMinimumDisplayMilliseconds { get; set; }
    public int SubtitleMaximumDisplayMilliseconds { get; set; }
    public MsOrFramesValue MinimumBetweenLines { get; set; } = new() { Milliseconds = 24, Frames = 2 };
    public int NewEmptyDefaultMs { get; set; }

    /// <summary>How much the time up/down controls change per step when the caret is on the
    /// milliseconds part. Frame mode always steps one frame (#12506).</summary>
    public int TimeCodeUpDownStepMs { get; set; }
    public bool PromptBeforeDelete { get; set; }
    public bool LockTimeCodes { get; set; }

    /// <summary>
    /// SE4 parity: whether an original subtitle that does not line up 1:1 may be edited (and
    /// therefore saved back over its file). Off means it is shown read-only, which is what protects
    /// the lines with no counterpart here (#13449). Remembered from the import prompt.
    /// </summary>
    public bool AllowEditOfOriginalSubtitle { get; set; }

    /// <summary>
    /// Whether that same prompt defaults to showing the original's non-matching lines as extra rows.
    /// Off by default - the plain side-by-side view is the familiar one, and showing the extra rows
    /// locks time codes. Remembered from the prompt (#13449).
    /// </summary>
    public bool ShowOriginalNonMatchingLines { get; set; }
    public bool RememberPositionAndSize { get; set; }
    public bool UndockVideoControls { get; set; }
    public List<SeWindowPosition> WindowPositions { get; set; } = new List<SeWindowPosition>();
    public bool AutoSave { get; set; }
    public bool AutoBackupOn { get; set; }
    public int AutoBackupIntervalMinutes { get; set; }
    public int AutoBackupDeleteAfterDays { get; set; }
    public bool ForceCrLfOnSave { get; set; }

    /// <summary>
    /// Warn before saving in a format with hard line limits (e.g. SCC's 32 chars x 4 lines) when
    /// some subtitles exceed them and will be re-wrapped/truncated. Cleared via "Do not show again".
    /// </summary>
    public bool ShowFormatLimitWarning { get; set; } = true;

    public bool ColorDurationTooShort { get; set; }
    public bool ColorDurationTooLong { get; set; }
    public bool ColorTextTooLong { get; set; }
    public bool ColorTextTooWide { get; set; }
    public int ColorTextTooWidePixels { get; set; }
    public string ColorTextTooWideFontName { get; set; }
    public int ColorTextTooWideFontSize { get; set; }
    public bool ColorTextTooManyLines { get; set; }
    public bool ColorCharactersPerSecond { get; set; }
    public bool ColorWordsPerMinute { get; set; }
    public bool ColorTimeCodeOverlap { get; set; }
    public bool ColorGapTooShort { get; set; }
    public string ErrorColor { get; set; }

    public string FfmpegPath { get; set; }
    public bool FfmpegUseCenterChannelOnly { get; set; }
    public string LibMpvPath { get; set; }

    public string ProxyAddress { get; set; }
    public string ProxyUserName { get; set; }
    public string ProxyPassword { get; set; }
    public string ProxyDomain { get; set; }
    public bool ProxyUseDefaultCredentials { get; set; }
    public string ProxyBypassList { get; set; }

    public bool CheckForUpdatesOnStartup { get; set; } = true;

    // "Stable", "Beta" or empty = auto (users on a beta build get beta updates, stable users stable only)
    public string CheckForUpdatesChannel { get; set; } = string.Empty;

    public bool ShowColumnStartTime { get; set; }
    public bool ShowColumnEndTime { get; set; }
    public bool ShowColumnTeletext { get; set; }
    public bool TeletextAlignmentPreview { get; set; }
    public bool ShowColumnGap { get; set; }
    public bool ShowColumnDuration { get; set; }
    public bool ShowColumnStyle { get; set; }
    public bool ShowColumnActor { get; set; }
    public bool ShowColumnCps { get; set; }
    public bool ShowColumnWpm { get; set; }
    public bool ShowColumnPixelWidth { get; set; }
    public bool ShowColumnLayer { get; set; }

    /// <summary>
    /// The forced-narrative column (#14322). Off by default - only dubbing/localization
    /// workflows that have to deliver a separate forced file need it.
    /// </summary>
    public bool ShowColumnForced { get; set; }

    // Subtitle grid column widths (pixels) keyed by column key (DataGridColumn.Tag),
    // snapshotted on exit and restored on startup. The stretchy Text/OriginalText
    // columns are intentionally not stored so they keep filling the window (#11415).
    public Dictionary<string, double> SubtitleGridColumnWidths { get; set; } = new();

    public bool SelectCurrentSubtitleWhilePlaying { get; set; }
    public bool WriteAn2Tag { get; set; }
    public bool AutoTrimWhiteSpace { get; set; }

    /// <summary>
    /// SE 4 parity (#13588): drop lines with no text when a subtitle file is opened or inserted.
    /// Off by default, like SE 4.
    /// </summary>
    public bool RemoveBlankLinesWhenOpening { get; set; }

    public long CurrentVideoOffsetInMs = 0;
    public bool CurrentVideoIsSmpte = false;

    /// <summary>
    /// Video offsets the user has applied, most recently used first, so the "Set video offset"
    /// dialog can offer them for one-click reuse instead of retyping the same time code every
    /// time (SE 4 parity). Capped at ten entries by the dialog.
    /// </summary>
    public List<long> VideoOffsetHistoryInMs { get; set; } = new List<long>();

    public SeGeneral()
    {
        Version = Se.Version;
        Language = "English";
        LayoutNumber = 0;
        SubtitleLineMaximumLength = 43;
        MaxNumberOfLines = 2;
        MaxNumberOfLinesPlusAbort = 1;
        UnbreakLinesShorterThan = 33;
        SubtitleMinimumDisplayMilliseconds = 1000;
        SubtitleMaximumDisplayMilliseconds = 8 * 1000;
        SubtitleMaximumCharactersPerSeconds = 25.0;
        SubtitleOptimalCharactersPerSeconds = 15.0;
        SubtitleMaximumWordsPerMinute = 400;
        DialogStyle = DialogType.DashBothLinesWithSpace.ToString();
        CpsLineLengthStrategy = nameof(CalcAll);
        ContinuationStyle = Core.Enums.ContinuationStyle.None.ToString();
        CustomContinuationStyle = new CustomContinuationStyle();
        FixContinuationStyleUncheckInsertsAllCaps = true;
        FixContinuationStyleUncheckInsertsItalic = true;
        FixContinuationStyleUncheckInsertsLowercase = true;
        FixContinuationStyleHideContinuationCandidatesWithoutName = true;
        FixContinuationStyleIgnoreLyrics = true;

        Profiles = new List<RulesProfile>();
        CurrentProfile = "Default";
        Profiles.Add(new RulesProfile
        {
            Name = CurrentProfile,
            SubtitleLineMaximumLength = SubtitleLineMaximumLength,
            MaxNumberOfLines = MaxNumberOfLines,
            MergeLinesShorterThan = UnbreakLinesShorterThan,
            SubtitleMaximumCharactersPerSeconds = (decimal)SubtitleMaximumCharactersPerSeconds,
            SubtitleOptimalCharactersPerSeconds = (decimal)SubtitleOptimalCharactersPerSeconds,
            SubtitleMaximumDisplayMilliseconds = SubtitleMaximumDisplayMilliseconds,
            SubtitleMinimumDisplayMilliseconds = SubtitleMinimumDisplayMilliseconds,
            SubtitleMaximumWordsPerMinute = (decimal)SubtitleMaximumWordsPerMinute,
            CpsLineLengthStrategy = CpsLineLengthStrategy,
            MinimumMillisecondsBetweenLines = MinimumBetweenLines.Milliseconds,
            DialogStyle = Enum.Parse<DialogType>(DialogStyle),
            ContinuationStyle = Enum.Parse<ContinuationStyle>(ContinuationStyle),
        });
        AddExtraProfiles(Profiles);

        UseFrameMode = false;
        DefaultFrameRate = 23.976;
        CurrentFrameRate = DefaultFrameRate;
        SubtitleLineMaximumPixelWidth = 576;
        DefaultSubtitleFormat = new SubRip().FriendlyName;
        DefaultEncoding = TextEncoding.Utf8WithBom;
        SubtitleEnterKeyAction = nameof(SubtitleEnterKeyActionType.GoToSubtitleAndSetVideoPosition);
        SubtitleSingleClickAction = nameof(SubtitleSingleClickActionType.None);
        SubtitleDoubleClickAction = nameof(SubtitleDoubleClickActionType.GoToSubtitleAndPause);
        SubtitleGridCenterSelectedRow = false;
        SaveAsBehavior = nameof(SaveAsBehaviourType.UseVideoFileNameThenSubtitleFileName);
        DefaultSaveLocation = nameof(DefaultSaveLocationType.SourceFileFolder);
        DefaultSaveLocationCustomFolder = string.Empty;
        SaveAsAppendLanguageCode = nameof(SaveAsLanguageAppendType.None);
        AutoConvertToUtf8 = false;
        AutoGuessAnsiEncoding = true;
        NewEmptyDefaultMs = 2000;
        TimeCodeUpDownStepMs = 100;
        PromptBeforeDelete = true;
        AutoBackupOn = true;
        AutoBackupIntervalMinutes = 5;
        AutoBackupDeleteAfterDays = 90;
        DefaultSaveAsFormat = new SubRip().FriendlyName;
        FavoriteSubtitleFormats = new SubRip().FriendlyName + ";" + new AdvancedSubStationAlpha().FriendlyName;
        FavoriteLanguages = string.Empty;
        CpsLineLengthStrategy = nameof(CalcAll);
        RememberPositionAndSize = true;

        ColorDurationTooShort = true;
        ColorDurationTooLong = true;
        ColorTextTooLong = true;
        ColorTextTooWide = false;
        ColorTextTooWidePixels = 1200;
        ColorTextTooWideFontName = "Arial";
        ColorTextTooWideFontSize = 40;
        ColorTextTooManyLines = true;
        ColorCharactersPerSecond = true;
        ColorWordsPerMinute = true;
        ColorTimeCodeOverlap = true;
        ColorGapTooShort = true;
        ErrorColor = Color.FromArgb(50, 255, 0, 0).FromColorToHex();

        FfmpegPath = string.Empty;
        LibMpvPath = string.Empty;

        ProxyAddress = string.Empty;
        ProxyUserName = string.Empty;
        ProxyPassword = string.Empty;
        ProxyDomain = string.Empty;
        ProxyBypassList = string.Empty;

        ShowColumnStartTime = true;
        ShowColumnEndTime = true;
        ShowColumnGap = false;
        ShowColumnDuration = true;
        ShowColumnTeletext = true;
        TeletextAlignmentPreview = true;
    }

    public static void AddExtraProfiles(List<RulesProfile> profiles)
    {
        profiles.Add(new RulesProfile
        {
            Name = "Netflix (English)",
            SubtitleLineMaximumLength = 42,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 43,
            SubtitleMaximumCharactersPerSeconds = 20,
            SubtitleOptimalCharactersPerSeconds = 15,
            SubtitleMaximumDisplayMilliseconds = 7007,
            SubtitleMinimumDisplayMilliseconds = 833,
            SubtitleMaximumWordsPerMinute = 240,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 83, // 2 frames for 23.976 fps videos
            DialogStyle = DialogType.DashBothLinesWithoutSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.NoneLeadingTrailingEllipsis
        });
        profiles.Add(new RulesProfile
        {
            Name = "Netflix (Other languages)",
            SubtitleLineMaximumLength = 42,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 43,
            SubtitleMaximumCharactersPerSeconds = 17,
            SubtitleOptimalCharactersPerSeconds = 12,
            SubtitleMaximumDisplayMilliseconds = 7007,
            SubtitleMinimumDisplayMilliseconds = 833,
            SubtitleMaximumWordsPerMinute = 204,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 83, // 2 frames for 23.976 fps videos
            DialogStyle = DialogType.DashBothLinesWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.NoneLeadingTrailingEllipsis
        });
        profiles.Add(new RulesProfile
        {
            Name = "Netflix (Dutch)",
            SubtitleLineMaximumLength = 42,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 43,
            SubtitleMaximumCharactersPerSeconds = 17,
            SubtitleOptimalCharactersPerSeconds = 12,
            SubtitleMaximumDisplayMilliseconds = 7007,
            SubtitleMinimumDisplayMilliseconds = 833,
            SubtitleMaximumWordsPerMinute = 204,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 83, // 2 frames for 23.976 fps videos
            DialogStyle = DialogType.DashSecondLineWithoutSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.LeadingTrailingEllipsis
        });
        profiles.Add(new RulesProfile
        {
            Name = "Netflix (Simplified Chinese)",
            SubtitleLineMaximumLength = 16,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 17,
            SubtitleMaximumCharactersPerSeconds = 9,
            SubtitleOptimalCharactersPerSeconds = 9,
            SubtitleMaximumDisplayMilliseconds = 7007,
            SubtitleMinimumDisplayMilliseconds = 833,
            SubtitleMaximumWordsPerMinute = 100,
            CpsLineLengthStrategy = "CalcAll",
            MinimumMillisecondsBetweenLines = 83, // 2 frames for 23.976 fps videos
            DialogStyle = DialogType.DashBothLinesWithoutSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.LeadingTrailingEllipsis
        });
        profiles.Add(new RulesProfile
        {
            Name = "Amazon Prime (English/Spanish/French)",
            SubtitleLineMaximumLength = 42,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 43,
            SubtitleMaximumCharactersPerSeconds = 17,
            SubtitleOptimalCharactersPerSeconds = 12,
            SubtitleMaximumDisplayMilliseconds = 7007,
            SubtitleMinimumDisplayMilliseconds = 1000,
            SubtitleMaximumWordsPerMinute = 204,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 83, // 2 frames for 23.976 fps videos
            DialogStyle = DialogType.DashBothLinesWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.NoneLeadingTrailingEllipsis,
        });
        profiles.Add(new RulesProfile
        {
            Name = "Amazon Prime (Arabic)",
            SubtitleLineMaximumLength = 42,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 43,
            SubtitleMaximumCharactersPerSeconds = 20,
            SubtitleOptimalCharactersPerSeconds = 12,
            SubtitleMaximumDisplayMilliseconds = 7007,
            SubtitleMinimumDisplayMilliseconds = 1000,
            SubtitleMaximumWordsPerMinute = 240,
            CpsLineLengthStrategy = typeof(CalcAll).Name,
            MinimumMillisecondsBetweenLines = 83, // 2 frames for 23.976 fps videos
            DialogStyle = DialogType.DashBothLinesWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.NoneLeadingTrailingEllipsis,
        });
        profiles.Add(new RulesProfile
        {
            Name = "Amazon Prime (Danish)",
            SubtitleLineMaximumLength = 42,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 43,
            SubtitleMaximumCharactersPerSeconds = 17,
            SubtitleOptimalCharactersPerSeconds = 12,
            SubtitleMaximumDisplayMilliseconds = 7007,
            SubtitleMinimumDisplayMilliseconds = 1000,
            SubtitleMaximumWordsPerMinute = 204,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 83, // 2 frames for 23.976 fps videos
            DialogStyle = DialogType.DashBothLinesWithoutSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.NoneLeadingTrailingEllipsis,
        });
        profiles.Add(new RulesProfile
        {
            Name = "Amazon Prime (Dutch)",
            SubtitleLineMaximumLength = 42,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 43,
            SubtitleMaximumCharactersPerSeconds = 17,
            SubtitleOptimalCharactersPerSeconds = 12,
            SubtitleMaximumDisplayMilliseconds = 7007,
            SubtitleMinimumDisplayMilliseconds = 1000,
            SubtitleMaximumWordsPerMinute = 204,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 83, // 2 frames for 23.976 fps videos
            DialogStyle = DialogType.DashSecondLineWithoutSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.OnlyTrailingEllipsis,
        });
        profiles.Add(new RulesProfile
        {
            Name = "TikTok/YouTube-shorts (9:16)",
            SubtitleLineMaximumLength = 24,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 0,
            SubtitleMaximumCharactersPerSeconds = 25,
            SubtitleOptimalCharactersPerSeconds = 18,
            SubtitleMaximumDisplayMilliseconds = 5000,
            SubtitleMinimumDisplayMilliseconds = 700,
            SubtitleMaximumWordsPerMinute = 300,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 0,
            DialogStyle = DialogType.DashBothLinesWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.None
        });
        profiles.Add(new RulesProfile
        {
            Name = "Arte (German/English)",
            SubtitleLineMaximumLength = 40,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 41,
            SubtitleMaximumCharactersPerSeconds = 20,
            SubtitleOptimalCharactersPerSeconds = 12,
            SubtitleMaximumDisplayMilliseconds = 10000,
            SubtitleMinimumDisplayMilliseconds = 1000,
            SubtitleMaximumWordsPerMinute = 240,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 200, // 5 frames for 25 fps videos
            DialogStyle = DialogType.DashBothLinesWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.None
        });
        profiles.Add(new RulesProfile
        {
            Name = "Dutch professional subtitles (23.976/24 fps)",
            SubtitleLineMaximumLength = 42,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 43,
            SubtitleMaximumCharactersPerSeconds = 15,
            SubtitleOptimalCharactersPerSeconds = 11,
            SubtitleMaximumDisplayMilliseconds = 7007,
            SubtitleMinimumDisplayMilliseconds = 1400,
            SubtitleMaximumWordsPerMinute = 180,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 125,
            DialogStyle = DialogType.DashSecondLineWithoutSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.OnlyTrailingDots
        });
        profiles.Add(new RulesProfile
        {
            Name = "Dutch professional subtitles (25 fps)",
            SubtitleLineMaximumLength = 42,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 43,
            SubtitleMaximumCharactersPerSeconds = 15,
            SubtitleOptimalCharactersPerSeconds = 11,
            SubtitleMaximumDisplayMilliseconds = 7000,
            SubtitleMinimumDisplayMilliseconds = 1400,
            SubtitleMaximumWordsPerMinute = 180,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 120,
            DialogStyle = DialogType.DashSecondLineWithoutSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.OnlyTrailingDots
        });
        profiles.Add(new RulesProfile
        {
            Name = "Dutch fansubs (23.976/24 fps)",
            SubtitleLineMaximumLength = 45,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 46,
            SubtitleMaximumCharactersPerSeconds = 22.5m,
            SubtitleOptimalCharactersPerSeconds = 12,
            SubtitleMaximumDisplayMilliseconds = 7007,
            SubtitleMinimumDisplayMilliseconds = 1200,
            SubtitleMaximumWordsPerMinute = 300,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 125,
            DialogStyle = DialogType.DashSecondLineWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.OnlyTrailingDots
        });
        profiles.Add(new RulesProfile
        {
            Name = "Dutch fansubs (25 fps)",
            SubtitleLineMaximumLength = 45,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 46,
            SubtitleMaximumCharactersPerSeconds = 22.5m,
            SubtitleOptimalCharactersPerSeconds = 12,
            SubtitleMaximumDisplayMilliseconds = 7000,
            SubtitleMinimumDisplayMilliseconds = 1200,
            SubtitleMaximumWordsPerMinute = 300,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 120,
            DialogStyle = DialogType.DashSecondLineWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.OnlyTrailingDots
        });
        profiles.Add(new RulesProfile
        {
            Name = "Danish professional subtitles (23.976/24 fps)",
            SubtitleLineMaximumLength = 40,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 41,
            SubtitleMaximumCharactersPerSeconds = 15,
            SubtitleOptimalCharactersPerSeconds = 10,
            SubtitleMaximumDisplayMilliseconds = 8008,
            SubtitleMinimumDisplayMilliseconds = 2002,
            SubtitleMaximumWordsPerMinute = 180,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 125,
            DialogStyle = DialogType.DashBothLinesWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.LeadingTrailingDashDots
        });
        profiles.Add(new RulesProfile
        {
            Name = "Danish professional subtitles (25 fps)",
            SubtitleLineMaximumLength = 40,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 41,
            SubtitleMaximumCharactersPerSeconds = 15,
            SubtitleOptimalCharactersPerSeconds = 10,
            SubtitleMaximumDisplayMilliseconds = 8000,
            SubtitleMinimumDisplayMilliseconds = 2000,
            SubtitleMaximumWordsPerMinute = 180,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 120,
            DialogStyle = DialogType.DashBothLinesWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.LeadingTrailingDashDots
        });
        profiles.Add(new RulesProfile
        {
            Name = "SDI (Dutch)",
            SubtitleLineMaximumLength = 37,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 38,
            SubtitleMaximumCharactersPerSeconds = 18.75m,
            SubtitleOptimalCharactersPerSeconds = 12,
            SubtitleMaximumDisplayMilliseconds = 7000,
            SubtitleMinimumDisplayMilliseconds = 1320,
            SubtitleMaximumWordsPerMinute = 225,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 160,
            DialogStyle = DialogType.DashSecondLineWithoutSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.OnlyTrailingDots
        });
        profiles.Add(new RulesProfile
        {
            Name = "SW2 (French) (23.976/24 fps)",
            SubtitleLineMaximumLength = 40,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 41,
            SubtitleMaximumCharactersPerSeconds = 25,
            SubtitleOptimalCharactersPerSeconds = 18,
            SubtitleMaximumDisplayMilliseconds = 5005,
            SubtitleMinimumDisplayMilliseconds = 792,
            SubtitleMaximumWordsPerMinute = 300,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 125,
            DialogStyle = DialogType.DashBothLinesWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.None
        });
        profiles.Add(new RulesProfile
        {
            Name = "SW2 (French) (25 fps)",
            SubtitleLineMaximumLength = 40,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 41,
            SubtitleMaximumCharactersPerSeconds = 25,
            SubtitleOptimalCharactersPerSeconds = 18,
            SubtitleMaximumDisplayMilliseconds = 5000,
            SubtitleMinimumDisplayMilliseconds = 800,
            SubtitleMaximumWordsPerMinute = 300,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 120,
            DialogStyle = DialogType.DashBothLinesWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.None
        });
        profiles.Add(new RulesProfile
        {
            Name = "SW3 (French) (23.976/24 fps)",
            SubtitleLineMaximumLength = 40,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 41,
            SubtitleMaximumCharactersPerSeconds = 25,
            SubtitleOptimalCharactersPerSeconds = 18,
            SubtitleMaximumDisplayMilliseconds = 5005,
            SubtitleMinimumDisplayMilliseconds = 792,
            SubtitleMaximumWordsPerMinute = 300,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 167,
            DialogStyle = DialogType.DashBothLinesWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.None
        });
        profiles.Add(new RulesProfile
        {
            Name = "SW3 (French) (25 fps)",
            SubtitleLineMaximumLength = 40,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 41,
            SubtitleMaximumCharactersPerSeconds = 25,
            SubtitleOptimalCharactersPerSeconds = 18,
            SubtitleMaximumDisplayMilliseconds = 5000,
            SubtitleMinimumDisplayMilliseconds = 800,
            SubtitleMaximumWordsPerMinute = 300,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 160,
            DialogStyle = DialogType.DashBothLinesWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.None
        });
        profiles.Add(new RulesProfile
        {
            Name = "SW4 (French) (23.976/24 fps)",
            SubtitleLineMaximumLength = 40,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 41,
            SubtitleMaximumCharactersPerSeconds = 25,
            SubtitleOptimalCharactersPerSeconds = 18,
            SubtitleMaximumDisplayMilliseconds = 5005,
            SubtitleMinimumDisplayMilliseconds = 792,
            SubtitleMaximumWordsPerMinute = 300,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 250,
            DialogStyle = DialogType.DashBothLinesWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.None
        });
        profiles.Add(new RulesProfile
        {
            Name = "SW4 (French) (25 fps)",
            SubtitleLineMaximumLength = 40,
            MaxNumberOfLines = 2,
            MergeLinesShorterThan = 41,
            SubtitleMaximumCharactersPerSeconds = 25,
            SubtitleOptimalCharactersPerSeconds = 18,
            SubtitleMaximumDisplayMilliseconds = 5000,
            SubtitleMinimumDisplayMilliseconds = 800,
            SubtitleMaximumWordsPerMinute = 300,
            CpsLineLengthStrategy = string.Empty,
            MinimumMillisecondsBetweenLines = 240,
            DialogStyle = DialogType.DashBothLinesWithSpace,
            ContinuationStyle = Core.Enums.ContinuationStyle.None
        });
    }

    internal bool IsLanguageRightToLeft() => Language is
        "Arabic" or
        "Dari" or
        "Hebrew" or
        "Kurdish" or
        "Pashto" or
        "Persian" or
        "Sindhi" or
        "Urdu" or
        "Yiddish";
}