using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class CsvUtil
    {
        public static List<List<string>> CsvSplitLines(List<string> lines, char separator)
        {
            var result = new List<List<string>>();
            var continuation = false;
            var lineResult = new List<string>();
            foreach (var line in lines)
            {
                if (!continuation)
                {
                    var parts = CsvSplit(line, false, out var con, separator);
                    continuation = con;

                    lineResult.AddRange(parts);

                    if (!continuation)
                    {
                        result.Add(lineResult);
                        lineResult = new List<string>();
                    }
                }
                else
                {
                    var parts = CsvSplit(line, true, out var con).ToList();
                    continuation = con;

                    if (parts.Count > 0)
                    {
                        if (lineResult.Count > 0)
                        {
                            lineResult[lineResult.Count - 1] += Environment.NewLine + parts[0];
                        }
                        else
                        {
                            lineResult.Add(parts[0]);
                        }

                        parts.RemoveAt(0);
                        lineResult.AddRange(parts);
                    }

                    if (!continuation)
                    {
                        result.Add(lineResult);
                        lineResult = new List<string>();
                    }
                }
            }

            if (lineResult.Count > 0)
            {
                result.Add(lineResult);
            }

            return result;
        }

        public static string[] CsvSplit(string line, bool quoteOn, out bool continuation, char separator = ',')
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
                        case ',' when !quoteOn && separator == ',':
                            lines.Add(item.ToString());
                            item = new StringBuilder();
                            index++;
                            stringOn = false;
                            continue;
                        case ';' when !quoteOn && separator == ';':
                            lines.Add(item.ToString());
                            item = new StringBuilder();
                            index++;
                            stringOn = false;
                            continue;
                        case '\t' when !quoteOn && separator == '\t':
                            lines.Add(item.ToString());
                            item = new StringBuilder();
                            index++;
                            stringOn = false;
                            continue;
                        case '"' when !quoteOn:
                            item.Append(ch);
                            index++;
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
                else if (ch == separator)
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
