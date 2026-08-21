using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Copies file system timestamps from a source file onto written output files, so a
    /// converted subtitle can keep the "modified" date of the file it was made from.
    /// </summary>
    public static class FileTimestampHelper
    {
        /// <summary>
        /// Copies creation and last-write time from <paramref name="sourceFileName"/> onto
        /// <paramref name="targetPath"/> (a file or a directory). Best-effort: returns false
        /// and leaves the target untouched if either path is missing or the OS refuses
        /// (e.g. read-only media, unsupported creation time on some Linux file systems).
        /// </summary>
        public static bool CopyTimestamps(string sourceFileName, string targetPath)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceFileName) || string.IsNullOrEmpty(targetPath) || !File.Exists(sourceFileName))
                {
                    return false;
                }

                var source = new FileInfo(sourceFileName);
                if (File.Exists(targetPath))
                {
                    var ok = TrySet(() => File.SetLastWriteTimeUtc(targetPath, source.LastWriteTimeUtc));
                    TrySet(() => File.SetCreationTimeUtc(targetPath, source.CreationTimeUtc));
                    return ok;
                }

                if (Directory.Exists(targetPath))
                {
                    var ok = TrySet(() => Directory.SetLastWriteTimeUtc(targetPath, source.LastWriteTimeUtc));
                    TrySet(() => Directory.SetCreationTimeUtc(targetPath, source.CreationTimeUtc));
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
            try
            {
                if (!Directory.Exists(directory))
                {
                    return;
                }

                foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                {
                    CopyTimestamps(sourceFileName, file);
                }

                CopyTimestamps(sourceFileName, directory);
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
