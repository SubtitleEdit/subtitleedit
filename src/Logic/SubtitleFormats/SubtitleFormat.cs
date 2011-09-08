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
                    new AdobeEncoreTabs(),
                    new AdobeEncoreWithLineNumbers(),
                    new AdvancedSubStationAlpha(),
                    new AQTitle(),
                    new Captionate(),
                    new CaraokeXml(),
                    new Csv(),
                    new DCSubtitle(),
                    new DigiBeta(),
                    new DvdStudioPro(),
                    new DvdStudioProSpace(),
                    new DvdSubtitle(),
                    new Eeg708(),
                    new FabSubtitler(),
                    new FinalCutProXml(),
                    new FinalCutProTestXml(),
                    new FlashXml(),
                    new MicroDvd(),
                    new MPlayer2(),
                    new OpenDvt(),
                    new PinnacleImpression(),
                    new QuickTimeText(),
                    new RealTime(),
                    new Scenarist(),
                    new SonyDVDArchitect(),
                    new SonyDVDArchitectExplicitDuration(),
                    new SonyDVDArchitectTabs(),
                    new SonyDVDArchitectWithLineNumbers(),
                    new SubStationAlpha(),
                    new SubViewer10(),
                    new SubViewer20(),
                    new Sami(),
                    new Spruce(),
                    new SpruceWithSpace(),
                    new StructuredTitles(),
                    new SubtitleEditorProject(),
                    new TimedText(),
                    new TimedText10(),
                    new TimeXml(),
                    new TranscriberXml(),
                    new TurboTitler(),
                    new TMPlayer(),
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
                    new UTSubtitleXml(),
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

        abstract public bool HasLineNumber
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

    }
}
