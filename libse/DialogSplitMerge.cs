using System;
using Nikse.SubtitleEdit.Core.Enums;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public class DialogSplitMerge
    {
        public DialogType DialogStyle { get; set; }

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

            var sb = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                var l = lines[i];
                var pre = GetStartTags(l);
                l = l.Remove(0, pre.Length);
                switch (DialogStyle)
                {
                    case DialogType.DashBothLinesWithSpace:
                        if (!l.TrimStart().StartsWith(GetDashChar()))
                        {
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.TrimStart());
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    case DialogType.DashSecondLineWithSpace:
                        if (i > 0 && !l.TrimStart().StartsWith(GetDashChar()))
                        {
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.TrimStart());
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
                        if (!l.TrimStart().StartsWith(GetDashChar()))
                        {
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.TrimStart());
                        }
                        else
                        {
                            sb.AppendLine(pre + l);
                        }
                        break;
                    case DialogType.DashSecondLineWithoutSpace:
                        if (i > 0 && !l.TrimStart().StartsWith(GetDashChar()))
                        {
                            sb.AppendLine(pre + GetLineStartFromDashStyle(i) + l.TrimStart());
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

            if (!s.TrimStart().StartsWith('-'))
            {
                return input;
            }

            return pre + s.TrimStart().TrimStart('-').TrimStart();
        }


        private static string GetStartTags(string input)
        {
            var pre = string.Empty;
            var s = input;
            if (s.StartsWith("{\\") && s.Contains('}'))
            {
                pre = s.Substring(0, s.IndexOf('}') + 1);
                s = s.Remove(0, pre.Length);
            }

            while (s.StartsWith("<") && s.Contains('>'))
            {
                var htmlPre = s.Substring(0, s.IndexOf('>') + 1);
                s = s.Remove(0, htmlPre.Length);
                pre += htmlPre;
            }

            return pre;
        }

        private static bool IsDialog(List<string> lines)
        {
            if (lines.Count < 2 || lines.Count > 3)
            {
                return false;
            }

            var l0 = HtmlUtil.RemoveHtmlTags(lines[0]).TrimEnd('"');
            var l1 = HtmlUtil.RemoveHtmlTags(lines[1], true);

            if (lines.Count == 2)
            {
                if ((l0.EndsWith('.') || l0.EndsWith('!') || l0.EndsWith('?')) &&
                    l1.TrimStart().StartsWith('-'))
                {
                    return true;
                }
            }

            if (lines.Count == 3)
            {
                var l2 = HtmlUtil.RemoveHtmlTags(lines[2], true);
                if (l0.HasSentenceEnding() &&
                    l1.TrimStart().StartsWith(GetDashChar()) &&
                    l1.HasSentenceEnding() &&
                    l2.TrimStart().StartsWith(GetDashChar()))
                {
                    return true;
                }

                // - I'm fine today, but I would have
                // been better if I had a some candy.
                // - How are you?
                if (!l0.HasSentenceEnding() &&
                    l1.HasSentenceEnding() &&
                    !l1.TrimStart().StartsWith(GetDashChar()) &&
                    l2.TrimStart().StartsWith(GetDashChar()))
                {
                    return true;
                }

                // - How are you?
                // - I'm fine today, but I would have
                // been better if I had a some candy.
                if (l0.HasSentenceEnding() &&
                    l1.TrimStart().StartsWith(GetDashChar()) &&
                    !l1.HasSentenceEnding() &&
                    !l2.TrimStart().StartsWith(GetDashChar()))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetLineStartFromDashStyle(int lineIndex)
        {
            switch (DialogStyle)
            {
                case DialogType.DashBothLinesWithSpace:
                    return "- ";
                case DialogType.DashBothLinesWithoutSpace:
                    return "-";
                case DialogType.DashSecondLineWithSpace:
                    return lineIndex == 0 ? string.Empty : "- ";
                case DialogType.DashSecondLineWithoutSpace:
                    return lineIndex == 0 ? string.Empty : "-";
                default:
                    return string.Empty;
            }
        }

        private static char GetDashChar() => '-';
    }
}
