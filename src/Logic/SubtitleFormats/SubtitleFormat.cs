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
                    new AdobeEncore(),
                    new AdobeEncoreTab(),
                    new DvdStudioPro(),
                    new DvdSubtitle(),
//                    new Ebu(),
                    new FinalCutProXml(),
                    new MicroDvd(),
                    new MPlayer2(),
                    new OpenDvt(),
                    new PinnacleImpression(),
                    new QuickTimeText(),
                    new RealTime(),
                    new Scenarist(),
                    new SonyDVDArchitect(),
                    new SonyDVDArchitectWithLineNumbers(),
                    new SubStationAlpha(),
                    new SubViewer10(),
                    new SubViewer20(),
                    new Sami(),
                    new Spruce(),
                    new SubtitleEditorProject(),
                    new TimedText(),
                    new TMPlayer(),
                    new YouTubeSbv(),
                //    new Idx(),
                    new UleadSubtitleFormat(),
                    new UnknownSubtitle1(),
                    new UnknownSubtitle2(),
                    new UnknownSubtitle3(),
                    new UnknownSubtitle4(),
                    new UnknownSubtitle5(),
                    new AbcIViewer(),
                    new Csv(),
                    new TimeXml(),
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
