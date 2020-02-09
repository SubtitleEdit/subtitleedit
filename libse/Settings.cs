using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Nikse.SubtitleEdit.Core
{
    // The settings classes are built for easy xml-serialization (makes save/load code simple)
    // ...but the built-in serialization is too slow - so a custom (de-)serialization has been used!

    public class RecentFileEntry
    {
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string VideoFileName { get; set; }
        public int FirstVisibleIndex { get; set; }
        public int FirstSelectedIndex { get; set; }
        public long VideoOffsetInMs { get; set; }
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

        public void Add(string fileName, int firstVisibleIndex, int firstSelectedIndex, string videoFileName, string originalFileName, long videoOffset)
        {
            var newList = new List<RecentFileEntry> { new RecentFileEntry { FileName = fileName, FirstVisibleIndex = firstVisibleIndex, FirstSelectedIndex = firstSelectedIndex, VideoFileName = videoFileName, OriginalFileName = originalFileName, VideoOffsetInMs = videoOffset } };
            int index = 0;
            foreach (var oldRecentFile in Files)
            {
                if (fileName != null && !fileName.Equals(oldRecentFile.FileName, StringComparison.OrdinalIgnoreCase) && index < MaxRecentFiles)
                {
                    newList.Add(new RecentFileEntry { FileName = oldRecentFile.FileName, FirstVisibleIndex = oldRecentFile.FirstVisibleIndex, FirstSelectedIndex = oldRecentFile.FirstSelectedIndex, VideoFileName = oldRecentFile.VideoFileName, OriginalFileName = oldRecentFile.OriginalFileName, VideoOffsetInMs = oldRecentFile.VideoOffsetInMs });
                }

                index++;
            }
            Files = newList;
        }

        public void Add(string fileName, string videoFileName, string originalFileName)
        {
            var newList = new List<RecentFileEntry>();
            foreach (var oldRecentFile in Files)
            {
                if (fileName != null && fileName.Equals(oldRecentFile.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    newList.Add(new RecentFileEntry { FileName = oldRecentFile.FileName, FirstVisibleIndex = oldRecentFile.FirstVisibleIndex, FirstSelectedIndex = oldRecentFile.FirstSelectedIndex, VideoFileName = oldRecentFile.VideoFileName, OriginalFileName = oldRecentFile.OriginalFileName, VideoOffsetInMs = oldRecentFile.VideoOffsetInMs });
                }
            }
            if (newList.Count == 0)
            {
                newList.Add(new RecentFileEntry { FileName = fileName ?? string.Empty, FirstVisibleIndex = -1, FirstSelectedIndex = -1, VideoFileName = videoFileName, OriginalFileName = originalFileName });
            }

            int index = 0;
            foreach (var oldRecentFile in Files)
            {
                if (fileName != null && !fileName.Equals(oldRecentFile.FileName, StringComparison.OrdinalIgnoreCase) && index < MaxRecentFiles)
                {
                    newList.Add(new RecentFileEntry { FileName = oldRecentFile.FileName, FirstVisibleIndex = oldRecentFile.FirstVisibleIndex, FirstSelectedIndex = oldRecentFile.FirstSelectedIndex, VideoFileName = oldRecentFile.VideoFileName, OriginalFileName = oldRecentFile.OriginalFileName, VideoOffsetInMs = oldRecentFile.VideoOffsetInMs });
                }

                index++;
            }
            Files = newList;
        }
    }

    public class ToolsSettings
    {
        public int StartSceneIndex { get; set; }
        public int EndSceneIndex { get; set; }
        public int VerifyPlaySeconds { get; set; }
        public bool FixShortDisplayTimesAllowMoveStartTime { get; set; }
        public bool RemoveEmptyLinesBetweenText { get; set; }
        public string MusicSymbol { get; set; }
        public string MusicSymbolReplace { get; set; }
        public string UnicodeSymbolsToInsert { get; set; }
        public bool SpellCheckAutoChangeNames { get; set; }
        public bool CheckOneLetterWords { get; set; }
        public bool SpellCheckEnglishAllowInQuoteAsIng { get; set; }
        public bool RememberUseAlwaysList { get; set; }
        public bool SpellCheckShowCompletedMessage { get; set; }
        public bool OcrFixUseHardcodedRules { get; set; }
        public int OcrBinaryImageCompareRgbThreshold { get; set; }
        public int OcrTesseract4RgbThreshold { get; set; }
        public string Interjections { get; set; }
        public string MicrosoftBingApiId { get; set; }
        public string MicrosoftTranslatorApiKey { get; set; }
        public string MicrosoftTranslatorTokenEndpoint { get; set; }
        public string GoogleApiV2Key { get; set; }
        public bool GoogleApiV2KeyInfoShow { get; set; }
        public bool GoogleTranslateNoKeyWarningShow { get; set; }
        public bool UseGooleApiPaidService { get; set; }
        public int GoogleApiV1ChunkSize { get; set; }
        public string GoogleTranslateLastTargetLanguage { get; set; }
        public bool TranslateAutoSplit { get; set; }
        public bool ListViewSyntaxColorDurationSmall { get; set; }
        public bool ListViewSyntaxColorDurationBig { get; set; }
        public bool ListViewSyntaxColorOverlap { get; set; }
        public bool ListViewSyntaxColorLongLines { get; set; }
        public bool ListViewSyntaxColorGap { get; set; }
        public bool ListViewSyntaxMoreThanXLines { get; set; }
        public Color ListViewSyntaxErrorColor { get; set; }
        public Color ListViewUnfocusedSelectedColor { get; set; }
        public bool ListViewShowColumnEndTime { get; set; }
        public bool ListViewShowColumnDuration { get; set; }
        public bool ListViewShowColumnCharsPerSec { get; set; }
        public bool ListViewShowColumnWordsPerMin { get; set; }
        public bool ListViewShowColumnGap { get; set; }
        public bool ListViewShowColumnActor { get; set; }
        public bool ListViewShowColumnRegion { get; set; }
        public bool SplitAdvanced { get; set; }
        public string SplitOutputFolder { get; set; }
        public int SplitNumberOfParts { get; set; }
        public string SplitVia { get; set; }
        public bool JoinCorrectTimeCodes { get; set; }
        public int JoinAddMs { get; set; }
        public string LastShowEarlierOrLaterSelection { get; set; }
        public string NewEmptyTranslationText { get; set; }
        public string BatchConvertOutputFolder { get; set; }
        public bool BatchConvertOverwriteExisting { get; set; }
        public bool BatchConvertSaveInSourceFolder { get; set; }
        public bool BatchConvertRemoveFormatting { get; set; }
        public bool BatchConvertBridgeGaps { get; set; }
        public bool BatchConvertFixCasing { get; set; }
        public bool BatchConvertRemoveTextForHI { get; set; }
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
        public bool BatchConvertApplyDurationLimits { get; set; }
        public bool BatchConvertOffsetTimeCodes { get; set; }
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
        public int Export3DType { get; set; }
        public int Export3DDepth { get; set; }
        public int ExportLastShadowTransparency { get; set; }
        public double ExportLastFrameRate { get; set; }
        public bool ExportFullFrame { get; set; }
        public bool ExportFcpFullPathUrl { get; set; }
        public string ExportPenLineJoin { get; set; }
        public bool FixCommonErrorsFixOverlapAllowEqualEndStart { get; set; }
        public bool FixCommonErrorsSkipStepOne { get; set; }
        public string ImportTextSplitting { get; set; }
        public string ImportTextLineBreak { get; set; }
        public bool ImportTextMergeShortLines { get; set; }
        public bool ImportTextRemoveEmptyLines { get; set; }
        public bool ImportTextAutoSplitAtBlank { get; set; }
        public bool ImportTextRemoveLinesNoLetters { get; set; }
        public bool ImportTextGenerateTimeCodes { get; set; }
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
        public string ExportCustomTemplates { get; set; }
        public string ChangeCasingChoice { get; set; }
        public bool UseNoLineBreakAfter { get; set; }
        public string NoLineBreakAfterEnglish { get; set; }
        public List<string> FindHistory { get; set; }
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
        public bool AutoBreakCommaBreakEarly { get; set; }
        public bool AutoBreakDashEarly { get; set; }
        public bool AutoBreakLineEndingEarly { get; set; }
        public bool AutoBreakUsePixelWidth { get; set; }
        public bool AutoBreakPreferBottomHeavy { get; set; }
        public double AutoBreakPreferBottomPercent { get; set; }
        public bool ApplyMinimumDurationLimit { get; set; }
        public bool ApplyMaximumDurationLimit { get; set; }
        public int MergeShortLinesMaxGap { get; set; }
        public int MergeShortLinesMaxChars { get; set; }
        public bool MergeShortLinesOnlyContinuous { get; set; }

        public ToolsSettings()
        {
            StartSceneIndex = 1;
            EndSceneIndex = 1;
            VerifyPlaySeconds = 2;
            FixShortDisplayTimesAllowMoveStartTime = false;
            RemoveEmptyLinesBetweenText = true;
            MusicSymbol = "♪";
            MusicSymbolReplace = "â™ª,â™," + // ♪ + ♫ in UTF-8 opened as ANSI
                                 "<s M/>,<s m/>," + // music symbols by subtitle creator
                                 "#,*,¶"; // common music symbols
            UnicodeSymbolsToInsert = "♪;♫;☺;☹;♥;©;☮;☯;Σ;∞;≡;⇒;π";
            SpellCheckAutoChangeNames = true;
            OcrFixUseHardcodedRules = true;
            OcrBinaryImageCompareRgbThreshold = 270;
            OcrTesseract4RgbThreshold = 200;
            Interjections = "Ah;Ahem;Ahh;Ahhh;Ahhhh;Eh;Ehh;Ehhh;Hm;Hmm;Hmmm;Huh;Mm;Mmm;Mmmm;Phew;Gah;Oh;Ohh;Ohhh;Ow;Oww;Owww;Ugh;Ughh;Uh;Uhh;Uhhh;Whew";
            GoogleApiV2KeyInfoShow = true;
            GoogleTranslateNoKeyWarningShow = true;
            UseGooleApiPaidService = false;
            GoogleApiV1ChunkSize = 1500;
            GoogleTranslateLastTargetLanguage = "en";
            TranslateAutoSplit = true;
            CheckOneLetterWords = true;
            SpellCheckEnglishAllowInQuoteAsIng = false;
            SpellCheckShowCompletedMessage = true;
            ListViewSyntaxColorDurationSmall = true;
            ListViewSyntaxColorDurationBig = true;
            ListViewSyntaxColorOverlap = true;
            ListViewSyntaxColorLongLines = true;
            ListViewSyntaxMoreThanXLines = true;
            ListViewSyntaxColorGap = true;
            ListViewSyntaxErrorColor = Color.FromArgb(255, 180, 150);
            ListViewUnfocusedSelectedColor = Color.LightBlue;
            ListViewShowColumnEndTime = true;
            ListViewShowColumnDuration = true;
            SplitAdvanced = false;
            SplitNumberOfParts = 3;
            SplitVia = "Lines";
            JoinCorrectTimeCodes = true;
            NewEmptyTranslationText = string.Empty;
            BatchConvertLanguage = string.Empty;
            BatchConvertTsOverrideBottomMargin = 5; // pct
            BatchConvertTsScreenWidth = 1920;
            BatchConvertTsScreenHeight = 1080;
            BatchConvertTsOverrideHAlign = "center"; // left center right
            BatchConvertTsOverrideHMargin = 5; // pct
            BatchConvertTsFileNameAppend = ".{two-letter-country-code}";
            ModifySelectionRule = "Contains";
            ModifySelectionText = string.Empty;
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
            ExportLastShadowTransparency = 200;
            ExportLastFrameRate = 24.0d;
            ExportFullFrame = false;
            ExportPenLineJoin = "Round";
            ExportFcpImageType = "Bmp";
            ExportFcpPalNtsc = "PAL";
            ExportLastBorderWidth = 4;
            BridgeGapMilliseconds = 100;
            ExportCustomTemplates = "SubRipÆÆ{number}\r\n{start} --> {end}\r\n{text}\r\n\r\nÆhh:mm:ss,zzzÆ[Do not modify]ÆæMicroDVDÆÆ{{start}}{{end}}{text}\r\nÆffÆ||Æ";
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
            AutoBreakCommaBreakEarly = false;
            AutoBreakDashEarly = true;
            AutoBreakLineEndingEarly = false;
            AutoBreakUsePixelWidth = true;
            AutoBreakPreferBottomHeavy = true;
            AutoBreakPreferBottomPercent = 5;
            ApplyMinimumDurationLimit = true;
            ApplyMaximumDurationLimit = true;
            MergeShortLinesMaxGap = 250;
            MergeShortLinesMaxChars = 50;
            MergeShortLinesOnlyContinuous = true;
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
            NamesUrl = "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/master/Dictionaries/names.xml";
        }
    }

    public class SubtitleSettings
    {
        public string SsaFontName { get; set; }
        public double SsaFontSize { get; set; }
        public int SsaFontColorArgb { get; set; }
        public bool SsaFontBold { get; set; }
        public decimal SsaOutline { get; set; }
        public decimal SsaShadow { get; set; }
        public bool SsaOpaqueBox { get; set; }
        public int SsaMarginLeft { get; set; }
        public int SsaMarginRight { get; set; }
        public int SsaMarginTopBottom { get; set; }

        public string DCinemaFontFile { get; set; }
        public string DCinemaLoadFontResource { get; set; }
        public int DCinemaFontSize { get; set; }
        public int DCinemaBottomMargin { get; set; }
        public double DCinemaZPosition { get; set; }
        public int DCinemaFadeUpTime { get; set; }
        public int DCinemaFadeDownTime { get; set; }

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

        public int FcpFontSize { get; set; }
        public string FcpFontName { get; set; }

        public string NuendoCharacterListFile { get; set; }

        public bool WebVttUseXTimestampMap { get; set; }
        public long WebVttTimescale { get; set; }

        public SubtitleSettings()
        {
            SsaFontName = "Arial";
            if (Configuration.IsRunningOnLinux)
            {
                SsaFontName = Configuration.DefaultLinuxFontName;
            }
            SsaFontSize = 20;
            SsaFontColorArgb = Color.FromArgb(255, 255, 255).ToArgb();
            SsaOutline = 2;
            SsaShadow = 1;
            SsaOpaqueBox = false;
            SsaMarginLeft = 10;
            SsaMarginRight = 10;
            SsaMarginTopBottom = 10;

            DCinemaFontFile = "Arial.ttf";
            DCinemaLoadFontResource = "urn:uuid:3dec6dc0-39d0-498d-97d0-928d2eb78391";
            DCinemaFontSize = 42;
            DCinemaBottomMargin = 8;
            DCinemaZPosition = 0;
            DCinemaFadeUpTime = 0;
            DCinemaFadeDownTime = 0;

            EbuStlTeletextUseBox = true;
            EbuStlTeletextUseDoubleHeight = true;
            EbuStlMarginTop = 0;
            EbuStlMarginBottom = 2;
            EbuStlNewLineRows = 2;

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

            FcpFontSize = 18;
            FcpFontName = "Lucida Grande";

            Cavena890StartOfMessage = "10:00:00:00";

            WebVttTimescale = 90000;
            WebVttUseXTimestampMap = true;
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
            CurrentDCinemaIssueDate = DateTime.Now.ToString("s") + ".000-00:00";
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
        public bool UnneededSpacesTicked { get; set; }
        public bool UnneededPeriodsTicked { get; set; }
        public bool MissingSpacesTicked { get; set; }
        public bool AddMissingQuotesTicked { get; set; }
        public bool Fix3PlusLinesTicked { get; set; }
        public bool FixHyphensTicked { get; set; }
        public bool FixHyphensAddTicked { get; set; }
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
        public bool TurkishAnsiTicked { get; set; }
        public bool DanishLetterITicked { get; set; }
        public bool SpanishInvertedQuestionAndExclamationMarksTicked { get; set; }
        public bool FixDoubleDashTicked { get; set; }
        public bool FixDoubleGreaterThanTicked { get; set; }
        public bool FixEllipsesStartTicked { get; set; }
        public bool FixMissingOpenBracketTicked { get; set; }
        public bool FixMusicNotationTicked { get; set; }

        public FixCommonErrorsSettings()
        {
            SetDefaultFixes();
        }

        public void SetDefaultFixes()
        {
            EmptyLinesTicked = true;
            OverlappingDisplayTimeTicked = true;
            TooShortDisplayTimeTicked = true;
            TooLongDisplayTimeTicked = true;
            TooShortGapTicked = true;
            InvalidItalicTagsTicked = true;
            BreakLongLinesTicked = true;
            MergeShortLinesTicked = true;
            UnneededPeriodsTicked = true;
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
        }

    }

    public class GeneralSettings
    {
        public List<RulesProfile> Profiles { get; set; }
        public string CurrentProfile { get; set; }
        public bool ShowToolbarNew { get; set; }
        public bool ShowToolbarOpen { get; set; }
        public bool ShowToolbarSave { get; set; }
        public bool ShowToolbarSaveAs { get; set; }
        public bool ShowToolbarFind { get; set; }
        public bool ShowToolbarReplace { get; set; }
        public bool ShowToolbarFixCommonErrors { get; set; }
        public bool ShowToolbarRemoveTextForHi { get; set; }
        public bool ShowToolbarVisualSync { get; set; }
        public bool ShowToolbarSpellCheck { get; set; }
        public bool ShowToolbarNetflixGlyphCheck { get; set; }
        public bool ShowToolbarSettings { get; set; }
        public bool ShowToolbarHelp { get; set; }

        public bool ShowVideoPlayer { get; set; }
        public bool ShowAudioVisualizer { get; set; }
        public bool ShowWaveform { get; set; }
        public bool ShowSpectrogram { get; set; }
        public bool ShowFrameRate { get; set; }
        public double DefaultFrameRate { get; set; }
        public double CurrentFrameRate { get; set; }
        public string DefaultSubtitleFormat { get; set; }
        public string DefaultEncoding { get; set; }
        public bool AutoConvertToUtf8 { get; set; }
        public bool AutoGuessAnsiEncoding { get; set; }
        public string SystemSubtitleFontNameOverride { get; set; }
        public int SystemSubtitleFontSizeOverride { get; set; }

        public string SubtitleFontName { get; set; }
        public int SubtitleFontSize { get; set; }
        public int SubtitleListViewFontSize { get; set; }
        public bool SubtitleFontBold { get; set; }
        public bool SubtitleListViewFontBold { get; set; }
        public Color SubtitleFontColor { get; set; }
        public Color SubtitleBackgroundColor { get; set; }
        public bool CenterSubtitleInTextBox { get; set; }
        public bool ShowRecentFiles { get; set; }
        public bool RememberSelectedLine { get; set; }
        public bool StartLoadLastFile { get; set; }
        public bool StartRememberPositionAndSize { get; set; }
        public string StartPosition { get; set; }
        public string StartSize { get; set; }
        public int SplitContainerMainSplitterDistance { get; set; }
        public int SplitContainer1SplitterDistance { get; set; }
        public int SplitContainerListViewAndTextSplitterDistance { get; set; }
        public bool StartInSourceView { get; set; }
        public bool RemoveBlankLinesWhenOpening { get; set; }
        public bool RemoveBadCharsWhenOpening { get; set; }
        public int SubtitleLineMaximumLength { get; set; }
        public int MaxNumberOfLines { get; set; }
        public int MergeLinesShorterThan { get; set; }
        public int SubtitleMinimumDisplayMilliseconds { get; set; }
        public int SubtitleMaximumDisplayMilliseconds { get; set; }
        public int MinimumMillisecondsBetweenLines { get; set; }
        public int SetStartEndHumanDelay { get; set; }
        public bool AutoWrapLineWhileTyping { get; set; }
        public double SubtitleMaximumCharactersPerSeconds { get; set; }
        public double SubtitleOptimalCharactersPerSeconds { get; set; }
        public bool CharactersPerSecondsIgnoreWhiteSpace { get; set; }
        public double SubtitleMaximumWordsPerMinute { get; set; }
        public string SpellCheckLanguage { get; set; }
        public string VideoPlayer { get; set; }
        public int VideoPlayerDefaultVolume { get; set; }
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
        public bool DirectShowDoubleLoadVideo { get; set; }
        public string VlcWaveTranscodeSettings { get; set; }
        public string VlcLocation { get; set; }
        public string VlcLocationRelative { get; set; }
        public string MpvVideoOutput { get; set; }
        public string MpvVideoOutputLinux { get; set; }
        public bool MpvHandlesPreviewText { get; set; }
        public string MpcHcLocation { get; set; }
        public bool UseFFmpegForWaveExtraction { get; set; }
        public string FFmpegLocation { get; set; }
        public string FFmpegSceneThreshold { get; set; }
        public bool UseTimeFormatHHMMSSFF { get; set; }
        public int ClearStatusBarAfterSeconds { get; set; }
        public string Company { get; set; }
        public bool MoveVideo100Or500MsPlaySmallSample { get; set; }
        public bool DisableVideoAutoLoading { get; set; }
        public bool AllowVolumeBoost { get; set; }
        public int NewEmptyDefaultMs { get; set; }
        public bool RightToLeftMode { get; set; }
        public string LastSaveAsFormat { get; set; }
        public bool CheckForUpdates { get; set; }
        public DateTime LastCheckForUpdates { get; set; }
        public bool AutoSave { get; set; }
        public string PreviewAssaText { get; set; }
        public bool ShowProgress { get; set; }
        public bool ShowNegativeDurationInfoOnSave { get; set; }
        public long CurrentVideoOffsetInMs { get; set; }
        public bool UseDarkTheme { get; set; }
        public bool ShowBetaStuff { get; set; }

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
            ShowToolbarSettings = false;
            ShowToolbarHelp = true;

            ShowVideoPlayer = true;
            ShowAudioVisualizer = true;
            ShowWaveform = true;
            ShowSpectrogram = true;
            ShowFrameRate = false;
            DefaultFrameRate = 23.976;
            CurrentFrameRate = DefaultFrameRate;
            SubtitleFontName = "Tahoma";
            if (Configuration.IsRunningOnLinux)
            {
                SubtitleFontName = Configuration.DefaultLinuxFontName;
            }
            else if (Environment.OSVersion.Version.Major < 6) // 6 == Vista/Win2008Server/Win7
            {
                SubtitleFontName = "Times New Roman";
            }

            SubtitleFontSize = 10;
            SubtitleListViewFontSize = 10;
            SubtitleFontBold = false;
            SubtitleFontColor = Color.Black;
            SubtitleBackgroundColor = Color.White;
            CenterSubtitleInTextBox = false;
            DefaultSubtitleFormat = "SubRip";
            DefaultEncoding = TextEncoding.Utf8WithBom;
            AutoConvertToUtf8 = false;
            AutoGuessAnsiEncoding = true;
            ShowRecentFiles = true;
            RememberSelectedLine = true;
            StartLoadLastFile = true;
            StartRememberPositionAndSize = true;
            SubtitleLineMaximumLength = 43;
            MaxNumberOfLines = 2;
            MergeLinesShorterThan = 33;
            SubtitleMinimumDisplayMilliseconds = 1000;
            SubtitleMaximumDisplayMilliseconds = 8 * 1000;
            RemoveBadCharsWhenOpening = true;
            MinimumMillisecondsBetweenLines = 24;
            SetStartEndHumanDelay = 100;
            AutoWrapLineWhileTyping = false;
            SubtitleMaximumCharactersPerSeconds = 25.0;
            SubtitleOptimalCharactersPerSeconds = 15.0;
            SubtitleMaximumWordsPerMinute = 300;
            SpellCheckLanguage = null;
            VideoPlayer = string.Empty;
            VideoPlayerDefaultVolume = 75;
            VideoPlayerPreviewFontSize = 12;
            VideoPlayerPreviewFontBold = true;
            VideoPlayerShowStopButton = true;
            VideoPlayerShowMuteButton = true;
            VideoPlayerShowFullscreenButton = true;
            ListViewLineSeparatorString = "<br />";
            ListViewDoubleClickAction = 1;
            SaveAsUseFileNameFrom = "video";
            UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWZYXÆØÃÅÄÖÉÈÁÂÀÇÊÍÓÔÕÚŁАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯĞİŞÜÙÁÌÑÎ";
            DefaultAdjustMilliseconds = 1000;
            AutoRepeatOn = true;
            AutoRepeatCount = 2;
            AutoContinueOn = false;
            AutoContinueDelay = 2;
            SyncListViewWithVideoWhilePlaying = false;
            AutoBackupSeconds = 60 * 15;
            AutoBackupDeleteAfterMonths = 6;
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
            DirectShowDoubleLoadVideo = false;
            VlcWaveTranscodeSettings = "acodec=s16l"; // "acodec=s16l,channels=1,ab=64,samplerate=8000";
            MpvVideoOutput = "direct3d";
            MpvVideoOutputLinux = "x11";
            MpvHandlesPreviewText = true;
            FFmpegSceneThreshold = "0.4"; // threshold for generating scene changes - 0.2 is sensitive (more scene change), 0.6 is less sensitive (fewer scene changes)
            UseTimeFormatHHMMSSFF = false;
            ClearStatusBarAfterSeconds = 10;
            MoveVideo100Or500MsPlaySmallSample = false;
            DisableVideoAutoLoading = false;
            RightToLeftMode = false;
            LastSaveAsFormat = string.Empty;
            SystemSubtitleFontNameOverride = string.Empty;
            CheckForUpdates = true;
            LastCheckForUpdates = DateTime.Now;
            ShowProgress = false;
            ShowNegativeDurationInfoOnSave = true;
            UseDarkTheme = false;
            PreviewAssaText = "ABCDEFGHIJKL abcdefghijkl 123";
            ShowBetaStuff = false;
            NewEmptyDefaultMs = 2000;

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
                CpsIncludesSpace = !CharactersPerSecondsIgnoreWhiteSpace,
                MinimumMillisecondsBetweenLines = MinimumMillisecondsBetweenLines,
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
                MergeLinesShorterThan = 42,
                SubtitleMaximumCharactersPerSeconds = 20,
                SubtitleOptimalCharactersPerSeconds = 15,
                SubtitleMaximumDisplayMilliseconds = 7000,
                SubtitleMinimumDisplayMilliseconds = 833,
                SubtitleMaximumWordsPerMinute = 300,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 84, // 2 frames for 23.976 fps videos
            });
            profiles.Add(new RulesProfile
            {
                Name = "Netflix (Other languages)",
                SubtitleLineMaximumLength = 42,
                MaxNumberOfLines = 2,
                MergeLinesShorterThan = 42,
                SubtitleMaximumCharactersPerSeconds = 17,
                SubtitleOptimalCharactersPerSeconds = 12,
                SubtitleMaximumDisplayMilliseconds = 7000,
                SubtitleMinimumDisplayMilliseconds = 833,
                SubtitleMaximumWordsPerMinute = 200,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 84, // 2 frames for 23.976 fps videos
            });
            profiles.Add(new RulesProfile
            {
                Name = "Arte (German/English)",
                SubtitleLineMaximumLength = 40,
                MaxNumberOfLines = 2,
                MergeLinesShorterThan = 40,
                SubtitleMaximumCharactersPerSeconds = 20,
                SubtitleOptimalCharactersPerSeconds = 12,
                SubtitleMaximumDisplayMilliseconds = 10000,
                SubtitleMinimumDisplayMilliseconds = 1000,
                SubtitleMaximumWordsPerMinute = 300,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 200, // 5 frames for 25 fps videos
            });
            profiles.Add(new RulesProfile
            {
                Name = "Dutch professional subtitles (23.976/24 fps)",
                SubtitleLineMaximumLength = 42,
                MaxNumberOfLines = 2,
                MergeLinesShorterThan = 37,
                SubtitleMaximumCharactersPerSeconds = 15,
                SubtitleOptimalCharactersPerSeconds = 11,
                SubtitleMaximumDisplayMilliseconds = 7007,
                SubtitleMinimumDisplayMilliseconds = 1400,
                SubtitleMaximumWordsPerMinute = 180,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 125,
            });
            profiles.Add(new RulesProfile
            {
                Name = "Dutch professional subtitles (25 fps)",
                SubtitleLineMaximumLength = 42,
                MaxNumberOfLines = 2,
                MergeLinesShorterThan = 37,
                SubtitleMaximumCharactersPerSeconds = 15,
                SubtitleOptimalCharactersPerSeconds = 11,
                SubtitleMaximumDisplayMilliseconds = 7000,
                SubtitleMinimumDisplayMilliseconds = 1400,
                SubtitleMaximumWordsPerMinute = 180,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 120,
            });
            profiles.Add(new RulesProfile
            {
                Name = "Dutch fansubs (23.976/24 fps)",
                SubtitleLineMaximumLength = 45,
                MaxNumberOfLines = 2,
                MergeLinesShorterThan = 40,
                SubtitleMaximumCharactersPerSeconds = 22.5m,
                SubtitleOptimalCharactersPerSeconds = 12,
                SubtitleMaximumDisplayMilliseconds = 7007,
                SubtitleMinimumDisplayMilliseconds = 1200,
                SubtitleMaximumWordsPerMinute = 240,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 125,
            });
            profiles.Add(new RulesProfile
            {
                Name = "Dutch fansubs (25 fps)",
                SubtitleLineMaximumLength = 45,
                MaxNumberOfLines = 2,
                MergeLinesShorterThan = 40,
                SubtitleMaximumCharactersPerSeconds = 22.5m,
                SubtitleOptimalCharactersPerSeconds = 12,
                SubtitleMaximumDisplayMilliseconds = 7000,
                SubtitleMinimumDisplayMilliseconds = 1200,
                SubtitleMaximumWordsPerMinute = 240,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 120,
            });
            profiles.Add(new RulesProfile
            {
                Name = "Danish professional subtitles (23.976/24 fps)",
                SubtitleLineMaximumLength = 40,
                MaxNumberOfLines = 2,
                MergeLinesShorterThan = 40,
                SubtitleMaximumCharactersPerSeconds = 15,
                SubtitleOptimalCharactersPerSeconds = 10,
                SubtitleMaximumDisplayMilliseconds = 8008,
                SubtitleMinimumDisplayMilliseconds = 2002,
                SubtitleMaximumWordsPerMinute = 180,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 125,
            });
            profiles.Add(new RulesProfile
            {
                Name = "Danish professional subtitles (25 fps)",
                SubtitleLineMaximumLength = 40,
                MaxNumberOfLines = 2,
                MergeLinesShorterThan = 40,
                SubtitleMaximumCharactersPerSeconds = 15,
                SubtitleOptimalCharactersPerSeconds = 10,
                SubtitleMaximumDisplayMilliseconds = 8000,
                SubtitleMinimumDisplayMilliseconds = 2000,
                SubtitleMaximumWordsPerMinute = 180,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 120,
            });
            profiles.Add(new RulesProfile
            {
                Name = "SW2 (French) (23.976/24 fps)",
                SubtitleLineMaximumLength = 40,
                MaxNumberOfLines = 2,
                MergeLinesShorterThan = 37,
                SubtitleMaximumCharactersPerSeconds = 25,
                SubtitleOptimalCharactersPerSeconds = 18,
                SubtitleMaximumDisplayMilliseconds = 5005,
                SubtitleMinimumDisplayMilliseconds = 792,
                SubtitleMaximumWordsPerMinute = 300,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 125,
            });
            profiles.Add(new RulesProfile
            {
                Name = "SW2 (French) (25 fps)",
                SubtitleLineMaximumLength = 40,
                MaxNumberOfLines = 2,
                MergeLinesShorterThan = 37,
                SubtitleMaximumCharactersPerSeconds = 25,
                SubtitleOptimalCharactersPerSeconds = 18,
                SubtitleMaximumDisplayMilliseconds = 5000,
                SubtitleMinimumDisplayMilliseconds = 800,
                SubtitleMaximumWordsPerMinute = 300,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 120,
            });
            profiles.Add(new RulesProfile
            {
                Name = "SW3 (French) (23.976/24 fps)",
                SubtitleLineMaximumLength = 40,
                MaxNumberOfLines = 2,
                MergeLinesShorterThan = 37,
                SubtitleMaximumCharactersPerSeconds = 25,
                SubtitleOptimalCharactersPerSeconds = 18,
                SubtitleMaximumDisplayMilliseconds = 5005,
                SubtitleMinimumDisplayMilliseconds = 792,
                SubtitleMaximumWordsPerMinute = 300,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 167,
            });
            profiles.Add(new RulesProfile
            {
                Name = "SW3 (French) (25 fps)",
                SubtitleLineMaximumLength = 40,
                MaxNumberOfLines = 2,
                MergeLinesShorterThan = 37,
                SubtitleMaximumCharactersPerSeconds = 25,
                SubtitleOptimalCharactersPerSeconds = 18,
                SubtitleMaximumDisplayMilliseconds = 5000,
                SubtitleMinimumDisplayMilliseconds = 800,
                SubtitleMaximumWordsPerMinute = 300,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 160,
            });
            profiles.Add(new RulesProfile
            {
                Name = "SW4 (French) (23.976/24 fps)",
                SubtitleLineMaximumLength = 40,
                MaxNumberOfLines = 2,
                MergeLinesShorterThan = 37,
                SubtitleMaximumCharactersPerSeconds = 25,
                SubtitleOptimalCharactersPerSeconds = 18,
                SubtitleMaximumDisplayMilliseconds = 5005,
                SubtitleMinimumDisplayMilliseconds = 792,
                SubtitleMaximumWordsPerMinute = 300,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 250,
            });
            profiles.Add(new RulesProfile
            {
                Name = "SW4 (French) (25 fps)",
                SubtitleLineMaximumLength = 40,
                MaxNumberOfLines = 2,
                MergeLinesShorterThan = 37,
                SubtitleMaximumCharactersPerSeconds = 25,
                SubtitleOptimalCharactersPerSeconds = 18,
                SubtitleMaximumDisplayMilliseconds = 5000,
                SubtitleMinimumDisplayMilliseconds = 800,
                SubtitleMaximumWordsPerMinute = 300,
                CpsIncludesSpace = true,
                MinimumMillisecondsBetweenLines = 240,
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
        public int WaveformBorderHitMs { get; set; }
        public Color WaveformGridColor { get; set; }
        public Color WaveformColor { get; set; }
        public Color WaveformSelectedColor { get; set; }
        public Color WaveformBackgroundColor { get; set; }
        public Color WaveformTextColor { get; set; }
        public int WaveformTextSize { get; set; }
        public bool WaveformTextBold { get; set; }
        public string WaveformDoubleClickOnNonParagraphAction { get; set; }
        public string WaveformRightClickOnNonParagraphAction { get; set; }
        public bool WaveformMouseWheelScrollUpIsForward { get; set; }
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

            LastActiveTab = "Translate";
            WaveformDrawGrid = true;
            WaveformAllowOverlap = false;
            WaveformBorderHitMs = 15;
            WaveformGridColor = Color.FromArgb(255, 20, 20, 18);
            WaveformColor = Color.FromArgb(255, 160, 240, 30);
            WaveformSelectedColor = Color.FromArgb(255, 230, 0, 0);
            WaveformBackgroundColor = Color.Black;
            WaveformTextColor = Color.Gray;
            WaveformTextSize = 9;
            WaveformTextBold = true;
            WaveformDoubleClickOnNonParagraphAction = "PlayPause";
            WaveformDoubleClickOnNonParagraphAction = string.Empty;
            WaveformMouseWheelScrollUpIsForward = true;
            SpectrogramAppearance = "OneColorGradient";
            WaveformMinimumSampleRate = 126;
            WaveformSeeksSilenceDurationSeconds = 0.3;
            WaveformSeeksSilenceMaxVolume = 0.1;
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
        public bool PromptForUnknownWords { get; set; }
        public bool GuessUnknownWords { get; set; }
        public bool AutoBreakSubtitleIfMoreThanTwoLines { get; set; }
        public double ItalicFactor { get; set; }

        public bool LineOcrDraw { get; set; }
        public bool LineOcrAdvancedItalic { get; set; }
        public string LineOcrLastLanguages { get; set; }
        public string LineOcrLastSpellCheck { get; set; }
        public int LineOcrXOrMorePixelsMakesSpace { get; set; }
        public int LineOcrMinLineHeight { get; set; }
        public int LineOcrMaxLineHeight { get; set; }
        public string LastBinaryImageCompareDb { get; set; }
        public string LastBinaryImageSpellCheck { get; set; }
        public bool BinaryAutoDetectBestDb { get; set; }
        public string LastTesseractSpellCheck { get; set; }
        public bool CaptureTopAlign { get; set; }

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
            PromptForUnknownWords = true;
            GuessUnknownWords = true;
            AutoBreakSubtitleIfMoreThanTwoLines = true;
            ItalicFactor = 0.2;
            BinaryAutoDetectBestDb = true;
            CaptureTopAlign = false;
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

    public class NetworkSettings
    {
        public string UserName { get; set; }
        public string WebServiceUrl { get; set; }
        public string SessionKey { get; set; }
        public int PollIntervalSeconds { get; set; }
        public string NewMessageSound { get; set; }

        public NetworkSettings()
        {
            UserName = string.Empty;
            SessionKey = "DemoSession"; // TODO: Leave blank or use guid
            WebServiceUrl = "https://www.nikse.dk/se/SeService.asmx";
            PollIntervalSeconds = 5;
        }
    }

    public class Shortcuts
    {
        public string GeneralGoToFirstSelectedLine { get; set; }
        public string GeneralGoToNextEmptyLine { get; set; }
        public string GeneralMergeSelectedLines { get; set; }
        public string GeneralMergeSelectedLinesAndAutoBreak { get; set; }
        public string GeneralMergeSelectedLinesAndUnbreak { get; set; }
        public string GeneralMergeSelectedLinesAndUnbreakCjk { get; set; }
        public string GeneralMergeSelectedLinesOnlyFirstText { get; set; }
        public string GeneralMergeSelectedLinesBilingual { get; set; }
        public string GeneralMergeWithNext { get; set; }
        public string GeneralMergeWithPrevious { get; set; }
        public string GeneralToggleTranslationMode { get; set; }
        public string GeneralSwitchOriginalAndTranslation { get; set; }
        public string GeneralMergeOriginalAndTranslation { get; set; }
        public string GeneralGoToNextSubtitle { get; set; }
        public string GeneralGoToPrevSubtitle { get; set; }
        public string GeneralGoToStartOfCurrentSubtitle { get; set; }
        public string GeneralGoToEndOfCurrentSubtitle { get; set; }
        public string GeneralGoToPreviousSubtitleAndFocusVideo { get; set; }
        public string GeneralGoToNextSubtitleAndFocusVideo { get; set; }
        public string GeneralExtendCurrentSubtitle { get; set; }
        public string GeneralAutoCalcCurrentDuration { get; set; }
        public string GeneralPlayFirstSelected { get; set; }
        public string GeneralHelp { get; set; }
        public string GeneralUnbrekNoSpace { get; set; }
        public string GeneralToggleBookmarks { get; set; }
        public string GeneralToggleBookmarksWithText { get; set; }
        public string GeneralClearBookmarks { get; set; }
        public string GeneralGoToBookmark { get; set; }
        public string GeneralGoToPreviousBookmark { get; set; }
        public string GeneralGoToNextBookmark { get; set; }
        public string ChooseProfile { get; set; }
        public string DuplicateLine { get; set; }
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
        public string MainFileImportPlainText { get; set; }
        public string MainFileImportTimeCodes { get; set; }
        public string MainFileExportEbu { get; set; }
        public string MainFileExportPlainText { get; set; }
        public string MainEditUndo { get; set; }
        public string MainEditRedo { get; set; }
        public string MainEditFind { get; set; }
        public string MainEditFindNext { get; set; }
        public string MainEditReplace { get; set; }
        public string MainEditMultipleReplace { get; set; }
        public string MainEditGoToLineNumber { get; set; }
        public string MainEditRightToLeft { get; set; }
        public string MainEditReverseStartAndEndingForRTL { get; set; }
        public string MainEditToggleTranslationOriginalInPreviews { get; set; }
        public string MainEditInverseSelection { get; set; }
        public string MainEditModifySelection { get; set; }
        public string MainToolsFixCommonErrors { get; set; }
        public string MainToolsFixCommonErrorsPreview { get; set; }
        public string MainToolsMergeShortLines { get; set; }
        public string MainToolsMakeEmptyFromCurrent { get; set; }
        public string MainToolsSplitLongLines { get; set; }
        public string MainToolsDurationsBridgeGap { get; set; }
        public string MainToolsMinimumDisplayTimeBetweenParagraphs { get; set; }

        public string MainToolsRenumber { get; set; }
        public string MainToolsRemoveTextForHI { get; set; }
        public string MainToolsChangeCasing { get; set; }
        public string MainToolsAutoDuration { get; set; }
        public string MainToolsBatchConvert { get; set; }
        public string MainToolsBeamer { get; set; }
        public string MainVideoOpen { get; set; }
        public string MainVideoClose { get; set; }
        public string MainVideoPause { get; set; }
        public string MainVideoPlayFromJustBefore { get; set; }
        public string MainVideoPlayPauseToggle { get; set; }
        public string MainVideoShowHideVideo { get; set; }
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
        public string MainVideoGoToPrevSubtitle { get; set; }
        public string MainVideoGoToNextSubtitle { get; set; }
        public string MainVideoFullscreen { get; set; }
        public string MainVideoSlower { get; set; }
        public string MainVideoFaster { get; set; }
        public string MainVideoReset { get; set; }
        public string MainSpellCheck { get; set; }
        public string MainSpellCheckFindDoubleWords { get; set; }
        public string MainSpellCheckAddWordToNames { get; set; }
        public string MainSynchronizationAdjustTimes { get; set; }
        public string MainSynchronizationVisualSync { get; set; }
        public string MainSynchronizationPointSync { get; set; }
        public string MainSynchronizationPointSyncViaFile { get; set; }
        public string MainSynchronizationChangeFrameRate { get; set; }
        public string MainListViewItalic { get; set; }
        public string MainListViewBold { get; set; }
        public string MainListViewUnderline { get; set; }
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
        public string MainRemoveFormatting { get; set; }
        public string MainListViewCopyText { get; set; }
        public string MainListViewCopyTextFromOriginalToCurrent { get; set; }
        public string MainListViewAutoDuration { get; set; }
        public string MainListViewColumnDeleteText { get; set; }
        public string MainListViewColumnDeleteTextAndShiftUp { get; set; }
        public string MainListViewColumnInsertText { get; set; }
        public string MainListViewColumnPaste { get; set; }
        public string MainListViewColumnTextUp { get; set; }
        public string MainListViewColumnTextDown { get; set; }
        public string MainListViewFocusWaveform { get; set; }
        public string MainListViewGoToNextError { get; set; }
        public string MainTextBoxItalic { get; set; }
        public string MainTextBoxSplitAtCursor { get; set; }
        public string MainTextBoxSplitAtCursorAndVideoPos { get; set; }
        public string MainTextBoxSplitSelectedLineBilingual { get; set; }
        public string MainTextBoxMoveLastWordDown { get; set; }
        public string MainTextBoxMoveFirstWordFromNextUp { get; set; }
        public string MainTextBoxMoveLastWordDownCurrent { get; set; }
        public string MainTextBoxMoveFirstWordUpCurrent { get; set; }
        public string MainTextBoxSelectionToLower { get; set; }
        public string MainTextBoxSelectionToUpper { get; set; }
        public string MainTextBoxSelectionToRuby { get; set; }
        public string MainTextBoxToggleAutoDuration { get; set; }
        public string MainCreateInsertSubAtVideoPos { get; set; }
        public string MainCreateSetStart { get; set; }
        public string MainCreateSetEnd { get; set; }
        public string MainCreateSetEndAddNewAndGoToNew { get; set; }
        public string MainCreateStartDownEndUp { get; set; }
        public string MainAdjustSetStartAndOffsetTheRest { get; set; }
        public string MainAdjustSetEndAndOffsetTheRest { get; set; }
        public string MainAdjustSetEndAndOffsetTheRestAndGoToNext { get; set; }
        public string MainAdjustSetEndAndGotoNext { get; set; }
        public string MainAdjustViaEndAutoStart { get; set; }
        public string MainAdjustViaEndAutoStartAndGoToNext { get; set; }
        public string MainAdjustSetStartAutoDurationAndGoToNext { get; set; }
        public string MainAdjustSetEndNextStartAndGoToNext { get; set; }
        public string MainAdjustStartDownEndUpAndGoToNext { get; set; }
        public string MainAdjustSetStartKeepDuration { get; set; }
        public string MainAdjustSelected100MsForward { get; set; }
        public string MainAdjustSelected100MsBack { get; set; }
        public string MainAdjustStartXMsBack { get; set; }
        public string MainAdjustStartXMsForward { get; set; }
        public string MainAdjustEndXMsBack { get; set; }
        public string MainAdjustEndXMsForward { get; set; }
        public string MainInsertAfter { get; set; }
        public string MainTextBoxAutoBreak { get; set; }
        public string MainTextBoxBreakAtPosition { get; set; }
        public string MainTextBoxBreakAtPositionAndGoToNext { get; set; }
        public string MainTextBoxUnbreak { get; set; }
        public string MainWaveformInsertAtCurrentPosition { get; set; }
        public string MainInsertBefore { get; set; }
        public string MainMergeDialog { get; set; }
        public string MainToggleFocus { get; set; }
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
        public string WaveformFocusListView { get; set; }
        public string WaveformGoToPreviousSceneChange { get; set; }
        public string WaveformGoToNextSceneChange { get; set; }
        public string WaveformToggleSceneChange { get; set; }
        public string MainTranslateGoogleIt { get; set; }
        public string MainTranslateGoogleTranslate { get; set; }
        public string MainTranslateCustomSearch1 { get; set; }
        public string MainTranslateCustomSearch2 { get; set; }
        public string MainTranslateCustomSearch3 { get; set; }
        public string MainTranslateCustomSearch4 { get; set; }
        public string MainTranslateCustomSearch5 { get; set; }

        public Shortcuts()
        {
            GeneralGoToFirstSelectedLine = "Control+L";
            GeneralMergeSelectedLines = "Control+Shift+M";
            GeneralToggleTranslationMode = "Control+Shift+O";
            GeneralSwitchOriginalAndTranslation = "Control+Alt+O";
            GeneralMergeOriginalAndTranslation = "Control+Alt+Shift+M";
            GeneralGoToNextSubtitle = "Shift+Return";
            GeneralGoToPrevSubtitle = string.Empty;
            GeneralGoToStartOfCurrentSubtitle = string.Empty;
            GeneralGoToEndOfCurrentSubtitle = string.Empty;
            GeneralToggleBookmarksWithText = "Control+Shift+B";
            MainFileNew = "Control+N";
            MainFileOpen = "Control+O";
            MainFileSave = "Control+S";
            MainFileSaveOriginal = string.Empty;
            MainFileSaveOriginalAs = string.Empty;
            MainFileSaveAs = string.Empty;
            MainFileSaveAll = string.Empty;
            MainFileExportEbu = string.Empty;
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
            MainToolsMergeShortLines = string.Empty;
            MainToolsSplitLongLines = string.Empty;
            MainToolsRenumber = "Control+Shift+N";
            MainToolsRemoveTextForHI = "Control+Shift+H";
            MainToolsChangeCasing = "Control+Shift+C";
            MainVideoPlayPauseToggle = "Control+P";
            MainVideoPause = "Control+Alt+P";
            MainVideoShowHideVideo = "Control+Q";
            MainVideo1FrameLeft = string.Empty;
            MainVideo1FrameRight = string.Empty;
            MainVideo100MsLeft = string.Empty;
            MainVideo100MsRight = string.Empty;
            MainVideo500MsLeft = "Alt+Left";
            MainVideo500MsRight = "Alt+Right";
            MainVideo1000MsLeft = string.Empty;
            MainVideo1000MsRight = string.Empty;
            MainVideo5000MsLeft = string.Empty;
            MainVideo5000MsRight = string.Empty;
            MainVideoFullscreen = "Alt+Return";
            MainVideoReset = "Control+D0";
            MainSpellCheck = "Control+Shift+S";
            MainSpellCheckFindDoubleWords = "Control+Shift+D";
            MainSpellCheckAddWordToNames = "Control+Shift+L";
            MainSynchronizationAdjustTimes = "Control+Shift+A";
            MainSynchronizationVisualSync = "Control+Shift+V";
            MainSynchronizationPointSync = "Control+Shift+P";
            MainSynchronizationChangeFrameRate = string.Empty;
            MainListViewItalic = "Control+I";
            MainEditReverseStartAndEndingForRTL = string.Empty;
            MainTextBoxItalic = "Control+I";
            MainTextBoxSplitAtCursor = "Control+Alt+V";
            MainToolsAutoDuration = string.Empty;
            MainTextBoxSelectionToLower = "Control+U";
            MainTextBoxSelectionToUpper = "Control+Shift+U";
            MainTextBoxToggleAutoDuration = string.Empty;
            MainToolsBeamer = "Control+Shift+Alt+B";
            MainCreateInsertSubAtVideoPos = string.Empty;
            MainCreateSetStart = string.Empty;
            MainCreateSetEnd = string.Empty;
            MainCreateSetEndAddNewAndGoToNew = string.Empty;
            MainCreateStartDownEndUp = string.Empty;
            MainAdjustSetStartAndOffsetTheRest = "Control+Space";
            MainAdjustSetEndAndOffsetTheRest = string.Empty;
            MainAdjustSetEndAndOffsetTheRestAndGoToNext = string.Empty;
            MainAdjustSetEndAndGotoNext = string.Empty;
            MainAdjustViaEndAutoStartAndGoToNext = string.Empty;
            MainAdjustSetStartAutoDurationAndGoToNext = string.Empty;
            MainAdjustSetEndNextStartAndGoToNext = string.Empty;
            MainAdjustStartDownEndUpAndGoToNext = string.Empty;
            MainAdjustSetStartKeepDuration = string.Empty;
            MainAdjustSelected100MsForward = string.Empty;
            MainAdjustSelected100MsBack = string.Empty;
            MainInsertAfter = "Alt+Insert";
            MainWaveformInsertAtCurrentPosition = "Insert";
            MainInsertBefore = "Control+Shift+Insert";
            MainTextBoxAutoBreak = "Control+R";
            MainTextBoxUnbreak = string.Empty;
            MainMergeDialog = string.Empty;
            WaveformVerticalZoom = "Shift+Add";
            WaveformVerticalZoomOut = "Shift+Subtract";
            WaveformPlaySelection = string.Empty;
            WaveformPlaySelectionEnd = string.Empty;
            GeneralGoToNextSubtitleAndFocusVideo = string.Empty;
            GeneralGoToPreviousSubtitleAndFocusVideo = string.Empty;
            GeneralExtendCurrentSubtitle = string.Empty;
            GeneralAutoCalcCurrentDuration = string.Empty;
            GeneralPlayFirstSelected = string.Empty;
            GeneralHelp = "F1";
            WaveformSearchSilenceForward = string.Empty;
            WaveformSearchSilenceBack = string.Empty;
            WaveformAddTextHere = "Return";
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
        public bool RemoveTextBetweenOnlySeperateLines { get; set; }
        public bool RemoveTextBeforeColon { get; set; }
        public bool RemoveTextBeforeColonOnlyIfUppercase { get; set; }
        public bool RemoveTextBeforeColonOnlyOnSeparateLine { get; set; }
        public bool RemoveInterjections { get; set; }
        public bool RemoveInterjectionsOnlyOnSeparateLine { get; set; }
        public bool RemoveIfContains { get; set; }
        public bool RemoveIfAllUppercase { get; set; }
        public string RemoveIfContainsText { get; set; }

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

    public class CompareSettings
    {
        public bool ShowOnlyDifferences { get; set; }
        public bool OnlyLookForDifferenceInText { get; set; }
        public bool IgnoreLineBreaks { get; set; }
        public bool IgnoreFormatting { get; set; }

        public CompareSettings()
        {
            OnlyLookForDifferenceInText = true;
        }
    }

    public class Settings
    {
        public CompareSettings Compare { get; set; }
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

        [XmlIgnore]
        public Language Language { get; set; }

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
            Language = new Language();
            Shortcuts = new Shortcuts();
            RemoveTextForHearingImpaired = new RemoveTextForHearingImpairedSettings();
            SubtitleBeaming = new SubtitleBeaming();
            Compare = new CompareSettings();
        }

        private Settings()
        {
            Reset();
        }

        public void Save()
        {
            //this is too slow: Serialize(Configuration.SettingsFileName, this);
            CustomSerialize(Configuration.SettingsFileName, this);
        }

        //private static void Serialize(string fileName, Settings settings)
        //{
        //    var s = new XmlSerializer(typeof(Settings));
        //    TextWriter w = new StreamWriter(fileName);
        //    s.Serialize(w, settings);
        //    w.Close();
        //}

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

                    if (settings.General.AutoConvertToUtf8 && !settings.General.DefaultEncoding.StartsWith("UTF-8", StringComparison.Ordinal))
                    {
                        settings.General.DefaultEncoding = "UTF-8 with BOM";
                    }
                }
                catch (Exception exception)
                {
                    settings = new Settings();
                    SeLogger.Error(exception, "Failed to load " + settingsFileName);
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

                xnode = nodeCompare.SelectSingleNode("IgnoreFormatting");
                if (xnode != null)
                {
                    settings.Compare.IgnoreFormatting = Convert.ToBoolean(xnode.InnerText);
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

                settings.RecentFiles.Files.Add(new RecentFileEntry { FileName = listNode.InnerText, FirstVisibleIndex = int.Parse(firstVisibleIndex, CultureInfo.InvariantCulture), FirstSelectedIndex = int.Parse(firstSelectedIndex, CultureInfo.InvariantCulture), VideoFileName = videoFileName, OriginalFileName = originalFileName, VideoOffsetInMs = videoOffset });
            }

            // General
            node = doc.DocumentElement.SelectSingleNode("General");

            // Profiles
            int profileCount = 0;
            foreach (XmlNode listNode in node.SelectNodes("Profiles/Profile"))
            {
                if (profileCount == 0)
                {
                    settings.General.Profiles.Clear();
                }

                var p = new RulesProfile();
                var subtitleLineMaximumLength = listNode.SelectSingleNode("SubtitleLineMaximumLength")?.InnerText;
                var subtitleMaximumCharactersPerSeconds = listNode.SelectSingleNode("SubtitleMaximumCharactersPerSeconds")?.InnerText;
                var subtitleOptimalCharactersPerSeconds = listNode.SelectSingleNode("SubtitleOptimalCharactersPerSeconds")?.InnerText;
                var subtitleMinimumDisplayMilliseconds = listNode.SelectSingleNode("SubtitleMinimumDisplayMilliseconds")?.InnerText;
                var subtitleMaximumDisplayMilliseconds = listNode.SelectSingleNode("SubtitleMaximumDisplayMilliseconds")?.InnerText;
                var subtitleMaximumWordsPerMinute = listNode.SelectSingleNode("SubtitleMaximumWordsPerMinute")?.InnerText;
                var cpsIncludesSpace = listNode.SelectSingleNode("CpsIncludesSpace")?.InnerText;
                var maxNumberOfLines = listNode.SelectSingleNode("MaxNumberOfLines")?.InnerText;
                var mergeLinesShorterThan = listNode.SelectSingleNode("MergeLinesShorterThan")?.InnerText;
                var minimumMillisecondsBetweenLines = listNode.SelectSingleNode("MinimumMillisecondsBetweenLines")?.InnerText;
                settings.General.Profiles.Add(new RulesProfile
                {
                    Name = listNode.SelectSingleNode("Name")?.InnerText,
                    SubtitleLineMaximumLength = Convert.ToInt32(subtitleLineMaximumLength, CultureInfo.InvariantCulture),
                    SubtitleMaximumCharactersPerSeconds = Convert.ToDecimal(subtitleMaximumCharactersPerSeconds, CultureInfo.InvariantCulture),
                    SubtitleOptimalCharactersPerSeconds = Convert.ToDecimal(subtitleOptimalCharactersPerSeconds, CultureInfo.InvariantCulture),
                    SubtitleMinimumDisplayMilliseconds = Convert.ToInt32(subtitleMinimumDisplayMilliseconds, CultureInfo.InvariantCulture),
                    SubtitleMaximumDisplayMilliseconds = Convert.ToInt32(subtitleMaximumDisplayMilliseconds, CultureInfo.InvariantCulture),
                    SubtitleMaximumWordsPerMinute = Convert.ToDecimal(subtitleMaximumWordsPerMinute, CultureInfo.InvariantCulture),
                    CpsIncludesSpace = Convert.ToBoolean(cpsIncludesSpace, CultureInfo.InvariantCulture),
                    MaxNumberOfLines = Convert.ToInt32(maxNumberOfLines, CultureInfo.InvariantCulture),
                    MergeLinesShorterThan = Convert.ToInt32(mergeLinesShorterThan, CultureInfo.InvariantCulture),
                    MinimumMillisecondsBetweenLines = Convert.ToInt32(minimumMillisecondsBetweenLines, CultureInfo.InvariantCulture)
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
                settings.General.ShowToolbarNew = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowToolbarOpen");
            if (subNode != null)
            {
                settings.General.ShowToolbarOpen = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowToolbarSave");
            if (subNode != null)
            {
                settings.General.ShowToolbarSave = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowToolbarSaveAs");
            if (subNode != null)
            {
                settings.General.ShowToolbarSaveAs = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowToolbarFind");
            if (subNode != null)
            {
                settings.General.ShowToolbarFind = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowToolbarReplace");
            if (subNode != null)
            {
                settings.General.ShowToolbarReplace = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowToolbarFixCommonErrors");
            if (subNode != null)
            {
                settings.General.ShowToolbarFixCommonErrors = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowToolbarRemoveTextForHi");
            if (subNode != null)
            {
                settings.General.ShowToolbarRemoveTextForHi = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowToolbarVisualSync");
            if (subNode != null)
            {
                settings.General.ShowToolbarVisualSync = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowToolbarSpellCheck");
            if (subNode != null)
            {
                settings.General.ShowToolbarSpellCheck = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowToolbarNetflixGlyphCheck");
            if (subNode != null)
            {
                settings.General.ShowToolbarNetflixGlyphCheck = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowToolbarSettings");
            if (subNode != null)
            {
                settings.General.ShowToolbarSettings = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowToolbarHelp");
            if (subNode != null)
            {
                settings.General.ShowToolbarHelp = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowFrameRate");
            if (subNode != null)
            {
                settings.General.ShowFrameRate = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowVideoPlayer");
            if (subNode != null)
            {
                settings.General.ShowVideoPlayer = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowAudioVisualizer");
            if (subNode != null)
            {
                settings.General.ShowAudioVisualizer = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowWaveform");
            if (subNode != null)
            {
                settings.General.ShowWaveform = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowSpectrogram");
            if (subNode != null)
            {
                settings.General.ShowSpectrogram = Convert.ToBoolean(subNode.InnerText);
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

            subNode = node.SelectSingleNode("DefaultEncoding");
            if (subNode != null)
            {
                settings.General.DefaultEncoding = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("AutoConvertToUtf8");
            if (subNode != null)
            {
                settings.General.AutoConvertToUtf8 = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AutoGuessAnsiEncoding");
            if (subNode != null)
            {
                settings.General.AutoGuessAnsiEncoding = Convert.ToBoolean(subNode.InnerText);
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

            subNode = node.SelectSingleNode("SubtitleFontSize");
            if (subNode != null)
            {
                settings.General.SubtitleFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleListViewFontSize");
            if (subNode != null)
            {
                settings.General.SubtitleListViewFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SubtitleFontBold");
            if (subNode != null)
            {
                settings.General.SubtitleFontBold = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("SubtitleListViewFontBold");
            if (subNode != null)
            {
                settings.General.SubtitleListViewFontBold = Convert.ToBoolean(subNode.InnerText);
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

            subNode = node.SelectSingleNode("CenterSubtitleInTextBox");
            if (subNode != null)
            {
                settings.General.CenterSubtitleInTextBox = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ShowRecentFiles");
            if (subNode != null)
            {
                settings.General.ShowRecentFiles = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("RememberSelectedLine");
            if (subNode != null)
            {
                settings.General.RememberSelectedLine = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("StartLoadLastFile");
            if (subNode != null)
            {
                settings.General.StartLoadLastFile = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("StartRememberPositionAndSize");
            if (subNode != null)
            {
                settings.General.StartRememberPositionAndSize = Convert.ToBoolean(subNode.InnerText);
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

            subNode = node.SelectSingleNode("SplitContainerMainSplitterDistance");
            if (subNode != null)
            {
                settings.General.SplitContainerMainSplitterDistance = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SplitContainer1SplitterDistance");
            if (subNode != null)
            {
                settings.General.SplitContainer1SplitterDistance = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SplitContainerListViewAndTextSplitterDistance");
            if (subNode != null)
            {
                settings.General.SplitContainerListViewAndTextSplitterDistance = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("StartInSourceView");
            if (subNode != null)
            {
                settings.General.StartInSourceView = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("RemoveBlankLinesWhenOpening");
            if (subNode != null)
            {
                settings.General.RemoveBlankLinesWhenOpening = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("RemoveBadCharsWhenOpening");
            if (subNode != null)
            {
                settings.General.RemoveBadCharsWhenOpening = Convert.ToBoolean(subNode.InnerText);
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
                settings.General.AutoWrapLineWhileTyping = Convert.ToBoolean(subNode.InnerText);
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

            subNode = node.SelectSingleNode("CharactersPerSecondsIgnoreWhiteSpace");
            if (subNode != null)
            {
                settings.General.CharactersPerSecondsIgnoreWhiteSpace = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("SubtitleMaximumWordsPerMinute");
            if (subNode != null)
            {
                settings.General.SubtitleMaximumWordsPerMinute = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
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
            }

            subNode = node.SelectSingleNode("VideoPlayerPreviewFontSize");
            if (subNode != null)
            {
                settings.General.VideoPlayerPreviewFontSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("VideoPlayerPreviewFontBold");
            if (subNode != null)
            {
                settings.General.VideoPlayerPreviewFontBold = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("VideoPlayerShowStopButton");
            if (subNode != null)
            {
                settings.General.VideoPlayerShowStopButton = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("VideoPlayerShowMuteButton");
            if (subNode != null)
            {
                settings.General.VideoPlayerShowMuteButton = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("VideoPlayerShowFullscreenButton");
            if (subNode != null)
            {
                settings.General.VideoPlayerShowFullscreenButton = Convert.ToBoolean(subNode.InnerText);
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
                settings.General.AutoRepeatOn = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AutoRepeatCount");
            if (subNode != null)
            {
                settings.General.AutoRepeatCount = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("SyncListViewWithVideoWhilePlaying");
            if (subNode != null)
            {
                settings.General.SyncListViewWithVideoWhilePlaying = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AutoContinueDelay");
            if (subNode != null)
            {
                settings.General.AutoContinueDelay = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("AutoContinueOn");
            if (subNode != null)
            {
                settings.General.AutoContinueOn = Convert.ToBoolean(subNode.InnerText);
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
                settings.General.AllowEditOfOriginalSubtitle = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("PromptDeleteLines");
            if (subNode != null)
            {
                settings.General.PromptDeleteLines = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("Undocked");
            if (subNode != null)
            {
                settings.General.Undocked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("UndockedVideoPosition");
            if (subNode != null)
            {
                settings.General.UndockedVideoPosition = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("UndockedVideoFullscreen");
            if (subNode != null)
            {
                settings.General.UndockedVideoFullscreen = Convert.ToBoolean(subNode.InnerText);
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
                settings.General.WaveformCenter = Convert.ToBoolean(subNode.InnerText);
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
                settings.General.ShowOriginalAsPreviewIfAvailable = Convert.ToBoolean(subNode.InnerText);
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

            subNode = node.SelectSingleNode("DirectShowDoubleLoadVideo");
            if (subNode != null)
            {
                settings.General.DirectShowDoubleLoadVideo = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
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

            subNode = node.SelectSingleNode("MpvVideoOutput");
            if (subNode != null)
            {
                settings.General.MpvVideoOutput = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("MpvVideoOutputLinux");
            if (subNode != null)
            {
                settings.General.MpvVideoOutputLinux = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("MpvHandlesPreviewText");
            if (subNode != null)
            {
                settings.General.MpvHandlesPreviewText = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("MpcHcLocation");
            if (subNode != null)
            {
                settings.General.MpcHcLocation = subNode.InnerText.Trim();
            }

            subNode = node.SelectSingleNode("UseFFmpegForWaveExtraction");
            if (subNode != null)
            {
                settings.General.UseFFmpegForWaveExtraction = Convert.ToBoolean(subNode.InnerText.Trim());
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

            subNode = node.SelectSingleNode("AllowVolumeBoost");
            if (subNode != null)
            {
                settings.General.AllowVolumeBoost = Convert.ToBoolean(subNode.InnerText.Trim());
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

            subNode = node.SelectSingleNode("ShowProgress");
            if (subNode != null)
            {
                settings.General.ShowProgress = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("ShowNegativeDurationInfoOnSave");
            if (subNode != null)
            {
                settings.General.ShowNegativeDurationInfoOnSave = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("UseDarkTheme");
            if (subNode != null)
            {
                settings.General.UseDarkTheme = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("ShowBetaStuff");
            if (subNode != null)
            {
                settings.General.ShowBetaStuff = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            subNode = node.SelectSingleNode("NewEmptyDefaultMs");
            if (subNode != null)
            {
                settings.General.NewEmptyDefaultMs = Convert.ToInt32(subNode.InnerText.Trim(), CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("MoveVideo100Or500MsPlaySmallSample");
            if (subNode != null)
            {
                settings.General.MoveVideo100Or500MsPlaySmallSample = Convert.ToBoolean(subNode.InnerText.Trim());
            }

            // Tools
            node = doc.DocumentElement.SelectSingleNode("Tools");
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
                settings.Tools.FixShortDisplayTimesAllowMoveStartTime = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("RemoveEmptyLinesBetweenText");
            if (subNode != null)
            {
                settings.Tools.RemoveEmptyLinesBetweenText = Convert.ToBoolean(subNode.InnerText);
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

            subNode = node.SelectSingleNode("SpellCheckAutoChangeNames");
            if (subNode != null)
            {
                settings.Tools.SpellCheckAutoChangeNames = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("SpellCheckOneLetterWords");
            if (subNode != null)
            {
                settings.Tools.CheckOneLetterWords = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("SpellCheckEnglishAllowInQuoteAsIng");
            if (subNode != null)
            {
                settings.Tools.SpellCheckEnglishAllowInQuoteAsIng = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("RememberUseAlwaysList");
            if (subNode != null)
            {
                settings.Tools.RememberUseAlwaysList = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("SpellCheckShowCompletedMessage");
            if (subNode != null)
            {
                settings.Tools.SpellCheckShowCompletedMessage = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("OcrFixUseHardcodedRules");
            if (subNode != null)
            {
                settings.Tools.OcrFixUseHardcodedRules = Convert.ToBoolean(subNode.InnerText);
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

            subNode = node.SelectSingleNode("Interjections");
            if (subNode != null)
            {
                settings.Tools.Interjections = subNode.InnerText;
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

            subNode = node.SelectSingleNode("GoogleApiV2Key");
            if (subNode != null)
            {
                settings.Tools.GoogleApiV2Key = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("GoogleTranslateNoKeyWarningShow");
            if (subNode != null)
            {
                settings.Tools.GoogleTranslateNoKeyWarningShow = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("GoogleApiV2KeyInfoShow");
            if (subNode != null)
            {
                settings.Tools.GoogleApiV2KeyInfoShow = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("UseGooleApiPaidService");
            if (subNode != null)
            {
                settings.Tools.UseGooleApiPaidService = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("GoogleApiV1ChunkSize");
            if (subNode != null)
            {
                settings.Tools.GoogleApiV1ChunkSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("GoogleTranslateLastTargetLanguage");
            if (subNode != null)
            {
                settings.Tools.GoogleTranslateLastTargetLanguage = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("TranslateAutoSplit");
            if (subNode != null)
            {
                settings.Tools.TranslateAutoSplit = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ListViewSyntaxColorDurationSmall");
            if (subNode != null)
            {
                settings.Tools.ListViewSyntaxColorDurationSmall = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ListViewSyntaxColorDurationBig");
            if (subNode != null)
            {
                settings.Tools.ListViewSyntaxColorDurationBig = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ListViewSyntaxColorLongLines");
            if (subNode != null)
            {
                settings.Tools.ListViewSyntaxColorLongLines = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ListViewSyntaxMoreThanXLines");
            if (subNode != null)
            {
                settings.Tools.ListViewSyntaxMoreThanXLines = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ListViewSyntaxColorOverlap");
            if (subNode != null)
            {
                settings.Tools.ListViewSyntaxColorOverlap = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ListViewSyntaxColorGap");
            if (subNode != null)
            {
                settings.Tools.ListViewSyntaxColorGap = Convert.ToBoolean(subNode.InnerText);
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

            subNode = node.SelectSingleNode("ListViewShowColumnEndTime");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnEndTime = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ListViewShowColumnDuration");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnDuration = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ListViewShowColumnCharsPerSec");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnCharsPerSec = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ListViewShowColumnWordsPerMin");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnWordsPerMin = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ListViewShowColumnGap");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnGap = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ListViewShowColumnActor");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnActor = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ListViewShowColumnRegion");
            if (subNode != null)
            {
                settings.Tools.ListViewShowColumnRegion = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("SplitAdvanced");
            if (subNode != null)
            {
                settings.Tools.SplitAdvanced = Convert.ToBoolean(subNode.InnerText);
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
                settings.Tools.JoinCorrectTimeCodes = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("JoinAddMs");
            if (subNode != null)
            {
                settings.Tools.JoinAddMs = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
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
                settings.Tools.BatchConvertOverwriteExisting = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertSaveInSourceFolder");
            if (subNode != null)
            {
                settings.Tools.BatchConvertSaveInSourceFolder = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertRemoveFormatting");
            if (subNode != null)
            {
                settings.Tools.BatchConvertRemoveFormatting = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertBridgeGaps");
            if (subNode != null)
            {
                settings.Tools.BatchConvertBridgeGaps = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertFixCasing");
            if (subNode != null)
            {
                settings.Tools.BatchConvertFixCasing = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertRemoveTextForHI");
            if (subNode != null)
            {
                settings.Tools.BatchConvertRemoveTextForHI = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertFixCommonErrors");
            if (subNode != null)
            {
                settings.Tools.BatchConvertFixCommonErrors = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertMultipleReplace");
            if (subNode != null)
            {
                settings.Tools.BatchConvertMultipleReplace = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertFixRtl");
            if (subNode != null)
            {
                settings.Tools.BatchConvertFixRtl = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertFixRtlMode");
            if (subNode != null)
            {
                settings.Tools.BatchConvertFixRtlMode = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("BatchConvertAutoBalance");
            if (subNode != null)
            {
                settings.Tools.BatchConvertAutoBalance = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertSplitLongLines");
            if (subNode != null)
            {
                settings.Tools.BatchConvertSplitLongLines = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertSetMinDisplayTimeBetweenSubtitles");
            if (subNode != null)
            {
                settings.Tools.BatchConvertSetMinDisplayTimeBetweenSubtitles = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertMergeShortLines");
            if (subNode != null)
            {
                settings.Tools.BatchConvertMergeShortLines = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertRemoveLineBreaks");
            if (subNode != null)
            {
                settings.Tools.BatchConvertRemoveLineBreaks = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertMergeSameText");
            if (subNode != null)
            {
                settings.Tools.BatchConvertMergeSameText = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertMergeSameTimeCodes");
            if (subNode != null)
            {
                settings.Tools.BatchConvertMergeSameTimeCodes = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertChangeSpeed");
            if (subNode != null)
            {
                settings.Tools.BatchConvertChangeSpeed = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertApplyDurationLimits");
            if (subNode != null)
            {
                settings.Tools.BatchConvertApplyDurationLimits = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertChangeFrameRate");
            if (subNode != null)
            {
                settings.Tools.BatchConvertChangeFrameRate = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BatchConvertOffsetTimeCodes");
            if (subNode != null)
            {
                settings.Tools.BatchConvertOffsetTimeCodes = Convert.ToBoolean(subNode.InnerText);
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
                settings.Tools.BatchConvertUseStyleFromSource = Convert.ToBoolean(subNode.InnerText);
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

            subNode = node.SelectSingleNode("BatchConvertTsFileNameAppend");
            if (subNode != null)
            {
                settings.Tools.BatchConvertTsFileNameAppend = subNode.InnerText;
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
                settings.Tools.ModifySelectionCaseSensitive = Convert.ToBoolean(subNode.InnerText);
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
                settings.Tools.ExportVobSubSimpleRendering = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ExportVobAntiAliasingWithTransparency");
            if (subNode != null)
            {
                settings.Tools.ExportVobAntiAliasingWithTransparency = Convert.ToBoolean(subNode.InnerText);
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
                settings.Tools.ExportLastFontBold = Convert.ToBoolean(subNode.InnerText);
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
                settings.Tools.ExportFullFrame = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ExportFcpFullPathUrl");
            if (subNode != null)
            {
                settings.Tools.ExportFcpFullPathUrl = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ExportPenLineJoin");
            if (subNode != null)
            {
                settings.Tools.ExportPenLineJoin = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("FixCommonErrorsFixOverlapAllowEqualEndStart");
            if (subNode != null)
            {
                settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("FixCommonErrorsSkipStepOne");
            if (subNode != null)
            {
                settings.Tools.FixCommonErrorsSkipStepOne = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ImportTextSplitting");
            if (subNode != null)
            {
                settings.Tools.ImportTextSplitting = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ImportTextMergeShortLines");
            if (subNode != null)
            {
                settings.Tools.ImportTextMergeShortLines = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ImportTextLineBreak");
            if (subNode != null)
            {
                settings.Tools.ImportTextLineBreak = subNode.InnerText;
            }

            subNode = node.SelectSingleNode("ImportTextMergeShortLines");
            if (subNode != null)
            {
                settings.Tools.ImportTextMergeShortLines = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ImportTextRemoveEmptyLines");
            if (subNode != null)
            {
                settings.Tools.ImportTextRemoveEmptyLines = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ImportTextAutoSplitAtBlank");
            if (subNode != null)
            {
                settings.Tools.ImportTextAutoSplitAtBlank = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ImportTextRemoveLinesNoLetters");
            if (subNode != null)
            {
                settings.Tools.ImportTextRemoveLinesNoLetters = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ImportTextGenerateTimeCodes");
            if (subNode != null)
            {
                settings.Tools.ImportTextGenerateTimeCodes = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ImportTextAutoBreak");
            if (subNode != null)
            {
                settings.Tools.ImportTextAutoBreak = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ImportTextAutoBreakAtEnd");
            if (subNode != null)
            {
                settings.Tools.ImportTextAutoBreakAtEnd = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ImportTextGap");
            if (subNode != null)
            {
                settings.Tools.ImportTextGap = Convert.ToDecimal(subNode.InnerText);
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
                settings.Tools.ImportTextDurationAuto = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ImportTextFixedDuration");
            if (subNode != null)
            {
                settings.Tools.ImportTextFixedDuration = Convert.ToDecimal(subNode.InnerText);
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

            subNode = node.SelectSingleNode("BridgeGapMilliseconds");
            if (subNode != null)
            {
                settings.Tools.BridgeGapMilliseconds = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
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

            subNode = node.SelectSingleNode("UseNoLineBreakAfter");
            if (subNode != null)
            {
                settings.Tools.UseNoLineBreakAfter = Convert.ToBoolean(subNode.InnerText);
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

            subNode = node.SelectSingleNode("ExportTextRemoveStyling");
            if (subNode != null)
            {
                settings.Tools.ExportTextRemoveStyling = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ExportTextShowLineNumbers");
            if (subNode != null)
            {
                settings.Tools.ExportTextShowLineNumbers = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ExportTextShowLineNumbersNewLine");
            if (subNode != null)
            {
                settings.Tools.ExportTextShowLineNumbersNewLine = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ExportTextShowTimeCodes");
            if (subNode != null)
            {
                settings.Tools.ExportTextShowTimeCodes = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ExportTextShowTimeCodesNewLine");
            if (subNode != null)
            {
                settings.Tools.ExportTextShowTimeCodesNewLine = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ExportTextNewLineAfterText");
            if (subNode != null)
            {
                settings.Tools.ExportTextNewLineAfterText = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ExportTextNewLineBetweenSubtitles");
            if (subNode != null)
            {
                settings.Tools.ExportTextNewLineBetweenSubtitles = Convert.ToBoolean(subNode.InnerText);
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
                settings.Tools.VideoOffsetKeepTimeCodes = Convert.ToBoolean(subNode.InnerText);
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
                settings.Tools.AdjustDurationExtendOnly = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AutoBreakCommaBreakEarly");
            if (subNode != null)
            {
                settings.Tools.AutoBreakCommaBreakEarly = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AutoBreakDashEarly");
            if (subNode != null)
            {
                settings.Tools.AutoBreakDashEarly = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AutoBreakLineEndingEarly");
            if (subNode != null)
            {
                settings.Tools.AutoBreakLineEndingEarly = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AutoBreakUsePixelWidth");
            if (subNode != null)
            {
                settings.Tools.AutoBreakUsePixelWidth = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AutoBreakPreferBottomHeavy");
            if (subNode != null)
            {
                settings.Tools.AutoBreakPreferBottomHeavy = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AutoBreakPreferBottomPercent");
            if (subNode != null)
            {
                settings.Tools.AutoBreakPreferBottomPercent = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("ApplyMinimumDurationLimit");
            if (subNode != null)
            {
                settings.Tools.ApplyMinimumDurationLimit = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ApplyMaximumDurationLimit");
            if (subNode != null)
            {
                settings.Tools.ApplyMaximumDurationLimit = Convert.ToBoolean(subNode.InnerText);
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
                settings.Tools.MergeShortLinesOnlyContinuous = Convert.ToBoolean(subNode.InnerText);
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
                subNode = node.SelectSingleNode("SsaFontName");
                if (subNode != null)
                {
                    settings.SubtitleSettings.SsaFontName = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("SsaFontSize");
                if (subNode != null)
                {
                    settings.SubtitleSettings.SsaFontSize = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("SsaFontColorArgb");
                if (subNode != null)
                {
                    settings.SubtitleSettings.SsaFontColorArgb = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("SsaFontBold");
                if (subNode != null)
                {
                    settings.SubtitleSettings.SsaFontBold = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("SsaOutline");
                if (subNode != null)
                {
                    settings.SubtitleSettings.SsaOutline = Convert.ToDecimal(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("SsaShadow");
                if (subNode != null)
                {
                    settings.SubtitleSettings.SsaShadow = Convert.ToDecimal(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("SsaOpaqueBox");
                if (subNode != null)
                {
                    settings.SubtitleSettings.SsaOpaqueBox = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("SsaMarginLeft");
                if (subNode != null)
                {
                    settings.SubtitleSettings.SsaMarginLeft = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("SsaMarginRight");
                if (subNode != null)
                {
                    settings.SubtitleSettings.SsaMarginRight = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                }

                subNode = node.SelectSingleNode("SsaMarginTopBottom");
                if (subNode != null)
                {
                    settings.SubtitleSettings.SsaMarginTopBottom = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
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

                subNode = node.SelectSingleNode("SamiDisplayTwoClassesAsTwoSubtitles");
                if (subNode != null)
                {
                    settings.SubtitleSettings.SamiDisplayTwoClassesAsTwoSubtitles = Convert.ToBoolean(subNode.InnerText);
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
                    settings.SubtitleSettings.TimedText10ShowStyleAndLanguage = Convert.ToBoolean(subNode.InnerText);
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
                    settings.SubtitleSettings.EbuStlTeletextUseBox = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("EbuStlTeletextUseDoubleHeight");
                if (subNode != null)
                {
                    settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight = Convert.ToBoolean(subNode.InnerText);
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
                    settings.SubtitleSettings.CheetahCaptionAlwayWriteEndTime = Convert.ToBoolean(subNode.InnerText);
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

                subNode = node.SelectSingleNode("WebVttUseXTimestampMap");
                if (subNode != null)
                {
                    settings.SubtitleSettings.WebVttUseXTimestampMap = Convert.ToBoolean(subNode.InnerText, CultureInfo.InvariantCulture);
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
                settings.WordLists.UseOnlineNames = Convert.ToBoolean(subNode.InnerText);
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
                settings.CommonErrors.EmptyLinesTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("OverlappingDisplayTimeTicked");
            if (subNode != null)
            {
                settings.CommonErrors.OverlappingDisplayTimeTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("TooShortDisplayTimeTicked");
            if (subNode != null)
            {
                settings.CommonErrors.TooShortDisplayTimeTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("TooLongDisplayTimeTicked");
            if (subNode != null)
            {
                settings.CommonErrors.TooLongDisplayTimeTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("TooShortGapTicked");
            if (subNode != null)
            {
                settings.CommonErrors.TooShortGapTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("InvalidItalicTagsTicked");
            if (subNode != null)
            {
                settings.CommonErrors.InvalidItalicTagsTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("BreakLongLinesTicked");
            if (subNode != null)
            {
                settings.CommonErrors.BreakLongLinesTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("MergeShortLinesTicked");
            if (subNode != null)
            {
                settings.CommonErrors.MergeShortLinesTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("MergeShortLinesAllTicked");
            if (subNode != null)
            {
                settings.CommonErrors.MergeShortLinesAllTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("UnneededSpacesTicked");
            if (subNode != null)
            {
                settings.CommonErrors.UnneededSpacesTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("UnneededPeriodsTicked");
            if (subNode != null)
            {
                settings.CommonErrors.UnneededPeriodsTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("MissingSpacesTicked");
            if (subNode != null)
            {
                settings.CommonErrors.MissingSpacesTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AddMissingQuotesTicked");
            if (subNode != null)
            {
                settings.CommonErrors.AddMissingQuotesTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("Fix3PlusLinesTicked");
            if (subNode != null)
            {
                settings.CommonErrors.Fix3PlusLinesTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("FixHyphensTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixHyphensTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("FixHyphensAddTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixHyphensAddTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("UppercaseIInsideLowercaseWordTicked");
            if (subNode != null)
            {
                settings.CommonErrors.UppercaseIInsideLowercaseWordTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("DoubleApostropheToQuoteTicked");
            if (subNode != null)
            {
                settings.CommonErrors.DoubleApostropheToQuoteTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AddPeriodAfterParagraphTicked");
            if (subNode != null)
            {
                settings.CommonErrors.AddPeriodAfterParagraphTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("StartWithUppercaseLetterAfterParagraphTicked");
            if (subNode != null)
            {
                settings.CommonErrors.StartWithUppercaseLetterAfterParagraphTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("StartWithUppercaseLetterAfterPeriodInsideParagraphTicked");
            if (subNode != null)
            {
                settings.CommonErrors.StartWithUppercaseLetterAfterPeriodInsideParagraphTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("StartWithUppercaseLetterAfterColonTicked");
            if (subNode != null)
            {
                settings.CommonErrors.StartWithUppercaseLetterAfterColonTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AloneLowercaseIToUppercaseIEnglishTicked");
            if (subNode != null)
            {
                settings.CommonErrors.AloneLowercaseIToUppercaseIEnglishTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("FixOcrErrorsViaReplaceListTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixOcrErrorsViaReplaceListTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("RemoveSpaceBetweenNumberTicked");
            if (subNode != null)
            {
                settings.CommonErrors.RemoveSpaceBetweenNumberTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("FixDialogsOnOneLineTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixDialogsOnOneLineTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("TurkishAnsiTicked");
            if (subNode != null)
            {
                settings.CommonErrors.TurkishAnsiTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("DanishLetterITicked");
            if (subNode != null)
            {
                settings.CommonErrors.DanishLetterITicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("SpanishInvertedQuestionAndExclamationMarksTicked");
            if (subNode != null)
            {
                settings.CommonErrors.SpanishInvertedQuestionAndExclamationMarksTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("FixDoubleDashTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixDoubleDashTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("FixDoubleGreaterThanTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixDoubleGreaterThanTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("FixEllipsesStartTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixEllipsesStartTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("FixMissingOpenBracketTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixMissingOpenBracketTicked = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("FixMusicNotationTicked");
            if (subNode != null)
            {
                settings.CommonErrors.FixMusicNotationTicked = Convert.ToBoolean(subNode.InnerText);
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
                settings.VideoControls.WaveformDrawGrid = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("WaveformDrawCps");
            if (subNode != null)
            {
                settings.VideoControls.WaveformDrawCps = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("WaveformDrawWpm");
            if (subNode != null)
            {
                settings.VideoControls.WaveformDrawWpm = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("WaveformAllowOverlap");
            if (subNode != null)
            {
                settings.VideoControls.WaveformAllowOverlap = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("WaveformFocusOnMouseEnter");
            if (subNode != null)
            {
                settings.VideoControls.WaveformFocusOnMouseEnter = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("WaveformListViewFocusOnMouseEnter");
            if (subNode != null)
            {
                settings.VideoControls.WaveformListViewFocusOnMouseEnter = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("WaveformSetVideoPositionOnMoveStartEnd");
            if (subNode != null)
            {
                settings.VideoControls.WaveformSetVideoPositionOnMoveStartEnd = Convert.ToBoolean(subNode.InnerText);
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

            subNode = node.SelectSingleNode("WaveformTextSize");
            if (subNode != null)
            {
                settings.VideoControls.WaveformTextSize = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("WaveformTextBold");
            if (subNode != null)
            {
                settings.VideoControls.WaveformTextBold = Convert.ToBoolean(subNode.InnerText);
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
                settings.VideoControls.WaveformMouseWheelScrollUpIsForward = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("GenerateSpectrogram");
            if (subNode != null)
            {
                settings.VideoControls.GenerateSpectrogram = Convert.ToBoolean(subNode.InnerText);
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

                subNode = node.SelectSingleNode("WebServiceUrl");
                if (subNode != null)
                {
                    settings.NetworkSettings.WebServiceUrl = subNode.InnerText;
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
                settings.VobSubOcr.UseTesseractFallback = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("UseItalicsInTesseract");
            if (subNode != null)
            {
                settings.VobSubOcr.UseItalicsInTesseract = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("TesseractEngineMode");
            if (subNode != null)
            {
                settings.VobSubOcr.TesseractEngineMode = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("UseMusicSymbolsInTesseract");
            if (subNode != null)
            {
                settings.VobSubOcr.UseMusicSymbolsInTesseract = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("RightToLeft");
            if (subNode != null)
            {
                settings.VobSubOcr.RightToLeft = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("TopToBottom");
            if (subNode != null)
            {
                settings.VobSubOcr.TopToBottom = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("DefaultMillisecondsForUnknownDurations");
            if (subNode != null)
            {
                settings.VobSubOcr.DefaultMillisecondsForUnknownDurations = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("PromptForUnknownWords");
            if (subNode != null)
            {
                settings.VobSubOcr.PromptForUnknownWords = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("GuessUnknownWords");
            if (subNode != null)
            {
                settings.VobSubOcr.GuessUnknownWords = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("AutoBreakSubtitleIfMoreThanTwoLines");
            if (subNode != null)
            {
                settings.VobSubOcr.AutoBreakSubtitleIfMoreThanTwoLines = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("ItalicFactor");
            if (subNode != null)
            {
                settings.VobSubOcr.ItalicFactor = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            }

            subNode = node.SelectSingleNode("LineOcrDraw");
            if (subNode != null)
            {
                settings.VobSubOcr.LineOcrDraw = Convert.ToBoolean(subNode.InnerText);
            }

            subNode = node.SelectSingleNode("LineOcrAdvancedItalic");
            if (subNode != null)
            {
                settings.VobSubOcr.LineOcrAdvancedItalic = Convert.ToBoolean(subNode.InnerText);
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

            subNode = node.SelectSingleNode("LineOcrXOrMorePixelsMakesSpace");
            if (subNode != null)
            {
                settings.VobSubOcr.LineOcrXOrMorePixelsMakesSpace = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
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

            foreach (XmlNode groupNode in doc.DocumentElement.SelectNodes("MultipleSearchAndReplaceGroups/Group"))
            {
                var group = new MultipleSearchAndReplaceGroup();
                group.Rules = new List<MultipleSearchAndReplaceSetting>();
                subNode = groupNode.SelectSingleNode("Name");
                if (subNode != null)
                {
                    group.Name = subNode.InnerText;
                }

                subNode = groupNode.SelectSingleNode("Enabled");
                if (subNode != null)
                {
                    group.Enabled = Convert.ToBoolean(subNode.InnerText);
                }

                settings.MultipleSearchAndReplaceGroups.Add(group);

                foreach (XmlNode listNode in groupNode.SelectNodes("Rule"))
                {
                    var item = new MultipleSearchAndReplaceSetting();
                    subNode = listNode.SelectSingleNode("Enabled");
                    if (subNode != null)
                    {
                        item.Enabled = Convert.ToBoolean(subNode.InnerText);
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
            node = doc.DocumentElement.SelectSingleNode("Shortcuts");
            if (node != null)
            {
                subNode = node.SelectSingleNode("GeneralGoToFirstSelectedLine");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralGoToFirstSelectedLine = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToNextEmptyLine");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralGoToNextEmptyLine = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeSelectedLines");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralMergeSelectedLines = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeSelectedLinesAndUnbreak");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralMergeSelectedLinesAndUnbreak = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeSelectedLinesAndAutoBreak");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralMergeSelectedLinesAndAutoBreak = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeSelectedLinesAndUnbreakCjk");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralMergeSelectedLinesAndUnbreakCjk = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeSelectedLinesOnlyFirstText");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralMergeSelectedLinesOnlyFirstText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeSelectedLinesBilingual");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralMergeSelectedLinesBilingual = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeWithNext");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralMergeWithNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeWithPrevious");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralMergeWithPrevious = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralToggleTranslationMode");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralToggleTranslationMode = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralSwitchOriginalAndTranslation");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralSwitchOriginalAndTranslation = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralMergeOriginalAndTranslation");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralMergeOriginalAndTranslation = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToNextSubtitle");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralGoToNextSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToPrevSubtitle");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralGoToPrevSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToEndOfCurrentSubtitle");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralGoToEndOfCurrentSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToStartOfCurrentSubtitle");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralGoToStartOfCurrentSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToNextSubtitleAndFocusVideo");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralGoToNextSubtitleAndFocusVideo = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToPreviousSubtitleAndFocusVideo");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralGoToPreviousSubtitleAndFocusVideo = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralExtendCurrentSubtitle");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralExtendCurrentSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralAutoCalcCurrentDuration");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralAutoCalcCurrentDuration = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralPlayFirstSelected");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralPlayFirstSelected = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralHelp");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralHelp = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralUnbrekNoSpace");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralUnbrekNoSpace = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralToggleBookmarks");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralToggleBookmarks = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralToggleBookmarksWithText");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralToggleBookmarksWithText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralClearBookmarks");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralClearBookmarks = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToBookmark");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralGoToBookmark = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToPreviousBookmark");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralGoToPreviousBookmark = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("GeneralGoToNextBookmark");
                if (subNode != null)
                {
                    settings.Shortcuts.GeneralGoToNextBookmark = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("ChooseProfile");
                if (subNode != null)
                {
                    settings.Shortcuts.ChooseProfile = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("DuplicateLine");
                if (subNode != null)
                {
                    settings.Shortcuts.DuplicateLine = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileNew");
                if (subNode != null)
                {
                    settings.Shortcuts.MainFileNew = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileOpen");
                if (subNode != null)
                {
                    settings.Shortcuts.MainFileOpen = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileOpenKeepVideo");
                if (subNode != null)
                {
                    settings.Shortcuts.MainFileOpenKeepVideo = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileSave");
                if (subNode != null)
                {
                    settings.Shortcuts.MainFileSave = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileSaveOriginal");
                if (subNode != null)
                {
                    settings.Shortcuts.MainFileSaveOriginal = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileSaveOriginalAs");
                if (subNode != null)
                {
                    settings.Shortcuts.MainFileSaveOriginalAs = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileSaveAs");
                if (subNode != null)
                {
                    settings.Shortcuts.MainFileSaveAs = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileSaveAll");
                if (subNode != null)
                {
                    settings.Shortcuts.MainFileSaveAll = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileCloseOriginal");
                if (subNode != null)
                {
                    settings.Shortcuts.MainFileCloseOriginal = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileOpenOriginal");
                if (subNode != null)
                {
                    settings.Shortcuts.MainFileOpenOriginal = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileImportPlainText");
                if (subNode != null)
                {
                    settings.Shortcuts.MainFileImportPlainText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileImportTimeCodes");
                if (subNode != null)
                {
                    settings.Shortcuts.MainFileImportTimeCodes = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileExportEbu");
                if (subNode != null)
                {
                    settings.Shortcuts.MainFileExportEbu = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainFileExportPlainText");
                if (subNode != null)
                {
                    settings.Shortcuts.MainFileExportPlainText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditUndo");
                if (subNode != null)
                {
                    settings.Shortcuts.MainEditUndo = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditRedo");
                if (subNode != null)
                {
                    settings.Shortcuts.MainEditRedo = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditFind");
                if (subNode != null)
                {
                    settings.Shortcuts.MainEditFind = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditFindNext");
                if (subNode != null)
                {
                    settings.Shortcuts.MainEditFindNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditReplace");
                if (subNode != null)
                {
                    settings.Shortcuts.MainEditReplace = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditMultipleReplace");
                if (subNode != null)
                {
                    settings.Shortcuts.MainEditMultipleReplace = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditGoToLineNumber");
                if (subNode != null)
                {
                    settings.Shortcuts.MainEditGoToLineNumber = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditRightToLeft");
                if (subNode != null)
                {
                    settings.Shortcuts.MainEditRightToLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsFixCommonErrors");
                if (subNode != null)
                {
                    settings.Shortcuts.MainToolsFixCommonErrors = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsFixCommonErrorsPreview");
                if (subNode != null)
                {
                    settings.Shortcuts.MainToolsFixCommonErrorsPreview = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsMergeShortLines");
                if (subNode != null)
                {
                    settings.Shortcuts.MainToolsMergeShortLines = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsMakeEmptyFromCurrent");
                if (subNode != null)
                {
                    settings.Shortcuts.MainToolsMakeEmptyFromCurrent = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsSplitLongLines");
                if (subNode != null)
                {
                    settings.Shortcuts.MainToolsSplitLongLines = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsDurationsBridgeGap");
                if (subNode != null)
                {
                    settings.Shortcuts.MainToolsDurationsBridgeGap = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsMinimumDisplayTimeBetweenParagraphs");
                if (subNode != null)
                {
                    settings.Shortcuts.MainToolsMinimumDisplayTimeBetweenParagraphs = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsRenumber");
                if (subNode != null)
                {
                    settings.Shortcuts.MainToolsRenumber = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsRemoveTextForHI");
                if (subNode != null)
                {
                    settings.Shortcuts.MainToolsRemoveTextForHI = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsChangeCasing");
                if (subNode != null)
                {
                    settings.Shortcuts.MainToolsChangeCasing = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsAutoDuration");
                if (subNode != null)
                {
                    settings.Shortcuts.MainToolsAutoDuration = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsBatchConvert");
                if (subNode != null)
                {
                    settings.Shortcuts.MainToolsBatchConvert = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsBeamer");
                if (subNode != null)
                {
                    settings.Shortcuts.MainToolsBeamer = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToolsToggleTranslationOriginalInPreviews");
                if (subNode != null)
                {
                    settings.Shortcuts.MainEditToggleTranslationOriginalInPreviews = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditInverseSelection");
                if (subNode != null)
                {
                    settings.Shortcuts.MainEditInverseSelection = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditModifySelection");
                if (subNode != null)
                {
                    settings.Shortcuts.MainEditModifySelection = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoOpen");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideoOpen = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoClose");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideoClose = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoPause");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideoPause = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoPlayFromJustBefore");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideoPlayFromJustBefore = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoPlayPauseToggle");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideoPlayPauseToggle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoShowHideVideo");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideoShowHideVideo = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoToggleVideoControls");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideoToggleVideoControls = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo1FrameLeft");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideo1FrameLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo1FrameRight");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideo1FrameRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo1FrameLeftWithPlay");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideo1FrameLeftWithPlay = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo1FrameRightWithPlay");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideo1FrameRightWithPlay = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo100MsLeft");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideo100MsLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo100MsRight");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideo100MsRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo500MsLeft");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideo500MsLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo500MsRight");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideo500MsRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo1000MsLeft");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideo1000MsLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo1000MsRight");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideo1000MsRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo5000MsLeft");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideo5000MsLeft = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideo5000MsRight");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideo5000MsRight = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoGoToPrevSubtitle");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideoGoToPrevSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoGoToNextSubtitle");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideoGoToNextSubtitle = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoFullscreen");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideoFullscreen = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoSlower");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideoSlower = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoFaster");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideoFaster = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainVideoReset");
                if (subNode != null)
                {
                    settings.Shortcuts.MainVideoReset = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSpellCheck");
                if (subNode != null)
                {
                    settings.Shortcuts.MainSpellCheck = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSpellCheckFindDoubleWords");
                if (subNode != null)
                {
                    settings.Shortcuts.MainSpellCheckFindDoubleWords = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSpellCheckAddWordToNames");
                if (subNode != null)
                {
                    settings.Shortcuts.MainSpellCheckAddWordToNames = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSynchronizationAdjustTimes");
                if (subNode != null)
                {
                    settings.Shortcuts.MainSynchronizationAdjustTimes = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSynchronizationVisualSync");
                if (subNode != null)
                {
                    settings.Shortcuts.MainSynchronizationVisualSync = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSynchronizationPointSync");
                if (subNode != null)
                {
                    settings.Shortcuts.MainSynchronizationPointSync = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSynchronizationPointSyncViaFile");
                if (subNode != null)
                {
                    settings.Shortcuts.MainSynchronizationPointSyncViaFile = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainSynchronizationChangeFrameRate");
                if (subNode != null)
                {
                    settings.Shortcuts.MainSynchronizationChangeFrameRate = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewItalic");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewItalic = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewBold");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewBold = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewUnderline");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewUnderline = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewToggleDashes");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewToggleDashes = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewToggleMusicSymbols");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewToggleMusicSymbols = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignment");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewAlignment = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN1");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewAlignmentN1 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN1");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewAlignmentN1 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN2");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewAlignmentN2 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN3");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewAlignmentN3 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN4");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewAlignmentN4 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN5");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewAlignmentN5 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN6");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewAlignmentN6 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN7");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewAlignmentN7 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN8");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewAlignmentN8 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAlignmentN9");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewAlignmentN9 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainRemoveFormatting");
                if (subNode != null)
                {
                    settings.Shortcuts.MainRemoveFormatting = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewCopyText");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewCopyText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewCopyTextFromOriginalToCurrent");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewCopyTextFromOriginalToCurrent = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewAutoDuration");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewAutoDuration = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColumnDeleteText");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewColumnDeleteText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColumnDeleteTextAndShiftUp");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewColumnDeleteTextAndShiftUp = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColumnInsertText");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewColumnInsertText = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColumnPaste");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewColumnPaste = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColumnTextUp");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewColumnTextUp = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewColumnTextDown");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewColumnTextDown = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewFocusWaveform");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewFocusWaveform = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainListViewGoToNextError");
                if (subNode != null)
                {
                    settings.Shortcuts.MainListViewGoToNextError = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainEditReverseStartAndEndingForRTL");
                if (subNode != null)
                {
                    settings.Shortcuts.MainEditReverseStartAndEndingForRTL = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxItalic");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxItalic = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxSplitAtCursor");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxSplitAtCursor = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxSplitAtCursorAndVideoPos");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxSplitAtCursorAndVideoPos = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxSplitSelectedLineBilingual");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxSplitSelectedLineBilingual = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxMoveLastWordDown");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxMoveLastWordDown = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxMoveFirstWordFromNextUp");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxMoveFirstWordFromNextUp = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxMoveLastWordDownCurrent");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxMoveLastWordDownCurrent = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxMoveFirstWordUpCurrent");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxMoveFirstWordUpCurrent = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxSelectionToLower");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxSelectionToLower = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxSelectionToUpper");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxSelectionToUpper = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxSelectionToRuby");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxSelectionToRuby = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxToggleAutoDuration");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxToggleAutoDuration = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainCreateInsertSubAtVideoPos");
                if (subNode != null)
                {
                    settings.Shortcuts.MainCreateInsertSubAtVideoPos = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainCreateSetStart");
                if (subNode != null)
                {
                    settings.Shortcuts.MainCreateSetStart = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainCreateSetEnd");
                if (subNode != null)
                {
                    settings.Shortcuts.MainCreateSetEnd = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainCreateSetEndAddNewAndGoToNew");
                if (subNode != null)
                {
                    settings.Shortcuts.MainCreateSetEndAddNewAndGoToNew = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainCreateStartDownEndUp");
                if (subNode != null)
                {
                    settings.Shortcuts.MainCreateStartDownEndUp = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetStartAndOffsetTheRest");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustSetStartAndOffsetTheRest = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetEndAndOffsetTheRest");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustSetEndAndOffsetTheRest = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetEndAndOffsetTheRestAndGoToNext");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustSetEndAndOffsetTheRestAndGoToNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetEndAndGotoNext");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustSetEndAndGotoNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustViaEndAutoStart");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustViaEndAutoStart = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustViaEndAutoStartAndGoToNext");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustViaEndAutoStartAndGoToNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetStartAutoDurationAndGoToNext");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustSetStartAutoDurationAndGoToNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetEndNextStartAndGoToNext");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustSetEndNextStartAndGoToNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustStartDownEndUpAndGoToNext");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustStartDownEndUpAndGoToNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSetStartKeepDuration");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustSetStartKeepDuration = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSelected100MsForward");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustSelected100MsForward = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustSelected100MsBack");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustSelected100MsBack = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustStartXMsBack");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustStartXMsBack = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustStartXMsForward");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustStartXMsForward = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustEndXMsBack");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustEndXMsBack = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainAdjustEndXMsForward");
                if (subNode != null)
                {
                    settings.Shortcuts.MainAdjustEndXMsForward = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainInsertAfter");
                if (subNode != null)
                {
                    settings.Shortcuts.MainInsertAfter = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxAutoBreak");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxAutoBreak = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxBreakAtPosition");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxBreakAtPosition = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxBreakAtPositionAndGoToNext");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxBreakAtPositionAndGoToNext = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTextBoxUnbreak");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTextBoxUnbreak = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainWaveformInsertAtCurrentPosition");
                if (subNode != null)
                {
                    settings.Shortcuts.MainWaveformInsertAtCurrentPosition = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainInsertBefore");
                if (subNode != null)
                {
                    settings.Shortcuts.MainInsertBefore = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainMergeDialog");
                if (subNode != null)
                {
                    settings.Shortcuts.MainMergeDialog = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainToggleFocus");
                if (subNode != null)
                {
                    settings.Shortcuts.MainToggleFocus = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformVerticalZoom");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformVerticalZoom = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformVerticalZoomOut");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformVerticalZoomOut = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformZoomIn");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformZoomIn = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformZoomOut");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformZoomOut = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformSplit");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformSplit = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformPlaySelection");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformPlaySelection = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformPlaySelectionEnd");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformPlaySelectionEnd = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformSearchSilenceForward");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformSearchSilenceForward = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformSearchSilenceBack");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformSearchSilenceBack = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformAddTextHere");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformAddTextHere = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformAddTextHereFromClipboard");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformAddTextHereFromClipboard = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformSetParagraphAsSelection");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformSetParagraphAsSelection = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformFocusListView");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformFocusListView = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformGoToPreviousSceneChange");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformGoToPreviousSceneChange = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformGoToNextSceneChange");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformGoToNextSceneChange = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("WaveformToggleSceneChange");
                if (subNode != null)
                {
                    settings.Shortcuts.WaveformToggleSceneChange = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateGoogleIt");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTranslateGoogleIt = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateGoogleTranslate");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTranslateGoogleTranslate = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateCustomSearch1");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTranslateCustomSearch1 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateCustomSearch2");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTranslateCustomSearch2 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateCustomSearch3");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTranslateCustomSearch3 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateCustomSearch4");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTranslateCustomSearch4 = subNode.InnerText;
                }

                subNode = node.SelectSingleNode("MainTranslateCustomSearch5");
                if (subNode != null)
                {
                    settings.Shortcuts.MainTranslateCustomSearch5 = subNode.InnerText;
                }
            }

            // Remove text for Hearing Impaired
            node = doc.DocumentElement.SelectSingleNode("RemoveTextForHearingImpaired");
            if (node != null)
            {
                subNode = node.SelectSingleNode("RemoveTextBetweenBrackets");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenBrackets = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("RemoveTextBetweenParentheses");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenParentheses = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("RemoveTextBetweenCurlyBrackets");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenCurlyBrackets = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("RemoveTextBetweenQuestionMarks");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenQuestionMarks = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("RemoveTextBetweenCustom");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustom = Convert.ToBoolean(subNode.InnerText);
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

                subNode = node.SelectSingleNode("RemoveTextBetweenOnlySeperateLines");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenOnlySeperateLines = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("RemoveTextBeforeColon");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBeforeColon = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("RemoveTextBeforeColonOnlyIfUppercase");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyIfUppercase = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("RemoveTextBeforeColonOnlyOnSeparateLine");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyOnSeparateLine = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("RemoveIfAllUppercase");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveIfAllUppercase = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("RemoveInterjections");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveInterjections = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("RemoveInterjectionsOnlyOnSeparateLine");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveInterjectionsOnlyOnSeparateLine = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("RemoveIfContains");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveIfContains = Convert.ToBoolean(subNode.InnerText);
                }

                subNode = node.SelectSingleNode("RemoveIfContainsText");
                if (subNode != null)
                {
                    settings.RemoveTextForHearingImpaired.RemoveIfContainsText = subNode.InnerText;
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

            if (profileCount == 0)
            {
                settings.General.CurrentProfile = "Default";
                settings.General.Profiles = new List<RulesProfile>();
                settings.General.Profiles.Add(new RulesProfile
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
                    CpsIncludesSpace = !settings.General.CharactersPerSecondsIgnoreWhiteSpace,
                    MinimumMillisecondsBetweenLines = settings.General.MinimumMillisecondsBetweenLines,
                });
                GeneralSettings.AddExtraProfiles(settings.General.Profiles);
            }

            return settings;
        }

        private static void CustomSerialize(string fileName, Settings settings)
        {
            var xws = new XmlWriterSettings { Indent = true };
            var sb = new StringBuilder();
            using (var textWriter = XmlWriter.Create(sb, xws))
            {
                textWriter.WriteStartDocument();

                textWriter.WriteStartElement("Settings", string.Empty);

                textWriter.WriteStartElement("Compare", string.Empty);
                textWriter.WriteElementString("ShowOnlyDifferences", settings.Compare.ShowOnlyDifferences.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OnlyLookForDifferenceInText", settings.Compare.OnlyLookForDifferenceInText.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("IgnoreLineBreaks", settings.Compare.IgnoreLineBreaks.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("IgnoreFormatting", settings.Compare.IgnoreFormatting.ToString(CultureInfo.InvariantCulture));
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
                    }

                    textWriter.WriteAttributeString("FirstVisibleIndex", item.FirstVisibleIndex.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteAttributeString("FirstSelectedIndex", item.FirstSelectedIndex.ToString(CultureInfo.InvariantCulture));
                    if (item.VideoOffsetInMs != 0)
                    {
                        textWriter.WriteAttributeString("VideoOffset", item.VideoOffsetInMs.ToString(CultureInfo.InvariantCulture));
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
                    textWriter.WriteElementString("CpsIncludesSpace", profile.CpsIncludesSpace.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteElementString("MaxNumberOfLines", profile.MaxNumberOfLines.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteElementString("MergeLinesShorterThan", profile.MergeLinesShorterThan.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteEndElement();
                }
                textWriter.WriteEndElement();

                textWriter.WriteElementString("CurrentProfile", settings.General.CurrentProfile);
                textWriter.WriteElementString("ShowToolbarNew", settings.General.ShowToolbarNew.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarOpen", settings.General.ShowToolbarOpen.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarSave", settings.General.ShowToolbarSave.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarSaveAs", settings.General.ShowToolbarSaveAs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarFind", settings.General.ShowToolbarFind.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarReplace", settings.General.ShowToolbarReplace.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarFixCommonErrors", settings.General.ShowToolbarFixCommonErrors.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarRemoveTextForHi", settings.General.ShowToolbarRemoveTextForHi.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarVisualSync", settings.General.ShowToolbarVisualSync.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarSpellCheck", settings.General.ShowToolbarSpellCheck.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarNetflixGlyphCheck", settings.General.ShowToolbarNetflixGlyphCheck.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarSettings", settings.General.ShowToolbarSettings.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowToolbarHelp", settings.General.ShowToolbarHelp.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowFrameRate", settings.General.ShowFrameRate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowVideoPlayer", settings.General.ShowVideoPlayer.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowAudioVisualizer", settings.General.ShowAudioVisualizer.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowWaveform", settings.General.ShowWaveform.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowSpectrogram", settings.General.ShowSpectrogram.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DefaultFrameRate", settings.General.DefaultFrameRate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DefaultSubtitleFormat", settings.General.DefaultSubtitleFormat);
                textWriter.WriteElementString("DefaultEncoding", settings.General.DefaultEncoding);
                textWriter.WriteElementString("AutoConvertToUtf8", settings.General.AutoConvertToUtf8.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoGuessAnsiEncoding", settings.General.AutoGuessAnsiEncoding.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SystemSubtitleFontNameOverride", settings.General.SystemSubtitleFontNameOverride);
                textWriter.WriteElementString("SystemSubtitleFontSizeOverride", settings.General.SystemSubtitleFontSizeOverride.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleFontName", settings.General.SubtitleFontName);
                textWriter.WriteElementString("SubtitleFontSize", settings.General.SubtitleFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleListViewFontSize", settings.General.SubtitleListViewFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleFontBold", settings.General.SubtitleFontBold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleListViewFontBold", settings.General.SubtitleListViewFontBold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleFontColor", settings.General.SubtitleFontColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleBackgroundColor", settings.General.SubtitleBackgroundColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CenterSubtitleInTextBox", settings.General.CenterSubtitleInTextBox.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowRecentFiles", settings.General.ShowRecentFiles.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RememberSelectedLine", settings.General.RememberSelectedLine.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("StartLoadLastFile", settings.General.StartLoadLastFile.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("StartRememberPositionAndSize", settings.General.StartRememberPositionAndSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("StartPosition", settings.General.StartPosition);
                textWriter.WriteElementString("StartSize", settings.General.StartSize);
                textWriter.WriteElementString("SplitContainerMainSplitterDistance", settings.General.SplitContainerMainSplitterDistance.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitContainer1SplitterDistance", settings.General.SplitContainer1SplitterDistance.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitContainerListViewAndTextSplitterDistance", settings.General.SplitContainerListViewAndTextSplitterDistance.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("StartInSourceView", settings.General.StartInSourceView.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveBlankLinesWhenOpening", settings.General.RemoveBlankLinesWhenOpening.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveBadCharsWhenOpening", settings.General.RemoveBadCharsWhenOpening.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleLineMaximumLength", settings.General.SubtitleLineMaximumLength.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MaxNumberOfLines", settings.General.MaxNumberOfLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeLinesShorterThan", settings.General.MergeLinesShorterThan.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleMinimumDisplayMilliseconds", settings.General.SubtitleMinimumDisplayMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleMaximumDisplayMilliseconds", settings.General.SubtitleMaximumDisplayMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MinimumMillisecondsBetweenLines", settings.General.MinimumMillisecondsBetweenLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SetStartEndHumanDelay", settings.General.SetStartEndHumanDelay.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoWrapLineWhileTyping", settings.General.AutoWrapLineWhileTyping.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleMaximumCharactersPerSeconds", settings.General.SubtitleMaximumCharactersPerSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleOptimalCharactersPerSeconds", settings.General.SubtitleOptimalCharactersPerSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CharactersPerSecondsIgnoreWhiteSpace", settings.General.CharactersPerSecondsIgnoreWhiteSpace.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleMaximumWordsPerMinute", settings.General.SubtitleMaximumWordsPerMinute.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpellCheckLanguage", settings.General.SpellCheckLanguage);
                textWriter.WriteElementString("VideoPlayer", settings.General.VideoPlayer);
                textWriter.WriteElementString("VideoPlayerDefaultVolume", settings.General.VideoPlayerDefaultVolume.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VideoPlayerPreviewFontSize", settings.General.VideoPlayerPreviewFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VideoPlayerPreviewFontBold", settings.General.VideoPlayerPreviewFontBold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VideoPlayerShowStopButton", settings.General.VideoPlayerShowStopButton.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VideoPlayerShowStopMute", settings.General.VideoPlayerShowMuteButton.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VideoPlayerShowStopFullscreen", settings.General.VideoPlayerShowFullscreenButton.ToString(CultureInfo.InvariantCulture));
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
                textWriter.WriteElementString("DirectShowDoubleLoadVideo", settings.General.DirectShowDoubleLoadVideo.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VlcWaveTranscodeSettings", settings.General.VlcWaveTranscodeSettings);
                textWriter.WriteElementString("VlcLocation", settings.General.VlcLocation);
                textWriter.WriteElementString("VlcLocationRelative", settings.General.VlcLocationRelative);
                textWriter.WriteElementString("MpvVideoOutput", settings.General.MpvVideoOutput);
                textWriter.WriteElementString("MpvVideoOutputLinux", settings.General.MpvVideoOutputLinux);
                textWriter.WriteElementString("MpvHandlesPreviewText", settings.General.MpvHandlesPreviewText.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MpcHcLocation", settings.General.MpcHcLocation);
                textWriter.WriteElementString("UseFFmpegForWaveExtraction", settings.General.UseFFmpegForWaveExtraction.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FFmpegLocation", settings.General.FFmpegLocation);
                textWriter.WriteElementString("FFmpegSceneThreshold", settings.General.FFmpegSceneThreshold);
                textWriter.WriteElementString("UseTimeFormatHHMMSSFF", settings.General.UseTimeFormatHHMMSSFF.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ClearStatusBarAfterSeconds", settings.General.ClearStatusBarAfterSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Company", settings.General.Company);
                textWriter.WriteElementString("MoveVideo100Or500MsPlaySmallSample", settings.General.MoveVideo100Or500MsPlaySmallSample.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DisableVideoAutoLoading", settings.General.DisableVideoAutoLoading.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AllowVolumeBoost", settings.General.AllowVolumeBoost.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RightToLeftMode", settings.General.RightToLeftMode.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LastSaveAsFormat", settings.General.LastSaveAsFormat);
                textWriter.WriteElementString("CheckForUpdates", settings.General.CheckForUpdates.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LastCheckForUpdates", settings.General.LastCheckForUpdates.ToString("yyyy-MM-dd"));
                textWriter.WriteElementString("AutoSave", settings.General.AutoSave.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("PreviewAssaText", settings.General.PreviewAssaText);
                textWriter.WriteElementString("ShowProgress", settings.General.ShowProgress.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowNegativeDurationInfoOnSave", settings.General.ShowNegativeDurationInfoOnSave.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UseDarkTheme", settings.General.UseDarkTheme.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowBetaStuff", settings.General.ShowBetaStuff.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("NewEmptyDefaultMs", settings.General.NewEmptyDefaultMs.ToString(CultureInfo.InvariantCulture));

                textWriter.WriteEndElement();

                textWriter.WriteStartElement("Tools", string.Empty);
                textWriter.WriteElementString("StartSceneIndex", settings.Tools.StartSceneIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("EndSceneIndex", settings.Tools.EndSceneIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VerifyPlaySeconds", settings.Tools.VerifyPlaySeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixShortDisplayTimesAllowMoveStartTime", settings.Tools.FixShortDisplayTimesAllowMoveStartTime.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveEmptyLinesBetweenText", settings.Tools.RemoveEmptyLinesBetweenText.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MusicSymbol", settings.Tools.MusicSymbol);
                textWriter.WriteElementString("MusicSymbolReplace", settings.Tools.MusicSymbolReplace);
                textWriter.WriteElementString("UnicodeSymbolsToInsert", settings.Tools.UnicodeSymbolsToInsert);
                textWriter.WriteElementString("SpellCheckAutoChangeNames", settings.Tools.SpellCheckAutoChangeNames.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpellCheckOneLetterWords", settings.Tools.CheckOneLetterWords.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpellCheckEnglishAllowInQuoteAsIng", settings.Tools.SpellCheckEnglishAllowInQuoteAsIng.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RememberUseAlwaysList", settings.Tools.RememberUseAlwaysList.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpellCheckShowCompletedMessage", settings.Tools.SpellCheckShowCompletedMessage.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OcrFixUseHardcodedRules", settings.Tools.OcrFixUseHardcodedRules.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OcrBinaryImageCompareRgbThreshold", settings.Tools.OcrBinaryImageCompareRgbThreshold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OcrTesseract4RgbThreshold", settings.Tools.OcrTesseract4RgbThreshold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Interjections", settings.Tools.Interjections);
                textWriter.WriteElementString("MicrosoftBingApiId", settings.Tools.MicrosoftBingApiId);
                textWriter.WriteElementString("MicrosoftTranslatorApiKey", settings.Tools.MicrosoftTranslatorApiKey);
                textWriter.WriteElementString("MicrosoftTranslatorTokenEndpoint", settings.Tools.MicrosoftTranslatorTokenEndpoint);
                textWriter.WriteElementString("GoogleApiV2Key", settings.Tools.GoogleApiV2Key);
                textWriter.WriteElementString("GoogleApiV2KeyInfoShow", settings.Tools.GoogleApiV2KeyInfoShow.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GoogleTranslateNoKeyWarningShow", settings.Tools.GoogleTranslateNoKeyWarningShow.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UseGooleApiPaidService", settings.Tools.UseGooleApiPaidService.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GoogleApiV1ChunkSize", settings.Tools.GoogleApiV1ChunkSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GoogleTranslateLastTargetLanguage", settings.Tools.GoogleTranslateLastTargetLanguage);
                textWriter.WriteElementString("TranslateAutoSplit", settings.Tools.TranslateAutoSplit.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxColorDurationSmall", settings.Tools.ListViewSyntaxColorDurationSmall.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxColorDurationBig", settings.Tools.ListViewSyntaxColorDurationBig.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxColorLongLines", settings.Tools.ListViewSyntaxColorLongLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxMoreThanXLines", settings.Tools.ListViewSyntaxMoreThanXLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxColorOverlap", settings.Tools.ListViewSyntaxColorOverlap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxColorGap", settings.Tools.ListViewSyntaxColorGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxErrorColor", settings.Tools.ListViewSyntaxErrorColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewUnfocusedSelectedColor", settings.Tools.ListViewUnfocusedSelectedColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewShowColumnEndTime", settings.Tools.ListViewShowColumnEndTime.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewShowColumnDuration", settings.Tools.ListViewShowColumnDuration.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewShowColumnCharsPerSec", settings.Tools.ListViewShowColumnCharsPerSec.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewShowColumnWordsPerMin", settings.Tools.ListViewShowColumnWordsPerMin.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewShowColumnGap", settings.Tools.ListViewShowColumnGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewShowColumnActor", settings.Tools.ListViewShowColumnActor.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewShowColumnRegion", settings.Tools.ListViewShowColumnRegion.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitAdvanced", settings.Tools.SplitAdvanced.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitOutputFolder", settings.Tools.SplitOutputFolder);
                textWriter.WriteElementString("SplitNumberOfParts", settings.Tools.SplitNumberOfParts.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitVia", settings.Tools.SplitVia);
                textWriter.WriteElementString("JoinCorrectTimeCodes", settings.Tools.JoinCorrectTimeCodes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("JoinAddMs", settings.Tools.JoinAddMs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("NewEmptyTranslationText", settings.Tools.NewEmptyTranslationText);
                textWriter.WriteElementString("BatchConvertOutputFolder", settings.Tools.BatchConvertOutputFolder);
                textWriter.WriteElementString("BatchConvertOverwriteExisting", settings.Tools.BatchConvertOverwriteExisting.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertSaveInSourceFolder", settings.Tools.BatchConvertSaveInSourceFolder.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertRemoveFormatting", settings.Tools.BatchConvertRemoveFormatting.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertBridgeGaps", settings.Tools.BatchConvertBridgeGaps.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertFixCasing", settings.Tools.BatchConvertFixCasing.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertRemoveTextForHI", settings.Tools.BatchConvertRemoveTextForHI.ToString(CultureInfo.InvariantCulture));
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
                textWriter.WriteElementString("BatchConvertApplyDurationLimits", settings.Tools.BatchConvertApplyDurationLimits.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertChangeFrameRate", settings.Tools.BatchConvertChangeFrameRate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BatchConvertOffsetTimeCodes", settings.Tools.BatchConvertOffsetTimeCodes.ToString(CultureInfo.InvariantCulture));
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
                textWriter.WriteElementString("BatchConvertTsFileNameAppend", settings.Tools.BatchConvertTsFileNameAppend);
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
                textWriter.WriteElementString("Export3DType", settings.Tools.Export3DType.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Export3DDepth", settings.Tools.Export3DDepth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportLastShadowTransparency", settings.Tools.ExportLastShadowTransparency.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportLastFrameRate", settings.Tools.ExportLastFrameRate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportFullFrame", settings.Tools.ExportFullFrame.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportFcpFullPathUrl", settings.Tools.ExportFcpFullPathUrl.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportPenLineJoin", settings.Tools.ExportPenLineJoin);
                textWriter.WriteElementString("FixCommonErrorsFixOverlapAllowEqualEndStart", settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixCommonErrorsSkipStepOne", settings.Tools.FixCommonErrorsSkipStepOne.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextSplitting", settings.Tools.ImportTextSplitting);
                textWriter.WriteElementString("ImportTextMergeShortLines", settings.Tools.ImportTextMergeShortLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextLineBreak", settings.Tools.ImportTextLineBreak);
                textWriter.WriteElementString("ImportTextMergeShortLines", settings.Tools.ImportTextMergeShortLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextRemoveEmptyLines", settings.Tools.ImportTextRemoveEmptyLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextAutoSplitAtBlank", settings.Tools.ImportTextAutoSplitAtBlank.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextRemoveLinesNoLetters", settings.Tools.ImportTextRemoveLinesNoLetters.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextGenerateTimeCodes", settings.Tools.ImportTextGenerateTimeCodes.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextAutoBreak", settings.Tools.ImportTextAutoBreak.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextAutoBreakAtEnd", settings.Tools.ImportTextAutoBreakAtEnd.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextGap", settings.Tools.ImportTextGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextAutoSplitNumberOfLines", settings.Tools.ImportTextAutoSplitNumberOfLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextAutoBreakAtEndMarkerText", settings.Tools.ImportTextAutoBreakAtEndMarkerText);
                textWriter.WriteElementString("ImportTextDurationAuto", settings.Tools.ImportTextDurationAuto.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ImportTextFixedDuration", settings.Tools.ImportTextFixedDuration.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenerateTimeCodePatterns", settings.Tools.GenerateTimeCodePatterns);
                textWriter.WriteElementString("GenerateTimeCodePatterns", settings.Tools.GenerateTimeCodePatterns);
                textWriter.WriteElementString("MusicSymbolStyle", settings.Tools.MusicSymbolStyle);
                textWriter.WriteElementString("BridgeGapMilliseconds", settings.Tools.BridgeGapMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportCustomTemplates", settings.Tools.ExportCustomTemplates);
                textWriter.WriteElementString("ChangeCasingChoice", settings.Tools.ChangeCasingChoice);
                textWriter.WriteElementString("UseNoLineBreakAfter", settings.Tools.UseNoLineBreakAfter.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("NoLineBreakAfterEnglish", settings.Tools.NoLineBreakAfterEnglish);
                textWriter.WriteElementString("UseNoLineBreakAfter", settings.Tools.ExportTextFormatText);
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
                textWriter.WriteElementString("AutoBreakCommaBreakEarly", settings.Tools.AutoBreakCommaBreakEarly.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBreakDashEarly", settings.Tools.AutoBreakDashEarly.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBreakLineEndingEarly", settings.Tools.AutoBreakLineEndingEarly.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBreakUsePixelWidth", settings.Tools.AutoBreakUsePixelWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBreakPreferBottomHeavy", settings.Tools.AutoBreakPreferBottomHeavy.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBreakPreferBottomPercent", settings.Tools.AutoBreakPreferBottomPercent.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ApplyMinimumDurationLimit", settings.Tools.ApplyMinimumDurationLimit.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ApplyMaximumDurationLimit", settings.Tools.ApplyMaximumDurationLimit.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeShortLinesMaxGap", settings.Tools.MergeShortLinesMaxGap.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeShortLinesMaxChars", settings.Tools.MergeShortLinesMaxChars.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeShortLinesOnlyContinuous", settings.Tools.MergeShortLinesOnlyContinuous.ToString(CultureInfo.InvariantCulture));

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
                textWriter.WriteElementString("SsaFontName", settings.SubtitleSettings.SsaFontName);
                textWriter.WriteElementString("SsaFontSize", settings.SubtitleSettings.SsaFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SsaFontColorArgb", settings.SubtitleSettings.SsaFontColorArgb.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SsaFontBold", settings.SubtitleSettings.SsaFontBold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SsaOutline", settings.SubtitleSettings.SsaOutline.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SsaShadow", settings.SubtitleSettings.SsaShadow.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SsaOpaqueBox", settings.SubtitleSettings.SsaOpaqueBox.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SsaMarginLeft", settings.SubtitleSettings.SsaMarginLeft.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SsaMarginRight", settings.SubtitleSettings.SsaMarginRight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SsaMarginTopBottom", settings.SubtitleSettings.SsaMarginTopBottom.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DCinemaFontFile", settings.SubtitleSettings.DCinemaFontFile);
                textWriter.WriteElementString("DCinemaFontSize", settings.SubtitleSettings.DCinemaFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DCinemaBottomMargin", settings.SubtitleSettings.DCinemaBottomMargin.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DCinemaZPosition", settings.SubtitleSettings.DCinemaZPosition.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DCinemaFadeUpTime", settings.SubtitleSettings.DCinemaFadeUpTime.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DCinemaFadeDownTime", settings.SubtitleSettings.DCinemaFadeDownTime.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SamiDisplayTwoClassesAsTwoSubtitles", settings.SubtitleSettings.SamiDisplayTwoClassesAsTwoSubtitles.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SamiFullHtmlEncode", settings.SubtitleSettings.SamiHtmlEncodeMode.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TimedText10TimeCodeFormat", settings.SubtitleSettings.TimedText10TimeCodeFormat);
                textWriter.WriteElementString("TimedText10ShowStyleAndLanguage", settings.SubtitleSettings.TimedText10ShowStyleAndLanguage.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FcpFontSize", settings.SubtitleSettings.FcpFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FcpFontName", settings.SubtitleSettings.FcpFontName);
                textWriter.WriteElementString("Cavena890StartOfMessage", settings.SubtitleSettings.Cavena890StartOfMessage);
                textWriter.WriteElementString("EbuStlTeletextUseBox", settings.SubtitleSettings.EbuStlTeletextUseBox.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("EbuStlTeletextUseDoubleHeight", settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("EbuStlMarginTop", settings.SubtitleSettings.EbuStlMarginTop.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("EbuStlMarginBottom", settings.SubtitleSettings.EbuStlMarginBottom.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("EbuStlNewLineRows", settings.SubtitleSettings.EbuStlNewLineRows.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DvdStudioProHeader", settings.SubtitleSettings.DvdStudioProHeader.TrimEnd() + Environment.NewLine);
                textWriter.WriteElementString("TmpegEncXmlFontName", settings.SubtitleSettings.TmpegEncXmlFontName.TrimEnd());
                textWriter.WriteElementString("TmpegEncXmlFontHeight", settings.SubtitleSettings.TmpegEncXmlFontHeight.TrimEnd());
                textWriter.WriteElementString("TmpegEncXmlPosition", settings.SubtitleSettings.TmpegEncXmlPosition.TrimEnd());
                textWriter.WriteElementString("CheetahCaptionAlwayWriteEndTime", settings.SubtitleSettings.CheetahCaptionAlwayWriteEndTime.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("NuendoCharacterListFile", settings.SubtitleSettings.NuendoCharacterListFile);
                textWriter.WriteElementString("WebVttTimescale", settings.SubtitleSettings.WebVttTimescale.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WebVttUseXTimestampMap", settings.SubtitleSettings.WebVttUseXTimestampMap.ToString(CultureInfo.InvariantCulture));
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
                textWriter.WriteElementString("UnneededSpacesTicked", settings.CommonErrors.UnneededSpacesTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UnneededPeriodsTicked", settings.CommonErrors.UnneededPeriodsTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MissingSpacesTicked", settings.CommonErrors.MissingSpacesTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AddMissingQuotesTicked", settings.CommonErrors.AddMissingQuotesTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Fix3PlusLinesTicked", settings.CommonErrors.Fix3PlusLinesTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixHyphensTicked", settings.CommonErrors.FixHyphensTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixHyphensAddTicked", settings.CommonErrors.FixHyphensAddTicked.ToString(CultureInfo.InvariantCulture));
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
                textWriter.WriteElementString("TurkishAnsiTicked", settings.CommonErrors.TurkishAnsiTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DanishLetterITicked", settings.CommonErrors.DanishLetterITicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpanishInvertedQuestionAndExclamationMarksTicked", settings.CommonErrors.SpanishInvertedQuestionAndExclamationMarksTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixDoubleDashTicked", settings.CommonErrors.FixDoubleDashTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixDoubleGreaterThanTicked", settings.CommonErrors.FixDoubleGreaterThanTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixEllipsesStartTicked", settings.CommonErrors.FixEllipsesStartTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixMissingOpenBracketTicked", settings.CommonErrors.FixMissingOpenBracketTicked.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixMusicNotationTicked", settings.CommonErrors.FixMusicNotationTicked.ToString(CultureInfo.InvariantCulture));
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
                textWriter.WriteElementString("WaveformBorderHitMs", settings.VideoControls.WaveformBorderHitMs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformGridColor", settings.VideoControls.WaveformGridColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformColor", settings.VideoControls.WaveformColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformSelectedColor", settings.VideoControls.WaveformSelectedColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformBackgroundColor", settings.VideoControls.WaveformBackgroundColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformTextColor", settings.VideoControls.WaveformTextColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformTextSize", settings.VideoControls.WaveformTextSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformTextBold", settings.VideoControls.WaveformTextBold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformDoubleClickOnNonParagraphAction", settings.VideoControls.WaveformDoubleClickOnNonParagraphAction);
                textWriter.WriteElementString("WaveformRightClickOnNonParagraphAction", settings.VideoControls.WaveformRightClickOnNonParagraphAction);
                textWriter.WriteElementString("WaveformMouseWheelScrollUpIsForward", settings.VideoControls.WaveformMouseWheelScrollUpIsForward.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GenerateSpectrogram", settings.VideoControls.GenerateSpectrogram.ToString(CultureInfo.InvariantCulture));
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
                textWriter.WriteElementString("WebServiceUrl", settings.NetworkSettings.WebServiceUrl);
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
                textWriter.WriteElementString("PromptForUnknownWords", settings.VobSubOcr.PromptForUnknownWords.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("GuessUnknownWords", settings.VobSubOcr.GuessUnknownWords.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoBreakSubtitleIfMoreThanTwoLines", settings.VobSubOcr.AutoBreakSubtitleIfMoreThanTwoLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ItalicFactor", settings.VobSubOcr.ItalicFactor.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrDraw", settings.VobSubOcr.LineOcrDraw.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrAdvancedItalic", settings.VobSubOcr.LineOcrAdvancedItalic.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrLastLanguages", settings.VobSubOcr.LineOcrLastLanguages);
                textWriter.WriteElementString("LineOcrLastSpellCheck", settings.VobSubOcr.LineOcrLastSpellCheck);
                textWriter.WriteElementString("LineOcrXOrMorePixelsMakesSpace", settings.VobSubOcr.LineOcrXOrMorePixelsMakesSpace.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrMinLineHeight", settings.VobSubOcr.LineOcrMinLineHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrMaxLineHeight", settings.VobSubOcr.LineOcrMaxLineHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LastBinaryImageCompareDb", settings.VobSubOcr.LastBinaryImageCompareDb);
                textWriter.WriteElementString("LastBinaryImageSpellCheck", settings.VobSubOcr.LastBinaryImageSpellCheck);
                textWriter.WriteElementString("BinaryAutoDetectBestDb", settings.VobSubOcr.BinaryAutoDetectBestDb.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LastTesseractSpellCheck", settings.VobSubOcr.LastTesseractSpellCheck);
                textWriter.WriteElementString("CaptureTopAlign", settings.VobSubOcr.CaptureTopAlign.ToString(CultureInfo.InvariantCulture));

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

                textWriter.WriteStartElement("Shortcuts", string.Empty);
                textWriter.WriteElementString("GeneralGoToFirstSelectedLine", settings.Shortcuts.GeneralGoToFirstSelectedLine);
                textWriter.WriteElementString("GeneralGoToNextEmptyLine", settings.Shortcuts.GeneralGoToNextEmptyLine);
                textWriter.WriteElementString("GeneralMergeSelectedLines", settings.Shortcuts.GeneralMergeSelectedLines);
                textWriter.WriteElementString("GeneralMergeSelectedLinesAndAutoBreak", settings.Shortcuts.GeneralMergeSelectedLinesAndAutoBreak);
                textWriter.WriteElementString("GeneralMergeSelectedLinesAndUnbreak", settings.Shortcuts.GeneralMergeSelectedLinesAndUnbreak);
                textWriter.WriteElementString("GeneralMergeSelectedLinesAndUnbreakCjk", settings.Shortcuts.GeneralMergeSelectedLinesAndUnbreakCjk);
                textWriter.WriteElementString("GeneralMergeSelectedLinesOnlyFirstText", settings.Shortcuts.GeneralMergeSelectedLinesOnlyFirstText);
                textWriter.WriteElementString("GeneralMergeSelectedLinesBilingual", settings.Shortcuts.GeneralMergeSelectedLinesBilingual);
                textWriter.WriteElementString("GeneralMergeWithNext", settings.Shortcuts.GeneralMergeWithNext);
                textWriter.WriteElementString("GeneralMergeWithPrevious", settings.Shortcuts.GeneralMergeWithPrevious);
                textWriter.WriteElementString("GeneralToggleTranslationMode", settings.Shortcuts.GeneralToggleTranslationMode);
                textWriter.WriteElementString("GeneralSwitchOriginalAndTranslation", settings.Shortcuts.GeneralSwitchOriginalAndTranslation);
                textWriter.WriteElementString("GeneralMergeOriginalAndTranslation", settings.Shortcuts.GeneralMergeOriginalAndTranslation);
                textWriter.WriteElementString("GeneralGoToNextSubtitle", settings.Shortcuts.GeneralGoToNextSubtitle);
                textWriter.WriteElementString("GeneralGoToPrevSubtitle", settings.Shortcuts.GeneralGoToPrevSubtitle);
                textWriter.WriteElementString("GeneralGoToEndOfCurrentSubtitle", settings.Shortcuts.GeneralGoToEndOfCurrentSubtitle);
                textWriter.WriteElementString("GeneralGoToStartOfCurrentSubtitle", settings.Shortcuts.GeneralGoToStartOfCurrentSubtitle);
                textWriter.WriteElementString("GeneralGoToPreviousSubtitleAndFocusVideo", settings.Shortcuts.GeneralGoToPreviousSubtitleAndFocusVideo);
                textWriter.WriteElementString("GeneralGoToNextSubtitleAndFocusVideo", settings.Shortcuts.GeneralGoToNextSubtitleAndFocusVideo);
                textWriter.WriteElementString("GeneralExtendCurrentSubtitle", settings.Shortcuts.GeneralExtendCurrentSubtitle);
                textWriter.WriteElementString("GeneralAutoCalcCurrentDuration", settings.Shortcuts.GeneralAutoCalcCurrentDuration);
                textWriter.WriteElementString("GeneralPlayFirstSelected", settings.Shortcuts.GeneralPlayFirstSelected);
                textWriter.WriteElementString("GeneralHelp", settings.Shortcuts.GeneralHelp);
                textWriter.WriteElementString("GeneralUnbrekNoSpace", settings.Shortcuts.GeneralUnbrekNoSpace);
                textWriter.WriteElementString("GeneralToggleBookmarks", settings.Shortcuts.GeneralToggleBookmarks);
                textWriter.WriteElementString("GeneralToggleBookmarksWithText", settings.Shortcuts.GeneralToggleBookmarksWithText);
                textWriter.WriteElementString("GeneralClearBookmarks", settings.Shortcuts.GeneralClearBookmarks);
                textWriter.WriteElementString("GeneralGoToBookmark", settings.Shortcuts.GeneralGoToBookmark);
                textWriter.WriteElementString("GeneralGoToNextBookmark", settings.Shortcuts.GeneralGoToNextBookmark);
                textWriter.WriteElementString("ChooseProfile", settings.Shortcuts.ChooseProfile);
                textWriter.WriteElementString("DuplicateLine", settings.Shortcuts.DuplicateLine);
                textWriter.WriteElementString("GeneralGoToPreviousBookmark", settings.Shortcuts.GeneralGoToPreviousBookmark);
                textWriter.WriteElementString("MainFileNew", settings.Shortcuts.MainFileNew);
                textWriter.WriteElementString("MainFileOpen", settings.Shortcuts.MainFileOpen);
                textWriter.WriteElementString("MainFileOpenKeepVideo", settings.Shortcuts.MainFileOpenKeepVideo);
                textWriter.WriteElementString("MainFileSave", settings.Shortcuts.MainFileSave);
                textWriter.WriteElementString("MainFileSaveOriginal", settings.Shortcuts.MainFileSaveOriginal);
                textWriter.WriteElementString("MainFileSaveOriginalAs", settings.Shortcuts.MainFileSaveOriginalAs);
                textWriter.WriteElementString("MainFileSaveAs", settings.Shortcuts.MainFileSaveAs);
                textWriter.WriteElementString("MainFileCloseOriginal", settings.Shortcuts.MainFileCloseOriginal);
                textWriter.WriteElementString("MainFileOpenOriginal", settings.Shortcuts.MainFileOpenOriginal);
                textWriter.WriteElementString("MainFileSaveAll", settings.Shortcuts.MainFileSaveAll);
                textWriter.WriteElementString("MainFileImportPlainText", settings.Shortcuts.MainFileImportPlainText);
                textWriter.WriteElementString("MainFileImportTimeCodes", settings.Shortcuts.MainFileImportTimeCodes);
                textWriter.WriteElementString("MainFileExportEbu", settings.Shortcuts.MainFileExportEbu);
                textWriter.WriteElementString("MainFileExportPlainText", settings.Shortcuts.MainFileExportPlainText);
                textWriter.WriteElementString("MainFileExportEbu", settings.Shortcuts.MainFileExportEbu);
                textWriter.WriteElementString("MainEditUndo", settings.Shortcuts.MainEditUndo);
                textWriter.WriteElementString("MainEditRedo", settings.Shortcuts.MainEditRedo);
                textWriter.WriteElementString("MainEditFind", settings.Shortcuts.MainEditFind);
                textWriter.WriteElementString("MainEditFindNext", settings.Shortcuts.MainEditFindNext);
                textWriter.WriteElementString("MainEditReplace", settings.Shortcuts.MainEditReplace);
                textWriter.WriteElementString("MainEditMultipleReplace", settings.Shortcuts.MainEditMultipleReplace);
                textWriter.WriteElementString("MainEditGoToLineNumber", settings.Shortcuts.MainEditGoToLineNumber);
                textWriter.WriteElementString("MainEditRightToLeft", settings.Shortcuts.MainEditRightToLeft);
                textWriter.WriteElementString("MainToolsFixCommonErrors", settings.Shortcuts.MainToolsFixCommonErrors);
                textWriter.WriteElementString("MainToolsFixCommonErrorsPreview", settings.Shortcuts.MainToolsFixCommonErrorsPreview);
                textWriter.WriteElementString("MainToolsMergeShortLines", settings.Shortcuts.MainToolsMergeShortLines);
                textWriter.WriteElementString("MainToolsMakeEmptyFromCurrent", settings.Shortcuts.MainToolsMakeEmptyFromCurrent);
                textWriter.WriteElementString("MainToolsSplitLongLines", settings.Shortcuts.MainToolsSplitLongLines);
                textWriter.WriteElementString("MainToolsMinimumDisplayTimeBetweenParagraphs", settings.Shortcuts.MainToolsMinimumDisplayTimeBetweenParagraphs);
                textWriter.WriteElementString("MainToolsDurationsBridgeGap", settings.Shortcuts.MainToolsDurationsBridgeGap);
                textWriter.WriteElementString("MainToolsRenumber", settings.Shortcuts.MainToolsRenumber);
                textWriter.WriteElementString("MainToolsRemoveTextForHI", settings.Shortcuts.MainToolsRemoveTextForHI);
                textWriter.WriteElementString("MainToolsChangeCasing", settings.Shortcuts.MainToolsChangeCasing);
                textWriter.WriteElementString("MainToolsAutoDuration", settings.Shortcuts.MainToolsAutoDuration);
                textWriter.WriteElementString("MainToolsBatchConvert", settings.Shortcuts.MainToolsBatchConvert);
                textWriter.WriteElementString("MainToolsBeamer", settings.Shortcuts.MainToolsBeamer);
                textWriter.WriteElementString("MainToolsToggleTranslationOriginalInPreviews", settings.Shortcuts.MainEditToggleTranslationOriginalInPreviews);
                textWriter.WriteElementString("MainEditInverseSelection", settings.Shortcuts.MainEditInverseSelection);
                textWriter.WriteElementString("MainEditModifySelection", settings.Shortcuts.MainEditModifySelection);
                textWriter.WriteElementString("MainVideoOpen", settings.Shortcuts.MainVideoOpen);
                textWriter.WriteElementString("MainVideoClose", settings.Shortcuts.MainVideoClose);
                textWriter.WriteElementString("MainVideoPause", settings.Shortcuts.MainVideoPause);
                textWriter.WriteElementString("MainVideoPlayFromJustBefore", settings.Shortcuts.MainVideoPlayFromJustBefore);
                textWriter.WriteElementString("MainVideoPlayPauseToggle", settings.Shortcuts.MainVideoPlayPauseToggle);
                textWriter.WriteElementString("MainVideoShowHideVideo", settings.Shortcuts.MainVideoShowHideVideo);
                textWriter.WriteElementString("MainVideoToggleVideoControls", settings.Shortcuts.MainVideoToggleVideoControls);
                textWriter.WriteElementString("MainVideo1FrameLeft", settings.Shortcuts.MainVideo1FrameLeft);
                textWriter.WriteElementString("MainVideo1FrameRight", settings.Shortcuts.MainVideo1FrameRight);
                textWriter.WriteElementString("MainVideo1FrameLeftWithPlay", settings.Shortcuts.MainVideo1FrameLeftWithPlay);
                textWriter.WriteElementString("MainVideo1FrameRightWithPlay", settings.Shortcuts.MainVideo1FrameRightWithPlay);
                textWriter.WriteElementString("MainVideo100MsLeft", settings.Shortcuts.MainVideo100MsLeft);
                textWriter.WriteElementString("MainVideo100MsRight", settings.Shortcuts.MainVideo100MsRight);
                textWriter.WriteElementString("MainVideo500MsLeft", settings.Shortcuts.MainVideo500MsLeft);
                textWriter.WriteElementString("MainVideo500MsRight", settings.Shortcuts.MainVideo500MsRight);
                textWriter.WriteElementString("MainVideo1000MsLeft", settings.Shortcuts.MainVideo1000MsLeft);
                textWriter.WriteElementString("MainVideo1000MsRight", settings.Shortcuts.MainVideo1000MsRight);
                textWriter.WriteElementString("MainVideo5000MsLeft", settings.Shortcuts.MainVideo5000MsLeft);
                textWriter.WriteElementString("MainVideo5000MsRight", settings.Shortcuts.MainVideo5000MsRight);
                textWriter.WriteElementString("MainVideoGoToPrevSubtitle", settings.Shortcuts.MainVideoGoToPrevSubtitle);
                textWriter.WriteElementString("MainVideoGoToNextSubtitle", settings.Shortcuts.MainVideoGoToNextSubtitle);
                textWriter.WriteElementString("MainVideoFullscreen", settings.Shortcuts.MainVideoFullscreen);
                textWriter.WriteElementString("MainVideoSlower", settings.Shortcuts.MainVideoSlower);
                textWriter.WriteElementString("MainVideoFaster", settings.Shortcuts.MainVideoFaster);
                textWriter.WriteElementString("MainVideoReset", settings.Shortcuts.MainVideoReset);
                textWriter.WriteElementString("MainSpellCheck", settings.Shortcuts.MainSpellCheck);
                textWriter.WriteElementString("MainSpellCheckFindDoubleWords", settings.Shortcuts.MainSpellCheckFindDoubleWords);
                textWriter.WriteElementString("MainSpellCheckAddWordToNames", settings.Shortcuts.MainSpellCheckAddWordToNames);
                textWriter.WriteElementString("MainSynchronizationAdjustTimes", settings.Shortcuts.MainSynchronizationAdjustTimes);
                textWriter.WriteElementString("MainSynchronizationVisualSync", settings.Shortcuts.MainSynchronizationVisualSync);
                textWriter.WriteElementString("MainSynchronizationPointSync", settings.Shortcuts.MainSynchronizationPointSync);
                textWriter.WriteElementString("MainSynchronizationPointSyncViaFile", settings.Shortcuts.MainSynchronizationPointSyncViaFile);
                textWriter.WriteElementString("MainSynchronizationChangeFrameRate", settings.Shortcuts.MainSynchronizationChangeFrameRate);
                textWriter.WriteElementString("MainListViewItalic", settings.Shortcuts.MainListViewItalic);
                textWriter.WriteElementString("MainListViewBold", settings.Shortcuts.MainListViewBold);
                textWriter.WriteElementString("MainListViewUnderline", settings.Shortcuts.MainListViewUnderline);
                textWriter.WriteElementString("MainListViewToggleDashes", settings.Shortcuts.MainListViewToggleDashes);
                textWriter.WriteElementString("MainListViewToggleMusicSymbols", settings.Shortcuts.MainListViewToggleMusicSymbols);
                textWriter.WriteElementString("MainListViewAlignment", settings.Shortcuts.MainListViewAlignment);
                textWriter.WriteElementString("MainListViewAlignmentN1", settings.Shortcuts.MainListViewAlignmentN1);
                textWriter.WriteElementString("MainListViewAlignmentN2", settings.Shortcuts.MainListViewAlignmentN2);
                textWriter.WriteElementString("MainListViewAlignmentN3", settings.Shortcuts.MainListViewAlignmentN3);
                textWriter.WriteElementString("MainListViewAlignmentN4", settings.Shortcuts.MainListViewAlignmentN4);
                textWriter.WriteElementString("MainListViewAlignmentN5", settings.Shortcuts.MainListViewAlignmentN5);
                textWriter.WriteElementString("MainListViewAlignmentN6", settings.Shortcuts.MainListViewAlignmentN6);
                textWriter.WriteElementString("MainListViewAlignmentN7", settings.Shortcuts.MainListViewAlignmentN7);
                textWriter.WriteElementString("MainListViewAlignmentN8", settings.Shortcuts.MainListViewAlignmentN8);
                textWriter.WriteElementString("MainListViewAlignmentN9", settings.Shortcuts.MainListViewAlignmentN9);
                textWriter.WriteElementString("MainRemoveFormatting", settings.Shortcuts.MainRemoveFormatting);
                textWriter.WriteElementString("MainListViewCopyText", settings.Shortcuts.MainListViewCopyText);
                textWriter.WriteElementString("MainListViewCopyTextFromOriginalToCurrent", settings.Shortcuts.MainListViewCopyTextFromOriginalToCurrent);
                textWriter.WriteElementString("MainListViewAutoDuration", settings.Shortcuts.MainListViewAutoDuration);
                textWriter.WriteElementString("MainListViewColumnDeleteText", settings.Shortcuts.MainListViewColumnDeleteText);
                textWriter.WriteElementString("MainListViewColumnDeleteTextAndShiftUp", settings.Shortcuts.MainListViewColumnDeleteTextAndShiftUp);
                textWriter.WriteElementString("MainListViewColumnInsertText", settings.Shortcuts.MainListViewColumnInsertText);
                textWriter.WriteElementString("MainListViewColumnPaste", settings.Shortcuts.MainListViewColumnPaste);
                textWriter.WriteElementString("MainListViewColumnTextUp", settings.Shortcuts.MainListViewColumnTextUp);
                textWriter.WriteElementString("MainListViewColumnTextDown", settings.Shortcuts.MainListViewColumnTextDown);
                textWriter.WriteElementString("MainListViewFocusWaveform", settings.Shortcuts.MainListViewFocusWaveform);
                textWriter.WriteElementString("MainListViewGoToNextError", settings.Shortcuts.MainListViewGoToNextError);
                textWriter.WriteElementString("MainEditReverseStartAndEndingForRTL", settings.Shortcuts.MainEditReverseStartAndEndingForRTL);
                textWriter.WriteElementString("MainTextBoxItalic", settings.Shortcuts.MainTextBoxItalic);
                textWriter.WriteElementString("MainTextBoxSplitAtCursor", settings.Shortcuts.MainTextBoxSplitAtCursor);
                textWriter.WriteElementString("MainTextBoxSplitAtCursorAndVideoPos", settings.Shortcuts.MainTextBoxSplitAtCursorAndVideoPos);
                textWriter.WriteElementString("MainTextBoxSplitSelectedLineBilingual", settings.Shortcuts.MainTextBoxSplitSelectedLineBilingual);
                textWriter.WriteElementString("MainTextBoxMoveLastWordDown", settings.Shortcuts.MainTextBoxMoveLastWordDown);
                textWriter.WriteElementString("MainTextBoxMoveFirstWordFromNextUp", settings.Shortcuts.MainTextBoxMoveFirstWordFromNextUp);
                textWriter.WriteElementString("MainTextBoxMoveLastWordDownCurrent", settings.Shortcuts.MainTextBoxMoveLastWordDownCurrent);
                textWriter.WriteElementString("MainTextBoxMoveFirstWordUpCurrent", settings.Shortcuts.MainTextBoxMoveFirstWordUpCurrent);
                textWriter.WriteElementString("MainTextBoxSelectionToLower", settings.Shortcuts.MainTextBoxSelectionToLower);
                textWriter.WriteElementString("MainTextBoxSelectionToUpper", settings.Shortcuts.MainTextBoxSelectionToUpper);
                textWriter.WriteElementString("MainTextBoxSelectionToRuby", settings.Shortcuts.MainTextBoxSelectionToRuby);
                textWriter.WriteElementString("MainTextBoxToggleAutoDuration", settings.Shortcuts.MainTextBoxToggleAutoDuration);
                textWriter.WriteElementString("MainCreateInsertSubAtVideoPos", settings.Shortcuts.MainCreateInsertSubAtVideoPos);
                textWriter.WriteElementString("MainCreateSetStart", settings.Shortcuts.MainCreateSetStart);
                textWriter.WriteElementString("MainCreateSetEnd", settings.Shortcuts.MainCreateSetEnd);
                textWriter.WriteElementString("MainCreateSetEndAddNewAndGoToNew", settings.Shortcuts.MainCreateSetEndAddNewAndGoToNew);
                textWriter.WriteElementString("MainCreateStartDownEndUp", settings.Shortcuts.MainCreateStartDownEndUp);
                textWriter.WriteElementString("MainAdjustSetStartAndOffsetTheRest", settings.Shortcuts.MainAdjustSetStartAndOffsetTheRest);
                textWriter.WriteElementString("MainAdjustSetEndAndOffsetTheRest", settings.Shortcuts.MainAdjustSetEndAndOffsetTheRest);
                textWriter.WriteElementString("MainAdjustSetEndAndOffsetTheRestAndGoToNext", settings.Shortcuts.MainAdjustSetEndAndOffsetTheRestAndGoToNext);
                textWriter.WriteElementString("MainAdjustSetEndAndGotoNext", settings.Shortcuts.MainAdjustSetEndAndGotoNext);
                textWriter.WriteElementString("MainAdjustViaEndAutoStart", settings.Shortcuts.MainAdjustViaEndAutoStart);
                textWriter.WriteElementString("MainAdjustViaEndAutoStartAndGoToNext", settings.Shortcuts.MainAdjustViaEndAutoStartAndGoToNext);
                textWriter.WriteElementString("MainAdjustSetStartAutoDurationAndGoToNext", settings.Shortcuts.MainAdjustSetStartAutoDurationAndGoToNext);
                textWriter.WriteElementString("MainAdjustSetEndNextStartAndGoToNext", settings.Shortcuts.MainAdjustSetEndNextStartAndGoToNext);
                textWriter.WriteElementString("MainAdjustStartDownEndUpAndGoToNext", settings.Shortcuts.MainAdjustStartDownEndUpAndGoToNext);
                textWriter.WriteElementString("MainAdjustSetStartKeepDuration", settings.Shortcuts.MainAdjustSetStartKeepDuration);
                textWriter.WriteElementString("MainAdjustSelected100MsForward", settings.Shortcuts.MainAdjustSelected100MsForward);
                textWriter.WriteElementString("MainAdjustSelected100MsBack", settings.Shortcuts.MainAdjustSelected100MsBack);
                textWriter.WriteElementString("MainAdjustStartXMsBack", settings.Shortcuts.MainAdjustStartXMsBack);
                textWriter.WriteElementString("MainAdjustStartXMsForward", settings.Shortcuts.MainAdjustStartXMsForward);
                textWriter.WriteElementString("MainAdjustEndXMsBack", settings.Shortcuts.MainAdjustEndXMsBack);
                textWriter.WriteElementString("MainAdjustEndXMsForward", settings.Shortcuts.MainAdjustEndXMsForward);
                textWriter.WriteElementString("MainInsertAfter", settings.Shortcuts.MainInsertAfter);
                textWriter.WriteElementString("MainTextBoxAutoBreak", settings.Shortcuts.MainTextBoxAutoBreak);
                textWriter.WriteElementString("MainTextBoxBreakAtPosition", settings.Shortcuts.MainTextBoxBreakAtPosition);
                textWriter.WriteElementString("MainTextBoxBreakAtPositionAndGoToNext", settings.Shortcuts.MainTextBoxBreakAtPositionAndGoToNext);
                textWriter.WriteElementString("MainTextBoxUnbreak", settings.Shortcuts.MainTextBoxUnbreak);
                textWriter.WriteElementString("MainWaveformInsertAtCurrentPosition", settings.Shortcuts.MainWaveformInsertAtCurrentPosition);
                textWriter.WriteElementString("MainInsertBefore", settings.Shortcuts.MainInsertBefore);
                textWriter.WriteElementString("MainMergeDialog", settings.Shortcuts.MainMergeDialog);
                textWriter.WriteElementString("MainToggleFocus", settings.Shortcuts.MainToggleFocus);
                textWriter.WriteElementString("WaveformVerticalZoom", settings.Shortcuts.WaveformVerticalZoom);
                textWriter.WriteElementString("WaveformVerticalZoomOut", settings.Shortcuts.WaveformVerticalZoomOut);
                textWriter.WriteElementString("WaveformZoomIn", settings.Shortcuts.WaveformZoomIn);
                textWriter.WriteElementString("WaveformZoomOut", settings.Shortcuts.WaveformZoomOut);
                textWriter.WriteElementString("WaveformSplit", settings.Shortcuts.WaveformSplit);
                textWriter.WriteElementString("WaveformPlaySelection", settings.Shortcuts.WaveformPlaySelection);
                textWriter.WriteElementString("WaveformPlaySelectionEnd", settings.Shortcuts.WaveformPlaySelectionEnd);
                textWriter.WriteElementString("WaveformSearchSilenceForward", settings.Shortcuts.WaveformSearchSilenceForward);
                textWriter.WriteElementString("WaveformSearchSilenceBack", settings.Shortcuts.WaveformSearchSilenceBack);
                textWriter.WriteElementString("WaveformAddTextHere", settings.Shortcuts.WaveformAddTextHere);
                textWriter.WriteElementString("WaveformAddTextHereFromClipboard", settings.Shortcuts.WaveformAddTextHereFromClipboard);
                textWriter.WriteElementString("WaveformSetParagraphAsSelection", settings.Shortcuts.WaveformSetParagraphAsSelection);
                textWriter.WriteElementString("WaveformFocusListView", settings.Shortcuts.WaveformFocusListView);
                textWriter.WriteElementString("WaveformGoToPreviousSceneChange", settings.Shortcuts.WaveformGoToPreviousSceneChange);
                textWriter.WriteElementString("WaveformGoToNextSceneChange", settings.Shortcuts.WaveformGoToNextSceneChange);
                textWriter.WriteElementString("WaveformToggleSceneChange", settings.Shortcuts.WaveformToggleSceneChange);
                textWriter.WriteElementString("MainTranslateGoogleIt", settings.Shortcuts.MainTranslateGoogleIt);
                textWriter.WriteElementString("MainTranslateGoogleTranslate", settings.Shortcuts.MainTranslateGoogleTranslate);
                textWriter.WriteElementString("MainTranslateCustomSearch1", settings.Shortcuts.MainTranslateCustomSearch1);
                textWriter.WriteElementString("MainTranslateCustomSearch2", settings.Shortcuts.MainTranslateCustomSearch2);
                textWriter.WriteElementString("MainTranslateCustomSearch3", settings.Shortcuts.MainTranslateCustomSearch3);
                textWriter.WriteElementString("MainTranslateCustomSearch4", settings.Shortcuts.MainTranslateCustomSearch4);
                textWriter.WriteElementString("MainTranslateCustomSearch5", settings.Shortcuts.MainTranslateCustomSearch5);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("RemoveTextForHearingImpaired", string.Empty);
                textWriter.WriteElementString("RemoveTextBetweenBrackets", settings.RemoveTextForHearingImpaired.RemoveTextBetweenBrackets.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBetweenParentheses", settings.RemoveTextForHearingImpaired.RemoveTextBetweenParentheses.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBetweenCurlyBrackets", settings.RemoveTextForHearingImpaired.RemoveTextBetweenCurlyBrackets.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBetweenQuestionMarks", settings.RemoveTextForHearingImpaired.RemoveTextBetweenQuestionMarks.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBetweenCustom", settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustom.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBetweenCustomBefore", settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomBefore);
                textWriter.WriteElementString("RemoveTextBetweenCustomAfter", settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomAfter);
                textWriter.WriteElementString("RemoveTextBetweenOnlySeperateLines", settings.RemoveTextForHearingImpaired.RemoveTextBetweenOnlySeperateLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBeforeColon", settings.RemoveTextForHearingImpaired.RemoveTextBeforeColon.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBeforeColonOnlyIfUppercase", settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyIfUppercase.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveTextBeforeColonOnlyOnSeparateLine", settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyOnSeparateLine.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveInterjections", settings.RemoveTextForHearingImpaired.RemoveInterjections.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveInterjectionsOnlyOnSeparateLine", settings.RemoveTextForHearingImpaired.RemoveInterjectionsOnlyOnSeparateLine.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveIfAllUppercase", settings.RemoveTextForHearingImpaired.RemoveIfAllUppercase.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveIfContains", settings.RemoveTextForHearingImpaired.RemoveIfContains.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("RemoveIfContainsText", settings.RemoveTextForHearingImpaired.RemoveIfContainsText);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("SubtitleBeaming", string.Empty);
                textWriter.WriteElementString("FontName", settings.SubtitleBeaming.FontName);
                textWriter.WriteElementString("FontColor", settings.SubtitleBeaming.FontColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FontSize", settings.SubtitleBeaming.FontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BorderColor", settings.SubtitleBeaming.BorderColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BorderWidth", settings.SubtitleBeaming.BorderWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteEndElement();

                textWriter.WriteEndElement();

                textWriter.WriteEndDocument();
                textWriter.Flush();

                try
                {
                    File.WriteAllText(fileName, sb.ToString().Replace("encoding=\"utf-16\"", "encoding=\"utf-8\""), Encoding.UTF8);
                }
                catch
                {
                    // ignored
                }
            }
        }

    }
}
