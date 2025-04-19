using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class HtmlDiffGenerator
    {
        private readonly Configuration _configuration;

        public HtmlDiffGenerator(Configuration configuration)
        {
            _configuration = configuration;
        }
        
        public string Generate(IEnumerable<DiffGeneratorItem> diffGeneratorItems)
        {
            var htmlFileName = FileUtil.GetTempFileName(".html");
            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset='utf-8'><title>Subtitle Edit - Fix common errors preview</title><style>body,p,td {font-size:90%; font-family:Tahoma;} td {border:1px solid black;padding:5px} table {border-collapse: collapse;}</style></head><body><table><tbody>");
            sb.AppendLine($"<tr><td style='font-weight:bold'>{_configuration.LineNumber}</td><td style='font-weight:bold'>{_configuration.Function}</td><td style='font-weight:bold'>{_configuration.Before}</td><td style='font-weight:bold'>{_configuration.After}</td></tr>");
            foreach (var generatorItem in diffGeneratorItems)
            {
                var what = generatorItem.What;
                var before = generatorItem.Before;
                var after = generatorItem.After;

                var arr = MakeDiffHtml(before, after);
                sb.AppendLine($"<tr><td>{generatorItem.Number}</td><td>{what}</td><td><pre>{arr[0]}</pre></td><td><pre>{arr[1]}</pre></td></tr>");
            }

            sb.Append("</table></body></html>");
            File.WriteAllText(htmlFileName, sb.ToString());

            return htmlFileName;
        }
        
        private static string[] MakeDiffHtml(string before, string after)
        {
            before = before.Replace("<br />", "↲");
            after = after.Replace("<br />", "↲");
            before = before.Replace(Environment.NewLine, "↲");
            after = after.Replace(Environment.NewLine, "↲");

            var beforeColors = new Dictionary<int, SKColor>();
            var beforeBackgroundColors = new Dictionary<int, SKColor>();
            var afterColors = new Dictionary<int, SKColor>();
            var afterBackgroundColors = new Dictionary<int, SKColor>();

            // from start
            int minLength = Math.Min(before.Length, after.Length);
            int startCharactersOk = 0;
            for (int i = 0; i < minLength; i++)
            {
                if (before[i] == after[i])
                {
                    startCharactersOk++;
                }
                else
                {
                    if (before.Length > i + 4 && after.Length > i + 4 &&
                        before[i + 1] == after[i + 1] &&
                        before[i + 2] == after[i + 2] &&
                        before[i + 3] == after[i + 3] &&
                        before[i + 4] == after[i + 4])
                    {
                        startCharactersOk++;

                        if (char.IsWhiteSpace(before[i]))
                        {
                            beforeBackgroundColors.Add(i, SKColors.Red);
                        }
                        else
                        {
                            beforeColors.Add(i, SKColors.Red);
                        }

                        if (char.IsWhiteSpace(after[i]))
                        {
                            afterBackgroundColors.Add(i, SKColors.Red);
                        }
                        else
                        {
                            afterColors.Add(i, SKColors.Red);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            int maxLength = Math.Max(before.Length, after.Length);
            for (int i = startCharactersOk; i <= maxLength; i++)
            {
                if (i < before.Length)
                {
                    if (char.IsWhiteSpace(before[i]))
                    {
                        beforeBackgroundColors.Add(i, SKColors.Red);
                    }
                    else
                    {
                        beforeColors.Add(i, SKColors.Red);
                    }
                }
                if (i < after.Length)
                {
                    if (char.IsWhiteSpace(after[i]))
                    {
                        afterBackgroundColors.Add(i, SKColors.Red);
                    }
                    else
                    {
                        afterColors.Add(i, SKColors.Red);
                    }
                }
            }

            // from end
            for (int i = 1; i < minLength; i++)
            {
                int bLength = before.Length - i;
                int aLength = after.Length - i;
                if (before[bLength] == after[aLength])
                {
                    if (beforeColors.ContainsKey(bLength))
                    {
                        beforeColors.Remove(bLength);
                    }

                    if (beforeBackgroundColors.ContainsKey(bLength))
                    {
                        beforeBackgroundColors.Remove(bLength);
                    }

                    if (afterColors.ContainsKey(aLength))
                    {
                        afterColors.Remove(aLength);
                    }

                    if (afterBackgroundColors.ContainsKey(aLength))
                    {
                        afterBackgroundColors.Remove(aLength);
                    }
                }
                else
                {
                    break;
                }
            }

            var sb = new StringBuilder();
            for (int i = 0; i < before.Length; i++)
            {
                var s = before[i];
                if (beforeColors.ContainsKey(i) && beforeBackgroundColors.ContainsKey(i))
                {
                    sb.AppendFormat("<span style=\"color:{0}; background-color: {1}\">{2}</span>", ColorTranslator.ToHtml(beforeColors[i]), ColorTranslator.ToHtml(beforeBackgroundColors[i]), s);
                }
                else if (beforeColors.ContainsKey(i))
                {
                    sb.AppendFormat("<span style=\"color:{0}; \">{1}</span>", ColorTranslator.ToHtml(beforeColors[i]), s);
                }
                else if (beforeBackgroundColors.ContainsKey(i))
                {
                    sb.AppendFormat("<span style=\"background-color: {0}\">{1}</span>", ColorTranslator.ToHtml(beforeBackgroundColors[i]), s);
                }
                else
                {
                    sb.Append(s);
                }
            }
            var sb2 = new StringBuilder();
            for (int i = 0; i < after.Length; i++)
            {
                var s = after[i];
                if (afterColors.ContainsKey(i) && afterBackgroundColors.ContainsKey(i))
                {
                    sb2.AppendFormat("<span style=\"color:{0}; background-color: {1}\">{2}</span>", ColorTranslator.ToHtml(afterColors[i]), ColorTranslator.ToHtml(afterBackgroundColors[i]), s);
                }
                else if (afterColors.ContainsKey(i))
                {
                    sb2.AppendFormat("<span style=\"color:{0}; \">{1}</span>", ColorTranslator.ToHtml(afterColors[i]), s);
                }
                else if (afterBackgroundColors.ContainsKey(i))
                {
                    sb2.AppendFormat("<span style=\"background-color: {0}\">{1}</span>", ColorTranslator.ToHtml(afterBackgroundColors[i]), s);
                }
                else
                {
                    sb2.Append(s);
                }
            }

            return new[] { sb.ToString(), sb2.ToString() };
        }

        public class Configuration
        {
            public string Function { get; set; }
            public string After { get; set; }
            public string LineNumber { get; set; }
            public string Before { get; set; }
        }

        public class DiffGeneratorItem
        {
            public DiffGeneratorItem(int number, string what, string before, string after)
            {
                Number = number;
                What = what;
                Before = before;
                After = after;
            }

            public int Number { get; }
            public string What { get; }
            public string Before { get; }
            public string After { get; }
        }
    }
}