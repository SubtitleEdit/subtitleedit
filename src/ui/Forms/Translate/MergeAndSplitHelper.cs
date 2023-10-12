using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Translate;

namespace Nikse.SubtitleEdit.Forms.Translate
{
    public static class MergeAndSplitHelper
    {
        public static async Task<int> MergeAndTranslateIfPossible(Subtitle sourceSubtitle, Subtitle targetSubtitle, TranslationPair source, TranslationPair target, int index, IAutoTranslator autoTranslator)
        {
            if (IsNonMergeLanguage(source.Code))
            {
                return 0;
            }

            var p = sourceSubtitle.Paragraphs[index];
            if (p.Text.Contains("{\\", StringComparison.Ordinal) || p.Text.EndsWith(')') || p.Text.StartsWith('-'))
            {
                return 0;
            }

            char? splitAtChar = null;
            var mergeCount = 0;
            var allItalic = false;
            var allBold = false;
            var text = string.Empty;
            var linesTranslate = 0;

            if (MergeWithThreeNext(sourceSubtitle, index, source.Code))
            {
                mergeCount = 3;
                allItalic = HasAllLinesTag(sourceSubtitle, index, mergeCount, "i");
                allBold = HasAllLinesTag(sourceSubtitle, index, mergeCount, "b");
                text = MergeLines(sourceSubtitle, index, mergeCount, allItalic, allBold);
            }
            else if (MergeWithTwoNext(sourceSubtitle, index, source.Code))
            {
                mergeCount = 2;
                allItalic = HasAllLinesTag(sourceSubtitle, index, mergeCount, "i");
                allBold = HasAllLinesTag(sourceSubtitle, index, mergeCount, "b");
                text = MergeLines(sourceSubtitle, index, mergeCount, allItalic, allBold);
            }
            else if (MergeWithNext(sourceSubtitle, index, source.Code))
            {
                mergeCount = 1;
                allItalic = HasAllLinesTag(sourceSubtitle, index, mergeCount, "i");
                allBold = HasAllLinesTag(sourceSubtitle, index, mergeCount, "b");
                text = MergeLines(sourceSubtitle, index, mergeCount, allItalic, allBold);
            }

            //  just take next sentence too
            var next = sourceSubtitle.GetParagraphOrDefault(index + 1);
            if (mergeCount == 0 && MergeWithNextOneLineEnding(sourceSubtitle, index, source.Code, out char splitChar))
            {
                splitAtChar = splitChar;
                mergeCount = 1;
                allItalic = HasAllLinesTag(sourceSubtitle, index, mergeCount, "i");
                allBold = HasAllLinesTag(sourceSubtitle, index, mergeCount, "b");
                text = Utilities.UnbreakLine(p.Text) + Environment.NewLine + Utilities.UnbreakLine(next.Text);
            }

            if (mergeCount > 0 && !text.Contains("{\\", StringComparison.Ordinal))
            {
                var mergedTranslation = await autoTranslator.Translate(text, source.Code, target.Code);
                List<string> result;
                if (splitAtChar != null && mergeCount == 1)
                {
                    result = SplitResultAtSplitChar(mergedTranslation, splitAtChar.Value, target.Code);
                }
                else
                {
                    result = SplitResult(mergedTranslation.SplitToLines(), mergeCount, source.Code);
                }

                if (allItalic)
                {
                    for (var k = 0; k < result.Count; k++)
                    {
                        result[k] = "<i>" + result[k] + "</i>";
                    }
                }

                if (allBold)
                {
                    for (var k = 0; k < result.Count; k++)
                    {
                        result[k] = "<b>" + result[k] + "</b>";
                    }
                }

                if (result.Count == mergeCount + 1 && result.All(t => !string.IsNullOrEmpty(t)))
                {
                    foreach (var line in result)
                    {
                        targetSubtitle.Paragraphs[index].Text = line;
                        index++;
                        linesTranslate++;
                    }

                    return linesTranslate;
                }
            }

            return linesTranslate;
        }

        private static bool HasAllLinesTag(Subtitle subtitle, int i, int mergeCount, string tag)
        {
            for (var j = i; j < subtitle.Paragraphs.Count && j <= i + mergeCount; j++)
            {
                var text = subtitle.Paragraphs[j].Text.Trim();
                if (!text.StartsWith("<" + tag + ">") && !text.EndsWith("</" + tag + ">"))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool MergeWithTwoNext(Subtitle subtitle, int i, string source)
        {
            if (i + 2 >= subtitle.Paragraphs.Count || IsNonMergeLanguage(source))
            {
                return false;
            }

            return MergeWithNext(subtitle, i, source) && MergeWithNext(subtitle, i + 1, source);
        }

        private static bool MergeWithThreeNext(Subtitle subtitle, int i, string source)
        {
            if (i + 3 >= subtitle.Paragraphs.Count || IsNonMergeLanguage(source))
            {
                return false;
            }

            return MergeWithNext(subtitle, i, source) && MergeWithNext(subtitle, i + 1, source) && MergeWithNext(subtitle, i + 2, source);
        }

        private static string MergeLines(Subtitle subtitle, int i, int mergeCount, bool italic, bool bold)
        {
            var sb = new StringBuilder();
            for (var j = i; j < subtitle.Paragraphs.Count && j <= i + mergeCount; j++)
            {
                var text = subtitle.Paragraphs[j].Text.Trim();
                sb.AppendLine(RemoveAllLinesTag(text, italic, bold));
            }

            return Utilities.RemoveLineBreaks(sb.ToString());
        }

        private static List<string> SplitResultAtSplitChar(string translation, char splitAtChar, string languageCode)
        {
            var idx = translation.IndexOf(splitAtChar);
            if (idx < 0 && idx < translation.Length - 1)
            {
                return new List<string>();
            }

            var line1 = Utilities.AutoBreakLine(translation.Substring(0, idx + 1).Trim(),
                Configuration.Settings.General.SubtitleLineMaximumLength,
                Configuration.Settings.General.MergeLinesShorterThan,
                languageCode);
            var line2 = Utilities.AutoBreakLine(translation.Remove(0, idx + 1).Trim(),
                Configuration.Settings.General.SubtitleLineMaximumLength,
                Configuration.Settings.General.MergeLinesShorterThan,
                languageCode);
            return new List<string> { line1, line2 };
        }

        private static bool MergeWithNextOneLineEnding(Subtitle subtitle, int index, string sourceCode, out char c)
        {
            c = '-';

            if (index + 1 >= subtitle.Paragraphs.Count || IsNonMergeLanguage(sourceCode))
            {
                return false;
            }

            if (MergeWithNext(subtitle, index, sourceCode))
            {
                return false;
            }

            if (subtitle.Paragraphs[index].Text.EndsWith(".") && Utilities.CountTagInText(subtitle.Paragraphs[index].Text, '.') == 1)
            {
                c = '.';
                return true;
            }

            if (subtitle.Paragraphs[index].Text.EndsWith("?") && Utilities.CountTagInText(subtitle.Paragraphs[index].Text, '?') == 1)
            {
                c = '?';
                return true;
            }

            if (subtitle.Paragraphs[index].Text.EndsWith("!") && Utilities.CountTagInText(subtitle.Paragraphs[index].Text, '!') == 1)
            {
                c = '!';
                return true;
            }

            return false;
        }

        private static bool MergeWithNext(Subtitle subtitle, int i, string source)
        {
            if (i + 1 >= subtitle.Paragraphs.Count || IsNonMergeLanguage(source))
            {
                return false;
            }

            var p = subtitle.Paragraphs[i];
            var text = HtmlUtil.RemoveHtmlTags(p.Text, true).TrimEnd('"');
            if (text.EndsWith(".", StringComparison.Ordinal) ||
                text.EndsWith("!", StringComparison.Ordinal) ||
                text.EndsWith("?", StringComparison.Ordinal))
            {
                return false;
            }

            var next = subtitle.Paragraphs[i + 1];
            return next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds < 500;
        }

        private static bool IsNonMergeLanguage(string source)
        {
            return source.ToLowerInvariant() == "zh" ||
                   source.ToLowerInvariant() == "zh-CN" ||
                   source.ToLowerInvariant() == "zh-TW" ||
                   source.ToLowerInvariant() == "yue_Hant" ||
                   source.ToLowerInvariant() == "zho_Hans" ||
                   source.ToLowerInvariant() == "zho_Hant" ||
                   source.ToLowerInvariant() == "jpn_Jpan" ||
                   source.ToLowerInvariant() == "ja";
        }



        private static string RemoveAllLinesTag(string text, bool allItalic, bool allBold)
        {
            if (allItalic)
            {
                text = text.Replace("<i>", string.Empty);
                text = text.Replace("</i>", string.Empty);
            }

            if (allBold)
            {
                text = text.Replace("<b>", string.Empty);
                text = text.Replace("</b>", string.Empty);
            }

            return text;
        }

        private static List<string> SplitResult(List<string> result, int mergeCount, string language)
        {
            if (result.Count != 1)
            {
                return result;
            }

            if (mergeCount == 1)
            {
                var arr = Utilities.AutoBreakLine(result[0], 84, 1, language).SplitToLines();
                if (arr.Count == 1)
                {
                    arr = Utilities.AutoBreakLine(result[0], 42, 1, language).SplitToLines();
                }

                if (arr.Count == 1)
                {
                    arr = Utilities.AutoBreakLine(result[0], 22, 1, language).SplitToLines();
                }

                if (arr.Count == 2)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                    };
                }

                if (arr.Count == 1)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                    };
                }

                return result;
            }

            if (mergeCount == 2)
            {
                var arr = SplitHelper.SplitToXLines(3, result[0], 84).ToArray();

                if (arr.Length == 3)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[2], 42, language == "zh" ? 0 : 25, language),
                    };
                }

                if (arr.Length == 2)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                    };
                }

                if (arr.Length == 1)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                        string.Empty,
                    };
                }

                return result;
            }

            if (mergeCount == 3)
            {
                var arr = SplitHelper.SplitToXLines(4, result[0], 84).ToArray();

                if (arr.Length == 4)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[2], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[3], 42, language == "zh" ? 0 : 25, language),
                    };
                }

                if (arr.Length == 3)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[2], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                    };
                }

                if (arr.Length == 2)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                        string.Empty,
                    };
                }

                if (arr.Length == 1)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                    };
                }

                return result;
            }

            return result;
        }

    }
}
