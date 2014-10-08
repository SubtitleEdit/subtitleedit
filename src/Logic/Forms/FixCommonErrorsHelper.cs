using System;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Logic.Forms
{
    public static class FixCommonErrorsHelper
    {
        public static string FixEllipsesStartHelper(string text)
        {
            if (text == null || text.Trim().Length < 4)
                return text;
            if (!text.Contains(".."))
                return text;

            if (text.StartsWith("..."))
            {
                text = text.TrimStart('.').TrimStart();
            }

            text = text.Replace("-..", "- ..");
            var tag = "- ...";
            if (text.StartsWith(tag))
            {
                text = "- " + text.Substring(tag.Length, text.Length - tag.Length);
                while (text.StartsWith("- ."))
                {
                    text = "- " + text.Substring(3, text.Length - 3);
                    text = text.Replace("  ", " ");
                }
            }

            tag = "<i>...";
            if (text.StartsWith(tag))
            {
                text = "<i>" + text.Substring(tag.Length, text.Length - tag.Length);
                while (text.StartsWith("<i>."))
                    text = "<i>" + text.Substring(4, text.Length - 4);
                while (text.StartsWith("<i> "))
                    text = "<i>" + text.Substring(4, text.Length - 4);
            }
            tag = "<i> ...";
            if (text.StartsWith(tag))
            {
                text = "<i>" + text.Substring(tag.Length, text.Length - tag.Length);
                while (text.StartsWith("<i>."))
                    text = "<i>" + text.Substring(4, text.Length - 4);
                while (text.StartsWith("<i> "))
                    text = "<i>" + text.Substring(4, text.Length - 4);
            }

            tag = "- <i>...";
            if (text.StartsWith(tag))
            {
                text = "- <i>" + text.Substring(tag.Length, text.Length - tag.Length);
                while (text.StartsWith("- <i>."))
                    text = "- <i>" + text.Substring(6, text.Length - 6);
            }
            tag = "- <i> ...";
            if (text.StartsWith(tag))
            {
                text = "- <i>" + text.Substring(tag.Length, text.Length - tag.Length);
                while (text.StartsWith("- <i>."))
                    text = "- <i>" + text.Substring(6, text.Length - 6);
            }

            // Narrator:... Hello foo!
            text = text.Replace(":..", ": ..");
            tag = ": ..";
            if (text.Contains(tag))
            {
                text = text.Replace(": ..", ": ");
                while (text.Contains(": ."))
                    text = text.Replace(": .", ": ");
            }

            // <i>- ... Foo</i>
            tag = "<i>- ...";
            if (text.StartsWith(tag))
            {
                text = text.Substring(tag.Length, text.Length - tag.Length);
                text = text.TrimStart('.', ' ');
                text = "<i>- " + text;
            }
            text = text.Replace("  ", " ");

            // WOMAN 2: <i>...24 hours a day at BabyC.</i>
            var index = text.IndexOf(':');
            if (index > 0 && text.Length > index + 1 && !char.IsDigit(text[index + 1]) && text.Contains(".."))
            {
                var post = text.Substring(0, index + 1);
                if (post.Length < 2)
                    return text;

                text = text.Remove(0, index + 1);
                text = text.Trim();
                text = FixEllipsesStartHelper(text);
                text = post + " " + text;
            }
            return text;
        }

        public static string FixDialogsOnOneLine(string text, string language)
        {
            if (text.Contains(" - ") && !text.Contains(Environment.NewLine))
            {
                string[] parts = text.Replace(" - ", Environment.NewLine).Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    string part0 = Utilities.RemoveHtmlTags(parts[0]).Trim();
                    string part1 = Utilities.RemoveHtmlTags(parts[1]).Trim();
                    if (part0.Length > 1 && "-—!?.\"".Contains(part0[part0.Length - 1]) &&
                        part1.Length > 1 && ("'" + Utilities.UppercaseLetters).Contains(part1[0]))
                    {
                        text = text.Replace(" - ", Environment.NewLine + "- ");
                        if (Utilities.AllLettersAndNumbers.Contains(part0[0]))
                        {
                            if (text.StartsWith("<i>"))
                                text = "<i>- " + text;
                            else
                                text = "- " + text;
                        }
                    }
                }
            }

            if ((text.Contains(". -") || text.Contains("! -") || text.Contains("? -") || text.Contains("— -") || text.Contains("-- -")) && Utilities.CountTagInText(text, Environment.NewLine) == 1)
            {
                string temp = Utilities.AutoBreakLine(text, 99, 33, language);
                var arr = text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                var arrTemp = temp.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length == 2 && arrTemp.Length == 2 && !arr[1].TrimStart().StartsWith('-') && arrTemp[1].TrimStart().StartsWith('-'))
                    text = temp;
                else if (arr.Length == 2 && arrTemp.Length == 2 && !arr[1].TrimStart().StartsWith("<i>-") && arrTemp[1].TrimStart().StartsWith("<i>-"))
                    text = temp;
            }
            else if ((text.Contains(". -") || text.Contains("! -") || text.Contains("? -") || text.Contains("-- -") || text.Contains("— -")) && !text.Contains(Environment.NewLine))
            {
                string temp = Utilities.AutoBreakLine(text, language);
                var arrTemp = temp.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                if (arrTemp.Length == 2)
                {
                    if (arrTemp[1].TrimStart().StartsWith('-') || arrTemp[1].TrimStart().StartsWith("<i>-"))
                        text = temp;
                }
                else
                {
                    int index = text.IndexOf(". -", StringComparison.Ordinal);
                    if (index < 0)
                        index = text.IndexOf("! -", StringComparison.Ordinal);
                    if (index < 0)
                        index = text.IndexOf("? -", StringComparison.Ordinal);
                    if (index < 0)
                        index = text.IndexOf("— -", StringComparison.Ordinal);
                    if (index < 0 && text.IndexOf("-- -", StringComparison.Ordinal) > 0)
                        index = text.IndexOf("-- -", StringComparison.Ordinal) + 1;
                    if (index > 0)
                    {
                        text = text.Remove(index + 1, 1).Insert(index + 1, Environment.NewLine);
                        text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                        text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                    }
                }
            }
            return text;
        }

    }
}