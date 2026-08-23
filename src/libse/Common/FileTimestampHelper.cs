using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// A file's creation and last-write time, captured before a conversion so they survive
    /// the conversion overwriting the file itself (batch convert "save in source folder" +
    /// "overwrite", seconv --overwrite): copying from the source path afterwards would just
    /// read back the time the output was written.
    /// </summary>
    public readonly struct FileTimestamps
    {
        public DateTime CreationTimeUtc { get; }
        public DateTime LastWriteTimeUtc { get; }

        public FileTimestamps(DateTime creationTimeUtc, DateTime lastWriteTimeUtc)
        {
            CreationTimeUtc = creationTimeUtc;
            LastWriteTimeUtc = lastWriteTimeUtc;
        }
    }

    /// <summary>
    /// Copies file system timestamps from a source file onto written output files, so a
    /// converted subtitle can keep the "modified" date of the file it was made from.
    /// </summary>
    public static class FileTimestampHelper
    {
        /// <summary>
        /// Reads <paramref name="sourceFileName"/>'s timestamps, or null when the file is missing.
        /// Call before converting; see <see cref="FileTimestamps"/>.
        /// </summary>
        public static FileTimestamps? Capture(string sourceFileName)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceFileName) || !File.Exists(sourceFileName))
                {
                    return null;
                }

                var source = new FileInfo(sourceFileName);
                return new FileTimestamps(source.CreationTimeUtc, source.LastWriteTimeUtc);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Copies creation and last-write time from <paramref name="sourceFileName"/> onto
        /// <paramref name="targetPath"/> (a file or a directory). Best-effort: returns false
        /// and leaves the target untouched if either path is missing or the OS refuses
        /// (e.g. read-only media, unsupported creation time on some Linux file systems).
        /// </summary>
        public static bool CopyTimestamps(string sourceFileName, string targetPath)
        {
            var timestamps = Capture(sourceFileName);
            return timestamps != null && CopyTimestamps(timestamps.Value, targetPath);
        }

        /// <summary>
        /// Stamps <paramref name="targetPath"/> (a file or a directory) with previously captured
        /// timestamps. Best-effort like the path overload.
        /// </summary>
        public static bool CopyTimestamps(FileTimestamps timestamps, string targetPath)
        {
            try
            {
                if (string.IsNullOrEmpty(targetPath))
                {
                    return false;
                }

                if (File.Exists(targetPath))
                {
                    var ok = TrySet(() => File.SetLastWriteTimeUtc(targetPath, timestamps.LastWriteTimeUtc));
                    TrySet(() => File.SetCreationTimeUtc(targetPath, timestamps.CreationTimeUtc));
                    return ok;
                }

                if (Directory.Exists(targetPath))
                {
                    var ok = TrySet(() => Directory.SetLastWriteTimeUtc(targetPath, timestamps.LastWriteTimeUtc));
                    TrySet(() => Directory.SetCreationTimeUtc(targetPath, timestamps.CreationTimeUtc));
                    return ok;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Copies timestamps onto every file inside <paramref name="directory"/> and the
        /// directory itself; used for image exports that write a folder of PNGs + index file.
        /// </summary>
        public static void CopyTimestampsToDirectoryContents(string sourceFileName, string directory)
        {
            var timestamps = Capture(sourceFileName);
            if (timestamps != null)
            {
                CopyTimestampsToDirectoryContents(timestamps.Value, directory);
            }
        }

        /// <summary>See <see cref="CopyTimestampsToDirectoryContents(string, string)"/>.</summary>
        public static void CopyTimestampsToDirectoryContents(FileTimestamps timestamps, string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    return;
                }

                foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                {
                    CopyTimestamps(timestamps, file);
                }

                CopyTimestamps(timestamps, directory);
            }
            catch
            {
                // best-effort
            }
        }

        private static bool TrySet(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
