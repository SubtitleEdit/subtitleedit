using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Nikse.SubtitleEdit.Logic
{
    // The settings classes are build for easy xml-serilization (makes save/load code simple)
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
            var newList = new List<RecentFileEntry> { new RecentFileEntry() { FileName = fileName, FirstVisibleIndex = firstVisibleIndex, FirstSelectedIndex = firstSelectedIndex, VideoFileName = videoFileName, OriginalFileName = originalFileName } };
            int index = 0;
            foreach (var oldRecentFile in Files)
            {
                if (string.Compare(fileName, oldRecentFile.FileName, true) != 0 && index < MaxRecentFiles)
                    newList.Add(new RecentFileEntry() { FileName = oldRecentFile.FileName, FirstVisibleIndex = oldRecentFile.FirstVisibleIndex, FirstSelectedIndex = oldRecentFile.FirstSelectedIndex, VideoFileName = oldRecentFile.VideoFileName, OriginalFileName = oldRecentFile.OriginalFileName });
                index++;
            }
            Files = newList;
        }

        public void Add(string fileName, string videoFileName, string originalFileName)
        {
            var newList = new List<RecentFileEntry>();
            foreach (var oldRecentFile in Files)
            {
                if (string.Compare(fileName, oldRecentFile.FileName, true) == 0)
                    newList.Add(new RecentFileEntry() { FileName = oldRecentFile.FileName, FirstVisibleIndex = oldRecentFile.FirstVisibleIndex, FirstSelectedIndex = oldRecentFile.FirstSelectedIndex, VideoFileName = oldRecentFile.VideoFileName, OriginalFileName = oldRecentFile.OriginalFileName });
            }
            if (newList.Count == 0)
                newList.Add(new RecentFileEntry() { FileName = fileName, FirstVisibleIndex = -1, FirstSelectedIndex = -1, VideoFileName = videoFileName, OriginalFileName = originalFileName });

            int index = 0;
            foreach (var oldRecentFile in Files)
            {
                if (string.Compare(fileName, oldRecentFile.FileName, true) != 0 && index < MaxRecentFiles)
                    newList.Add(new RecentFileEntry() { FileName = oldRecentFile.FileName, FirstVisibleIndex = oldRecentFile.FirstVisibleIndex, FirstSelectedIndex = oldRecentFile.FirstSelectedIndex, VideoFileName = oldRecentFile.VideoFileName, OriginalFileName = oldRecentFile.OriginalFileName });
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
        public string MusicSymbol { get; set; }
        public string MusicSymbolToReplace { get; set; }
        public bool SpellCheckAutoChangeNames { get; set; }
        public bool SpellCheckOneLetterWords { get; set; }
        public bool OcrFixUseHardcodedRules { get; set; }
        public string Interjections { get; set; }
        public string MicrosoftBingApiId { get; set; }
        public string GoogleApiKey { get; set; }

        public ToolsSettings()
        {
            StartSceneIndex = 1;
            EndSceneIndex = 1;
            VerifyPlaySeconds = 2;
            MergeLinesShorterThan = 33;
            MusicSymbol = "♪";
            MusicSymbolToReplace = "âª â¶ â™ª âTª ã¢â™âª ?t×3 ?t¤3";
            SpellCheckAutoChangeNames = true;
            OcrFixUseHardcodedRules = true;
            Interjections = "Ah;Ahh;Ahhh;Eh;Ehh;Ehhh;Hm;Hmm;Hmmm;Phew;Gah;Oh;Ohh;Ohhh;Ow;Oww;Owww;Ugh;Ughh;Uh;Uhh;Uhhh;Whew";
            MicrosoftBingApiId = "C2C2E9A508E6748F0494D68DFD92FAA1FF9B0BA4";
            GoogleApiKey = "ABQIAAAA4j5cWwa3lDH0RkZceh7PjBTDmNAghl5kWSyuukQ0wtoJG8nFBxRPlalq-gAvbeCXMCkmrysqjXV1Gw";
            SpellCheckOneLetterWords = true;
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
            NamesEtcUrl = "http://www.nikse.dk/se/Names_Etc.xml";
        }
    }

    public class SubtitleSettings
    {
        public string SsaFontName { get; set; }
        public double SsaFontSize { get; set; }
        public int SsaFontColorArgb { get; set; }
        public string DCinemaFontFile { get; set; }
        public int DCinemaFontSize { get; set; }
        public int DCinemaBottomMargin { get; set; }
        public int DCinemaFadeUpDownTime { get; set; }

        public SubtitleSettings()
        {
            SsaFontName = "Tahoma";
            SsaFontSize = 18;
            SsaFontColorArgb = System.Drawing.Color.FromArgb(255, 255, 255).ToArgb();
            DCinemaFontFile = "Arial.tff";
            DCinemaFontSize = 42;
            DCinemaBottomMargin = 8;
            DCinemaFadeUpDownTime = 20;
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
        public bool UppercaseIInsideLowercaseWordTicked { get; set; }
        public bool DoubleApostropheToQuoteTicked { get; set; }
        public bool AddPeriodAfterParagraphTicked { get; set; }
        public bool StartWithUppercaseLetterAfterParagraphTicked { get; set; }
        public bool StartWithUppercaseLetterAfterPeriodInsideParagraphTicked { get; set; }
        public bool AloneLowercaseIToUppercaseIEnglishTicked { get; set; }
        public bool FixOcrErrorsViaReplaceListTicked { get; set; }
        public bool RemoveSpaceBetweenNumberTicked { get; set; }
        public bool FixDialogsOnOneLineTicked { get; set; }
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
            TooShortDisplayTimeTicked  = true;
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
            AloneLowercaseIToUppercaseIEnglishTicked = false;
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
        public string DefaultEncoding { get; set; }
        public bool AutoGuessAnsiEncoding { get; set; }
        public string SubtitleFontName { get; set; }
        public int SubtitleFontSize { get; set; }
        public bool SubtitleFontBold { get; set; }
        public Color SubtitleFontColor { get; set; }
        public Color SubtitleBackgroundColor { get; set; }
        public bool ShowRecentFiles { get; set; }
        public bool RememberSelectedLine { get; set; }
        public bool StartLoadLastFile { get; set; }
        public bool StartRememberPositionAndSize { get; set; }
        public string StartPosition { get; set; }
        public string StartSize { get; set; }
        public int StartListViewWidth { get; set; }
        public bool StartInSourceView { get; set; }
        public bool RemoveBlankLinesWhenOpening { get; set; }
        public int SubtitleLineMaximumLength { get; set; }
        public int MininumMillisecondsBetweenLines { get; set; }
        public bool AutoWrapLineWhileTyping { get; set; }
        public int SubtitleMaximumCharactersPerSeconds { get; set; }
        public string SpellCheckLanguage { get; set; }
        public string VideoPlayer { get; set; }
        public int VideoPlayerDefaultVolume { get; set; }
        public int VideoPlayerPreviewFontSize { get; set; }
        public bool VideoPlayerShowStopButton { get; set; }
        public string Language { get; set; }
        public string ListViewLineSeparatorString { get; set; }
        public int ListViewDoubleClickAction { get; set; }
        public string UppercaseLetters { get; set; }
        public int DefaultAdjustMilliseconds { get; set; }
        public bool AutoRepeatOn { get; set; }
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
        public bool WaveFormCenter { get; set; }
        public int SmallDelayMilliseconds { get; set; }
        public int LargeDelayMilliseconds { get; set; }
        public bool ShowOriginalAsPreviewIfAvailable { get; set; }
        public int LastPacCodePage { get; set; }

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
                SubtitleFontName = Utilities.WinXp2kUnicodeFontName;

            SubtitleFontSize = 8;
            SubtitleFontBold = false;
            SubtitleFontColor = System.Drawing.Color.Black;
            SubtitleBackgroundColor = System.Drawing.Color.White;
            DefaultEncoding = "UTF-8";
            AutoGuessAnsiEncoding = false;
            ShowRecentFiles = true;
            RememberSelectedLine = true;
            StartLoadLastFile = true;
            StartRememberPositionAndSize = true;
            SubtitleLineMaximumLength = 43;
            MininumMillisecondsBetweenLines = 25;
            AutoWrapLineWhileTyping = false;
            SubtitleMaximumCharactersPerSeconds = 25;
            SpellCheckLanguage = null;
            VideoPlayer = string.Empty;
            VideoPlayerDefaultVolume = 50;
            VideoPlayerPreviewFontSize = 10;
            VideoPlayerShowStopButton = true;
            ListViewLineSeparatorString = "<br />";
            ListViewDoubleClickAction = 1;
            UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWZYXÆØÃÅÄÖÉÈÁÂÀÇÉÊÍÓÔÕÚŁ";
            DefaultAdjustMilliseconds = 1000;
            AutoRepeatOn = true;
            AutoContinueOn = false;
            SyncListViewWithVideoWhilePlaying = false;
            AutoBackupSeconds = 0;
            SpellChecker = "hunspell";
            AllowEditOfOriginalSubtitle = false;
            PromptDeleteLines = true;
            Undocked = false;
            UndockedVideoPosition = "-32000;-32000";
            UndockedWaveformPosition = "-32000;-32000";
            UndockedVideoControlsPosition = "-32000;-32000";

            SmallDelayMilliseconds = 500;
            LargeDelayMilliseconds = 5000;
        }
    }


    public class VideoControlsSettings
    {
        public string CustomSearchText { get; set; }
        public string CustomSearchUrl { get; set; }
        public string LastActiveTab { get; set; }
        public bool WaveFormDrawGrid { get; set; }
        public Color WaveFormGridColor { get; set; }
        public Color WaveFormColor { get; set; }
        public Color WaveFormSelectedColor { get; set; }
        public Color WaveFormBackgroundColor { get; set; }
        public Color WaveFormTextColor { get; set; }
        public string WaveFormDoubleClickOnNonParagraphAction { get; set; }
        public string WaveFormRightClickOnNonParagraphAction { get; set; }
        public bool WaveFormMouseWheelScrollUpIsForward { get; set; }
        public bool GenerateSpectrogram { get; set; }
        public string SpectrogramAppearance { get; set; }
        public int WaveFormMininumSampleRate { get; set; }

        public VideoControlsSettings()
        {
            CustomSearchText = "The Free Dictionary";
            CustomSearchUrl = "http://www.thefreedictionary.com/{0}";
            LastActiveTab = "Translate";
            WaveFormDrawGrid = true;
            WaveFormGridColor = Color.FromArgb(255, 20, 20, 18);
            WaveFormColor = Color.GreenYellow;
            WaveFormSelectedColor = Color.Red;
            WaveFormBackgroundColor = Color.Black;
            WaveFormTextColor = Color.Gray;
            WaveFormDoubleClickOnNonParagraphAction = "PlayPause";
            WaveFormDoubleClickOnNonParagraphAction = string.Empty;
            WaveFormMouseWheelScrollUpIsForward = true;
            SpectrogramAppearance = "OneColorGradient";
            WaveFormMininumSampleRate = 126;
        }
    }

    public class VobSubOcrSettings
    {
        public int XOrMorePixelsMakesSpace { get; set; }
        public double AllowDifferenceInPercent { get; set; }
        public string LastImageCompareFolder { get; set; }
        public int LastModiLanguageId { get; set; }
        public string LastOcrMethod { get; set; }
        public string TesseractLastLanguage { get; set; }
        public bool UseModiInTesseractForUnknownWords { get; set; }
        public bool UseItalicsInTesseract { get; set; }
        public bool RightToLeft { get; set; }
        public bool TopToBottom { get; set; }
        public int DefaultMillisecondsForUnknownDurations { get; set; }
        public bool PromptForUnknownWords { get; set; }

        public VobSubOcrSettings()
        {
            XOrMorePixelsMakesSpace = 8;
            AllowDifferenceInPercent = 1.0;
            LastImageCompareFolder = "English";
            LastModiLanguageId = 9;
            LastOcrMethod = "Tesseract";
            UseItalicsInTesseract = true;
            RightToLeft = false;
            TopToBottom = true;
            DefaultMillisecondsForUnknownDurations = 5000;
            PromptForUnknownWords = true;
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

        public NetworkSettings()
        {
            UserName = string.Empty;
            SessionKey = "DemoSession"; // TODO - leave blank or use guid
            WebServiceUrl = "http://www.nikse.dk/se/SeService.asmx";
        }
    }

    public class Shortcuts
    {
        public string MainFileNew { get; set; }
        public string MainFileOpen { get; set; }
        public string MainFileSave { get; set; }
        public string MainFileSaveAs { get; set; }
        public string MainFileExportEbu { get; set; }
        public string MainEditFind { get; set; }
        public string MainEditFindNext { get; set; }
        public string MainEditReplace { get; set; }
        public string MainEditMultipleReplace { get; set; }
        public string MainEditGoToLineNumber { get; set; }
        public string MainToolsFixCommonErrors { get; set; }
        public string MainVideoShowHideVideo { get; set; }
        public string MainVideoToggleVideoControls { get; set; }
        public string MainVideo100MsLeft { get; set; }
        public string MainVideo100MsRight { get; set; }
        public string MainVideo500MsLeft { get; set; }
        public string MainVideo500MsRight { get; set; }
        public string MainVideoFullscreen { get; set; }
        public string MainSynchronizationAdjustTimes { get; set; }
        public string MainListViewItalic { get; set; }
        public string MainListViewToggleDashes { get; set; }
        public string MainTextBoxItalic { get; set; }
        public string MainCreateInsertSubAtVideoPos { get; set; }
        public string MainCreatePlayFromJustBefore { get; set; }
        public string MainCreateSetStart { get; set; }
        public string MainCreateSetEnd { get; set; }
        public string MainAdjustSetStartAndOffsetTheRest { get; set; }
        public string MainAdjustSetEndAndGotoNext { get; set; }
        public string MainAdjustViaEndAutoStartAndGoToNext { get; set; }
        public string MainAdjustSetStartAutoDurationAndGoToNext { get; set; }
        public string MainAdjustSetStart { get; set; }
        public string MainAdjustSetEnd { get; set; }
        public string MainAdjustSelected100MsForward { get; set; }
        public string MainAdjustSelected100MsBack { get; set; }
        public string MainInsertAfter { get; set; }
        public string MainInsertBefore { get; set; }
        public string MainGoToNext { get; set; }
        public string MainGoToPrevious { get; set; }
        public string WaveformVerticalZoom { get; set; }
        public string WaveformZoomIn { get; set; }
        public string WaveformZoomOut { get; set; }
        public string WaveformPlaySelection { get; set; }

        public Shortcuts()
        {
            MainFileNew = "Control+N";
            MainFileOpen = "Control+O";
            MainFileSave = "Control+S";
            MainFileSaveAs = "";
            MainFileExportEbu = string.Empty;
            MainEditFind = "Control+F";
            MainEditFindNext = "F3";
            MainEditReplace = "Control+H";
            MainEditMultipleReplace = string.Empty;
            MainEditGoToLineNumber = "Control+G";
            MainToolsFixCommonErrors = "Control+Shift+F";
            MainVideoShowHideVideo = "Control+Q";
            MainVideo100MsLeft = "Control+Left";
            MainVideo100MsRight = "Control+Right";
            MainVideo500MsLeft = "Alt+Left";
            MainVideo500MsRight = "Alt+Right";
            MainVideoFullscreen = "Alt+Return";
            MainSynchronizationAdjustTimes = "Control+Shift+A";
            MainListViewItalic = "Control+I";
            MainTextBoxItalic = "Control+I";
            MainCreateInsertSubAtVideoPos = string.Empty;
            MainCreatePlayFromJustBefore = string.Empty;
            MainCreateSetStart = string.Empty;
            MainCreateSetEnd = string.Empty;
            MainAdjustSetStartAndOffsetTheRest = "Control+Space";
            MainAdjustSetEndAndGotoNext = string.Empty;
            MainAdjustViaEndAutoStartAndGoToNext = string.Empty;
            MainAdjustSetStartAutoDurationAndGoToNext = string.Empty;
            MainAdjustSetStart = string.Empty;
            MainAdjustSetEnd = string.Empty;
            MainAdjustSelected100MsForward = string.Empty;
            MainAdjustSelected100MsBack = string.Empty;
            MainInsertAfter = "Alt+Ins";
            MainInsertBefore = "Control+Shift+Ins";
            WaveformVerticalZoom = string.Empty;
            WaveformPlaySelection = string.Empty;
        }
    }

    public class RemoveTextForHearingImpairedSettings
    {
        public bool RemoveTextBeforeColor { get; set; }
        public bool RemoveTextBeforeColorOnlyIfUppercase { get; set; }
        public bool RemoveInterjections { get; set; }
        public bool RemoveIfContains { get; set; }
        public string RemoveIfContainsText { get; set; }

        public RemoveTextForHearingImpairedSettings()
        {
            RemoveTextBeforeColor = true;
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
            NetworkSettings = new Logic.NetworkSettings();
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

                    //too slow... :(  - settings = Deserialize(Configuration.BaseDirectory + "Settings.xml"); // 688 msecs
                }
                catch
                {
                    settings = new Settings();
                }

                if (string.IsNullOrEmpty(settings.General.ListViewLineSeparatorString))
                    settings.General.ListViewLineSeparatorString = Environment.NewLine;
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
            doc.Load(fileName);

            Settings settings = new Settings();

            settings.RecentFiles = new Nikse.SubtitleEdit.Logic.RecentFilesSettings();
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

            settings.General = new Nikse.SubtitleEdit.Logic.GeneralSettings();
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
                settings.General.DefaultFrameRate = Convert.ToDouble(subNode.InnerText);
                settings.General.CurrentFrameRate = Convert.ToDouble(subNode.InnerText);
            }
            subNode = node.SelectSingleNode("DefaultEncoding");
            if (subNode != null)
                settings.General.DefaultEncoding = subNode.InnerText;
            subNode = node.SelectSingleNode("AutoGuessAnsiEncoding");
            if (subNode != null)
                settings.General.AutoGuessAnsiEncoding = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SubtitleFontName");
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
            subNode = node.SelectSingleNode("StartListViewWidth");
            if (subNode != null)
                settings.General.StartListViewWidth = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("StartInSourceView");
            if (subNode != null)
                settings.General.StartInSourceView = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("RemoveBlankLinesWhenOpening");
            if (subNode != null)
                settings.General.RemoveBlankLinesWhenOpening = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SubtitleLineMaximumLength");
            if (subNode != null)
                settings.General.SubtitleLineMaximumLength = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("MininumMillisecondsBetweenLines");
            if (subNode != null)
                settings.General.MininumMillisecondsBetweenLines = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("AutoWrapLineWhileTyping");
            if (subNode != null)
                settings.General.AutoWrapLineWhileTyping = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SubtitleMaximumCharactersPerSeconds");
            if (subNode != null)
                settings.General.SubtitleMaximumCharactersPerSeconds = Convert.ToInt32(subNode.InnerText);
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
            subNode = node.SelectSingleNode("VideoPlayerShowStopButton");
            if (subNode != null)
                settings.General.VideoPlayerShowStopButton = Convert.ToBoolean(subNode.InnerText);
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
            subNode = node.SelectSingleNode("WaveFormCenter");
            if (subNode != null)
                settings.General.WaveFormCenter = Convert.ToBoolean(subNode.InnerText);
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

            settings.Tools = new Nikse.SubtitleEdit.Logic.ToolsSettings();
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
            subNode = node.SelectSingleNode("MusicSymbol");
            if (subNode != null)
                settings.Tools.MusicSymbol = subNode.InnerText;
            subNode = node.SelectSingleNode("MusicSymbolToReplace");
            if (subNode != null)
                settings.Tools.MusicSymbolToReplace = subNode.InnerText;
            subNode = node.SelectSingleNode("SpellCheckAutoChangeNames");
            if (subNode != null)
                settings.Tools.SpellCheckAutoChangeNames = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SpellCheckOneLetterWords");
            if (subNode != null)
                settings.Tools.SpellCheckOneLetterWords = Convert.ToBoolean(subNode.InnerText);
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

            settings.SubtitleSettings = new Nikse.SubtitleEdit.Logic.SubtitleSettings();
            node = doc.DocumentElement.SelectSingleNode("SubtitleSettings");
            if (node != null)
            {
                subNode = node.SelectSingleNode("SsaFontName");
                if (subNode != null)
                    settings.SubtitleSettings.SsaFontName = subNode.InnerText;
                subNode = node.SelectSingleNode("SsaFontSize");
                if (subNode != null)
                    settings.SubtitleSettings.SsaFontSize = Convert.ToDouble(subNode.InnerText);
                subNode = node.SelectSingleNode("SsaFontColorArgb");
                if (subNode != null)
                    settings.SubtitleSettings.SsaFontColorArgb = Convert.ToInt32(subNode.InnerText);
                subNode = node.SelectSingleNode("DCinemaFontFile");
                if (subNode != null)
                    settings.SubtitleSettings.DCinemaFontFile = subNode.InnerText;
                subNode = node.SelectSingleNode("DCinemaFontSize");
                if (subNode != null)
                    settings.SubtitleSettings.DCinemaFontSize = Convert.ToInt32(subNode.InnerText);
                subNode = node.SelectSingleNode("DCinemaBottomMargin");
                if (subNode != null)
                    settings.SubtitleSettings.DCinemaBottomMargin = Convert.ToInt32(subNode.InnerText);
            }

            settings.Proxy = new Nikse.SubtitleEdit.Logic.ProxySettings();
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

            settings.WordLists = new Nikse.SubtitleEdit.Logic.WordListSettings();
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

            settings.CommonErrors = new Nikse.SubtitleEdit.Logic.FixCommonErrorsSettings();
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
            subNode = node.SelectSingleNode("CustomSearchText");
            if (subNode != null)
                settings.VideoControls.CustomSearchText = subNode.InnerText;
            subNode = node.SelectSingleNode("CustomSearchUrl");
            if (subNode != null)
                settings.VideoControls.CustomSearchUrl = subNode.InnerText;
            subNode = node.SelectSingleNode("LastActiveTab");
            if (subNode != null)
                settings.VideoControls.LastActiveTab = subNode.InnerText;
            subNode = node.SelectSingleNode("WaveFormDrawGrid");
            if (subNode != null)
                settings.VideoControls.WaveFormDrawGrid = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("WaveFormGridColor");
            if (subNode != null)
                settings.VideoControls.WaveFormGridColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("WaveFormColor");
            if (subNode != null)
                settings.VideoControls.WaveFormColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("WaveFormSelectedColor");
            if (subNode != null)
                settings.VideoControls.WaveFormSelectedColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("WaveFormBackgroundColor");
            if (subNode != null)
                settings.VideoControls.WaveFormBackgroundColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("WaveFormTextColor");
            if (subNode != null)
                settings.VideoControls.WaveFormTextColor = Color.FromArgb(int.Parse(subNode.InnerText));
            subNode = node.SelectSingleNode("WaveFormDoubleClickOnNonParagraphAction");
            if (subNode != null)
                settings.VideoControls.WaveFormDoubleClickOnNonParagraphAction = subNode.InnerText;
            subNode = node.SelectSingleNode("WaveFormRightClickOnNonParagraphAction");
            if (subNode != null)
                settings.VideoControls.WaveFormRightClickOnNonParagraphAction = subNode.InnerText;
            subNode = node.SelectSingleNode("WaveFormMouseWheelScrollUpIsForward");
            if (subNode != null)
                settings.VideoControls.WaveFormMouseWheelScrollUpIsForward = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("GenerateSpectrogram");
            if (subNode != null)
                settings.VideoControls.GenerateSpectrogram = Convert.ToBoolean(subNode.InnerText);
            subNode = node.SelectSingleNode("SpectrogramAppearance");
            if (subNode != null)
                settings.VideoControls.SpectrogramAppearance = subNode.InnerText;
            subNode = node.SelectSingleNode("WaveFormMininumSampleRate");
            if (subNode != null)
                settings.VideoControls.WaveFormMininumSampleRate = Convert.ToInt32(subNode.InnerText);

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
            }

            settings.VobSubOcr = new Nikse.SubtitleEdit.Logic.VobSubOcrSettings();
            node = doc.DocumentElement.SelectSingleNode("VobSubOcr");
            subNode = node.SelectSingleNode("XOrMorePixelsMakesSpace");
            if (subNode != null)
                settings.VobSubOcr.XOrMorePixelsMakesSpace = Convert.ToInt32(subNode.InnerText);
            subNode = node.SelectSingleNode("AllowDifferenceInPercent");
            if (subNode != null)
                settings.VobSubOcr.AllowDifferenceInPercent = Convert.ToDouble(subNode.InnerText);
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
                subNode = node.SelectSingleNode("MainFileNew");
                if (subNode != null)
                    settings.Shortcuts.MainFileNew = subNode.InnerText;
                subNode = node.SelectSingleNode("MainFileOpen");
                if (subNode != null)
                    settings.Shortcuts.MainFileOpen = subNode.InnerText;
                subNode = node.SelectSingleNode("MainFileSave");
                if (subNode != null)
                    settings.Shortcuts.MainFileSave = subNode.InnerText;
                subNode = node.SelectSingleNode("MainFileSaveAs");
                if (subNode != null)
                    settings.Shortcuts.MainFileSaveAs = subNode.InnerText;
                subNode = node.SelectSingleNode("MainFileExportEbu");
                if (subNode != null)
                    settings.Shortcuts.MainFileExportEbu = subNode.InnerText;
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
                subNode = node.SelectSingleNode("MainToolsFixCommonErrors");
                if (subNode != null)
                    settings.Shortcuts.MainToolsFixCommonErrors = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideoShowHideVideo");
                if (subNode != null)
                    settings.Shortcuts.MainVideoShowHideVideo = subNode.InnerText;
                subNode = node.SelectSingleNode("MainVideoToggleVideoControls");
                if (subNode != null)
                    settings.Shortcuts.MainVideoToggleVideoControls = subNode.InnerText;
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
                subNode = node.SelectSingleNode("MainVideoFullscreen");
                if (subNode != null)
                    settings.Shortcuts.MainVideoFullscreen = subNode.InnerText;
                subNode = node.SelectSingleNode("MainSynchronizationAdjustTimes");
                if (subNode != null)
                    settings.Shortcuts.MainSynchronizationAdjustTimes = subNode.InnerText;
                subNode = node.SelectSingleNode("MainListViewItalic");
                if (subNode != null)
                    settings.Shortcuts.MainListViewItalic = subNode.InnerText;
                subNode = node.SelectSingleNode("MainListViewToggleDashes");
                if (subNode != null)
                    settings.Shortcuts.MainListViewToggleDashes = subNode.InnerText;
                subNode = node.SelectSingleNode("MainTextBoxItalic");
                if (subNode != null)
                    settings.Shortcuts.MainTextBoxItalic = subNode.InnerText;
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
                subNode = node.SelectSingleNode("MainAdjustSetStartAndOffsetTheRest");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSetStartAndOffsetTheRest = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustSetEndAndGotoNext");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSetEndAndGotoNext = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustViaEndAutoStartAndGoToNext");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustViaEndAutoStartAndGoToNext = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustSetStartAutoDurationAndGoToNext");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSetStartAutoDurationAndGoToNext = subNode.InnerText;
                subNode = node.SelectSingleNode("MainAdjustSetStart");
                if (subNode != null)
                    settings.Shortcuts.MainAdjustSetStart = subNode.InnerText;
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
                subNode = node.SelectSingleNode("MainInsertBefore");
                if (subNode != null)
                    settings.Shortcuts.MainInsertBefore = subNode.InnerText;
                subNode = node.SelectSingleNode("MainGoToNext");
                if (subNode != null)
                    settings.Shortcuts.MainGoToNext = subNode.InnerText;
                subNode = node.SelectSingleNode("MainGoToPrevious");
                if (subNode != null)
                    settings.Shortcuts.MainGoToPrevious = subNode.InnerText;
                subNode = node.SelectSingleNode("WaveformVerticalZoom");
                if (subNode != null)
                    settings.Shortcuts.WaveformVerticalZoom = subNode.InnerText;
                subNode = node.SelectSingleNode("WaveformZoomIn");
                if (subNode != null)
                    settings.Shortcuts.WaveformZoomIn = subNode.InnerText;
                subNode = node.SelectSingleNode("WaveformZoomOut");
                if (subNode != null)
                    settings.Shortcuts.WaveformZoomOut = subNode.InnerText;
                subNode = node.SelectSingleNode("WaveformPlaySelection");
                if (subNode != null)
                    settings.Shortcuts.WaveformPlaySelection = subNode.InnerText;
            }

            settings.RemoveTextForHearingImpaired = new RemoveTextForHearingImpairedSettings();
            node = doc.DocumentElement.SelectSingleNode("RemoveTextForHearingImpaired");
            if (node != null)
            {
                subNode = node.SelectSingleNode("RemoveTextBeforeColor");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveTextBeforeColor = Convert.ToBoolean(subNode.InnerText);
                subNode = node.SelectSingleNode("RemoveTextBeforeColorOnlyIfUppercase");
                if (subNode != null)
                    settings.RemoveTextForHearingImpaired.RemoveTextBeforeColorOnlyIfUppercase = Convert.ToBoolean(subNode.InnerText);
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
            var textWriter = new XmlTextWriter(fileName, null) {Formatting = Formatting.Indented};
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
                textWriter.WriteAttributeString("FirstVisibleIndex", item.FirstVisibleIndex.ToString());
                textWriter.WriteAttributeString("FirstSelectedIndex", item.FirstSelectedIndex.ToString());
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
            textWriter.WriteElementString("ShowToolbarVisualSync", settings.General.ShowToolbarVisualSync.ToString());
            textWriter.WriteElementString("ShowToolbarSpellCheck", settings.General.ShowToolbarSpellCheck.ToString());
            textWriter.WriteElementString("ShowToolbarSettings", settings.General.ShowToolbarSettings.ToString());
            textWriter.WriteElementString("ShowToolbarHelp", settings.General.ShowToolbarHelp.ToString());
            textWriter.WriteElementString("ShowFrameRate", settings.General.ShowFrameRate.ToString());
            textWriter.WriteElementString("ShowVideoPlayer", settings.General.ShowVideoPlayer.ToString());
            textWriter.WriteElementString("ShowAudioVisualizer", settings.General.ShowAudioVisualizer.ToString());
            textWriter.WriteElementString("ShowWaveform", settings.General.ShowWaveform.ToString());
            textWriter.WriteElementString("ShowSpectrogram", settings.General.ShowSpectrogram.ToString());
            textWriter.WriteElementString("DefaultFrameRate", settings.General.DefaultFrameRate.ToString());
            textWriter.WriteElementString("DefaultEncoding", settings.General.DefaultEncoding);
            textWriter.WriteElementString("AutoGuessAnsiEncoding", settings.General.AutoGuessAnsiEncoding.ToString());
            textWriter.WriteElementString("SubtitleFontName", settings.General.SubtitleFontName);
            textWriter.WriteElementString("SubtitleFontSize", settings.General.SubtitleFontSize.ToString());
            textWriter.WriteElementString("SubtitleFontBold", settings.General.SubtitleFontBold.ToString());
            textWriter.WriteElementString("SubtitleFontColor", settings.General.SubtitleFontColor.ToArgb().ToString());
            textWriter.WriteElementString("SubtitleBackgroundColor", settings.General.SubtitleBackgroundColor.ToArgb().ToString());
            textWriter.WriteElementString("ShowRecentFiles", settings.General.ShowRecentFiles.ToString());
            textWriter.WriteElementString("RememberSelectedLine", settings.General.RememberSelectedLine.ToString());
            textWriter.WriteElementString("StartLoadLastFile", settings.General.StartLoadLastFile.ToString());
            textWriter.WriteElementString("StartRememberPositionAndSize", settings.General.StartRememberPositionAndSize.ToString());
            textWriter.WriteElementString("StartPosition", settings.General.StartPosition);
            textWriter.WriteElementString("StartSize", settings.General.StartSize);
            textWriter.WriteElementString("StartListViewWidth", settings.General.StartListViewWidth.ToString());
            textWriter.WriteElementString("StartInSourceView", settings.General.StartInSourceView.ToString());
            textWriter.WriteElementString("RemoveBlankLinesWhenOpening", settings.General.RemoveBlankLinesWhenOpening.ToString());
            textWriter.WriteElementString("SubtitleLineMaximumLength", settings.General.SubtitleLineMaximumLength.ToString());
            textWriter.WriteElementString("MininumMillisecondsBetweenLines", settings.General.MininumMillisecondsBetweenLines.ToString());
            textWriter.WriteElementString("AutoWrapLineWhileTyping", settings.General.AutoWrapLineWhileTyping.ToString());
            textWriter.WriteElementString("SubtitleMaximumCharactersPerSeconds", settings.General.SubtitleMaximumCharactersPerSeconds.ToString());
            textWriter.WriteElementString("SpellCheckLanguage", settings.General.SpellCheckLanguage);
            textWriter.WriteElementString("VideoPlayer", settings.General.VideoPlayer);
            textWriter.WriteElementString("VideoPlayerDefaultVolume", settings.General.VideoPlayerDefaultVolume.ToString());
            textWriter.WriteElementString("VideoPlayerPreviewFontSize", settings.General.VideoPlayerPreviewFontSize.ToString());
            textWriter.WriteElementString("VideoPlayerShowStopButton", settings.General.VideoPlayerShowStopButton.ToString());
            textWriter.WriteElementString("Language", settings.General.Language);
            textWriter.WriteElementString("ListViewLineSeparatorString", settings.General.ListViewLineSeparatorString);
            textWriter.WriteElementString("ListViewDoubleClickAction", settings.General.ListViewDoubleClickAction.ToString());
            textWriter.WriteElementString("UppercaseLetters", settings.General.UppercaseLetters);
            textWriter.WriteElementString("DefaultAdjustMilliseconds", settings.General.DefaultAdjustMilliseconds.ToString());
            textWriter.WriteElementString("AutoRepeatOn", settings.General.AutoRepeatOn.ToString());
            textWriter.WriteElementString("AutoContinueOn", settings.General.AutoContinueOn.ToString());
            textWriter.WriteElementString("SyncListViewWithVideoWhilePlaying", settings.General.SyncListViewWithVideoWhilePlaying.ToString());
            textWriter.WriteElementString("AutoBackupSeconds", settings.General.AutoBackupSeconds.ToString());
            textWriter.WriteElementString("SpellChecker", settings.General.SpellChecker);
            textWriter.WriteElementString("AllowEditOfOriginalSubtitle", settings.General.AllowEditOfOriginalSubtitle.ToString());
            textWriter.WriteElementString("PromptDeleteLines", settings.General.PromptDeleteLines.ToString());
            textWriter.WriteElementString("Undocked", settings.General.Undocked.ToString());
            textWriter.WriteElementString("UndockedVideoPosition", settings.General.UndockedVideoPosition);
            textWriter.WriteElementString("UndockedWaveformPosition", settings.General.UndockedWaveformPosition);
            textWriter.WriteElementString("UndockedVideoControlsPosition", settings.General.UndockedVideoControlsPosition);
            textWriter.WriteElementString("WaveFormCenter", settings.General.WaveFormCenter.ToString());
            textWriter.WriteElementString("SmallDelayMilliseconds", settings.General.SmallDelayMilliseconds.ToString());
            textWriter.WriteElementString("LargeDelayMilliseconds", settings.General.LargeDelayMilliseconds.ToString());
            textWriter.WriteElementString("ShowOriginalAsPreviewIfAvailable", settings.General.ShowOriginalAsPreviewIfAvailable.ToString());
            textWriter.WriteElementString("LastPacCodePage", settings.General.LastPacCodePage.ToString());
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("Tools", "");
            textWriter.WriteElementString("StartSceneIndex", settings.Tools.StartSceneIndex.ToString());
            textWriter.WriteElementString("EndSceneIndex", settings.Tools.EndSceneIndex.ToString());
            textWriter.WriteElementString("VerifyPlaySeconds", settings.Tools.VerifyPlaySeconds.ToString());
            textWriter.WriteElementString("MergeLinesShorterThan", settings.Tools.MergeLinesShorterThan.ToString());
            textWriter.WriteElementString("MusicSymbol", settings.Tools.MusicSymbol);
            textWriter.WriteElementString("MusicSymbolToReplace", settings.Tools.MusicSymbolToReplace);
            textWriter.WriteElementString("SpellCheckAutoChangeNames", settings.Tools.SpellCheckAutoChangeNames.ToString());
            textWriter.WriteElementString("SpellCheckOneLetterWords", settings.Tools.SpellCheckOneLetterWords.ToString());
            textWriter.WriteElementString("OcrFixUseHardcodedRules", settings.Tools.OcrFixUseHardcodedRules.ToString());
            textWriter.WriteElementString("Interjections", settings.Tools.Interjections);
            textWriter.WriteElementString("MicrosoftBingApiId", settings.Tools.MicrosoftBingApiId);
            textWriter.WriteElementString("GoogleApiKey", settings.Tools.GoogleApiKey);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("SubtitleSettings", "");
            textWriter.WriteElementString("SsaFontName", settings.SubtitleSettings.SsaFontName);
            textWriter.WriteElementString("SsaFontSize", settings.SubtitleSettings.SsaFontSize.ToString());
            textWriter.WriteElementString("SsaFontColorArgb", settings.SubtitleSettings.SsaFontColorArgb.ToString());
            textWriter.WriteElementString("DCinemaFontFile", settings.SubtitleSettings.DCinemaFontFile);
            textWriter.WriteElementString("DCinemaFontSize", settings.SubtitleSettings.DCinemaFontSize.ToString());
            textWriter.WriteElementString("DCinemaBottomMargin", settings.SubtitleSettings.DCinemaBottomMargin.ToString());
            textWriter.WriteElementString("DCinemaFadeUpDownTime", settings.SubtitleSettings.DCinemaFadeUpDownTime.ToString());
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
            textWriter.WriteElementString("UppercaseIInsideLowercaseWordTicked", settings.CommonErrors.UppercaseIInsideLowercaseWordTicked.ToString());
            textWriter.WriteElementString("DoubleApostropheToQuoteTicked", settings.CommonErrors.DoubleApostropheToQuoteTicked.ToString());
            textWriter.WriteElementString("AddPeriodAfterParagraphTicked", settings.CommonErrors.AddPeriodAfterParagraphTicked.ToString());
            textWriter.WriteElementString("StartWithUppercaseLetterAfterParagraphTicked", settings.CommonErrors.StartWithUppercaseLetterAfterParagraphTicked.ToString());
            textWriter.WriteElementString("StartWithUppercaseLetterAfterPeriodInsideParagraphTicked", settings.CommonErrors.StartWithUppercaseLetterAfterPeriodInsideParagraphTicked.ToString());
            textWriter.WriteElementString("AloneLowercaseIToUppercaseIEnglishTicked", settings.CommonErrors.AloneLowercaseIToUppercaseIEnglishTicked.ToString());
            textWriter.WriteElementString("FixOcrErrorsViaReplaceListTicked", settings.CommonErrors.FixOcrErrorsViaReplaceListTicked.ToString());
            textWriter.WriteElementString("RemoveSpaceBetweenNumberTicked", settings.CommonErrors.RemoveSpaceBetweenNumberTicked.ToString());
            textWriter.WriteElementString("FixDialogsOnOneLineTicked", settings.CommonErrors.FixDialogsOnOneLineTicked.ToString());
            textWriter.WriteElementString("DanishLetterITicked", settings.CommonErrors.DanishLetterITicked.ToString());
            textWriter.WriteElementString("SpanishInvertedQuestionAndExclamationMarksTicked", settings.CommonErrors.SpanishInvertedQuestionAndExclamationMarksTicked.ToString());
            textWriter.WriteElementString("FixDoubleDashTicked", settings.CommonErrors.FixDoubleDashTicked.ToString());
            textWriter.WriteElementString("FixDoubleGreaterThanTicked", settings.CommonErrors.FixDoubleGreaterThanTicked.ToString());
            textWriter.WriteElementString("FixEllipsesStartTicked", settings.CommonErrors.FixEllipsesStartTicked.ToString());
            textWriter.WriteElementString("FixMissingOpenBracketTicked", settings.CommonErrors.FixMissingOpenBracketTicked.ToString());
            textWriter.WriteElementString("FixMusicNotationTicked", settings.CommonErrors.FixMusicNotationTicked.ToString());
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("VideoControls", "");
            textWriter.WriteElementString("CustomSearchText", settings.VideoControls.CustomSearchText);
            textWriter.WriteElementString("CustomSearchUrl", settings.VideoControls.CustomSearchUrl);
            textWriter.WriteElementString("LastActiveTab", settings.VideoControls.LastActiveTab);
            textWriter.WriteElementString("WaveFormDrawGrid", settings.VideoControls.WaveFormDrawGrid.ToString());
            textWriter.WriteElementString("WaveFormGridColor", settings.VideoControls.WaveFormGridColor.ToArgb().ToString());
            textWriter.WriteElementString("WaveFormColor", settings.VideoControls.WaveFormColor.ToArgb().ToString());
            textWriter.WriteElementString("WaveFormSelectedColor", settings.VideoControls.WaveFormSelectedColor.ToArgb().ToString());
            textWriter.WriteElementString("WaveFormBackgroundColor", settings.VideoControls.WaveFormBackgroundColor.ToArgb().ToString());
            textWriter.WriteElementString("WaveFormTextColor", settings.VideoControls.WaveFormTextColor.ToArgb().ToString());
            textWriter.WriteElementString("WaveFormDoubleClickOnNonParagraphAction", settings.VideoControls.WaveFormDoubleClickOnNonParagraphAction);
            textWriter.WriteElementString("WaveFormRightClickOnNonParagraphAction", settings.VideoControls.WaveFormRightClickOnNonParagraphAction);
            textWriter.WriteElementString("WaveFormMouseWheelScrollUpIsForward", settings.VideoControls.WaveFormMouseWheelScrollUpIsForward.ToString());
            textWriter.WriteElementString("GenerateSpectrogram", settings.VideoControls.GenerateSpectrogram.ToString());
            textWriter.WriteElementString("SpectrogramAppearance", settings.VideoControls.SpectrogramAppearance);
            textWriter.WriteElementString("WaveFormMininumSampleRate", settings.VideoControls.WaveFormMininumSampleRate.ToString());
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("NetworkSettings", "");
            textWriter.WriteElementString("SessionKey", settings.NetworkSettings.SessionKey);
            textWriter.WriteElementString("UserName", settings.NetworkSettings.UserName);
            textWriter.WriteElementString("WebServiceUrl", settings.NetworkSettings.WebServiceUrl);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("VobSubOcr", "");
            textWriter.WriteElementString("XOrMorePixelsMakesSpace", settings.VobSubOcr.XOrMorePixelsMakesSpace.ToString());
            textWriter.WriteElementString("AllowDifferenceInPercent", settings.VobSubOcr.AllowDifferenceInPercent.ToString());
            textWriter.WriteElementString("LastImageCompareFolder", settings.VobSubOcr.LastImageCompareFolder);
            textWriter.WriteElementString("LastModiLanguageId", settings.VobSubOcr.LastModiLanguageId.ToString());
            textWriter.WriteElementString("LastOcrMethod", settings.VobSubOcr.LastOcrMethod);
            textWriter.WriteElementString("TesseractLastLanguage", settings.VobSubOcr.TesseractLastLanguage);
            textWriter.WriteElementString("UseModiInTesseractForUnknownWords", settings.VobSubOcr.UseModiInTesseractForUnknownWords.ToString());
            textWriter.WriteElementString("UseItalicsInTesseract", settings.VobSubOcr.UseItalicsInTesseract.ToString());
            textWriter.WriteElementString("RightToLeft", settings.VobSubOcr.RightToLeft.ToString());
            textWriter.WriteElementString("TopToBottom", settings.VobSubOcr.TopToBottom.ToString());
            textWriter.WriteElementString("DefaultMillisecondsForUnknownDurations", settings.VobSubOcr.DefaultMillisecondsForUnknownDurations.ToString());
            textWriter.WriteElementString("PromptForUnknownWords", settings.VobSubOcr.PromptForUnknownWords.ToString());
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
            textWriter.WriteElementString("MainFileNew", settings.Shortcuts.MainFileNew);
            textWriter.WriteElementString("MainFileOpen", settings.Shortcuts.MainFileOpen);
            textWriter.WriteElementString("MainFileSave", settings.Shortcuts.MainFileSave);
            textWriter.WriteElementString("MainFileSaveAs", settings.Shortcuts.MainFileSaveAs);
            textWriter.WriteElementString("MainFileExportEbu", settings.Shortcuts.MainFileSaveAs);
            textWriter.WriteElementString("MainEditFind", settings.Shortcuts.MainEditFind);
            textWriter.WriteElementString("MainEditFindNext", settings.Shortcuts.MainEditFindNext);
            textWriter.WriteElementString("MainEditReplace", settings.Shortcuts.MainEditReplace);
            textWriter.WriteElementString("MainEditMultipleReplace", settings.Shortcuts.MainEditMultipleReplace);
            textWriter.WriteElementString("MainEditGoToLineNumber", settings.Shortcuts.MainEditGoToLineNumber);
            textWriter.WriteElementString("MainToolsFixCommonErrors", settings.Shortcuts.MainToolsFixCommonErrors);
            textWriter.WriteElementString("MainVideoShowHideVideo", settings.Shortcuts.MainVideoShowHideVideo);
            textWriter.WriteElementString("MainVideoToggleVideoControls", settings.Shortcuts.MainVideoToggleVideoControls);
            textWriter.WriteElementString("MainVideo100MsLeft", settings.Shortcuts.MainVideo100MsLeft);
            textWriter.WriteElementString("MainVideo100MsRight", settings.Shortcuts.MainVideo100MsRight);
            textWriter.WriteElementString("MainVideo500MsLeft", settings.Shortcuts.MainVideo500MsLeft);
            textWriter.WriteElementString("MainVideo500MsRight", settings.Shortcuts.MainVideo500MsRight);
            textWriter.WriteElementString("MainVideoFullscreen", settings.Shortcuts.MainVideoFullscreen);
            textWriter.WriteElementString("MainSynchronizationAdjustTimes", settings.Shortcuts.MainSynchronizationAdjustTimes);
            textWriter.WriteElementString("MainListViewItalic", settings.Shortcuts.MainListViewItalic);
            textWriter.WriteElementString("MainListViewToggleDashes", settings.Shortcuts.MainListViewToggleDashes);
            textWriter.WriteElementString("MainTextBoxItalic", settings.Shortcuts.MainTextBoxItalic);
            textWriter.WriteElementString("MainCreateInsertSubAtVideoPos", settings.Shortcuts.MainCreateInsertSubAtVideoPos);
            textWriter.WriteElementString("MainCreatePlayFromJustBefore", settings.Shortcuts.MainCreatePlayFromJustBefore);
            textWriter.WriteElementString("MainCreateSetStart", settings.Shortcuts.MainCreateSetStart);
            textWriter.WriteElementString("MainCreateSetEnd", settings.Shortcuts.MainCreateSetEnd);
            textWriter.WriteElementString("MainAdjustSetStartAndOffsetTheRest", settings.Shortcuts.MainAdjustSetStartAndOffsetTheRest);
            textWriter.WriteElementString("MainAdjustSetEndAndGotoNext", settings.Shortcuts.MainAdjustSetEndAndGotoNext);
            textWriter.WriteElementString("MainAdjustViaEndAutoStartAndGoToNext", settings.Shortcuts.MainAdjustViaEndAutoStartAndGoToNext);
            textWriter.WriteElementString("MainAdjustSetStartAutoDurationAndGoToNext", settings.Shortcuts.MainAdjustSetStartAutoDurationAndGoToNext);
            textWriter.WriteElementString("MainAdjustSetStart", settings.Shortcuts.MainAdjustSetStart);
            textWriter.WriteElementString("MainAdjustSetEnd", settings.Shortcuts.MainAdjustSetEnd);
            textWriter.WriteElementString("MainAdjustSelected100MsForward", settings.Shortcuts.MainAdjustSelected100MsForward);
            textWriter.WriteElementString("MainAdjustSelected100MsBack", settings.Shortcuts.MainAdjustSelected100MsBack);
            textWriter.WriteElementString("MainInsertAfter", settings.Shortcuts.MainInsertAfter);
            textWriter.WriteElementString("MainInsertBefore", settings.Shortcuts.MainInsertBefore);
            textWriter.WriteElementString("MainGoToNext", settings.Shortcuts.MainGoToNext);
            textWriter.WriteElementString("MainGoToPrevious", settings.Shortcuts.MainGoToPrevious);
            textWriter.WriteElementString("WaveformVerticalZoom", settings.Shortcuts.WaveformVerticalZoom);
            textWriter.WriteElementString("WaveformZoomIn", settings.Shortcuts.WaveformZoomIn);
            textWriter.WriteElementString("WaveformZoomOut", settings.Shortcuts.WaveformZoomOut);
            textWriter.WriteElementString("WaveformPlaySelection", settings.Shortcuts.WaveformPlaySelection);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("RemoveTextForHearingImpaired", "");
            textWriter.WriteElementString("RemoveTextBeforeColor", settings.RemoveTextForHearingImpaired.RemoveTextBeforeColor.ToString());
            textWriter.WriteElementString("RemoveTextBeforeColorOnlyIfUppercase", settings.RemoveTextForHearingImpaired.RemoveTextBeforeColorOnlyIfUppercase.ToString());
            textWriter.WriteElementString("RemoveInterjections", settings.RemoveTextForHearingImpaired.RemoveInterjections.ToString());
            textWriter.WriteElementString("RemoveIfContains", settings.RemoveTextForHearingImpaired.RemoveIfContains.ToString());
            textWriter.WriteElementString("RemoveIfContainsText", settings.RemoveTextForHearingImpaired.RemoveIfContainsText);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("SubtitleBeaming", "");
            textWriter.WriteElementString("FontName", settings.SubtitleBeaming.FontName);
            textWriter.WriteElementString("FontColor", settings.SubtitleBeaming.FontColor.ToArgb().ToString());
            textWriter.WriteElementString("FontSize", settings.SubtitleBeaming.FontSize.ToString());
            textWriter.WriteElementString("BorderColor", settings.SubtitleBeaming.BorderColor.ToArgb().ToString());
            textWriter.WriteElementString("BorderWidth", settings.SubtitleBeaming.BorderWidth.ToString());
            textWriter.WriteEndElement();

            textWriter.WriteEndElement();

            textWriter.WriteEndDocument();
            textWriter.Close();
        }

    }
}
