using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Plugins;
using System;

namespace Nikse.SubtitleEdit.Features.Options.Plugins;

public partial class PluginDisplayItem : ObservableObject
{
    [ObservableProperty] private bool _isEnabled;

    public InstalledPlugin Plugin { get; }

    public string Name => Plugin.Manifest.Name;
    public string Version => Plugin.Manifest.Version;
    public string Author => Plugin.Manifest.Author;
    public string Description => Plugin.Manifest.Description;
    public string FolderPath => Plugin.FolderPath;
    public bool CanRun => Plugin.CanRun;

    /// <summary>Catalog entry providing a newer version, or null when up to date / no catalog match.</summary>
    public PluginIndexEntry? AvailableUpdate { get; private set; }

    public bool UpdateAvailable => AvailableUpdate != null;

    public string StatusText => !Plugin.CanRun
        ? Se.Language.Plugins.NotSupportedOnThisOs
        : UpdateAvailable
            ? string.Format(Se.Language.Plugins.UpdateAvailableXToY, Version, AvailableUpdate!.Version)
            : IsEnabled ? Se.Language.Plugins.Enabled : Se.Language.Plugins.Disabled;

    public void SetAvailableUpdate(PluginIndexEntry? entry)
    {
        AvailableUpdate = entry;
        OnPropertyChanged(nameof(AvailableUpdate));
        OnPropertyChanged(nameof(UpdateAvailable));
        OnPropertyChanged(nameof(StatusText));
    }

    /// <summary>Raised when the user toggles the enabled checkbox.</summary>
    public Action<PluginDisplayItem>? EnabledChanged { get; set; }

    public PluginDisplayItem(InstalledPlugin plugin, bool isEnabled)
    {
        Plugin = plugin;
        _isEnabled = isEnabled;
    }

    partial void OnIsEnabledChanged(bool value)
    {
        OnPropertyChanged(nameof(StatusText));
        EnabledChanged?.Invoke(this);
    }
}
