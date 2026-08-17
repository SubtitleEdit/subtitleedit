using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Chapters
{
    /// <summary>
    /// Converts between <see cref="Chapter"/> lists and <see cref="Subtitle"/>, so the chapter file
    /// formats can plug into the normal subtitle machinery (open, save as, batch convert, seconv).
    /// </summary>
    public static class ChapterHelper
    {
        /// <summary>
        /// How long the final chapter is made to look when nothing says where it ends. Only affects
        /// display: writers that need an end time for the last chapter use this same value, and
        /// readers never treat it as meaningful.
        /// </summary>
        public const double LastChapterDurationMilliseconds = 4000;

        /// <summary>
        /// Chapters are points, so each one is stretched to meet the next and the list becomes a
        /// gapless subtitle. That keeps durations in the grid sensible instead of showing every
        /// chapter as an arbitrary few seconds.
        /// </summary>
        public static Subtitle ToSubtitle(IEnumerable<Chapter> chapters)
        {
            var subtitle = new Subtitle();
            var list = chapters.OrderBy(p => p.StartMilliseconds).ToList();
            for (var i = 0; i < list.Count; i++)
            {
                var chapter = list[i];
                var end = i + 1 < list.Count
                    ? list[i + 1].StartMilliseconds
                    : chapter.StartMilliseconds + LastChapterDurationMilliseconds;

                subtitle.Paragraphs.Add(new Paragraph(chapter.Title, chapter.StartMilliseconds, end));
            }

            subtitle.Renumber();
            return subtitle;
        }

        public static List<Chapter> FromSubtitle(Subtitle subtitle)
        {
            return subtitle.Paragraphs
                .Select(p => new Chapter(p.StartTime.TotalMilliseconds, ToTitle(p.Text)))
                .OrderBy(p => p.StartMilliseconds)
                .ToList();
        }

        /// <summary>
        /// A chapter title is a single line of plain text in every format here, so tags and line
        /// breaks from a subtitle being converted to chapters have to go.
        /// </summary>
        public static string ToTitle(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return HtmlUtil.RemoveHtmlTags(text, true)
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Trim();
        }

        /// <summary>
        /// End time for the chapter at <paramref name="index"/>, for the formats that require one.
        /// </summary>
        public static double GetEndMilliseconds(IList<Chapter> chapters, int index)
        {
            return index + 1 < chapters.Count
                ? chapters[index + 1].StartMilliseconds
                : chapters[index].StartMilliseconds + LastChapterDurationMilliseconds;
        }
    }
}
