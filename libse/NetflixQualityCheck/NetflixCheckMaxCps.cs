using System;
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
            // constant
            string comment = "Maximum " + controller.CharactersPerSecond + " characters per second";
            var oldIgnoreWhiteSpace = Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace;
            try
            {
                Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace = false;
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    int textLenExludingNonDisplayChars = GetTextLengthExcludingNonDisplayChars(p.Text);
                    double charactersPerSeconds = textLenExludingNonDisplayChars / p.Duration.TotalSeconds;
                    if (charactersPerSeconds > controller.CharactersPerSecond)
                    {
                        var fixedParagraph = new Paragraph(p, false);
                        double optimalDuration = textLenExludingNonDisplayChars * (1D / controller.CharactersPerSecond) * TimeCode.BaseUnit;
                        fixedParagraph.EndTime.TotalMilliseconds = fixedParagraph.StartTime.TotalMilliseconds + Math.Round(optimalDuration);
                        controller.AddRecord(p, fixedParagraph, comment, charactersPerSeconds.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            finally
            {
                Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace = oldIgnoreWhiteSpace;
            }
        }


        public static int GetTextLengthExcludingNonDisplayChars(string text)
        {
            int len = text.Length;
            // <i>
            if (len >= 3)
            {
                text = HtmlUtil.RemoveHtmlTags(text, true);
            }
            const char ZeroWidthSpace = '\u200B';
            const char ZeroWidthNoBreakSpace = '\uFEFF';
            for (int i = len - 1; i >= 0; i--)
            {
                char ch = text[i];
                switch (ch)
                {
                    case ' ':
                        if (Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace)
                        {
                            len--;
                        }
                        break;
                    case '\r':
                    case '\n':
                    case ZeroWidthSpace:
                    case ZeroWidthNoBreakSpace:
                        len--;
                        break;
                }
            }
            return len;
        }

    }
}
