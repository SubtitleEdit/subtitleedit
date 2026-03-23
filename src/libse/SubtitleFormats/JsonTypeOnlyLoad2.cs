using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// https://lv.ulikecam.com/
    /// </summary>
    public class JsonTypeOnlyLoad2 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type Only load 2";

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not implemented";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (var s in lines)
            {
                sb.Append(s);
            }

            var text = sb.ToString().TrimStart();
            if (text.IndexOf("canvas_config", StringComparison.Ordinal) < 0)
            {
                return;
            }

            subtitle.Paragraphs.Clear();
            var parser = new SeJsonParser();
            var textDictionary = new Dictionary<string, string>();
            var texts = parser.GetArrayElementsByName(text, "texts");
            foreach (var textElement in texts)
            {
                var id = parser.GetFirstObject(textElement, "id");
                var content = parser.GetFirstObject(textElement, "content");
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(content))
                {
                    textDictionary.Add(id, content);
                }
            }

            var timeDictionary = new Dictionary<string, Paragraph>();
            var tracks = parser.GetArrayElementsByName(text, "tracks");
            foreach (var track in tracks)
            {
                var trackType = parser.GetFirstObject(track, "type");
                if (trackType == "text")
                {
                    var segments = parser.GetArrayElementsByName(track, "segments");
                    foreach (var segment in segments)
                    {
                        var id = parser.GetFirstObject(segment, "material_id");
                        var targetTimeRage = parser.GetFirstObject(segment, "target_timerange");
                        if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(targetTimeRage))
                        {
                            var start = parser.GetFirstObject(targetTimeRage, "start");
                            var duration = parser.GetFirstObject(targetTimeRage, "duration");
                            if (double.TryParse(start, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var startNumber) &&
                                double.TryParse(duration, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var durationNumber))
                            {
                                var startMs = startNumber / 1_000;
                                var durationMs = durationNumber / 1_000;
                                var endMs = startMs + durationMs;
                                var p = new Paragraph(string.Empty, startMs, endMs);
                                timeDictionary.Add(id, p);
                            }
                        }
                    }
                }
            }

            foreach (var kvp in textDictionary)
            {
                if (timeDictionary.TryGetValue(kvp.Key, out var p))
                {
                    p.Text = kvp.Value;
                    subtitle.Paragraphs.Add(p);
                }
            }

            subtitle.Sort(Enums.SubtitleSortCriteria.StartTime);
            subtitle.Renumber();
        }
    }
}
