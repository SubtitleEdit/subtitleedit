using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Chapters
{
    /// <summary>
    /// Reads chapters embedded in a video file, whatever the container.
    /// </summary>
    public static class VideoChapterReader
    {
        private static readonly string[] MatroskaExtensions = { ".mkv", ".mks", ".webm", ".mka" };

        private static readonly string[] Mp4Extensions = { ".mp4", ".m4v", ".mov", ".m4a", ".3gp", ".3g2" };

        /// <summary>
        /// Chapters found in <paramref name="videoFileName"/>, or an empty list when the file has
        /// none, is of a container that does not carry chapters, or cannot be read.
        /// </summary>
        public static List<Chapter> GetChapters(string videoFileName)
        {
            if (string.IsNullOrEmpty(videoFileName) || !File.Exists(videoFileName))
            {
                return new List<Chapter>();
            }

            var extension = Path.GetExtension(videoFileName).ToLowerInvariant();

            try
            {
                if (MatroskaExtensions.Contains(extension))
                {
                    return GetMatroskaChapters(videoFileName);
                }

                if (Mp4Extensions.Contains(extension))
                {
                    return GetMp4Chapters(videoFileName);
                }
            }
            catch
            {
                // A container that will not parse simply has no chapters to offer.
            }

            return new List<Chapter>();
        }

        public static bool IsSupportedContainer(string videoFileName)
        {
            if (string.IsNullOrEmpty(videoFileName))
            {
                return false;
            }

            var extension = Path.GetExtension(videoFileName).ToLowerInvariant();
            return MatroskaExtensions.Contains(extension) || Mp4Extensions.Contains(extension);
        }

        private static List<Chapter> GetMatroskaChapters(string videoFileName)
        {
            using (var matroska = new MatroskaFile(videoFileName))
            {
                if (!matroska.IsValid)
                {
                    return new List<Chapter>();
                }

                // The Matroska reader appends a nested atom before the parent it belongs to, so the
                // raw order is not the timeline order - sort rather than trust it.
                return matroska.GetChapters()
                    .Select(p => new Chapter(p.StartTime * 1000.0, p.Name ?? string.Empty) { Nested = p.Nested })
                    .OrderBy(p => p.StartMilliseconds)
                    .ToList();
            }
        }

        private static List<Chapter> GetMp4Chapters(string videoFileName)
        {
            var parser = new MP4Parser(videoFileName);
            return parser.GetChapters();
        }
    }
}
