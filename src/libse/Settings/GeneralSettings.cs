using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Core.Enums;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class GeneralSettings
    {
        public List<RulesProfile> Profiles { get; set; }
        public string CurrentProfile { get; set; }
        public bool ShowToolbarNew { get; set; }
        public bool ShowToolbarOpen { get; set; }
        public bool ShowToolbarOpenVideo { get; set; }
        public bool ShowToolbarSave { get; set; }
        public bool ShowToolbarSaveAs { get; set; }
        public bool ShowToolbarFind { get; set; }
        public bool ShowToolbarReplace { get; set; }
        public bool ShowToolbarFixCommonErrors { get; set; }
        public bool ShowToolbarRemoveTextForHi { get; set; }
        public bool ShowToolbarToggleSourceView { get; set; }
        public bool ShowToolbarVisualSync { get; set; }
        public bool ShowToolbarBurnIn { get; set; }
        public bool ShowToolbarSpellCheck { get; set; }
        public bool ShowToolbarNetflixGlyphCheck { get; set; }
        public bool ShowToolbarBeautifyTimeCodes { get; set; }
        public bool ShowToolbarSettings { get; set; }
        public bool ShowToolbarHelp { get; set; }

        public int LayoutNumber { get; set; }
        public string LayoutSizes { get; set; }
        public bool ShowVideoPlayer { get; set; }
        public bool ShowAudioVisualizer { get; set; }
        public bool ShowWaveform { get; set; }
        public bool ShowSpectrogram { get; set; }
        public bool CombineSpectrogramAndWaveform { get; set; }
        public bool ShowFrameRate { get; set; }
        public bool ShowVideoControls { get; set; }
        public bool TextAndOrigianlTextBoxesSwitched { get; set; }
        public double DefaultFrameRate { get; set; }
        public double CurrentFrameRate { get; set; }
        public string DefaultSubtitleFormat { get; set; }
        public string DefaultSaveAsFormat { get; set; }
        public string FavoriteSubtitleFormats { get; set; }
        public string DefaultEncoding { get; set; }
        public bool AutoConvertToUtf8 { get; set; }
        public bool AutoGuessAnsiEncoding { get; set; }
        public string TranslationAutoSuffixes { get; set; }
        public string TranslationAutoSuffixDefault { get; set; }
        public string SystemSubtitleFontNameOverride { get; set; }
        public int SystemSubtitleFontSizeOverride { get; set; }

        public string SubtitleFontName { get; set; }
        public int SubtitleTextBoxFontSize { get; set; }
        public bool SubtitleTextBoxSyntaxColor { get; set; }
        public SKColor SubtitleTextBoxHtmlColor { get; set; }
        public SKColor SubtitleTextBoxAssColor { get; set; }
        public int SubtitleListViewFontSize { get; set; }
        public bool SubtitleTextBoxFontBold { get; set; }
        public bool SubtitleListViewFontBold { get; set; }
        public SKColor SubtitleFontColor { get; set; }
        public SKColor SubtitleBackgroundColor { get; set; }
        public string MeasureFontName { get; set; }
        public int MeasureFontSize { get; set; }
        public bool MeasureFontBold { get; set; }
        public int SubtitleLineMaximumPixelWidth { get; set; }
        public bool CenterSubtitleInTextBox { get; set; }
        public bool ShowRecentFiles { get; set; }
        public bool RememberSelectedLine { get; set; }
        public bool StartLoadLastFile { get; set; }
        public bool StartRememberPositionAndSize { get; set; }
        public string StartPosition { get; set; }
        public string StartSize { get; set; }
        public bool StartInSourceView { get; set; }
        public bool RemoveBlankLinesWhenOpening { get; set; }
        public bool RemoveBadCharsWhenOpening { get; set; }
        public int SubtitleLineMaximumLength { get; set; }
        public int MaxNumberOfLines { get; set; }
        public int MaxNumberOfLinesPlusAbort { get; set; }
        public int MergeLinesShorterThan { get; set; }
        public int SubtitleMinimumDisplayMilliseconds { get; set; }
        public int SubtitleMaximumDisplayMilliseconds { get; set; }
        public int MinimumMillisecondsBetweenLines { get; set; }
        public int SetStartEndHumanDelay { get; set; }
        public bool AutoWrapLineWhileTyping { get; set; }
        public double SubtitleMaximumCharactersPerSeconds { get; set; }
        public double SubtitleOptimalCharactersPerSeconds { get; set; }
        public string CpsLineLengthStrategy { get; set; }
        public double SubtitleMaximumWordsPerMinute { get; set; }
        public DialogType DialogStyle { get; set; }
        public ContinuationStyle ContinuationStyle { get; set; }
        public int ContinuationPause { get; set; }
        public string CustomContinuationStyleSuffix { get; set; }
        public bool CustomContinuationStyleSuffixApplyIfComma { get; set; }
        public bool CustomContinuationStyleSuffixAddSpace { get; set; }
        public bool CustomContinuationStyleSuffixReplaceComma { get; set; }
        public string CustomContinuationStylePrefix { get; set; }
        public bool CustomContinuationStylePrefixAddSpace { get; set; }
        public bool CustomContinuationStyleUseDifferentStyleGap { get; set; }
        public string CustomContinuationStyleGapSuffix { get; set; }
        public bool CustomContinuationStyleGapSuffixApplyIfComma { get; set; }
        public bool CustomContinuationStyleGapSuffixAddSpace { get; set; }
        public bool CustomContinuationStyleGapSuffixReplaceComma { get; set; }
        public string CustomContinuationStyleGapPrefix { get; set; }
        public bool CustomContinuationStyleGapPrefixAddSpace { get; set; }
        public bool FixContinuationStyleUncheckInsertsAllCaps { get; set; }
        public bool FixContinuationStyleUncheckInsertsItalic { get; set; }
        public bool FixContinuationStyleUncheckInsertsLowercase { get; set; }
        public bool FixContinuationStyleHideContinuationCandidatesWithoutName { get; set; }
        public bool FixContinuationStyleIgnoreLyrics { get; set; }
        public string SpellCheckLanguage { get; set; }
        public string VideoPlayer { get; set; }
        public int VideoPlayerDefaultVolume { get; set; }
        public string VideoPlayerPreviewFontName { get; set; }
        public int VideoPlayerPreviewFontSize { get; set; }
        public int VideoPlayerPreviewBoxHeight { get; set; }
        public bool VideoPlayerPreviewFontBold { get; set; }
        public bool VideoPlayerShowStopButton { get; set; }
        public bool VideoPlayerShowFullscreenButton { get; set; }
        public bool VideoPlayerShowMuteButton { get; set; }
        public string Language { get; set; }
        public string ListViewLineSeparatorString { get; set; }
        public int ListViewDoubleClickAction { get; set; }
        public string SaveAsUseFileNameFrom { get; set; }
        public string UppercaseLetters { get; set; }
        public int DefaultAdjustMilliseconds { get; set; }
        public bool AutoRepeatOn { get; set; }
        public int AutoRepeatCount { get; set; }
        public bool AutoContinueOn { get; set; }
        public int AutoContinueDelay { get; set; }
        public bool ReturnToStartAfterRepeat { get; set; }
        public bool SyncListViewWithVideoWhilePlaying { get; set; }
        public int AutoBackupSeconds { get; set; }
        public int AutoBackupDeleteAfterMonths { get; set; }
        public string SpellChecker { get; set; }
        public bool AllowEditOfOriginalSubtitle { get; set; }
        public bool PromptDeleteLines { get; set; }
        public bool Undocked { get; set; }
        public string UndockedVideoPosition { get; set; }
        public bool UndockedVideoFullscreen { get; set; }
        public string UndockedWaveformPosition { get; set; }
        public string UndockedVideoControlsPosition { get; set; }
        public bool WaveformCenter { get; set; }
        public bool WaveformAutoGenWhenOpeningVideo { get; set; }
        public int WaveformUpdateIntervalMs { get; set; }
        public int SmallDelayMilliseconds { get; set; }
        public int LargeDelayMilliseconds { get; set; }
        public bool ShowOriginalAsPreviewIfAvailable { get; set; }
        public int LastPacCodePage { get; set; }
        public string OpenSubtitleExtraExtensions { get; set; }
        public bool ListViewColumnsRememberSize { get; set; }

        public int ListViewNumberWidth { get; set; }
        public int ListViewStartWidth { get; set; }
        public int ListViewEndWidth { get; set; }
        public int ListViewDurationWidth { get; set; }
        public int ListViewCpsWidth { get; set; }
        public int ListViewWpmWidth { get; set; }
        public int ListViewGapWidth { get; set; }
        public int ListViewActorWidth { get; set; }
        public int ListViewRegionWidth { get; set; }
        public int ListViewTextWidth { get; set; }

        public int ListViewNumberDisplayIndex { get; set; } = -1;
        public int ListViewStartDisplayIndex { get; set; } = -1;
        public int ListViewEndDisplayIndex { get; set; } = -1;
        public int ListViewDurationDisplayIndex { get; set; } = -1;
        public int ListViewCpsDisplayIndex { get; set; } = -1;
        public int ListViewWpmDisplayIndex { get; set; } = -1;
        public int ListViewGapDisplayIndex { get; set; } = -1;
        public int ListViewActorDisplayIndex { get; set; } = -1;
        public int ListViewRegionDisplayIndex { get; set; } = -1;
        public int ListViewTextDisplayIndex { get; set; } = -1;

        public bool DirectShowDoubleLoad { get; set; }
        public string VlcWaveTranscodeSettings { get; set; }
        public string VlcLocation { get; set; }
        public string VlcLocationRelative { get; set; }
        public string MpvVideoOutputWindows { get; set; }
        public string MpvVideoOutputLinux { get; set; }
        public string MpvVideoVf { get; set; }
        public string MpvVideoAf { get; set; }
        public string MpvExtraOptions { get; set; }
        public bool MpvLogging { get; set; }
        public bool MpvHandlesPreviewText { get; set; }
        public SKColor MpvPreviewTextPrimaryColor { get; set; }
        public SKColor MpvPreviewTextOutlineColor { get; set; }
        public SKColor MpvPreviewTextBackgroundColor { get; set; }
        public decimal MpvPreviewTextOutlineWidth { get; set; }
        public decimal MpvPreviewTextShadowWidth { get; set; }
        public bool MpvPreviewTextOpaqueBox { get; set; }
        public string MpvPreviewTextOpaqueBoxStyle { get; set; }
        public string MpvPreviewTextAlignment { get; set; }
        public int MpvPreviewTextMarginVertical { get; set; }
        public string MpcHcLocation { get; set; }
        public string MkvMergeLocation { get; set; }
        public bool UseFFmpegForWaveExtraction { get; set; }
        public bool FFmpegUseCenterChannelOnly { get; set; }
        public string FFmpegLocation { get; set; }
        public string FFmpegSceneThreshold { get; set; }
        public bool UseTimeFormatHHMMSSFF { get; set; }
        public int SplitBehavior { get; set; }
        public bool SplitRemovesDashes { get; set; }
        public int ClearStatusBarAfterSeconds { get; set; }
        public string Company { get; set; }
        public bool MoveVideo100Or500MsPlaySmallSample { get; set; }
        public bool DisableVideoAutoLoading { get; set; }
        public bool DisableShowingLoadErrors { get; set; }
        public bool AllowVolumeBoost { get; set; }
        public int NewEmptyDefaultMs { get; set; }
        public bool NewEmptyUseAutoDuration { get; set; }
        public bool RightToLeftMode { get; set; }
        public string LastSaveAsFormat { get; set; }
        public bool CheckForUpdates { get; set; }
        public DateTime LastCheckForUpdates { get; set; }
        public bool AutoSave { get; set; }
        public string PreviewAssaText { get; set; }
        public string TagsInToggleHiTags { get; set; }
        public string TagsInToggleCustomTags { get; set; }
        public bool ShowProgress { get; set; }
        public bool ShowNegativeDurationInfoOnSave { get; set; }
        public bool ShowFormatRequiresUtf8Warning { get; set; }
        public long DefaultVideoOffsetInMs { get; set; }
        public string DefaultVideoOffsetInMsList { get; set; }
        public long CurrentVideoOffsetInMs { get; set; }
        public bool CurrentVideoIsSmpte { get; set; }
        public bool AutoSetVideoSmpteForTtml { get; set; }
        public bool AutoSetVideoSmpteForTtmlPrompt { get; set; }
        public string TitleBarAsterisk { get; set; } // Show asteriks "before" or "after" file name (any other value will hide asteriks)
        public bool TitleBarFullFileName { get; set; } // Show full file name with path or just file name
        public bool MeasurementConverterCloseOnInsert { get; set; }
        public string MeasurementConverterCategories { get; set; }
        public bool SubtitleTextBoxAutoVerticalScrollBars { get; set; }
        public int SubtitleTextBoxMaxHeight { get; set; }
        public bool AllowLetterShortcutsInTextBox { get; set; }
        public SKColor DarkThemeForeColor { get; set; }
        public SKColor DarkThemeBackColor { get; set; }
        public SKColor DarkThemeSelectedBackgroundColor { get; set; }
        public SKColor DarkThemeDisabledColor { get; set; }
        public SKColor LastColorPickerColor { get; set; }
        public SKColor LastColorPickerColor1 { get; set; }
        public SKColor LastColorPickerColor2 { get; set; }
        public SKColor LastColorPickerColor3 { get; set; }
        public SKColor LastColorPickerColor4 { get; set; }
        public SKColor LastColorPickerColor5 { get; set; }
        public SKColor LastColorPickerColor6 { get; set; }
        public SKColor LastColorPickerColor7 { get; set; }
        public SKColor LastColorPickerDropper { get; set; }
        public string ToolbarIconTheme { get; set; }
        public bool UseDarkTheme { get; set; }
        public bool DarkThemeShowListViewGridLines { get; set; }
        public bool ShowBetaStuff { get; set; }
        public bool DebugTranslationSync { get; set; }
        public bool UseLegacyDownloader { get; set; }
        public bool UseLegacyHtmlColor { get; set; } = true;
        public string DefaultLanguages { get; set; }

        public GeneralSettings()
        {
            ShowToolbarNew = true;
            ShowToolbarOpen = true;
            ShowToolbarSave = true;
            ShowToolbarSaveAs = false;
            ShowToolbarFind = true;
            ShowToolbarReplace = true;
            ShowToolbarFixCommonErrors = false;
            ShowToolbarVisualSync = true;
            ShowToolbarSpellCheck = true;
            ShowToolbarNetflixGlyphCheck = true;
            ShowToolbarBeautifyTimeCodes = false;
            ShowToolbarSettings = true;
            ShowToolbarHelp = true;
            ShowToolbarToggleSourceView = false;
            ShowVideoPlayer = true;
            ShowAudioVisualizer = true;
            ShowWaveform = true;
            ShowSpectrogram = true;
            ShowFrameRate = false;
            ShowVideoControls = true;
            DefaultFrameRate = 23.976;
            CurrentFrameRate = DefaultFrameRate;
            SubtitleFontName = "Tahoma";
            SubtitleTextBoxFontSize = 12;
            SubtitleListViewFontSize = 10;
            SubtitleTextBoxSyntaxColor = false;
            SubtitleTextBoxHtmlColor = SKColors.CornflowerBlue;
            SubtitleTextBoxAssColor = SKColors.BlueViolet;
            SubtitleTextBoxFontBold = true;
            SubtitleFontColor = SKColors.Black;
            MeasureFontName = "Arial";
            MeasureFontSize = 24;
            MeasureFontBold = false;
            SubtitleLineMaximumPixelWidth = 576;
            SubtitleBackgroundColor = SKColors.White;
            CenterSubtitleInTextBox = false;
            DefaultSubtitleFormat = "SubRip";
            DefaultEncoding = TextEncoding.Utf8WithBom;
            AutoConvertToUtf8 = false;
            AutoGuessAnsiEncoding = true;
            TranslationAutoSuffixes = ";.translation;_translation;.en;_en";
            TranslationAutoSuffixDefault = "<Auto>";
            ShowRecentFiles = true;
            RememberSelectedLine = true;
            StartLoadLastFile = true;
            StartRememberPositionAndSize = true;
            SubtitleLineMaximumLength = 43;
            MaxNumberOfLines = 2;
            MaxNumberOfLinesPlusAbort = 1;
            MergeLinesShorterThan = 33;
            SubtitleMinimumDisplayMilliseconds = 1000;
            SubtitleMaximumDisplayMilliseconds = 8 * 1000;
            RemoveBadCharsWhenOpening = true;
            MinimumMillisecondsBetweenLines = 24;
            SetStartEndHumanDelay = 100;
            AutoWrapLineWhileTyping = false;
            SubtitleMaximumCharactersPerSeconds = 25.0;
            SubtitleOptimalCharactersPerSeconds = 15.0;
            SubtitleMaximumWordsPerMinute = 400;
            DialogStyle = DialogType.DashBothLinesWithSpace;
            ContinuationStyle = ContinuationStyle.None;
            ContinuationPause = 300;
            CustomContinuationStyleSuffix = "";
            CustomContinuationStyleSuffixApplyIfComma = false;
            CustomContinuationStyleSuffixAddSpace = false;
            CustomContinuationStyleSuffixReplaceComma = false;
            CustomContinuationStylePrefix = "";
            CustomContinuationStylePrefixAddSpace = false;
            CustomContinuationStyleUseDifferentStyleGap = true;
            CustomContinuationStyleGapSuffix = "...";
            CustomContinuationStyleGapSuffixApplyIfComma = true;
            CustomContinuationStyleGapSuffixAddSpace = false;
            CustomContinuationStyleGapSuffixReplaceComma = true;
            CustomContinuationStyleGapPrefix = "...";
            CustomContinuationStyleGapPrefixAddSpace = false;
            FixContinuationStyleUncheckInsertsAllCaps = true;
            FixContinuationStyleUncheckInsertsItalic = true;
            FixContinuationStyleUncheckInsertsLowercase = true;
            FixContinuationStyleHideContinuationCandidatesWithoutName = true;
            FixContinuationStyleIgnoreLyrics = true;
            SpellCheckLanguage = null;
            VideoPlayer = string.Empty;
            VideoPlayerDefaultVolume = 75;
            VideoPlayerPreviewFontName = "Tahoma";
            VideoPlayerPreviewFontSize = 12;
            VideoPlayerPreviewBoxHeight = 57;
            VideoPlayerPreviewFontBold = true;
            VideoPlayerShowStopButton = true;
            VideoPlayerShowMuteButton = true;
            VideoPlayerShowFullscreenButton = true;
            ListViewLineSeparatorString = "<br />";
            ListViewDoubleClickAction = 1;
            SaveAsUseFileNameFrom = "video";
            UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWZYXÆØÃÅÄÖÉÈÁÂÀÇÊÍÓÔÕÚŁАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯĞİŞÜÙÁÌÑÎΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ";
            DefaultAdjustMilliseconds = 1000;
            AutoRepeatOn = true;
            AutoRepeatCount = 2;
            AutoContinueOn = false;
            AutoContinueDelay = 2;
            ReturnToStartAfterRepeat = false;
            SyncListViewWithVideoWhilePlaying = false;
            AutoBackupSeconds = 60 * 5;
            AutoBackupDeleteAfterMonths = 3;
            SpellChecker = "hunspell";
            AllowEditOfOriginalSubtitle = true;
            PromptDeleteLines = true;
            Undocked = false;
            UndockedVideoPosition = "-32000;-32000";
            UndockedWaveformPosition = "-32000;-32000";
            UndockedVideoControlsPosition = "-32000;-32000";
            WaveformUpdateIntervalMs = 40;
            SmallDelayMilliseconds = 500;
            LargeDelayMilliseconds = 5000;
            OpenSubtitleExtraExtensions = "*.mp4;*.m4v;*.mkv;*.ts"; // matroska/mp4/m4v files (can contain subtitles)
            ListViewColumnsRememberSize = true;
            DirectShowDoubleLoad = true;
            VlcWaveTranscodeSettings = "acodec=s16l"; // "acodec=s16l,channels=1,ab=64,samplerate=8000";
            MpvVideoOutputWindows = string.Empty; // could also be e.g. "gpu" or "directshow"
            MpvVideoOutputLinux = "x11"; // could also be e.g. "x11";
            MpvHandlesPreviewText = true;
            MpvPreviewTextPrimaryColor = SKColors.White;
            MpvPreviewTextOutlineColor = SKColors.Black;
            MpvPreviewTextBackgroundColor = SKColors.Black;
            MpvPreviewTextOutlineWidth = 1;
            MpvPreviewTextShadowWidth = 1;
            MpvPreviewTextOpaqueBox = false;
            MpvPreviewTextOpaqueBoxStyle = "1";
            MpvPreviewTextAlignment = "2";
            MpvPreviewTextMarginVertical = 10;
            FFmpegSceneThreshold = "0.4"; // threshold for generating shot changes - 0.2 is sensitive (more shot changes), 0.6 is less sensitive (fewer shot changes)
            UseTimeFormatHHMMSSFF = false;
            SplitBehavior = 1; // 0=take gap from left, 1=divide evenly, 2=take gap from right
            SplitRemovesDashes = true;
            ClearStatusBarAfterSeconds = 10;
            MoveVideo100Or500MsPlaySmallSample = false;
            DisableVideoAutoLoading = false;
            NewEmptyUseAutoDuration = true;
            RightToLeftMode = false;
            LastSaveAsFormat = string.Empty;
            SystemSubtitleFontNameOverride = string.Empty;
            CheckForUpdates = true;
            LastCheckForUpdates = DateTime.Now;
            ShowProgress = false;
            ShowNegativeDurationInfoOnSave = true;
            ShowFormatRequiresUtf8Warning = true;
            DefaultVideoOffsetInMs = 10 * 60 * 60 * 1000;
            DefaultVideoOffsetInMsList = "36000000;3600000";
            DarkThemeForeColor = new SKColor(155, 155, 155);
            DarkThemeBackColor = new SKColor(30, 30, 30);
            DarkThemeSelectedBackgroundColor = new SKColor(24, 52, 75);
            DarkThemeDisabledColor = new SKColor(120, 120, 120);
            LastColorPickerColor = SKColors.Yellow;
            LastColorPickerColor1 = SKColors.Red;
            LastColorPickerColor2 = SKColors.Green;
            LastColorPickerColor3 = SKColors.Blue;
            LastColorPickerColor4 = SKColors.White;
            LastColorPickerColor5 = SKColors.Black;
            LastColorPickerColor6 = SKColors.Cyan;
            LastColorPickerColor7 = new SKColor(255, 140, 0); // DarkOrange
            LastColorPickerDropper = SKColors.Transparent;
            ToolbarIconTheme = "Auto";
            UseDarkTheme = false;
            DarkThemeShowListViewGridLines = false;
            AutoSetVideoSmpteForTtml = true;
            AutoSetVideoSmpteForTtmlPrompt = true;
            TitleBarAsterisk = "before";
            MeasurementConverterCloseOnInsert = true;
            MeasurementConverterCategories = "Length;Kilometers;Meters";
            PreviewAssaText = "ABCDEFGHIJKL abcdefghijkl 123";
            TagsInToggleHiTags = "[;]";
            TagsInToggleCustomTags = "(Æ)";
            SubtitleTextBoxMaxHeight = 300;
            ShowBetaStuff = false;
            DebugTranslationSync = false;
            NewEmptyDefaultMs = 2000;
            DialogStyle = DialogType.DashBothLinesWithSpace;
            ContinuationStyle = ContinuationStyle.None;

            if (Configuration.IsRunningOnLinux)
            {
                SubtitleFontName = Configuration.DefaultLinuxFontName;
                VideoPlayerPreviewFontName = Configuration.DefaultLinuxFontName;
            }

            Profiles = new List<RulesProfile>();
            CurrentProfile = "Default";
            Profiles.Add(new RulesProfile
            {
                Name = CurrentProfile,
                SubtitleLineMaximumLength = SubtitleLineMaximumLength,
                MaxNumberOfLines = MaxNumberOfLines,
                MergeLinesShorterThan = MergeLinesShorterThan,
                SubtitleMaximumCharactersPerSeconds = (decimal)SubtitleMaximumCharactersPerSeconds,
                SubtitleOptimalCharactersPerSeconds = (decimal)SubtitleOptimalCharactersPerSeconds,
                SubtitleMaximumDisplayMilliseconds = SubtitleMaximumDisplayMilliseconds,
                SubtitleMinimumDisplayMilliseconds = SubtitleMinimumDisplayMilliseconds,
                SubtitleMaximumWordsPerMinute = (decimal)SubtitleMaximumWordsPerMinute,
                CpsLineLengthStrategy = CpsLineLengthStrategy,
                MinimumMillisecondsBetweenLines = MinimumMillisecondsBetweenLines,
                DialogStyle = DialogStyle,
                ContinuationStyle = ContinuationStyle
            });
            AddExtraProfiles(Profiles);
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
                ContinuationStyle = ContinuationStyle.NoneLeadingTrailingEllipsis
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
                ContinuationStyle = ContinuationStyle.NoneLeadingTrailingEllipsis
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
                ContinuationStyle = ContinuationStyle.LeadingTrailingEllipsis
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
                ContinuationStyle = ContinuationStyle.LeadingTrailingEllipsis
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
                ContinuationStyle = ContinuationStyle.NoneLeadingTrailingEllipsis,
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
                ContinuationStyle = ContinuationStyle.NoneLeadingTrailingEllipsis,
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
                ContinuationStyle = ContinuationStyle.NoneLeadingTrailingEllipsis,
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
                ContinuationStyle = ContinuationStyle.OnlyTrailingEllipsis,
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
                ContinuationStyle = ContinuationStyle.None
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
                ContinuationStyle = ContinuationStyle.None
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
                ContinuationStyle = ContinuationStyle.OnlyTrailingDots
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
                ContinuationStyle = ContinuationStyle.OnlyTrailingDots
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
                ContinuationStyle = ContinuationStyle.OnlyTrailingDots
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
                ContinuationStyle = ContinuationStyle.OnlyTrailingDots
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
                ContinuationStyle = ContinuationStyle.LeadingTrailingDashDots
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
                ContinuationStyle = ContinuationStyle.LeadingTrailingDashDots
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
                ContinuationStyle = ContinuationStyle.OnlyTrailingDots
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
                ContinuationStyle = ContinuationStyle.None
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
                ContinuationStyle = ContinuationStyle.None
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
                ContinuationStyle = ContinuationStyle.None
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
                ContinuationStyle = ContinuationStyle.None
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
                ContinuationStyle = ContinuationStyle.None
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
                ContinuationStyle = ContinuationStyle.None
            });
        }
    }
}