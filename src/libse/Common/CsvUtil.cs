using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class CsvUtil
    {
        public static string[] CsvSplit(string line, bool quoteOn, out bool continuation)
        {
            var lines = new List<string>();
            var stringOn = false;
            var item = new StringBuilder();
            var index = 0;
            while (index < line.Length)
            {
                var ch = line[index];
                char? next = null;
                if (index < line.Length - 1)
                {
                    next = line[index + 1];
                }

                if (stringOn)
                {
                    switch (ch)
                    {
                        case ',' when !quoteOn:
                            lines.Add(item.ToString());
                            item = new StringBuilder();
                            index++;
                            stringOn = false;
                            continue;
                        case '"' when !quoteOn:
                            item.Append(ch);
                            continue;
                        case '"':
                        {
                            if (next == '"')
                            {
                                item.Append(ch);
                                index++;
                            }
                            else
                            {
                                quoteOn = false;
                            }

                            index++;
                            continue;
                        }
                        default:
                            item.Append(ch);
                            break;
                    }
                }
                else if (ch == '"' && !quoteOn)
                {
                    stringOn = true;
                    quoteOn = true;
                    index++;
                    continue;
                }
                else if (ch == ',')
                {
                    lines.Add(item.ToString());
                    item = new StringBuilder();
                    index++;
                    continue;
                }
                else
                {
                    item.Append(ch);
                    stringOn = true;
                    index++;
                    continue;
                }

                index++;
            }

            if (item.Length > 0)
            {
                lines.Add(item.ToString());
            }

            continuation = quoteOn;

            return lines.ToArray();
        }
    }
}
