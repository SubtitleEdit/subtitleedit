using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Runs one format check (IsMine + load) for <see cref="Subtitle"/>'s format detection:
    /// picks which instance of <c>format</c> to use and whether to hold a lock.
    /// </summary>
    internal interface IFormatCheckGate
    {
        SubtitleFormat Run(SubtitleFormat format, Func<SubtitleFormat, SubtitleFormat> check);
    }

    /// <summary>
    /// Same detection as <see cref="Subtitle.Parse(string)"/> (same formats, same order, same
    /// UTF-16 retry), but safe to call from several threads at once - for loading many files in
    /// parallel.
    /// <para>
    /// Two things make plain Subtitle.Parse unsafe in parallel, and both are handled here:
    /// the shared <see cref="SubtitleFormat.AllSubtitleFormats"/> instances keep per-instance state
    /// (error counts, batch flags) - each thread gets its own instances instead; and formats that
    /// read a frame rate from the file (TTML, D-Cinema, ...) write it into
    /// Configuration.Settings.General.CurrentFrameRate - the per-thread instances have
    /// <see cref="SubtitleFormat.KeepDeclaredFrameRateLocal"/> set, so they keep it to themselves.
    /// </para>
    /// <para>
    /// A side effect: each file is parsed with the global frame rate (not whatever an earlier
    /// file declared), and the global frame rate is not changed - the rate a file declared is in
    /// the loaded subtitle's OriginalFormat.DeclaredFrameRate.
    /// </para>
    /// </summary>
    public static class ConcurrentSubtitleParser
    {
        /// <summary>
        /// Held while a format runs that cannot get a per-thread instance (no parameterless
        /// constructor), on its shared instance.
        /// </summary>
        private static readonly object SharedInstanceLock = new object();

        private static readonly ConcurrentDictionary<Type, bool> CanCopyType = new ConcurrentDictionary<Type, bool>();

        [ThreadStatic]
        private static Dictionary<Type, SubtitleFormat> _threadFormats;

        private static readonly IFormatCheckGate Gate = new PerThreadGate();

        /// <summary>
        /// Loads <paramref name="fileName"/> like <see cref="Subtitle.Parse(string)"/>.
        /// </summary>
        /// <returns>Loaded subtitle, null if the file is not a known text subtitle format.</returns>
        public static Subtitle Parse(string fileName)
        {
            return Parse(fileName, out _);
        }

        /// <summary>
        /// Loads <paramref name="fileName"/> like <see cref="Subtitle.Parse(string)"/>.
        /// </summary>
        /// <param name="fileName">File to load.</param>
        /// <param name="linesRead">
        /// The file's lines as read for format detection (encoding detected the same way), null if
        /// it could not be read - so more formats can be tried on an unknown file without reading
        /// it and detecting its encoding again.
        /// </param>
        /// <returns>Loaded subtitle, null if the file is not a known text subtitle format.</returns>
        public static Subtitle Parse(string fileName, out List<string> linesRead)
        {
            return Subtitle.Parse(fileName, null, Gate, out linesRead);
        }

        private static bool CanCopy(Type type)
        {
            return CanCopyType.GetOrAdd(type, t => t.GetConstructor(Type.EmptyTypes) != null);
        }

        private static SubtitleFormat GetThreadInstance(SubtitleFormat format)
        {
            var formats = _threadFormats ?? (_threadFormats = new Dictionary<Type, SubtitleFormat>());
            var type = format.GetType();
            if (!formats.TryGetValue(type, out var instance))
            {
                instance = (SubtitleFormat)Activator.CreateInstance(type);
                instance.KeepDeclaredFrameRateLocal = true;
                formats[type] = instance;
            }

            return instance;
        }

        private sealed class PerThreadGate : IFormatCheckGate
        {
            public SubtitleFormat Run(SubtitleFormat format, Func<SubtitleFormat, SubtitleFormat> check)
            {
                if (CanCopy(format.GetType()))
                {
                    return check(GetThreadInstance(format));
                }

                lock (SharedInstanceLock)
                {
                    var keepLocal = format.KeepDeclaredFrameRateLocal;
                    format.KeepDeclaredFrameRateLocal = true;
                    try
                    {
                        return check(format);
                    }
                    finally
                    {
                        format.KeepDeclaredFrameRateLocal = keepLocal;
                    }
                }
            }
        }
    }
}
