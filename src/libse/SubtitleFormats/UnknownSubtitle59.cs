using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle59 : SubtitleFormat
    {
        public static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\:\d\d\:\d\d\t.+\t\d\d\:\d\d\:\d\d$", RegexOptions.Compiled);
        public static readonly Regex RegexTimeCodes2 = new Regex(@"^\d\d\:\d\d\:\d\d.+\d\d\:\d\d\:\d\d$", RegexOptions.Compiled);
        private static readonly Regex RegexStartOnly = new Regex(@"^\d\d\:\d\d\:\d\d\t.+$", RegexOptions.Compiled);
        private static readonly Regex RegexEndOnly = new Regex(@"\d\d\:\d\d\:\d\d$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 59";

        public override string ToText(Subtitle subtitle, string title)
        {
            //00:06:12  Would you like to see any particular style? 00:06:13
            //
            //00:35:46  I made coffee. Would you like some?             00:35:47
            //Yes.
            //

            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                var lines = HtmlUtil.RemoveHtmlTags(p.Text).SplitToLines();
                // the end time is part of the format (see the header comment and RegexTimeCodes);
                // without it every cue reloaded with EndTime 0
                sb.AppendLine(EncodeTimeCode(p.StartTime) + "\t" + lines[0].Trim() + "\t" + EncodeTimeCode(p.EndTime));
                for (int i = 1; i < lines.Count; i++)
                {
                    sb.AppendLine("\t" + lines[i].Trim());
                }
            }

            return sb.ToString().Trim();
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            // round to whole seconds on the total so 59.6s carries into the minute instead of writing ":60"
            var rounded = new TimeCode(Math.Round(timeCode.TotalMilliseconds / 1000.0) * 1000.0);
            return $"{rounded.Hours:00}:{rounded.Minutes:00}:{rounded.Seconds:00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (s.Length > 7 && char.IsDigit(s[0]) && char.IsDigit(s[1]) && s[2] == ':')
                {
                    if (RegexTimeCodes.IsMatch(s) || RegexTimeCodes2.IsMatch(s))
                    {
                        // RegexTimeCodes2 is a superset of RegexTimeCodes, so every well-formed line counted
                        // as an error and the format could never be detected (errors == cues).
                        if (!RegexTimeCodes.IsMatch(s) && RegexTimeCodes2.IsMatch(s))
                        {
                            _errorCount++;
                        }

                        try
                        {
                            p = new Paragraph();
                            string[] start = s.Substring(0, 8).Split(':');
                            string[] end = s.Remove(0, s.Length - 8).Split(':');
                            if (start.Length == 3)
                            {
                                int hours = int.Parse(start[0]);
                                int minutes = int.Parse(start[1]);
                                int seconds = int.Parse(start[2]);
                                p.StartTime = new TimeCode(hours, minutes, seconds, 0);

                                hours = int.Parse(end[0]);
                                minutes = int.Parse(end[1]);
                                seconds = int.Parse(end[2]);
                                p.EndTime = new TimeCode(hours, minutes, seconds, 0);

                                string text = s.Remove(0, 8).Trim();
                                text = text.Substring(0, text.Length - 8).Trim();
                                p.Text = text;
                                if (text.Length > 1 && Utilities.IsInteger(text.Substring(0, 2)))
                                {
                                    _errorCount++;
                                }

                                subtitle.Paragraphs.Add(p);
                            }
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                    else if (RegexStartOnly.IsMatch(s))
                    {
                        try
                        {
                            p = new Paragraph();
                            string[] start = s.Substring(0, 8).Split(':');
                            if (start.Length == 3)
                            {
                                int hours = int.Parse(start[0]);
                                int minutes = int.Parse(start[1]);
                                int seconds = int.Parse(start[2]);
                                p.StartTime = new TimeCode(hours, minutes, seconds, 0);

                                string text = s.Remove(0, 8).Trim();
                                p.Text = text;
                                if (text.Length > 1 && Utilities.IsInteger(text.Substring(0, 2)))
                                {
                                    _errorCount++;
                                }

                                subtitle.Paragraphs.Add(p);
                            }
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                    else if (RegexEndOnly.IsMatch(s))
                    {
                        try
                        {
                            string[] end = s.Remove(0, s.Length - 8).Split(':');
                            if (end.Length == 3 && p != null)
                            {
                                int hours = int.Parse(end[0]);
                                int minutes = int.Parse(end[1]);
                                int seconds = int.Parse(end[2]);
                                p.EndTime = new TimeCode(hours, minutes, seconds, 0);

                                string text = s.Substring(0, s.Length - 8).Trim();
                                p.Text = p.Text + Environment.NewLine + text;
                                if (text.Length > 1 && Utilities.IsInteger(text.Substring(0, 2)))
                                {
                                    _errorCount++;
                                }

                                p = null;
                            }
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                    else if (!Utilities.IsInteger(s))
                    {
                        if (p != null && !p.Text.Contains(Environment.NewLine))
                        {
                            p.Text = p.Text + Environment.NewLine + s.Trim();
                        }
                        else
                        {
                            _errorCount++;
                        }
                    }
                }
                else if (s.Length > 0 && !Utilities.IsInteger(s))
                {
                    if (p != null && !p.Text.Contains(Environment.NewLine))
                    {
                        p.Text = p.Text + Environment.NewLine + s.Trim();
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }

            foreach (var p2 in subtitle.Paragraphs)
            {
                if (p2.DurationTotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    p2.EndTime.TotalMilliseconds = p2.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                }

                p2.Text = Utilities.AutoBreakLine(p2.Text);
            }

            subtitle.Renumber();
        }
    }
}
