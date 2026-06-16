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

    public Window? Window { get; set; }

    public PleaseWaitViewModel()
    {
        _statusText = Se.Language.General.PleaseWait;
    }

    public void Close()
    {
        Dispatcher.UIThread.Post(() => Window?.Close());
    }
}
