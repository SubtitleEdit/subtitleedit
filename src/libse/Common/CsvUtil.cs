using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Provides utility methods for working with CSV (Comma-separated values) data.
    /// </summary>
    public static class CsvUtil
    {
        /// <summary>
        /// Splits a CSV line into an array of strings based on the provided parameters.
        /// Handles quoted and non-quoted CSV entries.
        /// </summary>
        /// <param name="line">The CSV line to be split into parts.</param>
        /// <param name="quoteOn">A boolean indicating whether to handle quoted entries.</param>
        /// <param name="continuation">An out parameter indicating if the line ends with an open quote.</param>
        /// <returns>An array of strings representing the split parts of the CSV line.</returns>
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
