﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType9 : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".json"; }
        }

        public override string Name
        {
            get { return "JSON Type 9"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder(@"[");
            int count = 0;
            for (int index = 0; index < subtitle.Paragraphs.Count;)
            {
                Paragraph p = subtitle.Paragraphs[index];
                index++;
                if (count > 0)
                    sb.Append(',');
                sb.Append("{\"index\":");
                sb.Append(index);
                sb.Append(",\"start\":\"");
                sb.Append(p.StartTime);
                sb.Append("\",\"end\":\"");
                sb.Append(p.EndTime);
                sb.Append("\",\"text\": [");
                if (!string.IsNullOrEmpty(p.Text))
                {
                    foreach (var line in p.Text.SplitToLines())
                    {
                        sb.Append("\"");
                        Json.EncodeJsonText(line, sb);
                        sb.Append("\"");
                    }
                }
                sb.Append("]}");
                count++;
            }
            sb.Append(']');
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);
            var allText = sb.ToString().Trim();
            if (!allText.StartsWith("[", StringComparison.Ordinal) || !allText.Contains("\"index\""))
                return;

            foreach (var line in Json.ReadObjectArray(allText)) //allText.Split('{', '}', '[', ']'))
            {
                var s = line.Trim();
                if (s.Length > 10)
                {
                    var start = Json.ReadTag(s, "start");
                    var end = Json.ReadTag(s, "end");
                    var textLines = Json.ReadArray(s, "text");
                    try
                    {
                        sb.Clear();
                        foreach (var textLine in textLines)
                        {
                            sb.AppendLine(Json.DecodeJsonText(textLine));
                        }
                        subtitle.Paragraphs.Add(new Paragraph(sb.ToString().Trim(), TimeCode.ParseToMilliseconds(start), TimeCode.ParseToMilliseconds(end)));
                    }
                    catch (Exception)
                    {
                        _errorCount++;
                    }
                }
            }
            subtitle.Renumber();
        }

    }
}
