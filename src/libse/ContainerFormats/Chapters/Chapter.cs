using System;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Chapters
{
    /// <summary>
    /// A single chapter mark: a point in the video with a title. Container neutral - Matroska, MP4
    /// and the chapter file formats all read and write this same shape.
    /// </summary>
    /// <remarks>
    /// A chapter has no end time of its own: it runs until the next chapter starts. End times only
    /// appear in the file formats that insist on them (ffmetadata, ordered Matroska chapters), and
    /// are derived on the way out rather than stored here.
    /// </remarks>
    public class Chapter
    {
        public double StartMilliseconds { get; set; }

        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// True for a Matroska sub-chapter (a ChapterAtom inside another ChapterAtom). Kept so a
        /// nested source round-trips visibly rather than silently flattening into its parent.
        /// </summary>
        public bool Nested { get; set; }

        public Chapter()
        {
        }

        public Chapter(double startMilliseconds, string title)
        {
            StartMilliseconds = startMilliseconds;
            Title = title ?? string.Empty;
        }

        public TimeSpan StartTime => TimeSpan.FromMilliseconds(StartMilliseconds);

        public override string ToString() => $"{StartTime:hh\\:mm\\:ss\\.fff} {Title}";
    }
}
