using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class VideoIndexerJson : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "Video Indexer Transcript";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder(@"{
  ""transcript"": [");
            sb.AppendLine();
            var count = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.AppendLine(",");
                }

                sb.AppendLine("    {");

                sb.Append("      \"id\":");
                sb.Append(count.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine(",");

                sb.Append("      \"text\": \"");
                sb.Append(Json.EncodeJsonText(p.Text));
                sb.AppendLine("\",");

                sb.AppendLine("      \"instances\": [");
                sb.AppendLine("        {");

                sb.Append("          \"start\": \"");
                sb.Append(p.StartTime.ToString(false).Replace(",", "."));
                sb.AppendLine("\",");

                sb.Append("          \"end\": \"");
                sb.Append(p.EndTime.ToString(false).Replace(",", "."));
                sb.AppendLine("\"");
                sb.AppendLine("        }");
                sb.AppendLine("      ]");
                sb.Append("    }");
                count++;
            }

            sb.AppendLine();
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            foreach (var s in lines)
            {
                sb.Append(s);
            }

            var json = sb.ToString();
            if (!json.Contains("\"transcript\"", StringComparison.Ordinal))
            {
                return;
            }

            var parser = new SeJsonParser();
            var transcriptArray = parser.GetArrayElementsByName(json, "transcript");

            foreach (var s in transcriptArray)
            {
                var text = Json.ReadTag(s, "text");

                var instances = parser.GetArrayElementsByName(s, "instances");
                if (instances.Count > 0)
                {
                    var start = Json.ReadTag(s, "start");
                    var end = Json.ReadTag(s, "end");

                    if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
                    {
                        var startArr = start.Split(':', '.');
                        var endArr = end.Split(':', '.');

                        if (startArr.Length == 4 && endArr.Length == 4)
                        {
                            try
                            {
                                startArr[3] = startArr[3].PadRight(3, '0');
                                endArr[3] = endArr[3].PadRight(3, '0');

                                var startTime = DecodeTimeCodeMsFourParts(startArr);
                                var endTime = DecodeTimeCodeMsFourParts(endArr);

                                subtitle.Paragraphs.Add(new Paragraph(startTime, endTime, Json.DecodeJsonText(text)));

                            }
                            catch
                            {
                                _errorCount++;
                            }
                        }
                        else
                        {
                            _errorCount++;
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber();
        }
    }
}
