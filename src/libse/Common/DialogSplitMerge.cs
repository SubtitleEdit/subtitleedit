using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// DialogSplitMerge class provides functionality to handle dialog splits and merges,
    /// allowing for customized dialog styles and continuation styles within subtitles.
    /// </summary>
    public class DialogSplitMerge
    {
        /// <summary>
        /// Gets or sets the style of dialog to be used for splitting and merging subtitles.
        /// The <see cref="DialogStyle"/> determines how dashes and spaces are handled in dialogs.
        /// </summary>
        /// <remarks>
        /// Possible values for <see cref="DialogStyle"/> are defined in the <see cref="DialogType"/> enum and include:
        /// - DashBothLinesWithSpace
        /// - DashBothLinesWithoutSpace
        /// - DashSecondLineWithSpace
        /// - DashSecondLineWithoutSpace
        /// </remarks>
        public DialogType DialogStyle { get; set; }

        /// <summary>
        /// Gets or sets the continuation style used for handling subtitle lines that continue across multiple dialogues.
        /// The <see cref="ContinuationStyle"/> determines the punctuation and formatting applied to these continuing lines.
        /// </summary>
        /// <remarks>
        /// Possible values for <see cref="ContinuationStyle"/> are defined in the <see cref="Nikse.SubtitleEdit.Core.Enums.ContinuationStyle"/> enum and include options such as:
        /// - None
        /// - NoneTrailingDots
        /// - NoneLeadingTrailingDots
        /// - NoneTrailingEllipsis
        /// - NoneLeadingTrailingEllipsis
        /// - OnlyTrailingDots
        /// - LeadingTrailingDots
        /// - OnlyTrailingEllipsis
        /// - LeadingTrailingEllipsis
        /// - LeadingTrailingDash
        /// - LeadingTrailingDashDots
        /// - Custom
        /// </remarks>
        public ContinuationStyle ContinuationStyle { get; set; }

        /// <summary>
        /// Gets or sets the two-letter ISO 639-1 language code.
        /// The <see cref="TwoLetterLanguageCode"/> is used to determine language-specific processing behavior.
        /// </summary>
        /// <remarks>
        /// This property is important for functions that require language-specific rules,
        /// such as detecting if a language typically omits periods at the end of sentences.
        /// </remarks>
        public string TwoLetterLanguageCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to skip checking the line endings when
        /// performing dialog split and merge operations.
        /// </summary>
        /// <remarks>
        /// When set to <c>true</c>, dialog split and merge operations will not enforce line ending checks,
        /// which may be required for certain languages or specific subtitle correction scenarios.
        /// </remarks>
        public bool SkipLineEndingCheck { get; set; }

        /// <summary>
        /// Retrieves the dash character used for dialog formatting.
        /// </summary>
        /// <returns>A character representing the dash style used in dialogs.</returns>
        private static char GetDashChar() => '-';

        /// <summary>
        /// Retrieves the alternate dash character used for dialog formatting.
        /// </summary>
        /// <returns>A character representing an alternate dash style used in dialogs.</returns>
        private static char GetAlternateDashChar() => '‐'; // Unicode En Dash (\u2010)

        /// <summary>
        /// Fixes the dashes and adjusts the surrounding spacing based on the provided input text, current paragraph, and previous paragraph.
        /// </summary>
        /// <param name="input">The dialog text input to be processed.</param>
        /// <param name="p">The current paragraph containing the dialog text.</param>
        /// <param name="prev">The previous paragraph which may affect the style of the current dialog.</param>
        /// <returns>Formatted dialog text with corrected dashes and appropriate spacing.</returns>
        public string FixDashesAndSpaces(string input)
        {
            return FixSpaces(FixDashes(input));
        }

        /// <summary>
        /// Fixes the dashes and adjusts the surrounding spacing based on the given paragraphs.
        /// </summary>
        /// <param name="input">The dialog text input to be processed.</param>
        /// <param name="p">The current paragraph containing the dialog text.</param>
        /// <param name="prev">The previous paragraph which may affect the style of the current dialog.</param>
        /// <returns>Formatted dialog text with corrected dashes and appropriate spacing.</returns>
        public string FixDashesAndSpaces(string input, Paragraph p, Paragraph prev)
        {
            return FixSpaces(FixDashes(input, p, prev));
        }

        /// <summary>
        /// Adjusts the spacing around dialog dashes based on the specified dialog style.
        /// </summary>
        /// <param name="input">The dialog text input that requires spacing adjustments.</param>
        /// <returns>Formatted dialog text with correct spacing around dashes based on the dialog style.</returns>
        public string FixSpaces(string input)
        {
            var lines = input.SplitToLines();
            if (!IsDialog(lines))
            {
                return input;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                var l = lines[i];
                var pre = GetStartTags(l);
                l = l.Remove(0, pre.Length);
                switch (DialogStyle)
                {
                    case DialogType.DashBothLinesWithSpace:
                        if (l.TrimStart().StartsWith(GetDashChar()))
                        {
                            var rest = l.Remove(0, l.IndexOf(GetDashChar()) + 1);
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + rest.TrimStart());
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    case DialogType.DashSecondLineWithSpace:
                        if (i > 0 && l.TrimStart().StartsWith(GetDashChar()))
                        {
                            var rest = l.Remove(0, l.IndexOf(GetDashChar()) + 1);
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + rest.TrimStart());
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    case DialogType.DashBothLinesWithoutSpace:
                        if (l.TrimStart().StartsWith(GetDashChar()))
                        {
                            var rest = l.Remove(0, l.IndexOf(GetDashChar()) + 1);
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + rest.TrimStart());
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    case DialogType.DashSecondLineWithoutSpace:
                        if (i > 0 && l.TrimStart().StartsWith(GetDashChar()))
                        {
                            var rest = l.Remove(0, l.IndexOf(GetDashChar()) + 1);
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + rest.TrimStart());
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Adds spaces around dialog lines based on the dialog style settings.
        /// Processes each line of the input text and adjusts spaces around dashes appropriately.
        /// </summary>
        /// <param name="input">The input string containing dialog lines.</param>
        /// <returns>Formatted dialog text with appropriate spaces added around dashes.</returns>
        public string AddSpaces(string input)
        {
            var lines = input.SplitToLines();
            if (!IsDialog(lines))
            {
                return input;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                var l = lines[i];
                var pre = GetStartTags(l);
                l = l.Remove(0, pre.Length);
                switch (DialogStyle)
                {
                    case DialogType.DashBothLinesWithSpace:
                        if (l.TrimStart().StartsWith(GetDashChar()))
                        {
                            var rest = l.Remove(0, l.IndexOf(GetDashChar()) + 1);
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + rest.TrimStart());
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    case DialogType.DashSecondLineWithSpace:
                        if (i > 0 && l.TrimStart().StartsWith(GetDashChar()))
                        {
                            var rest = l.Remove(0, l.IndexOf(GetDashChar()) + 1);
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + rest.TrimStart());
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    default:
                        sb.AppendLine(pre + l);
                        break;
                }
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Removes unnecessary spaces from dialog lines based on the set dialog style.
        /// </summary>
        /// <param name="input">The dialog text from which spaces need to be removed.</param>
        /// <returns>A string with spaces removed based on the dialog style configuration.</returns>
        public string RemoveSpaces(string input)
        {
            var lines = input.SplitToLines();
            if (!IsDialog(lines))
            {
                return input;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                var l = lines[i];
                var pre = GetStartTags(l);
                l = l.Remove(0, pre.Length);
                switch (DialogStyle)
                {
                    case DialogType.DashBothLinesWithSpace:
                    case DialogType.DashSecondLineWithSpace:
                        sb.AppendLine(pre + l);
                        break;
                    case DialogType.DashBothLinesWithoutSpace:
                        if (l.TrimStart().StartsWith(GetDashChar()))
                        {
                            var rest = l.Remove(0, l.IndexOf(GetDashChar()) + 1);
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + rest.TrimStart());
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    case DialogType.DashSecondLineWithoutSpace:
                        if (i > 0 && l.TrimStart().StartsWith(GetDashChar()))
                        {
                            var rest = l.Remove(0, l.IndexOf(GetDashChar()) + 1);
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + rest.TrimStart());
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Fixes the dashes in the provided input text based on the dialog style and formatting rules
        /// determined by the current paragraph and the previous paragraph.
        /// </summary>
        /// <param name="input">The dialog text input to be processed.</param>
        /// <param name="p">The current paragraph containing the dialog text.</param>
        /// <param name="prev">The previous paragraph which may affect the style of the current dialog.</param>
        /// <returns>Formatted dialog text with corrected dashes according to the specified rules.</returns>
        public string FixDashes(string input, Paragraph p, Paragraph prev)
        {
            var lines = input.SplitToLines();
            if (!IsDialog(lines, p, prev))
            {
                return input;
            }

            var isDialogThreeLinesTwoOne = lines.Count == 3 && IsDialogThreeLinesTwoOne(lines[0], lines[1], lines[2]);
            var isDialogThreeLinesOneTwo = lines.Count == 3 && IsDialogThreeLinesOneTwo(lines[0], lines[1], lines[2]);

            var sb = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                var l = lines[i];
                var pre = GetStartTags(l);
                l = l.Remove(0, pre.Length);
                switch (DialogStyle)
                {
                    case DialogType.DashBothLinesWithSpace:
                        if (isDialogThreeLinesTwoOne && i == 1 || isDialogThreeLinesOneTwo && i == 2)
                        {
                            sb.AppendLine(pre + l);
                        }
                        else if (!l.TrimStart().StartsWith(GetDashChar()))
                        {
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.TrimStart().TrimStart(GetAlternateDashChar()));
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    case DialogType.DashSecondLineWithSpace:
                        if (i > 0 && !l.TrimStart().StartsWith(GetDashChar()))
                        {
                            if (isDialogThreeLinesTwoOne && i == 1 || isDialogThreeLinesOneTwo && i == 2)
                            {
                                sb.AppendLine(pre + l);
                            }
                            else
                            {
                                sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.TrimStart().TrimStart(GetAlternateDashChar()));
                            }
                        }
                        else if (i == 0 && l.TrimStart().StartsWith(GetDashChar()))
                        {
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.Remove(0, l.IndexOf(GetDashChar()) + 1).TrimStart());
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    case DialogType.DashBothLinesWithoutSpace:
                        if (isDialogThreeLinesTwoOne && i == 1 || isDialogThreeLinesOneTwo && i == 2)
                        {
                            sb.AppendLine(pre + l);
                        }
                        else if (!l.TrimStart().StartsWith(GetDashChar()))
                        {
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.TrimStart().TrimStart(GetAlternateDashChar()));
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    case DialogType.DashSecondLineWithoutSpace:
                        if (i > 0 && !l.TrimStart().StartsWith(GetDashChar()))
                        {
                            if (isDialogThreeLinesTwoOne && i == 1 || isDialogThreeLinesOneTwo && i == 2)
                            {
                                sb.AppendLine(pre + l);
                            }
                            else
                            {
                                sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.TrimStart().TrimStart(GetAlternateDashChar()));
                            }
                        }
                        else if (i == 0 && l.TrimStart().StartsWith(GetDashChar()))
                        {
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.Remove(0, l.IndexOf(GetDashChar()) + 1).TrimStart().TrimStart(GetAlternateDashChar()));
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Fixes the dashes in the provided input dialog text based on predefined styles, ensuring proper format across lines.
        /// </summary>
        /// <param name="input">The dialog text input to be processed.</param>
        /// <returns>Formatted dialog text with corrected dashes according to the specified style.</returns>
        public string FixDashes(string input)
        {
            var lines = input.SplitToLines();
            if (!IsDialog(lines))
            {
                return input;
            }

            var isDialogThreeLinesTwoOne = lines.Count == 3 && IsDialogThreeLinesTwoOne(lines[0], lines[1], lines[2]);
            var isDialogThreeLinesOneTwo = lines.Count == 3 && IsDialogThreeLinesOneTwo(lines[0], lines[1], lines[2]);

            var sb = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                var l = lines[i];
                var pre = GetStartTags(l);
                l = l.Remove(0, pre.Length);
                switch (DialogStyle)
                {
                    case DialogType.DashBothLinesWithSpace:
                        if (isDialogThreeLinesTwoOne && i == 1 || isDialogThreeLinesOneTwo && i == 2)
                        {
                            sb.AppendLine(pre + l);
                        }
                        else if (!l.TrimStart().StartsWith(GetDashChar()))
                        {
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.TrimStart().TrimStart(GetAlternateDashChar()));
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    case DialogType.DashSecondLineWithSpace:
                        if (i > 0 && !l.TrimStart().StartsWith(GetDashChar()))
                        {
                            if (isDialogThreeLinesTwoOne && i == 1 || isDialogThreeLinesOneTwo && i == 2)
                            {
                                sb.AppendLine(pre + l);
                            }
                            else
                            {
                                sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.TrimStart().TrimStart(GetAlternateDashChar()));
                            }
                        }
                        else if (i == 0 && l.TrimStart().StartsWith(GetDashChar()))
                        {
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.Remove(0, l.IndexOf(GetDashChar()) + 1).TrimStart());
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    case DialogType.DashBothLinesWithoutSpace:
                        if (isDialogThreeLinesTwoOne && i == 1 || isDialogThreeLinesOneTwo && i == 2)
                        {
                            sb.AppendLine(pre + l);
                        }
                        else if (!l.TrimStart().StartsWith(GetDashChar()))
                        {
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.TrimStart().TrimStart(GetAlternateDashChar()));
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    case DialogType.DashSecondLineWithoutSpace:
                        if (i > 0 && !l.TrimStart().StartsWith(GetDashChar()))
                        {
                            if (isDialogThreeLinesTwoOne && i == 1 || isDialogThreeLinesOneTwo && i == 2)
                            {
                                sb.AppendLine(pre + l);
                            }
                            else
                            {
                                sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.TrimStart().TrimStart(GetAlternateDashChar()));
                            }
                        }
                        else if (i == 0 && l.TrimStart().StartsWith(GetDashChar()))
                        {
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.Remove(0, l.IndexOf(GetDashChar()) + 1).TrimStart().TrimStart(GetAlternateDashChar()));
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Inserts a starting dash in the provided text, at the specified line index.
        /// </summary>
        /// <param name="input">The text to be processed.</param>
        /// <param name="lineIndex">The index of the line where the dash should be inserted.</param>
        /// <returns>The text with the starting dash appropriately inserted.</returns>
        public string InsertStartDash(string input, int lineIndex)
        {
            var pre = GetStartTags(input);
            var s = input.Remove(0, pre.Length);

            if (string.IsNullOrEmpty(s))
            {
                return input;
            }

            if (s.StartsWith(GetDashChar()))
            {
                s = s.TrimStart(GetDashChar());
            }

            return pre + GetLineStartFromDashStyle(lineIndex) + s.TrimStart();
        }

        /// <summary>
        /// Removes the starting dash from the input string, if present, and
        /// adjusts the surrounding spaces while preserving any leading tags.
        /// </summary>
        /// <param name="input">The input text from which the starting dash needs to be removed.</param>
        /// <returns>The input string without the starting dash, preserving leading tags and spaces.</returns>
        public static string RemoveStartDash(string input)
        {
            var pre = GetStartTags(input);
            var s = input.Remove(0, pre.Length);

            if (!s.TrimStart().StartsWith(GetDashChar()) && !s.TrimStart().StartsWith(GetAlternateDashChar()))
            {
                return input;
            }

            return pre + s.TrimStart().TrimStart(GetDashChar(), GetAlternateDashChar()).TrimStart();
        }

        /// <summary>
        /// Generates a dialog style preview string based on the specified dialog type.
        /// </summary>
        /// <param name="dialogType">The type of dialog formatting to use for generating the preview.</param>
        /// <returns>A formatted string representation of the dialog style preview.</returns>
        public static string GetDialogStylePreview(DialogType dialogType)
        {
            var line1 = "Lorem ipsum dolor sit amet.";
            var line2 = "Donec eget turpis consequat.";

            switch (dialogType)
            {
                case DialogType.DashBothLinesWithoutSpace: 
                    return "-" + line1 + "\n" + "-" + line2;
                case DialogType.DashSecondLineWithSpace:
                    return line1 + "\n" + "- " + line2;
                case DialogType.DashSecondLineWithoutSpace:
                    return line1 + "\n" + "-" + line2;
                default:
                    return "- " + line1 + "\n" + "- " + line2;
            }
        }

        /// <summary>
        /// Determines the dialog style based on the provided index.
        /// </summary>
        /// <param name="index">The index representing a specific dialog style.</param>
        /// <returns>The corresponding DialogType based on the index provided.</returns>
        public static DialogType GetDialogStyleFromIndex(int index)
        {
            switch (index)
            {
                case 1:
                    return DialogType.DashBothLinesWithoutSpace;
                case 2:
                    return DialogType.DashSecondLineWithSpace;
                case 3:
                    return DialogType.DashSecondLineWithoutSpace;
                default:
                    return DialogType.DashBothLinesWithSpace;
            }
        }

        /// <summary>
        /// Extracts and returns the starting tags (such as formatting tags) from the beginning of the given input string.
        /// </summary>
        /// <param name="input">The input string potentially containing tags at the beginning.</param>
        /// <returns>A string containing all starting tags extracted from the input.</returns>
        private static string GetStartTags(string input)
        {
            var pre = new StringBuilder();
            var s = input;
            if (s.StartsWith("{\\") && s.Contains('}'))
            {
                pre.Append(s.Substring(0, s.IndexOf('}') + 1));
                s = s.Remove(0, pre.Length);
            }

            while (s.StartsWith("<") && s.Contains('>'))
            {
                var htmlPre = s.Substring(0, s.IndexOf('>') + 1);
                s = s.Remove(0, htmlPre.Length);
                pre.Append(htmlPre);
            }

            return pre.ToString();
        }

        /// <summary>
        /// Determines whether a given list of lines represents a dialog based on specific rules and conditions.
        /// </summary>
        /// <param name="lines">A list of strings, where each string is a line of text to be evaluated.</param>
        /// <returns>True if the lines represent a dialog, otherwise false.</returns>
        public bool IsDialog(List<string> lines)
        {
            if (lines.Count < 2 || lines.Count > 3)
            {
                return false;
            }

            var l0 = HtmlUtil.RemoveHtmlTags(lines[0]);
            var l1 = HtmlUtil.RemoveHtmlTags(lines[1], true);
            var noLineEnding = SkipLineEndingCheck || LanguageAutoDetect.IsLanguageWithoutPeriods(TwoLetterLanguageCode);

            if (lines.Count == 2)
            {
                if ((l0.HasSentenceEnding(TwoLetterLanguageCode) || noLineEnding) &&
                    (l1.TrimStart().StartsWith(GetDashChar()) || l1.TrimStart().StartsWith(GetAlternateDashChar())))
                {
                    return true;
                }

                if ((l0.TrimStart().StartsWith(GetDashChar()) || l0.TrimStart().StartsWith(GetAlternateDashChar())) &&
                    l0.EndsWith("--", StringComparison.Ordinal) && !l0.EndsWith("---", StringComparison.Ordinal) &&
                    (l1.TrimStart().StartsWith(GetDashChar()) || l1.TrimStart().StartsWith(GetAlternateDashChar())))
                {
                    var trimmed = l0.TrimEnd('-').TrimEnd();
                    if (trimmed.Length > 0 && char.IsLetterOrDigit(trimmed[trimmed.Length - 1]))
                    {
                        return true;
                    }
                }
            }

            if (lines.Count == 3)
            {
                var l2 = HtmlUtil.RemoveHtmlTags(lines[2], true);

                // - I'm fine today, but I would have
                // been better if I had a some candy.
                // - How are you?
                if (IsDialogThreeLinesTwoOne(l0, l1, l2))
                {
                    return true;
                }

                // - How are you?
                // - I'm fine today, but I would have
                // been better if I had a some candy.
                if (IsDialogThreeLinesOneTwo(l0, l1, l2))
                {
                    return true;
                }

                if ((l0.HasSentenceEnding(TwoLetterLanguageCode) || noLineEnding) &&
                    (l1.TrimStart().StartsWith(GetDashChar()) || l1.TrimStart().StartsWith(GetAlternateDashChar())) &&
                    (l1.HasSentenceEnding(TwoLetterLanguageCode) || noLineEnding) &&
                    (l2.TrimStart().StartsWith(GetDashChar()) || l2.TrimStart().StartsWith(GetAlternateDashChar())))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if the provided list of lines represents a dialog based on the given paragraphs.
        /// </summary>
        /// <param name="lines">A list of strings representing the lines of text to check.</param>
        /// <param name="p">The current paragraph containing the text.</param>
        /// <param name="prev">The previous paragraph which might influence the dialog style of the current paragraph.</param>
        /// <returns>True if the text lines represent a dialog, otherwise false.</returns>
        public bool IsDialog(List<string> lines, Paragraph p, Paragraph prev)
        {
            if (lines.Count < 2 || lines.Count > 3)
            {
                return false;
            }

            var l0 = HtmlUtil.RemoveHtmlTags(lines[0]);
            var l1 = HtmlUtil.RemoveHtmlTags(lines[1], true);
            var noLineEnding = SkipLineEndingCheck || LanguageAutoDetect.IsLanguageWithoutPeriods(TwoLetterLanguageCode);

            if (lines.Count == 2)
            {
                if ((l0.HasSentenceEnding(TwoLetterLanguageCode) || noLineEnding) &&
                    (l1.TrimStart().StartsWith(GetDashChar()) || l1.TrimStart().StartsWith(GetAlternateDashChar())))
                {
                    return true;
                }

                var prevEnding = prev == null ||
                    prev.Text.HasSentenceEnding(TwoLetterLanguageCode) ||
                    p.StartTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds > 3000;
                if (prevEnding &&
                    (l0.StartsWith(GetDashChar()) || l0.StartsWith(GetAlternateDashChar())) &&
                    l0.HasSentenceEnding(TwoLetterLanguageCode))
                {
                    return true;
                }
            }

            if (lines.Count == 3)
            {
                var l2 = HtmlUtil.RemoveHtmlTags(lines[2], true);

                // - I'm fine today, but I would have
                // been better if I had a some candy.
                // - How are you?
                if (IsDialogThreeLinesTwoOne(l0, l1, l2))
                {
                    return true;
                }

                // - How are you?
                // - I'm fine today, but I would have
                // been better if I had a some candy.
                if (IsDialogThreeLinesOneTwo(l0, l1, l2))
                {
                    return true;
                }

                if ((l0.HasSentenceEnding(TwoLetterLanguageCode) || noLineEnding) &&
                    (l1.TrimStart().StartsWith(GetDashChar()) || l1.TrimStart().StartsWith(GetAlternateDashChar())) &&
                    (l1.HasSentenceEnding(TwoLetterLanguageCode) || noLineEnding) &&
                    (l2.TrimStart().StartsWith(GetDashChar()) || l2.TrimStart().StartsWith(GetAlternateDashChar())))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if the provided lines represent a three-line dialog where the first line ends with a sentence-ending punctuation,
        /// the second line starts with a dash, and the third line does not start with a dash.
        /// </summary>
        /// <param name="l0">The first line of the dialog.</param>
        /// <param name="l1">The second line of the dialog.</param>
        /// <param name="l2">The third line of the dialog.</param>
        /// <returns>True if the lines represent a three-line dialog in the specified format, otherwise false.</returns>
        private bool IsDialogThreeLinesOneTwo(string l0, string l1, string l2)
        {
            return l0.HasSentenceEnding(TwoLetterLanguageCode) &&
                   (l1.TrimStart().StartsWith(GetDashChar()) || l1.TrimStart().StartsWith(GetAlternateDashChar())) &&
                   !l1.HasSentenceEnding(TwoLetterLanguageCode) &&
                   !(l2.TrimStart().StartsWith(GetDashChar()) || l2.TrimStart().StartsWith(GetAlternateDashChar()));
        }

        /// <summary>
        /// Checks if the given lines follow a specific dialog pattern where the first line does not end with a sentence,
        /// the second line ends with a sentence, and the third line starts with a dash.
        /// </summary>
        /// <param name="l0">The first line of the dialog.</param>
        /// <param name="l1">The second line of the dialog.</param>
        /// <param name="l2">The third line of the dialog.</param>
        /// <returns>
        /// True if the lines match the specified three-line dialog pattern; otherwise, false.
        /// </returns>
        private bool IsDialogThreeLinesTwoOne(string l0, string l1, string l2)
        {
            return !l0.HasSentenceEnding(TwoLetterLanguageCode) &&
                   l1.HasSentenceEnding(TwoLetterLanguageCode) &&
                   !(l1.TrimStart().StartsWith(GetDashChar()) || l1.TrimStart().StartsWith(GetAlternateDashChar())) &&
                   (l2.TrimStart().StartsWith(GetDashChar())) || l2.TrimStart().StartsWith(GetAlternateDashChar());
        }

        /// <summary>
        /// Determines the line start based on the dash style for the dialog and the index of the line.
        /// </summary>
        /// <param name="lineIndex">The index of the line in the dialog.</param>
        /// <returns>A string representing the start of the line based on the dialog style and line index.</returns>
        private string GetLineStartFromDashStyle(int lineIndex)
        {
            switch (DialogStyle)
            {
                case DialogType.DashBothLinesWithSpace:
                    return GetDashChar() + " ";
                case DialogType.DashBothLinesWithoutSpace:
                    return GetDashChar().ToString();
                case DialogType.DashSecondLineWithSpace:
                    return lineIndex == 0 ? string.Empty : GetDashChar() + " ";
                case DialogType.DashSecondLineWithoutSpace:
                    return lineIndex == 0 ? string.Empty : GetDashChar().ToString();
                default:
                    return string.Empty;
            }
        }
    }
}
