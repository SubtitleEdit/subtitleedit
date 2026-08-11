using System;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Formatting categories that <see cref="RemoveFormattingUtil"/> can strip from subtitle text.
    /// <see cref="All"/> removes every tag wholesale (HTML plus SSA/ASSA override blocks), which is
    /// broader than the union of the named categories - e.g. positioning tags like <c>{\pos(..)}</c>
    /// only go away with <see cref="All"/>.
    /// </summary>
    [Flags]
    public enum RemoveFormattingType
    {
        None = 0,
        Italic = 1 << 0,
        Bold = 1 << 1,
        Underline = 1 << 2,
        FontName = 1 << 3,
        Alignment = 1 << 4,
        Color = 1 << 5,
        All = 1 << 6,
    }

    /// <summary>
    /// Removes formatting tags from subtitle text. Single source of truth for the GUI's batch
    /// convert "Remove formatting" function and seconv's <c>--remove-formatting</c> /
    /// <c>--remove-formatting-rules</c> options, so both strip tags identically (#13518).
    /// </summary>
    public static class RemoveFormattingUtil
    {
        public static string Remove(string text, RemoveFormattingType types)
        {
            if (string.IsNullOrEmpty(text) || types == RemoveFormattingType.None)
            {
                return text;
            }

            if ((types & RemoveFormattingType.All) != 0)
            {
                return HtmlUtil.RemoveHtmlTags(text, true);
            }

            if ((types & RemoveFormattingType.Italic) != 0)
            {
                text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagItalic);
                text = text
                    .Replace("{\\i}", string.Empty)
                    .Replace("{\\i0}", string.Empty)
                    .Replace("{\\i1}", string.Empty);
            }

            if ((types & RemoveFormattingType.Bold) != 0)
            {
                text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagBold);
                text = text
                    .Replace("{\\b}", string.Empty)
                    .Replace("{\\b0}", string.Empty)
                    .Replace("{\\b1}", string.Empty);
            }

            if ((types & RemoveFormattingType.Underline) != 0)
            {
                text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagUnderline);
                text = text
                    .Replace("{\\u}", string.Empty)
                    .Replace("{\\u0}", string.Empty)
                    .Replace("{\\u1}", string.Empty);
            }

            if ((types & RemoveFormattingType.Color) != 0)
            {
                text = HtmlUtil.RemoveColorTags(text);
                if (text.Contains("\\c") || text.Contains("\\1c"))
                {
                    text = HtmlUtil.RemoveAssaColor(text);
                }
            }

            if ((types & RemoveFormattingType.FontName) != 0)
            {
                text = HtmlUtil.RemoveFontName(text);
            }

            if ((types & RemoveFormattingType.Alignment) != 0 && text.Contains('{'))
            {
                text = HtmlUtil.RemoveAssAlignmentTags(text);
            }

            return text;
        }
    }
}
