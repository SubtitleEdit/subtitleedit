using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle41 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d+\.\d\d?$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Unknown 41";

        public override string ToText(Subtitle subtitle, string title)
        {
            Configuration.Settings.General.CurrentFrameRate = 24.0;
            const string paragraphWriteFormat = "{0}{3}{1}{3}{2}/{3}";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), p.Text, EncodeTimeCode(p.EndTime), Environment.NewLine));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //911.2
            //C’est l’enfant qui l’a tuée ?
            //915.8/

            //921.8
            //Comment elle s’appelait ?
            //924.6/

            Configuration.Settings.General.CurrentFrameRate = 24.0;
            _errorCount = 0;
            Paragraph p = null;
            bool textOn = false;
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                try
                {
                    if (textOn)
                    {
                        if (line.Length > 1 && line.Length < 11 && RegexTimeCodes.Match(line.TrimEnd('/')).Success)
                        {
                            p.EndTime = DecodeTimeCode(line.TrimEnd('/').Split('.'));
                            if (sb.Length > 0)
                            {
                                p.Text = sb.ToString().TrimEnd();
                                subtitle.Paragraphs.Add(p);
                                textOn = false;
                            }
                        }
                        else
                        {
                            sb.AppendLine(line);
                            if (sb.Length > 500)
                            {
                                _errorCount += 10;
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (line.Length > 1 && line.Length < 11 && RegexTimeCodes.Match(line).Success)
                        {
                            p = new Paragraph();
                            sb.Clear();
                            p.StartTime = DecodeTimeCode(line.Split('.'));
                            textOn = true;
                        }
                    }
                }
                catch
                {
                    textOn = false;
                    _errorCount++;
                }
            }
            if (textOn && sb.Length > 0)
            {
                p.Text = sb.ToString().TrimEnd();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            int frames = MillisecondsToFrames(time.TotalMilliseconds);
            int footage = frames / 16;
            int rest = (int)Math.Round(frames % 16.0 / 16.0 * Configuration.Settings.General.CurrentFrameRate);
            return $"{footage}.{rest:0}";
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            var frames16 = int.Parse(parts[0]);
            var frames = int.Parse(parts[1]);
            return new TimeCode(0, 0, 0, FramesToMilliseconds(16 * frames16 + frames));
        }

    }
}