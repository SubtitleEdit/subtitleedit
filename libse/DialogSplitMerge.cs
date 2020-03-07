using System;
using Nikse.SubtitleEdit.Core.Enums;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public class DialogSplitMerge
    {
        public DialogType DialogStyle { get; set; }
        public bool AllowDialogWithNoSentenceEnding { get; set; }

        public string FixDashesAndSpaces(string input)
        {
            return FixSpaces(FixDashes(input));
        }

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

        public bool IsDialog(List<string> lines)
        {
            if (lines.Count < 2 || lines.Count > 3)
            {
                return false;
            }

            var l0 = HtmlUtil.RemoveHtmlTags(lines[0]);
            var l1 = HtmlUtil.RemoveHtmlTags(lines[1], true);

            if (lines.Count == 2)
            {
                if ((l0.HasSentenceEnding() || AllowDialogWithNoSentenceEnding) && (l1.TrimStart().StartsWith(GetDashChar()) || l1.TrimStart().StartsWith(GetAlternateDashChar())))
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

                if ((l0.HasSentenceEnding() || AllowDialogWithNoSentenceEnding) &&
                    (l1.TrimStart().StartsWith(GetDashChar()) || l1.TrimStart().StartsWith(GetAlternateDashChar())) &&
                    (l1.HasSentenceEnding() || AllowDialogWithNoSentenceEnding) &&
                    (l2.TrimStart().StartsWith(GetDashChar()) || l2.TrimStart().StartsWith(GetAlternateDashChar())))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDialogThreeLinesOneTwo(string l0, string l1, string l2)
        {
            if (l0.HasSentenceEnding() &&
                (l1.TrimStart().StartsWith(GetDashChar()) || l1.TrimStart().StartsWith(GetAlternateDashChar())) &&
                !l1.HasSentenceEnding() &&
                !(l2.TrimStart().StartsWith(GetDashChar()) || l2.TrimStart().StartsWith(GetAlternateDashChar())))
            {
                return true;
            }

            return false;
        }

        private static bool IsDialogThreeLinesTwoOne(string l0, string l1, string l2)
        {
            if (!l0.HasSentenceEnding() &&
                l1.HasSentenceEnding() &&
                !(l1.TrimStart().StartsWith(GetDashChar()) || l1.TrimStart().StartsWith(GetAlternateDashChar())) &&
                (l2.TrimStart().StartsWith(GetDashChar())) || l2.TrimStart().StartsWith(GetAlternateDashChar()))
            {
                return true;
            }

            return false;
        }

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

        private static char GetDashChar() => '-';

        private static char GetAlternateDashChar() => '‐'; // Unicode En Dash
    }
}
