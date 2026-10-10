using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.Logic.Platform.Progress;

internal sealed class PlatformProgressGroup(Func<IPlatformProgress> createProgress, bool keepBackendWhenIdle = false)
{
    private readonly List<(object Source, double? Percentage, bool Indeterminate)> _sources = [];
    private IPlatformProgress? _progress;
    private (object Source, int? Percentage, bool Indeterminate)? _lastUpdate;

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
                _lastUpdate = null;
                // Keep the Linux connection alive so asynchronous clear requests can be sent.
                if (!keepBackendWhenIdle)
                {
                    _progress?.Dispose();
                    _progress = null;
                }
                return;
            }

            _progress ??= createProgress();
            var current = _sources[^1];
            var effectivePercentage = !current.Indeterminate && current.Percentage is { } value
                ? (int?)value : null;
            // Source changes must be published even when their visible state is the same.
            if (_lastUpdate is { } last && ReferenceEquals(last.Source, current.Source) &&
                last.Percentage == effectivePercentage && last.Indeterminate == current.Indeterminate)
                return;

            _progress.Update(effectivePercentage, current.Indeterminate);
            _lastUpdate = (current.Source, effectivePercentage, current.Indeterminate);
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Platform progress unavailable: {exception.Message}");
        }
    }
}
