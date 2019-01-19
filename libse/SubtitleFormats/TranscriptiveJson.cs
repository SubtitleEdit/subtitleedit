using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TranscriptiveJson : SubtitleFormat
    {

        public override string Extension => ".json";

        public override string Name => "Transcriptive Json";

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string s in lines)
            {
                sb.Append(s);
            }

            var allText = sb.ToString().Trim();
            if (!allText.StartsWith("{", StringComparison.Ordinal) || !allText.Contains("\"alternatives\""))
            {
                return;
            }

            var parser = new JsonParser();
            Dictionary<string, object> dictionary;
            try
            {
                dictionary = (Dictionary<string, object>)parser.Parse(allText);
            }
            catch (ParserException)
            {
                return;
            }
            foreach (var k in dictionary.Keys)
            {
                if (k != "results" || !(dictionary[k] is List<object> v))
                {
                    continue;
                }

                foreach (var item in v)
                {
                    if (!(item is Dictionary<string, object> dic))
                    {
                        continue;
                    }

                    foreach (var altKey in dic.Keys)
                    {
                        if (altKey != "alternatives")
                        {
                            continue;
                        }

                        if (!(dic[altKey] is List<object> altElementList))
                        {
                            continue;
                        }

                        foreach (var altSubElement in altElementList)
                        {
                            string text = string.Empty;
                            double start = -1;
                            double end = -1;
                            if (!(altSubElement is Dictionary<string, object> details))
                            {
                                continue;
                            }

                            foreach (var detailsKey in details)
                            {
                                if (detailsKey.Key == "transcript")
                                {
                                    text = detailsKey.Value.ToString();
                                }
                                else if (detailsKey.Key == "timestamps")
                                {
                                    if (!(detailsKey.Value is List<object> timestampList))
                                    {
                                        continue;
                                    }

                                    foreach (var timestamp in timestampList)
                                    {
                                        if (!(timestamp is List<object> timestampListWord))
                                        {
                                            continue;
                                        }

                                        foreach (var timestampValue in timestampListWord)
                                        {
                                            if (start < 0)
                                            {
                                                if (double.TryParse(timestampValue.ToString().Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var s))
                                                {
                                                    start = s;
                                                }
                                            }
                                            else
                                            {
                                                if (double.TryParse(timestampValue.ToString().Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var e))
                                                {
                                                    end = e;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(text) && end > 0)
                            {
                                subtitle.Paragraphs.Add(new Paragraph(text, start * TimeCode.BaseUnit, end * TimeCode.BaseUnit));
                            }
                        }
                    }
                }
            }
            subtitle.Renumber();
        }
    }
}

