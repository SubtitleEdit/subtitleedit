using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

/// <summary>
/// Memoizes formatted time codes for the grid's time converters.
/// </summary>
/// <remarks>
/// Every visible row formats its start and end time twice and its duration twice: once for the
/// cell, and once more for the row's accessible name, which is a MultiBinding over the same
/// values through the same converter instances (see InitListViewAndEditBox). So half of the
/// time-code formatting a repaint does - and half of the strings it allocates - is a value that
/// was just produced. Scrolling back over rows that were already shown hits too.
///
/// UI thread only, like the <c>TimeCode</c> instance the converters reuse for formatting.
/// </remarks>
internal sealed class TimeCodeDisplayCache
{
    // A viewport is a few dozen rows; the cap only exists so a long session of dragging time
    // codes cannot grow this without bound. Dropped wholesale, like the waveform's text caches.
    private const int Limit = 4096;

    private readonly Dictionary<long, string> _byTicks = new(256);

    // Everything outside the time value itself that the formatters consult. Held as a snapshot
    // rather than folded into the key so a hit stays a single lookup; a change drops the cache,
    // which is what happens when the user flips the time format or sets a video offset.
    private bool _frameMode;
    private double _frameRate;
    private double _videoOffsetMs;

    public bool TryGet(long ticks, out string value)
    {
        var general = Se.Settings.General;
        var frameMode = general.UseFrameMode;
        var frameRate = Configuration.Settings.General.CurrentFrameRate;
        var videoOffsetMs = general.CurrentVideoOffsetInMs;

        if (_frameMode != frameMode || _frameRate != frameRate || _videoOffsetMs != videoOffsetMs)
        {
            _frameMode = frameMode;
            _frameRate = frameRate;
            _videoOffsetMs = videoOffsetMs;
            _byTicks.Clear();
            value = string.Empty;
            return false;
        }

        return _byTicks.TryGetValue(ticks, out value!);
    }

    public void Set(long ticks, string value)
    {
        if (_byTicks.Count >= Limit)
        {
            _byTicks.Clear();
        }

        _byTicks[ticks] = value;
    }
}
