using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonTypeOnlyLoad6 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type Only load 6";

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not implemented";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            var parser = new SeJsonParser();
            foreach (var s in lines)
            {
                if (s.IndexOf("\"clickTrackingParams\"", StringComparison.Ordinal) < 0)
                {
                    _errorCount++;

                    if (_errorCount > 10)
                    {
                        return;
                    }
                }
                else
                {
                    var author = string.Empty;
                    var authorName = parser.GetFirstObject(s, "authorName");
                    if (!string.IsNullOrEmpty(authorName))
                    {
                        author = parser.GetFirstObject(authorName, "simpleText");
                    }

                    var startTime = string.Empty;
                    var timestampText = parser.GetFirstObject(s, "timestampText");
                    if (!string.IsNullOrEmpty(timestampText))
                    {
                        startTime = parser.GetFirstObject(timestampText, "simpleText");
                    }
                    var startTimeParts = startTime.Split(':', ',', ';').ToList();
                    if (startTimeParts.Count > 1)
                    {
                        while (startTimeParts.Count < 4)
                        {
                            startTimeParts.Insert(0, "0");
                        }
                        var tc = DecodeTimeCodeFramesFourParts(startTimeParts.ToArray());
                        var end = new TimeCode(tc.TotalMilliseconds + 2500);
                        var text = parser.GetAllTagsByNameAsStrings(s, "text");
                        var t = text.FirstOrDefault();

                        if (t != null)
                        {
                            if (!string.IsNullOrEmpty(author))
                            {
                                t = author + ": " + t;
                            }
                            var p = new Paragraph(tc, end, t);
                            p.Actor = author;
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                }
            }

            subtitle.Renumber();
        }
    }
}
