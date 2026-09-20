using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// Keeps one error from flooding the error log. A caller that fails on a timer logs the same
/// text at 6-60 Hz; unthrottled, that took a real error-log.txt to 2.8 million lines (100 MB)
/// of one three-line entry.
/// <para>
/// Each distinct text is written at most <see cref="MaxEntriesPerWindow"/> times per
/// <see cref="WindowMilliseconds"/>; the rest are only counted, and the count is reported on
/// the next entry that is written. Different errors never throttle each other.
/// </para>
/// </summary>
internal sealed class ErrorLogThrottle
{
    internal const int MaxEntriesPerWindow = 5;
    internal const long WindowMilliseconds = 60_000;
    internal const int MaxTrackedErrors = 64;

    private sealed class Entry
    {
        public long WindowStart;
        public int Written;
        public int Suppressed;
    }

    private readonly Dictionary<string, Entry> _entries = new();
    private readonly object _lock = new();

    /// <summary>
    /// Whether <paramref name="error"/> should be written now. <paramref name="suppressedBefore"/>
    /// is how many identical entries were dropped since the last one written, and
    /// <paramref name="isLastInWindow"/> is set on the entry after which the rest of the window
    /// is only counted.
    /// </summary>
    public bool ShouldLog(string error, long nowMilliseconds, out int suppressedBefore, out bool isLastInWindow)
    {
        lock (_lock)
        {
            if (!_entries.TryGetValue(error, out var entry))
            {
                if (_entries.Count >= MaxTrackedErrors)
                {
                    Trim(nowMilliseconds);
                }

                entry = new Entry { WindowStart = nowMilliseconds };
                _entries[error] = entry;
            }
            else if (nowMilliseconds - entry.WindowStart >= WindowMilliseconds)
            {
                entry.WindowStart = nowMilliseconds;
                entry.Written = 0;
            }

            if (entry.Written >= MaxEntriesPerWindow)
            {
                entry.Suppressed++;
                suppressedBefore = 0;
                isLastInWindow = false;
                return false;
            }

            entry.Written++;
            suppressedBefore = entry.Suppressed;
            entry.Suppressed = 0;
            isLastInWindow = entry.Written == MaxEntriesPerWindow;
            return true;
        }
    }

    // Drops the errors whose window has run out - or, when every tracked error is still live,
    // the one that started longest ago - so a run of distinct errors cannot grow this forever.
    private void Trim(long nowMilliseconds)
    {
        var expired = _entries
            .Where(p => nowMilliseconds - p.Value.WindowStart >= WindowMilliseconds)
            .Select(p => p.Key)
            .ToList();

        if (expired.Count == 0)
        {
            expired.Add(_entries.MinBy(p => p.Value.WindowStart).Key);
        }

        foreach (var key in expired)
        {
            _entries.Remove(key);
        }
    }
}
