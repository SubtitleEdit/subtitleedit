using System;
using System.Collections.Generic;
using System.Drawing;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Settings
{
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
        public string GroqUrl { get; set; }
        public string GroqPrompt { get; set; }
        public string GroqApiKey { get; set; }
        public string GroqModel { get; set; }
        public string OpenRouterUrl { get; set; }
        public string OpenRouterPrompt { get; set; }
        public string OpenRouterApiKey { get; set; }
        public string OpenRouterModel { get; set; }
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
        public int AutoTranslateMaxMerges { get; set; }
        public string AutoTranslateStrategy { get; set; }
        public string GeminiProApiKey { get; set; }
        public string TextToSpeechEngine { get; set; }
        public string TextToSpeechLastVoice { get; set; }
        public string TextToSpeechElevenLabsApiKey { get; set; }
        public string TextToSpeechAzureApiKey { get; set; }
        public string TextToSpeechAzureRegion { get; set; }
        public string TextToSpeechElevenLabsModel { get; set; }
        public string TextToSpeechElevenLabsLanguage { get; set; }
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
        public string BatchConvertTranslateEngine { get; set; }
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
        public string GenVideoPixelFormat { get; set; }
        public string GenVideoCrf { get; set; }
        public string GenVideoTune { get; set; }
        public string GenVideoAudioEncoding { get; set; }
        public bool GenVideoAudioForceStereo { get; set; }
        public string GenVideoAudioSampleRate { get; set; }
        public bool GenVideoTargetFileSize { get; set; }
        public float GenVideoFontSizePercentOfHeight { get; set; }
        public bool GenVideoNonAssaBox { get; set; }
        public bool GenTransparentVideoNonAssaBox { get; set; }
        public bool GenTransparentVideoNonAssaBoxPerLine { get; set; }
        public string GenTransparentVideoExtension { get; set; }
        public Color GenVideoNonAssaBoxColor { get; set; }
        public Color GenVideoNonAssaTextColor { get; set; }
        public Color GenVideoNonAssaShadowColor { get; set; }
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
            ChatGptModel = ChatGptTranslate.Models[0];
            GroqUrl = "https://api.groq.com/openai/v1/chat/completions";
            GroqPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments:";
            GroqModel = GroqTranslate.Models[0];
            OpenRouterUrl = "https://openrouter.ai/api/v1/chat/completions";
            OpenRouterPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments:";
            OpenRouterModel = OpenRouterTranslate.Models[0];
            LmStudioPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments:";
            OllamaApiUrl = "http://localhost:11434/api/generate";
            OllamaModels = "llama3.1,phi3,gemma2,qwen2,mistral";
            OllamaModel = "llama3.1";
            OllamaPrompt = "Translate from {0} to {1}, keep punctuation as input, do not censor the translation, give only the output without comments or notes:";
            AnthropicApiUrl = "https://api.anthropic.com/v1/messages";
            AnthropicPrompt = "Translate from {0} to {1}, keep sentences in {1} as they are, do not censor the translation, give only the output without comments:";
            AnthropicApiModel = AnthropicTranslate.Models[0];
            TextToSpeechAzureRegion = "westeurope";
            AutoTranslateMaxBytes = 2000;
            AutoTranslateMaxMerges = -1;
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
            BatchConvertTranslateEngine = LibreTranslate.StaticName;
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
            GenVideoNonAssaShadowColor = Color.Black;
            GenVideoEmbedOutputSuffix = "embed";
            GenVideoEmbedOutputReplace = "embed" + Environment.NewLine + "SoftSub" + Environment.NewLine + "SoftSubbed";
            GenVideoOutputFileSuffix = "_new";
            GenTransparentVideoExtension = ".mkv";
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
}