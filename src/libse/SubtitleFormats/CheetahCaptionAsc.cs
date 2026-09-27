using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Cheetah Caption text export (.asc), the text companion of the binary Cheetah .cap:
    ///
    /// ** Caption Number 1
    /// *PopOn
    /// *T 01:00:14:13
    /// *E 01:00:16:13
    /// *BottomUp
    /// *Cf16
    /// [SCREAM]\E
    ///
    /// "*E" (end) is optional - a caption without one lasts until the next caption starts
    /// (the last one gets an optimal duration).
    /// Text lines follow the "*" commands; the last one ends with "\E".
    /// </summary>
    public class CheetahCaptionAsc : SubtitleFormat
    {
        private static readonly Regex RegexCaptionNumber = new Regex(@"^\*\* ?Caption Number \d+\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RegexTimeCode = new Regex(@"^\*([TE]) +(\d\d?:\d\d:\d\d[:;]\d\d)\s*$", RegexOptions.Compiled);
        private static readonly char[] TimeCodeSplitChars = { ':', ';' };
        private const string EndOfCaption = "\\E";

        public override string Extension => ".asc";

        public override string Name => "Cheetah Caption ASC";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines == null || !lines.Exists(line => RegexCaptionNumber.IsMatch(line)))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            var frameRate = Configuration.Settings.General.CurrentFrameRate;
            if (Math.Abs(frameRate - 29.97) < 0.01 || Math.Abs(frameRate - 59.94) < 0.01)
            {
                sb.AppendLine("*DropFrame");
            }

            sb.AppendLine("*WIDTH 32");
            sb.AppendLine();
            var number = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                number++;
                sb.AppendLine("** Caption Number " + number);
                sb.AppendLine("*PopOn");
                sb.AppendLine("*T " + p.StartTime.ToHHMMSSFF());
                sb.AppendLine("*E " + p.EndTime.ToHHMMSSFF());
                sb.AppendLine("*BottomUp");
                sb.AppendLine("*Cf16");
                sb.AppendLine(HtmlUtil.RemoveHtmlTags(p.Text, true).TrimEnd() + EndOfCaption);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            Paragraph p = null;
            var hasStart = false;
            var hasEnd = false;
            var text = new StringBuilder();
            var missingEnds = new List<Paragraph>();

            void AddCaption()
            {
                if (p == null)
                {
                    return;
                }

                if (!hasStart)
                {
                    _errorCount++;
                    p = null;
                    return;
                }

                p.Text = text.ToString().TrimEnd();
                if (p.Text.EndsWith(EndOfCaption, StringComparison.Ordinal))
                {
                    p.Text = p.Text.Substring(0, p.Text.Length - EndOfCaption.Length).TrimEnd();
                }

                subtitle.Paragraphs.Add(p);
                if (!hasEnd)
                {
                    missingEnds.Add(p);
                }

                p = null;
            }

            foreach (var line in lines)
            {
                var s = line.Trim();
                if (RegexCaptionNumber.IsMatch(s))
                {
                    AddCaption();
                    p = new Paragraph();
                    hasStart = false;
                    hasEnd = false;
                    text.Clear();
                    continue;
                }

                if (p == null || s.Length == 0)
                {
                    continue;
                }

                var match = RegexTimeCode.Match(s);
                if (match.Success)
                {
                    try
                    {
                        var timeCode = DecodeTimeCodeFrames(match.Groups[2].Value, TimeCodeSplitChars);
                        if (match.Groups[1].Value == "T")
                        {
                            p.StartTime = timeCode;
                            hasStart = true;
                        }
                        else
                        {
                            p.EndTime = timeCode;
                            hasEnd = true;
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else if (s.StartsWith('*'))
                {
                    // other commands (*PopOn, *BottomUp, *Cf16, ...) - positioning is not kept
                }
                else
                {
                    if (text.Length > 0)
                    {
                        text.AppendLine();
                    }

                    text.Append(s);
                }
            }

            AddCaption();

            // a pop-on caption stays up until the next one replaces it
            foreach (var paragraph in missingEnds)
            {
                var next = subtitle.GetParagraphOrDefault(subtitle.Paragraphs.IndexOf(paragraph) + 1);
                var gap = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                paragraph.EndTime.TotalMilliseconds = next != null && next.StartTime.TotalMilliseconds - gap > paragraph.StartTime.TotalMilliseconds
                    ? next.StartTime.TotalMilliseconds - gap
                    : paragraph.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(paragraph.Text);
            }

            subtitle.Renumber();
        }
    }
}
