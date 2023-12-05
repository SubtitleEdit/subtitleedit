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
        public static async Task<int> MergeAndTranslateIfPossible(Subtitle sourceSubtitle, Subtitle targetSubtitle, TranslationPair source, TranslationPair target, int index, IAutoTranslator autoTranslator, bool forceSingleLineMode)
        {
            var noSentenceEndingSource = IsNonMergeLanguage(source.Code);
            var noSentenceEndingTarget = IsNonMergeLanguage(target.Code);

            if (forceSingleLineMode)
            {
                noSentenceEndingSource = true; // will add separator between lines
            }

            var p = sourceSubtitle.Paragraphs[index];
            char? splitAtChar = null;
            var mergeCount = 0;
            var allItalic = false;
            var allBold = false;
            var text = string.Empty;
            var linesTranslate = 0;

            MergeResult mergeResult = null;
            List<Formatting> formattings = null;


            // Try to handle (remove and save info for later restore) italics, bold, alignment, and more where possible
            var tempSubtitle = new Subtitle(sourceSubtitle);
            formattings = HandleFormatting(tempSubtitle, index, target.Code);

            // Merge text for better translation and save info enough to split again later
            mergeResult = MergeMultipleLines(tempSubtitle, index, autoTranslator.MaxCharacters, noSentenceEndingSource, noSentenceEndingTarget);
            mergeCount = mergeResult.ParagraphCount;
            text = mergeResult.Text;

            //TODO: handle mergeResult.Error !!!


            if (mergeCount == 0)
            {
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

            if (mergeResult != null)
            {
                var mergedTranslation = await autoTranslator.Translate(text, source.Code, target.Code);
                var splitResult = SplitMultipleLines(mergeResult, mergedTranslation, target.Code);
                if (splitResult.Count == mergeCount)
                {
                    var idx = 0;
                    foreach (var line in splitResult)
                    {
                        var reformattedText = formattings[idx].ReAddFormatting(line);
                        targetSubtitle.Paragraphs[index].Text = reformattedText;
                        index++;
                        linesTranslate++;
                        idx++;
                    }

                    return linesTranslate;
                }
            }

            if (mergeCount > 0)
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

        private static List<Formatting> HandleFormatting(Subtitle sourceSubtitle, int index, string sourceLanguage)
        {
            var formattings = new List<Formatting>();

            for (var i = index; i < sourceSubtitle.Paragraphs.Count; i++)
            {
                var p = sourceSubtitle.Paragraphs[i];
                var f = new Formatting();
                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
                p.Text = text;
                formattings.Add(f);
            }

            return formattings;
        }

        public class MergeResultItem
        {
            public string Text { get; set; }
            public int StartIndex { get; set; }
            public int EndIndex { get; set; }
            public bool AllItalic { get; set; }
            public bool AllBold { get; set; }
            public bool Continious { get; set; }
            public char EndChar { get; set; }
            public int EndCharOccurences { get; set; }
            public bool IsEmpty { get; set; }
            public bool HasError { get; set; }
        }


        public class MergeResult
        {
            public string Text { get; set; }
            public int ParagraphCount { get; set; }
            public List<MergeResultItem> MergeResultItems { get; set; }
            public bool HasError { get; set; }
            public bool NoSentenceEndingSource { get; set; }
            public bool NoSentenceEndingTarget { get; set; }
        }

        public static MergeResult MergeMultipleLines(Subtitle sourceSubtitle, int index, int maxTextSize, bool noSentenceEndingSource, bool noSentenceEndingTarget)
        {
            var result = new MergeResult
            {
                MergeResultItems = new List<MergeResultItem>(),
                NoSentenceEndingSource = noSentenceEndingSource,
                NoSentenceEndingTarget = noSentenceEndingTarget,
            };

            var item = new MergeResultItem();
            item.StartIndex = index;
            item.EndIndex = index;

            result.Text = sourceSubtitle.Paragraphs[index].Text;
            if (string.IsNullOrWhiteSpace(result.Text))
            {
                item.IsEmpty = true;
                result.MergeResultItems.Add(item);
                item = null;
                result.Text = string.Empty;
            }

            var textBuild = new StringBuilder(result.Text);
            var prev = sourceSubtitle.Paragraphs[index];

            for (var i = index + 1; i < sourceSubtitle.Paragraphs.Count; i++)
            {
                var p = sourceSubtitle.Paragraphs[i];

                if (item != null && Utilities.UrlEncodeLength(result.Text + Environment.NewLine + p.Text) > maxTextSize)
                {
                    break;
                }

                if (noSentenceEndingSource)
                {
                    result.Text += Environment.NewLine + "." + Environment.NewLine + p.Text;
                    if (item != null)
                    {
                        item.StartIndex = i - 1;
                        item.EndIndex = i - 1;
                        result.MergeResultItems.Add(item);

                        textBuild = new StringBuilder();
                        item = new MergeResultItem { StartIndex = i };
                    }
                }
                else if (string.IsNullOrWhiteSpace(p.Text))
                {
                    var endChar = result.Text[result.Text.Length - 1];
                    if (item != null)
                    {
                        item.EndChar = endChar;
                        var endCharOccurences = Utilities.CountTagInText(textBuild.ToString(), endChar);
                        item.EndCharOccurences = endCharOccurences;
                        result.MergeResultItems.Add(item);
                        textBuild = new StringBuilder();
                    }

                    result.MergeResultItems.Add(new MergeResultItem
                    {
                        StartIndex = index,
                        EndIndex = index,
                        IsEmpty = true
                    });

                    item = null;
                    textBuild = new StringBuilder();

                }
                else if (result.Text.HasSentenceEnding() || string.IsNullOrWhiteSpace(result.Text))
                {

                    if (string.IsNullOrWhiteSpace(result.Text))
                    {
                        if (item != null)
                        {
                            result.MergeResultItems.Add(item);
                            textBuild = new StringBuilder();
                        }
                    }
                    else
                    {
                        var endChar = result.Text[result.Text.Length - 1];

                        if (item != null)
                        {
                            item.EndChar = endChar;
                            var endCharOccurences = Utilities.CountTagInText(textBuild.ToString(), endChar);
                            item.EndCharOccurences = endCharOccurences;
                            result.MergeResultItems.Add(item);
                            textBuild = new StringBuilder();
                        }
                    }

                    textBuild.Append(p.Text);

                    result.Text += Environment.NewLine + p.Text;

                    item = new MergeResultItem { StartIndex = i };
                }
                else if (item != null && (item.Continious || item.StartIndex == item.EndIndex) && p.StartTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds < 1000)
                {
                    textBuild.Append(" ");
                    textBuild.Append(p.Text);
                    result.Text += " " + p.Text;
                    item.Continious = true;
                }
                else
                {
                    result.HasError = true;
                    break; // weird continuation
                }

                if (item != null)
                {
                    item.EndIndex = i;
                }

                prev = p;
            }

            if (item != null)
            {
                //TODO: skip last item if it does not has sentence ending (or skip early at sentence ending...)

                result.MergeResultItems.Add(item);

                if (result.Text.Length > 0 && result.Text.HasSentenceEnding())
                {
                    var endChar = result.Text[result.Text.Length - 1];
                    item.EndChar = endChar;
                    item.EndCharOccurences = Utilities.CountTagInText(textBuild.ToString(), endChar);
                }
            }

            result.Text = result.Text.Trim();
            result.ParagraphCount = result.MergeResultItems.Sum(p => p.EndIndex - p.StartIndex + 1);

            return result;
        }

        public static List<string> SplitMultipleLines(MergeResult mergeResult, string input, string language)
        {
            var lines = new List<string>();
            var text = input;

            if (mergeResult.NoSentenceEndingSource)
            {
                var sb = new StringBuilder();
                var translatedLines = text.SplitToLines();
                foreach (var translatedLine in translatedLines)
                {
                    var s = translatedLine.Trim();
                    if (s == ".")
                    {
                        lines.Add(sb.ToString().Trim());
                        sb.Clear();
                        continue;
                    }

                    sb.AppendLine(s);
                }

                if (sb.Length > 0)
                {
                    lines.Add(sb.ToString().Trim());
                }

                return lines;
            }

            foreach (var item in mergeResult.MergeResultItems)
            {
                if (item.IsEmpty)
                {
                    lines.Add(string.Empty);
                }
                else if (item.Continious)
                {
                    var part = GetPartFromItem(text, item);
                    text = text.Remove(0, part.Length).Trim();
                    var lineRAnge = SplitContontinous(part, item, language);
                    lines.AddRange(lineRAnge);
                }
                else
                {
                    var part = GetPartFromItem(text, item);
                    text = text.Remove(0, part.Length).Trim();
                    lines.Add(Utilities.AutoBreakLine(part));
                }
            }

            return lines;
        }

        private static string GetPartFromItem(string input, MergeResultItem item)
        {
            if (item.EndChar == '\0')
            {
                return input;
            }

            var idx = input.IndexOf(item.EndChar);
            if (idx < 0)
            {
                return string.Empty;
            }

            var count = 1;
            while (idx >= 0 && idx < input.Length - 1)
            {
                if (count == item.EndCharOccurences)
                {
                    return input.Substring(0, idx + 1);
                }

                idx = input.IndexOf(item.EndChar, idx + 1);
                count++;
            }

            return input;
        }

        private static List<string> SplitContontinous(string text, MergeResultItem item, string language)
        {
            var count = item.EndIndex - item.StartIndex + 1;

            if (count == 2)
            {
                var arr = Utilities.AutoBreakLine(text, Configuration.Settings.General.SubtitleLineMaximumLength * 2, 0, language).SplitToLines();
                if (arr.Count == 2)
                {
                    return arr;
                }
            }

            return TextSplit.SplitMulti(text, count, language);
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

            var next = subtitle.GetParagraphOrDefault(index + 1);
            if (next == null || !next.Text.HasSentenceEnding("en"))
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
