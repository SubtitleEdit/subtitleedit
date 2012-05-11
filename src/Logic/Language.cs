using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
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
        public LanguageStructure.AddWareForm AddWaveForm;
        public LanguageStructure.AdjustDisplayDuration AdjustDisplayDuration;
        public LanguageStructure.AutoBreakUnbreakLines AutoBreakUnbreakLines;
        public LanguageStructure.ChangeCasing ChangeCasing;
        public LanguageStructure.ChangeCasingNames ChangeCasingNames;
        public LanguageStructure.ChangeFrameRate ChangeFrameRate;
        public LanguageStructure.ChooseEncoding ChooseEncoding;
        public LanguageStructure.ChooseLanguage ChooseLanguage;
        public LanguageStructure.CompareSubtitles CompareSubtitles;
        public LanguageStructure.DvdSubRip DvdSubrip;
        public LanguageStructure.DvdSubRipChooseLanguage DvdSubRipChooseLanguage;
        public LanguageStructure.EbuSaveOtpions EbuSaveOtpions;
        public LanguageStructure.EffectKaraoke EffectKaraoke;
        public LanguageStructure.EffectTypewriter EffectTypewriter;
        public LanguageStructure.ExportPngXml ExportPngXml;
        public LanguageStructure.FindDialog FindDialog;
        public LanguageStructure.FindSubtitleLine FindSubtitleLine;
        public LanguageStructure.FixCommonErrors FixCommonErrors;
        public LanguageStructure.GetDictionaries GetDictionaries;
        public LanguageStructure.GoogleTranslate GoogleTranslate;
        public LanguageStructure.GoogleOrMicrosoftTranslate GoogleOrMicrosoftTranslate;
        public LanguageStructure.GoToLine GoToLine;
        public LanguageStructure.ImportText ImportText;
        public LanguageStructure.Interjections Interjections;
        public LanguageStructure.Main Main;
        public LanguageStructure.MatroskaSubtitleChooser MatroskaSubtitleChooser;
        public LanguageStructure.MergeShortLines MergedShortLines;
        public LanguageStructure.MultipleReplace MultipleReplace;
        public LanguageStructure.NetworkChat NetworkChat;
        public LanguageStructure.NetworkJoin NetworkJoin;
        public LanguageStructure.NetworkLogAndInfo NetworkLogAndInfo;
        public LanguageStructure.NetworkStart NetworkStart;
        public LanguageStructure.RemoveTextFromHearImpaired RemoveTextFromHearImpaired;
        public LanguageStructure.ReplaceDialog ReplaceDialog;
        public LanguageStructure.SetMinimumDisplayTimeBetweenParagraphs SetMinimumDisplayTimeBetweenParagraphs;
        public LanguageStructure.SetSyncPoint SetSyncPoint;
        public LanguageStructure.Settings Settings;
        public LanguageStructure.ShowEarlierLater ShowEarlierLater;
        public LanguageStructure.ShowHistory ShowHistory;
        public LanguageStructure.SpellCheck SpellCheck;
        public LanguageStructure.SplitLongLines SplitLongLines;
        public LanguageStructure.SplitSubtitle SplitSubtitle;
        public LanguageStructure.StartNumberingFrom StartNumberingFrom;
        public LanguageStructure.PointSync PointSync;
        public LanguageStructure.UnknownSubtitle UnknownSubtitle;
        public LanguageStructure.VisualSync VisualSync;
        public LanguageStructure.VobSubEditCharacters VobSubEditCharacters;
        public LanguageStructure.VobSubOcr VobSubOcr;
        public LanguageStructure.VobSubOcrCharacter VobSubOcrCharacter;
        public LanguageStructure.VobSubOcrCharacterInspect VobSubOcrCharacterInspect;
        public LanguageStructure.VobSubOcrNewFolder VobSubOcrNewFolder;
        public LanguageStructure.WaveForm WaveForm;

        public Language()
        {
            Name = "English";

            General = new LanguageStructure.General
            {
                Title = "Subtitle Edit",
                Version = "3.2",
                TranslatedBy = " ",
                CultureName = "en-US",
                HelpFile = string.Empty,
                OK = "&OK",
                Cancel = "C&ancel",
                Apply = "Apply",
                None = "None",
                Preview = "Preview",
                SubtitleFiles = "Subtitle files",
                AllFiles = "All files",
                VideoFiles = "Video files",
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
                HourMinutesSecondsMilliseconds = "Hour:min:sec:msec",
                Bold = "Bold",
                Italic = "Italic",
                Visible = "Visible",
                FrameRate = "Frame rate",
                Name = "Name",
                SingleLineLengths = "Single line length: ",
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
                WebServiceUrl = "Webservice url",
                IP = "IP",
                VideoWindowTitle = "Video - {0}",
                AudioWindowTitle = "Audio - {0}",
                ControlsWindowTitle = "Controls - {0}",
            };

            About = new LanguageStructure.About
            {
                Title = "About Subtitle Edit",
                AboutText1 = "Subtitle Edit is Free Software under the GNU Public License." + Environment.NewLine +
                             "You may distribute, modify and use it freely." + Environment.NewLine +
                             Environment.NewLine +
                             "C# source code is available on http://code.google.com/p/subtitleedit" + Environment.NewLine +
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

            AddWaveForm = new LanguageStructure.AddWareForm
            {
                Title = "Generate wave form data",
                GenerateWaveFormData = "Generate wave form data",
                SourceVideoFile = "Source video file:",
                PleaseWait = "This may take a few minutes - please wait",
                VlcMediaPlayerNotFoundTitle = "VLC Media Player not found",
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
                Seconds = "Seconds",
                Note = "Note: Display time will not overlap start time of next text",
                PleaseSelectAValueFromTheDropDownList = "Please select a value from the dropdown list",
                PleaseChoose =" - Please choose - ",
            };

            AutoBreakUnbreakLines = new LanguageStructure.AutoBreakUnbreakLines
            {
                TitleAutoBreak = "Auto balance selected lines",
                TitleUnbreak = "Remove line breaks from selected lines",
                LineNumber = "Line#",
                Before = "Before",
                After = "After",
                LinesFoundX = "Lines found: {0}",
                OnlyBreakLinesLongerThan = "Only break lines longer than",
                OnlyUnbreakLinesLongerThan = "Only un-break lines longer than",
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
                LineNumber = "Line#",
                Before = "Before",
                After = "After",
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

            CompareSubtitles = new LanguageStructure.CompareSubtitles
            {
                Title = "Compare subtitles",
                PreviousDifference = "&Previous difference",
                NextDifference = "&Next difference",
                SubtitlesNotAlike = "Subtitles have no similarities",
                XNumberOfDifference = "Number of differences: {0}",
                ShowOnlyDifferences = "Show only differences",
                OnlyLookForDifferencesInText = "Only look for differences in text",
            };

            DvdSubrip = new LanguageStructure.DvdSubRip
            {
                Title = "Rip subtitles from ifo/vobs (dvd)",
                DvdGroupTitle = "DVD files/info",
                IfoFile = "IFO file",
                IfoFiles = "IFO files",
                VobFiles ="VOB files",
                Add = "Add...",
                Remove = "Remove",
                Clear = "Clear",
                MoveUp = "Move up",
                MoveDown = "Move down",
                Languages = "Languages",
                PalNtsc = "PAL/NTSC",
                Pal = "PAL (25fps)",
                Ntsc = "NTSC (23.976fps)",
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
                SubtitleImageXofYAndWidthXHeight = "Subtitle image {0}/{1}  -  {2}x{3} ",
                SubtitleImage = "Subtitle image",
            };

            EbuSaveOtpions = new LanguageStructure.EbuSaveOtpions
            {
                Title = "EBU save options",
                GeneralSubtitleInformation = "General subtitle information",
                CodePageNumber = "Code page number",
                DiskFormatCode = "Disk format code",
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
                TextCentredText = "Centred text ",
                TextRightJustifiedText = "Right justified text",
            };

            EffectKaraoke = new LanguageStructure.EffectKaraoke
            {
                Title = "Karaoke effect",
                ChooseColor = "Choose color:",
                TotalMilliseconds = "Total millisecs.:",
                EndDelayInMilliseconds = "End delay in millisecs.:"
            };

            EffectTypewriter = new LanguageStructure.EffectTypewriter
            {
                Title = "Typewriter effect",
                TotalMilliseconds = "Total millisecs.:",
                EndDelayInMillisecs = "End delay in millisecs.:"
            };

            ExportPngXml = new LanguageStructure.ExportPngXml
            {
                Title = "Export BDN XML/PNG",
                ImageSettings = "Image settings",
                AntiAlias = "Anti alias",
                BorderColor = "Border color",
                BorderWidth = "Border width",
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
            };

            FindDialog = new LanguageStructure.FindDialog
            {
                Title = "Find",
                Find = "Find",
                Normal = "Normal",
                CaseSensitive = "Case sensitive",
                RegularExpression = "Regular expression",
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
                InverseSelection = "Inverse selection",
                Back = "< &Back",
                Next = "&Next >",
                Step2 = "Step 2/2 - Verify fixes",
                Fixes = "Fixes",
                Log = "Log",
                LineNumber = "Line#",
                Function = "Function",
                Before = "Before",
                After = "After",
                RemovedEmptyLine = "Removed empty line",
                RemovedEmptyLineAtTop = "Removed empty line at top",
                RemovedEmptyLineAtBottom = "Removed empty line at bottom",
                RemovedEmptyLinesUnsedLineBreaks = "Removed empty lines/unused line breaks",
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
                RemoveLineBreaksAll = "Remove line breaks in short texts (all except dialogues)",
                FixUppercaseIInsindeLowercaseWords = "Fix uppercase 'i' inside lowercase words (ocr error)",
                FixDoubleApostrophes = "Fix double apostrophe characters ('') to a single quote (\")",
                AddPeriods = "Add period after lines where next line start with uppercase letter",
                StartWithUppercaseLetterAfterParagraph = "Start with uppercase letter after paragraph",
                StartWithUppercaseLetterAfterPeriodInsideParagraph = "Start with uppercase letter after period inside paragraph",
                CommonOcrErrorsFixed = "Common OCR errors fixed (OcrReplaceList file used): {0}",
                RemoveSpaceBetweenNumber = "Remove space between numbers",
                FixDialogsOnOneLine = "Fix dialogs on one line",
                RemoveSpaceBetweenNumbersFixed = "Remove space between numbers fixed: {0}",
                FixLowercaseIToUppercaseI = "Fix alone lowercase 'i' to 'I' (English)",
                FixDanishLetterI = "Fix Danish letter 'i'",
                FixSpanishInvertedQuestionAndExclamationMarks = "Fix Spanish inverted question and exclamation marks",
                AddMissingQuote = "Add missing quote (\")",
                AddMissingQuotes = "Add missing quotes (\")",
                FixHyphens = "Fix lines beginning with hyphen (-)",
                FixHyphen = "Fix line beginning with hyphen (-)",
                XHyphensFixed = "Hyphens fixed: {0}",
                AddMissingQuotesExample = "\"How are you?  -> \"How are you?\"",
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
                MergeShortLineAll = "Merge short line (all except dialogues)",
                XLineBreaksAdded = "{0} line breaks added",
                BreakLongLine = "Break long line",
                FixLongDisplayTime = "Fix long display time",
                FixInvalidItalicTag = "Fix invalid italic tag",
                FixShortDisplayTime = "Fix short display time",
                FixOverlappingDisplayTime = "Fix overlapping display time",
                FixInvalidItalicTagsExample = "<i>What do i care.<i> -> <i>What do I care.</i>",
                RemoveUnneededSpacesExample = "Hey   you , there. -> Hey you, there.",
                RemoveUnneededPeriodsExample = "Hey you!. -> Hey you!",
                FixMissingSpacesExample = "Hey.You. -> Hey. You.",
                FixUppercaseIInsindeLowercaseWordsExample = "The earth is fIat. -> The earth is flat.",
                FixLowercaseIToUppercaseIExample = "What do i care. -> What do I care.",
                StartTimeLaterThanEndTime = "Text number {0}: Start time is later than end time: {4}{1} --> {2} {3}",
                UnableToFixStartTimeLaterThanEndTime = "Unable to fix text number {0}: Start time is later end end time: {1}",
                XFixedToYZ = "{0} fixed to: {1}{2}",
                UnableToFixTextXY = "Unable to fix text number {0}: {1}",
                XOverlappingTimestampsFixed = "{0} overlapping timestamps fixed",
                XDisplayTimesProlonged = "{0} display times prolonged",
                XInvalidHtmlTagsFixed = "{0} invalid html tags fixed",
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
                RefreshFixes ="Refresh available fixes",
                FixDoubleDash = "Fix '--' -> '...' ",
                FixDoubleGreaterThan = "Remove >> ",
                FixEllipsesStart = "Remove leading '...'",
                FixMissingOpenBracket = "Fix missing [ in line",
                FixMusicNotation = "Replace music symbols (e.g. âTª) with preferred symbol",
                FixDoubleDashs = "Fix '--'s -> '...'s ",
                FixDoubleGreaterThans = "Remove >> from start",
                FixEllipsesStarts = "Remove ... from start",
                FixMissingOpenBrackets = "Fix missing ['s ",
                FixMusicNotations = "Replace âTª's with *'s",
                FixDoubleDashExample = "'Whoa-- um yeah!' --> 'Whoa... um yeah!'",
                FixDoubleGreaterThanExample = "'>> Robert: Sup dude!'  --> 'Robert: Sup dude!'",
                FixEllipsesStartExample = "'... and then we' -> 'and then we'",
                FixMissingOpenBracketExample = "'clanks] Look out!' --> '[clanks] Look out!'",
                FixMusicNotationExample = "'âTª sweet dreams are' --> '♫ sweet dreams are'",
                XFixDoubleDash = "{0} fixed '--' ",
                XFixDoubleGreaterThan = "{0} removed >> ",
                XFixEllipsesStart = "{0} remove starting '...'",
                XFixMissingOpenBracket = "{0} fixed missing [ in line",
                XFixMusicNotation = "{0} fix music notation in line",
                AutoBreak = "Auto &br",
                Unbreak = "&Unbreak",
                FixCommonOcrErrors = "Fix common OCR errors (using OCR replace list)",
                NumberOfImportantLogMessages = "{0} important log messages!",
                FixedOkXY = "Fixed and OK - '{0}': {1}",
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

            ImportText = new LanguageStructure.ImportText
            {
                Title = "Import plain text",
                OpenTextFile = "Open text file...",
                ImportOptions = "Import options",
                Splitting = "Splitting",
                AutoSplitText = "Auto split text",
                OneLineIsOneSubtitle = "One line is one subtitle",
                SplitAtBlankLines = "Split at blank lines",
                MergeShortLines = "Merge short lines with continuation",
                RemoveEmptyLines = "Remove empty lines",
                RemoveLinesWithoutLetters = "Remove lines without letters",
                GapBetweenSubtitles = "Gap between subtitles (milliseconds)",
                Auto = "Auto",
                Fixed = "Fixed",
                Refresh = "&Refresh",
                TextFiles = "Text files",
                PreviewLinesModifiedX = "Preview - paragraphs modified: {0}",
            };

            Interjections = new LanguageStructure.Interjections
            {
                Title = "Interjections",
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
                FileXIsLargerThan10Mb = "File is larger than 10 MB: {0}",
                ContinueAnyway = "Continue anyway?",
                BeforeLoadOf = "Before load of {0}",
                LoadedSubtitleX = "Loaded subtitle {0}",
                LoadedEmptyOrShort = "Loaded empty or very short subtitle {0}",
                FileIsEmptyOrShort = "File is empty or very short!",
                FileNotFound = "File not found: {0}",
                SavedSubtitleX = "Saved subtitle {0}",
                SavedOriginalSubtitleX = "Saved original subtitle {0}",
                FileOnDiskModified = "file on disk modified",
                OverwriteModifiedFile = "Overwrite the file {0} modified at {1} {2}{3} with current file loaded from disk at {4} {5}?",
                UnableToSaveSubtitleX = "Unable to save subtitle file {0}",
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
                TextingForHearingImpairedRemovedOneLine = "Texting for hearing impaired removed : One line",
                TextingForHearingImpairedRemovedXLines = "Texting for hearing impaired removed : {0} lines",
                SubtitleSplitted = "Subtitle splitted",
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
                NothingToUndo = "Nothing to undo",
                InvalidLanguageNameX = "Invalid language name: {0}",
                UnableToChangeLanguage = "Unable to change language!",
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
                LineSplitted = "Line splitted",
                BeforeMergeLines = "Before merge lines",
                LinesMerged = "Lines merged",
                BeforeSettingColor = "Before setting color",
                BeforeSettingFontName = "Before setting font name",
                BeforeTypeWriterEffect = "Before typewriter effect",
                BeforeKaraokeEffect = "Before karaoke effect",
                BeforeImportingDvdSubtitle = "Before importing subtitle from dvd",
                OpenMatroskaFile = "Open Matroska file...",
                MatroskaFiles = "Matroska files",
                NoSubtitlesFound = "No subtitles found",
                NotAValidMatroskaFileX = "This is not a valid matroska file: {0}",
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
                XLinesSelected = "{0} lines selected",
                UnicodeMusicSymbolsAnsiWarning = "Subtitle contains unicode music notes. Saving using ANSI file encoding will lose these. Continue with saving?",
                NegativeTimeWarning = "Subtitle contains negative time codes. Continue with saving?",
                BeforeMergeShortLines = "Before merge short lines",
                BeforeSplitLongLines = "Before split long lines",
                MergedShortLinesX = "Number of lines merged: {0}",
                BeforeSetMinimumDisplayTimeBetweenParagraphs = "Before set minimum display time between paragraphs",
                XMinimumDisplayTimeBetweenParagraphsChanged = "Number of lines with minimum display time between paragraphs changed: {0}",
                BeforeImportText = "Before import plain text",
                TextImported = "Text imported",
                BeforePointSynchronization = "Before point synchronization",
                PointSynchronizationDone = "Point synchronization done",
                BeforeTimeCodeImport = "Before import of time codes",
                TimeCodeImportedFromXY = "Time codes imported from {0}: {1}",
                BeforeInsertSubtitleAtVideoPosition = "Before insert subtitle at video position",
                BeforeSetStartTimeAndOffsetTheRest = "Before set start time and off-set the rest",
                BeforeSetEndAndVideoPosition = "Before set end time at video position and auto calculate start",
                ContinueWithCurrentSpellCheck = "Continue with current spell check?",
                CharactersPerSecond = "Chars/sec: {0:0.00}",
                GetFrameRateFromVideoFile =  "Get frame rate from video file",
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
                BeforeToggleDialogueDashes = "Before toggle of dialogue dashes",
                TextFiles = "Text files",
                ExportPlainTextAs = "Export plain text as",

                Menu = new LanguageStructure.Main.MainMenu
                {
                    File = new LanguageStructure.Main.MainMenu.FileMenu
                    {
                        Title = "&File",
                        New = "&New",
                        Open = "&Open",
                        Reopen = "&Reopen",
                        Save = "&Save",
                        SaveAs = "Save &as...",
                        OpenOriginal = "Open original subtitle (translator mode)...",
                        SaveOriginal = "Save original subtitle",
                        CloseOriginal = "Close original subtitle",
                        OpenContainingFolder = "Open containing folder",
                        Compare = "&Compare...",
                        ImportOcrFromDvd = "Import/OCR subtitle from vob/ifo (dvd) ...",
                        ImportOcrVobSubSubtitle = "Import/OCR VobSub (sub/idx) subtitle...",
                        ImportBluRaySupFile = "Import/OCR Blu-ray sup file...",
                        ImportSubtitleFromMatroskaFile = "Import subtitle from Matroska file...",
                        ImportSubtitleWithManualChosenEncoding = "Import subtitle with manual chosen encoding...",
                        ImportText = "Import plain text...",
                        ImportTimecodes = "Import time codes...",
                        Export = "Export",
                        ExportBdnXml = "BDN xml/png...",
                        ExportBluRaySup = "Blu-ray sup...",
                        ExportVobSub = "VobSub (sub/idx)...",
                        ExportCavena890 = "Cavena 890...",
                        ExportEbu = "EBU stl...",
                        ExportPac = "PAC (Screen Electronics)...",
                        ExportPlainText = "Plain text...",
                        ExportPlainTextWithoutLineBreaks = "Plain text without line breaks...",
                        Exit = "E&xit"
                    },

                    Edit = new LanguageStructure.Main.MainMenu.EditMenu
                    {
                        Title = "Edit",
                        ShowUndoHistory = "Show history (for undo)",
                        InsertUnicodeSymbol = "Insert unicode symbol",
                        Find = "&Find",
                        FindNext = "Find &next",
                        Replace = "&Replace",
                        MultipleReplace = "&Multiple replace...",
                        GoToSubtitleNumber = "&Go to subtitle number...",
                        RightToLeftMode = "Right-to-left mode",
                        ReverseRightToLeftStartEnd = "Reverse RTL start/end (for selected lines)",
                    },

                    Tools = new LanguageStructure.Main.MainMenu.ToolsMenu
                    {
                        Title = "Tools",
                        AdjustDisplayDuration = "&Adjust durations...",
                        FixCommonErrors = "&Fix common errors...",
                        StartNumberingFrom = "Start numbering from...",
                        RemoveTextForHearingImpaired = "Remove text for hearing impaired...",
                        ChangeCasing = "Change casing...",
                        ChangeFrameRate = "Change frame rate...",
                        MergeShortLines = "Merge short lines...",
                        SplitLongLines = "Split long lines...",
                        MinimumDisplayTimeBetweenParagraphs = "Minimum display time between paragraphs...",
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
                        ShowOriginalTextInAudioAndVideoPreview = "Show original text in audio/video previews",
                        MakeNewEmptyTranslationFromCurrentSubtitle = "Make new empty translation from current subtitle",
                        SplitSubtitle = "Split subtitle...",
                        AppendSubtitle = "Append subtitle...",
                    },

                    Video = new LanguageStructure.Main.MainMenu.VideoMenu
                    {
                        Title = "Video",
                        OpenVideo = "Open video file...",
                        ChooseAudioTrack = "Choose audio track",
                        CloseVideo = "Close video file",
                        ShowHideVideo = "Show/hide video",
                        ShowHideWaveForm = "Show/hide waveform",
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
                        GetDictionaries = "Get dictionaries...",
                        AddToNamesEtcList = "Add word to names/ect list",
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
                        VisualSync = "Visual sync",
                        SpellCheck = "Spell check",
                        Settings = "Settings",
                        Help = "Help",
                        ShowHideWaveForm = "Show/hide wave form",
                        ShowHideVideo = "Show/hide video",
                    },

                    ContextMenu = new LanguageStructure.Main.MainMenu.ListViewContextMenu
                    {
                        SubStationAlphaSetStyle = "(Advanced) Sub Station Alpha - Set Style",
                        Cut = "Cut",
                        Copy = "Copy",
                        Paste = "Paste",
                        Delete = "Delete",
                        SplitLineAtCursorPosition = "Split line at cursor position",
                        SelectAll = "Select all",
                        InsertFirstLine = "Insert line",
                        InsertBefore = "Insert before",
                        InsertAfter = "Insert after",
                        InsertSubtitleAfter = "Insert subtitle after this line...",
                        CopyToClipboard = "Copy as text to clipboard",
                        Split = "Split",
                        MergeSelectedLines = "Merge selected lines",
                        MergeSelectedLinesASDialogue = "Merge selected lines as dialogue",
                        MergeWithLineBefore = "Merge with line before",
                        MergeWithLineAfter = "Merge with line after",
                        Normal = "Normal",
                        Underline = "Underline",
                        Color = "Color...",
                        FontName = "Font name...",
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
                    GoToSubtitlePositionAndPause = "Go to subposition and pause",
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

                    BeforeChangingTimeInWaveFormX = "Before changing time in wave form: {0}",
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

            MergedShortLines = new LanguageStructure.MergeShortLines
            {
                Title = "Merge short lines",
                MaximumCharacters = "Maximum characters in one paragraph",
                MaximumMillisecondsBetween = "Maximum milliseconds between lines",
                NumberOfMergesX = "Number of merges: {0}",
                LineNumber = "Line#",
                MergedText = "Merged text",
                OnlyMergeContinuationLines = "Only merge continuation lines",
            };

            MultipleReplace = new LanguageStructure.MultipleReplace
            {
                Title = "Multiple replace",
                FindWhat = "Find what",
                ReplaceWith = "Replace with",
                Normal = "Normal",
                CaseSensitive = "Case sensitive",
                RegularExpression = "Regular expression",
                LineNumber = "Line#",
                Before = "Before",
                After = "After",
                LinesFoundX = "Lines found: {0}",
                Delete = "Delete",
                Add = "Add",
                Update = "&Update",
                Enabled = "Enabled",
                SearchType = "Search type",
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
                LineNumber = "Line#",
                Before = "Before",
                After = "After",
                LinesFoundX = "Lines found: {0}",
                RemoveTextIfContains = "Remove text if it contains:",
                RemoveInterjections = "Remove interjections (shh, hmm, etc.)",
                EditInterjections = "Edit...",
            };

            ReplaceDialog = new LanguageStructure.ReplaceDialog
            {
                Title = "Replace",
                FindWhat = "Find what:",
                Normal = "Normal",
                CaseSensitive = "Case sensitive",
                RegularExpression = "Regular expression",
                ReplaceWith  = "Replace with",
                Find  = "&Find",
                Replace  = "&Replace",
                ReplaceAll = "Replace &all",
            };

            SetMinimumDisplayTimeBetweenParagraphs = new LanguageStructure.SetMinimumDisplayTimeBetweenParagraphs
            {
                Title = "Set minimum display time between paragraphs",
                PreviewLinesModifiedX = "Preview - paragraphs modified: {0}",
                MinimumMillisecondsBetweenParagraphs = "Minimum milliseconds between lines",
                ShowOnlyModifiedLines = "Show only modified lines",
            };

            SetSyncPoint = new LanguageStructure.SetSyncPoint
            {
                Title = "Set Sync point for line {0}",
                SyncPointTimeCode = "Sync point time code",
                ThreeSecondsBack = "<< 3 secs",
                HalfASecondBack = "<< ½ sec",
                HalfASecondForward = "½ sec >>",
                ThreeSecondsForward = "3 secs >>",
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
                SsaStyle = "SSA Style",
                Proxy =  "Proxy",
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
                AutoWrapWhileTyping = "Auto-wrap while typing",
                DurationMinimumMilliseconds = "Min. duration, milliseconds",
                DurationMaximumMilliseconds = "Max. duration, milliseconds",
                SubtitleFont = "Subtitle font",
                SubtitleFontSize = "Subtitle font size",
                SubtitleBold = "Bold",
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
                MainListViewDoubleClickAction = "Double-click on line in main window listview will",
                MainListViewNothing = "Nothing",
                MainListViewVideoGoToPositionAndPause= "Go to video pos and pause",
                MainListViewVideoGoToPositionAndPlay = "Go to video pos and play",
                MainListViewEditText = "Go to edit text box",
                AutoBackup = "Auto-backup",
                AutoBackupEveryMinute = "Every minute",
                AutoBackupEveryFiveMinutes = "Every 5th minute",
                AutoBackupEveryFifteenMinutes = "Every 15th minute",
                AllowEditOfOriginalSubtitle = "Allow edit of original subtitle",
                PromptDeleteLines = "Prompt for delete lines",
                VideoEngine = "Video engine",
                DirectShow = "DirectShow",
                DirectShowDescription = "quartz.dll in system32 folder",
                ManagedDirectX = "Managed DirectX",
                ManagedDirectXDescription = "Microsoft.DirectX.AudioVideoPlayback -  .NET Managed code from DirectX",
                MPlayer = "MPlayer",
                MPlayerDescription = "MPlayer2/Mplayer",
                VlcMediaPlayer = "VLC Media Player",
                VlcMediaPlayerDescription = "libvlc.dll from VLC Media Player 1.1.0 or newer",
                ShowStopButton = "Show stop button",
                DefaultVolume = "Default volume",
                VolumeNotes = "0 is no sound, 100 is highest volume",
                PreviewFontSize = "Subtitle preview font size",
                MainWindowVideoControls = "Main window video controls",
                CustomSearchTextAndUrl = "Custom search text and url",
                WaveFormAppearance = "Wave form appearance",
                WaveFormGridColor = "Grid color",
                WaveFormShowGridLines = "Show grid lines",
                ReverseMouseWheelScrollDirection = "Reverse mouse wheel scroll direction",
                WaveFormColor = "Color",
                WaveFormSelectedColor = "Selected color",
                WaveFormBackgroundColor = "Back color",
                WaveFormTextColor = "Text color",
                WaveformAndSpectrogramsFolderEmpty = "Empty 'Spectrograms' and 'Waveforms' folders",
                WaveformAndSpectrogramsFolderInfo = "'Waveforms' and 'Spectrograms' folders contain {0} files ({1:0.00} MB)",
                Spectrogram = "Spectrogram",
                GenerateSpectrogram = "Generate spectrogram",
                SpectrogramAppearance = "Spectrogram appearance",
                SpectrogramOneColorGradient = "One color gradient",
                SpectrogramClassic = "Classic",
                SubStationAlphaStyle = "Sub Station Alpha style",
                ChooseFont = "Choose font",
                ChooseColor = "Choose color",
                Example = "Example",
                Testing123 = "Testing 123...",
                Language = "Language",
                NamesIgnoreLists = "Names/ignore list (case sensitive)",
                AddNameEtc =  "Add name",
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
                MergeLinesShorterThan = "Merge lines shorter than",
                MusicSymbol = "Music symbol",
                MusicSymbolsToReplace = "Music symbols to replace (separate by space)",
                FixCommonOcrErrorsUseHardcodedRules = "Fix common OCR errors - also use hardcoded rules",
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
                AdjustViaEndAutoStartAndGoToNext = "Adjust via end position and go to next",
                AdjustSetStartAutoDurationAndGoToNext = "Set start, auto duration and go to next",
                AdjustSetEndNextStartAndGoToNext = "Set end, next start and go to next",
                AdjustStartDownEndUpAndGoToNext = "Key down=set start, Key up=set end and go to next",
                AdjustSelected100MsForward = "Move selected lines 100 ms forward",
                AdjustSelected100MsBack = "Move selected lines 100 ms back",
                AdjustSetStartTimeOnly = "Set start time, keep end time",
                MainCreateStartDownEndUp = "Create new at key-down, set end time at key-up",
                MergeDialogue = "Merge dialogue (insert dashes)",
                GoToNext = "Go to next line",
                GoToPrevious = "Go to previous line",
                ToggleFocus = "Toggle focus between list view and subtitle text box",
                ToggleDialogueDashes = "Toogle dialogue dashes",
                ReverseStartAndEndingForRTL = "Reverse RTL start/end",
                VerticalZoom = "Vertical zoom",
                WaveformSeekSilenceForward = "Seek silence forward",
                WaveformSeekSilenceBack = "Seek silence back",
                GoBack100Milliseconds = "100 ms back",
                GoForward100Milliseconds = "100 ms forward",
                GoBack500Milliseconds = "500 ms back",
                GoForward500Milliseconds = "500 ms forward",
                Pause = "Pause",
                TogglePlayPause = "Toggle play/pause",
                Fullscreen = "Fullscreen",
            };

            ShowEarlierLater = new LanguageStructure.ShowEarlierLater
            {
                Title = "Show selected lines earlier/later",
                TitleAll = "Show all lines earlier/later",
                ShowEarlier = "Show earlier",
                ShowLater = "Show later",
                TotalAdjustmentX = "Total adjustment: {0}",
                AllLines = "All lines",
                SelectedLinesonly = "Selected lines only",
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
                Abort = "Abort",
                Use = "Use",
                UseAlways = "&Use always",
                Suggestions = "Suggestions",
                SpellCheckProgress = "Spell check [{0}] - {1}",
                EditWholeText = "Edit whole text",
                EditWordOnly = "Edit word only",
                AddXToNamesEtc = "Add '{0}' to names/etc list",
                AutoFixNames = "Auto fix names where only casing differ",
                ImageText = "Image text",
                SpellCheckCompleted ="Spell check completed.",
                SpellCheckAborted = "Spell check aborted",
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
                UnableToSaveFileX =  "Unable to save {0}",
            };

            StartNumberingFrom = new LanguageStructure.StartNumberingFrom
            {
                Title = "Start numbering from...",
                StartFromNumber = "Start from number:",
                PleaseEnterAValidNumber = "Ups, please enter a number",
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
                HalfASecondBack = "< ½ sec",
                ThreeSecondsBack = "< 3 secs",
                PlayXSecondsAndBack = "Play {0} secs and back",
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
                OcrViaModi = "OCR via Microsoft Office Document Imaging (MODI). Requires MS Office",
                Language = "Language",
                OcrViaImageCompare = "OCR via image compare",
                ImageDatabase = "Image database",
                NoOfPixelsIsSpace = "No of pixels is space",
                New = "New",
                Edit = "Edit",
                StartOcr = "Start OCR",
                Stop = "Stop",
                StartOcrFrom = "Start OCR from subtitle no:",
                LoadingVobSubImages = "Loading VobSub images...",
                SubtitleImage = "Subtitle image",
                SubtitleText = "Subtitle text",
                UnableToCreateCharacterDatabaseFolder = "Unable to create 'Character database folder': {0}",
                SubtitleImageXofY = "Subtitle image {0} of {1}",
                ImagePalette = "Image palette",
                UseCustomColors = "Use custom colors",
                Transparent = "Transparent",
                PromptForUnknownWords = "Prompt for unknown words",
                TryToGuessUnkownWords = "Try to guess unknown words",
                AutoBreakSubtitleIfMoreThanTwoLines = "Auto break paragraph if more than two lines",
                AllFixes = "All fixes",
                GuessesUsed = "Guesses used",
                UnknownWords = "Unknown words",
                OcrAutoCorrectionSpellchecking = "OCR auto correction / spellchecking",
                OcrViaTesseract = "OCR via Tesseract",
                FixOcrErrors = "Fix OCR errors",
                ImportTextWithMatchingTimeCodes = "Import text with matching time codes...",
                SaveSubtitleImageAs = "Save subtitle image as...",
                SaveAllSubtitleImagesAsBdnXml = "Save all images (png/bdn xml)...",
                SaveAllSubtitleImagesWithHtml = "Save all images with html index...",
                XImagesSavedInY = "{0} images saved in {1}",
                TryModiForUnknownWords = "Try MS MODI OCR for unknown words",
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

            WaveForm = new LanguageStructure.WaveForm
            {
                ClickToAddWaveForm = "Click to add waveform",
                ClickToAddWaveformAndSpectrogram = "Click to add waveform/spectrogram",
                Seconds = "seconds",
                ZoomIn = "Zoom in",
                ZoomOut = "Zoom out",
                AddParagraphHere = "Add text here",
                DeleteParagraph = "Delete text",
                Split = "Split",
                SplitAtCursor = "Split at cursor",
                MergeWithPrevious = "Merge with previous",
                MergeWithNext = "Merge with next",
                PlaySelection = "Play selection",
                ShowWaveformAndSpectrogram = "Show waveform and spectrogram",
                ShowWaveformOnly = "Show waveform only",
                ShowSpectrogramOnly = "Show spectrogram only",
            };

        }

        public static Language LoadAndDecompress(StreamReader sr)
        {
            using (var zip = new GZipStream(sr.BaseStream, CompressionMode.Decompress))
            {
                var s = new XmlSerializer(typeof(Language));
                return (Language)s.Deserialize(zip);
            }
        }

        public static Language Load(StreamReader sr)
        {
            var s = new XmlSerializer(typeof(Language));
            var language = (Language)s.Deserialize(sr);
            return language;
        }

        public void Save()
        {
            var s = new XmlSerializer(typeof(Language));
            TextWriter w = new StreamWriter(Configuration.BaseDirectory + "Language.xml");
            s.Serialize(w, this);
            w.Close();
        }

        public static void TranslateViaGoogle(string languagePair)
        {
            var doc = new XmlDocument();
            doc.Load(Configuration.BaseDirectory + "Language.xml");
            if (doc.DocumentElement != null)
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                    TranslateNode(node, languagePair);

            doc.Save(Configuration.BaseDirectory + "Language.xml");
        }

        private static void TranslateNode(XmlNode node, string languagePair)
        {
            if (node.ChildNodes.Count == 0)
            {
                string oldText = node.InnerText;
                string newText = Forms.GoogleTranslate.TranslateTextViaApi(node.InnerText, languagePair);
                if (!string.IsNullOrEmpty(oldText) && !string.IsNullOrEmpty(newText))
                {
                    if (oldText.Contains("{0:"))
                    {
                        newText = oldText;
                    }
                    else
                    {
                        if (!oldText.Contains(" / "))
                            newText = newText.Replace(" / ", "/");

                        if (!oldText.Contains(" ..."))
                            newText = newText.Replace(" ...", "...");

                        if (!oldText.Contains("& "))
                            newText = newText.Replace("& ", "&");

                        if (!oldText.Contains("# "))
                            newText = newText.Replace("# ", "#");

                        if (!oldText.Contains("@ "))
                            newText = newText.Replace("@ ", "@");

                        if (oldText.Contains("{0}"))
                        {
                            newText = newText.Replace("(0)", "{0}");
                            newText = newText.Replace("(1)", "{1}");
                            newText = newText.Replace("(2)", "{2}");
                            newText = newText.Replace("(3)", "{3}");
                            newText = newText.Replace("(4)", "{4}");
                            newText = newText.Replace("(5)", "{5}");
                            newText = newText.Replace("(6)", "{6}");
                            newText = newText.Replace("(7)", "{7}");
                        }
                    }
                }
                node.InnerText = newText;
            }
            else
            {
                foreach (XmlNode childNode in node.ChildNodes)
                    TranslateNode(childNode, languagePair);
            }
        }

        private static void CompareNode(XmlNode node, string name, XmlDocument localLanguage, StringBuilder sb)
        {
            if (name.EndsWith("/#text"))
                name = name.Substring(0, name.Length - 6);
            if (localLanguage.SelectSingleNode(name) == null)
            {
                sb.AppendLine(name + " not found!");
            }
            else if (node.ChildNodes.Count > 0)
            {
                foreach (XmlNode childNode in node.ChildNodes)
                    CompareNode(childNode, name + "/" + childNode.Name, localLanguage, sb);
            }
        }
    }
}
  