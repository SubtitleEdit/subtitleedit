using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core
{
    public abstract class FilePathComparer : IEqualityComparer<string>, IComparer<string>
    {
        private sealed class WindowsFilePathComparer : FilePathComparer
        {
            public override bool Equals(string path1, string path2)
            {
                return ReferenceEquals(path1, path2) ||
                       (!(ReferenceEquals(path1, null) || ReferenceEquals(path2, null)) &&
                        string.Equals(Normalize(path1), Normalize(path2), StringComparison.OrdinalIgnoreCase));
            }

            public override int GetHashCode(string path) => Normalize(path).ToUpperInvariant().GetHashCode();
        }

        private sealed class UnixFilePathComparer : FilePathComparer
        {
            public override bool Equals(string path1, string path2)
            {
                return ReferenceEquals(path1, path2) ||
                       (!(ReferenceEquals(path1, null) || ReferenceEquals(path2, null)) &&
                        string.Equals(path1.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar),
                        path2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), StringComparison.Ordinal));
            }

            public override int GetHashCode(string path) => Normalize(path).GetHashCode();
        }

        public static FilePathComparer Native => Configuration.IsRunningOnWindows ? Windows : Unix;
        public static FilePathComparer Windows => new WindowsFilePathComparer();
        public static FilePathComparer Unix => new UnixFilePathComparer();

        /// <summary>
        /// Compares two paths using platform specific logic Windows or Unix.
        /// When extending <see cref="FilePathComparer"/>, make sure you override this method so it can give the expenting behaviour. 
        /// </summary>
        /// <returns>Zero path1 and path2 are equals, greater than zero path1 is valid and
        /// different from path2, lesser than zero path2 is valid and different from path1</returns>
        public virtual int Compare(string path1, string path2)
        {
            var comparison = StringComparison.OrdinalIgnoreCase;
            if (Native is UnixFilePathComparer)
            {
                comparison = StringComparison.Ordinal;
            }

            return ReferenceEquals(path1, path2)
                ? 0
                : ReferenceEquals(path2, null)
                ? 1
                : ReferenceEquals(path1, null)
                ? -1
                : string.Compare(Normalize(path1), Normalize(path2), comparison);
        }

        public abstract bool Equals(string path1, string path2);
        public abstract int GetHashCode(string path);

        /// <summary>
        /// Replace platform specific alternate directory separator character with current platform directory separator character.
        /// </summary>
        /// <returns>Normalized path string</returns>
        private string Normalize(string path) => path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

    }
}
