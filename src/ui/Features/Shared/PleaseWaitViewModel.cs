using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared;

/// <summary>
/// Tiny non-modal "Please wait..." window shown while a short background
/// operation runs (e.g. probing the installed yt-dlp version). The caller owns
/// the lifetime: show it via the window service and call <see cref="Close"/>
/// once the work is done.
/// </summary>
public partial class PleaseWaitViewModel : ObservableObject
{
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private bool _isIndeterminate = true;

    private int _lastPercent = -1;

    public Window? Window { get; set; }

    public PleaseWaitViewModel()
    {
        _statusText = Se.Language.General.PleaseWait;
    }

    /// <summary>
    /// Switches the progress bar to determinate mode and updates it (0-100).
    /// Safe to call from a background thread; updates are throttled to whole
    /// percent changes so a tight progress callback does not flood the UI thread.
    /// </summary>
    public void ReportProgress(long position, long total, string? statusText = null)
    {
        if (total <= 0)
        {
            return;
        }

        var percent = (int)(position * 100 / total);
        if (percent < 0)
        {
            percent = 0;
        }
        else if (percent > 100)
        {
            percent = 100;
        }

        if (percent == _lastPercent)
        {
            return;
        }

        _lastPercent = percent;
        Dispatcher.UIThread.Post(() =>
        {
            IsIndeterminate = false;
            ProgressValue = percent;
            if (statusText != null)
            {
                StatusText = statusText;
            }
        });
    }

    public void Close()
    {
        Dispatcher.UIThread.Post(() => Window?.Close());
    }
}
