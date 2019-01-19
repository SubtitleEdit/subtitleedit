using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SwiftInterchange2 : SubtitleFormat
    {
        private const string ItalicPrefix = "<fontstyle-italic>";
        private string _fileName;

        public override string Extension => ".sif";

        public override string Name => "Swift Interchange File V2";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines.Count > 0 && lines[0] != null && lines[0].StartsWith("{\\rtf1"))
            {
                return false;
            }

            _fileName = fileName;
            return base.IsMine(lines, fileName);
        }

        private static string GetOriginatingSwift(Subtitle subtitle)
        {
            string lang = "English (USA)";
            string languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            if (languageCode == "nl")
            {
                lang = "Dutch (Netherlands)";
            }
            else if (languageCode == "de")
            {
                lang = "German (German)";
            }
            return "Open 25 " + lang;
            // examples:
            //   Line21 30 DROP English (USA)
            //   Open 25  German (German)
            //   Open 25  Dutch (Netherlands)
            //TODO: Frame rate
        }

        private string GetVideoFileName(string title)
        {
            string fileNameNoExt = null;
            if (_fileName != null)
            {
                fileNameNoExt = _fileName.Substring(0, _fileName.Length - Path.GetExtension(_fileName).Length);
            }
            foreach (var ext in Utilities.VideoFileExtensions)
            {
                if (!string.IsNullOrEmpty(fileNameNoExt) && File.Exists(Path.Combine(fileNameNoExt, ext)))
                {
                    return Path.Combine(fileNameNoExt, ext);
                }
                if (!string.IsNullOrEmpty(title) && File.Exists(Path.Combine(title, ext)))
                {
                    return Path.Combine(title, ext);
                }
            }
            if (string.IsNullOrEmpty(title))
            {
                return "Unknown.mpg";
            }
            return title + ".mpg";
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            string date = string.Format("{0:00}/{1:00}/{2}", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
            const string header = @"# SWIFT INTERCHANGE FILE V2
# DO NOT EDIT LINES BEGINNING WITH '#' SIGN
# Originating Swift: [ORIGINATING_SWIFT]
# VIDEO CLIP : [VIDEO_FILE]
# BROADCAST DATE : [DATE]
# REVISION DATE : [DATE]
# CREATION DATE : [DATE]
# COUNTRY OF ORIGIN : ENG
# EPISODE NUMBER : 0
# DEADLINE DATE : [DATE]
# AUTO TX : false
# CURRENT STYLE : None
# STYLE DATE : None
# STYLE Time : None";
            var sb = new StringBuilder();
            var videoFileName = GetVideoFileName(title);
            sb.AppendLine(header.Replace("[DATE]", date).Replace("[VIDEO_FILE]", videoFileName).Replace("[ORIGINATING_SWIFT]", GetOriginatingSwift(subtitle)));
            sb.AppendLine();
            sb.AppendLine();
            const string paragraphWriteFormat = @"# SUBTITLE {3}
# TIMEIN {0}
# DURATION {1} AUTO
# TIMEOUT {2}
# START ROW BOTTOM
# ALIGN CENTRE JUSTIFY LEFT";
            int count = 1;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string startTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds));
                string endTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds));
                string duration = string.Format("{0:00}:{1:00}", p.Duration.Seconds, MillisecondsToFramesMaxFrameRate(p.Duration.Milliseconds));
                sb.AppendLine(string.Format(paragraphWriteFormat, startTime, duration, endTime, count));
                string text = HtmlUtil.RemoveHtmlTags(p.Text);
                if (p.Text.StartsWith("<i>", StringComparison.Ordinal) && p.Text.EndsWith("</i>", StringComparison.Ordinal))
                {
                    text = ItalicPrefix + text;
                }
                var arr = text.SplitToLines();
                for (int rowNo = 0; rowNo < arr.Count; rowNo++)
                {
                    if (rowNo == arr.Count - 1)
                    {
                        sb.AppendLine("# ROW " + rowNo);
                    }
                    else
                    {
                        sb.AppendLine("# ROW " + rowNo + " RETURN");
                    }
                    sb.AppendLine(arr[rowNo]);
                }
                sb.AppendLine();
                count++;
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            foreach (string line in lines)
            {
                if (line.StartsWith("# SUBTITLE", StringComparison.Ordinal))
                {
                    if (p != null)
                    {
                        subtitle.Paragraphs.Add(p);
                    }

                    p = new Paragraph();
                }
                else if (p != null && line.StartsWith("# TIMEIN", StringComparison.Ordinal))
                {
                    string timeCode = line.Remove(0, 8).Trim();
                    if (timeCode != "--:--:--:--" && !GetTimeCode(p.StartTime, timeCode))
                    {
                        _errorCount++;
                    }
                }
                else if (p != null && line.StartsWith("# DURATION", StringComparison.Ordinal))
                {
                    // # DURATION 01:17 AUTO
                    string timecode = line.Remove(0, 10).Replace("AUTO", string.Empty).Trim();
                    if (timecode != "--:--")
                    {
                        var arr = timecode.Split(new[] { ':', ' ' });
                        if (arr.Length > 1)
                        {
                            int sec;
                            int frame;
                            if (int.TryParse(arr[0], out sec) && int.TryParse(arr[1], out frame))
                            {
                                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + FramesToMillisecondsMax999(frame);
                                p.EndTime.TotalSeconds += sec;
                            }
                        }
                    }
                }
                else if (p != null && line.StartsWith("# TIMEOUT", StringComparison.Ordinal))
                {
                    string timeCode = line.Remove(0, 9).Trim();
                    if (timeCode != "--:--:--:--" && !GetTimeCode(p.EndTime, timeCode))
                    {
                        _errorCount++;
                    }
                }
                else if (p != null && !line.StartsWith('#'))
                {
                    if (p.Text.Length > 500)
                    {
                        _errorCount += 10;
                        return;
                    }
                    p.Text = (p.Text + Environment.NewLine + line).Trim();
                }
            }
            if (p != null)
            {
                subtitle.Paragraphs.Add(p);
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();

            foreach (var paragraph in subtitle.Paragraphs)
            {
                if (paragraph.Text.StartsWith(ItalicPrefix, StringComparison.Ordinal))
                {
                    paragraph.Text = "<i>" + paragraph.Text.Remove(0, ItalicPrefix.Length).TrimStart() + "</i>";
                }
            }
        }

        private static bool GetTimeCode(TimeCode timeCode, string timeString)
        {
            try
            {
                string[] timeParts = timeString.Split(':', '.');
                timeCode.Hours = int.Parse(timeParts[0]);
                timeCode.Minutes = int.Parse(timeParts[1]);
                timeCode.Seconds = int.Parse(timeParts[2]);
                timeCode.Milliseconds = FramesToMillisecondsMax999(int.Parse(timeParts[3]));
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
