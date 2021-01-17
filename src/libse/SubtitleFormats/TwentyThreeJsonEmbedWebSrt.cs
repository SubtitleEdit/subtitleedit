using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TwentyThreeJsonEmbedWebSrt : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "TwentyThree json embed srt";

        public override string ToText(Subtitle subtitle, string title)
        {
            var data = new SubRip().ToText(subtitle, title);
            data = data.Replace("\t", " ");
            var encodedData = Json.EncodeJsonText(data);
            encodedData = encodedData.Replace("<br />", "\\n");

            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"data\": {{ \"websrt\" : \"{encodedData}\" }} ");
            sb.AppendLine("}");
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
            var indexOfMainTag = allText.IndexOf("\"websrt\"", StringComparison.Ordinal);
            if (indexOfMainTag < 0 && !allText.Contains("\"data\""))
            {
                return;
            }

            var startIndex = allText.IndexOf('{');
            if (startIndex < 0)
            {
                return;
            }

            var json = allText.Substring(startIndex);
            var parser = new SeJsonParser();
            var subtitleData = parser.GetFirstObject(json, "websrt");
            if (string.IsNullOrEmpty(subtitleData))
            {
                return;
            }

            var format = new SubRip();
            var innerJson = Json.DecodeJsonText(subtitleData);
            format.LoadSubtitle(subtitle, innerJson.SplitToLines(), null);
        }
    }
}
