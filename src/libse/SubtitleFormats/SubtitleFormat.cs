using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public abstract class SubtitleFormat
    {
        public static bool SubtitleFormatsOrderChanged { get; set; }

        private static IList<SubtitleFormat> _allSubtitleFormats;
        private static IList<SubtitleFormat> _subtitleFormatsWithDefaultOrder;

        /// <summary>
        /// Guards the format cache below. Building it loads and JIT-compiles ~330 types, which is
        /// why <see cref="WarmUpAsync"/> exists to do it off the start-up critical path - and that
        /// makes concurrent access real. A lock rather than a lazy/volatile publish because the
        /// build deliberately assigns <see cref="_allSubtitleFormats"/> before ordering it, so
        /// <see cref="GetOrderedFormatsList"/> can re-enter this property through
        /// <c>Utilities.GetSubtitleFormatByFriendlyName</c>; Monitor is re-entrant on the same
        /// thread, so that keeps working, while another thread can no longer observe the
        /// intermediate unordered list.
        /// </summary>
        /// <remarks>
        /// Stays a plain <see cref="object"/> - unlike the rest of the code base, which uses
        /// <c>System.Threading.Lock</c> - because LibSE also targets netstandard2.1, where that
        /// type does not exist. Guarding one field with <c>#if NET9_0_OR_GREATER</c> is not worth
        /// the target-framework conditional: the lock is taken a handful of times at start-up, so
        /// there is nothing to gain from the faster primitive.
        /// </remarks>
        private static readonly object SubtitleFormatsLock = new object();

        protected static readonly char[] SplitCharColon = { ':' };

        // Auto-detection hands the same List<string> to every candidate format in turn
        // (60 formats claim ".xml" alone), and most of them join all lines into one string
        // before a cheap marker test rejects the file - allocating roughly the file size in
        // garbage per format. Memoize the last join per thread; the key is the list
        // reference plus its count (no caller mutates the list between joins).
        [ThreadStatic] private static List<string> _joinedLinesKey;
        [ThreadStatic] private static int _joinedLinesKeyCount;
        [ThreadStatic] private static string _joinedLines;
        [ThreadStatic] private static string _joinedLinesTrimmed;

        /// <summary>
        /// All lines joined with <see cref="Environment.NewLine"/> after each line -
        /// same result as appending every line to a StringBuilder with AppendLine.
        /// </summary>
        protected static string JoinLines(List<string> lines)
        {
            if (!ReferenceEquals(lines, _joinedLinesKey) || lines.Count != _joinedLinesKeyCount)
            {
                var capacity = 0;
                var newLineLength = Environment.NewLine.Length;
                foreach (var line in lines)
                {
                    capacity += line.Length + newLineLength;
                }

                var sb = new StringBuilder(capacity);
                foreach (var line in lines)
                {
                    sb.AppendLine(line);
                }

                _joinedLines = sb.ToString();
                _joinedLinesTrimmed = null;
                _joinedLinesKey = lines;
                _joinedLinesKeyCount = lines.Count;
            }

            return _joinedLines;
        }

        /// <summary>
        /// Whether any line contains <paramref name="value"/> (ordinal). For a marker without a
        /// line break this is the same as searching the joined text, without joining it - used
        /// by readers that XML-parse the whole file but can only find paragraphs in elements
        /// whose start tag must then appear in the text.
        /// </summary>
        protected static bool AnyLineContains(List<string> lines, string value)
        {
            foreach (var line in lines)
            {
                if (line.Contains(value))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Same as <see cref="JoinLines"/> followed by Trim().</summary>
        protected static string JoinLinesTrimmed(List<string> lines)
        {
            var joined = JoinLines(lines);
            return _joinedLinesTrimmed ??= joined.Trim();
        }

        [ThreadStatic] private static List<string> _concatLinesKey;
        [ThreadStatic] private static int _concatLinesKeyCount;
        [ThreadStatic] private static string _concatLines;
        [ThreadStatic] private static string _concatLinesTrimmed;

        /// <summary>
        /// All lines appended with no separator - same result as appending every line to a
        /// StringBuilder with Append (what the json readers do). Memoized per thread like
        /// <see cref="JoinLines"/>, as every json format in the detection walk does this join.
        /// </summary>
        protected static string ConcatLines(List<string> lines)
        {
            if (!ReferenceEquals(lines, _concatLinesKey) || lines.Count != _concatLinesKeyCount)
            {
                _concatLines = string.Concat(lines);
                _concatLinesTrimmed = null;
                _concatLinesKey = lines;
                _concatLinesKeyCount = lines.Count;
            }

            return _concatLines;
        }

        /// <summary>Same as <see cref="ConcatLines"/> followed by Trim().</summary>
        protected static string ConcatLinesTrimmed(List<string> lines)
        {
            var joined = ConcatLines(lines);
            return _concatLinesTrimmed ??= joined.Trim();
        }

        /// <summary>
        /// Whether the text starts with an RTF header ("{\rtf", leading white space ignored) -
        /// the same answer as joining all lines, trimming and testing StartsWith, but it only
        /// looks at the first non-blank line, so the many RTF readers can reject a file without
        /// joining it.
        /// </summary>
        protected static bool StartsWithRtfHeader(List<string> lines)
        {
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                return line.TrimStart().StartsWith("{\\rtf", StringComparison.Ordinal);
            }

            return false;
        }

        /// <summary>
        /// Builds the format cache on a worker thread so the ~330 type loads and constructor JITs
        /// overlap with other start-up work instead of blocking the first window. Purely an
        /// optimisation: any caller touching <see cref="AllSubtitleFormats"/> first, or while this
        /// is still running, simply blocks on the same lock and gets the same list.
        /// </summary>
        public static Task WarmUpAsync()
        {
            return Task.Run(() =>
            {
                foreach (var unused in AllSubtitleFormats)
                {
                    // Enumerating is enough - the work happens in the property getter.
                }
            });
        }

        /// <summary>
        /// Text formats supported by Subtitle Edit
        /// </summary>
        public static IEnumerable<SubtitleFormat> AllSubtitleFormats
        {
            get
            {
                lock (SubtitleFormatsLock)
                {
                    return GetOrBuildAllSubtitleFormats();
                }
            }
        }

        private static IEnumerable<SubtitleFormat> GetOrBuildAllSubtitleFormats()
        {
                if (_allSubtitleFormats != null)
                {
                    if (SubtitleFormatsOrderChanged)
                    {
                        SubtitleFormatsOrderChanged = false;
                        _allSubtitleFormats = GetOrderedFormatsList(_subtitleFormatsWithDefaultOrder);
                    }

                    return _allSubtitleFormats;
                }

                _allSubtitleFormats = new List<SubtitleFormat>
                {
                    new SubRip(),
                    new AbcIViewer(),
                    new AdobeAfterEffectsFTME(),
                    new AdobeEncore(),
                    new AdobeEncoreLineTabNewLine(),
                    new AdobeEncoreLineTabs(),
                    new AdobeEncoreTabs(),
                    new AdobeEncoreWithLineNumbers(),
                    new AdobeEncoreWithLineNumbersNtsc(),
                    new AdobePremiereMarkersCsv(),
                    new AdvancedSubStationAlpha(),
                    new AQTitle(),
                    new AudacityLabels(),
                    new AvidCaption(),
                    new AvidCaptionDropFrame(),
                    new AvidDvd(),
                    new AvidLocationMarkers(),
                    new AwsTranscribeJson(),
                    new BelleNuitSubtitler(),
                    new Bilibili(),
                    new Cappella(),
                    new CaptionAssistant(),
                    new Captionate(),
                    new CaptionateMs(),
                    new CaraokeXml(),
                    new CheetahCaptionAsc(),
                    new Csv(),
                    new Csv2(),
                    new Csv3(),
                    new Csv4(),
                    new Csv5(),
                    new CsvDaVinci(),
                    new CsvExcel(),
                    new CsvNuendo(),
                    new DCinemaInterop(),
                    new DCinemaSmpte2007(),
                    new DCinemaSmpte2010(),
                    new DCinemaSmpte2014(),
                    new DfxpBasic(),
                    new DigiBeta(),
                    new Drtic(),
                    new DvdStudioPro(),
                    new DvdStudioProSpaceOne(),
                    new DvdStudioProSpaceOneSemicolon(),
                    new DvdStudioProSpace(),
                    new DvdSubtitle(),
                    new DvdSubtitleSystem(),
                    new DvSubtitle(),
                    new DvbTeletext(),
                    new Ebu(),
                    new Edius4Frames(),
                    new Edius4Ms(),
                    new EdiusMarkerList2Frames(),
                    new EdiusMarkerList2Ms(),
                    new EdiusMarkerList3Frames(),
                    new EdiusMarkerList3Ms(),
                    new DaVinciResolveMarkerEdl(), // before Edl - the generic EDL would claim marker files with garbage text
                    new Edl(),
                    new Eeg708(),
                    new ElrPrint(),
                    new ESubXf(),
                    new F4Text(),
                    new EZTSubtitlesProject(),
                    new F4Rtf(),
                    new F4Xml(),
                    new FabSubtitler(),
                    new FilmEditXml(),
                    new FinalCutProXml(),
                    new FinalCutProXXml(),
                    new FinalCutProXmlGap(),
                    new FinalCutProXmlName(),
                    new FinalCutProXCM(),
                    new FinalCutProXml13(),
                    new FinalCutProXml14(),
                    new FinalCutProXml14Text(),
                    new FinalCutProXmlCaptions(),
                    new FinalCutProXml15(),
                    new FinalCutProXml16(),
                    new FinalCutProXml17(),
                    new FinalCutProXml18(),
                    new FinalCutProXml19(),
                    new FinalCutProXml110(),
                    new FinalCutProXml111(),
                    new FinalCutProXml112(),
                    new FinalCutProXml113(),
                    new FinalCutProXml114(),
                    new FinalCutProTestXml(),
                    new FinalCutProTest2Xml(),
                    new FfmpegMetadataChapters(),
                    new FlashXml(),
                    new FLVCoreCuePoints(),
                    new Footage(),
                    new GooglePlayJson(),
                    new GpacTtxt(),
                    new CcExtractorTimedTranscript(),
                    new Grid608(),
                    new Gremots(),
                    new HollyStarJson(),
                    new ImageLogicAutocaption(),
                    new InqScribe(),
                    new IssXml(),
                    new EbuTt(), // before EbuTtD - a Part 1 document also carries the urn:ebu:tt:style namespace EbuTtD sniffs for
                    new EbuTtD(), // before iTunes/TimedText10 - their generic TTML detection would otherwise claim EBU-TT-D files
                    new ItunesTimedText(),
                    new JacoSub(),
                    new JsonAeneas(),
                    new JsonTed(),
                    new Json(),
                    new JsonType2(),
                    new JsonType3(),
                    new JsonType4(),
                    new JsonType5(),
                    new JsonType6(),
                    new JsonType7(),
                    new JsonType8(),
                    new JsonType8b(),
                    new JsonType9(),
                    new JsonType10(),
                    new JsonType11(),
                    new JsonType12(),
                    new JsonType13(),
                    new JsonType14(),
                    new JsonType15(),
                    new JsonType16(),
                    new JsonType17(),
                    new JsonType18(),
                    new JsonType19(),
                    new JsonType20(),
                    new JsonType21(),
                    new JsonType22(),
                    new JsonType23(),
                    new WistiaJson(),
                    new JsonType24(),
                    new KanopyHtml(),
                    new LambdaCap(),
                    new Lrc(),
                    new Lrc3DigitsMs(),
                    new LrcNoEndTime(),
                    new MacCaption10(),
                    new MacSub(),
                    new MagicVideoTitler(),
                    new MatroskaChaptersXml(),
                    new MediaTransData(),
                    new MicroDvd(),
                    new MidwayInscriberCGX(),
                    new MPlayer2(),
                    new MsOfficeWorkbook(),
                    new NciTimedRollUpCaptions(),
                    new NetflixImsc11Japanese(),
                    new NetflixTimedText(),
                    new NinsightXml(),
                    new NVivoTranscript(),
                    new OgmChapters(),
                    new OpenDvt(),
                    new Oresme(),
                    new OresmeDocXDocument(),
                    new OtterAi(),
                    new Pe2(),
                    new PhoenixSubtitle(),
                    new PinnacleImpression(),
                    new PListCaption(),
                    new PodcastIndexer(),
                    new ProjectionSubtitleList(),
                    new QubeMasterImport(),
                    new QuickTimeText(),
                    new RealTime(),
                    new RhozetHarmonic(),
                    new Rtf1(),
                    new Rtf2(),
                    new RxMarker(),
                    new Sami(),
                    new SamiAvDicPlayer(),
                    new SamiModern(),
                    new SamiYouTube(),
                    new Scenarist(),
                    new ScenaristClosedCaptions(),
                    new ScenaristClosedCaptionsDropFrame(),
                    new SmartTitler(),
//                    new Smil30(),
                    new SmilTimesheetData(),
                    new SmpteTt2052(),
                    new SoftNiSub(),
                    new SoftNicolonSub(),
                    new SonyDVDArchitect(),
                    new SonyDVDArchitectExplicitDuration(),
                    new SonyDVDArchitectLineAndDuration(),
                    new SonyDVDArchitectLineDurationLength(),
                    new SonyDVDArchitectTabs(),
                    new SonyDVDArchitectWithLineNumbers(),
                    new SonicDvdProducer(), // after Adobe Encore w. line# - same layout, this one only takes the column-padded files
                    new Spc(), // before TMPlayer - TMPlayer reads "00:00:05:25&..." as its own "h:mm:ss:text" and keeps the rest as text
                    new Speechmatics(),
                    new Spruce(),
                    new SpruceWithSpace(),
                    new StructuredTitles(),
                    new SubStationAlpha(),
                    new SubtitleEditorProject(),
                    new SubUrbia(),
                    new SubViewer10(),
                    new SubViewer20(),
                    new SwiftInterchange2(),
                    new SwiftText(),
                    new SwiftTextLineNumber(),
                    new SwiftTextLineNOAndDur(),
                    new TimeXml(),
                    new TimeXml2(),
                    new TimedText10(),
                    new TimedText200604(),
                    new TimedText200604CData(),
                    new TimedText200604Ooyala(),
                    new TimedText(),
                    new TimedTextImsc11(),
                    new TimedTextNoNs(),
                    new TimedTextImscRosetta(),
                    new TitleExchangePro(),
                    new Titra(),
                    new TmpegEncText(),
                    new TmpegEncAW5(),
                    new TmpegEncXml(),
                    new TMPlayer(),
                    new TranscriberXml(),
                    new Tmx14(),
                    new Tsv1(),
                    new Tsv2(),
                    new TurboTitler(),
                    new TwentyThreeJson(),
                    new TwentyThreeJsonEmbed(),
                    new TwentyThreeJsonEmbedWebSrt(),
                    new UniversalSubtitleFormat(),
                    new UTSubtitleXml(),
                    new Utx(),
                    new UtxFrames(),
                    new UleadSubtitleFormat(),
                    new VideoIndexerJson(),
                    new VocapiaSplit(),
                    new WebVTT(),
                    new WebVTTFileWithLineNumber(),
                    new WhisperRaw(),
                    new WhisperRaw2(),
                    new Xif(),
                    new Xmp(),
                    new YouTubeAnnotations(),
                    new YouTubeChapters(),
                    new YouTubeSbv(),
                    new YouTubeTranscript(),
                    new YouTubeTranscriptOneLine(),
                    new ZeroG(),
                    // new Idx(),
                    new UnknownSubtitle1(),
                    new UnknownSubtitle2(),
                    new UnknownSubtitle3(),
                    new UnknownSubtitle4(),
                    new UnknownSubtitle5(),
                    new UnknownSubtitle6(),
                    new UnknownSubtitle7(),
                    new UnknownSubtitle8(),
                    new UnknownSubtitle9(),
                    new UnknownSubtitle10(),
                    new UnknownSubtitle11(),
                    new UnknownSubtitle12(),
                    new UnknownSubtitle13(),
                    new UnknownSubtitle14(),
                    new UnknownSubtitle15(),
                    new UnknownSubtitle16(),
                    new UnknownSubtitle17(),
                    new UnknownSubtitle18(),
                    new UnknownSubtitle19(),
                    new UnknownSubtitle20(),
                    new UnknownSubtitle21(),
                    new UnknownSubtitle22(),
                    new UnknownSubtitle23(),
                    new UnknownSubtitle24(),
                    new UnknownSubtitle25(),
                    new UnknownSubtitle26(),
                    new UnknownSubtitle27(),
                    new UnknownSubtitle28(),
                    new UnknownSubtitle29(),
                    new UnknownSubtitle30(),
                    new UnknownSubtitle31(),
                    new UnknownSubtitle32(),
                    new UnknownSubtitle33(),
                    new UnknownSubtitle34(),
                    new UnknownSubtitle35(),
                    new UnknownSubtitle36(),
                    new UnknownSubtitle37(),
                    new UnknownSubtitle38(),
                    new UnknownSubtitle39(),
                    new UnknownSubtitle40(),
                    new UnknownSubtitle41(),
                    new UnknownSubtitle42(),
                    new UnknownSubtitle43(),
                    new UnknownSubtitle44(),
                    new UnknownSubtitle45(),
                    new UnknownSubtitle46(),
                    new UnknownSubtitle47(),
                    new UnknownSubtitle48(),
                    new UnknownSubtitle49(),
                    new UnknownSubtitle50(),
                    new UnknownSubtitle51(),
                    new UnknownSubtitle52(),
                    new UnknownSubtitle53(),
                    new UnknownSubtitle54(),
                    new UnknownSubtitle55(),
                    new UnknownSubtitle56(),
                    new UnknownSubtitle57(),
                    new UnknownSubtitle58(),
                    new UnknownSubtitle59(),
                    new UnknownSubtitle60(),
                    new UnknownSubtitle61(),
                    new UnknownSubtitle62(),
                    new UnknownSubtitle63(),
                    new UnknownSubtitle64(),
                    new UnknownSubtitle65(),
                    new UnknownSubtitle66(),
                    new UnknownSubtitle67(),
                    new UnknownSubtitle68(),
                    new UnknownSubtitle69(),
                    new UnknownSubtitle70(),
                    new UnknownSubtitle71(),
                    new UnknownSubtitle72(),
                    new UnknownSubtitle73(),
                    new UnknownSubtitle74(),
                    new UnknownSubtitle75(),
                    new UnknownSubtitle76(),
                    new UnknownSubtitle77(),
                    new UnknownSubtitle78(),
                    new UnknownSubtitle79(),
                    new UnknownSubtitle80(),
                    new UnknownSubtitle81(),
                    new YouTubeTimedText(),
                    new UnknownSubtitle83(),
                    new UnknownSubtitle84(),
                    new UnknownSubtitle85(),
                    new UnknownSubtitle86(),
                    new UnknownSubtitle87(),
                    new UnknownSubtitle88(),
                    new UnknownSubtitle89(),
                    new UnknownSubtitle90(),
                    new UnknownSubtitle91(),
                    new UnknownSubtitle92(),
                    new UnknownSubtitle93(),
                    new UnknownSubtitle94(),
                    new UnknownSubtitle95(),
                    new UnknownSubtitle96(),
                    new UnknownSubtitle97(),
                    new UnknownSubtitle98(),
                    new UnknownSubtitle99(),
                    new UnknownSubtitle100(),
                    new UnknownSubtitle101(),
                    new UnknownSubtitle102(),
                    new UnknownSubtitle103(),
                    new UnknownSubtitle104(),
                    new UnknownSubtitle105(),
                    new UnknownSubtitle106(),
                    new UnknownSubtitle107(),
                    new UnknownSubtitle108(),
                };               

                _subtitleFormatsWithDefaultOrder = new List<SubtitleFormat>(_allSubtitleFormats);
                _allSubtitleFormats = GetOrderedFormatsList(_subtitleFormatsWithDefaultOrder);

                return _allSubtitleFormats;
        }

        protected int _errorCount;

        public abstract string Extension
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// The format name - combo boxes of formats report ToString() as their value to screen
        /// readers, which otherwise announced "Nikse.SubtitleEdit.Core.SubtitleFormats.SubRip".
        /// </summary>
        public override string ToString() => Name;

        public virtual bool IsTimeBased => true;

        public bool IsFrameBased => !IsTimeBased;

        /// <summary>
        /// True when the file pins lines to video frames - every frame-based format, plus time based ones
        /// that write frame counts (FCP 7 XML) or frame-aligned media time (FCP X). Toggling SMPTE timing
        /// keeps these lines on the same frame (#15753).
        /// </summary>
        public virtual bool StoresFrameNumbers => IsFrameBased;

        private string _friendlyName;
        private string _friendlyNameExtension;

        /// <summary>
        /// "Name (Extension)" - cached because it is accessed in loops over all ~300 formats
        /// (FromName, GetSubtitleFormatByFriendlyName, format combo boxes) and interpolation
        /// allocated a new string on every access. A few formats have a settings-dependent
        /// Extension (MPlayer2, TimedText), so the cache is keyed on the Extension instance:
        /// constants are interned and the settings values keep their instance until changed.
        /// </summary>
        public string FriendlyName
        {
            get
            {
                var extension = Extension;
                if (_friendlyName == null || !ReferenceEquals(_friendlyNameExtension, extension))
                {
                    _friendlyNameExtension = extension;
                    _friendlyName = $"{Name} ({extension})";
                }

                return _friendlyName;
            }
        }

        public int ErrorCount => _errorCount;

        public virtual bool IsMine(List<string> lines, string fileName) =>
            TryLoadAndDetect(new Subtitle(), lines, fileName, keep: false);

        /// <summary>
        /// The detection behind <see cref="IsMine"/> for formats that detect by loading the whole
        /// file: loads into <paramref name="subtitle"/> and says whether the lines are this format.
        /// With <paramref name="keep"/> and a match, the state a real load leaves (frame rate,
        /// errors) is kept too, so opening a file can use this parse instead of loading it a
        /// second time.
        /// </summary>
        internal virtual bool TryLoadAndDetect(Subtitle subtitle, List<string> lines, string fileName, bool keep)
        {
            if (KeepDeclaredFrameRateLocal)
            {
                // Nothing to restore: a declared frame rate stays on this instance
                LoadSubtitle(subtitle, lines, fileName);
                return subtitle.Paragraphs.Count > _errorCount;
            }

            var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
            LoadSubtitle(subtitle, lines, fileName);
            var detected = subtitle.Paragraphs.Count > _errorCount;
            if (!detected || !keep)
            {
                Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
            }

            return detected;
        }

        public abstract string ToText(Subtitle subtitle, string title);

        public abstract void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName);

        public virtual void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
        }

        public virtual List<string> AlternateExtensions => new List<string>();

        /// <summary>
        /// Names this format used to be called. Settings store formats by name
        /// (DefaultSubtitleFormat, FavoriteSubtitleFormats), so renaming a format would
        /// otherwise silently drop it from an existing user's default and favorites.
        /// </summary>
        public virtual List<string> AlternateNames => new List<string>();

        public static int MillisecondsToFrames(double milliseconds)
        {
            return MillisecondsToFrames(milliseconds, Configuration.Settings.General.CurrentFrameRate);
        }

        public static int MillisecondsToFrames(double milliseconds, double frameRate)
        {
            return (int)Math.Round(milliseconds / (TimeCode.BaseUnit / GetTimeCodeFrameRate(frameRate)), MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// True when SMPTE timing is on and <paramref name="frameRate"/> is 23.976, 29.97 or 59.94.
        /// The subtitle's times are then on the non-drop-frame time code clock - 24/30/60 frames per
        /// time code second, with the video stretched by 1.001 to match - so frame 86400 at 23.976 is
        /// 01:00:00:00, i.e. 3600000 ms, not 3603600 ms (#15753).
        /// </summary>
        public static bool IsSmpteTimeCodeClock(double frameRate)
        {
            return Configuration.Settings.General.CurrentVideoIsSmpte &&
                   (Math.Abs(frameRate - 23.976) < 0.001 ||
                    Math.Abs(frameRate - 29.97) < 0.001 ||
                    Math.Abs(frameRate - 59.94) < 0.001);
        }

        /// <summary>
        /// The frame rate subtitle times and frame numbers convert with: the whole-number time code
        /// rate in SMPTE timing (see <see cref="IsSmpteTimeCodeClock"/>), otherwise the exact
        /// NTSC rate from <see cref="GetFrameForCalculation"/>.
        /// </summary>
        public static double GetTimeCodeFrameRate(double frameRate)
        {
            return IsSmpteTimeCodeClock(frameRate) ? Math.Round(frameRate) : GetFrameForCalculation(frameRate);
        }

        /// <summary>
        /// For formats that count frames with their own math (FCP 7 XML): the whole-number rate in
        /// SMPTE timing (see <see cref="IsSmpteTimeCodeClock"/>), otherwise <paramref name="frameRate"/>
        /// unchanged.
        /// </summary>
        public static double ToTimeCodeFrameRate(double frameRate)
        {
            return IsSmpteTimeCodeClock(frameRate) ? Math.Round(frameRate) : frameRate;
        }

        /// <summary>
        /// Final Cut Pro X stores real media time (a 23.976 frame is 1001/24000 s), but in SMPTE
        /// timing the subtitle's times are on the time code clock, 1.001 faster (see
        /// <see cref="IsSmpteTimeCodeClock"/>). Returns a copy on media time to write, or
        /// <paramref name="subtitle"/> itself when SMPTE timing is off.
        /// </summary>
        protected static Subtitle ToMediaTime(Subtitle subtitle)
        {
            if (!IsSmpteTimeCodeClock(Configuration.Settings.General.CurrentFrameRate))
            {
                return subtitle;
            }

            var copy = new Subtitle(subtitle, false);
            foreach (var p in copy.Paragraphs)
            {
                p.StartTime.TotalMilliseconds *= 1.001;
                p.EndTime.TotalMilliseconds *= 1.001;
            }

            return copy;
        }

        /// <summary>
        /// The reverse of <see cref="ToMediaTime"/> for a subtitle just read from media time.
        /// </summary>
        protected static void FromMediaTime(Subtitle subtitle)
        {
            FromMediaTime(subtitle, Configuration.Settings.General.CurrentFrameRate);
        }

        /// <summary>
        /// The reverse of <see cref="ToMediaTime"/> for a subtitle just read from media time at
        /// <paramref name="frameRate"/>.
        /// </summary>
        protected static void FromMediaTime(Subtitle subtitle, double frameRate)
        {
            if (!IsSmpteTimeCodeClock(frameRate))
            {
                return;
            }

            foreach (var p in subtitle.Paragraphs)
            {
                p.StartTime.TotalMilliseconds /= 1.001;
                p.EndTime.TotalMilliseconds /= 1.001;
            }
        }

        public static double GetFrameForCalculation(double frameRate)
        {
            if (Math.Abs(frameRate - 23.976) < 0.001)
            {
                return 24000.0 / 1001.0;
            }
            if (Math.Abs(frameRate - 29.97) < 0.001)
            {
                return 30000.0 / 1001.0;
            }
            if (Math.Abs(frameRate - 59.94) < 0.001)
            {
                return 60000.0 / 1001.0;
            }

            return frameRate;
        }

        public static int MillisecondsToFramesMaxFrameRate(double milliseconds)
        {
            return MillisecondsToFramesMaxFrameRate(milliseconds, Configuration.Settings.General.CurrentFrameRate);
        }

        public static int MillisecondsToFramesMaxFrameRate(double milliseconds, double frameRate)
        {
            var frames = (int)Math.Round(milliseconds / (TimeCode.BaseUnit / GetTimeCodeFrameRate(frameRate)), MidpointRounding.AwayFromZero);
            if (frames >= frameRate)
            {
                frames = (int)(frameRate - 0.01);
            }

            return frames;
        }

        public static int FramesToMilliseconds(double frames)
        {
            return FramesToMilliseconds(frames, Configuration.Settings.General.CurrentFrameRate);
        }

        public static int FramesToMilliseconds(double frames, double frameRate)
        {
            return (int)Math.Round(frames * (TimeCode.BaseUnit / GetTimeCodeFrameRate(frameRate)), MidpointRounding.AwayFromZero);
        }

        public static int FramesToMillisecondsMax999(double frames)
        {
            return FramesToMillisecondsMax999(frames, Configuration.Settings.General.CurrentFrameRate);
        }

        public static int FramesToMillisecondsMax999(double frames, double frameRate)
        {
            var ms = (int)Math.Round(frames * (TimeCode.BaseUnit / GetTimeCodeFrameRate(frameRate)), MidpointRounding.AwayFromZero);
            return Math.Min(ms, 999);
        }

        public virtual bool HasStyleSupport => false;

        /// <summary>
        /// True when the format says where a line sits on the video - TTML regions, EBU STL teletext
        /// rows or PAC's vertical percentage - and keeps it on the paragraphs (Region, Effect or
        /// MarginV) and in the header.
        /// </summary>
        /// <remarks>
        /// The video preview asks before it turns any of that into ASSA alignment and margins (see
        /// <see cref="Common.SubtitlePositionToAssa"/>): the header and the paragraph fields of the
        /// file a subtitle was read from survive a format change in the toolbar, so a subtitle now
        /// shown as SubRip would otherwise keep the layout of a format it has left.
        /// </remarks>
        public virtual bool HasPositionSupport => false;

        /// <summary>
        /// Hard limits the format enforces when writing (e.g. CEA-608's 32 columns x 4 rows). A format
        /// that declares them lets the UI warn before a save silently re-wraps or truncates text
        /// that does not fit. Null means no such limits.
        /// </summary>
        /// <remarks>
        /// Declared without a nullable annotation: libse builds with nullable contexts off, so
        /// the annotation is inert here and only warns (CS8632). The null contract is above.
        /// </remarks>
        public virtual SubtitleFormatLimits FormatLimits => null;

        /// <summary>
        /// Largest time code the format can write. Defaults to 99:59:59,999; text formats whose
        /// readers take 3+ digit hours (SubRip, WebVTT, TTML, ASSA/SSA) allow up to
        /// <see cref="TimeCode.MaxSupportedTotalMilliseconds"/> (#16937).
        /// </summary>
        public virtual double MaxTimeCodeTotalMilliseconds => TimeCode.MaxTimeTotalMilliseconds;

        public bool BatchMode { get; set; }
        public double? BatchSourceFrameRate { get; set; }

        private double? _loadFrameRate;

        /// <summary>
        /// When true, a frame rate a file declares while loading (TTML ttp:frameRate, D-Cinema
        /// EditRate, EBU STL's header, ...) is kept on this instance only, in
        /// <see cref="DeclaredFrameRate"/>, and Configuration.Settings.General.CurrentFrameRate is
        /// never written - so several files can load at the same time
        /// (<see cref="ConcurrentSubtitleParser"/>). When false (the default) the declared frame rate
        /// is also written to CurrentFrameRate, as it always was, so File > Open picks it up.
        /// </summary>
        public bool KeepDeclaredFrameRateLocal { get; set; }

        /// <summary>
        /// The frame rate the last loaded file declared (or BatchSourceFrameRate), null if it
        /// declared none.
        /// </summary>
        public double? DeclaredFrameRate => _loadFrameRate;

        /// <summary>
        /// The frame rate to convert frames with while loading: the one the file declared (see
        /// <see cref="SetLoadFrameRate"/>), else Configuration.Settings.General.CurrentFrameRate.
        /// </summary>
        protected double LoadFrameRate => _loadFrameRate ?? Configuration.Settings.General.CurrentFrameRate;

        /// <summary>
        /// Call when the file being loaded declares its frame rate - use <see cref="LoadFrameRate"/>
        /// (not CurrentFrameRate) for the rest of the load.
        /// </summary>
        protected void SetLoadFrameRate(double frameRate)
        {
            _loadFrameRate = frameRate;
            if (!KeepDeclaredFrameRateLocal)
            {
                Configuration.Settings.General.CurrentFrameRate = frameRate;
            }
        }

        /// <summary>
        /// Call at the start of a load that may call <see cref="SetLoadFrameRate"/>, so a frame
        /// rate the previous file declared is not used for this one.
        /// </summary>
        protected void ResetLoadFrameRate()
        {
            _loadFrameRate = null;
        }

        public static string ToUtf8XmlString(XmlDocument xml, bool omitXmlDeclaration = false)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = omitXmlDeclaration,
            };
            var result = new StringBuilder();

            using (var xmlWriter = XmlWriter.Create(result, settings))
            {
                xml.Save(xmlWriter);
            }

            return result.ToString().Replace(" encoding=\"utf-16\"", " encoding=\"utf-8\"").Trim();
        }

        public virtual bool IsTextBased => true;

        protected static TimeCode DecodeTimeCodeFramesTwoParts(string[] tokens)
        {
            if (tokens == null)
            {
                return new TimeCode();
            }

            if (tokens.Length != 2)
            {
                throw new InvalidOperationException();
            }

            return new TimeCode(0, 0, int.Parse(tokens[0]), FramesToMillisecondsMax999(int.Parse(tokens[1])));
        }

        protected static TimeCode DecodeTimeCodeFramesFourParts(string[] tokens)
        {
            return DecodeTimeCodeFramesFourParts(tokens, Configuration.Settings.General.CurrentFrameRate);
        }

        protected static TimeCode DecodeTimeCodeFramesFourParts(string[] tokens, double frameRate)
        {
            if (tokens == null)
            {
                return new TimeCode();
            }

            if (tokens.Length != 4)
            {
                throw new InvalidOperationException();
            }

            if (tokens[0] == "--" && tokens[1] == "--" && tokens[2] == "--" && tokens[3] == "--")
            {
                return new TimeCode(TimeCode.MaxTimeTotalMilliseconds);
            }

            if (tokens[0] == "-" && tokens[1] == "-" && tokens[2] == "-" && tokens[3] == "-")
            {
                return new TimeCode(TimeCode.MaxTimeTotalMilliseconds);
            }

            return new TimeCode(int.Parse(tokens[0], CultureInfo.InvariantCulture), int.Parse(tokens[1], CultureInfo.InvariantCulture), int.Parse(tokens[2], CultureInfo.InvariantCulture), FramesToMillisecondsMax999(int.Parse(tokens[3], CultureInfo.InvariantCulture), frameRate));
        }

        protected static TimeCode DecodeTimeCodeMsFourParts(string[] tokens)
        {
            if (tokens == null)
            {
                return new TimeCode();
            }

            if (tokens.Length != 4)
            {
                throw new InvalidOperationException();
            }

            return new TimeCode(int.Parse(tokens[0]), int.Parse(tokens[1]), int.Parse(tokens[2]), int.Parse(tokens[3]));
        }

        protected static TimeCode DecodeTimeCodeFrames(string timestamp, char[] splitChars)
        {
            return DecodeTimeCodeFramesFourParts(timestamp.Split(splitChars, StringSplitOptions.RemoveEmptyEntries));
        }

        protected static TimeCode DecodeTimeCodeFrames(string timestamp, char[] splitChars, double frameRate)
        {
            return DecodeTimeCodeFramesFourParts(timestamp.Split(splitChars, StringSplitOptions.RemoveEmptyEntries), frameRate);
        }

        public static SubtitleFormat[] GetBinaryFormats(bool batchMode)
        {
            return new SubtitleFormat[]
            {
                new Ebu { BatchMode = batchMode },
                new DvbTeletext(),
                new Pac { BatchMode = batchMode },
                new PacUnicode(),
                new Cavena890 { BatchMode = batchMode },
                new Spt(),
                new CheetahCaption(),
                new CheetahCaptionOld(),
                new TSB4(),
                new Chk(),
                new EZTitlesBinary(),
                new Ayato(),
                new CapMakerPlus(),
                new Ultech130(),
                new NciCaption(),
                new AvidStl(),
                new WinCaps32(),
                new IsmtDfxp(),
                new Spt(),
                new Sptx(),
                new IaiSub(),
                new ELRStudioClosedCaption(),
                new CaptionsInc(),
                new TimeLineMvt(),
                new Cmaft(),
                new Pns(),
                new PlayCaptionsFreeEditor(),
                new VideoCdDat(),
                new CanvassSstg1(),
            };
        }

        public static SubtitleFormat[] GetTextOtherFormats()
        {
            return new SubtitleFormat[]
            {
                new NkhCuePoints(),
                new DlDd(),
                new Ted20(),
                new Captionate(),
                new TimeLineAscii(),
                new TimeLineFootageAscii(),
                new TimedTextImage(),
                new FinalCutProImage(),
                new SpuImage(),
                new Dost(),
                new SeImageHtmlIndex(),
                new BdnXml(),
                new TimedImagesXml(),
                new Wsb(),
                new JsonTypeOnlyLoad1(),
                new JsonTypeOnlyLoad2(),
                new JsonTypeOnlyLoad3(),
                new JsonTypeOnlyLoad4(),
                new JsonTypeOnlyLoad5(),
                new JsonTypeOnlyLoad6(),
                new TranscriptiveJson(),
                new KaraokeCdgCreatorText(),
                new VidIcelandic(),
                new JsonArchtime(),
                new Rdf1(),
                new CombinedXml(),
                new Fte(),
                new ClqttJson(),
            };
        }

        public static SubtitleFormat FromName(string formatName, SubtitleFormat defaultFormat)
        {
            var trimmedFormatName = formatName.Trim();
            foreach (var format in AllSubtitleFormats)
            {
                if (format.Name.Trim().Equals(trimmedFormatName, StringComparison.OrdinalIgnoreCase) ||
                    format.FriendlyName.Trim().Equals(trimmedFormatName, StringComparison.OrdinalIgnoreCase))
                {
                    return format;
                }
            }

            foreach (var format in AllSubtitleFormats)
            {
                foreach (var alternateName in format.AlternateNames)
                {
                    if (alternateName.Trim().Equals(trimmedFormatName, StringComparison.OrdinalIgnoreCase))
                    {
                        return format;
                    }
                }
            }

            return defaultFormat;
        }

        private static IList<SubtitleFormat> GetOrderedFormatsList(IEnumerable<SubtitleFormat> unorderedFormatsList)
        {
            IEnumerable<SubtitleFormat> newSelectedFormats = new[] { Utilities.GetSubtitleFormatByFriendlyName(Configuration.Settings.General.DefaultSubtitleFormat) };
            if (!string.IsNullOrEmpty(Configuration.Settings.General.FavoriteSubtitleFormats))
            {
                newSelectedFormats = newSelectedFormats.Union(Configuration.Settings.General.FavoriteSubtitleFormats.Split(';').Select(formatName => Utilities.GetSubtitleFormatByFriendlyName(formatName)));
            }

            return newSelectedFormats.Union(unorderedFormatsList).ToList();
        }
    }
}
