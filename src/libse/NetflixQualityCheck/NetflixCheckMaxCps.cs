using System.Globalization;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckMaxCps : INetflixQualityChecker
    {
        private bool _isChildrenProgram;

        public NetflixCheckMaxCps(bool isChildrenProgram = false)
        {
            _isChildrenProgram = isChildrenProgram;
        }

        /// <summary>
        /// Speed - max 20 (for most languages) characters per second
        /// for children programs, it's the normal minus 3
        /// </summary>
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            var oldIgnoreWhiteSpace = Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace;
            try
            {
                Configuration.Settings.General.CharactersPerSecondsIgnoreWhiteSpace = false;

                int charactersPerSecond = _isChildrenProgram ? controller.CharactersPerSecond - 3 : controller.CharactersPerSecond;
                string comment = "Maximum " + charactersPerSecond + " characters per second";
                
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    var jp = new Paragraph(p);
                    if (controller.Language == "ja")
                    {
                        jp.Text = HtmlUtil.RemoveHtmlTags(jp.Text, true);
                        jp.Text = NetflixImsc11Japanese.RemoveTags(jp.Text);
                    }
                    var charactersPerSeconds = Utilities.GetCharactersPerSecond(jp);
                    if (charactersPerSeconds > charactersPerSecond && !p.StartTime.IsMaxTime)
                    {
                        var fixedParagraph = new Paragraph(p, false);
                        while (Utilities.GetCharactersPerSecond(fixedParagraph) > charactersPerSecond)
                        {
                            fixedParagraph.EndTime.TotalMilliseconds++;
                        }
                        
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
