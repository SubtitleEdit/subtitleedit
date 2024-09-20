namespace Nikse.SubtitleEdit.Logic
{
    // The language classes are built for easy xml-serialization (makes save/load code simple)
    public static class LanguageStructure
    {
        public class General
        {
            public string Title { get; set; }
            public string Version { get; set; }
            public string TranslatedBy { get; set; }
            public string CultureName { get; set; }
            public string HelpFile { get; set; }
            public string Ok { get; set; }
            public string Cancel { get; set; }
            public string Yes { get; set; }
            public string No { get; set; }
            public string Close { get; set; }
            public string Apply { get; set; }
            public string ApplyTo { get; set; }
            public string None { get; set; }
            public string All { get; set; }
            public string Preview { get; set; }
            public string ShowPreview { get; set; }
            public string HidePreview { get; set; }
            public string SubtitleFile { get; set; }
            public string SubtitleFiles { get; set; }
            public string AllFiles { get; set; }
            public string VideoFiles { get; set; }
            public string Images { get; set; }
            public string Fonts { get; set; }
            public string AudioFiles { get; set; }
            public string OpenSubtitle { get; set; }
            public string OpenVideoFile { get; set; }
            public string OpenVideoFileTitle { get; set; }
            public string NoVideoLoaded { get; set; }
            public string OnlineVideoFeatureNotAvailable { get; set; }
            public string VideoInformation { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string Duration { get; set; }
            public string CharsPerSec { get; set; }
            public string WordsPerMin { get; set; }
            public string Actor { get; set; }
            public string Gap { get; set; }
            public string Region { get; set; }
            public string Layer { get; set; }
            public string NumberSymbol { get; set; }
            public string Number { get; set; }
            public string Text { get; set; }
            public string HourMinutesSecondsDecimalSeparatorMilliseconds { get; set; }
            public string HourMinutesSecondsFrames { get; set; }
            public string XSeconds { get; set; }
            public string Bold { get; set; }
            public string Italic { get; set; }
            public string Underline { get; set; }
            public string Strikeout { get; set; }
            public string Visible { get; set; }
            public string FrameRate { get; set; }
            public string Name { get; set; }
            public string FileNameXAndSize { get; set; }
            public string ResolutionX { get; set; }
            public string FrameRateX { get; set; }
            public string TotalFramesX { get; set; }
            public string VideoEncodingX { get; set; }
            public string SingleLineLengths { get; set; }
            public string TotalLengthX { get; set; }
            public string TotalLengthXSplitLine { get; set; }
            public string SplitLine { get; set; }
            public string NotAvailable { get; set; }
            public string Overlap { get; set; }
            public string OverlapPreviousLineX { get; set; }
            public string OverlapX { get; set; }
            public string OverlapNextX { get; set; }
            public string OverlapStartAndEnd { get; set; }
            public string Negative { get; set; }
            public string RegularExpressionIsNotValid { get; set; }
            public string CurrentSubtitle { get; set; }
            public string OriginalText { get; set; }
            public string OpenOriginalSubtitleFile { get; set; }
            public string PleaseWait { get; set; }
            public string SessionKey { get; set; }
            public string SessionKeyGenerate { get; set; }
            public string UserName { get; set; }
            public string UserNameAlreadyInUse { get; set; }
            public string WebServiceUrl { get; set; }
            public string IP { get; set; }
            public string VideoWindowTitle { get; set; }
            public string AudioWindowTitle { get; set; }
            public string ControlsWindowTitle { get; set; }
            public string Advanced { get; set; }
            public string Style { get; set; }
            public string StyleLanguage { get; set; }
            public string Character { get; set; }
            public string Class { get; set; }
            public string GeneralText { get; set; }
            public string LineNumber { get; set; }
            public string Before { get; set; }
            public string After { get; set; }
            public string Size { get; set; }
            public string Search { get; set; }
            public string DeleteCurrentLine { get; set; }
            public string Width { get; set; }
            public string Height { get; set; }
            public string Collapse { get; set; }
            public string ShortcutX { get; set; }
            public string ExampleX { get; set; }
            public string ViewX { get; set; }
            public string Reset { get; set; }
            public string Error { get; set; }
            public string Warning { get; set; }
            public string UseLargerFontForThisWindow { get; set; }
            public string ChangeLanguageFilter { get; set; }
            public string MoreInfo { get; set; }
        }

        public class About
        {
            public string Title { get; set; }
            public string AboutText1 { get; set; }
        }

        public class AddToNames
        {
            public string Title { get; set; }
            public string Description { get; set; }
        }

        public class AddToOcrReplaceList
        {
            public string Title { get; set; }
            public string Description { get; set; }
        }

        public class AddToUserDictionary
        {
            public string Title { get; set; }
            public string Description { get; set; }
        }

        public class AddWaveform
        {
            public string Title { get; set; }
            public string SourceVideoFile { get; set; }
            public string GenerateWaveformData { get; set; }
            public string PleaseWait { get; set; }
            public string FfmpegNotFound { get; set; }
            public string GeneratingPeakFile { get; set; }
            public string GeneratingSpectrogram { get; set; }
            public string ExtractingSeconds { get; set; }
            public string ExtractingMinutes { get; set; }
            public string WaveFileNotFound { get; set; }
            public string WaveFileMalformed { get; set; }
            public string LowDiskSpace { get; set; }
            public string FreeDiskSpace { get; set; }
            public string NoAudioTracksFoundGenerateEmptyWaveform { get; set; }
        }

        public class AddWaveformBatch
        {
            public string Title { get; set; }
            public string ExtractTimeCodes { get; set; }
            public string ExtractingAudio { get; set; }
            public string Calculating { get; set; }
            public string ExtractingTimeCodes { get; set; }
            public string DetectingShotChanges { get; set; }
            public string Done { get; set; }
            public string Error { get; set; }
        }

        public class AdjustDisplayDuration
        {
            public string Title { get; set; }
            public string AdjustVia { get; set; }
            public string Seconds { get; set; }
            public string Percent { get; set; }
            public string Recalculate { get; set; }
            public string AddSeconds { get; set; }
            public string SetAsPercent { get; set; }
            public string Note { get; set; }
            public string Fixed { get; set; }
            public string Milliseconds { get; set; }
            public string ExtendOnly { get; set; }
            public string EnforceDurationLimits { get; set; }
            public string CheckShotChanges { get; set; }
            public string BatchCheckShotChanges { get; set; }
        }

        public class ApplyDurationLimits
        {
            public string Title { get; set; }
            public string CheckShotChanges { get; set; }
            public string FixesAvailable { get; set; }
            public string UnableToFix { get; set; }
            public string BatchCheckShotChanges { get; set; }
        }

        public class AudioToText
        {
            public string Title { get; set; }
            public string Info { get; set; }
            public string WhisperInfo { get; set; }
            public string Engine { get; set; }
            public string VoskWebsite { get; set; }
            public string WhisperWebsite { get; set; }
            public string Model { get; set; }
            public string Models { get; set; }
            public string LanguagesAndModels { get; set; }
            public string ChooseModel { get; set; }
            public string ChooseLanguage { get; set; }
            public string OpenModelsFolder { get; set; }
            public string LoadingVoskModel { get; set; }
            public string Transcribing { get; set; }
            public string TranscribingXOfY { get; set; }
            public string PostProcessing { get; set; }
            public string XFilesSavedToVideoSourceFolder { get; set; }
            public string UsePostProcessing { get; set; }
            public string AutoAdjustTimings { get; set; }
            public string BatchMode { get; set; }
            public string KeepPartialTranscription { get; set; }
            public string TranslateToEnglish { get; set; }
            public string RemoveTemporaryFiles { get; set; }
            public string SetCppConstMeFolder { get; set; }
            public string OnlyRunPostProcessing { get; set; }
            public string DownloadFasterWhisperCuda { get; set; }
            public string NoTextFound { get; set; }
            public string FixCasing { get; set; }
            public string AddPeriods { get; set; }
            public string FixShortDuration { get; set; }
        }

        public class AssaAttachments
        {
            public string Title { get; set; }
            public string AttachFiles { get; set; }
            public string FontsAndImages { get; set; }
            public string Graphics { get; set; }
            public string Font { get; set; }
            public string ImageName { get; set; }
            public string IconName { get; set; }
            public string FontName { get; set; }
            public string ImageResized { get; set; }
            public string FilesSkippedX { get; set; }
            public string RemoveOneAttachment { get; set; }
            public string RemoveXAttachments { get; set; }
        }

        public class AssaOverrideTags
        {
            public string ApplyCustomTags { get; set; }
            public string History { get; set; }
            public string TagsToApply { get; set; }
            public string ApplyTo { get; set; }
            public string SelectedLinesX { get; set; }
            public string AdvancedSelection { get; set; }
        }

        public class AssaProgressBarGenerator
        {
            public string Title { get; set; }
            public string Progressbar { get; set; }
            public string Chapters { get; set; }
            public string SplitterWidth { get; set; }
            public string SplitterHeight { get; set; }
            public string XAdjustment { get; set; }
            public string YAdjustment { get; set; }
            public string Position { get; set; }
            public string TextAlignment { get; set; }
            public string SquareCorners { get; set; }
            public string RoundedCorners { get; set; }
            public string Top { get; set; }
            public string Bottom { get; set; }
            public string TakePosFromVideo { get; set; }
        }

        public class AssaResolutionChanger
        {
            public string Title { get; set; }
            public string SourceVideoRes { get; set; }
            public string TargetVideoRes { get; set; }
            public string ChangeResolutionMargins { get; set; }
            public string ChangeResolutionFontSize { get; set; }
            public string ChangeResolutionPositions { get; set; }
            public string ChangeResolutionDrawing { get; set; }
            public string SourceAndTargetEqual { get; set; }
        }

        public class ImageColorPicker
        {
            public string Title { get; set; }
            public string CopyColorHex { get; set; }
            public string CopyColorAssa { get; set; }
            public string CopyColorRgb { get; set; }
        }

        public class AssaSetBackgroundBox
        {
            public string Title { get; set; }
            public string Padding { get; set; }
            public string FillWidth { get; set; }
            public string Drawing { get; set; }
            public string BoxColor { get; set; }
            public string Radius { get; set; }
            public string Step { get; set; }
            public string Spikes { get; set; }
            public string Bubbles { get; set; }
            public string Circle { get; set; }
            public string MarginX { get; set; }
            public string MarginY { get; set; }
            public string OnlyDrawing { get; set; }
            public string DrawingFile { get; set; }
            public string ColorPickerSetLastColor { get; set; }
        }

        public class AssaSetPosition
        {
            public string SetPosition { get; set; }
            public string VideoResolutionX { get; set; }
            public string StyleAlignmentX { get; set; }
            public string CurrentMousePositionX { get; set; }
            public string CurrentTextPositionX { get; set; }
            public string SetPosInfo { get; set; }
            public string Clipboard { get; set; }
            public string ResolutionMissing { get; set; }
            public string RotateXAxis { get; set; }
            public string DistortX { get; set; }
        }

        public class AutoBreakUnbreakLines
        {
            public string TitleAutoBreak { get; set; }
            public string TitleUnbreak { get; set; }
            public string LinesFoundX { get; set; }
            public string OnlyBreakLinesLongerThan { get; set; }
            public string OnlyUnbreakLinesLongerThan { get; set; }
        }

        public class BatchConvert
        {
            public string Title { get; set; }
            public string Input { get; set; }
            public string InputDescription { get; set; }
            public string Status { get; set; }
            public string Output { get; set; }
            public string SaveInSourceFolder { get; set; }
            public string SaveInOutputFolder { get; set; }
            public string OverwriteFiles { get; set; }
            public string Style { get; set; }
            public string UseStyleFromSource { get; set; }
            public string ConvertOptions { get; set; }
            public string RemoveFormatting { get; set; }
            public string RemoveStyleActor { get; set; }
            public string StyleActor { get; set; }
            public string RemoveTextForHI { get; set; }
            public string ConvertColorsToDialog { get; set; }
            public string OverwriteOriginalFiles { get; set; }
            public string RedoCasing { get; set; }
            public string Convert { get; set; }
            public string NothingToConvert { get; set; }
            public string PleaseChooseOutputFolder { get; set; }
            public string NotConverted { get; set; }
            public string Converted { get; set; }
            public string Settings { get; set; }
            public string FixRtl { get; set; }
            public string FixRtlAddUnicode { get; set; }
            public string FixRtlRemoveUnicode { get; set; }
            public string FixRtlReverseStartEnd { get; set; }
            public string SplitLongLines { get; set; }
            public string AutoBalance { get; set; }
            public string ScanFolder { get; set; }
            public string Recursive { get; set; }
            public string BridgeGaps { get; set; }
            public string PlainText { get; set; }
            public string Ocr { get; set; }
            public string AddFiles { get; set; }
            public string Filter { get; set; }
            public string FilterSkipped { get; set; }
            public string FilterSrtNoUtf8BOM { get; set; }
            public string FilterMoreThanTwoLines { get; set; }
            public string FilterContains { get; set; }
            public string FilterFileNameContains { get; set; }
            public string LanguageCodeContains { get; set; }
            public string FixCommonErrorsErrorX { get; set; }
            public string MultipleReplaceErrorX { get; set; }
            public string AutoBalanceErrorX { get; set; }
            public string OffsetTimeCodes { get; set; }
            public string TransportStreamSettings { get; set; }
            public string TransportStreamOverrideXPosition { get; set; }
            public string TransportStreamOverrideYPosition { get; set; }
            public string TransportStreamOverrideVideoSize { get; set; }
            public string TransportStreamFileNameEnding { get; set; }
            public string TransportStreamSettingsButton { get; set; }
            public string RemoveLineBreaks { get; set; }
            public string DeleteLines { get; set; }
            public string TryToUseSourceEncoding { get; set; }
            public string DeleteFirstLines { get; set; }
            public string DeleteLastLines { get; set; }
            public string DeleteContaining { get; set; }
            public string MkvLanguageInOutputFileName { get; set; }
            public string MkvLanguageInOutputFileNameX { get; set; }
            public string MkvLanguageStyleTwoLetter { get; set; }
            public string MkvLanguageStyleThreeLetter { get; set; }
            public string MkvLanguageStyleEmpty { get; set; }
            public string SearchFolderScanVideo { get; set; }
        }

        public class BeautifyTimeCodes
        {
            public string Title { get; set; }
            public string TitleSelectedLines { get; set; }
            public string GroupTimeCodes { get; set; }
            public string AlignTimeCodes { get; set; }
            public string ExtractExactTimeCodes { get; set; }
            public string ExtractTimeCodes { get; set; }
            public string CancelTimeCodes { get; set; }
            public string GroupShotChanges { get; set; }
            public string SnapToShotChanges { get; set; }
            public string ImportShotChanges { get; set; }
            public string EditProfile { get; set; }
            public string NoTimeCodesLoaded { get; set; }
            public string XTimeCodesLoaded { get; set; }
            public string NoTimeCodesLoadedError { get; set; }
            public string NoShotChangesLoaded { get; set; }
            public string XShotChangesLoaded { get; set; }
            public string NoShotChangesLoadedError { get; set; }
            public string BatchAlignTimeCodes { get; set; }
            public string BatchUseExactTimeCodes { get; set; }
            public string BatchSnapToShotChanges { get; set; }
            public string UnfixableParagraphsTitle { get; set; }
            public string UnfixableParagraphsInstructions { get; set; }
            public string UnfixableParagraphsColumnParagraphs { get; set; }
            public string UnfixableParagraphsColumnParagraphsFormat { get; set; }
            public string UnfixableParagraphsColumnGap { get; set; }
        }

        public class BeautifyTimeCodesProfile
        {
            public string Title { get; set; }
            public string LoadPreset { get; set; }
            public string PresetDefault { get; set; }
            public string PresetNetflix { get; set; }
            public string PresetSDI { get; set; }
            public string CreateSimple { get; set; }
            public string General { get; set; }
            public string Gap { get; set; }
            public string GapSuffix { get; set; }
            public string InCues { get; set; }
            public string SubtitlePreviewText { get; set; }
            public string Zones { get; set; }
            public string OutCues { get; set; }
            public string ConnectedSubtitles { get; set; }
            public string InCueClosest { get; set; }
            public string OutCueClosest { get; set; }
            public string TreadAsConnected { get; set; }
            public string Milliseconds { get; set; }
            public string Chaining { get; set; }
            public string InCueOnShot { get; set; }
            public string OutCueOnShot { get; set; }
            public string CheckGeneral { get; set; }
            public string MaxGap { get; set; }
            public string ShotChangeBehavior { get; set; }
            public string DontChain { get; set; }
            public string ExtendCrossingShotChange { get; set; }
            public string ExtendUntilShotChange { get; set; }
            public string ResetWarning { get; set; }
            public string CreateSimpleTitle { get; set; }
            public string CreateSimpleInstruction { get; set; }
            public string CreateSimpleGapInstruction { get; set; }
            public string CreateSimpleInCues { get; set; }
            public string CreateSimpleInCues0Frames { get; set; }
            public string CreateSimpleInCues1Frames { get; set; }
            public string CreateSimpleInCues2Frames { get; set; }
            public string CreateSimpleInCues3Frames { get; set; }
            public string CreateSimpleOutCues { get; set; }
            public string CreateSimpleOutCues0Frames { get; set; }
            public string CreateSimpleOutCues1Frames { get; set; }
            public string CreateSimpleOutCues2Frames { get; set; }
            public string CreateSimpleOutCues3Frames { get; set; }
            public string CreateSimpleOutCuesGap { get; set; }
            public string CreateSimpleSnapClosestCue { get; set; }
            public string CreateSimpleMaxOffset { get; set; }
            public string CreateSimpleMaxOffsetInstruction { get; set; }
            public string CreateSimpleSafeZone { get; set; }
            public string CreateSimpleSafeZoneInstruction { get; set; }
            public string CreateSimpleChainingGap { get; set; }
            public string CreateSimpleChainingGapInstruction { get; set; }
            public string CreateSimpleChainingGapAfterShotChanges { get; set; }
            public string CreateSimpleChainingToolTip { get; set; }
            public string CreateSimpleLoadNetflixRules { get; set; }
            public string Frames { get; set; }
            public string Maximum { get; set; }
            public string GapInMsFormat { get; set; }
            public string OffsetSafeZoneError { get; set; }
        }

        public class BinEdit
        {
            public string ImportImage { get; set; }
            public string ExportImage { get; set; }
            public string SetText { get; set; }
            public string QuickOcr { get; set; }
            public string ResizeBitmaps { get; set; }
            public string ChangeBrightness { get; set; }
            public string ChangeAlpha { get; set; }
            public string ResizeBitmapsForSelectedLines { get; set; }
            public string ChangeColorForSelectedLines { get; set; }
            public string ChangeBrightnessForSelectedLines { get; set; }
            public string ChangeAlphaForSelectedLines { get; set; }
            public string AlignSelectedLines { get; set; }
            public string CenterSelectedLines { get; set; }
            public string TopAlignSelectedLines { get; set; }
            public string BottomAlignSelectedLines { get; set; }
            public string ToggleForcedSelectedLines { get; set; }
            public string SelectForcedLines { get; set; }
            public string SelectNonForcedLines { get; set; }
            public string SizeXY { get; set; }
            public string SetAspectRatio11 { get; set; }
            public string ChangeBrightnessTitle { get; set; }
            public string BrightnessX { get; set; }
            public string ResizeTitle { get; set; }
            public string ResizeX { get; set; }
            public string ChangeAlphaTitle { get; set; }
            public string AlphaX { get; set; }
        }

        public class Bookmarks
        {
            public string EditBookmark { get; set; }
            public string AddBookmark { get; set; }
            public string GoToBookmark { get; set; }
        }

        public class ChangeCasing
        {
            public string Title { get; set; }
            public string ChangeCasingTo { get; set; }
            public string NormalCasing { get; set; }
            public string FixNamesCasing { get; set; }
            public string FixOnlyNamesCasing { get; set; }
            public string OnlyChangeAllUppercaseLines { get; set; }
            public string AllUppercase { get; set; }
            public string AllLowercase { get; set; }
            public string ProperCase { get; set; }
        }

        public class ChangeCasingNames
        {
            public string Title { get; set; }
            public string NamesFoundInSubtitleX { get; set; }
            public string Enabled { get; set; }
            public string Name { get; set; }
            public string LinesFoundX { get; set; }
            public string ExtraNames { get; set; }
        }

        public class ChangeFrameRate
        {
            public string Title { get; set; }
            public string ConvertFrameRateOfSubtitle { get; set; }
            public string FromFrameRate { get; set; }
            public string ToFrameRate { get; set; }
            public string FrameRateNotCorrect { get; set; }
            public string FrameRateNotChanged { get; set; }
        }

        public class ChangeSpeedInPercent
        {
            public string Title { get; set; }
            public string TitleShort { get; set; }
            public string Info { get; set; }
            public string Custom { get; set; }
            public string ToDropFrame { get; set; }
            public string FromDropFrame { get; set; }
            public string AllowOverlap { get; set; }
        }

        public class CheckForUpdates
        {
            public string Title { get; set; }
            public string CheckingForUpdates { get; set; }
            public string CheckingForUpdatesFailedX { get; set; }
            public string CheckingForUpdatesNoneAvailable { get; set; }
            public string CheckingForUpdatesNewVersion { get; set; }
            public string InstallUpdate { get; set; }
            public string NoUpdates { get; set; }
            public string XPluginsHasAnUpdate { get; set; }
            public string OnePluginsHasAnUpdate { get; set; }
            public string Update { get; set; }
        }

        public class ChooseAudioTrack
        {
            public string Title { get; set; }
        }

        public class ChooseEncoding
        {
            public string Title { get; set; }
            public string CodePage { get; set; }
            public string DisplayName { get; set; }
            public string PleaseSelectAnEncoding { get; set; }
        }

        public class ChooseLanguage
        {
            public string Title { get; set; }
            public string Language { get; set; }
        }

        public class ColorChooser
        {
            public string Title { get; set; }
            public string Red { get; set; }
            public string Green { get; set; }
            public string Blue { get; set; }
            public string Alpha { get; set; }
        }

        public class ColumnPaste
        {
            public string Title { get; set; }
            public string ChooseColumn { get; set; }
            public string OverwriteShiftCellsDown { get; set; }
            public string Overwrite { get; set; }
            public string ShiftCellsDown { get; set; }
            public string TimeCodesOnly { get; set; }
            public string TextOnly { get; set; }
            public string OriginalTextOnly { get; set; }
        }

        public class CompareSubtitles
        {
            public string Title { get; set; }
            public string Reload { get; set; }
            public string PreviousDifference { get; set; }
            public string NextDifference { get; set; }
            public string SubtitlesNotAlike { get; set; }
            public string XNumberOfDifference { get; set; }
            public string XNumberOfDifferenceAndPercentChanged { get; set; }
            public string XNumberOfDifferenceAndPercentLettersChanged { get; set; }
            public string ShowOnlyDifferences { get; set; }
            public string IgnoreLineBreaks { get; set; }
            public string IgnoreWhitespace { get; set; }
            public string IgnoreFormatting { get; set; }
            public string OnlyLookForDifferencesInText { get; set; }
            public string CannotCompareWithImageBasedSubtitles { get; set; }
        }

        public class ConvertColorsToDialog
        {
            public string Title { get; set; }
            public string RemoveColorTags { get; set; }
            public string AddNewLines { get; set; }
            public string ReBreakLines { get; set; }
        }

        public class DCinemaProperties
        {
            public string Title { get; set; }
            public string TitleSmpte { get; set; }
            public string SubtitleId { get; set; }
            public string GenerateId { get; set; }
            public string MovieTitle { get; set; }
            public string ReelNumber { get; set; }
            public string Language { get; set; }
            public string IssueDate { get; set; }
            public string EditRate { get; set; }
            public string TimeCodeRate { get; set; }
            public string StartTime { get; set; }
            public string Font { get; set; }
            public string FontId { get; set; }
            public string FontUri { get; set; }
            public string FontColor { get; set; }
            public string FontEffect { get; set; }
            public string FontEffectColor { get; set; }
            public string FontSize { get; set; }
            public string TopBottomMargin { get; set; }
            public string FadeUpTime { get; set; }
            public string FadeDownTime { get; set; }
            public string ZPosition { get; set; }
            public string ZPositionHelp { get; set; }
            public string ChooseColor { get; set; }
            public string Generate { get; set; }
            public string GenerateNewIdOnSave { get; set; }
        }

        public class DurationsBridgeGaps
        {
            public string Title { get; set; }
            public string GapsBridgedX { get; set; }
            public string GapToNext { get; set; }
            public string GapToNextFrames { get; set; }
            public string BridgeGapsSmallerThanXPart1 { get; set; }
            public string BridgeGapsSmallerThanXPart2 { get; set; }
            public string BridgeGapsSmallerThanXPart1Frames { get; set; }
            public string BridgeGapsSmallerThanXPart2Frames { get; set; }

            public string MinMillisecondsBetweenLines { get; set; }
            public string MinFramesBetweenLines { get; set; }
            public string ProlongEndTime { get; set; }
            public string DivideEven { get; set; }
        }

        public class DvdSubRip
        {
            public string Title { get; set; }
            public string DvdGroupTitle { get; set; }
            public string IfoFile { get; set; }
            public string IfoFiles { get; set; }
            public string VobFiles { get; set; }
            public string Add { get; set; }
            public string Remove { get; set; }
            public string Clear { get; set; }
            public string MoveUp { get; set; }
            public string MoveDown { get; set; }
            public string Languages { get; set; }
            public string PalNtsc { get; set; }
            public string Pal { get; set; }
            public string Ntsc { get; set; }
            public string StartRipping { get; set; }
            public string Abort { get; set; }
            public string AbortedByUser { get; set; }
            public string ReadingSubtitleData { get; set; }
            public string RippingVobFileXofYZ { get; set; }
            public string WrongIfoType { get; set; }
        }

        public class DvdSubRipChooseLanguage
        {
            public string Title { get; set; }
            public string ChooseLanguageStreamId { get; set; }
            public string UnknownLanguage { get; set; }
            public string SubtitleImageXofYAndWidthXHeight { get; set; }
            public string SubtitleImage { get; set; }
        }

        public class EbuSaveOptions
        {
            public string Title { get; set; }
            public string GeneralSubtitleInformation { get; set; }
            public string CodePageNumber { get; set; }
            public string DiskFormatCode { get; set; }
            public string DisplayStandardCode { get; set; }
            public string ColorRequiresTeletext { get; set; }
            public string AlignmentRequiresTeletext { get; set; }
            public string TeletextCharsShouldBe38 { get; set; }
            public string CharacterCodeTable { get; set; }
            public string LanguageCode { get; set; }
            public string OriginalProgramTitle { get; set; }
            public string OriginalEpisodeTitle { get; set; }
            public string TranslatedProgramTitle { get; set; }
            public string TranslatedEpisodeTitle { get; set; }
            public string TranslatorsName { get; set; }
            public string SubtitleListReferenceCode { get; set; }
            public string CountryOfOrigin { get; set; }
            public string TimeCodeStatus { get; set; }
            public string TimeCodeStartOfProgramme { get; set; }
            public string RevisionNumber { get; set; }
            public string MaxNoOfDisplayableChars { get; set; }
            public string MaxNumberOfDisplayableRows { get; set; }
            public string DiskSequenceNumber { get; set; }
            public string TotalNumberOfDisks { get; set; }
            public string Import { get; set; }
            public string TextAndTimingInformation { get; set; }
            public string JustificationCode { get; set; }
            public string VerticalPosition { get; set; }
            public string MarginTop { get; set; }
            public string MarginBottom { get; set; }
            public string NewLineRows { get; set; }
            public string Teletext { get; set; }
            public string UseBox { get; set; }
            public string DoubleHeight { get; set; }
            public string Errors { get; set; }
            public string ErrorsX { get; set; }
            public string MaxLengthError { get; set; }
            public string TextUnchangedPresentation { get; set; }
            public string TextLeftJustifiedText { get; set; }
            public string TextCenteredText { get; set; }
            public string TextRightJustifiedText { get; set; }
            public string UseBoxForOneNewLine { get; set; }
        }

        public class EffectKaraoke
        {
            public string Title { get; set; }
            public string ChooseColor { get; set; }
            public string TotalSeconds { get; set; }
            public string EndDelayInSeconds { get; set; }
            public string WordEffect { get; set; }
            public string CharacterEffect { get; set; }
        }

        public class EffectTypewriter
        {
            public string Title { get; set; }
            public string TotalSeconds { get; set; }
            public string EndDelayInSeconds { get; set; }
        }

        public class ExportCustomText
        {
            public string Title { get; set; }
            public string Formats { get; set; }
            public string New { get; set; }
            public string Edit { get; set; }
            public string Delete { get; set; }
            public string SaveAs { get; set; }
            public string SaveSubtitleAs { get; set; }
            public string SubtitleExportedInCustomFormatToX { get; set; }
        }

        public class ExportCustomTextFormat
        {
            public string Title { get; set; }
            public string Template { get; set; }
            public string Header { get; set; }
            public string TextLine { get; set; }
            public string TimeCode { get; set; }
            public string NewLine { get; set; }
            public string Footer { get; set; }
            public string FileExtension { get; set; }
            public string DoNotModify { get; set; }
        }

        public class ExportFcpXmlAdvanced
        {
            public string Title { get; set; }
            public string FontName { get; set; }
            public string FontSize { get; set; }
            public string FontFace { get; set; }
            public string FontFaceRegular { get; set; }
            public string Alignment { get; set; }
            public string Baseline { get; set; }
        }

        public class ExportPngXml
        {
            public string Title { get; set; }
            public string ImageSettings { get; set; }
            public string FontFamily { get; set; }
            public string FontSize { get; set; }
            public string FontColor { get; set; }
            public string BorderColor { get; set; }
            public string BorderWidth { get; set; }
            public string BorderStyle { get; set; }
            public string BorderStyleOneBox { get; set; }
            public string BorderStyleBoxForEachLine { get; set; }
            public string BorderStyleNormalWidthX { get; set; }
            public string ShadowColor { get; set; }
            public string ShadowWidth { get; set; }
            public string Transparency { get; set; }
            public string ImageFormat { get; set; }
            public string FullFrameImage { get; set; }
            public string SimpleRendering { get; set; }
            public string AntiAliasingWithTransparency { get; set; }
            public string Text3D { get; set; }
            public string ImagePrefix { get; set; }
            public string SideBySide3D { get; set; }
            public string HalfTopBottom3D { get; set; }
            public string Depth { get; set; }
            public string ExportAllLines { get; set; }
            public string XImagesSavedInY { get; set; }
            public string VideoResolution { get; set; }
            public string Align { get; set; }
            public string Left { get; set; }
            public string Right { get; set; }
            public string Center { get; set; }
            public string CenterLeftJustify { get; set; }
            public string CenterLeftJustifyDialogs { get; set; }
            public string CenterTopJustify { get; set; }
            public string CenterRightJustify { get; set; }
            public string BottomMargin { get; set; }
            public string LeftRightMargin { get; set; }
            public string SaveBluRaySupAs { get; set; }
            public string SaveVobSubAs { get; set; }
            public string SaveFabImageScriptAs { get; set; }
            public string SaveDvdStudioProStlAs { get; set; }
            public string SaveDigitalCinemaInteropAs { get; set; }
            public string SaveDigitalCinemaSmpte2014 { get; set; }
            public string SavePremiereEdlAs { get; set; }
            public string SaveFcpAs { get; set; }
            public string SaveDostAs { get; set; }
            public string SomeLinesWereTooLongX { get; set; }
            public string LineHeight { get; set; }
            public string BoxSingleLine { get; set; }
            public string BoxMultiLine { get; set; }
            public string Forced { get; set; }
            public string ChooseBackgroundColor { get; set; }
            public string SaveImageAs { get; set; }
            public string FcpUseFullPathUrl { get; set; }
        }

        public class ExportText
        {
            public string Title { get; set; }
            public string Preview { get; set; }
            public string ExportOptions { get; set; }
            public string FormatText { get; set; }
            public string None { get; set; }
            public string MergeAllLines { get; set; }
            public string UnbreakLines { get; set; }
            public string RemoveStyling { get; set; }
            public string ShowLineNumbers { get; set; }
            public string AddNewLineAfterLineNumber { get; set; }
            public string ShowTimeCode { get; set; }
            public string AddNewLineAfterTimeCode { get; set; }
            public string AddNewLineAfterTexts { get; set; }
            public string AddNewLineBetweenSubtitles { get; set; }
            public string TimeCodeFormat { get; set; }
            public string Srt { get; set; }
            public string Milliseconds { get; set; }
            public string HHMMSSFF { get; set; }
            public string TimeCodeSeparator { get; set; }
        }

        public class ExtractDateTimeInfo
        {
            public string Title { get; set; }
            public string OpenVideoFile { get; set; }
            public string StartFrom { get; set; }
            public string DateTimeFormat { get; set; }
            public string Example { get; set; }
            public string GenerateSubtitle { get; set; }
        }

        public class FindDialog
        {
            public string Title { get; set; }
            public string FindNext { get; set; }
            public string FindPrevious { get; set; }
            public string Normal { get; set; }
            public string CaseSensitive { get; set; }
            public string RegularExpression { get; set; }
            public string WholeWord { get; set; }
            public string Count { get; set; }
            public string XNumberOfMatches { get; set; }
            public string OneMatch { get; set; }
        }

        public class FindSubtitleLine
        {
            public string Title { get; set; }
            public string Find { get; set; }
            public string FindNext { get; set; }
        }

        public class FixCommonErrors
        {
            public string Title { get; set; }
            public string Step1 { get; set; }
            public string WhatToFix { get; set; }
            public string Example { get; set; }
            public string SelectAll { get; set; }
            public string InverseSelection { get; set; }
            public string Back { get; set; }
            public string Next { get; set; }
            public string Step2 { get; set; }
            public string Fixes { get; set; }
            public string Log { get; set; }
            public string Function { get; set; }
            public string RemovedEmptyLine { get; set; }
            public string RemovedEmptyLineAtTop { get; set; }
            public string RemovedEmptyLineAtBottom { get; set; }
            public string RemovedEmptyLineInMiddle { get; set; }
            public string RemovedEmptyLinesUnusedLineBreaks { get; set; }
            public string FixOverlappingDisplayTimes { get; set; }
            public string FixShortDisplayTimes { get; set; }
            public string FixLongDisplayTimes { get; set; }
            public string FixShortGaps { get; set; }
            public string FixInvalidItalicTags { get; set; }
            public string RemoveUnneededSpaces { get; set; }
            public string RemoveUnneededPeriods { get; set; }
            public string FixCommas { get; set; }
            public string FixMissingSpaces { get; set; }
            public string BreakLongLines { get; set; }
            public string RemoveLineBreaks { get; set; }
            public string RemoveLineBreaksAll { get; set; }
            public string RemoveLineBreaksPixelWidth { get; set; }
            public string FixUppercaseIInsideLowercaseWords { get; set; }
            public string FixDoubleApostrophes { get; set; }
            public string AddPeriods { get; set; }
            public string StartWithUppercaseLetterAfterParagraph { get; set; }
            public string StartWithUppercaseLetterAfterPeriodInsideParagraph { get; set; }
            public string StartWithUppercaseLetterAfterColon { get; set; }
            public string FixLowercaseIToUppercaseI { get; set; }
            public string FixCommonOcrErrors { get; set; }
            public string CommonOcrErrorsFixed { get; set; }
            public string RemoveSpaceBetweenNumber { get; set; }
            public string BreakDialogsOnOneLine { get; set; }
            public string RemoveDialogFirstInNonDialogs { get; set; }
            public string NormalizeStrings { get; set; }
            public string FixTurkishAnsi { get; set; }
            public string FixDanishLetterI { get; set; }
            public string FixSpanishInvertedQuestionAndExclamationMarks { get; set; }
            public string AddMissingQuote { get; set; }
            public string AddMissingQuotes { get; set; }
            public string RemoveHyphensSingleLine { get; set; }
            public string FixHyphensInDialogs { get; set; }
            public string AddMissingQuotesExample { get; set; }
            public string XMissingQuotesAdded { get; set; }
            public string Fix3PlusLine { get; set; }
            public string Fix3PlusLines { get; set; }
            public string Analysing { get; set; }
            public string NothingToFix { get; set; }
            public string FixesFoundX { get; set; }
            public string XFixesApplied { get; set; }
            public string NothingFixableBut { get; set; }
            public string XFixedBut { get; set; }
            public string XCouldBeFixedBut { get; set; }
            public string FixFirstLetterToUppercaseAfterParagraph { get; set; }
            public string MergeShortLine { get; set; }
            public string MergeShortLineAll { get; set; }
            public string UnbreakShortLinePixelWidth { get; set; }
            public string BreakLongLine { get; set; }
            public string FixLongDisplayTime { get; set; }
            public string FixInvalidItalicTag { get; set; }
            public string FixShortDisplayTime { get; set; }
            public string FixOverlappingDisplayTime { get; set; }
            public string FixShortGap { get; set; }
            public string FixInvalidItalicTagsExample { get; set; }
            public string RemoveUnneededSpacesExample { get; set; }
            public string RemoveUnneededPeriodsExample { get; set; }
            public string FixMissingSpacesExample { get; set; }
            public string FixUppercaseIInsideLowercaseWordsExample { get; set; }
            public string FixLowercaseIToUppercaseIExample { get; set; }
            public string StartTimeLaterThanEndTime { get; set; }
            public string UnableToFixStartTimeLaterThanEndTime { get; set; }
            public string XFixedToYZ { get; set; }
            public string UnableToFixTextXY { get; set; }
            public string UnneededSpace { get; set; }
            public string UnneededPeriod { get; set; }
            public string FixMissingSpace { get; set; }
            public string FixUppercaseIInsideLowercaseWord { get; set; }
            public string FixMissingPeriodAtEndOfLine { get; set; }
            public string RefreshFixes { get; set; }
            public string ApplyFixes { get; set; }
            public string AutoBreak { get; set; }
            public string Unbreak { get; set; }
            public string FixDoubleDash { get; set; }
            public string FixDoubleGreaterThan { get; set; }
            public string FixEllipsesStart { get; set; }
            public string FixMissingOpenBracket { get; set; }
            public string FixMusicNotation { get; set; }
            public string FixDoubleDashExample { get; set; }
            public string FixDoubleGreaterThanExample { get; set; }
            public string FixEllipsesStartExample { get; set; }
            public string FixMissingOpenBracketExample { get; set; }
            public string FixMusicNotationExample { get; set; }
            public string NumberOfImportantLogMessages { get; set; }
            public string FixedOkXY { get; set; }
            public string FixOcrErrorExample { get; set; }
            public string FixSpaceBetweenNumbersExample { get; set; }
            public string FixDialogsOneLineExample { get; set; }
            public string RemoveDialogFirstInNonDialogsExample { get; set; }
            public string SelectDefault { get; set; }
            public string SetDefault { get; set; }
            public string FixContinuationStyleX { get; set; }
            public string FixUnnecessaryLeadingDots { get; set; }
        }

        public class GenerateBlankVideo
        {
            public string Title { get; set; }
            public string CheckeredImage { get; set; }
            public string SolidColor { get; set; }
            public string DurationInMinutes { get; set; }
            public string Background { get; set; }
            public string FfmpegParameters { get; set; }
            public string GenerateWithFfmpegParametersPrompt { get; set; }
        }

        public class GenerateVideoWithBurnedInSubs
        {
            public string Title { get; set; }
            public string InfoAssaOff { get; set; }
            public string InfoAssaOn { get; set; }
            public string XGeneratedWithBurnedInSubsInX { get; set; }
            public string TimeRemainingMinutes { get; set; }
            public string TimeRemainingOneMinute { get; set; }
            public string TimeRemainingSeconds { get; set; }
            public string TimeRemainingAFewSeconds { get; set; }
            public string TimeRemainingMinutesAndSeconds { get; set; }
            public string TimeRemainingOneMinuteAndSeconds { get; set; }
            public string TargetFileName { get; set; }
            public string TargetFileSize { get; set; }
            public string FileSizeMb { get; set; }
            public string PassX { get; set; }
            public string Encoding { get; set; }
            public string BitRate { get; set; }
            public string TotalBitRateX { get; set; }
            public string SampleRate { get; set; }
            public string Audio { get; set; }
            public string Stereo { get; set; }
            public string Preset { get; set; }
            public string PixelFormat { get; set; }
            public string Crf { get; set; }
            public string TuneFor { get; set; }
            public string AlignRight { get; set; }
            public string GetStartPosition { get; set; }
            public string GetEndPosition { get; set; }
            public string UseSource { get; set; }
            public string UseSourceResolution { get; set; }
            public string OutputSettings { get; set; }
        }

        public class GenerateVideoWithEmbeddedSubs
        {
            public string Title { get; set; }
            public string InputVideoFile { get; set; }
            public string SubtitlesX { get; set; }
            public string SetLanguage { get; set; }
            public string LanguageAndTitle { get; set; }
            public string ToggleForced { get; set; }
            public string ToggleDefault { get; set; }
            public string Default { get; set; }
            public string XGeneratedWithEmbeddedSubs { get; set; }
            public string DeleteInputVideo { get; set; }
            public string OutputFileNameSettings { get; set; }
        }

        public class GetDictionaries
        {
            public string Title { get; set; }
            public string DescriptionLine1 { get; set; }
            public string DescriptionLine2 { get; set; }
            public string ChooseLanguageAndClickDownload { get; set; }
            public string OpenDictionariesFolder { get; set; }
            public string Download { get; set; }
            public string XDownloaded { get; set; }
        }

        public class GetTesseractDictionaries
        {
            public string Title { get; set; }
            public string DescriptionLine1 { get; set; }
            public string DownloadFailed { get; set; }
            public string GetDictionariesHere { get; set; }
            public string ChooseLanguageAndClickDownload { get; set; }
            public string OpenDictionariesFolder { get; set; }
            public string Download { get; set; }
            public string XDownloaded { get; set; }
        }

        public class GoogleTranslate
        {
            public string Title { get; set; }
            public string From { get; set; }
            public string To { get; set; }
            public string Translate { get; set; }
            public string PleaseWait { get; set; }
            public string PoweredByX { get; set; }
            public string LineMergeHandling { get; set; }
            public string ProcessorMergeNext { get; set; }
            public string ProcessorSentence { get; set; }
            public string ProcessorSingle { get; set; }
            public string AutoTranslateViaCopyPaste { get; set; }
            public string CopyPasteMaxSize { get; set; }
            public string AutoCopyToClipboard { get; set; }
            public string AutoCopyLineSeparator { get; set; }
            public string TranslateBlockXOfY { get; set; }
            public string TranslateBlockInfo { get; set; }
            public string TranslateBlockGetFromClipboard { get; set; }
            public string TranslateBlockCopySourceText { get; set; }
            public string TranslateBlockClipboardError1 { get; set; }
            public string TranslateBlockClipboardError2 { get; set; }
            public string StartWebServerX { get; set; }
            public string XRequiresALocalWebServer { get; set; }
            public string XRequiresAnApiKey { get; set; }
            public string ReadMore { get; set; }
            public string Formality { get; set; }
            public string TranslateCurrentLine { get; set; }
            public string ReTranslateCurrentLine { get; set; }
            public string MergeSplitStrategy { get; set; }
            public string Delay { get; set; }
            public string MaxBytes { get; set; }
            public string MaxMerges { get; set; }
            public string PromptX { get; set; }
            public string TranslateLinesSeparately { get; set; }
        }

        public class GoogleOrMicrosoftTranslate
        {
            public string Title { get; set; }
            public string From { get; set; }
            public string To { get; set; }
            public string Translate { get; set; }
            public string SourceText { get; set; }
            public string GoogleTranslate { get; set; }
            public string MicrosoftTranslate { get; set; }
        }

        public class GoToLine
        {
            public string Title { get; set; }
            public string XIsNotAValidNumber { get; set; }
        }

        public class ImportImages
        {
            public string Title { get; set; }
            public string ImageFiles { get; set; }
            public string Input { get; set; }
            public string InputDescription { get; set; }
            public string Remove { get; set; }
            public string RemoveAll { get; set; }
        }

        public class ImportShotChanges
        {
            public string Title { get; set; }
            public string OpenTextFile { get; set; }
            public string Generate { get; set; }
            public string Import { get; set; }
            public string TextFiles { get; set; }
            public string TimeCodes { get; set; }
            public string Frames { get; set; }
            public string Seconds { get; set; }
            public string Milliseconds { get; set; }
            public string GetShotChangesWithFfmpeg { get; set; }
            public string Sensitivity { get; set; }
            public string SensitivityDescription { get; set; }
            public string NoShotChangesFound { get; set; }
        }

        public class ImportText
        {
            public string Title { get; set; }
            public string OneSubtitleIsOneFile { get; set; }
            public string OpenTextFile { get; set; }
            public string OpenTextFiles { get; set; }
            public string ImportOptions { get; set; }
            public string Splitting { get; set; }
            public string AutoSplitText { get; set; }
            public string OneLineIsOneSubtitle { get; set; }
            public string TwoLinesAreOneSubtitle { get; set; }
            public string LineBreak { get; set; }
            public string SplitAtBlankLines { get; set; }
            public string MergeShortLines { get; set; }
            public string RemoveEmptyLines { get; set; }
            public string RemoveLinesWithoutLetters { get; set; }
            public string GenerateTimeCodes { get; set; }
            public string TakeTimeFromCurrentFile { get; set; }
            public string TakeTimeFromFileName { get; set; }
            public string GapBetweenSubtitles { get; set; }
            public string Auto { get; set; }
            public string Fixed { get; set; }
            public string Refresh { get; set; }
            public string TextFiles { get; set; }
            public string PreviewLinesModifiedX { get; set; }
            public string TimeCodes { get; set; }
            public string SplitAtEndChars { get; set; }
        }

        public class Interjections
        {
            public string Title { get; set; }
            public string EditSkipList { get; set; }
            public string EditSkipListInfo { get; set; }
        }

        public class JoinSubtitles
        {
            public string Title { get; set; }
            public string Information { get; set; }
            public string NumberOfLines { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string FileName { get; set; }
            public string Join { get; set; }
            public string TotalNumberOfLinesX { get; set; }
            public string AlreadyCorrectTimeCodes { get; set; }
            public string AppendTimeCodes { get; set; }
            public string AddMs { get; set; }
        }

        public class LanguageNames
        {
            public string NotSpecified { get; set; }
            public string UnknownCodeX { get; set; }
            public string aaName { get; set; }
            public string abName { get; set; }
            public string afName { get; set; }
            public string amName { get; set; }
            public string arName { get; set; }
            public string asName { get; set; }
            public string ayName { get; set; }
            public string azName { get; set; }
            public string baName { get; set; }
            public string beName { get; set; }
            public string bgName { get; set; }
            public string bhName { get; set; }
            public string biName { get; set; }
            public string bnName { get; set; }
            public string boName { get; set; }
            public string brName { get; set; }
            public string caName { get; set; }
            public string coName { get; set; }
            public string csName { get; set; }
            public string cyName { get; set; }
            public string daName { get; set; }
            public string deName { get; set; }
            public string dzName { get; set; }
            public string elName { get; set; }
            public string enName { get; set; }
            public string eoName { get; set; }
            public string esName { get; set; }
            public string etName { get; set; }
            public string euName { get; set; }
            public string faName { get; set; }
            public string fiName { get; set; }
            public string fjName { get; set; }
            public string foName { get; set; }
            public string frName { get; set; }
            public string fyName { get; set; }
            public string gaName { get; set; }
            public string gdName { get; set; }
            public string glName { get; set; }
            public string gnName { get; set; }
            public string guName { get; set; }
            public string haName { get; set; }
            public string heName { get; set; }
            public string hiName { get; set; }
            public string hrName { get; set; }
            public string huName { get; set; }
            public string hyName { get; set; }
            public string iaName { get; set; }
            public string idName { get; set; }
            public string ieName { get; set; }
            public string ikName { get; set; }
            public string isName { get; set; }
            public string itName { get; set; }
            public string iuName { get; set; }
            public string jaName { get; set; }
            public string jvName { get; set; }
            public string kaName { get; set; }
            public string kkName { get; set; }
            public string klName { get; set; }
            public string kmName { get; set; }
            public string knName { get; set; }
            public string koName { get; set; }
            public string ksName { get; set; }
            public string kuName { get; set; }
            public string kyName { get; set; }
            public string laName { get; set; }
            public string lbName { get; set; }
            public string lnName { get; set; }
            public string loName { get; set; }
            public string ltName { get; set; }
            public string lvName { get; set; }
            public string mgName { get; set; }
            public string miName { get; set; }
            public string mkName { get; set; }
            public string mlName { get; set; }
            public string mnName { get; set; }
            public string moName { get; set; }
            public string mrName { get; set; }
            public string msName { get; set; }
            public string mtName { get; set; }
            public string myName { get; set; }
            public string naName { get; set; }
            public string neName { get; set; }
            public string nlName { get; set; }
            public string noName { get; set; }
            public string ocName { get; set; }
            public string omName { get; set; }
            public string orName { get; set; }
            public string paName { get; set; }
            public string plName { get; set; }
            public string psName { get; set; }
            public string ptName { get; set; }
            public string quName { get; set; }
            public string rmName { get; set; }
            public string rnName { get; set; }
            public string roName { get; set; }
            public string ruName { get; set; }
            public string rwName { get; set; }
            public string saName { get; set; }
            public string sdName { get; set; }
            public string sgName { get; set; }
            public string shName { get; set; }
            public string siName { get; set; }
            public string skName { get; set; }
            public string slName { get; set; }
            public string smName { get; set; }
            public string snName { get; set; }
            public string soName { get; set; }
            public string sqName { get; set; }
            public string srName { get; set; }
            public string ssName { get; set; }
            public string stName { get; set; }
            public string suName { get; set; }
            public string svName { get; set; }
            public string swName { get; set; }
            public string taName { get; set; }
            public string teName { get; set; }
            public string tgName { get; set; }
            public string thName { get; set; }
            public string tiName { get; set; }
            public string tkName { get; set; }
            public string tlName { get; set; }
            public string tnName { get; set; }
            public string toName { get; set; }
            public string trName { get; set; }
            public string tsName { get; set; }
            public string ttName { get; set; }
            public string twName { get; set; }
            public string ugName { get; set; }
            public string ukName { get; set; }
            public string urName { get; set; }
            public string uzName { get; set; }
            public string viName { get; set; }
            public string voName { get; set; }
            public string woName { get; set; }
            public string xhName { get; set; }
            public string yiName { get; set; }
            public string yoName { get; set; }
            public string zaName { get; set; }
            public string zhName { get; set; }
            public string zuName { get; set; }
        }

        public class Main
        {
            public MainMenu Menu { get; set; }
            public MainControls Controls { get; set; }
            public MainVideoControls VideoControls { get; set; }
            public string SaveChangesToUntitled { get; set; }
            public string SaveChangesToX { get; set; }
            public string SaveChangesToUntitledOriginal { get; set; }
            public string SaveChangesToOriginalX { get; set; }
            public string SaveSubtitleAs { get; set; }
            public string SaveOriginalSubtitleAs { get; set; }
            public string CannotSaveEmptySubtitle { get; set; }
            public string NoSubtitleLoaded { get; set; }
            public string VisualSyncSelectedLines { get; set; }
            public string VisualSyncTitle { get; set; }
            public string BeforeVisualSync { get; set; }
            public string VisualSyncPerformedOnSelectedLines { get; set; }
            public string VisualSyncPerformed { get; set; }
            public string FileXIsLargerThan10MB { get; set; }
            public string ContinueAnyway { get; set; }
            public string BeforeLoadOf { get; set; }
            public string LoadedSubtitleX { get; set; }
            public string LoadedEmptyOrShort { get; set; }
            public string FileIsEmptyOrShort { get; set; }
            public string FileNotFound { get; set; }
            public string FileLocked { get; set; }
            public string SavedSubtitleX { get; set; }
            public string SavedOriginalSubtitleX { get; set; }
            public string FileOnDiskModified { get; set; }
            public string OverwriteModifiedFile { get; set; }
            public string FileXIsReadOnly { get; set; }
            public string UnableToSaveSubtitleX { get; set; }
            public string FormatXShouldUseUft8 { get; set; }
            public string BeforeNew { get; set; }
            public string New { get; set; }
            public string BeforeConvertingToX { get; set; }
            public string ConvertedToX { get; set; }
            public string BeforeShowEarlier { get; set; }
            public string BeforeShowLater { get; set; }
            public string LineNumberX { get; set; }
            public string OpenVideoFile { get; set; }
            public string NewFrameRateUsedToCalculateTimeCodes { get; set; }
            public string NewFrameRateUsedToCalculateFrameNumbers { get; set; }
            public string FindContinue { get; set; }
            public string FindContinueTitle { get; set; }
            public string ReplaceContinueNotFound { get; set; }
            public string ReplaceXContinue { get; set; }
            public string ReplaceContinueTitle { get; set; }
            public string SearchingForXFromLineY { get; set; }
            public string XFoundAtLineNumberY { get; set; }
            public string XNotFound { get; set; }
            public string BeforeReplace { get; set; }
            public string MatchFoundX { get; set; }
            public string NoMatchFoundX { get; set; }
            public string FoundNothingToReplace { get; set; }
            public string ReplaceCountX { get; set; }
            public string NoXFoundAtLineY { get; set; }
            public string OneReplacementMade { get; set; }
            public string BeforeChangesMadeInSourceView { get; set; }
            public string UnableToParseSourceView { get; set; }
            public string GoToLineNumberX { get; set; }
            public string CreateAdjustChangesApplied { get; set; }
            public string SelectedLines { get; set; }
            public string BeforeDisplayTimeAdjustment { get; set; }
            public string DisplayTimeAdjustedX { get; set; }
            public string DisplayTimesAdjustedX { get; set; }
            public string StartTimeAdjustedX { get; set; }
            public string BeforeCommonErrorFixes { get; set; }
            public string CommonErrorsFixedInSelectedLines { get; set; }
            public string CommonErrorsFixed { get; set; }
            public string BeforeRenumbering { get; set; }
            public string RenumberedStartingFromX { get; set; }
            public string BeforeBeautifyTimeCodes { get; set; }
            public string BeforeBeautifyTimeCodesSelectedLines { get; set; }
            public string BeautifiedTimeCodes { get; set; }
            public string BeautifiedTimeCodesSelectedLines { get; set; }
            public string BeforeRemovalOfTextingForHearingImpaired { get; set; }
            public string TextingForHearingImpairedRemovedOneLine { get; set; }
            public string TextingForHearingImpairedRemovedXLines { get; set; }
            public string SubtitleSplitted { get; set; }
            public string SubtitleAppendPrompt { get; set; }
            public string SubtitleAppendPromptTitle { get; set; }
            public string OpenSubtitleToAppend { get; set; }
            public string AppendViaVisualSyncTitle { get; set; }
            public string AppendSynchronizedSubtitlePrompt { get; set; }
            public string BeforeAppend { get; set; }
            public string SubtitleAppendedX { get; set; }
            public string SubtitleNotAppended { get; set; }
            public string GoogleTranslate { get; set; }
            public string MicrosoftTranslate { get; set; }
            public string BeforeGoogleTranslation { get; set; }
            public string SelectedLinesTranslated { get; set; }
            public string SubtitleTranslated { get; set; }
            public string TranslateSwedishToDanish { get; set; }
            public string TranslateSwedishToDanishWarning { get; set; }
            public string TranslatingViaNikseDkMt { get; set; }
            public string BeforeSwedishToDanishTranslation { get; set; }
            public string TranslationFromSwedishToDanishComplete { get; set; }
            public string TranslationFromSwedishToDanishFailed { get; set; }
            public string UndoPerformed { get; set; }
            public string RedoPerformed { get; set; }
            public string NothingToUndo { get; set; }
            public string InvalidLanguageNameX { get; set; }
            public string DoNotDisplayMessageAgain { get; set; }
            public string DoNotAutoLoadVideo { get; set; }
            public string NumberOfCorrectedWords { get; set; }
            public string NumberOfSkippedWords { get; set; }
            public string NumberOfCorrectWords { get; set; }
            public string NumberOfWordsAddedToDictionary { get; set; }
            public string NumberOfNameHits { get; set; }
            public string SpellCheck { get; set; }
            public string BeforeSpellCheck { get; set; }
            public string SpellCheckChangedXToY { get; set; }
            public string BeforeAddingTagX { get; set; }
            public string TagXAdded { get; set; }
            public string LineXOfY { get; set; }
            public string XLinesSavedAsY { get; set; }
            public string XLinesDeleted { get; set; }
            public string BeforeDeletingXLines { get; set; }
            public string DeleteXLinesPrompt { get; set; }
            public string OneLineDeleted { get; set; }
            public string BeforeDeletingOneLine { get; set; }
            public string DeleteOneLinePrompt { get; set; }
            public string BeforeInsertLine { get; set; }
            public string LineInserted { get; set; }
            public string BeforeLineUpdatedInListView { get; set; }
            public string BeforeSplitLine { get; set; }
            public string LineSplitted { get; set; }
            public string BeforeMergeLines { get; set; }
            public string LinesMerged { get; set; }
            public string MergeSentences { get; set; }
            public string MergeSentencesXLines { get; set; }
            public string BeforeSettingColor { get; set; }
            public string BeforeSettingFontName { get; set; }
            public string BeforeTypeWriterEffect { get; set; }
            public string BeforeKaraokeEffect { get; set; }
            public string BeforeImportingDvdSubtitle { get; set; }
            public string OpenSubtitleVideoFile { get; set; }
            public string VideoFiles { get; set; }
            public string NoSubtitlesFound { get; set; }
            public string NotAValidMatroskaFileX { get; set; }
            public string BlurayNotSubtitlesFound { get; set; }
            public string ImportingChapters { get; set; }
            public string XChaptersImported { get; set; }
            public string ParsingMatroskaFile { get; set; }
            public string ParsingTransportStreamFile { get; set; }
            public string BeforeImportFromMatroskaFile { get; set; }
            public string SubtitleImportedFromMatroskaFile { get; set; }
            public string DropFileXNotAccepted { get; set; }
            public string DropSubtitleFileXNotAccepted { get; set; }
            public string DropOnlyOneFile { get; set; }
            public string OpenAnsiSubtitle { get; set; }
            public string BeforeChangeCasing { get; set; }
            public string CasingCompleteMessageNoNames { get; set; }
            public string CasingCompleteMessageOnlyNames { get; set; }
            public string CasingCompleteMessage { get; set; }
            public string BeforeChangeFrameRate { get; set; }
            public string BeforeAdjustSpeedInPercent { get; set; }
            public string FrameRateChangedFromXToY { get; set; }
            public string IdxFileNotFoundWarning { get; set; }
            public string InvalidVobSubHeader { get; set; }
            public string OpenVobSubFile { get; set; }
            public string VobSubFiles { get; set; }
            public string OpenBluRaySupFile { get; set; }
            public string BluRaySupFiles { get; set; }
            public string BeforeImportingVobSubFile { get; set; }
            public string BeforeImportingBluRaySupFile { get; set; }
            public string BeforeImportingBdnXml { get; set; }
            public string BeforeShowSelectedLinesEarlierLater { get; set; }
            public string ShowAllLinesXSecondsLinesEarlier { get; set; }
            public string ShowAllLinesXSecondsLinesLater { get; set; }
            public string ShowSelectedLinesXSecondsLinesEarlier { get; set; }
            public string ShowSelectedLinesXSecondsLinesLater { get; set; }
            public string ShowSelectionAndForwardXSecondsLinesEarlier { get; set; }
            public string ShowSelectionAndForwardXSecondsLinesLater { get; set; }
            public string DoubleWordsViaRegEx { get; set; }
            public string BeforeSortX { get; set; }
            public string SortedByX { get; set; }
            public string BeforeAutoBalanceSelectedLines { get; set; }
            public string NumberOfLinesAutoBalancedX { get; set; }
            public string BeforeEvenlyDistributeSelectedLines { get; set; }
            public string NumberOfLinesEvenlyDistributedX { get; set; }
            public string BeforeRemoveLineBreaksInSelectedLines { get; set; }
            public string NumberOfWithRemovedLineBreakX { get; set; }
            public string BeforeMultipleReplace { get; set; }
            public string NumberOfLinesReplacedX { get; set; }
            public string NameXAddedToNameList { get; set; }
            public string NameXNotAddedToNameList { get; set; }
            public string WordXAddedToUserDic { get; set; }
            public string WordXNotAddedToUserDic { get; set; }
            public string OcrReplacePairXAdded { get; set; }
            public string OcrReplacePairXNotAdded { get; set; }
            public string XLinesSelected { get; set; }
            public string UnicodeMusicSymbolsAnsiWarning { get; set; }
            public string NegativeTimeWarning { get; set; }
            public string BeforeMergeShortLines { get; set; }
            public string MergedShortLinesX { get; set; }
            public string BeforeSplitLongLines { get; set; }
            public string LongLinesSplitX { get; set; }
            public string BeforeDurationsBridgeGap { get; set; }
            public string BeforeSetMinimumDisplayTimeBetweenParagraphs { get; set; }
            public string XMinimumDisplayTimeBetweenParagraphsChanged { get; set; }
            public string BeforeImportText { get; set; }
            public string TextImported { get; set; }
            public string BeforePointSynchronization { get; set; }
            public string PointSynchronizationDone { get; set; }
            public string BeforeTimeCodeImport { get; set; }
            public string TimeCodeImportedFromXY { get; set; }
            public string BeforeInsertSubtitleAtVideoPosition { get; set; }
            public string BeforeSetStartTimeAndOffsetTheRest { get; set; }
            public string BeforeSetEndTimeAndOffsetTheRest { get; set; }
            public string BeforeSetEndAndVideoPosition { get; set; }
            public string ContinueWithCurrentSpellCheck { get; set; }
            public string CharactersPerSecond { get; set; }
            public string GetFrameRateFromVideoFile { get; set; }
            public string NetworkMessage { get; set; }
            public string NetworkUpdate { get; set; }
            public string NetworkInsert { get; set; }
            public string NetworkDelete { get; set; }
            public string NetworkNewUser { get; set; }
            public string NetworkByeUser { get; set; }
            public string NetworkUnableToConnectToServer { get; set; }
            public string UserAndAction { get; set; }
            public string NetworkMode { get; set; }
            public string XStartedSessionYAtZ { get; set; }
            public string OpenOtherSubtitle { get; set; }
            public string BeforeToggleDialogDashes { get; set; }
            public string ExportPlainTextAs { get; set; }
            public string TextFiles { get; set; }
            public string SubtitleExported { get; set; }
            public string LineNumberXErrorReadingFromSourceLineY { get; set; }
            public string LineNumberXErrorReadingTimeCodeFromSourceLineY { get; set; }
            public string LineNumberXExpectedNumberFromSourceLineY { get; set; }
            public string LineNumberXExpectedEmptyLine { get; set; }
            public string BeforeGuessingTimeCodes { get; set; }
            public string BeforeAutoDuration { get; set; }
            public string BeforeColumnPaste { get; set; }
            public string BeforeColumnDelete { get; set; }
            public string BeforeColumnImportText { get; set; }
            public string BeforeColumnShiftCellsDown { get; set; }
            public string BeforeX { get; set; }
            public string LinesUpdatedX { get; set; }
            public string ErrorLoadingPluginXErrorY { get; set; }
            public string BeforeRunningPluginXVersionY { get; set; }
            public string UnableToReadPluginResult { get; set; }
            public string UnableToCreateBackupDirectory { get; set; }
            public string BeforeDisplaySubtitleJoin { get; set; }
            public string SubtitlesJoined { get; set; }
            public string StatusLog { get; set; }
            public string XShotChangesImported { get; set; }
            public string PluginXExecuted { get; set; }
            public string NotAValidXSubFile { get; set; }
            public string BeforeMergeLinesWithSameText { get; set; }
            public string ImportTimeCodesDifferentNumberOfLinesWarning { get; set; }
            public string ParsingTransportStream { get; set; }
            public string XPercentCompleted { get; set; }
            public string NextX { get; set; }
            public string PromptInsertSubtitleOverlap { get; set; }
            public string SubtitleContainsNegativeDurationsX { get; set; }
            public string SetPlayRateX { get; set; }
            public string ErrorLoadIdx { get; set; }
            public string ErrorLoadRar { get; set; }
            public string ErrorLoadZip { get; set; }
            public string ErrorLoad7Zip { get; set; }
            public string ErrorLoadPng { get; set; }
            public string ErrorLoadJpg { get; set; }
            public string ErrorLoadSrr { get; set; }
            public string ErrorLoadTorrent { get; set; }
            public string ErrorLoadBinaryZeroes { get; set; }
            public string ErrorDirectoryDropNotAllowed { get; set; }
            public string NoSupportEncryptedVobSub { get; set; }
            public string NoSupportHereBluRaySup { get; set; }
            public string NoSupportHereDvdSup { get; set; }
            public string NoSupportHereVobSub { get; set; }
            public string NoSupportHereDivx { get; set; }
            public string NoChapters { get; set; }
            public string VideoFromUrlRequirements { get; set; }
            public string Url { get; set; }
            public string Errors { get; set; }
            public string ShowVideoControls { get; set; }
            public string HideVideoControls { get; set; }
            public string GeneratingWaveformInBackground { get; set; }
            public string AutoBackupSaved { get; set; }
            public string UsingOnlyFrontCenterChannel { get; set; }
            public string BeforeConvertingColorsToDialog { get; set; }
            public string ConvertedColorsToDialog { get; set; }
            public string PleaseInstallVideoPlayer { get; set; }
            public string UnableToPlayMediaFile { get; set; }
            public string SubtitleEditNeedsVideoPlayer { get; set; }
            public string UseRecommendMpv { get; set; }
            public string DownloadAndUseMpv { get; set; }
            public string ChooseLayout { get; set; }

            public class MainMenu
            {
                public class FileMenu
                {
                    public string Title { get; set; }
                    public string New { get; set; }
                    public string Open { get; set; }
                    public string OpenKeepVideo { get; set; }
                    public string Reopen { get; set; }
                    public string Save { get; set; }
                    public string SaveAs { get; set; }
                    public string RestoreAutoBackup { get; set; }
                    public string FormatXProperties { get; set; }
                    public string OpenOriginal { get; set; }
                    public string SaveOriginal { get; set; }
                    public string CloseOriginal { get; set; }
                    public string CloseTranslation { get; set; }
                    public string OpenContainingFolder { get; set; }
                    public string Compare { get; set; }
                    public string VerifyCompleteness { get; set; }
                    public string Statistics { get; set; }
                    public string Plugins { get; set; }
                    public string ImportSubtitleFromVideoFile { get; set; }
                    public string ImportOcrFromDvd { get; set; }
                    public string ImportOcrVobSubSubtitle { get; set; }
                    public string ImportBluRaySupFile { get; set; }
                    public string ImportBluRaySupFileEdit { get; set; }
                    public string ImportSubtitleWithManualChosenEncoding { get; set; }
                    public string ImportText { get; set; }
                    public string ImportImages { get; set; }
                    public string ImportTimecodes { get; set; }
                    public string Import { get; set; }
                    public string Export { get; set; }
                    public string ExportBdnXml { get; set; }
                    public string ExportBluRaySup { get; set; }
                    public string ExportVobSub { get; set; }
                    public string ExportCavena890 { get; set; }
                    public string ExportEbu { get; set; }
                    public string ExportPac { get; set; }
                    public string ExportPlainText { get; set; }
                    public string ExportAdobeEncoreFabImageScript { get; set; }
                    public string ExportKoreanAtsFilePair { get; set; }
                    public string ExportAvidStl { get; set; }
                    public string ExportDvdStudioProStl { get; set; }
                    public string ExportCapMakerPlus { get; set; }
                    public string ExportCaptionsInc { get; set; }
                    public string ExportCheetahCap { get; set; }
                    public string ExportUltech130 { get; set; }
                    public string ExportCustomTextFormat { get; set; }
                    public string Exit { get; set; }
                }
                public class EditMenu
                {
                    public string Title { get; set; }
                    public string Undo { get; set; }
                    public string Redo { get; set; }
                    public string ShowUndoHistory { get; set; }
                    public string InsertUnicodeSymbol { get; set; }
                    public string InsertUnicodeControlCharacters { get; set; }
                    public string InsertUnicodeControlCharactersLRM { get; set; }
                    public string InsertUnicodeControlCharactersRLM { get; set; }
                    public string InsertUnicodeControlCharactersLRE { get; set; }
                    public string InsertUnicodeControlCharactersRLE { get; set; }
                    public string InsertUnicodeControlCharactersLRO { get; set; }
                    public string InsertUnicodeControlCharactersRLO { get; set; }
                    public string Find { get; set; }
                    public string FindNext { get; set; }
                    public string Replace { get; set; }
                    public string MultipleReplace { get; set; }
                    public string GoToSubtitleNumber { get; set; }
                    public string RightToLeftMode { get; set; }
                    public string FixRtlViaUnicodeControlCharacters { get; set; }
                    public string RemoveUnicodeControlCharacters { get; set; }
                    public string ReverseRightToLeftStartEnd { get; set; }
                    public string ShowOriginalTextInAudioAndVideoPreview { get; set; }
                    public string ModifySelection { get; set; }
                    public string InverseSelection { get; set; }
                }
                public class ToolsMenu
                {
                    public string Title { get; set; }
                    public string AdjustDisplayDuration { get; set; }
                    public string ApplyDurationLimits { get; set; }
                    public string SubtitlesBridgeGaps { get; set; }
                    public string FixCommonErrors { get; set; }
                    public string StartNumberingFrom { get; set; }
                    public string RemoveTextForHearingImpaired { get; set; }
                    public string ConvertColorsToDialog { get; set; }
                    public string ChangeCasing { get; set; }
                    public string ChangeFrameRate { get; set; }
                    public string ChangeSpeedInPercent { get; set; }
                    public string MergeShortLines { get; set; }
                    public string MergeDuplicateText { get; set; }
                    public string MergeSameTimeCodes { get; set; }
                    public string SplitLongLines { get; set; }
                    public string MinimumDisplayTimeBetweenParagraphs { get; set; }
                    public string SortBy { get; set; }
                    public string Number { get; set; }
                    public string StartTime { get; set; }
                    public string EndTime { get; set; }
                    public string Duration { get; set; }
                    public string ListErrors { get; set; }
                    public string NetflixQualityCheck { get; set; }
                    public string BeautifyTimeCodes { get; set; }
                    public string TextAlphabetically { get; set; }
                    public string TextSingleLineMaximumLength { get; set; }
                    public string TextTotalLength { get; set; }
                    public string TextNumberOfLines { get; set; }
                    public string TextNumberOfCharactersPerSeconds { get; set; }
                    public string WordsPerMinute { get; set; }
                    public string Style { get; set; }
                    public string Ascending { get; set; }
                    public string Descending { get; set; }
                    public string MakeNewEmptyTranslationFromCurrentSubtitle { get; set; }
                    public string BatchConvert { get; set; }
                    public string GenerateTimeAsText { get; set; }
                    public string MeasurementConverter { get; set; }
                    public string SplitSubtitle { get; set; }
                    public string AppendSubtitle { get; set; }
                    public string JoinSubtitles { get; set; }
                }
                public class VideoMenu
                {
                    public string Title { get; set; }
                    public string OpenVideo { get; set; }
                    public string OpenVideoFromUrl { get; set; }
                    public string OpenDvd { get; set; }
                    public string ChooseAudioTrack { get; set; }
                    public string CloseVideo { get; set; }
                    public string OpenSecondSubtitle { get; set; }
                    public string SetVideoOffset { get; set; }
                    public string SmptTimeMode { get; set; }
                    public string GenerateTextFromVideo { get; set; }
                    public string GenerateBlankVideo { get; set; }
                    public string GenerateVideoWithBurnedInSub { get; set; }
                    public string GenerateVideoWithEmbeddedSubs { get; set; }
                    public string GenerateTransparentVideoWithSubs { get; set; }
                    public string VideoAudioToTextX { get; set; }
                    public string ImportChaptersFromVideo { get; set; }
                    public string GenerateImportShotChanges { get; set; }
                    public string RemoveOrExportShotChanges { get; set; }
                    public string WaveformBatchGenerate { get; set; }
                    public string ShowHideWaveformAndSpectrogram { get; set; }
                    public string TextToSpeechAndAddToVideo { get; set; }
                    public string UnDockVideoControls { get; set; }
                    public string ReDockVideoControls { get; set; }
                }
                public class SpellCheckMenu
                {
                    public string Title { get; set; }
                    public string SpellCheck { get; set; }
                    public string SpellCheckFromCurrentLine { get; set; }
                    public string FindDoubleWords { get; set; }
                    public string FindDoubleLines { get; set; }
                    public string GetDictionaries { get; set; }
                    public string AddToNameList { get; set; }
                }
                public class SynchronizationkMenu
                {
                    public string Title { get; set; }
                    public string AdjustAllTimes { get; set; }
                    public string VisualSync { get; set; }
                    public string PointSync { get; set; }
                    public string PointSyncViaOtherSubtitle { get; set; }
                }
                public class AutoTranslateMenu
                {
                    public string Title { get; set; }
                    public string AutoTranslate { get; set; }
                    public string AutoTranslateViaCopyPaste { get; set; }
                }
                public class OptionsMenu
                {
                    public string Title { get; set; }
                    public string Settings { get; set; }
                    public string WordLists { get; set; }
                    public string ChooseLanguage { get; set; }
                }

                public class NetworkingMenu
                {
                    public string Title { get; set; }
                    public string StartNewSession { get; set; }
                    public string JoinSession { get; set; }
                    public string ShowSessionInfoAndLog { get; set; }
                    public string Chat { get; set; }
                    public string LeaveSession { get; set; }
                }

                public class HelpMenu
                {
                    public string CheckForUpdates { get; set; }
                    public string Title { get; set; }
                    public string Help { get; set; }
                    public string About { get; set; }
                }

                public class ToolBarMenu
                {
                    public string New { get; set; }
                    public string Open { get; set; }
                    public string Save { get; set; }
                    public string SaveAs { get; set; }
                    public string Find { get; set; }
                    public string Replace { get; set; }
                    public string FixCommonErrors { get; set; }
                    public string RemoveTextForHi { get; set; }
                    public string VisualSync { get; set; }
                    public string SpellCheck { get; set; }
                    public string NetflixQualityCheck { get; set; }
                    public string BeautifyTimeCodes { get; set; }
                    public string Settings { get; set; }
                    public string Help { get; set; }
                    public string Layout { get; set; }
                    public string AssaDraw { get; set; }
                }

                public class ListViewContextMenu
                {
                    public string SizeAllColumnsToFit { get; set; }
                    public string SetStyle { get; set; }
                    public string SetActor { get; set; }
                    public string SetLayer { get; set; }
                    public string AssaTools { get; set; }
                    public string SubStationAlphaStyles { get; set; }
                    public string AdvancedSubStationAlphaStyles { get; set; }
                    public string TimedTextSetRegion { get; set; }
                    public string TimedTextSetStyle { get; set; }
                    public string TimedTextStyles { get; set; }
                    public string TimedTextSetLanguage { get; set; }
                    public string SamiSetStyle { get; set; }
                    public string NuendoSetStyle { get; set; }
                    public string WebVttSetStyle { get; set; }
                    public string WebVttSetVoice { get; set; }
                    public string WebVttBrowserPreview { get; set; }
                    public string Cut { get; set; }
                    public string Copy { get; set; }
                    public string Paste { get; set; }
                    public string Delete { get; set; }
                    public string SplitLineAtCursorPosition { get; set; }
                    public string SplitLineAtCursorPositionAndAutoBr { get; set; }
                    public string SplitLineAtCursorAndWaveformPosition { get; set; }
                    public string AutoDurationCurrentLine { get; set; }
                    public string SelectAll { get; set; }
                    public string InsertFirstLine { get; set; }
                    public string InsertBefore { get; set; }
                    public string InsertAfter { get; set; }
                    public string InsertSubtitleAfter { get; set; }
                    public string CopyToClipboard { get; set; }
                    public string Column { get; set; }
                    public string ColumnDeleteText { get; set; }
                    public string ColumnDeleteTextAndShiftCellsUp { get; set; }
                    public string ColumnInsertEmptyTextAndShiftCellsDown { get; set; }
                    public string ColumnInsertTextFromSubtitle { get; set; }
                    public string ColumnImportTextAndShiftCellsDown { get; set; }
                    public string ColumnPasteFromClipboard { get; set; }
                    public string ColumnTextUp { get; set; }
                    public string ColumnTextDown { get; set; }
                    public string ColumnCopyOriginalTextToCurrent { get; set; }
                    public string OcrSelectedLines { get; set; }
                    public string Split { get; set; }
                    public string MergeSelectedLines { get; set; }
                    public string MergeSelectedLinesAsDialog { get; set; }
                    public string MergeWithLineBefore { get; set; }
                    public string MergeWithLineAfter { get; set; }
                    public string ExtendToLineBefore { get; set; }
                    public string ExtendToLineAfter { get; set; }
                    public string RemoveFormatting { get; set; }
                    public string RemoveFormattingAll { get; set; }
                    public string RemoveFormattingBold { get; set; }
                    public string RemoveFormattingItalic { get; set; }
                    public string RemoveFormattingUnderline { get; set; }
                    public string RemoveFormattingColor { get; set; }
                    public string RemoveFormattingFontName { get; set; }
                    public string RemoveFormattingAlignment { get; set; }
                    public string Underline { get; set; }
                    public string Box { get; set; }
                    public string Color { get; set; }
                    public string FontName { get; set; }
                    public string Superscript { get; set; }
                    public string Subscript { get; set; }
                    public string Alignment { get; set; }
                    public string AutoBalanceSelectedLines { get; set; }
                    public string EvenlyDistributeSelectedLines { get; set; }
                    public string RemoveLineBreaksFromSelectedLines { get; set; }
                    public string TypewriterEffect { get; set; }
                    public string KaraokeEffect { get; set; }
                    public string ShowSelectedLinesEarlierLater { get; set; }
                    public string VisualSyncSelectedLines { get; set; }
                    public string BeautifyTimeCodesOfSelectedLines { get; set; }
                    public string GoogleAndMicrosoftTranslateSelectedLine { get; set; }
                    public string SelectedLines { get; set; }
                    public string TranslateSelectedLines { get; set; }
                    public string AdjustDisplayDurationForSelectedLines { get; set; }
                    public string ApplyDurationLimitsForSelectedLines { get; set; }
                    public string ApplyCustomOverrideTag { get; set; }
                    public string SetPosition { get; set; }
                    public string GenerateProgressBar { get; set; }
                    public string AssaResolutionChanger { get; set; }
                    public string AssaGenerateBackgroundBox { get; set; }
                    public string ImageColorPicker { get; set; }
                    public string FixCommonErrorsInSelectedLines { get; set; }
                    public string ChangeCasingForSelectedLines { get; set; }
                    public string SaveSelectedLines { get; set; }
                    public string WebVTTSetNewVoice { get; set; }
                    public string WebVTTRemoveVoices { get; set; }
                    public string NewActor { get; set; }
                    public string RemoveActors { get; set; }
                    public string EditBookmark { get; set; }
                    public string RemoveBookmark { get; set; }
                    public string GoToSourceView { get; set; }
                    public string GoToListView { get; set; }
                    public string ExtractAudio { get; set; }
                    public string MediaInfo { get; set; }
                }

                public FileMenu File { get; set; }
                public EditMenu Edit { get; set; }
                public ToolsMenu Tools { get; set; }
                public VideoMenu Video { get; set; }
                public SpellCheckMenu SpellCheck { get; set; }
                public SynchronizationkMenu Synchronization { get; set; }
                public AutoTranslateMenu AutoTranslate { get; set; }
                public OptionsMenu Options { get; set; }
                public NetworkingMenu Networking { get; set; }
                public HelpMenu Help { get; set; }
                public ToolBarMenu ToolBar { get; set; }
                public ListViewContextMenu ContextMenu { get; set; }
            }

            public class MainControls
            {
                public string SubtitleFormat { get; set; }
                public string FileEncoding { get; set; }
                public string UndoChangesInEditPanel { get; set; }
                public string Previous { get; set; }
                public string Next { get; set; }
                public string AutoBreak { get; set; }
                public string Unbreak { get; set; }
            }

            public class MainVideoControls
            {
                public string Translate { get; set; }
                public string CreateAndAdjust { get; set; }
                public string Create { get; set; }
                public string Adjust { get; set; }
                public string SelectCurrentElementWhilePlaying { get; set; }

                //translation helper
                public string AutoRepeat { get; set; }

                public string AutoRepeatOn { get; set; }
                public string AutoRepeatCount { get; set; }
                public string AutoContinue { get; set; }
                public string AutoContinueOn { get; set; }
                public string DelayInSeconds { get; set; }
                public string OriginalText { get; set; }
                public string Previous { get; set; }
                public string Stop { get; set; }
                public string PlayCurrent { get; set; }
                public string Next { get; set; }
                public string Playing { get; set; }
                public string RepeatingLastTime { get; set; }
                public string RepeatingXTimesLeft { get; set; }
                public string AutoContinueInOneSecond { get; set; }
                public string AutoContinueInXSeconds { get; set; }
                public string StillTypingAutoContinueStopped { get; set; }

                // create/adjust
                public string InsertNewSubtitleAtVideoPosition { get; set; }
                public string InsertNewSubtitleAtVideoPositionNoTextBoxFocus { get; set; }
                public string InsertNewSubtitleAtVideoPositionMax { get; set; }

                public string Auto { get; set; }
                public string PlayFromJustBeforeText { get; set; }
                public string PlayFromBeginning { get; set; }
                public string Pause { get; set; }
                public string GoToSubtitlePositionAndPause { get; set; }
                public string SetStartTime { get; set; }
                public string SetEndTimeAndGoToNext { get; set; }
                public string AdjustedViaEndTime { get; set; }
                public string SetEndTime { get; set; }
                public string SetStartTimeAndOffsetTheRest { get; set; }

                public string SearchTextOnline { get; set; }
                public string GoogleTranslate { get; set; }
                public string AutoTranslate { get; set; }
                public string GoogleIt { get; set; }
                public string SecondsBackShort { get; set; }
                public string SecondsForwardShort { get; set; }
                public string VideoPosition { get; set; }
                public string TranslateTip { get; set; }

                public string BeforeChangingTimeInWaveformX { get; set; }
                public string NewTextInsertAtX { get; set; }

                public string Center { get; set; }
                public string PlayRate { get; set; }
            }
        }

        public class MatroskaSubtitleChooser
        {
            public string Title { get; set; }
            public string TitleMp4 { get; set; }
            public string PleaseChoose { get; set; }
            public string TrackXLanguageYTypeZ { get; set; }
        }

        public class MeasurementConverter
        {
            public string Title { get; set; }
            public string ConvertFrom { get; set; }
            public string ConvertTo { get; set; }
            public string CopyToClipboard { get; set; }
            public string CloseOnInsert { get; set; }
            public string Insert { get; set; }
            public string Length { get; set; }
            public string Mass { get; set; }
            public string Volume { get; set; }
            public string Area { get; set; }
            public string Time { get; set; }
            public string Temperature { get; set; }
            public string Velocity { get; set; }
            public string Force { get; set; }
            public string Energy { get; set; }
            public string Power { get; set; }
            public string Pressure { get; set; }
            public string Kilometers { get; set; }
            public string Meters { get; set; }
            public string Centimeters { get; set; }
            public string Millimeters { get; set; }
            public string Micrometers { get; set; }
            public string Nanometers { get; set; }
            public string Angstroms { get; set; }
            public string MilesTerrestial { get; set; }
            public string MilesNautical { get; set; }
            public string Yards { get; set; }
            public string Feet { get; set; }
            public string Inches { get; set; }
            public string Chains { get; set; }
            public string Fathoms { get; set; }
            public string Hands { get; set; }
            public string Rods { get; set; }
            public string Spans { get; set; }
            public string LongTonnes { get; set; }
            public string ShortTonnes { get; set; }
            public string Tonnes { get; set; }
            public string Kilos { get; set; }
            public string Grams { get; set; }
            public string Milligrams { get; set; }
            public string Micrograms { get; set; }
            public string Pounds { get; set; }
            public string Ounces { get; set; }
            public string Carats { get; set; }
            public string Drams { get; set; }
            public string Grains { get; set; }
            public string Stones { get; set; }
            public string CubicKilometers { get; set; }
            public string CubicMeters { get; set; }
            public string Litres { get; set; }
            public string CubicCentimeters { get; set; }
            public string CubicMillimeters { get; set; }
            public string CubicMiles { get; set; }
            public string CubicYards { get; set; }
            public string CubicFTs { get; set; }
            public string CubicInches { get; set; }
            public string OilBarrels { get; set; }
            public string GallonUS { get; set; }
            public string QuartsUS { get; set; }
            public string PintsUS { get; set; }
            public string FluidOuncesUS { get; set; }
            public string Bushels { get; set; }
            public string Pecks { get; set; }
            public string GallonsUK { get; set; }
            public string QuartsUK { get; set; }
            public string PintsUK { get; set; }
            public string FluidOuncesUK { get; set; }
            public string SquareKilometers { get; set; }
            public string SquareMeters { get; set; }
            public string SquareCentimeters { get; set; }
            public string SquareMillimeters { get; set; }
            public string SquareMiles { get; set; }
            public string SquareYards { get; set; }
            public string SquareFTs { get; set; }
            public string SquareInches { get; set; }
            public string Hectares { get; set; }
            public string Acres { get; set; }
            public string Ares { get; set; }
            public string Hours { get; set; }
            public string Minutes { get; set; }
            public string Seconds { get; set; }
            public string Milliseconds { get; set; }
            public string Microseconds { get; set; }
            public string Fahrenheit { get; set; }
            public string Celsius { get; set; }
            public string Kelvin { get; set; }
            public string KilometersPerHour { get; set; }
            public string MetersPerSecond { get; set; }
            public string MilesPerHour { get; set; }
            public string YardsPerMinute { get; set; }
            public string FTsPerSecond { get; set; }
            public string Knots { get; set; }
            public string PoundsForce { get; set; }
            public string Newtons { get; set; }
            public string KilosForce { get; set; }
            public string Jouls { get; set; }
            public string Calories { get; set; }
            public string Ergs { get; set; }
            public string ElectronVolts { get; set; }
            public string Btus { get; set; }
            public string Watts { get; set; }
            public string Horsepower { get; set; }
            public string Atmospheres { get; set; }
            public string Bars { get; set; }
            public string Pascals { get; set; }
            public string MillimetersOfMercury { get; set; }
            public string PoundPerSquareInch { get; set; }
            public string KilogramPerSquareCentimeter { get; set; }
            public string KiloPascals { get; set; }
        }

        public class MergeDoubleLines
        {
            public string Title { get; set; }
            public string MaxMillisecondsBetweenLines { get; set; }
            public string IncludeIncrementing { get; set; }
        }

        public class MergeShortLines
        {
            public string Title { get; set; }
            public string MaximumCharacters { get; set; }
            public string MaximumMillisecondsBetween { get; set; }
            public string NumberOfMergesX { get; set; }
            public string MergedText { get; set; }
            public string OnlyMergeContinuationLines { get; set; }
        }

        public class MergeTextWithSameTimeCodes
        {
            public string Title { get; set; }
            public string MaxDifferenceMilliseconds { get; set; }
            public string MakeDialog { get; set; }
            public string ReBreakLines { get; set; }
            public string NumberOfMergesX { get; set; }
            public string MergedText { get; set; }
        }

        public class ModifySelection
        {
            public string Title { get; set; }
            public string Rule { get; set; }
            public string CaseSensitive { get; set; }
            public string DoWithMatches { get; set; }
            public string MakeNewSelection { get; set; }
            public string AddToCurrentSelection { get; set; }
            public string SubtractFromCurrentSelection { get; set; }
            public string IntersectWithCurrentSelection { get; set; }
            public string MatchingLinesX { get; set; }
            public string Contains { get; set; }
            public string StartsWith { get; set; }
            public string EndsWith { get; set; }
            public string NoContains { get; set; }
            public string RegEx { get; set; }
            public string OddLines { get; set; }
            public string EvenLines { get; set; }
            public string DurationLessThan { get; set; }
            public string DurationGreaterThan { get; set; }
            public string CpsLessThan { get; set; }
            public string CpsGreaterThan { get; set; }
            public string LengthLessThan { get; set; }
            public string LengthGreaterThan { get; set; }
            public string ExactlyOneLine { get; set; }
            public string ExactlyTwoLines { get; set; }
            public string MoreThanTwoLines { get; set; }
            public string Bookmarked { get; set; }
            public string BlankLines { get; set; }
        }

        public class MultipleReplace
        {
            public string Title { get; set; }
            public string FindWhat { get; set; }
            public string ReplaceWith { get; set; }
            public string Normal { get; set; }
            public string CaseSensitive { get; set; }
            public string RegularExpression { get; set; }
            public string Description { get; set; }
            public string LinesFoundX { get; set; }
            public string Remove { get; set; }
            public string Add { get; set; }
            public string Update { get; set; }
            public string Enabled { get; set; }
            public string SearchType { get; set; }
            public string RemoveAll { get; set; }
            public string Import { get; set; }
            public string Export { get; set; }
            public string ImportRulesTitle { get; set; }
            public string ExportRulesTitle { get; set; }
            public string ChooseGroupsToImport { get; set; }
            public string ChooseGroupsToExport { get; set; }
            public string Rules { get; set; }
            public string MoveToTop { get; set; }
            public string MoveToBottom { get; set; }
            public string MoveSelectedRulesToGroup { get; set; }
            public string Groups { get; set; }
            public string RulesForGroupX { get; set; }
            public string GroupName { get; set; }
            public string RenameGroup { get; set; }
            public string NewGroup { get; set; }
            public string NothingToImport { get; set; }
            public string RuleInfo { get; set; }
        }

        public class NetworkChat
        {
            public string Title { get; set; }
            public string Send { get; set; }
        }

        public class NetworkJoin
        {
            public string Title { get; set; }
            public string Information { get; set; }
            public string Join { get; set; }
        }

        public class NetworkLogAndInfo
        {
            public string Title { get; set; }
            public string Log { get; set; }
        }

        public class NetworkStart
        {
            public string Title { get; set; }
            public string ConnectionTo { get; set; }
            public string Information { get; set; }
            public string Start { get; set; }
        }

        public class OpenVideoDvd
        {
            public string Title { get; set; }
            public string OpenDvdFrom { get; set; }
            public string Disc { get; set; }
            public string Folder { get; set; }
            public string ChooseDrive { get; set; }
            public string ChooseFolder { get; set; }
        }

        public class PluginsGet
        {
            public string Title { get; set; }
            public string InstalledPlugins { get; set; }
            public string GetPlugins { get; set; }
            public string Description { get; set; }
            public string Version { get; set; }
            public string Date { get; set; }
            public string Type { get; set; }
            public string OpenPluginsFolder { get; set; }
            public string GetPluginsInfo1 { get; set; }
            public string GetPluginsInfo2 { get; set; }
            public string PluginXDownloaded { get; set; }
            public string Download { get; set; }
            public string Remove { get; set; }
            public string UpdateAllX { get; set; }
            public string UnableToDownloadPluginListX { get; set; }
            public string NewVersionOfSubtitleEditRequired { get; set; }
            public string UpdateAvailable { get; set; }
            public string UpdateAll { get; set; }
            public string XPluginsUpdated { get; set; }
        }

        public class RegularExpressionContextMenu
        {
            public string WordBoundary { get; set; }
            public string NonWordBoundary { get; set; }
            public string NewLine { get; set; }
            public string NewLineShort { get; set; }
            public string AnyDigit { get; set; }
            public string NonDigit { get; set; }
            public string AnyCharacter { get; set; }
            public string AnyWhitespace { get; set; }
            public string NonSpaceCharacter { get; set; }
            public string ZeroOrMore { get; set; }
            public string OneOrMore { get; set; }
            public string InCharacterGroup { get; set; }
            public string NotInCharacterGroup { get; set; }
        }

        public class RemoveTextFromHearImpaired
        {
            public string Title { get; set; }
            public string RemoveTextConditions { get; set; }
            public string RemoveTextBetween { get; set; }
            public string SquareBrackets { get; set; }
            public string Brackets { get; set; }
            public string Parentheses { get; set; }
            public string QuestionMarks { get; set; }
            public string And { get; set; }
            public string RemoveTextBeforeColon { get; set; }
            public string OnlyIfTextIsUppercase { get; set; }
            public string OnlyIfInSeparateLine { get; set; }
            public string LinesFoundX { get; set; }
            public string RemoveTextIfContains { get; set; }
            public string RemoveTextIfAllUppercase { get; set; }
            public string RemoveIfOnlyMusicSymbols { get; set; }
            public string RemoveInterjections { get; set; }
            public string EditInterjections { get; set; }
            public string Apply { get; set; }
        }

        public class ReplaceDialog
        {
            public string Title { get; set; }
            public string FindWhat { get; set; }
            public string Normal { get; set; }
            public string CaseSensitive { get; set; }
            public string RegularExpression { get; set; }
            public string ReplaceWith { get; set; }
            public string Find { get; set; }
            public string Replace { get; set; }
            public string ReplaceAll { get; set; }
            public string FindReplaceIn { get; set; }
            public string TranslationAndOriginal { get; set; }
            public string TranslationOnly { get; set; }
            public string OriginalOnly { get; set; }
        }

        public class RestoreAutoBackup
        {
            public string Title { get; set; }
            public string Information { get; set; }
            public string DateAndTime { get; set; }
            public string FileName { get; set; }
            public string Extension { get; set; }
            public string NoBackedUpFilesFound { get; set; }
        }

        public class SeekSilence
        {
            public string Title { get; set; }
            public string SearchDirection { get; set; }
            public string Forward { get; set; }
            public string Back { get; set; }
            public string LengthInSeconds { get; set; }
            public string MaxVolume { get; set; }
        }

        public class SetMinimumDisplayTimeBetweenParagraphs
        {
            public string Title { get; set; }
            public string PreviewLinesModifiedX { get; set; }
            public string ShowOnlyModifiedLines { get; set; }
            public string MinimumMillisecondsBetweenParagraphs { get; set; }
            public string FrameInfo { get; set; }
            public string Frames { get; set; }
            public string XFrameYisZMilliseconds { get; set; }
        }

        public class SetSyncPoint
        {
            public string Title { get; set; }
            public string SyncPointTimeCode { get; set; }
            public string ThreeSecondsBack { get; set; }
            public string HalfASecondBack { get; set; }
            public string HalfASecondForward { get; set; }
            public string ThreeSecondsForward { get; set; }
        }

        public class Settings
        {
            public string Title { get; set; }
            public string General { get; set; }
            public string SubtitleFormats { get; set; }
            public string Toolbar { get; set; }
            public string VideoPlayer { get; set; }
            public string WaveformAndSpectrogram { get; set; }
            public string Tools { get; set; }
            public string WordLists { get; set; }
            public string SsaStyle { get; set; }
            public string Network { get; set; }
            public string FileTypeAssociations { get; set; }
            public string Rules { get; set; }
            public string ShowToolBarButtons { get; set; }
            public string New { get; set; }
            public string Open { get; set; }
            public string Save { get; set; }
            public string SaveAs { get; set; }
            public string Find { get; set; }
            public string Replace { get; set; }
            public string VisualSync { get; set; }
            public string BurnIn { get; set; }
            public string SpellCheck { get; set; }
            public string NetflixQualityCheck { get; set; }
            public string BeautifyTimeCodes { get; set; }
            public string SettingsName { get; set; }
            public string ToggleBookmarks { get; set; }
            public string FocusTextBox { get; set; }
            public string ToggleBookmarksWithComment { get; set; }
            public string ClearBookmarks { get; set; }
            public string ExportBookmarks { get; set; }
            public string GoToBookmark { get; set; }
            public string GoToPreviousBookmark { get; set; }
            public string GoToNextBookmark { get; set; }
            public string ChooseProfile { get; set; }
            public string OpenDataFolder { get; set; }
            public string DuplicateLine { get; set; }
            public string ToggleView { get; set; }
            public string ToggleMode { get; set; }
            public string TogglePreviewOnVideo { get; set; }
            public string Help { get; set; }
            public string FontInUi { get; set; }
            public string Appearance { get; set; }
            public string ShowFrameRate { get; set; }
            public string DefaultFrameRate { get; set; }
            public string DefaultFileEncoding { get; set; }
            public string AutoDetectAnsiEncoding { get; set; }
            public string LanguageFilter { get; set; }
            public string Profile { get; set; }
            public string Profiles { get; set; }
            public string ImportProfiles { get; set; }
            public string ExportProfiles { get; set; }
            public string SubtitleLineMaximumLength { get; set; }
            public string OptimalCharactersPerSecond { get; set; }
            public string MaximumCharactersPerSecond { get; set; }
            public string MaximumWordsPerMinute { get; set; }
            public string AutoWrapWhileTyping { get; set; }
            public string DurationMinimumMilliseconds { get; set; }
            public string DurationMaximumMilliseconds { get; set; }
            public string MinimumGapMilliseconds { get; set; }
            public string MaximumLines { get; set; }

            public string SubtitleFont { get; set; }
            public string SubtitleFontSize { get; set; }
            public string SubtitleBold { get; set; }
            public string VideoAutoOpen { get; set; }
            public string AllowVolumeBoost { get; set; }

            public string SubtitleCenter { get; set; }
            public string SubtitleFontColor { get; set; }
            public string SubtitleBackgroundColor { get; set; }
            public string SpellChecker { get; set; }
            public string RememberRecentFiles { get; set; }
            public string StartWithLastFileLoaded { get; set; }
            public string RememberSelectedLine { get; set; }
            public string RememberPositionAndSize { get; set; }
            public string StartInSourceView { get; set; }
            public string RemoveBlankLinesWhenOpening { get; set; }
            public string RemoveBlankLines { get; set; }
            public string ApplyAssaOverrideTags { get; set; }
            public string SetAssaPosition { get; set; }
            public string SetAssaResolution { get; set; }
            public string SetAssaBgBox { get; set; }
            public string TakeAutoBackup { get; set; }
            public string ShowLineBreaksAs { get; set; }
            public string SaveAsFileNameFrom { get; set; }
            public string MainListViewDoubleClickAction { get; set; }
            public string MainListViewColumnsInfo { get; set; }
            public string MainListViewNothing { get; set; }
            public string MainListViewVideoGoToPositionAndPause { get; set; }
            public string MainListViewVideoGoToPositionAndPlay { get; set; }
            public string MainListViewVideoGoToPositionAndPlayCurrentAndPause { get; set; }
            public string MainListViewEditText { get; set; }
            public string MainListViewVideoGoToPositionMinus1SecAndPause { get; set; }
            public string MainListViewVideoGoToPositionMinusHalfSecAndPause { get; set; }
            public string MainListViewVideoGoToPositionMinus1SecAndPlay { get; set; }
            public string MainListViewEditTextAndPause { get; set; }
            public string VideoFileName { get; set; }
            public string ExistingFileName { get; set; }
            public string AutoBackup { get; set; }

            public string AutoBackupEveryMinute { get; set; }
            public string AutoBackupEveryFiveMinutes { get; set; }
            public string AutoBackupEveryFifteenMinutes { get; set; }
            public string AutoBackupDeleteAfter { get; set; }
            public string TranslationAutoSuffix { get; set; }
            public string AutoBackupDeleteAfterOneMonth { get; set; }
            public string AutoBackupDeleteAfterXMonths { get; set; }
            public string CheckForUpdates { get; set; }
            public string AutoSave { get; set; }
            public string AllowEditOfOriginalSubtitle { get; set; }
            public string PromptDeleteLines { get; set; }
            public string TimeCodeMode { get; set; }
            public string TimeCodeModeHHMMSSMS { get; set; }
            public string TimeCodeModeHHMMSSFF { get; set; }
            public string SplitBehavior { get; set; }
            public string SplitBehaviorPrevious { get; set; }
            public string SplitBehaviorHalf { get; set; }
            public string SplitBehaviorNext { get; set; }
            public string VideoEngine { get; set; }
            public string DirectShow { get; set; }
            public string DirectShowDescription { get; set; }
            public string MpcHc { get; set; }
            public string MpcHcDescription { get; set; }
            public string MpvPlayer { get; set; }
            public string MpvPlayerDescription { get; set; }
            public string MpvHandlesPreviewText { get; set; }
            public string VlcMediaPlayer { get; set; }
            public string VlcMediaPlayerDescription { get; set; }
            public string VlcBrowseToLabel { get; set; }
            public string ShowStopButton { get; set; }
            public string ShowMuteButton { get; set; }
            public string ShowFullscreenButton { get; set; }
            public string PreviewFontName { get; set; }
            public string PreviewFontSize { get; set; }
            public string PreviewVerticalMargin { get; set; }
            public string MainWindowVideoControls { get; set; }
            public string CustomSearchTextAndUrl { get; set; }
            public string WaveformAppearance { get; set; }
            public string WaveformGridColor { get; set; }
            public string WaveformShowGridLines { get; set; }
            public string WaveformShowCps { get; set; }
            public string WaveformShowWpm { get; set; }
            public string ReverseMouseWheelScrollDirection { get; set; }
            public string WaveformAllowOverlap { get; set; }
            public string WaveformSetVideoPosMoveStartEnd { get; set; }
            public string WaveformFocusMouseEnter { get; set; }
            public string WaveformListViewFocusMouseEnter { get; set; }
            public string WaveformSingleClickSelect { get; set; }
            public string WaveformSnapToShotChanges { get; set; }
            public string WaveformEditShotChangesProfile { get; set; }
            public string WaveformAutoGen { get; set; }
            public string WaveformBorderHitMs1 { get; set; }
            public string WaveformBorderHitMs2 { get; set; }
            public string WaveformColor { get; set; }
            public string WaveformSelectedColor { get; set; }
            public string WaveformBackgroundColor { get; set; }
            public string WaveformCursorColor { get; set; }
            public string WaveformTextColor { get; set; }
            public string WaveformTextFontSize { get; set; }
            public string WaveformAndSpectrogramsFolderEmpty { get; set; }
            public string WaveformAndSpectrogramsFolderInfo { get; set; }
            public string Spectrogram { get; set; }
            public string GenerateSpectrogram { get; set; }
            public string SpectrogramAppearance { get; set; }
            public string SpectrogramOneColorGradient { get; set; }
            public string SpectrogramClassic { get; set; }
            public string WaveformUseFFmpeg { get; set; }
            public string WaveformUseCenterChannelOnly { get; set; }
            public string DownloadX { get; set; }
            public string WaveformFFmpegPath { get; set; }
            public string WaveformBrowseToFFmpeg { get; set; }
            public string WaveformBrowseToVLC { get; set; }
            public string SubStationAlphaStyle { get; set; }
            public string ChooseColor { get; set; }
            public string SsaOutline { get; set; }
            public string SsaShadow { get; set; }
            public string SsaOpaqueBox { get; set; }
            public string Testing123 { get; set; }
            public string Language { get; set; }
            public string NamesIgnoreLists { get; set; }
            public string AddName { get; set; }
            public string AddWord { get; set; }
            public string Remove { get; set; }
            public string AddPair { get; set; }
            public string UserWordList { get; set; }
            public string OcrFixList { get; set; }
            public string Location { get; set; }
            public string UseOnlineNames { get; set; }
            public string WordAddedX { get; set; }
            public string WordAlreadyExists { get; set; }
            public string WordNotFound { get; set; }
            public string RemoveX { get; set; }
            public string CannotUpdateNamesOnline { get; set; }
            public string ProxyServerSettings { get; set; }
            public string ProxyAddress { get; set; }
            public string ProxyAuthentication { get; set; }
            public string ProxyUserName { get; set; }
            public string ProxyPassword { get; set; }
            public string ProxyDomain { get; set; }
            public string ProxyAuthType { get; set; }
            public string ProxyUseDefaultCredentials { get; set; }
            public string NetworkSessionSettings { get; set; }
            public string NetworkSessionNewSound { get; set; }
            public string PlayXSecondsAndBack { get; set; }
            public string StartSceneIndex { get; set; }
            public string EndSceneIndex { get; set; }
            public string FirstPlusX { get; set; }
            public string LastMinusX { get; set; }
            public string FixCommonerrors { get; set; }
            public string RemoveTextForHi { get; set; }
            public string MergeLinesShorterThan { get; set; }
            public string DialogStyle { get; set; }
            public string DialogStyleDashBothLinesWithSpace { get; set; }
            public string DialogStyleDashBothLinesWithoutSpace { get; set; }
            public string DialogStyleDashSecondLineWithSpace { get; set; }
            public string DialogStyleDashSecondLineWithoutSpace { get; set; }
            public string ContinuationStyle { get; set; }
            public string ContinuationStyleNone { get; set; }
            public string ContinuationStyleNoneTrailingDots { get; set; }
            public string ContinuationStyleNoneLeadingTrailingDots { get; set; }
            public string ContinuationStyleNoneTrailingEllipsis { get; set; }
            public string ContinuationStyleNoneLeadingTrailingEllipsis { get; set; }
            public string ContinuationStyleOnlyTrailingDots { get; set; }
            public string ContinuationStyleLeadingTrailingDots { get; set; }
            public string ContinuationStyleOnlyTrailingEllipsis { get; set; }
            public string ContinuationStyleLeadingTrailingEllipsis { get; set; }
            public string ContinuationStyleLeadingTrailingDash { get; set; }
            public string ContinuationStyleLeadingTrailingDashDots { get; set; }
            public string ContinuationStyleCustom { get; set; }
            public string CpsLineLengthStyle { get; set; }
            public string CpsLineLengthStyleCalcAll { get; set; }
            public string CpsLineLengthStyleCalcNoSpaceCpsOnly { get; set; }
            public string CpsLineLengthStyleCalcNoSpace { get; set; }
            public string CpsLineLengthStyleCalcCjk { get; set; }
            public string CpsLineLengthStyleCalcCjkNoSpace { get; set; }
            public string CpsLineLengthStyleCalcIncludeCompositionCharacters { get; set; }
            public string CpsLineLengthStyleCalcIncludeCompositionCharactersNotSpace { get; set; }
            public string CpsLineLengthStyleCalcNoSpaceOrPunctuation { get; set; }
            public string CpsLineLengthStyleCalcNoSpaceOrPunctuationCpsOnly { get; set; }
            public string MusicSymbol { get; set; }
            public string MusicSymbolsReplace { get; set; }
            public string FixCommonOcrErrorsUseHardcodedRules { get; set; }
            public string UseWordSplitList { get; set; }
            public string AvoidPropercase { get; set; }
            public string FixCommonerrorsFixShortDisplayTimesAllowMoveStartTime { get; set; }
            public string FixCommonErrorsSkipStepOne { get; set; }

            public string DefaultFormat { get; set; }
            public string DefaultSaveAsFormat { get; set; }
            public string DefaultSaveAsFormatAuto { get; set; }
            public string Favorites { get; set; }
            public string FavoriteFormats { get; set; }
            public string FavoriteSubtitleFormatsNote { get; set; }

            public string Shortcuts { get; set; }
            public string Shortcut { get; set; }
            public string Control { get; set; }
            public string Alt { get; set; }
            public string Shift { get; set; }
            public string Key { get; set; }
            public string ListViewAndTextBox { get; set; }
            public string ListView { get; set; }
            public string TextBox { get; set; }
            public string UseSyntaxColoring { get; set; }
            public string HtmlColor { get; set; }
            public string AssaColor { get; set; }
            public string Theme { get; set; }
            public string Automatic { get; set; }
            public string DarkTheme { get; set; }
            public string DarkThemeEnabled { get; set; }
            public string DarkThemeShowGridViewLines { get; set; }
            public string GraphicsButtons { get; set; }
            public string UpdateShortcut { get; set; }
            public string FocusSetVideoPosition { get; set; }
            public string ToggleDockUndockOfVideoControls { get; set; }
            public string CreateSetEndAddNewAndGoToNew { get; set; }
            public string AdjustViaEndAutoStart { get; set; }
            public string AdjustViaEndAutoStartAndGoToNext { get; set; }
            public string AdjustSetEndMinusGapAndStartNextHere { get; set; }
            public string AdjustSetEndAndStartNextAfterGap { get; set; }
            public string AdjustSetStartTimeAndGoToNext { get; set; }
            public string AdjustSetEndTimeAndGoToNext { get; set; }
            public string AdjustSetEndTimeAndPause { get; set; }
            public string AdjustSetStartAutoDurationAndGoToNext { get; set; }
            public string AdjustSetEndNextStartAndGoToNext { get; set; }
            public string AdjustStartDownEndUpAndGoToNext { get; set; }
            public string AdjustSetStartAndEndOfPrevious { get; set; }
            public string AdjustSetStartAndEndOfPreviousAndGoToNext { get; set; }
            public string AdjustSelected100MsForward { get; set; }
            public string AdjustSelected100MsBack { get; set; }
            public string AdjustStartXMsBack { get; set; }
            public string AdjustStartXMsForward { get; set; }
            public string AdjustEndXMsBack { get; set; }
            public string AdjustEndXMsForward { get; set; }
            public string AdjustStartOneFrameBack { get; set; }
            public string AdjustStartOneFrameForward { get; set; }
            public string AdjustEndOneFrameBack { get; set; }
            public string AdjustEndOneFrameForward { get; set; }
            public string AdjustStartOneFrameBackKeepGapPrev { get; set; }
            public string AdjustStartOneFrameForwardKeepGapPrev { get; set; }
            public string AdjustEndOneFrameBackKeepGapNext { get; set; }
            public string AdjustEndOneFrameForwardKeepGapNext { get; set; }
            public string AdjustSetStartTimeKeepDuration { get; set; }
            public string AdjustVideoSetStartForAppropriateLine { get; set; }
            public string AdjustVideoSetEndForAppropriateLine { get; set; }
            public string AdjustSetStartAndOffsetTheWholeSubtitle { get; set; }
            public string AdjustSetEndAndOffsetTheRest { get; set; }
            public string AdjustSetEndAndOffsetTheRestAndGoToNext { get; set; }
            public string AdjustSnapStartToNextShotChange { get; set; }
            public string AdjustSnapEndToPreviousShotChange { get; set; }
            public string AdjustExtendToNextShotChange { get; set; }
            public string AdjustExtendToPreviousShotChange { get; set; }
            public string AdjustExtendToNextSubtitle { get; set; }
            public string AdjustExtendToPreviousSubtitle { get; set; }
            public string AdjustExtendToNextSubtitleMinusChainingGap { get; set; }
            public string AdjustExtendToPreviousSubtitleMinusChainingGap { get; set; }
            public string AdjustExtendCurrentSubtitle { get; set; }
            public string AdjustExtendPreviousLineEndToCurrentStart { get; set; }
            public string AdjustExtendNextLineStartToCurrentEnd { get; set; }
            public string RecalculateDurationOfCurrentSubtitle { get; set; }
            public string RecalculateDurationOfCurrentSubtitleByOptimalReadingSpeed { get; set; }
            public string RecalculateDurationOfCurrentSubtitleByMinReadingSpeed { get; set; }
            public string SetInCueToClosestShotChangeLeftGreenZone { get; set; }
            public string SetInCueToClosestShotChangeRightGreenZone { get; set; }
            public string SetOutCueToClosestShotChangeLeftGreenZone { get; set; }
            public string SetOutCueToClosestShotChangeRightGreenZone { get; set; }
            public string MainCreateStartDownEndUp { get; set; }
            public string MergeDialog { get; set; }
            public string MergeDialogWithNext { get; set; }
            public string MergeDialogWithPrevious { get; set; }
            public string AutoBalanceSelectedLines { get; set; }
            public string EvenlyDistributeSelectedLines { get; set; }
            public string GoToNext { get; set; }
            public string GoToNextPlayTranslate { get; set; }
            public string GoToNextCursorAtEnd { get; set; }
            public string GoToPrevious { get; set; }
            public string GoToPreviousPlayTranslate { get; set; }
            public string GoToCurrentSubtitleStart { get; set; }
            public string GoToCurrentSubtitleEnd { get; set; }
            public string GoToPreviousSubtitleAndFocusVideo { get; set; }
            public string GoToNextSubtitleAndFocusVideo { get; set; }
            public string GoToPrevSubtitleAndPlay { get; set; }
            public string GoToNextSubtitleAndPlay { get; set; }
            public string GoToPreviousSubtitleAndFocusWaveform { get; set; }
            public string GoToNextSubtitleAndFocusWaveform { get; set; }
            public string ToggleFocus { get; set; }
            public string ToggleFocusWaveform { get; set; }
            public string ToggleFocusWaveformTextBox { get; set; }
            public string ToggleDialogDashes { get; set; }
            public string ToggleQuotes { get; set; }
            public string ToggleHiTags { get; set; }
            public string ToggleCustomTags { get; set; }
            public string ToggleMusicSymbols { get; set; }
            public string Alignment { get; set; }
            public string AlignmentN1 { get; set; }
            public string AlignmentN2 { get; set; }
            public string AlignmentN3 { get; set; }
            public string AlignmentN4 { get; set; }
            public string AlignmentN5 { get; set; }
            public string AlignmentN6 { get; set; }
            public string AlignmentN7 { get; set; }
            public string AlignmentN8 { get; set; }
            public string AlignmentN9 { get; set; }
            public string ColorX { get; set; }
            public string CopyTextOnly { get; set; }
            public string CopyPlainText { get; set; }
            public string CopyTextOnlyFromOriginalToCurrent { get; set; }
            public string AutoDurationSelectedLines { get; set; }
            public string FixRTLViaUnicodeChars { get; set; }
            public string RemoveRTLUnicodeChars { get; set; }
            public string ReverseStartAndEndingForRtl { get; set; }
            public string VerticalZoom { get; set; }
            public string VerticalZoomOut { get; set; }
            public string WaveformSeekSilenceForward { get; set; }
            public string WaveformSeekSilenceBack { get; set; }
            public string WaveformAddTextHere { get; set; }
            public string ChooseLayoutX { get; set; }
            public string WaveformAddTextHereFromClipboard { get; set; }
            public string SetParagraphAsSelection { get; set; }
            public string WaveformPlayNewSelection { get; set; }
            public string WaveformPlayNewSelectionEnd { get; set; }
            public string WaveformPlayFirstSelectedSubtitle { get; set; }
            public string WaveformGoToPreviousShotChange { get; set; }
            public string WaveformGoToNextShotChange { get; set; }
            public string WaveformToggleShotChange { get; set; }
            public string WaveformAllShotChangesOneFrameForward { get; set; }
            public string WaveformAllShotChangesOneFrameBack { get; set; }
            public string WaveformRemoveOrExportShotChanges { get; set; }
            public string WaveformGuessStart { get; set; }
            public string GoBack1Frame { get; set; }
            public string GoForward1Frame { get; set; }
            public string GoBack1FrameWithPlay { get; set; }
            public string GoForward1FrameWithPlay { get; set; }
            public string GoBack100Milliseconds { get; set; }
            public string GoForward100Milliseconds { get; set; }
            public string GoBack500Milliseconds { get; set; }
            public string GoForward500Milliseconds { get; set; }
            public string GoBack1Second { get; set; }
            public string GoForward1Second { get; set; }
            public string GoBack3Seconds { get; set; }
            public string GoForward3Seconds { get; set; }
            public string GoBack5Seconds { get; set; }
            public string GoForward5Seconds { get; set; }
            public string GoBackXSSeconds { get; set; }
            public string GoForwardXSSeconds { get; set; }
            public string GoBackXLSeconds { get; set; }
            public string GoForwardXLSeconds { get; set; }
            public string GoToStartCurrent { get; set; }
            public string ToggleStartEndCurrent { get; set; }
            public string PlaySelectedLines { get; set; }
            public string LoopSelectedLines { get; set; }

            public string WaveformGoToPrevSubtitle { get; set; }
            public string WaveformGoToNextSubtitle { get; set; }
            public string WaveformGoToPrevTimeCode { get; set; }
            public string WaveformGoToNextTimeCode { get; set; }
            public string WaveformGoToPrevChapter { get; set; }
            public string WaveformGoToNextChapter { get; set; }
            public string WaveformSelectNextSubtitle { get; set; }
            public string TogglePlayPause { get; set; }
            public string Pause { get; set; }
            public string Fullscreen { get; set; }
            public string Play150Speed { get; set; }
            public string Play200Speed { get; set; }
            public string PlayRateSlower { get; set; }
            public string PlayRateFaster { get; set; }
            public string PlayRateToggle { get; set; }
            public string VideoResetSpeedAndZoom { get; set; }
            public string MainToggleVideoControls { get; set; }
            public string VideoToggleContrast { get; set; }
            public string AudioToTextX { get; set; }
            public string AudioExtractSelectedLines { get; set; }
            public string AudioToTextSelectedLinesX { get; set; }
            public string VideoToggleBrightness { get; set; }
            public string AutoTranslateSelectedLines { get; set; }
            public string CustomSearch1 { get; set; }
            public string CustomSearch2 { get; set; }
            public string CustomSearch3 { get; set; }
            public string CustomSearch4 { get; set; }
            public string CustomSearch5 { get; set; }
            public string SyntaxColoring { get; set; }
            public string ListViewSyntaxColoring { get; set; }
            public string SyntaxColorDurationIfTooSmall { get; set; }
            public string SyntaxColorDurationIfTooLarge { get; set; }
            public string SyntaxColorTextIfTooLong { get; set; }
            public string SyntaxColorTextIfTooWide { get; set; }
            public string SyntaxColorTextMoreThanMaxLines { get; set; }
            public string SyntaxColorOverlap { get; set; }
            public string SyntaxColorGap { get; set; }
            public string SyntaxErrorColor { get; set; }
            public string SyntaxLineWidthSettings { get; set; }
            public string LineWidthSettings { get; set; }
            public string MaximumLineWidth { get; set; }
            public string Pixels { get; set; }
            public string MeasureFont { get; set; }
            public string GoToFirstSelectedLine { get; set; }
            public string GoToNextEmptyLine { get; set; }
            public string MergeSelectedLines { get; set; }
            public string MergeSelectedLinesAndAutoBreak { get; set; }
            public string MergeSelectedLinesAndUnbreak { get; set; }
            public string MergeSelectedLinesAndUnbreakCjk { get; set; }
            public string MergeSelectedLinesOnlyFirstText { get; set; }
            public string MergeSelectedLinesBilingual { get; set; }
            public string MergeWithPreviousBilingual { get; set; }
            public string MergeWithNextBilingual { get; set; }
            public string SplitSelectedLineBilingual { get; set; }
            public string ToggleTranslationMode { get; set; }
            public string SwitchOriginalAndTranslation { get; set; }
            public string SwitchOriginalAndTranslationTextBoxes { get; set; }
            public string MergeOriginalAndTranslation { get; set; }
            public string MergeWithNext { get; set; }
            public string MergeWithPreviousAndUnbreak { get; set; }
            public string MergeWithNextAndUnbreak { get; set; }
            public string MergeWithPreviousAndBreak { get; set; }
            public string MergeWithNextAndBreak { get; set; }
            public string MergeWithPrevious { get; set; }
            public string ShortcutIsAlreadyDefinedX { get; set; }
            public string ToggleTranslationAndOriginalInPreviews { get; set; }
            public string ListViewColumnDelete { get; set; }
            public string ListViewColumnDeleteAndShiftUp { get; set; }
            public string ListViewColumnInsert { get; set; }
            public string ListViewColumnPaste { get; set; }
            public string ListViewColumnTextUp { get; set; }
            public string ListViewColumnTextDown { get; set; }
            public string ListViewGoToNextError { get; set; }
            public string ListViewListErrors { get; set; }
            public string ListViewListSortByX { get; set; }
            public string ShowStyleManager { get; set; }
            public string MainTextBoxMoveLastWordDown { get; set; }
            public string MainTextBoxMoveFirstWordFromNextUp { get; set; }
            public string MainTextBoxMoveFirstWordUpCurrent { get; set; }
            public string MainTextBoxMoveFromCursorToNext { get; set; }
            public string MainTextBoxMoveLastWordDownCurrent { get; set; }
            public string MainTextBoxSelectionToLower { get; set; }
            public string MainTextBoxSelectionToUpper { get; set; }
            public string MainTextBoxSelectionToggleCasing { get; set; }
            public string MainTextBoxSelectionToRuby { get; set; }
            public string MainTextBoxToggleAutoDuration { get; set; }
            public string MainTextBoxAutoBreak { get; set; }
            public string MainTextBoxAutoBreakFromPos { get; set; }
            public string MainTextBoxAutoBreakFromPosAndGoToNext { get; set; }
            public string MainTextBoxDictate { get; set; }
            public string MainTextBoxUnbreak { get; set; }
            public string MainTextBoxUnbreakNoSpace { get; set; }
            public string MainTextBoxAssaIntellisense { get; set; }
            public string MainTextBoxAssaRemoveTag { get; set; }
            public string MainFileSaveAll { get; set; }
            public string Miscellaneous { get; set; }
            public string CpsIncludesSpace { get; set; }
            public string UseDoNotBreakAfterList { get; set; }
            public string BreakEarlyForLineEnding { get; set; }
            public string BreakByPixelWidth { get; set; }
            public string BreakPreferBottomHeavy { get; set; }
            public string BreakEarlyForDashDialog { get; set; }
            public string BreakEarlyForComma { get; set; }
            public string GoogleTranslate { get; set; }
            public string GoogleTranslateApiKey { get; set; }
            public string MicrosoftBingTranslator { get; set; }
            public string HowToSignUp { get; set; }
            public string MicrosoftTranslateApiKey { get; set; }
            public string MicrosoftTranslateTokenEndpoint { get; set; }
            public string FontNote { get; set; }
            public string RestoreDefaultSettings { get; set; }
            public string RestoreDefaultSettingsMsg { get; set; }
            public string RemoveTimeCodes { get; set; }
            public string EditFixContinuationStyleSettings { get; set; }
            public string FixContinuationStyleSettings { get; set; }
            public string UncheckInsertsAllCaps { get; set; }
            public string UncheckInsertsItalic { get; set; }
            public string UncheckInsertsLowercase { get; set; }
            public string HideContinuationCandidatesWithoutName { get; set; }
            public string IgnoreLyrics { get; set; }
            public string ContinuationPause { get; set; }
            public string Milliseconds { get; set; }
            public string EditCustomContinuationStyle { get; set; }
            public string MinFrameGap { get; set; }
            public string XFramesAtYFrameRateGivesZMs { get; set; }
            public string UseXAsNewGap { get; set; }
            public string BDOpensIn { get; set; }
            public string BDOpensInOcr { get; set; }
            public string BDOpensInEdit { get; set; }
            public string ShortcutsAllowSingleLetterOrNumberInTextBox { get; set; }
            public string ShortcutCustomToggle { get; set; }
            public string UpdateFileTypeAssociations { get; set; }
            public string FileTypeAssociationsUpdated { get; set; }
            public string CustomContinuationStyle { get; set; }
            public string LoadStyle { get; set; }
            public string Suffix { get; set; }
            public string AddSuffixForComma { get; set; }
            public string AddSpace { get; set; }
            public string RemoveComma { get; set; }
            public string Prefix { get; set; }
            public string DifferentStyleGap { get; set; }
            public string Preview { get; set; }
            public string PreviewPause { get; set; }
            public string CustomContinuationStyleNote { get; set; }
            public string ResetCustomContinuationStyleWarning { get; set; }
            public string ExportAsHtml { get; set; }
            public string SetNewActor { get; set; }
            public string SetActorX { get; set; }
            public string Used { get; set; }
            public string Unused { get; set; }
        }

        public class SettingsMpv
        {
            public string DownloadMpv { get; set; }
            public string DownloadMpvFailed { get; set; }
            public string DownloadMpvOk { get; set; }
        }

        public class SettingsFfmpeg
        {
            public string XDownloadFailed { get; set; }
            public string XDownloadOk { get; set; }
        }

        public class SetVideoOffset
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string RelativeToCurrentVideoPosition { get; set; }
            public string KeepTimeCodes { get; set; }
            public string Reset { get; set; }
        }

        public class ShowEarlierLater
        {
            public string Title { get; set; }
            public string TitleAll { get; set; }
            public string ShowEarlier { get; set; }
            public string ShowLater { get; set; }
            public string TotalAdjustmentX { get; set; }
            public string AllLines { get; set; }
            public string SelectedLinesOnly { get; set; }
            public string SelectedLinesAndForward { get; set; }
        }

        public class ShowHistory
        {
            public string Title { get; set; }
            public string SelectRollbackPoint { get; set; }
            public string Time { get; set; }
            public string Description { get; set; }
            public string CompareHistoryItems { get; set; }
            public string CompareWithCurrent { get; set; }
            public string Rollback { get; set; }
        }

        public class SpellCheck
        {
            public string Title { get; set; }
            public string FullText { get; set; }
            public string WordNotFound { get; set; }
            public string Language { get; set; }
            public string Change { get; set; }
            public string ChangeAll { get; set; }
            public string SkipOnce { get; set; }
            public string SkipAll { get; set; }
            public string AddToUserDictionary { get; set; }
            public string AddToNamesAndIgnoreList { get; set; }
            public string AddToOcrReplaceList { get; set; }
            public string Abort { get; set; }
            public string Use { get; set; }
            public string UseAlways { get; set; }
            public string Suggestions { get; set; }
            public string SpellCheckProgress { get; set; }
            public string EditWholeText { get; set; }
            public string EditWordOnly { get; set; }
            public string AddXToNames { get; set; }
            public string AddXToUserDictionary { get; set; }
            public string AutoFixNames { get; set; }
            public string AutoFixNamesViaSuggestions { get; set; }
            public string CheckOneLetterWords { get; set; }
            public string TreatINQuoteAsING { get; set; }
            public string RememberUseAlwaysList { get; set; }
            public string LiveSpellCheck { get; set; }
            public string LiveSpellCheckLanguage { get; set; }
            public string NoDictionaryForLiveSpellCheck { get; set; }
            public string ImageText { get; set; }
            public string SpellCheckCompleted { get; set; }
            public string SpellCheckAborted { get; set; }
            public string SpacesNotAllowed { get; set; }
            public string UndoX { get; set; }
            public string OpenImageBasedSourceFile { get; set; }
        }

        public class NetflixQualityCheck
        {
            public string GlyphCheckReport { get; set; }
            public string WhiteSpaceCheckReport { get; set; }
            public string ReportPrompt { get; set; }
            public string OpenReportInFolder { get; set; }
            public string FoundXIssues { get; set; }
            public string CheckOk { get; set; }
            public string MaximumXCharsPerSecond { get; set; }
            public string MaximumLineLength { get; set; }
            public string MinimumDuration { get; set; }
        }

        public class Split
        {
            public string Title { get; set; }
            public string SplitOptions { get; set; }
            public string Lines { get; set; }
            public string Characters { get; set; }
            public string NumberOfEqualParts { get; set; }
            public string SubtitleInfo { get; set; }
            public string NumberOfLinesX { get; set; }
            public string NumberOfCharactersX { get; set; }
            public string Output { get; set; }
            public string FileName { get; set; }
            public string OutputFolder { get; set; }
            public string DoSplit { get; set; }
            public string Basic { get; set; }
        }

        public class SplitLongLines
        {
            public string Title { get; set; }
            public string SingleLineMaximumLength { get; set; }
            public string LineMaximumLength { get; set; }
            public string LineContinuationBeginEndStrings { get; set; }
            public string NumberOfSplits { get; set; }
            public string LongestSingleLineIsXAtY { get; set; }
            public string LongestLineIsXAtY { get; set; }
            public string SplitAtLineBreaks { get; set; }
        }

        public class SplitSubtitle
        {
            public string Title { get; set; }
            public string Description1 { get; set; }
            public string Description2 { get; set; }
            public string Split { get; set; }
            public string Done { get; set; }
            public string NothingToSplit { get; set; }
            public string SavePartOneAs { get; set; }
            public string SavePartTwoAs { get; set; }
            public string Part1 { get; set; }
            public string Part2 { get; set; }
            public string UnableToSaveFileX { get; set; }
            public string OverwriteExistingFiles { get; set; }
            public string FolderNotFoundX { get; set; }
            public string Untitled { get; set; }
        }

        public class StartNumberingFrom
        {
            public string Title { get; set; }
            public string StartFromNumber { get; set; }
            public string PleaseEnterAValidNumber { get; set; }
        }

        public class Statistics
        {
            public string Title { get; set; }
            public string TitleWithFileName { get; set; }
            public string GeneralStatistics { get; set; }
            public string MostUsed { get; set; }
            public string MostUsedLines { get; set; }
            public string MostUsedWords { get; set; }
            public string NothingFound { get; set; }
            public string NumberOfLinesX { get; set; }
            public string LengthInFormatXinCharactersY { get; set; }
            public string NumberOfCharactersInTextOnly { get; set; }
            public string TotalDuration { get; set; }
            public string TotalCharsPerSecond { get; set; }
            public string TotalWords { get; set; }
            public string NumberOfItalicTags { get; set; }
            public string NumberOfBoldTags { get; set; }
            public string NumberOfUnderlineTags { get; set; }
            public string NumberOfFontTags { get; set; }
            public string NumberOfAlignmentTags { get; set; }
            public string LineLengthMinimum { get; set; }
            public string LineLengthMaximum { get; set; }
            public string LineLengthAverage { get; set; }
            public string LinesPerSubtitleAverage { get; set; }
            public string SingleLineLengthMinimum { get; set; }
            public string SingleLineLengthMaximum { get; set; }
            public string SingleLineLengthAverage { get; set; }
            public string SingleLineLengthExceedingMaximum { get; set; }
            public string SingleLineWidthMinimum { get; set; }
            public string SingleLineWidthMaximum { get; set; }
            public string SingleLineWidthAverage { get; set; }
            public string SingleLineWidthExceedingMaximum { get; set; }
            public string DurationMinimum { get; set; }
            public string DurationMaximum { get; set; }
            public string DurationAverage { get; set; }
            public string DurationExceedingMinimum { get; set; }
            public string DurationExceedingMaximum { get; set; }
            public string CharactersPerSecondMinimum { get; set; }
            public string CharactersPerSecondMaximum { get; set; }
            public string CharactersPerSecondAverage { get; set; }
            public string CharactersPerSecondExceedingOptimal { get; set; }
            public string CharactersPerSecondExceedingMaximum { get; set; }
            public string WordsPerMinuteMinimum { get; set; }
            public string WordsPerMinuteMaximum { get; set; }
            public string WordsPerMinuteAverage { get; set; }
            public string WordsPerMinuteExceedingMaximum { get; set; }
            public string GapMinimum { get; set; }
            public string GapMaximum { get; set; }
            public string GapAverage { get; set; }
            public string GapExceedingMinimum { get; set; }
            public string Export { get; set; }
        }

        public class SubStationAlphaProperties
        {
            public string Title { get; set; }
            public string TitleSubstationAlpha { get; set; }
            public string Script { get; set; }
            public string ScriptTitle { get; set; }
            public string OriginalScript { get; set; }
            public string Translation { get; set; }
            public string Editing { get; set; }
            public string Timing { get; set; }
            public string SyncPoint { get; set; }
            public string UpdatedBy { get; set; }
            public string UpdateDetails { get; set; }
            public string Resolution { get; set; }
            public string VideoResolution { get; set; }
            public string FromCurrentVideo { get; set; }
            public string Options { get; set; }
            public string WrapStyle { get; set; }
            public string Collision { get; set; }
            public string ScaleBorderAndShadow { get; set; }
            public string WrapStyle0 { get; set; }
            public string WrapStyle1 { get; set; }
            public string WrapStyle2 { get; set; }
            public string WrapStyle3 { get; set; }
        }

        public class SubStationAlphaStyles
        {
            public string Title { get; set; }
            public string TitleSubstationAlpha { get; set; }
            public string Styles { get; set; }
            public string Properties { get; set; }
            public string Name { get; set; }
            public string Font { get; set; }
            public string FontName { get; set; }
            public string FontSize { get; set; }
            public string UseCount { get; set; }
            public string Primary { get; set; }
            public string Secondary { get; set; }
            public string Tertiary { get; set; }
            public string Outline { get; set; }
            public string Shadow { get; set; }
            public string Back { get; set; }
            public string Alignment { get; set; }
            public string TopLeft { get; set; }
            public string TopCenter { get; set; }
            public string TopRight { get; set; }
            public string MiddleLeft { get; set; }
            public string MiddleCenter { get; set; }
            public string MiddleRight { get; set; }
            public string BottomLeft { get; set; }
            public string BottomCenter { get; set; }
            public string BottomRight { get; set; }
            public string Colors { get; set; }
            public string Margins { get; set; }
            public string MarginLeft { get; set; }
            public string MarginRight { get; set; }
            public string MarginVertical { get; set; }
            public string Vertical { get; set; }
            public string Border { get; set; }
            public string PlusShadow { get; set; }
            public string OpaqueBox { get; set; }
            public string Import { get; set; }
            public string Export { get; set; }
            public string Copy { get; set; }
            public string CopyOfY { get; set; }
            public string CopyXOfY { get; set; }
            public string New { get; set; }
            public string Remove { get; set; }
            public string ReplaceWith { get; set; }
            public string RemoveAll { get; set; }
            public string ImportStyleFromFile { get; set; }
            public string ExportStyleToFile { get; set; }
            public string ChooseStyle { get; set; }
            public string StyleAlreadyExits { get; set; }
            public string StyleXExportedToFileY { get; set; }
            public string StyleXImportedFromFileY { get; set; }
            public string SetPreviewText { get; set; }
            public string AddToFile { get; set; }
            public string AddToStorage { get; set; }
            public string StyleStorage { get; set; }
            public string StyleCurrentFile { get; set; }
            public string OverwriteX { get; set; }
            public string CategoryNote { get; set; }
            public string CategoriesManage { get; set; }
            public string MoveToCategory { get; set; }
            public string ScaleX { get; set; }
            public string ScaleY { get; set; }
            public string Spacing { get; set; }
            public string Angle { get; set; }
            public string BoxPerLine { get; set; }
            public string BoxMultiLine { get; set; }
            public string BoxPerLineShort { get; set; }
            public string BoxMultiLineShort { get; set; }
            public string BoxType { get; set; }
            public string DuplicateStyleNames { get; set; }
        }

        public class SubStationAlphaStylesCategoriesManager
        {
            public string Category { get; set; }
            public string Categories { get; set; }
            public string CategoryName { get; set; }
            public string CategoryDelete { get; set; }
            public string NewCategory { get; set; }
            public string CategoryRename { get; set; }
            public string CategorySetDefault { get; set; }
            public string NumberOfStyles { get; set; }
            public string CategoryDefault { get; set; }
            public string ChooseCategories { get; set; }
            public string ImportCategoriesTitle { get; set; }
            public string ExportCategoriesTitle { get; set; }
        }

        public class PointSync
        {
            public string Title { get; set; }
            public string TitleViaOtherSubtitle { get; set; }
            public string SyncHelp { get; set; }
            public string SetSyncPoint { get; set; }
            public string RemoveSyncPoint { get; set; }
            public string SyncPointsX { get; set; }
            public string Info { get; set; }
            public string ApplySync { get; set; }
        }

        public class TextToSpeech
        {
            public string Title { get; set; }
            public string Voice { get; set; }
            public string TestVoice { get; set; }
            public string DefaultVoice { get; set; }
            public string AddAudioToVideo { get; set; }
            public string GenerateSpeech { get; set; }
            public string ActorInfo { get; set; }
            public string AdjustingSpeedXOfY { get; set; }
            public string MergingAudioTrackXOfY { get; set; }
            public string GeneratingSpeechFromTextXOfY { get; set; }
            public string ReviewAudioClips { get; set; }
            public string CustomAudioEncoding { get; set; }
            public string ReviewInfo { get; set; }
            public string Play { get; set; }
            public string AutoContinue { get; set; }
            public string Regenerate { get; set; }
            public string Speed { get; set; }
        }

        public class TimedTextSmpteTiming
        {
            public string Title { get; set; }
            public string UseSmpteTiming { get; set; }
            public string SmpteTimingInfo { get; set; }
            public string YesAlways { get; set; }
            public string NoNever { get; set; }
        }

        public class TransportStreamSubtitleChooser
        {
            public string Title { get; set; }
            public string PidLineImage { get; set; }
            public string PidLineTeletext { get; set; }
            public string SubLine { get; set; }
        }

        public class UnknownSubtitle
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public string ImportAsPlainText { get; set; }
        }

        public class VerifyCompleteness
        {
            public string Title { get; set; }
            public string OpenControlSubtitle { get; set; }
            public string ControlSubtitleError { get; set; }
            public string ControlSubtitleX { get; set; }
            public string Coverage { get; set; }
            public string CoveragePercentageX { get; set; }
            public string SortByCoverage { get; set; }
            public string SortByTime { get; set; }
            public string Reload { get; set; }
            public string Insert { get; set; }
            public string InsertAndNext { get; set; }
            public string Dismiss { get; set; }
            public string DismissAndNext { get; set; }
        }

        public class VisualSync
        {
            public string Title { get; set; }
            public string StartScene { get; set; }
            public string EndScene { get; set; }
            public string Synchronize { get; set; }
            public string HalfASecondBack { get; set; }
            public string ThreeSecondsBack { get; set; }
            public string PlayXSecondsAndBack { get; set; }
            public string FindText { get; set; }
            public string GoToSubPosition { get; set; }
            public string KeepChangesTitle { get; set; }
            public string KeepChangesMessage { get; set; }
            public string SynchronizationDone { get; set; }
            public string StartSceneMustComeBeforeEndScene { get; set; }
            public string Tip { get; set; }
        }

        public class VobSubEditCharacters
        {
            public string Title { get; set; }
            public string ChooseCharacter { get; set; }
            public string ImageCompareFiles { get; set; }
            public string CurrentCompareImage { get; set; }
            public string TextAssociatedWithImage { get; set; }
            public string IsItalic { get; set; }
            public string Update { get; set; }
            public string Delete { get; set; }
            public string ImageDoubleSize { get; set; }
            public string ImageFileNotFound { get; set; }
            public string Image { get; set; }
        }

        public class VobSubOcr
        {
            public string Title { get; set; }
            public string TitleBluRay { get; set; }
            public string OcrMethod { get; set; }
            public string OcrViaTesseractVersionX { get; set; }
            public string OcrViaImageCompare { get; set; }
            public string OcrViaModi { get; set; }
            public string OcrViaNOCR { get; set; }
            public string OcrViaCloudVision { get; set; }
            public string TesseractEngineMode { get; set; }
            public string TesseractEngineModeLegacy { get; set; }
            public string TesseractEngineModeNeural { get; set; }
            public string TesseractEngineModeBoth { get; set; }
            public string TesseractEngineModeDefault { get; set; }
            public string Language { get; set; }
            public string ImageDatabase { get; set; }
            public string NoOfPixelsIsSpace { get; set; }
            public string MaxErrorPercent { get; set; }
            public string New { get; set; }
            public string Edit { get; set; }
            public string StartOcr { get; set; }
            public string StartOcrFrom { get; set; }
            public string LoadingVobSubImages { get; set; }
            public string LoadingImageCompareDatabase { get; set; }
            public string ConvertingImageCompareDatabase { get; set; }
            public string SubtitleImage { get; set; }
            public string SubtitleText { get; set; }
            public string UnableToCreateCharacterDatabaseFolder { get; set; }
            public string SubtitleImageXofY { get; set; }
            public string ImagePalette { get; set; }
            public string UseCustomColors { get; set; }
            public string Transparent { get; set; }
            public string TransparentMinAlpha { get; set; }
            public string TransportStream { get; set; }
            public string TransportStreamGrayscale { get; set; }
            public string TransportStreamGetColor { get; set; }
            public string PromptForUnknownWords { get; set; }
            public string TryToGuessUnknownWords { get; set; }
            public string AutoBreakSubtitleIfMoreThanTwoLines { get; set; }
            public string AllFixes { get; set; }
            public string GuessesUsed { get; set; }
            public string UnknownWords { get; set; }
            public string UnknownWordToGuessInLine { get; set; }
            public string OcrAutoCorrectionSpellChecking { get; set; }
            public string FixOcrErrors { get; set; }
            public string ImportTextWithMatchingTimeCodes { get; set; }
            public string ImportNewTimeCodes { get; set; }
            public string SaveSubtitleImageAs { get; set; }
            public string SaveAllSubtitleImagesAsBdnXml { get; set; }
            public string SaveAllSubtitleImagesWithHtml { get; set; }
            public string XImagesSavedInY { get; set; }
            public string DictionaryX { get; set; }
            public string RightToLeft { get; set; }
            public string ShowOnlyForcedSubtitles { get; set; }
            public string UseTimeCodesFromIdx { get; set; }
            public string NoMatch { get; set; }
            public string AutoTransparentBackground { get; set; }
            public string CaptureTopAlign { get; set; }
            public string InspectCompareMatchesForCurrentImage { get; set; }
            public string EditLastAdditions { get; set; }
            public string SetItalicAngle { get; set; }
            public string ItalicAngle { get; set; }
            public string DiscardTitle { get; set; }
            public string DiscardText { get; set; }
            public string MinLineSplitHeight { get; set; }
            public string FallbackToX { get; set; }
            public string ImagePreProcessing { get; set; }
            public string EditImageDb { get; set; }
            public string OcrTraining { get; set; }
            public string SubtitleTrainingFile { get; set; }
            public string LetterCombinations { get; set; }
            public string TrainingOptions { get; set; }
            public string NumberOfSegments { get; set; }
            public string AlsoTrainItalic { get; set; }
            public string AlsoTrainBold { get; set; }
            public string StartTraining { get; set; }
            public string NowTraining { get; set; }
            public string ImagesWithTimeCodesInFileName { get; set; }
            public string CloudVisionApi { get; set; }
            public string ApiKey { get; set; }
            public string SendOriginalImages { get; set; }
            public string SeHandlesTextMerge { get; set; }
        }

        public class VobSubOcrCharacter
        {
            public string Title { get; set; }
            public string ShrinkSelection { get; set; }
            public string ExpandSelection { get; set; }
            public string SubtitleImage { get; set; }
            public string Characters { get; set; }
            public string CharactersAsText { get; set; }
            public string Italic { get; set; }
            public string Abort { get; set; }
            public string Skip { get; set; }
            public string UseOnce { get; set; }
            public string Nordic { get; set; }
            public string Spanish { get; set; }
            public string German { get; set; }
            public string AutoSubmitOnFirstChar { get; set; }
            public string EditLastX { get; set; }
        }

        public class VobSubOcrCharacterInspect
        {
            public string Title { get; set; }
            public string InspectItems { get; set; }
            public string AddBetterMatch { get; set; }
            public string Add { get; set; }
            public string AddBetterMultiMatch { get; set; }
            public string AddOrUpdateMatch { get; set; }
            public string SelectPrevousMatch { get; set; }
            public string SelectNextMatch { get; set; }
            public string JumpPreviousMissingMatch { get; set; }
            public string JumpNextMissingMatch { get; set; }
            public string FocusTextbox { get; set; }
        }

        public class VobSubOcrNewFolder
        {
            public string Title { get; set; }
            public string Message { get; set; }
        }

        public class VobSubOcrSetItalicAngle
        {
            public string Title { get; set; }
            public string Description { get; set; }
        }

        public class OcrPreprocessing
        {
            public string Title { get; set; }
            public string Colors { get; set; }
            public string AdjustAlpha { get; set; }
            public string OriginalImage { get; set; }
            public string PostImage { get; set; }
            public string BinaryThreshold { get; set; }
            public string InvertColors { get; set; }
            public string YellowToWhite { get; set; }
            public string ColorToWhite { get; set; }
            public string ColorToRemove { get; set; }
            public string Cropping { get; set; }
            public string CropTransparentColors { get; set; }
        }

        public class Watermark
        {
            public string Title { get; set; }
            public string WatermarkX { get; set; }
            public string GenerateWatermarkTitle { get; set; }
            public string SpreadOverEntireSubtitle { get; set; }
            public string CurrentLineOnlyX { get; set; }
            public string Generate { get; set; }
            public string Remove { get; set; }
            public string BeforeWatermark { get; set; }
            public string ErrorUnicodeEncodingOnly { get; set; }
        }

        public class Waveform
        {
            public string AddWaveformAndSpectrogram { get; set; }
            public string ClickToAddWaveform { get; set; }
            public string ClickToAddWaveformAndSpectrogram { get; set; }
            public string Seconds { get; set; }
            public string ZoomIn { get; set; }
            public string ZoomOut { get; set; }
            public string AddParagraphHere { get; set; }
            public string AddParagraphHereAndPasteText { get; set; }
            public string SetParagraphAsSelection { get; set; }
            public string FocusTextBox { get; set; }
            public string GoToPrevious { get; set; }
            public string GoToNext { get; set; }
            public string DeleteParagraph { get; set; }
            public string Split { get; set; }
            public string SplitAtCursor { get; set; }
            public string MergeWithPrevious { get; set; }
            public string MergeWithNext { get; set; }
            public string RunWhisperSelectedParagraph { get; set; }
            public string ExtendToPrevious { get; set; }
            public string ExtendToNext { get; set; }
            public string PlaySelection { get; set; }
            public string ShowWaveformAndSpectrogram { get; set; }
            public string ShowWaveformOnly { get; set; }
            public string ShowSpectrogramOnly { get; set; }
            public string AddShotChange { get; set; }
            public string RemoveShotChange { get; set; }
            public string RemoveShotChangesFromSelection { get; set; }
            public string GuessTimeCodes { get; set; }
            public string SeekSilence { get; set; }
            public string InsertSubtitleHere { get; set; }
            public string InsertSubtitleFileHere { get; set; }
            public string CharsSecX { get; set; }
            public string WordsMinX { get; set; }
        }

        public class WaveformGenerateTimeCodes
        {
            public string Title { get; set; }
            public string StartFrom { get; set; }
            public string CurrentVideoPosition { get; set; }
            public string Beginning { get; set; }
            public string DeleteLines { get; set; }
            public string FromCurrentVideoPosition { get; set; }
            public string DetectOptions { get; set; }
            public string ScanBlocksOfMs { get; set; }
            public string BlockAverageVolMin1 { get; set; }
            public string BlockAverageVolMin2 { get; set; }
            public string BlockAverageVolMax1 { get; set; }
            public string BlockAverageVolMax2 { get; set; }
            public string SplitLongLinesAt1 { get; set; }
            public string SplitLongLinesAt2 { get; set; }
            public string Other { get; set; }
        }

        public class WebVttNewVoice
        {
            public string Title { get; set; }
            public string VoiceName { get; set; }
        }

        public class WebVttProperties
        {
            public string UseXTimeStamp { get; set; }
            public string MergeLines { get; set; }
            public string MergeStyleTags { get; set; }
        }

        public class WebVttStyleManager
        {
            public string Title { get; set; }
        }

        public class WhisperAdvanced
        {
            public string Title { get; set; }
            public string CommandLineArguments { get; set; }
            public string Info { get; set; }
            public string Standard { get; set; }
            public string StandardAsia { get; set; }
            public string HighlightCurrentWord { get; set; }
            public string SingleWords { get; set; }
            public string Sentence { get; set; }
        }
    }
}
