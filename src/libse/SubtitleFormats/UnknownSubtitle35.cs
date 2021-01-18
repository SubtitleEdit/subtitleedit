using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle35 : SubtitleFormat
    {

        //0072.08-0076.05
        //Pidin täna peaaegu surma saama,
        //kuna röövisid vale koera.

        //0076.09-0078.14
        //Mõtled seda tõsiselt või?

        private static readonly Regex RegexTimeCode = new Regex(@"^\d\d\d\d\.\d\d-\d\d\d\d\.\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 35";

        private static string MakeTimeCode(TimeCode tc)
        {
            return $"{(int)tc.TotalSeconds:0000}.{MillisecondsToFramesMaxFrameRate(tc.Milliseconds):00}";
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                sb.AppendLine($"{MakeTimeCode(p.StartTime)}-{MakeTimeCode(p.EndTime)}\r\n{text}\r\n");
            }
            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            var sb = new StringBuilder();
            char[] splitChars = { ':', ';', ',', '.' };
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (RegexTimeCode.IsMatch(s))
                {
                    try
                    {
                        if (p != null)
                        {
                            p.Text = sb.ToString().Trim();
                            subtitle.Paragraphs.Add(p);
                        }
                        sb.Clear();
                        string[] arr = s.Split('-');
                        if (arr.Length == 2)
                        {
                            var parts1 = arr[0].Split(splitChars);
                            var parts2 = arr[1].Split(splitChars);

                            // parts1/2 most have length of 2
                            if (parts1.Length + parts2.Length == 4)
                            {
                                p = new Paragraph(DecodeTimeCodeFramesTwoParts(parts1), DecodeTimeCodeFramesTwoParts(parts2), string.Empty);
                            }
                            else
                            {
                                p = new Paragraph(DecodeTimeCodeFramesTwoParts(new[] { parts1[0], parts1[1] }), DecodeTimeCodeFramesTwoParts(new[] { parts2[0], parts2[1] }), string.Empty);
                            }
                        }
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(s))
                {
                    sb.AppendLine(s);
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
        }

    }
}
