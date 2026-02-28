using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class TextSplit
    {
        private readonly List<TextSplitResult> _splits;
        private readonly List<TextSplitResult> _allSplits;
        private readonly int _singleLineMaxLength;
        private const string EndLineChars = ".!?…؟。？！";
        private const string Commas = ",،，、";

        public TextSplit(string text, int singleLineMaxLength, string language)
        {
            _singleLineMaxLength = singleLineMaxLength;

            // create list with all split possibilities
            _splits = new List<TextSplitResult>();
            _allSplits = new List<TextSplitResult>();
            for (var i = 1; i < text.Length - 1; i++)
            {
                if (text[i] == ' ')
                {
                    var l1 = text.Substring(0, i).Trim();
                    var l2 = text.Substring(i + 1).Trim();
                    _allSplits.Add(new TextSplitResult(new List<string> { l1, l2 }));
                    if (Utilities.CanBreak(text, i, language))
                    {
                        _splits.Add(new TextSplitResult(new List<string> { l1, l2 }));
                    }
                }
                else if ((language == "zh" || language == "ja" || language == "ko") && "，。？、?".Contains(text[i]))
                {
                    var l1 = text.Substring(0, i + 1).Trim();
                    var l2 = text.Substring(i + 1).Trim();
                    _allSplits.Add(new TextSplitResult(new List<string> { l1, l2 }));
                    _splits.Add(new TextSplitResult(new List<string> { l1, l2 }));
                }
            }
        }

        public string AutoBreak(bool breakEarlyDialog, bool breakEarlyLineEnding, bool breakEarlyComma, bool usePixelWidth)
        {
            if (breakEarlyDialog)
            {
                var s = GetBestDialog();
                if (s != null)
                {
                    return s;
                }
            }
            if (breakEarlyLineEnding)
            {
                var s = GetBestEnding(EndLineChars);
                if (s != null)
                {
                    return s;
                }
            }
            if (breakEarlyComma)
            {
                var s = GetBestEnding(Commas);
                if (s != null)
                {
                    return s;
                }
            }
            if (usePixelWidth)
            {
                var s = GetBestPixelSplit();
                if (s != null)
                {
                    return s;
                }
            }

            return GetBestLengthSplit();
        }

        private string GetBestDialog()
        {
            var orderedArray = _splits.Where(IsDialog).OrderBy(p => p.DiffFromAveragePixel());
            var best = orderedArray.FirstOrDefault();
            return best != null ? string.Join(Environment.NewLine, best.Lines) : null;
        }

        private string GetBestEnding(string chars)
        {
            var orderedArray = _splits.Where(p => p.IsLineLengthOkay(_singleLineMaxLength) && EndsWith(p, chars)).OrderBy(p => p.DiffFromAveragePixel());
            var best = orderedArray.FirstOrDefault();
            return best != null ? string.Join(Environment.NewLine, best.Lines) : null;
        }

        private string GetBestPixelSplit()
        {
            var orderedArray = _splits.Where(p => p.IsLineLengthOkay(_singleLineMaxLength)).OrderBy(p => p.DiffFromAveragePixel());
            var best = orderedArray.FirstOrDefault();
            if (best != null && !best.IsBottomHeavy && Configuration.Settings.Tools.AutoBreakUsePixelWidth && Configuration.Settings.Tools.AutoBreakPreferBottomHeavy)
            {
                orderedArray = _splits.Where(p => p.IsLineLengthOkay(_singleLineMaxLength)).OrderBy(p => p.DiffFromAveragePixelBottomHeavy());
                best = orderedArray.FirstOrDefault() ?? best;
            }

            return best != null ? string.Join(Environment.NewLine, best.Lines) : null;
        }

        private string GetBestLengthSplit()
        {
            var orderedArray = _splits.OrderBy(p => p.DiffFromAverage());
            var best = orderedArray.FirstOrDefault();
            if (best != null)
            {
                return string.Join(Environment.NewLine, best.Lines);
            }

            orderedArray = _allSplits.OrderBy(p => p.DiffFromAverage());
            best = orderedArray.FirstOrDefault();
            return best != null ? string.Join(Environment.NewLine, best.Lines) : null;
        }

        private static bool EndsWith(TextSplitResult textSplitResult, string chars)
        {
            var line1 = textSplitResult.Lines[0].TrimEnd('"', '\'');
            return line1.Length > 0 && chars.Contains(line1[line1.Length - 1]);
        }

        private static bool IsDialog(TextSplitResult textSplitResult)
        {
            var line1 = textSplitResult.Lines[0].TrimEnd('"', '\'');
            var line2 = textSplitResult.Lines[1].TrimStart('"', '\'');
            return line2.Length > 0 && line1.Length > 0 &&
                   line2.StartsWith('-') && EndLineChars.Contains(line1[line1.Length - 1]);
        }

        public static List<string> SplitMulti(string input, int numberOfParts, string language)
        {
            var inputText = input.Trim();
            if (inputText.Split(new char[] { ' ' }, StringSplitOptions.None).Length < numberOfParts)
            {
                return new List<string>();
            }

            var texts = SplitText(inputText, numberOfParts);
            var subtitle = new Subtitle();
            var startMs = 0;
            for (var i = 0; i < texts.Count; i++)
            {
                subtitle.Paragraphs.Add(new Paragraph(texts[i], startMs, startMs + 3000));
                startMs += 3000;
            }

            subtitle = TryForWholeSentences(subtitle, language, Configuration.Settings.General.SubtitleLineMaximumLength);
            subtitle = TryForWholeSentences(subtitle, language, Configuration.Settings.General.SubtitleLineMaximumLength);

            return subtitle.Paragraphs.Select(p => p.Text).ToList();
        }

        public static List<string> SplitText(string text, int numberOfParts)
        {
            var parts = new List<string>();

            var totalLength = text.Length;
            var partLength = totalLength / numberOfParts;

            var startIndex = 0;

            for (var i = 0; i < numberOfParts - 1; i++)
            {
                var endIndex = FindNearestSpaceIndex(text, startIndex + partLength);

                if (endIndex == -1)
                {
                    // If no space is found, break the loop
                    break;
                }

                var length = endIndex - startIndex;
                if (length <= 0)
                {
                    parts.Add(string.Empty);
                    break;
                }

                var part = text.Substring(startIndex, length);
                parts.Add(part);

                // Move the start index to the next character after the space
                startIndex = endIndex + 1;
            }

            // Add the remaining text as the last part
            if (startIndex < totalLength)
            {
                var lastPart = text.Substring(startIndex);
                parts.Add(lastPart);
            }

            return parts;
        }

        static int FindNearestSpaceIndex(string text, int startIndex)
        {
            var spaceIndex = text.LastIndexOf(' ', Math.Min(startIndex, text.Length - 1));
            return spaceIndex;
        }

        public static Subtitle TryForWholeSentences(Subtitle inputSubtitle, string language, int lineMaxLength)
        {
            var s = new Subtitle(inputSubtitle);
            const int maxMoveChunkSize = 15;
            var deleteIndices = new List<int>();

            for (var i = 0; i < s.Paragraphs.Count - 1; i++)
            {
                var p = s.Paragraphs[i];
                var next = s.Paragraphs[i + 1];

                if (p == null ||
                    next == null ||
                    p.EndTime.TotalMilliseconds - next.StartTime.TotalMilliseconds > 100 ||
                    p.Text.Contains('<') ||
                    next.Text.Contains('<') ||
                    !(p.Text.Contains('.') || p.Text.Contains('?') || p.Text.Contains('!') || next.Text.Contains('.') || next.Text.Contains('?') || next.Text.Contains('!')) ||
                    p.Text.EndsWith('.') ||
                    p.Text.EndsWith('?') ||
                    p.Text.EndsWith('!'))
                {
                    continue;
                }

                if (deleteIndices.Contains(i))
                {
                    continue;
                }

                // check for period in last part of current
                var lastPeriodIdx = p.Text.LastIndexOfAny(new[] { '.', '?', '!' });
                if (lastPeriodIdx > 3 && lastPeriodIdx > p.Text.Length - maxMoveChunkSize)
                {
                    var newCurrentText = p.Text.Substring(0, lastPeriodIdx + 1).Trim();
                    var newNextText = p.Text.Remove(0, lastPeriodIdx + 1).Trim();

                    newCurrentText = Utilities.AutoBreakLine(newCurrentText, language);
                    newNextText = Utilities.AutoBreakLine(newNextText + " " + next.Text);

                    var arrayCurrent = newCurrentText.SplitToLines();
                    var arrayNext = newNextText.SplitToLines();

                    var currentOk = arrayCurrent.Count == 1 || (arrayCurrent.Count == 2 && arrayCurrent[0].Length < lineMaxLength * 2);
                    var nextOk = arrayNext.Count == 1 || (arrayNext.Count == 2 && arrayNext[0].Length < lineMaxLength * 2);
                    var allOk = newCurrentText.Length < lineMaxLength * 2 &&
                                     newNextText.Length < lineMaxLength * 2;

                    if (currentOk && nextOk && allOk)
                    {
                        var oldP = new Paragraph(p);

                        p.Text = newCurrentText;
                        next.Text = newNextText;

                        if (string.IsNullOrWhiteSpace(newCurrentText))
                        {
                            deleteIndices.Add(i);
                            next.StartTime.TotalMilliseconds = p.StartTime.TotalMilliseconds;
                        }
                        else
                        {
                            var durationToMove = CalcDurationToMove(oldP, p, next);
                            p.EndTime.TotalMilliseconds += durationToMove;
                            next.StartTime.TotalMilliseconds += durationToMove;
                        }

                        continue;
                    }
                }

                // check for period in beginning of next
                var firstPeriodIdx = next.Text.IndexOfAny(new[] { '.', '?', '!' });
                if (firstPeriodIdx >= 3 && firstPeriodIdx < maxMoveChunkSize)
                {
                    var newCurrentText = next.Text.Substring(0, firstPeriodIdx + 1).Trim();
                    var newNextText = next.Text.Remove(0, firstPeriodIdx + 1).Trim();

                    newCurrentText = Utilities.AutoBreakLine(p.Text + " " + newCurrentText, language);
                    newNextText = Utilities.AutoBreakLine(newNextText);

                    var arrayCurrent = newCurrentText.SplitToLines();
                    var arrayNext = newNextText.SplitToLines();

                    var currentOk = arrayCurrent.Count == 1 || (arrayCurrent.Count == 2 && arrayCurrent[0].Length < lineMaxLength * 2);
                    var nextOk = arrayNext.Count == 1 || (arrayNext.Count == 2 && arrayNext[0].Length < lineMaxLength * 2);
                    var allOk = newCurrentText.Length < lineMaxLength * 2 &&
                                     newNextText.Length < lineMaxLength * 2;

                    if (currentOk && nextOk && allOk)
                    {
                        var oldP = new Paragraph(p);

                        p.Text = newCurrentText;
                        next.Text = newNextText;

                        if (string.IsNullOrWhiteSpace(newNextText))
                        {
                            deleteIndices.Add(i + 1);
                            p.EndTime.TotalMilliseconds = next.EndTime.TotalMilliseconds;
                        }
                        else
                        {
                            var durationToMove = CalcDurationToMove(oldP, p, next);
                            p.EndTime.TotalMilliseconds += durationToMove;
                            next.StartTime.TotalMilliseconds += durationToMove;
                        }
                    }
                }
            }

            s.RemoveParagraphsByIndices(deleteIndices);

            return s;
        }

        public static double CalcDurationToMove(Paragraph oldCurrent, Paragraph current, Paragraph next)
        {
            if (current.DurationTotalMilliseconds < 0 || next.DurationTotalMilliseconds < 0)
            {
                return 0;
            }

            var totalDuration = current.DurationTotalMilliseconds + next.DurationTotalMilliseconds;
            var totalChars = current.Text.Length + next.Text.Length;
            var durChar = totalDuration / totalChars;

            var diffLength = current.Text.Length - oldCurrent.Text.Length;
            var result = durChar * diffLength;
            return result;
        }
    }
}
