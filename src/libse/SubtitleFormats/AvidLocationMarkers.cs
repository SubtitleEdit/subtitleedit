using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AvidLocationMarkers : SubtitleFormat
    {
        private static readonly Regex RegexTimeCode = new Regex(@"^\t\d\d:\d\d:\d\d:\d\d\t[A-Z]\d\t[a-z]{3,8}\t", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Avid Loc Markers";

        private static string MakeTimeCode(TimeCode tc)
        {
            return tc.ToHHMMSSFF();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            var count = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                var text = Utilities.UnbreakLine(p.Text);
                text = HtmlUtil.RemoveHtmlTags(text, true);
                if (count == 0)
                {
                    sb.AppendLine($"\t{MakeTimeCode(p.StartTime)}\tstart");
                }

                sb.AppendLine($"\t{MakeTimeCode(p.StartTime)}\tA1\tblack\t{text}");
                count++;
            }

            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (var line in lines)
            {
                var s = line.TrimEnd();
                var match = RegexTimeCode.Match(s);
                if (match.Success)
                {
                    try
                    {
                        var text = s.Remove(0, match.Length).Trim();
                        text = Utilities.AutoBreakLine(text);
                        var arr = match.Value.Split('\t');
                        if (arr.Length== 5 && text.Length > 0)
                        {
                            if (string.IsNullOrWhiteSpace(text
                                .RemoveChar('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ':', ',')))
                            {
                                _errorCount++;
                            }

                            char[] splitChars = { ',', '.', ':' };
                            var p = new Paragraph(DecodeTimeCodeFrames(arr[1], splitChars), DecodeTimeCodeFrames(arr[1], splitChars), text);
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
            }

            new FixShortDisplayTimes().Fix(subtitle, new EmptyFixCallback());
            subtitle.Renumber();
        }
    }
}
