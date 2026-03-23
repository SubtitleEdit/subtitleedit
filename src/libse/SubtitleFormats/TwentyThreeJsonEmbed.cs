using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TwentyThreeJsonEmbed : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "TwentyThree json embed";

        public override string ToText(Subtitle subtitle, string title)
        {
            var data = new TwentyThreeJson().ToText(subtitle, title);

            var encodedData = Json.EncodeJsonText(data
                .Replace(Environment.NewLine, " ")
                .Replace("\t", " ")
                );

            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"data\": \"{encodedData}\"");
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
            var indexOfMainTag = allText.IndexOf("\"timestamp_begin\"", StringComparison.Ordinal);
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
            var subtitleData = parser.GetFirstObject(json, "data");
            if (string.IsNullOrEmpty(subtitleData))
            {
                return;
            }

            var format = new TwentyThreeJson();
            var innerJson = Json.DecodeJsonText(subtitleData);
            format.LoadSubtitle(subtitle, new List<string> { innerJson }, null);
        }
    }
}
