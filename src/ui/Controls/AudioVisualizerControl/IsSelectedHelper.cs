using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

public class IsSelectedHelper
{
    private SelectionRange[] _ranges = Array.Empty<SelectionRange>();
    private int _rangeCount;
    private int _lastPosition = int.MaxValue;
    private SelectionRange _nextSelection;

    /// <summary>
    /// Loads the selection ranges that can affect the sample window
    /// [<paramref name="windowStartSample"/>, <paramref name="windowEndSample"/>] - i.e. the
    /// horizontal span the waveform is about to draw.
    /// </summary>
    /// <remarks>
    /// Keeping only the ranges that intersect the window matters: <see cref="FindNextSelection"/>
    /// re-scans every loaded range each time the per-pixel walk passes the current one, so with
    /// "select all" on a large subtitle the unfiltered list turned the waveform rebuild into
    /// O(pixels x selection). Filtering is exact, not an approximation - <see cref="IsSelected"/>
    /// is only ever asked about positions inside the window, and a range outside it can never
    /// contain one.
    /// </remarks>
    public void Reset(List<SubtitleLineViewModel> paragraphs, int sampleRate, int windowStartSample, int windowEndSample)
    {
        var count = paragraphs.Count;
        if (_ranges.Length < count)
        {
            Array.Resize(ref _ranges, count);
        }

        var kept = 0;
        for (var index = 0; index < count; index++)
        {
            var p = paragraphs[index];
            var start = (int)Math.Round(p.StartTime.TotalSeconds * sampleRate);
            if (start > windowEndSample)
            {
                continue;
            }

            var end = (int)Math.Round(p.EndTime.TotalSeconds * sampleRate);
            if (end < windowStartSample)
            {
                continue;
            }

            _ranges[kept++] = new SelectionRange(start, end);
        }

        _rangeCount = kept;
        _lastPosition = int.MaxValue;
        _nextSelection = new SelectionRange(int.MaxValue, int.MaxValue);
    }

    public bool IsSelected(int position)
    {
        if (position < _lastPosition || position > _nextSelection.End)
        {
            FindNextSelection(position);
        }

        _lastPosition = position;

        return position >= _nextSelection.Start && position <= _nextSelection.End;
    }

    private void FindNextSelection(int position)
    {
        _nextSelection = new SelectionRange(int.MaxValue, int.MaxValue);
        for (var index = 0; index < _rangeCount; index++)
        {
            var range = _ranges[index];
            if (range.End >= position && (range.Start < _nextSelection.Start || range.Start == _nextSelection.Start && range.End > _nextSelection.End))
            {
                _nextSelection = range;
            }
        }
    }

    private readonly struct SelectionRange
    {
        public readonly int Start;
        public readonly int End;

        public SelectionRange(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}
