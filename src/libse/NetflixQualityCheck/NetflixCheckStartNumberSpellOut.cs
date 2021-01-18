using Nikse.SubtitleEdit.Core.Common;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    /// <summary>
    /// When a number begins a sentence, it should always be spelled out.
    /// </summary>
    public class NetflixCheckStartNumberSpellOut : INetflixQualityChecker
    {
        private static readonly Regex NumberStart = new Regex(@"^\d+ [A-Za-z]", RegexOptions.Compiled);
        private static readonly Regex NumberStartInside = new Regex(@"[\.,!] \d+ [A-Za-z]", RegexOptions.Compiled);
        private static readonly Regex NumberStartInside2 = new Regex(@"[\.,!]\r\n\d+ [A-Za-z]", RegexOptions.Compiled);

        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string newText = p.Text;

                var m = NumberStart.Match(newText);
                while (m.Success)
                {
                    int length = m.Length - 2;
                    newText = newText.Remove(m.Index, length).Insert(m.Index, NetflixHelper.ConvertNumberToString(m.Value.Substring(0, length), true, controller.Language));
                    m = NumberStart.Match(newText, m.Index + 1);
                }

                m = NumberStartInside.Match(newText);
                while (m.Success)
                {
                    int length = m.Length - 4;
                    newText = newText.Remove(m.Index + 2, length).Insert(m.Index + 2, NetflixHelper.ConvertNumberToString(m.Value.Substring(2, length), true, controller.Language));
                    m = NumberStartInside.Match(newText, m.Index + 1);
                }

                m = NumberStartInside2.Match(newText);
                while (m.Success)
                {
                    int length = m.Length - 5;
                    newText = newText.Remove(m.Index + 3, length).Insert(m.Index + 3, NetflixHelper.ConvertNumberToString(m.Value.Substring(3, length), true, controller.Language));
                    m = NumberStartInside2.Match(newText, m.Index + 1);
                }

                if (newText != p.Text)
                {
                    var fixedParagraph = new Paragraph(p, false) { Text = newText };
                    string comment = "When a number begins a sentence, it should always be spelled out";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }

    }
}
