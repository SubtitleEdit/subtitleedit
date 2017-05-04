using System.Globalization;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckMaxCps : INetflixQualityChecker
    {
        /// <summary>
        /// Speed - max 17 (for most languages) characters per second
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            var oldIgnoreWhiteSpace = Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace;
            try
            {
                Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace = false;
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    var charactersPerSeconds = Utilities.GetCharactersPerSecond(p);
                    if (charactersPerSeconds > controller.CharactersPerSecond)
                    {
                        var fixedParagraph = new Paragraph(p, false);
                        while (Utilities.GetCharactersPerSecond(fixedParagraph) > controller.CharactersPerSecond)
                        {
                            fixedParagraph.EndTime.TotalMilliseconds++;
                        }
                        string comment = "Maximum " + controller.CharactersPerSecond  + " characters per second";
                        controller.AddRecord(p, fixedParagraph, comment, charactersPerSeconds.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            finally
            {
                Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace = oldIgnoreWhiteSpace;
            }
        }

    }
}
