using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core
{
    public abstract class FilePathComparer : IEqualityComparer<string>, IComparer<string>
    {
        private sealed class WindowsFilePathComparer : FilePathComparer { }
        private sealed class UnixFilePathComparer : FilePathComparer { }

        public static FilePathComparer Native => Configuration.IsRunningOnWindows ? Windows : Unix;
        public static FilePathComparer Windows => new WindowsFilePathComparer();
        public static FilePathComparer Unix => new UnixFilePathComparer();

        public int Compare(string path1, string path2)
        {
            return ReferenceEquals(path1, path2)
                    ? 0
                    : path2 is null
                    ? 1
                    : path1 is null
                    ? -1
                    : string.Compare(path1.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), path2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), GetComparison());
        }

        public bool Equals(string path1, string path2)
        {
            return ReferenceEquals(path1, path2) ||
                       (!(path1 is null || path2 is null) &&
                        string.Equals(path1.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), path2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), GetComparison()));
        }

        private StringComparison GetComparison() => IsWindowsInstance() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        private bool IsWindowsInstance() => (this is UnixFilePathComparer) == false;

        public int GetHashCode(string path)
        {
            return IsWindowsInstance() ? path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToUpperInvariant().GetHashCode()
                : path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).GetHashCode();
        }
    }
}
