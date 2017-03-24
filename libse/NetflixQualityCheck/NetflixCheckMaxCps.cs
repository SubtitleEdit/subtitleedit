namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckMaxCps : INetflixQualityChecker
    {
        /// <summary>
        /// Speed - max 17 characters per second
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            var oldIgnoreWhiteSpace = Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace;
            try
            {
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    var charactersPerSeconds = Utilities.GetCharactersPerSecond(p);
                    if (charactersPerSeconds > 17)
                    {
                        var fixedParagraph = new Paragraph(p, false);
                        while (Utilities.GetCharactersPerSecond(fixedParagraph) > 17)
                        {
                            fixedParagraph.EndTime.TotalMilliseconds++;
                        }
                        string comment = "Maximum 17 characters per second";
                        controller.AddRecord(p, fixedParagraph, comment);
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
