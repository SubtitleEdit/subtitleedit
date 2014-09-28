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
        public LanguageStructure.AdjustDisplayDuration AdjustDisplayDuration;
        public LanguageStructure.ApplyDurationLimits ApplyDurationLimits;
        public LanguageStructure.AutoBreakUnbreakLines AutoBreakUnbreakLines;
        public LanguageStructure.BatchConvert BatchConvert;
        public LanguageStructure.Beamer Beamer;
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
        public LanguageStructure.DCinemaProperties DCinemaProperties;
        public LanguageStructure.DurationsBridgeGaps DurationsBridgeGaps;
        public LanguageStructure.DvdSubRip DvdSubRip;
        public LanguageStructure.DvdSubRipChooseLanguage DvdSubRipChooseLanguage;
        public LanguageStructure.EbuSaveOptions EbuSaveOptions;
        public LanguageStructure.EffectKaraoke EffectKaraoke;
        public LanguageStructure.EffectTypewriter EffectTypewriter;
        public LanguageStructure.ExportCustomText ExportCustomText;
        public LanguageStructure.ExportCustomTextFormat ExportCustomTextFormat;
        public LanguageStructure.ExportPngXml ExportPngXml;
        public LanguageStructure.ExportText ExportText;
        public LanguageStructure.ExtractDateTimeInfo ExtractDateTimeInfo;
        public LanguageStructure.FindDialog FindDialog;
        public LanguageStructure.FindSubtitleLine FindSubtitleLine;
        public LanguageStructure.FixCommonErrors FixCommonErrors;
        public LanguageStructure.GetDictionaries GetDictionaries;
        public LanguageStructure.GetTesseractDictionaries GetTesseractDictionaries;
        public LanguageStructure.GoogleTranslate GoogleTranslate;
        public LanguageStructure.GoogleOrMicrosoftTranslate GoogleOrMicrosoftTranslate;
        public LanguageStructure.GoToLine GoToLine;
        public LanguageStructure.ImportImages ImportImages;
        public LanguageStructure.ImportSceneChanges ImportSceneChanges;
        public LanguageStructure.ImportText ImportText;
        public LanguageStructure.Interjections Interjections;
        public LanguageStructure.JoinSubtitles JoinSubtitles;
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
        public LanguageStructure.SetVideoOffset SetVideoOffset;
        public LanguageStructure.ShowEarlierLater ShowEarlierLater;
        public LanguageStructure.ShowHistory ShowHistory;
        public LanguageStructure.SpellCheck SpellCheck;
        public LanguageStructure.Split Split;
        public LanguageStructure.SplitLongLines SplitLongLines;
        public LanguageStructure.SplitSubtitle SplitSubtitle;
        public LanguageStructure.StartNumberingFrom StartNumberingFrom;
        public LanguageStructure.Statistics Statistics;
        public LanguageStructure.SubStationAlphaProperties SubStationAlphaProperties;
        public LanguageStructure.SubStationAlphaStyles SubStationAlphaStyles;
        public LanguageStructure.PointSync PointSync;
        public LanguageStructure.TransportStreamSubtitleChooser TransportStreamSubtitleChooser;
        public LanguageStructure.UnknownSubtitle UnknownSubtitle;
        public LanguageStructure.VisualSync VisualSync;
        public LanguageStructure.VobSubEditCharacters VobSubEditCharacters;
        public LanguageStructure.VobSubOcr VobSubOcr;
        public LanguageStructure.VobSubOcrCharacter VobSubOcrCharacter;
        public LanguageStructure.VobSubOcrCharacterInspect VobSubOcrCharacterInspect;
        public LanguageStructure.VobSubOcrNewFolder VobSubOcrNewFolder;
        public LanguageStructure.Waveform Waveform;
        public LanguageStructure.WaveformGenerateTimeCodes WaveformGenerateTimeCodes;
        public LanguageStructure.WebVttNewVoice WebVttNewVoice;

        public Language()
        {
            Name = "English";

            General = new LanguageStructure.General
            {
                Title = "Subtitle Edit",
                Version = "3.4",
                TranslatedBy = " ",
                CultureName = "en-US",
                HelpFile = string.Empty,
                Ok = "&OK",
                Cancel = "C&ancel",
                Apply = "Apply",
                None = "None",
                All = "All",
                Preview = "Preview",
                SubtitleFiles = "Subtitle files",
                AllFiles = "All files",
                VideoFiles = "Video files",
                AudioFiles = "Audio files",
                OpenSubtitle = "Open subtitle...",
                OpenVideoFile = "Open video file...",
                OpenVideoFileTitle = "Open video file...",
                NoVideoLoaded = "No video loaded",
                VideoInformation = "Video info",
                PositionX = "Position / duration: {0}",
                StartTime = "Start time",
                EndTime = "End time",
                Duration = "Duration",
                NumberSymbol = "#",
                Number = "Number",
                Text = "Text",
                HourMinutesSecondsMilliseconds = "Hour:min:sec:ms",
                Bold = "Bold",
                Italic = "Italic",
                Underline = "Underline",
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
                OverlapPreviousLineX = "Overlap prev line ({0:#,##0.###})",
                OverlapX = "Overlap ({0:#,##0.###})",
                OverlapNextX = "Overlap next ({0:#,##0.###})",
                Negative = "Negative",
                RegularExpressionIsNotValid = "Regular expression is not valid!",
                SubtitleSaved = "Subtitle saved",
                CurrentSubtitle = "Current subtitle",
                OriginalText = "Original text",
                OpenOriginalSubtitleFile = "Open original subtitle file...",
                PleaseWait = "Please wait...",
                SessionKey = "Session key",
                UserName = "Username",
                UserNameAlreadyInUse = "Username already in use",
                WebServiceUrl = "Webservice URL",
                IP = "IP",
                VideoWindowTitle = "Video - {0}",
                AudioWindowTitle = "Audio - {0}",
                ControlsWindowTitle = "Controls - {0}",
                Advanced = "Advanced",
                Style = "Style",
                Class = "Class",
                GeneralText = "General",
                LineNumber = "Line#",
                Before = "Before",
                After = "After",
                Size = "Size",
            };

            About = new LanguageStructure.About
            {
                Title = "About Subtitle Edit",
                AboutText1 = "Subtitle Edit is Free Software under the GNU Public License." + Environment.NewLine +
                             "You may distribute, modify and use it freely." + Environment.NewLine +
                             Environment.NewLine +
                             "C# source code is available on https://github.com/SubtitleEdit/subtitleedit" + Environment.NewLine +
                             Environment.NewLine +
                             "Visit www.nikse.dk for the latest version." + Environment.NewLine +
                             Environment.NewLine +
                             "Suggestions are very welcome." + Environment.NewLine +
                             Environment.NewLine +
                             "Email: mailto:nikse.dk@gmail.com",
            };

            AddToNames = new LanguageStructure.AddToNames
            {
                Title = "Add to names/etc list",
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
                VlcMediaPlayerNotFoundTitle = "VLC media player not found",
                VlcMediaPlayerNotFound = "Subtitle Edit needs VLC media player 1.1.x or newer for extracting audio data.",
                GoToVlcMediaPlayerHomePage = "Do you want to go to the VLC media player home page?",
                GeneratingPeakFile = "Generating peak file...",
                GeneratingSpectrogram = "Generating spectrogram...",
                ExtractingSeconds = "Extracting audio: {0:0.0} seconds",
                ExtractingMinutes = "Extracting audio: {0}.{1:00} minutes",
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
                PleaseSelectAValueFromTheDropDownList = "Please select a value from the dropdown list",
                PleaseChoose = " - Please choose - ",
            };

            ApplyDurationLimits = new LanguageStructure.ApplyDurationLimits
            {
                Title = "Apply duration limits",
                FixesAvailable = "Fixes available: {0}",
                UnableToFix = "Unable to fix: {0}",
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
                ChooseOutputFolder = "Choose output folder",
                ConvertOptions = "Convert options",
                RemoveTextForHI = "Remove text for HI",
                InputDescription = "Input files (browse or drag'n'drop)",
                Convert = "Convert",
                OverwriteExistingFiles = "Overwrite existing files",
                RedoCasing = "Redo casing",
                RemoveFormatting = "Remove formatting tags",
                Status = "Status",
                Style = "Style...",
                NothingToConvert = "Nothing to convert!",
                PleaseChooseOutputFolder = "Please choose output folder",
                Converted = "Converted",
                ConvertedX = "Converted ({0})",
                Settings = "Settings",
                SplitLongLines = "Split long lines",
                AutoBalance = "Auto balance lines",
                OverwriteOriginalFiles = "Overwrite original files (new extension if format is changed)",
                ScanFolder = "Scan folder...",
                ScanningFolder = "Scanning {0} and subfolders for subtitle files...",
                Recursive = "Include sub folders",
                SetMinMsBetweenSubtitles = "Set min. milliseconds between subtitles",
                PlainText = "Plain text",
            };

            Beamer = new LanguageStructure.Beamer
            {
                Title = "Beamer",
            };

            ChangeCasing = new LanguageStructure.ChangeCasing
            {
                Title = "Change casing",
                ChangeCasingTo = "Change casing to",
                NormalCasing = "Normal casing. Sentences begin with uppercase letter.",
                FixNamesCasing = @"Fix names casing (via Dictionaries\NamesEtc.xml)",
                FixOnlyNamesCasing = @"Fix only names casing (via Dictionaries\NamesEtc.xml)",
                OnlyChangeAllUppercaseLines = "Only change all upper case lines.",
                AllUppercase = "ALL UPPERCASE",
                AllLowercase = "all lowercase",
            };

            ChangeCasingNames = new LanguageStructure.ChangeCasingNames
            {
                Title = "Change casing - Names",
                NamesFoundInSubtitleX = "Names found in subtitle: {0}",
                Enabled = "Enabled",
                Name = "Name",
                LinesFoundX = "Lines found: {0}",
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
                Info = "Change speed of subtitle in percent",
                Custom = "Custom",
                ToDropFrame = "To drop frame",
                FromDropFrame = "From drop frame",
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
                PreviousDifference = "&Previous difference",
                NextDifference = "&Next difference",
                SubtitlesNotAlike = "Subtitles have no similarities",
                XNumberOfDifference = "Number of differences: {0}",
                XNumberOfDifferenceAndPercentChanged = "Number of differences: {0} ({1}% of words changed)",
                XNumberOfDifferenceAndPercentLettersChanged = "Number of differences: {0} ({1}% of letters changed)",
                ShowOnlyDifferences = "Show only differences",
                IgnoreLineBreaks = "Ignore line breaks",
                OnlyLookForDifferencesInText = "Only look for differences in text",
                CannotCompareWithImageBasedSubtitles = "Cannot compare with image based subtitles",
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
                ZPositionHelp = "Positive numbers moves text away, negative numbers moves text closer, if z-position is zero then it's 2D",
                ChooseColor = "Choose color...",
                Generate = "Generate",
            };

            DurationsBridgeGaps = new LanguageStructure.DurationsBridgeGaps
            {
                Title = "Bridge small gaps in durations",
                GapsBridgedX = "Number of small gaps bridged: {0}",
                GapToNext = "Gap to next in seconds",
                BridgeGapsSmallerThanXPart1 = "Bridge gaps smaller than",
                BridgeGapsSmallerThanXPart2 = "milliseconds",
                MinMsBetweenLines = "Min. milliseconds between lines",
                ProlongEndTime = "Previous text takes all gap time",
                DivideEven = "Texts divides gap time",
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
                CharacterCodeTable = "Character table",
                LanguageCode = "Language code",
                OriginalProgramTitle = "Original program title",
                OriginalEpisodeTitle = "Original episode title",
                TranslatedProgramTitle = "Translated program title",
                TranslatedEpisodeTitle = "Translated episode title",
                TranslatorsName = "Translators name",
                SubtitleListReferenceCode = "Subtitle list reference code",
                CountryOfOrigin = "Country of origin",
                RevisionNumber = "Revision number",
                MaxNoOfDisplayableChars = "Max# of chars per row",
                MaxNumberOfDisplayableRows = "Max# of rows",
                DiskSequenceNumber = "Disk sequence number",
                TotalNumberOfDisks = "Total number of disks",
                Import = "Import...",
                TextAndTimingInformation = "Text and timing information",
                JustificationCode = "Justification code",
                Errors = "Errors",
                ErrorsX = "Errors: {0}",
                MaxLengthError = "Line {0} exceeds max length ({1}) by {2}: {3}",
                TextUnchangedPresentation = "Unchanged presentation",
                TextLeftJustifiedText = "Left justified text",
                TextCenteredText = "Centered text",
                TextRightJustifiedText = "Right justified text",
            };

            EffectKaraoke = new LanguageStructure.EffectKaraoke
            {
                Title = "Karaoke effect",
                ChooseColor = "Choose color:",
                TotalMilliseconds = "Total milliseconds:",
                EndDelayInMilliseconds = "End delay in milliseconds:"
            };

            EffectTypewriter = new LanguageStructure.EffectTypewriter
            {
                Title = "Typewriter effect",
                TotalMilliseconds = "Total milliseconds:",
                EndDelayInMilliseconds = "End delay in milliseconds:"
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
                DoNotModify = "[Do not modify]",
            };

            ExportPngXml = new LanguageStructure.ExportPngXml
            {
                Title = "Export BDN XML/PNG",
                ImageSettings = "Image settings",
                SimpleRendering = "Simple rendering",
                AntiAliasingWithTransparency = "Anti-aliasing with transparency",
                Text3D = "3D",
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
                ExportAllLines = "Export all lines...",
                FontColor = "Font color",
                FontFamily = "Font family",
                FontSize = "Font size",
                XImagesSavedInY = "{0} images saved in {1}",
                VideoResolution = "Video res",
                Align = "Align",
                Left = "Left",
                Center = "Center",
                Right = "Right",
                BottomMargin = "Bottom margin",
                SaveBluRraySupAs = "Choose Blu-ray sup file name",
                SaveVobSubAs = "Choose VobSub file name",
                SaveFabImageScriptAs = "Choose Blu-ray sup file name",
                SaveDvdStudioProStlAs = "Choose DVD Studio Pro STL file name",
                SomeLinesWereTooLongX = "Some lines were too long:\r\n{0}",
                LineHeight = "Line height",
                BoxSingleLine = "Box - single line",
                BoxMultiLine = "Box - multi line",
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
                TimeCodeSeperator = "Time code separator",
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
                Find = "Find",
                Normal = "&Normal",
                CaseSensitive = "&Case sensitive",
                RegularExpression = "Regular e&xpression",
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
                RemovedEmptyLinesUnsedLineBreaks = "Remove empty lines/unused line breaks",
                EmptyLinesRemovedX = "Empty lines removed: {0}",
                FixOverlappingDisplayTimes = "Fix overlapping display times",
                FixShortDisplayTimes = "Fix short display times",
                FixLongDisplayTimes = "Fix long display times",
                FixInvalidItalicTags = "Fix invalid italic tags",
                RemoveUnneededSpaces = "Remove unneeded spaces",
                RemoveUnneededPeriods = "Remove unneeded periods",
                FixMissingSpaces = "Fix missing spaces",
                BreakLongLines = "Break long lines",
                RemoveLineBreaks = "Remove line breaks in short texts with only one sentence",
                RemoveLineBreaksAll = "Remove line breaks in short texts (all except dialogs)",
                FixUppercaseIInsindeLowercaseWords = "Fix uppercase 'i' inside lowercase words (OCR error)",
                FixDoubleApostrophes = "Fix double apostrophe characters ('') to a single quote (\")",
                AddPeriods = "Add period after lines where next line start with uppercase letter",
                StartWithUppercaseLetterAfterParagraph = "Start with uppercase letter after paragraph",
                StartWithUppercaseLetterAfterPeriodInsideParagraph = "Start with uppercase letter after period inside paragraph",
                StartWithUppercaseLetterAfterColon = "Start with uppercase letter after colon/semicolon",
                CommonOcrErrorsFixed = "Common OCR errors fixed (OcrReplaceList file used): {0}",
                RemoveSpaceBetweenNumber = "Remove space between numbers",
                FixDialogsOnOneLine = "Fix dialogs on one line",
                RemoveSpaceBetweenNumbersFixed = "Remove space between numbers fixed: {0}",
                FixLowercaseIToUppercaseI = "Fix alone lowercase 'i' to 'I' (English)",
                FixTurkishAnsi = "Fix Turkish ANSI (Icelandic) letters to Unicode",
                FixDanishLetterI = "Fix Danish letter 'i'",
                FixSpanishInvertedQuestionAndExclamationMarks = "Fix Spanish inverted question and exclamation marks",
                AddMissingQuote = "Add missing quote (\")",
                AddMissingQuotes = "Add missing quotes (\")",
                FixHyphens = "Fix (remove dash) lines beginning with dash (-)",
                FixHyphensAdd = "Fix (add dash) line pairs with only one dash (-)",
                FixHyphen = "Fix line beginning with dash (-)",
                XHyphensFixed = "Dashes removed: {0}",
                AddMissingQuotesExample = "\"How are you? -> \"How are you?\"",
                XMissingQuotesAdded = "Missing quotes added: {0}",
                Fix3PlusLines = "Fix subtitles with more than two lines",
                Fix3PlusLine = "Fix subtitle with more than two lines",
                X3PlusLinesFixed = "Subtitles with more than two lines fixed: {0}",
                Analysing = "Analyzing...",
                NothingToFix = "Nothing to fix :)",
                FixesFoundX = "Fixes found: {0}",
                XFixesApplied = "Fixes applied: {0}",
                NothingToFixBut = "Nothing to fix but a few things could be improved - see log for details",
                FixLowercaseIToUppercaseICheckedButCurrentLanguageIsNotEnglish = "\"Fix alone lowercase 'i' to 'I' (English)\" is checked but currently loaded subtitle seems not be English.",
                Continue = "Continue",
                ContinueAnyway = "Continue anyway?",
                UncheckedFixLowercaseIToUppercaseI = "Unchecked \"Fix alone lowercase 'i' to 'I' (English)\"",
                XIsChangedToUppercase = "{0} i's changed to uppercase",
                FixFirstLetterToUppercaseAfterParagraph = "Fix first letter to uppercase after paragraph",
                MergeShortLine = "Merge short line (single sentence)",
                MergeShortLineAll = "Merge short line (all except dialogs)",
                XLineBreaksAdded = "{0} line breaks added",
                BreakLongLine = "Break long line",
                FixLongDisplayTime = "Fix long display time",
                FixInvalidItalicTag = "Fix invalid italic tag",
                FixShortDisplayTime = "Fix short display time",
                FixOverlappingDisplayTime = "Fix overlapping display time",
                FixInvalidItalicTagsExample = "<i>What do I care.<i> -> <i>What do I care.</i>",
                RemoveUnneededSpacesExample = "Hey   you , there. -> Hey you, there.",
                RemoveUnneededPeriodsExample = "Hey you!. -> Hey you!",
                FixMissingSpacesExample = "Hey.You. -> Hey. You.",
                FixUppercaseIInsindeLowercaseWordsExample = "The earth is fIat. -> The earth is flat.",
                FixLowercaseIToUppercaseIExample = "What do i care. -> What do I care.",
                StartTimeLaterThanEndTime = "Text number {0}: Start time is later than end time: {4}{1} -> {2} {3}",
                UnableToFixStartTimeLaterThanEndTime = "Unable to fix text number {0}: Start time is later end end time: {1}",
                XFixedToYZ = "{0} fixed to: {1}{2}",
                UnableToFixTextXY = "Unable to fix text number {0}: {1}",
                XOverlappingTimestampsFixed = "{0} overlapping timestamps fixed",
                XDisplayTimesProlonged = "{0} display times prolonged",
                XInvalidHtmlTagsFixed = "{0} invalid HTML tags fixed",
                XDisplayTimesShortned = "{0} display times shortened",
                XLinesUnbreaked = "{0} lines unbreaked",
                UnneededSpace = "Unneeded space",
                XUnneededSpacesRemoved = "{0} unneeded spaces removed",
                UnneededPeriod = "Unneeded period",
                XUnneededPeriodsRemoved = "{0} unneeded periods removed",
                FixMissingSpace = "Fix missing space",
                XMissingSpacesAdded = "{0} missing spaces added",
                FixUppercaseIInsideLowercaseWord = "Fix uppercase 'i' inside lowercase word",
                XPeriodsAdded = "{0} periods added.",
                FixMissingPeriodAtEndOfLine = "Add missing period at end of line",
                XDoubleApostrophesFixed = "{0} double apostrophes fixed.",
                XUppercaseIsFoundInsideLowercaseWords = "{0} uppercase 'i's found inside lowercase words",
                ApplyFixes = "Apply selected fixes",
                RefreshFixes = "Refresh available fixes",
                FixDoubleDash = "Fix '--' -> '...'",
                FixDoubleGreaterThan = "Remove '>>'",
                FixEllipsesStart = "Remove leading '...'",
                FixMissingOpenBracket = "Fix missing [ in line",
                FixMusicNotation = "Replace music symbols (e.g. âTª) with preferred symbol",
                FixDoubleDashExample = "'Whoa-- um yeah!' -> 'Whoa... um yeah!'",
                FixDoubleGreaterThanExample = "'>> Robert: Sup dude!' -> 'Robert: Sup dude!'",
                FixEllipsesStartExample = "'... and then we' -> 'and then we'",
                FixMissingOpenBracketExample = "'clanks] Look out!' -> '[clanks] Look out!'",
                FixMusicNotationExample = "'âTª sweet dreams are' -> '♫ sweet dreams are'",
                XFixDoubleDash = "{0} fixed '--'",
                XFixDoubleGreaterThan = "{0} removed '>>'",
                XFixEllipsesStart = "{0} remove starting '...'",
                XFixMissingOpenBracket = "{0} fixed missing [ in line",
                XFixMusicNotation = "{0} fix music notation in line",
                AutoBreak = "Auto &br",
                Unbreak = "&Unbreak",
                FixCommonOcrErrors = "Fix common OCR errors (using OCR replace list)",
                NumberOfImportantLogMessages = "{0} important log messages!",
                FixedOkXY = "Fixed and OK - '{0}': {1}",
                FixOcrErrorExample = "D0n't -> Don't",
                FixSpaceBetweenNumbersExample = "1 100 -> 1100",
                FixDialogsOneLineExample = "Hi John! - Hi Ida! -> Hi John!<br />- Hi Ida!",
            };

            GetDictionaries = new LanguageStructure.GetDictionaries
            {
                Title = "Need dictionaries?",
                DescriptionLine1 = "Subtitle Edit's spell check is based on the NHunspell engine which",
                DescriptionLine2 = "uses the spell checking dictionaries from Open Office.",
                GetDictionariesHere = "Get dictionaries here:",
                OpenOpenOfficeWiki = "Open Office Wiki Dictionaries list",
                GetAllDictionaries = "Get all dictionaries",
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
                Title = "Google translate",
                From = "From:",
                To = "To:",
                Translate = "Translate",
                PleaseWait = "Please wait... this may take a while",
                PoweredByGoogleTranslate = "Powered by Google translate",
                PoweredByMicrosoftTranslate = "Powered by Microsoft translate",
            };

            GoogleOrMicrosoftTranslate = new LanguageStructure.GoogleOrMicrosoftTranslate
            {
                Title = "Google vs Microsoft translate",
                From = "From:",
                To = "To:",
                Translate = "Translate",
                SourceText = "Source text",
                GoogleTranslate = "Google translate",
                MicrosoftTranslate = "Microsoft translate",
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
            };

            ImportSceneChanges = new LanguageStructure.ImportSceneChanges
            {
                Title = "Import scene changes",
                OpenTextFile = "Open text file...",
                ImportOptions = "Import options",
                TextFiles = "Text files",
                TimeCodes = "Time codes",
                Frames = "Frames",
                Seconds = "Seconds",
                Milliseconds = "Milliseconds",
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
                LineBreak = "Line break",
                SplitAtBlankLines = "Split at blank lines",
                MergeShortLines = "Merge short lines with continuation",
                RemoveEmptyLines = "Remove empty lines",
                RemoveLinesWithoutLetters = "Remove lines without letters",
                GenerateTimeCodes = "Generate time codes",
                GapBetweenSubtitles = "Gap between subtitles (milliseconds)",
                Auto = "Auto",
                Fixed = "Fixed",
                Refresh = "&Refresh",
                TextFiles = "Text files",
                PreviewLinesModifiedX = "Preview - subtitles modified: {0}",
                TimeCodes = "Time codes",
            };

            Interjections = new LanguageStructure.Interjections
            {
                Title = "Interjections",
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
            };

            Main = new LanguageStructure.Main
            {
                SaveChangesToUntitled = "Save changes to untitled?",
                SaveChangesToX = "Save changes to {0}?",
                SaveChangesToUntitledOriginal = "Save changes to untitled original?",
                SaveChangesToOriginalX = "Save changes to original {0}?",
                SaveSubtitleAs = "Save subtitle as...",
                SaveOriginalSubtitleAs = "Save original subtitle as...",
                NoSubtitleLoaded = "No subtitle loaded",
                VisualSyncSelectedLines = "Visual sync - selected lines",
                VisualSyncTitle = "Visual sync",
                BeforeVisualSync = "Before visual sync",
                VisualSyncPerformedOnSelectedLines = "Visual sync performed on selected lines",
                VisualSyncPerformed = "Visual sync performed",
                ImportThisVobSubSubtitle = "Import this VobSub subtitle?",
                FileXIsLargerThan10MB = "File is larger than 10 MB: {0}",
                ContinueAnyway = "Continue anyway?",
                BeforeLoadOf = "Before load of {0}",
                LoadedSubtitleX = "Loaded subtitle {0}",
                LoadedEmptyOrShort = "Loaded empty or very short subtitle {0}",
                FileIsEmptyOrShort = "File is empty or very short!",
                FileNotFound = "File not found: {0}",
                SavedSubtitleX = "Saved subtitle {0}",
                SavedOriginalSubtitleX = "Saved original subtitle {0}",
                FileOnDiskModified = "File on disk modified",
                OverwriteModifiedFile = "Overwrite the file {0} modified at {1} {2}{3} with current file loaded from disk at {4} {5}?",
                FileXIsReadOnly = "Cannot save {0}\r\n\r\nFile is read-only!",
                UnableToSaveSubtitleX = "Unable to save subtitle file {0}" + Environment.NewLine + "Subtitle seems to be empty - try to re-save if you're working on a valid subtitle!",
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
                ReplaceCountX = "Replace count: {0}",
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
                StarTimeAdjustedX = "Start time adjusted: {0}",
                BeforeCommonErrorFixes = "Before common error fixes",
                CommonErrorsFixedInSelectedLines = "Common errors fixed in selected lines",
                CommonErrorsFixed = "Common errors fixed",
                BeforeRenumbering = "Before renumbering",
                RenumberedStartingFromX = "Renumbered starting from: {0}",
                BeforeRemovalOfTextingForHearingImpaired = "Before removal of texting for hearing impaired",
                TextingForHearingImpairedRemovedOneLine = "Texting for hearing impaired removed: One line",
                TextingForHearingImpairedRemovedXLines = "Texting for hearing impaired removed: {0} lines",
                SubtitleSplitted = "Subtitle was split",
                SubtitleAppendPrompt = "This will append an existing subtitle to the currently loaded subtitle which should" + Environment.NewLine +
                                    "already be in sync with video file." + Environment.NewLine +
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
                MicrosoftTranslate = "Microsoft translate",
                BeforeGoogleTranslation = "Before Google translation",
                SelectedLinesTranslated = "Selected lines translated",
                SubtitleTranslated = "Subtitle translated",
                TranslateSwedishToDanish = "Translate currently loaded Swedish subtitle to Danish",
                TranslateSwedishToDanishWarning = "Translate currently loaded SWEDISH (are you sure it's Swedish?) subtitle to Danish?",
                TranslatingViaNikseDkMt = "Translating via www.nikse.dk/mt...",
                BeforeSwedishToDanishTranslation = "Before Swedish to Danish translation",
                TranslationFromSwedishToDanishComplete = "Translation from Swedish to Danish complete",
                TranslationFromSwedishToDanishFailed = "Translation from Swedish to Danish failed",
                BeforeUndo = "Before undo",
                UndoPerformed = "Undo performed",
                RedoPerformed = "Redo performed",
                NothingToUndo = "Nothing to undo",
                InvalidLanguageNameX = "Invalid language name: {0}",
                UnableToChangeLanguage = "Unable to change language!",
                DoNotDisplayMessageAgain = "Don't display this message again",
                NumberOfCorrectedWords = "Number of corrected words: {0}",
                NumberOfSkippedWords = "Number of skipped words: {0}",
                NumberOfCorrectWords = "Number of correct words: {0}",
                NumberOfWordsAddedToDictionary = "Number of words added to dictionary: {0}",
                NumberOfNameHits = "Number of name hits: {0}",
                SpellCheck = "Spell check",
                BeforeSpellCheck = "Before spell check",
                SpellCheckChangedXToY = "Spell check: Changed '{0}' to '{1}'",
                BeforeAddingTagX = "Before adding <{0}> tag",
                TagXAdded = "<{0}> tags added",
                LineXOfY = "line {0} of {1}",
                XLinesSavedAsY = "{0} lines saved as {1}",
                XLinesDeleted = "{0} lines deleted",
                BeforeDeletingXLines = "Before deleting {0} lines",
                DeleteXLinesPrompt = "Delete {0} lines?",
                OneLineDeleted = "Line deleted",
                BeforeDeletingOneLine = "Before deleting one line",
                DeleteOneLinePrompt = "Delete one line?",
                BeforeInsertLine = "Before insert line",
                BeforeLineUpdatedInListView = "Before line updated in listview",
                LineInserted = "Line inserted",
                BeforeSettingFontToNormal = "Before setting font to normal",
                BeforeSplitLine = "Before split line",
                LineSplitted = "Line was split",
                BeforeMergeLines = "Before merge lines",
                LinesMerged = "Lines merged",
                BeforeSettingColor = "Before setting color",
                BeforeSettingFontName = "Before setting font name",
                BeforeTypeWriterEffect = "Before typewriter effect",
                BeforeKaraokeEffect = "Before karaoke effect",
                BeforeImportingDvdSubtitle = "Before importing subtitle from DVD",
                OpenMatroskaFile = "Open Matroska file...",
                MatroskaFiles = "Matroska files",
                NoSubtitlesFound = "No subtitles found",
                NotAValidMatroskaFileX = "This is not a valid Matroska file: {0}",
                ParsingMatroskaFile = "Parsing Matroska file. Please wait...",
                BeforeImportFromMatroskaFile = "Before import subtitle from Matroska file",
                SubtitleImportedFromMatroskaFile = "Subtitle imported from Matroska file",
                DropFileXNotAccepted = "Drop file '{0}' not accepted - file is too large",
                DropOnlyOneFile = "You can only drop one file",
                BeforeCreateAdjustLines = "Before create/adjust lines",
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
                OpenXSubFiles = "Open XSub file...",
                XSubFiles = "XSub files",
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
                ShowSelectedLinesEarlierLaterPerformed = "Show earlier/later performed on selected lines",
                DoubleWordsViaRegEx = "Double words via regex {0}",
                BeforeSortX = "Before sort: {0}",
                SortedByX = "Sorted by: {0}",
                BeforeAutoBalanceSelectedLines = "Before auto balance selected lines",
                NumberOfLinesAutoBalancedX = "Number of lines auto balanced: {0}",
                BeforeRemoveLineBreaksInSelectedLines = "Before remove line-breaks from selected lines",
                NumberOfWithRemovedLineBreakX = "Number of lines with removed line-break: {0}",
                BeforeMultipleReplace = "Before multiple replace",
                NumberOfLinesReplacedX = "Number of lines with text replaced: {0}",
                NameXAddedToNamesEtcList = "The name '{0}' was added to names/etc list",
                NameXNotAddedToNamesEtcList = "The name '{0}' was NOT added to names/etc list",
                WordXAddedToUserDic = "The word '{0}' was added to the user dictionary",
                WordXNotAddedToUserDic = "The name '{0}' was NOT added to the user dictionary",
                OcrReplacePairXAdded = "The OCR replace list pair '{0} -> {1}' was added to the OCR replace list",
                OcrReplacePairXNotAdded = "The OCR replace list pair '{0} -> {1}' was NOT added to the OCR replace list",
                XLinesSelected = "{0} lines selected",
                UnicodeMusicSymbolsAnsiWarning = "Subtitle contains unicode characters. Saving using ANSI file encoding will lose these. Continue with saving?",
                UnicodeCharactersAnsiWarning = "Subtitle contains unicode characters. Saving using ANSI file encoding will lose these. Continue with saving?",
                NegativeTimeWarning = "Subtitle contains negative time codes. Continue with saving?",
                BeforeMergeShortLines = "Before merge short lines",
                BeforeSplitLongLines = "Before split long lines",
                MergedShortLinesX = "Number of lines merged: {0}",
                BeforeSetMinimumDisplayTimeBetweenParagraphs = "Before set minimum display time between subtitles",
                XMinimumDisplayTimeBetweenParagraphsChanged = "Number of lines with minimum display time between subtitles changed: {0}",
                BeforeImportText = "Before import plain text",
                TextImported = "Text imported",
                BeforePointSynchronization = "Before point synchronization",
                PointSynchronizationDone = "Point synchronization done",
                BeforeTimeCodeImport = "Before import of time codes",
                TimeCodeImportedFromXY = "Time codes imported from {0}: {1}",
                BeforeInsertSubtitleAtVideoPosition = "Before insert subtitle at video position",
                BeforeSetStartTimeAndOffsetTheRest = "Before set start time and off-set the rest",
                BeforeSetEndTimeAndOffsetTheRest = "Before set end time and off-set the rest",
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
                SpellChekingViaWordXLineYOfX = "Spell checking using Word {0} - line {1} / {2}",
                UnableToStartWord = "Unable to start Microsoft Word",
                SpellCheckAbortedXCorrections = "Spell check aborted. {0} lines were modified.",
                SpellCheckCompletedXCorrections = "Spell check completed. {0} lines were modified.",
                OpenOtherSubtitle = "Open other subtitle",
                BeforeToggleDialogDashes = "Before toggle of dialog dashes",
                TextFiles = "Text files",
                ExportPlainTextAs = "Export plain text as",
                SubtitleExported = "Subtitle exported",
                LineNumberXErrorReadingFromSourceLineY = "Line {0} - error reading: {1}",
                LineNumberXErrorReadingTimeCodeFromSourceLineY = "Line {0} - error reading time code: {1}",
                LineNumberXExpectedNumberFromSourceLineY = "Line {0} - expected subtitle number: {1}",
                BeforeGuessingTimeCodes = "Before guessing time codes",
                BeforeAutoDuration = "Before auto-duration for selected lines",
                BeforeColumnPaste = "Before column paste",
                BeforeColumnDelete = "Before column delete",
                BeforeColumnImportText = "Before column import text",
                BeforeColumnShiftCellsDown = "Before column shift cells down",
                ErrorLoadingPluginXErrorY = "Error loading plugin: {0}: {1}",
                BeforeRunningPluginXVersionY = "Before running plugin: {0}: {1}",
                UnableToReadPluginResult = "Unable to read subtitle result from plugin!",
                UnableToCreateBackupDirectory = "Unable to create backup directory {0}: {1}",
                BeforeDisplaySubtitleJoin = "Before join of subtitles",
                SubtitlesJoined = "Subtitles joined",
                StatusLog = "Status log",
                XSceneChangesImported = "{0} scene changes imported",
                PluginXExecuted = "Plugin '{0}' executed.",
                NotAValidXSubFile = "Not a valid XSub file!",
                BeforeMergeLinesWithSameText = "Before merging lines with same text",
                ImportTimeCodesDifferentNumberOfLinesWarning = "Subtitle with time codes has a different number of lines ({0}) than current subtitle ({1}) - continue anyway?",
                ParsingTransportStream = "Parsing transport stream - please wait...",
                ErrorLoadIdx = "Cannot read/edit .idx files. Idx files are a part of an idx/sub file pair (also called VobSub), and Subtitle Edit can open the .sub file.",
                ErrorLoadRar = "This file seems to be a compressed .rar file. Subtitle Edit cannot open compressed files.",
                ErrorLoadZip = "This file seems to be a compressed .zip file. Subtitle Edit cannot open compressed files.",

                Menu = new LanguageStructure.Main.MainMenu
                {
                    File = new LanguageStructure.Main.MainMenu.FileMenu
                    {
                        Title = "&File",
                        New = "&New",
                        Open = "&Open",
                        OpenKeepVideo = "Open (keep video)",
                        Reopen = "&Reopen",
                        Save = "&Save",
                        SaveAs = "Save &as...",
                        RestoreAutoBackup = "Restore auto-backup...",
                        AdvancedSubStationAlphaProperties = "Advanced Sub Station Alpha properties...",
                        SubStationAlphaProperties = "Sub Station Alpha properties...",
                        EbuProperties = "EBU STL properties...",
                        PacProperties = "PAC properties...",
                        OpenOriginal = "Open original subtitle (translator mode)...",
                        SaveOriginal = "Save original subtitle",
                        CloseOriginal = "Close original subtitle",
                        OpenContainingFolder = "Open containing folder",
                        Compare = "&Compare...",
                        Statistics = "Statisti&cs...",
                        Plugins = "Plugins...",
                        ImportOcrFromDvd = "Import/OCR subtitle from VOB/IFO (DVD)...",
                        ImportOcrVobSubSubtitle = "Import/OCR VobSub (sub/idx) subtitle...",
                        ImportBluRaySupFile = "Import/OCR Blu-ray (.sup) subtitle file...",
                        ImportXSub = "Import/OCR XSub from divx/avi...",
                        ImportSubtitleFromMatroskaFile = "Import subtitle from Matroska (.mkv) file...",
                        ImportSubtitleWithManualChosenEncoding = "Import subtitle with manual chosen encoding...",
                        ImportText = "Import plain text...",
                        ImportImages = "Import images...",
                        ImportTimecodes = "Import time codes...",
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
                        Exit = "E&xit"
                    },

                    Edit = new LanguageStructure.Main.MainMenu.EditMenu
                    {
                        Title = "Edit",
                        Undo = "Undo",
                        Redo = "Redo",
                        ShowUndoHistory = "Show history (for undo)",
                        InsertUnicodeSymbol = "Insert unicode symbol",
                        Find = "&Find",
                        FindNext = "Find &next",
                        Replace = "&Replace",
                        MultipleReplace = "&Multiple replace...",
                        GoToSubtitleNumber = "&Go to subtitle number...",
                        RightToLeftMode = "Right-to-left mode",
                        FixTrlViaUnicodeControlCharacters = "Fix RTL via Unicode control characters (for selected lines)",
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
                        DurationsBridgeGap = "Bridge gap in durations...",
                        FixCommonErrors = "&Fix common errors...",
                        StartNumberingFrom = "Renumber...",
                        RemoveTextForHearingImpaired = "Remove text for hearing impaired...",
                        ChangeCasing = "Change casing...",
                        ChangeFrameRate = "Change frame rate...",
                        ChangeSpeedInPercent = "Changed speed (percent)...",
                        MergeShortLines = "Merge short lines...",
                        MergeDuplicateText = "Merge lines with same text...",
                        MergeSameTimeCodes = "Merge lines with same time codes...",
                        SplitLongLines = "Split long lines...",
                        MinimumDisplayTimeBetweenParagraphs = "Minimum display time between subtitles...",
                        SortBy = "Sort by",
                        Number = "Number",
                        StartTime = "Start time",
                        EndTime = "End time",
                        Duration = "Duration",
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
                        OpenDvd = "Open DVD...",
                        ChooseAudioTrack = "Choose audio track",
                        CloseVideo = "Close video file",
                        ImportSceneChanges = "Import scene changes...",
                        RemoveSceneChanges = "Remove scene changes",
                        ShowHideVideo = "Show/hide video",
                        ShowHideWaveform = "Show/hide waveform",
                        ShowHideWaveformAndSpectrogram = "Show/hide waveform and spectrogram",
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
                        AddToNamesEtcList = "Add word to names/etc list",
                    },

                    Synchronization = new LanguageStructure.Main.MainMenu.SynchronizationkMenu
                    {
                        Title = "Synchronization",
                        AdjustAllTimes = "Adjust all times (show earlier/later)...",
                        VisualSync = "&Visual sync...",
                        PointSync = "Point sync...",
                        PointSyncViaOtherSubtitle = "Point sync via other subtitle...",
                    },

                    AutoTranslate = new LanguageStructure.Main.MainMenu.AutoTranslateMenu
                    {
                        Title = "Auto-translate",
                        TranslatePoweredByGoogle = "Translate (powered by Google)...",
                        TranslatePoweredByMicrosoft = "Translate (powered by Microsoft)...",
                        TranslateFromSwedishToDanish = "Translate from Swedish to Danish (powered by nikse.dk)...",
                    },

                    Options = new LanguageStructure.Main.MainMenu.OptionsMenu
                    {
                        Title = "Options",
                        Settings = "&Settings...",
                        ChooseLanguage = "&Choose language...",
                    },

                    Help = new LanguageStructure.Main.MainMenu.HelpMenu
                    {
                        CheckForUpdates = "Check for updates...",
                        Title = "Help",
                        Help = "&Help",
                        About = "&About"
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
                        VisualSync = "Visual sync",
                        SpellCheck = "Spell check",
                        Settings = "Settings",
                        Help = "Help",
                        ShowHideWaveform = "Show/hide waveform",
                        ShowHideVideo = "Show/hide video",
                    },

                    ContextMenu = new LanguageStructure.Main.MainMenu.ListViewContextMenu
                    {
                        AdvancedSubStationAlphaSetStyle = "Advanced Sub Station Alpha - set style",
                        SubStationAlphaSetStyle = "Sub Station Alpha - set style",
                        AdvancedSubStationAlphaStyles = "Advanced Sub Station Alpha styles...",
                        SubStationAlphaStyles = "Sub Station Alpha styles...",
                        TimedTextStyles = "Timed Text styles...",
                        TimedTextSetStyle = "Timed Text - set style",
                        TimedTextSetLanguage = "Timed Text - set language",
                        SamiSetStyle = "Sami - set class",
                        Cut = "Cut",
                        Copy = "Copy",
                        Paste = "Paste",
                        Delete = "Delete",
                        SplitLineAtCursorPosition = "Split line at cursor position",
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
                        ColumnCopyOriginalTextToCurrent = "Copy text from original to current",
                        Split = "Split",
                        MergeSelectedLines = "Merge selected lines",
                        MergeSelectedLinesAsDialog = "Merge selected lines as dialog",
                        MergeWithLineBefore = "Merge with line before",
                        MergeWithLineAfter = "Merge with line after",
                        Normal = "Normal",
                        Underline = "Underline",
                        Color = "Color...",
                        FontName = "Font name...",
                        Alignment = "Alignment...",
                        AutoBalanceSelectedLines = "Auto balance selected lines...",
                        RemoveLineBreaksFromSelectedLines = "Remove line-breaks from selected lines...",
                        TypewriterEffect = "Typewriter effect...",
                        KaraokeEffect = "Karaoke effect...",
                        ShowSelectedLinesEarlierLater = "Show selected lines earlier/later...",
                        VisualSyncSelectedLines = "Visual sync selected lines...",
                        GoogleAndMicrosoftTranslateSelectedLine = "Google/Microsoft translate original line",
                        GoogleTranslateSelectedLines = "Google translate selected lines...",
                        AdjustDisplayDurationForSelectedLines = "Adjust durations for selected lines...",
                        FixCommonErrorsInSelectedLines = "Fix common errors in selected lines...",
                        ChangeCasingForSelectedLines = "Change casing for selected lines...",
                        SaveSelectedLines = "Save selected lines as...",
                        WebVTTSetNewVoice = "Set new voice...",
                        WebVTTRemoveVoices = "Remove voices",
                    }
                },

                Controls = new LanguageStructure.Main.MainControls
                {
                    SubtitleFormat = "Format",
                    FileEncoding = "Encoding",
                    ListView = "List view",
                    SourceView = "Source view",
                    UndoChangesInEditPanel = "Undo changes in edit panel",
                    Previous = "< Prev",
                    Next = "Next >",
                    AutoBreak = "Auto &br",
                    Unbreak = "Unbreak"
                },

                VideoControls = new LanguageStructure.Main.MainVideoControls
                {
                    Translate = "Translate",
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

                    InsertNewSubtitleAtVideoPosition = "&Insert new subtitle at video pos",
                    Auto = "Auto",
                    PlayFromJustBeforeText = "Play from just before &text",
                    Pause = "Pause",
                    GoToSubtitlePositionAndPause = "Go to sub position and pause",
                    SetStartTime = "Set &start time",
                    SetEndTimeAndGoToNext = "Set &end && go to next",
                    AdjustedViaEndTime = "Adjusted via end time {0}",
                    SetEndTime = "Set e&nd time",
                    SetstartTimeAndOffsetOfRest = "Set sta&rt and off-set the rest",

                    GoogleIt = "Google it",
                    GoogleTranslate = "Google translate",
                    OriginalText = "Original text",
                    SearchTextOnline = "Search text online",
                    SecondsBackShort = "<<",
                    SecondsForwardShort = ">>",
                    VideoPosition = "Video position:",
                    TranslateTip = "Tip: Use <alt+arrow up/down> to go to previous/next subtitle",
                    CreateTip = "Tip: Use <ctrl+arrow left/right> keys",
                    AdjustTip = "Tip: Use <alt+arrow up/down> to go to previous/next subtitle",

                    BeforeChangingTimeInWaveformX = "Before changing time in waveform: {0}",
                    NewTextInsertAtX = "New text inserted at {0}",

                    Center = "Center",
                    PlayRate = "Play rate (speed)",
                    Slow = "Slow",
                    Normal = "Normal",
                    Fast = "Fast",
                    VeryFast = "Very fast",
                },
            };

            MatroskaSubtitleChooser = new LanguageStructure.MatroskaSubtitleChooser
            {
                Title = "Choose subtitle from Matroska file",
                PleaseChoose = "More than one subtitle found - please choose",
                TrackXLanguageYTypeZ = "Track {0} - {1} - language: {2} - type: {3}",
            };

            MeasurementConverter = new LanguageStructure.MeasurementConverter
            {
                Title = "Measurement converter",
                ConvertFrom = "Convert from",
                ConvertTo = "Convert to",
                CopyToClipboard = "Copy to clipboard",
                Celsius = "Celsius",
                Fahrenheit = "Fahrenheit",
                Miles = "Miles",
                Kilometers = "Kilometers",
                Meters = "Meters",
                Yards = "Yards",
                Feet = "Feet",
                Inches = "Inches",
                Pounds = "Pounds",
                Kilos = "Kilos",
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
                MatchingLinesX = "Matching lines: {0}",
                Contains = "Contains",
                StartsWith = "Starts with",
                EndsWith = "Ends with",
                NoContains = "Not contains",
                RegEx = "Regular expression",
                UnequalLines = "Unequal lines",
                EqualLines = "Equal lines",
            };

            MultipleReplace = new LanguageStructure.MultipleReplace
            {
                Title = "Multiple replace",
                FindWhat = "Find what",
                ReplaceWith = "Replace with",
                Normal = "Normal",
                CaseSensitive = "Case sensitive",
                RegularExpression = "Regular expression",
                LinesFoundX = "Lines found: {0}",
                Delete = "Delete",
                Add = "Add",
                Update = "&Update",
                Enabled = "Enabled",
                SearchType = "Search type",
                RemoveAll = "Remove all",
                Import = "Import...",
                Export = "Export...",
                ImportRulesTitle = "Import replace rule(s) from...",
                ExportRulesTitle = "Export replace rule(s) to...",
                Rules = "Export rules",
                MoveToBottom = "Move to bottom",
                MoveToTop = "Move to top"
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
                Log = "Log:"
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
                AnyCharacter = "Any character (.)",
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
                RemoveInterjections = "Remove interjections (shh, hmm, etc.)",
                EditInterjections = "Edit...",
            };

            ReplaceDialog = new LanguageStructure.ReplaceDialog
            {
                Title = "Replace",
                FindWhat = "Find what:",
                Normal = "&Normal",
                CaseSensitive = "&Case sensitive",
                RegularExpression = "Regular e&xpression",
                ReplaceWith = "Replace with",
                Find = "&Find",
                Replace = "&Replace",
                ReplaceAll = "Replace &all",
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
                LengthInSeconds = "Silence must be at at least (seconds)",
                MaxVolume = "Volume must be below",
            };

            SetMinimumDisplayTimeBetweenParagraphs = new LanguageStructure.SetMinimumDisplayTimeBetweenParagraphs
            {
                Title = "Set minimum display time between subtitles",
                PreviewLinesModifiedX = "Preview - subtitles modified: {0}",
                MinimumMillisecondsBetweenParagraphs = "Minimum milliseconds between lines",
                ShowOnlyModifiedLines = "Show only modified lines",
                FrameInfo = "Frame rate info",
                OneFrameXisYMilliseconds = "One frame at {0:0.00} fps is {1} milliseconds",
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
                Toolbar = "Toolbar",
                VideoPlayer = "Video player",
                WaveformAndSpectrogram = "Waveform/spectrogram",
                Tools = "Tools",
                WordLists = "Word lists",
                SsaStyle = "ASS/SSA Style",
                Proxy = "Proxy",
                ShowToolBarButtons = "Show tool bar buttons",
                New = "New",
                Open = "Open",
                Save = "Save",
                SaveAs = "Save as",
                Find = "Find",
                Replace = "Replace",
                VisualSync = "Visual sync",
                SpellCheck = "Spell check",
                SettingsName = "Settings",
                Help = "Help",
                ShowFrameRate = "Show frame rate in toolbar",
                DefaultFrameRate = "Default frame rate",
                DefaultFileEncoding = "Default file encoding",
                AutoDetectAnsiEncoding = "Auto detect ANSI encoding",
                SubtitleLineMaximumLength = "Single line max. length",
                MaximumCharactersPerSecond = "Max. chars/sec",
                AutoWrapWhileTyping = "Auto-wrap while typing",
                DurationMinimumMilliseconds = "Min. duration, milliseconds",
                DurationMaximumMilliseconds = "Max. duration, milliseconds",
                MinimumGapMilliseconds = "Min. gap between subtitles in ms",
                SubtitleFont = "Subtitle font",
                SubtitleFontSize = "Subtitle font size",
                SubtitleBold = "Bold",
                SubtitleCenter = "Center",
                SubtitleFontColor = "Subtitle font color",
                SubtitleBackgroundColor = "Subtitle background color",
                SpellChecker = "Spell checker",
                RememberRecentFiles = "Remember recent files (for reopen)",
                StartWithLastFileLoaded = "Start with last file loaded",
                RememberSelectedLine = "Remember selected line",
                RememberPositionAndSize = "Remember main window position and size",
                StartInSourceView = "Start in source view",
                RemoveBlankLinesWhenOpening = "Remove blank lines when opening a subtitle",
                ShowLineBreaksAs = "Show line breaks in list view as",
                MainListViewDoubleClickAction = "Double-clicking line in main window list view will",
                MainListViewNothing = "Nothing",
                MainListViewVideoGoToPositionAndPause = "Go to video position and pause",
                MainListViewVideoGoToPositionAndPlay = "Go to video position and play",
                MainListViewEditText = "Go to edit text box",
                MainListViewVideoGoToPositionMinus1SecAndPause = "Go to video position - 1 s and pause",
                MainListViewVideoGoToPositionMinusHalfSecAndPause = "Go to video position - 0.5 s and pause",
                MainListViewVideoGoToPositionMinus1SecAndPlay = "Go to video position - 1 s and play",
                MainListViewEditTextAndPause = "Go to edit text box, and pause at video position",
                AutoBackup = "Auto-backup",
                AutoBackupEveryMinute = "Every minute",
                AutoBackupEveryFiveMinutes = "Every 5th minute",
                AutoBackupEveryFifteenMinutes = "Every 15th minute",
                CheckForUpdates = "Check for updates",
                AllowEditOfOriginalSubtitle = "Allow edit of original subtitle",
                PromptDeleteLines = "Prompt for delete lines",
                TimeCodeMode = "Time code mode",
                TimeCodeModeHHMMSSMS = "HH:MM:SS.MS (00:00:01.500)",
                TimeCodeModeHHMMSSFF = "HH:MM:SS:FF (00:00:01:12)",
                VideoEngine = "Video engine",
                DirectShow = "DirectShow",
                DirectShowDescription = "quartz.dll in system32 folder",
                ManagedDirectX = "Managed DirectX",
                ManagedDirectXDescription = "Microsoft.DirectX.AudioVideoPlayback - .NET Managed code from DirectX",
                MpcHc = "MPC-HC",
                MpcHcDescription = "Media Player Classic - Home Cinema",
                MPlayer = "MPlayer",
                MPlayerDescription = "MPlayer2/Mplayer",
                VlcMediaPlayer = "VLC media player",
                VlcMediaPlayerDescription = "libvlc.dll from VLC media player 1.1.0 or newer",
                VlcBrowseToLabel = "VLC path (only needed if you're using the portable version of VLC)",
                ShowStopButton = "Show stop button",
                ShowMuteButton = "Show mute button",
                ShowFullscreenButton = "Show fullscreen button",
                PreviewFontSize = "Subtitle preview font size",
                MainWindowVideoControls = "Main window video controls",
                CustomSearchTextAndUrl = "Custom search text and URL",
                WaveformAppearance = "Waveform appearance",
                WaveformGridColor = "Grid color",
                WaveformShowGridLines = "Show grid lines",
                ReverseMouseWheelScrollDirection = "Reverse mouse wheel scroll direction",
                WaveformAllowOverlap = "Allow overlap (when moving/resizing)",
                WaveformFocusMouseEnter = "Set focus on mouse enter",
                WaveformListViewFocusMouseEnter = "Also set list view focus on mouse enter in list view",
                WaveformBorderHitMs1 = "Border marker hit must be within",
                WaveformBorderHitMs2 = "milliseconds",
                WaveformColor = "Color",
                WaveformSelectedColor = "Selected color",
                WaveformBackgroundColor = "Back color",
                WaveformTextColor = "Text color",
                WaveformAndSpectrogramsFolderEmpty = "Empty 'Spectrograms' and 'Waveforms' folders",
                WaveformAndSpectrogramsFolderInfo = "'Waveforms' and 'Spectrograms' folders contain {0} files ({1:0.00} MB)",
                Spectrogram = "Spectrogram",
                GenerateSpectrogram = "Generate spectrogram",
                SpectrogramAppearance = "Spectrogram appearance",
                SpectrogramOneColorGradient = "One color gradient",
                SpectrogramClassic = "Classic",
                WaveformUseFFmpeg = "Use FFmpeg for wave extraction",
                WaveformFFmpegPath = "Path to FFmpeg",
                WaveformBrowseToFFmpeg = "Browse to FFmpeg",
                WaveformBrowseToVLC = "Browse to VLC portable",
                SubStationAlphaStyle = "(Advanced) Sub Station Alpha style",
                ChooseFont = "Choose font",
                ChooseColor = "Choose color",
                SsaOutline = "Outline",
                SsaShadow = "Shadow",
                SsaOpaqueBox = "Opaque box",
                Testing123 = "Testing 123...",
                Language = "Language",
                NamesIgnoreLists = "Names/ignore list (case sensitive)",
                AddNameEtc = "Add name",
                AddWord = "Add word",
                Remove = "Remove",
                AddPair = "Add pair",
                UserWordList = "User word list",
                OcrFixList = "OCR fix list",
                Location = "Location",
                UseOnlineNamesEtc = "Use online names etc xml file",
                WordAddedX = "Word added: {0}",
                WordAlreadyExists = "Word already exists!",
                RemoveX = "Remove {0}?",
                WordNotFound = "Word not found",
                CannotUpdateNamesEtcOnline = "Cannot update NamesEtc.xml online!",
                ProxyServerSettings = "Proxy server settings",
                ProxyAddress = "Proxy address",
                ProxyAuthentication = "Authentication",
                ProxyUserName = "User name",
                ProxyPassword = "Password",
                ProxyDomain = "Domain",
                PlayXSecondsAndBack = "Play X seconds and back, X is",
                StartSceneIndex = "Start scene paragraph is",
                EndSceneIndex = "End scene paragraph is",
                FirstPlusX = "First + {0}",
                LastMinusX = "Last - {0}",
                FixCommonerrors = "Fix common errors",
                MergeLinesShorterThan = "Unbreak subtitles shorter than",
                MusicSymbol = "Music symbol",
                MusicSymbolsToReplace = "Music symbols to replace (separate by space)",
                FixCommonOcrErrorsUseHardcodedRules = "Fix common OCR errors - also use hardcoded rules",
                FixCommonerrorsFixShortDisplayTimesAllowMoveStartTime = "Fix short display time - allow move of start time",
                Shortcuts = "Shortcuts",
                Shortcut = "Shortcut",
                Control = "Control",
                Alt = "Alt",
                Shift = "Shift",
                Key = "Key",
                TextBox = "Textbox",
                UpdateShortcut = "Update",
                ShortcutIsNotValid = "Shortcut is not valid: {0}",
                ToggleDockUndockOfVideoControls = "Toggle dock/undock of video controls",
                CreateSetEndAddNewAndGoToNew = "Set end, add new and go to new",
                AdjustViaEndAutoStartAndGoToNext = "Adjust via end position and go to next",
                AdjustSetStartAutoDurationAndGoToNext = "Set start, auto duration and go to next",
                AdjustSetEndNextStartAndGoToNext = "Set end, next start and go to next",
                AdjustStartDownEndUpAndGoToNext = "Key down=set start, Key up=set end and go to next",
                AdjustSelected100MsForward = "Move selected lines 100 ms forward",
                AdjustSelected100MsBack = "Move selected lines 100 ms back",
                AdjustSetStartTimeKeepDuration = "Set start time, keep duration",
                AdjustSetEndAndOffsetTheRest = "Set end, offset the rest",
                AdjustSetEndAndOffsetTheRestAndGoToNext = "Set end, offset the rest and go to next",
                MainCreateStartDownEndUp = "Create new at key-down, set end time at key-up",
                MergeDialog = "Merge dialog (insert dashes)",
                GoToNext = "Go to next line",
                GoToPrevious = "Go to previous line",
                GoToCurrentSubtitleStart = "Go to current line start",
                GoToCurrentSubtitleEnd = "Go to current line end",
                ToggleFocus = "Toggle focus between list view and subtitle text box",
                ToggleDialogDashes = "Toggle dialog dashes",
                Alignment = "Alignment (selected lines)",
                CopyTextOnly = "Copy text only to clip board (selected lines)",
                AutoDurationSelectedLines = "Auto-duration (selected lines)",
                ReverseStartAndEndingForRTL = "Reverse RTL start/end",
                VerticalZoom = "Vertical zoom in",
                VerticalZoomOut = "Vertical zoom out",
                WaveformSeekSilenceForward = "Seek silence forward",
                WaveformSeekSilenceBack = "Seek silence back",
                WaveformAddTextHere = "Add text here (for new selection)",
                WaveformPlayNewSelection = "Play new selection",
                WaveformPlayFirstSelectedSubtitle = "Play first selected subtitle",
                WaveformFocusListView = "Focus list view",
                GoBack1Frame = "One frame back",
                GoForward1Frame = "One frame forward",
                GoBack100Milliseconds = "100 ms back",
                GoForward100Milliseconds = "100 ms forward",
                GoBack500Milliseconds = "500 ms back",
                GoForward500Milliseconds = "500 ms forward",
                GoBack1Second = "One second back",
                GoForward1Second = "One second forward",
                Pause = "Pause",
                TogglePlayPause = "Toggle play/pause",
                Fullscreen = "Fullscreen",
                CustomSearch1 = "Translate, custom search 1",
                CustomSearch2 = "Translate, custom search 2",
                CustomSearch3 = "Translate, custom search 3",
                CustomSearch4 = "Translate, custom search 4",
                CustomSearch5 = "Translate, custom search 5",
                CustomSearch6 = "Translate, custom search 6",
                SyntaxColoring = "Syntax coloring",
                ListViewSyntaxColoring = "List view syntax coloring",
                SyntaxColorDurationIfTooSmall = "Color duration if too short",
                SyntaxColorDurationIfTooLarge = "Color duration if too long",
                SyntaxColorTextIfTooLong = "Color text if too long",
                SyntaxColorTextMoreThanXLines = "Color text if more than lines:",
                SyntaxColorOverlap = "Color time code overlap",
                SyntaxErrorColor = "Error color",
                GoToFirstSelectedLine = "Go to first selected line",
                GoToNextEmptyLine = "Go to next empty line",
                MergeSelectedLines = "Merge selected lines",
                MergeSelectedLinesOnlyFirstText = "Merge selected lines, keep only first non-empty text",
                ToggleTranslationMode = "Toggle translation mode",
                SwitchOriginalAndTranslation = "Switch original and translation",
                MergeOriginalAndTranslation = "Merge original and translation",
                ShortcutIsAlreadyDefinedX = "Shortcut already defined: {0}",
                ToggleTranslationAndOriginalInPreviews = "Toggle translation and original in video/audio preview",
                ListViewColumnDelete = "Column, delete text",
                ListViewColumnInsert = "Column, insert text",
                ListViewColumnPaste = "Column, paste",
                ListViewFocusWaveform = "Focus waveform/spectrogram",
                ListViewGoToNextError = "Go to next error",
                ShowBeamer = "Start subtitle fullscreen beamer",
                MainTextBoxMoveLastWordDown = "Move last word down to next subtitle line",
                MainTextBoxMoveFirstWordFromNextUp = "Move first word from next subtitle line up",
                MainTextBoxSelectionToLower = "Selection to lowercase",
                MainTextBoxSelectionToUpper = "Selection to uppercase",
                MainTextBoxAutoBreak = "Auto break text",
                MainTextBoxUnbreak = "Unbreak text",
                MainFileSaveAll = "Save all",
                Miscellaneous = "Misc.",
                UseDoNotBreakAfterList = "Use do-not-break-after list (for auto-br)",
            };

            SetVideoOffset = new LanguageStructure.SetVideoOffset
            {
                Title = "Set video offset",
                Description = "Set video offset (subtitles should not follow real video time, but e.g. +10 hours)",
                RelativeToCurrentVideoPosition = "Relative to current video position"
            };

            ShowEarlierLater = new LanguageStructure.ShowEarlierLater
            {
                Title = "Show selected lines earlier/later",
                TitleAll = "Show all lines earlier/later",
                ShowEarlier = "Show earlier",
                ShowLater = "Show later",
                TotalAdjustmentX = "Total adjustment: {0}",
                AllLines = "All lines",
                SelectedLinesOnly = "Selected lines only",
                SelectedLinesAndForward = "Selected line(s) and forward",
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
                AddXToNamesEtc = "Add '{0}' to names/etc list",
                AutoFixNames = "Auto fix names where only casing differ",
                CheckOneLetterWords = "Prompt for unknown one letter words",
                TreatINQuoteAsING = "Treat word ending \" in' \" as \" ing \" (English only)",
                ImageText = "Image text",
                SpellCheckCompleted = "Spell check completed",
                SpellCheckAborted = "Spell check aborted",
                UndoX = "Undo: {0}",
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
                Title = "Split long lines",
                SingleLineMaximumLength = "Single line maximum length",
                LineMaximumLength = "Line maximum length",
                LineContinuationBeginEndStrings = "Line continuation begin/end strings",
                NumberOfSplits = "Number of splits: {0}",
                LongestSingleLineIsXAtY = "Longest single line length is {0} at line {1}",
                LongestLineIsXAtY = "Longest total line length is {0} at line {1}",
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
                PleaseEnterAValidNumber = "Ups, please enter a number",
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
                NumberOfLinesX = "Number of subtitle lines: {0:#,###}",
                LengthInFormatXinCharactersY = "Number of characters as {0}: {1:#,###,##0}",
                NumberOfCharactersInTextOnly = "Number of characters in text only: {0:#,###,##0}",
                NumberOfItalicTags = "Number of italic tags: {0}",
                TotalCharsPerSecond = "Total characters/second: {0:0.0} seconds",
                NumberOfBoldTags = "Number of bold tags: {0}",
                NumberOfUnderlineTags = "Number of underline tags: {0}",
                NumberOfFontTags = "Number of font tags: {0}",
                NumberOfAlignmentTags = "Number of alignment tags: {0}",
                LineLengthMinimum = "Subtitle length - minimum: {0}",
                LineLengthMaximum = "Subtitle length - maximum: {0}",
                LineLengthAverage = "Subtitle length - average: {0}",
                LinesPerSubtitleAverage = "Subtitle, number of lines - average: {0:0.0}",
                SingleLineLengthMinimum = "Single line length - minimum: {0}",
                SingleLineLengthMaximum = "Single line length - maximum: {0}",
                SingleLineLengthAverage = "Single line length - average: {0}",
                DurationMinimum = "Duration - minimum: {0:0.000} seconds",
                DurationMaximum = "Duration - maximum: {0:0.000} seconds",
                DurationAverage = "Duration - average: {0:0.000} seconds",
                CharactersPerSecondMinimum = "Characters/sec - minimum: {0:0.000}",
                CharactersPerSecondMaximum = "Characters/sec - maximum: {0:0.000}",
                CharactersPerSecondAverage = "Characters/sec - average: {0:0.000}",
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
                Options = "Options",
                WrapStyle = "Wrap style",
                Collision = "Collision",
                ScaleBorderAndShadow = "Scale border and shadow",
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
                Border = "Border",
                PlusShadow = "+ Shadow",
                OpaqueBox = "Opaque box (uses outline color)",
                Import = "Import...",
                Export = "Export...",
                New = "New",
                Copy = "Copy",
                CopyOfY = "Copy of {0}",
                CopyXOfY = "Copy {0} of {1}",
                Remove = "Remove",
                RemoveAll = "Remove all",
                ImportStyleFromFile = "Import style from file...",
                ExportStyleToFile = "Export style to file... (will add style if file already exists)",
                ChooseStyle = "Choose style to import",
                StyleAlreadyExits = "Style already exists: {0}",
                StyleXExportedToFileY = "Style '{0}' exported to file '{1}'",
                StyleXImportedFromFileY = "Style '{0}' imported from file '{1}'",
            };

            PointSync = new LanguageStructure.PointSync
            {
                Title = "Point synchronization",
                TitleViaOtherSubtitle = "Point sync via other subtitle",
                SyncHelp = "Set at least two sync points to make rough synchronization",
                SetSyncPoint = "Set sync point",
                RemoveSyncPoint = "Remove sync point",
                SyncPointsX = "Sync points: {0}",
                Info = "One sync point will adjust position, two or more sync points will adjust position and speed",
                ApplySync = "Apply",
            };

            TransportStreamSubtitleChooser = new LanguageStructure.TransportStreamSubtitleChooser
            {
                Title = "Transport stream subtitle chooser - {0}",
                PidLine = "Transport Packet Identifier (PID) = {0}, number of subtitles = {1}",
                SubLine = "{0}: {1} -> {2}, {3} image(s)",
            };

            UnknownSubtitle = new LanguageStructure.UnknownSubtitle
            {
                Title = "Unknown subtitle type",
                Message = "If you want this fixed please send an email to mailto:niksedk@gmail.com and include a copy of the subtitle.",
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
                OcrViaModi = "OCR via Microsoft Office Document Imaging (MODI). Requires Microsoft Office",
                Language = "Language",
                OcrViaImageCompare = "OCR via image compare",
                ImageDatabase = "Image database",
                NoOfPixelsIsSpace = "No of pixels is space",
                MaxErrorPercent = "Max. error%",
                New = "New",
                Edit = "Edit",
                StartOcr = "Start OCR",
                Stop = "Stop",
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
                TransportStreamGetColor = "Use color (will include some splitting of lines)",
                PromptForUnknownWords = "Prompt for unknown words",
                TryToGuessUnkownWords = "Try to guess unknown words",
                AutoBreakSubtitleIfMoreThanTwoLines = "Auto break paragraph if more than two lines",
                AllFixes = "All fixes",
                GuessesUsed = "Guesses used",
                UnknownWords = "Unknown words",
                OcrAutoCorrectionSpellChecking = "OCR auto correction / spell checking",
                OcrViaTesseract = "OCR via Tesseract",
                OcrViaNOCR = "OCR via nOCR",
                FixOcrErrors = "Fix OCR errors",
                ImportTextWithMatchingTimeCodes = "Import text with matching time codes...",
                ImportNewTimeCodes = "Import new time codes",
                SaveSubtitleImageAs = "Save subtitle image as...",
                SaveAllSubtitleImagesAsBdnXml = "Save all images (png/bdn xml)...",
                SaveAllSubtitleImagesWithHtml = "Save all images with HTML index...",
                XImagesSavedInY = "{0} images saved in {1}",
                TryModiForUnknownWords = "Try Microsoft MODI OCR for unknown words",
                DictionaryX = "Dictionary: {0}",
                RightToLeft = "Right to left",
                ShowOnlyForcedSubtitles = "Show only forced subtitles",
                UseTimeCodesFromIdx = "Use time codes from .idx file",
                NoMatch = "<No match>",
                AutoTransparentBackground = "Auto transparent background",
                InspectCompareMatchesForCurrentImage = "Inspect compare matches for current image...",
                EditLastAdditions = "Edit last image compare additions..."
            };

            VobSubOcrCharacter = new LanguageStructure.VobSubOcrCharacter
            {
                Title = "VobSub - Manual image to text",
                Abort = "&Abort",
                Skip = "&Skip",
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
            };

            VobSubOcrNewFolder = new LanguageStructure.VobSubOcrNewFolder
            {
                Title = "New folder",
                Message = "Name of new character database folder",
            };

            Waveform = new LanguageStructure.Waveform
            {
                ClickToAddWaveform = "Click to add waveform",
                ClickToAddWaveformAndSpectrogram = "Click to add waveform/spectrogram",
                Seconds = "seconds",
                ZoomIn = "Zoom in",
                ZoomOut = "Zoom out",
                AddParagraphHere = "Add text here",
                AddParagraphHereAndPasteText = "Add text from clipboard here",
                FocusTextBox = "Focus text box",
                DeleteParagraph = "Delete text",
                Split = "Split",
                SplitAtCursor = "Split at cursor",
                MergeWithPrevious = "Merge with previous",
                MergeWithNext = "Merge with next",
                PlaySelection = "Play selection",
                ShowWaveformAndSpectrogram = "Show waveform and spectrogram",
                ShowWaveformOnly = "Show waveform only",
                ShowSpectrogramOnly = "Show spectrogram only",
                SeekSilence = "Seek silence...",
                GuessTimeCodes = "Guess time codes...",
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

        }

        //public static Language Load(StreamReader sr) // normal but slow .net way
        //{
        //    var s = new XmlSerializer(typeof(Language));
        //    var language = (Language)s.Deserialize(sr);
        //    return language;
        //}

        public static Language Load(string fileName)
        {
            return LanguageDeserializer.CustomDeserializeLanguage(fileName);
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
