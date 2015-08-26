namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TimedText200604CData : TimedText200604
    {

        public TimedText200604CData()
        {
            UseCDataForParagraphText = true;
        }

        public override string Name
        {
            get { return "Timed Text draft 2006-04 CDATA"; }
        }

    }
}
