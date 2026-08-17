using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class RealTime : SubtitleFormat
    {
        // Cue starts are <time .../> tags; the text a cue shows is everything up to the next
        // <time .../> tag. Real-world files (RealPlayer captions) use lowercase tags, usually
        // only a begin= attribute, and timestamps in every short form the RealText spec allows
        // ("3", "3.5", ".5", "1:20", "1:20s", "0:03:24.8"), so parse permissively.
        private static readonly Regex RegexTimeTag = new Regex("<time\\s[^>]*begin\\s*=\\s*\"[^\"]*\"[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RegexBeginAttribute = new Regex("begin\\s*=\\s*\"([^\"]*)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RegexEndAttribute = new Regex("end\\s*=\\s*\"([^\"]*)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RegexWindowTag = new Regex("<window[\\s>]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RegexDurationAttribute = new Regex("duration\\s*=\\s*\"([^\"]*)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RegexLineBreakTag = new Regex("<(?:br|p|/p|clear)\\s*/?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RegexTag = new Regex("<[^>]+>", RegexOptions.Compiled);

        public override string Extension => ".rt";

        public override string Name => "RealTime";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<Window" + Environment.NewLine +
                "  Width    = \"640\"" + Environment.NewLine +
                "  Height   = \"480\"" + Environment.NewLine +
                "  WordWrap = \"true\"" + Environment.NewLine +
                "  Loop     = \"true\"" + Environment.NewLine +
                "  bgcolor  = \"black\"" + Environment.NewLine +
                ">" + Environment.NewLine +
                "<Font" + Environment.NewLine +
                "  Color = \"white\"" + Environment.NewLine +
                "  Face  = \"Arial\"" + Environment.NewLine +
                "  Size  = \"+2\"" + Environment.NewLine +
                ">" + Environment.NewLine +
                "<center>" + Environment.NewLine +
                "<b>" + Environment.NewLine);
            const string writeFormat = "<Time begin=\"{0}\" end=\"{1}\" /><clear/>{2}";
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //<Time begin="0:03:24.8" end="0:03:29.4" /><clear/>Man stjæler ikke fra Chavo, nej.
                sb.AppendLine(string.Format(writeFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), p.Text.Replace(Environment.NewLine, " ")));
            }
            sb.AppendLine("</b>");
            sb.AppendLine("</center>");
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //0:03:24.8
            return $"{time.Hours:0}:{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds / 100:0}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            _errorCount = 0;

            var text = string.Join("\n", lines);
            var matches = RegexTimeTag.Matches(text);
            if (matches.Count == 0)
            {
                // a static caption: one <window duration="..."> with text and no <time> tags
                var windowMatch = RegexWindowTag.Match(text);
                var windowTagEnd = windowMatch.Success ? text.IndexOf('>', windowMatch.Index) : -1;
                if (windowTagEnd >= 0)
                {
                    var staticText = ExtractText(text.Substring(windowTagEnd + 1));
                    if (staticText.Length > 0)
                    {
                        var durationMatch = RegexDurationAttribute.Match(text.Substring(windowMatch.Index, windowTagEnd - windowMatch.Index + 1));
                        var end = durationMatch.Success && TryParseTimeCode(durationMatch.Groups[1].Value, out var duration) && duration > 0
                            ? duration
                            : Utilities.GetOptimalDisplayMilliseconds(staticText);
                        subtitle.Paragraphs.Add(new Paragraph(staticText, 0, end));
                        subtitle.Renumber();
                    }
                }

                return;
            }
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var beginMatch = RegexBeginAttribute.Match(match.Value);
                if (!TryParseTimeCode(beginMatch.Groups[1].Value, out var startMilliseconds))
                {
                    _errorCount++;
                    continue;
                }

                var textEndIndex = i + 1 < matches.Count ? matches[i + 1].Index : text.Length;
                var cueText = ExtractText(text.Substring(match.Index + match.Length, textEndIndex - match.Index - match.Length));
                if (cueText.Length == 0)
                {
                    continue;
                }

                double endMilliseconds;
                var endMatch = RegexEndAttribute.Match(match.Value);
                if (endMatch.Success && TryParseTimeCode(endMatch.Groups[1].Value, out var explicitEnd) && explicitEnd > startMilliseconds)
                {
                    endMilliseconds = explicitEnd;
                }
                else
                {
                    // no end= - the text stays until the next <time> tag replaces/clears it
                    endMilliseconds = startMilliseconds + Utilities.GetOptimalDisplayMilliseconds(cueText);
                    if (i + 1 < matches.Count)
                    {
                        var nextBeginMatch = RegexBeginAttribute.Match(matches[i + 1].Value);
                        if (TryParseTimeCode(nextBeginMatch.Groups[1].Value, out var nextStart) &&
                            nextStart > startMilliseconds &&
                            nextStart - startMilliseconds <= Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                        {
                            endMilliseconds = nextStart;
                        }
                    }
                }

                subtitle.Paragraphs.Add(new Paragraph(cueText, startMilliseconds, endMilliseconds));
            }

            // text between the <window> element and the first <time> tag is shown from the start
            if (matches.Count > 0)
            {
                var introText = ExtractText(text.Substring(0, matches[0].Index));
                if (introText.Length > 0)
                {
                    var firstBegin = RegexBeginAttribute.Match(matches[0].Value);
                    var end = TryParseTimeCode(firstBegin.Groups[1].Value, out var firstStart) && firstStart > 0
                        ? Math.Min(firstStart, Utilities.GetOptimalDisplayMilliseconds(introText))
                        : Utilities.GetOptimalDisplayMilliseconds(introText);
                    subtitle.Paragraphs.Insert(0, new Paragraph(introText, 0, end));
                }
            }

            subtitle.Sort(SubtitleSortCriteria.StartTime);
            subtitle.Renumber();
        }

        private static string ExtractText(string s)
        {
            s = RegexLineBreakTag.Replace(s, "\n");
            s = RegexTag.Replace(s, string.Empty);
            s = System.Net.WebUtility.HtmlDecode(s);

            var sb = new StringBuilder(s.Length);
            foreach (var line in s.SplitToLines())
            {
                var trimmed = line.Trim();
                if (trimmed.Length > 0)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(Environment.NewLine);
                    }

                    sb.Append(trimmed);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Parses a RealText timestamp: "dd:hh:mm:ss.xy" where all leading components and the
        /// fraction are optional ("3", "3.5", ".5", "1:20", "0:03:24.8"), plus an optional
        /// trailing unit as seen in the wild ("1:20s").
        /// </summary>
        private static bool TryParseTimeCode(string s, out double milliseconds)
        {
            milliseconds = 0;
            s = s.Trim().TrimEnd('s', 'S');
            if (s.Length == 0)
            {
                return false;
            }

            var parts = s.Split(':');
            if (parts.Length > 4)
            {
                return false;
            }

            double factor = 1000; // seconds
            double total = 0;
            for (var i = parts.Length - 1; i >= 0; i--)
            {
                var part = parts[i].Trim();
                if (part.Length == 0)
                {
                    part = "0";
                }

                if (!double.TryParse(part, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value) || value < 0)
                {
                    return false;
                }

                total += value * factor;
                factor *= i >= parts.Length - 3 ? 60 : 24; // s -> min -> hours -> days
            }

            milliseconds = total;
            return true;
        }
    }
}
