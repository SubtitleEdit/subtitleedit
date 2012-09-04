using System.Collections.Generic;

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
                    new AdobeEncoreLineTabs(),
                    new AdobeEncoreTabs(),
                    new AdobeEncoreWithLineNumbers(),
                    new AdobeEncoreWithLineNumbersNtsc(),
                    new AdvancedSubStationAlpha(),
                    new AQTitle(),
                    new AvidCaption(),
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
                    new GpacTtxt(),
                    new ImageLogicAutocaption(),
                    new IssXml(),
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
                    new QuickTimeText(),
                    new RealTime(),
                    new RhozetHarmonic(),
                    new Sami(),
                    new SamiModern(),
                    new Scenarist(),
                    new ScenaristClosedCaptions(),
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
                    new SwiftText(),
                    new TimedText10(),
                    new TimedText200604(),
                    new TimedText(),
                    new TimeXml(),
                    new TitleExchangePro(),
                    new TmpegEncText(),
                    new TmpegEncXml(),
                    new TMPlayer(),
                    new TranscriberXml(),
                    new TurboTitler(),
                    //    new Idx(),
                    new UleadSubtitleFormat(),
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
                    new UTSubtitleXml(),
                    new Utx(),
                    new UtxFrames(),
                    new WebVTT(),
                    new YouTubeAnnotations(),
                    new YouTubeSbv(),
                    new ZeroG(),
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

        public virtual void RemoveNativeFormatting(Subtitle subtitle)
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

        public static int FramesToMilliseconds(double frames)
        {
            return (int)System.Math.Round(frames * (1000.0 / Configuration.Settings.General.CurrentFrameRate));
        }

        public virtual bool HasStyleSupport
        {
            get
            {
                return false;
            }
        }

    }
}
