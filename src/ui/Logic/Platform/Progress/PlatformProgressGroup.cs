using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.Logic.Platform.Progress;

internal sealed class PlatformProgressGroup(Func<IPlatformProgress> createProgress)
{
    private readonly List<(object Source, double? Percentage, bool Indeterminate)> _sources = [];
    private IPlatformProgress? _progress;

    public void Update(object source, double? percentage, bool indeterminate)
    {
        var index = _sources.FindIndex(item => ReferenceEquals(item.Source, source));
        if (percentage.HasValue || indeterminate)
        {
            percentage = percentage.HasValue ? Math.Clamp(double.IsFinite(percentage.Value) ? percentage.Value : 0, 0, 100) : null;
            if (index < 0)
                _sources.Add((source, percentage, indeterminate));
            else
                _sources[index] = (source, percentage, indeterminate);
        }
        else if (index >= 0)
        {
            _sources.RemoveAt(index);
        }

        try
        {
            if (_sources.Count == 0)
            {
                _progress?.Update(null, false);
                _progress?.Dispose();
                _progress = null;
                return;
            }

            _progress ??= createProgress();
            var current = _sources[^1];
            _progress.Update(current.Percentage, current.Indeterminate);
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Platform progress unavailable: {exception.Message}");
        }
    }
}
