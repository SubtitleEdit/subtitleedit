using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Forms.Translate
{
    internal static class SplitHelper
    {
        internal class SplitListItem
        {
            internal List<string> Lines { get; set; }

            internal double DiffFromAverage(double avg)
            {
                return Lines.Sum(line => Math.Abs(avg - line.Length));
            }
        }

        public static List<string> SplitToXLines(int lineCount, string input, int singleLineMaxLength)
        {
            var text = input.Trim();
            var results = new List<SplitListItem>();
            for (var maxLength = singleLineMaxLength; maxLength > 5; maxLength--)
            {
                var list = new List<string>();
                var lastIndexOfSpace = -1;
                var start = 0;
                for (var i = 0; i < text.Length; i++)
                {
                    var ch = text[i];
                    if (ch == ' ')
                    {
                        if (i - start > maxLength && lastIndexOfSpace > start)
                        {
                            var line = text.Substring(start, lastIndexOfSpace - start);
                            list.Add(line.Trim());
                            start = lastIndexOfSpace + 1;
                        }
                        lastIndexOfSpace = i;
                    }
                }

                var lastLine = text.Substring(start);
                list.Add(lastLine.Trim());
                if (list.Count > lineCount)
                {
                    break;
                }

                results.Add(new SplitListItem { Lines = list });
            }

            var avg = text.Length / (double)lineCount;
            var best = results
                .Where(p => p.Lines.Count == lineCount)
                .OrderBy(p => p.DiffFromAverage(avg))
                .FirstOrDefault() ?? results.Where(p => p.Lines.Count == lineCount)
                .OrderBy(p => p.DiffFromAverage(avg))
                .FirstOrDefault();

            return best == null ? new List<string> { text } : best.Lines;
        }
    }
}
