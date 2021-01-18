using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Tek : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\d+ \d+ \d \d \d$", RegexOptions.Compiled);

        public override string Extension => ".tek";

        public override string Name => "TEK";

        public override bool IsTimeBased => false;

        public override string ToText(Subtitle subtitle, string title)
        {
            //1.
            //8.03
            //10.06
            //- Labai aèiû.
            //- Jûs rimtai?

            //2.
            //16.00
            //19.06
            //Kaip reikalai ðunø grobimo versle?

            const string paragraphWriteFormat = "{0} {1} 1 1 0\r\n{2}";
            var sb = new StringBuilder();
            sb.AppendLine(@"ý Smart Titl Editor / Smart Titler  (A)(C)1992-2001. Dragutin Nikolic
ý Serial No: XXXXXXXXXXXXXX
ý Korisnik: Prava i Prevodi - prevodioci
ý
ý KONFIGURACIONI PODACI
ý Dozvoljeno slova u redu: 30
ý Vremenska korekcija:  1.0000000000E+00
ý Radjeno vremenskih korekcija: TRUE
ý Slovni raspored ASCIR
ý
ý                                       Kraj info blocka.");
            sb.AppendLine();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var text = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagFont);
                sb.AppendLine(string.Format(paragraphWriteFormat, MillisecondsToFrames(p.StartTime.TotalMilliseconds), MillisecondsToFrames(p.EndTime.TotalMilliseconds), text));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            Paragraph paragraph = null;
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (RegexTimeCode.IsMatch(s))
                {
                    if (paragraph != null)
                    {
                        subtitle.Paragraphs.Add(paragraph);
                    }

                    paragraph = new Paragraph();
                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 5)
                    {
                        try
                        {
                            paragraph.StartTime.TotalMilliseconds = FramesToMilliseconds(int.Parse(parts[0]));
                            paragraph.EndTime.TotalMilliseconds = FramesToMilliseconds(int.Parse(parts[1]));
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
                else if (paragraph != null && s.Length > 0)
                {
                    paragraph.Text = (paragraph.Text + Environment.NewLine + s).Trim();
                    if (paragraph.Text.Length > 2000)
                    {
                        _errorCount += 100;
                        return;
                    }
                }
                else if (s.Length > 0 && !s.StartsWith('ý'))
                {
                    _errorCount++;
                }
            }
            if (paragraph != null)
            {
                subtitle.Paragraphs.Add(paragraph);
            }

            subtitle.Renumber();
        }
    }
}
