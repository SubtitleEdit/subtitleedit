using System;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// While active, Configuration.Settings.General.CurrentFrameRate reads and writes a value of
    /// the current thread only, starting at the value it had when the scope began.
    /// <para>
    /// Many formats write the frame rate a file declares into CurrentFrameRate while checking or
    /// loading it (and the default <see cref="SubtitleFormats.SubtitleFormat.IsMine"/> saves and
    /// restores it), then decode the file's frames with it. Loading several files in parallel
    /// would mix those values up - inside a scope each thread keeps its own, and the global value
    /// is left alone.
    /// </para>
    /// </summary>
    public sealed class ThreadFrameRateScope : IDisposable
    {
        [ThreadStatic]
        private static bool _active;

        [ThreadStatic]
        private static double _frameRate;

        private readonly bool _wasActive;
        private readonly double _previousFrameRate;
        private bool _disposed;

        private ThreadFrameRateScope(double startFrameRate)
        {
            _wasActive = _active;
            _previousFrameRate = _frameRate;
            _frameRate = startFrameRate;
            _active = true;
        }

        /// <summary>
        /// Starts a scope on the current thread. Dispose it on the same thread.
        /// </summary>
        public static ThreadFrameRateScope Begin()
        {
            return new ThreadFrameRateScope(Configuration.Settings.General.CurrentFrameRate);
        }

        internal static bool TryGet(out double frameRate)
        {
            frameRate = _frameRate;
            return _active;
        }

        internal static bool TrySet(double frameRate)
        {
            if (!_active)
            {
                return false;
            }

            _frameRate = frameRate;
            return true;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _frameRate = _previousFrameRate;
            _active = _wasActive;
        }
    }
}
