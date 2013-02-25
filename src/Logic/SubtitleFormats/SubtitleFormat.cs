using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public abstract class SubtitleFormat
    {

        /// <summary>
        /// Formats supported by Subtitle Edit
        /// </summary>
        public static IList<SubtitleFormat> AllSubtitleFormats
        {
            get
            {
                return new List<SubtitleFormat>
                {
                    new SubRip(),
                    new AbcIViewer(),
                    new AdobeEncore(),
                    new AdobeEncoreLineTabNewLine(),
                    new AdobeEncoreLineTabs(),
                    new AdobeEncoreTabs(),
                    new AdobeEncoreWithLineNumbers(),
                    new AdobeEncoreWithLineNumbersNtsc(),
                    new AdvancedSubStationAlpha(),
                    new AQTitle(),
                    new AvidCaption(),
                    new AvidDvd(),
                    new BelleNuitSubtitler(),
                    new Captionate(),
                    new CaptionateMs(),
                    new CaraokeXml(),
                    new Csv(),
                    new DCSubtitle(),
                    new DCinemaSmpte(),
                    new DigiBeta(),
                    new DvdStudioPro(),
                    new DvdStudioProSpace(),
                    new DvdSubtitle(),
                    new Eeg708(),
                    new F4Text(),
                    new F4Rtf(),
                    new F4Xml(),
                    new FabSubtitler(),
                    new FinalCutProXml(),
                    new FinalCutProXXml(),
                    new FinalCutProTestXml(),
                    new FlashXml(),
                    new Footage(),
                    new GpacTtxt(),
                    new ImageLogicAutocaption(),
                    new IssXml(),
                    new ItunesTimedText(),
                    new Json(),
                    new JsonType2(),
                    new JsonType3(),
                    new MicroDvd(),
                    new MidwayInscriberCGX(),
                    new MPlayer2(),
                    new OpenDvt(),
                    new PE2(),
                    new PinnacleImpression(),
                    new PListCaption(),
                    new QubeMasterImport(),
                    new QuickTimeText(),
                    new RealTime(),
                    new RhozetHarmonic(),
                    new Sami(),
                    new SamiModern(),
                    new Scenarist(),
                    new ScenaristClosedCaptions(),
                    new SmilTimesheetData(),
                    new SonyDVDArchitect(),
                    new SonyDVDArchitectExplicitDuration(),
                    new SonyDVDArchitectLineAndDuration(),
                    new SonyDVDArchitectTabs(),
                    new SonyDVDArchitectWithLineNumbers(),
                    new SoftNiSub(),
                    new Spruce(),
                    new SpruceWithSpace(),
                    new StructuredTitles(),
                    new SubStationAlpha(),
                    new SubtitleEditorProject(),
                    new SubViewer10(),
                    new SubViewer20(),
                    new SwiftInterchange2(),
                    new SwiftText(),
                    new Tek(),
                    new TimeXml(),
                    new TimedText10(),
                    new TimedText200604(),
                    new TimedText(),
                    new TitleExchangePro(),
                    new TmpegEncText(),
                    new TmpegEncAW5(),
                    new TmpegEncXml(),
                    new TMPlayer(),
                    new TranscriberXml(),
                    new Tmx14(),
                    new TurboTitler(),
                    new UTSubtitleXml(),
                    new Utx(),
                    new UtxFrames(),
                    new UleadSubtitleFormat(),
                    new WebVTT(),
                    new YouTubeAnnotations(),
                    new YouTubeSbv(),
                    new YouTubeTranscript(),
                    new ZeroG(),

                    //    new Idx(),
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
                };
            }
        }

        protected int _errorCount;

        abstract public string Extension
        {
            get;
        }

        abstract public string Name
        {
            get;
        }

        abstract public bool IsTimeBased
        {
            get;
        }

        public bool IsFrameBased
        {
            get
            {
                return !IsTimeBased;
            }
        }

        public string FriendlyName
        {
            get
            {
                return string.Format("{0} ({1})", Name, Extension);
            }
        }

        public int ErrorCount
        {
            get { return _errorCount; }
        }

        abstract public bool IsMine(List<string> lines, string fileName);

        abstract public string ToText(Subtitle subtitle, string title);

        abstract public void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName);

        public bool IsVobSubIndexFile
        {
            get { return Extension == new Idx().Extension; }
        }

        public virtual void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
        }

        public virtual List<string> AlternateExtensions
        {
            get
            {
                return new List<string>();
            }
        }

        public static int MillisecondsToFrames(double milliseconds)
        {
            return (int)System.Math.Round(milliseconds / (1000.0 / Configuration.Settings.General.CurrentFrameRate));
        }

        public static int MillisecondsToFramesMaxFrameRate(double milliseconds)
        {
            int frames = (int)System.Math.Round(milliseconds / (1000.0 / Configuration.Settings.General.CurrentFrameRate));
            if (frames >= Configuration.Settings.General.CurrentFrameRate)
                frames = (int)(Configuration.Settings.General.CurrentFrameRate - 0.01);
            return frames;
        }

        public static int FramesToMilliseconds(double frames)
        {
            return (int)System.Math.Round(frames * (1000.0 / Configuration.Settings.General.CurrentFrameRate));
        }

        public static int FramesToMillisecondsMax999(double frames)
        {
            int ms = (int)System.Math.Round(frames * (1000.0 / Configuration.Settings.General.CurrentFrameRate));
            if (ms > 999)
                ms = 999;
            return ms;
        }

        public virtual bool HasStyleSupport
        {
            get
            {
                return false;
            }
        }

        public static string ToUtf8XmlString(XmlDocument xml, bool omitXmlDeclaration)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UnicodeEncoding(false, false); // no BOM in a .NET string
            settings.Indent = true;
            settings.OmitXmlDeclaration = omitXmlDeclaration;

            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    xml.Save(xmlWriter);
                }
                return textWriter.ToString().Replace(" encoding=\"utf-16\"", " encoding=\"utf-8\"").Trim();
            }
        }

        public static string ToUtf8XmlString(XmlDocument xml)
        {
            return ToUtf8XmlString(xml, false);
        }

    }
}
