﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType7 : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".json"; }
        }

        public override string Name
        {
            get { return "JSON Type 7"; }
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
            var sb = new StringBuilder();
            sb.Append("{\"start\": [");
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                sb.Append(p.StartTime.TotalMilliseconds);
                if (i < subtitle.Paragraphs.Count - 1)
                    sb.Append(',');
            }
            sb.AppendLine("],");

            sb.Append("{\"end\": [");
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                sb.Append(p.EndTime.TotalMilliseconds);
                if (i < subtitle.Paragraphs.Count - 1)
                    sb.Append(',');
            }
            sb.AppendLine("],");

            sb.Append("\"text\": [");
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                sb.Append('"');
                Json.EncodeJsonText(Utilities.UnbreakLine(p.Text), sb);
                sb.Append('"');
                if (i < subtitle.Paragraphs.Count - 1)
                    sb.Append(',');
            }
            sb.Append("]");
            sb.Append('}');
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);

            string allText = sb.ToString();
            if (!allText.Contains("\"text\""))
                return;

            var startTimes = Json.ReadArray(allText, "start");
            var endTimes = Json.ReadArray(allText, "end");
            var texts = Json.ReadArray(allText, "text");

            for (int i = 0; i < Math.Min(Math.Min(startTimes.Count, texts.Count), endTimes.Count); i++)
            {
                try
                {
                    string text = Json.DecodeJsonText(texts[i]);
                    var p = new Paragraph(text, int.Parse(startTimes[i]), int.Parse(endTimes[i]));
                    subtitle.Paragraphs.Add(p);
                }
                catch
                {
                    _errorCount++;
                }
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

    }
}
