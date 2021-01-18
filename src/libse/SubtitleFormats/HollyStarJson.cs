using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class HollyStarJson : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "HoliStar Json";

        public override string ToText(Subtitle subtitle, string title)
        {
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            var sb = new StringBuilder();
            var header = @"
{
    'SubtitleLanguages': [
        {
            'IsForced': false,
            'ClassName': '[LANGUAGE_CODE]',
            'Name': '[LANGUAGE_ENGLISH]',
            'NativeName': '[LANGUAGE_NATIVE]',
            'SubtitleItems': ["
                .Replace('\'', '"')
                .Replace("[LANGUAGE_CODE]", language);

            try
            {
                var ci = new CultureInfo(language);
                header = header.Replace("[LANGUAGE_ENGLISH]", ci.EnglishName);
                header = header.Replace("[LANGUAGE_NATIVE]", ci.NativeName);
            }
            catch
            {
                header = header.Replace("[LANGUAGE_ENGLISH]", "unknown");
                header = header.Replace("[LANGUAGE_NATIVE]", "unknown");
            }

            sb.AppendLine(header);

            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                if (i > 0)
                {
                    sb.AppendLine("\t\t,");
                }

                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\t\"TextLines\": [");
                Paragraph p = subtitle.Paragraphs[i];
                var lines = p.Text.SplitToLines();
                for (int j = 0; j < lines.Count; j++)
                {
                    sb.Append("\t\t\t");
                    sb.Append('"');
                    sb.Append(Json.EncodeJsonText(lines[j]));
                    sb.Append('"');
                    if (j < lines.Count - 1)
                    {
                        sb.Append(',');
                    }
                    sb.AppendLine();
                }
                sb.AppendLine("\t\t\t],");

                sb.AppendLine($"\t\t\t\"ClassName\": \"{language}\", ");
                sb.AppendLine($"\t\t\t\"ShowTime\": {p.StartTime.TotalMilliseconds}, ");
                sb.AppendLine($"\t\t\t\"HideTime\": {p.EndTime.TotalMilliseconds}");
                sb.AppendLine("\t\t}");
            }

            sb.Append(@"
            ],
            'ReturnCode': {
                'Id': 1,
                'Code': 'SUCCESS'
            }
        }
    ]
}").Replace('\'', '"');

            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            foreach (string s in lines)
            {
                sb.Append(s);
            }

            string allText = sb.ToString();
            var indexOfMainTag = allText.IndexOf("\"SubtitleLanguages\"", StringComparison.Ordinal);
            if (indexOfMainTag < 0)
            {
                return;
            }

            var startIndex = allText.Remove(indexOfMainTag).LastIndexOf('{');
            if (startIndex < 0)
            {
                return;
            }

            var json = allText.Substring(startIndex);
            var parser = new SeJsonParser();
            var items = parser.GetArrayElementsByName(json, "SubtitleItems");
            foreach (var item in items)
            {
                var textLines = parser.GetArrayElementsByName(item, "TextLines");
                var showTime = parser.GetAllTagsByNameAsStrings(item, "ShowTime");
                var hideTime = parser.GetAllTagsByNameAsStrings(item, "HideTime");
                if (textLines.Count > 0 && showTime.Count == 1 && hideTime.Count == 1 && long.TryParse(showTime[0], out var startMs) && long.TryParse(hideTime[0], out var endMs))
                {
                    var text = new StringBuilder();
                    foreach (var line in textLines)
                    {
                        text.AppendLine(Json.DecodeJsonText(line));
                    }

                    var p = new Paragraph(text.ToString().Trim(), startMs, endMs);
                    var className = parser.GetAllTagsByNameAsStrings(item, "ClassName");
                    if (className.Count == 1)
                    {
                        p.Language = className[0];
                    }

                    subtitle.Paragraphs.Add(p);
                }
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }
    }
}
