using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Plugins;

namespace Nikse.SubtitleEdit.Features.Options.Plugins;

public partial class GetPluginsDisplayItem : ObservableObject
{
    private static readonly IBrush AccentBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _statusText = string.Empty;
    [ObservableProperty] private string _actionText = string.Empty;
    [ObservableProperty] private double _downloadProgress;

    public PluginIndexEntry Entry { get; }

    public string Name => Entry.Name;
    public string Version => Entry.Version;
    public string Author => Entry.Author;
    public string Description => Entry.Description;
    public string Date => Entry.Date;

    public bool IsInstalled { get; private set; }
    public bool UpdateAvailable { get; private set; }
    public bool IsSupportedOnCurrentPlatform { get; }
    public bool NotBusy => !IsBusy;
    public bool CanInstall => NotBusy && IsSupportedOnCurrentPlatform;

    public IBrush? StatusBrush => UpdateAvailable ? AccentBrush : null;
    public FontWeight StatusFontWeight => UpdateAvailable ? FontWeight.SemiBold : FontWeight.Normal;
    public double StatusOpacity => UpdateAvailable ? 1.0 : 0.6;

    public string DownloadProgressText => $"{(int)System.Math.Round(DownloadProgress)}%";

    public GetPluginsDisplayItem(PluginIndexEntry entry, string? installedVersion)
    {
        Entry = entry;
        IsSupportedOnCurrentPlatform = PluginPlatform.IsSupportedByEntry(entry);
        Refresh(installedVersion);
    }

    public void Refresh(string? installedVersion)
    {
        if (!IsSupportedOnCurrentPlatform)
        {
            IsInstalled = false;
            UpdateAvailable = false;
            StatusText = Se.Language.Plugins.NotSupportedOnThisOs;
            ActionText = Se.Language.Plugins.Install;
            NotifyStatusVisualsChanged();
            return;
        }

        if (string.IsNullOrEmpty(installedVersion))
        {
            IsInstalled = false;
            UpdateAvailable = false;
            StatusText = Se.Language.Plugins.NotInstalled;
            ActionText = Se.Language.Plugins.Install;
            NotifyStatusVisualsChanged();
            return;
        }

        IsInstalled = true;
        UpdateAvailable = IsNewer(Entry.Version, installedVersion);
        if (UpdateAvailable)
        {
            StatusText = string.Format(Se.Language.Plugins.UpdateAvailableXToY, installedVersion, Entry.Version);
            ActionText = Se.Language.Plugins.Update;
        }
        else
        {
            StatusText = string.Format(Se.Language.Plugins.InstalledX, installedVersion);
            ActionText = Se.Language.Plugins.Reinstall;
        }
        NotifyStatusVisualsChanged();
    }

    private void NotifyStatusVisualsChanged()
    {
        OnPropertyChanged(nameof(UpdateAvailable));
        OnPropertyChanged(nameof(IsInstalled));
        OnPropertyChanged(nameof(StatusBrush));
        OnPropertyChanged(nameof(StatusFontWeight));
        OnPropertyChanged(nameof(StatusOpacity));
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(NotBusy));
        OnPropertyChanged(nameof(CanInstall));
    }

    partial void OnDownloadProgressChanged(double value)
    {
        OnPropertyChanged(nameof(DownloadProgressText));
    }

    private static bool IsNewer(string candidate, string current)
    {
        return System.Version.TryParse(candidate, out var c) &&
               System.Version.TryParse(current, out var cur) &&
               c > cur;
    }
}
