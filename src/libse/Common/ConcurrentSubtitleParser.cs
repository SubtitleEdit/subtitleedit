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
    /// (error counts, batch flags) - each thread gets its own instances instead; and many formats
    /// write a file's declared frame rate into Configuration.Settings.General.CurrentFrameRate -
    /// each parse runs in a <see cref="ThreadFrameRateScope"/>, so that value is per thread.
    /// </para>
    /// <para>
    /// A side effect: each file is parsed with the frame rate as it was when parsing started
    /// (not whatever an earlier file declared), and the global frame rate is not changed.
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
            using (ThreadFrameRateScope.Begin())
            {
                return Subtitle.Parse(fileName, null, Gate);
            }
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
                    return check(format);
                }
            }
        }
    }
}
