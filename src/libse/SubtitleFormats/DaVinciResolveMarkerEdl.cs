using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Timeline markers as exported/imported by DaVinci Resolve via
    /// "Timelines &gt; Export/Import &gt; Timeline Markers to/from EDL".
    /// Each marker is a one-frame CMX event followed by a comment line:
    ///   001  001      V     C        01:00:05:00 01:00:05:01 01:00:05:00 01:00:05:01
    ///    |C:ResolveColorBlue |M:Marker name |D:120
    /// where C is the marker color, M the marker name and D the duration in frames.
    /// </summary>
    public class DaVinciResolveMarkerEdl : SubtitleFormat
    {
        private static readonly Regex EventLineRegex = new Regex(@"^\d{1,6}\s+\S+\s+V\s+C\s+(\d\d:\d\d:\d\d[:;]\d\d)\s+(\d\d:\d\d:\d\d[:;]\d\d)\s+(\d\d:\d\d:\d\d[:;]\d\d)\s+(\d\d:\d\d:\d\d[:;]\d\d)\s*$", RegexOptions.Compiled);

        public override string Extension => ".edl";

        public const string NameOfFormat = "DaVinci Resolve Marker EDL";

        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var hasMarkerLine = false;
            foreach (var line in lines)
            {
                if (line.Contains("|M:", StringComparison.Ordinal))
                {
                    hasMarkerLine = true;
                    break;
                }
            }

            if (!hasMarkerLine)
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("TITLE: " + title);
            sb.AppendLine(Configuration.Settings.General.CurrentFrameRate % 1.0 > 0.01 ? "FCM: DROP FRAME" : "FCM: NON-DROP FRAME");
            sb.AppendLine();

            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var p = subtitle.Paragraphs[index];
                var start = EncodeTimeCode(p.StartTime);
                var startPlusOne = EncodeTimeCode(new TimeCode(p.StartTime.TotalMilliseconds + FramesToMilliseconds(1)));
                var durationFrames = Math.Max(1, MillisecondsToFrames(p.DurationTotalMilliseconds));
                var name = HtmlUtil.RemoveHtmlTags(p.Text, true)
                    .Replace(Environment.NewLine, " ")
                    .Replace("|", " ")
                    .Trim();

                sb.AppendLine($"{index + 1:000}  001      V     C        {start} {startPlusOne} {start} {startPlusOne}  ");
                sb.AppendLine($" |C:ResolveColorBlue |M:{name} |D:{durationFrames.ToString(CultureInfo.InvariantCulture)}");
                sb.AppendLine();
            }

            return sb.ToString().Trim() + Environment.NewLine;
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            return $"{timeCode.Hours:00}:{timeCode.Minutes:00}:{timeCode.Seconds:00}:{MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            TimeCode pendingStart = null;

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd();
                var match = EventLineRegex.Match(line);
                if (match.Success)
                {
                    try
                    {
                        // Record-in carries the marker's timeline position
                        pendingStart = DecodeTimeCodeFrames(match.Groups[3].Value.Replace(';', ':'), SplitCharColon);
                    }
                    catch
                    {
                        pendingStart = null;
                        _errorCount++;
                    }

                    continue;
                }

                var markerIndex = line.IndexOf("|M:", StringComparison.Ordinal);
                if (markerIndex < 0 || pendingStart == null)
                {
                    continue;
                }

                var durationFrames = 0;
                var durationIndex = line.LastIndexOf("|D:", StringComparison.Ordinal);
                string name;
                if (durationIndex > markerIndex)
                {
                    name = line.Substring(markerIndex + 3, durationIndex - markerIndex - 3).Trim();
                    var durationText = line.Substring(durationIndex + 3).Trim();
                    int.TryParse(durationText, NumberStyles.Integer, CultureInfo.InvariantCulture, out durationFrames);
                }
                else
                {
                    name = line.Substring(markerIndex + 3).Trim();
                }

                var p = new Paragraph(pendingStart, new TimeCode(pendingStart.TotalMilliseconds), name);
                if (durationFrames > 1)
                {
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + FramesToMilliseconds(durationFrames);
                }
                else
                {
                    // A plain marker is one frame long - give it a readable display time
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(name);
                }

                subtitle.Paragraphs.Add(p);
                pendingStart = null;
            }

            subtitle.Renumber();
        }
    }
}
