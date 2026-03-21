using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

public class IsSelectedHelper
{
    private readonly SelectionRange[] _ranges;
    private int _lastPosition = int.MaxValue;
    private SelectionRange _nextSelection;

    public IsSelectedHelper(List<SubtitleLineViewModel> paragraphs, int sampleRate)
    {
        var count = paragraphs.Count;
        _ranges = new SelectionRange[count];
        for (var index = 0; index < count; index++)
        {
            var p = paragraphs[index];
            var start = (int)Math.Round(p.StartTime.TotalSeconds * sampleRate);
            var end = (int)Math.Round(p.EndTime.TotalSeconds * sampleRate);
            _ranges[index] = new SelectionRange(start, end);
        }
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
        for (var index = 0; index < _ranges.Length; index++)
        {
            var range = _ranges[index];
            if (range.End >= position && (range.Start < _nextSelection.Start || range.Start == _nextSelection.Start && range.End > _nextSelection.End))
            {
                _nextSelection = range;
            }
        }
    }

    private struct SelectionRange
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