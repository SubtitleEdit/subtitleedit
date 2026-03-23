namespace Nikse.SubtitleEdit.Core.Common
{
    public class ActorConverterResult
    {
        public Paragraph Paragraph { get; set; }
        public Paragraph NextParagraph { get; set; }
        public bool Selected { get; set; }
        public bool Skip { get; internal set; }
    }
}
