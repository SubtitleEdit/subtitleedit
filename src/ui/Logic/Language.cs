using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Nikse.SubtitleEdit.Logic
{
    public class Language
    {
        [XmlAttribute("Name")]
        public string Name;
        public LanguageStructure.General General;
        public LanguageStructure.About About;
        public LanguageStructure.AddToNames AddToNames;
        public LanguageStructure.AddToOcrReplaceList AddToOcrReplaceList;
        public LanguageStructure.AddToUserDictionary AddToUserDictionary;
        public LanguageStructure.AddWaveform AddWaveform;
        public LanguageStructure.AddWaveformBatch AddWaveformBatch;
        public LanguageStructure.AdjustDisplayDuration AdjustDisplayDuration;
        public LanguageStructure.ApplyDurationLimits ApplyDurationLimits;
        public LanguageStructure.AudioToText AudioToText;
        public LanguageStructure.AssaAttachments AssaAttachments;
        public LanguageStructure.AssaOverrideTags AssaOverrideTags;
        public LanguageStructure.AssaProgressBarGenerator AssaProgressBarGenerator;
        public LanguageStructure.AssaResolutionChanger AssaResolutionChanger;
        public LanguageStructure.ImageColorPicker ImageColorPicker;
        public LanguageStructure.AssaSetBackgroundBox AssaSetBackgroundBox;
        public LanguageStructure.AssaSetPosition AssaSetPosition;
        public LanguageStructure.AutoBreakUnbreakLines AutoBreakUnbreakLines;
        public LanguageStructure.BatchConvert BatchConvert;
        public LanguageStructure.BeautifyTimeCodes BeautifyTimeCodes;
        public LanguageStructure.BeautifyTimeCodesProfile BeautifyTimeCodesProfile;
        public LanguageStructure.BinEdit BinEdit;
        public LanguageStructure.Bookmarks Bookmarks;
        public LanguageStructure.ChangeCasing ChangeCasing;
        public LanguageStructure.ChangeCasingNames ChangeCasingNames;
        public LanguageStructure.ChangeFrameRate ChangeFrameRate;
        public LanguageStructure.ChangeSpeedInPercent ChangeSpeedInPercent;
        public LanguageStructure.CheckForUpdates CheckForUpdates;
        public LanguageStructure.ChooseAudioTrack ChooseAudioTrack;
        public LanguageStructure.ChooseEncoding ChooseEncoding;
        public LanguageStructure.ChooseLanguage ChooseLanguage;
        public LanguageStructure.ColorChooser ColorChooser;
        public LanguageStructure.ColumnPaste ColumnPaste;
        public LanguageStructure.CompareSubtitles CompareSubtitles;
        public LanguageStructure.ConvertColorsToDialog ConvertColorsToDialog;
        public LanguageStructure.DCinemaProperties DCinemaProperties;
        public LanguageStructure.DurationsBridgeGaps DurationsBridgeGaps;
        public LanguageStructure.DvdSubRip DvdSubRip;
        public LanguageStructure.DvdSubRipChooseLanguage DvdSubRipChooseLanguage;
        public LanguageStructure.EbuSaveOptions EbuSaveOptions;
        public LanguageStructure.EffectKaraoke EffectKaraoke;
        public LanguageStructure.EffectTypewriter EffectTypewriter;
        public LanguageStructure.ExportCustomText ExportCustomText;
        public LanguageStructure.ExportCustomTextFormat ExportCustomTextFormat;
        public LanguageStructure.ExportFcpXmlAdvanced ExportFcpXmlAdvanced;
        public LanguageStructure.ExportPngXml ExportPngXml;
        public LanguageStructure.ExportText ExportText;
        public LanguageStructure.ExtractDateTimeInfo ExtractDateTimeInfo;
        public LanguageStructure.FindDialog FindDialog;
        public LanguageStructure.FindSubtitleLine FindSubtitleLine;
        public LanguageStructure.FixCommonErrors FixCommonErrors;
        public LanguageStructure.GenerateBlankVideo GenerateBlankVideo;
        public LanguageStructure.GenerateVideoWithBurnedInSubs GenerateVideoWithBurnedInSubs;
        public LanguageStructure.GenerateVideoWithEmbeddedSubs GenerateVideoWithEmbeddedSubs;
        public LanguageStructure.GetDictionaries GetDictionaries;
        public LanguageStructure.GetTesseractDictionaries GetTesseractDictionaries;
        public LanguageStructure.GoogleTranslate GoogleTranslate;
        public LanguageStructure.GoogleOrMicrosoftTranslate GoogleOrMicrosoftTranslate;
        public LanguageStructure.GoToLine GoToLine;
        public LanguageStructure.ImportImages ImportImages;
        public LanguageStructure.ImportShotChanges ImportShotChanges;
        public LanguageStructure.ImportText ImportText;
        public LanguageStructure.Interjections Interjections;
        public LanguageStructure.JoinSubtitles JoinSubtitles;
        public LanguageStructure.LanguageNames LanguageNames;
        public LanguageStructure.Main Main;
        public LanguageStructure.MatroskaSubtitleChooser MatroskaSubtitleChooser;
        public LanguageStructure.MeasurementConverter MeasurementConverter;
        public LanguageStructure.MergeDoubleLines MergeDoubleLines;
        public LanguageStructure.MergeShortLines MergedShortLines;
        public LanguageStructure.MergeTextWithSameTimeCodes MergeTextWithSameTimeCodes;
        public LanguageStructure.ModifySelection ModifySelection;
        public LanguageStructure.MultipleReplace MultipleReplace;
        public LanguageStructure.NetworkChat NetworkChat;
        public LanguageStructure.NetworkJoin NetworkJoin;
        public LanguageStructure.NetworkLogAndInfo NetworkLogAndInfo;
        public LanguageStructure.NetworkStart NetworkStart;
        public LanguageStructure.OpenVideoDvd OpenVideoDvd;
        public LanguageStructure.PluginsGet PluginsGet;
        public LanguageStructure.RegularExpressionContextMenu RegularExpressionContextMenu;
        public LanguageStructure.RemoveTextFromHearImpaired RemoveTextFromHearImpaired;
        public LanguageStructure.ReplaceDialog ReplaceDialog;
        public LanguageStructure.RestoreAutoBackup RestoreAutoBackup;
        public LanguageStructure.SeekSilence SeekSilence;
        public LanguageStructure.SetMinimumDisplayTimeBetweenParagraphs SetMinimumDisplayTimeBetweenParagraphs;
        public LanguageStructure.SetSyncPoint SetSyncPoint;
        public LanguageStructure.Settings Settings;
        public LanguageStructure.SettingsMpv SettingsMpv;
        public LanguageStructure.SettingsFfmpeg SettingsFfmpeg;
        public LanguageStructure.SetVideoOffset SetVideoOffset;
        public LanguageStructure.ShowEarlierLater ShowEarlierLater;
        public LanguageStructure.ShowHistory ShowHistory;
        public LanguageStructure.SpellCheck SpellCheck;
        public LanguageStructure.NetflixQualityCheck NetflixQualityCheck;
        public LanguageStructure.Split Split;
        public LanguageStructure.SplitLongLines SplitLongLines;
        public LanguageStructure.SplitSubtitle SplitSubtitle;
        public LanguageStructure.StartNumberingFrom StartNumberingFrom;
        public LanguageStructure.Statistics Statistics;
        public LanguageStructure.SubStationAlphaProperties SubStationAlphaProperties;
        public LanguageStructure.SubStationAlphaStyles SubStationAlphaStyles;
        public LanguageStructure.SubStationAlphaStylesCategoriesManager SubStationAlphaStylesCategoriesManager;
        public LanguageStructure.PointSync PointSync;
        public LanguageStructure.TextToSpeech TextToSpeech;
        public LanguageStructure.TimedTextSmpteTiming TimedTextSmpteTiming;
        public LanguageStructure.TransportStreamSubtitleChooser TransportStreamSubtitleChooser;
        public LanguageStructure.UnknownSubtitle UnknownSubtitle;
        public LanguageStructure.VerifyCompleteness VerifyCompleteness;
        public LanguageStructure.VisualSync VisualSync;
        public LanguageStructure.VobSubEditCharacters VobSubEditCharacters;
        public LanguageStructure.VobSubOcr VobSubOcr;
        public LanguageStructure.VobSubOcrCharacter VobSubOcrCharacter;
        public LanguageStructure.VobSubOcrCharacterInspect VobSubOcrCharacterInspect;
        public LanguageStructure.VobSubOcrNewFolder VobSubOcrNewFolder;
        public LanguageStructure.VobSubOcrSetItalicAngle VobSubOcrSetItalicAngle;
        public LanguageStructure.OcrPreprocessing OcrPreprocessing;
        public LanguageStructure.Watermark Watermark;
        public LanguageStructure.Waveform Waveform;
        public LanguageStructure.WaveformGenerateTimeCodes WaveformGenerateTimeCodes;
        public LanguageStructure.WebVttNewVoice WebVttNewVoice;
        public LanguageStructure.WebVttProperties WebVttProperties;
        public LanguageStructure.WebVttStyleManager WebVttStyleManager;
        public LanguageStructure.WhisperAdvanced WhisperAdvanced;

        public Language()
        {
            Name = "English";

            General = new LanguageStructure.General
            {
                Title = "Subtitle Edit",
                Version = "3.5",
                TranslatedBy = " ",
                CultureName = "en-US",
                HelpFile = string.Empty,
                Ok = "&OK",
                Cancel = "C&ancel",
                Yes = "Yes",
                No = "No",
                Close = "Close",
                Apply = "Apply",
                ApplyTo = "Apply to",
                None = "None",
                All = "All",
                Preview = "Preview",
                ShowPreview = "Show preview",
                HidePreview = "Hide preview",
                SubtitleFile = "Subtitle file",
                SubtitleFiles = "Subtitle files",
                AllFiles = "All files",
                VideoFiles = "Video files",
                Images = "Images",
                Fonts = "Fonts",
                AudioFiles = "Audio files",
                OpenSubtitle = "Open subtitle...",
                OpenVideoFile = "Open video file...",
                OpenVideoFileTitle = "Open video file...",
                NoVideoLoaded = "No video loaded",
                OnlineVideoFeatureNotAvailable = "Feature not available for online video",
                VideoInformation = "Video info",
                StartTime = "Start time",
                EndTime = "End time",
                Duration = "Duration",
                CharsPerSec = "Chars/sec",
                WordsPerMin = "Words/min",
                Actor = "Actor",
                Gap = "Gap",
                Region = "Region",
                Layer = "Layer",
                NumberSymbol = "#",
                Number = "Number",
                Text = "Text",
                HourMinutesSecondsDecimalSeparatorMilliseconds = "Hour:min:sec{0}ms",
                HourMinutesSecondsFrames = "Hour:min:sec:frames",
                XSeconds = "{0:0.0##} seconds",
                Bold = "Bold",
                Italic = "Italic",
                Underline = "Underline",
                Strikeout = "Strikeout",
                Visible = "Visible",
                FrameRate = "Frame rate",
                Name = "Name",
                SingleLineLengths = "Single line length:",
                TotalLengthX = "Total length: {0}",
                TotalLengthXSplitLine = "Total length: {0} (split line!)",
                SplitLine = "Split line!",
                NotAvailable = "N/A",
                FileNameXAndSize = "File name: {0} ({1})",
                ResolutionX = "Resolution: {0}",
                FrameRateX = "Frame rate: {0:0.0###}",
                TotalFramesX = "Total frames: {0:#,##0.##}",
                VideoEncodingX = "Video encoding: {0}",
                Overlap = "Overlap",
                OverlapPreviousLineX = "Overlap prev line ({0:#,##0.###})",
                OverlapX = "Overlap ({0:#,##0.###})",
                OverlapNextX = "Overlap next ({0:#,##0.###})",
                OverlapStartAndEnd = "Overlap start and end",
                Negative = "Negative",
                RegularExpressionIsNotValid = "Regular expression is not valid!",
                CurrentSubtitle = "Current subtitle",
                OriginalText = "Original text",
                OpenOriginalSubtitleFile = "Open original subtitle file...",
                PleaseWait = "Please wait...",
                SessionKey = "Session key",
                SessionKeyGenerate = "Generate new key",
                UserName = "Username",
                UserNameAlreadyInUse = "Username already in use",
                WebServiceUrl = "Webservice URL",
                IP = "IP",
                VideoWindowTitle = "Video - {0}",
                AudioWindowTitle = "Audio - {0}",
                ControlsWindowTitle = "Controls - {0}",
                Advanced = "Advanced",
                Style = "Style",
                StyleLanguage = "Style / Language",
                Character = "Character",
                Class = "Class",
                GeneralText = "General",
                LineNumber = "Line#",
                Before = "Before",
                After = "After",
                Size = "Size",
                Search = "Search",
                DeleteCurrentLine = "Delete current line",
                Width = "Width",
                Height = "Height",
                Collapse = "Collapse",
                ShortcutX = "Shortcut: {0}",
                ExampleX = "Example: {0}",
                ViewX = "View {0}",
                Reset = "Reset",
                Error = "Error",
                Warning = "Warning",
                UseLargerFontForThisWindow = "Use larger font for this window",
                ChangeLanguageFilter = "Change language filter...",
                MoreInfo = "More info",
            };

            About = new LanguageStructure.About
            {
                Title = "About Subtitle Edit",
                AboutText1 = "Subtitle Edit is Free Software under the GNU Public License." + Environment.NewLine +
                             "You may distribute, modify and use it freely." + Environment.NewLine +
                             Environment.NewLine +
                             "C# source code is available on https://github.com/SubtitleEdit/subtitleedit" + Environment.NewLine +
                             Environment.NewLine +
                             "Visit https://www.nikse.dk for the latest version." + Environment.NewLine +
                             Environment.NewLine +
                             "Suggestions are very welcome." + Environment.NewLine +
                             Environment.NewLine +
                             "Email: mailto:nikse.dk@gmail.com",
            };

            AddToNames = new LanguageStructure.AddToNames
            {
                Title = "Add to name list",
                Description = "Add to names/noise list (case sensitive)",
            };

            AddToOcrReplaceList = new LanguageStructure.AddToOcrReplaceList
            {
                Title = "Add to OCR replace list",
                Description = "Add pair to OCR replace list (case sensitive)",
            };

            AddToUserDictionary = new LanguageStructure.AddToUserDictionary
            {
                Title = "Add to user dictionary",
                Description = "Add word to user dictionary (not case sensitive)",
            };

            AddWaveform = new LanguageStructure.AddWaveform
            {
                Title = "Generate waveform data",
                GenerateWaveformData = "Generate waveform data",
                SourceVideoFile = "Source video file:",
                PleaseWait = "This may take a few minutes - please wait",
                FfmpegNotFound = "Subtitle Edit needs FFmpeg for extracting audio data to generate waveform." + Environment.NewLine +
                                         Environment.NewLine +
                                         "Do you want to download and use FFmpeg?",
                GeneratingPeakFile = "Generating peak file...",
                GeneratingSpectrogram = "Generating spectrogram...",
                ExtractingSeconds = "Extracting audio: {0:0.0} seconds",
                ExtractingMinutes = "Extracting audio: {0}:{1:00} minutes",
                WaveFileNotFound = "Could not find extracted wave file!" + Environment.NewLine +
                                   "This feature requires VLC media player 1.1.x or newer ({0}-bit)." + Environment.NewLine +
                                   Environment.NewLine +
                                   "Command line: {1} {2}",
                WaveFileMalformed = "{0} was unable to extract audio data to wave file!" + Environment.NewLine +
                                    Environment.NewLine +
                                    "Command line: {1} {2}" + Environment.NewLine +
                                    Environment.NewLine +
                                    "Note: Do check free disk space.",
                LowDiskSpace = "LOW DISK SPACE!",
                FreeDiskSpace = "{0} free",
                NoAudioTracksFoundGenerateEmptyWaveform = "No audio tracks found! Generate empty waveform?",
            };

            AddWaveformBatch = new LanguageStructure.AddWaveformBatch
            {
                Title = "Batch generate waveform data",
                ExtractTimeCodes = "Extract time codes with FFprobe",
                ExtractingAudio = "Extracting audio...",
                Calculating = "Calculating...",
                ExtractingTimeCodes = "Extracting time codes...",
                DetectingShotChanges = "Detecting shot changes...",
                Done = "Done",
                Error = "Error",
            };

            AdjustDisplayDuration = new LanguageStructure.AdjustDisplayDuration
            {
                Title = "Adjust durations",
                AdjustVia = "Adjust via",
                AddSeconds = "Add seconds",
                SetAsPercent = "Set as percent of duration",
                Percent = "Percent",
                Recalculate = "Recalculate",
                Seconds = "Seconds",
                Note = "Note: Display time will not overlap start time of next text",
                Fixed = "Fixed",
                Milliseconds = "Milliseconds",
                ExtendOnly = "Extend only",
                EnforceDurationLimits = "Enforce minimum and maximum duration",
                CheckShotChanges = "Don't extend past shot changes",
                BatchCheckShotChanges = "Respect shot changes (if available)",
            };

            ApplyDurationLimits = new LanguageStructure.ApplyDurationLimits
            {
                Title = "Apply duration limits",
                CheckShotChanges = "Don't extend past shot changes",
                FixesAvailable = "Fixes available: {0}",
                UnableToFix = "Unable to fix: {0}",
                BatchCheckShotChanges = "Respect shot changes (if available)",
            };

            AudioToText = new LanguageStructure.AudioToText
            {
                Title = "Audio to text",
                Info = "Generate text from audio via Vosk/Kaldi speech recognition",
                WhisperInfo = "Generate text from audio via Whisper speech recognition",
                Engine = "Engine",
                VoskWebsite = "Vosk website",
                WhisperWebsite = "Whisper website",
                Model = "Model",
                Models = "Models",
                LanguagesAndModels = "Languages and models",
                ChooseModel = "Choose model",
                ChooseLanguage = "Choose language",
                OpenModelsFolder = "Open models folder",
                LoadingVoskModel = "Loading Vosk speech recognition model...",
                Transcribing = "Transcribing audio to text...",
                TranscribingXOfY = "Transcribing audio to text - file {0} of {1}...",
                PostProcessing = "Post-processing...",
                UsePostProcessing = "Use post-processing (line merge, fix casing, punctuation, and more)",
                AutoAdjustTimings = "Auto adjust timings",
                BatchMode = "Batch mode",
                XFilesSavedToVideoSourceFolder = "{0} files saved to video source folder",
                KeepPartialTranscription = "Keep partial transcription",
                TranslateToEnglish = "Translate to English",
                RemoveTemporaryFiles = "Remove temporary files",
                SetCppConstMeFolder = "Set CPP/Const-me models folder...",
                OnlyRunPostProcessing = "Run only post-processing/adjust timings",
                DownloadFasterWhisperCuda = "Download cuBLAS and cuDNN libs for Faster-Whisper",
                NoTextFound = "No text found!",
                FixCasing = "Fix casing",
                AddPeriods = "Add periods",
                FixShortDuration = "Fix short duration",
            };

            AssaAttachments = new LanguageStructure.AssaAttachments
            {
                Title = "Advanced Sub Station Alpha attachments",
                AttachFiles = "Attach files...",
                FontsAndImages = "Fonts and images",
                FontName = "Font name:",
                IconName = "Icon name:",
                ImageName = "Image name ({0}x{1}):",
                ImageResized = "Image resized to fit current window",
                Font = "Font",
                Graphics = "Graphics",
                FilesSkippedX = "Files skipped: {0}",
                RemoveOneAttachment = "Remove one attachment?",
                RemoveXAttachments = "Remove {0} attachments?",
            };

            AssaOverrideTags = new LanguageStructure.AssaOverrideTags
            {
                ApplyCustomTags = "Apply custom override tags",
                AdvancedSelection = "Advanced selection",
                ApplyTo = "Apply to",
                History = "History",
                SelectedLinesX = "Selected lines: {0}",
                TagsToApply = "Tags to apply",
            };

            AssaProgressBarGenerator = new LanguageStructure.AssaProgressBarGenerator
            {
                Title = "Generate progress bar",
                Chapters = "Chapters",
                Progressbar = "Progress bar",
                SplitterHeight = "Splitter height",
                SplitterWidth = "Splitter width",
                XAdjustment = "X adjustment",
                YAdjustment = "Y adjustment",
                Position = "Position",
                TextAlignment = "Text alignment",
                RoundedCorners = "Rounded corners",
                SquareCorners = "Square corners",
                Bottom = "Bottom",
                Top = "Top",
                TakePosFromVideo = "Take video position",
            };

            AssaResolutionChanger = new LanguageStructure.AssaResolutionChanger
            {
                Title = "Change ASSA script resolution",
                SourceVideoRes = "Source video resolution",
                TargetVideoRes = "Target video resolution",
                ChangeResolutionMargins = "Change resolution for margin",
                ChangeResolutionFontSize = "Change resolution for font size",
                ChangeResolutionPositions = "Change resolution for position",
                ChangeResolutionDrawing = "Change resolution for drawing",
                SourceAndTargetEqual = "Source and target resolution is the same - nothing to do.",
            };

            ImageColorPicker = new LanguageStructure.ImageColorPicker
            {
                Title = "Image color picker",
                CopyColorHex = "Copy to clipboard as HEX color {0}",
                CopyColorAssa = "Copy to clipboard as ASSA color {0}",
                CopyColorRgb = "Copy to clipboard as RGB color {0}"
            };

            AssaSetBackgroundBox = new LanguageStructure.AssaSetBackgroundBox
            {
                Title = "Generate background box",
                Padding = "Padding",
                FillWidth = "Fill width",
                Drawing = "Drawing",
                BoxColor = "Box color",
                Radius = "Radius",
                Step = "Step",
                Spikes = "Spikes",
                Circle = "Circle",
                Bubbles = "Bubbles",
                MarginX = "MarginX",
                MarginY = "MarginY",
                OnlyDrawing = "Only drawing",
                DrawingFile = "Drawing file",
                ColorPickerSetLastColor = "Color picker last color is now: {0}",
            };

            AssaSetPosition = new LanguageStructure.AssaSetPosition
            {
                SetPosition = "Set position",
                VideoResolutionX = "Video resolution: {0}",
                StyleAlignmentX = "Style alignment: {0}",
                CurrentMousePositionX = "Mouse position: {0}",
                CurrentTextPositionX = "Text position: {0}",
                SetPosInfo = "Click on video to toggle set/move position",
                Clipboard = "Clipboard",
                ResolutionMissing = "PlayResX/PlayResY are not set - set the resolution now?",
                RotateXAxis = "Rotate {0} axis",
                DistortX = "Distort {0}",
            };

            AutoBreakUnbreakLines = new LanguageStructure.AutoBreakUnbreakLines
            {
                TitleAutoBreak = "Auto balance selected lines",
                TitleUnbreak = "Remove line breaks from selected lines",
                LinesFoundX = "Lines found: {0}",
                OnlyBreakLinesLongerThan = "Only break lines longer than",
                OnlyUnbreakLinesLongerThan = "Only un-break lines longer than",
            };

            BatchConvert = new LanguageStructure.BatchConvert
            {
                Title = "Batch convert",
                Input = "Input",
                Output = "Output",
                SaveInSourceFolder = "Save in source file folder",
                SaveInOutputFolder = "Save in output folder below",
                ConvertOptions = "Convert options",
                RemoveTextForHI = "Remove text for HI",
                ConvertColorsToDialog = "Convert colors to dialog",
                InputDescription = "Input files (browse or drag-n-drop)",
                Convert = "Convert",
                OverwriteFiles = "Overwrite files",
                RedoCasing = "Redo casing",
                RemoveFormatting = "Remove formatting tags",
                RemoveStyleActor = "Remove lines w style/actor",
                StyleActor = "Style/actor (separate with comma)",
                Status = "Status",
                Style = "Style...",
                UseStyleFromSource = "Use style from source",
                NothingToConvert = "Nothing to convert!",
                PleaseChooseOutputFolder = "Please choose output folder",
                NotConverted = "Failed",
                Converted = "Converted",
                Settings = "Settings",
                FixRtl = "Fix RTL",
                FixRtlAddUnicode = "Fix RTL via Unicode tags",
                FixRtlRemoveUnicode = "Remove RTL Unicode tags",
                FixRtlReverseStartEnd = "Reverse RTL start/end",
                SplitLongLines = "Split long lines",
                AutoBalance = "Auto balance lines",
                OverwriteOriginalFiles = "Overwrite original files (new extension if format is changed)",
                ScanFolder = "Scan folder...",
                Recursive = "Include sub folders",
                BridgeGaps = "Bridge gaps",
                PlainText = "Plain text",
                Ocr = "OCR...",
                AddFiles = "Add files...",
                Filter = "Filter",
                FilterSkipped = "Skipped by filter",
                FilterSrtNoUtf8BOM = "SubRip (.srt) files without UTF-8 BOM header",
                FilterMoreThanTwoLines = "More than two lines in one subtitle",
                FilterContains = "Text contains...",
                FilterFileNameContains = "File name contains...",
                LanguageCodeContains = "Language code (mkv/mp4) contains...",
                FixCommonErrorsErrorX = "Fix common errors: {0}",
                MultipleReplaceErrorX = "Multiple replace: {0}",
                AutoBalanceErrorX = "Auto balance: {0}",
                OffsetTimeCodes = "Offset time codes",
                TransportStreamSettings = "Transport Stream settings",
                TransportStreamOverrideXPosition = "Override original X position",
                TransportStreamOverrideYPosition = "Override original Y position",
                TransportStreamOverrideVideoSize = "Override original video size",
                TransportStreamFileNameEnding = "File name ending",
                TransportStreamSettingsButton = "TS settings...",
                RemoveLineBreaks = "Remove line-breaks",
                DeleteLines = "Delete lines",
                TryToUseSourceEncoding = "Try to use source encoding",
                DeleteFirstLines = "Delete first lines",
                DeleteLastLines = "Delete last lines",
                DeleteContaining = "Delete lines containing",
                MkvLanguageInOutputFileName = "\"Language\" in output file name",
                MkvLanguageInOutputFileNameX = "Matroska (.mkv) \"Language\" in output file name: {0}",
                MkvLanguageStyleTwoLetter = "Two letter language code",
                MkvLanguageStyleThreeLetter = "Three letter language code",
                MkvLanguageStyleEmpty = "No language code",
                SearchFolderScanVideo = "Also scan video files in \"Search folder\" (slow)",
            };

            BeautifyTimeCodes = new LanguageStructure.BeautifyTimeCodes
            {
                Title = "Beautify time codes",
                TitleSelectedLines = "Beautify time codes ({0} selected lines)",
                GroupTimeCodes = "Time codes",
                AlignTimeCodes = "Align time codes to frame time codes",
                ExtractExactTimeCodes = "Use FFprobe to extract exact frame time codes",
                ExtractTimeCodes = "Extract time codes",
                CancelTimeCodes = "Cancel",
                GroupShotChanges = "Shot changes",
                SnapToShotChanges = "Snap cues to shot changes",
                ImportShotChanges = "Generate / import shot changes...",
                EditProfile = "Edit profile...",
                NoTimeCodesLoaded = "No time codes loaded",
                XTimeCodesLoaded = "{0} time codes loaded",
                NoTimeCodesLoadedError =
                    "You've selected to extract exact frame time codes, but there are no time codes loaded." +
                    Environment.NewLine + Environment.NewLine +
                    "Please click \"{0}\" to extract the time codes first, or disable this option.",
                NoShotChangesLoaded = "No shot changes loaded",
                XShotChangesLoaded = "{0} shot changes loaded",
                NoShotChangesLoadedError =
                    "You've selected to snap cues to shot changes, but there are no shot changes loaded." +
                    Environment.NewLine + Environment.NewLine +
                    "Please click \"{0}\" to generate or import shot changes first, or disable this option.",
                BatchAlignTimeCodes = "Align time codes to frame time codes",
                BatchUseExactTimeCodes = "Use exact time codes (if available)",
                BatchSnapToShotChanges = "Snap cues to shot changes (if available)",
                UnfixableParagraphsTitle = "Review not fully chained subtitles",
                UnfixableParagraphsInstructions = "Some subtitles were not fully chained in accordance with your profile, most likely due to too tightly clustered shot changes (possibly false positives)." +
                    Environment.NewLine + Environment.NewLine +
                    "You might want to review these cases manually to ensure your cues are snapped to the correct (real) shot changes.",
                UnfixableParagraphsColumnParagraphs = "Lines",
                UnfixableParagraphsColumnParagraphsFormat = "#{0} – #{1}",
                UnfixableParagraphsColumnGap = "Gap (frames)"
            };

            BeautifyTimeCodesProfile = new LanguageStructure.BeautifyTimeCodesProfile
            {
                Title = "Edit profile",
                LoadPreset = "Load preset...",
                PresetDefault = "Default",
                PresetNetflix = "Netflix",
                PresetSDI = "SDI",
                CreateSimple = "Simple mode...",
                General = "General",
                Gap = "Gap:",
                GapSuffix = "frames (will overwrite custom settings)",
                InCues = "In cues",
                SubtitlePreviewText = "Subtitle text.",
                Zones = "Zones:",
                OutCues = "Out cues",
                ConnectedSubtitles = "Connected subtitles",
                InCueClosest = "In cue is closest",
                OutCueClosest = "Out cue is closest",
                TreadAsConnected = "Treat as connected if gap is smaller than:",
                Milliseconds = "ms",
                Chaining = "Chaining",
                InCueOnShot = "In cue on shot change",
                OutCueOnShot = "Out cue on shot change",
                CheckGeneral = "Still enforce General rules when unaffected",
                MaxGap = "Max. gap:",
                ShotChangeBehavior = "If there is a shot change in between:",
                DontChain = "Don't chain",
                ExtendCrossingShotChange = "Extend, crossing shot change",
                ExtendUntilShotChange = "Extend until shot change",
                ResetWarning =
                    "This will reset your current profile and replace all values with those of the selected preset. This cannot be undone." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "Do you want to continue?",
                CreateSimpleTitle = "Create simple",
                CreateSimpleInstruction =
                    "Enter these basic rules, and the current profile will be updated accordingly.",
                CreateSimpleGapInstruction = "The minimum amount of space between subtitles.",
                CreateSimpleInCues = "In cues should be:",
                CreateSimpleInCues0Frames = "On the shot change",
                CreateSimpleInCues1Frames = "1 frame after the shot change",
                CreateSimpleInCues2Frames = "2 frames after the shot change",
                CreateSimpleInCues3Frames = "3 frames after the shot change",
                CreateSimpleOutCues = "Out cues should be:",
                CreateSimpleOutCues0Frames = "On the shot change",
                CreateSimpleOutCues1Frames = "1 frame before the shot change",
                CreateSimpleOutCues2Frames = "2 frames before the shot change",
                CreateSimpleOutCues3Frames = "3 frames before the shot change",
                CreateSimpleOutCuesGap = "Minimum gap before the shot change",
                CreateSimpleSnapClosestCue =
                    "For connected subtitles, snap the in or out cue to a shot change based on which one is closer",
                CreateSimpleMaxOffset = "Max. offset:",
                CreateSimpleMaxOffsetInstruction =
                    "Cues within this distance from shot changes will be snapped to the shot change.",
                CreateSimpleSafeZone = "Safe zone:",
                CreateSimpleSafeZoneInstruction = "Cues within this distance from shot changes will be moved away from the shot change.",
                CreateSimpleChainingGap = "Max. chaining gap:",
                CreateSimpleChainingGapInstruction =
                    "If the space between two subtitles is smaller than this amount, the subtitles will be connected.",
                CreateSimpleChainingGapAfterShotChanges = "After an out cue on a shot change, the gap may be smaller",
                CreateSimpleChainingToolTip =
                    "Chaining subtitles is recommended to ensure a consistent \"rhythm\" in the \"flashing\" of the subtitles." +
                    Environment.NewLine +
                    "This offers a more relaxed viewing experience." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "After chaining, subtitles are either connected (i.e. a subtitle disappears and a new subtitle appears immediately after a slight pause) or not." +
                    Environment.NewLine +
                    "This gives the viewer some sense on when they can shift their focus back to the screen." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "The length of the chaining gap can be a bit smaller right after a subtitle disappears on a shot change, because the changing shot \"resets\" the image in a way." +
                    Environment.NewLine +
                    "We leverage the intrinsic rhythm of the image.",
                CreateSimpleLoadNetflixRules = "Load Netflix rules",
                Frames = "frames",
                Maximum = "Max.",
                GapInMsFormat = "{0} ms @ {1} FPS",
                OffsetSafeZoneError = "The safe zone should be larger than the max. offset.",
            };

            BinEdit = new LanguageStructure.BinEdit
            {
                ImportImage = "Import image...",
                ExportImage = "Export image...",
                SetText = "Set text...",
                QuickOcr = "Quick OCR texts (for overview only)",
                ResizeBitmaps = "Resize images...",
                ChangeBrightness = "Adjust brightness...",
                ChangeAlpha = "Adjust alpha...",
                ResizeBitmapsForSelectedLines = "Resize images for selected lines...",
                ChangeColorForSelectedLines = "Change color for selected lines...",
                ChangeBrightnessForSelectedLines = "Adjust brightness for selected lines...",
                ChangeAlphaForSelectedLines = "Adjust alpha for selected lines...",
                AlignSelectedLines = "Align selected lines",
                CenterSelectedLines = "Center selected lines (horizontally, keep vertical position)",
                TopAlignSelectedLines = "Top align selected lines (keep horizontal position)",
                BottomAlignSelectedLines = "Bottom align selected lines (keep horizontal position)",
                ToggleForcedSelectedLines = "Toggle \"Forced\" for selected lines",
                SelectForcedLines = "Select forced lines",
                SelectNonForcedLines = "Select non-forced lines",
                SizeXY = "Size: {0}x{1}",
                SetAspectRatio11 = "Set aspect ratio 1:1",
                ChangeBrightnessTitle = "Adjust brightness",
                BrightnessX = "Brightness: {0}%",
                ResizeTitle = "Resize images",
                ResizeX = "Resize: {0}%",
                ChangeAlphaTitle = "Adjust alpha",
                AlphaX = "Alpha: {0}%",
            };

            Bookmarks = new LanguageStructure.Bookmarks
            {
                GoToBookmark = "Go to bookmark",
                EditBookmark = "Edit bookmark",
                AddBookmark = "Add bookmark",
            };

            ChangeCasing = new LanguageStructure.ChangeCasing
            {
                Title = "Change casing",
                ChangeCasingTo = "Change casing to",
                NormalCasing = "Normal casing. Sentences begin with uppercase letter.",
                FixNamesCasing = @"Fix names casing (via Dictionaries\names.xml)",
                FixOnlyNamesCasing = @"Fix only names casing (via Dictionaries\names.xml)",
                OnlyChangeAllUppercaseLines = "Only change all uppercase lines.",
                AllUppercase = "ALL UPPERCASE",
                AllLowercase = "all lowercase",
                ProperCase = "Proper Case",
            };

            ChangeCasingNames = new LanguageStructure.ChangeCasingNames
            {
                Title = "Change casing - Names",
                NamesFoundInSubtitleX = "Names found in subtitle: {0}",
                Enabled = "Enabled",
                Name = "Name",
                LinesFoundX = "Lines found: {0}",
                ExtraNames = "Add extra names (separate by comma, one-time use only)",
            };

            ChangeFrameRate = new LanguageStructure.ChangeFrameRate
            {
                Title = "Change frame rate",
                ConvertFrameRateOfSubtitle = "Convert frame rate of subtitle",
                FromFrameRate = "From frame rate",
                ToFrameRate = "To frame rate",
                FrameRateNotCorrect = "Frame rate is not correct",
                FrameRateNotChanged = "Frame rate is the same - nothing to convert",
            };

            ChangeSpeedInPercent = new LanguageStructure.ChangeSpeedInPercent
            {
                Title = "Adjust speed in percent",
                TitleShort = "Adjust speed",
                Info = "Change speed of subtitle in percent",
                Custom = "Custom",
                ToDropFrame = "To drop frame",
                FromDropFrame = "From drop frame",
                AllowOverlap = "Allow overlap",
            };

            CheckForUpdates = new LanguageStructure.CheckForUpdates
            {
                Title = "Check for updates",
                CheckingForUpdates = "Checking for updates...",
                CheckingForUpdatesFailedX = "Checking for updates failed: {0}",
                CheckingForUpdatesNoneAvailable = "You're using the latest version of Subtitle Edit :)",
                CheckingForUpdatesNewVersion = "New version available!",
                InstallUpdate = "Go to download page",
                NoUpdates = "Don't update",
                OnePluginsHasAnUpdate = "One plugin has an update -",
                XPluginsHasAnUpdate = "{0} plugins have an updates -",
                Update = "update",
            };

            ChooseAudioTrack = new LanguageStructure.ChooseAudioTrack
            {
                Title = "Choose audio track",
            };

            ChooseEncoding = new LanguageStructure.ChooseEncoding
            {
                Title = "Choose encoding",
                CodePage = "Code page",
                DisplayName = "Display name",
                PleaseSelectAnEncoding = "Please select an encoding",
            };

            ChooseLanguage = new LanguageStructure.ChooseLanguage
            {
                Title = "Choose language",
                Language = "Language",
            };

            ColorChooser = new LanguageStructure.ColorChooser
            {
                Title = "Choose color",
                Red = "Red",
                Green = "Green",
                Blue = "Blue",
                Alpha = "Alpha",
            };

            ColumnPaste = new LanguageStructure.ColumnPaste
            {
                Title = "Column paste",
                ChooseColumn = "Choose column",
                OverwriteShiftCellsDown = "Overwrite/Shift cells down",
                Overwrite = "Overwrite",
                ShiftCellsDown = "Shift cells down",
                TimeCodesOnly = "Time codes only",
                TextOnly = "Text only",
                OriginalTextOnly = "Original text only",
            };

            CompareSubtitles = new LanguageStructure.CompareSubtitles
            {
                Title = "Compare subtitles",
                Reload = "Reload",
                PreviousDifference = "&Previous difference",
                NextDifference = "&Next difference",
                SubtitlesNotAlike = "Subtitles have no similarities",
                XNumberOfDifference = "Number of differences: {0}",
                XNumberOfDifferenceAndPercentChanged = "Number of differences: {0} ({1:0.##}% of words changed)",
                XNumberOfDifferenceAndPercentLettersChanged = "Number of differences: {0} ({1:0.##}% of letters changed)",
                ShowOnlyDifferences = "Show only differences",
                IgnoreLineBreaks = "Ignore line breaks",
                IgnoreWhitespace = "Ignore whitespace",
                IgnoreFormatting = "Ignore formatting",
                OnlyLookForDifferencesInText = "Only look for differences in text",
                CannotCompareWithImageBasedSubtitles = "Cannot compare with image-based subtitles",
            };

            ConvertColorsToDialog = new LanguageStructure.ConvertColorsToDialog
            {
                Title = "Convert colors to dialog",
                RemoveColorTags = "Remove color tags",
                AddNewLines = "Place every dash on new line",
                ReBreakLines = "Re-break lines",
            };

            DCinemaProperties = new LanguageStructure.DCinemaProperties
            {
                Title = "D-Cinema properties (interop)",
                TitleSmpte = "D-Cinema properties (SMPTE)",
                SubtitleId = "Subtitle ID",
                GenerateId = "Generate ID",
                MovieTitle = "Movie title",
                ReelNumber = "Reel number",
                Language = "Language",
                IssueDate = "Issue date",
                EditRate = "Edit rate",
                TimeCodeRate = "Time code rate",
                StartTime = "Start time",
                Font = "Font",
                FontId = "ID",
                FontUri = "URI",
                FontColor = "Color",
                FontEffect = "Effect",
                FontEffectColor = "Effect color",
                FontSize = "Size",
                TopBottomMargin = "Top/bottom margin",
                FadeUpTime = "Fade up time",
                FadeDownTime = "Fade down time",
                ZPosition = "Z-position",
                ZPositionHelp = "Positive numbers move text away, negative numbers move text closer, if z-position is zero then it's 2D",
                ChooseColor = "Choose color...",
                Generate = "Generate",
                GenerateNewIdOnSave = "Generate new ID on save",
            };

            DurationsBridgeGaps = new LanguageStructure.DurationsBridgeGaps
            {
                Title = "Bridge small gaps between subtitles",
                GapsBridgedX = "Number of small gaps bridged: {0}",
                GapToNext = "Gap to next in seconds",
                GapToNextFrames = "Gap to next in frames",
                BridgeGapsSmallerThanXPart1 = "Bridge gaps smaller than",
                BridgeGapsSmallerThanXPart2 = "milliseconds",
                BridgeGapsSmallerThanXPart1Frames = "Bridge gaps smaller than",
                BridgeGapsSmallerThanXPart2Frames = "frames",
                MinMillisecondsBetweenLines = "Min. milliseconds between lines",
                MinFramesBetweenLines = "Min. frames between lines",
                ProlongEndTime = "Previous text takes all gap time",
                DivideEven = "Texts divide gap time",
            };

            DvdSubRip = new LanguageStructure.DvdSubRip
            {
                Title = "Rip subtitles from IFO/VOBs (DVD)",
                DvdGroupTitle = "DVD files/info",
                IfoFile = "IFO file",
                IfoFiles = "IFO files",
                VobFiles = "VOB files",
                Add = "Add...",
                Remove = "Remove",
                Clear = "Clear",
                MoveUp = "Move up",
                MoveDown = "Move down",
                Languages = "Languages",
                PalNtsc = "PAL/NTSC",
                Pal = "PAL (25fps)",
                Ntsc = "NTSC (29.97fps)",
                StartRipping = "Start ripping",
                Abort = "Abort",
                AbortedByUser = "Aborted by user",
                ReadingSubtitleData = "Reading subtitle data...",
                RippingVobFileXofYZ = "Ripping vob file {1} of {2}: {0}",
                WrongIfoType = "IFO type is '{0}' and not 'DVDVIDEO-VTS'.{1}Try another file than {2}",
            };

            DvdSubRipChooseLanguage = new LanguageStructure.DvdSubRipChooseLanguage
            {
                Title = "Choose language",
                ChooseLanguageStreamId = "Choose language (stream-id)",
                UnknownLanguage = "Unknown language",
                SubtitleImageXofYAndWidthXHeight = "Subtitle image {0}/{1} - {2}x{3}",
                SubtitleImage = "Subtitle image",
            };

            EbuSaveOptions = new LanguageStructure.EbuSaveOptions
            {
                Title = "EBU save options",
                GeneralSubtitleInformation = "General subtitle information",
                CodePageNumber = "Code page number",
                DiskFormatCode = "Disk format code",
                DisplayStandardCode = "Display standard code",
                ColorRequiresTeletext = "Colors require teletext!",
                AlignmentRequiresTeletext = "Alignment requires teletext!",
                TeletextCharsShouldBe38 = "'Max# of chars per row' for teletext should be 38!",
                CharacterCodeTable = "Character table",
                LanguageCode = "Language code",
                OriginalProgramTitle = "Original program title",
                OriginalEpisodeTitle = "Original episode title",
                TranslatedProgramTitle = "Translated program title",
                TranslatedEpisodeTitle = "Translated episode title",
                TranslatorsName = "Translator's name",
                SubtitleListReferenceCode = "Subtitle list reference code",
                CountryOfOrigin = "Country of origin",
                TimeCodeStatus = "Time code status",
                TimeCodeStartOfProgramme = "Time code: Start of programme",
                RevisionNumber = "Revision number",
                MaxNoOfDisplayableChars = "Max# of chars per row",
                MaxNumberOfDisplayableRows = "Max# of rows",
                DiskSequenceNumber = "Disk sequence number",
                TotalNumberOfDisks = "Total number of disks",
                Import = "Import...",
                TextAndTimingInformation = "Text and timing information",
                JustificationCode = "Justification code",
                VerticalPosition = "Vertical position",
                MarginTop = "Margin top (for top aligned subtitles)",
                MarginBottom = "Margin bottom (for bottom aligned subtitles)",
                NewLineRows = "Number of rows added by a new line",
                Teletext = "Teletext",
                UseBox = "Use box around text",
                DoubleHeight = "Use double height for text",
                Errors = "Errors",
                ErrorsX = "Errors: {0}",
                MaxLengthError = "Line {0} exceeds max length ({1}) by {2}: {3}",
                TextUnchangedPresentation = "Unchanged presentation",
                TextLeftJustifiedText = "Left justified text",
                TextCenteredText = "Centered text",
                TextRightJustifiedText = "Right justified text",
                UseBoxForOneNewLine = "Check 'Use box around text' for only one new-line",
            };

            EffectKaraoke = new LanguageStructure.EffectKaraoke
            {
                Title = "Karaoke effect",
                ChooseColor = "Choose color:",
                TotalSeconds = "Total seconds:",
                EndDelayInSeconds = "End delay in seconds:",
                WordEffect = "Word effect",
                CharacterEffect = "Character effect",
            };

            EffectTypewriter = new LanguageStructure.EffectTypewriter
            {
                Title = "Typewriter effect",
                TotalSeconds = "Total seconds:",
                EndDelayInSeconds = "End delay in seconds:",
            };

            ExportCustomText = new LanguageStructure.ExportCustomText
            {
                Title = "Export custom text format",
                Formats = "Formats",
                New = "New",
                Edit = "Edit",
                Delete = "Delete",
                SaveAs = "S&ave as...",
                SaveSubtitleAs = "Save subtitle as...",
                SubtitleExportedInCustomFormatToX = "Subtitle exported in custom format to: {0}",
            };

            ExportCustomTextFormat = new LanguageStructure.ExportCustomTextFormat
            {
                Title = "Custom text format template",
                Template = "Template",
                Header = "Header",
                TextLine = "Text line (paragraph)",
                TimeCode = "Time code",
                NewLine = "New line",
                Footer = "Footer",
                FileExtension = "File extension",
                DoNotModify = "[Do not modify]",
            };

            ExportFcpXmlAdvanced = new LanguageStructure.ExportFcpXmlAdvanced
            {
                Title = "Export Final Cut Pro XML advanced",
                FontName = "Font name",
                FontSize = "Font size",
                FontFace = "Font face",
                FontFaceRegular = "Regular",
                Alignment = "Alignment",
                Baseline = "Baseline",
            };

            ExportPngXml = new LanguageStructure.ExportPngXml
            {
                Title = "Export BDN XML/PNG",
                ImageSettings = "Image settings",
                SimpleRendering = "Simple rendering",
                AntiAliasingWithTransparency = "Anti-aliasing with transparency",
                Text3D = "3D",
                ImagePrefix = "Image prefix",
                SideBySide3D = "Half-side-by-side",
                HalfTopBottom3D = "Half-Top/Bottom",
                Depth = "Depth",
                BorderColor = "Border color",
                BorderWidth = "Border width",
                BorderStyle = "Border style",
                BorderStyleOneBox = "One box",
                BorderStyleBoxForEachLine = "Box for each line",
                BorderStyleNormalWidthX = "Normal, width={0}",
                ShadowColor = "Shadow color",
                ShadowWidth = "Shadow width",
                Transparency = "Alpha",
                ImageFormat = "Image format",
                FullFrameImage = "Full frame image",
                ExportAllLines = "Export all lines...",
                FontColor = "Font color",
                FontFamily = "Font family",
                FontSize = "Font size",
                XImagesSavedInY = "{0:#,##0} images saved in {1}",
                VideoResolution = "Video res",
                Align = "Align",
                Left = "Left",
                Center = "Center",
                Right = "Right",
                CenterLeftJustify = "Center, left justify",
                CenterLeftJustifyDialogs = "Center, left justify dialog",
                CenterTopJustify = "Center, top justify",
                CenterRightJustify = "Center, right justify",
                BottomMargin = "Bottom margin",
                LeftRightMargin = "Left/right margin",
                SaveBluRaySupAs = "Choose Blu-ray sup file name",
                SaveVobSubAs = "Choose VobSub file name",
                SaveFabImageScriptAs = "Choose FAB image script file name",
                SaveDvdStudioProStlAs = "Choose DVD Studio Pro STL file name",
                SaveDigitalCinemaInteropAs = "Choose Digital Cinema Interop file name",
                SaveDigitalCinemaSmpte2014 = "Choose Digital Cinema SMPTE 2014 file name",
                SavePremiereEdlAs = "Choose Premiere EDL file name",
                SaveFcpAs = "Choose Final Cut Pro xml file name",
                SaveDostAs = "Choose DoStudio dost file name",
                SomeLinesWereTooLongX = "Some lines were too long:" + Environment.NewLine + "{0}",
                LineHeight = "Line height",
                BoxSingleLine = "Box - single line",
                BoxMultiLine = "Box - multi line",
                Forced = "Forced",
                ChooseBackgroundColor = "Choose background color",
                SaveImageAs = "Save image as...",
                FcpUseFullPathUrl = "Use full image path URL in FCP xml",
            };

            ExportText = new LanguageStructure.ExportText
            {
                Title = "Export text",
                Preview = "Preview",
                ExportOptions = "Export options",
                FormatText = "Format text",
                None = "None",
                MergeAllLines = "Merge all lines",
                UnbreakLines = "Unbreak lines",
                RemoveStyling = "Remove styling",
                ShowLineNumbers = "Show line numbers",
                AddNewLineAfterLineNumber = "Add new line after line number",
                ShowTimeCode = "Show time code",
                AddNewLineAfterTimeCode = "Add new line after time code",
                AddNewLineAfterTexts = "Add new line after text",
                AddNewLineBetweenSubtitles = "Add new line between subtitles",
                TimeCodeFormat = "Time code format",
                Srt = ".srt",
                Milliseconds = "Milliseconds",
                HHMMSSFF = "HH:MM:SS:FF",
                TimeCodeSeparator = "Time code separator",
            };

            ExtractDateTimeInfo = new LanguageStructure.ExtractDateTimeInfo
            {
                Title = "Generate time as text",
                OpenVideoFile = "Choose video file to extract date/time info from",
                StartFrom = "Start from",
                DateTimeFormat = "Date/time format",
                Example = "Example",
                GenerateSubtitle = "&Generate subtitle",
            };

            FindDialog = new LanguageStructure.FindDialog
            {
                Title = "Find",
                FindNext = "&Find next",
                FindPrevious = "Find &previous",
                Normal = "&Normal",
                CaseSensitive = "&Case sensitive",
                RegularExpression = "Regular e&xpression",
                WholeWord = "&Whole word",
                Count = "Coun&t",
                XNumberOfMatches = "{0:#,##0} matches",
                OneMatch = "One match",
            };

            FindSubtitleLine = new LanguageStructure.FindSubtitleLine
            {
                Title = "Find subtitle line",
                Find = "&Find",
                FindNext = "Find &next",
            };

            FixCommonErrors = new LanguageStructure.FixCommonErrors
            {
                Title = "Fix common errors",
                Step1 = "Step 1/2 - Choose which errors to fix",
                WhatToFix = "What to fix",
                Example = "Example",
                SelectAll = "Select all",
                InverseSelection = "Invert selection",
                Back = "< &Back",
                Next = "&Next >",
                Step2 = "Step 2/2 - Verify fixes",
                Fixes = "Fixes",
                Log = "Log",
                Function = "Function",
                RemovedEmptyLine = "Remove empty line",
                RemovedEmptyLineAtTop = "Remove empty line at top",
                RemovedEmptyLineAtBottom = "Remove empty line at bottom",
                RemovedEmptyLineInMiddle = "Remove empty line in middle",
                RemovedEmptyLinesUnusedLineBreaks = "Remove empty lines/unused line breaks",
                FixOverlappingDisplayTimes = "Fix overlapping display times",
                FixShortDisplayTimes = "Fix short display times",
                FixLongDisplayTimes = "Fix long display times",
                FixShortGaps = "Fix short gaps",
                FixInvalidItalicTags = "Fix invalid italic tags",
                RemoveUnneededSpaces = "Remove unneeded spaces",
                RemoveUnneededPeriods = "Remove unneeded periods",
                FixCommas = "Fix commas",
                FixMissingSpaces = "Fix missing spaces",
                BreakLongLines = "Break long lines",
                RemoveLineBreaks = "Remove line breaks in short texts with only one sentence",
                RemoveLineBreaksAll = "Remove line breaks in short texts (all except dialogs)",
                RemoveLineBreaksPixelWidth = "Unbreak subtitles that can fit on one line (pixel width)",
                FixUppercaseIInsideLowercaseWords = "Fix uppercase 'i' inside lowercase words (OCR error)",
                FixDoubleApostrophes = "Fix double apostrophe characters ('') to a single quote (\")",
                AddPeriods = "Add period after lines where next line starts with uppercase letter",
                StartWithUppercaseLetterAfterParagraph = "Start with uppercase letter after paragraph",
                StartWithUppercaseLetterAfterPeriodInsideParagraph = "Start with uppercase letter after period inside paragraph",
                StartWithUppercaseLetterAfterColon = "Start with uppercase letter after colon/semicolon",
                CommonOcrErrorsFixed = "Common OCR errors fixed (OcrReplaceList file used): {0}",
                RemoveSpaceBetweenNumber = "Remove space between numbers",
                BreakDialogsOnOneLine = "Split dialogs on one line",
                RemoveDialogFirstInNonDialogs = "Remove start dash in first line for non-dialogs",
                NormalizeStrings = "Normalize strings",
                FixLowercaseIToUppercaseI = "Fix alone lowercase 'i' to 'I' (English)",
                FixTurkishAnsi = "Fix Turkish ANSI (Icelandic) letters to Unicode",
                FixDanishLetterI = "Fix Danish letter 'i'",
                FixSpanishInvertedQuestionAndExclamationMarks = "Fix Spanish inverted question and exclamation marks",
                AddMissingQuote = "Add missing quote (\")",
                AddMissingQuotes = "Add missing quotes (\")",
                RemoveHyphensSingleLine = "Remove dialog dashes in single lines",
                FixHyphensInDialogs = "Fix dash in dialogs via style: {0}",
                AddMissingQuotesExample = "\"How are you? -> \"How are you?\"",
                XMissingQuotesAdded = "Missing quotes added: {0}",
                Fix3PlusLine = "Fix subtitle with more than two lines",
                Fix3PlusLines = "Fix subtitles with more than two lines",
                Analysing = "Analyzing...",
                NothingToFix = "Nothing to fix :)",
                FixesFoundX = "Fixes found: {0}",
                XFixesApplied = "Fixes applied: {0}",
                NothingFixableBut = "Nothing could be fixed automatically. The subtitle contains errors - see log for details",
                XFixedBut = "{0} issue(s) fixed but the subtitle still contains errors - see log for details",
                XCouldBeFixedBut = "{0} issue(s) could be fixed but the subtitle will still contain errors - see log for details",
                FixFirstLetterToUppercaseAfterParagraph = "Fix first letter to uppercase after paragraph",
                MergeShortLine = "Merge short line (single sentence)",
                MergeShortLineAll = "Merge short line (all except dialogs)",
                UnbreakShortLinePixelWidth = "Unbreak short line (pixel width)",
                BreakLongLine = "Break long line",
                FixLongDisplayTime = "Fix long display time",
                FixInvalidItalicTag = "Fix invalid italic tag",
                FixShortDisplayTime = "Fix short display time",
                FixOverlappingDisplayTime = "Fix overlapping display time",
                FixShortGap = "Fix short gap",
                FixInvalidItalicTagsExample = "<i>What do I care.<i> -> <i>What do I care.</i>",
                RemoveUnneededSpacesExample = "Hey   you , there. -> Hey you, there.",
                RemoveUnneededPeriodsExample = "Hey you!. -> Hey you!",
                FixMissingSpacesExample = "Hey.You. -> Hey. You.",
                FixUppercaseIInsideLowercaseWordsExample = "The earth is fIat. -> The earth is flat.",
                FixLowercaseIToUppercaseIExample = "What do i care. -> What do I care.",
                StartTimeLaterThanEndTime = "Text number {0}: Start time is later than end time: {4}{1} -> {2} {3}",
                UnableToFixStartTimeLaterThanEndTime = "Unable to fix text number {0}: Start time is later than end time: {1}",
                XFixedToYZ = "{0} fixed to: {1}{2}",
                UnableToFixTextXY = "Unable to fix text number {0}: {1}",
                UnneededSpace = "Unneeded space",
                UnneededPeriod = "Unneeded period",
                FixMissingSpace = "Fix missing space",
                FixUppercaseIInsideLowercaseWord = "Fix uppercase 'i' inside lowercase word",
                FixMissingPeriodAtEndOfLine = "Add missing period at end of line",
                ApplyFixes = "A&pply selected fixes",
                RefreshFixes = "Refresh available fixes",
                FixDoubleDash = "Fix '--' -> '...'",
                FixDoubleGreaterThan = "Remove '>>'",
                FixEllipsesStart = "Remove leading '...'",
                FixMissingOpenBracket = "Fix missing [ or ( in line",
                FixMusicNotation = "Replace music symbols (e.g. âTª) with preferred symbol",
                FixDoubleDashExample = "'Whoa-- um yeah!' -> 'Whoa... um yeah!'",
                FixDoubleGreaterThanExample = "'>> Robert: Sup dude!' -> 'Robert: Sup dude!'",
                FixEllipsesStartExample = "'... and then we' -> 'and then we'",
                FixMissingOpenBracketExample = "'clanks] Look out!' -> '[clanks] Look out!'",
                FixMusicNotationExample = "'âTª sweet dreams are' -> '♫ sweet dreams are'",
                AutoBreak = "Auto &br",
                Unbreak = "&Unbreak",
                FixCommonOcrErrors = "Fix common OCR errors (using OCR replace list)",
                NumberOfImportantLogMessages = "{0} important log messages!",
                FixedOkXY = "Fixed and OK - '{0}': {1}",
                FixOcrErrorExample = "D0n't -> Don't",
                FixSpaceBetweenNumbersExample = "1 100 -> 1100",
                FixDialogsOneLineExample = "Hi John! - Hi Ida! -> Hi John!<br />- Hi Ida!",
                RemoveDialogFirstInNonDialogsExample = "- How are you? -> How are you?",
                SelectDefault = "Select default",
                SetDefault = "Set current fixes as default",
                FixContinuationStyleX = "Fix continuation style: {0}",
                FixUnnecessaryLeadingDots = "Remove unnecessary leading dots",
            };

            GenerateBlankVideo = new LanguageStructure.GenerateBlankVideo
            {
                Title = "Generate blank video file",
                CheckeredImage = "Checkered image",
                DurationInMinutes = "Duration in minutes",
                SolidColor = "Solid color",
                Background = "Background",
                FfmpegParameters = "Run FFmpeg with the following parameters:",
                GenerateWithFfmpegParametersPrompt = "Generate - prompt FFmpeg parameters",
            };

            GenerateVideoWithBurnedInSubs = new LanguageStructure.GenerateVideoWithBurnedInSubs
            {
                Title = "Generate video with burned-in subtitle",
                InfoAssaOff = "Note: Advanced SubStation Alpha styling supported.",
                InfoAssaOn = "Note: Advanced SubStation Alpha styling will be used :)",
                XGeneratedWithBurnedInSubsInX = "\"{0}\" generated with burned-in subtitle in {1}.",
                TimeRemainingMinutes = "Time remaining: {0} minutes",
                TimeRemainingOneMinute = "Time remaining: One minute",
                TimeRemainingSeconds = "Time remaining: {0} seconds",
                TimeRemainingAFewSeconds = "Time remaining: A few seconds",
                TimeRemainingMinutesAndSeconds = "Time remaining: {0} minutes and {1} seconds",
                TimeRemainingOneMinuteAndSeconds = "Time remaining: One minute and {0} seconds",
                TargetFileName = "Target file name: {0}",
                TargetFileSize = "Target file size (requires 2 pass encoding)",
                FileSizeMb = "File size in MB",
                PassX = "Pass {0}",
                Encoding = "Encoding",
                BitRate = "Bit rate",
                TotalBitRateX = "Total bit rate: {0}",
                SampleRate = "Sample rate",
                Audio = "Audio",
                Stereo = "Stereo",
                Preset = "Preset",
                PixelFormat = "Pixel format",
                Crf = "CRF",
                TuneFor = "Tune for",
                AlignRight = "Align right",
                GetStartPosition = "Get start position",
                GetEndPosition = "Get end position",
                UseSource = "Use source",
                UseSourceResolution = "Use source resolution",
                OutputSettings = "Output file/folder...",
            };

            GenerateVideoWithEmbeddedSubs = new LanguageStructure.GenerateVideoWithEmbeddedSubs
            {
                Title = "Generate video with added/removed embedded subtitles",
                InputVideoFile = "Input video file",
                SubtitlesX = "Subtitles ({0})",
                ToggleForced = "Toggle forced",
                ToggleDefault = "Toggle default",
                Default = "Default",
                SetLanguage = "Set language...",
                LanguageAndTitle = "Language/title",
                XGeneratedWithEmbeddedSubs = "\"{0}\" generated with embedded subtitles",
                DeleteInputVideo = "Delete input video file after \"Generate\"",
                OutputFileNameSettings = "Output file name settings...",
            };

            GetDictionaries = new LanguageStructure.GetDictionaries
            {
                Title = "Need dictionaries?",
                DescriptionLine1 = "Subtitle Edit's spell check is based on the NHunspell engine which",
                DescriptionLine2 = "uses the spell checking dictionaries from LibreOffice.",
                OpenDictionariesFolder = "Open 'Dictionaries' folder",
                Download = "Download",
                ChooseLanguageAndClickDownload = "Choose your language and click download",
                XDownloaded = "{0} has been downloaded and installed",
            };

            GetTesseractDictionaries = new LanguageStructure.GetTesseractDictionaries
            {
                Title = "Need dictionaries?",
                DescriptionLine1 = "Get Tesseract OCR dictionaries from the web",
                DownloadFailed = "Download failed!",
                GetDictionariesHere = "Get dictionaries here:",
                OpenDictionariesFolder = "Open 'Dictionaries' folder",
                Download = "Download",
                ChooseLanguageAndClickDownload = "Choose your language and click download",
                XDownloaded = "{0} has been downloaded and installed",
            };

            GoogleTranslate = new LanguageStructure.GoogleTranslate
            {
                Title = "Translate",
                From = "From:",
                To = "To:",
                Translate = "Translate",
                PleaseWait = "Please wait... this may take a while",
                PoweredByX = "Powered by {0}",
                LineMergeHandling = "Line merge:",
                ProcessorMergeNext = "Merge max two lines",
                ProcessorSentence = "Merge sentences",
                ProcessorSingle = "No merging",
                AutoTranslateViaCopyPaste = "Auto-translate via copy-paste",
                CopyPasteMaxSize = "Max block size",
                AutoCopyToClipboard = "Auto-copy to clipboard",
                AutoCopyLineSeparator = "Line separator",
                TranslateBlockXOfY = "Translate block {0} of {1}",
                TranslateBlockInfo = "Go to translator and paste text, copy result back to clipboard and click the button below",
                TranslateBlockGetFromClipboard = "Get translated text from clipboard" + Environment.NewLine + "(Ctrl + V)",
                TranslateBlockCopySourceText = "Copy source text clipboard",
                TranslateBlockClipboardError1 = "Clipboard contains source text!",
                TranslateBlockClipboardError2 = "Go to translator and translate, then copy the results to the clipboard and click this button again.",
                StartWebServerX = "Start \"{0}\" web server",
                XRequiresALocalWebServer = "\"{0}\" requires a web server running locally!",
                XRequiresAnApiKey = "\"{0}\" requires an API key.",
                ReadMore = "Read more?",
                Formality = "Formality",
                TranslateCurrentLine = "Translate only current line",
                ReTranslateCurrentLine = "Re-translate current line",
                Delay = "Delay between server calls",
                MaxBytes = "Maximum bytes in each server call",
                MaxMerges = "Max merge attempts(always=-1,never=0)",
                MergeSplitStrategy = "Split/merge handling",
                PromptX = "Prompt for {0}",
                TranslateLinesSeparately = "Translate each line separately",
            };

            GoogleOrMicrosoftTranslate = new LanguageStructure.GoogleOrMicrosoftTranslate
            {
                Title = "Google vs Microsoft translate",
                From = "From:",
                To = "To:",
                Translate = "Translate",
                SourceText = "Source text",
                GoogleTranslate = "Google translate",
                MicrosoftTranslate = "Bing Microsoft translate",
            };

            GoToLine = new LanguageStructure.GoToLine
            {
                Title = "Go to subtitle number",
                XIsNotAValidNumber = "{0} is not a valid number",
            };

            ImportImages = new LanguageStructure.ImportImages
            {
                Title = "Import images",
                Input = "Input",
                InputDescription = "Choose input files (browse or drag-n-drop)",
                ImageFiles = "Image files",
                Remove = "Remove",
                RemoveAll = "Remove all",
            };

            ImportShotChanges = new LanguageStructure.ImportShotChanges
            {
                Title = "Generate/import shot changes",
                OpenTextFile = "Open text file...",
                Import = "Import shot changes",
                Generate = "Generate shot changes",
                TextFiles = "Text files",
                TimeCodes = "Time codes",
                Frames = "Frames",
                Seconds = "Seconds",
                Milliseconds = "Milliseconds",
                GetShotChangesWithFfmpeg = "Generate shot changes with FFmpeg",
                Sensitivity = "Sensitivity",
                SensitivityDescription = "Lower value gives more shot changes",
                NoShotChangesFound = "No shot changes found.",
            };

            ImportText = new LanguageStructure.ImportText
            {
                Title = "Import plain text",
                OneSubtitleIsOneFile = "Multiple files - one file is one subtitle",
                OpenTextFile = "Open text file...",
                OpenTextFiles = "Open text files...",
                ImportOptions = "Import options",
                Splitting = "Splitting",
                AutoSplitText = "Auto split text",
                OneLineIsOneSubtitle = "One line is one subtitle",
                TwoLinesAreOneSubtitle = "Two lines are one subtitle",
                LineBreak = "Line break",
                SplitAtBlankLines = "Split at blank lines",
                MergeShortLines = "Merge short lines with continuation",
                RemoveEmptyLines = "Remove empty lines",
                RemoveLinesWithoutLetters = "Remove lines without letters",
                GenerateTimeCodes = "Generate time codes",
                TakeTimeFromCurrentFile = "Take time from current file",
                TakeTimeFromFileName = "Take time from file name",
                GapBetweenSubtitles = "Gap between subtitles (milliseconds)",
                Auto = "Auto",
                Fixed = "Fixed",
                Refresh = "&Refresh",
                TextFiles = "Text files",
                PreviewLinesModifiedX = "Preview - subtitles modified: {0}",
                TimeCodes = "Time codes",
                SplitAtEndChars = "Split at end chars",
            };

            Interjections = new LanguageStructure.Interjections
            {
                Title = "Interjections",
                EditSkipList = "Edit skip list...",
                EditSkipListInfo = "Interjections will be skipped if source text starts with these:",
            };

            JoinSubtitles = new LanguageStructure.JoinSubtitles
            {
                Title = "Join subtitles",
                Information = "Add subtitles to join (drop also supported)",
                NumberOfLines = "#Lines",
                StartTime = "Start time",
                EndTime = "End time",
                FileName = "File name",
                Join = "Join",
                TotalNumberOfLinesX = "Total number of lines: {0:#,###,###}",
                AlreadyCorrectTimeCodes = "Files already have correct time codes",
                AppendTimeCodes = "Add end time of previous file",
                AddMs = "Add milliseconds after each file",
            };

            LanguageNames = new LanguageStructure.LanguageNames
            {
                NotSpecified = "Not Specified",
                UnknownCodeX = "Unknown ({0})",
                aaName = "Afar",
                abName = "Abkhazian",
                afName = "Afrikaans",
                amName = "Amharic",
                arName = "Arabic",
                asName = "Assamese",
                ayName = "Aymara",
                azName = "Azerbaijani",
                baName = "Bashkir",
                beName = "Belarusian",
                bgName = "Bulgarian",
                bhName = "Bihari",
                biName = "Bislama",
                bnName = "Bengali",
                boName = "Tibetan",
                brName = "Breton",
                caName = "Catalan",
                coName = "Corsican",
                csName = "Czech",
                cyName = "Welsh",
                daName = "Danish",
                deName = "German",
                dzName = "Dzongkha",
                elName = "Greek",
                enName = "English",
                eoName = "Esperanto",
                esName = "Spanish",
                etName = "Estonian",
                euName = "Basque",
                faName = "Persian",
                fiName = "Finnish",
                fjName = "Fijian",
                foName = "Faroese",
                frName = "French",
                fyName = "Western Frisian",
                gaName = "Irish",
                gdName = "Scottish Gaelic",
                glName = "Galician",
                gnName = "Guarani",
                guName = "Gujarati",
                haName = "Hausa",
                heName = "Hebrew",
                hiName = "Hindi",
                hrName = "Croatian",
                huName = "Hungarian",
                hyName = "Armenian",
                iaName = "Interlingua",
                idName = "Indonesian",
                ieName = "Interlingue",
                ikName = "Inupiaq",
                isName = "Icelandic",
                itName = "Italian",
                iuName = "Inuktitut",
                jaName = "Japanese",
                jvName = "Javanese",
                kaName = "Georgian",
                kkName = "Kazakh",
                klName = "Kalaallisut",
                kmName = "Khmer",
                knName = "Kannada",
                koName = "Korean",
                ksName = "Kashmiri",
                kuName = "Kurdish",
                kyName = "Kyrgyz",
                laName = "Latin",
                lbName = "Luxembourgish",
                lnName = "Lingala",
                loName = "Lao",
                ltName = "Lithuanian",
                lvName = "Latvian",
                mgName = "Malagasy",
                miName = "Maori",
                mkName = "Macedonian",
                mlName = "Malayalam",
                mnName = "Mongolian",
                moName = "Moldavian",
                mrName = "Marathi",
                msName = "Malay",
                mtName = "Maltese",
                myName = "Burmese",
                naName = "Nauru",
                neName = "Nepali",
                nlName = "Dutch",
                noName = "Norwegian",
                ocName = "Occitan",
                omName = "Oromo",
                orName = "Oriya",
                paName = "Punjabi",
                plName = "Polish",
                psName = "Pashto",
                ptName = "Portuguese",
                quName = "Quechua",
                rmName = "Romansh",
                rnName = "Rundi",
                roName = "Romanian",
                ruName = "Russian",
                rwName = "Kinyarwanda",
                saName = "Sanskrit",
                sdName = "Sindhi",
                sgName = "Sango",
                shName = "Serbo-Croatian",
                siName = "Sinhala",
                skName = "Slovak",
                slName = "Slovenian",
                smName = "Samoan",
                snName = "Shona",
                soName = "Somali",
                sqName = "Albanian",
                srName = "Serbian",
                ssName = "Swati",
                stName = "Southern Sotho",
                suName = "Sundanese",
                svName = "Swedish",
                swName = "Swahili",
                taName = "Tamil",
                teName = "Telugu",
                tgName = "Tajik",
                thName = "Thai",
                tiName = "Tigrinya",
                tkName = "Turkmen",
                tlName = "Tagalog",
                tnName = "Tswana",
                toName = "Tongan",
                trName = "Turkish",
                tsName = "Tsonga",
                ttName = "Tatar",
                twName = "Twi",
                ugName = "Uyghur",
                ukName = "Ukrainian",
                urName = "Urdu",
                uzName = "Uzbek",
                viName = "Vietnamese",
                voName = "Volapük",
                woName = "Wolof",
                xhName = "Xhosa",
                yiName = "Yiddish",
                yoName = "Yoruba",
                zaName = "Zhuang",
                zhName = "Chinese",
                zuName = "Zulu",
            };

            Main = new LanguageStructure.Main
            {
                SaveChangesToUntitled = "Save changes to untitled?",
                SaveChangesToX = "Save changes to {0}?",
                SaveChangesToUntitledOriginal = "Save changes to untitled original?",
                SaveChangesToOriginalX = "Save changes to original {0}?",
                SaveSubtitleAs = "Save subtitle as...",
                SaveOriginalSubtitleAs = "Save original subtitle as...",
                CannotSaveEmptySubtitle = "Cannot save empty subtitle",
                NoSubtitleLoaded = "No subtitle loaded",
                VisualSyncSelectedLines = "Visual sync - selected lines",
                VisualSyncTitle = "Visual sync",
                BeforeVisualSync = "Before visual sync",
                VisualSyncPerformedOnSelectedLines = "Visual sync performed on selected lines",
                VisualSyncPerformed = "Visual sync performed",
                FileXIsLargerThan10MB = "File is larger than 10 MB: {0}",
                ContinueAnyway = "Continue anyway?",
                BeforeLoadOf = "Before load of {0}",
                LoadedSubtitleX = "Loaded subtitle {0}",
                LoadedEmptyOrShort = "Loaded empty or very short subtitle {0}",
                FileIsEmptyOrShort = "File is empty or very short!",
                FileNotFound = "File not found: {0}",
                FileLocked = "Unable to open file as it is in use by another program: {0}",
                SavedSubtitleX = "Saved subtitle {0}",
                SavedOriginalSubtitleX = "Saved original subtitle {0}",
                FileOnDiskModified = "File on disk modified",
                OverwriteModifiedFile = "Overwrite the file {0} modified at {1} {2}{3} with current file loaded from disk at {4} {5}?",
                FileXIsReadOnly = "Cannot save {0}" + Environment.NewLine + Environment.NewLine + "File is read-only!",
                UnableToSaveSubtitleX = "Unable to save subtitle file {0}" + Environment.NewLine + "Subtitle seems to be empty - try to re-save if you're working on a valid subtitle!",
                FormatXShouldUseUft8 = "UTF-8 encoding should be used when saving {0} files!",
                BeforeNew = "Before new",
                New = "New",
                BeforeConvertingToX = "Before converting to {0}",
                ConvertedToX = "Converted to {0}",
                BeforeShowEarlier = "Before show earlier",
                BeforeShowLater = "Before show later",
                LineNumberX = "Line number: {0:#,##0.##}",
                OpenVideoFile = "Open video file...",
                NewFrameRateUsedToCalculateTimeCodes = "New frame rate ({0}) was used for calculating start/end time codes",
                NewFrameRateUsedToCalculateFrameNumbers = "New frame rate ({0}) was used for calculating start/end frame numbers",
                FindContinueTitle = "Continue Find?",
                FindContinue = "The search item was not found." + Environment.NewLine +
                               "Would you like to start from the top of the document and search one more time?",
                ReplaceContinueTitle = "Continue 'Replace'?",
                ReplaceContinueNotFound = "The search item was not found." + Environment.NewLine +
                               "Would you like to start from the top of the document and continue search and replace?",

                ReplaceXContinue = "The search item was replaced {0} time(s)." + Environment.NewLine +
                               "Would you like to start from the top of the document and continue search and replace?",

                SearchingForXFromLineY = "Searching for '{0}' from line number {1}...",
                XFoundAtLineNumberY = "'{0}' found at line number {1}",
                XNotFound = "'{0}' not found",
                BeforeReplace = "Before replace: {0}",
                MatchFoundX = "Match found: {0}",
                NoMatchFoundX = "No match found: {0}",
                FoundNothingToReplace = "Found nothing to replace",
                ReplaceCountX = "Replace count: {0:#,##0}",
                NoXFoundAtLineY = "Match found at line {0}: {1}",
                OneReplacementMade = "One replacement made.",
                BeforeChangesMadeInSourceView = "Before changes made in source view",
                UnableToParseSourceView = "Unable to parse source view text!",
                GoToLineNumberX = "Go to line number {0}",
                CreateAdjustChangesApplied = "Create/adjust lines changes applied",
                SelectedLines = "selected lines",
                BeforeDisplayTimeAdjustment = "Before display time adjustment",
                DisplayTimesAdjustedX = "Display times adjusted: {0}",
                DisplayTimeAdjustedX = "Display time adjusted: {0}",
                StartTimeAdjustedX = "Start time adjusted: {0}",
                BeforeCommonErrorFixes = "Before common error fixes",
                CommonErrorsFixedInSelectedLines = "Common errors fixed in selected lines",
                CommonErrorsFixed = "Common errors fixed",
                BeforeRenumbering = "Before renumbering",
                RenumberedStartingFromX = "Renumbered starting from: {0}",
                BeforeBeautifyTimeCodes = "Before beautifying time codes",
                BeforeBeautifyTimeCodesSelectedLines = "Before beautifying time codes of selected lines",
                BeautifiedTimeCodes = "Time codes beautified",
                BeautifiedTimeCodesSelectedLines = "Time codes of selected lines beautified",
                BeforeRemovalOfTextingForHearingImpaired = "Before removal of texting for hearing impaired",
                TextingForHearingImpairedRemovedOneLine = "Texting for hearing impaired removed: One line",
                TextingForHearingImpairedRemovedXLines = "Texting for hearing impaired removed: {0} lines",
                SubtitleSplitted = "Subtitle was split",
                SubtitleAppendPrompt = "This will append an existing subtitle to the currently loaded subtitle which should already be in sync with video file." + Environment.NewLine +
                                    Environment.NewLine +
                                    "Continue?",
                SubtitleAppendPromptTitle = "Append subtitle",
                OpenSubtitleToAppend = "Open subtitle to append...",
                AppendViaVisualSyncTitle = "Visual sync - append second part of subtitle",
                AppendSynchronizedSubtitlePrompt = "Append this synchronized subtitle?",
                BeforeAppend = "Before append",
                SubtitleAppendedX = "Subtitle appended: {0}",
                SubtitleNotAppended = "Subtitle NOT appended!",
                GoogleTranslate = "Google translate",
                MicrosoftTranslate = "Bing Microsoft translate",
                BeforeGoogleTranslation = "Before Google translation",
                SelectedLinesTranslated = "Selected lines translated",
                SubtitleTranslated = "Subtitle translated",
                TranslateSwedishToDanish = "Translate currently loaded Swedish subtitle to Danish",
                TranslateSwedishToDanishWarning = "Translate currently loaded SWEDISH (are you sure it's Swedish?) subtitle to Danish?",
                TranslatingViaNikseDkMt = "Translating via www.nikse.dk/mt...",
                BeforeSwedishToDanishTranslation = "Before Swedish to Danish translation",
                TranslationFromSwedishToDanishComplete = "Translation from Swedish to Danish complete",
                TranslationFromSwedishToDanishFailed = "Translation from Swedish to Danish failed",
                UndoPerformed = "Undo performed",
                RedoPerformed = "Redo performed",
                NothingToUndo = "Nothing to undo",
                InvalidLanguageNameX = "Invalid language name: {0}",
                DoNotDisplayMessageAgain = "Don't display this message again",
                DoNotAutoLoadVideo = "Do not autoload video",
                NumberOfCorrectedWords = "Number of corrected words: {0}",
                NumberOfSkippedWords = "Number of skipped words: {0}",
                NumberOfCorrectWords = "Number of correct words: {0}",
                NumberOfWordsAddedToDictionary = "Number of words added to dictionary: {0}",
                NumberOfNameHits = "Number of name hits: {0}",
                SpellCheck = "Spell check",
                BeforeSpellCheck = "Before spell check",
                SpellCheckChangedXToY = "Spell check: Changed '{0}' to '{1}'",
                BeforeAddingTagX = "Before adding <{0}> tag",
                TagXAdded = "{0} tag added",
                LineXOfY = "line {0} of {1}",
                XLinesSavedAsY = "{0} lines saved as {1}",
                XLinesDeleted = "{0} lines deleted",
                BeforeDeletingXLines = "Before deleting {0:#,##0} lines",
                DeleteXLinesPrompt = "Delete {0:#,##0} lines?",
                OneLineDeleted = "Line deleted",
                BeforeDeletingOneLine = "Before deleting one line",
                DeleteOneLinePrompt = "Delete one line?",
                BeforeInsertLine = "Before insert line",
                BeforeLineUpdatedInListView = "Before line updated in listview",
                LineInserted = "Line inserted",
                BeforeSplitLine = "Before split line",
                LineSplitted = "Line was split",
                BeforeMergeLines = "Before merge lines",
                LinesMerged = "Lines merged",
                MergeSentences = "Merge sentences...",
                MergeSentencesXLines = "Merge sentences - lines merged: {0}",
                BeforeSettingColor = "Before setting color",
                BeforeSettingFontName = "Before setting font name",
                BeforeTypeWriterEffect = "Before typewriter effect",
                BeforeKaraokeEffect = "Before karaoke effect",
                BeforeImportingDvdSubtitle = "Before importing subtitle from DVD",
                OpenSubtitleVideoFile = "Open subtitle from video file...",
                VideoFiles = "Video files",
                NoSubtitlesFound = "No subtitles found",
                NotAValidMatroskaFileX = "This is not a valid Matroska file: {0}",
                BlurayNotSubtitlesFound = "Blu-ray sup file does not contain any subtitles or contains errors - try demuxing again.",
                ImportingChapters = "Importing chapters...",
                XChaptersImported = "{0} chapters imported",
                ParsingMatroskaFile = "Parsing Matroska file. Please wait...",
                ParsingTransportStreamFile = "Parsing Transport Stream file. Please wait...",
                BeforeImportFromMatroskaFile = "Before import subtitle from Matroska file",
                SubtitleImportedFromMatroskaFile = "Subtitle imported from Matroska file",
                DropFileXNotAccepted = "Drop file '{0}' not accepted - file is too large",
                DropSubtitleFileXNotAccepted = "Drop file '{0}' not accepted - file is too large for a subtitle",
                DropOnlyOneFile = "You can only drop one file",
                OpenAnsiSubtitle = "Open subtitle...",
                BeforeChangeCasing = "Before change casing",
                CasingCompleteMessage = "Number of lines with casing changed: {0}/{1}, changed casing for names: {2}",
                CasingCompleteMessageNoNames = "Number of lines with casing changed: {0}/{1}",
                CasingCompleteMessageOnlyNames = "Number of lines with names casing changed: {0}/{1}",
                BeforeChangeFrameRate = "Before change frame rate",
                BeforeAdjustSpeedInPercent = "Before adjust speed in percent",
                FrameRateChangedFromXToY = "Frame rate changed from {0} to {1}",
                IdxFileNotFoundWarning = "{0} not found! Import VobSub file anyway?",
                InvalidVobSubHeader = "Header not valid VobSub file: {0}",
                OpenVobSubFile = "Open VobSub (sub/idx) subtitle...",
                VobSubFiles = "VobSub subtitle files",
                OpenBluRaySupFile = "Open Blu-ray .sup file...",
                BluRaySupFiles = "Blu-ray .sup files",
                BeforeImportingVobSubFile = "Before importing VobSub subtitle",
                BeforeImportingBluRaySupFile = "Before importing Blu-ray sup file",
                BeforeImportingBdnXml = "Before importing BDN xml file",
                BeforeShowSelectedLinesEarlierLater = "Before show selected lines earlier/later",
                ShowAllLinesXSecondsLinesEarlier = "Show all lines {0:0.0##} seconds earlier",
                ShowAllLinesXSecondsLinesLater = "Show all lines {0:0.0##} seconds later",
                ShowSelectedLinesXSecondsLinesEarlier = "Show selected lines {0:0.0##} seconds earlier",
                ShowSelectedLinesXSecondsLinesLater = "Show selected lines {0:0.0##} seconds later",
                ShowSelectionAndForwardXSecondsLinesEarlier = "Show selection and forward {0:0.0##} seconds earlier",
                ShowSelectionAndForwardXSecondsLinesLater = "Show selection and forward {0:0.0##} seconds later",
                DoubleWordsViaRegEx = "Double words via regex {0}",
                BeforeSortX = "Before sort: {0}",
                SortedByX = "Sorted by: {0}",
                BeforeAutoBalanceSelectedLines = "Before auto balance selected lines",
                NumberOfLinesAutoBalancedX = "Number of auto balanced lines: {0}",
                BeforeEvenlyDistributeSelectedLines = "Before evenly distribute selected lines",
                NumberOfLinesEvenlyDistributedX = "Number of evenly distributed lines: {0}",
                BeforeRemoveLineBreaksInSelectedLines = "Before remove line-breaks from selected lines",
                NumberOfWithRemovedLineBreakX = "Number of lines with removed line-break: {0}",
                BeforeMultipleReplace = "Before multiple replace",
                NumberOfLinesReplacedX = "Number of lines with text replaced: {0}",
                NameXAddedToNameList = "The name '{0}' was added to name list",
                NameXNotAddedToNameList = "The name '{0}' was NOT added to name list",
                WordXAddedToUserDic = "The word '{0}' was added to the user dictionary",
                WordXNotAddedToUserDic = "The word '{0}' was NOT added to the user dictionary",
                OcrReplacePairXAdded = "The OCR replace list pair '{0} -> {1}' was added to the OCR replace list",
                OcrReplacePairXNotAdded = "The OCR replace list pair '{0} -> {1}' was NOT added to the OCR replace list",
                XLinesSelected = "{0} lines selected",
                UnicodeMusicSymbolsAnsiWarning = "Subtitle contains Unicode characters. Saving using ANSI file encoding will lose these. Continue with saving?",
                NegativeTimeWarning = "Subtitle contains negative time codes. Continue with saving?",
                BeforeMergeShortLines = "Before merge short lines",
                BeforeSplitLongLines = "Before split long lines",
                LongLinesSplitX = "Number of lines split: {0}",
                MergedShortLinesX = "Number of lines merged: {0}",
                BeforeDurationsBridgeGap = "Before bridge small gaps",
                BeforeSetMinimumDisplayTimeBetweenParagraphs = "Before set minimum display time between subtitles",
                XMinimumDisplayTimeBetweenParagraphsChanged = "Number of lines with minimum display time between subtitles changed: {0}",
                BeforeImportText = "Before import plain text",
                TextImported = "Text imported",
                BeforePointSynchronization = "Before point synchronization",
                PointSynchronizationDone = "Point synchronization done",
                BeforeTimeCodeImport = "Before import of time codes",
                TimeCodeImportedFromXY = "Time codes imported from {0}: {1}",
                BeforeInsertSubtitleAtVideoPosition = "Before insert subtitle at video position",
                BeforeSetStartTimeAndOffsetTheRest = "Before set start time and offset the rest",
                BeforeSetEndTimeAndOffsetTheRest = "Before set end time and offset the rest",
                BeforeSetEndAndVideoPosition = "Before set end time at video position and auto calculate start",
                ContinueWithCurrentSpellCheck = "Continue with current spell check?",
                CharactersPerSecond = "Chars/sec: {0:0.00}",
                GetFrameRateFromVideoFile = "Get frame rate from video file",
                NetworkMessage = "New message: {0} ({1}): {2}",
                NetworkUpdate = "Line updated: {0} ({1}): Index={2}, Text={3}",
                NetworkInsert = "Line inserted: {0} ({1}): Index={2}, Text={3}",
                NetworkDelete = "Line deleted: {0} ({1}): Index={2}",
                NetworkNewUser = "New user: {0} ({1})",
                NetworkByeUser = "Bye {0} ({1})",
                NetworkUnableToConnectToServer = "Unable to connect to server: {0}",
                NetworkMode = "Networking mode",
                UserAndAction = "User/action",
                XStartedSessionYAtZ = "{0}: Started session {1} at {2}",
                OpenOtherSubtitle = "Open another subtitle",
                BeforeToggleDialogDashes = "Before toggle of dialog dashes",
                TextFiles = "Text files",
                ExportPlainTextAs = "Export plain text as",
                SubtitleExported = "Subtitle exported",
                LineNumberXErrorReadingFromSourceLineY = "Line {0} - error reading: {1}",
                LineNumberXErrorReadingTimeCodeFromSourceLineY = "Line {0} - error reading time code: {1}",
                LineNumberXExpectedNumberFromSourceLineY = "Line {0} - expected subtitle number: {1}",
                LineNumberXExpectedEmptyLine = "Line {0}: Empty line expected, but found number ({1}) followed by time code.",
                BeforeGuessingTimeCodes = "Before guessing time codes",
                BeforeAutoDuration = "Before auto-duration for selected lines",
                BeforeColumnPaste = "Before column paste",
                BeforeColumnDelete = "Before column delete",
                BeforeColumnImportText = "Before column import text",
                BeforeColumnShiftCellsDown = "Before column shift cells down",
                BeforeX = "Before: {0}",
                LinesUpdatedX = "Lines updated: {0}",
                ErrorLoadingPluginXErrorY = "Error loading plugin: {0}: {1}",
                BeforeRunningPluginXVersionY = "Before running plugin: {0}: {1}",
                UnableToReadPluginResult = "Unable to read subtitle result from plugin!",
                UnableToCreateBackupDirectory = "Unable to create backup folder {0}: {1}",
                BeforeDisplaySubtitleJoin = "Before join of subtitles",
                SubtitlesJoined = "Subtitles joined",
                StatusLog = "Status log",
                XShotChangesImported = "{0} shot changes imported",
                PluginXExecuted = "Plugin '{0}' executed.",
                NotAValidXSubFile = "Not a valid XSub file!",
                BeforeMergeLinesWithSameText = "Before merging lines with same text",
                ImportTimeCodesDifferentNumberOfLinesWarning = "Subtitle with time codes has a different number of lines ({0}) than current subtitle ({1}) - continue anyway?",
                ParsingTransportStream = "Parsing transport stream - please wait...",
                XPercentCompleted = "{0}% completed",
                NextX = "Next: {0}",
                PromptInsertSubtitleOverlap = "Insert subtitle at waveform position will cause overlap!" + Environment.NewLine +
                                              Environment.NewLine +
                                              "Continue anyway?",
                SubtitleContainsNegativeDurationsX = "Subtitle contains negative duration in line(s): {0}",
                SetPlayRateX = "Set play rate (speed) to {0}%",
                ErrorLoadIdx = "Cannot read/edit .idx files. Idx files are a part of an idx/sub file pair (also called VobSub), and Subtitle Edit can open the .sub file.",
                ErrorLoadRar = "This file seems to be a compressed .rar file. Subtitle Edit cannot open compressed files.",
                ErrorLoadZip = "This file seems to be a compressed .zip file. Subtitle Edit cannot open compressed files.",
                ErrorLoad7Zip = "This file seems to be a compressed 7-Zip file. Subtitle Edit cannot open compressed files.",
                ErrorLoadPng = "This file seems to be a PNG image file. Subtitle Edit cannot open PNG files.",
                ErrorLoadJpg = "This file seems to be a JPG image file. Subtitle Edit cannot open JPG files.",
                ErrorLoadSrr = "This file seems to be a ReScene .srr file - not a subtitle file.",
                ErrorLoadTorrent = "This file seems to be a BitTorrent file - not a subtitle file.",
                ErrorLoadBinaryZeroes = "Sorry, this file contains only binary zeroes!\r\n\r\nIf you have edited this file with Subtitle Edit you might be able to find a backup via the menu item File -> Restore auto-backup...",
                ErrorDirectoryDropNotAllowed = "Folder drop is not supported here.",
                NoSupportEncryptedVobSub = "Encrypted VobSub content is not supported.",
                NoSupportHereBluRaySup = "Blu-ray sup files are not supported here.",
                NoSupportHereDvdSup = "DVD sup files are not supported here.",
                NoSupportHereVobSub = "VobSub files are not supported here.",
                NoSupportHereDivx = "DivX files are not supported here.",
                NoChapters = "No chapters found in the video.",
                VideoFromUrlRequirements = "Opening video from URL requires mpv and youtube-dl.\r\n\r\nDownload and continue?",
                Url = "URL",
                Errors = "Errors",
                ShowVideoControls = "Show video controls",
                HideVideoControls = "Hide video controls",
                GeneratingWaveformInBackground = "Generating waveform in background...",
                AutoBackupSaved = "Auto-backup saved",
                UsingOnlyFrontCenterChannel = "Using only front center audio channel",
                BeforeConvertingColorsToDialog = "Before converting colors to dialog",
                ConvertedColorsToDialog = "Converted colors to dialog",
                PleaseInstallVideoPlayer = "Please install video player",
                UnableToPlayMediaFile = "SE was unable to play the video/audio file (or file is not a valid video/audio file).",
                SubtitleEditNeedsVideoPlayer = "Subtitle Edit needs a video player.",
                UseRecommendMpv = "To use the recommended video player \"mpv\" click on the button below.",
                DownloadAndUseMpv = "Download and use \"mpv\" as video player",
                ChooseLayout = "Choose layout",

                Menu = new LanguageStructure.Main.MainMenu
                {
                    File = new LanguageStructure.Main.MainMenu.FileMenu
                    {
                        Title = "&File",
                        New = "&New",
                        Open = "&Open...",
                        OpenKeepVideo = "Open (keep video)...",
                        Reopen = "&Reopen",
                        Save = "&Save",
                        SaveAs = "Save &as...",
                        RestoreAutoBackup = "Restore auto-backup...",
                        FormatXProperties = "{0} properties...",
                        OpenOriginal = "Open original subtitle (translator mode)...",
                        SaveOriginal = "Save original subtitle",
                        CloseOriginal = "Close original subtitle",
                        CloseTranslation = "Close translated subtitle",
                        OpenContainingFolder = "Open containing folder",
                        Compare = "&Compare...",
                        VerifyCompleteness = "Verify completeness...",
                        Statistics = "S&tatistics...",
                        Plugins = "&Plugins...",
                        ImportSubtitleFromVideoFile = "Subtitle from video file...",
                        ImportOcrFromDvd = "Subtitle from VOB/IFO (DVD)...",
                        ImportOcrVobSubSubtitle = "VobSub (sub/idx) subtitle for OCR...",
                        ImportBluRaySupFile = "Blu-ray (.sup) subtitle file for OCR...",
                        ImportBluRaySupFileEdit = "Blu-ray (.sup) subtitle file for edit...",
                        ImportSubtitleWithManualChosenEncoding = "Subtitle with manually chosen encoding...",
                        ImportText = "Plain text...",
                        ImportImages = "Images...",
                        ImportTimecodes = "Time codes...",
                        Import = "Import",
                        Export = "Export",
                        ExportBdnXml = "BDN xml/png...",
                        ExportBluRaySup = "Blu-ray sup...",
                        ExportVobSub = "VobSub (sub/idx)...",
                        ExportCavena890 = "Cavena 890...",
                        ExportEbu = "EBU STL...",
                        ExportPac = "PAC (Screen Electronics)...",
                        ExportPlainText = "Plain text...",
                        ExportAvidStl = "Avid STL...",
                        ExportAdobeEncoreFabImageScript = "Adobe Encore FAB image script...",
                        ExportKoreanAtsFilePair = "Korean ATS file pair...",
                        ExportDvdStudioProStl = "DVD Studio Pro STL...",
                        ExportCapMakerPlus = "CapMaker Plus...",
                        ExportCaptionsInc = "Captions Inc...",
                        ExportCheetahCap = "Cheetah CAP...",
                        ExportUltech130 = "Ultech caption...",
                        ExportCustomTextFormat = "Export custom text format...",
                        Exit = "E&xit",
                    },

                    Edit = new LanguageStructure.Main.MainMenu.EditMenu
                    {
                        Title = "Edit",
                        Undo = "Undo",
                        Redo = "Redo",
                        ShowUndoHistory = "Show history (for undo)",
                        InsertUnicodeSymbol = "Insert Unicode symbol",
                        InsertUnicodeControlCharacters = "Insert Unicode control characters",
                        InsertUnicodeControlCharactersLRM = "Left-to-right mark (LRM)",
                        InsertUnicodeControlCharactersRLM = "Right-to-left mark (RLM)",
                        InsertUnicodeControlCharactersLRE = "Start of left-to-right embedding (LRE)",
                        InsertUnicodeControlCharactersRLE = "Start of right-to-left embedding (RLE)",
                        InsertUnicodeControlCharactersLRO = "Start of left-to-right override (LRO)",
                        InsertUnicodeControlCharactersRLO = "Start of right-to-left override (RLO)",
                        Find = "&Find",
                        FindNext = "Find &next",
                        Replace = "&Replace",
                        MultipleReplace = "&Multiple replace...",
                        GoToSubtitleNumber = "&Go to subtitle number...",
                        RightToLeftMode = "Right-to-left mode",
                        FixRtlViaUnicodeControlCharacters = "Fix RTL via Unicode control characters (for selected lines)",
                        RemoveUnicodeControlCharacters = "Remove Unicode control characters (from selected lines)",
                        ReverseRightToLeftStartEnd = "Reverse RTL start/end (for selected lines)",
                        ShowOriginalTextInAudioAndVideoPreview = "Show original text in audio/video previews",
                        ModifySelection = "Modify selection...",
                        InverseSelection = "Invert selection",
                    },

                    Tools = new LanguageStructure.Main.MainMenu.ToolsMenu
                    {
                        Title = "Tools",
                        AdjustDisplayDuration = "&Adjust durations...",
                        ApplyDurationLimits = "Apply duration limits...",
                        SubtitlesBridgeGaps = "Bridge gaps between subtitles...",
                        FixCommonErrors = "&Fix common errors...",
                        StartNumberingFrom = "Renumber...",
                        RemoveTextForHearingImpaired = "Remove text for hearing impaired...",
                        ConvertColorsToDialog = "Convert colors to dialog...",
                        ChangeCasing = "Change casing...",
                        ChangeFrameRate = "Change frame rate...",
                        ChangeSpeedInPercent = "Changed speed (percent)...",
                        MergeShortLines = "Merge short lines...",
                        MergeDuplicateText = "Merge lines with same text...",
                        MergeSameTimeCodes = "Merge lines with same time codes...",
                        SplitLongLines = "Break/split long lines...",
                        MinimumDisplayTimeBetweenParagraphs = "Apply minimum gap between subtitles...",
                        NetflixQualityCheck = "Netflix quality check...",
                        BeautifyTimeCodes = "Beautify time codes...",
                        SortBy = "Sort by",
                        Number = "Number",
                        StartTime = "Start time",
                        EndTime = "End time",
                        Duration = "Duration",
                        ListErrors = "List errors...",
                        TextAlphabetically = "Text - alphabetically",
                        TextSingleLineMaximumLength = "Text - single line max. length",
                        TextTotalLength = "Text - total length",
                        TextNumberOfLines = "Text - number of lines",
                        TextNumberOfCharactersPerSeconds = "Text - number of chars/sec",
                        WordsPerMinute = "Text - words per minute (wpm)",
                        Style = "Style",
                        Ascending = "Ascending",
                        Descending = "Descending",
                        MakeNewEmptyTranslationFromCurrentSubtitle = "Make new empty translation from current subtitle",
                        BatchConvert = "Batch convert...",
                        GenerateTimeAsText = "Generate time as text...",
                        MeasurementConverter = "Measurement converter...",
                        SplitSubtitle = "Split subtitle...",
                        AppendSubtitle = "Append subtitle...",
                        JoinSubtitles = "Join subtitles...",
                    },

                    Video = new LanguageStructure.Main.MainMenu.VideoMenu
                    {
                        Title = "Video",
                        OpenVideo = "Open video file...",
                        OpenVideoFromUrl = "Open video from URL...",
                        OpenDvd = "Open DVD...",
                        ChooseAudioTrack = "Choose audio track",
                        CloseVideo = "Close video file",
                        OpenSecondSubtitle = "Open second subtitle file...",
                        SetVideoOffset = "Set video offset...",
                        SmptTimeMode = "SMPTE timing (non integer frame rate)",
                        GenerateTextFromVideo = "Generate text from video...",
                        GenerateBlankVideo = "Generate blank video...",
                        GenerateVideoWithEmbeddedSubs = "Generate video with added/removed embedded subtitles...",
                        GenerateVideoWithBurnedInSub = "Generate video with burned-in subtitle...",
                        GenerateTransparentVideoWithSubs = "Generate transparent video with subtitles...",
                        VideoAudioToTextX = "Audio to text ({0})...",
                        ImportChaptersFromVideo = "Import chapters from video",
                        GenerateImportShotChanges = "Generate/import shot changes...",
                        RemoveOrExportShotChanges = "Remove/export shot changes...",
                        WaveformBatchGenerate = "Batch generate waveforms...",
                        ShowHideWaveformAndSpectrogram = "Show/hide waveform and spectrogram",
                        TextToSpeechAndAddToVideo = "Text to speech and add to video...",
                        UnDockVideoControls = "Un-dock video controls",
                        ReDockVideoControls = "Re-dock video controls",
                    },

                    SpellCheck = new LanguageStructure.Main.MainMenu.SpellCheckMenu
                    {
                        Title = "Spell check",
                        FindDoubleWords = "Find double words",
                        FindDoubleLines = "Find double lines",
                        SpellCheck = "&Spell check...",
                        SpellCheckFromCurrentLine = "Spell check from current line...",
                        GetDictionaries = "Get dictionaries...",
                        AddToNameList = "Add word to name list",
                    },

                    Synchronization = new LanguageStructure.Main.MainMenu.SynchronizationkMenu
                    {
                        Title = "Synchronization",
                        AdjustAllTimes = "Adjust all times (show earlier/later)...",
                        VisualSync = "&Visual sync...",
                        PointSync = "Point sync...",
                        PointSyncViaOtherSubtitle = "Point sync via another subtitle...",
                    },

                    AutoTranslate = new LanguageStructure.Main.MainMenu.AutoTranslateMenu
                    {
                        Title = "Auto-translate",
                        AutoTranslate = "Auto-translate...",
                        AutoTranslateViaCopyPaste = "Auto-translate via copy-paste...",
                    },

                    Options = new LanguageStructure.Main.MainMenu.OptionsMenu
                    {
                        Title = "Options",
                        Settings = "&Settings...",
                        WordLists = "Word lists...",
                        ChooseLanguage = "&Choose language...",
                    },

                    Help = new LanguageStructure.Main.MainMenu.HelpMenu
                    {
                        CheckForUpdates = "Check for updates...",
                        Title = "Help",
                        Help = "&Help",
                        About = "&About",
                    },

                    Networking = new LanguageStructure.Main.MainMenu.NetworkingMenu
                    {
                        Title = "Networking",
                        StartNewSession = "Start new session",
                        JoinSession = "Join session",
                        ShowSessionInfoAndLog = "Show session info and log",
                        Chat = "Chat",
                        LeaveSession = "Leave session",
                    },

                    ToolBar = new LanguageStructure.Main.MainMenu.ToolBarMenu
                    {
                        New = "New",
                        Open = "Open",
                        Save = "Save",
                        SaveAs = "Save as",
                        Find = "Find",
                        Replace = "Replace",
                        FixCommonErrors = "Fix common errors",
                        RemoveTextForHi = "Remove text for hearing impaired",
                        VisualSync = "Visual sync",
                        SpellCheck = "Spell check",
                        NetflixQualityCheck = "Netflix quality check",
                        BeautifyTimeCodes = "Beautify time codes",
                        Settings = "Settings",
                        Help = "Help",
                        Layout = "Layout",
                        AssaDraw = "Advanced Sub Station Alpha draw",
                    },

                    ContextMenu = new LanguageStructure.Main.MainMenu.ListViewContextMenu
                    {
                        SizeAllColumnsToFit = "Size all columns to fit",
                        SetStyle = "Set style",
                        SetActor = "Set actor",
                        SetLayer = "Set layer",
                        AssaTools = "ASSA tools",
                        AdvancedSubStationAlphaStyles = "Advanced Sub Station Alpha styles...",
                        SubStationAlphaStyles = "Sub Station Alpha styles...",
                        TimedTextStyles = "Timed Text styles...",
                        TimedTextSetRegion = "Timed Text - set region",
                        TimedTextSetStyle = "Timed Text - set style",
                        TimedTextSetLanguage = "Timed Text - set language",
                        SamiSetStyle = "Sami - set class",
                        NuendoSetStyle = "Nuendo - set character",
                        WebVttSetVoice = "WebVTT - set voice",
                        WebVttSetStyle = "WebVTT - set style",
                        WebVttBrowserPreview = "WebVTT - browser preview",
                        Cut = "Cut",
                        Copy = "Copy",
                        Paste = "Paste",
                        Delete = "Delete",
                        SplitLineAtCursorPosition = "Split line at cursor position",
                        SplitLineAtCursorPositionAndAutoBr = "Split line at cursor position and auto-break",
                        SplitLineAtCursorAndWaveformPosition = "Split line at cursor/video position",
                        AutoDurationCurrentLine = "Auto duration (current line)",
                        SelectAll = "Select all",
                        InsertFirstLine = "Insert line",
                        InsertBefore = "Insert before",
                        InsertAfter = "Insert after",
                        InsertSubtitleAfter = "Insert subtitle after this line...",
                        CopyToClipboard = "Copy as text to clipboard",
                        Column = "Column",
                        ColumnDeleteText = "Delete text",
                        ColumnDeleteTextAndShiftCellsUp = "Delete text and shift cells up",
                        ColumnInsertEmptyTextAndShiftCellsDown = "Insert empty text and shift cells down",
                        ColumnInsertTextFromSubtitle = "Insert text from subtitle...",
                        ColumnImportTextAndShiftCellsDown = "Import text and shift cells down",
                        ColumnPasteFromClipboard = "Paste from clipboard...",
                        ColumnTextUp = "Text up",
                        ColumnTextDown = "Text down",
                        ColumnCopyOriginalTextToCurrent = "Copy text from original to current",
                        OcrSelectedLines = "OCR selected lines",
                        Split = "Split",
                        MergeSelectedLines = "Merge selected lines",
                        MergeSelectedLinesAsDialog = "Merge selected lines as dialog",
                        MergeWithLineBefore = "Merge with line before",
                        MergeWithLineAfter = "Merge with line after",
                        ExtendToLineBefore = "Extend to line before",
                        ExtendToLineAfter = "Extend to line after",
                        RemoveFormatting = "Remove formatting",
                        RemoveFormattingAll = "Remove all formatting",
                        RemoveFormattingItalic = "Remove italic",
                        RemoveFormattingBold = "Remove bold",
                        RemoveFormattingUnderline = "Remove underline",
                        RemoveFormattingColor = "Remove color",
                        RemoveFormattingFontName = "Remove font name",
                        RemoveFormattingAlignment = "Remove alignment",
                        Underline = "Underline",
                        Box = "Box",
                        Color = "Color...",
                        FontName = "Font name...",
                        Superscript = "Superscript",
                        Subscript = "Subscript",
                        Alignment = "Alignment...",
                        AutoBalanceSelectedLines = "Auto balance selected lines...",
                        EvenlyDistributeSelectedLines = "Evenly distribute selected lines (CPS)",
                        RemoveLineBreaksFromSelectedLines = "Remove line-breaks from selected lines...",
                        TypewriterEffect = "Typewriter effect...",
                        KaraokeEffect = "Karaoke effect...",
                        ShowSelectedLinesEarlierLater = "Show selected lines earlier/later...",
                        VisualSyncSelectedLines = "Visual sync selected lines...",
                        BeautifyTimeCodesOfSelectedLines = "Beautify time codes of selected lines...",
                        GoogleAndMicrosoftTranslateSelectedLine = "Google/Microsoft translate original line",
                        SelectedLines = "Selected lines",
                        TranslateSelectedLines = "Translate selected lines...",
                        AdjustDisplayDurationForSelectedLines = "Adjust durations for selected lines...",
                        ApplyDurationLimitsForSelectedLines = "Apply duration limits for selected lines...",
                        ApplyCustomOverrideTag = "Apply custom override tags...",
                        SetPosition = "Set position...",
                        GenerateProgressBar = "Generate progress bar...",
                        AssaResolutionChanger = "Change ASSA script resolution...",
                        AssaGenerateBackgroundBox = "Generate background box...",
                        ImageColorPicker = "Image color picker...",
                        FixCommonErrorsInSelectedLines = "Fix common errors in selected lines...",
                        ChangeCasingForSelectedLines = "Change casing for selected lines...",
                        SaveSelectedLines = "Save selected lines as...",
                        WebVTTSetNewVoice = "Set new voice...",
                        WebVTTRemoveVoices = "Remove voices",
                        NewActor = "New actor...",
                        RemoveActors = "Remove actors",
                        EditBookmark = "Edit bookmark...",
                        RemoveBookmark = "Remove bookmark",
                        GoToSourceView = "Go to source view",
                        GoToListView = "Go to list view",
                        ExtractAudio = "Extract audio...",
                        MediaInfo = "Media information",
                    }
                },

                Controls = new LanguageStructure.Main.MainControls
                {
                    SubtitleFormat = "Format",
                    FileEncoding = "Encoding",
                    UndoChangesInEditPanel = "Undo changes in edit panel",
                    Previous = "< Prev",
                    Next = "Next >",
                    AutoBreak = "Auto &br",
                    Unbreak = "Unbreak",
                },

                VideoControls = new LanguageStructure.Main.MainVideoControls
                {
                    Translate = "Translate",
                    CreateAndAdjust = "Create/adjust",
                    Create = "Create",
                    Adjust = "Adjust",
                    SelectCurrentElementWhilePlaying = "Select current subtitle while playing",

                    AutoRepeat = "Auto repeat",
                    AutoRepeatOn = "Auto repeat on",
                    AutoRepeatCount = "Repeat count (times)",
                    AutoContinue = "Auto continue",
                    AutoContinueOn = "Auto continue on",
                    DelayInSeconds = "Delay (seconds)",
                    Previous = "< Pre&vious",
                    Next = "&Next >",
                    PlayCurrent = "&Play current",
                    Stop = "&Stop",
                    Playing = "Playing...",
                    RepeatingLastTime = "Repeating... last time",
                    RepeatingXTimesLeft = "Repeating... {0} times left",
                    AutoContinueInOneSecond = "Auto continue in one second",
                    AutoContinueInXSeconds = "Auto continue in {0} seconds",
                    StillTypingAutoContinueStopped = "Still typing... auto continue stopped",

                    InsertNewSubtitleAtVideoPosition = "Insert new subtitle at video pos",
                    InsertNewSubtitleAtVideoPositionNoTextBoxFocus = "Insert new subtitle at video pos (no text box focus)",
                    InsertNewSubtitleAtVideoPositionMax = "Insert new subtitle at video pos (as long as possible)",
                    Auto = "Auto",
                    PlayFromJustBeforeText = "Play from just before text",
                    PlayFromBeginning = "Play from beginning of video",
                    Pause = "Pause",
                    GoToSubtitlePositionAndPause = "Go to sub position and pause",
                    SetStartTime = "Set start time",
                    SetEndTimeAndGoToNext = "Set end && go to next",
                    AdjustedViaEndTime = "Adjusted via end time {0}",
                    SetEndTime = "Set end time",
                    SetStartTimeAndOffsetTheRest = "Set start and offset the rest",

                    GoogleIt = "Google it",
                    GoogleTranslate = "Google translate",
                    AutoTranslate = "Auto-translate",
                    OriginalText = "Original text",
                    SearchTextOnline = "Search text online",
                    SecondsBackShort = "<<",
                    SecondsForwardShort = ">>",
                    VideoPosition = "Video position:",
                    TranslateTip = "Tip: Use <alt+arrow up/down> to go to previous/next subtitle",

                    BeforeChangingTimeInWaveformX = "Before changing time in waveform: {0}",
                    NewTextInsertAtX = "New text inserted at {0}",

                    Center = "Center",
                    PlayRate = "Play rate (speed)",
                },
            };

            MatroskaSubtitleChooser = new LanguageStructure.MatroskaSubtitleChooser
            {
                Title = "Choose subtitle from Matroska file",
                TitleMp4 = "Choose subtitle from MP4 file",
                PleaseChoose = "More than one subtitle found - please choose",
                TrackXLanguageYTypeZ = "Track {0} - language: {1} - type: {2}",
            };

            MeasurementConverter = new LanguageStructure.MeasurementConverter
            {
                Title = "Measurement converter",
                ConvertFrom = "Convert from",
                ConvertTo = "Convert to",
                CopyToClipboard = "Copy to clipboard",
                CloseOnInsert = "Close on insert",
                Insert = "Insert",

                Length = "Length",
                Mass = "Mass",
                Volume = "Volume",
                Area = "Area",
                Time = "Time",
                Temperature = "Temperature",
                Velocity = "Velocity",
                Force = "Force",
                Energy = "Energy",
                Power = "Power",
                Pressure = "Pressure",

                Kilometers = "Kilometers",
                Meters = "Meters",
                Centimeters = "Centimeters",
                Millimeters = "Millimeters",
                Micrometers = "Micrometers",
                Nanometers = "Nanometers",
                Angstroms = "Angstroms",
                MilesTerrestial = "Miles (terrestrial)",
                MilesNautical = "Miles (nautical)",
                Yards = "Yards",
                Feet = "Feet",
                Inches = "Inches",
                Chains = "Chains",
                Fathoms = "Fathoms",
                Hands = "Hands",
                Rods = "Rods",
                Spans = "Spans",

                LongTonnes = "Long tonnes",
                ShortTonnes = "Short tonnes",
                Tonnes = "Tonnes",
                Kilos = "Kilograms",
                Grams = "Grams",
                Milligrams = "Milligrams",
                Micrograms = "Micrograms",
                Pounds = "Pounds",
                Ounces = "Ounces",
                Carats = "Carats",
                Drams = "Drams",
                Grains = "Grains",
                Stones = "Stones",

                CubicKilometers = "Cubic kilometers",
                CubicMeters = "Cubic meters",
                Litres = "Litres",
                CubicCentimeters = "Cubic centimeters",
                CubicMillimeters = "Cubic millimeters",
                CubicMiles = "Cubic miles",
                CubicYards = "Cubic yards",
                CubicFTs = "Cubic fts",
                CubicInches = "Cubic inches",
                OilBarrels = "Oil barrels",
                GallonUS = "Gallon (US)",
                QuartsUS = "Quarts (US)",
                PintsUS = "Pints (US)",
                FluidOuncesUS = "Fluid ounces (US)",
                Bushels = "Bushels",
                Pecks = "Pecks",
                GallonsUK = "Gallons (UK)",
                QuartsUK = "Quarts (UK)",
                PintsUK = "Pints (UK)",
                FluidOuncesUK = "Fluid ounces (UK)",

                SquareKilometers = "Square kilometers",
                SquareMeters = "Square meters",
                SquareCentimeters = "Square Centimeters",
                SquareMillimeters = "Square millimeters",
                SquareMiles = "Square miles",
                SquareYards = "Square yards",
                SquareFTs = "Square fts",
                SquareInches = "Square inches",
                Hectares = "Hectares",
                Acres = "Acres",
                Ares = "Ares",

                Hours = "Hours",
                Minutes = "Minutes",
                Seconds = "Seconds",
                Milliseconds = "Milliseconds",
                Microseconds = "Microseconds",

                Fahrenheit = "Fahrenheit",
                Celsius = "Celsius",
                Kelvin = "Kelvin",

                KilometersPerHour = "Kilometers/Hour",
                MetersPerSecond = "Meters/Seconds",
                MilesPerHour = "Miles/Hour",
                YardsPerMinute = "Yards/Minute",
                FTsPerSecond = "fts/Second",
                Knots = "Knots",

                PoundsForce = "Pounds-Force",
                Newtons = "Newtons",
                KilosForce = "Kilos-Force",

                Jouls = "Jouls",
                Calories = "Calories",
                Ergs = "Ergs",
                ElectronVolts = "Electron-volts",
                Btus = "Btus",

                Watts = "Watts",
                Horsepower = "Horsepower",

                Atmospheres = "Atmospheres",
                Bars = "Bars",
                Pascals = "Pascals",
                MillimetersOfMercury = "Millimeters of Mercury",
                PoundPerSquareInch = "Pound-force per square inch",
                KilogramPerSquareCentimeter = "Kilogram-force per square centimeter",
                KiloPascals = "Kilopascals",
            };

            MergeDoubleLines = new LanguageStructure.MergeDoubleLines
            {
                Title = "Merge lines with same text",
                MaxMillisecondsBetweenLines = "Max. milliseconds between lines",
                IncludeIncrementing = "Include incrementing lines",
            };

            MergedShortLines = new LanguageStructure.MergeShortLines
            {
                Title = "Merge short lines",
                MaximumCharacters = "Maximum characters in one paragraph",
                MaximumMillisecondsBetween = "Maximum milliseconds between lines",
                NumberOfMergesX = "Number of merges: {0}",
                MergedText = "Merged text",
                OnlyMergeContinuationLines = "Only merge continuation lines",
            };

            MergeTextWithSameTimeCodes = new LanguageStructure.MergeTextWithSameTimeCodes
            {
                Title = "Merge lines with same time codes",
                MaxDifferenceMilliseconds = "Max. milliseconds difference",
                MakeDialog = "Make dialog",
                ReBreakLines = "Re-break lines",
                NumberOfMergesX = "Number of merges: {0}",
                MergedText = "Merged text",
            };

            ModifySelection = new LanguageStructure.ModifySelection
            {
                Title = "Modify selection",
                Rule = "Rule",
                CaseSensitive = "Case sensitive",
                DoWithMatches = "What to do with matches",
                MakeNewSelection = "Make new selection",
                AddToCurrentSelection = "Add to current selection",
                SubtractFromCurrentSelection = "Subtract from current selection",
                IntersectWithCurrentSelection = "Intersect with current selection",
                MatchingLinesX = "Matching lines: {0:#,##0}",
                Contains = "Contains",
                StartsWith = "Starts with",
                EndsWith = "Ends with",
                NoContains = "Not contains",
                RegEx = "Regular expression",
                OddLines = "Odd-numbered lines",
                EvenLines = "Even-numbered lines",
                DurationLessThan = "Duration less than",
                DurationGreaterThan = "Duration greater than",
                CpsLessThan = "CPS less than",
                CpsGreaterThan = "CPS greater than",
                LengthLessThan = "Length less than",
                LengthGreaterThan = "Length greater than",
                ExactlyOneLine = "Exactly one line",
                ExactlyTwoLines = "Exactly two lines",
                MoreThanTwoLines = "More than two lines",
                Bookmarked = "Bookmarked",
                BlankLines = "Blank lines",
            };

            MultipleReplace = new LanguageStructure.MultipleReplace
            {
                Title = "Multiple replace",
                FindWhat = "Find what",
                ReplaceWith = "Replace with",
                Normal = "Normal",
                CaseSensitive = "Case sensitive",
                RegularExpression = "Regular expression",
                Description = "Description",
                LinesFoundX = "Lines found: {0}",
                Remove = "Remove",
                Add = "&Add",
                Update = "&Update",
                Enabled = "Enabled",
                SearchType = "Search type",
                RemoveAll = "Remove all",
                Import = "Import...",
                Export = "Export...",
                ImportRulesTitle = "Import replace rule(s) from...",
                ExportRulesTitle = "Export replace rule(s) to...",
                ChooseGroupsToImport = "Choose groups to import",
                ChooseGroupsToExport = "Choose groups to export",
                Rules = "Find and replace rules",
                MoveToBottom = "Move to bottom",
                MoveToTop = "Move to top",
                GroupName = "Group name",
                Groups = "Groups",
                MoveSelectedRulesToGroup = "Move selected rules to group",
                RenameGroup = "Rename group...",
                RulesForGroupX = "Rules for group \"{0}\"",
                NewGroup = "New group...",
                NothingToImport = "Nothing to import",
                RuleInfo = "Rule information",
            };

            NetworkChat = new LanguageStructure.NetworkChat
            {
                Title = "Chat",
                Send = "Send",
            };

            NetworkJoin = new LanguageStructure.NetworkJoin
            {
                Title = "Join network session",
                Information = @"Join existing session where multiple persons
can edit in same subtitle file (collaboration)",
                Join = "Join",
            };

            NetworkLogAndInfo = new LanguageStructure.NetworkLogAndInfo
            {
                Title = "Network session info and log",
                Log = "Log:",
            };

            NetworkStart = new LanguageStructure.NetworkStart
            {
                Title = "Start network session",
                ConnectionTo = "Connecting to {0}...",
                Information = @"Start new session where multiple persons
can edit in same subtitle file (collaboration)",
                Start = "Start",
            };

            OpenVideoDvd = new LanguageStructure.OpenVideoDvd
            {
                Title = "Open DVD via VLC",
                OpenDvdFrom = "Open DVD from...",
                Disc = "Disc",
                Folder = "Folder",
                ChooseDrive = "Choose drive",
                ChooseFolder = "Choose folder",
            };

            PluginsGet = new LanguageStructure.PluginsGet
            {
                Title = "Plugins",
                InstalledPlugins = "Installed plugins",
                GetPlugins = "Get plugins",
                Description = "Description",
                Version = "Version",
                Date = "Date",
                Type = "Type",
                OpenPluginsFolder = "Open 'Plugins' folder",
                GetPluginsInfo1 = "Subtitle Edit plugins must be downloaded to the 'Plugins' folder",
                GetPluginsInfo2 = "Choose plugin and click 'Download'",
                PluginXDownloaded = "Plugin {0} downloaded",
                Download = "&Download",
                Remove = "&Remove",
                UpdateAllX = "Update all ({0})",
                UnableToDownloadPluginListX = "Unable to download plug list: {0}",
                NewVersionOfSubtitleEditRequired = "Newer version of Subtitle Edit required!",
                UpdateAvailable = "[Update available!]",
                UpdateAll = "Update all",
                XPluginsUpdated = "{0} plugin(s) updated",
            };

            RegularExpressionContextMenu = new LanguageStructure.RegularExpressionContextMenu
            {
                WordBoundary = "Word boundary (\\b)",
                NonWordBoundary = "Non word boundary (\\B)",
                NewLine = "New line (\\r\\n)",
                NewLineShort = "New line (\\n)",
                AnyDigit = "Any digit (\\d)",
                NonDigit = "Non digit (\\D)",
                AnyCharacter = "Any character (.)",
                NonSpaceCharacter = "Non space character (\\S)",
                AnyWhitespace = "Any whitespace (\\s)",
                ZeroOrMore = "Zero or more (*)",
                OneOrMore = "One or more (+)",
                InCharacterGroup = "In character group ([test])",
                NotInCharacterGroup = "Not in character group ([^test])",
            };

            RemoveTextFromHearImpaired = new LanguageStructure.RemoveTextFromHearImpaired
            {
                Title = "Remove text for hearing impaired",
                RemoveTextConditions = "Remove text conditions",
                RemoveTextBetween = "Remove text between",
                SquareBrackets = "'[' and ']'",
                Brackets = "'{' and '}'",
                QuestionMarks = "'?' and '?'",
                Parentheses = "'(' and ')'",
                And = "and",
                RemoveTextBeforeColon = "Remove text before a colon (':')",
                OnlyIfTextIsUppercase = "Only if text is UPPERCASE",
                OnlyIfInSeparateLine = "Only if in separate line",
                LinesFoundX = "Lines found: {0}",
                RemoveTextIfContains = "Remove text if it contains:",
                RemoveTextIfAllUppercase = "Remove line if UPPERCASE",
                RemoveIfOnlyMusicSymbols = "Remove if only music symbols",
                RemoveInterjections = "Remove interjections (shh, hmm, etc.)",
                EditInterjections = "Edit...",
                Apply = "A&pply",
            };

            ReplaceDialog = new LanguageStructure.ReplaceDialog
            {
                Title = "Replace",
                FindWhat = "Find what:",
                Normal = "&Normal",
                CaseSensitive = "&Case sensitive",
                RegularExpression = "Regular e&xpression",
                ReplaceWith = "Replace with:",
                Find = "&Find",
                Replace = "&Replace",
                ReplaceAll = "Replace &all",
                FindReplaceIn = "Replace/search in:",
                TranslationAndOriginal = "Translation and original",
                TranslationOnly = "Translation only",
                OriginalOnly = "Original only",
            };

            RestoreAutoBackup = new LanguageStructure.RestoreAutoBackup
            {
                Title = "Restore auto backup",
                Information = "Open auto-saved backup",
                DateAndTime = "Date and time",
                FileName = "File name",
                Extension = "Extension",
                NoBackedUpFilesFound = "No backup files found!",
            };

            SeekSilence = new LanguageStructure.SeekSilence
            {
                Title = "Seek silence",
                SearchDirection = "Search direction",
                Forward = "Forward",
                Back = "Back",
                LengthInSeconds = "Silence must be at least (seconds)",
                MaxVolume = "Volume must be below",
            };

            SetMinimumDisplayTimeBetweenParagraphs = new LanguageStructure.SetMinimumDisplayTimeBetweenParagraphs
            {
                Title = "Apply minimum gap between subtitles",
                PreviewLinesModifiedX = "Preview - subtitles modified: {0}",
                MinimumMillisecondsBetweenParagraphs = "Minimum milliseconds between lines",
                ShowOnlyModifiedLines = "Show only modified lines",
                FrameInfo = "Frame rate info",
                Frames = "Frames",
                XFrameYisZMilliseconds = "{0} frame(s) at {1} fps is {2} milliseconds",
            };

            SetSyncPoint = new LanguageStructure.SetSyncPoint
            {
                Title = "Set Sync point for line {0}",
                SyncPointTimeCode = "Sync point time code",
                ThreeSecondsBack = "<< 3 s",
                HalfASecondBack = "<< ½ s",
                HalfASecondForward = "½ s >>",
                ThreeSecondsForward = "3 s >>",
            };

            Settings = new LanguageStructure.Settings
            {
                Title = "Settings",
                General = "General",
                SubtitleFormats = "Subtitle formats",
                Toolbar = "Toolbar",
                VideoPlayer = "Video player",
                WaveformAndSpectrogram = "Waveform/spectrogram",
                Tools = "Tools",
                WordLists = "Word lists",
                SsaStyle = "ASS/SSA Style",
                Network = "Network",
                FileTypeAssociations = "File type associations",
                Rules = "Rules",
                NetworkSessionSettings = "Network session settings",
                NetworkSessionNewSound = "Play sound file when new message arrives",
                ShowToolBarButtons = "Show tool bar buttons",
                New = "New",
                Open = "Open",
                Save = "Save",
                SaveAs = "Save as",
                Find = "Find",
                Replace = "Replace",
                VisualSync = "Visual sync",
                BurnIn = "Burn in",
                SpellCheck = "Spell check",
                NetflixQualityCheck = "Netflix quality check",
                BeautifyTimeCodes = "Beautify time codes",
                SettingsName = "Settings",
                ToggleBookmarks = "Toggle bookmarks",
                FocusTextBox = "Focus text box",
                ToggleBookmarksWithComment = "Toggle bookmarks - add comment",
                ClearBookmarks = "Clear bookmarks",
                ExportBookmarks = "Export bookmarks...",
                GoToBookmark = "Go to bookmark...",
                GoToPreviousBookmark = "Go to previous bookmark",
                GoToNextBookmark = "Go to next bookmark",
                ChooseProfile = "Choose profile",
                OpenDataFolder = "Open the Subtitle Edit data folder",
                DuplicateLine = "Duplicate line",
                ToggleView = "Toggle list/source view",
                TogglePreviewOnVideo = "Toggle preview on video",
                ToggleMode = "Toggle translate/create/adjust mode",
                Help = "Help",
                FontInUi = "UI Font",
                Appearance = "Appearance",
                ShowFrameRate = "Show frame rate in toolbar",
                DefaultFrameRate = "Default frame rate",
                DefaultFileEncoding = "Default file encoding",
                AutoDetectAnsiEncoding = "Auto detect ANSI encoding",
                LanguageFilter = "Language filter",
                Profile = "Profile",
                Profiles = "Profiles",
                ImportProfiles = "Import profiles",
                ExportProfiles = "Export profiles",
                SubtitleLineMaximumLength = "Single line max. length",
                OptimalCharactersPerSecond = "Optimal chars/sec",
                MaximumCharactersPerSecond = "Max. chars/sec",
                MaximumWordsPerMinute = "Max. words/min",
                AutoWrapWhileTyping = "Auto-wrap while typing",
                DurationMinimumMilliseconds = "Min. duration, milliseconds",
                DurationMaximumMilliseconds = "Max. duration, milliseconds",
                MinimumGapMilliseconds = "Min. gap between subtitles in ms",
                MaximumLines = "Max. number of lines",
                SubtitleFont = "Subtitle font",
                SubtitleFontSize = "Subtitle font size",
                SubtitleBold = "Bold",
                SubtitleCenter = "Center",
                SubtitleFontColor = "Subtitle font color",
                SubtitleBackgroundColor = "Subtitle background color",
                SpellChecker = "Spell checker",
                VideoAutoOpen = "Auto-open video file when opening subtitle",
                AllowVolumeBoost = "Allow volume boost",
                RememberRecentFiles = "Show recent files (for reopen)",
                StartWithLastFileLoaded = "Start with last file loaded",
                RememberSelectedLine = "Remember selected line",
                RememberPositionAndSize = "Remember main window position and size",
                StartInSourceView = "Start in source view",
                RemoveBlankLinesWhenOpening = "Remove blank lines when opening a subtitle",
                RemoveBlankLines = "Remove blank lines",
                ApplyAssaOverrideTags = "Apply ASSA override tags to selection",
                SetAssaPosition = "Set/get ASSA position",
                SetAssaResolution = "Set ASSA resolution (PlayResX/PlayResY)",
                SetAssaBgBox = "Set ASSA background box",
                TakeAutoBackup = "Take auto-backup now",
                ShowLineBreaksAs = "Show line breaks in list view as",
                SaveAsFileNameFrom = "\"Save as...\" uses file name from",
                MainListViewDoubleClickAction = "Double-clicking line in main window list view will",
                MainListViewColumnsInfo = "Choose visible list view columns",
                MainListViewNothing = "Nothing",
                MainListViewVideoGoToPositionAndPause = "Go to video position and pause",
                MainListViewVideoGoToPositionAndPlay = "Go to video position and play",
                MainListViewVideoGoToPositionAndPlayCurrentAndPause = "Go to video position, play current, and pause",
                MainListViewEditText = "Go to edit text box",
                MainListViewVideoGoToPositionMinus1SecAndPause = "Go to video position - 1 s and pause",
                MainListViewVideoGoToPositionMinusHalfSecAndPause = "Go to video position - 0.5 s and pause",
                MainListViewVideoGoToPositionMinus1SecAndPlay = "Go to video position - 1 s and play",
                MainListViewEditTextAndPause = "Go to edit text box, and pause at video position",
                VideoFileName = "Video file name",
                ExistingFileName = "Existing file name",
                AutoBackup = "Auto-backup",
                AutoBackupEveryMinute = "Every minute",
                AutoBackupEveryFiveMinutes = "Every 5th minute",
                AutoBackupEveryFifteenMinutes = "Every 15th minute",
                AutoBackupDeleteAfter = "Delete after",
                TranslationAutoSuffix = "Translation file name auto suffix",
                AutoBackupDeleteAfterOneMonth = "1 month",
                AutoBackupDeleteAfterXMonths = "{0} months",
                CheckForUpdates = "Check for updates",
                AutoSave = "Auto save",
                AllowEditOfOriginalSubtitle = "Allow edit of original subtitle",
                PromptDeleteLines = "Prompt for delete lines",
                TimeCodeMode = "Time code mode",
                TimeCodeModeHHMMSSMS = "HH:MM:SS.MS (00:00:01.500)",
                TimeCodeModeHHMMSSFF = "HH:MM:SS:FF (00:00:01:12)",
                SplitBehavior = "Split behavior",
                SplitBehaviorPrevious = "Add gap to the left of split point (focus right)",
                SplitBehaviorHalf = "Add gap in the center of split point (focus left)",
                SplitBehaviorNext = "Add gap to the right of split point (focus left)",
                VideoEngine = "Video engine",
                DirectShow = "DirectShow",
                DirectShowDescription = "quartz.dll in system32 folder",
                MpcHc = "MPC-HC",
                MpcHcDescription = "Media Player Classic - Home Cinema",
                MpvPlayer = "mpv",
                MpvPlayerDescription = "https://mpv.io/ - free, open source, and cross-platform media player",
                MpvHandlesPreviewText = "mpv handles preview text",
                VlcMediaPlayer = "VLC media player",
                VlcMediaPlayerDescription = "libvlc.dll from VLC media player 1.1.0 or newer",
                VlcBrowseToLabel = "VLC path (only needed if you're using the portable version of VLC)",
                ShowStopButton = "Show stop button",
                ShowMuteButton = "Show mute button",
                ShowFullscreenButton = "Show fullscreen button",
                PreviewFontName = "Subtitle preview font name",
                PreviewFontSize = "Subtitle preview font size",
                PreviewVerticalMargin = "Vertical margin",
                MainWindowVideoControls = "Main window video controls",
                CustomSearchTextAndUrl = "Custom search text and URL",
                WaveformAppearance = "Waveform appearance",
                WaveformGridColor = "Grid color",
                WaveformShowGridLines = "Show grid lines",
                WaveformShowCps = "Show chars/sec",
                WaveformShowWpm = "Show words/min",
                ReverseMouseWheelScrollDirection = "Reverse mouse wheel scroll direction",
                WaveformAllowOverlap = "Allow overlap (when moving/resizing)",
                WaveformSetVideoPosMoveStartEnd = "Set video position when moving start/end",
                WaveformFocusMouseEnter = "Set focus on mouse enter",
                WaveformListViewFocusMouseEnter = "Also set list view focus on mouse enter in list view",
                WaveformSingleClickSelect = "Single click to select subtitles",
                WaveformSnapToShotChanges = "Snap to shot changes (hold Shift to override)",
                WaveformEditShotChangesProfile = "Edit profile...",
                WaveformAutoGen = "Auto generate waveform when opening video",
                WaveformBorderHitMs1 = "Border marker hit must be within",
                WaveformBorderHitMs2 = "milliseconds",
                WaveformColor = "Color",
                WaveformSelectedColor = "Selected color",
                WaveformBackgroundColor = "Back color",
                WaveformTextColor = "Text color",
                WaveformCursorColor = "Cursor color",
                WaveformTextFontSize = "Text font size",
                WaveformAndSpectrogramsFolderEmpty = "Empty 'Spectrograms' and 'Waveforms' folders",
                WaveformAndSpectrogramsFolderInfo = "'Waveforms' and 'Spectrograms' folders contain {0} files ({1:0.00} MB)",
                Spectrogram = "Spectrogram",
                GenerateSpectrogram = "Generate spectrogram",
                SpectrogramAppearance = "Spectrogram appearance",
                SpectrogramOneColorGradient = "One color gradient",
                SpectrogramClassic = "Classic",
                WaveformUseFFmpeg = "Use FFmpeg for wave extraction",
                WaveformUseCenterChannelOnly = "Use front center channel only (for 5.1/7.1)",
                DownloadX = "Download {0}",
                WaveformFFmpegPath = "Path to FFmpeg",
                WaveformBrowseToFFmpeg = "Browse to FFmpeg",
                WaveformBrowseToVLC = "Browse to VLC portable",
                SubStationAlphaStyle = "(Advanced) Sub Station Alpha style",
                ChooseColor = "Choose color",
                SsaOutline = "Outline",
                SsaShadow = "Shadow",
                SsaOpaqueBox = "Opaque box",
                Testing123 = "Testing 123...",
                Language = "Language",
                NamesIgnoreLists = "Names/ignore list (case sensitive)",
                AddName = "Add name",
                AddWord = "Add word",
                Remove = "Remove",
                AddPair = "Add pair",
                UserWordList = "User word list",
                OcrFixList = "OCR fix list",
                Location = "Location",
                UseOnlineNames = "Use online names xml file",
                WordAddedX = "Word added: {0}",
                WordAlreadyExists = "Word already exists!",
                RemoveX = "Remove {0}?",
                WordNotFound = "Word not found",
                CannotUpdateNamesOnline = "Cannot update names.xml online!",
                ProxyServerSettings = "Proxy server settings",
                ProxyAddress = "Proxy address",
                ProxyAuthentication = "Authentication",
                ProxyUserName = "User name",
                ProxyPassword = "Password",
                ProxyDomain = "Domain",
                ProxyAuthType = "Auth type",
                ProxyUseDefaultCredentials = "Use default credentials",
                PlayXSecondsAndBack = "Play X seconds and back, X is",
                StartSceneIndex = "Start scene paragraph is",
                EndSceneIndex = "End scene paragraph is",
                FirstPlusX = "First + {0}",
                LastMinusX = "Last - {0}",
                FixCommonerrors = "Fix common errors",
                RemoveTextForHi = "Remove text for HI",
                MergeLinesShorterThan = "Unbreak subtitles shorter than",
                DialogStyle = "Dialog style",
                DialogStyleDashBothLinesWithSpace = "Dash both lines with space",
                DialogStyleDashBothLinesWithoutSpace = "Dash both lines without space",
                DialogStyleDashSecondLineWithSpace = "Dash second line with space",
                DialogStyleDashSecondLineWithoutSpace = "Dash second line without space",
                ContinuationStyle = "Continuation style",
                CpsLineLengthStyle = "Cps/line-length",
                CpsLineLengthStyleCalcAll = "Count all characters",
                CpsLineLengthStyleCalcNoSpaceCpsOnly = "Count all except space, cps only",
                CpsLineLengthStyleCalcNoSpace = "Count all except space",
                CpsLineLengthStyleCalcCjk = "CJK 1, Latin 0.5",
                CpsLineLengthStyleCalcCjkNoSpace = "CJK 1, Latin 0.5, space 0",
                CpsLineLengthStyleCalcIncludeCompositionCharacters = "Include composition characters",
                CpsLineLengthStyleCalcIncludeCompositionCharactersNotSpace = "Include composition characters, not space",
                CpsLineLengthStyleCalcNoSpaceOrPunctuation = "No space or punctuation ()[]-:;,.!?",
                CpsLineLengthStyleCalcNoSpaceOrPunctuationCpsOnly = "No space or punctuation, cps only",
                ContinuationStyleNone = "None",
                ContinuationStyleNoneTrailingDots = "None, dots for pauses (trailing only)",
                ContinuationStyleNoneLeadingTrailingDots = "None, dots for pauses",
                ContinuationStyleNoneTrailingEllipsis = "None, ellipsis for pauses (trailing only)",
                ContinuationStyleNoneLeadingTrailingEllipsis = "None, ellipsis for pauses",
                ContinuationStyleOnlyTrailingDots = "Dots (trailing only)",
                ContinuationStyleLeadingTrailingDots = "Dots",
                ContinuationStyleOnlyTrailingEllipsis = "Ellipsis (trailing only)",
                ContinuationStyleLeadingTrailingEllipsis = "Ellipsis",
                ContinuationStyleLeadingTrailingDash = "Dash",
                ContinuationStyleLeadingTrailingDashDots = "Dash, but dots for pauses",
                ContinuationStyleCustom = "Custom",
                MusicSymbol = "Music symbol",
                MusicSymbolsReplace = "Music symbols to replace (separate by comma)",
                FixCommonOcrErrorsUseHardcodedRules = "Fix common OCR errors - also use hard-coded rules",
                UseWordSplitList = "Use word split list (OCR + FCE)",
                AvoidPropercase = "Avoid propercase",
                FixCommonerrorsFixShortDisplayTimesAllowMoveStartTime = "Fix short display time - allow move of start time",
                FixCommonErrorsSkipStepOne = "Skip step one (choose fix rules)",
                DefaultFormat = "Default format",
                DefaultSaveAsFormat = "Default save as format",
                DefaultSaveAsFormatAuto = "- Auto -",
                Favorites = "Favorites",
                FavoriteFormats = "Favorite formats",
                FavoriteSubtitleFormatsNote = "Note: favorite formats will be shown first when selecting a format, the default format will always be shown first",
                Shortcuts = "Shortcuts",
                Shortcut = "Shortcut",
                Control = "Control",
                Alt = "Alt",
                Shift = "Shift",
                Key = "Key",
                ListView = "List view",
                TextBox = "Text box",
                UseSyntaxColoring = "Use syntax coloring",
                HtmlColor = "Html color",
                AssaColor = "ASSA color",
                Automatic = "Automatic",
                Theme = "Theme",
                DarkTheme = "Dark theme",
                DarkThemeEnabled = "Use dark theme",
                DarkThemeShowGridViewLines = "Show list view grid lines",
                GraphicsButtons = "Graphics buttons",
                ListViewAndTextBox = "List view and text box",
                UpdateShortcut = "Update",
                FocusSetVideoPosition = "Focus set video position",
                ToggleDockUndockOfVideoControls = "Toggle dock/undock of video controls",
                CreateSetEndAddNewAndGoToNew = "Set end, add new and go to new",
                AdjustViaEndAutoStart = "Adjust via end position",
                AdjustViaEndAutoStartAndGoToNext = "Adjust via end position and go to next",
                AdjustSetEndMinusGapAndStartNextHere = "Set end minus gap, go to next and start next here",
                AdjustSetEndAndStartNextAfterGap = "Set end and start of next after gap",
                AdjustSetStartTimeAndGoToNext = "Set start and go to next",
                AdjustSetEndTimeAndGoToNext = "Set end and go to next",
                AdjustSetEndTimeAndPause = "Set end and pause",
                AdjustSetStartAutoDurationAndGoToNext = "Set start, auto duration and go to next",
                AdjustSetEndNextStartAndGoToNext = "Set end, next start and go to next",
                AdjustStartDownEndUpAndGoToNext = "Key down=set start, Key up=set end and go to next",
                AdjustSetStartAndEndOfPrevious = "Set start and set end of previous (minus min gap)",
                AdjustSetStartAndEndOfPreviousAndGoToNext = "Set start and set end of previous and go to next (minus min gap)",
                AdjustSelected100MsForward = "Move selected lines 100 ms forward",
                AdjustSelected100MsBack = "Move selected lines 100 ms back",
                AdjustStartXMsBack = "Move start {0} ms back",
                AdjustStartXMsForward = "Move start {0} ms forward",
                AdjustEndXMsBack = "Move end {0} ms back",
                AdjustEndXMsForward = "Move end {0} ms forward",
                AdjustStartOneFrameBack = "Move start 1 frame back",
                AdjustStartOneFrameForward = "Move start 1 frame forward",
                AdjustEndOneFrameBack = "Move end 1 frame back",
                AdjustEndOneFrameForward = "Move end 1 frame forward",
                AdjustStartOneFrameBackKeepGapPrev = "Move start 1 frame back (keep gap to previous if close)",
                AdjustStartOneFrameForwardKeepGapPrev = "Move start 1 frame forward (keep gap to previous if close)",
                AdjustEndOneFrameBackKeepGapNext = "Move end 1 frame back (keep gap to next if close)",
                AdjustEndOneFrameForwardKeepGapNext = "Move end 1 frame forward (keep gap to next if close)",
                AdjustSetStartTimeKeepDuration = "Set start time, keep duration",
                AdjustVideoSetStartForAppropriateLine = "Set start for appropriate line",
                AdjustVideoSetEndForAppropriateLine = "Set end for appropriate line",
                AdjustSetStartAndOffsetTheWholeSubtitle = "Set start time, offset the whole subtitle",
                AdjustSetEndAndOffsetTheRest = "Set end, offset the rest",
                AdjustSetEndAndOffsetTheRestAndGoToNext = "Set end, offset the rest and go to next",
                AdjustSnapStartToNextShotChange = "Snap selected lines start to next shot change",
                AdjustSnapEndToPreviousShotChange = "Snap selected lines end to previous shot change",
                AdjustExtendToNextShotChange = "Extend selected lines to next shot change (or next subtitle)",
                AdjustExtendToPreviousShotChange = "Extend selected lines to previous shot change (or previous subtitle)",
                AdjustExtendToNextSubtitle = "Extend selected lines to next subtitle",
                AdjustExtendToPreviousSubtitle = "Extend selected lines to previous subtitle",
                AdjustExtendToNextSubtitleMinusChainingGap = "Extend selected lines to next subtitle with chaining gap",
                AdjustExtendToPreviousSubtitleMinusChainingGap = "Extend selected lines to previous subtitle with chaining gap",
                AdjustExtendCurrentSubtitle = "Extend current line to next subtitle or max duration",
                AdjustExtendPreviousLineEndToCurrentStart = "Extend previous line's end to current's start",
                AdjustExtendNextLineStartToCurrentEnd = "Extend next line's start to current's end",
                RecalculateDurationOfCurrentSubtitle = "Re-calculate duration of current subtitle",
                RecalculateDurationOfCurrentSubtitleByOptimalReadingSpeed = "Re-calculate duration of current subtitle (based on optimal reading speed)",
                RecalculateDurationOfCurrentSubtitleByMinReadingSpeed = "Re-calculate duration of current subtitle (based on minimum reading speed)",
                SetInCueToClosestShotChangeLeftGreenZone = "Set in cue to minimum distance before closest shot change (left green zone)",
                SetInCueToClosestShotChangeRightGreenZone = "Set in cue to minimum distance after closest shot change (right green zone)",
                SetOutCueToClosestShotChangeLeftGreenZone = "Set out cue to minimum distance before closest shot change (left green zone)",
                SetOutCueToClosestShotChangeRightGreenZone = "Set out cue to minimum distance after closest shot change (right green zone)",
                MainCreateStartDownEndUp = "Insert new subtitle at key-down, set end time at key-up",
                MergeDialog = "Merge dialog (insert dashes)",
                MergeDialogWithNext = "Merge dialog with next (insert dashes)",
                MergeDialogWithPrevious = "Merge dialog with previous (insert dashes)",
                AutoBalanceSelectedLines = "Auto balance selected lines",
                EvenlyDistributeSelectedLines = "Evenly distribute selected lines (CPS)",
                GoToNext = "Go to next line",
                GoToNextPlayTranslate = "Go to next line (and play in 'Translate mode')",
                GoToNextCursorAtEnd = "Go to next line and set cursor at end",
                GoToPrevious = "Go to previous line",
                GoToPreviousPlayTranslate = "Go to previous line (and play in 'Translate mode')",
                GoToCurrentSubtitleStart = "Go to current line start",
                GoToCurrentSubtitleEnd = "Go to current line end",
                GoToPreviousSubtitleAndFocusVideo = "Go to previous line and set video position",
                GoToNextSubtitleAndFocusVideo = "Go to next line and set video position",
                GoToPrevSubtitleAndPlay = "Go to previous line and play",
                GoToNextSubtitleAndPlay = "Go to next line and play",
                GoToPreviousSubtitleAndFocusWaveform = "Go to previous line and focus waveform",
                GoToNextSubtitleAndFocusWaveform = "Go to next line and focus waveform",
                ToggleFocus = "Toggle focus between list view and subtitle text box",
                ToggleFocusWaveform = "Toggle focus between list view and waveform/spectrogram",
                ToggleFocusWaveformTextBox = "Toggle focus between text box and waveform/spectrogram",
                ToggleDialogDashes = "Toggle dialog dashes",
                ToggleQuotes = "Toggle quotes",
                ToggleHiTags = "Toggle HI tags",
                ToggleCustomTags = "Toggle custom tags (surround with)",
                ToggleMusicSymbols = "Toggle music symbols",
                Alignment = "Alignment (selected lines)",
                AlignmentN1 = "Alignment bottom left - {\\an1}",
                AlignmentN2 = "Alignment bottom center - {\\an2}",
                AlignmentN3 = "Alignment bottom right - {\\an3}",
                AlignmentN4 = "Alignment middle left - {\\an4}",
                AlignmentN5 = "Alignment middle center - {\\an5}",
                AlignmentN6 = "Alignment middle right - {\\an6}",
                AlignmentN7 = "Alignment top left - {\\an7}",
                AlignmentN8 = "Alignment top center - {\\an8}",
                AlignmentN9 = "Alignment top right - {\\an9}",
                ColorX = "Color {0} ({1})",
                CopyTextOnly = "Copy text only to clipboard (selected lines)",
                CopyPlainText = "Copy plain text to clipboard (selected lines)",
                CopyTextOnlyFromOriginalToCurrent = "Copy text from original to current",
                AutoDurationSelectedLines = "Auto-duration (selected lines)",
                FixRTLViaUnicodeChars = "Fix RTL via Unicode control characters",
                RemoveRTLUnicodeChars = "Remove Unicode control characters",
                ReverseStartAndEndingForRtl = "Reverse RTL start/end",
                VerticalZoom = "Vertical zoom in",
                VerticalZoomOut = "Vertical zoom out",
                WaveformSeekSilenceForward = "Seek silence forward",
                WaveformSeekSilenceBack = "Seek silence back",
                WaveformAddTextHere = "Add text here (for new selection)",
                WaveformAddTextHereFromClipboard = "Add text here (for new selection from clipboard)",
                ChooseLayoutX = "Choose layout {0}",
                SetParagraphAsSelection = "Set current as new selection",
                WaveformPlayNewSelection = "Play selection",
                WaveformPlayNewSelectionEnd = "Play end of selection",
                WaveformPlayFirstSelectedSubtitle = "Play first selected subtitle",
                WaveformGoToPrevSubtitle = "Go to previous subtitle (from video position)",
                WaveformGoToNextSubtitle = "Go to next subtitle (from video position)",
                WaveformGoToPrevTimeCode = "Go to previous time code (from video position)",
                WaveformGoToNextTimeCode = "Go to next time code (from video position)",
                WaveformGoToPrevChapter = "Go to previous chapter",
                WaveformGoToNextChapter = "Go to next chapter",
                WaveformSelectNextSubtitle = "Select next subtitle (from video position, keep video pos)",
                WaveformGoToPreviousShotChange = "Go to previous shot change",
                WaveformGoToNextShotChange = "Go to next shot change",
                WaveformToggleShotChange = "Toggle shot change",
                WaveformAllShotChangesOneFrameForward = "Move all shot changes one frame forward",
                WaveformAllShotChangesOneFrameBack = "Move all shot changes one frame back",
                WaveformRemoveOrExportShotChanges = "Remove/export shot changes",
                WaveformGuessStart = "Auto adjust start via volume/shot change",
                GoBack1Frame = "One frame back",
                GoForward1Frame = "One frame forward",
                GoBack1FrameWithPlay = "One frame back (with play)",
                GoForward1FrameWithPlay = "One frame forward (with play)",
                GoBack100Milliseconds = "100 ms back",
                GoForward100Milliseconds = "100 ms forward",
                GoBack500Milliseconds = "500 ms back",
                GoForward500Milliseconds = "500 ms forward",
                GoBack1Second = "One second back",
                GoForward1Second = "One second forward",
                GoForward3Seconds = "Three seconds forward",
                GoBack3Seconds = "Three seconds back",
                GoBack5Seconds = "Five seconds back",
                GoForward5Seconds = "Five seconds forward",
                GoBackXSSeconds = "Small selected time back",
                GoForwardXSSeconds = "Small selected time forward",
                GoBackXLSeconds = "Large selected time back",
                GoForwardXLSeconds = "Large selected time forward",
                GoToStartCurrent = "Set video pos to start of current subtitle",
                ToggleStartEndCurrent = "Toggle video pos between start/end of current subtitle",
                PlaySelectedLines = "Play selected lines",
                LoopSelectedLines = "Loop selected lines",
                Pause = "Pause",
                TogglePlayPause = "Toggle play/pause",
                Fullscreen = "Fullscreen",
                Play150Speed = "Play rate 1.5x speed",
                Play200Speed = "Play rate 2.0x speed",
                PlayRateSlower = "Play rate slower (speed)",
                PlayRateFaster = "Play rate faster (speed)",
                PlayRateToggle = "Play rate (speed) toggle (0.5x, 1x, 1.5x, 2x)",
                VideoResetSpeedAndZoom = "Reset play rate (speed) and waveform zoom",
                MainToggleVideoControls = "Toggle video controls",
                VideoToggleContrast = "Toggle contrast (mpv only)",
                AudioToTextX = "Audio to text ({0})",
                AudioToTextSelectedLinesX = "Audio to text selected lines ({0})",
                AudioExtractSelectedLines = "Extract audio (selected lines)",
                VideoToggleBrightness = "Toggle brightness (mpv only)",
                AutoTranslateSelectedLines = "Auto-translate selected  lines",
                CustomSearch1 = "Translate, custom search 1",
                CustomSearch2 = "Translate, custom search 2",
                CustomSearch3 = "Translate, custom search 3",
                CustomSearch4 = "Translate, custom search 4",
                CustomSearch5 = "Translate, custom search 5",
                SyntaxColoring = "Syntax coloring",
                ListViewSyntaxColoring = "List view syntax coloring",
                SyntaxColorDurationIfTooSmall = "Color duration if too short",
                SyntaxColorDurationIfTooLarge = "Color duration if too long",
                SyntaxColorTextIfTooLong = "Color text if too long",
                SyntaxColorTextIfTooWide = "Color text if too wide (pixels)",
                SyntaxColorTextMoreThanMaxLines = "Color text if more than {0} lines",
                SyntaxColorOverlap = "Color time code overlap",
                SyntaxColorGap = "Color gap if too short",
                SyntaxErrorColor = "Error color",
                SyntaxLineWidthSettings = "Settings...",
                LineWidthSettings = "Line width settings",
                MaximumLineWidth = "Maximum line width:",
                Pixels = "pixels",
                MeasureFont = "Measuring font:",
                GoToFirstSelectedLine = "Go to first selected line",
                GoToNextEmptyLine = "Go to next empty line",
                MergeSelectedLines = "Merge selected lines",
                MergeSelectedLinesAndAutoBreak = "Merge selected lines and auto-break",
                MergeSelectedLinesAndUnbreak = "Merge selected lines and unbreak",
                MergeSelectedLinesAndUnbreakCjk = "Merge selected lines and unbreak without space (CJK)",
                MergeSelectedLinesOnlyFirstText = "Merge selected lines, keep only first non-empty text",
                MergeSelectedLinesBilingual = "Merge selected lines bilingual",
                MergeWithPreviousBilingual = "Merge with previous bilingual",
                MergeWithNextBilingual = "Merge with next bilingual",
                SplitSelectedLineBilingual = "Split selected line bilingual",
                ToggleTranslationMode = "Toggle translator mode",
                SwitchOriginalAndTranslation = "Switch original and translation",
                SwitchOriginalAndTranslationTextBoxes = "Switch original and translation text boxes/list view columns",
                MergeOriginalAndTranslation = "Merge original and translation",
                MergeWithNext = "Merge with next",
                MergeWithPrevious = "Merge with previous",
                MergeWithNextAndUnbreak = "Merge with next and unbreak",
                MergeWithPreviousAndUnbreak = "Merge with previous and unbreak",
                MergeWithNextAndBreak = "Merge with next and auto-break",
                MergeWithPreviousAndBreak = "Merge with previous and auto-break",
                ShortcutIsAlreadyDefinedX = "Shortcut already defined: {0}",
                ToggleTranslationAndOriginalInPreviews = "Toggle translation and original in video/audio preview",
                ListViewColumnDelete = "Column, delete text",
                ListViewColumnDeleteAndShiftUp = "Column, delete text and shift up",
                ListViewColumnInsert = "Column, insert text",
                ListViewColumnPaste = "Column, paste",
                ListViewColumnTextUp = "Column, text up",
                ListViewColumnTextDown = "Column, text down",
                ListViewGoToNextError = "Go to next error",
                ListViewListErrors = "List errors",
                ListViewListSortByX = "Sort by {0}",
                ShowStyleManager = "Show style manager",
                MainTextBoxMoveLastWordDown = "Move last word to next subtitle",
                MainTextBoxMoveFirstWordFromNextUp = "Fetch first word from next subtitle",
                MainTextBoxMoveLastWordDownCurrent = "Move last word from first line down (current subtitle)",
                MainTextBoxMoveFirstWordUpCurrent = "Move first word from next line up (current subtitle)",
                MainTextBoxMoveFromCursorToNext = "Move text after cursor position to next subtitle and go to next",
                MainTextBoxSelectionToLower = "Selection to lowercase",
                MainTextBoxSelectionToUpper = "Selection to uppercase",
                MainTextBoxSelectionToggleCasing = "Toggle casing of selection (propercase/uppercase/lowercase)",
                MainTextBoxSelectionToRuby = "Selection to Ruby (Japanese)",
                MainTextBoxToggleAutoDuration = "Toggle auto duration",
                MainTextBoxAutoBreak = "Auto break text",
                MainTextBoxAutoBreakFromPos = "Break at first space from cursor position",
                MainTextBoxAutoBreakFromPosAndGoToNext = "Break at first space from cursor position and go to next",
                MainTextBoxDictate = "Dictate (key down=start recording, key up=end recording)",
                MainTextBoxUnbreak = "Unbreak text",
                MainTextBoxUnbreakNoSpace = "Unbreak without space (CJK)",
                MainTextBoxAssaIntellisense = "Show ASSA tag helper",
                MainTextBoxAssaRemoveTag = "Remove ASSA tag at cursor",
                MainFileSaveAll = "Save all",
                Miscellaneous = "Misc.",
                UseDoNotBreakAfterList = "Use do-not-break-after list",
                BreakEarlyForComma = "Break early for comma",
                BreakEarlyForDashDialog = "Break early for dialog dash",
                BreakEarlyForLineEnding = "Break early for end of sentence (.!?)",
                BreakByPixelWidth = "Break by pixel width",
                BreakPreferBottomHeavy = "Prefer bottom heavy",
                GoogleTranslate = "Google Translate",
                GoogleTranslateApiKey = "API key",
                CpsIncludesSpace = "Chars/sec (CPS) includes spaces",
                MicrosoftBingTranslator = "Microsoft Translator",
                HowToSignUp = "How to sign up",
                MicrosoftTranslateApiKey = "Key",
                MicrosoftTranslateTokenEndpoint = "Token endpoint",
                FontNote = "Note: These font settings are for the Subtitle Edit UI only." + Environment.NewLine +
                           "Setting a font for a subtitle is normally done in the video player, but can also be done when using a subtitle format with built-in font information like " + Environment.NewLine +
                           "\"Advanced Sub Station Alpha\" or via export to image-based formats.",
                RestoreDefaultSettings = "Restore default settings",
                RestoreDefaultSettingsMsg = "All settings will be restored to default values" + Environment.NewLine +
                                            Environment.NewLine +
                                            "Continue?",
                RemoveTimeCodes = "Remove time codes",
                EditFixContinuationStyleSettings = "Edit settings for fixing continuation style...",
                FixContinuationStyleSettings = "Settings for fixing continuation style",
                UncheckInsertsAllCaps = "Detect and uncheck single titles in all-caps (for example: NO ENTRY)",
                UncheckInsertsItalic = "Detect and uncheck italic single titles, or lyrics",
                UncheckInsertsLowercase = "Detect and uncheck single titles, or lyrics, in lowercase",
                HideContinuationCandidatesWithoutName = "Hide unlikely continuation sentences",
                IgnoreLyrics = "Ignore lyrics between music symbols",
                ContinuationPause = "Pause threshold:",
                Milliseconds = "ms",
                EditCustomContinuationStyle = "Edit custom continuation style...",
                MinFrameGap = "Min. gap in frames",
                XFramesAtYFrameRateGivesZMs = "{0} frames at a frame rate of {1} gives {2} milliseconds.",
                UseXAsNewGap = "Use \"{0}\" milliseconds as new minimum gap?",
                BDOpensIn = "BD sup/bdn-xml opens in",
                BDOpensInOcr = "OCR",
                BDOpensInEdit = "Edit",
                ShortcutsAllowSingleLetterOrNumberInTextBox = "Shortcuts: Allow single letter/number in text box",
                ShortcutCustomToggle = "Shortcut toggle custom start/end",
                UpdateFileTypeAssociations = "Update file type associations",
                FileTypeAssociationsUpdated = "File type associations updated",
                CustomContinuationStyle = "Edit custom continuation style",
                LoadStyle = "Load style...",
                Suffix = "Suffix:",
                AddSuffixForComma = "Process if ends with comma",
                AddSpace = "Add space",
                RemoveComma = "Remove comma",
                Prefix = "Prefix:",
                DifferentStyleGap = "Use different style for gaps longer than",
                Preview = "Preview",
                PreviewPause = "(pause)",
                CustomContinuationStyleNote = "Note: The custom continuation style is shared across profiles.",
                ResetCustomContinuationStyleWarning = "This will override the values in the dialog. Are you sure?",
                ExportAsHtml = "Export as HTML...",
                SetNewActor = "Set new actor/voice",
                SetActorX = "Set actor/voice {0}",
                Used = "Used",
                Unused = "Unused",
            };

            SettingsMpv = new LanguageStructure.SettingsMpv
            {
                DownloadMpv = "Download mpv lib",
                DownloadMpvFailed = "Unable to download mpv - please re-try later!",
                DownloadMpvOk = "The mpv lib was downloaded and is ready for use.",
            };


            SettingsFfmpeg = new LanguageStructure.SettingsFfmpeg
            {
                XDownloadFailed = "Unable to download {0} - please re-try later!",
                XDownloadOk = "{0} was downloaded and is ready for use.",
            };

            SetVideoOffset = new LanguageStructure.SetVideoOffset
            {
                Title = "Set video offset",
                Description = "Set video offset (subtitles should not follow real video time, but e.g. +10 hours)",
                RelativeToCurrentVideoPosition = "Relative to current video position",
                KeepTimeCodes = "Keep existing time codes (do not add video offset)",
                Reset = "Reset",
            };

            ShowEarlierLater = new LanguageStructure.ShowEarlierLater
            {
                Title = "Show selected lines earlier/later",
                TitleAll = "Show all lines earlier/later",
                ShowEarlier = "Show &earlier",
                ShowLater = "Show &later",
                TotalAdjustmentX = "Total adjustment: {0}",
                AllLines = "&All lines",
                SelectedLinesOnly = "&Selected lines only",
                SelectedLinesAndForward = "Selected and subse&quent lines",
            };

            ShowHistory = new LanguageStructure.ShowHistory
            {
                Title = "History (for undo)",
                SelectRollbackPoint = "Select time/description for rollback",
                Time = "Time",
                Description = "Description",
                CompareHistoryItems = "Compare history items",
                CompareWithCurrent = "Compare with current",
                Rollback = "Rollback",
            };

            SpellCheck = new LanguageStructure.SpellCheck
            {
                Title = "Spell check",
                FullText = "Full text",
                WordNotFound = "Word not found",
                Language = "Language",
                Change = "Change",
                ChangeAll = "Change all",
                SkipOnce = "Skip &one",
                SkipAll = "&Skip all",
                AddToUserDictionary = "Add to user dictionary",
                AddToNamesAndIgnoreList = "Add to names/noise list (case sensitive)",
                AddToOcrReplaceList = "Add pair to OCR replace list",
                Abort = "Abort",
                Use = "Use",
                UseAlways = "&Use always",
                Suggestions = "Suggestions",
                SpellCheckProgress = "Spell check [{0}] - {1}",
                EditWholeText = "Edit whole text",
                EditWordOnly = "Edit word only",
                AddXToNames = "Add '{0}' to name list",
                AddXToUserDictionary = "Add '{0}' to user dictionary",
                AutoFixNames = "Auto fix names where only casing differs",
                AutoFixNamesViaSuggestions = "Also fix names via 'spell check suggestions'",
                CheckOneLetterWords = "Prompt for unknown one letter words",
                TreatINQuoteAsING = "Treat word ending \" in' \" as \" ing \" (English only)",
                RememberUseAlwaysList = "Remember \"Use always\" list",
                LiveSpellCheck = "Live spell check",
                LiveSpellCheckLanguage = "Live spell check - Working with language [{0}]",
                NoDictionaryForLiveSpellCheck = "Live spell check - You don't have dictionaries for this language [{0}]",
                ImageText = "Image text",
                SpellCheckCompleted = "Spell check completed",
                SpellCheckAborted = "Spell check aborted",
                SpacesNotAllowed = "Spaces not allowed in single word!",
                UndoX = "Undo: \"{0}\"",
                OpenImageBasedSourceFile = "Open image-based source file...",
            };

            NetflixQualityCheck = new LanguageStructure.NetflixQualityCheck
            {
                GlyphCheckReport = "Invalid character {0} found at column {1}",
                WhiteSpaceCheckReport = "Invalid white space found at column {0}.",
                ReportPrompt = "Please see full report here: {0}.",
                OpenReportInFolder = "Open report in folder",
                FoundXIssues = "Netflix quality check found {0:#,##0} issues.",
                MaximumXCharsPerSecond = "Maximum {0} characters per second (incl. white spaces)",
                MaximumLineLength = "Maximum line length ({0})",
                MinimumDuration = "Minimum duration: 5/6 second (833 ms)",
                CheckOk = "Netflix quality check OK :)",
            };

            Split = new LanguageStructure.Split
            {
                Title = "Split",
                SplitOptions = "Split options",
                Lines = "Lines",
                Characters = "Characters",
                NumberOfEqualParts = "Number of equal parts",
                SubtitleInfo = "Subtitle info",
                NumberOfLinesX = "Number of lines: {0:#,###}",
                NumberOfCharactersX = "Number of characters: {0:#,###,###}",
                Output = "Output",
                FileName = "File name",
                OutputFolder = "Output folder",
                DoSplit = "Split",
                Basic = "Basic",
            };

            SplitLongLines = new LanguageStructure.SplitLongLines
            {
                Title = "Break/split long lines",
                SingleLineMaximumLength = "Single line maximum length",
                LineMaximumLength = "Line maximum length",
                LineContinuationBeginEndStrings = "Line continuation begin/end strings",
                NumberOfSplits = "Number of breaks/splits: {0}",
                LongestSingleLineIsXAtY = "Longest single line length is {0} at line {1}",
                LongestLineIsXAtY = "Longest total line length is {0} at line {1}",
                SplitAtLineBreaks = "Split at line breaks",
            };

            SplitSubtitle = new LanguageStructure.SplitSubtitle
            {
                Title = "Split subtitle",
                Description1 = "Enter length of first part of video or browse",
                Description2 = "and get length from video file:",
                Part1 = "Part1",
                Part2 = "Part2",
                Done = "&Done",
                Split = "&Split",
                SavePartOneAs = "Save part 1 as...",
                SavePartTwoAs = "Save part 2 as...",
                NothingToSplit = "Nothing to split!",
                UnableToSaveFileX = "Unable to save {0}",
                OverwriteExistingFiles = "Overwrite existing files?",
                FolderNotFoundX = "Folder not found: {0}",
                Untitled = "Untitled",
            };

            StartNumberingFrom = new LanguageStructure.StartNumberingFrom
            {
                Title = "Renumber",
                StartFromNumber = "Start from number:",
                PleaseEnterAValidNumber = "Oops, please enter a valid number",
            };

            Statistics = new LanguageStructure.Statistics
            {
                Title = "Statistics",
                TitleWithFileName = "Statistics - {0}",
                GeneralStatistics = "General statistics",
                NothingFound = "Nothing found",
                MostUsed = "Most used...",
                MostUsedWords = "Most used words",
                MostUsedLines = "Most used lines",
                NumberOfLinesX = "Number of subtitle lines: {0:#,##0}",
                LengthInFormatXinCharactersY = "Number of characters as {0}: {1:#,###,##0}",
                NumberOfCharactersInTextOnly = "Number of characters in text only: {0:#,###,##0}",
                NumberOfItalicTags = "Number of italic tags: {0:#,##0}",
                TotalDuration = "Total duration of all subtitles: {0:#,##0}",
                TotalCharsPerSecond = "Total characters/second: {0:0.0} seconds",
                TotalWords = "Total words in subtitle: {0:#,##0}",
                NumberOfBoldTags = "Number of bold tags: {0:#,##0}",
                NumberOfUnderlineTags = "Number of underline tags: {0:#,##0}",
                NumberOfFontTags = "Number of font tags: {0:#,##0}",
                NumberOfAlignmentTags = "Number of alignment tags: {0:#,##0}",
                LineLengthMinimum = "Subtitle length - minimum: {0}",
                LineLengthMaximum = "Subtitle length - maximum: {0}",
                LineLengthAverage = "Subtitle length - average: {0:#.###}",
                LinesPerSubtitleAverage = "Subtitle, number of lines - average: {0:0.###}",
                SingleLineLengthMinimum = "Single line length - minimum: {0}",
                SingleLineLengthMaximum = "Single line length - maximum: {0}",
                SingleLineLengthAverage = "Single line length - average: {0:#.###}",
                SingleLineLengthExceedingMaximum = "Single line length - exceeding maximum ({0} chars): {1} ({2:0.00}%)",
                SingleLineWidthMinimum = "Single line width - minimum: {0} pixels",
                SingleLineWidthMaximum = "Single line width - maximum: {0} pixels",
                SingleLineWidthAverage = "Single line width - average: {0:#.###} pixels",
                SingleLineWidthExceedingMaximum = "Single line width - exceeding maximum ({0} pixels): {1} ({2:0.00}%)",
                DurationMinimum = "Duration - minimum: {0:0.000} seconds",
                DurationMaximum = "Duration - maximum: {0:0.000} seconds",
                DurationAverage = "Duration - average: {0:0.000} seconds",
                DurationExceedingMinimum = "Duration - below minimum ({0:0.###} sec): {1} ({2:0.00}%)",
                DurationExceedingMaximum = "Duration - exceeding maximum ({0:0.###} sec): {1} ({2:0.00}%)",
                CharactersPerSecondMinimum = "Characters/sec - minimum: {0:0.000}",
                CharactersPerSecondMaximum = "Characters/sec - maximum: {0:0.000}",
                CharactersPerSecondAverage = "Characters/sec - average: {0:0.000}",
                CharactersPerSecondExceedingOptimal = "Characters/sec - exceeding optimal ({0:0.##} cps): {1} ({2:0.00}%)",
                CharactersPerSecondExceedingMaximum = "Characters/sec - exceeding maximum ({0:0.##} cps): {1} ({2:0.00}%)",
                WordsPerMinuteMinimum = "Words/min - minimum: {0:0.000}",
                WordsPerMinuteMaximum = "Words/min - maximum: {0:0.000}",
                WordsPerMinuteAverage = "Words/min - average: {0:0.000}",
                WordsPerMinuteExceedingMaximum = "Words/min - exceeding maximum ({0} wpm): {1} ({2:0.00}%)",
                GapMinimum = "Gap - minimum: {0:#,##0} ms",
                GapMaximum = "Gap - maximum: {0:#,##0} ms",
                GapAverage = "Gap - average: {0:#,##0.##} ms",
                GapExceedingMinimum = "Gap - below minimum ({0:#,##0} ms): {1} ({2:0.00}%)",
                Export = "Export...",
            };

            SubStationAlphaProperties = new LanguageStructure.SubStationAlphaProperties
            {
                Title = "Advanced Sub Station Alpha properties",
                TitleSubstationAlpha = "Sub Station Alpha properties",
                Script = "Script",
                ScriptTitle = "Title",
                OriginalScript = "Original script",
                Translation = "Translation",
                Editing = "Editing",
                Timing = "Timing",
                SyncPoint = "Sync point",
                UpdatedBy = "Updated by",
                UpdateDetails = "Update details",
                Resolution = "Resolution",
                VideoResolution = "Video resolution",
                FromCurrentVideo = "From current video",
                Options = "Options",
                WrapStyle = "Wrap style",
                Collision = "Collision",
                ScaleBorderAndShadow = "Scale border and shadow",
                WrapStyle0 = "0: Smart wrapping, top line is wider",
                WrapStyle1 = "1: End-of-line word wrapping, only \\N breaks",
                WrapStyle2 = "2: No word wrapping, both \\n and \\N break",
                WrapStyle3 = "3: Smart wrapping, bottom line is wider",
            };

            SubStationAlphaStyles = new LanguageStructure.SubStationAlphaStyles
            {
                Title = "Advanced Sub Station Alpha styles",
                TitleSubstationAlpha = "Sub Station Alpha styles",
                Styles = "Styles",
                Properties = "Properties",
                Name = "Name",
                Font = "Font",
                FontName = "Font name",
                FontSize = "Font size",
                UseCount = "Used",
                Primary = "Primary",
                Secondary = "Secondary",
                Tertiary = "Tertiary",
                Outline = "Outline",
                Shadow = "Shadow",
                Back = "Back",
                Alignment = "Alignment",
                TopLeft = "Top/left",
                TopCenter = "Top/center",
                TopRight = "Top/right",
                MiddleLeft = "Middle/left",
                MiddleCenter = "Middle/center",
                MiddleRight = "Middle/right",
                BottomLeft = "Bottom/left",
                BottomCenter = "Bottom/center",
                BottomRight = "Bottom/right",
                Colors = "Colors",
                Margins = "Margins",
                MarginLeft = "Margin left",
                MarginRight = "Margin right",
                MarginVertical = "Margin vertical",
                Vertical = "Vertical",
                Border = "Border",
                PlusShadow = "+ Shadow",
                OpaqueBox = "Opaque box",
                Import = "Import...",
                Export = "Export...",
                New = "New",
                Copy = "Copy",
                CopyOfY = "Copy of {0}",
                CopyXOfY = "Copy {0} of {1}",
                Remove = "Remove",
                ReplaceWith = "Replace with...",
                RemoveAll = "Remove all",
                ImportStyleFromFile = "Import style from file...",
                ExportStyleToFile = "Export style to file... (will add style if file already exists)",
                ChooseStyle = "Choose style to import",
                StyleAlreadyExits = "Style already exists: {0}",
                StyleXExportedToFileY = "Style '{0}' exported to file '{1}'",
                StyleXImportedFromFileY = "Style '{0}' imported from file '{1}'",
                SetPreviewText = "Set preview text...",
                AddToFile = "Add to file",
                AddToStorage = "Add to storage",
                StyleStorage = "Style storage",
                StyleCurrentFile = "Styles in current file",
                OverwriteX = "Overwrite {0}?",
                CategoryNote = "Note: The styles in the default category (colored in green) will be applied to new ASSA files",
                CategoriesManage = "Manage",
                MoveToCategory = "Move selected styles to category...",
                ScaleX = "ScaleX",
                ScaleY = "ScaleY",
                Spacing = "Spacing",
                Angle = "Angle",
                BoxPerLine = "Box per line (use outline color)",
                BoxMultiLine = "One box (use shadow color)",
                BoxPerLineShort = "Box per line",
                BoxMultiLineShort = "One box",
                BoxType = "Box type",
                DuplicateStyleNames = "Duplicate style names: {0}",
            };

            SubStationAlphaStylesCategoriesManager = new LanguageStructure.SubStationAlphaStylesCategoriesManager
            {
                Category = "Category",
                Categories = "Categories",
                CategoryName = "Category name",
                CategoryDelete = "Are you sure you want to delete the selected category/categories?",
                NewCategory = "New category",
                CategoryRename = "Rename category",
                CategorySetDefault = "Set as default",
                NumberOfStyles = "Number of styles",
                CategoryDefault = "Default",
                ChooseCategories = "Choose categories to {0}",
                ImportCategoriesTitle = "Import categories from...",
                ExportCategoriesTitle = "Export categories to...",
            };

            PointSync = new LanguageStructure.PointSync
            {
                Title = "Point synchronization",
                TitleViaOtherSubtitle = "Point sync via another subtitle",
                SyncHelp = "Set at least two sync points to make rough synchronization",
                SetSyncPoint = "Set sync point",
                RemoveSyncPoint = "Remove sync point",
                SyncPointsX = "Sync points: {0}",
                Info = "One sync point will adjust position, two or more sync points will adjust position and speed",
                ApplySync = "Apply",
            };

            TextToSpeech = new LanguageStructure.TextToSpeech
            {
                Title = "Text to speech",
                Voice = "Voice",
                DefaultVoice = "Default voice",
                TestVoice = "Test voice",
                ActorInfo = "Right-click to assign actor to voice",
                AddAudioToVideo = "Add audio to video file (new file)",
                GenerateSpeech = "Generate speech from text",
                AdjustingSpeedXOfY = "Adjusting speed: {0} / {1}...",
                MergingAudioTrackXOfY = "Merging audio track: {0} / {1}...",
                GeneratingSpeechFromTextXOfY = "Generating speech from text: {0} / {1}...",
                ReviewAudioClips = "Review audio clips",
                ReviewInfo = "Review and edit/remove audio clips",
                CustomAudioEncoding = "Custom audio encoding",
                AutoContinue = "Auto-continue",
                Play = "Play",
                Regenerate = "Regenerate",
                Speed = "Speed",
            };

            TimedTextSmpteTiming = new LanguageStructure.TimedTextSmpteTiming
            {
                Title = "SMPTE timing",
                UseSmpteTiming = "Use SMPTE timing for current subtitle?",
                SmpteTimingInfo = "Note: SMPTE timing can be changed later in the \"Video menu\"",
                NoNever = "No, never",
                YesAlways = "Yes, always for non-whole-number frame rates",
            };

            TransportStreamSubtitleChooser = new LanguageStructure.TransportStreamSubtitleChooser
            {
                Title = "Transport stream subtitle chooser - {0}",
                PidLineImage = "Images - Transport Packet Identifier (PID) = {0}, language = {1}, number of subtitles = {2}",
                PidLineTeletext = "Teletext - Transport Packet Identifier (PID) = {1}, page {0}, language = {2}, number of subtitles = {3}",
                SubLine = "{0}: {1} -> {2}, {3} image(s)",
            };

            UnknownSubtitle = new LanguageStructure.UnknownSubtitle
            {
                Title = "Unknown subtitle type",
                Message = "If you want this fixed, please send an email to mailto:niksedk@gmail.com and include a copy of the subtitle.",
                ImportAsPlainText = "Import as plain text...",
            };

            VerifyCompleteness = new LanguageStructure.VerifyCompleteness
            {
                Title = "Verify completeness against other subtitle",
                OpenControlSubtitle = "Open control subtitle",
                ControlSubtitleError = "Control subtitle is empty or could not be loaded.",
                ControlSubtitleX = "Control subtitle: {0}",
                Coverage = "Coverage",
                CoveragePercentageX = "{0:0.##}%",
                SortByCoverage = "Sort by coverage",
                SortByTime = "Sort by time",
                Reload = "Re-verify",
                Insert = "Insert",
                InsertAndNext = "Insert and go to next",
                Dismiss = "Dismiss",
                DismissAndNext = "Dismiss and go to next",
            };

            VisualSync = new LanguageStructure.VisualSync
            {
                Title = "Visual sync",
                StartScene = "Start scene",
                EndScene = "End scene",
                Synchronize = "Sync",
                HalfASecondBack = "< ½ s",
                ThreeSecondsBack = "< 3 s",
                PlayXSecondsAndBack = "Play {0} s and back",
                FindText = "Find text",
                GoToSubPosition = "Go to sub pos",
                KeepChangesTitle = "Keep changes?",
                KeepChangesMessage = @"Changes have been made to subtitle in 'Visual sync'.

Keep changes?",
                SynchronizationDone = "Sync done!",
                StartSceneMustComeBeforeEndScene = "Start scene must come before end scene!",
                Tip = "Tip: Use <ctrl+arrow left/right> keys to move 100 ms back/forward",
            };

            VobSubEditCharacters = new LanguageStructure.VobSubEditCharacters
            {
                Title = "Edit image compare database",
                ChooseCharacter = "Choose character(s)",
                ImageCompareFiles = "Image compare files",
                CurrentCompareImage = "Current compare image",
                TextAssociatedWithImage = "Text associated with image",
                IsItalic = "Is &italic",
                Update = "&Update",
                Delete = "&Delete",
                ImageDoubleSize = "Image double size",
                ImageFileNotFound = "Image file not found",
                Image = "Image",
            };

            VobSubOcr = new LanguageStructure.VobSubOcr
            {
                Title = "Import/OCR VobSub (sub/idx) subtitle",
                TitleBluRay = "Import/OCR Blu-ray (.sup) subtitle",
                OcrMethod = "OCR method",
                OcrViaTesseractVersionX = "Tesseract {0}",
                OcrViaImageCompare = "Binary image compare",
                OcrViaModi = "Microsoft Office Document Imaging (MODI). Requires Microsoft Office",
                OcrViaNOCR = "OCR via nOCR",
                OcrViaCloudVision = "OCR via Google Cloud Vision API",
                TesseractEngineMode = "Engine mode",
                TesseractEngineModeLegacy = "Original Tesseract only (can detect italic)",
                TesseractEngineModeNeural = "Neural nets LSTM only",
                TesseractEngineModeBoth = "Tesseract + LSTM",
                TesseractEngineModeDefault = "Default, based on what is available",
                Language = "Language",
                ImageDatabase = "Image database",
                NoOfPixelsIsSpace = "No of pixels is space",
                MaxErrorPercent = "Max. error%",
                New = "New",
                Edit = "Edit",
                StartOcr = "Start OCR",
                StartOcrFrom = "Start OCR from subtitle no:",
                LoadingVobSubImages = "Loading VobSub images...",
                LoadingImageCompareDatabase = "Loading image compare database...",
                ConvertingImageCompareDatabase = "Converting image compare database to new format (images.db/images.xml)...",
                SubtitleImage = "Subtitle image",
                SubtitleText = "Subtitle text",
                UnableToCreateCharacterDatabaseFolder = "Unable to create 'Character database folder': {0}",
                SubtitleImageXofY = "Subtitle image {0} of {1}",
                ImagePalette = "Image palette",
                UseCustomColors = "Use custom colors",
                Transparent = "Transparent",
                TransparentMinAlpha = "Min. alpha value (0=transparent, 255=fully visible)",
                TransportStream = "Transport stream",
                TransportStreamGrayscale = "Grayscale",
                TransportStreamGetColor = "Use color (splitting of lines may occur)",
                PromptForUnknownWords = "Prompt for unknown words",
                TryToGuessUnknownWords = "Try to guess unknown words",
                AutoBreakSubtitleIfMoreThanTwoLines = "Auto break paragraph if more than two lines",
                AllFixes = "All fixes",
                GuessesUsed = "Guesses used",
                UnknownWords = "Unknown words",
                UnknownWordToGuessInLine = "{0} ⇒ {1} via 'OCRFixReplaceList/WordSplitList' in line: {2}",
                OcrAutoCorrectionSpellChecking = "OCR auto correction / spell checking",
                FixOcrErrors = "Fix OCR errors",
                ImportTextWithMatchingTimeCodes = "Import text with matching time codes...",
                ImportNewTimeCodes = "Import new time codes",
                SaveSubtitleImageAs = "Save subtitle image as...",
                SaveAllSubtitleImagesAsBdnXml = "Save all images (png/bdn xml)...",
                SaveAllSubtitleImagesWithHtml = "Save all images with HTML index...",
                XImagesSavedInY = "{0} images saved in {1}",
                DictionaryX = "Dictionary: {0}",
                RightToLeft = "Right to left",
                ShowOnlyForcedSubtitles = "Show only forced subtitles",
                UseTimeCodesFromIdx = "Use time codes from .idx file",
                NoMatch = "<No match>",
                AutoTransparentBackground = "Auto transparent background",
                CaptureTopAlign = "Capture top align",
                InspectCompareMatchesForCurrentImage = "Inspect compare matches for current image...",
                EditLastAdditions = "Edit last image compare additions...",
                SetItalicAngle = "Set italic angle...",
                ItalicAngle = "Italic angle",
                DiscardTitle = "Discard changes made in OCR?",
                DiscardText = "Do you want to discard changes made in current OCR session?",
                MinLineSplitHeight = "Min. line height (split)",
                FallbackToX = "Fallback to {0}",
                ImagePreProcessing = "Image pre-processing...",
                EditImageDb = "Edit image db",
                OcrTraining = "OCR training...",
                SubtitleTrainingFile = "Subtitle file for training",
                LetterCombinations = "Letter combinations that might be split as one image",
                TrainingOptions = "Training options",
                NumberOfSegments = "Number of segments per letter",
                AlsoTrainItalic = "Also train italic",
                AlsoTrainBold = "Also train bold",
                StartTraining = "Start training",
                NowTraining = "Now training font '{1}'. Total chars trained: {0:#,###,##0}, {2:#,###,##0} known",
                ImagesWithTimeCodesInFileName = "Images with time codes in file name...",
                CloudVisionApi = "Google Cloud Vision API",
                ApiKey = "API key",
                SendOriginalImages = "Send original images",
                SeHandlesTextMerge = "SE handles text merge",
            };

            VobSubOcrCharacter = new LanguageStructure.VobSubOcrCharacter
            {
                Title = "OCR - Manual image to text",
                Abort = "&Abort",
                Skip = "&Skip",
                UseOnce = "&Use once",
                SubtitleImage = "Subtitle image",
                ShrinkSelection = "Shrink selection",
                ExpandSelection = "Expand selection",
                Characters = "Character(s)",
                CharactersAsText = "Character(s) as text",
                Italic = "&Italic",
                Nordic = "Nordic",
                Spanish = "Spanish",
                German = "German",
                AutoSubmitOnFirstChar = "Auto submit on &first char",
                EditLastX = "Edit last: {0}",
            };

            VobSubOcrCharacterInspect = new LanguageStructure.VobSubOcrCharacterInspect
            {
                Title = "Inspect compare matches for current image",
                InspectItems = "Inspect items",
                AddBetterMatch = "Add better match",
                Add = "Add",
                AddBetterMultiMatch = "Add better multi match",
                AddOrUpdateMatch = "Add or update match",
                SelectPrevousMatch = "Select the previous match",
                SelectNextMatch = "Select the next match",
                JumpPreviousMissingMatch = "Jump to the previous missing match",
                JumpNextMissingMatch = "Jump to the next missing match",
                FocusTextbox = "Focus the text input",
            };

            VobSubOcrNewFolder = new LanguageStructure.VobSubOcrNewFolder
            {
                Title = "New folder",
                Message = "Name of new character database folder",
            };

            VobSubOcrSetItalicAngle = new LanguageStructure.VobSubOcrSetItalicAngle
            {
                Title = "Set italic angle",
                Description = "Adjust value until text style is normal and not italic. Note that original image should be italic.",
            };

            OcrPreprocessing = new LanguageStructure.OcrPreprocessing
            {
                Title = "OCR image preprocessing",
                Colors = "Colors",
                AdjustAlpha = "Adjust value until text is shown clearly (normally values between 200 and 300)",
                BinaryThreshold = "Binary image compare threshold",
                ColorToRemove = "Color to remove",
                ColorToWhite = "Color to white",
                CropTransparentColors = "Crop transparent colors",
                Cropping = "Cropping",
                InvertColors = "Invert colors",
                OriginalImage = "Original image",
                PostImage = "Image after preprocessing",
                YellowToWhite = "Yellow to white",
            };

            Watermark = new LanguageStructure.Watermark
            {
                Title = "Watermark",
                WatermarkX = "Watermark: {0}",
                GenerateWatermarkTitle = "Generate watermark",
                SpreadOverEntireSubtitle = "Spread over entire subtitle",
                CurrentLineOnlyX = "Only on current line: {0}",
                Generate = "Generate",
                Remove = "Remove",
                BeforeWatermark = "Before Watermark",
                ErrorUnicodeEncodingOnly = "Watermark only works with Unicode file encoding.",
            };

            Waveform = new LanguageStructure.Waveform
            {
                AddWaveformAndSpectrogram = "Add waveform/spectrogram",
                ClickToAddWaveform = "Click to add waveform",
                ClickToAddWaveformAndSpectrogram = "Click to add waveform/spectrogram",
                Seconds = "seconds",
                ZoomIn = "Zoom in",
                ZoomOut = "Zoom out",
                AddParagraphHere = "Add text here (Enter)",
                AddParagraphHereAndPasteText = "Add text from clipboard here",
                SetParagraphAsSelection = "Set current as new selection",
                FocusTextBox = "Focus text box",
                GoToPrevious = "Go to previous subtitle",
                GoToNext = "Go to next subtitle",
                DeleteParagraph = "Delete text",
                Split = "Split",
                SplitAtCursor = "Split at cursor",
                MergeWithPrevious = "Merge with previous",
                MergeWithNext = "Merge with next",
                ExtendToPrevious = "Extend to previous",
                ExtendToNext = "Extend to next",
                RunWhisperSelectedParagraph = "Run Whisper on selected paragraph...",
                PlaySelection = "Play selection",
                ShowWaveformAndSpectrogram = "Show waveform and spectrogram",
                ShowWaveformOnly = "Show waveform only",
                ShowSpectrogramOnly = "Show spectrogram only",
                AddShotChange = "Add shot change",
                RemoveShotChange = "Remove shot change",
                RemoveShotChangesFromSelection = "Remove shot changes from selection",
                SeekSilence = "Seek silence...",
                InsertSubtitleHere = "Insert text here",
                InsertSubtitleFileHere = "Insert subtitle file here...",
                GuessTimeCodes = "Guess time codes...",
                CharsSecX = "CPS: {0:0.00}",
                WordsMinX = "WPM: {0:0.00}",
            };

            WaveformGenerateTimeCodes = new LanguageStructure.WaveformGenerateTimeCodes
            {
                Title = "Guess time codes",
                StartFrom = "Start from",
                CurrentVideoPosition = "Current video position",
                Beginning = "Beginning",
                DeleteLines = "Delete lines",
                FromCurrentVideoPosition = "From current video position",
                DetectOptions = "Detect options",
                ScanBlocksOfMs = "Scan blocks of milliseconds",
                BlockAverageVolMin1 = "Block average volume must be above",
                BlockAverageVolMin2 = "% of total average volume",
                BlockAverageVolMax1 = "Block average volume must be below",
                BlockAverageVolMax2 = "% of total max volume",
                SplitLongLinesAt1 = "Split long subtitles at",
                SplitLongLinesAt2 = "milliseconds",
                Other = "Other",
            };

            WebVttNewVoice = new LanguageStructure.WebVttNewVoice
            {
                Title = "WebVTT - set new voice",
                VoiceName = "Name of voice",
            };

            WebVttProperties = new LanguageStructure.WebVttProperties
            {
                UseXTimeStamp = "Use X-TIMESTAMP-MAP header value",
                MergeLines = "Merge lines with same text on load",
                MergeStyleTags = "Merge style tags",
            };

            WebVttStyleManager = new LanguageStructure.WebVttStyleManager
            {
                Title = "WebVTT styles",
            };

            WhisperAdvanced = new LanguageStructure.WhisperAdvanced
            {
                Title = "Whisper Advanced - extra command line arguments",
                CommandLineArguments = "Extra parameters for Whisper command line:",
                Info = "Note: Different Whisper implementations have different command line parameters!",
                Standard = "Standard",
                StandardAsia = "Standard Asia",
                HighlightCurrentWord = "Highlight current word",
                Sentence = "Sentence",
                SingleWords = "Single words",
            };
        }

        //public static Language Load(StreamReader sr) // normal but slow .net way
        //{
        //    var s = new XmlSerializer(typeof(Language));
        //    var language = (Language)s.Deserialize(sr);
        //    return language;
        //}

        public static Language Load(string fileName)
        {
            var language = LanguageDeserializer.CustomDeserializeLanguage(fileName);
            var english = new Language();

            // Use alternative, if translated (Forms/Main.cs)
            if (language.Main.Menu.Tools.Number == english.Main.Menu.Tools.Number && language.General.Number != english.General.Number)
            {
                language.Main.Menu.Tools.Number = language.General.Number;
            }

            if (language.Main.Menu.Tools.EndTime == english.Main.Menu.Tools.EndTime && language.General.EndTime != english.General.EndTime)
            {
                language.Main.Menu.Tools.EndTime = language.General.EndTime;
            }

            if (language.Main.Menu.Tools.Duration == english.Main.Menu.Tools.Duration && language.General.Duration != english.General.Duration)
            {
                language.Main.Menu.Tools.Duration = language.General.Duration;
            }

            if (language.Main.Menu.Tools.StartTime == english.Main.Menu.Tools.StartTime && language.General.StartTime != english.General.StartTime)
            {
                language.Main.Menu.Tools.StartTime = language.General.StartTime;
            }

            if (language.Main.BeforeMergeLinesWithSameText == english.Main.BeforeMergeLinesWithSameText && language.Main.BeforeMergeShortLines != english.Main.BeforeMergeShortLines)
            {
                language.Main.BeforeMergeLinesWithSameText = language.Main.BeforeMergeShortLines;
            }
            // Use alternative, if translated (Forms/Settings.cs)
            if (language.Settings.AdjustSetEndTimeAndGoToNext == english.Settings.AdjustSetEndTimeAndGoToNext && language.Main.VideoControls.SetEndTimeAndGoToNext != english.Main.VideoControls.SetEndTimeAndGoToNext)
            {
                language.Settings.AdjustSetEndTimeAndGoToNext = language.Main.VideoControls.SetEndTimeAndGoToNext;
            }
            // Translated alternative without format item (../Forms/PluginsGet.cs)
            if (language.PluginsGet.UpdateAllX == english.PluginsGet.UpdateAllX && language.PluginsGet.UpdateAll != english.PluginsGet.UpdateAll)
            {
                language.PluginsGet.UpdateAllX = null;
            }

            return language;
        }

        public string GetCurrentLanguageAsXml()
        {
            var s = new XmlSerializer(typeof(Language));
            var sb = new StringBuilder();
            var w = new StringWriter(sb);
            s.Serialize(w, this);
            w.Close();

            string xml = sb.ToString();
            xml = xml.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" ", string.Empty);
            xml = xml.Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" ", string.Empty);
            xml = xml.Replace("encoding=\"utf-16\"", "encoding=\"utf-8\"");
            xml = xml.Replace("<TranslatedBy> </TranslatedBy>", "<TranslatedBy>Translated by Nikse</TranslatedBy>");
            return xml.Trim();
        }

        public void Save(string fileName)
        {
            File.WriteAllText(fileName, GetCurrentLanguageAsXml(), Encoding.UTF8);
        }
    }
}
