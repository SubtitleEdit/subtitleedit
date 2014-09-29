using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Nikse.SubtitleEdit.Logic
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

        public void Add(string fileName, int firstVisibleIndex, int firstSelectedIndex, string videoFileName, string originalFileName)
        {
            var newList = new List<RecentFileEntry> { new RecentFileEntry { FileName = fileName, FirstVisibleIndex = firstVisibleIndex, FirstSelectedIndex = firstSelectedIndex, VideoFileName = videoFileName, OriginalFileName = originalFileName } };
            int index = 0;
            foreach (var oldRecentFile in Files)
            {
                if (!fileName.Equals(oldRecentFile.FileName, StringComparison.OrdinalIgnoreCase) && index < MaxRecentFiles)
                    newList.Add(new RecentFileEntry { FileName = oldRecentFile.FileName, FirstVisibleIndex = oldRecentFile.FirstVisibleIndex, FirstSelectedIndex = oldRecentFile.FirstSelectedIndex, VideoFileName = oldRecentFile.VideoFileName, OriginalFileName = oldRecentFile.OriginalFileName });
                index++;
            }
            Files = newList;
        }

        public void Add(string fileName, string videoFileName, string originalFileName)
        {
            var newList = new List<RecentFileEntry>();
            foreach (var oldRecentFile in Files)
            {
                if (fileName.Equals(oldRecentFile.FileName, StringComparison.OrdinalIgnoreCase))
                    newList.Add(new RecentFileEntry { FileName = oldRecentFile.FileName, FirstVisibleIndex = oldRecentFile.FirstVisibleIndex, FirstSelectedIndex = oldRecentFile.FirstSelectedIndex, VideoFileName = oldRecentFile.VideoFileName, OriginalFileName = oldRecentFile.OriginalFileName });
            }
            if (newList.Count == 0)
                newList.Add(new RecentFileEntry { FileName = fileName, FirstVisibleIndex = -1, FirstSelectedIndex = -1, VideoFileName = videoFileName, OriginalFileName = originalFileName });

            int index = 0;
            foreach (var oldRecentFile in Files)
            {
                if (!fileName.Equals(oldRecentFile.FileName, StringComparison.OrdinalIgnoreCase) && index < MaxRecentFiles)
                    newList.Add(new RecentFileEntry { FileName = oldRecentFile.FileName, FirstVisibleIndex = oldRecentFile.FirstVisibleIndex, FirstSelectedIndex = oldRecentFile.FirstSelectedIndex, VideoFileName = oldRecentFile.VideoFileName, OriginalFileName = oldRecentFile.OriginalFileName });
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
        public int MergeLinesShorterThan { get; set; }
        public bool FixShortDisplayTimesAllowMoveStartTime { get; set; }
        public string MusicSymbol { get; set; }
        public string MusicSymbolToReplace { get; set; }
        public string UnicodeSymbolsToInsert { get; set; }
        public bool SpellCheckAutoChangeNames { get; set; }
        public bool SpellCheckOneLetterWords { get; set; }
        public bool SpellCheckEnglishAllowInQuoteAsIng { get; set; }
        public bool SpellCheckShowCompletedMessage { get; set; }
        public bool OcrFixUseHardcodedRules { get; set; }
        public string Interjections { get; set; }
        public string MicrosoftBingApiId { get; set; }
        public string GoogleApiKey { get; set; }
        public bool UseGooleApiPaidService { get; set; }
        public string GoogleTranslateLastTargetLanguage { get; set; }
        public bool ListViewSyntaxColorDurationSmall { get; set; }
        public bool ListViewSyntaxColorDurationBig { get; set; }
        public bool ListViewSyntaxColorOverlap { get; set; }
        public bool ListViewSyntaxColorLongLines { get; set; }
        public bool ListViewSyntaxMoreThanXLines { get; set; }
        public int ListViewSyntaxMoreThanXLinesX { get; set; }
        public Color ListViewSyntaxErrorColor { get; set; }
        public Color ListViewUnfocusedSelectedColor { get; set; }
        public bool SplitAdvanced { get; set; }
        public string SplitOutputFolder { get; set; }
        public int SplitNumberOfParts { get; set; }
        public string SplitVia { get; set; }
        public string LastShowEarlierOrLaterSelection { get; set; }
        public string NewEmptyTranslationText { get; set; }
        public string BatchConvertOutputFolder { get; set; }
        public bool BatchConvertOverwriteExisting { get; set; }
        public bool BatchConvertOverwriteOriginal { get; set; }
        public bool BatchConvertRemoveFormatting { get; set; }
        public bool BatchConvertFixCasing { get; set; }
        public bool BatchConvertRemoveTextForHI { get; set; }
        public bool BatchConvertFixCommonErrors { get; set; }
        public bool BatchConvertMultipleReplace { get; set; }
        public bool BatchConvertSplitLongLines { get; set; }
        public bool BatchConvertAutoBalance { get; set; }
        public bool BatchConvertSetMinDisplayTimeBetweenSubtitles { get; set; }
        public string BatchConvertLanguage { get; set; }
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
        public int ExportLastFontSize { get; set; }
        public int ExportLastLineHeight { get; set; }
        public int ExportLastBorderWidth { get; set; }
        public bool ExportLastFontBold { get; set; }
        public string ExportBluRayVideoResolution { get; set; }
        public Color ExportFontColor { get; set; }
        public Color ExportBorderColor { get; set; }
        public Color ExportShadowColor { get; set; }
        public int ExportBottomMargin { get; set; }
        public int ExportHorizontalAlignment { get; set; }
        public int ExportBluRayBottomMargin { get; set; }
        public int ExportBluRayShadow { get; set; }
        public int Export3DType { get; set; }
        public int Export3DDepth { get; set; }
        public int ExportLastShadowTransparency { get; set; }
        public double ExportLastFrameRate { get; set; }
        public bool FixCommonErrorsFixOverlapAllowEqualEndStart { get; set; }
        public string ImportTextSplitting { get; set; }
        public bool ImportTextMergeShortLines { get; set; }
        public string GenerateTimeCodePatterns { get; set; }
        public string MusicSymbolStyle { get; set; }
        public int BridgeGapMilliseconds { get; set; }
        public string ExportCustomTemplates { get; set; }
        public string ChangeCasingChoice { get; set; }
        public bool UseNoLineBreakAfter { get; set; }
        public string NoLineBreakAfterEnglish { get; set; }

        public ToolsSettings()
        {
            StartSceneIndex = 1;
            EndSceneIndex = 1;
            VerifyPlaySeconds = 2;
            MergeLinesShorterThan = 33;
            FixShortDisplayTimesAllowMoveStartTime = false;
            MusicSymbol = "♪";
            MusicSymbolToReplace = "âª â¶ â™ª âTª ã¢â™âª ?t×3 ?t¤3 #";
            UnicodeSymbolsToInsert = "♪;♫;☺;☹;♥;©;☮;☯;Σ;∞;≡;⇒;π";
            SpellCheckAutoChangeNames = true;
            OcrFixUseHardcodedRules = true;
            Interjections = "Ah;Ahh;Ahhh;Ahhhh;Eh;Ehh;Ehhh;Hm;Hmm;Hmmm;Huh;Mm;Mmm;Mmmm;Phew;Gah;Oh;Ohh;Ohhh;Ow;Oww;Owww;Ugh;Ughh;Uh;Uhh;Uhhh;Whew";
            MicrosoftBingApiId = "C2C2E9A508E6748F0494D68DFD92FAA1FF9B0BA4";
            GoogleApiKey = "ABQIAAAA4j5cWwa3lDH0RkZceh7PjBTDmNAghl5kWSyuukQ0wtoJG8nFBxRPlalq-gAvbeCXMCkmrysqjXV1Gw";
            UseGooleApiPaidService = false;
            GoogleTranslateLastTargetLanguage = "en";
            SpellCheckOneLetterWords = true;
            SpellCheckEnglishAllowInQuoteAsIng = false;
            SpellCheckShowCompletedMessage = true;
            ListViewSyntaxColorDurationSmall = true;
            ListViewSyntaxColorDurationBig = true;
            ListViewSyntaxColorOverlap = true;
            ListViewSyntaxColorLongLines = true;
            ListViewSyntaxMoreThanXLines = true;
            ListViewSyntaxMoreThanXLinesX = 2;
            ListViewSyntaxErrorColor = Color.FromArgb(255, 180, 150);
            ListViewUnfocusedSelectedColor = Color.LightBlue;
            SplitAdvanced = false;
            SplitNumberOfParts = 3;
            SplitVia = "Lines";
            NewEmptyTranslationText = string.Empty;
            BatchConvertLanguage = "en";
            ModifySelectionRule = "Contains";
            ModifySelectionText = string.Empty;
            GenerateTimeCodePatterns = "HH:mm:ss;yyyy-MM-dd;dddd dd MMMM yyyy <br>HH:mm:ss;dddd dd MMMM yyyy <br>hh:mm:ss tt;s";
            MusicSymbolStyle = "Double"; // 'Double' or 'Single'
            ExportFontColor = Color.White;
            ExportBorderColor = Color.Black;
            ExportShadowColor = Color.Black;
            ExportBottomMargin = 15;
            ExportHorizontalAlignment = 1; // 1=center (0=left, 2=right)
            ExportVobSubSimpleRendering = true;
            ExportVobAntiAliasingWithTransparency = true;
            ExportBluRayBottomMargin = 20;
            ExportBluRayShadow = 1;
            Export3DType = 0;
            Export3DDepth = 0;
            ExportLastShadowTransparency = 200;
            ExportLastFrameRate = 24.0d;
            ExportFcpImageType = "Bmp";
            ExportLastBorderWidth = 2;
            BridgeGapMilliseconds = 100;
            ExportCustomTemplates = "SubRipÆÆ{number}\r\n{start} --> {end}\r\n{text}\r\n\r\nÆhh:mm:ss,zzzÆ[Do not modify]ÆæMicroDVDÆÆ{{start}}{{end}}{text}\r\nÆffÆ||Æ";
            UseNoLineBreakAfter = false;
            NoLineBreakAfterEnglish = " Mrs.; Ms.; Mr.; Dr.; a; an; the; my; my own; your; his; our; their; it's; is; are;'s; 're; would;'ll;'ve;'d; will; that; which; who; whom; whose; whichever; whoever; wherever; each; either; every; all; both; few; many; sevaral; all; any; most; been; been doing; none; some; my own; your own; his own; her own; our own; their own; I; she; he; as per; as regards; into; onto; than; where as; abaft; aboard; about; above; across; afore; after; against; along; alongside; amid; amidst; among; amongst; anenst; apropos; apud; around; as; aside; astride; at; athwart; atop; barring; before; behind; below; beneath; beside; besides; between; betwixt; beyond; but; by; circa; ca; concerning; despite; down; during; except; excluding; following; for; forenenst; from; given; in; including; inside; into; lest; like; minus; modulo; near; next; of; off; on; onto; opposite; out; outside; over; pace; past; per; plus; pro; qua; regarding; round; sans; save; since; than; through; thru; throughout; thruout; till; to; toward; towards; under; underneath; unlike; until; unto; up; upon; versus; vs; via; vice; with; within; without; considering; respecting; one; two; another; three; our; five; six; seven; eight; nine; ten; eleven; twelve; thirteen; fourteen; fifteen; sixteen; seventeen; eighteen; nineteen; twenty; thirty; forty; fifty; sixty; seventy; eighty; ninety; hundred; thousand; million; billion; trillion; while; however; what; zero; little; enough; after; although; and; as; if; though; although; because; before; both; but; even; how; than; nor; or; only; unless; until; yet; was; were";
        }

    }

    public class WordListSettings
    {
        public string LastLanguage { get; set; }
        public string NamesEtcUrl { get; set; }
        public bool UseOnlineNamesEtc { get; set; }

        public WordListSettings()
        {
            LastLanguage = "en-US";
            NamesEtcUrl = "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/master/Dictionaries/names_etc.xml";
        }
    }

    public class SubtitleSettings
    {
        public string SsaFontName { get; set; }
        public double SsaFontSize { get; set; }
        public int SsaFontColorArgb { get; set; }
        public int SsaOutline { get; set; }
        public int SsaShadow { get; set; }
        public bool SsaOpaqueBox { get; set; }
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

        public bool CheetahCaptionAlwayWriteEndTime { get; set; }

        public bool SamiDisplayTwoClassesAsTwoSubtitles { get; set; }
        public int SamiHtmlEncodeMode { get; set; }

        public string TimedText10TimeCodeFormat { get; set; }

        public int FcpFontSize { get; set; }
        public string FcpFontName { get; set; }

        public string NuendoCharacterListFile { get; set; }

        public SubtitleSettings()
        {
            SsaFontName = "Arial";
            SsaFontSize = 20;
            SsaFontColorArgb = Color.FromArgb(255, 255, 255).ToArgb();
            SsaOutline = 2;
            SsaShadow = 1;
            SsaOpaqueBox = false;

            DCinemaFontFile = "Arial.ttf";
            DCinemaLoadFontResource = "urn:uuid:3dec6dc0-39d0-498d-97d0-928d2eb78391";
            DCinemaFontSize = 42;
            DCinemaBottomMargin = 8;
            DCinemaZPosition = 0;
            DCinemaFadeUpTime = 5;
            DCinemaFadeDownTime = 5;

            SamiDisplayTwoClassesAsTwoSubtitles = true;
            SamiHtmlEncodeMode = 0;

            TimedText10TimeCodeFormat = "Default";

            FcpFontSize = 18;
            FcpFontName = "Lucida Grande";
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
                string hex = Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower();
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
            EmptyLinesTicked = true;
            OverlappingDisplayTimeTicked = true;
            TooShortDisplayTimeTicked = true;
            TooLongDisplayTimeTicked = true;
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
        public bool ShowToolbarNew { get; set; }
        public bool ShowToolbarOpen { get; set; }
        public bool ShowToolbarSave { get; set; }
        public bool ShowToolbarSaveAs { get; set; }
        public bool ShowToolbarFind { get; set; }
        public bool ShowToolbarReplace { get; set; }
        public bool ShowToolbarFixCommonErrors { get; set; }
        public bool ShowToolbarVisualSync { get; set; }
        public bool ShowToolbarSpellCheck { get; set; }
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
        public string SubtitleFontName { get; set; }
        public int SubtitleFontSize { get; set; }
        public bool SubtitleFontBold { get; set; }
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
        public int SubtitleLineMaximumLength { get; set; }
        public int SubtitleMinimumDisplayMilliseconds { get; set; }
        public int SubtitleMaximumDisplayMilliseconds { get; set; }
        public int MininumMillisecondsBetweenLines { get; set; }
        public int SetStartEndHumanDelay { get; set; }
        public bool AutoWrapLineWhileTyping { get; set; }
        public double SubtitleMaximumCharactersPerSeconds { get; set; }
        public double SubtitleOptimalCharactersPerSeconds { get; set; }
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
        public string UppercaseLetters { get; set; }
        public int DefaultAdjustMilliseconds { get; set; }
        public bool AutoRepeatOn { get; set; }
        public int AutoRepeatCount { get; set; }
        public bool AutoContinueOn { get; set; }
        public bool SyncListViewWithVideoWhilePlaying { get; set; }
        public int AutoBackupSeconds { get; set; }
        public string SpellChecker { get; set; }
        public bool AllowEditOfOriginalSubtitle { get; set; }
        public bool PromptDeleteLines { get; set; }
        public bool Undocked { get; set; }
        public string UndockedVideoPosition { get; set; }
        public string UndockedWaveformPosition { get; set; }
        public string UndockedVideoControlsPosition { get; set; }
        public bool WaveformCenter { get; set; }
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
        public int ListViewTextWidth { get; set; }
        public string VlcWaveTranscodeSettings { get; set; }
        public string VlcLocation { get; set; }
        public string VlcLocationRelative { get; set; }
        public string MpcHcLocation { get; set; }
        public bool UseFFmpegForWaveExtraction { get; set; }
        public string FFmpegLocation { get; set; }
        public bool UseTimeFormatHHMMSSFF { get; set; }
        public int ClearStatusBarAfterSeconds { get; set; }
        public string Company { get; set; }
        public bool MoveVideo100Or500MsPlaySmallSample { get; set; }
        public bool DisableVideoAutoLoading { get; set; }
        public int NewEmptyDefaultMs { get; set; }
        public bool RightToLeftMode { get; set; }
        public string LastSaveAsFormat { get; set; }
        public bool CheckForUpdates { get; set; }
        public DateTime LastCheckForUpdates { get; set; }
        public bool ShowBetaStuff { get; set; }

        public GeneralSettings()
        {
            ShowToolbarNew = true;
            ShowToolbarOpen = true;
            ShowToolbarSave = true;
            ShowToolbarSaveAs = false;
            ShowToolbarFind = true;
            ShowToolbarReplace = true;
            ShowToolbarVisualSync = true;
            ShowToolbarSpellCheck = true;
            ShowToolbarSettings = false;
            ShowToolbarHelp = true;

            ShowVideoPlayer = false;
            ShowAudioVisualizer = false;
            ShowWaveform = true;
            ShowSpectrogram = true;
            ShowFrameRate = false;
            DefaultFrameRate = 23.976;
            CurrentFrameRate = DefaultFrameRate;
            SubtitleFontName = "Tahoma";
            if (Environment.OSVersion.Version.Major < 6) // 6 == Vista/Win2008Server/Win7
                SubtitleFontName = "Times New Roman";

            SubtitleFontSize = 8;
            SubtitleFontBold = false;
            SubtitleFontColor = Color.Black;
            SubtitleBackgroundColor = Color.White;
            CenterSubtitleInTextBox = false;
            DefaultSubtitleFormat = "SubRip";
            DefaultEncoding = Encoding.UTF8.EncodingName;
            AutoConvertToUtf8 = false;
            AutoGuessAnsiEncoding = false;
            ShowRecentFiles = true;
            RememberSelectedLine = true;
            StartLoadLastFile = true;
            StartRememberPositionAndSize = true;
            SubtitleLineMaximumLength = 43;
            SubtitleMinimumDisplayMilliseconds = 1000;
            SubtitleMaximumDisplayMilliseconds = 8 * 1000;
            MininumMillisecondsBetweenLines = 24;
            SetStartEndHumanDelay = 100;
            AutoWrapLineWhileTyping = false;
            SubtitleMaximumCharactersPerSeconds = 25.0;
            SubtitleOptimalCharactersPerSeconds = 15.0;
            SpellCheckLanguage = null;
            VideoPlayer = string.Empty;
            VideoPlayerDefaultVolume = 75;
            VideoPlayerPreviewFontSize = 10;
            VideoPlayerPreviewFontBold = true;
            VideoPlayerShowStopButton = true;
            VideoPlayerShowMuteButton = true;
            VideoPlayerShowFullscreenButton = true;
            ListViewLineSeparatorString = "<br />";
            ListViewDoubleClickAction = 1;
            UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWZYXÆØÃÅÄÖÉÈÁÂÀÇÊÍÓÔÕÚŁАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯĞİŞÜÙÁÌÑÎ";
            DefaultAdjustMilliseconds = 1000;
            AutoRepeatOn = true;
            AutoRepeatCount = 2;
            AutoContinueOn = false;
            SyncListViewWithVideoWhilePlaying = false;
            AutoBackupSeconds = 60 * 15;
            SpellChecker = "hunspell";
            AllowEditOfOriginalSubtitle = true;
            PromptDeleteLines = true;
            Undocked = false;
            UndockedVideoPosition = "-32000;-32000";
            UndockedWaveformPosition = "-32000;-32000";
            UndockedVideoControlsPosition = "-32000;-32000";
            SmallDelayMilliseconds = 500;
            LargeDelayMilliseconds = 5000;
            OpenSubtitleExtraExtensions = "*.mp4;*.m4v;*.mkv;*.ts"; // matroska/mp4/m4v files (can contain subtitles)
            ListViewColumnsRememberSize = true;
            VlcWaveTranscodeSettings = "acodec=s16l"; // "acodec=s16l,channels=1,ab=64,samplerate=8000";
            UseTimeFormatHHMMSSFF = false;
            ClearStatusBarAfterSeconds = 10;
            MoveVideo100Or500MsPlaySmallSample = false;
            DisableVideoAutoLoading = false;
            RightToLeftMode = false;
            LastSaveAsFormat = string.Empty;
            CheckForUpdates = true;
            LastCheckForUpdates = DateTime.Now;
            ShowBetaStuff = false;
            NewEmptyDefaultMs = 2000;
        }

    }

    public class VideoControlsSettings
    {
        public string CustomSearchText1 { get; set; }
        public string CustomSearchText2 { get; set; }
        public string CustomSearchText3 { get; set; }
        public string CustomSearchText4 { get; set; }
        public string CustomSearchText5 { get; set; }
        public string CustomSearchText6 { get; set; }
        public string CustomSearchUrl1 { get; set; }
        public string CustomSearchUrl2 { get; set; }
        public string CustomSearchUrl3 { get; set; }
        public string CustomSearchUrl4 { get; set; }
        public string CustomSearchUrl5 { get; set; }
        public string CustomSearchUrl6 { get; set; }
        public string LastActiveTab { get; set; }
        public bool WaveformDrawGrid { get; set; }
        public bool WaveformAllowOverlap { get; set; }
        public bool WaveformFocusOnMouseEnter { get; set; }
        public bool WaveformListViewFocusOnMouseEnter { get; set; }
        public int WaveformBorderHitMs { get; set; }
        public Color WaveformGridColor { get; set; }
        public Color WaveformColor { get; set; }
        public Color WaveformSelectedColor { get; set; }
        public Color WaveformBackgroundColor { get; set; }
        public Color WaveformTextColor { get; set; }
        public string WaveformDoubleClickOnNonParagraphAction { get; set; }
        public string WaveformRightClickOnNonParagraphAction { get; set; }
        public bool WaveformMouseWheelScrollUpIsForward { get; set; }
        public bool GenerateSpectrogram { get; set; }
        public string SpectrogramAppearance { get; set; }
        public int WaveformMininumSampleRate { get; set; }
        public double WaveformSeeksSilenceDurationSeconds { get; set; }
        public int WaveformSeeksSilenceMaxVolume { get; set; }

        public VideoControlsSettings()
        {
            CustomSearchText1 = "The Free Dictionary";
            CustomSearchUrl1 = "http://www.thefreedictionary.com/{0}";
            CustomSearchText2 = "Wikipedia";
            CustomSearchUrl2 = "http://en.m.wikipedia.org/wiki?search={0}";

            LastActiveTab = "Translate";
            WaveformDrawGrid = true;
            WaveformAllowOverlap = false;
            WaveformBorderHitMs = 15;
            WaveformGridColor = Color.FromArgb(255, 20, 20, 18);
            WaveformColor = Color.GreenYellow;
            WaveformSelectedColor = Color.Red;
            WaveformBackgroundColor = Color.Black;
            WaveformTextColor = Color.Gray;
            WaveformDoubleClickOnNonParagraphAction = "PlayPause";
            WaveformDoubleClickOnNonParagraphAction = string.Empty;
            WaveformMouseWheelScrollUpIsForward = true;
            SpectrogramAppearance = "OneColorGradient";
            WaveformMininumSampleRate = 126;
            WaveformSeeksSilenceDurationSeconds = 0.3;
            WaveformSeeksSilenceMaxVolume = 10;
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
        public bool UseModiInTesseractForUnknownWords { get; set; }
        public bool UseItalicsInTesseract { get; set; }
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

        public VobSubOcrSettings()
        {
            XOrMorePixelsMakesSpace = 8;
            AllowDifferenceInPercent = 1.0;
            BlurayAllowDifferenceInPercent = 7.5;
            LastImageCompareFolder = "English";
            LastModiLanguageId = 9;
            LastOcrMethod = "Tesseract";
            UseItalicsInTesseract = true;
            UseMusicSymbolsInTesseract = true;
            RightToLeft = false;
            TopToBottom = true;
            DefaultMillisecondsForUnknownDurations = 5000;
            PromptForUnknownWords = true;
            GuessUnknownWords = true;
            AutoBreakSubtitleIfMoreThanTwoLines = true;
            ItalicFactor = 0.2;

        }
    }

    public class MultipleSearchAndReplaceSetting
    {
        public bool Enabled { get; set; }
        public string FindWhat { get; set; }
        public string ReplaceWith { get; set; }
        public string SearchType { get; set; }
    }

    public class NetworkSettings
    {
        public string UserName { get; set; }
        public string WebServiceUrl { get; set; }
        public string SessionKey { get; set; }
        public int PollIntervalSeconds { get; set; }

        public NetworkSettings()
        {
            UserName = string.Empty;
            SessionKey = "DemoSession"; // TODO - leave blank or use guid
            WebServiceUrl = "http://www.nikse.dk/se/SeService.asmx";
            PollIntervalSeconds = 5;
        }
    }

    public class Shortcuts
    {
        public string GeneralGoToFirstSelectedLine { get; set; }
        public string GeneralGoToNextEmptyLine { get; set; }
        public string GeneralMergeSelectedLines { get; set; }
        public string GeneralMergeSelectedLinesOnlyFirstText { get; set; }
        public string GeneralToggleTranslationMode { get; set; }
        public string GeneralSwitchOriginalAndTranslation { get; set; }
        public string GeneralMergeOriginalAndTranslation { get; set; }
        public string GeneralGoToNextSubtitle { get; set; }
        public string GeneralGoToPrevSubtitle { get; set; }
        public string GeneralGoToStartOfCurrentSubtitle { get; set; }
        public string GeneralGoToEndOfCurrentSubtitle { get; set; }
        public string GeneralPlayFirstSelected { get; set; }
        public string MainFileNew { get; set; }
        public string MainFileOpen { get; set; }
        public string MainFileOpenKeepVideo { get; set; }
        public string MainFileSave { get; set; }
        public string MainFileSaveOriginal { get; set; }
        public string MainFileSaveOriginalAs { get; set; }
        public string MainFileSaveAs { get; set; }
        public string MainFileSaveAll { get; set; }
        public string MainFileExportEbu { get; set; }
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
        public string MainToolsSplitLongLines { get; set; }
        public string MainToolsRenumber { get; set; }
        public string MainToolsRemoveTextForHI { get; set; }
        public string MainToolsChangeCasing { get; set; }
        public string MainToolsAutoDuration { get; set; }
        public string MainToolsBatchConvert { get; set; }
        public string MainToolsBeamer { get; set; }
        public string MainVideoPause { get; set; }
        public string MainVideoPlayPauseToggle { get; set; }
        public string MainVideoShowHideVideo { get; set; }
        public string MainVideoToggleVideoControls { get; set; }
        public string MainVideo1FrameLeft { get; set; }
        public string MainVideo1FrameRight { get; set; }
        public string MainVideo100MsLeft { get; set; }
        public string MainVideo100MsRight { get; set; }
        public string MainVideo500MsLeft { get; set; }
        public string MainVideo500MsRight { get; set; }
        public string MainVideo1000MsLeft { get; set; }
        public string MainVideo1000MsRight { get; set; }
        public string MainVideoFullscreen { get; set; }
        public string MainSpellCheck { get; set; }
        public string MainSpellCheckFindDoubleWords { get; set; }
        public string MainSpellCheckAddWordToNames { get; set; }
        public string MainSynchronizationAdjustTimes { get; set; }
        public string MainSynchronizationVisualSync { get; set; }
        public string MainSynchronizationPointSync { get; set; }
        public string MainSynchronizationChangeFrameRate { get; set; }
        public string MainListViewItalic { get; set; }
        public string MainListViewToggleDashes { get; set; }
        public string MainListViewAlignment { get; set; }
        public string MainListViewCopyText { get; set; }
        public string MainListViewAutoDuration { get; set; }
        public string MainListViewColumnDeleteText { get; set; }
        public string MainListViewColumnInsertText { get; set; }
        public string MainListViewColumnPaste { get; set; }
        public string MainListViewFocusWaveform { get; set; }
        public string MainListViewGoToNextError { get; set; }
        public string MainTextBoxItalic { get; set; }
        public string MainTextBoxSplitAtCursor { get; set; }
        public string MainTextBoxMoveLastWordDown { get; set; }
        public string MainTextBoxMoveFirstWordFromNextUp { get; set; }
        public string MainTextBoxSelectionToLower { get; set; }
        public string MainTextBoxSelectionToUpper { get; set; }
        public string MainCreateInsertSubAtVideoPos { get; set; }
        public string MainCreatePlayFromJustBefore { get; set; }
        public string MainCreateSetStart { get; set; }
        public string MainCreateSetEnd { get; set; }
        public string MainCreateSetEndAddNewAndGoToNew { get; set; }
        public string MainCreateStartDownEndUp { get; set; }
        public string MainAdjustSetStartAndOffsetTheRest { get; set; }
        public string MainAdjustSetEndAndOffsetTheRest { get; set; }
        public string MainAdjustSetEndAndOffsetTheRestAndGoToNext { get; set; }
        public string MainAdjustSetEndAndGotoNext { get; set; }
        public string MainAdjustViaEndAutoStartAndGoToNext { get; set; }
        public string MainAdjustSetStartAutoDurationAndGoToNext { get; set; }
        public string MainAdjustSetEndNextStartAndGoToNext { get; set; }
        public string MainAdjustStartDownEndUpAndGoToNext { get; set; }
        public string MainAdjustSetStart { get; set; }
        public string MainAdjustSetStartKeepDuration { get; set; }
        public string MainAdjustSetEnd { get; set; }
        public string MainAdjustSelected100MsForward { get; set; }
        public string MainAdjustSelected100MsBack { get; set; }
        public string MainInsertAfter { get; set; }
        public string MainTextBoxInsertAfter { get; set; }
        public string MainTextBoxAutoBreak { get; set; }
        public string MainTextBoxUnbreak { get; set; }
        public string MainWaveformInsertAtCurrentPosition { get; set; }
        public string MainInsertBefore { get; set; }
        public string MainMergeDialog { get; set; }
        public string MainToggleFocus { get; set; }
        public string WaveformVerticalZoom { get; set; }
        public string WaveformVerticalZoomOut { get; set; }
        public string WaveformZoomIn { get; set; }
        public string WaveformZoomOut { get; set; }
        public string WaveformPlaySelection { get; set; }
        public string WaveformSearchSilenceForward { get; set; }
        public string WaveformSearchSilenceBack { get; set; }
        public string WaveformAddTextHere { get; set; }
        public string WaveformFocusListView { get; set; }
        public string MainTranslateCustomSearch1 { get; set; }
        public string MainTranslateCustomSearch2 { get; set; }
        public string MainTranslateCustomSearch3 { get; set; }
        public string MainTranslateCustomSearch4 { get; set; }
        public string MainTranslateCustomSearch5 { get; set; }
        public string MainTranslateCustomSearch6 { get; set; }

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
            MainVideoFullscreen = "Alt+Return";
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
            MainToolsBeamer = "Control+Shift+Alt+B";
            MainCreateInsertSubAtVideoPos = string.Empty;
            MainCreatePlayFromJustBefore = string.Empty;
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
            MainAdjustSetStart = string.Empty;
            MainAdjustSetStartKeepDuration = string.Empty;
            MainAdjustSetEnd = string.Empty;
            MainAdjustSelected100MsForward = string.Empty;
            MainAdjustSelected100MsBack = string.Empty;
            MainInsertAfter = "Alt+Insert";
            MainWaveformInsertAtCurrentPosition = "Insert";
            MainInsertBefore = "Control+Shift+Insert";
            MainTextBoxInsertAfter = "Alt+Insert";
            MainTextBoxAutoBreak = "Control+R";
            MainTextBoxUnbreak = string.Empty;
            MainMergeDialog = string.Empty;
            WaveformVerticalZoom = "Shift+Add";
            WaveformVerticalZoomOut = "Shift+Subtract";
            WaveformPlaySelection = string.Empty;
            GeneralPlayFirstSelected = string.Empty;
            WaveformSearchSilenceForward = string.Empty;
            WaveformSearchSilenceBack = string.Empty;
            WaveformAddTextHere = string.Empty;
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

    public class Settings
    {
        public RecentFilesSettings RecentFiles { get; set; }
        public GeneralSettings General { get; set; }
        public ToolsSettings Tools { get; set; }
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

        [XmlArrayItem("MultipleSearchAndReplaceItem")]
        public List<MultipleSearchAndReplaceSetting> MultipleSearchAndReplaceList { get; set; }

        [XmlIgnore]
        public Language Language { get; set; }

        private Settings()
        {
            RecentFiles = new RecentFilesSettings();
            General = new GeneralSettings();
            Tools = new ToolsSettings();
            WordLists = new WordListSettings();
            SubtitleSettings = new SubtitleSettings();
            Proxy = new ProxySettings();
            CommonErrors = new FixCommonErrorsSettings();
            VobSubOcr = new VobSubOcrSettings();
            VideoControls = new VideoControlsSettings();
            NetworkSettings = new NetworkSettings();
            MultipleSearchAndReplaceList = new List<MultipleSearchAndReplaceSetting>();
            Language = new Language();
            Shortcuts = new Shortcuts();
            RemoveTextForHearingImpaired = new RemoveTextForHearingImpairedSettings();
            SubtitleBeaming = new SubtitleBeaming();
        }

        public void Save()
        {
            //this is too slow: Serialize(Configuration.BaseDirectory + "Settings.xml", this);

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
            Settings settings = new Settings();
            string settingsFileName = Configuration.SettingsFileName;
            if (File.Exists(settingsFileName))
            {
                try
                {
                    settings = CustomDeserialize(settingsFileName); //  15 msecs

                    if (settings.General.AutoConvertToUtf8)
                        settings.General.DefaultEncoding = Encoding.UTF8.EncodingName;
                    //too slow... :(  - settings = Deserialize(Configuration.BaseDirectory + "Settings.xml"); // 688 msecs
                }
                catch
                {
                    settings = new Settings();
                }

                if (!string.IsNullOrEmpty(settings.General.ListViewLineSeparatorString))
                    settings.General.ListViewLineSeparatorString = settings.General.ListViewLineSeparatorString.Replace("\n", string.Empty).Replace("\r", string.Empty);

                if (string.IsNullOrWhiteSpace(settings.General.ListViewLineSeparatorString))
                    settings.General.ListViewLineSeparatorString = "<br />";

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
            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;

            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            doc.Load(stream);
            stream.Close();

            var settings = new Settings();

            settings.RecentFiles = new RecentFilesSettings();
            XmlNode node = doc.DocumentElement.SelectSingleNode("RecentFiles");
            foreach (XmlNode listNode in node.SelectNodes("FileNames/FileName"))
            {
                string firstVisibleIndex = "-1";
                if (listNode.Attributes["FirstVisibleIndex"] != null)
                    firstVisibleIndex = listNode.Attributes["FirstVisibleIndex"].Value;

                string firstSelectedIndex = "-1";
                if (listNode.Attributes["FirstSelectedIndex"] != null)
                    firstSelectedIndex = listNode.Attributes["FirstSelectedIndex"].Value;

                string videoFileName = null;
                if (listNode.Attributes["VideoFileName"] != null)
                    videoFileName = listNode.Attributes["VideoFileName"].Value;

                string originalFileName = null;
                if (listNode.Attributes["OriginalFileName"] != null)
                    originalFileName = listNode.Attributes["OriginalFileName"].Value;

                settings.RecentFiles.Files.Add(new RecentFileEntry() { FileName = listNode.InnerText, FirstVisibleIndex = int.Parse(firstVisibleIndex), FirstSelectedIndex = int.Parse(firstSelectedIndex), VideoFileName = videoFileName, OriginalFileName = originalFileName });
            }

            settings.General = new GeneralSettings();
            node = doc.DocumentElement.SelectSingleNode("General");
            XmlNode subNode = node.SelectSingleNode("ShowToolbarNew");
            if (subNode != null)
                settings.General.ShowToolbarNew = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowToolbarOpen");
            if (subNode != null)
                settings.General.ShowToolbarOpen = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowToolbarSave");
            if (subNode != null)
                settings.General.ShowToolbarSave = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowToolbarSaveAs");
            if (subNode != null)
                settings.General.ShowToolbarSaveAs = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowToolbarFind");
            if (subNode != null)
                settings.General.ShowToolbarFind = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowToolbarReplace");
            if (subNode != null)
                settings.General.ShowToolbarReplace = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowToolbarFixCommonErrors");
            if (subNode != null)
                settings.General.ShowToolbarFixCommonErrors = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowToolbarVisualSync");
            if (subNode != null)
                settings.General.ShowToolbarVisualSync = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowToolbarSpellCheck");
            if (subNode != null)
                settings.General.ShowToolbarSpellCheck = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowToolbarSettings");
            if (subNode != null)
                settings.General.ShowToolbarSettings = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowToolbarHelp");
            if (subNode != null)
                settings.General.ShowToolbarHelp = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowFrameRate");
            if (subNode != null)
                settings.General.ShowFrameRate = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowVideoPlayer");
            if (subNode != null)
                settings.General.ShowVideoPlayer = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowAudioVisualizer");
            if (subNode != null)
                settings.General.ShowAudioVisualizer = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowWaveform");
            if (subNode != null)
                settings.General.ShowWaveform = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowSpectrogram");
            if (subNode != null)
                settings.General.ShowSpectrogram = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("DefaultFrameRate");
            if (subNode != null)
            {
                settings.General.DefaultFrameRate = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
                if (settings.General.DefaultFrameRate > 23975)
                    settings.General.DefaultFrameRate = 23.976;
                settings.General.CurrentFrameRate = settings.General.DefaultFrameRate;
            }
            subNode = node.SelectSingleNode("DefaultSubtitleFormat");
            if (subNode != null)
                settings.General.DefaultSubtitleFormat = subNode.InnerText;
            subNode = node.SelectSingleNode("DefaultEncoding");
            if (subNode != null)
                settings.General.DefaultEncoding = subNode.InnerText;
            subNode = node.SelectSingleNode("AutoConvertToUtf8");
            if (subNode != null)
                settings.General.AutoConvertToUtf8 = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("AutoGuessAnsiEncoding");
            if (subNode != null)
                settings.General.AutoGuessAnsiEncoding = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("_subtitleFontName");
            if (subNode != null)
                settings.General.SubtitleFontName = subNode.InnerText;
            subNode = node.SelectSingleNode("SubtitleFontSize");
            if (subNode != null)
                settings.General.SubtitleFontSize = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("SubtitleFontBold");
            if (subNode != null)
                settings.General.SubtitleFontBold = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SubtitleFontColor");
            if (subNode != null)
                settings.General.SubtitleFontColor = Color.FromArgb(Convert.ToInt32(subNode.InnerText));
            subNode = node.SelectSingleNode("SubtitleBackgroundColor");
            if (subNode != null)
                settings.General.SubtitleBackgroundColor = Color.FromArgb(Convert.ToInt32(subNode.InnerText));
            subNode = node.SelectSingleNode("CenterSubtitleInTextBox");
            if (subNode != null)
                settings.General.CenterSubtitleInTextBox = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ShowRecentFiles");
            if (subNode != null)
                settings.General.ShowRecentFiles = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("RememberSelectedLine");
            if (subNode != null)
                settings.General.RememberSelectedLine = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("StartLoadLastFile");
            if (subNode != null)
                settings.General.StartLoadLastFile = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("StartRememberPositionAndSize");
            if (subNode != null)
                settings.General.StartRememberPositionAndSize = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("StartPosition");
            if (subNode != null)
                settings.General.StartPosition = subNode.InnerText;
            subNode = node.SelectSingleNode("StartSize");
            if (subNode != null)
                settings.General.StartSize = subNode.InnerText;
            subNode = node.SelectSingleNode("SplitContainerMainSplitterDistance");
            if (subNode != null)
                settings.General.SplitContainerMainSplitterDistance = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("SplitContainer1SplitterDistance");
            if (subNode != null)
                settings.General.SplitContainer1SplitterDistance = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("SplitContainerListViewAndTextSplitterDistance");
            if (subNode != null)
                settings.General.SplitContainerListViewAndTextSplitterDistance = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("StartInSourceView");
            if (subNode != null)
                settings.General.StartInSourceView = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("RemoveBlankLinesWhenOpening");
            if (subNode != null)
                settings.General.RemoveBlankLinesWhenOpening = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SubtitleLineMaximumLength");
            if (subNode != null)
                settings.General.SubtitleLineMaximumLength = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("SubtitleMinimumDisplayMilliseconds");
            if (subNode != null)
                settings.General.SubtitleMinimumDisplayMilliseconds = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("SubtitleMaximumDisplayMilliseconds");
            if (subNode != null)
                settings.General.SubtitleMaximumDisplayMilliseconds = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("MininumMillisecondsBetweenLines");
            if (subNode != null)
                settings.General.MininumMillisecondsBetweenLines = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("SetStartEndHumanDelay");
            if (subNode != null)
                settings.General.SetStartEndHumanDelay = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("AutoWrapLineWhileTyping");
            if (subNode != null)
                settings.General.AutoWrapLineWhileTyping = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SubtitleMaximumCharactersPerSeconds");
            if (subNode != null)
                settings.General.SubtitleMaximumCharactersPerSeconds = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            subNode = node.SelectSingleNode("SubtitleOptimalCharactersPerSeconds");
            if (subNode != null)
                settings.General.SubtitleOptimalCharactersPerSeconds = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            subNode = node.SelectSingleNode("SpellCheckLanguage");
            if (subNode != null)
                settings.General.SpellCheckLanguage = subNode.InnerText;
            subNode = node.SelectSingleNode("VideoPlayer");
            if (subNode != null)
                settings.General.VideoPlayer = subNode.InnerText;
            subNode = node.SelectSingleNode("VideoPlayerDefaultVolume");
            if (subNode != null)
                settings.General.VideoPlayerDefaultVolume = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("VideoPlayerPreviewFontSize");
            if (subNode != null)
                settings.General.VideoPlayerPreviewFontSize = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("VideoPlayerPreviewFontBold");
            if (subNode != null)
                settings.General.VideoPlayerPreviewFontBold = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("VideoPlayerShowStopButton");
            if (subNode != null)
                settings.General.VideoPlayerShowStopButton = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("VideoPlayerShowMuteButton");
            if (subNode != null)
                settings.General.VideoPlayerShowMuteButton = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("VideoPlayerShowFullscreenButton");
            if (subNode != null)
                settings.General.VideoPlayerShowFullscreenButton = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("Language");
            if (subNode != null)
                settings.General.Language = subNode.InnerText;
            subNode = node.SelectSingleNode("ListViewLineSeparatorString");
            if (subNode != null)
                settings.General.ListViewLineSeparatorString = subNode.InnerText;
            subNode = node.SelectSingleNode("ListViewDoubleClickAction");
            if (subNode != null)
                settings.General.ListViewDoubleClickAction = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("UppercaseLetters");
            if (subNode != null)
                settings.General.UppercaseLetters = subNode.InnerText;
            subNode = node.SelectSingleNode("DefaultAdjustMilliseconds");
            if (subNode != null)
                settings.General.DefaultAdjustMilliseconds = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("AutoRepeatOn");
            if (subNode != null)
                settings.General.AutoRepeatOn = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("AutoRepeatCount");
            if (subNode != null)
                settings.General.AutoRepeatCount = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("SyncListViewWithVideoWhilePlaying");
            if (subNode != null)
                settings.General.SyncListViewWithVideoWhilePlaying = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("AutoContinueOn");
            if (subNode != null)
                settings.General.AutoContinueOn = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("AutoBackupSeconds");
            if (subNode != null)
                settings.General.AutoBackupSeconds = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("SpellChecker");
            if (subNode != null)
                settings.General.SpellChecker = subNode.InnerText;
            subNode = node.SelectSingleNode("AllowEditOfOriginalSubtitle");
            if (subNode != null)
                settings.General.AllowEditOfOriginalSubtitle = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("PromptDeleteLines");
            if (subNode != null)
                settings.General.PromptDeleteLines = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("Undocked");
            if (subNode != null)
                settings.General.Undocked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("UndockedVideoPosition");
            if (subNode != null)
                settings.General.UndockedVideoPosition = subNode.InnerText;
            subNode = node.SelectSingleNode("UndockedWaveformPosition");
            if (subNode != null)
                settings.General.UndockedWaveformPosition = subNode.InnerText;
            subNode = node.SelectSingleNode("UndockedVideoControlsPosition");
            if (subNode != null)
                settings.General.UndockedVideoControlsPosition = subNode.InnerText;
            subNode = node.SelectSingleNode("WaveformCenter");
            if (subNode != null)
                settings.General.WaveformCenter = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SmallDelayMilliseconds");
            if (subNode != null)
                settings.General.SmallDelayMilliseconds = Convert.ToInt32((subNode.InnerText));
            subNode = node.SelectSingleNode("LargeDelayMilliseconds");
            if (subNode != null)
                settings.General.LargeDelayMilliseconds = Convert.ToInt32((subNode.InnerText));
            subNode = node.SelectSingleNode("ShowOriginalAsPreviewIfAvailable");
            if (subNode != null)
                settings.General.ShowOriginalAsPreviewIfAvailable = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("LastPacCodePage");
            if (subNode != null)
                settings.General.LastPacCodePage = Convert.ToInt32((subNode.InnerText));
            subNode = node.SelectSingleNode("OpenSubtitleExtraExtensions");
            if (subNode != null)
                settings.General.OpenSubtitleExtraExtensions = subNode.InnerText.Trim();
            subNode = node.SelectSingleNode("ListViewColumnsRememberSize");
            if (subNode != null)
                settings.General.ListViewColumnsRememberSize = Convert.ToBoolean(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("ListViewNumberWidth");
            if (subNode != null)
                settings.General.ListViewNumberWidth = Convert.ToInt32(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("ListViewStartWidth");
            if (subNode != null)
                settings.General.ListViewStartWidth = Convert.ToInt32(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("ListViewEndWidth");
            if (subNode != null)
                settings.General.ListViewEndWidth = Convert.ToInt32(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("ListViewDurationWidth");
            if (subNode != null)
                settings.General.ListViewDurationWidth = Convert.ToInt32(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("ListViewTextWidth");
            if (subNode != null)
                settings.General.ListViewTextWidth = Convert.ToInt32(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("VlcWaveTranscodeSettings");
            if (subNode != null)
                settings.General.VlcWaveTranscodeSettings = subNode.InnerText.Trim();
            subNode = node.SelectSingleNode("VlcLocation");
            if (subNode != null)
                settings.General.VlcLocation = subNode.InnerText.Trim();
            subNode = node.SelectSingleNode("VlcLocationRelative");
            if (subNode != null)
                settings.General.VlcLocationRelative = subNode.InnerText.Trim();
            subNode = node.SelectSingleNode("MpcHcLocation");
            if (subNode != null)
                settings.General.MpcHcLocation = subNode.InnerText.Trim();
            subNode = node.SelectSingleNode("UseFFmpegForWaveExtraction");
            if (subNode != null)
                settings.General.UseFFmpegForWaveExtraction = Convert.ToBoolean(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("FFmpegLocation");
            if (subNode != null)
                settings.General.FFmpegLocation = subNode.InnerText.Trim();
            subNode = node.SelectSingleNode("UseTimeFormatHHMMSSFF");
            if (subNode != null)
                settings.General.UseTimeFormatHHMMSSFF = Convert.ToBoolean(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("ClearStatusBarAfterSeconds");
            if (subNode != null)
                settings.General.ClearStatusBarAfterSeconds = Convert.ToInt32(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("Company");
            if (subNode != null)
                settings.General.Company = subNode.InnerText;
            subNode = node.SelectSingleNode("DisableVideoAutoLoading");
            if (subNode != null)
                settings.General.DisableVideoAutoLoading = Convert.ToBoolean(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("RightToLeftMode");
            if (subNode != null)
                settings.General.RightToLeftMode = Convert.ToBoolean(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("LastSaveAsFormat");
            if (subNode != null)
                settings.General.LastSaveAsFormat = subNode.InnerText.Trim();
            subNode = node.SelectSingleNode("CheckForUpdates");
            if (subNode != null)
                settings.General.CheckForUpdates = Convert.ToBoolean(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("LastCheckForUpdates");
            if (subNode != null)
                settings.General.LastCheckForUpdates = Convert.ToDateTime(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("ShowBetaStuff");
            if (subNode != null)
                settings.General.ShowBetaStuff = Convert.ToBoolean(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("NewEmptyDefaultMs");
            if (subNode != null)
                settings.General.NewEmptyDefaultMs = Convert.ToInt32(subNode.InnerText.Trim());
            subNode = node.SelectSingleNode("MoveVideo100Or500MsPlaySmallSample");
            if (subNode != null)
                settings.General.MoveVideo100Or500MsPlaySmallSample = Convert.ToBoolean(subNode.InnerText.Trim());

            settings.Tools = new ToolsSettings();
            node = doc.DocumentElement.SelectSingleNode("Tools");
            subNode = node.SelectSingleNode("StartSceneIndex");
            if (subNode != null)
                settings.Tools.StartSceneIndex = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("EndSceneIndex");
            if (subNode != null)
                settings.Tools.EndSceneIndex = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("VerifyPlaySeconds");
            if (subNode != null)
                settings.Tools.VerifyPlaySeconds = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("MergeLinesShorterThan");
            if (subNode != null)
                settings.Tools.MergeLinesShorterThan = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("FixShortDisplayTimesAllowMoveStartTime");
            if (subNode != null)
                settings.Tools.FixShortDisplayTimesAllowMoveStartTime = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("MusicSymbol");
            if (subNode != null)
                settings.Tools.MusicSymbol = subNode.InnerText;
            subNode = node.SelectSingleNode("MusicSymbolToReplace");
            if (subNode != null)
                settings.Tools.MusicSymbolToReplace = subNode.InnerText;
            subNode = node.SelectSingleNode("UnicodeSymbolsToInsert");
            if (subNode != null)
                settings.Tools.UnicodeSymbolsToInsert = subNode.InnerText;
            subNode = node.SelectSingleNode("SpellCheckAutoChangeNames");
            if (subNode != null)
                settings.Tools.SpellCheckAutoChangeNames = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SpellCheckOneLetterWords");
            if (subNode != null)
                settings.Tools.SpellCheckOneLetterWords = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SpellCheckEnglishAllowInQuoteAsIng");
            if (subNode != null)
                settings.Tools.SpellCheckEnglishAllowInQuoteAsIng = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SpellCheckShowCompletedMessage");
            if (subNode != null)
                settings.Tools.SpellCheckShowCompletedMessage = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("OcrFixUseHardcodedRules");
            if (subNode != null)
                settings.Tools.OcrFixUseHardcodedRules = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("Interjections");
            if (subNode != null)
                settings.Tools.Interjections = subNode.InnerText;
            subNode = node.SelectSingleNode("MicrosoftBingApiId");
            if (subNode != null)
                settings.Tools.MicrosoftBingApiId = subNode.InnerText;
            subNode = node.SelectSingleNode("GoogleApiKey");
            if (subNode != null)
                settings.Tools.GoogleApiKey = subNode.InnerText;
            subNode = node.SelectSingleNode("UseGooleApiPaidService");
            if (subNode != null)
                settings.Tools.UseGooleApiPaidService = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("GoogleTranslateLastTargetLanguage");
            if (subNode != null)
                settings.Tools.GoogleTranslateLastTargetLanguage = subNode.InnerText;
            subNode = node.SelectSingleNode("ListViewSyntaxColorDurationSmall");
            if (subNode != null)
                settings.Tools.ListViewSyntaxColorDurationSmall = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ListViewSyntaxColorDurationBig");
            if (subNode != null)
                settings.Tools.ListViewSyntaxColorDurationBig = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ListViewSyntaxColorLongLines");
            if (subNode != null)
                settings.Tools.ListViewSyntaxColorLongLines = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ListViewSyntaxMoreThanXLines");
            if (subNode != null)
                settings.Tools.ListViewSyntaxMoreThanXLines = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ListViewSyntaxMoreThanXLinesX");
            if (subNode != null)
                settings.Tools.ListViewSyntaxMoreThanXLinesX = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("ListViewSyntaxColorOverlap");
            if (subNode != null)
                settings.Tools.ListViewSyntaxColorOverlap = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ListViewSyntaxErrorColor");
            if (subNode != null)
                settings.Tools.ListViewSyntaxErrorColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("ListViewUnfocusedSelectedColor");
            if (subNode != null)
                settings.Tools.ListViewUnfocusedSelectedColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("SplitAdvanced");
            if (subNode != null)
                settings.Tools.SplitAdvanced = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SplitOutputFolder");
            if (subNode != null)
                settings.Tools.SplitOutputFolder = subNode.InnerText;
            subNode = node.SelectSingleNode("SplitNumberOfParts");
            if (subNode != null)
                settings.Tools.SplitNumberOfParts = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("SplitVia");
            if (subNode != null)
                settings.Tools.SplitVia = subNode.InnerText;
            subNode = node.SelectSingleNode("NewEmptyTranslationText");
            if (subNode != null)
                settings.Tools.NewEmptyTranslationText = subNode.InnerText;
            subNode = node.SelectSingleNode("BatchConvertOutputFolder");
            if (subNode != null)
                settings.Tools.BatchConvertOutputFolder = subNode.InnerText;
            subNode = node.SelectSingleNode("BatchConvertOverwriteExisting");
            if (subNode != null)
                settings.Tools.BatchConvertOverwriteExisting = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("BatchConvertOverwriteOriginal");
            if (subNode != null)
                settings.Tools.BatchConvertOverwriteOriginal = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("BatchConvertRemoveFormatting");
            if (subNode != null)
                settings.Tools.BatchConvertRemoveFormatting = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("BatchConvertFixCasing");
            if (subNode != null)
                settings.Tools.BatchConvertFixCasing = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("BatchConvertRemoveTextForHI");
            if (subNode != null)
                settings.Tools.BatchConvertRemoveTextForHI = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("BatchConvertFixCommonErrors");
            if (subNode != null)
                settings.Tools.BatchConvertFixCommonErrors = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("BatchConvertMultipleReplace");
            if (subNode != null)
                settings.Tools.BatchConvertMultipleReplace = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("BatchConvertAutoBalance");
            if (subNode != null)
                settings.Tools.BatchConvertAutoBalance = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("BatchConvertSplitLongLines");
            if (subNode != null)
                settings.Tools.BatchConvertSplitLongLines = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("BatchConvertSetMinDisplayTimeBetweenSubtitles");
            if (subNode != null)
                settings.Tools.BatchConvertSetMinDisplayTimeBetweenSubtitles = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("BatchConvertLanguage");
            if (subNode != null)
                settings.Tools.BatchConvertLanguage = subNode.InnerText;
            subNode = node.SelectSingleNode("ModifySelectionRule");
            if (subNode != null)
                settings.Tools.ModifySelectionRule = subNode.InnerText;
            subNode = node.SelectSingleNode("ModifySelectionText");
            if (subNode != null)
                settings.Tools.ModifySelectionText = subNode.InnerText;
            subNode = node.SelectSingleNode("ModifySelectionCaseSensitive");
            if (subNode != null)
                settings.Tools.ModifySelectionCaseSensitive = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportVobSubFontName");
            if (subNode != null)
                settings.Tools.ExportVobSubFontName = subNode.InnerText;
            subNode = node.SelectSingleNode("ExportVobSubFontSize");
            if (subNode != null)
                settings.Tools.ExportVobSubFontSize = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportVobSubVideoResolution");
            if (subNode != null)
                settings.Tools.ExportVobSubVideoResolution = subNode.InnerText;
            subNode = node.SelectSingleNode("ExportVobSubSimpleRendering");
            if (subNode != null)
                settings.Tools.ExportVobSubSimpleRendering = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportVobAntiAliasingWithTransparency");
            if (subNode != null)
                settings.Tools.ExportVobAntiAliasingWithTransparency = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportVobSubLanguage");
            if (subNode != null)
                settings.Tools.ExportVobSubLanguage = subNode.InnerText;
            subNode = node.SelectSingleNode("ExportBluRayFontName");
            if (subNode != null)
                settings.Tools.ExportBluRayFontName = subNode.InnerText;
            subNode = node.SelectSingleNode("ExportBluRayFontSize");
            if (subNode != null)
                settings.Tools.ExportBluRayFontSize = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportFcpFontName");
            if (subNode != null)
                settings.Tools.ExportFcpFontName = subNode.InnerText;
            subNode = node.SelectSingleNode("ExportFontNameOther");
            if (subNode != null)
                settings.Tools.ExportFontNameOther = subNode.InnerText;
            subNode = node.SelectSingleNode("ExportFcpFontSize");
            if (subNode != null)
                settings.Tools.ExportFcpFontSize = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportFcpImageType");
            if (subNode != null)
                settings.Tools.ExportFcpImageType = subNode.InnerText;
            subNode = node.SelectSingleNode("ExportLastFontSize");
            if (subNode != null)
                settings.Tools.ExportLastFontSize = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportLastLineHeight");
            if (subNode != null)
                settings.Tools.ExportLastLineHeight = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportLastBorderWidth");
            if (subNode != null)
                settings.Tools.ExportLastBorderWidth = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportLastFontBold");
            if (subNode != null)
                settings.Tools.ExportLastFontBold = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportBluRayVideoResolution");
            if (subNode != null)
                settings.Tools.ExportBluRayVideoResolution = subNode.InnerText;
            subNode = node.SelectSingleNode("ExportFontColor");
            if (subNode != null)
                settings.Tools.ExportFontColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("ExportBorderColor");
            if (subNode != null)
                settings.Tools.ExportBorderColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("ExportShadowColor");
            if (subNode != null)
                settings.Tools.ExportShadowColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("ExportBottomMargin");
            if (subNode != null)
                settings.Tools.ExportBottomMargin = int.Parse(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportHorizontalAlignment");
            if (subNode != null)
                settings.Tools.ExportHorizontalAlignment = int.Parse(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportBluRayBottomMargin");
            if (subNode != null)
                settings.Tools.ExportBluRayBottomMargin = int.Parse(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportBluRayShadow");
            if (subNode != null)
                settings.Tools.ExportBluRayShadow = int.Parse(subNode.InnerText);
            subNode = node.SelectSingleNode("Export3DType");
            if (subNode != null)
                settings.Tools.Export3DType = int.Parse(subNode.InnerText);
            subNode = node.SelectSingleNode("Export3DDepth");
            if (subNode != null)
                settings.Tools.Export3DDepth = int.Parse(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportLastShadowTransparency");
            if (subNode != null)
                settings.Tools.ExportLastShadowTransparency = int.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            subNode = node.SelectSingleNode("ExportLastFrameRate");
            if (subNode != null)
                settings.Tools.ExportLastFrameRate = double.Parse(subNode.InnerText, CultureInfo.InvariantCulture);
            subNode = node.SelectSingleNode("FixCommonErrorsFixOverlapAllowEqualEndStart");
            if (subNode != null)
                settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ImportTextSplitting");
            if (subNode != null)
                settings.Tools.ImportTextSplitting = subNode.InnerText;
            subNode = node.SelectSingleNode("ImportTextMergeShortLines");
            if (subNode != null)
                settings.Tools.ImportTextMergeShortLines = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("GenerateTimeCodePatterns");
            if (subNode != null)
                settings.Tools.GenerateTimeCodePatterns = subNode.InnerText;
            subNode = node.SelectSingleNode("MusicSymbolStyle");
            if (subNode != null)
                settings.Tools.MusicSymbolStyle = subNode.InnerText;
            subNode = node.SelectSingleNode("BridgeGapMilliseconds");
            if (subNode != null)
                settings.Tools.BridgeGapMilliseconds = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("ExportCustomTemplates");
            if (subNode != null)
                settings.Tools.ExportCustomTemplates = subNode.InnerText;
            subNode = node.SelectSingleNode("ChangeCasingChoice");
            if (subNode != null)
                settings.Tools.ChangeCasingChoice = subNode.InnerText;
            subNode = node.SelectSingleNode("UseNoLineBreakAfter");
            if (subNode != null)
                settings.Tools.UseNoLineBreakAfter = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("NoLineBreakAfterEnglish");
            if (subNode != null)
                settings.Tools.NoLineBreakAfterEnglish = subNode.InnerText.Replace("  ", " ");

            settings.SubtitleSettings = new SubtitleSettings();
            node = doc.DocumentElement.SelectSingleNode("SubtitleSettings");
            if (node != null)
            {
                subNode = node.SelectSingleNode("SsaFontName");
                if (subNode != null)
                    settings.SubtitleSettings.SsaFontName = subNode.InnerText;
                subNode = node.SelectSingleNode("SsaFontSize");
                if (subNode != null)
                    settings.SubtitleSettings.SsaFontSize = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
                subNode = node.SelectSingleNode("SsaFontColorArgb");
                if (subNode != null)
                    settings.SubtitleSettings.SsaFontColorArgb = Convert.ToInt32(subNode.InnerText);
                subNode = node.SelectSingleNode("SsaOutline");
                if (subNode != null)
                    settings.SubtitleSettings.SsaOutline = Convert.ToInt32(subNode.InnerText);
                subNode = node.SelectSingleNode("SsaShadow");
                if (subNode != null)
                    settings.SubtitleSettings.SsaShadow = Convert.ToInt32(subNode.InnerText);
                subNode = node.SelectSingleNode("SsaOpaqueBox");
                if (subNode != null)
                    settings.SubtitleSettings.SsaOpaqueBox = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("DCinemaFontFile");
                if (subNode != null)
                    settings.SubtitleSettings.DCinemaFontFile = subNode.InnerText;
                subNode = node.SelectSingleNode("DCinemaFontSize");
                if (subNode != null)
                    settings.SubtitleSettings.DCinemaFontSize = Convert.ToInt32(subNode.InnerText);
                subNode = node.SelectSingleNode("DCinemaBottomMargin");
                if (subNode != null)
                    settings.SubtitleSettings.DCinemaBottomMargin = Convert.ToInt32(subNode.InnerText, CultureInfo.InvariantCulture);
                subNode = node.SelectSingleNode("DCinemaZPosition");
                if (subNode != null)
                    settings.SubtitleSettings.DCinemaZPosition = Convert.ToDouble(subNode.InnerText);
                subNode = node.SelectSingleNode("DCinemaFadeUpTime");
                if (subNode != null)
                    settings.SubtitleSettings.DCinemaFadeUpTime = Convert.ToInt32(subNode.InnerText);
                subNode = node.SelectSingleNode("DCinemaFadeDownTime");
                if (subNode != null)
                    settings.SubtitleSettings.DCinemaFadeDownTime = Convert.ToInt32(subNode.InnerText);
                subNode = node.SelectSingleNode("SamiDisplayTwoClassesAsTwoSubtitles");
                if (subNode != null)
                    settings.SubtitleSettings.SamiDisplayTwoClassesAsTwoSubtitles = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("SamiHtmlEncodeMode");
                if (subNode != null)
                    settings.SubtitleSettings.SamiHtmlEncodeMode = Convert.ToInt32(subNode.InnerText);
                subNode = node.SelectSingleNode("TimedText10TimeCodeFormat");
                if (subNode != null)
                    settings.SubtitleSettings.TimedText10TimeCodeFormat = subNode.InnerText;
                subNode = node.SelectSingleNode("FcpFontSize");
                if (subNode != null)
                    settings.SubtitleSettings.FcpFontSize = Convert.ToInt32(subNode.InnerText);
                subNode = node.SelectSingleNode("FcpFontName");
                if (subNode != null)
                    settings.SubtitleSettings.FcpFontName = subNode.InnerText;
                subNode = node.SelectSingleNode("CheetahCaptionAlwayWriteEndTime");
                if (subNode != null)
                    settings.SubtitleSettings.CheetahCaptionAlwayWriteEndTime = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("NuendoCharacterListFile");
                if (subNode != null)
                    settings.SubtitleSettings.NuendoCharacterListFile = subNode.InnerText;
            }

            settings.Proxy = new ProxySettings();
            node = doc.DocumentElement.SelectSingleNode("Proxy");
            subNode = node.SelectSingleNode("ProxyAddress");
            if (subNode != null)
                settings.Proxy.ProxyAddress = subNode.InnerText;
            subNode = node.SelectSingleNode("UserName");
            if (subNode != null)
                settings.Proxy.UserName = subNode.InnerText;
            subNode = node.SelectSingleNode("Password");
            if (subNode != null)
                settings.Proxy.Password = subNode.InnerText;
            subNode = node.SelectSingleNode("Domain");
            if (subNode != null)
                settings.Proxy.Domain = subNode.InnerText;

            settings.WordLists = new WordListSettings();
            node = doc.DocumentElement.SelectSingleNode("WordLists");
            subNode = node.SelectSingleNode("LastLanguage");
            if (subNode != null)
                settings.WordLists.LastLanguage = subNode.InnerText;
            subNode = node.SelectSingleNode("NamesEtcUrl");
            if (subNode != null)
                settings.WordLists.NamesEtcUrl = subNode.InnerText;
            subNode = node.SelectSingleNode("UseOnlineNamesEtc");
            if (subNode != null)
                settings.WordLists.UseOnlineNamesEtc = Convert.ToBoolean(subNode.InnerText);

            settings.CommonErrors = new FixCommonErrorsSettings();
            node = doc.DocumentElement.SelectSingleNode("CommonErrors");
            subNode = node.SelectSingleNode("StartPosition");
            if (subNode != null)
                settings.CommonErrors.StartPosition = subNode.InnerText;
            subNode = node.SelectSingleNode("StartSize");
            if (subNode != null)
                settings.CommonErrors.StartSize = subNode.InnerText;
            subNode = node.SelectSingleNode("EmptyLinesTicked");
            if (subNode != null)
                settings.CommonErrors.EmptyLinesTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("OverlappingDisplayTimeTicked");
            if (subNode != null)
                settings.CommonErrors.OverlappingDisplayTimeTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("TooShortDisplayTimeTicked");
            if (subNode != null)
                settings.CommonErrors.TooShortDisplayTimeTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("TooLongDisplayTimeTicked");
            if (subNode != null)
                settings.CommonErrors.TooLongDisplayTimeTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("InvalidItalicTagsTicked");
            if (subNode != null)
                settings.CommonErrors.InvalidItalicTagsTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("BreakLongLinesTicked");
            if (subNode != null)
                settings.CommonErrors.BreakLongLinesTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("MergeShortLinesTicked");
            if (subNode != null)
                settings.CommonErrors.MergeShortLinesTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("MergeShortLinesAllTicked");
            if (subNode != null)
                settings.CommonErrors.MergeShortLinesAllTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("UnneededSpacesTicked");
            if (subNode != null)
                settings.CommonErrors.UnneededSpacesTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("UnneededPeriodsTicked");
            if (subNode != null)
                settings.CommonErrors.UnneededPeriodsTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("MissingSpacesTicked");
            if (subNode != null)
                settings.CommonErrors.MissingSpacesTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("AddMissingQuotesTicked");
            if (subNode != null)
                settings.CommonErrors.AddMissingQuotesTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("Fix3PlusLinesTicked");
            if (subNode != null)
                settings.CommonErrors.Fix3PlusLinesTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("FixHyphensTicked");
            if (subNode != null)
                settings.CommonErrors.FixHyphensTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("FixHyphensAddTicked");
            if (subNode != null)
                settings.CommonErrors.FixHyphensAddTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("UppercaseIInsideLowercaseWordTicked");
            if (subNode != null)
                settings.CommonErrors.UppercaseIInsideLowercaseWordTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("DoubleApostropheToQuoteTicked");
            if (subNode != null)
                settings.CommonErrors.DoubleApostropheToQuoteTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("AddPeriodAfterParagraphTicked");
            if (subNode != null)
                settings.CommonErrors.AddPeriodAfterParagraphTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("StartWithUppercaseLetterAfterParagraphTicked");
            if (subNode != null)
                settings.CommonErrors.StartWithUppercaseLetterAfterParagraphTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("StartWithUppercaseLetterAfterPeriodInsideParagraphTicked");
            if (subNode != null)
                settings.CommonErrors.StartWithUppercaseLetterAfterPeriodInsideParagraphTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("StartWithUppercaseLetterAfterColonTicked");
            if (subNode != null)
                settings.CommonErrors.StartWithUppercaseLetterAfterColonTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("AloneLowercaseIToUppercaseIEnglishTicked");
            if (subNode != null)
                settings.CommonErrors.AloneLowercaseIToUppercaseIEnglishTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("FixOcrErrorsViaReplaceListTicked");
            if (subNode != null)
                settings.CommonErrors.FixOcrErrorsViaReplaceListTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("RemoveSpaceBetweenNumberTicked");
            if (subNode != null)
                settings.CommonErrors.RemoveSpaceBetweenNumberTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("FixDialogsOnOneLineTicked");
            if (subNode != null)
                settings.CommonErrors.FixDialogsOnOneLineTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("TurkishAnsiTicked");
            if (subNode != null)
                settings.CommonErrors.TurkishAnsiTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("DanishLetterITicked");
            if (subNode != null)
                settings.CommonErrors.DanishLetterITicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SpanishInvertedQuestionAndExclamationMarksTicked");
            if (subNode != null)
                settings.CommonErrors.SpanishInvertedQuestionAndExclamationMarksTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("FixDoubleDashTicked");
            if (subNode != null)
                settings.CommonErrors.FixDoubleDashTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("FixDoubleGreaterThanTicked");
            if (subNode != null)
                settings.CommonErrors.FixDoubleGreaterThanTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("FixEllipsesStartTicked");
            if (subNode != null)
                settings.CommonErrors.FixEllipsesStartTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("FixMissingOpenBracketTicked");
            if (subNode != null)
                settings.CommonErrors.FixMissingOpenBracketTicked = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("FixMusicNotationTicked");
            if (subNode != null)
                settings.CommonErrors.FixMusicNotationTicked = Convert.ToBoolean(subNode.InnerText);

            settings.VideoControls = new VideoControlsSettings();
            node = doc.DocumentElement.SelectSingleNode("VideoControls");
            subNode = node.SelectSingleNode("CustomSearchText1");
            if (subNode != null)
                settings.VideoControls.CustomSearchText1 = subNode.InnerText;
            subNode = node.SelectSingleNode("CustomSearchText2");
            if (subNode != null)
                settings.VideoControls.CustomSearchText2 = subNode.InnerText;
            subNode = node.SelectSingleNode("CustomSearchText3");
            if (subNode != null)
                settings.VideoControls.CustomSearchText3 = subNode.InnerText;
            subNode = node.SelectSingleNode("CustomSearchText4");
            if (subNode != null)
                settings.VideoControls.CustomSearchText4 = subNode.InnerText;
            subNode = node.SelectSingleNode("CustomSearchText5");
            if (subNode != null)
                settings.VideoControls.CustomSearchText5 = subNode.InnerText;
            subNode = node.SelectSingleNode("CustomSearchText6");
            if (subNode != null)
                settings.VideoControls.CustomSearchText6 = subNode.InnerText;
            subNode = node.SelectSingleNode("CustomSearchUrl1");
            if (subNode != null)
                settings.VideoControls.CustomSearchUrl1 = subNode.InnerText;
            subNode = node.SelectSingleNode("CustomSearchUrl1");
            if (subNode != null)
                settings.VideoControls.CustomSearchUrl1 = subNode.InnerText;
            subNode = node.SelectSingleNode("CustomSearchUrl2");
            if (subNode != null)
                settings.VideoControls.CustomSearchUrl2 = subNode.InnerText;
            subNode = node.SelectSingleNode("CustomSearchUrl3");
            if (subNode != null)
                settings.VideoControls.CustomSearchUrl3 = subNode.InnerText;
            subNode = node.SelectSingleNode("CustomSearchUrl4");
            if (subNode != null)
                settings.VideoControls.CustomSearchUrl4 = subNode.InnerText;
            subNode = node.SelectSingleNode("CustomSearchUrl5");
            if (subNode != null)
                settings.VideoControls.CustomSearchUrl5 = subNode.InnerText;
            subNode = node.SelectSingleNode("CustomSearchUrl6");
            if (subNode != null)
                settings.VideoControls.CustomSearchUrl6 = subNode.InnerText;
            subNode = node.SelectSingleNode("LastActiveTab");
            if (subNode != null)
                settings.VideoControls.LastActiveTab = subNode.InnerText;
            subNode = node.SelectSingleNode("WaveformDrawGrid");
            if (subNode != null)
                settings.VideoControls.WaveformDrawGrid = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("WaveformAllowOverlap");
            if (subNode != null)
                settings.VideoControls.WaveformAllowOverlap = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("WaveformFocusOnMouseEnter");
            if (subNode != null)
                settings.VideoControls.WaveformFocusOnMouseEnter = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("WaveformListViewFocusOnMouseEnter");
            if (subNode != null)
                settings.VideoControls.WaveformListViewFocusOnMouseEnter = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("WaveformBorderHitMs");
            if (subNode != null)
                settings.VideoControls.WaveformBorderHitMs = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("WaveformGridColor");
            if (subNode != null)
                settings.VideoControls.WaveformGridColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("WaveformColor");
            if (subNode != null)
                settings.VideoControls.WaveformColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("WaveformSelectedColor");
            if (subNode != null)
                settings.VideoControls.WaveformSelectedColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("WaveformBackgroundColor");
            if (subNode != null)
                settings.VideoControls.WaveformBackgroundColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("WaveformTextColor");
            if (subNode != null)
                settings.VideoControls.WaveformTextColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("WaveformDoubleClickOnNonParagraphAction");
            if (subNode != null)
                settings.VideoControls.WaveformDoubleClickOnNonParagraphAction = subNode.InnerText;
            subNode = node.SelectSingleNode("WaveformRightClickOnNonParagraphAction");
            if (subNode != null)
                settings.VideoControls.WaveformRightClickOnNonParagraphAction = subNode.InnerText;
            subNode = node.SelectSingleNode("WaveformMouseWheelScrollUpIsForward");
            if (subNode != null)
                settings.VideoControls.WaveformMouseWheelScrollUpIsForward = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("GenerateSpectrogram");
            if (subNode != null)
                settings.VideoControls.GenerateSpectrogram = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SpectrogramAppearance");
            if (subNode != null)
                settings.VideoControls.SpectrogramAppearance = subNode.InnerText;
            subNode = node.SelectSingleNode("WaveformMininumSampleRate");
            if (subNode != null)
                settings.VideoControls.WaveformMininumSampleRate = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("WaveformSeeksSilenceDurationSeconds");
            if (subNode != null)
                settings.VideoControls.WaveformSeeksSilenceDurationSeconds = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            subNode = node.SelectSingleNode("WaveformSeeksSilenceMaxVolume");
            if (subNode != null)
                settings.VideoControls.WaveformSeeksSilenceMaxVolume = Convert.ToInt32(subNode.InnerText);

            settings.NetworkSettings = new NetworkSettings();
            node = doc.DocumentElement.SelectSingleNode("NetworkSettings");
            if (node != null)
            {
                subNode = node.SelectSingleNode("SessionKey");
                if (subNode != null)
                    settings.NetworkSettings.SessionKey = subNode.InnerText;
                subNode = node.SelectSingleNode("UserName");
                if (subNode != null)
                    settings.NetworkSettings.UserName = subNode.InnerText;
                subNode = node.SelectSingleNode("WebServiceUrl");
                if (subNode != null)
                    settings.NetworkSettings.WebServiceUrl = subNode.InnerText;
                subNode = node.SelectSingleNode("PollIntervalSeconds");
                if (subNode != null)
                    settings.NetworkSettings.PollIntervalSeconds = Convert.ToInt32(subNode.InnerText);
            }

            settings.VobSubOcr = new VobSubOcrSettings();
            node = doc.DocumentElement.SelectSingleNode("VobSubOcr");
            subNode = node.SelectSingleNode("XOrMorePixelsMakesSpace");
            if (subNode != null)
                settings.VobSubOcr.XOrMorePixelsMakesSpace = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("AllowDifferenceInPercent");
            if (subNode != null)
                settings.VobSubOcr.AllowDifferenceInPercent = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            subNode = node.SelectSingleNode("BlurayAllowDifferenceInPercent");
            if (subNode != null)
                settings.VobSubOcr.BlurayAllowDifferenceInPercent = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            subNode = node.SelectSingleNode("LastImageCompareFolder");
            if (subNode != null)
                settings.VobSubOcr.LastImageCompareFolder = subNode.InnerText;
            subNode = node.SelectSingleNode("LastModiLanguageId");
            if (subNode != null)
                settings.VobSubOcr.LastModiLanguageId = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("LastOcrMethod");
            if (subNode != null)
                settings.VobSubOcr.LastOcrMethod = subNode.InnerText;
            subNode = node.SelectSingleNode("TesseractLastLanguage");
            if (subNode != null)
                settings.VobSubOcr.TesseractLastLanguage = subNode.InnerText;
            subNode = node.SelectSingleNode("UseModiInTesseractForUnknownWords");
            if (subNode != null)
                settings.VobSubOcr.UseModiInTesseractForUnknownWords = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("UseItalicsInTesseract");
            if (subNode != null)
                settings.VobSubOcr.UseItalicsInTesseract = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("UseMusicSymbolsInTesseract");
            if (subNode != null)
                settings.VobSubOcr.UseMusicSymbolsInTesseract = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("RightToLeft");
            if (subNode != null)
                settings.VobSubOcr.RightToLeft = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("TopToBottom");
            if (subNode != null)
                settings.VobSubOcr.TopToBottom = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("DefaultMillisecondsForUnknownDurations");
            if (subNode != null)
                settings.VobSubOcr.DefaultMillisecondsForUnknownDurations = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("PromptForUnknownWords");
            if (subNode != null)
                settings.VobSubOcr.PromptForUnknownWords = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("GuessUnknownWords");
            if (subNode != null)
                settings.VobSubOcr.GuessUnknownWords = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("AutoBreakSubtitleIfMoreThanTwoLines");
            if (subNode != null)
                settings.VobSubOcr.AutoBreakSubtitleIfMoreThanTwoLines = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("ItalicFactor");
            if (subNode != null)
                settings.VobSubOcr.ItalicFactor = Convert.ToDouble(subNode.InnerText, CultureInfo.InvariantCulture);
            subNode = node.SelectSingleNode("LineOcrDraw");
            if (subNode != null)
                settings.VobSubOcr.LineOcrDraw = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("LineOcrAdvancedItalic");
            if (subNode != null)
                settings.VobSubOcr.LineOcrAdvancedItalic = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("LineOcrLastLanguages");
            if (subNode != null)
                settings.VobSubOcr.LineOcrLastLanguages = subNode.InnerText;
            subNode = node.SelectSingleNode("LineOcrLastSpellCheck");
            if (subNode != null)
                settings.VobSubOcr.LineOcrLastSpellCheck = subNode.InnerText;
            subNode = node.SelectSingleNode("LineOcrXOrMorePixelsMakesSpace");
            if (subNode != null)
                settings.VobSubOcr.LineOcrXOrMorePixelsMakesSpace = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("LineOcrMinLineHeight");
            if (subNode != null)
                settings.VobSubOcr.LineOcrMinLineHeight = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("LineOcrMaxLineHeight");
            if (subNode != null)
                settings.VobSubOcr.LineOcrMaxLineHeight = Convert.ToInt32(subNode.InnerText);

            foreach (XmlNode listNode in doc.DocumentElement.SelectNodes("MultipleSearchAndReplaceList/MultipleSearchAndReplaceItem"))
            {
                MultipleSearchAndReplaceSetting item = new MultipleSearchAndReplaceSetting();
                subNode = listNode.SelectSingleNode("Enabled");
                if (subNode != null)
                    item.Enabled = Convert.ToBoolean(subNode.InnerText);
                subNode = listNode.SelectSingleNode("FindWhat");
                if (subNode != null)
                    item.FindWhat = subNode.InnerText;
                subNode = listNode.SelectSingleNode("ReplaceWith");
                if (subNode != null)
                    item.ReplaceWith = subNode.InnerText;
                subNode = listNode.SelectSingleNode("SearchType");
                if (subNode != null)
                    item.SearchType = subNode.InnerText;
                settings.MultipleSearchAndReplaceList.Add(item);
            }

            settings.Shortcuts = new Shortcuts();
            node = doc.DocumentElement.SelectSingleNode("Shortcuts");
            if (node != null)
            {
                subNode = node.SelectSingleNode("GeneralGoToFirstSelectedLine");
                if (subNode != null)
                    settings.Shortcuts.GeneralGoToFirstSelectedLine = subNode.InnerText;
                subNode = node.SelectSingleNode("GeneralGoToNextEmptyLine");
                if (subNode != null)
                    settings.Shortcuts.GeneralGoToNextEmptyLine = subNode.InnerText;
                subNode = node.SelectSingleNode("GeneralMergeSelectedLines");
                if (subNode != null)
                    settings.Shortcuts.GeneralMergeSelectedLines = subNode.InnerText;
                subNode = node.SelectSingleNode("GeneralMergeSelectedLinesOnlyFirstText");
                if (subNode != null)
                    settings.Shortcuts.GeneralMergeSelectedLinesOnlyFirstText = subNode.InnerText;
                subNode = node.SelectSingleNode("GeneralToggleTranslationMode");
                if (subNode != null)
                    settings.Shortcuts.GeneralToggleTranslationMode = subNode.InnerText;
                subNode = node.SelectSingleNode("GeneralSwitchOriginalAndTranslation");
                if (subNode != null)
                    settings.Shortcuts.GeneralSwitchOriginalAndTranslation = subNode.InnerText;
                subNode = node.SelectSingleNode("GeneralMergeOriginalAndTranslation");
                if (subNode != null)
                    settings.Shortcuts.GeneralMergeOriginalAndTranslation = subNode.InnerText;
                subNode = node.SelectSingleNode("GeneralGoToNextSubtitle");
                if (subNode != null)
                    settings.Shortcuts.GeneralGoToNextSubtitle = subNode.InnerText;
                subNode = node.SelectSingleNode("GeneralGoToPrevSubtitle");
                if (subNode != null)
                    settings.Shortcuts.GeneralGoToPrevSubtitle = subNode.InnerText;
                subNode = node.SelectSingleNode("GeneralGoToEndOfCurrentSubtitle");
                if (subNode != null)
                    settings.Shortcuts.GeneralGoToEndOfCurrentSubtitle = subNode.InnerText;
                subNode = node.SelectSingleNode("GeneralGoToStartOfCurrentSubtitle");
                if (subNode != null)
                    settings.Shortcuts.GeneralGoToStartOfCurrentSubtitle = subNode.InnerText;
                subNode = node.SelectSingleNode("GeneralPlayFirstSelected");
                if (subNode != null)
                    settings.Shortcuts.GeneralPlayFirstSelected = subNode.InnerText;
                subNode = node.SelectSingleNode("MainFileNew");
                if (subNode != null)
                    settings.Shortcuts.MainFileNew = subNode.InnerText;
                subNode = node.SelectSingleNode("MainFileOpen");
                if (subNode != null)
                    settings.Shortcuts.MainFileOpen = subNode.InnerText;
                subNode = node.SelectSingleNode("MainFileOpenKeepVideo");
                if (subNode != null)
                    settings.Shortcuts.MainFileOpenKeepVideo = subNode.InnerText;
                subNode = node.SelectSingleNode("MainFileSave");
                if (subNode != null)
                    settings.Shortcuts.MainFileSave = subNode.InnerText;
                subNode = node.SelectSingleNode("MainFileSaveOriginal");
                if (subNode != null)
                    settings.Shortcuts.MainFileSaveOriginal = subNode.InnerText;
                subNode = node.SelectSingleNode("MainFileSaveOriginalAs");
                if (subNode != null)
                    settings.Shortcuts.MainFileSaveOriginalAs = subNode.InnerText;
                subNode = node.SelectSingleNode("MainFileSaveAs");
                if (subNode != null)
                    settings.Shortcuts.MainFileSaveAs = subNode.InnerText;
                subNode = node.SelectSingleNode("MainFileSaveAll");
                if (subNode != null)
                    settings.Shortcuts.MainFileSaveAll = subNode.InnerText;
                subNode = node.SelectSingleNode("MainFileExportEbu");
                if (subNode != null)
                    settings.Shortcuts.MainFileExportEbu = subNode.InnerText;
                subNode = node.SelectSingleNode("MainEditUndo");
                if (subNode != null)
                    settings.Shortcuts.MainEditUndo = subNode.InnerText;
                subNode = node.SelectSingleNode("MainEditRedo");
                if (subNode != null)
                    settings.Shortcuts.MainEditRedo = subNode.InnerText;
                subNode = node.SelectSingleNode("MainEditFind");
                if (subNode != null)
                    settings.Shortcuts.MainEditFind = subNode.InnerText;
                subNode = node.SelectSingleNode("MainEditFindNext");
                if (subNode != null)
                    settings.Shortcuts.MainEditFindNext = subNode.InnerText;
                subNode = node.SelectSingleNode("MainEditReplace");
                if (subNode != null)
                    settings.Shortcuts.MainEditReplace = subNode.InnerText;
                subNode = node.SelectSingleNode("MainEditMultipleReplace");
                if (subNode != null)
                    settings.Shortcuts.MainEditMultipleReplace = subNode.InnerText;
                subNode = node.SelectSingleNode("MainEditGoToLineNumber");
                if (subNode != null)
                    settings.Shortcuts.MainEditGoToLineNumber = subNode.InnerText;
                subNode = node.SelectSingleNode("MainEditRightToLeft");
                if (subNode != null)
                    settings.Shortcuts.MainEditRightToLeft = subNode.InnerText;
                subNode = node.SelectSingleNode("MainToolsFixCommonErrors");
                if (subNode != null)
                    settings.Shortcuts.MainToolsFixCommonErrors = subNode.InnerText;
                subNode = node.SelectSingleNode("MainToolsFixCommonErrorsPreview");
                if (subNode != null)
                    settings.Shortcuts.MainToolsFixCommonErrorsPreview = subNode.InnerText;
                subNode = node.SelectSingleNode("MainToolsMergeShortLines");
                if (subNode != null)
                    settings.Shortcuts.MainToolsMergeShortLines = subNode.InnerText;
                subNode = node.SelectSingleNode("MainToolsSplitLongLines");
                if (subNode != null)
                    settings.Shortcuts.MainToolsSplitLongLines = subNode.InnerText;
                subNode = node.SelectSingleNode("MainToolsRenumber");
                if (subNode != null)
                    settings.Shortcuts.MainToolsRenumber = subNode.InnerText;
                subNode = node.SelectSingleNode("MainToolsRemoveTextForHI");
                if (subNode != null)
                    settings.Shortcuts.MainToolsRemoveTextForHI = subNode.InnerText;
                subNode = node.SelectSingleNode("MainToolsChangeCasing");
                if (subNode != null)
                    settings.Shortcuts.MainToolsChangeCasing = subNode.InnerText;
                subNode = node.SelectSingleNode("MainToolsAutoDuration");
                if (subNode != null)
                    settings.Shortcuts.MainToolsAutoDuration = subNode.InnerText;
                subNode = node.SelectSingleNode("MainToolsBatchConvert");
                if (subNode != null)
                    settings.Shortcuts.MainToolsBatchConvert = subNode.InnerText;
                subNode = node.SelectSingleNode("MainToolsBeamer");
                if (subNode != null)
                    settings.Shortcuts.MainToolsBeamer = subNode.InnerText;
                subNode = node.SelectSingleNode("MainToolsToggleTranslationOriginalInPreviews");
                if (subNode != null)
                    settings.Shortcuts.MainEditToggleTranslationOriginalInPreviews = subNode.InnerText;
                subNode = node.SelectSingleNode("MainEditInverseSelection");
                if (subNode != null)
                    settings.Shortcuts.MainEditInverseSelection = subNode.InnerText;
                subNode = node.SelectSingleNode("MainEditModifySelection");
                if (subNode != null)
                    settings.Shortcuts.MainEditModifySelection = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideoPause");
                if (subNode != null)
                    settings.Shortcuts.MainVideoPause = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideoPlayPauseToggle");
                if (subNode != null)
                    settings.Shortcuts.MainVideoPlayPauseToggle = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideoShowHideVideo");
                if (subNode != null)
                    settings.Shortcuts.MainVideoShowHideVideo = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideoToggleVideoControls");
                if (subNode != null)
                    settings.Shortcuts.MainVideoToggleVideoControls = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideo1FrameLeft");
                if (subNode != null)
                    settings.Shortcuts.MainVideo1FrameLeft = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideo1FrameRight");
                if (subNode != null)
                    settings.Shortcuts.MainVideo1FrameRight = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideo100MsLeft");
                if (subNode != null)
                    settings.Shortcuts.MainVideo100MsLeft = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideo100MsRight");
                if (subNode != null)
                    settings.Shortcuts.MainVideo100MsRight = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideo500MsLeft");
                if (subNode != null)
                    settings.Shortcuts.MainVideo500MsLeft = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideo500MsRight");
                if (subNode != null)
                    settings.Shortcuts.MainVideo500MsRight = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideo1000MsLeft");
                if (subNode != null)
                    settings.Shortcuts.MainVideo1000MsLeft = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideo1000MsRight");
                if (subNode != null)
                    settings.Shortcuts.MainVideo1000MsRight = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideoFullscreen");
                if (subNode != null)
                    settings.Shortcuts.MainVideoFullscreen = subNode.InnerText;
                subNode = node.SelectSingleNode("MainSpellCheck");
                if (subNode != null)
                    settings.Shortcuts.MainSpellCheck = subNode.InnerText;
                subNode = node.SelectSingleNode("MainSpellCheckFindDoubleWords");
                if (subNode != null)
                    settings.Shortcuts.MainSpellCheckFindDoubleWords = subNode.InnerText;
                subNode = node.SelectSingleNode("MainSpellCheckAddWordToNames");
                if (subNode != null)
                    settings.Shortcuts.MainSpellCheckAddWordToNames = subNode.InnerText;
                subNode = node.SelectSingleNode("MainSynchronizationAdjustTimes");
                if (subNode != null)
                    settings.Shortcuts.MainSynchronizationAdjustTimes = subNode.InnerText;
                subNode = node.SelectSingleNode("MainSynchronizationVisualSync");
                if (subNode != null)
                    settings.Shortcuts.MainSynchronizationVisualSync = subNode.InnerText;
                subNode = node.SelectSingleNode("MainSynchronizationPointSync");
                if (subNode != null)
                    settings.Shortcuts.MainSynchronizationPointSync = subNode.InnerText;
                subNode = node.SelectSingleNode("MainSynchronizationChangeFrameRate");
                if (subNode != null)
                    settings.Shortcuts.MainSynchronizationChangeFrameRate = subNode.InnerText;
                subNode = node.SelectSingleNode("MainListViewItalic");
                if (subNode != null)
                    settings.Shortcuts.MainListViewItalic = subNode.InnerText;
                subNode = node.SelectSingleNode("MainListViewToggleDashes");
                if (subNode != null)
                    settings.Shortcuts.MainListViewToggleDashes = subNode.InnerText;
                subNode = node.SelectSingleNode("MainListViewAlignment");
                if (subNode != null)
                    settings.Shortcuts.MainListViewAlignment = subNode.InnerText;
                subNode = node.SelectSingleNode("MainListViewCopyText");
                if (subNode != null)
                    settings.Shortcuts.MainListViewCopyText = subNode.InnerText;
                subNode = node.SelectSingleNode("MainListViewAutoDuration");
                if (subNode != null)
                    settings.Shortcuts.MainListViewAutoDuration = subNode.InnerText;
                subNode = node.SelectSingleNode("MainListViewColumnDeleteText");
                if (subNode != null)
                    settings.Shortcuts.MainListViewColumnDeleteText = subNode.InnerText;
                subNode = node.SelectSingleNode("MainListViewColumnInsertText");
                if (subNode != null)
                    settings.Shortcuts.MainListViewColumnInsertText = subNode.InnerText;
                subNode = node.SelectSingleNode("MainListViewColumnPaste");
                if (subNode != null)
                    settings.Shortcuts.MainListViewColumnPaste = subNode.InnerText;
                subNode = node.SelectSingleNode("MainListViewFocusWaveform");
                if (subNode != null)
                    settings.Shortcuts.MainListViewFocusWaveform = subNode.InnerText;
                subNode = node.SelectSingleNode("MainListViewGoToNextError");
                if (subNode != null)
                    settings.Shortcuts.MainListViewGoToNextError = subNode.InnerText;
                subNode = node.SelectSingleNode("MainEditReverseStartAndEndingForRTL");
                if (subNode != null)
                    settings.Shortcuts.MainEditReverseStartAndEndingForRTL = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTextBoxItalic");
                if (subNode != null)
                    settings.Shortcuts.MainTextBoxItalic = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTextBoxSplitAtCursor");
                if (subNode != null)
                    settings.Shortcuts.MainTextBoxSplitAtCursor = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTextBoxMoveLastWordDown");
                if (subNode != null)
                    settings.Shortcuts.MainTextBoxMoveLastWordDown = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTextBoxMoveFirstWordFromNextUp");
                if (subNode != null)
                    settings.Shortcuts.MainTextBoxMoveFirstWordFromNextUp = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTextBoxSelectionToLower");
                if (subNode != null)
                    settings.Shortcuts.MainTextBoxSelectionToLower = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTextBoxSelectionToUpper");
                if (subNode != null)
                    settings.Shortcuts.MainTextBoxSelectionToUpper = subNode.InnerText;
                subNode = node.SelectSingleNode("MainCreateInsertSubAtVideoPos");
                if (subNode != null)
                    settings.Shortcuts.MainCreateInsertSubAtVideoPos = subNode.InnerText;
                subNode = node.SelectSingleNode("MainCreatePlayFromJustBefore");
                if (subNode != null)
                    settings.Shortcuts.MainCreatePlayFromJustBefore = subNode.InnerText;
                subNode = node.SelectSingleNode("MainCreateSetStart");
                if (subNode != null)
                    settings.Shortcuts.MainCreateSetStart = subNode.InnerText;
                subNode = node.SelectSingleNode("MainCreateSetEnd");
                if (subNode != null)
                    settings.Shortcuts.MainCreateSetEnd = subNode.InnerText;
                subNode = node.SelectSingleNode("MainCreateSetEndAddNewAndGoToNew");
                if (subNode != null)
                    settings.Shortcuts.MainCreateSetEndAddNewAndGoToNew = subNode.InnerText;
                subNode = node.SelectSingleNode("MainCreateStartDownEndUp");
                if (subNode != null)
                    settings.Shortcuts.MainCreateStartDownEndUp = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustSetStartAndOffsetTheRest");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSetStartAndOffsetTheRest = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustSetEndAndOffsetTheRest");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSetEndAndOffsetTheRest = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustSetEndAndOffsetTheRestAndGoToNext");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSetEndAndOffsetTheRestAndGoToNext = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustSetEndAndGotoNext");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSetEndAndGotoNext = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustViaEndAutoStartAndGoToNext");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustViaEndAutoStartAndGoToNext = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustSetStartAutoDurationAndGoToNext");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSetStartAutoDurationAndGoToNext = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustSetEndNextStartAndGoToNext");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSetEndNextStartAndGoToNext = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustStartDownEndUpAndGoToNext");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustStartDownEndUpAndGoToNext = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustSetStart");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSetStart = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustSetStartKeepDuration");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSetStartKeepDuration = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustSetEnd");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSetEnd = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustSelected100MsForward");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSelected100MsForward = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustSelected100MsBack");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSelected100MsBack = subNode.InnerText;
                subNode = node.SelectSingleNode("MainInsertAfter");
                if (subNode != null)
                    settings.Shortcuts.MainInsertAfter = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTextBoxInsertAfter");
                if (subNode != null)
                    settings.Shortcuts.MainTextBoxInsertAfter = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTextBoxAutoBreak");
                if (subNode != null)
                    settings.Shortcuts.MainTextBoxAutoBreak = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTextBoxUnbreak");
                if (subNode != null)
                    settings.Shortcuts.MainTextBoxUnbreak = subNode.InnerText;
                subNode = node.SelectSingleNode("MainWaveformInsertAtCurrentPosition");
                if (subNode != null)
                    settings.Shortcuts.MainWaveformInsertAtCurrentPosition = subNode.InnerText;
                subNode = node.SelectSingleNode("MainInsertBefore");
                if (subNode != null)
                    settings.Shortcuts.MainInsertBefore = subNode.InnerText;
                subNode = node.SelectSingleNode("MainMergeDialog");
                if (subNode != null)
                    settings.Shortcuts.MainMergeDialog = subNode.InnerText;
                subNode = node.SelectSingleNode("MainToggleFocus");
                if (subNode != null)
                    settings.Shortcuts.MainToggleFocus = subNode.InnerText;
                subNode = node.SelectSingleNode("WaveformVerticalZoom");
                if (subNode != null)
                    settings.Shortcuts.WaveformVerticalZoom = subNode.InnerText;
                subNode = node.SelectSingleNode("WaveformVerticalZoomOut");
                if (subNode != null)
                    settings.Shortcuts.WaveformVerticalZoomOut = subNode.InnerText;
                subNode = node.SelectSingleNode("WaveformZoomIn");
                if (subNode != null)
                    settings.Shortcuts.WaveformZoomIn = subNode.InnerText;
                subNode = node.SelectSingleNode("WaveformZoomOut");
                if (subNode != null)
                    settings.Shortcuts.WaveformZoomOut = subNode.InnerText;
                subNode = node.SelectSingleNode("WaveformPlaySelection");
                if (subNode != null)
                    settings.Shortcuts.WaveformPlaySelection = subNode.InnerText;
                subNode = node.SelectSingleNode("WaveformSearchSilenceForward");
                if (subNode != null)
                    settings.Shortcuts.WaveformSearchSilenceForward = subNode.InnerText;
                subNode = node.SelectSingleNode("WaveformSearchSilenceBack");
                if (subNode != null)
                    settings.Shortcuts.WaveformSearchSilenceBack = subNode.InnerText;
                subNode = node.SelectSingleNode("WaveformAddTextHere");
                if (subNode != null)
                    settings.Shortcuts.WaveformAddTextHere = subNode.InnerText;
                subNode = node.SelectSingleNode("WaveformFocusListView");
                if (subNode != null)
                    settings.Shortcuts.WaveformFocusListView = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTranslateCustomSearch1");
                if (subNode != null)
                    settings.Shortcuts.MainTranslateCustomSearch1 = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTranslateCustomSearch2");
                if (subNode != null)
                    settings.Shortcuts.MainTranslateCustomSearch2 = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTranslateCustomSearch3");
                if (subNode != null)
                    settings.Shortcuts.MainTranslateCustomSearch3 = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTranslateCustomSearch4");
                if (subNode != null)
                    settings.Shortcuts.MainTranslateCustomSearch4 = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTranslateCustomSearch5");
                if (subNode != null)
                    settings.Shortcuts.MainTranslateCustomSearch5 = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTranslateCustomSearch6");
                if (subNode != null)
                    settings.Shortcuts.MainTranslateCustomSearch6 = subNode.InnerText;
            }

            settings.RemoveTextForHearingImpaired = new RemoveTextForHearingImpairedSettings();
            node = doc.DocumentElement.SelectSingleNode("RemoveTextForHearingImpaired");
            if (node != null)
            {
                subNode = node.SelectSingleNode("RemoveTextBetweenBrackets");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenBrackets = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("RemoveTextBetweenParentheses");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenParentheses = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("RemoveTextBetweenCurlyBrackets");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenCurlyBrackets = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("RemoveTextBetweenQuestionMarks");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenQuestionMarks = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("RemoveTextBetweenCustom");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustom = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("RemoveTextBetweenCustomBefore");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomBefore = subNode.InnerText;
                subNode = node.SelectSingleNode("RemoveTextBetweenCustomAfter");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomAfter = subNode.InnerText;
                subNode = node.SelectSingleNode("RemoveTextBetweenOnlySeperateLines");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveTextBetweenOnlySeperateLines = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("RemoveTextBeforeColon");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveTextBeforeColon = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("RemoveTextBeforeColonOnlyIfUppercase");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyIfUppercase = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("RemoveTextBeforeColonOnlyOnSeparateLine");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyOnSeparateLine = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("RemoveIfAllUppercase");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveIfAllUppercase = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("RemoveInterjections");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveInterjections = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("RemoveIfContains");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveIfContains = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("RemoveIfContainsText");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveIfContainsText = subNode.InnerText;
            }

            settings.SubtitleBeaming = new SubtitleBeaming();
            node = doc.DocumentElement.SelectSingleNode("SubtitleBeaming");
            if (node != null)
            {
                subNode = node.SelectSingleNode("FontName");
                if (subNode != null)
                    settings.SubtitleBeaming.FontName = subNode.InnerText;
                subNode = node.SelectSingleNode("FontColor");
                if (subNode != null)
                    settings.SubtitleBeaming.FontColor = Color.FromArgb(Convert.ToInt32(subNode.InnerText));
                subNode = node.SelectSingleNode("FontSize");
                if (subNode != null)
                    settings.SubtitleBeaming.FontSize = Convert.ToInt32(subNode.InnerText);
                subNode = node.SelectSingleNode("BorderColor");
                if (subNode != null)
                    settings.SubtitleBeaming.BorderColor = Color.FromArgb(Convert.ToInt32(subNode.InnerText));
                subNode = node.SelectSingleNode("BorderWidth");
                if (subNode != null)
                    settings.SubtitleBeaming.BorderWidth = Convert.ToInt32(subNode.InnerText);
            }

            return settings;
        }

        private static void CustomSerialize(string fileName, Settings settings)
        {
            var sw = new StringWriter();
            using (var textWriter = new XmlTextWriter(sw) { Formatting = Formatting.Indented })
            {
                textWriter.WriteStartDocument();

                textWriter.WriteStartElement("Settings", "");

                textWriter.WriteStartElement("RecentFiles", "");
                textWriter.WriteStartElement("FileNames", "");
                foreach (var item in settings.RecentFiles.Files)
                {
                    textWriter.WriteStartElement("FileName");
                    if (item.OriginalFileName != null)
                        textWriter.WriteAttributeString("OriginalFileName", item.OriginalFileName);
                    if (item.VideoFileName != null)
                        textWriter.WriteAttributeString("VideoFileName", item.VideoFileName);
                    textWriter.WriteAttributeString("FirstVisibleIndex", item.FirstVisibleIndex.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteAttributeString("FirstSelectedIndex", item.FirstSelectedIndex.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteString(item.FileName);
                    textWriter.WriteEndElement();
                }
                textWriter.WriteEndElement();
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("General", "");
                textWriter.WriteElementString("ShowToolbarNew", settings.General.ShowToolbarNew.ToString());
                textWriter.WriteElementString("ShowToolbarOpen", settings.General.ShowToolbarOpen.ToString());
                textWriter.WriteElementString("ShowToolbarSave", settings.General.ShowToolbarSave.ToString());
                textWriter.WriteElementString("ShowToolbarSaveAs", settings.General.ShowToolbarSaveAs.ToString());
                textWriter.WriteElementString("ShowToolbarFind", settings.General.ShowToolbarFind.ToString());
                textWriter.WriteElementString("ShowToolbarReplace", settings.General.ShowToolbarReplace.ToString());
                textWriter.WriteElementString("ShowToolbarFixCommonErrors", settings.General.ShowToolbarFixCommonErrors.ToString());
                textWriter.WriteElementString("ShowToolbarVisualSync", settings.General.ShowToolbarVisualSync.ToString());
                textWriter.WriteElementString("ShowToolbarSpellCheck", settings.General.ShowToolbarSpellCheck.ToString());
                textWriter.WriteElementString("ShowToolbarSettings", settings.General.ShowToolbarSettings.ToString());
                textWriter.WriteElementString("ShowToolbarHelp", settings.General.ShowToolbarHelp.ToString());
                textWriter.WriteElementString("ShowFrameRate", settings.General.ShowFrameRate.ToString());
                textWriter.WriteElementString("ShowVideoPlayer", settings.General.ShowVideoPlayer.ToString());
                textWriter.WriteElementString("ShowAudioVisualizer", settings.General.ShowAudioVisualizer.ToString());
                textWriter.WriteElementString("ShowWaveform", settings.General.ShowWaveform.ToString());
                textWriter.WriteElementString("ShowSpectrogram", settings.General.ShowSpectrogram.ToString());
                textWriter.WriteElementString("DefaultFrameRate", settings.General.DefaultFrameRate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DefaultSubtitleFormat", settings.General.DefaultSubtitleFormat);
                textWriter.WriteElementString("DefaultEncoding", settings.General.DefaultEncoding);
                textWriter.WriteElementString("AutoConvertToUtf8", settings.General.AutoConvertToUtf8.ToString());
                textWriter.WriteElementString("AutoGuessAnsiEncoding", settings.General.AutoGuessAnsiEncoding.ToString());
                textWriter.WriteElementString("_subtitleFontName", settings.General.SubtitleFontName);
                textWriter.WriteElementString("SubtitleFontSize", settings.General.SubtitleFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleFontBold", settings.General.SubtitleFontBold.ToString());
                textWriter.WriteElementString("SubtitleFontColor", settings.General.SubtitleFontColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleBackgroundColor", settings.General.SubtitleBackgroundColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("CenterSubtitleInTextBox", settings.General.CenterSubtitleInTextBox.ToString());
                textWriter.WriteElementString("ShowRecentFiles", settings.General.ShowRecentFiles.ToString());
                textWriter.WriteElementString("RememberSelectedLine", settings.General.RememberSelectedLine.ToString());
                textWriter.WriteElementString("StartLoadLastFile", settings.General.StartLoadLastFile.ToString());
                textWriter.WriteElementString("StartRememberPositionAndSize", settings.General.StartRememberPositionAndSize.ToString());
                textWriter.WriteElementString("StartPosition", settings.General.StartPosition);
                textWriter.WriteElementString("StartSize", settings.General.StartSize);
                textWriter.WriteElementString("SplitContainerMainSplitterDistance", settings.General.SplitContainerMainSplitterDistance.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitContainer1SplitterDistance", settings.General.SplitContainer1SplitterDistance.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitContainerListViewAndTextSplitterDistance", settings.General.SplitContainerListViewAndTextSplitterDistance.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("StartInSourceView", settings.General.StartInSourceView.ToString());
                textWriter.WriteElementString("RemoveBlankLinesWhenOpening", settings.General.RemoveBlankLinesWhenOpening.ToString());
                textWriter.WriteElementString("SubtitleLineMaximumLength", settings.General.SubtitleLineMaximumLength.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleMinimumDisplayMilliseconds", settings.General.SubtitleMinimumDisplayMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleMaximumDisplayMilliseconds", settings.General.SubtitleMaximumDisplayMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MininumMillisecondsBetweenLines", settings.General.MininumMillisecondsBetweenLines.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SetStartEndHumanDelay", settings.General.SetStartEndHumanDelay.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoWrapLineWhileTyping", settings.General.AutoWrapLineWhileTyping.ToString());
                textWriter.WriteElementString("SubtitleMaximumCharactersPerSeconds", settings.General.SubtitleMaximumCharactersPerSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SubtitleOptimalCharactersPerSeconds", settings.General.SubtitleOptimalCharactersPerSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpellCheckLanguage", settings.General.SpellCheckLanguage);
                textWriter.WriteElementString("VideoPlayer", settings.General.VideoPlayer);
                textWriter.WriteElementString("VideoPlayerDefaultVolume", settings.General.VideoPlayerDefaultVolume.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VideoPlayerPreviewFontSize", settings.General.VideoPlayerPreviewFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VideoPlayerPreviewFontBold", settings.General.VideoPlayerPreviewFontBold.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VideoPlayerShowStopButton", settings.General.VideoPlayerShowStopButton.ToString());
                textWriter.WriteElementString("VideoPlayerShowStopMute", settings.General.VideoPlayerShowMuteButton.ToString());
                textWriter.WriteElementString("VideoPlayerShowStopFullscreen", settings.General.VideoPlayerShowFullscreenButton.ToString());
                textWriter.WriteElementString("Language", settings.General.Language);
                textWriter.WriteElementString("ListViewLineSeparatorString", settings.General.ListViewLineSeparatorString);
                textWriter.WriteElementString("ListViewDoubleClickAction", settings.General.ListViewDoubleClickAction.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("UppercaseLetters", settings.General.UppercaseLetters);
                textWriter.WriteElementString("DefaultAdjustMilliseconds", settings.General.DefaultAdjustMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoRepeatOn", settings.General.AutoRepeatOn.ToString());
                textWriter.WriteElementString("AutoRepeatCount", settings.General.AutoRepeatCount.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AutoContinueOn", settings.General.AutoContinueOn.ToString());
                textWriter.WriteElementString("SyncListViewWithVideoWhilePlaying", settings.General.SyncListViewWithVideoWhilePlaying.ToString());
                textWriter.WriteElementString("AutoBackupSeconds", settings.General.AutoBackupSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SpellChecker", settings.General.SpellChecker);
                textWriter.WriteElementString("AllowEditOfOriginalSubtitle", settings.General.AllowEditOfOriginalSubtitle.ToString());
                textWriter.WriteElementString("PromptDeleteLines", settings.General.PromptDeleteLines.ToString());
                textWriter.WriteElementString("Undocked", settings.General.Undocked.ToString());
                textWriter.WriteElementString("UndockedVideoPosition", settings.General.UndockedVideoPosition);
                textWriter.WriteElementString("UndockedWaveformPosition", settings.General.UndockedWaveformPosition);
                textWriter.WriteElementString("UndockedVideoControlsPosition", settings.General.UndockedVideoControlsPosition);
                textWriter.WriteElementString("WaveformCenter", settings.General.WaveformCenter.ToString());
                textWriter.WriteElementString("SmallDelayMilliseconds", settings.General.SmallDelayMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LargeDelayMilliseconds", settings.General.LargeDelayMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ShowOriginalAsPreviewIfAvailable", settings.General.ShowOriginalAsPreviewIfAvailable.ToString());
                textWriter.WriteElementString("LastPacCodePage", settings.General.LastPacCodePage.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("OpenSubtitleExtraExtensions", settings.General.OpenSubtitleExtraExtensions);
                textWriter.WriteElementString("ListViewColumnsRememberSize", settings.General.ListViewColumnsRememberSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewNumberWidth", settings.General.ListViewNumberWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewStartWidth", settings.General.ListViewStartWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewEndWidth", settings.General.ListViewEndWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewDurationWidth", settings.General.ListViewDurationWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewTextWidth", settings.General.ListViewTextWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VlcWaveTranscodeSettings", settings.General.VlcWaveTranscodeSettings);
                textWriter.WriteElementString("VlcLocation", settings.General.VlcLocation);
                textWriter.WriteElementString("VlcLocationRelative", settings.General.VlcLocationRelative);
                textWriter.WriteElementString("MpcHcLocation", settings.General.MpcHcLocation);
                textWriter.WriteElementString("UseFFmpegForWaveExtraction", settings.General.UseFFmpegForWaveExtraction.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FFmpegLocation", settings.General.FFmpegLocation);
                textWriter.WriteElementString("UseTimeFormatHHMMSSFF", settings.General.UseTimeFormatHHMMSSFF.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ClearStatusBarAfterSeconds", settings.General.ClearStatusBarAfterSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Company", settings.General.Company);
                textWriter.WriteElementString("MoveVideo100Or500MsPlaySmallSample", settings.General.MoveVideo100Or500MsPlaySmallSample.ToString());
                textWriter.WriteElementString("DisableVideoAutoLoading", settings.General.DisableVideoAutoLoading.ToString());
                textWriter.WriteElementString("RightToLeftMode", settings.General.RightToLeftMode.ToString());
                textWriter.WriteElementString("LastSaveAsFormat", settings.General.LastSaveAsFormat);
                textWriter.WriteElementString("CheckForUpdates", settings.General.CheckForUpdates.ToString());
                textWriter.WriteElementString("LastCheckForUpdates", settings.General.LastCheckForUpdates.ToString("yyyy-MM-dd"));
                textWriter.WriteElementString("ShowBetaStuff", settings.General.ShowBetaStuff.ToString());
                textWriter.WriteElementString("NewEmptyDefaultMs", settings.General.NewEmptyDefaultMs.ToString(CultureInfo.InvariantCulture));

                textWriter.WriteEndElement();

                textWriter.WriteStartElement("Tools", "");
                textWriter.WriteElementString("StartSceneIndex", settings.Tools.StartSceneIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("EndSceneIndex", settings.Tools.EndSceneIndex.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("VerifyPlaySeconds", settings.Tools.VerifyPlaySeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("MergeLinesShorterThan", settings.Tools.MergeLinesShorterThan.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixShortDisplayTimesAllowMoveStartTime", settings.Tools.FixShortDisplayTimesAllowMoveStartTime.ToString());
                textWriter.WriteElementString("MusicSymbol", settings.Tools.MusicSymbol);
                textWriter.WriteElementString("MusicSymbolToReplace", settings.Tools.MusicSymbolToReplace);
                textWriter.WriteElementString("UnicodeSymbolsToInsert", settings.Tools.UnicodeSymbolsToInsert);
                textWriter.WriteElementString("SpellCheckAutoChangeNames", settings.Tools.SpellCheckAutoChangeNames.ToString());
                textWriter.WriteElementString("SpellCheckOneLetterWords", settings.Tools.SpellCheckOneLetterWords.ToString());
                textWriter.WriteElementString("SpellCheckEnglishAllowInQuoteAsIng", settings.Tools.SpellCheckEnglishAllowInQuoteAsIng.ToString());
                textWriter.WriteElementString("SpellCheckShowCompletedMessage", settings.Tools.SpellCheckShowCompletedMessage.ToString());
                textWriter.WriteElementString("OcrFixUseHardcodedRules", settings.Tools.OcrFixUseHardcodedRules.ToString());
                textWriter.WriteElementString("Interjections", settings.Tools.Interjections);
                textWriter.WriteElementString("MicrosoftBingApiId", settings.Tools.MicrosoftBingApiId);
                textWriter.WriteElementString("GoogleApiKey", settings.Tools.GoogleApiKey);
                textWriter.WriteElementString("UseGooleApiPaidService", settings.Tools.UseGooleApiPaidService.ToString());
                textWriter.WriteElementString("GoogleTranslateLastTargetLanguage", settings.Tools.GoogleTranslateLastTargetLanguage);
                textWriter.WriteElementString("ListViewSyntaxColorDurationSmall", settings.Tools.ListViewSyntaxColorDurationSmall.ToString());
                textWriter.WriteElementString("ListViewSyntaxColorDurationBig", settings.Tools.ListViewSyntaxColorDurationBig.ToString());
                textWriter.WriteElementString("ListViewSyntaxColorLongLines", settings.Tools.ListViewSyntaxColorLongLines.ToString());
                textWriter.WriteElementString("ListViewSyntaxMoreThanXLines", settings.Tools.ListViewSyntaxMoreThanXLines.ToString());
                textWriter.WriteElementString("ListViewSyntaxMoreThanXLinesX", settings.Tools.ListViewSyntaxMoreThanXLinesX.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewSyntaxColorOverlap", settings.Tools.ListViewSyntaxColorOverlap.ToString());
                textWriter.WriteElementString("ListViewSyntaxErrorColor", settings.Tools.ListViewSyntaxErrorColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ListViewUnfocusedSelectedColor", settings.Tools.ListViewUnfocusedSelectedColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitAdvanced", settings.Tools.SplitAdvanced.ToString());
                textWriter.WriteElementString("SplitOutputFolder", settings.Tools.SplitOutputFolder);
                textWriter.WriteElementString("SplitNumberOfParts", settings.Tools.SplitNumberOfParts.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SplitVia", settings.Tools.SplitVia);
                textWriter.WriteElementString("NewEmptyTranslationText", settings.Tools.NewEmptyTranslationText);
                textWriter.WriteElementString("BatchConvertOutputFolder", settings.Tools.BatchConvertOutputFolder);
                textWriter.WriteElementString("BatchConvertOverwriteExisting", settings.Tools.BatchConvertOverwriteExisting.ToString());
                textWriter.WriteElementString("BatchConvertOverwriteOriginal", settings.Tools.BatchConvertOverwriteOriginal.ToString());
                textWriter.WriteElementString("BatchConvertRemoveFormatting", settings.Tools.BatchConvertRemoveFormatting.ToString());
                textWriter.WriteElementString("BatchConvertFixCasing", settings.Tools.BatchConvertFixCasing.ToString());
                textWriter.WriteElementString("BatchConvertRemoveTextForHI", settings.Tools.BatchConvertRemoveTextForHI.ToString());
                textWriter.WriteElementString("BatchConvertSplitLongLines", settings.Tools.BatchConvertSplitLongLines.ToString());
                textWriter.WriteElementString("BatchConvertFixCommonErrors", settings.Tools.BatchConvertFixCommonErrors.ToString());
                textWriter.WriteElementString("BatchConvertMultipleReplace", settings.Tools.BatchConvertMultipleReplace.ToString());
                textWriter.WriteElementString("BatchConvertAutoBalance", settings.Tools.BatchConvertAutoBalance.ToString());
                textWriter.WriteElementString("BatchConvertSetMinDisplayTimeBetweenSubtitles", settings.Tools.BatchConvertSetMinDisplayTimeBetweenSubtitles.ToString());
                textWriter.WriteElementString("BatchConvertLanguage", settings.Tools.BatchConvertLanguage);
                textWriter.WriteElementString("ModifySelectionRule", settings.Tools.ModifySelectionRule);
                textWriter.WriteElementString("ModifySelectionText", settings.Tools.ModifySelectionText);
                textWriter.WriteElementString("ModifySelectionCaseSensitive", settings.Tools.ModifySelectionCaseSensitive.ToString());
                textWriter.WriteElementString("ExportVobSubFontName", settings.Tools.ExportVobSubFontName);
                textWriter.WriteElementString("ExportVobSubFontSize", settings.Tools.ExportVobSubFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportVobSubVideoResolution", settings.Tools.ExportVobSubVideoResolution);
                textWriter.WriteElementString("ExportVobSubLanguage", settings.Tools.ExportVobSubLanguage);
                textWriter.WriteElementString("ExportVobSubSimpleRendering", settings.Tools.ExportVobSubSimpleRendering.ToString());
                textWriter.WriteElementString("ExportVobAntiAliasingWithTransparency", settings.Tools.ExportVobAntiAliasingWithTransparency.ToString());
                textWriter.WriteElementString("ExportBluRayFontName", settings.Tools.ExportBluRayFontName);
                textWriter.WriteElementString("ExportBluRayFontSize", settings.Tools.ExportBluRayFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportFcpFontName", settings.Tools.ExportFcpFontName);
                textWriter.WriteElementString("ExportFontNameOther", settings.Tools.ExportFontNameOther);
                textWriter.WriteElementString("ExportFcpFontSize", settings.Tools.ExportFcpFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportFcpImageType", settings.Tools.ExportFcpImageType);
                textWriter.WriteElementString("ExportLastFontSize", settings.Tools.ExportLastFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportLastLineHeight", settings.Tools.ExportLastLineHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportLastBorderWidth", settings.Tools.ExportLastBorderWidth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportLastFontBold", settings.Tools.ExportLastFontBold.ToString());
                textWriter.WriteElementString("ExportBluRayVideoResolution", settings.Tools.ExportBluRayVideoResolution);
                textWriter.WriteElementString("ExportFontColor", settings.Tools.ExportFontColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportBorderColor", settings.Tools.ExportBorderColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportShadowColor", settings.Tools.ExportShadowColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportBottomMargin", settings.Tools.ExportBottomMargin.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportHorizontalAlignment", settings.Tools.ExportHorizontalAlignment.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportBluRayBottomMargin", settings.Tools.ExportBluRayBottomMargin.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportBluRayShadow", settings.Tools.ExportBluRayShadow.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Export3DType", settings.Tools.Export3DType.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("Export3DDepth", settings.Tools.Export3DDepth.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportLastShadowTransparency", settings.Tools.ExportLastShadowTransparency.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportLastFrameRate", settings.Tools.ExportLastFrameRate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FixCommonErrorsFixOverlapAllowEqualEndStart", settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart.ToString());
                textWriter.WriteElementString("ImportTextSplitting", settings.Tools.ImportTextSplitting);
                textWriter.WriteElementString("ImportTextMergeShortLines", settings.Tools.ImportTextMergeShortLines.ToString());
                textWriter.WriteElementString("GenerateTimeCodePatterns", settings.Tools.GenerateTimeCodePatterns);
                textWriter.WriteElementString("MusicSymbolStyle", settings.Tools.MusicSymbolStyle);
                textWriter.WriteElementString("BridgeGapMilliseconds", settings.Tools.BridgeGapMilliseconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("ExportCustomTemplates", settings.Tools.ExportCustomTemplates);
                textWriter.WriteElementString("ChangeCasingChoice", settings.Tools.ChangeCasingChoice);
                textWriter.WriteElementString("UseNoLineBreakAfter", settings.Tools.UseNoLineBreakAfter.ToString());
                textWriter.WriteElementString("NoLineBreakAfterEnglish", settings.Tools.NoLineBreakAfterEnglish);

                textWriter.WriteEndElement();

                textWriter.WriteStartElement("SubtitleSettings", "");
                textWriter.WriteElementString("SsaFontName", settings.SubtitleSettings.SsaFontName);
                textWriter.WriteElementString("SsaFontSize", settings.SubtitleSettings.SsaFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SsaFontColorArgb", settings.SubtitleSettings.SsaFontColorArgb.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SsaOutline", settings.SubtitleSettings.SsaOutline.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SsaShadow", settings.SubtitleSettings.SsaShadow.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SsaOpaqueBox", settings.SubtitleSettings.SsaOpaqueBox.ToString());
                textWriter.WriteElementString("DCinemaFontFile", settings.SubtitleSettings.DCinemaFontFile);
                textWriter.WriteElementString("DCinemaFontSize", settings.SubtitleSettings.DCinemaFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DCinemaBottomMargin", settings.SubtitleSettings.DCinemaBottomMargin.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DCinemaZPosition", settings.SubtitleSettings.DCinemaZPosition.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DCinemaFadeUpTime", settings.SubtitleSettings.DCinemaFadeUpTime.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("DCinemaFadeDownTime", settings.SubtitleSettings.DCinemaFadeDownTime.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("SamiDisplayTwoClassesAsTwoSubtitles", settings.SubtitleSettings.SamiDisplayTwoClassesAsTwoSubtitles.ToString());
                textWriter.WriteElementString("SamiFullHtmlEncode", settings.SubtitleSettings.SamiHtmlEncodeMode.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("TimedText10TimeCodeFormat", settings.SubtitleSettings.TimedText10TimeCodeFormat);
                textWriter.WriteElementString("FcpFontSize", settings.SubtitleSettings.FcpFontSize.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("FcpFontName", settings.SubtitleSettings.FcpFontName);
                textWriter.WriteElementString("CheetahCaptionAlwayWriteEndTime", settings.SubtitleSettings.CheetahCaptionAlwayWriteEndTime.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("NuendoCharacterListFile", settings.SubtitleSettings.NuendoCharacterListFile);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("Proxy", "");
                textWriter.WriteElementString("ProxyAddress", settings.Proxy.ProxyAddress);
                textWriter.WriteElementString("UserName", settings.Proxy.UserName);
                textWriter.WriteElementString("Password", settings.Proxy.Password);
                textWriter.WriteElementString("Domain", settings.Proxy.Domain);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("WordLists", "");
                textWriter.WriteElementString("LastLanguage", settings.WordLists.LastLanguage);
                textWriter.WriteElementString("NamesEtcUrl", settings.WordLists.NamesEtcUrl);
                textWriter.WriteElementString("UseOnlineNamesEtc", settings.WordLists.UseOnlineNamesEtc.ToString());
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("CommonErrors", "");
                textWriter.WriteElementString("StartPosition", settings.CommonErrors.StartPosition);
                textWriter.WriteElementString("StartSize", settings.CommonErrors.StartSize);
                textWriter.WriteElementString("EmptyLinesTicked", settings.CommonErrors.EmptyLinesTicked.ToString());
                textWriter.WriteElementString("OverlappingDisplayTimeTicked", settings.CommonErrors.OverlappingDisplayTimeTicked.ToString());
                textWriter.WriteElementString("TooShortDisplayTimeTicked", settings.CommonErrors.TooShortDisplayTimeTicked.ToString());
                textWriter.WriteElementString("TooLongDisplayTimeTicked", settings.CommonErrors.TooLongDisplayTimeTicked.ToString());
                textWriter.WriteElementString("InvalidItalicTagsTicked", settings.CommonErrors.InvalidItalicTagsTicked.ToString());
                textWriter.WriteElementString("BreakLongLinesTicked", settings.CommonErrors.BreakLongLinesTicked.ToString());
                textWriter.WriteElementString("MergeShortLinesTicked", settings.CommonErrors.MergeShortLinesTicked.ToString());
                textWriter.WriteElementString("MergeShortLinesAllTicked", settings.CommonErrors.MergeShortLinesAllTicked.ToString());
                textWriter.WriteElementString("UnneededSpacesTicked", settings.CommonErrors.UnneededSpacesTicked.ToString());
                textWriter.WriteElementString("UnneededPeriodsTicked", settings.CommonErrors.UnneededPeriodsTicked.ToString());
                textWriter.WriteElementString("MissingSpacesTicked", settings.CommonErrors.MissingSpacesTicked.ToString());
                textWriter.WriteElementString("AddMissingQuotesTicked", settings.CommonErrors.AddMissingQuotesTicked.ToString());
                textWriter.WriteElementString("Fix3PlusLinesTicked", settings.CommonErrors.Fix3PlusLinesTicked.ToString());
                textWriter.WriteElementString("FixHyphensTicked", settings.CommonErrors.FixHyphensTicked.ToString());
                textWriter.WriteElementString("FixHyphensAddTicked", settings.CommonErrors.FixHyphensAddTicked.ToString());
                textWriter.WriteElementString("UppercaseIInsideLowercaseWordTicked", settings.CommonErrors.UppercaseIInsideLowercaseWordTicked.ToString());
                textWriter.WriteElementString("DoubleApostropheToQuoteTicked", settings.CommonErrors.DoubleApostropheToQuoteTicked.ToString());
                textWriter.WriteElementString("AddPeriodAfterParagraphTicked", settings.CommonErrors.AddPeriodAfterParagraphTicked.ToString());
                textWriter.WriteElementString("StartWithUppercaseLetterAfterParagraphTicked", settings.CommonErrors.StartWithUppercaseLetterAfterParagraphTicked.ToString());
                textWriter.WriteElementString("StartWithUppercaseLetterAfterPeriodInsideParagraphTicked", settings.CommonErrors.StartWithUppercaseLetterAfterPeriodInsideParagraphTicked.ToString());
                textWriter.WriteElementString("StartWithUppercaseLetterAfterColonTicked", settings.CommonErrors.StartWithUppercaseLetterAfterColonTicked.ToString());
                textWriter.WriteElementString("AloneLowercaseIToUppercaseIEnglishTicked", settings.CommonErrors.AloneLowercaseIToUppercaseIEnglishTicked.ToString());
                textWriter.WriteElementString("FixOcrErrorsViaReplaceListTicked", settings.CommonErrors.FixOcrErrorsViaReplaceListTicked.ToString());
                textWriter.WriteElementString("RemoveSpaceBetweenNumberTicked", settings.CommonErrors.RemoveSpaceBetweenNumberTicked.ToString());
                textWriter.WriteElementString("FixDialogsOnOneLineTicked", settings.CommonErrors.FixDialogsOnOneLineTicked.ToString());
                textWriter.WriteElementString("TurkishAnsiTicked", settings.CommonErrors.TurkishAnsiTicked.ToString());
                textWriter.WriteElementString("DanishLetterITicked", settings.CommonErrors.DanishLetterITicked.ToString());
                textWriter.WriteElementString("SpanishInvertedQuestionAndExclamationMarksTicked", settings.CommonErrors.SpanishInvertedQuestionAndExclamationMarksTicked.ToString());
                textWriter.WriteElementString("FixDoubleDashTicked", settings.CommonErrors.FixDoubleDashTicked.ToString());
                textWriter.WriteElementString("FixDoubleGreaterThanTicked", settings.CommonErrors.FixDoubleGreaterThanTicked.ToString());
                textWriter.WriteElementString("FixEllipsesStartTicked", settings.CommonErrors.FixEllipsesStartTicked.ToString());
                textWriter.WriteElementString("FixMissingOpenBracketTicked", settings.CommonErrors.FixMissingOpenBracketTicked.ToString());
                textWriter.WriteElementString("FixMusicNotationTicked", settings.CommonErrors.FixMusicNotationTicked.ToString());
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("VideoControls", "");
                textWriter.WriteElementString("CustomSearchText1", settings.VideoControls.CustomSearchText1);
                textWriter.WriteElementString("CustomSearchText2", settings.VideoControls.CustomSearchText2);
                textWriter.WriteElementString("CustomSearchText3", settings.VideoControls.CustomSearchText3);
                textWriter.WriteElementString("CustomSearchText4", settings.VideoControls.CustomSearchText4);
                textWriter.WriteElementString("CustomSearchText5", settings.VideoControls.CustomSearchText5);
                textWriter.WriteElementString("CustomSearchText6", settings.VideoControls.CustomSearchText6);
                textWriter.WriteElementString("CustomSearchUrl1", settings.VideoControls.CustomSearchUrl1);
                textWriter.WriteElementString("CustomSearchUrl2", settings.VideoControls.CustomSearchUrl2);
                textWriter.WriteElementString("CustomSearchUrl3", settings.VideoControls.CustomSearchUrl3);
                textWriter.WriteElementString("CustomSearchUrl4", settings.VideoControls.CustomSearchUrl4);
                textWriter.WriteElementString("CustomSearchUrl5", settings.VideoControls.CustomSearchUrl5);
                textWriter.WriteElementString("CustomSearchUrl6", settings.VideoControls.CustomSearchUrl6);
                textWriter.WriteElementString("LastActiveTab", settings.VideoControls.LastActiveTab);
                textWriter.WriteElementString("WaveformDrawGrid", settings.VideoControls.WaveformDrawGrid.ToString());
                textWriter.WriteElementString("WaveformAllowOverlap", settings.VideoControls.WaveformAllowOverlap.ToString());
                textWriter.WriteElementString("WaveformFocusOnMouseEnter", settings.VideoControls.WaveformFocusOnMouseEnter.ToString());
                textWriter.WriteElementString("WaveformListViewFocusOnMouseEnter", settings.VideoControls.WaveformListViewFocusOnMouseEnter.ToString());
                textWriter.WriteElementString("WaveformBorderHitMs", settings.VideoControls.WaveformBorderHitMs.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformGridColor", settings.VideoControls.WaveformGridColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformColor", settings.VideoControls.WaveformColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformSelectedColor", settings.VideoControls.WaveformSelectedColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformBackgroundColor", settings.VideoControls.WaveformBackgroundColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformTextColor", settings.VideoControls.WaveformTextColor.ToArgb().ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformDoubleClickOnNonParagraphAction", settings.VideoControls.WaveformDoubleClickOnNonParagraphAction);
                textWriter.WriteElementString("WaveformRightClickOnNonParagraphAction", settings.VideoControls.WaveformRightClickOnNonParagraphAction);
                textWriter.WriteElementString("WaveformMouseWheelScrollUpIsForward", settings.VideoControls.WaveformMouseWheelScrollUpIsForward.ToString());
                textWriter.WriteElementString("GenerateSpectrogram", settings.VideoControls.GenerateSpectrogram.ToString());
                textWriter.WriteElementString("SpectrogramAppearance", settings.VideoControls.SpectrogramAppearance);
                textWriter.WriteElementString("WaveformMininumSampleRate", settings.VideoControls.WaveformMininumSampleRate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformSeeksSilenceDurationSeconds", settings.VideoControls.WaveformSeeksSilenceDurationSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("WaveformSeeksSilenceMaxVolume", settings.VideoControls.WaveformSeeksSilenceMaxVolume.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("NetworkSettings", "");
                textWriter.WriteElementString("SessionKey", settings.NetworkSettings.SessionKey);
                textWriter.WriteElementString("UserName", settings.NetworkSettings.UserName);
                textWriter.WriteElementString("WebServiceUrl", settings.NetworkSettings.WebServiceUrl);
                textWriter.WriteElementString("PollIntervalSeconds", settings.NetworkSettings.PollIntervalSeconds.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("VobSubOcr", "");
                textWriter.WriteElementString("XOrMorePixelsMakesSpace", settings.VobSubOcr.XOrMorePixelsMakesSpace.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("AllowDifferenceInPercent", settings.VobSubOcr.AllowDifferenceInPercent.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("BlurayAllowDifferenceInPercent", settings.VobSubOcr.BlurayAllowDifferenceInPercent.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LastImageCompareFolder", settings.VobSubOcr.LastImageCompareFolder);
                textWriter.WriteElementString("LastModiLanguageId", settings.VobSubOcr.LastModiLanguageId.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LastOcrMethod", settings.VobSubOcr.LastOcrMethod);
                textWriter.WriteElementString("TesseractLastLanguage", settings.VobSubOcr.TesseractLastLanguage);
                textWriter.WriteElementString("UseModiInTesseractForUnknownWords", settings.VobSubOcr.UseModiInTesseractForUnknownWords.ToString());
                textWriter.WriteElementString("UseItalicsInTesseract", settings.VobSubOcr.UseItalicsInTesseract.ToString());
                textWriter.WriteElementString("UseMusicSymbolsInTesseract", settings.VobSubOcr.UseMusicSymbolsInTesseract.ToString());
                textWriter.WriteElementString("RightToLeft", settings.VobSubOcr.RightToLeft.ToString());
                textWriter.WriteElementString("TopToBottom", settings.VobSubOcr.TopToBottom.ToString());
                textWriter.WriteElementString("DefaultMillisecondsForUnknownDurations", settings.VobSubOcr.DefaultMillisecondsForUnknownDurations.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("PromptForUnknownWords", settings.VobSubOcr.PromptForUnknownWords.ToString());
                textWriter.WriteElementString("GuessUnknownWords", settings.VobSubOcr.GuessUnknownWords.ToString());
                textWriter.WriteElementString("AutoBreakSubtitleIfMoreThanTwoLines", settings.VobSubOcr.AutoBreakSubtitleIfMoreThanTwoLines.ToString());
                textWriter.WriteElementString("ItalicFactor", settings.VobSubOcr.ItalicFactor.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrDraw", settings.VobSubOcr.LineOcrDraw.ToString());
                textWriter.WriteElementString("LineOcrAdvancedItalic", settings.VobSubOcr.LineOcrAdvancedItalic.ToString());
                textWriter.WriteElementString("LineOcrLastLanguages", settings.VobSubOcr.LineOcrLastLanguages);
                textWriter.WriteElementString("LineOcrLastSpellCheck", settings.VobSubOcr.LineOcrLastSpellCheck);
                textWriter.WriteElementString("LineOcrXOrMorePixelsMakesSpace", settings.VobSubOcr.LineOcrXOrMorePixelsMakesSpace.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrMinLineHeight", settings.VobSubOcr.LineOcrMinLineHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteElementString("LineOcrMaxLineHeight", settings.VobSubOcr.LineOcrMaxLineHeight.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("MultipleSearchAndReplaceList", "");
                foreach (var item in settings.MultipleSearchAndReplaceList)
                {
                    textWriter.WriteStartElement("MultipleSearchAndReplaceItem", "");
                    textWriter.WriteElementString("Enabled", item.Enabled.ToString());
                    textWriter.WriteElementString("FindWhat", item.FindWhat);
                    textWriter.WriteElementString("ReplaceWith", item.ReplaceWith);
                    textWriter.WriteElementString("SearchType", item.SearchType);
                    textWriter.WriteEndElement();
                }
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("Shortcuts", "");
                textWriter.WriteElementString("GeneralGoToFirstSelectedLine", settings.Shortcuts.GeneralGoToFirstSelectedLine);
                textWriter.WriteElementString("GeneralGoToNextEmptyLine", settings.Shortcuts.GeneralGoToNextEmptyLine);
                textWriter.WriteElementString("GeneralMergeSelectedLines", settings.Shortcuts.GeneralMergeSelectedLines);
                textWriter.WriteElementString("GeneralMergeSelectedLinesOnlyFirstText", settings.Shortcuts.GeneralMergeSelectedLinesOnlyFirstText);
                textWriter.WriteElementString("GeneralToggleTranslationMode", settings.Shortcuts.GeneralToggleTranslationMode);
                textWriter.WriteElementString("GeneralSwitchOriginalAndTranslation", settings.Shortcuts.GeneralSwitchOriginalAndTranslation);
                textWriter.WriteElementString("GeneralMergeOriginalAndTranslation", settings.Shortcuts.GeneralMergeOriginalAndTranslation);
                textWriter.WriteElementString("GeneralGoToNextSubtitle", settings.Shortcuts.GeneralGoToNextSubtitle);
                textWriter.WriteElementString("GeneralGoToPrevSubtitle", settings.Shortcuts.GeneralGoToPrevSubtitle);
                textWriter.WriteElementString("GeneralGoToEndOfCurrentSubtitle", settings.Shortcuts.GeneralGoToEndOfCurrentSubtitle);
                textWriter.WriteElementString("GeneralGoToStartOfCurrentSubtitle", settings.Shortcuts.GeneralGoToStartOfCurrentSubtitle);
                textWriter.WriteElementString("GeneralPlayFirstSelected", settings.Shortcuts.GeneralPlayFirstSelected);
                textWriter.WriteElementString("MainFileNew", settings.Shortcuts.MainFileNew);
                textWriter.WriteElementString("MainFileOpen", settings.Shortcuts.MainFileOpen);
                textWriter.WriteElementString("MainFileOpenKeepVideo", settings.Shortcuts.MainFileOpenKeepVideo);
                textWriter.WriteElementString("MainFileSave", settings.Shortcuts.MainFileSave);
                textWriter.WriteElementString("MainFileSaveOriginal", settings.Shortcuts.MainFileSaveOriginal);
                textWriter.WriteElementString("MainFileSaveOriginalAs", settings.Shortcuts.MainFileSaveOriginalAs);
                textWriter.WriteElementString("MainFileSaveAs", settings.Shortcuts.MainFileSaveAs);
                textWriter.WriteElementString("MainFileSaveAll", settings.Shortcuts.MainFileSaveAll);
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
                textWriter.WriteElementString("MainToolsSplitLongLines", settings.Shortcuts.MainToolsSplitLongLines);
                textWriter.WriteElementString("MainToolsRenumber", settings.Shortcuts.MainToolsRenumber);
                textWriter.WriteElementString("MainToolsRemoveTextForHI", settings.Shortcuts.MainToolsRemoveTextForHI);
                textWriter.WriteElementString("MainToolsChangeCasing", settings.Shortcuts.MainToolsChangeCasing);
                textWriter.WriteElementString("MainToolsAutoDuration", settings.Shortcuts.MainToolsAutoDuration);
                textWriter.WriteElementString("MainToolsBatchConvert", settings.Shortcuts.MainToolsBatchConvert);
                textWriter.WriteElementString("MainToolsBeamer", settings.Shortcuts.MainToolsBeamer);
                textWriter.WriteElementString("MainToolsToggleTranslationOriginalInPreviews", settings.Shortcuts.MainEditToggleTranslationOriginalInPreviews);
                textWriter.WriteElementString("MainEditInverseSelection", settings.Shortcuts.MainEditInverseSelection);
                textWriter.WriteElementString("MainEditModifySelection", settings.Shortcuts.MainEditModifySelection);
                textWriter.WriteElementString("MainVideoPause", settings.Shortcuts.MainVideoPause);
                textWriter.WriteElementString("MainVideoPlayPauseToggle", settings.Shortcuts.MainVideoPlayPauseToggle);
                textWriter.WriteElementString("MainVideoShowHideVideo", settings.Shortcuts.MainVideoShowHideVideo);
                textWriter.WriteElementString("MainVideoToggleVideoControls", settings.Shortcuts.MainVideoToggleVideoControls);
                textWriter.WriteElementString("MainVideo1FrameLeft", settings.Shortcuts.MainVideo1FrameLeft);
                textWriter.WriteElementString("MainVideo1FrameRight", settings.Shortcuts.MainVideo1FrameRight);
                textWriter.WriteElementString("MainVideo100MsLeft", settings.Shortcuts.MainVideo100MsLeft);
                textWriter.WriteElementString("MainVideo100MsRight", settings.Shortcuts.MainVideo100MsRight);
                textWriter.WriteElementString("MainVideo500MsLeft", settings.Shortcuts.MainVideo500MsLeft);
                textWriter.WriteElementString("MainVideo500MsRight", settings.Shortcuts.MainVideo500MsRight);
                textWriter.WriteElementString("MainVideo1000MsLeft", settings.Shortcuts.MainVideo1000MsLeft);
                textWriter.WriteElementString("MainVideo1000MsRight", settings.Shortcuts.MainVideo1000MsRight);
                textWriter.WriteElementString("MainVideoFullscreen", settings.Shortcuts.MainVideoFullscreen);
                textWriter.WriteElementString("MainSpellCheck", settings.Shortcuts.MainSpellCheck);
                textWriter.WriteElementString("MainSpellCheckFindDoubleWords", settings.Shortcuts.MainSpellCheckFindDoubleWords);
                textWriter.WriteElementString("MainSpellCheckAddWordToNames", settings.Shortcuts.MainSpellCheckAddWordToNames);
                textWriter.WriteElementString("MainSynchronizationAdjustTimes", settings.Shortcuts.MainSynchronizationAdjustTimes);
                textWriter.WriteElementString("MainSynchronizationVisualSync", settings.Shortcuts.MainSynchronizationVisualSync);
                textWriter.WriteElementString("MainSynchronizationPointSync", settings.Shortcuts.MainSynchronizationPointSync);
                textWriter.WriteElementString("MainSynchronizationChangeFrameRate", settings.Shortcuts.MainSynchronizationChangeFrameRate);
                textWriter.WriteElementString("MainListViewItalic", settings.Shortcuts.MainListViewItalic);
                textWriter.WriteElementString("MainListViewToggleDashes", settings.Shortcuts.MainListViewToggleDashes);
                textWriter.WriteElementString("MainListViewAlignment", settings.Shortcuts.MainListViewAlignment);
                textWriter.WriteElementString("MainListViewCopyText", settings.Shortcuts.MainListViewCopyText);
                textWriter.WriteElementString("MainListViewAutoDuration", settings.Shortcuts.MainListViewAutoDuration);
                textWriter.WriteElementString("MainListViewColumnDeleteText", settings.Shortcuts.MainListViewColumnDeleteText);
                textWriter.WriteElementString("MainListViewColumnInsertText", settings.Shortcuts.MainListViewColumnInsertText);
                textWriter.WriteElementString("MainListViewColumnPaste", settings.Shortcuts.MainListViewColumnPaste);
                textWriter.WriteElementString("MainListViewFocusWaveform", settings.Shortcuts.MainListViewFocusWaveform);
                textWriter.WriteElementString("MainListViewGoToNextError", settings.Shortcuts.MainListViewGoToNextError);
                textWriter.WriteElementString("MainEditReverseStartAndEndingForRTL", settings.Shortcuts.MainEditReverseStartAndEndingForRTL);
                textWriter.WriteElementString("MainTextBoxItalic", settings.Shortcuts.MainTextBoxItalic);
                textWriter.WriteElementString("MainTextBoxSplitAtCursor", settings.Shortcuts.MainTextBoxSplitAtCursor);
                textWriter.WriteElementString("MainTextBoxMoveLastWordDown", settings.Shortcuts.MainTextBoxMoveLastWordDown);
                textWriter.WriteElementString("MainTextBoxMoveFirstWordFromNextUp", settings.Shortcuts.MainTextBoxMoveFirstWordFromNextUp);
                textWriter.WriteElementString("MainTextBoxSelectionToLower", settings.Shortcuts.MainTextBoxSelectionToLower);
                textWriter.WriteElementString("MainTextBoxSelectionToUpper", settings.Shortcuts.MainTextBoxSelectionToUpper);
                textWriter.WriteElementString("MainCreateInsertSubAtVideoPos", settings.Shortcuts.MainCreateInsertSubAtVideoPos);
                textWriter.WriteElementString("MainCreatePlayFromJustBefore", settings.Shortcuts.MainCreatePlayFromJustBefore);
                textWriter.WriteElementString("MainCreateSetStart", settings.Shortcuts.MainCreateSetStart);
                textWriter.WriteElementString("MainCreateSetEnd", settings.Shortcuts.MainCreateSetEnd);
                textWriter.WriteElementString("MainCreateSetEndAddNewAndGoToNew", settings.Shortcuts.MainCreateSetEndAddNewAndGoToNew);
                textWriter.WriteElementString("MainCreateStartDownEndUp", settings.Shortcuts.MainCreateStartDownEndUp);
                textWriter.WriteElementString("MainAdjustSetStartAndOffsetTheRest", settings.Shortcuts.MainAdjustSetStartAndOffsetTheRest);
                textWriter.WriteElementString("MainAdjustSetEndAndOffsetTheRest", settings.Shortcuts.MainAdjustSetEndAndOffsetTheRest);
                textWriter.WriteElementString("MainAdjustSetEndAndOffsetTheRestAndGoToNext", settings.Shortcuts.MainAdjustSetEndAndOffsetTheRestAndGoToNext);
                textWriter.WriteElementString("MainAdjustSetEndAndGotoNext", settings.Shortcuts.MainAdjustSetEndAndGotoNext);
                textWriter.WriteElementString("MainAdjustViaEndAutoStartAndGoToNext", settings.Shortcuts.MainAdjustViaEndAutoStartAndGoToNext);
                textWriter.WriteElementString("MainAdjustSetStartAutoDurationAndGoToNext", settings.Shortcuts.MainAdjustSetStartAutoDurationAndGoToNext);
                textWriter.WriteElementString("MainAdjustSetEndNextStartAndGoToNext", settings.Shortcuts.MainAdjustSetEndNextStartAndGoToNext);
                textWriter.WriteElementString("MainAdjustStartDownEndUpAndGoToNext", settings.Shortcuts.MainAdjustStartDownEndUpAndGoToNext);
                textWriter.WriteElementString("MainAdjustSetStart", settings.Shortcuts.MainAdjustSetStart);
                textWriter.WriteElementString("MainAdjustSetStartKeepDuration", settings.Shortcuts.MainAdjustSetStartKeepDuration);
                textWriter.WriteElementString("MainAdjustSetEnd", settings.Shortcuts.MainAdjustSetEnd);
                textWriter.WriteElementString("MainAdjustSelected100MsForward", settings.Shortcuts.MainAdjustSelected100MsForward);
                textWriter.WriteElementString("MainAdjustSelected100MsBack", settings.Shortcuts.MainAdjustSelected100MsBack);
                textWriter.WriteElementString("MainInsertAfter", settings.Shortcuts.MainInsertAfter);
                textWriter.WriteElementString("MainTextBoxInsertAfter", settings.Shortcuts.MainTextBoxInsertAfter);
                textWriter.WriteElementString("MainTextBoxAutoBreak", settings.Shortcuts.MainTextBoxAutoBreak);
                textWriter.WriteElementString("MainTextBoxUnbreak", settings.Shortcuts.MainTextBoxUnbreak);
                textWriter.WriteElementString("MainWaveformInsertAtCurrentPosition", settings.Shortcuts.MainWaveformInsertAtCurrentPosition);
                textWriter.WriteElementString("MainInsertBefore", settings.Shortcuts.MainInsertBefore);
                textWriter.WriteElementString("MainMergeDialog", settings.Shortcuts.MainMergeDialog);
                textWriter.WriteElementString("MainToggleFocus", settings.Shortcuts.MainToggleFocus);
                textWriter.WriteElementString("WaveformVerticalZoom", settings.Shortcuts.WaveformVerticalZoom);
                textWriter.WriteElementString("WaveformVerticalZoomOut", settings.Shortcuts.WaveformVerticalZoomOut);
                textWriter.WriteElementString("WaveformZoomIn", settings.Shortcuts.WaveformZoomIn);
                textWriter.WriteElementString("WaveformZoomOut", settings.Shortcuts.WaveformZoomOut);
                textWriter.WriteElementString("WaveformPlaySelection", settings.Shortcuts.WaveformPlaySelection);
                textWriter.WriteElementString("WaveformSearchSilenceForward", settings.Shortcuts.WaveformSearchSilenceForward);
                textWriter.WriteElementString("WaveformSearchSilenceBack", settings.Shortcuts.WaveformSearchSilenceBack);
                textWriter.WriteElementString("WaveformAddTextHere", settings.Shortcuts.WaveformAddTextHere);
                textWriter.WriteElementString("WaveformFocusListView", settings.Shortcuts.WaveformFocusListView);
                textWriter.WriteElementString("MainTranslateCustomSearch1", settings.Shortcuts.MainTranslateCustomSearch1);
                textWriter.WriteElementString("MainTranslateCustomSearch2", settings.Shortcuts.MainTranslateCustomSearch2);
                textWriter.WriteElementString("MainTranslateCustomSearch3", settings.Shortcuts.MainTranslateCustomSearch3);
                textWriter.WriteElementString("MainTranslateCustomSearch4", settings.Shortcuts.MainTranslateCustomSearch4);
                textWriter.WriteElementString("MainTranslateCustomSearch5", settings.Shortcuts.MainTranslateCustomSearch5);
                textWriter.WriteElementString("MainTranslateCustomSearch6", settings.Shortcuts.MainTranslateCustomSearch6);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("RemoveTextForHearingImpaired", "");
                textWriter.WriteElementString("RemoveTextBetweenBrackets", settings.RemoveTextForHearingImpaired.RemoveTextBetweenBrackets.ToString());
                textWriter.WriteElementString("RemoveTextBetweenParentheses", settings.RemoveTextForHearingImpaired.RemoveTextBetweenParentheses.ToString());
                textWriter.WriteElementString("RemoveTextBetweenCurlyBrackets", settings.RemoveTextForHearingImpaired.RemoveTextBetweenCurlyBrackets.ToString());
                textWriter.WriteElementString("RemoveTextBetweenQuestionMarks", settings.RemoveTextForHearingImpaired.RemoveTextBetweenQuestionMarks.ToString());
                textWriter.WriteElementString("RemoveTextBetweenCustom", settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustom.ToString());
                textWriter.WriteElementString("RemoveTextBetweenCustomBefore", settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomBefore);
                textWriter.WriteElementString("RemoveTextBetweenCustomAfter", settings.RemoveTextForHearingImpaired.RemoveTextBetweenCustomAfter);
                textWriter.WriteElementString("RemoveTextBetweenOnlySeperateLines", settings.RemoveTextForHearingImpaired.RemoveTextBetweenOnlySeperateLines.ToString());
                textWriter.WriteElementString("RemoveTextBeforeColon", settings.RemoveTextForHearingImpaired.RemoveTextBeforeColon.ToString());
                textWriter.WriteElementString("RemoveTextBeforeColonOnlyIfUppercase", settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyIfUppercase.ToString());
                textWriter.WriteElementString("RemoveTextBeforeColonOnlyOnSeparateLine", settings.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyOnSeparateLine.ToString());
                textWriter.WriteElementString("RemoveInterjections", settings.RemoveTextForHearingImpaired.RemoveInterjections.ToString());
                textWriter.WriteElementString("RemoveIfAllUppercase", settings.RemoveTextForHearingImpaired.RemoveIfAllUppercase.ToString());
                textWriter.WriteElementString("RemoveIfContains", settings.RemoveTextForHearingImpaired.RemoveIfContains.ToString());
                textWriter.WriteElementString("RemoveIfContainsText", settings.RemoveTextForHearingImpaired.RemoveIfContainsText);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("SubtitleBeaming", "");
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
                    File.WriteAllText(fileName, sw.ToString().Replace("encoding=\"utf-16\"", "encoding=\"utf-8\""), Encoding.UTF8);
                }
                catch
                {
                }
            }
        }

    }
}
