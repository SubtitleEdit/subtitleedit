using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Help.CheckForUpdates;

public partial class CheckForUpdatesViewModel : ObservableObject
{
    private const string ReleasesUrl = "https://github.com/SubtitleEdit/subtitleedit/releases";

    private readonly IUpdateCheckService _updateCheckService;

    public CheckForUpdatesViewModel(IUpdateCheckService updateCheckService)
    {
        _updateCheckService = updateCheckService;
    }

    public Window? Window { get; set; }

    [ObservableProperty] private string _statusText = string.Empty;

    [ObservableProperty] private string _changeLogText = string.Empty;

    [ObservableProperty] private bool _isDownloadLinkVisible;

    public async Task CheckForUpdates()
    {
        StatusText = Se.Language.Help.CheckForUpdatesChecking;
        IsDownloadLinkVisible = false;

        try
        {
            var result = await _updateCheckService.CheckForUpdates();
            ChangeLogText = result.ChangeLogText;

            if (string.IsNullOrEmpty(result.LatestVersion))
            {
                StatusText = Se.Language.Help.CheckForUpdatesUnableToCheck;
                return;
            }

            if (result.IsNewVersionAvailable)
            {
                StatusText = string.Format(Se.Language.Help.CheckForUpdatesNewVersionAvailable, result.LatestVersion);
                IsDownloadLinkVisible = true;
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
    }
}
