using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Drtic : MicroDvd
    {
        public override string Extension => ".dtc";

        public override string Name => "Drtic";

        public override bool IsTimeBased => false;

        public override bool IsMine(List<string> lines, string fileName)
        {
            var headerFound = false;
            var trimmedLines = new List<string>();
            int errors = 0;
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    if (line.StartsWith("[JRT2:", StringComparison.Ordinal))
                    {
                        headerFound = true;
                    }
                    else if (line.Contains('{'))
                    {
                        string s = RemoveIllegalSpacesAndFixEmptyCodes(line);
                        if (RegexMicroDvdLine.IsMatch(s))
                        {
                            trimmedLines.Add(s);
                        }
                        else
                        {
                            errors++;
                        }
                    }
                    else
                    {
                        errors++;
                    }
                }
            }

            return headerFound && trimmedLines.Count > errors;
        }

        private static string RemoveIllegalSpacesAndFixEmptyCodes(string line)
        {
            if (string.IsNullOrEmpty(line) || line.Length > 2000)
            {
                return line;
            }

            int index = line.IndexOf('}');
            if (index >= 0 && index + 1 < line.Length)
            {
                index = line.IndexOf('}', index + 1);
                if (index >= 0 && index + 1 < line.Length)
                {
                    var indexOfBrackets = line.IndexOf("{}", StringComparison.Ordinal);
                    if (indexOfBrackets >= 0 && indexOfBrackets < index)
                    {
                        line = line.Insert(indexOfBrackets + 1, "0"); // set empty time codes to zero
                        index++;
                    }

                    while (line.Contains(' ') && line.IndexOf(' ') < index)
                    {
                        line = line.Remove(line.IndexOf(' '), 1);
                        index--;
                    }
                }
            }
            return line;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return $"[JRT2: {subtitle.Paragraphs.Count} 0 ]{Environment.NewLine}{base.ToText(subtitle, title)}{Environment.NewLine}";
        }
    }
}
