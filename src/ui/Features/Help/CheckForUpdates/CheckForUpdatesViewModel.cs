using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Help.CheckForUpdates;

public partial class CheckForUpdatesViewModel : ObservableObject
{
    private const string ReleasesUrl = "https://github.com/SubtitleEdit/subtitleedit/releases";

    private readonly IUpdateCheckService _updateCheckService;
    private readonly IAutoUpdateService _autoUpdateService;
    private readonly IWindowService _windowService;
    private string _latestVersion = string.Empty;
    private CancellationTokenSource? _downloadCancellation;

    public CheckForUpdatesViewModel(IUpdateCheckService updateCheckService, IAutoUpdateService autoUpdateService, IWindowService windowService)
    {
        _updateCheckService = updateCheckService;
        _autoUpdateService = autoUpdateService;
        _windowService = windowService;
    }

    public Window? Window { get; set; }

    [ObservableProperty] private string _statusText = string.Empty;

    [ObservableProperty] private string _changeLogText = string.Empty;

    [ObservableProperty] private bool _isDownloadLinkVisible;

    [ObservableProperty] private bool _isInstallVisible;

    [ObservableProperty] private bool _isDownloading;

    [ObservableProperty] private double _downloadProgress;

    public async Task CheckForUpdates()
    {
        if (IsDownloading)
        {
            return; // re-activated while the update downloads
        }

        StatusText = Se.Language.Help.CheckForUpdatesChecking;
        IsDownloadLinkVisible = false;
        IsInstallVisible = false;

        try
        {
            var result = await _updateCheckService.CheckForUpdates();
            ChangeLogText = result.ChangeLogText;

            if (string.IsNullOrEmpty(result.LatestVersion))
            {
                StatusText = Se.Language.Help.CheckForUpdatesUnableToCheck;
                return;
            }

            if (result.IsNewVersionAvailable && _autoUpdateService.IsInstallPending)
            {
                // Downloaded and handed to the helper earlier in this session (the user stayed
                // at the unsaved-changes prompt) - it installs when Subtitle Edit closes.
                StatusText = Se.Language.Help.CheckForUpdatesInstallsWhenClosed;
                IsDownloadLinkVisible = true;
            }
            else if (result.IsNewVersionAvailable)
            {
                StatusText = string.Format(Se.Language.Help.CheckForUpdatesNewVersionAvailable, result.LatestVersion);
                IsDownloadLinkVisible = true;
                IsInstallVisible = _autoUpdateService.Kind != AutoUpdateKind.None;
                _latestVersion = result.LatestVersion;
            }
            else
            {
                StatusText = Se.Language.Help.CheckForUpdatesUpToDate;
            }
        }
        catch
        {
            StatusText = Se.Language.Help.CheckForUpdatesUnableToCheck;
        }
    }

    [RelayCommand]
    private async Task OpenDownloadPage()
    {
        if (Window == null)
        {
            return;
        }

        UiUtil.OpenUrl(ReleasesUrl);
    }

    [RelayCommand]
    private async Task InstallUpdate()
    {
        if (IsDownloading || string.IsNullOrEmpty(_latestVersion))
        {
            return;
        }

        if (_autoUpdateService.IsInstallPending)
        {
            IsInstallVisible = false;
            StatusText = Se.Language.Help.CheckForUpdatesInstallsWhenClosed;
            return;
        }

        IsInstallVisible = false;
        IsDownloading = true;
        DownloadProgress = 0;
        StatusText = string.Format(Se.Language.General.DownloadingXDotDotDot, _latestVersion);
        _downloadCancellation = new CancellationTokenSource();
        var progress = new Progress<float>(p => DownloadProgress = p * 100.0);

        try
        {
            var file = await _autoUpdateService.DownloadAsync(_latestVersion, progress, _downloadCancellation.Token);
            _autoUpdateService.StartInstallAfterExit(file);
        }
        catch (OperationCanceledException) when (_downloadCancellation?.IsCancellationRequested == true)
        {
            return; // dialog closed while downloading
        }
        catch (Exception exception) // also timeouts, which surface as OperationCanceledException
        {
            Se.LogError(exception, "Automatic update");
            IsDownloading = false;
            StatusText = Se.Language.Help.CheckForUpdatesInstallFailed;
            return;
        }

        // The helper now waits for this process to exit. Closing the editor windows still asks
        // about unsaved changes; if the user stays, the update installs whenever they quit.
        IsDownloading = false;
        StatusText = Se.Language.Help.CheckForUpdatesInstallsWhenClosed;
        Window?.Close();
        Dispatcher.UIThread.Post(MainWindowFactory.CloseAllMainWindows);
    }

    [RelayCommand]
    private async Task OpenSettings()
    {
        if (Window == null || IsDownloading)
        {
            return;
        }

        // The window re-checks on re-activation (see CheckForUpdatesWindow), which happens when
        // this dialog closes - so a changed channel shows its releases without a second call here.
        await _windowService.ShowDialogAsync<CheckForUpdatesSettingsWindow, CheckForUpdatesSettingsViewModel>(Window, vm => vm.LoadValues());
    }

    public void OnClosed()
    {
        _downloadCancellation?.Cancel();
    }

    [RelayCommand]
    private void Ok()
    {
        Window?.Close();
    }

    public void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/settings", "updates");
        }
    }
}
