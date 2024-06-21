﻿using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;

namespace Nikse.SubtitleEdit.Core.Common
{
    // The settings classes are built for easy xml-serialization (makes save/load code simple)
    // ...but the built-in serialization is too slow - so a custom (de-)serialization has been used!

    public class RecentFileEntry
    {
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string VideoFileName { get; set; }
        public int AudioTrack { get; set; }
        public int FirstVisibleIndex { get; set; }
        public int FirstSelectedIndex { get; set; }
        public long VideoOffsetInMs { get; set; }
        public bool VideoIsSmpte { get; set; }
    }

    public class RecentFilesSettings
    {
        private const int MaxRecentFiles = 25;

        [XmlArrayItem("FileName")]
        public List<RecentFileEntry> Files { get; set; }

        public RecentFilesSettings()
        {
            Files = new List<RecentFileEntry>();
        }

        public void Add(string fileName, int firstVisibleIndex, int firstSelectedIndex, string videoFileName, int audioTrack, string originalFileName, long videoOffset, bool isSmpte)
        {
            Files = Files.Where(p => !string.IsNullOrEmpty(p.FileName)).ToList();

            if (string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(originalFileName))
            {
                fileName = originalFileName;
                originalFileName = null;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                Files.Insert(0, new RecentFileEntry { FileName = string.Empty });
                return;
            }

            var existingEntry = GetRecentFile(fileName, originalFileName);
            if (existingEntry == null)
            {
                Files.Insert(0, new RecentFileEntry { FileName = fileName ?? string.Empty, FirstVisibleIndex = -1, FirstSelectedIndex = -1, VideoFileName = videoFileName, AudioTrack = audioTrack, OriginalFileName = originalFileName });
            }
            else
            {
                Files.Remove(existingEntry);
                existingEntry.FirstSelectedIndex = firstSelectedIndex;
                existingEntry.FirstVisibleIndex = firstVisibleIndex;
                existingEntry.VideoFileName = videoFileName;
                existingEntry.AudioTrack = audioTrack;
                existingEntry.OriginalFileName = originalFileName;
                existingEntry.VideoOffsetInMs = videoOffset;
                existingEntry.VideoIsSmpte = isSmpte;
                Files.Insert(0, existingEntry);
            }
            Files = Files.Take(MaxRecentFiles).ToList();
        }

        public void Add(string fileName, string videoFileName, int audioTrack, string originalFileName)
        {
            Files = Files.Where(p => !string.IsNullOrEmpty(p.FileName)).ToList();

            var existingEntry = GetRecentFile(fileName, originalFileName);
            if (existingEntry == null)
            {
                Files.Insert(0, new RecentFileEntry { FileName = fileName ?? string.Empty, FirstVisibleIndex = -1, FirstSelectedIndex = -1, VideoFileName = videoFileName, AudioTrack = audioTrack, OriginalFileName = originalFileName });
            }
            else
            {
                Files.Remove(existingEntry);
                Files.Insert(0, existingEntry);
            }
            Files = Files.Take(MaxRecentFiles).ToList();
        }

        private RecentFileEntry GetRecentFile(string fileName, string originalFileName)
        {
            RecentFileEntry existingEntry;
            if (string.IsNullOrEmpty(originalFileName))
            {
                existingEntry = Files.Find(p => !string.IsNullOrEmpty(p.FileName) &&
                                                string.IsNullOrEmpty(p.OriginalFileName) &&
                                                p.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                existingEntry = Files.Find(p => !string.IsNullOrEmpty(p.FileName) &&
                                                !string.IsNullOrEmpty(p.OriginalFileName) &&
                                                p.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) &&
                                                p.OriginalFileName.Equals(originalFileName, StringComparison.OrdinalIgnoreCase));
            }
            return existingEntry;
        }
    }

    public class ToolsSettings
    {
        public List<AssaTemplateItem> AssaTagTemplates { get; set; }
        public int StartSceneIndex { get; set; }
        public int EndSceneIndex { get; set; }
        public int VerifyPlaySeconds { get; set; }
        public bool FixShortDisplayTimesAllowMoveStartTime { get; set; }
        public bool RemoveEmptyLinesBetweenText { get; set; }
        public string MusicSymbol { get; set; }
        public string MusicSymbolReplace { get; set; }
        public string UnicodeSymbolsToInsert { get; set; }
        public bool SpellCheckAutoChangeNameCasing { get; set; }
        public bool SpellCheckUseLargerFont { get; set; }
        public bool SpellCheckAutoChangeNamesUseSuggestions { get; set; }
        public string SpellCheckSearchEngine { get; set; }
        public bool CheckOneLetterWords { get; set; }
        public bool SpellCheckEnglishAllowInQuoteAsIng { get; set; }
        public bool RememberUseAlwaysList { get; set; }
        public bool LiveSpellCheck { get; set; }
        public bool SpellCheckShowCompletedMessage { get; set; }
        public bool OcrFixUseHardcodedRules { get; set; }
        public bool OcrGoogleCloudVisionSeHandlesTextMerge { get; set; }
        public int OcrBinaryImageCompareRgbThreshold { get; set; }
        public int OcrTesseract4RgbThreshold { get; set; }
        public string OcrAddLetterRow1 { get; set; }
        public string OcrAddLetterRow2 { get; set; }
        public string OcrTrainFonts { get; set; }
        public string OcrTrainMergedLetters { get; set; }
        public string OcrTrainSrtFile { get; set; }
        public bool OcrUseWordSplitList { get; set; }
        public bool OcrUseWordSplitListAvoidPropercase { get; set; }
        public string BDOpenIn { get; set; }
        public string MicrosoftBingApiId { get; set; }
        public string MicrosoftTranslatorApiKey { get; set; }
        public string MicrosoftTranslatorTokenEndpoint { get; set; }
        public string MicrosoftTranslatorCategory { get; set; }
        public string GoogleApiV2Key { get; set; }
        public bool GoogleTranslateNoKeyWarningShow { get; set; }
        public int GoogleApiV1ChunkSize { get; set; }
        public string GoogleTranslateLastSourceLanguage { get; set; }
        public string GoogleTranslateLastTargetLanguage { get; set; }
        public string AutoTranslateLastName { get; set; }
        public string AutoTranslateLastUrl { get; set; }
        public string AutoTranslateNllbApiUrl { get; set; }
        public string AutoTranslateNllbServeUrl { get; set; }
        public string AutoTranslateNllbServeModel { get; set; }
        public string AutoTranslateLibreUrl { get; set; }
        public string AutoTranslateLibreApiKey { get; set; }
        public string AutoTranslateMyMemoryApiKey { get; set; }
        public string AutoTranslateSeamlessM4TUrl { get; set; }
        public string AutoTranslateDeepLApiKey { get; set; }
        public string AutoTranslateDeepLUrl { get; set; }
        public string AutoTranslatePapagoApiKeyId { get; set; }
        public string AutoTranslatePapagoApiKey { get; set; }
        public string AutoTranslateDeepLFormality { get; set; }
        public bool TranslateAllowSplit { get; set; }
        public string TranslateLastService { get; set; }
        public string TranslateMergeStrategy { get; set; }
        public string TranslateViaCopyPasteSeparator { get; set; }
        public int TranslateViaCopyPasteMaxSize { get; set; }
        public bool TranslateViaCopyPasteAutoCopyToClipboard { get; set; }
        public string ChatGptUrl { get; set; }
        public string ChatGptPrompt { get; set; }
        public string ChatGptApiKey { get; set; }
        public string ChatGptModel { get; set; }
        public string LmStudioApiUrl { get; set; }
        public string LmStudioModel { get; set; }
        public string LmStudioPrompt { get; set; }
        public string OllamaApiUrl { get; set; }
        public string OllamaModels { get; set; }
        public string OllamaModel { get; set; }
        public string OllamaPrompt { get; set; }

        public string AnthropicApiUrl { get; set; }
        public string AnthropicPrompt { get; set; }
        public string AnthropicApiKey { get; set; }
        public string AnthropicApiModel { get; set; }
        public int AutoTranslateDelaySeconds { get; set; }
        public int AutoTranslateMaxBytes { get; set; }
        public string AutoTranslateStrategy { get; set; }
        public string GeminiProApiKey { get; set; }
        public string TextToSpeechEngine { get; set; }
        public string TextToSpeechLastVoice { get; set; }
        public string TextToSpeechElevenLabsApiKey { get; set; }
        public string TextToSpeechAzureApiKey { get; set; }
        public string TextToSpeechAzureRegion { get; set; }
        public bool TextToSpeechPreview { get; set; }
        public bool TextToSpeechCustomAudio { get; set; }
        public bool TextToSpeechCustomAudioStereo { get; set; }
        public string TextToSpeechCustomAudioEncoding { get; set; }
        public bool TextToSpeechAddToVideoFile { get; set; }
        public bool ListViewSyntaxColorDurationSmall { get; set; }
        public bool ListViewSyntaxColorDurationBig { get; set; }
        public bool ListViewSyntaxColorOverlap { get; set; }
        public bool ListViewSyntaxColorLongLines { get; set; }
        public bool ListViewSyntaxColorWideLines { get; set; }
        public bool ListViewSyntaxColorGap { get; set; }
        public bool ListViewSyntaxMoreThanXLines { get; set; }
        public Color ListViewSyntaxErrorColor { get; set; }
        public Color ListViewUnfocusedSelectedColor { get; set; }
        public Color Color1 { get; set; }
        public Color Color2 { get; set; }
        public Color Color3 { get; set; }
        public Color Color4 { get; set; }
        public Color Color5 { get; set; }
        public Color Color6 { get; set; }
        public Color Color7 { get; set; }
        public Color Color8 { get; set; }
        public bool ListViewShowColumnStartTime { get; set; }
        public bool ListViewShowColumnEndTime { get; set; }
        public bool ListViewShowColumnDuration { get; set; }
        public bool ListViewShowColumnCharsPerSec { get; set; }
        public bool ListViewShowColumnWordsPerMin { get; set; }
        public bool ListViewShowColumnGap { get; set; }
        public bool ListViewShowColumnActor { get; set; }
        public bool ListViewShowColumnRegion { get; set; }
        public bool ListViewMultipleReplaceShowColumnRuleInfo { get; set; }
        public bool SplitAdvanced { get; set; }
        public string SplitOutputFolder { get; set; }
        public int SplitNumberOfParts { get; set; }
        public string SplitVia { get; set; }
        public bool JoinCorrectTimeCodes { get; set; }
        public int JoinAddMs { get; set; }
        public int SplitLongLinesMax { get; set; }
        public string LastShowEarlierOrLaterSelection { get; set; }
        public string NewEmptyTranslationText { get; set; }
        public string BatchConvertOutputFolder { get; set; }
        public bool BatchConvertOverwriteExisting { get; set; }
        public bool BatchConvertSaveInSourceFolder { get; set; }
        public bool BatchConvertRemoveFormatting { get; set; }
        public bool BatchConvertRemoveFormattingAll { get; set; }
        public bool BatchConvertRemoveFormattingItalic { get; set; }
        public bool BatchConvertRemoveFormattingBold { get; set; }
        public bool BatchConvertRemoveFormattingUnderline { get; set; }
        public bool BatchConvertRemoveFormattingFontName { get; set; }
        public bool BatchConvertRemoveFormattingColor { get; set; }
        public bool BatchConvertRemoveFormattingAlignment { get; set; }
        public bool BatchConvertRemoveStyle { get; set; }
        public bool BatchConvertBridgeGaps { get; set; }
        public bool BatchConvertFixCasing { get; set; }
        public bool BatchConvertRemoveTextForHI { get; set; }
        public bool BatchConvertConvertColorsToDialog { get; set; }
        public bool BatchConvertBeautifyTimeCodes { get; set; }
        public bool BatchConvertAutoTranslate { get; set; }
        public bool BatchConvertFixCommonErrors { get; set; }
        public bool BatchConvertMultipleReplace { get; set; }
        public bool BatchConvertFixRtl { get; set; }
        public string BatchConvertFixRtlMode { get; set; }
        public bool BatchConvertSplitLongLines { get; set; }
        public bool BatchConvertAutoBalance { get; set; }
        public bool BatchConvertSetMinDisplayTimeBetweenSubtitles { get; set; }
        public bool BatchConvertMergeShortLines { get; set; }
        public bool BatchConvertRemoveLineBreaks { get; set; }
        public bool BatchConvertMergeSameText { get; set; }
        public bool BatchConvertMergeSameTimeCodes { get; set; }
        public bool BatchConvertChangeFrameRate { get; set; }
        public bool BatchConvertChangeSpeed { get; set; }
        public bool BatchConvertAdjustDisplayDuration { get; set; }
        public bool BatchConvertApplyDurationLimits { get; set; }
        public bool BatchConvertDeleteLines { get; set; }
        public bool BatchConvertAssaChangeRes { get; set; }
        public bool BatchConvertSortBy { get; set; }
        public string BatchConvertSortByChoice { get; set; }
        public bool BatchConvertOffsetTimeCodes { get; set; }
        public bool BatchConvertScanFolderIncludeVideo { get; set; }
        public string BatchConvertLanguage { get; set; }
        public string BatchConvertFormat { get; set; }
        public string BatchConvertAssStyles { get; set; }
        public string BatchConvertSsaStyles { get; set; }
        public bool BatchConvertUseStyleFromSource { get; set; }
        public string BatchConvertExportCustomTextTemplate { get; set; }
        public bool BatchConvertTsOverrideXPosition { get; set; }
        public bool BatchConvertTsOverrideYPosition { get; set; }
        public int BatchConvertTsOverrideBottomMargin { get; set; }
        public string BatchConvertTsOverrideHAlign { get; set; }
        public int BatchConvertTsOverrideHMargin { get; set; }
        public bool BatchConvertTsOverrideScreenSize { get; set; }
        public int BatchConvertTsScreenWidth { get; set; }
        public int BatchConvertTsScreenHeight { get; set; }
        public string BatchConvertTsFileNameAppend { get; set; }
        public bool BatchConvertTsOnlyTeletext { get; set; }
        public string BatchConvertMkvLanguageCodeStyle { get; set; }
        public string BatchConvertOcrEngine { get; set; }
        public string BatchConvertOcrLanguage { get; set; }
        public string WaveformBatchLastFolder { get; set; }
        public string ModifySelectionText { get; set; }
        public string ModifySelectionRule { get; set; }
        public bool ModifySelectionCaseSensitive { get; set; }
        public string ExportVobSubFontName { get; set; }
        public int ExportVobSubFontSize { get; set; }
        public string ExportVobSubVideoResolution { get; set; }
        public string ExportVobSubLanguage { get; set; }
        public bool ExportVobSubSimpleRendering { get; set; }
        public bool ExportVobAntiAliasingWithTransparency { get; set; }
        public string ExportBluRayFontName { get; set; }
        public int ExportBluRayFontSize { get; set; }
        public string ExportFcpFontName { get; set; }
        public string ExportFontNameOther { get; set; }
        public int ExportFcpFontSize { get; set; }
        public string ExportFcpImageType { get; set; }
        public string ExportFcpPalNtsc { get; set; }
        public string ExportBdnXmlImageType { get; set; }
        public int ExportLastFontSize { get; set; }
        public int ExportLastLineHeight { get; set; }
        public int ExportLastBorderWidth { get; set; }
        public bool ExportLastFontBold { get; set; }
        public string ExportBluRayVideoResolution { get; set; }
        public string ExportFcpVideoResolution { get; set; }
        public Color ExportFontColor { get; set; }
        public Color ExportBorderColor { get; set; }
        public Color ExportShadowColor { get; set; }
        public int ExportBoxBorderSize { get; set; }
        public string ExportBottomMarginUnit { get; set; }
        public int ExportBottomMarginPercent { get; set; }
        public int ExportBottomMarginPixels { get; set; }
        public string ExportLeftRightMarginUnit { get; set; }
        public int ExportLeftRightMarginPercent { get; set; }
        public int ExportLeftRightMarginPixels { get; set; }
        public int ExportHorizontalAlignment { get; set; }
        public int ExportBluRayBottomMarginPercent { get; set; }
        public int ExportBluRayBottomMarginPixels { get; set; }
        public int ExportBluRayShadow { get; set; }
        public bool ExportBluRayRemoveSmallGaps { get; set; }
        public string ExportCdgBackgroundImage { get; set; }
        public int ExportCdgMarginLeft { get; set; }
        public int ExportCdgMarginBottom { get; set; }
        public string ExportCdgFormat { get; set; }
        public int Export3DType { get; set; }
        public int Export3DDepth { get; set; }
        public int ExportLastShadowTransparency { get; set; }
        public double ExportLastFrameRate { get; set; }
        public bool ExportFullFrame { get; set; }
        public bool ExportFcpFullPathUrl { get; set; }
        public string ExportPenLineJoin { get; set; }
        public Color BinEditBackgroundColor { get; set; }
        public Color BinEditImageBackgroundColor { get; set; }
        public int BinEditTopMargin { get; set; }
        public int BinEditBottomMargin { get; set; }
        public int BinEditLeftMargin { get; set; }
        public int BinEditRightMargin { get; set; }
        public string BinEditStartPosition { get; set; }
        public string BinEditStartSize { get; set; }
        public bool BinEditShowColumnGap { get; set; }
        public bool FixCommonErrorsFixOverlapAllowEqualEndStart { get; set; }
        public bool FixCommonErrorsSkipStepOne { get; set; }
        public string ImportTextSplitting { get; set; }
        public string ImportTextSplittingLineMode { get; set; }
        public string ImportTextLineBreak { get; set; }
        public bool ImportTextMergeShortLines { get; set; }
        public bool ImportTextRemoveEmptyLines { get; set; }
        public bool ImportTextAutoSplitAtBlank { get; set; }
        public bool ImportTextRemoveLinesNoLetters { get; set; }
        public bool ImportTextGenerateTimeCodes { get; set; }
        public bool ImportTextTakeTimeCodeFromFileName { get; set; }
        public bool ImportTextAutoBreak { get; set; }
        public bool ImportTextAutoBreakAtEnd { get; set; }
        public decimal ImportTextGap { get; set; }
        public decimal ImportTextAutoSplitNumberOfLines { get; set; }
        public string ImportTextAutoBreakAtEndMarkerText { get; set; }
        public bool ImportTextDurationAuto { get; set; }
        public decimal ImportTextFixedDuration { get; set; }
        public string GenerateTimeCodePatterns { get; set; }
        public string MusicSymbolStyle { get; set; }
        public int BridgeGapMilliseconds { get; set; }
        public int BridgeGapMillisecondsMinGap { get; set; }
        public string ExportCustomTemplates { get; set; }
        public string ChangeCasingChoice { get; set; }
        public bool ChangeCasingNormalFixNames { get; set; }
        public bool ChangeCasingNormalOnlyUppercase { get; set; }
        public bool UseNoLineBreakAfter { get; set; }
        public string NoLineBreakAfterEnglish { get; set; }
        public List<string> FindHistory { get; set; }
        public string ReplaceIn { get; set; }
        public string ExportTextFormatText { get; set; }
        public bool ExportTextRemoveStyling { get; set; }
        public bool ExportTextShowLineNumbers { get; set; }
        public bool ExportTextShowLineNumbersNewLine { get; set; }
        public bool ExportTextShowTimeCodes { get; set; }
        public bool ExportTextShowTimeCodesNewLine { get; set; }
        public bool ExportTextNewLineAfterText { get; set; }
        public bool ExportTextNewLineBetweenSubtitles { get; set; }
        public string ExportTextTimeCodeFormat { get; set; }
        public string ExportTextTimeCodeSeparator { get; set; }
        public bool VideoOffsetKeepTimeCodes { get; set; }
        public int MoveStartEndMs { get; set; }
        public decimal AdjustDurationSeconds { get; set; }
        public int AdjustDurationPercent { get; set; }
        public string AdjustDurationLast { get; set; }
        public bool AdjustDurationExtendOnly { get; set; }
        public bool AdjustDurationExtendEnforceDurationLimits { get; set; }
        public bool AdjustDurationExtendCheckShotChanges { get; set; }
        public bool ChangeSpeedAllowOverlap { get; set; }
        public bool AutoBreakCommaBreakEarly { get; set; }
        public bool AutoBreakDashEarly { get; set; }
        public bool AutoBreakLineEndingEarly { get; set; }
        public bool AutoBreakUsePixelWidth { get; set; }
        public bool AutoBreakPreferBottomHeavy { get; set; }
        public double AutoBreakPreferBottomPercent { get; set; }
        public bool ApplyMinimumDurationLimit { get; set; }
        public bool ApplyMinimumDurationLimitCheckShotChanges { get; set; }
        public bool ApplyMaximumDurationLimit { get; set; }
        public int MergeShortLinesMaxGap { get; set; }
        public int MergeShortLinesMaxChars { get; set; }
        public bool MergeShortLinesOnlyContinuous { get; set; }
        public int MergeTextWithSameTimeCodesMaxGap { get; set; }
        public bool MergeTextWithSameTimeCodesMakeDialog { get; set; }
        public bool MergeTextWithSameTimeCodesReBreakLines { get; set; }
        public int MergeLinesWithSameTextMaxMs { get; set; }
        public bool MergeLinesWithSameTextIncrement { get; set; }
        public bool ConvertColorsToDialogRemoveColorTags { get; set; }
        public bool ConvertColorsToDialogAddNewLines { get; set; }
        public bool ConvertColorsToDialogReBreakLines { get; set; }
        public string ColumnPasteColumn { get; set; }
        public string ColumnPasteOverwriteMode { get; set; }
        public string AssaAttachmentFontTextPreview { get; set; }
        public string AssaSetPositionTarget { get; set; }
        public string VisualSyncStartSize { get; set; }
        public Color BlankVideoColor { get; set; }
        public bool BlankVideoUseCheckeredImage { get; set; }
        public int BlankVideoMinutes { get; set; }
        public decimal BlankVideoFrameRate { get; set; }
        public Color AssaProgressBarForeColor { get; set; }
        public Color AssaProgressBarBackColor { get; set; }
        public Color AssaProgressBarTextColor { get; set; }
        public int AssaProgressBarHeight { get; set; }
        public int AssaProgressBarSplitterWidth { get; set; }
        public int AssaProgressBarSplitterHeight { get; set; }
        public string AssaProgressBarFontName { get; set; }
        public int AssaProgressBarFontSize { get; set; }
        public bool AssaProgressBarTopAlign { get; set; }
        public string AssaProgressBarTextAlign { get; set; }


        public int AssaBgBoxPaddingLeft { get; set; }
        public int AssaBgBoxPaddingRight { get; set; }
        public int AssaBgBoxPaddingTop { get; set; }
        public int AssaBgBoxPaddingBottom { get; set; }
        public int AssaBgBoxDrawingMarginV { get; set; }
        public int AssaBgBoxDrawingMarginH { get; set; }
        public string AssaBgBoxDrawingAlignment { get; set; }
        public Color AssaBgBoxColor { get; set; }
        public Color AssaBgBoxOutlineColor { get; set; }
        public Color AssaBgBoxShadowColor { get; set; }
        public Color AssaBgBoxTransparentColor { get; set; }
        public string AssaBgBoxStyle { get; set; }
        public int AssaBgBoxStyleRadius { get; set; }
        public int AssaBgBoxStyleCircleAdjustY { get; set; }
        public int AssaBgBoxStyleSpikesStep { get; set; }
        public int AssaBgBoxStyleSpikesHeight { get; set; }
        public int AssaBgBoxStyleBubblesStep { get; set; }
        public int AssaBgBoxStyleBubblesHeight { get; set; }
        public int AssaBgBoxOutlineWidth { get; set; }
        public int AssaBgBoxLayer { get; set; }
        public string AssaBgBoxDrawing { get; set; }
        public bool AssaBgBoxDrawingFileWatch { get; set; }
        public bool AssaBgBoxDrawingOnly { get; set; }


        public string GenVideoFontName { get; set; }
        public bool GenVideoFontBold { get; set; }
        public decimal GenVideoOutline { get; set; }
        public int GenVideoFontSize { get; set; }
        public string GenVideoEncoding { get; set; }
        public string GenVideoPreset { get; set; }
        public string GenVideoCrf { get; set; }
        public string GenVideoTune { get; set; }
        public string GenVideoAudioEncoding { get; set; }
        public bool GenVideoAudioForceStereo { get; set; }
        public string GenVideoAudioSampleRate { get; set; }
        public bool GenVideoTargetFileSize { get; set; }
        public float GenVideoFontSizePercentOfHeight { get; set; }
        public bool GenVideoNonAssaBox { get; set; }
        public Color GenVideoNonAssaBoxColor { get; set; }
        public Color GenVideoNonAssaTextColor { get; set; }
        public bool GenVideoNonAssaAlignRight { get; set; }
        public bool GenVideoNonAssaFixRtlUnicode { get; set; }
        public string GenVideoEmbedOutputExt { get; set; }
        public string GenVideoEmbedOutputSuffix { get; set; }
        public string GenVideoEmbedOutputReplace { get; set; }
        public bool GenVideoDeleteInputVideoFile { get; set; }
        public bool GenVideoUseOutputFolder { get; set; }
        public string GenVideoOutputFolder { get; set; }
        public string GenVideoOutputFileSuffix { get; set; }

        public bool VoskPostProcessing { get; set; }
        public string VoskModel { get; set; }
        public string WhisperChoice { get; set; }
        public bool WhisperIgnoreVersion { get; set; }

        public bool WhisperDeleteTempFiles { get; set; }
        public string WhisperModel { get; set; }
        public string WhisperLanguageCode { get; set; }
        public string WhisperLocation { get; set; }
        public string WhisperCtranslate2Location { get; set; }
        public string WhisperPurfviewFasterWhisperLocation { get; set; }
        public string WhisperPurfviewFasterWhisperDefaultCmd { get; set; }
        public string WhisperXLocation { get; set; }
        public string WhisperStableTsLocation { get; set; }
        public string WhisperCppModelLocation { get; set; }
        public string WhisperExtraSettings { get; set; }
        public string WhisperExtraSettingsHistory { get; set; }
        public bool WhisperAutoAdjustTimings { get; set; }
        public bool WhisperUseLineMaxChars { get; set; }
        public bool WhisperPostProcessingAddPeriods { get; set; }
        public bool WhisperPostProcessingMergeLines { get; set; }
        public bool WhisperPostProcessingSplitLines { get; set; }
        public bool WhisperPostProcessingFixCasing { get; set; }
        public bool WhisperPostProcessingFixShortDuration { get; set; }
        public int AudioToTextLineMaxChars { get; set; }
        public int AudioToTextLineMaxCharsJp { get; set; }
        public int AudioToTextLineMaxCharsCn { get; set; }
        public int BreakLinesLongerThan { get; set; }
        public int UnbreakLinesLongerThan { get; set; }

        public ToolsSettings()
        {
            AssaTagTemplates = new List<AssaTemplateItem>();
            StartSceneIndex = 1;
            EndSceneIndex = 1;
            VerifyPlaySeconds = 2;
            FixShortDisplayTimesAllowMoveStartTime = false;
            RemoveEmptyLinesBetweenText = true;
            MusicSymbol = "♪";
            MusicSymbolReplace = "â™ª,â™," + // ♪ + ♫ in UTF-8 opened as ANSI
                                 "<s M/>,<s m/>," + // music symbols by subtitle creator
                                 "#,*,¶"; // common music symbols
            UnicodeSymbolsToInsert = "♪;♫;°;☺;☹;♥;©;☮;☯;Σ;∞;≡;⇒;π";
            SpellCheckAutoChangeNameCasing = false;
            SpellCheckAutoChangeNamesUseSuggestions = false;
            OcrFixUseHardcodedRules = true;
            OcrGoogleCloudVisionSeHandlesTextMerge = true;
            OcrBinaryImageCompareRgbThreshold = 200;
            OcrTesseract4RgbThreshold = 200;
            OcrAddLetterRow1 = "♪;á;é;í;ó;ö;ő;ú;ü;ű;ç;ñ;å;¿";
            OcrAddLetterRow2 = "♫;Á;É;Í;Ó;Ö;Ő;Ú;Ü;Ű;Ç;Ñ;Å;¡";
            OcrTrainFonts = "Arial;Calibri;Corbel;Futura Std Book;Futura Bis;Helvetica Neue;Lucida Console;Tahoma;Trebuchet MS;Verdana";
            OcrTrainMergedLetters = "ff ft fi fj fy fl rf rt rv rw ry rt rz ryt tt TV tw yt yw wy wf ryt xy";
            OcrUseWordSplitList = true;
            OcrUseWordSplitListAvoidPropercase = true;
            MicrosoftTranslatorTokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
            GoogleTranslateNoKeyWarningShow = true;
            GoogleApiV1ChunkSize = 1500;
            GoogleTranslateLastTargetLanguage = "en";
            AutoTranslateNllbServeUrl = "http://127.0.0.1:6060/";
            AutoTranslateNllbApiUrl = "http://localhost:7860/api/v2/";
            AutoTranslateLibreUrl = "http://localhost:5000/";
            AutoTranslateSeamlessM4TUrl = "http://localhost:5000/";
            AutoTranslateDeepLUrl = "https://api-free.deepl.com/";
            ChatGptUrl = "https://api.openai.com/v1/chat/completions";
            ChatGptPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments:";
            ChatGptModel = "gpt-4o";
            LmStudioPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments:";
            OllamaApiUrl = "http://localhost:11434/api/generate";
            OllamaModels = "llama3,llama2,mistral,dolphin-phi,phi,neural-chat,starling-lm,codellama,llama2-uncensored,llama2:13b,llama2:70b,orca-mini,vicuna,llava,gemma:2b,gemma:7b";
            OllamaModel = "llama3";
            OllamaPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments or notes:";
            AnthropicApiUrl = "https://api.anthropic.com/v1/messages";
            AnthropicPrompt = "Translate from {0} to {1}, keep sentences in {1} as they are, do not censor the translation, give only the output without comments:";
            AnthropicApiModel = "claude-3-5-sonnet-20240620";
            TextToSpeechAzureRegion = "westeurope";
            AutoTranslateMaxBytes = 2000;
            TextToSpeechAddToVideoFile = true;
            TranslateAllowSplit = true;
            TranslateViaCopyPasteAutoCopyToClipboard = true;
            TranslateViaCopyPasteMaxSize = 5000;
            TranslateViaCopyPasteSeparator = ".";
            CheckOneLetterWords = true;
            SpellCheckEnglishAllowInQuoteAsIng = false;
            SpellCheckShowCompletedMessage = true;
            ListViewSyntaxColorDurationSmall = true;
            ListViewSyntaxColorDurationBig = true;
            ListViewSyntaxColorOverlap = true;
            ListViewSyntaxColorLongLines = true;
            ListViewSyntaxColorWideLines = false;
            ListViewSyntaxMoreThanXLines = true;
            ListViewSyntaxColorGap = true;
            ListViewSyntaxErrorColor = Color.FromArgb(255, 180, 150);
            ListViewUnfocusedSelectedColor = Color.LightBlue;
            Color1 = Color.Yellow;
            Color2 = Color.FromArgb(byte.MaxValue, 0, 0);
            Color3 = Color.FromArgb(0, byte.MaxValue, 0);
            Color4 = Color.Cyan;
            Color5 = Color.Black;
            Color6 = Color.White;
            Color7 = Color.Orange;
            Color8 = Color.Pink;
            ListViewShowColumnStartTime = true;
            ListViewShowColumnEndTime = true;
            ListViewShowColumnDuration = true;
            SplitAdvanced = false;
            SplitNumberOfParts = 3;
            SplitVia = "Lines";
            JoinCorrectTimeCodes = true;
            SplitLongLinesMax = 90;
            NewEmptyTranslationText = string.Empty;
            BatchConvertLanguage = string.Empty;
            BatchConvertTsOverrideBottomMargin = 5; // pct
            BatchConvertTsScreenWidth = 1920;
            BatchConvertTsScreenHeight = 1080;
            BatchConvertOcrEngine = "Tesseract";
            BatchConvertOcrLanguage = "en";
            BatchConvertTsOverrideHAlign = "center"; // left center right
            BatchConvertTsOverrideHMargin = 5; // pct
            BatchConvertTsFileNameAppend = ".{two-letter-country-code}";
            BatchConvertMkvLanguageCodeStyle = "2";
            ModifySelectionRule = "Contains";
            ModifySelectionText = string.Empty;
            ImportTextDurationAuto = true;
            ImportTextGap = 84;
            ImportTextFixedDuration = 2500;
            GenerateTimeCodePatterns = "HH:mm:ss;yyyy-MM-dd;dddd dd MMMM yyyy <br>HH:mm:ss;dddd dd MMMM yyyy <br>hh:mm:ss tt;s";
            MusicSymbolStyle = "Double"; // 'Double' or 'Single'
            ExportFontColor = Color.White;
            ExportBorderColor = Color.FromArgb(255, 0, 0, 0);
            ExportShadowColor = Color.FromArgb(255, 0, 0, 0);
            ExportBoxBorderSize = 8;
            ExportBottomMarginUnit = "%";
            ExportBottomMarginPercent = 5;
            ExportBottomMarginPixels = 15;
            ExportLeftRightMarginUnit = "%";
            ExportLeftRightMarginPercent = 5;
            ExportLeftRightMarginPixels = 15;
            ExportHorizontalAlignment = 1; // 1=center (0=left, 2=right)
            ExportVobSubSimpleRendering = false;
            ExportVobAntiAliasingWithTransparency = true;
            ExportBluRayBottomMarginPercent = 5;
            ExportBluRayBottomMarginPixels = 20;
            ExportBluRayShadow = 1;
            Export3DType = 0;
            Export3DDepth = 0;
            ExportCdgMarginLeft = 160;
            ExportCdgMarginBottom = 67;
            ExportLastShadowTransparency = 200;
            ExportLastFrameRate = 24.0d;
            ExportFullFrame = false;
            ExportPenLineJoin = "Round";
            ExportFcpImageType = "Bmp";
            ExportFcpPalNtsc = "PAL";
            ExportLastBorderWidth = 4;
            BinEditBackgroundColor = Color.Black;
            BinEditImageBackgroundColor = Color.Blue;
            BinEditTopMargin = 10;
            BinEditBottomMargin = 10;
            BinEditLeftMargin = 10;
            BinEditRightMargin = 10;
            BridgeGapMilliseconds = 100;
            BridgeGapMillisecondsMinGap = 24;
            ChangeCasingNormalFixNames = true;
            ExportCustomTemplates = "SubRipÆÆ{number}\r\n{start} --> {end}\r\n{text}\r\n\r\nÆhh:mm:ss,zzzÆ[Do not modify]ÆÆsrtæMicroDVDÆÆ{{start}}{{end}}{text}\r\nÆffÆ||ÆÆsub";
            UseNoLineBreakAfter = false;
            NoLineBreakAfterEnglish = " Mrs.; Ms.; Mr.; Dr.; a; an; the; my; my own; your; his; our; their; it's; is; are;'s; 're; would;'ll;'ve;'d; will; that; which; who; whom; whose; whichever; whoever; wherever; each; either; every; all; both; few; many; sevaral; all; any; most; been; been doing; none; some; my own; your own; his own; her own; our own; their own; I; she; he; as per; as regards; into; onto; than; where as; abaft; aboard; about; above; across; afore; after; against; along; alongside; amid; amidst; among; amongst; anenst; apropos; apud; around; as; aside; astride; at; athwart; atop; barring; before; behind; below; beneath; beside; besides; between; betwixt; beyond; but; by; circa; ca; concerning; despite; down; during; except; excluding; following; for; forenenst; from; given; in; including; inside; into; lest; like; minus; modulo; near; next; of; off; on; onto; opposite; out; outside; over; pace; past; per; plus; pro; qua; regarding; round; sans; save; since; than; through; thru; throughout; thruout; till; to; toward; towards; under; underneath; unlike; until; unto; up; upon; versus; vs; via; vice; with; within; without; considering; respecting; one; two; another; three; our; five; six; seven; eight; nine; ten; eleven; twelve; thirteen; fourteen; fifteen; sixteen; seventeen; eighteen; nineteen; twenty; thirty; forty; fifty; sixty; seventy; eighty; ninety; hundred; thousand; million; billion; trillion; while; however; what; zero; little; enough; after; although; and; as; if; though; although; because; before; both; but; even; how; than; nor; or; only; unless; until; yet; was; were";
            FindHistory = new List<string>();
            ExportTextFormatText = "None";
            ExportTextRemoveStyling = true;
            ExportTextShowLineNumbersNewLine = true;
            ExportTextShowTimeCodesNewLine = true;
            ExportTextNewLineAfterText = true;
            ExportTextNewLineBetweenSubtitles = true;
            ImportTextLineBreak = "|";
            ImportTextAutoSplitNumberOfLines = 2;
            ImportTextAutoSplitAtBlank = true;
            ImportTextAutoBreakAtEndMarkerText = ".!?";
            ImportTextAutoBreakAtEnd = true;
            MoveStartEndMs = 100;
            AdjustDurationSeconds = 0.1m;
            AdjustDurationPercent = 120;
            AdjustDurationExtendOnly = true;
            AdjustDurationExtendEnforceDurationLimits = true;
            AdjustDurationExtendCheckShotChanges = true;
            AutoBreakCommaBreakEarly = false;
            AutoBreakDashEarly = true;
            AutoBreakLineEndingEarly = false;
            AutoBreakUsePixelWidth = true;
            AutoBreakPreferBottomHeavy = true;
            AutoBreakPreferBottomPercent = 5;
            ApplyMinimumDurationLimit = true;
            ApplyMinimumDurationLimitCheckShotChanges = true;
            ApplyMaximumDurationLimit = true;
            MergeShortLinesMaxGap = 250;
            MergeShortLinesMaxChars = 55;
            MergeShortLinesOnlyContinuous = true;
            MergeTextWithSameTimeCodesMaxGap = 250;
            MergeTextWithSameTimeCodesReBreakLines = false;
            MergeLinesWithSameTextMaxMs = 250;
            MergeLinesWithSameTextIncrement = true;
            MergeTextWithSameTimeCodesMakeDialog = false;
            ConvertColorsToDialogRemoveColorTags = true;
            ConvertColorsToDialogAddNewLines = true;
            ConvertColorsToDialogReBreakLines = false;
            ColumnPasteColumn = "all";
            ColumnPasteOverwriteMode = "overwrite";
            AssaAttachmentFontTextPreview =
                "Hello World!" + Environment.NewLine +
                "こんにちは世界" + Environment.NewLine +
                "你好世界！" + Environment.NewLine +
                "1234567890";
            BlankVideoColor = Color.CadetBlue;
            BlankVideoUseCheckeredImage = true;
            BlankVideoMinutes = 2;
            BlankVideoFrameRate = 23.976m;
            AssaProgressBarForeColor = Color.FromArgb(200, 200, 0, 0);
            AssaProgressBarBackColor = Color.FromArgb(150, 80, 80, 80);
            AssaProgressBarTextColor = Color.White;
            AssaProgressBarHeight = 40;
            AssaProgressBarSplitterWidth = 2;
            AssaProgressBarSplitterHeight = 40;
            AssaProgressBarFontName = "Arial";
            AssaProgressBarFontSize = 30;
            AssaProgressBarTextAlign = "left";

            AssaBgBoxPaddingLeft = 10;
            AssaBgBoxPaddingRight = 10;
            AssaBgBoxPaddingTop = 6;
            AssaBgBoxPaddingBottom = 6;
            AssaBgBoxColor = Color.FromArgb(200, 0, 0, 0);
            AssaBgBoxOutlineColor = Color.FromArgb(200, 80, 80, 80);
            AssaBgBoxShadowColor = Color.FromArgb(100, 0, 0, 0);
            AssaBgBoxTransparentColor = Color.Cyan;
            AssaBgBoxStyle = "square";
            AssaBgBoxStyleRadius = 30;
            AssaBgBoxStyleCircleAdjustY = 30;
            AssaBgBoxStyleSpikesStep = 15;
            AssaBgBoxStyleSpikesHeight = 30;
            AssaBgBoxStyleBubblesStep = 75;
            AssaBgBoxStyleBubblesHeight = 40;
            AssaBgBoxOutlineWidth = 0;
            AssaBgBoxLayer = -11893;
            AssaBgBoxDrawingFileWatch = true;

            GenVideoEncoding = "libx264";
            GenVideoPreset = "medium";
            GenVideoCrf = "23";
            GenVideoTune = "film";
            GenVideoAudioEncoding = "copy";
            GenVideoAudioForceStereo = true;
            GenVideoAudioSampleRate = "48000";
            GenVideoFontBold = true;
            GenVideoOutline = 6;
            GenVideoFontSizePercentOfHeight = 0.078f;
            GenVideoNonAssaBox = true;
            GenVideoNonAssaBoxColor = Color.FromArgb(150, 0, 0, 0);
            GenVideoNonAssaTextColor = Color.White;
            GenVideoEmbedOutputSuffix = "embed";
            GenVideoEmbedOutputReplace = "embed" + Environment.NewLine + "SoftSub" + Environment.NewLine + "SoftSubbed";
            GenVideoOutputFileSuffix = "_new";
            VoskPostProcessing = true;
            WhisperChoice = Configuration.IsRunningOnWindows ? AudioToText.WhisperChoice.PurfviewFasterWhisper : AudioToText.WhisperChoice.OpenAi;
            WhisperDeleteTempFiles = true;
            WhisperPurfviewFasterWhisperDefaultCmd = "--standard";
            WhisperExtraSettings = "";
            WhisperLanguageCode = "en";
            WhisperAutoAdjustTimings = true;
            WhisperPostProcessingAddPeriods = false;
            WhisperPostProcessingMergeLines = true;
            WhisperPostProcessingSplitLines = true;
            WhisperPostProcessingFixCasing = false;
            WhisperPostProcessingFixShortDuration = true;
            AudioToTextLineMaxChars = 86;
            AudioToTextLineMaxCharsJp = 32;
            AudioToTextLineMaxCharsCn = 36;
        }
    }

    public class FcpExportSettings
    {
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public string Alignment { get; set; }
        public int Baseline { get; set; }
        public Color Color { get; set; }

        public FcpExportSettings()
        {
            FontName = "Lucida Sans";
            FontSize = 36;
            Alignment = "center";
            Baseline = 29;
            Color = Color.WhiteSmoke;
        }
    }


    public class WordListSettings
    {
        public string LastLanguage { get; set; }
        public string NamesUrl { get; set; }
        public bool UseOnlineNames { get; set; }

        public WordListSettings()
        {
            LastLanguage = "en-US";
            NamesUrl = "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/main/Dictionaries/names.xml";
        }
    }

    public class SubtitleSettings
    {
        public List<AssaStorageCategory> AssaStyleStorageCategories { get; set; }
        public List<string> AssaOverrideTagHistory { get; set; }
        public bool AssaResolutionAutoNew { get; set; }
        public bool AssaResolutionPromptChange { get; set; }
        public bool AssaShowScaledBorderAndShadow { get; set; }
        public bool AssaShowPlayDepth { get; set; }
        public string DCinemaFontFile { get; set; }
        public string DCinemaLoadFontResource { get; set; }
        public int DCinemaFontSize { get; set; }
        public int DCinemaBottomMargin { get; set; }
        public double DCinemaZPosition { get; set; }
        public int DCinemaFadeUpTime { get; set; }
        public int DCinemaFadeDownTime { get; set; }
        public bool DCinemaAutoGenerateSubtitleId { get; set; }

        public string CurrentDCinemaSubtitleId { get; set; }
        public string CurrentDCinemaMovieTitle { get; set; }
        public string CurrentDCinemaReelNumber { get; set; }
        public string CurrentDCinemaIssueDate { get; set; }
        public string CurrentDCinemaLanguage { get; set; }
        public string CurrentDCinemaEditRate { get; set; }
        public string CurrentDCinemaTimeCodeRate { get; set; }
        public string CurrentDCinemaStartTime { get; set; }
        public string CurrentDCinemaFontId { get; set; }
        public string CurrentDCinemaFontUri { get; set; }
        public Color CurrentDCinemaFontColor { get; set; }
        public string CurrentDCinemaFontEffect { get; set; }
        public Color CurrentDCinemaFontEffectColor { get; set; }
        public int CurrentDCinemaFontSize { get; set; }

        public int CurrentCavena890LanguageIdLine1 { get; set; }
        public int CurrentCavena890LanguageIdLine2 { get; set; }
        public string CurrentCavena89Title { get; set; }
        public string CurrentCavena890riginalTitle { get; set; }
        public string CurrentCavena890Translator { get; set; }
        public string CurrentCavena89Comment { get; set; }
        public int CurrentCavena89LanguageId { get; set; }
        public string Cavena890StartOfMessage { get; set; }

        public bool EbuStlTeletextUseBox { get; set; }
        public bool EbuStlTeletextUseDoubleHeight { get; set; }
        public int EbuStlMarginTop { get; set; }
        public int EbuStlMarginBottom { get; set; }
        public int EbuStlNewLineRows { get; set; }
        public bool EbuStlRemoveEmptyLines { get; set; }
        public int PacVerticalTop { get; set; }
        public int PacVerticalCenter { get; set; }
        public int PacVerticalBottom { get; set; }

        public string DvdStudioProHeader { get; set; }

        public string TmpegEncXmlFontName { get; set; }
        public string TmpegEncXmlFontHeight { get; set; }
        public string TmpegEncXmlPosition { get; set; }

        public bool CheetahCaptionAlwayWriteEndTime { get; set; }

        public bool SamiDisplayTwoClassesAsTwoSubtitles { get; set; }
        public int SamiHtmlEncodeMode { get; set; }

        public string TimedText10TimeCodeFormat { get; set; }
        public string TimedText10TimeCodeFormatSource { get; set; }
        public bool TimedText10ShowStyleAndLanguage { get; set; }
        public string TimedText10FileExtension { get; set; }
        public string TimedTextItunesTopOrigin { get; set; }
        public string TimedTextItunesTopExtent { get; set; }
        public string TimedTextItunesBottomOrigin { get; set; }
        public string TimedTextItunesBottomExtent { get; set; }
        public string TimedTextItunesTimeCodeFormat { get; set; }
        public string TimedTextItunesStyleAttribute { get; set; }
        public string TimedTextImsc11TimeCodeFormat { get; set; }
        public string TimedTextImsc11FileExtension { get; set; }


        public int FcpFontSize { get; set; }
        public string FcpFontName { get; set; }

        public string NuendoCharacterListFile { get; set; }

        public bool WebVttUseXTimestampMap { get; set; }
        public bool WebVttUseMultipleXTimestampMap { get; set; }
        public bool WebVttMergeLinesWithSameText { get; set; }
        public long WebVttTimescale { get; set; }
        public string WebVttCueAn1 { get; set; }
        public string WebVttCueAn2 { get; set; }
        public string WebVttCueAn3 { get; set; }
        public string WebVttCueAn4 { get; set; }
        public string WebVttCueAn5 { get; set; }
        public string WebVttCueAn6 { get; set; }
        public string WebVttCueAn7 { get; set; }
        public string WebVttCueAn8 { get; set; }
        public string WebVttCueAn9 { get; set; }
        public string MPlayer2Extension { get; set; }
        public bool TeletextItalicFix { get; set; }
        public bool MccDebug { get; set; }
        public bool BluRaySupSkipMerge { get; set; }
        public bool BluRaySupForceMergeAll { get; set; }

        public SubtitleSettings()
        {
            AssaStyleStorageCategories = new List<AssaStorageCategory>();
            AssaOverrideTagHistory = new List<string>();
            AssaResolutionAutoNew = true;
            AssaResolutionPromptChange = true;
            AssaShowScaledBorderAndShadow = true;
            AssaShowPlayDepth = true;

            DCinemaFontFile = "Arial.ttf";
            DCinemaLoadFontResource = "urn:uuid:3dec6dc0-39d0-498d-97d0-928d2eb78391";
            DCinemaFontSize = 42;
            DCinemaBottomMargin = 8;
            DCinemaZPosition = 0;
            DCinemaFadeUpTime = 0;
            DCinemaFadeDownTime = 0;
            DCinemaAutoGenerateSubtitleId = true;

            EbuStlTeletextUseBox = true;
            EbuStlTeletextUseDoubleHeight = true;
            EbuStlMarginTop = 0;
            EbuStlMarginBottom = 2;
            EbuStlNewLineRows = 2;

            PacVerticalTop = 1;
            PacVerticalCenter = 6;
            PacVerticalBottom = 11;

            DvdStudioProHeader = @"$VertAlign          =   Bottom
$Bold               =   FALSE
$Underlined         =   FALSE
$Italic             =   FALSE
$XOffset                =   0
$YOffset                =   -5
$TextContrast           =   15
$Outline1Contrast           =   15
$Outline2Contrast           =   13
$BackgroundContrast     =   0
$ForceDisplay           =   FALSE
$FadeIn             =   0
$FadeOut                =   0
$HorzAlign          =   Center
";

            TmpegEncXmlFontName = "Tahoma";
            TmpegEncXmlFontHeight = "0.069";
            TmpegEncXmlPosition = "23";

            SamiDisplayTwoClassesAsTwoSubtitles = true;
            SamiHtmlEncodeMode = 0;

            TimedText10TimeCodeFormat = "Source";
            TimedText10ShowStyleAndLanguage = true;
            TimedText10FileExtension = ".xml";

            TimedTextItunesTopOrigin = "0% 0%";
            TimedTextItunesTopExtent = "100% 15%";
            TimedTextItunesBottomOrigin = "0% 85%";
            TimedTextItunesBottomExtent = "100% 15%";
            TimedTextItunesTimeCodeFormat = "Frames";
            TimedTextItunesStyleAttribute = "tts:fontStyle";
            TimedTextImsc11TimeCodeFormat = "hh:mm:ss.ms";
            TimedTextImsc11FileExtension = ".xml";

            FcpFontSize = 18;
            FcpFontName = "Lucida Grande";

            Cavena890StartOfMessage = "10:00:00:00";

            WebVttTimescale = 90000;
            WebVttUseXTimestampMap = true;
            WebVttUseMultipleXTimestampMap = true;
            WebVttCueAn1 = "position:20%";
            WebVttCueAn2 = "";
            WebVttCueAn3 = "position:80%";
            WebVttCueAn4 = "position:20% line:50%";
            WebVttCueAn5 = "line:50%";
            WebVttCueAn6 = "position:80% line:50%";
            WebVttCueAn7 = "position:20% line:20%";
            WebVttCueAn8 = "line:20%";
            WebVttCueAn9 = "position:80% line:20%";

            MPlayer2Extension = ".txt";

            TeletextItalicFix = true;
        }

        public void InitializeDCinameSettings(bool smpte)
        {
            if (smpte)
            {
                CurrentDCinemaSubtitleId = "urn:uuid:" + Guid.NewGuid();
                CurrentDCinemaLanguage = "en";
                CurrentDCinemaFontUri = DCinemaLoadFontResource;
                CurrentDCinemaFontId = "theFontId";
            }
            else
            {
                string hex = Guid.NewGuid().ToString().RemoveChar('-').ToLowerInvariant();
                hex = hex.Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
                CurrentDCinemaSubtitleId = hex;
                CurrentDCinemaLanguage = "English";
                CurrentDCinemaFontUri = DCinemaFontFile;
                CurrentDCinemaFontId = "Arial";
            }
            CurrentDCinemaIssueDate = DateTime.Now.ToString("s");
            CurrentDCinemaMovieTitle = "title";
            CurrentDCinemaReelNumber = "1";
            CurrentDCinemaFontColor = Color.White;
            CurrentDCinemaFontEffect = "border";
            CurrentDCinemaFontEffectColor = Color.Black;
            CurrentDCinemaFontSize = DCinemaFontSize;
            CurrentCavena890LanguageIdLine1 = -1;
            CurrentCavena890LanguageIdLine2 = -1;
        }
    }

    public class ProxySettings
    {
        public string ProxyAddress { get; set; }
        public string AuthType { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }

        public string DecodePassword()
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(Password));
        }
        public void EncodePassword(string unencryptedPassword)
        {
            Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(unencryptedPassword));
        }
    }

    public class FixCommonErrorsSettings
    {
        public string StartPosition { get; set; }
        public string StartSize { get; set; }
        public bool EmptyLinesTicked { get; set; }
        public bool OverlappingDisplayTimeTicked { get; set; }
        public bool TooShortDisplayTimeTicked { get; set; }
        public bool TooLongDisplayTimeTicked { get; set; }
        public bool TooShortGapTicked { get; set; }
        public bool InvalidItalicTagsTicked { get; set; }
        public bool BreakLongLinesTicked { get; set; }
        public bool MergeShortLinesTicked { get; set; }
        public bool MergeShortLinesAllTicked { get; set; }
        public bool MergeShortLinesPixelWidthTicked { get; set; }
        public bool UnneededSpacesTicked { get; set; }
        public bool UnneededPeriodsTicked { get; set; }
        public bool FixCommasTicked { get; set; }
        public bool MissingSpacesTicked { get; set; }
        public bool AddMissingQuotesTicked { get; set; }
        public bool Fix3PlusLinesTicked { get; set; }
        public bool FixHyphensTicked { get; set; }
        public bool FixHyphensRemoveSingleLineTicked { get; set; }
        public bool UppercaseIInsideLowercaseWordTicked { get; set; }
        public bool DoubleApostropheToQuoteTicked { get; set; }
        public bool AddPeriodAfterParagraphTicked { get; set; }
        public bool StartWithUppercaseLetterAfterParagraphTicked { get; set; }
        public bool StartWithUppercaseLetterAfterPeriodInsideParagraphTicked { get; set; }
        public bool StartWithUppercaseLetterAfterColonTicked { get; set; }
        public bool AloneLowercaseIToUppercaseIEnglishTicked { get; set; }
        public bool FixOcrErrorsViaReplaceListTicked { get; set; }
        public bool RemoveSpaceBetweenNumberTicked { get; set; }
        public bool FixDialogsOnOneLineTicked { get; set; }
        public bool RemoveDialogFirstLineInNonDialogs { get; set; }
        public bool TurkishAnsiTicked { get; set; }
        public bool DanishLetterITicked { get; set; }
        public bool SpanishInvertedQuestionAndExclamationMarksTicked { get; set; }
        public bool FixDoubleDashTicked { get; set; }
        public bool FixDoubleGreaterThanTicked { get; set; }
        public bool FixEllipsesStartTicked { get; set; }
        public bool FixMissingOpenBracketTicked { get; set; }
        public bool FixMusicNotationTicked { get; set; }
        public bool FixContinuationStyleTicked { get; set; }
        public bool FixUnnecessaryLeadingDotsTicked { get; set; }
        public bool NormalizeStringsTicked { get; set; }
        public string DefaultFixes { get; set; }


        public FixCommonErrorsSettings()
        {
            SetDefaultFixes();
        }

        public void SaveUserDefaultFixes()
        {
            var sb = new StringBuilder();

            if (EmptyLinesTicked)
            {
                sb.Append(nameof(EmptyLinesTicked) + ";");
            }

            if (OverlappingDisplayTimeTicked)
            {
                sb.Append(nameof(OverlappingDisplayTimeTicked) + ";");
            }

            if (TooShortDisplayTimeTicked)
            {
                sb.Append(nameof(TooShortDisplayTimeTicked) + ";");
            }

            if (TooLongDisplayTimeTicked)
            {
                sb.Append(nameof(TooLongDisplayTimeTicked) + ";");
            }

            if (TooShortGapTicked)
            {
                sb.Append(nameof(TooShortGapTicked) + ";");
            }

            if (InvalidItalicTagsTicked)
            {
                sb.Append(nameof(InvalidItalicTagsTicked) + ";");
            }

            if (BreakLongLinesTicked)
            {
                sb.Append(nameof(BreakLongLinesTicked) + ";");
            }

            if (MergeShortLinesTicked)
            {
                sb.Append(nameof(MergeShortLinesTicked) + ";");
            }

            if (MergeShortLinesAllTicked)
            {
                sb.Append(nameof(MergeShortLinesAllTicked) + ";");
            }

            if (MergeShortLinesPixelWidthTicked)
            {
                sb.Append(nameof(MergeShortLinesPixelWidthTicked) + ";");
            }

            if (UnneededSpacesTicked)
            {
                sb.Append(nameof(UnneededSpacesTicked) + ";");
            }

            if (UnneededPeriodsTicked)
            {
                sb.Append(nameof(UnneededPeriodsTicked) + ";");
            }

            if (FixCommasTicked)
            {
                sb.Append(nameof(FixCommasTicked) + ";");
            }

            if (MissingSpacesTicked)
            {
                sb.Append(nameof(MissingSpacesTicked) + ";");
            }

            if (AddMissingQuotesTicked)
            {
                sb.Append(nameof(AddMissingQuotesTicked) + ";");
            }

            if (Fix3PlusLinesTicked)
            {
                sb.Append(nameof(Fix3PlusLinesTicked) + ";");
            }

            if (FixHyphensTicked)
            {
                sb.Append(nameof(FixHyphensTicked) + ";");
            }

            if (FixHyphensRemoveSingleLineTicked)
            {
                sb.Append(nameof(FixHyphensRemoveSingleLineTicked) + ";");
            }

            if (UppercaseIInsideLowercaseWordTicked)
            {
                sb.Append(nameof(UppercaseIInsideLowercaseWordTicked) + ";");
            }

            if (DoubleApostropheToQuoteTicked)
            {
                sb.Append(nameof(DoubleApostropheToQuoteTicked) + ";");
            }

            if (AddPeriodAfterParagraphTicked)
            {
                sb.Append(nameof(AddPeriodAfterParagraphTicked) + ";");
            }

            if (StartWithUppercaseLetterAfterParagraphTicked)
            {
                sb.Append(nameof(StartWithUppercaseLetterAfterParagraphTicked) + ";");
            }

            if (StartWithUppercaseLetterAfterPeriodInsideParagraphTicked)
            {
                sb.Append(nameof(StartWithUppercaseLetterAfterPeriodInsideParagraphTicked) + ";");
            }

            if (StartWithUppercaseLetterAfterColonTicked)
            {
                sb.Append(nameof(StartWithUppercaseLetterAfterColonTicked) + ";");
            }

            if (AloneLowercaseIToUppercaseIEnglishTicked)
            {
                sb.Append(nameof(AloneLowercaseIToUppercaseIEnglishTicked) + ";");
            }

            if (FixOcrErrorsViaReplaceListTicked)
            {
                sb.Append(nameof(FixOcrErrorsViaReplaceListTicked) + ";");
            }

            if (RemoveSpaceBetweenNumberTicked)
            {
                sb.Append(nameof(RemoveSpaceBetweenNumberTicked) + ";");
            }

            if (FixDialogsOnOneLineTicked)
            {
                sb.Append(nameof(FixDialogsOnOneLineTicked) + ";");
            }

            if (RemoveDialogFirstLineInNonDialogs)
            {
                sb.Append(nameof(RemoveDialogFirstLineInNonDialogs) + ";");
            }

            if (TurkishAnsiTicked)
            {
                sb.Append(nameof(TurkishAnsiTicked) + ";");
            }

            if (DanishLetterITicked)
            {
                sb.Append(nameof(DanishLetterITicked) + ";");
            }

            if (SpanishInvertedQuestionAndExclamationMarksTicked)
            {
                sb.Append(nameof(SpanishInvertedQuestionAndExclamationMarksTicked) + ";");
            }

            if (FixDoubleDashTicked)
            {
                sb.Append(nameof(FixDoubleDashTicked) + ";");
            }

            if (FixEllipsesStartTicked)
            {
                sb.Append(nameof(FixEllipsesStartTicked) + ";");
            }

            if (FixMissingOpenBracketTicked)
            {
                sb.Append(nameof(FixMissingOpenBracketTicked) + ";");
            }

            if (FixMusicNotationTicked)
            {
                sb.Append(nameof(FixMusicNotationTicked) + ";");
            }

            if (FixContinuationStyleTicked)
            {
                sb.Append(nameof(FixContinuationStyleTicked) + ";");
            }

            if (FixUnnecessaryLeadingDotsTicked)
            {
                sb.Append(nameof(FixUnnecessaryLeadingDotsTicked) + ";");
            }

            if (NormalizeStringsTicked)
            {
                sb.Append(nameof(NormalizeStringsTicked) + ";");
            }

            DefaultFixes = sb.ToString().TrimEnd(';');
        }

        public void LoadUserDefaultFixes(string fixes)
        {
            var list = fixes.Split(';');
            EmptyLinesTicked = list.Contains(nameof(EmptyLinesTicked));
            OverlappingDisplayTimeTicked = list.Contains(nameof(OverlappingDisplayTimeTicked));
            TooShortDisplayTimeTicked = list.Contains(nameof(TooShortDisplayTimeTicked));
            TooLongDisplayTimeTicked = list.Contains(nameof(TooLongDisplayTimeTicked));
            TooShortGapTicked = list.Contains(nameof(TooShortGapTicked));
            InvalidItalicTagsTicked = list.Contains(nameof(InvalidItalicTagsTicked));
            BreakLongLinesTicked = list.Contains(nameof(BreakLongLinesTicked));
            MergeShortLinesTicked = list.Contains(nameof(MergeShortLinesTicked));
            MergeShortLinesAllTicked = list.Contains(nameof(MergeShortLinesAllTicked));
            MergeShortLinesPixelWidthTicked = list.Contains(nameof(MergeShortLinesPixelWidthTicked));
            UnneededSpacesTicked = list.Contains(nameof(UnneededSpacesTicked));
            UnneededPeriodsTicked = list.Contains(nameof(UnneededPeriodsTicked));
            FixCommasTicked = list.Contains(nameof(FixCommasTicked));
            MissingSpacesTicked = list.Contains(nameof(MissingSpacesTicked));
            AddMissingQuotesTicked = list.Contains(nameof(AddMissingQuotesTicked));
            Fix3PlusLinesTicked = list.Contains(nameof(Fix3PlusLinesTicked));
            FixHyphensTicked = list.Contains(nameof(FixHyphensTicked));
            FixHyphensRemoveSingleLineTicked = list.Contains(nameof(FixHyphensRemoveSingleLineTicked));
            UppercaseIInsideLowercaseWordTicked = list.Contains(nameof(UppercaseIInsideLowercaseWordTicked));
            DoubleApostropheToQuoteTicked = list.Contains(nameof(DoubleApostropheToQuoteTicked));
            AddPeriodAfterParagraphTicked = list.Contains(nameof(AddPeriodAfterParagraphTicked));
            StartWithUppercaseLetterAfterParagraphTicked = list.Contains(nameof(StartWithUppercaseLetterAfterParagraphTicked));
            StartWithUppercaseLetterAfterPeriodInsideParagraphTicked = list.Contains(nameof(StartWithUppercaseLetterAfterPeriodInsideParagraphTicked));
            StartWithUppercaseLetterAfterColonTicked = list.Contains(nameof(StartWithUppercaseLetterAfterColonTicked));
            AloneLowercaseIToUppercaseIEnglishTicked = list.Contains(nameof(AloneLowercaseIToUppercaseIEnglishTicked));
            FixOcrErrorsViaReplaceListTicked = list.Contains(nameof(FixOcrErrorsViaReplaceListTicked));
            RemoveSpaceBetweenNumberTicked = list.Contains(nameof(RemoveSpaceBetweenNumberTicked));
            FixDialogsOnOneLineTicked = list.Contains(nameof(FixDialogsOnOneLineTicked));
            RemoveDialogFirstLineInNonDialogs = list.Contains(nameof(RemoveDialogFirstLineInNonDialogs));
            TurkishAnsiTicked = list.Contains(nameof(TurkishAnsiTicked));
            DanishLetterITicked = list.Contains(nameof(DanishLetterITicked));
            SpanishInvertedQuestionAndExclamationMarksTicked = list.Contains(nameof(SpanishInvertedQuestionAndExclamationMarksTicked));
            FixDoubleDashTicked = list.Contains(nameof(FixDoubleDashTicked));
            FixEllipsesStartTicked = list.Contains(nameof(FixEllipsesStartTicked));
            FixMissingOpenBracketTicked = list.Contains(nameof(FixMissingOpenBracketTicked));
            FixMusicNotationTicked = list.Contains(nameof(FixMusicNotationTicked));
            FixContinuationStyleTicked = list.Contains(nameof(FixContinuationStyleTicked));
            FixUnnecessaryLeadingDotsTicked = list.Contains(nameof(FixUnnecessaryLeadingDotsTicked));
            NormalizeStringsTicked = list.Contains(nameof(NormalizeStringsTicked));
        }

        public void SetDefaultFixes()
        {
            LoadUserDefaultFixes(string.Empty);
            EmptyLinesTicked = true;
            OverlappingDisplayTimeTicked = true;
            TooShortDisplayTimeTicked = true;
            TooLongDisplayTimeTicked = true;
            TooShortGapTicked = false;
            InvalidItalicTagsTicked = true;
            BreakLongLinesTicked = true;
            MergeShortLinesTicked = true;
            MergeShortLinesPixelWidthTicked = false;
            UnneededPeriodsTicked = true;
            FixCommasTicked = true;
            UnneededSpacesTicked = true;
            MissingSpacesTicked = true;
            UppercaseIInsideLowercaseWordTicked = true;
            DoubleApostropheToQuoteTicked = true;
            AddPeriodAfterParagraphTicked = false;
            StartWithUppercaseLetterAfterParagraphTicked = true;
            StartWithUppercaseLetterAfterPeriodInsideParagraphTicked = false;
            StartWithUppercaseLetterAfterColonTicked = false;
            AloneLowercaseIToUppercaseIEnglishTicked = false;
            TurkishAnsiTicked = false;
            DanishLetterITicked = false;
            FixDoubleDashTicked = true;
            FixDoubleGreaterThanTicked = true;
            FixEllipsesStartTicked = true;
            FixMissingOpenBracketTicked = true;
            FixMusicNotationTicked = true;
            FixContinuationStyleTicked = false;
            FixUnnecessaryLeadingDotsTicked = true;
            NormalizeStringsTicked = false;
            SaveUserDefaultFixes();
        }
    }

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
        public Color SubtitleTextBoxHtmlColor { get; set; }
        public Color SubtitleTextBoxAssColor { get; set; }
        public int SubtitleListViewFontSize { get; set; }
        public bool SubtitleTextBoxFontBold { get; set; }
        public bool SubtitleListViewFontBold { get; set; }
        public Color SubtitleFontColor { get; set; }
        public Color SubtitleBackgroundColor { get; set; }
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
        public Color MpvPreviewTextPrimaryColor { get; set; }
        public Color MpvPreviewTextOutlineColor { get; set; }
        public Color MpvPreviewTextBackgroundColor { get; set; }
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
        public Color DarkThemeForeColor { get; set; }
        public Color DarkThemeBackColor { get; set; }
        public Color DarkThemeSelectedBackgroundColor { get; set; }
        public Color DarkThemeDisabledColor { get; set; }
        public Color LastColorPickerColor { get; set; }
        public Color LastColorPickerColor1 { get; set; }
        public Color LastColorPickerColor2 { get; set; }
        public Color LastColorPickerColor3 { get; set; }
        public Color LastColorPickerColor4 { get; set; }
        public Color LastColorPickerColor5 { get; set; }
        public Color LastColorPickerColor6 { get; set; }
        public Color LastColorPickerColor7 { get; set; }
        public Color LastColorPickerDropper { get; set; }
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
            SubtitleTextBoxHtmlColor = Color.CornflowerBlue;
            SubtitleTextBoxAssColor = Color.BlueViolet;
            SubtitleTextBoxFontBold = true;
            SubtitleFontColor = Color.Black;
            MeasureFontName = "Arial";
            MeasureFontSize = 24;
            MeasureFontBold = false;
            SubtitleLineMaximumPixelWidth = 576;
            SubtitleBackgroundColor = Color.White;
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
            MpvPreviewTextPrimaryColor = Color.White;
            MpvPreviewTextOutlineColor = Color.Black;
            MpvPreviewTextBackgroundColor = Color.Black;
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
            DarkThemeForeColor = Color.FromArgb(155, 155, 155);
            DarkThemeBackColor = Color.FromArgb(30, 30, 30);
            DarkThemeSelectedBackgroundColor = Color.FromArgb(24, 52, 75);
            DarkThemeDisabledColor = Color.FromArgb(120, 120, 120);
            LastColorPickerColor = Color.Yellow;
            LastColorPickerColor1 = Color.Red;
            LastColorPickerColor2 = Color.Green;
            LastColorPickerColor3 = Color.Blue;
            LastColorPickerColor4 = Color.White;
            LastColorPickerColor5 = Color.Black;
            LastColorPickerColor6 = Color.Cyan;
            LastColorPickerColor7 = Color.DarkOrange;
            LastColorPickerDropper = Color.Transparent;
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

        internal static void AddExtraProfiles(List<RulesProfile> profiles)
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

    public class VideoControlsSettings
    {
        public string CustomSearchText1 { get; set; }
        public string CustomSearchText2 { get; set; }
        public string CustomSearchText3 { get; set; }
        public string CustomSearchText4 { get; set; }
        public string CustomSearchText5 { get; set; }
        public string CustomSearchUrl1 { get; set; }
        public string CustomSearchUrl2 { get; set; }
        public string CustomSearchUrl3 { get; set; }
        public string CustomSearchUrl4 { get; set; }
        public string CustomSearchUrl5 { get; set; }
        public string LastActiveTab { get; set; }
        public bool WaveformDrawGrid { get; set; }
        public bool WaveformDrawCps { get; set; }
        public bool WaveformDrawWpm { get; set; }
        public bool WaveformAllowOverlap { get; set; }
        public bool WaveformFocusOnMouseEnter { get; set; }
        public bool WaveformListViewFocusOnMouseEnter { get; set; }
        public bool WaveformSetVideoPositionOnMoveStartEnd { get; set; }
        public bool WaveformSingleClickSelect { get; set; }
        public bool WaveformSnapToShotChanges { get; set; }
        public int WaveformShotChangeStartTimeBeforeMs { get; set; }
        public int WaveformShotChangeStartTimeAfterMs { get; set; }
        public int WaveformShotChangeEndTimeBeforeMs { get; set; }
        public int WaveformShotChangeEndTimeAfterMs { get; set; }
        public int WaveformBorderHitMs { get; set; }
        public Color WaveformGridColor { get; set; }
        public Color WaveformColor { get; set; }
        public Color WaveformSelectedColor { get; set; }
        public Color WaveformBackgroundColor { get; set; }
        public Color WaveformTextColor { get; set; }
        public Color WaveformCursorColor { get; set; }
        public Color WaveformChaptersColor { get; set; }
        public int WaveformTextSize { get; set; }
        public bool WaveformTextBold { get; set; }
        public string WaveformDoubleClickOnNonParagraphAction { get; set; }
        public string WaveformRightClickOnNonParagraphAction { get; set; }
        public bool WaveformMouseWheelScrollUpIsForward { get; set; }
        public bool WaveformLabelShowCodec { get; set; }
        public bool GenerateSpectrogram { get; set; }
        public string SpectrogramAppearance { get; set; }
        public int WaveformMinimumSampleRate { get; set; }
        public double WaveformSeeksSilenceDurationSeconds { get; set; }
        public double WaveformSeeksSilenceMaxVolume { get; set; }
        public bool WaveformUnwrapText { get; set; }
        public bool WaveformHideWpmCpsLabels { get; set; }


        public VideoControlsSettings()
        {
            CustomSearchText1 = "The Free Dictionary";
            CustomSearchUrl1 = "https://www.thefreedictionary.com/{0}";
            CustomSearchText2 = "Wikipedia";
            CustomSearchUrl2 = "https://en.wikipedia.org/wiki?search={0}";
            CustomSearchText3 = "DuckDuckGo";
            CustomSearchUrl3 = "https://duckduckgo.com/?q={0}";

            LastActiveTab = "Translate";
            WaveformDrawGrid = true;
            WaveformAllowOverlap = false;
            WaveformBorderHitMs = 15;
            WaveformGridColor = Color.FromArgb(255, 20, 20, 18);
            WaveformColor = Color.FromArgb(255, 160, 240, 30);
            WaveformSelectedColor = Color.FromArgb(255, 230, 0, 0);
            WaveformBackgroundColor = Color.Black;
            WaveformTextColor = Color.Gray;
            WaveformCursorColor = Color.Turquoise;
            WaveformChaptersColor = Color.FromArgb(255, 104, 33, 122);
            WaveformTextSize = 9;
            WaveformTextBold = true;
            WaveformDoubleClickOnNonParagraphAction = "PlayPause";
            WaveformDoubleClickOnNonParagraphAction = string.Empty;
            WaveformMouseWheelScrollUpIsForward = true;
            WaveformLabelShowCodec = true;
            SpectrogramAppearance = "OneColorGradient";
            WaveformMinimumSampleRate = 126;
            WaveformSeeksSilenceDurationSeconds = 0.3;
            WaveformSeeksSilenceMaxVolume = 0.1;
            WaveformSnapToShotChanges = true;
        }
    }

    public class VobSubOcrSettings
    {
        public int XOrMorePixelsMakesSpace { get; set; }
        public double AllowDifferenceInPercent { get; set; }
        public double BlurayAllowDifferenceInPercent { get; set; }
        public string LastImageCompareFolder { get; set; }
        public int LastModiLanguageId { get; set; }
        public string LastOcrMethod { get; set; }
        public string TesseractLastLanguage { get; set; }
        public bool UseTesseractFallback { get; set; }
        public bool UseItalicsInTesseract { get; set; }
        public int TesseractEngineMode { get; set; }
        public bool UseMusicSymbolsInTesseract { get; set; }
        public bool RightToLeft { get; set; }
        public bool TopToBottom { get; set; }
        public int DefaultMillisecondsForUnknownDurations { get; set; }
        public bool FixOcrErrors { get; set; }
        public bool PromptForUnknownWords { get; set; }
        public bool GuessUnknownWords { get; set; }
        public bool AutoBreakSubtitleIfMoreThanTwoLines { get; set; }
        public double ItalicFactor { get; set; }

        public bool LineOcrDraw { get; set; }
        public int LineOcrMinHeightSplit { get; set; }
        public bool LineOcrAdvancedItalic { get; set; }
        public string LineOcrLastLanguages { get; set; }
        public string LineOcrLastSpellCheck { get; set; }
        public int LineOcrLinesToAutoGuess { get; set; }
        public int LineOcrMinLineHeight { get; set; }
        public int LineOcrMaxLineHeight { get; set; }
        public int LineOcrMaxErrorPixels { get; set; }
        public string LastBinaryImageCompareDb { get; set; }
        public string LastBinaryImageSpellCheck { get; set; }
        public bool BinaryAutoDetectBestDb { get; set; }
        public string LastTesseractSpellCheck { get; set; }
        public bool CaptureTopAlign { get; set; }
        public int UnfocusedAttentionBlinkCount { get; set; }
        public int UnfocusedAttentionPlaySoundCount { get; set; }
        public string CloudVisionApiKey { get; set; }
        public string CloudVisionLanguage { get; set; }
        public bool CloudVisionSendOriginalImages { get; set; }

        public VobSubOcrSettings()
        {
            XOrMorePixelsMakesSpace = 8;
            AllowDifferenceInPercent = 1.0;
            BlurayAllowDifferenceInPercent = 7.5;
            LastImageCompareFolder = "English";
            LastModiLanguageId = 9;
            LastOcrMethod = "Tesseract";
            UseItalicsInTesseract = true;
            TesseractEngineMode = 3; // Default, based on what is available (T4 docs)
            UseMusicSymbolsInTesseract = true;
            UseTesseractFallback = true;
            RightToLeft = false;
            TopToBottom = true;
            DefaultMillisecondsForUnknownDurations = 5000;
            FixOcrErrors = true;
            PromptForUnknownWords = true;
            GuessUnknownWords = true;
            AutoBreakSubtitleIfMoreThanTwoLines = true;
            ItalicFactor = 0.2f;
            LineOcrLinesToAutoGuess = 100;
            LineOcrMaxErrorPixels = 45;
            LastBinaryImageCompareDb = "Latin+Latin";
            BinaryAutoDetectBestDb = true;
            CaptureTopAlign = false;
            UnfocusedAttentionBlinkCount = 50;
            UnfocusedAttentionPlaySoundCount = 1;
            CloudVisionApiKey = string.Empty;
            CloudVisionLanguage = "en";
            CloudVisionSendOriginalImages = false;
        }
    }

    public class MultipleSearchAndReplaceSetting
    {
        public bool Enabled { get; set; }
        public string FindWhat { get; set; }
        public string ReplaceWith { get; set; }
        public string SearchType { get; set; }
        public string Description { get; set; }
    }

    public class MultipleSearchAndReplaceGroup
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public List<MultipleSearchAndReplaceSetting> Rules { get; set; }
    }

    public class AssaStorageCategory
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public List<SsaStyle> Styles { get; set; }
    }

    public class NetworkSettings
    {
        public string UserName { get; set; }
        public string WebApiUrl { get; set; }
        public string SessionKey { get; set; }
        public int PollIntervalSeconds { get; set; }
        public string NewMessageSound { get; set; }

        public NetworkSettings()
        {
            UserName = string.Empty;
            SessionKey = Guid.NewGuid().ToString();
            WebApiUrl = "https://www.nikse.dk/api/SeNet";
            PollIntervalSeconds = 5;
        }
    }

    public class Shortcuts
    {
        // General
        public string GeneralMergeSelectedLines { get; set; }
        public string GeneralMergeWithPrevious { get; set; }
        public string GeneralMergeWithNext { get; set; }
        public string GeneralMergeWithPreviousAndUnbreak { get; set; }
        public string GeneralMergeWithNextAndUnbreak { get; set; }
        public string GeneralMergeWithPreviousAndBreak { get; set; }
        public string GeneralMergeWithNextAndBreak { get; set; }
        public string GeneralMergeSelectedLinesAndAutoBreak { get; set; }
        public string GeneralMergeSelectedLinesAndUnbreak { get; set; }
        public string GeneralMergeSelectedLinesAndUnbreakCjk { get; set; }
        public string GeneralMergeSelectedLinesOnlyFirstText { get; set; }
        public string GeneralMergeSelectedLinesBilingual { get; set; }
        public string GeneralMergeWithPreviousBilingual { get; set; }
        public string GeneralMergeWithNextBilingual { get; set; }
        public string GeneralMergeOriginalAndTranslation { get; set; }
        public string GeneralToggleTranslationMode { get; set; }
        public string GeneralSwitchOriginalAndTranslation { get; set; }
        public string GeneralSwitchOriginalAndTranslationTextBoxes { get; set; }
        public string GeneralLayoutChoose { get; set; }
        public string GeneralLayoutChoose1 { get; set; }
        public string GeneralLayoutChoose2 { get; set; }
        public string GeneralLayoutChoose3 { get; set; }
        public string GeneralLayoutChoose4 { get; set; }
        public string GeneralLayoutChoose5 { get; set; }
        public string GeneralLayoutChoose6 { get; set; }
        public string GeneralLayoutChoose7 { get; set; }
        public string GeneralLayoutChoose8 { get; set; }
        public string GeneralLayoutChoose9 { get; set; }
        public string GeneralLayoutChoose10 { get; set; }
        public string GeneralLayoutChoose11 { get; set; }
        public string GeneralLayoutChoose12 { get; set; }
        public string GeneralPlayFirstSelected { get; set; }
        public string GeneralGoToFirstSelectedLine { get; set; }
        public string GeneralGoToNextEmptyLine { get; set; }
        public string GeneralGoToNextSubtitle { get; set; }
        public string GeneralGoToNextSubtitlePlayTranslate { get; set; }
        public string GeneralGoToNextSubtitleCursorAtEnd { get; set; }
        public string GeneralGoToPrevSubtitle { get; set; }
        public string GeneralGoToPrevSubtitlePlayTranslate { get; set; }
        public string GeneralGoToStartOfCurrentSubtitle { get; set; }
        public string GeneralGoToEndOfCurrentSubtitle { get; set; }
        public string GeneralGoToPreviousSubtitleAndFocusVideo { get; set; }
        public string GeneralGoToNextSubtitleAndFocusVideo { get; set; }
        public string GeneralGoToPreviousSubtitleAndFocusWaveform { get; set; }
        public string GeneralGoToNextSubtitleAndFocusWaveform { get; set; }
        public string GeneralGoToPrevSubtitleAndPlay { get; set; }
        public string GeneralGoToNextSubtitleAndPlay { get; set; }
        public string GeneralToggleBookmarks { get; set; }
        public string GeneralToggleBookmarksWithText { get; set; }
        public string GeneralEditBookmarks { get; set; }
        public string GeneralClearBookmarks { get; set; }
        public string GeneralGoToBookmark { get; set; }
        public string GeneralGoToPreviousBookmark { get; set; }
        public string GeneralGoToNextBookmark { get; set; }
        public string GeneralChooseProfile { get; set; }
        public string GeneralDuplicateLine { get; set; }
        public string OpenDataFolder { get; set; }
        public string OpenContainingFolder { get; set; }
        public string GeneralToggleView { get; set; }
        public string GeneralToggleMode { get; set; }
        public string GeneralTogglePreviewOnVideo { get; set; }
        public string GeneralRemoveBlankLines { get; set; }
        public string GeneralApplyAssaOverrideTags { get; set; }
        public string GeneralSetAssaPosition { get; set; }
        public string GeneralSetAssaResolution { get; set; }
        public string GeneralSetAssaBgBox { get; set; }
        public string GeneralColorPicker { get; set; }
        public string GeneralTakeAutoBackup { get; set; }
        public string GeneralHelp { get; set; }
        public string GeneralFocusTextBox { get; set; }

        // File
        public string MainFileNew { get; set; }
        public string MainFileOpen { get; set; }
        public string MainFileOpenKeepVideo { get; set; }
        public string MainFileSave { get; set; }
        public string MainFileSaveOriginal { get; set; }
        public string MainFileSaveOriginalAs { get; set; }
        public string MainFileSaveAs { get; set; }
        public string MainFileSaveAll { get; set; }
        public string MainFileOpenOriginal { get; set; }
        public string MainFileCloseOriginal { get; set; }
        public string MainFileCloseTranslation { get; set; }
        public string MainFileCompare { get; set; }
        public string MainFileVerifyCompleteness { get; set; }
        public string MainFileImportPlainText { get; set; }
        public string MainFileImportBdSupForEdit { get; set; }
        public string MainFileImportTimeCodes { get; set; }
        public string MainFileExportEbu { get; set; }
        public string MainFileExportPac { get; set; }
        public string MainFileExportBdSup { get; set; }
        public string MainFileExportEdlClip { get; set; }
        public string MainFileExportPlainText { get; set; }
        public string MainFileExit { get; set; }

        // Edit
        public string MainEditUndo { get; set; }
        public string MainEditRedo { get; set; }
        public string MainEditFind { get; set; }
        public string MainEditFindNext { get; set; }
        public string MainEditReplace { get; set; }
        public string MainEditMultipleReplace { get; set; }
        public string MainEditGoToLineNumber { get; set; }
        public string MainEditRightToLeft { get; set; }
        public string MainEditFixRTLViaUnicodeChars { get; set; }
        public string MainEditRemoveRTLUnicodeChars { get; set; }
        public string MainEditReverseStartAndEndingForRTL { get; set; }
        public string MainVideoToggleControls { get; set; }
        public string MainEditToggleTranslationOriginalInPreviews { get; set; }
        public string MainEditInverseSelection { get; set; }
        public string MainEditModifySelection { get; set; }

        // Tools
        public string MainToolsAdjustDuration { get; set; }
        public string MainToolsAdjustDurationLimits { get; set; }
        public string MainToolsFixCommonErrors { get; set; }
        public string MainToolsFixCommonErrorsPreview { get; set; }
        public string MainToolsMergeShortLines { get; set; }
        public string MainToolsMergeDuplicateText { get; set; }
        public string MainToolsMergeSameTimeCodes { get; set; }
        public string MainToolsMakeEmptyFromCurrent { get; set; }
        public string MainToolsSplitLongLines { get; set; }
        public string MainToolsDurationsBridgeGap { get; set; }
        public string MainToolsMinimumDisplayTimeBetweenParagraphs { get; set; }

        public string MainToolsRenumber { get; set; }
        public string MainToolsRemoveTextForHI { get; set; }
        public string MainToolsConvertColorsToDialog { get; set; }
        public string MainToolsChangeCasing { get; set; }
        public string MainToolsAutoDuration { get; set; }
        public string MainToolsBatchConvert { get; set; }
        public string MainToolsMeasurementConverter { get; set; }
        public string MainToolsSplit { get; set; }
        public string MainToolsAppend { get; set; }
        public string MainToolsJoin { get; set; }
        public string MainToolsStyleManager { get; set; }

        // Video
        public string MainVideoOpen { get; set; }
        public string MainVideoClose { get; set; }
        public string MainVideoPause { get; set; }
        public string MainVideoStop { get; set; }
        public string MainVideoPlayFromJustBefore { get; set; }
        public string MainVideoPlayFromBeginning { get; set; }
        public string MainVideoPlayPauseToggle { get; set; }
        public string MainVideoPlay150Speed { get; set; }
        public string MainVideoPlay200Speed { get; set; }
        public string MainVideoFocusSetVideoPosition { get; set; }
        public string MainVideoToggleVideoControls { get; set; }
        public string MainVideo1FrameLeft { get; set; }
        public string MainVideo1FrameRight { get; set; }
        public string MainVideo1FrameLeftWithPlay { get; set; }
        public string MainVideo1FrameRightWithPlay { get; set; }
        public string MainVideo100MsLeft { get; set; }
        public string MainVideo100MsRight { get; set; }
        public string MainVideo500MsLeft { get; set; }
        public string MainVideo500MsRight { get; set; }
        public string MainVideo1000MsLeft { get; set; }
        public string MainVideo1000MsRight { get; set; }
        public string MainVideo5000MsLeft { get; set; }
        public string MainVideo5000MsRight { get; set; }
        public string MainVideoXSMsLeft { get; set; }
        public string MainVideoXSMsRight { get; set; }
        public string MainVideoXLMsLeft { get; set; }
        public string MainVideoXLMsRight { get; set; }
        public string MainVideo3000MsLeft { get; set; }
        public string MainVideo3000MsRight { get; set; }
        public string MainVideoGoToStartCurrent { get; set; }
        public string MainVideoToggleStartEndCurrent { get; set; }
        public string MainVideoPlaySelectedLines { get; set; }
        public string MainVideoLoopSelectedLines { get; set; }
        public string MainVideoGoToPrevSubtitle { get; set; }
        public string MainVideoGoToNextSubtitle { get; set; }
        public string MainVideoGoToPrevTimeCode { get; set; }
        public string MainVideoGoToNextTimeCode { get; set; }
        public string MainVideoGoToPrevChapter { get; set; }
        public string MainVideoGoToNextChapter { get; set; }
        public string MainVideoSelectNextSubtitle { get; set; }
        public string MainVideoFullscreen { get; set; }
        public string MainVideoSlower { get; set; }
        public string MainVideoFaster { get; set; }
        public string MainVideoSpeedToggle { get; set; }
        public string MainVideoReset { get; set; }
        public string MainVideoToggleBrightness { get; set; }
        public string MainVideoToggleContrast { get; set; }
        public string MainVideoAudioToTextVosk { get; set; }
        public string MainVideoAudioToTextWhisper { get; set; }
        public string MainVideoAudioExtractAudioSelectedLines { get; set; }
        public string MainVideoTextToSpeech { get; set; }

        // spell check
        public string MainSpellCheck { get; set; }
        public string MainSpellCheckFindDoubleWords { get; set; }
        public string MainSpellCheckAddWordToNames { get; set; }

        // Sync
        public string MainSynchronizationAdjustTimes { get; set; }
        public string MainSynchronizationVisualSync { get; set; }
        public string MainSynchronizationPointSync { get; set; }
        public string MainSynchronizationPointSyncViaFile { get; set; }
        public string MainSynchronizationChangeFrameRate { get; set; }

        // List view
        public string MainListViewItalic { get; set; }
        public string MainListViewBold { get; set; }
        public string MainListViewUnderline { get; set; }
        public string MainListViewBox { get; set; }
        public string MainListViewToggleQuotes { get; set; }
        public string MainListViewToggleHiTags { get; set; }
        public string MainListViewToggleCustomTags { get; set; }
        public string MainListViewSplit { get; set; }
        public string MainListViewToggleDashes { get; set; }
        public string MainListViewToggleMusicSymbols { get; set; }
        public string MainListViewAlignment { get; set; }
        public string MainListViewAlignmentN1 { get; set; }
        public string MainListViewAlignmentN2 { get; set; }
        public string MainListViewAlignmentN3 { get; set; }
        public string MainListViewAlignmentN4 { get; set; }
        public string MainListViewAlignmentN5 { get; set; }
        public string MainListViewAlignmentN6 { get; set; }
        public string MainListViewAlignmentN7 { get; set; }
        public string MainListViewAlignmentN8 { get; set; }
        public string MainListViewAlignmentN9 { get; set; }
        public string MainListViewColor1 { get; set; }
        public string MainListViewColor2 { get; set; }
        public string MainListViewColor3 { get; set; }
        public string MainListViewColor4 { get; set; }
        public string MainListViewColor5 { get; set; }
        public string MainListViewColor6 { get; set; }
        public string MainListViewColor7 { get; set; }
        public string MainListViewColor8 { get; set; }
        public string MainListViewSetNewActor { get; set; }
        public string MainListViewSetActor1 { get; set; }
        public string MainListViewSetActor2 { get; set; }
        public string MainListViewSetActor3 { get; set; }
        public string MainListViewSetActor4 { get; set; }
        public string MainListViewSetActor5 { get; set; }
        public string MainListViewSetActor6 { get; set; }
        public string MainListViewSetActor7 { get; set; }
        public string MainListViewSetActor8 { get; set; }
        public string MainListViewSetActor9 { get; set; }
        public string MainListViewSetActor10 { get; set; }
        public string MainListViewColorChoose { get; set; }
        public string MainRemoveFormatting { get; set; }
        public string MainListViewCopyText { get; set; }
        public string MainListViewCopyPlainText { get; set; }
        public string MainListViewCopyTextFromOriginalToCurrent { get; set; }
        public string MainListViewAutoDuration { get; set; }
        public string MainListViewColumnDeleteText { get; set; }
        public string MainListViewColumnDeleteTextAndShiftUp { get; set; }
        public string MainListViewColumnInsertText { get; set; }
        public string MainListViewColumnPaste { get; set; }
        public string MainListViewColumnTextUp { get; set; }
        public string MainListViewColumnTextDown { get; set; }
        public string MainListViewGoToNextError { get; set; }
        public string MainListViewListErrors { get; set; }
        public string MainListViewSortByNumber { get; set; }
        public string MainListViewSortByStartTime { get; set; }
        public string MainListViewSortByEndTime { get; set; }
        public string MainListViewSortByDuration { get; set; }
        public string MainListViewSortByGap { get; set; }
        public string MainListViewSortByText { get; set; }
        public string MainListViewSortBySingleLineMaxLen { get; set; }
        public string MainListViewSortByTextTotalLength { get; set; }
        public string MainListViewSortByCps { get; set; }
        public string MainListViewSortByWpm { get; set; }
        public string MainListViewSortByNumberOfLines { get; set; }
        public string MainListViewSortByActor { get; set; }
        public string MainListViewSortByStyle { get; set; }
        public string MainListViewRemoveTimeCodes { get; set; }
        public string MainTextBoxSplitAtCursor { get; set; }
        public string MainTextBoxSplitAtCursorAndAutoBr { get; set; }
        public string MainTextBoxSplitAtCursorAndVideoPos { get; set; }
        public string MainTextBoxSplitSelectedLineBilingual { get; set; }
        public string MainTextBoxMoveLastWordDown { get; set; }
        public string MainTextBoxMoveFirstWordFromNextUp { get; set; }
        public string MainTextBoxMoveLastWordDownCurrent { get; set; }
        public string MainTextBoxMoveFirstWordUpCurrent { get; set; }
        public string MainTextBoxMoveFromCursorToNextAndGoToNext { get; set; }
        public string MainTextBoxSelectionToLower { get; set; }
        public string MainTextBoxSelectionToUpper { get; set; }
        public string MainTextBoxSelectionToggleCasing { get; set; }
        public string MainTextBoxSelectionToRuby { get; set; }
        public string MainTextBoxToggleAutoDuration { get; set; }
        public string MainCreateInsertSubAtVideoPos { get; set; }
        public string MainCreateInsertSubAtVideoPosMax { get; set; }
        public string MainCreateInsertSubAtVideoPosNoTextBoxFocus { get; set; }
        public string MainCreateSetStart { get; set; }
        public string MainCreateSetEnd { get; set; }
        public string MainAdjustVideoSetStartForAppropriateLine { get; set; }
        public string MainAdjustVideoSetEndForAppropriateLine { get; set; }
        public string MainAdjustSetEndAndPause { get; set; }
        public string MainCreateSetEndAddNewAndGoToNew { get; set; }
        public string MainCreateStartDownEndUp { get; set; }
        public string MainAdjustSetStartAndOffsetTheRest { get; set; }
        public string MainAdjustSetStartAndOffsetTheRest2 { get; set; }
        public string MainAdjustSetStartAndOffsetTheWholeSubtitle { get; set; }
        public string MainAdjustSetEndAndOffsetTheRest { get; set; }
        public string MainAdjustSetEndAndOffsetTheRestAndGoToNext { get; set; }
        public string MainAdjustSetStartAndGotoNext { get; set; }
        public string MainAdjustSetEndAndGotoNext { get; set; }
        public string MainAdjustViaEndAutoStart { get; set; }
        public string MainAdjustViaEndAutoStartAndGoToNext { get; set; }
        public string MainAdjustSetEndMinusGapAndStartNextHere { get; set; }
        public string MainSetEndAndStartNextAfterGap { get; set; }
        public string MainAdjustSetStartAutoDurationAndGoToNext { get; set; }
        public string MainAdjustSetEndNextStartAndGoToNext { get; set; }
        public string MainAdjustStartDownEndUpAndGoToNext { get; set; }
        public string MainAdjustSetStartAndEndOfPrevious { get; set; }
        public string MainAdjustSetStartAndEndOfPreviousAndGoToNext { get; set; }
        public string MainAdjustSetStartKeepDuration { get; set; }
        public string MainAdjustSelected100MsForward { get; set; }
        public string MainAdjustSelected100MsBack { get; set; }
        public string MainAdjustStartXMsBack { get; set; }
        public string MainAdjustStartXMsForward { get; set; }
        public string MainAdjustEndXMsBack { get; set; }
        public string MainAdjustEndXMsForward { get; set; }
        public string MoveStartOneFrameBack { get; set; }
        public string MoveStartOneFrameForward { get; set; }
        public string MoveEndOneFrameBack { get; set; }
        public string MoveEndOneFrameForward { get; set; }
        public string MoveStartOneFrameBackKeepGapPrev { get; set; }
        public string MoveStartOneFrameForwardKeepGapPrev { get; set; }
        public string MoveEndOneFrameBackKeepGapNext { get; set; }
        public string MoveEndOneFrameForwardKeepGapNext { get; set; }
        public string MainAdjustSnapStartToNextShotChange { get; set; }
        public string MainAdjustSnapEndToPreviousShotChange { get; set; }
        public string MainAdjustExtendToNextShotChange { get; set; }
        public string MainAdjustExtendToPreviousShotChange { get; set; }
        public string MainAdjustExtendToNextSubtitle { get; set; }
        public string MainAdjustExtendToPreviousSubtitle { get; set; }
        public string MainAdjustExtendToNextSubtitleMinusChainingGap { get; set; }
        public string MainAdjustExtendToPreviousSubtitleMinusChainingGap { get; set; }
        public string MainAdjustExtendCurrentSubtitle { get; set; }
        public string MainAdjustExtendPreviousLineEndToCurrentStart { get; set; }
        public string MainAdjustExtendNextLineStartToCurrentEnd { get; set; }
        public string MainSetInCueToClosestShotChangeLeftGreenZone { get; set; }
        public string MainSetInCueToClosestShotChangeRightGreenZone { get; set; }
        public string MainSetOutCueToClosestShotChangeLeftGreenZone { get; set; }
        public string MainSetOutCueToClosestShotChangeRightGreenZone { get; set; }
        public string GeneralAutoCalcCurrentDuration { get; set; }
        public string GeneralAutoCalcCurrentDurationByOptimalReadingSpeed { get; set; }
        public string GeneralAutoCalcCurrentDurationByMinReadingSpeed { get; set; }
        public string MainInsertAfter { get; set; }
        public string MainTextBoxAutoBreak { get; set; }
        public string MainTextBoxBreakAtPosition { get; set; }
        public string MainTextBoxBreakAtPositionAndGoToNext { get; set; }
        public string MainTextBoxRecord { get; set; }
        public string MainTextBoxUnbreak { get; set; }
        public string MainTextBoxUnbreakNoSpace { get; set; }
        public string MainTextBoxAssaIntellisense { get; set; }
        public string MainTextBoxAssaRemoveTag { get; set; }
        public string MainWaveformInsertAtCurrentPosition { get; set; }
        public string MainInsertBefore { get; set; }
        public string MainMergeDialog { get; set; }
        public string MainMergeDialogWithNext { get; set; }
        public string MainMergeDialogWithPrevious { get; set; }
        public string MainAutoBalanceSelectedLines { get; set; }
        public string MainEvenlyDistributeSelectedLines { get; set; }
        public string MainToggleFocus { get; set; }
        public string MainToggleFocusWaveform { get; set; }
        public string MainToggleFocusWaveformTextBox { get; set; }
        public string WaveformAdd { get; set; }
        public string WaveformVerticalZoom { get; set; }
        public string WaveformVerticalZoomOut { get; set; }
        public string WaveformZoomIn { get; set; }
        public string WaveformZoomOut { get; set; }
        public string WaveformSplit { get; set; }
        public string WaveformPlaySelection { get; set; }
        public string WaveformPlaySelectionEnd { get; set; }
        public string WaveformSearchSilenceForward { get; set; }
        public string WaveformSearchSilenceBack { get; set; }
        public string WaveformAddTextHere { get; set; }
        public string WaveformAddTextHereFromClipboard { get; set; }
        public string WaveformSetParagraphAsSelection { get; set; }
        public string WaveformGoToPreviousShotChange { get; set; }
        public string WaveformGoToNextShotChange { get; set; }
        public string WaveformToggleShotChange { get; set; }
        public string WaveformListShotChanges { get; set; }
        public string WaveformGuessStart { get; set; }
        public string Waveform100MsLeft { get; set; }
        public string Waveform100MsRight { get; set; }
        public string Waveform1000MsLeft { get; set; }
        public string Waveform1000MsRight { get; set; }
        public string WaveformAudioToTextVosk { get; set; }
        public string WaveformAudioToTextWhisper { get; set; }
        public string MainCheckFixTimingViaShotChanges { get; set; }
        public string MainTranslateGoogleIt { get; set; }
        public string MainTranslateGoogleTranslateIt { get; set; }
        public string MainTranslateAuto { get; set; }
        public string MainTranslateAutoSelectedLines { get; set; }
        public string MainTranslateCustomSearch1 { get; set; }
        public string MainTranslateCustomSearch2 { get; set; }
        public string MainTranslateCustomSearch3 { get; set; }
        public string MainTranslateCustomSearch4 { get; set; }
        public string MainTranslateCustomSearch5 { get; set; }
        public List<PluginShortcut> PluginShortcuts { get; set; }


        public Shortcuts()
        {
            GeneralGoToFirstSelectedLine = "Control+L";
            GeneralMergeSelectedLines = "Control+Shift+M";
            GeneralToggleTranslationMode = "Control+Shift+O";
            GeneralMergeOriginalAndTranslation = "Control+Alt+Shift+M";
            GeneralGoToNextSubtitle = "Shift+Return";
            GeneralGoToNextSubtitlePlayTranslate = "Alt+Down";
            GeneralGoToPrevSubtitlePlayTranslate = "Alt+Up";
            GeneralToggleBookmarksWithText = "Control+Shift+B";
            OpenDataFolder = "Control+Alt+Shift+D";
            GeneralToggleView = "F2";
            GeneralHelp = "F1";
            MainFileNew = "Control+N";
            MainFileOpen = "Control+O";
            MainFileSave = "Control+S";
            MainFileSaveAs = "Control+Shift+S";
            MainEditUndo = "Control+Z";
            MainEditRedo = "Control+Y";
            MainEditFind = "Control+F";
            MainEditFindNext = "F3";
            MainEditReplace = "Control+H";
            MainEditMultipleReplace = "Control+Alt+M";
            MainEditGoToLineNumber = "Control+G";
            MainEditRightToLeft = "Control+Shift+Alt+R";
            MainEditInverseSelection = "Control+Shift+I";
            MainToolsFixCommonErrors = "Control+Shift+F";
            MainToolsFixCommonErrorsPreview = "Control+P";
            MainToolsRenumber = "Control+Shift+N";
            MainToolsRemoveTextForHI = "Control+Shift+H";
            MainToolsChangeCasing = "Control+Shift+C";
            MainVideoPlayFromJustBefore = "Shift+F10";
            MainVideoPlayPauseToggle = "Control+P";
            MainVideoPause = "Control+Alt+P";
            MainVideo500MsLeft = "Alt+Left";
            MainVideo500MsRight = "Alt+Right";
            MainVideoFullscreen = "Alt+Return";
            MainVideoReset = "Control+D0";
            MainSpellCheck = "Control+Shift+S";
            MainSpellCheckFindDoubleWords = "Control+Shift+D";
            MainSpellCheckAddWordToNames = "Control+Shift+L";
            MainSynchronizationAdjustTimes = "Control+Shift+A";
            MainSynchronizationVisualSync = "Control+Shift+V";
            MainSynchronizationPointSync = "Control+Shift+P";
            MainListViewItalic = "Control+I";
            MainTextBoxSplitAtCursor = "Control+Alt+V";
            MainTextBoxSelectionToLower = "Control+U";
            MainTextBoxSelectionToUpper = "Control+Shift+U";
            MainTextBoxSelectionToggleCasing = "Control+Shift+F3";
            MainCreateInsertSubAtVideoPos = "Shift+F9";
            MainVideoGoToStartCurrent = "Shift+F11";
            MainVideoToggleStartEndCurrent = "F4";
            MainVideoPlaySelectedLines = "F5";
            MainVideoGoToStartCurrent = "F6";
            MainVideo3000MsLeft = "F7";
            MainListViewGoToNextError = "F8";
            MainListViewListErrors = "Control+F8";
            MainCreateSetStart = "F11";
            MainCreateSetEnd = "F12";
            MainAdjustSetStartAndOffsetTheRest = "Control+Space";
            MainAdjustSetStartAndOffsetTheRest2 = "F9";
            MainAdjustSetEndAndGotoNext = "F10";
            MainInsertAfter = "Alt+Insert";
            MainWaveformInsertAtCurrentPosition = "Insert";
            MainInsertBefore = "Control+Shift+Insert";
            MainTextBoxAutoBreak = "Control+R";
            MainTranslateAuto = "Control+Shift+G";
            MainAdjustExtendToNextSubtitle = "Control+Shift+E";
            MainAdjustExtendToPreviousSubtitle = "Alt+Shift+E";
            MainAdjustExtendToNextSubtitleMinusChainingGap = "Control+Shift+W";
            MainAdjustExtendToPreviousSubtitleMinusChainingGap = "Alt+Shift+W";
            WaveformVerticalZoom = "Shift+Add";
            WaveformVerticalZoomOut = "Shift+Subtract";
            WaveformAddTextHere = "Return";
            Waveform100MsLeft = "Shift+Left";
            Waveform100MsRight = "Shift+Right";
            Waveform1000MsLeft = "Left";
            Waveform1000MsRight = "Right";
            MainCheckFixTimingViaShotChanges = "Control+Shift+D9";
            PluginShortcuts = new List<PluginShortcut>();
        }

        public Shortcuts Clone()
        {
            var xws = new XmlWriterSettings { Indent = true };
            var sb = new StringBuilder();
            using (var textWriter = XmlWriter.Create(sb, xws))
            {
                textWriter.WriteStartDocument();
                textWriter.WriteStartElement("Settings", string.Empty);
                Settings.WriteShortcuts(this, textWriter);
                textWriter.WriteEndElement();
                textWriter.WriteEndDocument();
            }

            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.LoadXml(sb.ToString().Replace("encoding=\"utf-16\"", "encoding=\"utf-8\""));
            var shortcuts = new Shortcuts();
            Settings.ReadShortcuts(doc, shortcuts);
            return shortcuts;
        }

        public static void Save(string fileName, Shortcuts shortcuts)
        {
            var xws = new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 };
            var sb = new StringBuilder();
            using (var textWriter = XmlWriter.Create(sb, xws))
            {
                textWriter.WriteStartDocument();
                textWriter.WriteStartElement("Settings", string.Empty);
                Settings.WriteShortcuts(shortcuts, textWriter);
                textWriter.WriteEndElement();
                textWriter.WriteEndDocument();
            }
            File.WriteAllText(fileName, sb.ToString().Replace("encoding=\"utf-16\"", "encoding=\"utf-8\""), Encoding.UTF8);
        }

        public static Shortcuts Load(string fileName)
        {
            var doc = new XmlDocument { PreserveWhitespace = true };
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                doc.Load(stream);
                var shortcuts = new Shortcuts();
                Settings.ReadShortcuts(doc, shortcuts);
                return shortcuts;
            }
        }
    }

    public class RemoveTextForHearingImpairedSettings
    {
        public bool RemoveTextBetweenBrackets { get; set; }
        public bool RemoveTextBetweenParentheses { get; set; }
        public bool RemoveTextBetweenCurlyBrackets { get; set; }
        public bool RemoveTextBetweenQuestionMarks { get; set; }
        public bool RemoveTextBetweenCustom { get; set; }
        public string RemoveTextBetweenCustomBefore { get; set; }
        public string RemoveTextBetweenCustomAfter { get; set; }
        public bool RemoveTextBetweenOnlySeparateLines { get; set; }
        public bool RemoveTextBeforeColon { get; set; }
        public bool RemoveTextBeforeColonOnlyIfUppercase { get; set; }
        public bool RemoveTextBeforeColonOnlyOnSeparateLine { get; set; }
        public bool RemoveInterjections { get; set; }
        public bool RemoveInterjectionsOnlyOnSeparateLine { get; set; }
        public bool RemoveIfContains { get; set; }
        public bool RemoveIfAllUppercase { get; set; }
        public string RemoveIfContainsText { get; set; }
        public bool RemoveIfOnlyMusicSymbols { get; set; }

        public RemoveTextForHearingImpairedSettings()
        {
            RemoveTextBetweenBrackets = true;
            RemoveTextBetweenParentheses = true;
            RemoveTextBetweenCurlyBrackets = true;
            RemoveTextBetweenQuestionMarks = true;
            RemoveTextBetweenCustom = false;
            RemoveTextBetweenCustomBefore = "¶";
            RemoveTextBetweenCustomAfter = "¶";
            RemoveTextBeforeColon = true;
            RemoveTextBeforeColonOnlyIfUppercase = true;
            RemoveIfContainsText = "¶";
            RemoveIfOnlyMusicSymbols = true;
        }
    }

    public class SubtitleBeaming
    {
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public Color FontColor { get; set; }
        public Color BorderColor { get; set; }
        public int BorderWidth { get; set; }

        public SubtitleBeaming()
        {
            FontName = "Verdana";
            FontSize = 30;
            FontColor = Color.White;
            BorderColor = Color.DarkGray;
            BorderWidth = 2;
        }
    }

    public class BeautifyTimeCodesSettings
    {
        public bool AlignTimeCodes { get; set; }
        public bool ExtractExactTimeCodes { get; set; }
        public bool SnapToShotChanges { get; set; }
        public int OverlapThreshold { get; set; }
        public BeautifyTimeCodesProfile Profile { get; set; }

        public BeautifyTimeCodesSettings()
        {
            AlignTimeCodes = true;
            ExtractExactTimeCodes = false;
            SnapToShotChanges = true;
            OverlapThreshold = 1000;
            Profile = new BeautifyTimeCodesProfile();
        }

        public class BeautifyTimeCodesProfile
        {
            // General
            public int Gap { get; set; }

            // In cues
            public int InCuesGap { get; set; }
            public int InCuesLeftGreenZone { get; set; }
            public int InCuesLeftRedZone { get; set; }
            public int InCuesRightRedZone { get; set; }
            public int InCuesRightGreenZone { get; set; }

            // Out cues
            public int OutCuesGap { get; set; }
            public int OutCuesLeftGreenZone { get; set; }
            public int OutCuesLeftRedZone { get; set; }
            public int OutCuesRightRedZone { get; set; }
            public int OutCuesRightGreenZone { get; set; }

            // Connected subtitles
            public int ConnectedSubtitlesInCueClosestLeftGap { get; set; }
            public int ConnectedSubtitlesInCueClosestRightGap { get; set; }
            public int ConnectedSubtitlesOutCueClosestLeftGap { get; set; }
            public int ConnectedSubtitlesOutCueClosestRightGap { get; set; }
            public int ConnectedSubtitlesLeftGreenZone { get; set; }
            public int ConnectedSubtitlesLeftRedZone { get; set; }
            public int ConnectedSubtitlesRightRedZone { get; set; }
            public int ConnectedSubtitlesRightGreenZone { get; set; }
            public int ConnectedSubtitlesTreatConnected { get; set; }

            // Chaining
            public bool ChainingGeneralUseZones { get; set; }
            public int ChainingGeneralMaxGap { get; set; }
            public int ChainingGeneralLeftGreenZone { get; set; }
            public int ChainingGeneralLeftRedZone { get; set; }
            public ChainingShotChangeBehaviorEnum ChainingGeneralShotChangeBehavior { get; set; }
            public bool ChainingInCueOnShotUseZones { get; set; }
            public int ChainingInCueOnShotMaxGap { get; set; }
            public int ChainingInCueOnShotLeftGreenZone { get; set; }
            public int ChainingInCueOnShotLeftRedZone { get; set; }
            public ChainingShotChangeBehaviorEnum ChainingInCueOnShotShotChangeBehavior { get; set; }
            public bool ChainingInCueOnShotCheckGeneral { get; set; }
            public bool ChainingOutCueOnShotUseZones { get; set; }
            public int ChainingOutCueOnShotMaxGap { get; set; }
            public int ChainingOutCueOnShotRightRedZone { get; set; }
            public int ChainingOutCueOnShotRightGreenZone { get; set; }
            public ChainingShotChangeBehaviorEnum ChainingOutCueOnShotShotChangeBehavior { get; set; }
            public bool ChainingOutCueOnShotCheckGeneral { get; set; }

            public enum Preset : int
            {
                Default = 0,
                Netflix = 1,
                SDI = 2,
            }

            public enum ChainingShotChangeBehaviorEnum : int
            {
                DontChain = 0,
                ExtendCrossingShotChange = 1,
                ExtendUntilShotChange = 2
            }

            public BeautifyTimeCodesProfile(Preset preset = Preset.Default)
            {
                switch (preset)
                {
                    case Preset.Netflix:
                        Gap = 2;

                        InCuesGap = 0;
                        InCuesLeftGreenZone = 12;
                        InCuesLeftRedZone = 7;
                        InCuesRightRedZone = 7;
                        InCuesRightGreenZone = 12;

                        OutCuesGap = 2;
                        OutCuesLeftGreenZone = 12;
                        OutCuesLeftRedZone = 7;
                        OutCuesRightRedZone = 7;
                        OutCuesRightGreenZone = 12;

                        ConnectedSubtitlesInCueClosestLeftGap = 2;
                        ConnectedSubtitlesInCueClosestRightGap = 0;
                        ConnectedSubtitlesOutCueClosestLeftGap = 2;
                        ConnectedSubtitlesOutCueClosestRightGap = 0;
                        ConnectedSubtitlesLeftGreenZone = 12;
                        ConnectedSubtitlesLeftRedZone = 7;
                        ConnectedSubtitlesRightRedZone = 7;
                        ConnectedSubtitlesRightGreenZone = 12;
                        ConnectedSubtitlesTreatConnected = 180;

                        ChainingGeneralUseZones = false;
                        ChainingGeneralMaxGap = 500;
                        ChainingGeneralLeftGreenZone = 12;
                        ChainingGeneralLeftRedZone = 11;
                        ChainingGeneralShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendUntilShotChange;
                        ChainingInCueOnShotUseZones = false;
                        ChainingInCueOnShotMaxGap = 500;
                        ChainingInCueOnShotLeftGreenZone = 12;
                        ChainingInCueOnShotLeftRedZone = 11;
                        ChainingInCueOnShotShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendUntilShotChange;
                        ChainingInCueOnShotCheckGeneral = true;
                        ChainingOutCueOnShotUseZones = false;
                        ChainingOutCueOnShotMaxGap = 500;
                        ChainingOutCueOnShotRightRedZone = 11;
                        ChainingOutCueOnShotRightGreenZone = 12;
                        ChainingOutCueOnShotShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendUntilShotChange;
                        ChainingOutCueOnShotCheckGeneral = true;
                        break;
                    case Preset.SDI:
                        Gap = 4;

                        InCuesGap = 2;
                        InCuesLeftGreenZone = 12;
                        InCuesLeftRedZone = 7;
                        InCuesRightRedZone = 7;
                        InCuesRightGreenZone = 12;

                        OutCuesGap = 2;
                        OutCuesLeftGreenZone = 12;
                        OutCuesLeftRedZone = 7;
                        OutCuesRightRedZone = 7;
                        OutCuesRightGreenZone = 12;

                        ConnectedSubtitlesInCueClosestLeftGap = 2;
                        ConnectedSubtitlesInCueClosestRightGap = 2;
                        ConnectedSubtitlesOutCueClosestLeftGap = 2;
                        ConnectedSubtitlesOutCueClosestRightGap = 2;
                        ConnectedSubtitlesLeftGreenZone = 12;
                        ConnectedSubtitlesLeftRedZone = 7;
                        ConnectedSubtitlesRightRedZone = 7;
                        ConnectedSubtitlesRightGreenZone = 12;
                        ConnectedSubtitlesTreatConnected = 240;

                        ChainingGeneralUseZones = false;
                        ChainingGeneralMaxGap = 1000;
                        ChainingGeneralLeftGreenZone = 25;
                        ChainingGeneralLeftRedZone = 24;
                        ChainingGeneralShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendCrossingShotChange;
                        ChainingInCueOnShotUseZones = false;
                        ChainingInCueOnShotMaxGap = 1000;
                        ChainingInCueOnShotLeftGreenZone = 25;
                        ChainingInCueOnShotLeftRedZone = 24;
                        ChainingInCueOnShotShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendCrossingShotChange;
                        ChainingInCueOnShotCheckGeneral = true;
                        ChainingOutCueOnShotUseZones = true;
                        ChainingOutCueOnShotMaxGap = 500;
                        ChainingOutCueOnShotRightRedZone = 7;
                        ChainingOutCueOnShotRightGreenZone = 12;
                        ChainingOutCueOnShotShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendCrossingShotChange;
                        ChainingOutCueOnShotCheckGeneral = true;
                        break;
                    default:
                        Gap = 3;

                        InCuesGap = 0;
                        InCuesLeftGreenZone = 3;
                        InCuesLeftRedZone = 3;
                        InCuesRightRedZone = 5;
                        InCuesRightGreenZone = 5;

                        OutCuesGap = 0;
                        OutCuesLeftGreenZone = 10;
                        OutCuesLeftRedZone = 10;
                        OutCuesRightRedZone = 3;
                        OutCuesRightGreenZone = 12;

                        ConnectedSubtitlesInCueClosestLeftGap = 3;
                        ConnectedSubtitlesInCueClosestRightGap = 0;
                        ConnectedSubtitlesOutCueClosestLeftGap = 0;
                        ConnectedSubtitlesOutCueClosestRightGap = 3;
                        ConnectedSubtitlesLeftGreenZone = 3;
                        ConnectedSubtitlesLeftRedZone = 3;
                        ConnectedSubtitlesRightRedZone = 3;
                        ConnectedSubtitlesRightGreenZone = 3;
                        ConnectedSubtitlesTreatConnected = 180;

                        ChainingGeneralUseZones = false;
                        ChainingGeneralMaxGap = 1000;
                        ChainingGeneralLeftGreenZone = 25;
                        ChainingGeneralLeftRedZone = 24;
                        ChainingGeneralShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendUntilShotChange;
                        ChainingInCueOnShotUseZones = false;
                        ChainingInCueOnShotMaxGap = 1000;
                        ChainingInCueOnShotLeftGreenZone = 25;
                        ChainingInCueOnShotLeftRedZone = 24;
                        ChainingInCueOnShotShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendUntilShotChange;
                        ChainingInCueOnShotCheckGeneral = true;
                        ChainingOutCueOnShotUseZones = false;
                        ChainingOutCueOnShotMaxGap = 500;
                        ChainingOutCueOnShotRightRedZone = 11;
                        ChainingOutCueOnShotRightGreenZone = 12;
                        ChainingOutCueOnShotShotChangeBehavior = ChainingShotChangeBehaviorEnum.ExtendUntilShotChange;
                        ChainingOutCueOnShotCheckGeneral = true;
                        break;
                }
            }
        }
    }

    public class CompareSettings
    {
        public bool ShowOnlyDifferences { get; set; }
        public bool OnlyLookForDifferenceInText { get; set; }
        public bool IgnoreLineBreaks { get; set; }
        public bool IgnoreWhitespace { get; set; }
        public bool IgnoreFormatting { get; set; }

        public CompareSettings()
        {
            OnlyLookForDifferenceInText = true;
        }
    }

    public class VerifyCompletenessSettings
    {
        public ListSortEnum ListSort { get; set; }

        public enum ListSortEnum : int
        {
            Coverage = 0,
            Time = 1,
        }

        public VerifyCompletenessSettings()
        {
            ListSort = ListSortEnum.Coverage;
        }
    }

    public class Settings
    {
        public string Version { get; set; }
        public bool InitialLoad { get; set; }
        public CompareSettings Compare { get; set; }
        public VerifyCompletenessSettings VerifyCompleteness { get; set; }
        public RecentFilesSettings RecentFiles { get; set; }
        public GeneralSettings General { get; set; }
        public ToolsSettings Tools { get; set; }
        public FcpExportSettings FcpExportSettings { get; set; }
        public SubtitleSettings SubtitleSettings { get; set; }
        public ProxySettings Proxy { get; set; }
        public WordListSettings WordLists { get; set; }
        public FixCommonErrorsSettings CommonErrors { get; set; }
        public VobSubOcrSettings VobSubOcr { get; set; }
        public VideoControlsSettings VideoControls { get; set; }
        public NetworkSettings NetworkSettings { get; set; }
        public Shortcuts Shortcuts { get; set; }
        public RemoveTextForHearingImpairedSettings RemoveTextForHearingImpaired { get; set; }
        public SubtitleBeaming SubtitleBeaming { get; set; }
        public List<MultipleSearchAndReplaceGroup> MultipleSearchAndReplaceGroups { get; set; }
        public BeautifyTimeCodesSettings BeautifyTimeCodes { get; set; }

        public void Reset()
        {
            RecentFiles = new RecentFilesSettings();
            General = new GeneralSettings();
            Tools = new ToolsSettings();
            FcpExportSettings = new FcpExportSettings();
            WordLists = new WordListSettings();
            SubtitleSettings = new SubtitleSettings();
            Proxy = new ProxySettings();
            CommonErrors = new FixCommonErrorsSettings();
            VobSubOcr = new VobSubOcrSettings();
            VideoControls = new VideoControlsSettings();
            NetworkSettings = new NetworkSettings();
            MultipleSearchAndReplaceGroups = new List<MultipleSearchAndReplaceGroup>();
            Shortcuts = new Shortcuts();
            RemoveTextForHearingImpaired = new RemoveTextForHearingImpairedSettings();
            SubtitleBeaming = new SubtitleBeaming();
            Compare = new CompareSettings();
            VerifyCompleteness = new VerifyCompletenessSettings();
            BeautifyTimeCodes = new BeautifyTimeCodesSettings();
        }

        private Settings()
        {
            Reset();
        }

        public void Save()
        {
            // this is too slow: Serialize(Configuration.SettingsFileName, this);
            CustomSerialize(Configuration.SettingsFileName, this);
        }

        //private static void Serialize(string fileName, Settings settings)
        //{
        //    var s = new XmlSerializer(typeof(Settings));
        //    TextWriter w = new StreamWriter(fileName);
        //    s.Serialize(w, settings);
        //    w.Close();
        //}

        public static bool UseLegacyHtmlColor = true;
        public static bool IsVersion3 = false;

        public static Settings GetSettings()
        {
            var settings = new Settings();
            var settingsFileName = Configuration.SettingsFileName;
            if (File.Exists(settingsFileName))
            {
                try
                {
                    //too slow... :(  - settings = Deserialize(settingsFileName); // 688 msecs
                    settings = CustomDeserialize(settingsFileName); //  15 msecs

                    if (settings.General.DefaultEncoding.StartsWith("utf-8", StringComparison.Ordinal))
                    {
                        settings.General.DefaultEncoding = TextEncoding.Utf8WithBom;
                    }

                    if (settings.Version.StartsWith("3.", StringComparison.Ordinal))
                    {
                        IsVersion3 = true;
                    }

                    settings.General.UseLegacyHtmlColor = false;
                    UseLegacyHtmlColor = false;
                }
                catch (Exception exception)
                {
                    settings = new Settings();
                    settings.InitialLoad = true;
                    SeLogger.Error(exception, "Failed to load " + settingsFileName);

                    var ffmpegFullPath = Path.Combine(Configuration.DataDirectory, "ffmpeg", "ffmpeg.exe");
                    if (Configuration.IsRunningOnWindows && File.Exists(ffmpegFullPath))
                    {
                        settings.General.FFmpegLocation = ffmpegFullPath;
                        settings.General.UseFFmpegForWaveExtraction = true;
                    }
                }

                if (!string.IsNullOrEmpty(settings.General.ListViewLineSeparatorString))
                {
                    settings.General.ListViewLineSeparatorString = settings.General.ListViewLineSeparatorString.Replace("\n", string.Empty).Replace("\r", string.Empty);
                }

                if (string.IsNullOrWhiteSpace(settings.General.ListViewLineSeparatorString))
                {
                    settings.General.ListViewLineSeparatorString = "<br />";
                }

                if (settings.Shortcuts.GeneralToggleTranslationMode == "Control+U" && settings.Shortcuts.MainTextBoxSelectionToLower == "Control+U")
                {
                    settings.Shortcuts.GeneralToggleTranslationMode = "Control+Shift+O";
                    settings.Shortcuts.GeneralSwitchOriginalAndTranslation = "Control+Alt+O";
                }

                if (settings.General.UseFFmpegForWaveExtraction && string.IsNullOrEmpty(settings.General.FFmpegLocation) && Configuration.IsRunningOnWindows)
                {
                    var guessPath = Path.Combine(Configuration.DataDirectory, "ffmpeg", "ffmpeg.exe");
                    if (File.Exists(guessPath))
                    {
                        settings.General.FFmpegLocation = guessPath;
                    }
                }
            }
            else
            {
                settings.InitialLoad = true;
                var ffmpegFullPath = Path.Combine(Configuration.DataDirectory, "ffmpeg", "ffmpeg.exe");
                if (Configuration.IsRunningOnWindows && File.Exists(ffmpegFullPath))
                {
                    settings.General.FFmpegLocation = ffmpegFullPath;
                    settings.General.UseFFmpegForWaveExtraction = true;
                }
            }

            return settings;
        }

        //private static Settings Deserialize(string fileName)
        //{
        //    var r = new StreamReader(fileName);
        //    var s = new XmlSerializer(typeof(Settings));
        //    var settings = (Settings)s.Deserialize(r);
        //    r.Close();

        //    if (settings.RecentFiles == null)
        //        settings.RecentFiles = new RecentFilesSettings();
        //    if (settings.General == null)
        //        settings.General = new GeneralSettings();
        //    if (settings.SsaStyle == null)
        //        settings.SsaStyle = new SsaStyleSettings();
        //    if (settings.CommonErrors == null)
        //        settings.CommonErrors = new FixCommonErrorsSettings();
        //    if (settings.VideoControls == null)
        //        settings.VideoControls = new VideoControlsSettings();
        //    if (settings.VobSubOcr == null)
        //        settings.VobSubOcr = new VobSubOcrSettings();
        //    if (settings.MultipleSearchAndReplaceList == null)
        //        settings.MultipleSearchAndReplaceList = new List<MultipleSearchAndReplaceSetting>();
        //    if (settings.NetworkSettings == null)
        //        settings.NetworkSettings = new NetworkSettings();
        //    if (settings.Shortcuts == null)
        //        settings.Shortcuts = new Shortcuts();

        //    return settings;
        //}

        /// <summary>
        /// A faster serializer than xml serializer... which is insanely slow (first time)!!!!
        /// This method is auto-generated with XmlSerializerGenerator
        /// </summary>
        /// <param name="fileName">File name of xml settings file to load</param>
        /// <returns>Newly loaded settings</returns>
        private static Settings CustomDeserialize(string fileName)
        {
            var doc = new XmlDocument { PreserveWhitespace = true };
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                doc.Load(stream);
            }

            var settings = new Settings();

            XmlNode versionNode = doc.DocumentElement.SelectSingleNode("Version");
            if (versionNode != null)
            {
                settings.Version = versionNode.InnerText;
            }

            // Compare
            XmlNode nodeCompare = doc.DocumentElement.SelectSingleNode("Compare");
            if (nodeCompare != null)
            {
                XmlNode xnode = nodeCompare.SelectSingleNode("ShowOnlyDifferences");
                if (xnode != null)
                {
                    settings.Compare.ShowOnlyDifferences = Convert.ToBoolean(xnode.InnerText);
                }

                xnode = nodeCompare.SelectSingleNode("OnlyLookForDifferenceInText");
                if (xnode != null)
                {
                    settings.Compare.OnlyLookForDifferenceInText = Convert.ToBoolean(xnode.InnerText);
                }

                xnode = nodeCompare.SelectSingleNode("IgnoreLineBreaks");
                if (xnode != null)
                {
                    settings.Compare.IgnoreLineBreaks = Convert.ToBoolean(xnode.InnerText);
                }

                xnode = nodeCompare.SelectSingleNode("IgnoreWhitespace");
                if (xnode != null)
                {
                    settings.Compare.IgnoreWhitespace = Convert.ToBoolean(xnode.InnerText);
                }

                xnode = nodeCompare.SelectSingleNode("IgnoreFormatting");
                if (xnode != null)
                {
                    settings.Compare.IgnoreFormatting = Convert.ToBoolean(xnode.InnerText);
                }
            }

            // Verify completeness
            XmlNode nodeVerifyCompleteness = doc.DocumentElement.SelectSingleNode("VerifyCompleteness");
            if (nodeVerifyCompleteness != null)
            {
                XmlNode xnode = nodeVerifyCompleteness.SelectSingleNode("ListSort");
                if (xnode != null)
                {
                    settings.VerifyCompleteness.ListSort = (VerifyCompletenessSettings.ListSortEnum)Enum.Parse(typeof(VerifyCompletenessSettings.ListSortEnum), xnode.InnerText);
                }
            }

            // Recent files
            XmlNode node = doc.DocumentElement.SelectSingleNode("RecentFiles");
            foreach (XmlNode listNode in node.SelectNodes("FileNames/FileName"))
            {
                string firstVisibleIndex = "-1";
                if (listNode.Attributes["FirstVisibleIndex"] != null)
                {
                    firstVisibleIndex = listNode.Attributes["FirstVisibleIndex"].Value;
                }

                string firstSelectedIndex = "-1";
                if (listNode.Attributes["FirstSelectedIndex"] != null)
                {
                    firstSelectedIndex = listNode.Attributes["FirstSelectedIndex"].Value;
                }

                string videoFileName = null;
                if (listNode.Attributes["VideoFileName"] != null)
                {
                    videoFileName = listNode.Attributes["VideoFileName"].Value;
                }

                string audioTrack = "-1";
                if (listNode.Attributes["AudioTrack"] != null)
                {
                    audioTrack = listNode.Attributes["AudioTrack"].Value;
                }

                string originalFileName = null;
                if (listNode.Attributes["OriginalFileName"] != null)
                {
                    originalFileName = listNode.Attributes["OriginalFileName"].Value;
                }

                long videoOffset = 0;
                if (listNode.Attributes["VideoOffset"] != null)
                {
                    long.TryParse(listNode.Attributes["VideoOffset"].Value, out videoOffset);
                }

                bool isSmpte = false;
                if (listNode.Attributes["IsSmpte"] != null)
                {
                    bool.TryParse(listNode.Attributes["IsSmpte"].Value, out isSmpte);
                }

                settings.RecentFiles.Files.Add(new RecentFileEntry { FileName = listNode.InnerText, FirstVisibleIndex = int.Parse(firstVisibleIndex, CultureInfo.InvariantCulture), FirstSelectedIndex = int.Parse(firstSelectedIndex, CultureInfo.InvariantCulture), VideoFileName = videoFileName, AudioTrack = int.Parse(audioTrack, CultureInfo.InvariantCulture), OriginalFileName = originalFileName, VideoOffsetInMs = videoOffset, VideoIsSmpte = isSmpte });
            }

            // General
            node = doc.DocumentElement.SelectSingleNode("General");

            var useLegacyHtmlColorNode = node.SelectSingleNode("UseLegacyHtmlColor");
            if (useLegacyHtmlColorNode != null)
            {
                settings.General.UseLegacyHtmlColor = false;
                UseLegacyHtmlColor = settings.General.UseLegacyHtmlColor;
            }


            // Profiles
            int profileCount = 0;
            foreach (XmlNode listNode in node.SelectNodes("Profiles/Profile"))
            {
                if (profileCount == 0)
                {
                    settings.General.Profiles.Clear();
                }

                var subtitleLineMaximumLength = listNode.SelectSingleNode("SubtitleLineMaximumLength")?.InnerText;
                var subtitleMaximumCharactersPerSeconds = listNode.SelectSingleNode("SubtitleMaximumCharactersPerSeconds")?.InnerText;
                var subtitleOptimalCharactersPerSeconds = listNode.SelectSingleNode("SubtitleOptimalCharactersPerSeconds")?.InnerText;
                var subtitleMinimumDisplayMilliseconds = listNode.SelectSingleNode("SubtitleMinimumDisplayMilliseconds")?.InnerText;
                var subtitleMaximumDisplayMilliseconds = listNode.SelectSingleNode("SubtitleMaximumDisplayMilliseconds")?.InnerText;
                var subtitleMaximumWordsPerMinute = listNode.SelectSingleNode("SubtitleMaximumWordsPerMinute")?.InnerText;
                var cpsLineLengthStrategy = listNode.SelectSingleNode("CpsLineLengthStrategy")?.InnerText;
                var maxNumberOfLines = listNode.SelectSingleNode("MaxNumberOfLines")?.InnerText;
                var mergeLinesShorterThan = listNode.SelectSingleNode("MergeLinesShorterThan")?.InnerText;
                var minimumMillisecondsBetweenLines = listNode.SelectSingleNode("MinimumMillisecondsBetweenLines")?.InnerText;

                var dialogStyle = (DialogType)Enum.Parse(typeof(DialogType), listNode.SelectSingleNode("DialogStyle")?.InnerText);
                var continuationStyle = (ContinuationStyle)Enum.Parse(typeof(ContinuationStyle), listNode.SelectSingleNode("ContinuationStyle")?.InnerText);

                settings.General.Profiles.Add(new RulesProfile
                {
                    Name = listNode.SelectSingleNode("Name")?.InnerText,
                    SubtitleLineMaximumLength = Convert.ToInt32(subtitleLineMaximumLength, CultureInfo.InvariantCulture),
                    SubtitleMaximumCharactersPerSeconds = Convert.ToDecimal(subtitleMaximumCharactersPerSeconds, CultureInfo.InvariantCulture),
                    SubtitleOptimalCharactersPerSeconds = Convert.ToDecimal(subtitleOptimalCharactersPerSeconds, CultureInfo.InvariantCulture),
                    SubtitleMinimumDisplayMilliseconds = Convert.ToInt32(subtitleMinimumDisplayMilliseconds, CultureInfo.InvariantCulture),
                    SubtitleMaximumDisplayMilliseconds = Convert.ToInt32(subtitleMaximumDisplayMilliseconds, CultureInfo.InvariantCulture),
                    SubtitleMaximumWordsPerMinute = Convert.ToDecimal(subtitleMaximumWordsPerMinute, CultureInfo.InvariantCulture),
                    CpsLineLengthStrategy = cpsLineLengthStrategy,
                    MaxNumberOfLines = Convert.ToInt32(maxNumberOfLines, CultureInfo.InvariantCulture),
                    MergeLinesShorterThan = Convert.ToInt32(mergeLinesShorterThan, CultureInfo.InvariantCulture),
                    MinimumMillisecondsBetweenLines = Convert.ToInt32(minimumMillisecondsBetweenLines, CultureInfo.InvariantCulture),
                    DialogStyle = dialogStyle,
                    ContinuationStyle = continuationStyle
                });
                profileCount++;
            }


            XmlNode subNode = node.SelectSingleNode("CurrentProfile");
            if (subNode != null)
            {
                settings.General.CurrentProfile = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ShowToolbarNew");
            if (subNode != null)
            {
                settings.General.ShowToolbarNew = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarOpen");
            if (subNode != null)
            {
                settings.General.ShowToolbarOpen = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarOpenVideo");
            if (subNode != null)
            {
                settings.General.ShowToolbarOpenVideo = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarSave");
            if (subNode != null)
            {
                settings.General.ShowToolbarSave = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarSaveAs");
            if (subNode != null)
            {
                settings.General.ShowToolbarSaveAs = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarFind");
            if (subNode != null)
            {
                settings.General.ShowToolbarFind = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarReplace");
            if (subNode != null)
            {
                settings.General.ShowToolbarReplace = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarFixCommonErrors");
            if (subNode != null)
            {
                settings.General.ShowToolbarFixCommonErrors = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarRemoveTextForHi");
            if (subNode != null)
            {
                settings.General.ShowToolbarRemoveTextForHi = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarVisualSync");
            if (subNode != null)
            {
                settings.General.ShowToolbarVisualSync = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarToggleSourceView");
            if (subNode != null)
            {
                settings.General.ShowToolbarToggleSourceView = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarBurnIn");
            if (subNode != null)
            {
                settings.General.ShowToolbarBurnIn = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarSpellCheck");
            if (subNode != null)
            {
                settings.General.ShowToolbarSpellCheck = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarNetflixGlyphCheck");
            if (subNode != null)
            {
                settings.General.ShowToolbarNetflixGlyphCheck = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarBeautifyTimeCodes");
            if (subNode != null)
            {
                settings.General.ShowToolbarBeautifyTimeCodes = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarSettings");
            if (subNode != null)
            {
                settings.General.ShowToolbarSettings = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowToolbarHelp");
            if (subNode != null)
            {
                settings.General.ShowToolbarHelp = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowFrameRate");
            if (subNode != null)
            {
                settings.General.ShowFrameRate = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowVideoControls");
            if (subNode != null)
            {
                settings.General.ShowVideoControls = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("TextAndOrigianlTextBoxesSwitched");
            if (subNode != null)
            {
                settings.General.TextAndOrigianlTextBoxesSwitched = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LayoutNumber");
            if (subNode != null)
            {
                settings.General.LayoutNumber = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LayoutSizes");
            if (subNode != null)
            {
                settings.General.LayoutSizes = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ShowVideoPlayer");
            if (subNode != null)
            {
                settings.General.ShowVideoPlayer = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowAudioVisualizer");
            if (subNode != null)
            {
                settings.General.ShowAudioVisualizer = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowWaveform");
            if (subNode != null)
            {
                settings.General.ShowWaveform = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowSpectrogram");
            if (subNode != null)
            {
                settings.General.ShowSpectrogram = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("DefaultFrameRate");
            if (subNode != null)
            {
                settings.General.DefaultFrameRate = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
                if (settings.General.DefaultFrameRate > 23975)
                {
                    settings.General.DefaultFrameRate = 23.976;
                }

                settings.General.CurrentFrameRate = settings.General.DefaultFrameRate;
            }

            subNode = node.SelectSingleNode("DefaultSubtitleFormat");
            if (subNode != null)
            {
                settings.General.DefaultSubtitleFormat = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("DefaultSaveAsFormat");
            if (subNode != null)
            {
                settings.General.DefaultSaveAsFormat = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("FavoriteSubtitleFormats");
            if (subNode != null)
            {
                settings.General.FavoriteSubtitleFormats = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("DefaultEncoding");
            if (subNode != null)
            {
                settings.General.DefaultEncoding = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoConvertToUtf8");
            if (subNode != null)
            {
                settings.General.AutoConvertToUtf8 = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoGuessAnsiEncoding");
            if (subNode != null)
            {
                settings.General.AutoGuessAnsiEncoding = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("TranslationAutoSuffixes");
            if (subNode != null)
            {
                settings.General.TranslationAutoSuffixes = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TranslationAutoSuffixDefault");
            if (subNode != null)
            {
                settings.General.TranslationAutoSuffixDefault = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("SystemSubtitleFontNameOverride");
            if (subNode != null)
            {
                settings.General.SystemSubtitleFontNameOverride = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("SystemSubtitleFontSizeOverride");
            if (!string.IsNullOrEmpty(subNode?.InnerText))
            {
                settings.General.SystemSubtitleFontSizeOverride = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleFontName");
            if (subNode != null)
            {
                settings.General.SubtitleFontName = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("SubtitleTextBoxFontSize");
            if (subNode != null)
            {
                settings.General.SubtitleTextBoxFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleListViewFontSize");
            if (subNode != null)
            {
                settings.General.SubtitleListViewFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleTextBoxFontBold");
            if (subNode != null)
            {
                settings.General.SubtitleTextBoxFontBold = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleListViewFontBold");
            if (subNode != null)
            {
                settings.General.SubtitleListViewFontBold = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleTextBoxSyntaxColor");
            if (subNode != null)
            {
                settings.General.SubtitleTextBoxSyntaxColor = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleTextBoxHtmlColor");
            if (subNode != null)
            {
                settings.General.SubtitleTextBoxHtmlColor = Color.FromArgb(Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("SubtitleTextBoxAssColor");
            if (subNode != null)
            {
                settings.General.SubtitleTextBoxAssColor = Color.FromArgb(Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("SubtitleFontColor");
            if (subNode != null)
            {
                settings.General.SubtitleFontColor = Color.FromArgb(Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("SubtitleBackgroundColor");
            if (subNode != null)
            {
                settings.General.SubtitleBackgroundColor = Color.FromArgb(Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("MeasureFontName");
            if (subNode != null)
            {
                settings.General.MeasureFontName = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("MeasureFontSize");
            if (subNode != null)
            {
                settings.General.MeasureFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MeasureFontBold");
            if (subNode != null)
            {
                settings.General.MeasureFontBold = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleLineMaximumPixelWidth");
            if (subNode != null)
            {
                settings.General.SubtitleLineMaximumPixelWidth = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CenterSubtitleInTextBox");
            if (subNode != null)
            {
                settings.General.CenterSubtitleInTextBox = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowRecentFiles");
            if (subNode != null)
            {
                settings.General.ShowRecentFiles = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("RememberSelectedLine");
            if (subNode != null)
            {
                settings.General.RememberSelectedLine = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("StartLoadLastFile");
            if (subNode != null)
            {
                settings.General.StartLoadLastFile = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("StartRememberPositionAndSize");
            if (subNode != null)
            {
                settings.General.StartRememberPositionAndSize = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("StartPosition");
            if (subNode != null)
            {
                settings.General.StartPosition = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("StartSize");
            if (subNode != null)
            {
                settings.General.StartSize = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("StartInSourceView");
            if (subNode != null)
            {
                settings.General.StartInSourceView = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("RemoveBlankLinesWhenOpening");
            if (subNode != null)
            {
                settings.General.RemoveBlankLinesWhenOpening = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("RemoveBadCharsWhenOpening");
            if (subNode != null)
            {
                settings.General.RemoveBadCharsWhenOpening = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleLineMaximumLength");
            if (subNode != null)
            {
                settings.General.SubtitleLineMaximumLength = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MaxNumberOfLines");
            if (subNode != null)
            {
                settings.General.MaxNumberOfLines = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MaxNumberOfLinesPlusAbort");
            if (subNode != null)
            {
                settings.General.MaxNumberOfLinesPlusAbort = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MergeLinesShorterThan");
            if (subNode != null)
            {
                settings.General.MergeLinesShorterThan = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleMinimumDisplayMilliseconds");
            if (subNode != null)
            {
                settings.General.SubtitleMinimumDisplayMilliseconds = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleMaximumDisplayMilliseconds");
            if (subNode != null)
            {
                settings.General.SubtitleMaximumDisplayMilliseconds = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MinimumMillisecondsBetweenLines");
            if (subNode != null)
            {
                settings.General.MinimumMillisecondsBetweenLines = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SetStartEndHumanDelay");
            if (subNode != null)
            {
                settings.General.SetStartEndHumanDelay = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoWrapLineWhileTyping");
            if (subNode != null)
            {
                settings.General.AutoWrapLineWhileTyping = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleMaximumCharactersPerSeconds");
            if (subNode != null)
            {
                settings.General.SubtitleMaximumCharactersPerSeconds = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleOptimalCharactersPerSeconds");
            if (subNode != null)
            {
                settings.General.SubtitleOptimalCharactersPerSeconds = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CpsLineLengthStrategy");
            if (subNode != null)
            {
                settings.General.CpsLineLengthStrategy = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("SubtitleMaximumWordsPerMinute");
            if (subNode != null)
            {
                settings.General.SubtitleMaximumWordsPerMinute = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("DialogStyle");
            if (subNode != null)
            {
                settings.General.DialogStyle = (DialogType)Enum.Parse(typeof(DialogType), subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ContinuationStyle");
            if (subNode != null)
            {
                settings.General.ContinuationStyle = (ContinuationStyle)Enum.Parse(typeof(ContinuationStyle), subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ContinuationPause");
            if (subNode != null)
            {
                settings.General.ContinuationPause = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CustomContinuationStyleSuffix");
            if (subNode != null)
            {
                settings.General.CustomContinuationStyleSuffix = Convert.ToString(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CustomContinuationStyleSuffixApplyIfComma");
            if (subNode != null)
            {
                settings.General.CustomContinuationStyleSuffixApplyIfComma = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CustomContinuationStyleSuffixAddSpace");
            if (subNode != null)
            {
                settings.General.CustomContinuationStyleSuffixAddSpace = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CustomContinuationStyleSuffixReplaceComma");
            if (subNode != null)
            {
                settings.General.CustomContinuationStyleSuffixReplaceComma = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CustomContinuationStylePrefix");
            if (subNode != null)
            {
                settings.General.CustomContinuationStylePrefix = Convert.ToString(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CustomContinuationStylePrefixAddSpace");
            if (subNode != null)
            {
                settings.General.CustomContinuationStylePrefixAddSpace = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CustomContinuationStyleUseDifferentStyleGap");
            if (subNode != null)
            {
                settings.General.CustomContinuationStyleUseDifferentStyleGap = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CustomContinuationStyleGapSuffix");
            if (subNode != null)
            {
                settings.General.CustomContinuationStyleGapSuffix = Convert.ToString(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CustomContinuationStyleGapSuffixApplyIfComma");
            if (subNode != null)
            {
                settings.General.CustomContinuationStyleGapSuffixApplyIfComma = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CustomContinuationStyleGapSuffixAddSpace");
            if (subNode != null)
            {
                settings.General.CustomContinuationStyleGapSuffixAddSpace = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CustomContinuationStyleGapSuffixReplaceComma");
            if (subNode != null)
            {
                settings.General.CustomContinuationStyleGapSuffixReplaceComma = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CustomContinuationStyleGapPrefix");
            if (subNode != null)
            {
                settings.General.CustomContinuationStyleGapPrefix = Convert.ToString(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CustomContinuationStyleGapPrefixAddSpace");
            if (subNode != null)
            {
                settings.General.CustomContinuationStyleGapPrefixAddSpace = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixContinuationStyleUncheckInsertsAllCaps");
            if (subNode != null)
            {
                settings.General.FixContinuationStyleUncheckInsertsAllCaps = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixContinuationStyleUncheckInsertsItalic");
            if (subNode != null)
            {
                settings.General.FixContinuationStyleUncheckInsertsItalic = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixContinuationStyleUncheckInsertsLowercase");
            if (subNode != null)
            {
                settings.General.FixContinuationStyleUncheckInsertsLowercase = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixContinuationStyleHideContinuationCandidatesWithoutName");
            if (subNode != null)
            {
                settings.General.FixContinuationStyleHideContinuationCandidatesWithoutName = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixContinuationStyleIgnoreLyrics");
            if (subNode != null)
            {
                settings.General.FixContinuationStyleIgnoreLyrics = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SpellCheckLanguage");
            if (subNode != null)
            {
                settings.General.SpellCheckLanguage = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("VideoPlayer");
            if (subNode != null)
            {
                settings.General.VideoPlayer = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("VideoPlayerDefaultVolume");
            if (subNode != null)
            {
                settings.General.VideoPlayerDefaultVolume = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                if (settings.General.VideoPlayerDefaultVolume < 0)
                {
                    settings.General.VideoPlayerDefaultVolume = 75;
                }
            }

            subNode = node.SelectSingleNode("VideoPlayerPreviewFontName");
            if (subNode != null)
            {
                settings.General.VideoPlayerPreviewFontName = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("VideoPlayerPreviewFontSize");
            if (subNode != null)
            {
                settings.General.VideoPlayerPreviewFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("VideoPlayerPreviewFontBold");
            if (subNode != null)
            {
                settings.General.VideoPlayerPreviewFontBold = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("VideoPlayerShowStopButton");
            if (subNode != null)
            {
                settings.General.VideoPlayerShowStopButton = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("VideoPlayerShowMuteButton");
            if (subNode != null)
            {
                settings.General.VideoPlayerShowMuteButton = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("VideoPlayerShowFullscreenButton");
            if (subNode != null)
            {
                settings.General.VideoPlayerShowFullscreenButton = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("Language");
            if (subNode != null)
            {
                settings.General.Language = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ListViewLineSeparatorString");
            if (subNode != null)
            {
                settings.General.ListViewLineSeparatorString = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ListViewDoubleClickAction");
            if (subNode != null)
            {
                settings.General.ListViewDoubleClickAction = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SaveAsUseFileNameFrom");
            if (subNode != null)
            {
                settings.General.SaveAsUseFileNameFrom = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("UppercaseLetters");
            if (subNode != null)
            {
                settings.General.UppercaseLetters = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("DefaultAdjustMilliseconds");
            if (subNode != null)
            {
                settings.General.DefaultAdjustMilliseconds = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoRepeatOn");
            if (subNode != null)
            {
                settings.General.AutoRepeatOn = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoRepeatCount");
            if (subNode != null)
            {
                settings.General.AutoRepeatCount = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SyncListViewWithVideoWhilePlaying");
            if (subNode != null)
            {
                settings.General.SyncListViewWithVideoWhilePlaying = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoContinueOn");
            if (subNode != null)
            {
                settings.General.AutoContinueOn = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoContinueDelay");
            if (subNode != null)
            {
                settings.General.AutoContinueDelay = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ReturnToStartAfterRepeat");
            if (subNode != null)
            {
                settings.General.ReturnToStartAfterRepeat = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoBackupSeconds");
            if (subNode != null)
            {
                settings.General.AutoBackupSeconds = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoBackupDeleteAfterMonths");
            if (subNode != null)
            {
                settings.General.AutoBackupDeleteAfterMonths = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SpellChecker");
            if (subNode != null)
            {
                settings.General.SpellChecker = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AllowEditOfOriginalSubtitle");
            if (subNode != null)
            {
                settings.General.AllowEditOfOriginalSubtitle = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("PromptDeleteLines");
            if (subNode != null)
            {
                settings.General.PromptDeleteLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("Undocked");
            if (subNode != null)
            {
                settings.General.Undocked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("UndockedVideoPosition");
            if (subNode != null)
            {
                settings.General.UndockedVideoPosition = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("UndockedVideoFullscreen");
            if (subNode != null)
            {
                settings.General.UndockedVideoFullscreen = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("UndockedWaveformPosition");
            if (subNode != null)
            {
                settings.General.UndockedWaveformPosition = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("UndockedVideoControlsPosition");
            if (subNode != null)
            {
                settings.General.UndockedVideoControlsPosition = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WaveformCenter");
            if (subNode != null)
            {
                settings.General.WaveformCenter = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformAutoGenWhenOpeningVideo");
            if (subNode != null)
            {
                settings.General.WaveformAutoGenWhenOpeningVideo = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformUpdateIntervalMs");
            if (subNode != null)
            {
                settings.General.WaveformUpdateIntervalMs = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SmallDelayMilliseconds");
            if (subNode != null)
            {
                settings.General.SmallDelayMilliseconds = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LargeDelayMilliseconds");
            if (subNode != null)
            {
                settings.General.LargeDelayMilliseconds = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowOriginalAsPreviewIfAvailable");
            if (subNode != null)
            {
                settings.General.ShowOriginalAsPreviewIfAvailable = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LastPacCodePage");
            if (subNode != null)
            {
                settings.General.LastPacCodePage = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("OpenSubtitleExtraExtensions");
            if (subNode != null)
            {
                settings.General.OpenSubtitleExtraExtensions = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("ListViewColumnsRememberSize");
            if (subNode != null)
            {
                settings.General.ListViewColumnsRememberSize = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("ListViewNumberWidth");
            if (subNode != null)
            {
                settings.General.ListViewNumberWidth = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewStartWidth");
            if (subNode != null)
            {
                settings.General.ListViewStartWidth = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewEndWidth");
            if (subNode != null)
            {
                settings.General.ListViewEndWidth = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewDurationWidth");
            if (subNode != null)
            {
                settings.General.ListViewDurationWidth = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewCpsWidth");
            if (subNode != null)
            {
                settings.General.ListViewCpsWidth = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewWpmWidth");
            if (subNode != null)
            {
                settings.General.ListViewWpmWidth = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewGapWidth");
            if (subNode != null)
            {
                settings.General.ListViewGapWidth = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewActorWidth");
            if (subNode != null)
            {
                settings.General.ListViewActorWidth = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewRegionWidth");
            if (subNode != null)
            {
                settings.General.ListViewRegionWidth = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewTextWidth");
            if (subNode != null)
            {
                settings.General.ListViewTextWidth = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewNumberDisplayIndex");
            if (subNode != null)
            {
                settings.General.ListViewNumberDisplayIndex = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewStartDisplayIndex");
            if (subNode != null)
            {
                settings.General.ListViewStartDisplayIndex = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewEndDisplayIndex");
            if (subNode != null)
            {
                settings.General.ListViewEndDisplayIndex = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewDurationDisplayIndex");
            if (subNode != null)
            {
                settings.General.ListViewDurationDisplayIndex = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewCpsDisplayIndex");
            if (subNode != null)
            {
                settings.General.ListViewCpsDisplayIndex = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewWpmDisplayIndex");
            if (subNode != null)
            {
                settings.General.ListViewWpmDisplayIndex = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewGapDisplayIndex");
            if (subNode != null)
            {
                settings.General.ListViewGapDisplayIndex = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewActorDisplayIndex");
            if (subNode != null)
            {
                settings.General.ListViewActorDisplayIndex = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewRegionDisplayIndex");
            if (subNode != null)
            {
                settings.General.ListViewRegionDisplayIndex = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewTextDisplayIndex");
            if (subNode != null)
            {
                settings.General.ListViewTextDisplayIndex = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("DirectShowDoubleLoad");
            if (subNode != null)
            {
                settings.General.DirectShowDoubleLoad = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("VlcWaveTranscodeSettings");
            if (subNode != null)
            {
                settings.General.VlcWaveTranscodeSettings = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("VlcLocation");
            if (subNode != null)
            {
                settings.General.VlcLocation = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("VlcLocationRelative");
            if (subNode != null)
            {
                settings.General.VlcLocationRelative = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("MpvVideoOutputWindows");
            if (subNode != null)
            {
                settings.General.MpvVideoOutputWindows = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("MpvVideoOutputLinux");
            if (subNode != null)
            {
                settings.General.MpvVideoOutputLinux = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("MpvVideoVf");
            if (subNode != null)
            {
                settings.General.MpvVideoVf = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("MpvVideoAf");
            if (subNode != null)
            {
                settings.General.MpvVideoAf = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("MpvExtraOptions");
            if (subNode != null)
            {
                settings.General.MpvExtraOptions = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("MpvLogging");
            if (subNode != null)
            {
                settings.General.MpvLogging = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("MpvHandlesPreviewText");
            if (subNode != null)
            {
                settings.General.MpvHandlesPreviewText = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("MpvPreviewTextPrimaryColor");
            if (subNode != null)
            {
                settings.General.MpvPreviewTextPrimaryColor = FromHtml(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("MpvPreviewTextOutlineColor");
            if (subNode != null)
            {
                settings.General.MpvPreviewTextOutlineColor = FromHtml(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("MpvPreviewTextBackgroundColor");
            if (subNode != null)
            {
                settings.General.MpvPreviewTextBackgroundColor = FromHtml(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("MpvPreviewTextOutlineWidth");
            if (subNode != null)
            {
                settings.General.MpvPreviewTextOutlineWidth = Convert.ToDecimal(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MpvPreviewTextShadowWidth");
            if (subNode != null)
            {
                settings.General.MpvPreviewTextShadowWidth = Convert.ToDecimal(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MpvPreviewTextOpaqueBox");
            if (subNode != null)
            {
                settings.General.MpvPreviewTextOpaqueBox = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("MpvPreviewTextOpaqueBoxStyle");
            if (subNode != null)
            {
                settings.General.MpvPreviewTextOpaqueBoxStyle = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("MpvPreviewTextAlignment");
            if (subNode != null)
            {
                settings.General.MpvPreviewTextAlignment = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("MpvPreviewTextMarginVertical");
            if (subNode != null)
            {
                settings.General.MpvPreviewTextMarginVertical = Convert.ToInt32(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("MpcHcLocation");
            if (subNode != null)
            {
                settings.General.MpcHcLocation = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("MkvMergeLocation");
            if (subNode != null)
            {
                settings.General.MkvMergeLocation = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("UseFFmpegForWaveExtraction");
            if (subNode != null)
            {
                settings.General.UseFFmpegForWaveExtraction = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("FFmpegUseCenterChannelOnly");
            if (subNode != null)
            {
                settings.General.FFmpegUseCenterChannelOnly = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("FFmpegLocation");
            if (subNode != null)
            {
                settings.General.FFmpegLocation = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("FFmpegSceneThreshold");
            if (subNode != null)
            {
                settings.General.FFmpegSceneThreshold = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("UseTimeFormatHHMMSSFF");
            if (subNode != null)
            {
                settings.General.UseTimeFormatHHMMSSFF = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("SplitBehavior");
            if (subNode != null)
            {
                settings.General.SplitBehavior = Convert.ToInt32(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("SplitRemovesDashes");
            if (subNode != null)
            {
                settings.General.SplitRemovesDashes = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("ClearStatusBarAfterSeconds");
            if (subNode != null)
            {
                settings.General.ClearStatusBarAfterSeconds = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("Company");
            if (subNode != null)
            {
                settings.General.Company = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("DisableVideoAutoLoading");
            if (subNode != null)
            {
                settings.General.DisableVideoAutoLoading = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("DisableShowingLoadErrors");
            if (subNode != null)
            {
                settings.General.DisableShowingLoadErrors = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("AllowVolumeBoost");
            if (subNode != null)
            {
                settings.General.AllowVolumeBoost = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("NewEmptyUseAutoDuration");
            if (subNode != null)
            {
                settings.General.NewEmptyUseAutoDuration = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("RightToLeftMode");
            if (subNode != null)
            {
                settings.General.RightToLeftMode = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("LastSaveAsFormat");
            if (subNode != null)
            {
                settings.General.LastSaveAsFormat = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("CheckForUpdates");
            if (subNode != null)
            {
                settings.General.CheckForUpdates = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("LastCheckForUpdates");
            if (subNode != null)
            {
                settings.General.LastCheckForUpdates = Convert.ToDateTime(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("AutoSave");
            if (subNode != null)
            {
                settings.General.AutoSave = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("PreviewAssaText");
            if (subNode != null)
            {
                settings.General.PreviewAssaText = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TagsInToggleHiTags");
            if (subNode != null)
            {
                settings.General.TagsInToggleHiTags = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TagsInToggleCustomTags");
            if (subNode != null)
            {
                settings.General.TagsInToggleCustomTags = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ShowProgress");
            if (subNode != null)
            {
                settings.General.ShowProgress = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowNegativeDurationInfoOnSave");
            if (subNode != null)
            {
                settings.General.ShowNegativeDurationInfoOnSave = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowFormatRequiresUtf8Warning");
            if (subNode != null)
            {
                settings.General.ShowFormatRequiresUtf8Warning = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("DefaultVideoOffsetInMs");
            if (subNode != null)
            {
                settings.General.DefaultVideoOffsetInMs = Convert.ToInt64(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("DefaultVideoOffsetInMsList");
            if (subNode != null)
            {
                settings.General.DefaultVideoOffsetInMsList = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoSetVideoSmpteForTtml");
            if (subNode != null)
            {
                settings.General.AutoSetVideoSmpteForTtml = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoSetVideoSmpteForTtmlPrompt");
            if (subNode != null)
            {
                settings.General.AutoSetVideoSmpteForTtmlPrompt = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("TitleBarAsterisk");
            if (subNode != null)
            {
                settings.General.TitleBarAsterisk = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("TitleBarFullFileName");
            if (subNode != null)
            {
                settings.General.TitleBarFullFileName = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MeasurementConverterCloseOnInsert");
            if (subNode != null)
            {
                settings.General.MeasurementConverterCloseOnInsert = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MeasurementConverterCategories");
            if (subNode != null)
            {
                settings.General.MeasurementConverterCategories = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("SubtitleTextBoxAutoVerticalScrollBars");
            if (subNode != null)
            {
                settings.General.SubtitleTextBoxAutoVerticalScrollBars = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleTextBoxMaxHeight");
            if (subNode != null)
            {
                settings.General.SubtitleTextBoxMaxHeight = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AllowLetterShortcutsInTextBox");
            if (subNode != null)
            {
                settings.General.AllowLetterShortcutsInTextBox = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LastColorPickerColor");
            if (subNode != null)
            {
                settings.General.LastColorPickerColor = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("LastColorPickerColor1");
            if (subNode != null)
            {
                settings.General.LastColorPickerColor1 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("LastColorPickerColor2");
            if (subNode != null)
            {
                settings.General.LastColorPickerColor2 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("LastColorPickerColor3");
            if (subNode != null)
            {
                settings.General.LastColorPickerColor3 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("LastColorPickerColor4");
            if (subNode != null)
            {
                settings.General.LastColorPickerColor4 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("LastColorPickerColor5");
            if (subNode != null)
            {
                settings.General.LastColorPickerColor5 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("LastColorPickerColor6");
            if (subNode != null)
            {
                settings.General.LastColorPickerColor6 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("LastColorPickerColor7");
            if (subNode != null)
            {
                settings.General.LastColorPickerColor7 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("DarkThemeBackColor");
            if (subNode != null)
            {
                var x = subNode.InnerText;
                if (x == "-14803426")
                {
                    settings.General.DarkThemeBackColor = Color.FromArgb(-14803426); //TODO: remove at some point
                }
                else
                {
                    settings.General.DarkThemeBackColor = FromHtml(subNode.InnerText);
                }
            }

            subNode = node.SelectSingleNode("DarkThemeSelectedBackgroundColor");
            if (subNode != null)
            {
                var x = subNode.InnerText;
                if (x == "")
                {
                    settings.General.DarkThemeSelectedBackgroundColor = Color.FromArgb(1); //TODO: remove at some point
                }
                else
                {
                    settings.General.DarkThemeSelectedBackgroundColor = FromHtml(subNode.InnerText);
                }

            }

            subNode = node.SelectSingleNode("DarkThemeForeColor");
            if (subNode != null)
            {
                var x = subNode.InnerText;
                if (x == "-6579301")
                {
                    settings.General.DarkThemeForeColor = Color.FromArgb(-6579301); //TODO: remove at some point
                }
                else
                {
                    settings.General.DarkThemeForeColor = FromHtml(subNode.InnerText);
                }
            }

            subNode = node.SelectSingleNode("DarkThemeDisabledColor");
            if (subNode != null)
            {
                settings.General.DarkThemeDisabledColor = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("UseDarkTheme");
            if (subNode != null)
            {
                settings.General.UseDarkTheme = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ToolbarIconTheme");
            if (subNode != null)
            {
                settings.General.ToolbarIconTheme = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("DarkThemeShowListViewGridLines");
            if (subNode != null)
            {
                settings.General.DarkThemeShowListViewGridLines = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ShowBetaStuff");
            if (subNode != null)
            {
                settings.General.ShowBetaStuff = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("DebugTranslationSync");
            if (subNode != null)
            {
                settings.General.DebugTranslationSync = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("UseLegacyDownloader");
            if (subNode != null)
            {
                settings.General.UseLegacyDownloader = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("DefaultLanguages");
            if (subNode != null)
            {
                settings.General.DefaultLanguages = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("NewEmptyDefaultMs");
            if (subNode != null)
            {
                settings.General.NewEmptyDefaultMs = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MoveVideo100Or500MsPlaySmallSample");
            if (subNode != null)
            {
                settings.General.MoveVideo100Or500MsPlaySmallSample = Convert.ToBoolean(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            // Tools
            node = doc.DocumentElement.SelectSingleNode("Tools");

            // ASSA templates (by user)
            int assaTagTemplateCount = 0;
            foreach (XmlNode listNode in node.SelectNodes("AssTagTemplates/Template"))
            {
                if (assaTagTemplateCount == 0)
                {
                    settings.Tools.AssaTagTemplates = new List<AssaTemplateItem>();
                }

                var template = new AssaTemplateItem();
                template.Tag = listNode.SelectSingleNode("Tag")?.InnerText;
                template.Hint = listNode.SelectSingleNode("Hint")?.InnerText;
                if (!string.IsNullOrEmpty(template.Tag))
                {
                    settings.Tools.AssaTagTemplates.Add(template);
                }

                assaTagTemplateCount++;
            }

            subNode = node.SelectSingleNode("StartSceneIndex");
            if (subNode != null)
            {
                settings.Tools.StartSceneIndex = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("EndSceneIndex");
            if (subNode != null)
            {
                settings.Tools.EndSceneIndex = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("VerifyPlaySeconds");
            if (subNode != null)
            {
                settings.Tools.VerifyPlaySeconds = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixShortDisplayTimesAllowMoveStartTime");
            if (subNode != null)
            {
                settings.Tools.FixShortDisplayTimesAllowMoveStartTime = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("RemoveEmptyLinesBetweenText");
            if (subNode != null)
            {
                settings.Tools.RemoveEmptyLinesBetweenText = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MusicSymbol");
            if (subNode != null)
            {
                settings.Tools.MusicSymbol = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("MusicSymbolReplace");
            if (subNode != null)
            {
                settings.Tools.MusicSymbolReplace = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("UnicodeSymbolsToInsert");
            if (subNode != null)
            {
                settings.Tools.UnicodeSymbolsToInsert = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("SpellCheckAutoChangeNameCasing");
            if (subNode != null)
            {
                settings.Tools.SpellCheckAutoChangeNameCasing = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SpellCheckUseLargerFont");
            if (subNode != null)
            {
                settings.Tools.SpellCheckUseLargerFont = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SpellCheckAutoChangeNamesUseSuggestions");
            if (subNode != null)
            {
                settings.Tools.SpellCheckAutoChangeNamesUseSuggestions = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SpellCheckSearchEngine");
            if (subNode != null)
            {
                settings.Tools.SpellCheckSearchEngine = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("SpellCheckOneLetterWords");
            if (subNode != null)
            {
                settings.Tools.CheckOneLetterWords = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SpellCheckEnglishAllowInQuoteAsIng");
            if (subNode != null)
            {
                settings.Tools.SpellCheckEnglishAllowInQuoteAsIng = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("RememberUseAlwaysList");
            if (subNode != null)
            {
                settings.Tools.RememberUseAlwaysList = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LiveSpellCheck");
            if (subNode != null)
            {
                settings.Tools.LiveSpellCheck = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SpellCheckShowCompletedMessage");
            if (subNode != null)
            {
                settings.Tools.SpellCheckShowCompletedMessage = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("OcrFixUseHardcodedRules");
            if (subNode != null)
            {
                settings.Tools.OcrFixUseHardcodedRules = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("OcrGoogleCloudVisionSeHandlesTextMerge");
            if (subNode != null)
            {
                settings.Tools.OcrGoogleCloudVisionSeHandlesTextMerge = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("OcrBinaryImageCompareRgbThreshold");
            if (subNode != null)
            {
                settings.Tools.OcrBinaryImageCompareRgbThreshold = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("OcrTesseract4RgbThreshold");
            if (subNode != null)
            {
                settings.Tools.OcrTesseract4RgbThreshold = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("OcrAddLetterRow1");
            if (subNode != null)
            {
                settings.Tools.OcrAddLetterRow1 = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("OcrAddLetterRow2");
            if (subNode != null)
            {
                settings.Tools.OcrAddLetterRow2 = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("OcrTrainFonts");
            if (subNode != null)
            {
                settings.Tools.OcrTrainFonts = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("OcrTrainMergedLetters");
            if (subNode != null)
            {
                settings.Tools.OcrTrainMergedLetters = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("OcrTrainSrtFile");
            if (subNode != null)
            {
                settings.Tools.OcrTrainSrtFile = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("OcrUseWordSplitList");
            if (subNode != null)
            {
                settings.Tools.OcrUseWordSplitList = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("OcrUseWordSplitListAvoidPropercase");
            if (subNode != null)
            {
                settings.Tools.OcrUseWordSplitListAvoidPropercase = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BDOpenIn");
            if (subNode != null)
            {
                settings.Tools.BDOpenIn = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("MicrosoftBingApiId");
            if (subNode != null)
            {
                settings.Tools.MicrosoftBingApiId = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("MicrosoftTranslatorApiKey");
            if (subNode != null)
            {
                settings.Tools.MicrosoftTranslatorApiKey = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("MicrosoftTranslatorTokenEndpoint");
            if (subNode != null)
            {
                settings.Tools.MicrosoftTranslatorTokenEndpoint = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("MicrosoftTranslatorCategory");
            if (subNode != null)
            {
                settings.Tools.MicrosoftTranslatorCategory = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GoogleApiV2Key");
            if (subNode != null)
            {
                settings.Tools.GoogleApiV2Key = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GoogleTranslateNoKeyWarningShow");
            if (subNode != null)
            {
                settings.Tools.GoogleTranslateNoKeyWarningShow = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GoogleApiV1ChunkSize");
            if (subNode != null)
            {
                settings.Tools.GoogleApiV1ChunkSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GoogleTranslateLastSourceLanguage");
            if (subNode != null)
            {
                settings.Tools.GoogleTranslateLastSourceLanguage = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GoogleTranslateLastTargetLanguage");
            if (subNode != null)
            {
                settings.Tools.GoogleTranslateLastTargetLanguage = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslateLastName");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateLastName = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslateLastUrl");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateLastUrl = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslateNllbApiUrl");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateNllbApiUrl = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslateNllbServeUrl");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateNllbServeUrl = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslateNllbServeModel");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateNllbServeModel = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslateLibreUrl");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateLibreUrl = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslateLibreApiKey");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateLibreApiKey = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslateMyMemoryApiKey");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateMyMemoryApiKey = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslateSeamlessM4TUrl");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateSeamlessM4TUrl = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslateDeepLApiKey");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateDeepLApiKey = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslateDeepLUrl");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateDeepLUrl = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslatePapagoApiKeyId");
            if (subNode != null)
            {
                settings.Tools.AutoTranslatePapagoApiKeyId = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslatePapagoApiKey");
            if (subNode != null)
            {
                settings.Tools.AutoTranslatePapagoApiKey = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslateDeepLFormality");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateDeepLFormality = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TranslateAllowSplit");
            if (subNode != null)
            {
                settings.Tools.TranslateAllowSplit = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("TranslateLastService");
            if (subNode != null)
            {
                settings.Tools.TranslateLastService = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TranslateMergeStrategy");
            if (subNode != null)
            {
                settings.Tools.TranslateMergeStrategy = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TranslateViaCopyPasteSeparator");
            if (subNode != null)
            {
                settings.Tools.TranslateViaCopyPasteSeparator = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TranslateViaCopyPasteMaxSize");
            if (subNode != null)
            {
                settings.Tools.TranslateViaCopyPasteMaxSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ChatGptUrl");
            if (subNode != null)
            {
                settings.Tools.ChatGptUrl = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ChatGptPrompt");
            if (subNode != null)
            {
                settings.Tools.ChatGptPrompt = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ChatGptApiKey");
            if (subNode != null)
            {
                settings.Tools.ChatGptApiKey = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ChatGptModel");
            if (subNode != null)
            {
                settings.Tools.ChatGptModel = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("LmStudioApiUrl");
            if (subNode != null)
            {
                settings.Tools.LmStudioApiUrl = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("LmStudioModel");
            if (subNode != null)
            {
                settings.Tools.LmStudioModel = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("LmStudioPrompt");
            if (subNode != null)
            {
                settings.Tools.LmStudioPrompt = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("OllamaApiUrl");
            if (subNode != null)
            {
                settings.Tools.OllamaApiUrl = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("OllamaModels");
            if (subNode != null)
            {
                settings.Tools.OllamaModels = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("OllamaModel");
            if (subNode != null)
            {
                settings.Tools.OllamaModel = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("OllamaPrompt");
            if (subNode != null)
            {
                settings.Tools.OllamaPrompt = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AnthropicApiUrl");
            if (subNode != null)
            {
                settings.Tools.AnthropicApiUrl = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AnthropicPrompt");
            if (subNode != null)
            {
                settings.Tools.AnthropicPrompt = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AnthropicApiKey");
            if (subNode != null)
            {
                settings.Tools.AnthropicApiKey = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AnthropicApiModel");
            if (subNode != null)
            {
                settings.Tools.AnthropicApiModel = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoTranslateDelaySeconds");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateDelaySeconds = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoTranslateMaxBytes");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateMaxBytes = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoTranslateStrategy");
            if (subNode != null)
            {
                settings.Tools.AutoTranslateStrategy = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GeminiProApiKey");
            if (subNode != null)
            {
                settings.Tools.GeminiProApiKey = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TextToSpeechEngine");
            if (subNode != null)
            {
                settings.Tools.TextToSpeechEngine = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TextToSpeechLastVoice");
            if (subNode != null)
            {
                settings.Tools.TextToSpeechLastVoice = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TextToSpeechElevenLabsApiKey");
            if (subNode != null)
            {
                settings.Tools.TextToSpeechElevenLabsApiKey = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TextToSpeechAzureApiKey");
            if (subNode != null)
            {
                settings.Tools.TextToSpeechAzureApiKey = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TextToSpeechAzureRegion");
            if (subNode != null)
            {
                settings.Tools.TextToSpeechAzureRegion = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TranslateViaCopyPasteAutoCopyToClipboard");
            if (subNode != null)
            {
                settings.Tools.TranslateViaCopyPasteAutoCopyToClipboard = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("TextToSpeechPreview");
            if (subNode != null)
            {
                settings.Tools.TextToSpeechPreview = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("TextToSpeechCustomAudio");
            if (subNode != null)
            {
                settings.Tools.TextToSpeechCustomAudio = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("TextToSpeechCustomAudioStereo");
            if (subNode != null)
            {
                settings.Tools.TextToSpeechCustomAudioStereo = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("TextToSpeechCustomAudioEncoding");
            if (subNode != null)
            {
                settings.Tools.TextToSpeechCustomAudioEncoding = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TextToSpeechAddToVideoFile");
            if (subNode != null)
            {
                settings.Tools.TextToSpeechAddToVideoFile = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewSyntaxColorDurationSmall");
            if (subNode != null)
            {
                settings.Tools.ListViewSyntaxColorDurationSmall = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewSyntaxColorDurationBig");
            if (subNode != null)
            {
                settings.Tools.ListViewSyntaxColorDurationBig = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewSyntaxColorLongLines");
            if (subNode != null)
            {
                settings.Tools.ListViewSyntaxColorLongLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewSyntaxColorWideLines");
            if (subNode != null)
            {
                settings.Tools.ListViewSyntaxColorWideLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewSyntaxMoreThanXLines");
            if (subNode != null)
            {
                settings.Tools.ListViewSyntaxMoreThanXLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewSyntaxColorOverlap");
            if (subNode != null)
            {
                settings.Tools.ListViewSyntaxColorOverlap = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewSyntaxColorGap");
            if (subNode != null)
            {
                settings.Tools.ListViewSyntaxColorGap = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewSyntaxErrorColor");
            if (subNode != null)
            {
                settings.Tools.ListViewSyntaxErrorColor = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("ListViewUnfocusedSelectedColor");
            if (subNode != null)
            {
                settings.Tools.ListViewUnfocusedSelectedColor = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("Color1");
            if (subNode != null)
            {
                settings.Tools.Color1 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("Color2");
            if (subNode != null)
            {
                settings.Tools.Color2 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("Color3");
            if (subNode != null)
            {
                settings.Tools.Color3 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("Color4");
            if (subNode != null)
            {
                settings.Tools.Color4 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("Color5");
            if (subNode != null)
            {
                settings.Tools.Color5 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("Color6");
            if (subNode != null)
            {
                settings.Tools.Color6 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("Color7");
            if (subNode != null)
            {
                settings.Tools.Color7 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("Color8");
            if (subNode != null)
            {
                settings.Tools.Color8 = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ListViewShowColumnStartTime");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnStartTime = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewShowColumnEndTime");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnEndTime = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewShowColumnDuration");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnDuration = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewShowColumnCharsPerSec");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnCharsPerSec = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewShowColumnWordsPerMin");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnWordsPerMin = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewShowColumnGap");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnGap = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewShowColumnActor");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnActor = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewShowColumnRegion");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnRegion = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ListViewMultipleReplaceShowColumnRuleInfo");
            if (subNode != null)
            {
                settings.Tools.ListViewMultipleReplaceShowColumnRuleInfo = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SplitAdvanced");
            if (subNode != null)
            {
                settings.Tools.SplitAdvanced = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SplitOutputFolder");
            if (subNode != null)
            {
                settings.Tools.SplitOutputFolder = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("SplitNumberOfParts");
            if (subNode != null)
            {
                settings.Tools.SplitNumberOfParts = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SplitVia");
            if (subNode != null)
            {
                settings.Tools.SplitVia = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("JoinCorrectTimeCodes");
            if (subNode != null)
            {
                settings.Tools.JoinCorrectTimeCodes = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("JoinAddMs");
            if (subNode != null)
            {
                settings.Tools.JoinAddMs = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SplitLongLinesMax");
            if (subNode != null)
            {
                settings.Tools.SplitLongLinesMax = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("NewEmptyTranslationText");
            if (subNode != null)
            {
                settings.Tools.NewEmptyTranslationText = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BatchConvertOutputFolder");
            if (subNode != null)
            {
                settings.Tools.BatchConvertOutputFolder = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BatchConvertOverwriteExisting");
            if (subNode != null)
            {
                settings.Tools.BatchConvertOverwriteExisting = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertSaveInSourceFolder");
            if (subNode != null)
            {
                settings.Tools.BatchConvertSaveInSourceFolder = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertRemoveFormatting");
            if (subNode != null)
            {
                settings.Tools.BatchConvertRemoveFormatting = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertRemoveFormattingAll");
            if (subNode != null)
            {
                settings.Tools.BatchConvertRemoveFormattingAll = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertRemoveFormattingItalic");
            if (subNode != null)
            {
                settings.Tools.BatchConvertRemoveFormattingItalic = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertRemoveFormattingBold");
            if (subNode != null)
            {
                settings.Tools.BatchConvertRemoveFormattingBold = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertRemoveFormattingUnderline");
            if (subNode != null)
            {
                settings.Tools.BatchConvertRemoveFormattingUnderline = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertRemoveFormattingFontName");
            if (subNode != null)
            {
                settings.Tools.BatchConvertRemoveFormattingFontName = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertRemoveFormattingColor");
            if (subNode != null)
            {
                settings.Tools.BatchConvertRemoveFormattingColor = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertRemoveFormattingAlignment");
            if (subNode != null)
            {
                settings.Tools.BatchConvertRemoveFormattingAlignment = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertRemoveStyle");
            if (subNode != null)
            {
                settings.Tools.BatchConvertRemoveStyle = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertBridgeGaps");
            if (subNode != null)
            {
                settings.Tools.BatchConvertBridgeGaps = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertFixCasing");
            if (subNode != null)
            {
                settings.Tools.BatchConvertFixCasing = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertRemoveTextForHI");
            if (subNode != null)
            {
                settings.Tools.BatchConvertRemoveTextForHI = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertConvertColorsToDialog");
            if (subNode != null)
            {
                settings.Tools.BatchConvertConvertColorsToDialog = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertBeautifyTimeCodes");
            if (subNode != null)
            {
                settings.Tools.BatchConvertBeautifyTimeCodes = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertAutoTranslate");
            if (subNode != null)
            {
                settings.Tools.BatchConvertAutoTranslate = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertFixCommonErrors");
            if (subNode != null)
            {
                settings.Tools.BatchConvertFixCommonErrors = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertMultipleReplace");
            if (subNode != null)
            {
                settings.Tools.BatchConvertMultipleReplace = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertFixRtl");
            if (subNode != null)
            {
                settings.Tools.BatchConvertFixRtl = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertFixRtlMode");
            if (subNode != null)
            {
                settings.Tools.BatchConvertFixRtlMode = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BatchConvertAutoBalance");
            if (subNode != null)
            {
                settings.Tools.BatchConvertAutoBalance = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertSplitLongLines");
            if (subNode != null)
            {
                settings.Tools.BatchConvertSplitLongLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertSetMinDisplayTimeBetweenSubtitles");
            if (subNode != null)
            {
                settings.Tools.BatchConvertSetMinDisplayTimeBetweenSubtitles = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertMergeShortLines");
            if (subNode != null)
            {
                settings.Tools.BatchConvertMergeShortLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertRemoveLineBreaks");
            if (subNode != null)
            {
                settings.Tools.BatchConvertRemoveLineBreaks = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertMergeSameText");
            if (subNode != null)
            {
                settings.Tools.BatchConvertMergeSameText = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertMergeSameTimeCodes");
            if (subNode != null)
            {
                settings.Tools.BatchConvertMergeSameTimeCodes = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertChangeSpeed");
            if (subNode != null)
            {
                settings.Tools.BatchConvertChangeSpeed = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertAdjustDisplayDuration");
            if (subNode != null)
            {
                settings.Tools.BatchConvertAdjustDisplayDuration = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertApplyDurationLimits");
            if (subNode != null)
            {
                settings.Tools.BatchConvertApplyDurationLimits = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertDeleteLines");
            if (subNode != null)
            {
                settings.Tools.BatchConvertDeleteLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertAssaChangeRes");
            if (subNode != null)
            {
                settings.Tools.BatchConvertAssaChangeRes = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertSortBy");
            if (subNode != null)
            {
                settings.Tools.BatchConvertSortBy = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertSortByChoice");
            if (subNode != null)
            {
                settings.Tools.BatchConvertSortByChoice = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BatchConvertChangeFrameRate");
            if (subNode != null)
            {
                settings.Tools.BatchConvertChangeFrameRate = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertOffsetTimeCodes");
            if (subNode != null)
            {
                settings.Tools.BatchConvertOffsetTimeCodes = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertScanFolderIncludeVideo");
            if (subNode != null)
            {
                settings.Tools.BatchConvertScanFolderIncludeVideo = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertLanguage");
            if (subNode != null)
            {
                settings.Tools.BatchConvertLanguage = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BatchConvertFormat");
            if (subNode != null)
            {
                settings.Tools.BatchConvertFormat = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BatchConvertAssStyles");
            if (subNode != null)
            {
                settings.Tools.BatchConvertAssStyles = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BatchConvertSsaStyles");
            if (subNode != null)
            {
                settings.Tools.BatchConvertSsaStyles = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BatchConvertUseStyleFromSource");
            if (subNode != null)
            {
                settings.Tools.BatchConvertUseStyleFromSource = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertExportCustomTextTemplate");
            if (subNode != null)
            {
                settings.Tools.BatchConvertExportCustomTextTemplate = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BatchConvertTsOverrideXPosition");
            if (subNode != null)
            {
                settings.Tools.BatchConvertTsOverrideXPosition = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertTsOverrideYPosition");
            if (subNode != null)
            {
                settings.Tools.BatchConvertTsOverrideYPosition = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertTsOverrideBottomMargin");
            if (subNode != null)
            {
                settings.Tools.BatchConvertTsOverrideBottomMargin = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertTsOverrideHAlign");
            if (subNode != null)
            {
                settings.Tools.BatchConvertTsOverrideHAlign = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BatchConvertTsOverrideHMargin");
            if (subNode != null)
            {
                settings.Tools.BatchConvertTsOverrideHMargin = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertTsOverrideScreenSize");
            if (subNode != null)
            {
                settings.Tools.BatchConvertTsOverrideScreenSize = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertTsScreenWidth");
            if (subNode != null)
            {
                settings.Tools.BatchConvertTsScreenWidth = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertTsScreenHeight");
            if (subNode != null)
            {
                settings.Tools.BatchConvertTsScreenHeight = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertTsOnlyTeletext");
            if (subNode != null)
            {
                settings.Tools.BatchConvertTsOnlyTeletext = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BatchConvertTsFileNameAppend");
            if (subNode != null)
            {
                settings.Tools.BatchConvertTsFileNameAppend = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BatchConvertMkvLanguageCodeStyle");
            if (subNode != null)
            {
                settings.Tools.BatchConvertMkvLanguageCodeStyle = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BatchConvertOcrEngine");
            if (subNode != null)
            {
                settings.Tools.BatchConvertOcrEngine = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BatchConvertOcrLanguage");
            if (subNode != null)
            {
                settings.Tools.BatchConvertOcrLanguage = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WaveformBatchLastFolder");
            if (subNode != null)
            {
                settings.Tools.WaveformBatchLastFolder = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ModifySelectionRule");
            if (subNode != null)
            {
                settings.Tools.ModifySelectionRule = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ModifySelectionText");
            if (subNode != null)
            {
                settings.Tools.ModifySelectionText = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ModifySelectionCaseSensitive");
            if (subNode != null)
            {
                settings.Tools.ModifySelectionCaseSensitive = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportVobSubFontName");
            if (subNode != null)
            {
                settings.Tools.ExportVobSubFontName = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportVobSubFontSize");
            if (subNode != null)
            {
                settings.Tools.ExportVobSubFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportVobSubVideoResolution");
            if (subNode != null)
            {
                settings.Tools.ExportVobSubVideoResolution = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportVobSubSimpleRendering");
            if (subNode != null)
            {
                settings.Tools.ExportVobSubSimpleRendering = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportVobAntiAliasingWithTransparency");
            if (subNode != null)
            {
                settings.Tools.ExportVobAntiAliasingWithTransparency = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportVobSubLanguage");
            if (subNode != null)
            {
                settings.Tools.ExportVobSubLanguage = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportBluRayFontName");
            if (subNode != null)
            {
                settings.Tools.ExportBluRayFontName = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportBluRayFontSize");
            if (subNode != null)
            {
                settings.Tools.ExportBluRayFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportFcpFontName");
            if (subNode != null)
            {
                settings.Tools.ExportFcpFontName = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportFontNameOther");
            if (subNode != null)
            {
                settings.Tools.ExportFontNameOther = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportFcpFontSize");
            if (subNode != null)
            {
                settings.Tools.ExportFcpFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportFcpImageType");
            if (subNode != null)
            {
                settings.Tools.ExportFcpImageType = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportFcpPalNtsc");
            if (subNode != null)
            {
                settings.Tools.ExportFcpPalNtsc = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportBdnXmlImageType");
            if (subNode != null)
            {
                settings.Tools.ExportBdnXmlImageType = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportLastFontSize");
            if (subNode != null)
            {
                settings.Tools.ExportLastFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportLastLineHeight");
            if (subNode != null)
            {
                settings.Tools.ExportLastLineHeight = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportLastBorderWidth");
            if (subNode != null)
            {
                settings.Tools.ExportLastBorderWidth = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportLastFontBold");
            if (subNode != null)
            {
                settings.Tools.ExportLastFontBold = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportBluRayVideoResolution");
            if (subNode != null)
            {
                settings.Tools.ExportBluRayVideoResolution = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportFcpVideoResolution");
            if (subNode != null)
            {
                settings.Tools.ExportFcpVideoResolution = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportFontColor");
            if (subNode != null)
            {
                settings.Tools.ExportFontColor = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("ExportBorderColor");
            if (subNode != null)
            {
                settings.Tools.ExportBorderColor = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("ExportShadowColor");
            if (subNode != null)
            {
                settings.Tools.ExportShadowColor = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("ExportBoxBorderSize");
            if (subNode != null)
            {
                settings.Tools.ExportBoxBorderSize = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportBottomMarginUnit");
            if (subNode != null)
            {
                settings.Tools.ExportBottomMarginUnit = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportBottomMarginPercent");
            if (subNode != null)
            {
                settings.Tools.ExportBottomMarginPercent = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportBottomMarginPixels");
            if (subNode != null)
            {
                settings.Tools.ExportBottomMarginPixels = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportLeftRightMarginUnit");
            if (subNode != null)
            {
                settings.Tools.ExportLeftRightMarginUnit = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportLeftRightMarginPercent");
            if (subNode != null)
            {
                settings.Tools.ExportLeftRightMarginPercent = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportLeftRightMarginPixels");
            if (subNode != null)
            {
                settings.Tools.ExportLeftRightMarginPixels = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportHorizontalAlignment");
            if (subNode != null)
            {
                settings.Tools.ExportHorizontalAlignment = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportBluRayBottomMarginPercent");
            if (subNode != null)
            {
                settings.Tools.ExportBluRayBottomMarginPercent = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportBluRayBottomMarginPixels");
            if (subNode != null)
            {
                settings.Tools.ExportBluRayBottomMarginPixels = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportBluRayShadow");
            if (subNode != null)
            {
                settings.Tools.ExportBluRayShadow = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportBluRayRemoveSmallGaps");
            if (subNode != null)
            {
                settings.Tools.ExportBluRayRemoveSmallGaps = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportCdgBackgroundImage");
            if (subNode != null)
            {
                settings.Tools.ExportCdgBackgroundImage = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportCdgMarginLeft");
            if (subNode != null)
            {
                settings.Tools.ExportCdgMarginLeft = Convert.ToInt32(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ExportCdgMarginBottom");
            if (subNode != null)
            {
                settings.Tools.ExportCdgMarginBottom = Convert.ToInt32(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ExportCdgFormat");
            if (subNode != null)
            {
                settings.Tools.ExportCdgFormat = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("Export3DType");
            if (subNode != null)
            {
                settings.Tools.Export3DType = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("Export3DDepth");
            if (subNode != null)
            {
                settings.Tools.Export3DDepth = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportLastShadowTransparency");
            if (subNode != null)
            {
                settings.Tools.ExportLastShadowTransparency = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportLastFrameRate");
            if (subNode != null)
            {
                settings.Tools.ExportLastFrameRate = double.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportFullFrame");
            if (subNode != null)
            {
                settings.Tools.ExportFullFrame = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportFcpFullPathUrl");
            if (subNode != null)
            {
                settings.Tools.ExportFcpFullPathUrl = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportPenLineJoin");
            if (subNode != null)
            {
                settings.Tools.ExportPenLineJoin = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BinEditShowColumnGap");
            if (subNode != null)
            {
                settings.Tools.BinEditShowColumnGap = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixCommonErrorsFixOverlapAllowEqualEndStart");
            if (subNode != null)
            {
                settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixCommonErrorsSkipStepOne");
            if (subNode != null)
            {
                settings.Tools.FixCommonErrorsSkipStepOne = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ImportTextSplitting");
            if (subNode != null)
            {
                settings.Tools.ImportTextSplitting = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ImportTextSplittingLineMode");
            if (subNode != null)
            {
                settings.Tools.ImportTextSplittingLineMode = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ImportTextMergeShortLines");
            if (subNode != null)
            {
                settings.Tools.ImportTextMergeShortLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ImportTextLineBreak");
            if (subNode != null)
            {
                settings.Tools.ImportTextLineBreak = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ImportTextRemoveEmptyLines");
            if (subNode != null)
            {
                settings.Tools.ImportTextRemoveEmptyLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ImportTextAutoSplitAtBlank");
            if (subNode != null)
            {
                settings.Tools.ImportTextAutoSplitAtBlank = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ImportTextRemoveLinesNoLetters");
            if (subNode != null)
            {
                settings.Tools.ImportTextRemoveLinesNoLetters = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ImportTextGenerateTimeCodes");
            if (subNode != null)
            {
                settings.Tools.ImportTextGenerateTimeCodes = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ImportTextTakeTimeCodeFromFileName");
            if (subNode != null)
            {
                settings.Tools.ImportTextTakeTimeCodeFromFileName = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ImportTextAutoBreak");
            if (subNode != null)
            {
                settings.Tools.ImportTextAutoBreak = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ImportTextAutoBreakAtEnd");
            if (subNode != null)
            {
                settings.Tools.ImportTextAutoBreakAtEnd = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ImportTextGap");
            if (subNode != null)
            {
                settings.Tools.ImportTextGap = Convert.ToDecimal(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ImportTextAutoSplitNumberOfLines");
            if (subNode != null)
            {
                settings.Tools.ImportTextAutoSplitNumberOfLines = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ImportTextAutoBreakAtEndMarkerText");
            if (subNode != null)
            {
                settings.Tools.ImportTextAutoBreakAtEndMarkerText = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ImportTextDurationAuto");
            if (subNode != null)
            {
                settings.Tools.ImportTextDurationAuto = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ImportTextFixedDuration");
            if (subNode != null)
            {
                settings.Tools.ImportTextFixedDuration = Convert.ToDecimal(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GenerateTimeCodePatterns");
            if (subNode != null)
            {
                settings.Tools.GenerateTimeCodePatterns = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GenerateTimeCodePatterns");
            if (subNode != null)
            {
                settings.Tools.GenerateTimeCodePatterns = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("MusicSymbolStyle");
            if (subNode != null)
            {
                settings.Tools.MusicSymbolStyle = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BinEditBackgroundColor");
            if (subNode != null)
            {
                settings.Tools.BinEditBackgroundColor = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("BinEditImageBackgroundColor");
            if (subNode != null)
            {
                settings.Tools.BinEditImageBackgroundColor = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("BinEditTopMargin");
            if (subNode != null)
            {
                settings.Tools.BinEditTopMargin = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BinEditBottomMargin");
            if (subNode != null)
            {
                settings.Tools.BinEditBottomMargin = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BinEditLeftMargin");
            if (subNode != null)
            {
                settings.Tools.BinEditLeftMargin = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BinEditRightMargin");
            if (subNode != null)
            {
                settings.Tools.BinEditRightMargin = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BinEditStartPosition");
            if (subNode != null)
            {
                settings.Tools.BinEditStartPosition = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BinEditStartSize");
            if (subNode != null)
            {
                settings.Tools.BinEditStartSize = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BridgeGapMilliseconds");
            if (subNode != null)
            {
                settings.Tools.BridgeGapMilliseconds = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BridgeGapMillisecondsMinGap");
            if (subNode != null)
            {
                settings.Tools.BridgeGapMillisecondsMinGap = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportCustomTemplates");
            if (subNode != null)
            {
                settings.Tools.ExportCustomTemplates = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ChangeCasingChoice");
            if (subNode != null)
            {
                settings.Tools.ChangeCasingChoice = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ChangeCasingNormalFixNames");
            if (subNode != null)
            {
                settings.Tools.ChangeCasingNormalFixNames = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ChangeCasingNormalOnlyUppercase");
            if (subNode != null)
            {
                settings.Tools.ChangeCasingNormalOnlyUppercase = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("UseNoLineBreakAfter");
            if (subNode != null)
            {
                settings.Tools.UseNoLineBreakAfter = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("NoLineBreakAfterEnglish");
            if (subNode != null)
            {
                settings.Tools.NoLineBreakAfterEnglish = subNode.InnerText.Replace("  ", " ");
            }

            subNode = node.SelectSingleNode("ExportTextFormatText");
            if (subNode != null)
            {
                settings.Tools.ExportTextFormatText = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ReplaceIn");
            if (subNode != null)
            {
                settings.Tools.ReplaceIn = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportTextRemoveStyling");
            if (subNode != null)
            {
                settings.Tools.ExportTextRemoveStyling = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportTextShowLineNumbers");
            if (subNode != null)
            {
                settings.Tools.ExportTextShowLineNumbers = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportTextShowLineNumbersNewLine");
            if (subNode != null)
            {
                settings.Tools.ExportTextShowLineNumbersNewLine = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportTextShowTimeCodes");
            if (subNode != null)
            {
                settings.Tools.ExportTextShowTimeCodes = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportTextShowTimeCodesNewLine");
            if (subNode != null)
            {
                settings.Tools.ExportTextShowTimeCodesNewLine = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportTextNewLineAfterText");
            if (subNode != null)
            {
                settings.Tools.ExportTextNewLineAfterText = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportTextNewLineBetweenSubtitles");
            if (subNode != null)
            {
                settings.Tools.ExportTextNewLineBetweenSubtitles = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ExportTextTimeCodeFormat");
            if (subNode != null)
            {
                settings.Tools.ExportTextTimeCodeFormat = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ExportTextTimeCodeSeparator");
            if (subNode != null)
            {
                settings.Tools.ExportTextTimeCodeSeparator = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("VideoOffsetKeepTimeCodes");
            if (subNode != null)
            {
                settings.Tools.VideoOffsetKeepTimeCodes = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MoveStartEndMs");
            if (subNode != null)
            {
                settings.Tools.MoveStartEndMs = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AdjustDurationSeconds");
            if (subNode != null)
            {
                settings.Tools.AdjustDurationSeconds = Convert.ToDecimal(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AdjustDurationPercent");
            if (subNode != null)
            {
                settings.Tools.AdjustDurationPercent = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AdjustDurationLast");
            if (subNode != null)
            {
                settings.Tools.AdjustDurationLast = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AdjustDurationExtendOnly");
            if (subNode != null)
            {
                settings.Tools.AdjustDurationExtendOnly = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AdjustDurationExtendEnforceDurationLimits");
            if (subNode != null)
            {
                settings.Tools.AdjustDurationExtendEnforceDurationLimits = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AdjustDurationExtendCheckShotChanges");
            if (subNode != null)
            {
                settings.Tools.AdjustDurationExtendCheckShotChanges = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ChangeSpeedAllowOverlap");
            if (subNode != null)
            {
                settings.Tools.ChangeSpeedAllowOverlap = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoBreakCommaBreakEarly");
            if (subNode != null)
            {
                settings.Tools.AutoBreakCommaBreakEarly = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoBreakDashEarly");
            if (subNode != null)
            {
                settings.Tools.AutoBreakDashEarly = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoBreakLineEndingEarly");
            if (subNode != null)
            {
                settings.Tools.AutoBreakLineEndingEarly = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoBreakUsePixelWidth");
            if (subNode != null)
            {
                settings.Tools.AutoBreakUsePixelWidth = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoBreakPreferBottomHeavy");
            if (subNode != null)
            {
                settings.Tools.AutoBreakPreferBottomHeavy = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoBreakPreferBottomPercent");
            if (subNode != null)
            {
                settings.Tools.AutoBreakPreferBottomPercent = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ApplyMinimumDurationLimit");
            if (subNode != null)
            {
                settings.Tools.ApplyMinimumDurationLimit = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ApplyMinimumDurationLimitCheckShotChanges");
            if (subNode != null)
            {
                settings.Tools.ApplyMinimumDurationLimitCheckShotChanges = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ApplyMaximumDurationLimit");
            if (subNode != null)
            {
                settings.Tools.ApplyMaximumDurationLimit = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MergeShortLinesMaxGap");
            if (subNode != null)
            {
                settings.Tools.MergeShortLinesMaxGap = Convert.ToInt32(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("MergeShortLinesMaxChars");
            if (subNode != null)
            {
                settings.Tools.MergeShortLinesMaxChars = Convert.ToInt32(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("MergeShortLinesOnlyContinuous");
            if (subNode != null)
            {
                settings.Tools.MergeShortLinesOnlyContinuous = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MergeTextWithSameTimeCodesMaxGap");
            if (subNode != null)
            {
                settings.Tools.MergeTextWithSameTimeCodesMaxGap = Convert.ToInt32(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("MergeTextWithSameTimeCodesMakeDialog");
            if (subNode != null)
            {
                settings.Tools.MergeTextWithSameTimeCodesMakeDialog = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MergeTextWithSameTimeCodesReBreakLines");
            if (subNode != null)
            {
                settings.Tools.MergeTextWithSameTimeCodesReBreakLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MergeLinesWithSameTextMaxMs");
            if (subNode != null)
            {
                settings.Tools.MergeLinesWithSameTextMaxMs = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MergeLinesWithSameTextIncrement");
            if (subNode != null)
            {
                settings.Tools.MergeLinesWithSameTextIncrement = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ConvertColorsToDialogRemoveColorTags");
            if (subNode != null)
            {
                settings.Tools.ConvertColorsToDialogRemoveColorTags = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ConvertColorsToDialogAddNewLines");
            if (subNode != null)
            {
                settings.Tools.ConvertColorsToDialogAddNewLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ConvertColorsToDialogReBreakLines");
            if (subNode != null)
            {
                settings.Tools.ConvertColorsToDialogReBreakLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ColumnPasteColumn");
            if (subNode != null)
            {
                settings.Tools.ColumnPasteColumn = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ColumnPasteOverwriteMode");
            if (subNode != null)
            {
                settings.Tools.ColumnPasteOverwriteMode = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AssaAttachmentFontTextPreview");
            if (subNode != null)
            {
                settings.Tools.AssaAttachmentFontTextPreview = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AssaSetPositionTarget");
            if (subNode != null)
            {
                settings.Tools.AssaSetPositionTarget = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("VisualSyncStartSize");
            if (subNode != null)
            {
                settings.Tools.VisualSyncStartSize = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BlankVideoColor");
            if (subNode != null)
            {
                settings.Tools.BlankVideoColor = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BlankVideoMinutes");
            if (subNode != null)
            {
                settings.Tools.BlankVideoMinutes = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BlankVideoFrameRate");
            if (subNode != null)
            {
                settings.Tools.BlankVideoFrameRate = Convert.ToDecimal(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BlankVideoUseCheckeredImage");
            if (subNode != null)
            {
                settings.Tools.BlankVideoUseCheckeredImage = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaProgressBarBackColor");
            if (subNode != null)
            {
                settings.Tools.AssaProgressBarBackColor = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AssaProgressBarForeColor");
            if (subNode != null)
            {
                settings.Tools.AssaProgressBarForeColor = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AssaProgressBarTextColor");
            if (subNode != null)
            {
                settings.Tools.AssaProgressBarTextColor = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AssaProgressBarHeight");
            if (subNode != null)
            {
                settings.Tools.AssaProgressBarHeight = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaProgressBarSplitterWidth");
            if (subNode != null)
            {
                settings.Tools.AssaProgressBarSplitterWidth = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaProgressBarSplitterHeight");
            if (subNode != null)
            {
                settings.Tools.AssaProgressBarSplitterHeight = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaProgressBarFontName");
            if (subNode != null)
            {
                settings.Tools.AssaProgressBarFontName = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AssaProgressBarFontSize");
            if (subNode != null)
            {
                settings.Tools.AssaProgressBarFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaProgressBarTopAlign");
            if (subNode != null)
            {
                settings.Tools.AssaProgressBarTopAlign = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaProgressBarTextAlign");
            if (subNode != null)
            {
                settings.Tools.AssaProgressBarTextAlign = subNode.InnerText;
            }


            subNode = node.SelectSingleNode("AssaBgBoxPaddingLeft");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxPaddingLeft = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxPaddingRight");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxPaddingRight = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxPaddingTop");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxPaddingTop = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxPaddingBottom");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxPaddingBottom = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxDrawingMarginV");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxDrawingMarginV = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxDrawingMarginH");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxDrawingMarginH = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxDrawingAlignment");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxDrawingAlignment = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AssaBgBoxColor");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxColor = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AssaBgBoxOutlineColor");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxOutlineColor = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AssaBgBoxShadowColor");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxShadowColor = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AssaBgBoxTransparentColor");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxTransparentColor = FromHtml(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AssaBgBoxStyle");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxStyle = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AssaBgBoxStyleRadius");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxStyleRadius = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxStyleCircleAdjustY");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxStyleCircleAdjustY = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxStyleSpikesStep");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxStyleSpikesStep = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxStyleSpikesHeight");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxStyleSpikesHeight = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxStyleBubblesStep");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxStyleBubblesStep = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxStyleBubblesHeight");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxStyleBubblesHeight = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxOutlineWidth");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxOutlineWidth = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxLayer");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxLayer = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxDrawingFileWatch");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxDrawingFileWatch = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxDrawingOnly");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxDrawingOnly = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AssaBgBoxDrawing");
            if (subNode != null)
            {
                settings.Tools.AssaBgBoxDrawing = subNode.InnerText;
            }


            subNode = node.SelectSingleNode("GenVideoFontName");
            if (subNode != null)
            {
                settings.Tools.GenVideoFontName = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GenVideoFontBold");
            if (subNode != null)
            {
                settings.Tools.GenVideoFontBold = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GenVideoOutline");
            if (subNode != null)
            {
                settings.Tools.GenVideoOutline = Convert.ToDecimal(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GenVideoFontSize");
            if (subNode != null)
            {
                settings.Tools.GenVideoFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GenVideoEncoding");
            if (subNode != null)
            {
                settings.Tools.GenVideoEncoding = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GenVideoPreset");
            if (subNode != null)
            {
                settings.Tools.GenVideoPreset = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GenVideoCrf");
            if (subNode != null)
            {
                settings.Tools.GenVideoCrf = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GenVideoTune");
            if (subNode != null)
            {
                settings.Tools.GenVideoTune = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GenVideoAudioEncoding");
            if (subNode != null)
            {
                settings.Tools.GenVideoAudioEncoding = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GenVideoAudioForceStereo");
            if (subNode != null)
            {
                settings.Tools.GenVideoAudioForceStereo = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GenVideoAudioSampleRate");
            if (subNode != null)
            {
                settings.Tools.GenVideoAudioSampleRate = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GenVideoTargetFileSize");
            if (subNode != null)
            {
                settings.Tools.GenVideoTargetFileSize = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GenVideoFontSizePercentOfHeight");
            if (subNode != null)
            {
                settings.Tools.GenVideoFontSizePercentOfHeight = (float)Convert.ToDecimal(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GenVideoNonAssaBox");
            if (subNode != null)
            {
                settings.Tools.GenVideoNonAssaBox = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GenVideoNonAssaBoxColor");
            if (subNode != null)
            {
                settings.Tools.GenVideoNonAssaBoxColor = FromHtml(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("GenVideoNonAssaTextColor");
            if (subNode != null)
            {
                settings.Tools.GenVideoNonAssaTextColor = FromHtml(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("GenVideoNonAssaAlignRight");
            if (subNode != null)
            {
                settings.Tools.GenVideoNonAssaAlignRight = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GenVideoNonAssaFixRtlUnicode");
            if (subNode != null)
            {
                settings.Tools.GenVideoNonAssaFixRtlUnicode = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GenVideoEmbedOutputExt");
            if (subNode != null)
            {
                settings.Tools.GenVideoEmbedOutputExt = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GenVideoEmbedOutputSuffix");
            if (subNode != null)
            {
                settings.Tools.GenVideoEmbedOutputSuffix = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GenVideoEmbedOutputReplace");
            if (subNode != null)
            {
                settings.Tools.GenVideoEmbedOutputReplace = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GenVideoDeleteInputVideoFile");
            if (subNode != null)
            {
                settings.Tools.GenVideoDeleteInputVideoFile = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GenVideoUseOutputFolder");
            if (subNode != null)
            {
                settings.Tools.GenVideoUseOutputFolder = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GenVideoOutputFolder");
            if (subNode != null)
            {
                settings.Tools.GenVideoOutputFolder = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GenVideoOutputFileSuffix");
            if (subNode != null)
            {
                settings.Tools.GenVideoOutputFileSuffix = subNode.InnerText;
            }


            subNode = node.SelectSingleNode("VoskPostProcessing");
            if (subNode != null)
            {
                settings.Tools.VoskPostProcessing = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("VoskModel");
            if (subNode != null)
            {
                settings.Tools.VoskModel = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WhisperChoice");
            if (subNode != null)
            {
                settings.Tools.WhisperChoice = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WhisperIgnoreVersion");
            if (subNode != null)
            {
                settings.Tools.WhisperIgnoreVersion = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WhisperDeleteTempFiles");
            if (subNode != null)
            {
                settings.Tools.WhisperDeleteTempFiles = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WhisperPostProcessingAddPeriods");
            if (subNode != null)
            {
                settings.Tools.WhisperPostProcessingAddPeriods = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WhisperPostProcessingMergeLines");
            if (subNode != null)
            {
                settings.Tools.WhisperPostProcessingMergeLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WhisperPostProcessingSplitLines");
            if (subNode != null)
            {
                settings.Tools.WhisperPostProcessingSplitLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WhisperPostProcessingFixCasing");
            if (subNode != null)
            {
                settings.Tools.WhisperPostProcessingFixCasing = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WhisperPostProcessingFixShortDuration");
            if (subNode != null)
            {
                settings.Tools.WhisperPostProcessingFixShortDuration = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WhisperModel");
            if (subNode != null)
            {
                settings.Tools.WhisperModel = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WhisperLocation");
            if (subNode != null)
            {
                settings.Tools.WhisperLocation = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WhisperXLocation");
            if (subNode != null)
            {
                settings.Tools.WhisperXLocation = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WhisperStableTsLocation");
            if (subNode != null)
            {
                settings.Tools.WhisperStableTsLocation = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WhisperCppModelLocation");
            if (subNode != null)
            {
                settings.Tools.WhisperCppModelLocation = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WhisperCtranslate2Location");
            if (subNode != null)
            {
                settings.Tools.WhisperCtranslate2Location = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WhisperPurfviewFasterWhisperLocation");
            if (subNode != null)
            {
                settings.Tools.WhisperPurfviewFasterWhisperLocation = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WhisperPurfviewFasterWhisperDefaultCmd");
            if (subNode != null)
            {
                settings.Tools.WhisperPurfviewFasterWhisperDefaultCmd = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WhisperExtraSettings");
            if (subNode != null)
            {
                settings.Tools.WhisperExtraSettings = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WhisperExtraSettingsHistory");
            if (subNode != null)
            {
                settings.Tools.WhisperExtraSettingsHistory = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WhisperLanguageCode");
            if (subNode != null)
            {
                settings.Tools.WhisperLanguageCode = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WhisperAutoAdjustTimings");
            if (subNode != null)
            {
                settings.Tools.WhisperAutoAdjustTimings = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WhisperUseLineMaxChars");
            if (subNode != null)
            {
                settings.Tools.WhisperUseLineMaxChars = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AudioToTextLineMaxChars");
            if (subNode != null)
            {
                settings.Tools.AudioToTextLineMaxChars = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AudioToTextLineMaxCharsJp");
            if (subNode != null)
            {
                settings.Tools.AudioToTextLineMaxCharsJp = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AudioToTextLineMaxCharsCn");
            if (subNode != null)
            {
                settings.Tools.AudioToTextLineMaxCharsCn = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BreakLinesLongerThan");
            if (subNode != null)
            {
                settings.Tools.BreakLinesLongerThan = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("UnbreakLinesLongerThan");
            if (subNode != null)
            {
                settings.Tools.UnbreakLinesLongerThan = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FindHistory");
            if (subNode != null)
            {
                foreach (XmlNode findItem in subNode.ChildNodes)
                {
                    if (findItem.Name == "Text")
                    {
                        settings.Tools.FindHistory.Add(findItem.InnerText);
                    }
                }
            }

            // Subtitle
            node = doc.DocumentElement.SelectSingleNode("SubtitleSettings");
            if (node != null)
            {
                foreach (XmlNode categoryNode in node.SelectNodes("AssaStyleStorageCategories/Category"))
                {
                    var category = new AssaStorageCategory
                    {
                        Styles = new List<SsaStyle>()
                    };
                    subNode = categoryNode.SelectSingleNode("Name");
                    if (subNode != null)
                    {
                        category.Name = subNode.InnerText;
                    }

                    subNode = categoryNode.SelectSingleNode("IsDefault");
                    if (subNode != null)
                    {
                        category.IsDefault = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    settings.SubtitleSettings.AssaStyleStorageCategories.Add(category);

                    foreach (XmlNode listNode in categoryNode.SelectNodes("Style"))
                    {
                        var item = new SsaStyle();
                        subNode = listNode.SelectSingleNode("Name");
                        if (subNode != null)
                        {
                            item.Name = subNode.InnerText;
                        }

                        subNode = listNode.SelectSingleNode("FontName");
                        if (subNode != null)
                        {
                            item.FontName = subNode.InnerText;
                        }

                        subNode = listNode.SelectSingleNode("FontSize");
                        if (subNode != null)
                        {
                            item.FontSize = Convert.ToDecimal(subNode.InnerText, CultureInfo.InvariantCulture);
                        }

                        subNode = listNode.SelectSingleNode("Bold");
                        if (subNode != null)
                        {
                            item.Bold = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                        }

                        subNode = listNode.SelectSingleNode("Italic");
                        if (subNode != null)
                        {
                            item.Italic = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                        }

                        subNode = listNode.SelectSingleNode("Underline");
                        if (subNode != null)
                        {
                            item.Underline = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                        }

                        subNode = listNode.SelectSingleNode("StrikeOut");
                        if (subNode != null)
                        {
                            item.Strikeout = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                        }

                        subNode = listNode.SelectSingleNode("Primary");
                        if (subNode != null)
                        {
                            item.Primary = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
                        }

                        subNode = listNode.SelectSingleNode("Secondary");
                        if (subNode != null)
                        {
                            item.Secondary = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
                        }

                        subNode = listNode.SelectSingleNode("Outline");
                        if (subNode != null)
                        {
                            item.Outline = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
                        }

                        subNode = listNode.SelectSingleNode("Background");
                        if (subNode != null)
                        {
                            item.Background = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
                        }

                        subNode = listNode.SelectSingleNode("ShadowWidth");
                        if (subNode != null)
                        {
                            item.ShadowWidth = Convert.ToDecimal(subNode.InnerText, CultureInfo.InvariantCulture);
                        }

                        subNode = listNode.SelectSingleNode("OutlineWidth");
                        if (subNode != null)
                        {
                            item.OutlineWidth = Convert.ToDecimal(subNode.InnerText, CultureInfo.InvariantCulture);
                        }

                        subNode = listNode.SelectSingleNode("Alignment");
                        if (subNode != null)
                        {
                            item.Alignment = subNode.InnerText;
                        }

                        subNode = listNode.SelectSingleNode("MarginLeft");
                        if (subNode != null)
                        {
                            item.MarginLeft = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                        }

                        subNode = listNode.SelectSingleNode("MarginRight");
                        if (subNode != null)
                        {
                            item.MarginRight = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                        }

                        subNode = listNode.SelectSingleNode("MarginVertical");
                        if (subNode != null)
                        {
                            item.MarginVertical = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                        }

                        subNode = listNode.SelectSingleNode("BorderStyle");
                        if (subNode != null)
                        {
                            item.BorderStyle = subNode.InnerText;
                        }

                        subNode = listNode.SelectSingleNode("ScaleX");
                        if (subNode != null)
                        {
                            item.ScaleX = Convert.ToDecimal(subNode.InnerText, CultureInfo.InvariantCulture);
                        }

                        subNode = listNode.SelectSingleNode("ScaleY");
                        if (subNode != null)
                        {
                            item.ScaleY = Convert.ToDecimal(subNode.InnerText, CultureInfo.InvariantCulture);
                        }

                        subNode = listNode.SelectSingleNode("Spacing");
                        if (subNode != null)
                        {
                            item.Spacing = Convert.ToDecimal(subNode.InnerText, CultureInfo.InvariantCulture);
                        }

                        subNode = listNode.SelectSingleNode("Angle");
                        if (subNode != null)
                        {
                            item.Angle = Convert.ToDecimal(subNode.InnerText, CultureInfo.InvariantCulture);
                        }

                        category.Styles.Add(item);
                    }
                }

                foreach (XmlNode tagNode in node.SelectNodes("AssaApplyOverrideTags/Tag"))
                {
                    var tag = tagNode.InnerText;
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        settings.SubtitleSettings.AssaOverrideTagHistory.Add(tag);
                    }
                }

                subNode = node.SelectSingleNode("AssaResolutionAutoNew");
                if (subNode != null)
                {
                    settings.SubtitleSettings.AssaResolutionAutoNew = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("AssaResolutionPromptChange");
                if (subNode != null)
                {
                    settings.SubtitleSettings.AssaResolutionPromptChange = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("AssaShowPlayDepth");
                if (subNode != null)
                {
                    settings.SubtitleSettings.AssaShowPlayDepth = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("AssaShowScaledBorderAndShadow");
                if (subNode != null)
                {
                    settings.SubtitleSettings.AssaShowScaledBorderAndShadow = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("DCinemaFontFile");
                if (subNode != null)
                {
                    settings.SubtitleSettings.DCinemaFontFile = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("DCinemaFontSize");
                if (subNode != null)
                {
                    settings.SubtitleSettings.DCinemaFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("DCinemaBottomMargin");
                if (subNode != null)
                {
                    settings.SubtitleSettings.DCinemaBottomMargin = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("DCinemaZPosition");
                if (subNode != null)
                {
                    settings.SubtitleSettings.DCinemaZPosition = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("DCinemaFadeUpTime");
                if (subNode != null)
                {
                    settings.SubtitleSettings.DCinemaFadeUpTime = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("DCinemaFadeDownTime");
                if (subNode != null)
                {
                    settings.SubtitleSettings.DCinemaFadeDownTime = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("DCinemaAutoGenerateSubtitleId");
                if (subNode != null)
                {
                    settings.SubtitleSettings.DCinemaAutoGenerateSubtitleId = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("SamiDisplayTwoClassesAsTwoSubtitles");
                if (subNode != null)
                {
                    settings.SubtitleSettings.SamiDisplayTwoClassesAsTwoSubtitles = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("SamiHtmlEncodeMode");
                if (subNode != null)
                {
                    settings.SubtitleSettings.SamiHtmlEncodeMode = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("TimedText10TimeCodeFormat");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TimedText10TimeCodeFormat = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("TimedText10ShowStyleAndLanguage");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TimedText10ShowStyleAndLanguage = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("TimedText10FileExtension");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TimedText10FileExtension = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("TimedTextItunesTopOrigin");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TimedTextItunesTopOrigin = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("TimedTextItunesTopExtent");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TimedTextItunesTopExtent = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("TimedTextItunesBottomOrigin");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TimedTextItunesBottomOrigin = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("TimedTextItunesBottomExtent");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TimedTextItunesBottomExtent = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("TimedTextItunesTimeCodeFormat");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TimedTextItunesTimeCodeFormat = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("TimedTextItunesStyleAttribute");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TimedTextItunesStyleAttribute = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("TimedTextImsc11TimeCodeFormat");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TimedTextImsc11TimeCodeFormat = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("TimedTextImsc11FileExtension");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TimedTextImsc11FileExtension = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("FcpFontSize");
                if (subNode != null)
                {
                    settings.SubtitleSettings.FcpFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("FcpFontName");
                if (subNode != null)
                {
                    settings.SubtitleSettings.FcpFontName = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("EbuStlTeletextUseBox");
                if (subNode != null)
                {
                    settings.SubtitleSettings.EbuStlTeletextUseBox = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("EbuStlTeletextUseDoubleHeight");
                if (subNode != null)
                {
                    settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("EbuStlMarginTop");
                if (subNode != null)
                {
                    settings.SubtitleSettings.EbuStlMarginTop = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("EbuStlMarginBottom");
                if (subNode != null)
                {
                    settings.SubtitleSettings.EbuStlMarginBottom = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("EbuStlNewLineRows");
                if (subNode != null)
                {
                    settings.SubtitleSettings.EbuStlNewLineRows = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("EbuStlRemoveEmptyLines");
                if (subNode != null)
                {
                    settings.SubtitleSettings.EbuStlRemoveEmptyLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("PacVerticalTop");
                if (subNode != null)
                {
                    settings.SubtitleSettings.PacVerticalTop = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("PacVerticalCenter");
                if (subNode != null)
                {
                    settings.SubtitleSettings.PacVerticalCenter = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("PacVerticalBottom");
                if (subNode != null)
                {
                    settings.SubtitleSettings.PacVerticalBottom = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("DvdStudioProHeader");
                if (subNode != null)
                {
                    settings.SubtitleSettings.DvdStudioProHeader = subNode.InnerText.TrimEnd() + Environment.NewLine;
                }

                subNode = node.SelectSingleNode("TmpegEncXmlFontName");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TmpegEncXmlFontName = subNode.InnerText.TrimEnd();
                }

                subNode = node.SelectSingleNode("TmpegEncXmlFontHeight");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TmpegEncXmlFontHeight = subNode.InnerText.TrimEnd();
                }

                subNode = node.SelectSingleNode("TmpegEncXmlPosition");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TmpegEncXmlPosition = subNode.InnerText.TrimEnd();
                }

                subNode = node.SelectSingleNode("CheetahCaptionAlwayWriteEndTime");
                if (subNode != null)
                {
                    settings.SubtitleSettings.CheetahCaptionAlwayWriteEndTime = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("NuendoCharacterListFile");
                if (subNode != null)
                {
                    settings.SubtitleSettings.NuendoCharacterListFile = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("Cavena890StartOfMessage");
                if (subNode != null)
                {
                    settings.SubtitleSettings.Cavena890StartOfMessage = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WebVttTimescale");
                if (subNode != null)
                {
                    settings.SubtitleSettings.WebVttTimescale = long.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("WebVttCueAn1");
                if (subNode != null)
                {
                    settings.SubtitleSettings.WebVttCueAn1 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WebVttCueAn2");
                if (subNode != null)
                {
                    settings.SubtitleSettings.WebVttCueAn2 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WebVttCueAn3");
                if (subNode != null)
                {
                    settings.SubtitleSettings.WebVttCueAn3 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WebVttCueAn4");
                if (subNode != null)
                {
                    settings.SubtitleSettings.WebVttCueAn4 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WebVttCueAn5");
                if (subNode != null)
                {
                    settings.SubtitleSettings.WebVttCueAn5 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WebVttCueAn6");
                if (subNode != null)
                {
                    settings.SubtitleSettings.WebVttCueAn6 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WebVttCueAn7");
                if (subNode != null)
                {
                    settings.SubtitleSettings.WebVttCueAn7 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WebVttCueAn8");
                if (subNode != null)
                {
                    settings.SubtitleSettings.WebVttCueAn8 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WebVttCueAn9");
                if (subNode != null)
                {
                    settings.SubtitleSettings.WebVttCueAn9 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MPlayer2Extension");
                if (subNode != null)
                {
                    settings.SubtitleSettings.MPlayer2Extension = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("TeletextItalicFix");
                if (subNode != null)
                {
                    settings.SubtitleSettings.TeletextItalicFix = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("MccDebug");
                if (subNode != null)
                {
                    settings.SubtitleSettings.MccDebug = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("BluRaySupSkipMerge");
                if (subNode != null)
                {
                    settings.SubtitleSettings.BluRaySupSkipMerge = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("BluRaySupForceMergeAll");
                if (subNode != null)
                {
                    settings.SubtitleSettings.BluRaySupForceMergeAll = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("WebVttUseXTimestampMap");
                if (subNode != null)
                {
                    settings.SubtitleSettings.WebVttUseXTimestampMap = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("WebVttUseMultipleXTimestampMap");
                if (subNode != null)
                {
                    settings.SubtitleSettings.WebVttUseMultipleXTimestampMap = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("WebVttMergeLinesWithSameText");
                if (subNode != null)
                {
                    settings.SubtitleSettings.WebVttMergeLinesWithSameText = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }
            }

            // Proxy
            node = doc.DocumentElement.SelectSingleNode("Proxy");
            subNode = node.SelectSingleNode("ProxyAddress");
            if (subNode != null)
            {
                settings.Proxy.ProxyAddress = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("UserName");
            if (subNode != null)
            {
                settings.Proxy.UserName = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("Password");
            if (subNode != null)
            {
                settings.Proxy.Password = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("Domain");
            if (subNode != null)
            {
                settings.Proxy.Domain = subNode.InnerText;
            }

            // Fxp xml export settings
            node = doc.DocumentElement.SelectSingleNode("FcpExportSettings");
            if (node != null)
            {
                subNode = node.SelectSingleNode("FontName");
                if (subNode != null)
                {
                    settings.FcpExportSettings.FontName = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("FontSize");
                if (subNode != null)
                {
                    settings.FcpExportSettings.FontSize = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("Alignment");
                if (subNode != null)
                {
                    settings.FcpExportSettings.Alignment = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("Baseline");
                if (subNode != null)
                {
                    settings.FcpExportSettings.Baseline = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("Color");
                if (subNode != null)
                {
                    settings.FcpExportSettings.Color = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
                }
            }

            // Word List
            node = doc.DocumentElement.SelectSingleNode("WordLists");
            subNode = node.SelectSingleNode("LastLanguage");
            if (subNode != null)
            {
                settings.WordLists.LastLanguage = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("Names");
            if (subNode != null)
            {
                settings.WordLists.NamesUrl = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("UseOnlineNames");
            if (subNode != null)
            {
                settings.WordLists.UseOnlineNames = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            // Fix Common Errors
            node = doc.DocumentElement.SelectSingleNode("CommonErrors");
            subNode = node.SelectSingleNode("StartPosition");
            if (subNode != null)
            {
                settings.CommonErrors.StartPosition = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("StartSize");
            if (subNode != null)
            {
                settings.CommonErrors.StartSize = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("EmptyLinesTicked");
            if (subNode != null)
            {
                settings.CommonErrors.EmptyLinesTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("OverlappingDisplayTimeTicked");
            if (subNode != null)
            {
                settings.CommonErrors.OverlappingDisplayTimeTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("TooShortDisplayTimeTicked");
            if (subNode != null)
            {
                settings.CommonErrors.TooShortDisplayTimeTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("TooLongDisplayTimeTicked");
            if (subNode != null)
            {
                settings.CommonErrors.TooLongDisplayTimeTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("TooShortGapTicked");
            if (subNode != null)
            {
                settings.CommonErrors.TooShortGapTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("InvalidItalicTagsTicked");
            if (subNode != null)
            {
                settings.CommonErrors.InvalidItalicTagsTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BreakLongLinesTicked");
            if (subNode != null)
            {
                settings.CommonErrors.BreakLongLinesTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MergeShortLinesTicked");
            if (subNode != null)
            {
                settings.CommonErrors.MergeShortLinesTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MergeShortLinesAllTicked");
            if (subNode != null)
            {
                settings.CommonErrors.MergeShortLinesAllTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MergeShortLinesPixelWidthTicked");
            if (subNode != null)
            {
                settings.CommonErrors.MergeShortLinesPixelWidthTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("UnneededSpacesTicked");
            if (subNode != null)
            {
                settings.CommonErrors.UnneededSpacesTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("UnneededPeriodsTicked");
            if (subNode != null)
            {
                settings.CommonErrors.UnneededPeriodsTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixCommasTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixCommasTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MissingSpacesTicked");
            if (subNode != null)
            {
                settings.CommonErrors.MissingSpacesTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AddMissingQuotesTicked");
            if (subNode != null)
            {
                settings.CommonErrors.AddMissingQuotesTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("Fix3PlusLinesTicked");
            if (subNode != null)
            {
                settings.CommonErrors.Fix3PlusLinesTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixHyphensTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixHyphensTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixHyphensRemoveSingleLineTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixHyphensRemoveSingleLineTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("UppercaseIInsideLowercaseWordTicked");
            if (subNode != null)
            {
                settings.CommonErrors.UppercaseIInsideLowercaseWordTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("DoubleApostropheToQuoteTicked");
            if (subNode != null)
            {
                settings.CommonErrors.DoubleApostropheToQuoteTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AddPeriodAfterParagraphTicked");
            if (subNode != null)
            {
                settings.CommonErrors.AddPeriodAfterParagraphTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("StartWithUppercaseLetterAfterParagraphTicked");
            if (subNode != null)
            {
                settings.CommonErrors.StartWithUppercaseLetterAfterParagraphTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("StartWithUppercaseLetterAfterPeriodInsideParagraphTicked");
            if (subNode != null)
            {
                settings.CommonErrors.StartWithUppercaseLetterAfterPeriodInsideParagraphTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("StartWithUppercaseLetterAfterColonTicked");
            if (subNode != null)
            {
                settings.CommonErrors.StartWithUppercaseLetterAfterColonTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AloneLowercaseIToUppercaseIEnglishTicked");
            if (subNode != null)
            {
                settings.CommonErrors.AloneLowercaseIToUppercaseIEnglishTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixOcrErrorsViaReplaceListTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixOcrErrorsViaReplaceListTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("RemoveSpaceBetweenNumberTicked");
            if (subNode != null)
            {
                settings.CommonErrors.RemoveSpaceBetweenNumberTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixDialogsOnOneLineTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixDialogsOnOneLineTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("RemoveDialogFirstLineInNonDialogs");
            if (subNode != null)
            {
                settings.CommonErrors.RemoveDialogFirstLineInNonDialogs = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("TurkishAnsiTicked");
            if (subNode != null)
            {
                settings.CommonErrors.TurkishAnsiTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("DanishLetterITicked");
            if (subNode != null)
            {
                settings.CommonErrors.DanishLetterITicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SpanishInvertedQuestionAndExclamationMarksTicked");
            if (subNode != null)
            {
                settings.CommonErrors.SpanishInvertedQuestionAndExclamationMarksTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixDoubleDashTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixDoubleDashTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixDoubleGreaterThanTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixDoubleGreaterThanTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixEllipsesStartTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixEllipsesStartTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixMissingOpenBracketTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixMissingOpenBracketTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixMusicNotationTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixMusicNotationTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixContinuationStyleTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixContinuationStyleTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixUnnecessaryLeadingDotsTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixUnnecessaryLeadingDotsTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("NormalizeStringsTicked");
            if (subNode != null)
            {
                settings.CommonErrors.NormalizeStringsTicked = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("DefaultFixes");
            if (subNode != null)
            {
                settings.CommonErrors.DefaultFixes = subNode.InnerText;
            }

            // Video Controls
            node = doc.DocumentElement.SelectSingleNode("VideoControls");
            subNode = node.SelectSingleNode("CustomSearchText1");
            if (subNode != null)
            {
                settings.VideoControls.CustomSearchText1 = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("CustomSearchText2");
            if (subNode != null)
            {
                settings.VideoControls.CustomSearchText2 = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("CustomSearchText3");
            if (subNode != null)
            {
                settings.VideoControls.CustomSearchText3 = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("CustomSearchText4");
            if (subNode != null)
            {
                settings.VideoControls.CustomSearchText4 = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("CustomSearchText5");
            if (subNode != null)
            {
                settings.VideoControls.CustomSearchText5 = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("CustomSearchUrl1");
            if (subNode != null)
            {
                settings.VideoControls.CustomSearchUrl1 = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("CustomSearchUrl1");
            if (subNode != null)
            {
                settings.VideoControls.CustomSearchUrl1 = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("CustomSearchUrl2");
            if (subNode != null)
            {
                settings.VideoControls.CustomSearchUrl2 = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("CustomSearchUrl3");
            if (subNode != null)
            {
                settings.VideoControls.CustomSearchUrl3 = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("CustomSearchUrl4");
            if (subNode != null)
            {
                settings.VideoControls.CustomSearchUrl4 = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("CustomSearchUrl5");
            if (subNode != null)
            {
                settings.VideoControls.CustomSearchUrl5 = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("LastActiveTab");
            if (subNode != null)
            {
                settings.VideoControls.LastActiveTab = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WaveformDrawGrid");
            if (subNode != null)
            {
                settings.VideoControls.WaveformDrawGrid = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformDrawCps");
            if (subNode != null)
            {
                settings.VideoControls.WaveformDrawCps = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformDrawWpm");
            if (subNode != null)
            {
                settings.VideoControls.WaveformDrawWpm = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformAllowOverlap");
            if (subNode != null)
            {
                settings.VideoControls.WaveformAllowOverlap = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformFocusOnMouseEnter");
            if (subNode != null)
            {
                settings.VideoControls.WaveformFocusOnMouseEnter = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformListViewFocusOnMouseEnter");
            if (subNode != null)
            {
                settings.VideoControls.WaveformListViewFocusOnMouseEnter = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformSingleClickSelect");
            if (subNode != null)
            {
                settings.VideoControls.WaveformSingleClickSelect = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformSnapToShotChanges");
            if (subNode != null)
            {
                settings.VideoControls.WaveformSnapToShotChanges = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformShotChangeStartTimeBeforeMs");
            if (subNode != null)
            {
                settings.VideoControls.WaveformShotChangeStartTimeBeforeMs = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformShotChangeStartTimeAfterMs");
            if (subNode != null)
            {
                settings.VideoControls.WaveformShotChangeStartTimeAfterMs = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformShotChangeEndTimeBeforeMs");
            if (subNode != null)
            {
                settings.VideoControls.WaveformShotChangeEndTimeBeforeMs = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformShotChangeEndTimeAfterMs");
            if (subNode != null)
            {
                settings.VideoControls.WaveformShotChangeEndTimeAfterMs = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformSetVideoPositionOnMoveStartEnd");
            if (subNode != null)
            {
                settings.VideoControls.WaveformSetVideoPositionOnMoveStartEnd = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformBorderHitMs");
            if (subNode != null)
            {
                settings.VideoControls.WaveformBorderHitMs = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformGridColor");
            if (subNode != null)
            {
                settings.VideoControls.WaveformGridColor = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("WaveformColor");
            if (subNode != null)
            {
                settings.VideoControls.WaveformColor = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("WaveformSelectedColor");
            if (subNode != null)
            {
                settings.VideoControls.WaveformSelectedColor = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("WaveformBackgroundColor");
            if (subNode != null)
            {
                settings.VideoControls.WaveformBackgroundColor = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("WaveformTextColor");
            if (subNode != null)
            {
                settings.VideoControls.WaveformTextColor = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("WaveformCursorColor");
            if (subNode != null)
            {
                settings.VideoControls.WaveformCursorColor = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("WaveformChaptersColor");
            if (subNode != null)
            {
                settings.VideoControls.WaveformChaptersColor = Color.FromArgb(int.Parse(subNode.InnerText, CultureInfo.InvariantCulture));
            }

            subNode = node.SelectSingleNode("WaveformTextSize");
            if (subNode != null)
            {
                settings.VideoControls.WaveformTextSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformTextBold");
            if (subNode != null)
            {
                settings.VideoControls.WaveformTextBold = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformDoubleClickOnNonParagraphAction");
            if (subNode != null)
            {
                settings.VideoControls.WaveformDoubleClickOnNonParagraphAction = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WaveformRightClickOnNonParagraphAction");
            if (subNode != null)
            {
                settings.VideoControls.WaveformRightClickOnNonParagraphAction = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WaveformMouseWheelScrollUpIsForward");
            if (subNode != null)
            {
                settings.VideoControls.WaveformMouseWheelScrollUpIsForward = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GenerateSpectrogram");
            if (subNode != null)
            {
                settings.VideoControls.GenerateSpectrogram = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformLabelShowCodec");
            if (subNode != null)
            {
                settings.VideoControls.WaveformLabelShowCodec = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SpectrogramAppearance");
            if (subNode != null)
            {
                settings.VideoControls.SpectrogramAppearance = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("WaveformMinimumSampleRate");
            if (subNode != null)
            {
                settings.VideoControls.WaveformMinimumSampleRate = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformSeeksSilenceDurationSeconds");
            if (subNode != null)
            {
                settings.VideoControls.WaveformSeeksSilenceDurationSeconds = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformSeeksSilenceMaxVolume");
            if (subNode != null)
            {
                settings.VideoControls.WaveformSeeksSilenceMaxVolume = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformUnwrapText");
            if (subNode != null)
            {
                settings.VideoControls.WaveformUnwrapText = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformHideWpmCpsLabels");
            if (subNode != null)
            {
                settings.VideoControls.WaveformHideWpmCpsLabels = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            // Network
            node = doc.DocumentElement.SelectSingleNode("NetworkSettings");
            if (node != null)
            {
                subNode = node.SelectSingleNode("SessionKey");
                if (subNode != null)
                {
                    settings.NetworkSettings.SessionKey = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("UserName");
                if (subNode != null)
                {
                    settings.NetworkSettings.UserName = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WebApiUrl");
                if (subNode != null)
                {
                    settings.NetworkSettings.WebApiUrl = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("PollIntervalSeconds");
                if (subNode != null)
                {
                    settings.NetworkSettings.PollIntervalSeconds = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("NewMessageSound");
                if (subNode != null)
                {
                    settings.NetworkSettings.NewMessageSound = subNode.InnerText;
                }
            }

            // VobSub Ocr
            node = doc.DocumentElement.SelectSingleNode("VobSubOcr");
            subNode = node.SelectSingleNode("XOrMorePixelsMakesSpace");
            if (subNode != null)
            {
                settings.VobSubOcr.XOrMorePixelsMakesSpace = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AllowDifferenceInPercent");
            if (subNode != null)
            {
                settings.VobSubOcr.AllowDifferenceInPercent = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("BlurayAllowDifferenceInPercent");
            if (subNode != null)
            {
                settings.VobSubOcr.BlurayAllowDifferenceInPercent = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LastImageCompareFolder");
            if (subNode != null)
            {
                settings.VobSubOcr.LastImageCompareFolder = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("LastModiLanguageId");
            if (subNode != null)
            {
                settings.VobSubOcr.LastModiLanguageId = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LastOcrMethod");
            if (subNode != null)
            {
                settings.VobSubOcr.LastOcrMethod = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TesseractLastLanguage");
            if (subNode != null)
            {
                settings.VobSubOcr.TesseractLastLanguage = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("UseTesseractFallback");
            if (subNode != null)
            {
                settings.VobSubOcr.UseTesseractFallback = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("UseItalicsInTesseract");
            if (subNode != null)
            {
                settings.VobSubOcr.UseItalicsInTesseract = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("TesseractEngineMode");
            if (subNode != null)
            {
                settings.VobSubOcr.TesseractEngineMode = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("UseMusicSymbolsInTesseract");
            if (subNode != null)
            {
                settings.VobSubOcr.UseMusicSymbolsInTesseract = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("RightToLeft");
            if (subNode != null)
            {
                settings.VobSubOcr.RightToLeft = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("TopToBottom");
            if (subNode != null)
            {
                settings.VobSubOcr.TopToBottom = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("DefaultMillisecondsForUnknownDurations");
            if (subNode != null)
            {
                settings.VobSubOcr.DefaultMillisecondsForUnknownDurations = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("FixOcrErrors");
            if (subNode != null)
            {
                settings.VobSubOcr.FixOcrErrors = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("PromptForUnknownWords");
            if (subNode != null)
            {
                settings.VobSubOcr.PromptForUnknownWords = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GuessUnknownWords");
            if (subNode != null)
            {
                settings.VobSubOcr.GuessUnknownWords = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoBreakSubtitleIfMoreThanTwoLines");
            if (subNode != null)
            {
                settings.VobSubOcr.AutoBreakSubtitleIfMoreThanTwoLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ItalicFactor");
            if (subNode != null)
            {
                settings.VobSubOcr.ItalicFactor = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LineOcrDraw");
            if (subNode != null)
            {
                settings.VobSubOcr.LineOcrDraw = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LineOcrMinHeightSplit");
            if (subNode != null)
            {
                settings.VobSubOcr.LineOcrMinHeightSplit = Convert.ToInt32(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("LineOcrAdvancedItalic");
            if (subNode != null)
            {
                settings.VobSubOcr.LineOcrAdvancedItalic = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LineOcrLastLanguages");
            if (subNode != null)
            {
                settings.VobSubOcr.LineOcrLastLanguages = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("LineOcrLastSpellCheck");
            if (subNode != null)
            {
                settings.VobSubOcr.LineOcrLastSpellCheck = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("LineOcrLinesToAutoGuess");
            if (subNode != null)
            {
                settings.VobSubOcr.LineOcrLinesToAutoGuess = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LineOcrMinLineHeight");
            if (subNode != null)
            {
                settings.VobSubOcr.LineOcrMinLineHeight = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LineOcrMaxLineHeight");
            if (subNode != null)
            {
                settings.VobSubOcr.LineOcrMaxLineHeight = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LineOcrMaxErrorPixels");
            if (subNode != null)
            {
                settings.VobSubOcr.LineOcrMaxErrorPixels = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LastBinaryImageCompareDb");
            if (subNode != null)
            {
                settings.VobSubOcr.LastBinaryImageCompareDb = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("LastBinaryImageSpellCheck");
            if (subNode != null)
            {
                settings.VobSubOcr.LastBinaryImageSpellCheck = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BinaryAutoDetectBestDb");
            if (subNode != null)
            {
                settings.VobSubOcr.BinaryAutoDetectBestDb = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LastTesseractSpellCheck");
            if (subNode != null)
            {
                settings.VobSubOcr.LastTesseractSpellCheck = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("CaptureTopAlign");
            if (subNode != null)
            {
                settings.VobSubOcr.CaptureTopAlign = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("UnfocusedAttentionBlinkCount");
            if (subNode != null)
            {
                settings.VobSubOcr.UnfocusedAttentionBlinkCount = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("UnfocusedAttentionPlaySoundCount");
            if (subNode != null)
            {
                settings.VobSubOcr.UnfocusedAttentionPlaySoundCount = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("CloudVisionApiKey");
            if (subNode != null)
            {
                settings.VobSubOcr.CloudVisionApiKey = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("CloudVisionLanguage");
            if (subNode != null)
            {
                settings.VobSubOcr.CloudVisionLanguage = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("CloudVisionSendOriginalImages");
            if (subNode != null)
            {
                settings.VobSubOcr.CloudVisionSendOriginalImages = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            foreach (XmlNode groupNode in doc.DocumentElement.SelectNodes("MultipleSearchAndReplaceGroups/Group"))
            {
                var group = new MultipleSearchAndReplaceGroup
                {
                    Rules = new List<MultipleSearchAndReplaceSetting>()
                };
                subNode = groupNode.SelectSingleNode("Name");
                if (subNode != null)
                {
                    group.Name = subNode.InnerText;
                }

                subNode = groupNode.SelectSingleNode("Enabled");
                if (subNode != null)
                {
                    group.Enabled = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                settings.MultipleSearchAndReplaceGroups.Add(group);

                foreach (XmlNode listNode in groupNode.SelectNodes("Rule"))
                {
                    var item = new MultipleSearchAndReplaceSetting();
                    subNode = listNode.SelectSingleNode("Enabled");
                    if (subNode != null)
                    {
                        item.Enabled = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = listNode.SelectSingleNode("FindWhat");
                    if (subNode != null)
                    {
                        item.FindWhat = subNode.InnerText;
                    }

                    subNode = listNode.SelectSingleNode("ReplaceWith");
                    if (subNode != null)
                    {
                        item.ReplaceWith = subNode.InnerText;
                    }

                    subNode = listNode.SelectSingleNode("SearchType");
                    if (subNode != null)
                    {
                        item.SearchType = subNode.InnerText;
                    }

                    subNode = listNode.SelectSingleNode("Description");
                    if (subNode != null)
                    {
                        item.Description = subNode.InnerText;
                    }

                    group.Rules.Add(item);
                }
            }

            // Shortcuts
            ReadShortcuts(doc, settings.Shortcuts);

            // Remove text for Hearing Impaired
            node = doc.DocumentElement.SelectSingleNode("RemoveTextForHearingImpaired");
            if (node != null)
            {
                subNode = node.SelectSingleNode("RemoveTextBetweenBrackets");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenBrackets = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("RemoveTextBetweenParentheses");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenParentheses = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("RemoveTextBetweenCurlyBrackets");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenCurlyBrackets = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("RemoveTextBetweenQuestionMarks");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenQuestionMarks = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("RemoveTextBetweenCustom");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustom = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("RemoveTextBetweenCustomBefore");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomBefore = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("RemoveTextBetweenCustomAfter");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomAfter = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("RemoveTextBetweenOnlySeparateLines");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenOnlySeparateLines = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("RemoveTextBeforeColon");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBeforeColon = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("RemoveTextBeforeColonOnlyIfUppercase");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyIfUppercase = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("RemoveTextBeforeColonOnlyOnSeparateLine");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyOnSeparateLine = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("RemoveIfAllUppercase");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveIfAllUppercase = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("RemoveInterjections");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveInterjections = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("RemoveInterjectionsOnlyOnSeparateLine");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveInterjectionsOnlyOnSeparateLine = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("RemoveIfOnlyMusicSymbols");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveIfOnlyMusicSymbols = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("RemoveIfContainsText");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveIfContainsText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("RemoveIfContains");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveIfContains = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }
            }

            // Subtitle Beaming
            node = doc.DocumentElement.SelectSingleNode("SubtitleBeaming");
            if (node != null)
            {
                subNode = node.SelectSingleNode("FontName");
                if (subNode != null)
                {
                    settings.SubtitleBeaming.FontName = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("FontColor");
                if (subNode != null)
                {
                    settings.SubtitleBeaming.FontColor = Color.FromArgb(Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture));
                }

                subNode = node.SelectSingleNode("FontSize");
                if (subNode != null)
                {
                    settings.SubtitleBeaming.FontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("BorderColor");
                if (subNode != null)
                {
                    settings.SubtitleBeaming.BorderColor = Color.FromArgb(Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture));
                }

                subNode = node.SelectSingleNode("BorderWidth");
                if (subNode != null)
                {
                    settings.SubtitleBeaming.BorderWidth = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }
            }

            // Beautify time codes
            node = doc.DocumentElement.SelectSingleNode("BeautifyTimeCodes");
            if (node != null)
            {
                subNode = node.SelectSingleNode("AlignTimeCodes");
                if (subNode != null)
                {
                    settings.BeautifyTimeCodes.AlignTimeCodes = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("ExtractExactTimeCodes");
                if (subNode != null)
                {
                    settings.BeautifyTimeCodes.ExtractExactTimeCodes = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("SnapToShotChanges");
                if (subNode != null)
                {
                    settings.BeautifyTimeCodes.SnapToShotChanges = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("OverlapThreshold");
                if (subNode != null)
                {
                    settings.BeautifyTimeCodes.OverlapThreshold = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                var profileNode = node.SelectSingleNode("Profile");
                if (profileNode != null)
                {
                    subNode = profileNode.SelectSingleNode("Gap");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.Gap = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("InCuesGap");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.InCuesGap = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("InCuesLeftGreenZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.InCuesLeftGreenZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("InCuesLeftRedZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.InCuesLeftRedZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("InCuesRightRedZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.InCuesRightRedZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("InCuesRightGreenZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.InCuesRightGreenZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("OutCuesGap");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.OutCuesGap = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("OutCuesLeftGreenZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.OutCuesLeftGreenZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("OutCuesLeftRedZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.OutCuesLeftRedZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("OutCuesRightRedZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.OutCuesRightRedZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("OutCuesRightGreenZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.OutCuesRightGreenZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ConnectedSubtitlesInCueClosestLeftGap");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesInCueClosestLeftGap = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ConnectedSubtitlesInCueClosestRightGap");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesInCueClosestRightGap = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ConnectedSubtitlesOutCueClosestLeftGap");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesOutCueClosestLeftGap = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ConnectedSubtitlesOutCueClosestRightGap");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesOutCueClosestRightGap = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ConnectedSubtitlesLeftGreenZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesLeftGreenZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ConnectedSubtitlesLeftRedZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesLeftRedZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ConnectedSubtitlesRightRedZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesRightRedZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ConnectedSubtitlesRightGreenZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesRightGreenZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ConnectedSubtitlesTreatConnected");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesTreatConnected = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingGeneralUseZones");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingGeneralUseZones = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingGeneralMaxGap");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingGeneralMaxGap = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingGeneralLeftGreenZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftGreenZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingGeneralLeftRedZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftRedZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingGeneralShotChangeBehavior");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingGeneralShotChangeBehavior = (BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingShotChangeBehaviorEnum)Enum.Parse(typeof(BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingShotChangeBehaviorEnum), subNode.InnerText);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingInCueOnShotUseZones");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotUseZones = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingInCueOnShotMaxGap");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotMaxGap = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingInCueOnShotLeftGreenZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotLeftGreenZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingInCueOnShotLeftRedZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotLeftRedZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingInCueOnShotShotChangeBehavior");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotShotChangeBehavior = (BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingShotChangeBehaviorEnum)Enum.Parse(typeof(BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingShotChangeBehaviorEnum), subNode.InnerText);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingInCueOnShotCheckGeneral");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotCheckGeneral = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingOutCueOnShotUseZones");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotUseZones = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingOutCueOnShotMaxGap");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotMaxGap = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingOutCueOnShotRightRedZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotRightRedZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingOutCueOnShotRightGreenZone");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotRightGreenZone = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingOutCueOnShotShotChangeBehavior");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotShotChangeBehavior = (BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingShotChangeBehaviorEnum)Enum.Parse(typeof(BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingShotChangeBehaviorEnum), subNode.InnerText);
                    }

                    subNode = profileNode.SelectSingleNode("ChainingOutCueOnShotCheckGeneral");
                    if (subNode != null)
                    {
                        settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotCheckGeneral = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
                    }
                }
            }

            if (profileCount == 0)
            {
                settings.General.CurrentProfile = "Default";
                settings.General.Profiles = new List<RulesProfile>
                {
                    new RulesProfile
                    {
                        Name = settings.General.CurrentProfile,
                        SubtitleLineMaximumLength = settings.General.SubtitleLineMaximumLength,
                        MaxNumberOfLines = settings.General.MaxNumberOfLines,
                        MergeLinesShorterThan = settings.General.MergeLinesShorterThan,
                        SubtitleMaximumCharactersPerSeconds = (decimal)settings.General.SubtitleMaximumCharactersPerSeconds,
                        SubtitleOptimalCharactersPerSeconds = (decimal)settings.General.SubtitleOptimalCharactersPerSeconds,
                        SubtitleMaximumDisplayMilliseconds = settings.General.SubtitleMaximumDisplayMilliseconds,
                        SubtitleMinimumDisplayMilliseconds = settings.General.SubtitleMinimumDisplayMilliseconds,
                        SubtitleMaximumWordsPerMinute = (decimal)settings.General.SubtitleMaximumWordsPerMinute,
                        CpsLineLengthStrategy = settings.General.CpsLineLengthStrategy,
                        MinimumMillisecondsBetweenLines = settings.General.MinimumMillisecondsBetweenLines,
                        DialogStyle = settings.General.DialogStyle,
                        ContinuationStyle = settings.General.ContinuationStyle
                    }
                };
                GeneralSettings.AddExtraProfiles(settings.General.Profiles);
            }

            return settings;
        }

        internal static void ReadShortcuts(XmlDocument doc, Shortcuts shortcuts)
        {
            XmlNode node;
            XmlNode subNode;
            node = doc.DocumentElement.SelectSingleNode("Shortcuts");
            if (node != null)
            {
                foreach (XmlNode pluginNode in node.SelectNodes("Plugins/Plugin"))
                {
                    var nameNode = pluginNode.SelectSingleNode("Name");
                    var shortcutNode = pluginNode.SelectSingleNode("Shortcut");
                    if (nameNode != null && shortcutNode != null)
                    {
                        var name = nameNode.InnerText;
                        var shortcut = shortcutNode.InnerText;
                        shortcuts.PluginShortcuts.Add(new PluginShortcut { Name = name, Shortcut = shortcut });
                    }
                }

                subNode = node.SelectSingleNode("GeneralGoToFirstSelectedLine");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToFirstSelectedLine = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToNextEmptyLine");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToNextEmptyLine = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeSelectedLines");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeSelectedLines = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeSelectedLinesAndUnbreak");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeSelectedLinesAndUnbreak = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeSelectedLinesAndAutoBreak");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeSelectedLinesAndAutoBreak = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeSelectedLinesAndUnbreakCjk");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeSelectedLinesAndUnbreakCjk = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeSelectedLinesOnlyFirstText");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeSelectedLinesOnlyFirstText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeSelectedLinesBilingual");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeSelectedLinesBilingual = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeWithPreviousBilingual");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeWithPreviousBilingual = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeWithNextBilingual");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeWithNextBilingual = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeWithNext");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeWithNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeWithPrevious");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeWithPrevious = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeWithPreviousAndUnbreak");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeWithPreviousAndUnbreak = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeWithNextAndUnbreak");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeWithNextAndUnbreak = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeWithPreviousAndBreak");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeWithPreviousAndBreak = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeWithNextAndBreak");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeWithNextAndBreak = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralToggleTranslationMode");
                if (subNode != null)
                {
                    shortcuts.GeneralToggleTranslationMode = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralSwitchOriginalAndTranslation");
                if (subNode != null)
                {
                    shortcuts.GeneralSwitchOriginalAndTranslation = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralSwitchOriginalAndTranslationTextBoxes");
                if (subNode != null)
                {
                    shortcuts.GeneralSwitchOriginalAndTranslationTextBoxes = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralLayoutChoose");
                if (subNode != null)
                {
                    shortcuts.GeneralLayoutChoose = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralLayoutChoose1");
                if (subNode != null)
                {
                    shortcuts.GeneralLayoutChoose1 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralLayoutChoose2");
                if (subNode != null)
                {
                    shortcuts.GeneralLayoutChoose2 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralLayoutChoose3");
                if (subNode != null)
                {
                    shortcuts.GeneralLayoutChoose3 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralLayoutChoose4");
                if (subNode != null)
                {
                    shortcuts.GeneralLayoutChoose4 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralLayoutChoose5");
                if (subNode != null)
                {
                    shortcuts.GeneralLayoutChoose5 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralLayoutChoose6");
                if (subNode != null)
                {
                    shortcuts.GeneralLayoutChoose6 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralLayoutChoose7");
                if (subNode != null)
                {
                    shortcuts.GeneralLayoutChoose7 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralLayoutChoose8");
                if (subNode != null)
                {
                    shortcuts.GeneralLayoutChoose8 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralLayoutChoose9");
                if (subNode != null)
                {
                    shortcuts.GeneralLayoutChoose9 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralLayoutChoose10");
                if (subNode != null)
                {
                    shortcuts.GeneralLayoutChoose10 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralLayoutChoose11");
                if (subNode != null)
                {
                    shortcuts.GeneralLayoutChoose11 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralLayoutChoose12");
                if (subNode != null)
                {
                    shortcuts.GeneralLayoutChoose12 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeOriginalAndTranslation");
                if (subNode != null)
                {
                    shortcuts.GeneralMergeOriginalAndTranslation = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToNextSubtitle");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToNextSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToNextSubtitlePlayTranslate");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToNextSubtitlePlayTranslate = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToNextSubtitleCursorAtEnd");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToNextSubtitleCursorAtEnd = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToPrevSubtitle");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToPrevSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToPrevSubtitlePlayTranslate");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToPrevSubtitlePlayTranslate = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToEndOfCurrentSubtitle");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToEndOfCurrentSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToStartOfCurrentSubtitle");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToStartOfCurrentSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToNextSubtitleAndFocusVideo");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToNextSubtitleAndFocusVideo = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToPrevSubtitleAndPlay");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToPrevSubtitleAndPlay = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToNextSubtitleAndPlay");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToNextSubtitleAndPlay = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToPreviousSubtitleAndFocusVideo");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToPreviousSubtitleAndFocusVideo = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToPreviousSubtitleAndFocusWaveform");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToPreviousSubtitleAndFocusWaveform = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToNextSubtitleAndFocusWaveform");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToNextSubtitleAndFocusWaveform = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralAutoCalcCurrentDuration");
                if (subNode != null)
                {
                    shortcuts.GeneralAutoCalcCurrentDuration = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralAutoCalcCurrentDurationByOptimalReadingSpeed");
                if (subNode != null)
                {
                    shortcuts.GeneralAutoCalcCurrentDurationByOptimalReadingSpeed = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralAutoCalcCurrentDurationByMinReadingSpeed");
                if (subNode != null)
                {
                    shortcuts.GeneralAutoCalcCurrentDurationByMinReadingSpeed = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralPlayFirstSelected");
                if (subNode != null)
                {
                    shortcuts.GeneralPlayFirstSelected = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralToggleBookmarks");
                if (subNode != null)
                {
                    shortcuts.GeneralToggleBookmarks = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralToggleBookmarksWithText");
                if (subNode != null)
                {
                    shortcuts.GeneralToggleBookmarksWithText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralEditBookmarks");
                if (subNode != null)
                {
                    shortcuts.GeneralEditBookmarks = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralClearBookmarks");
                if (subNode != null)
                {
                    shortcuts.GeneralClearBookmarks = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToBookmark");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToBookmark = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToPreviousBookmark");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToPreviousBookmark = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToNextBookmark");
                if (subNode != null)
                {
                    shortcuts.GeneralGoToNextBookmark = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralChooseProfile");
                if (subNode != null)
                {
                    shortcuts.GeneralChooseProfile = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("OpenDataFolder");
                if (subNode != null)
                {
                    shortcuts.OpenDataFolder = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("OpenContainingFolder");
                if (subNode != null)
                {
                    shortcuts.OpenContainingFolder = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralDuplicateLine");
                if (subNode != null)
                {
                    shortcuts.GeneralDuplicateLine = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralToggleView");
                if (subNode != null)
                {
                    shortcuts.GeneralToggleView = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralToggleMode");
                if (subNode != null)
                {
                    shortcuts.GeneralToggleMode = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralTogglePreviewOnVideo");
                if (subNode != null)
                {
                    shortcuts.GeneralTogglePreviewOnVideo = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralHelp");
                if (subNode != null)
                {
                    shortcuts.GeneralHelp = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralFocusTextBox");
                if (subNode != null)
                {
                    shortcuts.GeneralFocusTextBox = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileNew");
                if (subNode != null)
                {
                    shortcuts.MainFileNew = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileOpen");
                if (subNode != null)
                {
                    shortcuts.MainFileOpen = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileOpenKeepVideo");
                if (subNode != null)
                {
                    shortcuts.MainFileOpenKeepVideo = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileSave");
                if (subNode != null)
                {
                    shortcuts.MainFileSave = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileSaveOriginal");
                if (subNode != null)
                {
                    shortcuts.MainFileSaveOriginal = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileSaveOriginalAs");
                if (subNode != null)
                {
                    shortcuts.MainFileSaveOriginalAs = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileSaveAs");
                if (subNode != null)
                {
                    shortcuts.MainFileSaveAs = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileSaveAll");
                if (subNode != null)
                {
                    shortcuts.MainFileSaveAll = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileCloseOriginal");
                if (subNode != null)
                {
                    shortcuts.MainFileCloseOriginal = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileCloseTranslation");
                if (subNode != null)
                {
                    shortcuts.MainFileCloseTranslation = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileCompare");
                if (subNode != null)
                {
                    shortcuts.MainFileCompare = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileVerifyCompleteness");
                if (subNode != null)
                {
                    shortcuts.MainFileVerifyCompleteness = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileOpenOriginal");
                if (subNode != null)
                {
                    shortcuts.MainFileOpenOriginal = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileImportPlainText");
                if (subNode != null)
                {
                    shortcuts.MainFileImportPlainText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileImportBdSupForEdit");
                if (subNode != null)
                {
                    shortcuts.MainFileImportBdSupForEdit = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileImportTimeCodes");
                if (subNode != null)
                {
                    shortcuts.MainFileImportTimeCodes = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileExportEbu");
                if (subNode != null)
                {
                    shortcuts.MainFileExportEbu = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileExportPac");
                if (subNode != null)
                {
                    shortcuts.MainFileExportPac = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileExportBdSup");
                if (subNode != null)
                {
                    shortcuts.MainFileExportBdSup = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileExportEdlClip");
                if (subNode != null)
                {
                    shortcuts.MainFileExportEdlClip = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileExportPlainText");
                if (subNode != null)
                {
                    shortcuts.MainFileExportPlainText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileExit");
                if (subNode != null)
                {
                    shortcuts.MainFileExit = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditUndo");
                if (subNode != null)
                {
                    shortcuts.MainEditUndo = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditRedo");
                if (subNode != null)
                {
                    shortcuts.MainEditRedo = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditFind");
                if (subNode != null)
                {
                    shortcuts.MainEditFind = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditFindNext");
                if (subNode != null)
                {
                    shortcuts.MainEditFindNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditReplace");
                if (subNode != null)
                {
                    shortcuts.MainEditReplace = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditMultipleReplace");
                if (subNode != null)
                {
                    shortcuts.MainEditMultipleReplace = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditGoToLineNumber");
                if (subNode != null)
                {
                    shortcuts.MainEditGoToLineNumber = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditRightToLeft");
                if (subNode != null)
                {
                    shortcuts.MainEditRightToLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsAdjustDuration");
                if (subNode != null)
                {
                    shortcuts.MainToolsAdjustDuration = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsAdjustDurationLimits");
                if (subNode != null)
                {
                    shortcuts.MainToolsAdjustDurationLimits = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsFixCommonErrors");
                if (subNode != null)
                {
                    shortcuts.MainToolsFixCommonErrors = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsFixCommonErrorsPreview");
                if (subNode != null)
                {
                    shortcuts.MainToolsFixCommonErrorsPreview = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsMergeShortLines");
                if (subNode != null)
                {
                    shortcuts.MainToolsMergeShortLines = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsMergeDuplicateText");
                if (subNode != null)
                {
                    shortcuts.MainToolsMergeDuplicateText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsMergeSameTimeCodes");
                if (subNode != null)
                {
                    shortcuts.MainToolsMergeSameTimeCodes = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsMakeEmptyFromCurrent");
                if (subNode != null)
                {
                    shortcuts.MainToolsMakeEmptyFromCurrent = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsSplitLongLines");
                if (subNode != null)
                {
                    shortcuts.MainToolsSplitLongLines = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsDurationsBridgeGap");
                if (subNode != null)
                {
                    shortcuts.MainToolsDurationsBridgeGap = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsMinimumDisplayTimeBetweenParagraphs");
                if (subNode != null)
                {
                    shortcuts.MainToolsMinimumDisplayTimeBetweenParagraphs = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsRenumber");
                if (subNode != null)
                {
                    shortcuts.MainToolsRenumber = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsRemoveTextForHI");
                if (subNode != null)
                {
                    shortcuts.MainToolsRemoveTextForHI = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsConvertColorsToDialog");
                if (subNode != null)
                {
                    shortcuts.MainToolsConvertColorsToDialog = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsChangeCasing");
                if (subNode != null)
                {
                    shortcuts.MainToolsChangeCasing = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsAutoDuration");
                if (subNode != null)
                {
                    shortcuts.MainToolsAutoDuration = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsBatchConvert");
                if (subNode != null)
                {
                    shortcuts.MainToolsBatchConvert = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsMeasurementConverter");
                if (subNode != null)
                {
                    shortcuts.MainToolsMeasurementConverter = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsSplit");
                if (subNode != null)
                {
                    shortcuts.MainToolsSplit = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsAppend");
                if (subNode != null)
                {
                    shortcuts.MainToolsAppend = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsJoin");
                if (subNode != null)
                {
                    shortcuts.MainToolsJoin = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsStyleManager");
                if (subNode != null)
                {
                    shortcuts.MainToolsStyleManager = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditToggleTranslationOriginalInPreviews");
                if (subNode != null)
                {
                    shortcuts.MainEditToggleTranslationOriginalInPreviews = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditInverseSelection");
                if (subNode != null)
                {
                    shortcuts.MainEditInverseSelection = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditModifySelection");
                if (subNode != null)
                {
                    shortcuts.MainEditModifySelection = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoOpen");
                if (subNode != null)
                {
                    shortcuts.MainVideoOpen = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoClose");
                if (subNode != null)
                {
                    shortcuts.MainVideoClose = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoPause");
                if (subNode != null)
                {
                    shortcuts.MainVideoPause = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoStop");
                if (subNode != null)
                {
                    shortcuts.MainVideoStop = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoPlayFromJustBefore");
                if (subNode != null)
                {
                    shortcuts.MainVideoPlayFromJustBefore = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoPlayFromBeginning");
                if (subNode != null)
                {
                    shortcuts.MainVideoPlayFromBeginning = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoPlayPauseToggle");
                if (subNode != null)
                {
                    shortcuts.MainVideoPlayPauseToggle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoPlay150Speed");
                if (subNode != null)
                {
                    shortcuts.MainVideoPlay150Speed = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoPlay200Speed");
                if (subNode != null)
                {
                    shortcuts.MainVideoPlay200Speed = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoFocusSetVideoPosition");
                if (subNode != null)
                {
                    shortcuts.MainVideoFocusSetVideoPosition = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoToggleVideoControls");
                if (subNode != null)
                {
                    shortcuts.MainVideoToggleVideoControls = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo1FrameLeft");
                if (subNode != null)
                {
                    shortcuts.MainVideo1FrameLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo1FrameRight");
                if (subNode != null)
                {
                    shortcuts.MainVideo1FrameRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo1FrameLeftWithPlay");
                if (subNode != null)
                {
                    shortcuts.MainVideo1FrameLeftWithPlay = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo1FrameRightWithPlay");
                if (subNode != null)
                {
                    shortcuts.MainVideo1FrameRightWithPlay = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo100MsLeft");
                if (subNode != null)
                {
                    shortcuts.MainVideo100MsLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo100MsRight");
                if (subNode != null)
                {
                    shortcuts.MainVideo100MsRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo500MsLeft");
                if (subNode != null)
                {
                    shortcuts.MainVideo500MsLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo500MsRight");
                if (subNode != null)
                {
                    shortcuts.MainVideo500MsRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo1000MsLeft");
                if (subNode != null)
                {
                    shortcuts.MainVideo1000MsLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo1000MsRight");
                if (subNode != null)
                {
                    shortcuts.MainVideo1000MsRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo5000MsLeft");
                if (subNode != null)
                {
                    shortcuts.MainVideo5000MsLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo5000MsRight");
                if (subNode != null)
                {
                    shortcuts.MainVideo5000MsRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoXSMsLeft");
                if (subNode != null)
                {
                    shortcuts.MainVideoXSMsLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoXSMsRight");
                if (subNode != null)
                {
                    shortcuts.MainVideoXSMsRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoXLMsLeft");
                if (subNode != null)
                {
                    shortcuts.MainVideoXLMsLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoXLMsRight");
                if (subNode != null)
                {
                    shortcuts.MainVideoXLMsRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo3000MsLeft");
                if (subNode != null)
                {
                    shortcuts.MainVideo3000MsLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo3000MsRight");
                if (subNode != null)
                {
                    shortcuts.MainVideo3000MsRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoGoToStartCurrent");
                if (subNode != null)
                {
                    shortcuts.MainVideoGoToStartCurrent = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoToggleStartEndCurrent");
                if (subNode != null)
                {
                    shortcuts.MainVideoToggleStartEndCurrent = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoPlaySelectedLines");
                if (subNode != null)
                {
                    shortcuts.MainVideoPlaySelectedLines = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoLoopSelectedLines");
                if (subNode != null)
                {
                    shortcuts.MainVideoLoopSelectedLines = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoGoToPrevSubtitle");
                if (subNode != null)
                {
                    shortcuts.MainVideoGoToPrevSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoGoToNextSubtitle");
                if (subNode != null)
                {
                    shortcuts.MainVideoGoToNextSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoGoToPrevTimeCode");
                if (subNode != null)
                {
                    shortcuts.MainVideoGoToPrevTimeCode = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoGoToNextTimeCode");
                if (subNode != null)
                {
                    shortcuts.MainVideoGoToNextTimeCode = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoGoToPrevChapter");
                if (subNode != null)
                {
                    shortcuts.MainVideoGoToPrevChapter = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoGoToNextChapter");
                if (subNode != null)
                {
                    shortcuts.MainVideoGoToNextChapter = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoSelectNextSubtitle");
                if (subNode != null)
                {
                    shortcuts.MainVideoSelectNextSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoFullscreen");
                if (subNode != null)
                {
                    shortcuts.MainVideoFullscreen = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoSlower");
                if (subNode != null)
                {
                    shortcuts.MainVideoSlower = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoFaster");
                if (subNode != null)
                {
                    shortcuts.MainVideoFaster = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoSpeedToggle");
                if (subNode != null)
                {
                    shortcuts.MainVideoSpeedToggle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoReset");
                if (subNode != null)
                {
                    shortcuts.MainVideoReset = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoToggleBrightness");
                if (subNode != null)
                {
                    shortcuts.MainVideoToggleBrightness = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoToggleContrast");
                if (subNode != null)
                {
                    shortcuts.MainVideoToggleContrast = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoAudioToTextVosk");
                if (subNode != null)
                {
                    shortcuts.MainVideoAudioToTextVosk = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoAudioToTextWhisper");
                if (subNode != null)
                {
                    shortcuts.MainVideoAudioToTextWhisper = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoAudioExtractAudioSelectedLines");
                if (subNode != null)
                {
                    shortcuts.MainVideoAudioExtractAudioSelectedLines = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoTextToSpeech");
                if (subNode != null)
                {
                    shortcuts.MainVideoTextToSpeech = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSpellCheck");
                if (subNode != null)
                {
                    shortcuts.MainSpellCheck = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSpellCheckFindDoubleWords");
                if (subNode != null)
                {
                    shortcuts.MainSpellCheckFindDoubleWords = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSpellCheckAddWordToNames");
                if (subNode != null)
                {
                    shortcuts.MainSpellCheckAddWordToNames = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSynchronizationAdjustTimes");
                if (subNode != null)
                {
                    shortcuts.MainSynchronizationAdjustTimes = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSynchronizationVisualSync");
                if (subNode != null)
                {
                    shortcuts.MainSynchronizationVisualSync = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSynchronizationPointSync");
                if (subNode != null)
                {
                    shortcuts.MainSynchronizationPointSync = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSynchronizationPointSyncViaFile");
                if (subNode != null)
                {
                    shortcuts.MainSynchronizationPointSyncViaFile = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSynchronizationChangeFrameRate");
                if (subNode != null)
                {
                    shortcuts.MainSynchronizationChangeFrameRate = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewItalic");
                if (subNode != null)
                {
                    shortcuts.MainListViewItalic = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewBold");
                if (subNode != null)
                {
                    shortcuts.MainListViewBold = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewUnderline");
                if (subNode != null)
                {
                    shortcuts.MainListViewUnderline = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewBox");
                if (subNode != null)
                {
                    shortcuts.MainListViewBox = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewToggleQuotes");
                if (subNode != null)
                {
                    shortcuts.MainListViewToggleQuotes = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewToggleHiTags");
                if (subNode != null)
                {
                    shortcuts.MainListViewToggleHiTags = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewToggleCustomTags");
                if (subNode != null)
                {
                    shortcuts.MainListViewToggleCustomTags = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSplit");
                if (subNode != null)
                {
                    shortcuts.MainListViewSplit = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewToggleDashes");
                if (subNode != null)
                {
                    shortcuts.MainListViewToggleDashes = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewToggleMusicSymbols");
                if (subNode != null)
                {
                    shortcuts.MainListViewToggleMusicSymbols = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignment");
                if (subNode != null)
                {
                    shortcuts.MainListViewAlignment = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN1");
                if (subNode != null)
                {
                    shortcuts.MainListViewAlignmentN1 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN1");
                if (subNode != null)
                {
                    shortcuts.MainListViewAlignmentN1 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN2");
                if (subNode != null)
                {
                    shortcuts.MainListViewAlignmentN2 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN3");
                if (subNode != null)
                {
                    shortcuts.MainListViewAlignmentN3 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN4");
                if (subNode != null)
                {
                    shortcuts.MainListViewAlignmentN4 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN5");
                if (subNode != null)
                {
                    shortcuts.MainListViewAlignmentN5 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN6");
                if (subNode != null)
                {
                    shortcuts.MainListViewAlignmentN6 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN7");
                if (subNode != null)
                {
                    shortcuts.MainListViewAlignmentN7 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN8");
                if (subNode != null)
                {
                    shortcuts.MainListViewAlignmentN8 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN9");
                if (subNode != null)
                {
                    shortcuts.MainListViewAlignmentN9 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColor1");
                if (subNode != null)
                {
                    shortcuts.MainListViewColor1 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColor2");
                if (subNode != null)
                {
                    shortcuts.MainListViewColor2 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColor3");
                if (subNode != null)
                {
                    shortcuts.MainListViewColor3 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColor4");
                if (subNode != null)
                {
                    shortcuts.MainListViewColor4 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColor5");
                if (subNode != null)
                {
                    shortcuts.MainListViewColor5 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColor6");
                if (subNode != null)
                {
                    shortcuts.MainListViewColor6 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColor7");
                if (subNode != null)
                {
                    shortcuts.MainListViewColor7 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColor8");
                if (subNode != null)
                {
                    shortcuts.MainListViewColor8 = subNode.InnerText;
                }


                subNode = node.SelectSingleNode("MainListViewSetNewActor");
                if (subNode != null)
                {
                    shortcuts.MainListViewSetNewActor = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSetActor1");
                if (subNode != null)
                {
                    shortcuts.MainListViewSetActor1 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSetActor2");
                if (subNode != null)
                {
                    shortcuts.MainListViewSetActor2 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSetActor3");
                if (subNode != null)
                {
                    shortcuts.MainListViewSetActor3 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSetActor4");
                if (subNode != null)
                {
                    shortcuts.MainListViewSetActor4 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSetActor5");
                if (subNode != null)
                {
                    shortcuts.MainListViewSetActor5 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSetActor6");
                if (subNode != null)
                {
                    shortcuts.MainListViewSetActor6 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSetActor7");
                if (subNode != null)
                {
                    shortcuts.MainListViewSetActor7 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSetActor8");
                if (subNode != null)
                {
                    shortcuts.MainListViewSetActor8 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSetActor9");
                if (subNode != null)
                {
                    shortcuts.MainListViewSetActor9 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSetActor10");
                if (subNode != null)
                {
                    shortcuts.MainListViewSetActor10 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColorChoose");
                if (subNode != null)
                {
                    shortcuts.MainListViewColorChoose = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainRemoveFormatting");
                if (subNode != null)
                {
                    shortcuts.MainRemoveFormatting = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewCopyText");
                if (subNode != null)
                {
                    shortcuts.MainListViewCopyText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewCopyPlainText");
                if (subNode != null)
                {
                    shortcuts.MainListViewCopyPlainText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewCopyTextFromOriginalToCurrent");
                if (subNode != null)
                {
                    shortcuts.MainListViewCopyTextFromOriginalToCurrent = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAutoDuration");
                if (subNode != null)
                {
                    shortcuts.MainListViewAutoDuration = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColumnDeleteText");
                if (subNode != null)
                {
                    shortcuts.MainListViewColumnDeleteText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColumnDeleteTextAndShiftUp");
                if (subNode != null)
                {
                    shortcuts.MainListViewColumnDeleteTextAndShiftUp = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColumnInsertText");
                if (subNode != null)
                {
                    shortcuts.MainListViewColumnInsertText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColumnPaste");
                if (subNode != null)
                {
                    shortcuts.MainListViewColumnPaste = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColumnTextUp");
                if (subNode != null)
                {
                    shortcuts.MainListViewColumnTextUp = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColumnTextDown");
                if (subNode != null)
                {
                    shortcuts.MainListViewColumnTextDown = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewGoToNextError");
                if (subNode != null)
                {
                    shortcuts.MainListViewGoToNextError = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewListErrors");
                if (subNode != null)
                {
                    shortcuts.MainListViewListErrors = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSortByNumber");
                if (subNode != null)
                {
                    shortcuts.MainListViewSortByNumber = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSortByStartTime");
                if (subNode != null)
                {
                    shortcuts.MainListViewSortByStartTime = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSortByEndTime");
                if (subNode != null)
                {
                    shortcuts.MainListViewSortByEndTime = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSortByDuration");
                if (subNode != null)
                {
                    shortcuts.MainListViewSortByDuration = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSortByGap");
                if (subNode != null)
                {
                    shortcuts.MainListViewSortByGap = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSortByText");
                if (subNode != null)
                {
                    shortcuts.MainListViewSortByText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSortBySingleLineMaxLen");
                if (subNode != null)
                {
                    shortcuts.MainListViewSortBySingleLineMaxLen = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSortByTextTotalLength");
                if (subNode != null)
                {
                    shortcuts.MainListViewSortByTextTotalLength = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSortByCps");
                if (subNode != null)
                {
                    shortcuts.MainListViewSortByCps = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSortByWpm");
                if (subNode != null)
                {
                    shortcuts.MainListViewSortByWpm = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSortByNumberOfLines");
                if (subNode != null)
                {
                    shortcuts.MainListViewSortByNumberOfLines = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSortByActor");
                if (subNode != null)
                {
                    shortcuts.MainListViewSortByActor = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewSortByStyle");
                if (subNode != null)
                {
                    shortcuts.MainListViewSortByStyle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralRemoveBlankLines");
                if (subNode != null)
                {
                    shortcuts.GeneralRemoveBlankLines = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralApplyAssaOverrideTags");
                if (subNode != null)
                {
                    shortcuts.GeneralApplyAssaOverrideTags = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralSetAssaPosition");
                if (subNode != null)
                {
                    shortcuts.GeneralSetAssaPosition = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralSetAssaResolution");
                if (subNode != null)
                {
                    shortcuts.GeneralSetAssaResolution = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralSetAssaBgBox");
                if (subNode != null)
                {
                    shortcuts.GeneralSetAssaBgBox = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralColorPicker");
                if (subNode != null)
                {
                    shortcuts.GeneralColorPicker = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralTakeAutoBackup");
                if (subNode != null)
                {
                    shortcuts.GeneralTakeAutoBackup = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewRemoveTimeCodes");
                if (subNode != null)
                {
                    shortcuts.MainListViewRemoveTimeCodes = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditFixRTLViaUnicodeChars");
                if (subNode != null)
                {
                    shortcuts.MainEditFixRTLViaUnicodeChars = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditRemoveRTLUnicodeChars");
                if (subNode != null)
                {
                    shortcuts.MainEditRemoveRTLUnicodeChars = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditReverseStartAndEndingForRTL");
                if (subNode != null)
                {
                    shortcuts.MainEditReverseStartAndEndingForRTL = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoToggleControls");
                if (subNode != null)
                {
                    shortcuts.MainVideoToggleControls = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxSplitAtCursor");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxSplitAtCursor = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxSplitAtCursorAndAutoBr");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxSplitAtCursorAndAutoBr = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxSplitAtCursorAndVideoPos");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxSplitAtCursorAndVideoPos = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxSplitSelectedLineBilingual");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxSplitSelectedLineBilingual = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxMoveLastWordDown");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxMoveLastWordDown = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxMoveFirstWordFromNextUp");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxMoveFirstWordFromNextUp = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxMoveLastWordDownCurrent");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxMoveLastWordDownCurrent = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxMoveFirstWordUpCurrent");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxMoveFirstWordUpCurrent = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxMoveFromCursorToNext");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxMoveFromCursorToNextAndGoToNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxSelectionToLower");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxSelectionToLower = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxSelectionToUpper");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxSelectionToUpper = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxSelectionToggleCasing");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxSelectionToggleCasing = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxSelectionToRuby");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxSelectionToRuby = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxToggleAutoDuration");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxToggleAutoDuration = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainCreateInsertSubAtVideoPos");
                if (subNode != null)
                {
                    shortcuts.MainCreateInsertSubAtVideoPos = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainCreateInsertSubAtVideoPosMax");
                if (subNode != null)
                {
                    shortcuts.MainCreateInsertSubAtVideoPosMax = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainCreateInsertSubAtVideoPosNoTextBoxFocus");
                if (subNode != null)
                {
                    shortcuts.MainCreateInsertSubAtVideoPosNoTextBoxFocus = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainCreateSetStart");
                if (subNode != null)
                {
                    shortcuts.MainCreateSetStart = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainCreateSetEnd");
                if (subNode != null)
                {
                    shortcuts.MainCreateSetEnd = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustVideoSetStartForAppropriateLine");
                if (subNode != null)
                {
                    shortcuts.MainAdjustVideoSetStartForAppropriateLine = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustVideoSetEndForAppropriateLine");
                if (subNode != null)
                {
                    shortcuts.MainAdjustVideoSetEndForAppropriateLine = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetEndAndPause");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSetEndAndPause = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainCreateSetEndAddNewAndGoToNew");
                if (subNode != null)
                {
                    shortcuts.MainCreateSetEndAddNewAndGoToNew = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainCreateStartDownEndUp");
                if (subNode != null)
                {
                    shortcuts.MainCreateStartDownEndUp = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetStartAndOffsetTheRest");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSetStartAndOffsetTheRest = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetStartAndOffsetTheRest2");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSetStartAndOffsetTheRest2 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetStartAndOffsetTheWholeSubtitle");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSetStartAndOffsetTheWholeSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetEndAndOffsetTheRest");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSetEndAndOffsetTheRest = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetEndAndOffsetTheRestAndGoToNext");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSetEndAndOffsetTheRestAndGoToNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetStartAndGotoNext");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSetStartAndGotoNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetEndAndGotoNext");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSetEndAndGotoNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustViaEndAutoStart");
                if (subNode != null)
                {
                    shortcuts.MainAdjustViaEndAutoStart = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustViaEndAutoStartAndGoToNext");
                if (subNode != null)
                {
                    shortcuts.MainAdjustViaEndAutoStartAndGoToNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetEndMinusGapAndStartNextHere");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSetEndMinusGapAndStartNextHere = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSetEndAndStartNextAfterGap");
                if (subNode != null)
                {
                    shortcuts.MainSetEndAndStartNextAfterGap = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetStartAutoDurationAndGoToNext");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSetStartAutoDurationAndGoToNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetEndNextStartAndGoToNext");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSetEndNextStartAndGoToNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustStartDownEndUpAndGoToNext");
                if (subNode != null)
                {
                    shortcuts.MainAdjustStartDownEndUpAndGoToNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetStartAndEndOfPrevious");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSetStartAndEndOfPrevious = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetStartAndEndOfPreviousAndGoToNext");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSetStartAndEndOfPreviousAndGoToNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetStartKeepDuration");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSetStartKeepDuration = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSelected100MsForward");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSelected100MsForward = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSelected100MsBack");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSelected100MsBack = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustStartXMsBack");
                if (subNode != null)
                {
                    shortcuts.MainAdjustStartXMsBack = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustStartXMsForward");
                if (subNode != null)
                {
                    shortcuts.MainAdjustStartXMsForward = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustEndXMsBack");
                if (subNode != null)
                {
                    shortcuts.MainAdjustEndXMsBack = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustEndXMsForward");
                if (subNode != null)
                {
                    shortcuts.MainAdjustEndXMsForward = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MoveStartOneFrameBack");
                if (subNode != null)
                {
                    shortcuts.MoveStartOneFrameBack = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MoveStartOneFrameForward");
                if (subNode != null)
                {
                    shortcuts.MoveStartOneFrameForward = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MoveEndOneFrameBack");
                if (subNode != null)
                {
                    shortcuts.MoveEndOneFrameBack = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MoveEndOneFrameForward");
                if (subNode != null)
                {
                    shortcuts.MoveEndOneFrameForward = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MoveStartOneFrameBackKeepGapPrev");
                if (subNode != null)
                {
                    shortcuts.MoveStartOneFrameBackKeepGapPrev = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MoveStartOneFrameForwardKeepGapPrev");
                if (subNode != null)
                {
                    shortcuts.MoveStartOneFrameForwardKeepGapPrev = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MoveEndOneFrameBackKeepGapNext");
                if (subNode != null)
                {
                    shortcuts.MoveEndOneFrameBackKeepGapNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MoveEndOneFrameForwardKeepGapNext");
                if (subNode != null)
                {
                    shortcuts.MoveEndOneFrameForwardKeepGapNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSnapStartToNextShotChange");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSnapStartToNextShotChange = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSnapEndToPreviousShotChange");
                if (subNode != null)
                {
                    shortcuts.MainAdjustSnapEndToPreviousShotChange = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustExtendToNextShotChange");
                if (subNode != null)
                {
                    shortcuts.MainAdjustExtendToNextShotChange = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustExtendToPreviousShotChange");
                if (subNode != null)
                {
                    shortcuts.MainAdjustExtendToPreviousShotChange = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustExtendToNextSubtitle");
                if (subNode != null)
                {
                    shortcuts.MainAdjustExtendToNextSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustExtendToPreviousSubtitle");
                if (subNode != null)
                {
                    shortcuts.MainAdjustExtendToPreviousSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustExtendToNextSubtitleMinusChainingGap");
                if (subNode != null)
                {
                    shortcuts.MainAdjustExtendToNextSubtitleMinusChainingGap = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustExtendToPreviousSubtitleMinusChainingGap");
                if (subNode != null)
                {
                    shortcuts.MainAdjustExtendToPreviousSubtitleMinusChainingGap = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustExtendCurrentSubtitle");
                if (subNode != null)
                {
                    shortcuts.MainAdjustExtendCurrentSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustExtendPreviousLineEndToCurrentStart");
                if (subNode != null)
                {
                    shortcuts.MainAdjustExtendPreviousLineEndToCurrentStart = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustExtendNextLineStartToCurrentEnd");
                if (subNode != null)
                {
                    shortcuts.MainAdjustExtendNextLineStartToCurrentEnd = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSetInCueToClosestShotChangeLeftGreenZone");
                if (subNode != null)
                {
                    shortcuts.MainSetInCueToClosestShotChangeLeftGreenZone = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSetInCueToClosestShotChangeRightGreenZone");
                if (subNode != null)
                {
                    shortcuts.MainSetInCueToClosestShotChangeRightGreenZone = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSetOutCueToClosestShotChangeLeftGreenZone");
                if (subNode != null)
                {
                    shortcuts.MainSetOutCueToClosestShotChangeLeftGreenZone = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSetOutCueToClosestShotChangeRightGreenZone");
                if (subNode != null)
                {
                    shortcuts.MainSetOutCueToClosestShotChangeRightGreenZone = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainInsertAfter");
                if (subNode != null)
                {
                    shortcuts.MainInsertAfter = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxAutoBreak");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxAutoBreak = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxBreakAtPosition");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxBreakAtPosition = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxBreakAtPositionAndGoToNext");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxBreakAtPositionAndGoToNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxRecord");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxRecord = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxUnbreak");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxUnbreak = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxUnbrekNoSpace");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxUnbreakNoSpace = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxAssaIntellisense");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxAssaIntellisense = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxAssaRemoveTag");
                if (subNode != null)
                {
                    shortcuts.MainTextBoxAssaRemoveTag = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainWaveformInsertAtCurrentPosition");
                if (subNode != null)
                {
                    shortcuts.MainWaveformInsertAtCurrentPosition = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainInsertBefore");
                if (subNode != null)
                {
                    shortcuts.MainInsertBefore = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainMergeDialog");
                if (subNode != null)
                {
                    shortcuts.MainMergeDialog = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainMergeDialogWithNext");
                if (subNode != null)
                {
                    shortcuts.MainMergeDialogWithNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainMergeDialogWithPrevious");
                if (subNode != null)
                {
                    shortcuts.MainMergeDialogWithPrevious = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAutoBalanceSelectedLines");
                if (subNode != null)
                {
                    shortcuts.MainAutoBalanceSelectedLines = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEvenlyDistributeSelectedLines");
                if (subNode != null)
                {
                    shortcuts.MainEvenlyDistributeSelectedLines = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToggleFocus");
                if (subNode != null)
                {
                    shortcuts.MainToggleFocus = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToggleFocusWaveform");
                if (subNode != null)
                {
                    shortcuts.MainToggleFocusWaveform = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToggleFocusWaveformTextBox");
                if (subNode != null)
                {
                    shortcuts.MainToggleFocusWaveformTextBox = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformAdd");
                if (subNode != null)
                {
                    shortcuts.WaveformAdd = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformVerticalZoom");
                if (subNode != null)
                {
                    shortcuts.WaveformVerticalZoom = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformVerticalZoomOut");
                if (subNode != null)
                {
                    shortcuts.WaveformVerticalZoomOut = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformZoomIn");
                if (subNode != null)
                {
                    shortcuts.WaveformZoomIn = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformZoomOut");
                if (subNode != null)
                {
                    shortcuts.WaveformZoomOut = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformSplit");
                if (subNode != null)
                {
                    shortcuts.WaveformSplit = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformPlaySelection");
                if (subNode != null)
                {
                    shortcuts.WaveformPlaySelection = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformPlaySelectionEnd");
                if (subNode != null)
                {
                    shortcuts.WaveformPlaySelectionEnd = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformSearchSilenceForward");
                if (subNode != null)
                {
                    shortcuts.WaveformSearchSilenceForward = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformSearchSilenceBack");
                if (subNode != null)
                {
                    shortcuts.WaveformSearchSilenceBack = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformAddTextHere");
                if (subNode != null)
                {
                    shortcuts.WaveformAddTextHere = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformAddTextHereFromClipboard");
                if (subNode != null)
                {
                    shortcuts.WaveformAddTextHereFromClipboard = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformSetParagraphAsSelection");
                if (subNode != null)
                {
                    shortcuts.WaveformSetParagraphAsSelection = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformGoToPreviousShotChange");
                if (subNode != null)
                {
                    shortcuts.WaveformGoToPreviousShotChange = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformGoToNextShotChange");
                if (subNode != null)
                {
                    shortcuts.WaveformGoToNextShotChange = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformToggleShotChange");
                if (subNode != null)
                {
                    shortcuts.WaveformToggleShotChange = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformListShotChanges");
                if (subNode != null)
                {
                    shortcuts.WaveformListShotChanges = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformGuessStart");
                if (subNode != null)
                {
                    shortcuts.WaveformGuessStart = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("Waveform100MsLeft");
                if (subNode != null)
                {
                    shortcuts.Waveform100MsLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("Waveform100MsRight");
                if (subNode != null)
                {
                    shortcuts.Waveform100MsRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("Waveform1000MsLeft");
                if (subNode != null)
                {
                    shortcuts.Waveform1000MsLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("Waveform1000MsRight");
                if (subNode != null)
                {
                    shortcuts.Waveform1000MsRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformAudioToTextVosk");
                if (subNode != null)
                {
                    shortcuts.WaveformAudioToTextVosk = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformAudioToTextWhisper");
                if (subNode != null)
                {
                    shortcuts.WaveformAudioToTextWhisper = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainCheckFixTimingViaShotChanges");
                if (subNode != null)
                {
                    shortcuts.MainCheckFixTimingViaShotChanges = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateGoogleIt");
                if (subNode != null)
                {
                    shortcuts.MainTranslateGoogleIt = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateGoogleTranslateIt");
                if (subNode != null)
                {
                    shortcuts.MainTranslateGoogleTranslateIt = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateAuto");
                if (subNode != null)
                {
                    shortcuts.MainTranslateAuto = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateAutoSelectedLines");
                if (subNode != null)
                {
                    shortcuts.MainTranslateAutoSelectedLines = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateCustomSearch1");
                if (subNode != null)
                {
                    shortcuts.MainTranslateCustomSearch1 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateCustomSearch2");
                if (subNode != null)
                {
                    shortcuts.MainTranslateCustomSearch2 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateCustomSearch3");
                if (subNode != null)
                {
                    shortcuts.MainTranslateCustomSearch3 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateCustomSearch4");
                if (subNode != null)
                {
                    shortcuts.MainTranslateCustomSearch4 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateCustomSearch5");
                if (subNode != null)
                {
                    shortcuts.MainTranslateCustomSearch5 = subNode.InnerText;
                }
            }
        }

        public static string CustomSerialize(Settings settings)
        {
            var xws = new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 };
            var sb = new StringBuilder();
            using (var textWriter = XmlWriter.Create(sb, xws))
            {
                textWriter.WriteStartDocument();

                textWriter.WriteStartElement("Settings", string.Empty);

                textWriter.WriteElementString("Version", Utilities.AssemblyVersion);

                textWriter.WriteStartElement("Compare", string.Empty);
                textWriter.WriteElementString("ShowOnlyDifferences", settings.Compare.ShowOnlyDifferences.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OnlyLookForDifferenceInText", settings.Compare.OnlyLookForDifferenceInText.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("IgnoreLineBreaks", settings.Compare.IgnoreLineBreaks.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("IgnoreWhitespace", settings.Compare.IgnoreWhitespace.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("IgnoreFormatting", settings.Compare.IgnoreFormatting.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("VerifyCompleteness", string.Empty);
                textWriter.WriteElementString("ListSort", settings.VerifyCompleteness.ListSort.ToString());
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("RecentFiles", string.Empty);
                textWriter.WriteStartElement("FileNames", string.Empty);
                foreach (var item in settings.RecentFiles.Files)
                {
                    textWriter.WriteStartElement("FileName");
                    if (item.OriginalFileName != null)
                    {
                        textWriter.WriteAttributeString("OriginalFileName", item.OriginalFileName);
                    }

                    if (item.VideoFileName != null)
                    {
                        textWriter.WriteAttributeString("VideoFileName", item.VideoFileName);
                        textWriter.WriteAttributeString("AudioTrack", item.AudioTrack.ToString(CultureInfo.InvariantCulture));
                    }

                    textWriter.WriteAttributeString("FirstVisibleIndex", item.FirstVisibleIndex.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteAttributeString("FirstSelectedIndex", item.FirstSelectedIndex.ToString(CultureInfo.InvariantCulture));

                    if (item.VideoOffsetInMs != 0)
                    {
                        textWriter.WriteAttributeString("VideoOffset", item.VideoOffsetInMs.ToString(CultureInfo.InvariantCulture));
                    }

                    if (item.VideoIsSmpte)
                    {
                        textWriter.WriteAttributeString("IsSmpte", item.VideoIsSmpte.ToString(CultureInfo.InvariantCulture));
                    }

                    textWriter.WriteString(item.FileName);
                    textWriter.WriteEndElement();
                }

                textWriter.WriteEndElement();
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("General", string.Empty);

                textWriter.WriteStartElement("Profiles", string.Empty);
                foreach (var profile in settings.General.Profiles)
                {
                    textWriter.WriteStartElement("Profile");
                    textWriter.WriteElementString("Name", profile.Name);
                    textWriter.WriteElementString("SubtitleLineMaximumLength", profile.SubtitleLineMaximumLength.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteElementString("SubtitleMaximumCharactersPerSeconds", profile.SubtitleMaximumCharactersPerSeconds.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteElementString("SubtitleOptimalCharactersPerSeconds", profile.SubtitleOptimalCharactersPerSeconds.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteElementString("SubtitleMinimumDisplayMilliseconds", profile.SubtitleMinimumDisplayMilliseconds.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteElementString("SubtitleMaximumDisplayMilliseconds", profile.SubtitleMaximumDisplayMilliseconds.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteElementString("SubtitleMaximumWordsPerMinute", profile.SubtitleMaximumWordsPerMinute.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteElementString("MinimumMillisecondsBetweenLines", profile.MinimumMillisecondsBetweenLines.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteElementString("CpsLineLengthStrategy", profile.CpsLineLengthStrategy);
                    textWriter.WriteElementString("MaxNumberOfLines", profile.MaxNumberOfLines.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteElementString("MergeLinesShorterThan", profile.MergeLinesShorterThan.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteElementString("DialogStyle", profile.DialogStyle.ToString());
                    textWriter.WriteElementString("ContinuationStyle", profile.ContinuationStyle.ToString());
                    textWriter.WriteEndElement();
                }

                textWriter.WriteEndElement();

                textWriter.WriteElementString("CurrentProfile", settings.General.CurrentProfile);
                textWriter.WriteElementString("ShowToolbarNew", settings.General.ShowToolbarNew.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarOpen", settings.General.ShowToolbarOpen.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarOpenVideo", settings.General.ShowToolbarOpenVideo.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarSave", settings.General.ShowToolbarSave.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarSaveAs", settings.General.ShowToolbarSaveAs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarFind", settings.General.ShowToolbarFind.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarReplace", settings.General.ShowToolbarReplace.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarFixCommonErrors", settings.General.ShowToolbarFixCommonErrors.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarRemoveTextForHi", settings.General.ShowToolbarRemoveTextForHi.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarToggleSourceView", settings.General.ShowToolbarToggleSourceView.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarVisualSync", settings.General.ShowToolbarVisualSync.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarBurnIn", settings.General.ShowToolbarBurnIn.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarSpellCheck", settings.General.ShowToolbarSpellCheck.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarNetflixGlyphCheck", settings.General.ShowToolbarNetflixGlyphCheck.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarBeautifyTimeCodes", settings.General.ShowToolbarBeautifyTimeCodes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarSettings", settings.General.ShowToolbarSettings.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarHelp", settings.General.ShowToolbarHelp.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowFrameRate", settings.General.ShowFrameRate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowVideoControls", settings.General.ShowVideoControls.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TextAndOrigianlTextBoxesSwitched", settings.General.TextAndOrigianlTextBoxesSwitched.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LayoutNumber", settings.General.LayoutNumber.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LayoutSizes", settings.General.LayoutSizes);

                textWriter.WriteElementString("ShowVideoPlayer", settings.General.ShowVideoPlayer.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowAudioVisualizer", settings.General.ShowAudioVisualizer.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowWaveform", settings.General.ShowWaveform.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowSpectrogram", settings.General.ShowSpectrogram.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DefaultFrameRate", settings.General.DefaultFrameRate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DefaultSubtitleFormat", settings.General.DefaultSubtitleFormat);
                textWriter.WriteElementString("DefaultSaveAsFormat", settings.General.DefaultSaveAsFormat);
                textWriter.WriteElementString("FavoriteSubtitleFormats", settings.General.FavoriteSubtitleFormats);
                textWriter.WriteElementString("DefaultEncoding", settings.General.DefaultEncoding);
                textWriter.WriteElementString("AutoConvertToUtf8", settings.General.AutoConvertToUtf8.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoGuessAnsiEncoding", settings.General.AutoGuessAnsiEncoding.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TranslationAutoSuffixes", settings.General.TranslationAutoSuffixes);
                textWriter.WriteElementString("TranslationAutoSuffixDefault", settings.General.TranslationAutoSuffixDefault);
                textWriter.WriteElementString("SystemSubtitleFontNameOverride", settings.General.SystemSubtitleFontNameOverride);
                textWriter.WriteElementString("SystemSubtitleFontSizeOverride", settings.General.SystemSubtitleFontSizeOverride.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleFontName", settings.General.SubtitleFontName);
                textWriter.WriteElementString("SubtitleTextBoxFontSize", settings.General.SubtitleTextBoxFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleListViewFontSize", settings.General.SubtitleListViewFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleTextBoxFontBold", settings.General.SubtitleTextBoxFontBold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleListViewFontBold", settings.General.SubtitleListViewFontBold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleTextBoxSyntaxColor", settings.General.SubtitleTextBoxSyntaxColor.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleTextBoxHtmlColor", settings.General.SubtitleTextBoxHtmlColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleTextBoxAssColor", settings.General.SubtitleTextBoxAssColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleFontColor", settings.General.SubtitleFontColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleBackgroundColor", settings.General.SubtitleBackgroundColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MeasureFontName", settings.General.MeasureFontName);
                textWriter.WriteElementString("MeasureFontSize", settings.General.MeasureFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MeasureFontBold", settings.General.MeasureFontBold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleLineMaximumPixelWidth", settings.General.SubtitleLineMaximumPixelWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CenterSubtitleInTextBox", settings.General.CenterSubtitleInTextBox.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowRecentFiles", settings.General.ShowRecentFiles.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RememberSelectedLine", settings.General.RememberSelectedLine.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("StartLoadLastFile", settings.General.StartLoadLastFile.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("StartRememberPositionAndSize", settings.General.StartRememberPositionAndSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("StartPosition", settings.General.StartPosition);
                textWriter.WriteElementString("StartSize", settings.General.StartSize);
                textWriter.WriteElementString("StartInSourceView", settings.General.StartInSourceView.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveBlankLinesWhenOpening", settings.General.RemoveBlankLinesWhenOpening.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveBadCharsWhenOpening", settings.General.RemoveBadCharsWhenOpening.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleLineMaximumLength", settings.General.SubtitleLineMaximumLength.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MaxNumberOfLines", settings.General.MaxNumberOfLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MaxNumberOfLinesPlusAbort", settings.General.MaxNumberOfLinesPlusAbort.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeLinesShorterThan", settings.General.MergeLinesShorterThan.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleMinimumDisplayMilliseconds", settings.General.SubtitleMinimumDisplayMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleMaximumDisplayMilliseconds", settings.General.SubtitleMaximumDisplayMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MinimumMillisecondsBetweenLines", settings.General.MinimumMillisecondsBetweenLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SetStartEndHumanDelay", settings.General.SetStartEndHumanDelay.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoWrapLineWhileTyping", settings.General.AutoWrapLineWhileTyping.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleMaximumCharactersPerSeconds", settings.General.SubtitleMaximumCharactersPerSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleOptimalCharactersPerSeconds", settings.General.SubtitleOptimalCharactersPerSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CpsLineLengthStrategy", settings.General.CpsLineLengthStrategy);
                textWriter.WriteElementString("SubtitleMaximumWordsPerMinute", settings.General.SubtitleMaximumWordsPerMinute.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DialogStyle", settings.General.DialogStyle.ToString());

                textWriter.WriteElementString("ContinuationStyle", settings.General.ContinuationStyle.ToString());

                textWriter.WriteElementString("ContinuationPause", settings.General.ContinuationPause.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CustomContinuationStyleSuffix", settings.General.CustomContinuationStyleSuffix.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CustomContinuationStyleSuffixApplyIfComma", settings.General.CustomContinuationStyleSuffixApplyIfComma.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CustomContinuationStyleSuffixAddSpace", settings.General.CustomContinuationStyleSuffixAddSpace.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CustomContinuationStyleSuffixReplaceComma", settings.General.CustomContinuationStyleSuffixReplaceComma.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CustomContinuationStylePrefix", settings.General.CustomContinuationStylePrefix.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CustomContinuationStylePrefixAddSpace", settings.General.CustomContinuationStylePrefixAddSpace.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CustomContinuationStyleUseDifferentStyleGap", settings.General.CustomContinuationStyleUseDifferentStyleGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CustomContinuationStyleGapSuffix", settings.General.CustomContinuationStyleGapSuffix.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CustomContinuationStyleGapSuffixApplyIfComma", settings.General.CustomContinuationStyleGapSuffixApplyIfComma.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CustomContinuationStyleGapSuffixAddSpace", settings.General.CustomContinuationStyleGapSuffixAddSpace.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CustomContinuationStyleGapSuffixReplaceComma", settings.General.CustomContinuationStyleGapSuffixReplaceComma.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CustomContinuationStyleGapPrefix", settings.General.CustomContinuationStyleGapPrefix.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CustomContinuationStyleGapPrefixAddSpace", settings.General.CustomContinuationStyleGapPrefixAddSpace.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixContinuationStyleUncheckInsertsAllCaps", settings.General.FixContinuationStyleUncheckInsertsAllCaps.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixContinuationStyleUncheckInsertsItalic", settings.General.FixContinuationStyleUncheckInsertsItalic.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixContinuationStyleUncheckInsertsLowercase", settings.General.FixContinuationStyleUncheckInsertsLowercase.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixContinuationStyleHideContinuationCandidatesWithoutName", settings.General.FixContinuationStyleHideContinuationCandidatesWithoutName.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpellCheckLanguage", settings.General.SpellCheckLanguage);
                textWriter.WriteElementString("VideoPlayer", settings.General.VideoPlayer);

                textWriter.WriteElementString("VideoPlayerDefaultVolume", settings.General.VideoPlayerDefaultVolume.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VideoPlayerPreviewFontName", settings.General.VideoPlayerPreviewFontName);
                textWriter.WriteElementString("VideoPlayerPreviewFontSize", settings.General.VideoPlayerPreviewFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VideoPlayerPreviewFontBold", settings.General.VideoPlayerPreviewFontBold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VideoPlayerShowStopButton", settings.General.VideoPlayerShowStopButton.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VideoPlayerShowMuteButton", settings.General.VideoPlayerShowMuteButton.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VideoPlayerShowFullscreenButton", settings.General.VideoPlayerShowFullscreenButton.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Language", settings.General.Language);
                textWriter.WriteElementString("ListViewLineSeparatorString", settings.General.ListViewLineSeparatorString);
                textWriter.WriteElementString("ListViewDoubleClickAction", settings.General.ListViewDoubleClickAction.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SaveAsUseFileNameFrom", settings.General.SaveAsUseFileNameFrom);
                textWriter.WriteElementString("UppercaseLetters", settings.General.UppercaseLetters);
                textWriter.WriteElementString("DefaultAdjustMilliseconds", settings.General.DefaultAdjustMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoRepeatOn", settings.General.AutoRepeatOn.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoRepeatCount", settings.General.AutoRepeatCount.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoContinueOn", settings.General.AutoContinueOn.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoContinueDelay", settings.General.AutoContinueDelay.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ReturnToStartAfterRepeat", settings.General.ReturnToStartAfterRepeat.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SyncListViewWithVideoWhilePlaying", settings.General.SyncListViewWithVideoWhilePlaying.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBackupSeconds", settings.General.AutoBackupSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBackupDeleteAfterMonths", settings.General.AutoBackupDeleteAfterMonths.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpellChecker", settings.General.SpellChecker);
                textWriter.WriteElementString("AllowEditOfOriginalSubtitle", settings.General.AllowEditOfOriginalSubtitle.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("PromptDeleteLines", settings.General.PromptDeleteLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Undocked", settings.General.Undocked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UndockedVideoPosition", settings.General.UndockedVideoPosition);
                textWriter.WriteElementString("UndockedVideoFullscreen", settings.General.UndockedVideoFullscreen.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UndockedWaveformPosition", settings.General.UndockedWaveformPosition);
                textWriter.WriteElementString("UndockedVideoControlsPosition", settings.General.UndockedVideoControlsPosition);
                textWriter.WriteElementString("WaveformCenter", settings.General.WaveformCenter.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformAutoGenWhenOpeningVideo", settings.General.WaveformAutoGenWhenOpeningVideo.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformUpdateIntervalMs", settings.General.WaveformUpdateIntervalMs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SmallDelayMilliseconds", settings.General.SmallDelayMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LargeDelayMilliseconds", settings.General.LargeDelayMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowOriginalAsPreviewIfAvailable", settings.General.ShowOriginalAsPreviewIfAvailable.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LastPacCodePage", settings.General.LastPacCodePage.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OpenSubtitleExtraExtensions", settings.General.OpenSubtitleExtraExtensions);
                textWriter.WriteElementString("ListViewColumnsRememberSize", settings.General.ListViewColumnsRememberSize.ToString(CultureInfo.InvariantCulture));

                textWriter.WriteElementString("ListViewNumberWidth", settings.General.ListViewNumberWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewStartWidth", settings.General.ListViewStartWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewEndWidth", settings.General.ListViewEndWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewDurationWidth", settings.General.ListViewDurationWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewCpsWidth", settings.General.ListViewCpsWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewWpmWidth", settings.General.ListViewWpmWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewGapWidth", settings.General.ListViewGapWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewActorWidth", settings.General.ListViewActorWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewRegionWidth", settings.General.ListViewRegionWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewTextWidth", settings.General.ListViewTextWidth.ToString(CultureInfo.InvariantCulture));

                textWriter.WriteElementString("ListViewNumberDisplayIndex", settings.General.ListViewNumberDisplayIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewStartDisplayIndex", settings.General.ListViewStartDisplayIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewEndDisplayIndex", settings.General.ListViewEndDisplayIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewDurationDisplayIndex", settings.General.ListViewDurationDisplayIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewCpsDisplayIndex", settings.General.ListViewCpsDisplayIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewWpmDisplayIndex", settings.General.ListViewWpmDisplayIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewGapDisplayIndex", settings.General.ListViewGapDisplayIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewActorDisplayIndex", settings.General.ListViewActorDisplayIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewRegionDisplayIndex", settings.General.ListViewRegionDisplayIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewTextDisplayIndex", settings.General.ListViewTextDisplayIndex.ToString(CultureInfo.InvariantCulture));

                textWriter.WriteElementString("DirectShowDoubleLoad", settings.General.DirectShowDoubleLoad.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VlcWaveTranscodeSettings", settings.General.VlcWaveTranscodeSettings);
                textWriter.WriteElementString("VlcLocation", settings.General.VlcLocation);

                textWriter.WriteElementString("VlcLocationRelative", settings.General.VlcLocationRelative);
                textWriter.WriteElementString("MpvVideoOutputWindows", settings.General.MpvVideoOutputWindows);
                textWriter.WriteElementString("MpvVideoOutputLinux", settings.General.MpvVideoOutputLinux);
                textWriter.WriteElementString("MpvVideoVf", settings.General.MpvVideoVf);

                textWriter.WriteElementString("MpvVideoAf", settings.General.MpvVideoAf);

                textWriter.WriteElementString("MpvExtraOptions", settings.General.MpvExtraOptions);
                textWriter.WriteElementString("MpvLogging", settings.General.MpvLogging.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MpvHandlesPreviewText", settings.General.MpvHandlesPreviewText.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MpvPreviewTextPrimaryColor", ToHtml(settings.General.MpvPreviewTextPrimaryColor));
                textWriter.WriteElementString("MpvPreviewTextOutlineColor", ToHtml(settings.General.MpvPreviewTextOutlineColor));
                textWriter.WriteElementString("MpvPreviewTextBackgroundColor", ToHtml(settings.General.MpvPreviewTextBackgroundColor));
                textWriter.WriteElementString("MpvPreviewTextOutlineWidth", settings.General.MpvPreviewTextOutlineWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MpvPreviewTextShadowWidth", settings.General.MpvPreviewTextShadowWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MpvPreviewTextOpaqueBox", settings.General.MpvPreviewTextOpaqueBox.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MpvPreviewTextOpaqueBoxStyle", settings.General.MpvPreviewTextOpaqueBoxStyle);
                textWriter.WriteElementString("MpvPreviewTextAlignment", settings.General.MpvPreviewTextAlignment);
                textWriter.WriteElementString("MpvPreviewTextMarginVertical", settings.General.MpvPreviewTextMarginVertical.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MpcHcLocation", settings.General.MpcHcLocation);
                textWriter.WriteElementString("MkvMergeLocation", settings.General.MkvMergeLocation);
                textWriter.WriteElementString("UseFFmpegForWaveExtraction", settings.General.UseFFmpegForWaveExtraction.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FFmpegUseCenterChannelOnly", settings.General.FFmpegUseCenterChannelOnly.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FFmpegLocation", settings.General.FFmpegLocation);
                textWriter.WriteElementString("FFmpegSceneThreshold", settings.General.FFmpegSceneThreshold);
                textWriter.WriteElementString("UseTimeFormatHHMMSSFF", settings.General.UseTimeFormatHHMMSSFF.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitBehavior", settings.General.SplitBehavior.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitRemovesDashes", settings.General.SplitRemovesDashes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ClearStatusBarAfterSeconds", settings.General.ClearStatusBarAfterSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Company", settings.General.Company);
                textWriter.WriteElementString("MoveVideo100Or500MsPlaySmallSample", settings.General.MoveVideo100Or500MsPlaySmallSample.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DisableVideoAutoLoading", settings.General.DisableVideoAutoLoading.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DisableShowingLoadErrors", settings.General.DisableShowingLoadErrors.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AllowVolumeBoost", settings.General.AllowVolumeBoost.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("NewEmptyUseAutoDuration", settings.General.NewEmptyUseAutoDuration.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RightToLeftMode", settings.General.RightToLeftMode.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LastSaveAsFormat", settings.General.LastSaveAsFormat);
                textWriter.WriteElementString("CheckForUpdates", settings.General.CheckForUpdates.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LastCheckForUpdates", settings.General.LastCheckForUpdates.ToString("yyyy-MM-dd"));
                textWriter.WriteElementString("AutoSave", settings.General.AutoSave.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("PreviewAssaText", settings.General.PreviewAssaText);
                textWriter.WriteElementString("TagsInToggleHiTags", settings.General.TagsInToggleHiTags);
                textWriter.WriteElementString("TagsInToggleCustomTags", settings.General.TagsInToggleCustomTags);
                textWriter.WriteElementString("ShowProgress", settings.General.ShowProgress.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowNegativeDurationInfoOnSave", settings.General.ShowNegativeDurationInfoOnSave.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowFormatRequiresUtf8Warning", settings.General.ShowFormatRequiresUtf8Warning.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DefaultVideoOffsetInMs", settings.General.DefaultVideoOffsetInMs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DefaultVideoOffsetInMsList", settings.General.DefaultVideoOffsetInMsList);
                textWriter.WriteElementString("AutoSetVideoSmpteForTtml", settings.General.AutoSetVideoSmpteForTtml.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoSetVideoSmpteForTtmlPrompt", settings.General.AutoSetVideoSmpteForTtmlPrompt.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TitleBarAsterisk", settings.General.TitleBarAsterisk);
                textWriter.WriteElementString("TitleBarFullFileName", settings.General.TitleBarFullFileName.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MeasurementConverterCloseOnInsert", settings.General.MeasurementConverterCloseOnInsert.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MeasurementConverterCategories", settings.General.MeasurementConverterCategories);
                textWriter.WriteElementString("SubtitleTextBoxAutoVerticalScrollBars", settings.General.SubtitleTextBoxAutoVerticalScrollBars.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleTextBoxMaxHeight", settings.General.SubtitleTextBoxMaxHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AllowLetterShortcutsInTextBox", settings.General.AllowLetterShortcutsInTextBox.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LastColorPickerColor", ToHtml(settings.General.LastColorPickerColor));
                textWriter.WriteElementString("LastColorPickerColor1", ToHtml(settings.General.LastColorPickerColor1));
                textWriter.WriteElementString("LastColorPickerColor2", ToHtml(settings.General.LastColorPickerColor2));
                textWriter.WriteElementString("LastColorPickerColor3", ToHtml(settings.General.LastColorPickerColor3));
                textWriter.WriteElementString("LastColorPickerColor4", ToHtml(settings.General.LastColorPickerColor4));
                textWriter.WriteElementString("LastColorPickerColor5", ToHtml(settings.General.LastColorPickerColor5));
                textWriter.WriteElementString("LastColorPickerColor6", ToHtml(settings.General.LastColorPickerColor6));
                textWriter.WriteElementString("LastColorPickerColor7", ToHtml(settings.General.LastColorPickerColor7));
                textWriter.WriteElementString("DarkThemeBackColor", ToHtml(settings.General.DarkThemeBackColor));
                textWriter.WriteElementString("DarkThemeBackColor", ToHtml(settings.General.DarkThemeSelectedBackgroundColor));
                textWriter.WriteElementString("DarkThemeForeColor", ToHtml(settings.General.DarkThemeForeColor));
                textWriter.WriteElementString("DarkThemeDisabledColor", ToHtml(settings.General.DarkThemeDisabledColor));
                textWriter.WriteElementString("ToolbarIconTheme", settings.General.ToolbarIconTheme);
                textWriter.WriteElementString("UseDarkTheme", settings.General.UseDarkTheme.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DarkThemeShowListViewGridLines", settings.General.DarkThemeShowListViewGridLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowBetaStuff", settings.General.ShowBetaStuff.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DebugTranslationSync", settings.General.DebugTranslationSync.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UseLegacyDownloader", settings.General.UseLegacyDownloader.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UseLegacyHtmlColor", false.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DefaultLanguages", settings.General.DefaultLanguages);
                textWriter.WriteElementString("NewEmptyDefaultMs", settings.General.NewEmptyDefaultMs.ToString(CultureInfo.InvariantCulture));

                textWriter.WriteEndElement();

                textWriter.WriteStartElement("Tools", string.Empty);

                textWriter.WriteStartElement("AssTagTemplates", string.Empty);
                foreach (var template in settings.Tools.AssaTagTemplates)
                {
                    textWriter.WriteStartElement("Template");
                    textWriter.WriteElementString("Tag", template.Tag);
                    textWriter.WriteElementString("Hint", template.Hint);
                    textWriter.WriteEndElement();
                }

                textWriter.WriteEndElement();

                textWriter.WriteElementString("StartSceneIndex", settings.Tools.StartSceneIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("EndSceneIndex", settings.Tools.EndSceneIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VerifyPlaySeconds", settings.Tools.VerifyPlaySeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixShortDisplayTimesAllowMoveStartTime", settings.Tools.FixShortDisplayTimesAllowMoveStartTime.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveEmptyLinesBetweenText", settings.Tools.RemoveEmptyLinesBetweenText.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MusicSymbol", settings.Tools.MusicSymbol);
                textWriter.WriteElementString("MusicSymbolReplace", settings.Tools.MusicSymbolReplace);
                textWriter.WriteElementString("UnicodeSymbolsToInsert", settings.Tools.UnicodeSymbolsToInsert);
                textWriter.WriteElementString("SpellCheckAutoChangeNameCasing", settings.Tools.SpellCheckAutoChangeNameCasing.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpellCheckUseLargerFont", settings.Tools.SpellCheckUseLargerFont.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpellCheckAutoChangeNamesUseSuggestions", settings.Tools.SpellCheckAutoChangeNamesUseSuggestions.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpellCheckSearchEngine", settings.Tools.SpellCheckSearchEngine);
                textWriter.WriteElementString("SpellCheckOneLetterWords", settings.Tools.CheckOneLetterWords.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpellCheckEnglishAllowInQuoteAsIng", settings.Tools.SpellCheckEnglishAllowInQuoteAsIng.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RememberUseAlwaysList", settings.Tools.RememberUseAlwaysList.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LiveSpellCheck", settings.Tools.LiveSpellCheck.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpellCheckShowCompletedMessage", settings.Tools.SpellCheckShowCompletedMessage.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OcrFixUseHardcodedRules", settings.Tools.OcrFixUseHardcodedRules.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OcrGoogleCloudVisionSeHandlesTextMerge", settings.Tools.OcrGoogleCloudVisionSeHandlesTextMerge.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OcrBinaryImageCompareRgbThreshold", settings.Tools.OcrBinaryImageCompareRgbThreshold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OcrTesseract4RgbThreshold", settings.Tools.OcrTesseract4RgbThreshold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OcrAddLetterRow1", settings.Tools.OcrAddLetterRow1);
                textWriter.WriteElementString("OcrAddLetterRow2", settings.Tools.OcrAddLetterRow2);
                textWriter.WriteElementString("OcrTrainFonts", settings.Tools.OcrTrainFonts);
                textWriter.WriteElementString("OcrTrainMergedLetters", settings.Tools.OcrTrainMergedLetters);
                textWriter.WriteElementString("OcrTrainSrtFile", settings.Tools.OcrTrainSrtFile);
                textWriter.WriteElementString("OcrUseWordSplitList", settings.Tools.OcrUseWordSplitList.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OcrUseWordSplitListAvoidPropercase", settings.Tools.OcrUseWordSplitListAvoidPropercase.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BDOpenIn", settings.Tools.BDOpenIn);
                textWriter.WriteElementString("MicrosoftBingApiId", settings.Tools.MicrosoftBingApiId);
                textWriter.WriteElementString("MicrosoftTranslatorApiKey", settings.Tools.MicrosoftTranslatorApiKey);
                textWriter.WriteElementString("MicrosoftTranslatorTokenEndpoint", settings.Tools.MicrosoftTranslatorTokenEndpoint);
                textWriter.WriteElementString("MicrosoftTranslatorCategory", settings.Tools.MicrosoftTranslatorCategory);
                textWriter.WriteElementString("GoogleApiV2Key", settings.Tools.GoogleApiV2Key);
                textWriter.WriteElementString("GoogleTranslateNoKeyWarningShow", settings.Tools.GoogleTranslateNoKeyWarningShow.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GoogleApiV1ChunkSize", settings.Tools.GoogleApiV1ChunkSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GoogleTranslateLastSourceLanguage", settings.Tools.GoogleTranslateLastSourceLanguage);
                textWriter.WriteElementString("GoogleTranslateLastTargetLanguage", settings.Tools.GoogleTranslateLastTargetLanguage);
                textWriter.WriteElementString("AutoTranslateLastName", settings.Tools.AutoTranslateLastName);
                textWriter.WriteElementString("AutoTranslateLastUrl", settings.Tools.AutoTranslateLastUrl);
                textWriter.WriteElementString("AutoTranslateNllbApiUrl", settings.Tools.AutoTranslateNllbApiUrl);
                textWriter.WriteElementString("AutoTranslateNllbServeUrl", settings.Tools.AutoTranslateNllbServeUrl);
                textWriter.WriteElementString("AutoTranslateNllbServeModel", settings.Tools.AutoTranslateNllbServeModel);
                textWriter.WriteElementString("AutoTranslateLibreUrl", settings.Tools.AutoTranslateLibreUrl);
                textWriter.WriteElementString("AutoTranslateLibreApiKey", settings.Tools.AutoTranslateLibreApiKey);
                textWriter.WriteElementString("AutoTranslateMyMemoryApiKey", settings.Tools.AutoTranslateMyMemoryApiKey);
                textWriter.WriteElementString("AutoTranslateSeamlessM4TUrl", settings.Tools.AutoTranslateSeamlessM4TUrl);
                textWriter.WriteElementString("AutoTranslateDeepLApiKey", settings.Tools.AutoTranslateDeepLApiKey);
                textWriter.WriteElementString("AutoTranslateDeepLUrl", settings.Tools.AutoTranslateDeepLUrl);
                textWriter.WriteElementString("AutoTranslatePapagoApiKeyId", settings.Tools.AutoTranslatePapagoApiKeyId);
                textWriter.WriteElementString("AutoTranslatePapagoApiKey", settings.Tools.AutoTranslatePapagoApiKey);
                textWriter.WriteElementString("AutoTranslateDeepLFormality", settings.Tools.AutoTranslateDeepLFormality);
                textWriter.WriteElementString("TranslateAllowSplit", settings.Tools.TranslateAllowSplit.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TranslateLastService", settings.Tools.TranslateLastService);
                textWriter.WriteElementString("TranslateMergeStrategy", settings.Tools.TranslateMergeStrategy);
                textWriter.WriteElementString("TranslateViaCopyPasteSeparator", settings.Tools.TranslateViaCopyPasteSeparator);
                textWriter.WriteElementString("TranslateViaCopyPasteMaxSize", settings.Tools.TranslateViaCopyPasteMaxSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TranslateViaCopyPasteAutoCopyToClipboard", settings.Tools.TranslateViaCopyPasteAutoCopyToClipboard.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChatGptUrl", settings.Tools.ChatGptUrl);
                textWriter.WriteElementString("ChatGptPrompt", settings.Tools.ChatGptPrompt);
                textWriter.WriteElementString("ChatGptApiKey", settings.Tools.ChatGptApiKey);
                textWriter.WriteElementString("ChatGptModel", settings.Tools.ChatGptModel);
                textWriter.WriteElementString("LmStudioApiUrl", settings.Tools.LmStudioApiUrl);
                textWriter.WriteElementString("LmStudioModel", settings.Tools.LmStudioModel);
                textWriter.WriteElementString("LmStudioPrompt", settings.Tools.LmStudioPrompt);
                textWriter.WriteElementString("LmStudioApiUrl", settings.Tools.LmStudioApiUrl);
                textWriter.WriteElementString("OllamaModels", settings.Tools.OllamaModels);
                textWriter.WriteElementString("OllamaModel", settings.Tools.OllamaModel);
                textWriter.WriteElementString("OllamaPrompt", settings.Tools.OllamaPrompt);
                textWriter.WriteElementString("OllamaApiUrl", settings.Tools.OllamaApiUrl);
                textWriter.WriteElementString("AnthropicPrompt", settings.Tools.AnthropicPrompt);
                textWriter.WriteElementString("AnthropicApiKey", settings.Tools.AnthropicApiKey);
                textWriter.WriteElementString("AnthropicApiModel", settings.Tools.AnthropicApiModel);
                textWriter.WriteElementString("AutoTranslateDelaySeconds", settings.Tools.AutoTranslateDelaySeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoTranslateMaxBytes", settings.Tools.AutoTranslateMaxBytes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoTranslateStrategy", settings.Tools.AutoTranslateStrategy);
                textWriter.WriteElementString("GeminiProApiKey", settings.Tools.GeminiProApiKey);
                textWriter.WriteElementString("TextToSpeechEngine", settings.Tools.TextToSpeechEngine);
                textWriter.WriteElementString("TextToSpeechLastVoice", settings.Tools.TextToSpeechLastVoice);
                textWriter.WriteElementString("TextToSpeechElevenLabsApiKey", settings.Tools.TextToSpeechElevenLabsApiKey);
                textWriter.WriteElementString("TextToSpeechAzureApiKey", settings.Tools.TextToSpeechAzureApiKey);
                textWriter.WriteElementString("TextToSpeechAzureRegion", settings.Tools.TextToSpeechAzureRegion);
                textWriter.WriteElementString("TextToSpeechPreview", settings.Tools.TextToSpeechPreview.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TextToSpeechCustomAudio", settings.Tools.TextToSpeechCustomAudio.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TextToSpeechCustomAudioStereo", settings.Tools.TextToSpeechCustomAudioStereo.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TextToSpeechCustomAudioEncoding", settings.Tools.TextToSpeechCustomAudioEncoding);
                textWriter.WriteElementString("TextToSpeechAddToVideoFile", settings.Tools.TextToSpeechAddToVideoFile.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxColorDurationSmall", settings.Tools.ListViewSyntaxColorDurationSmall.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxColorDurationBig", settings.Tools.ListViewSyntaxColorDurationBig.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxColorLongLines", settings.Tools.ListViewSyntaxColorLongLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxColorWideLines", settings.Tools.ListViewSyntaxColorWideLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxMoreThanXLines", settings.Tools.ListViewSyntaxMoreThanXLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxColorOverlap", settings.Tools.ListViewSyntaxColorOverlap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxColorGap", settings.Tools.ListViewSyntaxColorGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxErrorColor", settings.Tools.ListViewSyntaxErrorColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewUnfocusedSelectedColor", settings.Tools.ListViewUnfocusedSelectedColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Color1", ToHtml(settings.Tools.Color1));
                textWriter.WriteElementString("Color2", ToHtml(settings.Tools.Color2));
                textWriter.WriteElementString("Color3", ToHtml(settings.Tools.Color3));
                textWriter.WriteElementString("Color4", ToHtml(settings.Tools.Color4));
                textWriter.WriteElementString("Color5", ToHtml(settings.Tools.Color5));
                textWriter.WriteElementString("Color6", ToHtml(settings.Tools.Color6));
                textWriter.WriteElementString("Color7", ToHtml(settings.Tools.Color7));
                textWriter.WriteElementString("Color8", ToHtml(settings.Tools.Color8));
                textWriter.WriteElementString("ListViewShowColumnStartTime", settings.Tools.ListViewShowColumnStartTime.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewShowColumnEndTime", settings.Tools.ListViewShowColumnEndTime.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewShowColumnDuration", settings.Tools.ListViewShowColumnDuration.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewShowColumnCharsPerSec", settings.Tools.ListViewShowColumnCharsPerSec.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewShowColumnWordsPerMin", settings.Tools.ListViewShowColumnWordsPerMin.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewShowColumnGap", settings.Tools.ListViewShowColumnGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewShowColumnActor", settings.Tools.ListViewShowColumnActor.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewShowColumnRegion", settings.Tools.ListViewShowColumnRegion.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewMultipleReplaceShowColumnRuleInfo", settings.Tools.ListViewMultipleReplaceShowColumnRuleInfo.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitAdvanced", settings.Tools.SplitAdvanced.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitOutputFolder", settings.Tools.SplitOutputFolder);
                textWriter.WriteElementString("SplitNumberOfParts", settings.Tools.SplitNumberOfParts.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitVia", settings.Tools.SplitVia);
                textWriter.WriteElementString("JoinCorrectTimeCodes", settings.Tools.JoinCorrectTimeCodes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("JoinAddMs", settings.Tools.JoinAddMs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitLongLinesMax", settings.Tools.SplitLongLinesMax.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("NewEmptyTranslationText", settings.Tools.NewEmptyTranslationText);
                textWriter.WriteElementString("BatchConvertOutputFolder", settings.Tools.BatchConvertOutputFolder);
                textWriter.WriteElementString("BatchConvertOverwriteExisting", settings.Tools.BatchConvertOverwriteExisting.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertSaveInSourceFolder", settings.Tools.BatchConvertSaveInSourceFolder.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertRemoveFormatting", settings.Tools.BatchConvertRemoveFormatting.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertRemoveFormattingAll", settings.Tools.BatchConvertRemoveFormattingAll.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertRemoveFormattingItalic", settings.Tools.BatchConvertRemoveFormattingItalic.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertRemoveFormattingBold", settings.Tools.BatchConvertRemoveFormattingBold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertRemoveFormattingUnderline", settings.Tools.BatchConvertRemoveFormattingUnderline.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertRemoveFormattingFontName", settings.Tools.BatchConvertRemoveFormattingFontName.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertRemoveFormattingColor", settings.Tools.BatchConvertRemoveFormattingColor.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertRemoveFormattingAlignment", settings.Tools.BatchConvertRemoveFormattingAlignment.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertRemoveStyle", settings.Tools.BatchConvertRemoveStyle.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertBridgeGaps", settings.Tools.BatchConvertBridgeGaps.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertFixCasing", settings.Tools.BatchConvertFixCasing.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertRemoveTextForHI", settings.Tools.BatchConvertRemoveTextForHI.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertConvertColorsToDialog", settings.Tools.BatchConvertConvertColorsToDialog.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertBeautifyTimeCodes", settings.Tools.BatchConvertBeautifyTimeCodes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertAutoTranslate", settings.Tools.BatchConvertAutoTranslate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertSplitLongLines", settings.Tools.BatchConvertSplitLongLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertFixCommonErrors", settings.Tools.BatchConvertFixCommonErrors.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertMultipleReplace", settings.Tools.BatchConvertMultipleReplace.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertFixRtl", settings.Tools.BatchConvertFixRtl.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertFixRtlMode", settings.Tools.BatchConvertFixRtlMode);
                textWriter.WriteElementString("BatchConvertAutoBalance", settings.Tools.BatchConvertAutoBalance.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertSetMinDisplayTimeBetweenSubtitles", settings.Tools.BatchConvertSetMinDisplayTimeBetweenSubtitles.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertMergeShortLines", settings.Tools.BatchConvertMergeShortLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertRemoveLineBreaks", settings.Tools.BatchConvertRemoveLineBreaks.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertMergeSameText", settings.Tools.BatchConvertMergeSameText.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertMergeSameTimeCodes", settings.Tools.BatchConvertMergeSameTimeCodes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertChangeSpeed", settings.Tools.BatchConvertChangeSpeed.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertAdjustDisplayDuration", settings.Tools.BatchConvertAdjustDisplayDuration.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertApplyDurationLimits", settings.Tools.BatchConvertApplyDurationLimits.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertDeleteLines", settings.Tools.BatchConvertDeleteLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertAssaChangeRes", settings.Tools.BatchConvertAssaChangeRes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertSortBy", settings.Tools.BatchConvertSortBy.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertSortByChoice", settings.Tools.BatchConvertSortByChoice);
                textWriter.WriteElementString("BatchConvertChangeFrameRate", settings.Tools.BatchConvertChangeFrameRate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertOffsetTimeCodes", settings.Tools.BatchConvertOffsetTimeCodes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertScanFolderIncludeVideo", settings.Tools.BatchConvertScanFolderIncludeVideo.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertLanguage", settings.Tools.BatchConvertLanguage);
                textWriter.WriteElementString("BatchConvertFormat", settings.Tools.BatchConvertFormat);
                textWriter.WriteElementString("BatchConvertAssStyles", settings.Tools.BatchConvertAssStyles);
                textWriter.WriteElementString("BatchConvertSsaStyles", settings.Tools.BatchConvertSsaStyles);
                textWriter.WriteElementString("BatchConvertUseStyleFromSource", settings.Tools.BatchConvertUseStyleFromSource.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertExportCustomTextTemplate", settings.Tools.BatchConvertExportCustomTextTemplate);
                textWriter.WriteElementString("BatchConvertTsOverrideXPosition", settings.Tools.BatchConvertTsOverrideXPosition.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertTsOverrideYPosition", settings.Tools.BatchConvertTsOverrideYPosition.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertTsOverrideBottomMargin", settings.Tools.BatchConvertTsOverrideBottomMargin.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertTsOverrideHAlign", settings.Tools.BatchConvertTsOverrideHAlign);
                textWriter.WriteElementString("BatchConvertTsOverrideHMargin", settings.Tools.BatchConvertTsOverrideHMargin.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertTsOverrideScreenSize", settings.Tools.BatchConvertTsOverrideScreenSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertTsScreenWidth", settings.Tools.BatchConvertTsScreenWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertTsScreenHeight", settings.Tools.BatchConvertTsScreenHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertTsOnlyTeletext", settings.Tools.BatchConvertTsOnlyTeletext.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertTsFileNameAppend", settings.Tools.BatchConvertTsFileNameAppend);
                textWriter.WriteElementString("BatchConvertMkvLanguageCodeStyle", settings.Tools.BatchConvertMkvLanguageCodeStyle);
                textWriter.WriteElementString("BatchConvertOcrEngine", settings.Tools.BatchConvertOcrEngine);
                textWriter.WriteElementString("BatchConvertOcrLanguage", settings.Tools.BatchConvertOcrLanguage);
                textWriter.WriteElementString("WaveformBatchLastFolder", settings.Tools.WaveformBatchLastFolder);
                textWriter.WriteElementString("ModifySelectionRule", settings.Tools.ModifySelectionRule);
                textWriter.WriteElementString("ModifySelectionText", settings.Tools.ModifySelectionText);
                textWriter.WriteElementString("ModifySelectionCaseSensitive", settings.Tools.ModifySelectionCaseSensitive.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportVobSubFontName", settings.Tools.ExportVobSubFontName);
                textWriter.WriteElementString("ExportVobSubFontSize", settings.Tools.ExportVobSubFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportVobSubVideoResolution", settings.Tools.ExportVobSubVideoResolution);
                textWriter.WriteElementString("ExportVobSubLanguage", settings.Tools.ExportVobSubLanguage);
                textWriter.WriteElementString("ExportVobSubSimpleRendering", settings.Tools.ExportVobSubSimpleRendering.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportVobAntiAliasingWithTransparency", settings.Tools.ExportVobAntiAliasingWithTransparency.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportBluRayFontName", settings.Tools.ExportBluRayFontName);
                textWriter.WriteElementString("ExportBluRayFontSize", settings.Tools.ExportBluRayFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportFcpFontName", settings.Tools.ExportFcpFontName);
                textWriter.WriteElementString("ExportFontNameOther", settings.Tools.ExportFontNameOther);
                textWriter.WriteElementString("ExportFcpFontSize", settings.Tools.ExportFcpFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportFcpImageType", settings.Tools.ExportFcpImageType);
                textWriter.WriteElementString("ExportFcpPalNtsc", settings.Tools.ExportFcpPalNtsc);
                textWriter.WriteElementString("ExportBdnXmlImageType", settings.Tools.ExportBdnXmlImageType);
                textWriter.WriteElementString("ExportLastFontSize", settings.Tools.ExportLastFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportLastLineHeight", settings.Tools.ExportLastLineHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportLastBorderWidth", settings.Tools.ExportLastBorderWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportLastFontBold", settings.Tools.ExportLastFontBold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportBluRayVideoResolution", settings.Tools.ExportBluRayVideoResolution);
                textWriter.WriteElementString("ExportFcpVideoResolution", settings.Tools.ExportFcpVideoResolution);
                textWriter.WriteElementString("ExportFontColor", settings.Tools.ExportFontColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportBorderColor", settings.Tools.ExportBorderColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportShadowColor", settings.Tools.ExportShadowColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportBoxBorderSize", settings.Tools.ExportBoxBorderSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportBottomMarginUnit", settings.Tools.ExportBottomMarginUnit);
                textWriter.WriteElementString("ExportBottomMarginPercent", settings.Tools.ExportBottomMarginPercent.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportBottomMarginPixels", settings.Tools.ExportBottomMarginPixels.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportLeftRightMarginUnit", settings.Tools.ExportLeftRightMarginUnit);
                textWriter.WriteElementString("ExportLeftRightMarginPercent", settings.Tools.ExportLeftRightMarginPercent.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportLeftRightMarginPixels", settings.Tools.ExportLeftRightMarginPixels.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportHorizontalAlignment", settings.Tools.ExportHorizontalAlignment.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportBluRayBottomMarginPercent", settings.Tools.ExportBluRayBottomMarginPercent.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportBluRayBottomMarginPixels", settings.Tools.ExportBluRayBottomMarginPixels.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportBluRayShadow", settings.Tools.ExportBluRayShadow.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportBluRayRemoveSmallGaps", settings.Tools.ExportBluRayRemoveSmallGaps.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportCdgBackgroundImage", settings.Tools.ExportCdgBackgroundImage);
                textWriter.WriteElementString("ExportCdgMarginLeft", settings.Tools.ExportCdgMarginLeft.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportCdgMarginBottom", settings.Tools.ExportCdgMarginBottom.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportCdgFormat", settings.Tools.ExportCdgFormat);
                textWriter.WriteElementString("Export3DType", settings.Tools.Export3DType.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Export3DDepth", settings.Tools.Export3DDepth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportLastShadowTransparency", settings.Tools.ExportLastShadowTransparency.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportLastFrameRate", settings.Tools.ExportLastFrameRate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportFullFrame", settings.Tools.ExportFullFrame.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportFcpFullPathUrl", settings.Tools.ExportFcpFullPathUrl.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportPenLineJoin", settings.Tools.ExportPenLineJoin);
                textWriter.WriteElementString("BinEditShowColumnGap", settings.Tools.BinEditShowColumnGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixCommonErrorsFixOverlapAllowEqualEndStart", settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixCommonErrorsSkipStepOne", settings.Tools.FixCommonErrorsSkipStepOne.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextSplitting", settings.Tools.ImportTextSplitting);
                textWriter.WriteElementString("ImportTextSplittingLineMode", settings.Tools.ImportTextSplittingLineMode);
                textWriter.WriteElementString("ImportTextMergeShortLines", settings.Tools.ImportTextMergeShortLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextLineBreak", settings.Tools.ImportTextLineBreak);
                textWriter.WriteElementString("ImportTextRemoveEmptyLines", settings.Tools.ImportTextRemoveEmptyLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextAutoSplitAtBlank", settings.Tools.ImportTextAutoSplitAtBlank.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextRemoveLinesNoLetters", settings.Tools.ImportTextRemoveLinesNoLetters.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextGenerateTimeCodes", settings.Tools.ImportTextGenerateTimeCodes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextTakeTimeCodeFromFileName", settings.Tools.ImportTextTakeTimeCodeFromFileName.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextAutoBreak", settings.Tools.ImportTextAutoBreak.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextAutoBreakAtEnd", settings.Tools.ImportTextAutoBreakAtEnd.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextGap", settings.Tools.ImportTextGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextAutoSplitNumberOfLines", settings.Tools.ImportTextAutoSplitNumberOfLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextAutoBreakAtEndMarkerText", settings.Tools.ImportTextAutoBreakAtEndMarkerText);
                textWriter.WriteElementString("ImportTextDurationAuto", settings.Tools.ImportTextDurationAuto.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextFixedDuration", settings.Tools.ImportTextFixedDuration.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenerateTimeCodePatterns", settings.Tools.GenerateTimeCodePatterns);
                textWriter.WriteElementString("MusicSymbolStyle", settings.Tools.MusicSymbolStyle);
                textWriter.WriteElementString("BinEditBackgroundColor", settings.Tools.BinEditBackgroundColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BinEditImageBackgroundColor", settings.Tools.BinEditImageBackgroundColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BinEditTopMargin", settings.Tools.BinEditTopMargin.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BinEditBottomMargin", settings.Tools.BinEditBottomMargin.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BinEditLeftMargin", settings.Tools.BinEditLeftMargin.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BinEditRightMargin", settings.Tools.BinEditRightMargin.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BinEditStartPosition", settings.Tools.BinEditStartPosition);
                textWriter.WriteElementString("BinEditStartSize", settings.Tools.BinEditStartSize);
                textWriter.WriteElementString("BridgeGapMilliseconds", settings.Tools.BridgeGapMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BridgeGapMillisecondsMinGap", settings.Tools.BridgeGapMillisecondsMinGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportCustomTemplates", settings.Tools.ExportCustomTemplates);
                textWriter.WriteElementString("ChangeCasingChoice", settings.Tools.ChangeCasingChoice);
                textWriter.WriteElementString("ChangeCasingNormalFixNames", settings.Tools.ChangeCasingNormalFixNames.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChangeCasingNormalOnlyUppercase", settings.Tools.ChangeCasingNormalOnlyUppercase.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UseNoLineBreakAfter", settings.Tools.UseNoLineBreakAfter.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("NoLineBreakAfterEnglish", settings.Tools.NoLineBreakAfterEnglish);
                textWriter.WriteElementString("ExportTextFormatText", settings.Tools.ExportTextFormatText);
                textWriter.WriteElementString("ReplaceIn", settings.Tools.ReplaceIn);
                textWriter.WriteElementString("ExportTextRemoveStyling", settings.Tools.ExportTextRemoveStyling.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportTextShowLineNumbers", settings.Tools.ExportTextShowLineNumbers.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportTextShowLineNumbersNewLine", settings.Tools.ExportTextShowLineNumbersNewLine.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportTextShowTimeCodes", settings.Tools.ExportTextShowTimeCodes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportTextShowTimeCodesNewLine", settings.Tools.ExportTextShowTimeCodesNewLine.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportTextNewLineAfterText", settings.Tools.ExportTextNewLineAfterText.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportTextNewLineBetweenSubtitles", settings.Tools.ExportTextNewLineBetweenSubtitles.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportTextTimeCodeFormat", settings.Tools.ExportTextTimeCodeFormat);
                textWriter.WriteElementString("ExportTextTimeCodeSeparator", settings.Tools.ExportTextTimeCodeSeparator);
                textWriter.WriteElementString("VideoOffsetKeepTimeCodes", settings.Tools.VideoOffsetKeepTimeCodes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MoveStartEndMs", settings.Tools.MoveStartEndMs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AdjustDurationSeconds", settings.Tools.AdjustDurationSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AdjustDurationPercent", settings.Tools.AdjustDurationPercent.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AdjustDurationLast", settings.Tools.AdjustDurationLast);
                textWriter.WriteElementString("AdjustDurationExtendOnly", settings.Tools.AdjustDurationExtendOnly.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AdjustDurationExtendEnforceDurationLimits", settings.Tools.AdjustDurationExtendEnforceDurationLimits.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AdjustDurationExtendCheckShotChanges", settings.Tools.AdjustDurationExtendCheckShotChanges.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChangeSpeedAllowOverlap", settings.Tools.ChangeSpeedAllowOverlap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBreakCommaBreakEarly", settings.Tools.AutoBreakCommaBreakEarly.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBreakDashEarly", settings.Tools.AutoBreakDashEarly.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBreakLineEndingEarly", settings.Tools.AutoBreakLineEndingEarly.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBreakUsePixelWidth", settings.Tools.AutoBreakUsePixelWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBreakPreferBottomHeavy", settings.Tools.AutoBreakPreferBottomHeavy.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBreakPreferBottomPercent", settings.Tools.AutoBreakPreferBottomPercent.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ApplyMinimumDurationLimit", settings.Tools.ApplyMinimumDurationLimit.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ApplyMinimumDurationLimitCheckShotChanges", settings.Tools.ApplyMinimumDurationLimitCheckShotChanges.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ApplyMaximumDurationLimit", settings.Tools.ApplyMaximumDurationLimit.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeShortLinesMaxGap", settings.Tools.MergeShortLinesMaxGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeShortLinesMaxChars", settings.Tools.MergeShortLinesMaxChars.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeShortLinesOnlyContinuous", settings.Tools.MergeShortLinesOnlyContinuous.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeTextWithSameTimeCodesMaxGap", settings.Tools.MergeTextWithSameTimeCodesMaxGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeTextWithSameTimeCodesMakeDialog", settings.Tools.MergeTextWithSameTimeCodesMakeDialog.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeTextWithSameTimeCodesReBreakLines", settings.Tools.MergeTextWithSameTimeCodesReBreakLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeLinesWithSameTextMaxMs", settings.Tools.MergeLinesWithSameTextMaxMs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeLinesWithSameTextIncrement", settings.Tools.MergeLinesWithSameTextIncrement.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ConvertColorsToDialogRemoveColorTags", settings.Tools.ConvertColorsToDialogRemoveColorTags.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ConvertColorsToDialogAddNewLines", settings.Tools.ConvertColorsToDialogAddNewLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ConvertColorsToDialogReBreakLines", settings.Tools.ConvertColorsToDialogReBreakLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ColumnPasteColumn", settings.Tools.ColumnPasteColumn);
                textWriter.WriteElementString("ColumnPasteOverwriteMode", settings.Tools.ColumnPasteOverwriteMode);
                textWriter.WriteElementString("AssaAttachmentFontTextPreview", settings.Tools.AssaAttachmentFontTextPreview);
                textWriter.WriteElementString("AssaSetPositionTarget", settings.Tools.AssaSetPositionTarget);
                textWriter.WriteElementString("VisualSyncStartSize", settings.Tools.VisualSyncStartSize);
                textWriter.WriteElementString("BlankVideoColor", ToHtml(settings.Tools.BlankVideoColor));
                textWriter.WriteElementString("BlankVideoUseCheckeredImage", settings.Tools.BlankVideoUseCheckeredImage.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BlankVideoMinutes", settings.Tools.BlankVideoMinutes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BlankVideoFrameRate", settings.Tools.BlankVideoFrameRate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaProgressBarBackColor", ToHtml(settings.Tools.AssaProgressBarBackColor));
                textWriter.WriteElementString("AssaProgressBarForeColor", ToHtml(settings.Tools.AssaProgressBarForeColor));
                textWriter.WriteElementString("AssaProgressBarTextColor", ToHtml(settings.Tools.AssaProgressBarTextColor));
                textWriter.WriteElementString("AssaProgressBarHeight", settings.Tools.AssaProgressBarHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaProgressBarSplitterWidth", settings.Tools.AssaProgressBarSplitterWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaProgressBarSplitterHeight", settings.Tools.AssaProgressBarSplitterHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaProgressBarFontName", settings.Tools.AssaProgressBarFontName);
                textWriter.WriteElementString("AssaProgressBarFontSize", settings.Tools.AssaProgressBarFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaProgressBarTopAlign", settings.Tools.AssaProgressBarTopAlign.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaProgressBarTextAlign", settings.Tools.AssaProgressBarTextAlign);
                textWriter.WriteElementString("AssaBgBoxPaddingLeft", settings.Tools.AssaBgBoxPaddingLeft.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxPaddingRight", settings.Tools.AssaBgBoxPaddingRight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxPaddingTop", settings.Tools.AssaBgBoxPaddingTop.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxPaddingBottom", settings.Tools.AssaBgBoxPaddingBottom.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxDrawingMarginH", settings.Tools.AssaBgBoxDrawingMarginH.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxDrawingMarginV", settings.Tools.AssaBgBoxDrawingMarginV.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxDrawingAlignment", settings.Tools.AssaBgBoxDrawingAlignment);
                textWriter.WriteElementString("AssaBgBoxColor", ToHtml(settings.Tools.AssaBgBoxColor));
                textWriter.WriteElementString("AssaBgBoxOutlineColor", ToHtml(settings.Tools.AssaBgBoxOutlineColor));
                textWriter.WriteElementString("AssaBgBoxShadowColor", ToHtml(settings.Tools.AssaBgBoxShadowColor));
                textWriter.WriteElementString("AssaBgBoxTransparentColor", ToHtml(settings.Tools.AssaBgBoxTransparentColor));
                textWriter.WriteElementString("AssaBgBoxStyle", settings.Tools.AssaBgBoxStyle);
                textWriter.WriteElementString("AssaBgBoxStyleRadius", settings.Tools.AssaBgBoxStyleRadius.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxStyleCircleAdjustY", settings.Tools.AssaBgBoxStyleCircleAdjustY.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxStyleSpikesStep", settings.Tools.AssaBgBoxStyleSpikesStep.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxStyleSpikesHeight", settings.Tools.AssaBgBoxStyleSpikesHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxStyleBubblesStep", settings.Tools.AssaBgBoxStyleBubblesStep.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxStyleBubblesHeight", settings.Tools.AssaBgBoxStyleBubblesHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxOutlineWidth", settings.Tools.AssaBgBoxOutlineWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxLayer", settings.Tools.AssaBgBoxLayer.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxDrawingFileWatch", settings.Tools.AssaBgBoxDrawingFileWatch.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxDrawingOnly", settings.Tools.AssaBgBoxDrawingOnly.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaBgBoxDrawing", settings.Tools.AssaBgBoxDrawing);
                textWriter.WriteElementString("GenVideoFontName", settings.Tools.GenVideoFontName);
                textWriter.WriteElementString("GenVideoFontBold", settings.Tools.GenVideoFontBold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenVideoOutline", settings.Tools.GenVideoOutline.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenVideoFontSize", settings.Tools.GenVideoFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenVideoEncoding", settings.Tools.GenVideoEncoding);
                textWriter.WriteElementString("GenVideoPreset", settings.Tools.GenVideoPreset);
                textWriter.WriteElementString("GenVideoCrf", settings.Tools.GenVideoCrf);
                textWriter.WriteElementString("GenVideoTune", settings.Tools.GenVideoTune);
                textWriter.WriteElementString("GenVideoAudioEncoding", settings.Tools.GenVideoAudioEncoding);
                textWriter.WriteElementString("GenVideoAudioForceStereo", settings.Tools.GenVideoAudioForceStereo.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenVideoAudioSampleRate", settings.Tools.GenVideoAudioSampleRate);
                textWriter.WriteElementString("GenVideoTargetFileSize", settings.Tools.GenVideoTargetFileSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenVideoFontSizePercentOfHeight", settings.Tools.GenVideoFontSizePercentOfHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenVideoNonAssaBox", settings.Tools.GenVideoNonAssaBox.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenVideoNonAssaBoxColor", ToHtml(settings.Tools.GenVideoNonAssaBoxColor));
                textWriter.WriteElementString("GenVideoNonAssaTextColor", ToHtml(settings.Tools.GenVideoNonAssaTextColor));
                textWriter.WriteElementString("GenVideoNonAssaAlignRight", settings.Tools.GenVideoNonAssaAlignRight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenVideoNonAssaFixRtlUnicode", settings.Tools.GenVideoNonAssaFixRtlUnicode.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenVideoEmbedOutputExt", settings.Tools.GenVideoEmbedOutputExt);
                textWriter.WriteElementString("GenVideoEmbedOutputSuffix", settings.Tools.GenVideoEmbedOutputSuffix);
                textWriter.WriteElementString("GenVideoEmbedOutputReplace", settings.Tools.GenVideoEmbedOutputReplace);
                textWriter.WriteElementString("GenVideoDeleteInputVideoFile", settings.Tools.GenVideoDeleteInputVideoFile.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenVideoUseOutputFolder", settings.Tools.GenVideoUseOutputFolder.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenVideoOutputFolder", settings.Tools.GenVideoOutputFolder);
                textWriter.WriteElementString("GenVideoOutputFileSuffix", settings.Tools.GenVideoOutputFileSuffix);
                textWriter.WriteElementString("VoskPostProcessing", settings.Tools.VoskPostProcessing.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VoskModel", settings.Tools.VoskModel);
                textWriter.WriteElementString("WhisperChoice", settings.Tools.WhisperChoice);
                textWriter.WriteElementString("WhisperIgnoreVersion", settings.Tools.WhisperIgnoreVersion.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WhisperDeleteTempFiles", settings.Tools.WhisperDeleteTempFiles.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WhisperModel", settings.Tools.WhisperModel);
                textWriter.WriteElementString("WhisperLocation", settings.Tools.WhisperLocation);
                textWriter.WriteElementString("WhisperCtranslate2Location", settings.Tools.WhisperCtranslate2Location);
                textWriter.WriteElementString("WhisperPurfviewFasterWhisperLocation", settings.Tools.WhisperPurfviewFasterWhisperLocation);
                textWriter.WriteElementString("WhisperPurfviewFasterWhisperDefaultCmd", settings.Tools.WhisperPurfviewFasterWhisperDefaultCmd);
                textWriter.WriteElementString("WhisperXLocation", settings.Tools.WhisperXLocation);
                textWriter.WriteElementString("WhisperStableTsLocation", settings.Tools.WhisperStableTsLocation);
                textWriter.WriteElementString("WhisperCppModelLocation", settings.Tools.WhisperCppModelLocation);
                textWriter.WriteElementString("WhisperExtraSettings", settings.Tools.WhisperExtraSettings);
                textWriter.WriteElementString("WhisperExtraSettingsHistory", settings.Tools.WhisperExtraSettingsHistory);
                textWriter.WriteElementString("WhisperLanguageCode", settings.Tools.WhisperLanguageCode);
                textWriter.WriteElementString("WhisperAutoAdjustTimings", settings.Tools.WhisperAutoAdjustTimings.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WhisperUseLineMaxChars", settings.Tools.WhisperUseLineMaxChars.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WhisperPostProcessingAddPeriods", settings.Tools.WhisperPostProcessingAddPeriods.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WhisperPostProcessingSplitLines", settings.Tools.WhisperPostProcessingSplitLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WhisperPostProcessingMergeLines", settings.Tools.WhisperPostProcessingMergeLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WhisperPostProcessingFixCasing", settings.Tools.WhisperPostProcessingFixCasing.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WhisperPostProcessingFixShortDuration", settings.Tools.WhisperPostProcessingFixShortDuration.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AudioToTextLineMaxChars", settings.Tools.AudioToTextLineMaxChars.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AudioToTextLineMaxCharsJp", settings.Tools.AudioToTextLineMaxCharsJp.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AudioToTextLineMaxCharsCn", settings.Tools.AudioToTextLineMaxCharsCn.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UnbreakLinesLongerThan", settings.Tools.UnbreakLinesLongerThan.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BreakLinesLongerThan", settings.Tools.BreakLinesLongerThan.ToString(CultureInfo.InvariantCulture));

                if (settings.Tools.FindHistory != null && settings.Tools.FindHistory.Count > 0)
                {
                    const int maximumFindHistoryItems = 10;
                    textWriter.WriteStartElement("FindHistory", string.Empty);
                    int maxIndex = settings.Tools.FindHistory.Count;
                    if (maxIndex > maximumFindHistoryItems)
                    {
                        maxIndex = maximumFindHistoryItems;
                    }

                    for (int index = 0; index < maxIndex; index++)
                    {
                        var text = settings.Tools.FindHistory[index];
                        textWriter.WriteElementString("Text", text);
                    }

                    textWriter.WriteEndElement();
                }

                textWriter.WriteEndElement();

                textWriter.WriteStartElement("SubtitleSettings", string.Empty);
                textWriter.WriteStartElement("AssaStyleStorageCategories", string.Empty);
                foreach (var category in settings.SubtitleSettings.AssaStyleStorageCategories)
                {
                    if (!string.IsNullOrEmpty(category?.Name))
                    {
                        textWriter.WriteStartElement("Category", string.Empty);
                        textWriter.WriteElementString("Name", category.Name);
                        textWriter.WriteElementString("IsDefault", category.IsDefault.ToString(CultureInfo.InvariantCulture));
                        foreach (var style in category.Styles)
                        {
                            textWriter.WriteStartElement("Style");
                            textWriter.WriteElementString("Name", style.Name);
                            textWriter.WriteElementString("FontName", style.FontName);
                            textWriter.WriteElementString("FontSize", style.FontSize.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("Bold", style.Bold.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("Italic", style.Italic.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("Underline", style.Underline.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("StrikeOut", style.Strikeout.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("Primary", style.Primary.ToArgb().ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("Secondary", style.Secondary.ToArgb().ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("Outline", style.Outline.ToArgb().ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("Background", style.Background.ToArgb().ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("ShadowWidth", style.ShadowWidth.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("OutlineWidth", style.OutlineWidth.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("Alignment", style.Alignment);
                            textWriter.WriteElementString("MarginLeft", style.MarginLeft.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("MarginRight", style.MarginRight.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("MarginVertical", style.MarginVertical.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("BorderStyle", style.BorderStyle);
                            textWriter.WriteElementString("ScaleX", style.ScaleX.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("ScaleY", style.ScaleY.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("Spacing", style.Spacing.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("Angle", style.Angle.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteEndElement();
                        }

                        textWriter.WriteEndElement();
                    }
                }

                textWriter.WriteEndElement();

                textWriter.WriteStartElement("AssaApplyOverrideTags", string.Empty);
                foreach (var tag in settings.SubtitleSettings.AssaOverrideTagHistory)
                {
                    textWriter.WriteElementString("Tag", tag);
                }

                textWriter.WriteEndElement();

                textWriter.WriteElementString("AssaResolutionAutoNew", settings.SubtitleSettings.AssaResolutionAutoNew.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaResolutionPromptChange", settings.SubtitleSettings.AssaResolutionPromptChange.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaShowPlayDepth", settings.SubtitleSettings.AssaShowPlayDepth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AssaShowScaledBorderAndShadow", settings.SubtitleSettings.AssaShowScaledBorderAndShadow.ToString(CultureInfo.InvariantCulture));

                textWriter.WriteElementString("DCinemaFontFile", settings.SubtitleSettings.DCinemaFontFile);
                textWriter.WriteElementString("DCinemaFontSize", settings.SubtitleSettings.DCinemaFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DCinemaBottomMargin", settings.SubtitleSettings.DCinemaBottomMargin.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DCinemaZPosition", settings.SubtitleSettings.DCinemaZPosition.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DCinemaFadeUpTime", settings.SubtitleSettings.DCinemaFadeUpTime.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DCinemaFadeDownTime", settings.SubtitleSettings.DCinemaFadeDownTime.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DCinemaAutoGenerateSubtitleId", settings.SubtitleSettings.DCinemaAutoGenerateSubtitleId.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SamiDisplayTwoClassesAsTwoSubtitles", settings.SubtitleSettings.SamiDisplayTwoClassesAsTwoSubtitles.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SamiHtmlEncodeMode", settings.SubtitleSettings.SamiHtmlEncodeMode.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TimedText10TimeCodeFormat", settings.SubtitleSettings.TimedText10TimeCodeFormat);
                textWriter.WriteElementString("TimedText10ShowStyleAndLanguage", settings.SubtitleSettings.TimedText10ShowStyleAndLanguage.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TimedText10FileExtension", settings.SubtitleSettings.TimedText10FileExtension);
                textWriter.WriteElementString("TimedTextItunesTopOrigin", settings.SubtitleSettings.TimedTextItunesTopOrigin);
                textWriter.WriteElementString("TimedTextItunesTopExtent", settings.SubtitleSettings.TimedTextItunesTopExtent);
                textWriter.WriteElementString("TimedTextItunesBottomOrigin", settings.SubtitleSettings.TimedTextItunesBottomOrigin);
                textWriter.WriteElementString("TimedTextItunesBottomExtent", settings.SubtitleSettings.TimedTextItunesBottomExtent);
                textWriter.WriteElementString("TimedTextItunesTimeCodeFormat", settings.SubtitleSettings.TimedTextItunesTimeCodeFormat);
                textWriter.WriteElementString("TimedTextItunesStyleAttribute", settings.SubtitleSettings.TimedTextItunesStyleAttribute);
                textWriter.WriteElementString("TimedTextImsc11TimeCodeFormat", settings.SubtitleSettings.TimedTextImsc11TimeCodeFormat);
                textWriter.WriteElementString("TimedTextImsc11FileExtension", settings.SubtitleSettings.TimedTextImsc11FileExtension);
                textWriter.WriteElementString("FcpFontSize", settings.SubtitleSettings.FcpFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FcpFontName", settings.SubtitleSettings.FcpFontName);
                textWriter.WriteElementString("Cavena890StartOfMessage", settings.SubtitleSettings.Cavena890StartOfMessage);
                textWriter.WriteElementString("EbuStlTeletextUseBox", settings.SubtitleSettings.EbuStlTeletextUseBox.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("EbuStlTeletextUseDoubleHeight", settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("EbuStlMarginTop", settings.SubtitleSettings.EbuStlMarginTop.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("EbuStlMarginBottom", settings.SubtitleSettings.EbuStlMarginBottom.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("EbuStlNewLineRows", settings.SubtitleSettings.EbuStlNewLineRows.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("EbuStlRemoveEmptyLines", settings.SubtitleSettings.EbuStlRemoveEmptyLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("PacVerticalTop", settings.SubtitleSettings.PacVerticalTop.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("PacVerticalCenter", settings.SubtitleSettings.PacVerticalCenter.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("PacVerticalBottom", settings.SubtitleSettings.PacVerticalBottom.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DvdStudioProHeader", settings.SubtitleSettings.DvdStudioProHeader.TrimEnd() + Environment.NewLine);
                textWriter.WriteElementString("TmpegEncXmlFontName", settings.SubtitleSettings.TmpegEncXmlFontName.TrimEnd());
                textWriter.WriteElementString("TmpegEncXmlFontHeight", settings.SubtitleSettings.TmpegEncXmlFontHeight.TrimEnd());
                textWriter.WriteElementString("TmpegEncXmlPosition", settings.SubtitleSettings.TmpegEncXmlPosition.TrimEnd());
                textWriter.WriteElementString("CheetahCaptionAlwayWriteEndTime", settings.SubtitleSettings.CheetahCaptionAlwayWriteEndTime.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("NuendoCharacterListFile", settings.SubtitleSettings.NuendoCharacterListFile);
                textWriter.WriteElementString("WebVttTimescale", settings.SubtitleSettings.WebVttTimescale.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WebVttCueAn1", settings.SubtitleSettings.WebVttCueAn1);
                textWriter.WriteElementString("WebVttCueAn2", settings.SubtitleSettings.WebVttCueAn2);
                textWriter.WriteElementString("WebVttCueAn3", settings.SubtitleSettings.WebVttCueAn3);
                textWriter.WriteElementString("WebVttCueAn4", settings.SubtitleSettings.WebVttCueAn4);
                textWriter.WriteElementString("WebVttCueAn5", settings.SubtitleSettings.WebVttCueAn5);
                textWriter.WriteElementString("WebVttCueAn6", settings.SubtitleSettings.WebVttCueAn6);
                textWriter.WriteElementString("WebVttCueAn7", settings.SubtitleSettings.WebVttCueAn7);
                textWriter.WriteElementString("WebVttCueAn8", settings.SubtitleSettings.WebVttCueAn8);
                textWriter.WriteElementString("WebVttCueAn9", settings.SubtitleSettings.WebVttCueAn9);
                textWriter.WriteElementString("MPlayer2Extension", settings.SubtitleSettings.MPlayer2Extension);
                textWriter.WriteElementString("TeletextItalicFix", settings.SubtitleSettings.TeletextItalicFix.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MccDebug", settings.SubtitleSettings.MccDebug.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BluRaySupSkipMerge", settings.SubtitleSettings.BluRaySupSkipMerge.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BluRaySupForceMergeAll", settings.SubtitleSettings.BluRaySupForceMergeAll.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WebVttUseXTimestampMap", settings.SubtitleSettings.WebVttUseXTimestampMap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WebVttUseMultipleXTimestampMap", settings.SubtitleSettings.WebVttUseMultipleXTimestampMap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WebVttMergeLinesWithSameText", settings.SubtitleSettings.WebVttMergeLinesWithSameText.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("Proxy", string.Empty);
                textWriter.WriteElementString("ProxyAddress", settings.Proxy.ProxyAddress);
                textWriter.WriteElementString("UserName", settings.Proxy.UserName);
                textWriter.WriteElementString("Password", settings.Proxy.Password);
                textWriter.WriteElementString("Domain", settings.Proxy.Domain);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("WordLists", string.Empty);
                textWriter.WriteElementString("LastLanguage", settings.WordLists.LastLanguage);
                textWriter.WriteElementString("Names", settings.WordLists.NamesUrl);
                textWriter.WriteElementString("UseOnlineNames", settings.WordLists.UseOnlineNames.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("FcpExportSettings", string.Empty);
                textWriter.WriteElementString("FontName", settings.FcpExportSettings.FontName);
                textWriter.WriteElementString("FontSize", settings.FcpExportSettings.FontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Alignment", settings.FcpExportSettings.Alignment);
                textWriter.WriteElementString("Baseline", settings.FcpExportSettings.Baseline.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Color", settings.FcpExportSettings.Color.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("CommonErrors", string.Empty);
                textWriter.WriteElementString("StartPosition", settings.CommonErrors.StartPosition);
                textWriter.WriteElementString("StartSize", settings.CommonErrors.StartSize);
                textWriter.WriteElementString("EmptyLinesTicked", settings.CommonErrors.EmptyLinesTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OverlappingDisplayTimeTicked", settings.CommonErrors.OverlappingDisplayTimeTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TooShortDisplayTimeTicked", settings.CommonErrors.TooShortDisplayTimeTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TooLongDisplayTimeTicked", settings.CommonErrors.TooLongDisplayTimeTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TooShortGapTicked", settings.CommonErrors.TooShortGapTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("InvalidItalicTagsTicked", settings.CommonErrors.InvalidItalicTagsTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BreakLongLinesTicked", settings.CommonErrors.BreakLongLinesTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeShortLinesTicked", settings.CommonErrors.MergeShortLinesTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeShortLinesAllTicked", settings.CommonErrors.MergeShortLinesAllTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeShortLinesPixelWidthTicked", settings.CommonErrors.MergeShortLinesPixelWidthTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UnneededSpacesTicked", settings.CommonErrors.UnneededSpacesTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UnneededPeriodsTicked", settings.CommonErrors.UnneededPeriodsTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixCommasTicked", settings.CommonErrors.FixCommasTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MissingSpacesTicked", settings.CommonErrors.MissingSpacesTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AddMissingQuotesTicked", settings.CommonErrors.AddMissingQuotesTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Fix3PlusLinesTicked", settings.CommonErrors.Fix3PlusLinesTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixHyphensTicked", settings.CommonErrors.FixHyphensTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixHyphensRemoveSingleLineTicked", settings.CommonErrors.FixHyphensRemoveSingleLineTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UppercaseIInsideLowercaseWordTicked", settings.CommonErrors.UppercaseIInsideLowercaseWordTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DoubleApostropheToQuoteTicked", settings.CommonErrors.DoubleApostropheToQuoteTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AddPeriodAfterParagraphTicked", settings.CommonErrors.AddPeriodAfterParagraphTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("StartWithUppercaseLetterAfterParagraphTicked", settings.CommonErrors.StartWithUppercaseLetterAfterParagraphTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("StartWithUppercaseLetterAfterPeriodInsideParagraphTicked", settings.CommonErrors.StartWithUppercaseLetterAfterPeriodInsideParagraphTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("StartWithUppercaseLetterAfterColonTicked", settings.CommonErrors.StartWithUppercaseLetterAfterColonTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AloneLowercaseIToUppercaseIEnglishTicked", settings.CommonErrors.AloneLowercaseIToUppercaseIEnglishTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixOcrErrorsViaReplaceListTicked", settings.CommonErrors.FixOcrErrorsViaReplaceListTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveSpaceBetweenNumberTicked", settings.CommonErrors.RemoveSpaceBetweenNumberTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixDialogsOnOneLineTicked", settings.CommonErrors.FixDialogsOnOneLineTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveDialogFirstLineInNonDialogs", settings.CommonErrors.RemoveDialogFirstLineInNonDialogs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TurkishAnsiTicked", settings.CommonErrors.TurkishAnsiTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DanishLetterITicked", settings.CommonErrors.DanishLetterITicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpanishInvertedQuestionAndExclamationMarksTicked", settings.CommonErrors.SpanishInvertedQuestionAndExclamationMarksTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixDoubleDashTicked", settings.CommonErrors.FixDoubleDashTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixDoubleGreaterThanTicked", settings.CommonErrors.FixDoubleGreaterThanTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixEllipsesStartTicked", settings.CommonErrors.FixEllipsesStartTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixMissingOpenBracketTicked", settings.CommonErrors.FixMissingOpenBracketTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixMusicNotationTicked", settings.CommonErrors.FixMusicNotationTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixContinuationStyleTicked", settings.CommonErrors.FixContinuationStyleTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixUnnecessaryLeadingDotsTicked", settings.CommonErrors.FixUnnecessaryLeadingDotsTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("NormalizeStringsTicked", settings.CommonErrors.NormalizeStringsTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DefaultFixes", settings.CommonErrors.DefaultFixes);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("VideoControls", string.Empty);
                textWriter.WriteElementString("CustomSearchText1", settings.VideoControls.CustomSearchText1);
                textWriter.WriteElementString("CustomSearchText2", settings.VideoControls.CustomSearchText2);
                textWriter.WriteElementString("CustomSearchText3", settings.VideoControls.CustomSearchText3);
                textWriter.WriteElementString("CustomSearchText4", settings.VideoControls.CustomSearchText4);
                textWriter.WriteElementString("CustomSearchText5", settings.VideoControls.CustomSearchText5);
                textWriter.WriteElementString("CustomSearchUrl1", settings.VideoControls.CustomSearchUrl1);
                textWriter.WriteElementString("CustomSearchUrl2", settings.VideoControls.CustomSearchUrl2);
                textWriter.WriteElementString("CustomSearchUrl3", settings.VideoControls.CustomSearchUrl3);
                textWriter.WriteElementString("CustomSearchUrl4", settings.VideoControls.CustomSearchUrl4);
                textWriter.WriteElementString("CustomSearchUrl5", settings.VideoControls.CustomSearchUrl5);
                textWriter.WriteElementString("LastActiveTab", settings.VideoControls.LastActiveTab);
                textWriter.WriteElementString("WaveformDrawGrid", settings.VideoControls.WaveformDrawGrid.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformDrawCps", settings.VideoControls.WaveformDrawCps.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformDrawWpm", settings.VideoControls.WaveformDrawWpm.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformAllowOverlap", settings.VideoControls.WaveformAllowOverlap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformFocusOnMouseEnter", settings.VideoControls.WaveformFocusOnMouseEnter.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformListViewFocusOnMouseEnter", settings.VideoControls.WaveformListViewFocusOnMouseEnter.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformSetVideoPositionOnMoveStartEnd", settings.VideoControls.WaveformSetVideoPositionOnMoveStartEnd.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformSingleClickSelect", settings.VideoControls.WaveformSingleClickSelect.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformSnapToShotChanges", settings.VideoControls.WaveformSnapToShotChanges.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformShotChangeStartTimeBeforeMs", settings.VideoControls.WaveformShotChangeStartTimeBeforeMs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformShotChangeStartTimeAfterMs", settings.VideoControls.WaveformShotChangeStartTimeAfterMs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformShotChangeEndTimeBeforeMs", settings.VideoControls.WaveformShotChangeEndTimeBeforeMs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformShotChangeEndTimeAfterMs", settings.VideoControls.WaveformShotChangeEndTimeAfterMs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformBorderHitMs", settings.VideoControls.WaveformBorderHitMs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformGridColor", settings.VideoControls.WaveformGridColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformColor", settings.VideoControls.WaveformColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformSelectedColor", settings.VideoControls.WaveformSelectedColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformBackgroundColor", settings.VideoControls.WaveformBackgroundColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformTextColor", settings.VideoControls.WaveformTextColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformCursorColor", settings.VideoControls.WaveformCursorColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformChaptersColor", settings.VideoControls.WaveformChaptersColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformTextSize", settings.VideoControls.WaveformTextSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformTextBold", settings.VideoControls.WaveformTextBold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformDoubleClickOnNonParagraphAction", settings.VideoControls.WaveformDoubleClickOnNonParagraphAction);
                textWriter.WriteElementString("WaveformRightClickOnNonParagraphAction", settings.VideoControls.WaveformRightClickOnNonParagraphAction);
                textWriter.WriteElementString("WaveformMouseWheelScrollUpIsForward", settings.VideoControls.WaveformMouseWheelScrollUpIsForward.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenerateSpectrogram", settings.VideoControls.GenerateSpectrogram.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformLabelShowCodec", settings.VideoControls.WaveformLabelShowCodec.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpectrogramAppearance", settings.VideoControls.SpectrogramAppearance);
                textWriter.WriteElementString("WaveformMinimumSampleRate", settings.VideoControls.WaveformMinimumSampleRate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformSeeksSilenceDurationSeconds", settings.VideoControls.WaveformSeeksSilenceDurationSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformSeeksSilenceMaxVolume", settings.VideoControls.WaveformSeeksSilenceMaxVolume.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformUnwrapText", settings.VideoControls.WaveformUnwrapText.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformHideWpmCpsLabels", settings.VideoControls.WaveformHideWpmCpsLabels.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("NetworkSettings", string.Empty);
                textWriter.WriteElementString("SessionKey", settings.NetworkSettings.SessionKey);
                textWriter.WriteElementString("UserName", settings.NetworkSettings.UserName);
                textWriter.WriteElementString("WebApiUrl", settings.NetworkSettings.WebApiUrl);
                textWriter.WriteElementString("PollIntervalSeconds", settings.NetworkSettings.PollIntervalSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("NewMessageSound", settings.NetworkSettings.NewMessageSound);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("VobSubOcr", string.Empty);
                textWriter.WriteElementString("XOrMorePixelsMakesSpace", settings.VobSubOcr.XOrMorePixelsMakesSpace.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AllowDifferenceInPercent", settings.VobSubOcr.AllowDifferenceInPercent.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BlurayAllowDifferenceInPercent", settings.VobSubOcr.BlurayAllowDifferenceInPercent.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LastImageCompareFolder", settings.VobSubOcr.LastImageCompareFolder);
                textWriter.WriteElementString("LastModiLanguageId", settings.VobSubOcr.LastModiLanguageId.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LastOcrMethod", settings.VobSubOcr.LastOcrMethod);
                textWriter.WriteElementString("TesseractLastLanguage", settings.VobSubOcr.TesseractLastLanguage);
                textWriter.WriteElementString("UseTesseractFallback", settings.VobSubOcr.UseTesseractFallback.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UseItalicsInTesseract", settings.VobSubOcr.UseItalicsInTesseract.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TesseractEngineMode", settings.VobSubOcr.TesseractEngineMode.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UseMusicSymbolsInTesseract", settings.VobSubOcr.UseMusicSymbolsInTesseract.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RightToLeft", settings.VobSubOcr.RightToLeft.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TopToBottom", settings.VobSubOcr.TopToBottom.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DefaultMillisecondsForUnknownDurations", settings.VobSubOcr.DefaultMillisecondsForUnknownDurations.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixOcrErrors", settings.VobSubOcr.FixOcrErrors.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("PromptForUnknownWords", settings.VobSubOcr.PromptForUnknownWords.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GuessUnknownWords", settings.VobSubOcr.GuessUnknownWords.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBreakSubtitleIfMoreThanTwoLines", settings.VobSubOcr.AutoBreakSubtitleIfMoreThanTwoLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ItalicFactor", settings.VobSubOcr.ItalicFactor.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrDraw", settings.VobSubOcr.LineOcrDraw.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrMinHeightSplit", settings.VobSubOcr.LineOcrMinHeightSplit.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrAdvancedItalic", settings.VobSubOcr.LineOcrAdvancedItalic.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrLastLanguages", settings.VobSubOcr.LineOcrLastLanguages);
                textWriter.WriteElementString("LineOcrLastSpellCheck", settings.VobSubOcr.LineOcrLastSpellCheck);
                textWriter.WriteElementString("LineOcrLinesToAutoGuess", settings.VobSubOcr.LineOcrLinesToAutoGuess.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrMinLineHeight", settings.VobSubOcr.LineOcrMinLineHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrMaxLineHeight", settings.VobSubOcr.LineOcrMaxLineHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrMaxErrorPixels", settings.VobSubOcr.LineOcrMaxErrorPixels.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LastBinaryImageCompareDb", settings.VobSubOcr.LastBinaryImageCompareDb);
                textWriter.WriteElementString("LastBinaryImageSpellCheck", settings.VobSubOcr.LastBinaryImageSpellCheck);
                textWriter.WriteElementString("BinaryAutoDetectBestDb", settings.VobSubOcr.BinaryAutoDetectBestDb.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LastTesseractSpellCheck", settings.VobSubOcr.LastTesseractSpellCheck);
                textWriter.WriteElementString("CaptureTopAlign", settings.VobSubOcr.CaptureTopAlign.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UnfocusedAttentionBlinkCount", settings.VobSubOcr.UnfocusedAttentionBlinkCount.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UnfocusedAttentionPlaySoundCount", settings.VobSubOcr.UnfocusedAttentionPlaySoundCount.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CloudVisionApiKey", settings.VobSubOcr.CloudVisionApiKey);
                textWriter.WriteElementString("CloudVisionLanguage", settings.VobSubOcr.CloudVisionLanguage);
                textWriter.WriteElementString("CloudVisionSendOriginalImages", settings.VobSubOcr.CloudVisionSendOriginalImages.ToString(CultureInfo.InvariantCulture));

                textWriter.WriteEndElement();

                textWriter.WriteStartElement("MultipleSearchAndReplaceGroups", string.Empty);
                foreach (var group in settings.MultipleSearchAndReplaceGroups)
                {
                    if (!string.IsNullOrEmpty(group?.Name))
                    {
                        textWriter.WriteStartElement("Group", string.Empty);
                        textWriter.WriteElementString("Name", group.Name);
                        textWriter.WriteElementString("Enabled", group.Enabled.ToString(CultureInfo.InvariantCulture));
                        foreach (var item in group.Rules)
                        {
                            textWriter.WriteStartElement("Rule", string.Empty);
                            textWriter.WriteElementString("Enabled", item.Enabled.ToString(CultureInfo.InvariantCulture));
                            textWriter.WriteElementString("FindWhat", item.FindWhat);
                            textWriter.WriteElementString("ReplaceWith", item.ReplaceWith);
                            textWriter.WriteElementString("SearchType", item.SearchType);
                            textWriter.WriteElementString("Description", item.Description);
                            textWriter.WriteEndElement();
                        }

                        textWriter.WriteEndElement();
                    }
                }

                textWriter.WriteEndElement();

                WriteShortcuts(settings.Shortcuts, textWriter);

                textWriter.WriteStartElement("RemoveTextForHearingImpaired", string.Empty);
                textWriter.WriteElementString("RemoveTextBetweenBrackets", settings.RemoveTextForHearingImpaired.RemoveTextBetweenBrackets.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBetweenParentheses", settings.RemoveTextForHearingImpaired.RemoveTextBetweenParentheses.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBetweenCurlyBrackets", settings.RemoveTextForHearingImpaired.RemoveTextBetweenCurlyBrackets.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBetweenQuestionMarks", settings.RemoveTextForHearingImpaired.RemoveTextBetweenQuestionMarks.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBetweenCustom", settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustom.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBetweenCustomBefore", settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomBefore);
                textWriter.WriteElementString("RemoveTextBetweenCustomAfter", settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomAfter);
                textWriter.WriteElementString("RemoveTextBetweenOnlySeparateLines", settings.RemoveTextForHearingImpaired.RemoveTextBetweenOnlySeparateLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBeforeColon", settings.RemoveTextForHearingImpaired.RemoveTextBeforeColon.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBeforeColonOnlyIfUppercase", settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyIfUppercase.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBeforeColonOnlyOnSeparateLine", settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyOnSeparateLine.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveInterjections", settings.RemoveTextForHearingImpaired.RemoveInterjections.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveInterjectionsOnlyOnSeparateLine", settings.RemoveTextForHearingImpaired.RemoveInterjectionsOnlyOnSeparateLine.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveIfAllUppercase", settings.RemoveTextForHearingImpaired.RemoveIfAllUppercase.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveIfContains", settings.RemoveTextForHearingImpaired.RemoveIfContains.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveIfContainsText", settings.RemoveTextForHearingImpaired.RemoveIfContainsText);
                textWriter.WriteElementString("RemoveIfOnlyMusicSymbols", settings.RemoveTextForHearingImpaired.RemoveIfOnlyMusicSymbols.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("SubtitleBeaming", string.Empty);
                textWriter.WriteElementString("FontName", settings.SubtitleBeaming.FontName);
                textWriter.WriteElementString("FontColor", settings.SubtitleBeaming.FontColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FontSize", settings.SubtitleBeaming.FontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BorderColor", settings.SubtitleBeaming.BorderColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BorderWidth", settings.SubtitleBeaming.BorderWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("BeautifyTimeCodes", string.Empty);
                textWriter.WriteElementString("AlignTimeCodes", settings.BeautifyTimeCodes.AlignTimeCodes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExtractExactTimeCodes", settings.BeautifyTimeCodes.ExtractExactTimeCodes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SnapToShotChanges", settings.BeautifyTimeCodes.SnapToShotChanges.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OverlapThreshold", settings.BeautifyTimeCodes.OverlapThreshold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteStartElement("Profile", string.Empty);
                textWriter.WriteElementString("Gap", settings.BeautifyTimeCodes.Profile.Gap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("InCuesGap", settings.BeautifyTimeCodes.Profile.InCuesGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("InCuesLeftGreenZone", settings.BeautifyTimeCodes.Profile.InCuesLeftGreenZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("InCuesLeftRedZone", settings.BeautifyTimeCodes.Profile.InCuesLeftRedZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("InCuesRightRedZone", settings.BeautifyTimeCodes.Profile.InCuesRightRedZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("InCuesRightGreenZone", settings.BeautifyTimeCodes.Profile.InCuesRightGreenZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OutCuesGap", settings.BeautifyTimeCodes.Profile.OutCuesGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OutCuesLeftGreenZone", settings.BeautifyTimeCodes.Profile.OutCuesLeftGreenZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OutCuesLeftRedZone", settings.BeautifyTimeCodes.Profile.OutCuesLeftRedZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OutCuesRightRedZone", settings.BeautifyTimeCodes.Profile.OutCuesRightRedZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OutCuesRightGreenZone", settings.BeautifyTimeCodes.Profile.OutCuesRightGreenZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ConnectedSubtitlesInCueClosestLeftGap", settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesInCueClosestLeftGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ConnectedSubtitlesInCueClosestRightGap", settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesInCueClosestRightGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ConnectedSubtitlesOutCueClosestLeftGap", settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesOutCueClosestLeftGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ConnectedSubtitlesOutCueClosestRightGap", settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesOutCueClosestRightGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ConnectedSubtitlesLeftGreenZone", settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesLeftGreenZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ConnectedSubtitlesLeftRedZone", settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesLeftRedZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ConnectedSubtitlesRightRedZone", settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesRightRedZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ConnectedSubtitlesRightGreenZone", settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesRightGreenZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ConnectedSubtitlesTreatConnected", settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesTreatConnected.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChainingGeneralUseZones", settings.BeautifyTimeCodes.Profile.ChainingGeneralUseZones.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChainingGeneralMaxGap", settings.BeautifyTimeCodes.Profile.ChainingGeneralMaxGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChainingGeneralLeftGreenZone", settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftGreenZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChainingGeneralLeftRedZone", settings.BeautifyTimeCodes.Profile.ChainingGeneralLeftRedZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChainingGeneralShotChangeBehavior", settings.BeautifyTimeCodes.Profile.ChainingGeneralShotChangeBehavior.ToString());
                textWriter.WriteElementString("ChainingInCueOnShotUseZones", settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotUseZones.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChainingInCueOnShotMaxGap", settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotMaxGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChainingInCueOnShotLeftGreenZone", settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotLeftGreenZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChainingInCueOnShotLeftRedZone", settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotLeftRedZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChainingInCueOnShotShotChangeBehavior", settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotShotChangeBehavior.ToString());
                textWriter.WriteElementString("ChainingInCueOnShotCheckGeneral", settings.BeautifyTimeCodes.Profile.ChainingInCueOnShotCheckGeneral.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChainingOutCueOnShotUseZones", settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotUseZones.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChainingOutCueOnShotMaxGap", settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotMaxGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChainingOutCueOnShotRightRedZone", settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotRightRedZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChainingOutCueOnShotRightGreenZone", settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotRightGreenZone.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ChainingOutCueOnShotShotChangeBehavior", settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotShotChangeBehavior.ToString());
                textWriter.WriteElementString("ChainingOutCueOnShotCheckGeneral", settings.BeautifyTimeCodes.Profile.ChainingOutCueOnShotCheckGeneral.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteEndElement();
                textWriter.WriteEndElement();

                textWriter.WriteEndElement();

                textWriter.WriteEndDocument();
                textWriter.Flush();

                return sb.ToString().Replace("encoding=\"utf-16\"", "encoding=\"utf-8\"");
            }
        }

        public static void CustomSerialize(string fileName, Settings settings)
        {
            try
            {
                File.WriteAllText(fileName, CustomSerialize(settings), Encoding.UTF8);
            }
            catch
            {
                // ignored
            }
        }

        public static string ToHtml(Color c)
        {
            return Utilities.ColorToHexWithTransparency(c);
        }

        public static Color FromHtml(string hex)
        {
            var s = hex.Trim().TrimStart('#');

            if (s.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
            {
                var arr = s.Remove(0, 4).TrimEnd(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length >= 3)
                {
                    try
                    {
                        return Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                    }
                    catch
                    {
                        return Color.White;
                    }
                }

                return Color.White;
            }


            if (s.Length == 6)
            {
                try
                {
                    return ColorTranslator.FromHtml("#" + s);
                }
                catch
                {
                    return Color.White;
                }
            }

            if (s.Length == 8)
            {
                if (UseLegacyHtmlColor && IsVersion3)
                {
                    if (!int.TryParse(s.Substring(0, 2), NumberStyles.HexNumber, null, out var alpha))
                    {
                        alpha = 255; // full solid color
                    }

                    s = s.Substring(2);
                    try
                    {
                        var c = HtmlUtil.GetColorFromString("#" + s);
                        return Color.FromArgb(alpha, c);
                    }
                    catch
                    {
                        return Color.White;
                    }
                }
                else
                {
                    if (!int.TryParse(s.Substring(6, 2), NumberStyles.HexNumber, null, out var alpha))
                    {
                        alpha = 255; // full solid color
                    }

                    s = s.Substring(0, 6);
                    try
                    {
                        var c = HtmlUtil.GetColorFromString("#" + s);
                        return Color.FromArgb(alpha, c);
                    }
                    catch
                    {
                        return Color.White;
                    }
                }
            }

            return Color.White;
        }

        internal static void WriteShortcuts(Shortcuts shortcuts, XmlWriter textWriter)
        {
            textWriter.WriteStartElement("Shortcuts", string.Empty);

            textWriter.WriteStartElement("Plugins", string.Empty);
            foreach (var shortcut in shortcuts.PluginShortcuts)
            {
                textWriter.WriteStartElement("Plugin");
                textWriter.WriteElementString("Name", shortcut.Name);
                textWriter.WriteElementString("Shortcut", shortcut.Shortcut);
                textWriter.WriteEndElement();
            }
            textWriter.WriteEndElement();

            textWriter.WriteElementString("GeneralGoToFirstSelectedLine", shortcuts.GeneralGoToFirstSelectedLine);
            textWriter.WriteElementString("GeneralGoToNextEmptyLine", shortcuts.GeneralGoToNextEmptyLine);
            textWriter.WriteElementString("GeneralMergeSelectedLines", shortcuts.GeneralMergeSelectedLines);
            textWriter.WriteElementString("GeneralMergeSelectedLinesAndAutoBreak", shortcuts.GeneralMergeSelectedLinesAndAutoBreak);
            textWriter.WriteElementString("GeneralMergeSelectedLinesAndUnbreak", shortcuts.GeneralMergeSelectedLinesAndUnbreak);
            textWriter.WriteElementString("GeneralMergeSelectedLinesAndUnbreakCjk", shortcuts.GeneralMergeSelectedLinesAndUnbreakCjk);
            textWriter.WriteElementString("GeneralMergeSelectedLinesOnlyFirstText", shortcuts.GeneralMergeSelectedLinesOnlyFirstText);
            textWriter.WriteElementString("GeneralMergeSelectedLinesBilingual", shortcuts.GeneralMergeSelectedLinesBilingual);
            textWriter.WriteElementString("GeneralMergeWithPreviousBilingual", shortcuts.GeneralMergeWithPreviousBilingual);
            textWriter.WriteElementString("GeneralMergeWithNextBilingual", shortcuts.GeneralMergeWithNextBilingual);
            textWriter.WriteElementString("GeneralMergeWithNext", shortcuts.GeneralMergeWithNext);
            textWriter.WriteElementString("GeneralMergeWithPrevious", shortcuts.GeneralMergeWithPrevious);
            textWriter.WriteElementString("GeneralMergeWithPreviousAndUnbreak", shortcuts.GeneralMergeWithPreviousAndUnbreak);
            textWriter.WriteElementString("GeneralMergeWithNextAndUnbreak", shortcuts.GeneralMergeWithNextAndUnbreak);
            textWriter.WriteElementString("GeneralMergeWithPreviousAndBreak", shortcuts.GeneralMergeWithPreviousAndBreak);
            textWriter.WriteElementString("GeneralMergeWithNextAndBreak", shortcuts.GeneralMergeWithNextAndBreak);
            textWriter.WriteElementString("GeneralToggleTranslationMode", shortcuts.GeneralToggleTranslationMode);
            textWriter.WriteElementString("GeneralSwitchOriginalAndTranslation", shortcuts.GeneralSwitchOriginalAndTranslation);
            textWriter.WriteElementString("GeneralSwitchOriginalAndTranslationTextBoxes", shortcuts.GeneralSwitchOriginalAndTranslationTextBoxes);
            textWriter.WriteElementString("GeneralLayoutChoose", shortcuts.GeneralLayoutChoose);
            textWriter.WriteElementString("GeneralLayoutChoose1", shortcuts.GeneralLayoutChoose1);
            textWriter.WriteElementString("GeneralLayoutChoose2", shortcuts.GeneralLayoutChoose2);
            textWriter.WriteElementString("GeneralLayoutChoose3", shortcuts.GeneralLayoutChoose3);
            textWriter.WriteElementString("GeneralLayoutChoose4", shortcuts.GeneralLayoutChoose4);
            textWriter.WriteElementString("GeneralLayoutChoose5", shortcuts.GeneralLayoutChoose5);
            textWriter.WriteElementString("GeneralLayoutChoose6", shortcuts.GeneralLayoutChoose6);
            textWriter.WriteElementString("GeneralLayoutChoose7", shortcuts.GeneralLayoutChoose7);
            textWriter.WriteElementString("GeneralLayoutChoose8", shortcuts.GeneralLayoutChoose8);
            textWriter.WriteElementString("GeneralLayoutChoose9", shortcuts.GeneralLayoutChoose9);
            textWriter.WriteElementString("GeneralLayoutChoose10", shortcuts.GeneralLayoutChoose10);
            textWriter.WriteElementString("GeneralLayoutChoose11", shortcuts.GeneralLayoutChoose11);
            textWriter.WriteElementString("GeneralLayoutChoose12", shortcuts.GeneralLayoutChoose12);
            textWriter.WriteElementString("GeneralMergeOriginalAndTranslation", shortcuts.GeneralMergeOriginalAndTranslation);
            textWriter.WriteElementString("GeneralGoToNextSubtitle", shortcuts.GeneralGoToNextSubtitle);
            textWriter.WriteElementString("GeneralGoToNextSubtitlePlayTranslate", shortcuts.GeneralGoToNextSubtitlePlayTranslate);
            textWriter.WriteElementString("GeneralGoToNextSubtitleCursorAtEnd", shortcuts.GeneralGoToNextSubtitleCursorAtEnd);
            textWriter.WriteElementString("GeneralGoToPrevSubtitle", shortcuts.GeneralGoToPrevSubtitle);
            textWriter.WriteElementString("GeneralGoToPrevSubtitlePlayTranslate", shortcuts.GeneralGoToPrevSubtitlePlayTranslate);
            textWriter.WriteElementString("GeneralGoToEndOfCurrentSubtitle", shortcuts.GeneralGoToEndOfCurrentSubtitle);
            textWriter.WriteElementString("GeneralGoToStartOfCurrentSubtitle", shortcuts.GeneralGoToStartOfCurrentSubtitle);
            textWriter.WriteElementString("GeneralGoToPreviousSubtitleAndFocusVideo", shortcuts.GeneralGoToPreviousSubtitleAndFocusVideo);
            textWriter.WriteElementString("GeneralGoToNextSubtitleAndFocusVideo", shortcuts.GeneralGoToNextSubtitleAndFocusVideo);
            textWriter.WriteElementString("GeneralGoToPreviousSubtitleAndFocusWaveform", shortcuts.GeneralGoToPreviousSubtitleAndFocusWaveform);
            textWriter.WriteElementString("GeneralGoToNextSubtitleAndFocusWaveform", shortcuts.GeneralGoToNextSubtitleAndFocusWaveform);
            textWriter.WriteElementString("GeneralGoToPrevSubtitleAndPlay", shortcuts.GeneralGoToPrevSubtitleAndPlay);
            textWriter.WriteElementString("GeneralGoToNextSubtitleAndPlay", shortcuts.GeneralGoToNextSubtitleAndPlay);
            textWriter.WriteElementString("GeneralAutoCalcCurrentDuration", shortcuts.GeneralAutoCalcCurrentDuration);
            textWriter.WriteElementString("GeneralAutoCalcCurrentDurationByOptimalReadingSpeed", shortcuts.GeneralAutoCalcCurrentDurationByOptimalReadingSpeed);
            textWriter.WriteElementString("GeneralAutoCalcCurrentDurationByMinReadingSpeed", shortcuts.GeneralAutoCalcCurrentDurationByMinReadingSpeed);
            textWriter.WriteElementString("GeneralPlayFirstSelected", shortcuts.GeneralPlayFirstSelected);
            textWriter.WriteElementString("GeneralToggleBookmarks", shortcuts.GeneralToggleBookmarks);
            textWriter.WriteElementString("GeneralToggleBookmarksWithText", shortcuts.GeneralToggleBookmarksWithText);
            textWriter.WriteElementString("GeneralEditBookmarks", shortcuts.GeneralEditBookmarks);
            textWriter.WriteElementString("GeneralClearBookmarks", shortcuts.GeneralClearBookmarks);
            textWriter.WriteElementString("GeneralGoToBookmark", shortcuts.GeneralGoToBookmark);
            textWriter.WriteElementString("GeneralGoToNextBookmark", shortcuts.GeneralGoToNextBookmark);
            textWriter.WriteElementString("GeneralGoToPreviousBookmark", shortcuts.GeneralGoToPreviousBookmark);
            textWriter.WriteElementString("GeneralChooseProfile", shortcuts.GeneralChooseProfile);
            textWriter.WriteElementString("OpenDataFolder", shortcuts.OpenDataFolder);
            textWriter.WriteElementString("OpenContainingFolder", shortcuts.OpenContainingFolder);
            textWriter.WriteElementString("GeneralDuplicateLine", shortcuts.GeneralDuplicateLine);
            textWriter.WriteElementString("GeneralToggleView", shortcuts.GeneralToggleView);
            textWriter.WriteElementString("GeneralToggleMode", shortcuts.GeneralToggleMode);
            textWriter.WriteElementString("GeneralTogglePreviewOnVideo", shortcuts.GeneralTogglePreviewOnVideo);
            textWriter.WriteElementString("GeneralHelp", shortcuts.GeneralHelp);
            textWriter.WriteElementString("GeneralFocusTextBox", shortcuts.GeneralFocusTextBox);
            textWriter.WriteElementString("MainFileNew", shortcuts.MainFileNew);
            textWriter.WriteElementString("MainFileOpen", shortcuts.MainFileOpen);
            textWriter.WriteElementString("MainFileOpenKeepVideo", shortcuts.MainFileOpenKeepVideo);
            textWriter.WriteElementString("MainFileSave", shortcuts.MainFileSave);
            textWriter.WriteElementString("MainFileSaveOriginal", shortcuts.MainFileSaveOriginal);
            textWriter.WriteElementString("MainFileSaveOriginalAs", shortcuts.MainFileSaveOriginalAs);
            textWriter.WriteElementString("MainFileSaveAs", shortcuts.MainFileSaveAs);
            textWriter.WriteElementString("MainFileCloseOriginal", shortcuts.MainFileCloseOriginal);
            textWriter.WriteElementString("MainFileCloseTranslation", shortcuts.MainFileCloseTranslation);
            textWriter.WriteElementString("MainFileCompare", shortcuts.MainFileCompare);
            textWriter.WriteElementString("MainFileVerifyCompleteness", shortcuts.MainFileVerifyCompleteness);
            textWriter.WriteElementString("MainFileOpenOriginal", shortcuts.MainFileOpenOriginal);
            textWriter.WriteElementString("MainFileSaveAll", shortcuts.MainFileSaveAll);
            textWriter.WriteElementString("MainFileImportPlainText", shortcuts.MainFileImportPlainText);
            textWriter.WriteElementString("MainFileImportBdSupForEdit", shortcuts.MainFileImportBdSupForEdit);
            textWriter.WriteElementString("MainFileImportTimeCodes", shortcuts.MainFileImportTimeCodes);
            textWriter.WriteElementString("MainFileExportPlainText", shortcuts.MainFileExportPlainText);
            textWriter.WriteElementString("MainFileExportEbu", shortcuts.MainFileExportEbu);
            textWriter.WriteElementString("MainFileExportPac", shortcuts.MainFileExportPac);
            textWriter.WriteElementString("MainFileExportBdSup", shortcuts.MainFileExportBdSup);
            textWriter.WriteElementString("MainFileExportEdlClip", shortcuts.MainFileExportEdlClip);
            textWriter.WriteElementString("MainFileExit", shortcuts.MainFileExit);
            textWriter.WriteElementString("MainEditUndo", shortcuts.MainEditUndo);
            textWriter.WriteElementString("MainEditRedo", shortcuts.MainEditRedo);
            textWriter.WriteElementString("MainEditFind", shortcuts.MainEditFind);
            textWriter.WriteElementString("MainEditFindNext", shortcuts.MainEditFindNext);
            textWriter.WriteElementString("MainEditReplace", shortcuts.MainEditReplace);
            textWriter.WriteElementString("MainEditMultipleReplace", shortcuts.MainEditMultipleReplace);
            textWriter.WriteElementString("MainEditGoToLineNumber", shortcuts.MainEditGoToLineNumber);
            textWriter.WriteElementString("MainEditRightToLeft", shortcuts.MainEditRightToLeft);
            textWriter.WriteElementString("MainToolsAdjustDuration", shortcuts.MainToolsAdjustDuration);
            textWriter.WriteElementString("MainToolsAdjustDurationLimits", shortcuts.MainToolsAdjustDurationLimits);
            textWriter.WriteElementString("MainToolsFixCommonErrors", shortcuts.MainToolsFixCommonErrors);
            textWriter.WriteElementString("MainToolsFixCommonErrorsPreview", shortcuts.MainToolsFixCommonErrorsPreview);
            textWriter.WriteElementString("MainToolsMergeShortLines", shortcuts.MainToolsMergeShortLines);
            textWriter.WriteElementString("MainToolsMergeDuplicateText", shortcuts.MainToolsMergeDuplicateText);
            textWriter.WriteElementString("MainToolsMergeSameTimeCodes", shortcuts.MainToolsMergeSameTimeCodes);
            textWriter.WriteElementString("MainToolsMakeEmptyFromCurrent", shortcuts.MainToolsMakeEmptyFromCurrent);
            textWriter.WriteElementString("MainToolsSplitLongLines", shortcuts.MainToolsSplitLongLines);
            textWriter.WriteElementString("MainToolsMinimumDisplayTimeBetweenParagraphs", shortcuts.MainToolsMinimumDisplayTimeBetweenParagraphs);
            textWriter.WriteElementString("MainToolsDurationsBridgeGap", shortcuts.MainToolsDurationsBridgeGap);
            textWriter.WriteElementString("MainToolsRenumber", shortcuts.MainToolsRenumber);
            textWriter.WriteElementString("MainToolsRemoveTextForHI", shortcuts.MainToolsRemoveTextForHI);
            textWriter.WriteElementString("MainToolsConvertColorsToDialog", shortcuts.MainToolsConvertColorsToDialog);
            textWriter.WriteElementString("MainToolsChangeCasing", shortcuts.MainToolsChangeCasing);
            textWriter.WriteElementString("MainToolsAutoDuration", shortcuts.MainToolsAutoDuration);
            textWriter.WriteElementString("MainToolsBatchConvert", shortcuts.MainToolsBatchConvert);
            textWriter.WriteElementString("MainToolsMeasurementConverter", shortcuts.MainToolsMeasurementConverter);
            textWriter.WriteElementString("MainToolsSplit", shortcuts.MainToolsSplit);
            textWriter.WriteElementString("MainToolsAppend", shortcuts.MainToolsAppend);
            textWriter.WriteElementString("MainToolsJoin", shortcuts.MainToolsJoin);
            textWriter.WriteElementString("MainToolsStyleManager", shortcuts.MainToolsStyleManager);
            textWriter.WriteElementString("MainEditToggleTranslationOriginalInPreviews", shortcuts.MainEditToggleTranslationOriginalInPreviews);
            textWriter.WriteElementString("MainEditInverseSelection", shortcuts.MainEditInverseSelection);
            textWriter.WriteElementString("MainEditModifySelection", shortcuts.MainEditModifySelection);
            textWriter.WriteElementString("MainVideoOpen", shortcuts.MainVideoOpen);
            textWriter.WriteElementString("MainVideoClose", shortcuts.MainVideoClose);
            textWriter.WriteElementString("MainVideoPause", shortcuts.MainVideoPause);
            textWriter.WriteElementString("MainVideoStop", shortcuts.MainVideoStop);
            textWriter.WriteElementString("MainVideoPlayFromJustBefore", shortcuts.MainVideoPlayFromJustBefore);
            textWriter.WriteElementString("MainVideoPlayFromBeginning", shortcuts.MainVideoPlayFromBeginning);
            textWriter.WriteElementString("MainVideoPlayPauseToggle", shortcuts.MainVideoPlayPauseToggle);
            textWriter.WriteElementString("MainVideoPlay150Speed", shortcuts.MainVideoPlay150Speed);
            textWriter.WriteElementString("MainVideoPlay200Speed", shortcuts.MainVideoPlay200Speed);
            textWriter.WriteElementString("MainVideoFocusSetVideoPosition", shortcuts.MainVideoFocusSetVideoPosition);
            textWriter.WriteElementString("MainVideoToggleVideoControls", shortcuts.MainVideoToggleVideoControls);
            textWriter.WriteElementString("MainVideo1FrameLeft", shortcuts.MainVideo1FrameLeft);
            textWriter.WriteElementString("MainVideo1FrameRight", shortcuts.MainVideo1FrameRight);
            textWriter.WriteElementString("MainVideo1FrameLeftWithPlay", shortcuts.MainVideo1FrameLeftWithPlay);
            textWriter.WriteElementString("MainVideo1FrameRightWithPlay", shortcuts.MainVideo1FrameRightWithPlay);
            textWriter.WriteElementString("MainVideo100MsLeft", shortcuts.MainVideo100MsLeft);
            textWriter.WriteElementString("MainVideo100MsRight", shortcuts.MainVideo100MsRight);
            textWriter.WriteElementString("MainVideo500MsLeft", shortcuts.MainVideo500MsLeft);
            textWriter.WriteElementString("MainVideo500MsRight", shortcuts.MainVideo500MsRight);
            textWriter.WriteElementString("MainVideo1000MsLeft", shortcuts.MainVideo1000MsLeft);
            textWriter.WriteElementString("MainVideo1000MsRight", shortcuts.MainVideo1000MsRight);
            textWriter.WriteElementString("MainVideo5000MsLeft", shortcuts.MainVideo5000MsLeft);
            textWriter.WriteElementString("MainVideo5000MsRight", shortcuts.MainVideo5000MsRight);
            textWriter.WriteElementString("MainVideoXSMsLeft", shortcuts.MainVideoXSMsLeft);
            textWriter.WriteElementString("MainVideoXSMsRight", shortcuts.MainVideoXSMsRight);
            textWriter.WriteElementString("MainVideoXLMsLeft", shortcuts.MainVideoXLMsLeft);
            textWriter.WriteElementString("MainVideoXLMsRight", shortcuts.MainVideoXLMsRight);
            textWriter.WriteElementString("MainVideo3000MsLeft", shortcuts.MainVideo3000MsLeft);
            textWriter.WriteElementString("MainVideo3000MsRight", shortcuts.MainVideo3000MsRight);
            textWriter.WriteElementString("MainVideoGoToStartCurrent", shortcuts.MainVideoGoToStartCurrent);
            textWriter.WriteElementString("MainVideoToggleStartEndCurrent", shortcuts.MainVideoToggleStartEndCurrent);
            textWriter.WriteElementString("MainVideoPlaySelectedLines", shortcuts.MainVideoPlaySelectedLines);
            textWriter.WriteElementString("MainVideoLoopSelectedLines", shortcuts.MainVideoLoopSelectedLines);
            textWriter.WriteElementString("MainVideoGoToPrevSubtitle", shortcuts.MainVideoGoToPrevSubtitle);
            textWriter.WriteElementString("MainVideoGoToNextSubtitle", shortcuts.MainVideoGoToNextSubtitle);
            textWriter.WriteElementString("MainVideoGoToPrevTimeCode", shortcuts.MainVideoGoToPrevTimeCode);
            textWriter.WriteElementString("MainVideoGoToNextTimeCode", shortcuts.MainVideoGoToNextTimeCode);
            textWriter.WriteElementString("MainVideoGoToPrevChapter", shortcuts.MainVideoGoToPrevChapter);
            textWriter.WriteElementString("MainVideoGoToNextChapter", shortcuts.MainVideoGoToNextChapter);
            textWriter.WriteElementString("MainVideoSelectNextSubtitle", shortcuts.MainVideoSelectNextSubtitle);
            textWriter.WriteElementString("MainVideoFullscreen", shortcuts.MainVideoFullscreen);
            textWriter.WriteElementString("MainVideoSlower", shortcuts.MainVideoSlower);
            textWriter.WriteElementString("MainVideoFaster", shortcuts.MainVideoFaster);
            textWriter.WriteElementString("MainVideoSpeedToggle", shortcuts.MainVideoSpeedToggle);
            textWriter.WriteElementString("MainVideoReset", shortcuts.MainVideoReset);
            textWriter.WriteElementString("MainVideoToggleBrightness", shortcuts.MainVideoToggleBrightness);
            textWriter.WriteElementString("MainVideoToggleContrast", shortcuts.MainVideoToggleContrast);
            textWriter.WriteElementString("MainVideoAudioToTextVosk", shortcuts.MainVideoAudioToTextVosk);
            textWriter.WriteElementString("MainVideoAudioToTextWhisper", shortcuts.MainVideoAudioToTextWhisper);
            textWriter.WriteElementString("MainVideoAudioExtractAudioSelectedLines", shortcuts.MainVideoAudioExtractAudioSelectedLines);
            textWriter.WriteElementString("MainVideoTextToSpeech", shortcuts.MainVideoTextToSpeech);
            textWriter.WriteElementString("MainSpellCheck", shortcuts.MainSpellCheck);
            textWriter.WriteElementString("MainSpellCheckFindDoubleWords", shortcuts.MainSpellCheckFindDoubleWords);
            textWriter.WriteElementString("MainSpellCheckAddWordToNames", shortcuts.MainSpellCheckAddWordToNames);
            textWriter.WriteElementString("MainSynchronizationAdjustTimes", shortcuts.MainSynchronizationAdjustTimes);
            textWriter.WriteElementString("MainSynchronizationVisualSync", shortcuts.MainSynchronizationVisualSync);
            textWriter.WriteElementString("MainSynchronizationPointSync", shortcuts.MainSynchronizationPointSync);
            textWriter.WriteElementString("MainSynchronizationPointSyncViaFile", shortcuts.MainSynchronizationPointSyncViaFile);
            textWriter.WriteElementString("MainSynchronizationChangeFrameRate", shortcuts.MainSynchronizationChangeFrameRate);
            textWriter.WriteElementString("MainListViewItalic", shortcuts.MainListViewItalic);
            textWriter.WriteElementString("MainListViewBold", shortcuts.MainListViewBold);
            textWriter.WriteElementString("MainListViewUnderline", shortcuts.MainListViewUnderline);
            textWriter.WriteElementString("MainListViewBox", shortcuts.MainListViewBox);
            textWriter.WriteElementString("MainListViewToggleQuotes", shortcuts.MainListViewToggleQuotes);
            textWriter.WriteElementString("MainListViewToggleHiTags", shortcuts.MainListViewToggleHiTags);
            textWriter.WriteElementString("MainListViewToggleCustomTags", shortcuts.MainListViewToggleCustomTags);
            textWriter.WriteElementString("MainListViewSplit", shortcuts.MainListViewSplit);
            textWriter.WriteElementString("MainListViewToggleDashes", shortcuts.MainListViewToggleDashes);
            textWriter.WriteElementString("MainListViewToggleMusicSymbols", shortcuts.MainListViewToggleMusicSymbols);
            textWriter.WriteElementString("MainListViewAlignment", shortcuts.MainListViewAlignment);
            textWriter.WriteElementString("MainListViewAlignmentN1", shortcuts.MainListViewAlignmentN1);
            textWriter.WriteElementString("MainListViewAlignmentN2", shortcuts.MainListViewAlignmentN2);
            textWriter.WriteElementString("MainListViewAlignmentN3", shortcuts.MainListViewAlignmentN3);
            textWriter.WriteElementString("MainListViewAlignmentN4", shortcuts.MainListViewAlignmentN4);
            textWriter.WriteElementString("MainListViewAlignmentN5", shortcuts.MainListViewAlignmentN5);
            textWriter.WriteElementString("MainListViewAlignmentN6", shortcuts.MainListViewAlignmentN6);
            textWriter.WriteElementString("MainListViewAlignmentN7", shortcuts.MainListViewAlignmentN7);
            textWriter.WriteElementString("MainListViewAlignmentN8", shortcuts.MainListViewAlignmentN8);
            textWriter.WriteElementString("MainListViewAlignmentN9", shortcuts.MainListViewAlignmentN9);
            textWriter.WriteElementString("MainListViewColor1", shortcuts.MainListViewColor1);
            textWriter.WriteElementString("MainListViewColor2", shortcuts.MainListViewColor2);
            textWriter.WriteElementString("MainListViewColor3", shortcuts.MainListViewColor3);
            textWriter.WriteElementString("MainListViewColor4", shortcuts.MainListViewColor4);
            textWriter.WriteElementString("MainListViewColor5", shortcuts.MainListViewColor5);
            textWriter.WriteElementString("MainListViewColor6", shortcuts.MainListViewColor6);
            textWriter.WriteElementString("MainListViewColor7", shortcuts.MainListViewColor7);
            textWriter.WriteElementString("MainListViewColor8", shortcuts.MainListViewColor8);
            textWriter.WriteElementString("MainListViewSetNewActor", shortcuts.MainListViewSetNewActor);
            textWriter.WriteElementString("MainListViewSetActor1", shortcuts.MainListViewSetActor1);
            textWriter.WriteElementString("MainListViewSetActor2", shortcuts.MainListViewSetActor2);
            textWriter.WriteElementString("MainListViewSetActor3", shortcuts.MainListViewSetActor3);
            textWriter.WriteElementString("MainListViewSetActor4", shortcuts.MainListViewSetActor4);
            textWriter.WriteElementString("MainListViewSetActor5", shortcuts.MainListViewSetActor5);
            textWriter.WriteElementString("MainListViewSetActor6", shortcuts.MainListViewSetActor6);
            textWriter.WriteElementString("MainListViewSetActor7", shortcuts.MainListViewSetActor7);
            textWriter.WriteElementString("MainListViewSetActor8", shortcuts.MainListViewSetActor8);
            textWriter.WriteElementString("MainListViewSetActor9", shortcuts.MainListViewSetActor9);
            textWriter.WriteElementString("MainListViewSetActor10", shortcuts.MainListViewSetActor10);
            textWriter.WriteElementString("MainListViewColorChoose", shortcuts.MainListViewColorChoose);
            textWriter.WriteElementString("MainRemoveFormatting", shortcuts.MainRemoveFormatting);
            textWriter.WriteElementString("MainListViewCopyText", shortcuts.MainListViewCopyText);
            textWriter.WriteElementString("MainListViewCopyPlainText", shortcuts.MainListViewCopyPlainText);
            textWriter.WriteElementString("MainListViewCopyTextFromOriginalToCurrent", shortcuts.MainListViewCopyTextFromOriginalToCurrent);
            textWriter.WriteElementString("MainListViewAutoDuration", shortcuts.MainListViewAutoDuration);
            textWriter.WriteElementString("MainListViewColumnDeleteText", shortcuts.MainListViewColumnDeleteText);
            textWriter.WriteElementString("MainListViewColumnDeleteTextAndShiftUp", shortcuts.MainListViewColumnDeleteTextAndShiftUp);
            textWriter.WriteElementString("MainListViewColumnInsertText", shortcuts.MainListViewColumnInsertText);
            textWriter.WriteElementString("MainListViewColumnPaste", shortcuts.MainListViewColumnPaste);
            textWriter.WriteElementString("MainListViewColumnTextUp", shortcuts.MainListViewColumnTextUp);
            textWriter.WriteElementString("MainListViewColumnTextDown", shortcuts.MainListViewColumnTextDown);
            textWriter.WriteElementString("MainListViewGoToNextError", shortcuts.MainListViewGoToNextError);
            textWriter.WriteElementString("MainListViewListErrors", shortcuts.MainListViewListErrors);
            textWriter.WriteElementString("MainListViewSortByNumber", shortcuts.MainListViewSortByNumber);
            textWriter.WriteElementString("MainListViewSortByStartTime", shortcuts.MainListViewSortByStartTime);
            textWriter.WriteElementString("MainListViewSortByEndTime", shortcuts.MainListViewSortByEndTime);
            textWriter.WriteElementString("MainListViewSortByDuration", shortcuts.MainListViewSortByDuration);
            textWriter.WriteElementString("MainListViewSortByGap", shortcuts.MainListViewSortByGap);
            textWriter.WriteElementString("MainListViewSortByText", shortcuts.MainListViewSortByText);
            textWriter.WriteElementString("MainListViewSortBySingleLineMaxLen", shortcuts.MainListViewSortBySingleLineMaxLen);
            textWriter.WriteElementString("MainListViewSortByTextTotalLength", shortcuts.MainListViewSortByTextTotalLength);
            textWriter.WriteElementString("MainListViewSortByCps", shortcuts.MainListViewSortByCps);
            textWriter.WriteElementString("MainListViewSortByWpm", shortcuts.MainListViewSortByWpm);
            textWriter.WriteElementString("MainListViewSortByNumberOfLines", shortcuts.MainListViewSortByNumberOfLines);
            textWriter.WriteElementString("MainListViewSortByActor", shortcuts.MainListViewSortByActor);
            textWriter.WriteElementString("MainListViewSortByStyle", shortcuts.MainListViewSortByStyle);
            textWriter.WriteElementString("GeneralRemoveBlankLines", shortcuts.GeneralRemoveBlankLines);
            textWriter.WriteElementString("GeneralApplyAssaOverrideTags", shortcuts.GeneralApplyAssaOverrideTags);
            textWriter.WriteElementString("GeneralSetAssaPosition", shortcuts.GeneralSetAssaPosition);
            textWriter.WriteElementString("GeneralSetAssaResolution", shortcuts.GeneralSetAssaResolution);
            textWriter.WriteElementString("GeneralSetAssaBgBox", shortcuts.GeneralSetAssaBgBox);
            textWriter.WriteElementString("GeneralColorPicker", shortcuts.GeneralColorPicker);
            textWriter.WriteElementString("GeneralTakeAutoBackup", shortcuts.GeneralTakeAutoBackup);
            textWriter.WriteElementString("MainListViewRemoveTimeCodes", shortcuts.MainListViewRemoveTimeCodes);
            textWriter.WriteElementString("MainEditFixRTLViaUnicodeChars", shortcuts.MainEditFixRTLViaUnicodeChars);
            textWriter.WriteElementString("MainEditRemoveRTLUnicodeChars", shortcuts.MainEditRemoveRTLUnicodeChars);
            textWriter.WriteElementString("MainEditReverseStartAndEndingForRTL", shortcuts.MainEditReverseStartAndEndingForRTL);
            textWriter.WriteElementString("MainVideoToggleControls", shortcuts.MainVideoToggleControls);
            textWriter.WriteElementString("MainTextBoxSplitAtCursor", shortcuts.MainTextBoxSplitAtCursor);
            textWriter.WriteElementString("MainTextBoxSplitAtCursorAndAutoBr", shortcuts.MainTextBoxSplitAtCursorAndAutoBr);
            textWriter.WriteElementString("MainTextBoxSplitAtCursorAndVideoPos", shortcuts.MainTextBoxSplitAtCursorAndVideoPos);
            textWriter.WriteElementString("MainTextBoxSplitSelectedLineBilingual", shortcuts.MainTextBoxSplitSelectedLineBilingual);
            textWriter.WriteElementString("MainTextBoxMoveLastWordDown", shortcuts.MainTextBoxMoveLastWordDown);
            textWriter.WriteElementString("MainTextBoxMoveFirstWordFromNextUp", shortcuts.MainTextBoxMoveFirstWordFromNextUp);
            textWriter.WriteElementString("MainTextBoxMoveLastWordDownCurrent", shortcuts.MainTextBoxMoveLastWordDownCurrent);
            textWriter.WriteElementString("MainTextBoxMoveFirstWordUpCurrent", shortcuts.MainTextBoxMoveFirstWordUpCurrent);
            textWriter.WriteElementString("MainTextBoxMoveFromCursorToNext", shortcuts.MainTextBoxMoveFromCursorToNextAndGoToNext);
            textWriter.WriteElementString("MainTextBoxSelectionToLower", shortcuts.MainTextBoxSelectionToLower);
            textWriter.WriteElementString("MainTextBoxSelectionToUpper", shortcuts.MainTextBoxSelectionToUpper);
            textWriter.WriteElementString("MainTextBoxSelectionToggleCasing", shortcuts.MainTextBoxSelectionToggleCasing);
            textWriter.WriteElementString("MainTextBoxSelectionToRuby", shortcuts.MainTextBoxSelectionToRuby);
            textWriter.WriteElementString("MainTextBoxToggleAutoDuration", shortcuts.MainTextBoxToggleAutoDuration);
            textWriter.WriteElementString("MainCreateInsertSubAtVideoPos", shortcuts.MainCreateInsertSubAtVideoPos);
            textWriter.WriteElementString("MainCreateInsertSubAtVideoPosMax", shortcuts.MainCreateInsertSubAtVideoPosMax);
            textWriter.WriteElementString("MainCreateInsertSubAtVideoPosNoTextBoxFocus", shortcuts.MainCreateInsertSubAtVideoPosNoTextBoxFocus);
            textWriter.WriteElementString("MainCreateSetStart", shortcuts.MainCreateSetStart);
            textWriter.WriteElementString("MainCreateSetEnd", shortcuts.MainCreateSetEnd);
            textWriter.WriteElementString("MainAdjustVideoSetStartForAppropriateLine", shortcuts.MainAdjustVideoSetStartForAppropriateLine);
            textWriter.WriteElementString("MainAdjustVideoSetEndForAppropriateLine", shortcuts.MainAdjustVideoSetEndForAppropriateLine);
            textWriter.WriteElementString("MainAdjustSetEndAndPause", shortcuts.MainAdjustSetEndAndPause);
            textWriter.WriteElementString("MainCreateSetEndAddNewAndGoToNew", shortcuts.MainCreateSetEndAddNewAndGoToNew);
            textWriter.WriteElementString("MainCreateStartDownEndUp", shortcuts.MainCreateStartDownEndUp);
            textWriter.WriteElementString("MainAdjustSetStartAndOffsetTheRest", shortcuts.MainAdjustSetStartAndOffsetTheRest);
            textWriter.WriteElementString("MainAdjustSetStartAndOffsetTheRest2", shortcuts.MainAdjustSetStartAndOffsetTheRest2);
            textWriter.WriteElementString("MainAdjustSetStartAndOffsetTheWholeSubtitle", shortcuts.MainAdjustSetStartAndOffsetTheWholeSubtitle);
            textWriter.WriteElementString("MainAdjustSetEndAndOffsetTheRest", shortcuts.MainAdjustSetEndAndOffsetTheRest);
            textWriter.WriteElementString("MainAdjustSetEndAndOffsetTheRestAndGoToNext", shortcuts.MainAdjustSetEndAndOffsetTheRestAndGoToNext);
            textWriter.WriteElementString("MainAdjustSetStartAndGotoNext", shortcuts.MainAdjustSetStartAndGotoNext);
            textWriter.WriteElementString("MainAdjustSetEndAndGotoNext", shortcuts.MainAdjustSetEndAndGotoNext);
            textWriter.WriteElementString("MainAdjustViaEndAutoStart", shortcuts.MainAdjustViaEndAutoStart);
            textWriter.WriteElementString("MainAdjustViaEndAutoStartAndGoToNext", shortcuts.MainAdjustViaEndAutoStartAndGoToNext);
            textWriter.WriteElementString("MainAdjustSetEndMinusGapAndStartNextHere", shortcuts.MainAdjustSetEndMinusGapAndStartNextHere);
            textWriter.WriteElementString("MainSetEndAndStartNextAfterGap", shortcuts.MainSetEndAndStartNextAfterGap);
            textWriter.WriteElementString("MainAdjustSetStartAutoDurationAndGoToNext", shortcuts.MainAdjustSetStartAutoDurationAndGoToNext);
            textWriter.WriteElementString("MainAdjustSetEndNextStartAndGoToNext", shortcuts.MainAdjustSetEndNextStartAndGoToNext);
            textWriter.WriteElementString("MainAdjustStartDownEndUpAndGoToNext", shortcuts.MainAdjustStartDownEndUpAndGoToNext);
            textWriter.WriteElementString("MainAdjustSetStartAndEndOfPrevious", shortcuts.MainAdjustSetStartAndEndOfPrevious);
            textWriter.WriteElementString("MainAdjustSetStartAndEndOfPreviousAndGoToNext", shortcuts.MainAdjustSetStartAndEndOfPreviousAndGoToNext);
            textWriter.WriteElementString("MainAdjustSetStartKeepDuration", shortcuts.MainAdjustSetStartKeepDuration);
            textWriter.WriteElementString("MainAdjustSelected100MsForward", shortcuts.MainAdjustSelected100MsForward);
            textWriter.WriteElementString("MainAdjustSelected100MsBack", shortcuts.MainAdjustSelected100MsBack);
            textWriter.WriteElementString("MainAdjustStartXMsBack", shortcuts.MainAdjustStartXMsBack);
            textWriter.WriteElementString("MainAdjustStartXMsForward", shortcuts.MainAdjustStartXMsForward);
            textWriter.WriteElementString("MainAdjustEndXMsBack", shortcuts.MainAdjustEndXMsBack);
            textWriter.WriteElementString("MainAdjustEndXMsForward", shortcuts.MainAdjustEndXMsForward);
            textWriter.WriteElementString("MoveStartOneFrameBack", shortcuts.MoveStartOneFrameBack);
            textWriter.WriteElementString("MoveStartOneFrameForward", shortcuts.MoveStartOneFrameForward);
            textWriter.WriteElementString("MoveEndOneFrameBack", shortcuts.MoveEndOneFrameBack);
            textWriter.WriteElementString("MoveEndOneFrameForward", shortcuts.MoveEndOneFrameForward);
            textWriter.WriteElementString("MoveStartOneFrameBackKeepGapPrev", shortcuts.MoveStartOneFrameBackKeepGapPrev);
            textWriter.WriteElementString("MoveStartOneFrameForwardKeepGapPrev", shortcuts.MoveStartOneFrameForwardKeepGapPrev);
            textWriter.WriteElementString("MoveEndOneFrameBackKeepGapNext", shortcuts.MoveEndOneFrameBackKeepGapNext);
            textWriter.WriteElementString("MoveEndOneFrameForwardKeepGapNext", shortcuts.MoveEndOneFrameForwardKeepGapNext);
            textWriter.WriteElementString("MainAdjustSnapStartToNextShotChange", shortcuts.MainAdjustSnapStartToNextShotChange);
            textWriter.WriteElementString("MainAdjustSnapEndToPreviousShotChange", shortcuts.MainAdjustSnapEndToPreviousShotChange);
            textWriter.WriteElementString("MainAdjustExtendToNextShotChange", shortcuts.MainAdjustExtendToNextShotChange);
            textWriter.WriteElementString("MainAdjustExtendToPreviousShotChange", shortcuts.MainAdjustExtendToPreviousShotChange);
            textWriter.WriteElementString("MainAdjustExtendToNextSubtitle", shortcuts.MainAdjustExtendToNextSubtitle);
            textWriter.WriteElementString("MainAdjustExtendToPreviousSubtitle", shortcuts.MainAdjustExtendToPreviousSubtitle);
            textWriter.WriteElementString("MainAdjustExtendToNextSubtitleMinusChainingGap", shortcuts.MainAdjustExtendToNextSubtitleMinusChainingGap);
            textWriter.WriteElementString("MainAdjustExtendToPreviousSubtitleMinusChainingGap", shortcuts.MainAdjustExtendToPreviousSubtitleMinusChainingGap);
            textWriter.WriteElementString("MainAdjustExtendCurrentSubtitle", shortcuts.MainAdjustExtendCurrentSubtitle);
            textWriter.WriteElementString("MainAdjustExtendPreviousLineEndToCurrentStart", shortcuts.MainAdjustExtendPreviousLineEndToCurrentStart);
            textWriter.WriteElementString("MainAdjustExtendNextLineStartToCurrentEnd", shortcuts.MainAdjustExtendNextLineStartToCurrentEnd);
            textWriter.WriteElementString("MainSetInCueToClosestShotChangeLeftGreenZone", shortcuts.MainSetInCueToClosestShotChangeLeftGreenZone);
            textWriter.WriteElementString("MainSetInCueToClosestShotChangeRightGreenZone", shortcuts.MainSetInCueToClosestShotChangeRightGreenZone);
            textWriter.WriteElementString("MainSetOutCueToClosestShotChangeLeftGreenZone", shortcuts.MainSetOutCueToClosestShotChangeLeftGreenZone);
            textWriter.WriteElementString("MainSetOutCueToClosestShotChangeRightGreenZone", shortcuts.MainSetOutCueToClosestShotChangeRightGreenZone);
            textWriter.WriteElementString("MainInsertAfter", shortcuts.MainInsertAfter);
            textWriter.WriteElementString("MainTextBoxAutoBreak", shortcuts.MainTextBoxAutoBreak);
            textWriter.WriteElementString("MainTextBoxBreakAtPosition", shortcuts.MainTextBoxBreakAtPosition);
            textWriter.WriteElementString("MainTextBoxBreakAtPositionAndGoToNext", shortcuts.MainTextBoxBreakAtPositionAndGoToNext);
            textWriter.WriteElementString("MainTextBoxUnbreak", shortcuts.MainTextBoxUnbreak);
            textWriter.WriteElementString("MainTextBoxRecord", shortcuts.MainTextBoxRecord);
            textWriter.WriteElementString("MainTextBoxUnbrekNoSpace", shortcuts.MainTextBoxUnbreakNoSpace);
            textWriter.WriteElementString("MainTextBoxAssaIntellisense", shortcuts.MainTextBoxAssaIntellisense);
            textWriter.WriteElementString("MainTextBoxAssaRemoveTag", shortcuts.MainTextBoxAssaRemoveTag);
            textWriter.WriteElementString("MainWaveformInsertAtCurrentPosition", shortcuts.MainWaveformInsertAtCurrentPosition);
            textWriter.WriteElementString("MainInsertBefore", shortcuts.MainInsertBefore);
            textWriter.WriteElementString("MainMergeDialog", shortcuts.MainMergeDialog);
            textWriter.WriteElementString("MainMergeDialogWithNext", shortcuts.MainMergeDialogWithNext);
            textWriter.WriteElementString("MainMergeDialogWithPrevious", shortcuts.MainMergeDialogWithPrevious);
            textWriter.WriteElementString("MainAutoBalanceSelectedLines", shortcuts.MainAutoBalanceSelectedLines);
            textWriter.WriteElementString("MainEvenlyDistributeSelectedLines", shortcuts.MainEvenlyDistributeSelectedLines);
            textWriter.WriteElementString("MainToggleFocus", shortcuts.MainToggleFocus);
            textWriter.WriteElementString("MainToggleFocusWaveform", shortcuts.MainToggleFocusWaveform);
            textWriter.WriteElementString("MainToggleFocusWaveformTextBox", shortcuts.MainToggleFocusWaveformTextBox);
            textWriter.WriteElementString("WaveformAdd", shortcuts.WaveformAdd);
            textWriter.WriteElementString("WaveformVerticalZoom", shortcuts.WaveformVerticalZoom);
            textWriter.WriteElementString("WaveformVerticalZoomOut", shortcuts.WaveformVerticalZoomOut);
            textWriter.WriteElementString("WaveformZoomIn", shortcuts.WaveformZoomIn);
            textWriter.WriteElementString("WaveformZoomOut", shortcuts.WaveformZoomOut);
            textWriter.WriteElementString("WaveformSplit", shortcuts.WaveformSplit);
            textWriter.WriteElementString("WaveformPlaySelection", shortcuts.WaveformPlaySelection);
            textWriter.WriteElementString("WaveformPlaySelectionEnd", shortcuts.WaveformPlaySelectionEnd);
            textWriter.WriteElementString("WaveformSearchSilenceForward", shortcuts.WaveformSearchSilenceForward);
            textWriter.WriteElementString("WaveformSearchSilenceBack", shortcuts.WaveformSearchSilenceBack);
            textWriter.WriteElementString("WaveformAddTextHere", shortcuts.WaveformAddTextHere);
            textWriter.WriteElementString("WaveformAddTextHereFromClipboard", shortcuts.WaveformAddTextHereFromClipboard);
            textWriter.WriteElementString("WaveformSetParagraphAsSelection", shortcuts.WaveformSetParagraphAsSelection);
            textWriter.WriteElementString("WaveformGoToPreviousShotChange", shortcuts.WaveformGoToPreviousShotChange);
            textWriter.WriteElementString("WaveformGoToNextShotChange", shortcuts.WaveformGoToNextShotChange);
            textWriter.WriteElementString("WaveformToggleShotChange", shortcuts.WaveformToggleShotChange);
            textWriter.WriteElementString("WaveformListShotChanges", shortcuts.WaveformListShotChanges);
            textWriter.WriteElementString("WaveformGuessStart", shortcuts.WaveformGuessStart);
            textWriter.WriteElementString("Waveform100MsLeft", shortcuts.Waveform100MsLeft);
            textWriter.WriteElementString("Waveform100MsRight", shortcuts.Waveform100MsRight);
            textWriter.WriteElementString("Waveform1000MsLeft", shortcuts.Waveform1000MsLeft);
            textWriter.WriteElementString("Waveform1000MsRight", shortcuts.Waveform1000MsRight);
            textWriter.WriteElementString("WaveformAudioToTextVosk", shortcuts.WaveformAudioToTextVosk);
            textWriter.WriteElementString("WaveformAudioToTextWhisper", shortcuts.WaveformAudioToTextWhisper);
            textWriter.WriteElementString("MainCheckFixTimingViaShotChanges", shortcuts.MainCheckFixTimingViaShotChanges);
            textWriter.WriteElementString("MainTranslateGoogleIt", shortcuts.MainTranslateGoogleIt);
            textWriter.WriteElementString("MainTranslateGoogleTranslateIt", shortcuts.MainTranslateGoogleTranslateIt);
            textWriter.WriteElementString("MainTranslateAuto", shortcuts.MainTranslateAuto);
            textWriter.WriteElementString("MainTranslateAutoSelectedLines", shortcuts.MainTranslateAutoSelectedLines);
            textWriter.WriteElementString("MainTranslateCustomSearch1", shortcuts.MainTranslateCustomSearch1);
            textWriter.WriteElementString("MainTranslateCustomSearch2", shortcuts.MainTranslateCustomSearch2);
            textWriter.WriteElementString("MainTranslateCustomSearch3", shortcuts.MainTranslateCustomSearch3);
            textWriter.WriteElementString("MainTranslateCustomSearch4", shortcuts.MainTranslateCustomSearch4);
            textWriter.WriteElementString("MainTranslateCustomSearch5", shortcuts.MainTranslateCustomSearch5);
            textWriter.WriteEndElement();
        }
    }
}
