using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core
{
    public abstract class FilePathComparer : IEqualityComparer<string>, IComparer<string>
    {
        private sealed class WindowsFilePathComparer : FilePathComparer
        {
            public override int Compare(string path1, string path2)
            {
                return ReferenceEquals(path1, path2)
                     ? 0
                     : ReferenceEquals(path2, null)
                     ? 1
                     : ReferenceEquals(path1, null)
                     ? -1
                     : string.Compare(path1.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), path2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
            }

            public override bool Equals(string path1, string path2)
            {
                return ReferenceEquals(path1, path2) ||
                       (!(ReferenceEquals(path1, null) || ReferenceEquals(path2, null)) &&
                        string.Equals(path1.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), path2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase));
            }

            public override int GetHashCode(string path)
            {
                return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToUpperInvariant().GetHashCode();
            }
        }

        private sealed class UnixFilePathComparer : FilePathComparer
        {
            public override int Compare(string path1, string path2)
            {
                return ReferenceEquals(path1, path2)
                     ? 0
                     : ReferenceEquals(path2, null)
                     ? 1
                     : ReferenceEquals(path1, null)
                     ? -1
                     : string.Compare(path1.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), path2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), StringComparison.Ordinal);
            }

            public override bool Equals(string path1, string path2)
            {
                return ReferenceEquals(path1, path2) ||
                       (!(ReferenceEquals(path1, null) || ReferenceEquals(path2, null)) &&
                        string.Equals(path1.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), path2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), StringComparison.Ordinal));
            }

            public override int GetHashCode(string path)
            {
                return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).GetHashCode();
            }
        }

        public static FilePathComparer Native => Configuration.IsRunningOnWindows ? Windows : Unix;
        public static FilePathComparer Windows => new WindowsFilePathComparer();
        public static FilePathComparer Unix => new UnixFilePathComparer();

        public abstract int Compare(string path1, string path2);
        public abstract bool Equals(string path1, string path2);
        public abstract int GetHashCode(string path);
    }
}
