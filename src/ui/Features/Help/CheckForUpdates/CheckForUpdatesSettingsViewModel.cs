using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Help.CheckForUpdates;

/// <summary>
/// The update settings reachable from the check-for-updates window: the same two values the
/// Settings window's "Updates" section edits (startup check and release channel), saved the same way.
/// </summary>
public partial class CheckForUpdatesSettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _checkForUpdatesOnStartup;
    [ObservableProperty] private ObservableCollection<string> _updateChannels;
    [ObservableProperty] private string _selectedUpdateChannel;

    private string _loadedUpdateChannel;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>True when the user picked another channel and pressed OK - the opener re-checks then.</summary>
    public bool ChannelChanged { get; private set; }

    /// <summary>Store-managed installs (Flatpak) update through the store, so the startup check is hidden there.</summary>
    public bool IsStartupCheckVisible => !UpdateCheckService.IsStoreManagedInstall;

    public CheckForUpdatesSettingsViewModel()
    {
        UpdateChannels =
        [
            Se.Language.Options.Settings.CheckForUpdatesChannelStable,
            Se.Language.Options.Settings.CheckForUpdatesChannelStableAndBeta,
        ];
        SelectedUpdateChannel = UpdateChannels[0];
        _loadedUpdateChannel = SelectedUpdateChannel;
    }

    public void LoadValues()
    {
        CheckForUpdatesOnStartup = Se.Settings.General.CheckForUpdatesOnStartup;
        SelectedUpdateChannel = UpdateCheckService.ChannelDisplayName();
        _loadedUpdateChannel = SelectedUpdateChannel;
    }

    [RelayCommand]
    private void Ok()
    {
        var general = Se.Settings.General;
        general.CheckForUpdatesOnStartup = CheckForUpdatesOnStartup;
        if (SelectedUpdateChannel != _loadedUpdateChannel)
        {
            // Only an actual change is stored, so an untouched dropdown keeps the
            // "auto" default (beta users follow betas, stable users stable only).
            general.CheckForUpdatesChannel = UpdateCheckService.ChannelFromDisplayName(SelectedUpdateChannel);
            ChannelChanged = true;
        }

        Se.SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    public void OnKeyDown(KeyEventArgs e)
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
