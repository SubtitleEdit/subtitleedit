using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public abstract class SubtitleFormat
    {
        public static bool SubtitleFormatsOrderChanged { get; set; }

        private static IList<SubtitleFormat> _allSubtitleFormats;
        private static IList<SubtitleFormat> _subtitleFormatsWithDefaultOrder;

        protected static readonly char[] SplitCharColon = { ':' };

        /// <summary>
        /// Text formats supported by Subtitle Edit
        /// </summary>
        public static IEnumerable<SubtitleFormat> AllSubtitleFormats
        {
            get
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
                    new AdobeEncoreTabs(),
                    new AdobeEncoreWithLineNumbers(),
                    new AdobeEncoreWithLineNumbersNtsc(),
                    new AdvancedSubStationAlpha(),
                    new AQTitle(),
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
                    new Csv(),
                    new Csv2(),
                    new Csv3(),
                    new Csv4(),
                    new Csv5(),
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
                    new Ebu(),
                    new Edius4Frames(),
                    new Edius4Ms(),
                    new EdiusMarkerList2Frames(),
                    new EdiusMarkerList2Ms(),
                    new EdiusMarkerList3Frames(),
                    new EdiusMarkerList3Ms(),
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
                    new FinalCutProXml15(),
                    new FinalCutProXml16(),
                    new FinalCutProXml17(),
                    new FinalCutProXml18(),
                    new FinalCutProXml19(),
                    new FinalCutProXml110(),
                    new FinalCutProXml111(),
                    new FinalCutProTestXml(),
                    new FinalCutProTest2Xml(),
                    new FlashXml(),
                    new FLVCoreCuePoints(),
                    new Footage(),
                    new GooglePlayJson(),
                    new GpacTtxt(),
                    new Gremots(),
                    new HollyStarJson(),
                    new ImageLogicAutocaption(),
                    new InqScribe(),
                    new IssXml(),
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
                    new KanopyHtml(),
                    new LambdaCap(),
                    new Lrc(),
                    new Lrc3DigitsMs(),
                    new LrcNoEndTime(),
                    new MacSub(),
                    new MagicVideoTitler(),
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
                    new UnknownSubtitle82(),
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
                };

                foreach (var pluginFileName in Configuration.GetPlugins())
                {
                    try
                    {
                        var assembly = System.Reflection.Assembly.Load(FileUtil.ReadAllBytesShared(pluginFileName));
                        foreach (var exportedType in assembly.GetExportedTypes())
                        {
                            try
                            {
                                var pluginObject = Activator.CreateInstance(exportedType);
                                if (pluginObject is SubtitleFormat po)
                                {
                                    _allSubtitleFormats.Insert(1, po);
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                _subtitleFormatsWithDefaultOrder = new List<SubtitleFormat>(_allSubtitleFormats);
                _allSubtitleFormats = GetOrderedFormatsList(_subtitleFormatsWithDefaultOrder);

                return _allSubtitleFormats;
            }
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

        public virtual bool IsTimeBased => true;

        public bool IsFrameBased => !IsTimeBased;

        public string FriendlyName => $"{Name} ({Extension})";

        public int ErrorCount => _errorCount;

        public virtual bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
            LoadSubtitle(subtitle, lines, fileName);
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public abstract string ToText(Subtitle subtitle, string title);

        public abstract void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName);

        public bool IsVobSubIndexFile => Extension.Equals(".idx", StringComparison.Ordinal);

        public virtual void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
        }

        public virtual List<string> AlternateExtensions => new List<string>();

        public static int MillisecondsToFrames(double milliseconds)
        {
            return MillisecondsToFrames(milliseconds, Configuration.Settings.General.CurrentFrameRate);
        }

        public static int MillisecondsToFrames(double milliseconds, double frameRate)
        {
            return (int)Math.Round(milliseconds / (TimeCode.BaseUnit / GetFrameForCalculation(frameRate)), MidpointRounding.AwayFromZero);
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
            var frames = (int)Math.Round(milliseconds / (TimeCode.BaseUnit / GetFrameForCalculation(Configuration.Settings.General.CurrentFrameRate)), MidpointRounding.AwayFromZero);
            if (frames >= Configuration.Settings.General.CurrentFrameRate)
            {
                frames = (int)(Configuration.Settings.General.CurrentFrameRate - 0.01);
            }

            return frames;
        }

        public static int FramesToMilliseconds(double frames)
        {
            return FramesToMilliseconds(frames, Configuration.Settings.General.CurrentFrameRate);
        }

        public static int FramesToMilliseconds(double frames, double frameRate)
        {
            return (int)Math.Round(frames * (TimeCode.BaseUnit / GetFrameForCalculation(frameRate)), MidpointRounding.AwayFromZero);
        }

        public static int FramesToMillisecondsMax999(double frames)
        {
            var ms = (int)Math.Round(frames * (TimeCode.BaseUnit / GetFrameForCalculation(Configuration.Settings.General.CurrentFrameRate)), MidpointRounding.AwayFromZero);
            return Math.Min(ms, 999);
        }

        public virtual bool HasStyleSupport => false;

        public bool BatchMode { get; set; }
        public double? BatchSourceFrameRate { get; set; }

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

            return new TimeCode(int.Parse(tokens[0]), int.Parse(tokens[1]), int.Parse(tokens[2]), FramesToMillisecondsMax999(int.Parse(tokens[3])));
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

        /// <summary>
        /// Load subtitle type of 'formats' from file.
        /// </summary>
        /// <param name="formats">List of possible formats</param>
        /// <param name="fileName">Name of subtitle file</param>
        /// <param name="subtitle">Subtitle to load file into</param>
        /// <returns>The format of the file, null of not format match found</returns>
        public static SubtitleFormat LoadSubtitleFromFile(SubtitleFormat[] formats, string fileName, Subtitle subtitle)
        {
            if (formats == null || formats.Length == 0 || string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var list = new List<string>(File.ReadAllLines(fileName, LanguageAutoDetect.GetEncodingFromFile(fileName)));
            foreach (var subtitleFormat in formats)
            {
                if (subtitleFormat.IsMine(list, fileName))
                {
                    subtitleFormat.LoadSubtitle(subtitle, list, fileName);
                    return subtitleFormat;
                }
            }
            return null;
        }

        /// <summary>
        /// Load subtitle from a list of lines and a file name (the last can be null).
        /// </summary>
        /// <param name="lines">Text lines from subtitle file</param>
        /// <param name="fileName">Optional file name</param>
        /// <returns>Subtitle, null if format not recognized</returns>
        public static Subtitle LoadSubtitleFromLines(List<string> lines, string fileName)
        {
            if (lines == null || lines.Count == 0)
            {
                return null;
            }

            var subtitle = new Subtitle();
            foreach (var subtitleFormat in AllSubtitleFormats)
            {
                if (subtitleFormat.IsMine(lines, fileName))
                {
                    subtitleFormat.LoadSubtitle(subtitle, lines, fileName);
                    return subtitle;
                }
            }

            return null;
        }

        public static SubtitleFormat[] GetBinaryFormats(bool batchMode)
        {
            return new SubtitleFormat[]
            {
                new Ebu { BatchMode = batchMode },
                new Pac { BatchMode = batchMode },
                new PacUnicode(),
                new Cavena890 { BatchMode = batchMode },
                new Spt(),
                new CheetahCaption(),
                new CheetahCaptionOld(),
                new TSB4(),
                new Chk(),
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
                new Wsb(),
                new JsonTypeOnlyLoad1(),
                new JsonTypeOnlyLoad2(),
                new JsonTypeOnlyLoad3(),
                new JsonTypeOnlyLoad4(),
                new TranscriptiveJson(),
                new KaraokeCdgCreatorText(),
                new VidIcelandic(),
                new JsonArchtime(),
                new MacCaption10(),
                new Rdf1(),
                new CombinedXml(),
                new AudacityLabels(),
                new Fte(),
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
