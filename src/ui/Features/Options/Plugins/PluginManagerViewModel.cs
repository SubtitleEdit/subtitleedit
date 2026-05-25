using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Plugins;

public partial class PluginManagerViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<PluginDisplayItem> _plugins;
    [ObservableProperty] private PluginDisplayItem? _selectedPlugin;
    [ObservableProperty] private int _updateAvailableCount;
    [ObservableProperty] private bool _isUpdating;
    [ObservableProperty] private string _updateAllButtonText = string.Empty;

    public bool HasUpdates => UpdateAvailableCount > 0;
    public bool CanUpdateAll => HasUpdates && !IsUpdating;

    public Window? Window { get; set; }

    private readonly IPluginCatalog _pluginCatalog;
    private readonly IPluginDownloadService _downloadService;
    private readonly IFolderHelper _folderHelper;
    private readonly IWindowService _windowService;
    private CancellationTokenSource? _checkCts;
    private CancellationTokenSource? _updateAllCts;

    public PluginManagerViewModel(IPluginCatalog pluginCatalog, IPluginDownloadService downloadService, IFolderHelper folderHelper, IWindowService windowService)
    {
        _pluginCatalog = pluginCatalog;
        _downloadService = downloadService;
        _folderHelper = folderHelper;
        _windowService = windowService;
        _plugins = new ObservableCollection<PluginDisplayItem>();
        RefreshUpdateAllButtonText();
    }

    public void Initialize()
    {
        LoadPlugins();
        _ = CheckForUpdatesAsync();
    }

    private void LoadPlugins()
    {
        Plugins.Clear();
        foreach (var plugin in _pluginCatalog.GetPlugins().OrderBy(p => p.Manifest.Name))
        {
            var enabled = !Se.Settings.Plugins.DisabledPluginNames.Contains(plugin.Manifest.Name);
            var item = new PluginDisplayItem(plugin, enabled)
            {
                EnabledChanged = OnPluginEnabledChanged,
            };
            Plugins.Add(item);
        }

        SelectedPlugin = Plugins.FirstOrDefault();
        UpdateAvailableCount = 0;
        RefreshUpdateAllButtonText();
    }

    /// <summary>Fetch the online catalog and mark installed plugins that have a newer version.</summary>
    private async Task CheckForUpdatesAsync()
    {
        _checkCts?.Cancel();
        _checkCts = new CancellationTokenSource();
        var token = _checkCts.Token;

        try
        {
            var index = await _downloadService.GetIndexAsync(token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() => ApplyCatalog(index));
        }
        catch
        {
            // Opportunistic check; if the network/index is unavailable we simply
            // don't show the update affordance. No popup, no log spam.
        }
    }

    private void ApplyCatalog(PluginIndex index)
    {
        var entriesByName = index.Plugins
            .Where(p => !string.IsNullOrEmpty(p.Name))
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var count = 0;
        foreach (var item in Plugins)
        {
            if (entriesByName.TryGetValue(item.Name, out var entry) &&
                PluginPlatform.IsSupportedByEntry(entry) &&
                IsNewer(entry.Version, item.Version))
            {
                item.SetAvailableUpdate(entry);
                count++;
            }
            else
            {
                item.SetAvailableUpdate(null);
            }
        }

        UpdateAvailableCount = count;
        RefreshUpdateAllButtonText();
    }

    private static bool IsNewer(string candidate, string current)
    {
        return System.Version.TryParse(candidate, out var c) &&
               System.Version.TryParse(current, out var cur) &&
               c > cur;
    }

    [RelayCommand]
    private async Task UpdateAll()
    {
        if (IsUpdating)
        {
            return;
        }

        var toUpdate = Plugins.Where(p => p.UpdateAvailable && p.AvailableUpdate != null)
            .Select(p => (Item: p, Entry: p.AvailableUpdate!))
            .ToList();
        if (toUpdate.Count == 0)
        {
            return;
        }

        IsUpdating = true;
        var cts = new CancellationTokenSource();
        _updateAllCts = cts;
        try
        {
            var done = 0;
            foreach (var (item, entry) in toUpdate)
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }

                done++;
                UpdateAllButtonText = string.Format(Se.Language.Plugins.UpdatingXOfY, done, toUpdate.Count);
                try
                {
                    await _downloadService.InstallAsync(entry, progress: null, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception exception)
                {
                    Se.LogError(exception, "Failed to update plugin: " + item.Name);
                }
            }

            LoadPlugins();
            await CheckForUpdatesAsync();
        }
        finally
        {
            IsUpdating = false;
            _updateAllCts = null;
            cts.Dispose();
            RefreshUpdateAllButtonText();
        }
    }

    private void RefreshUpdateAllButtonText()
    {
        UpdateAllButtonText = UpdateAvailableCount > 0
            ? string.Format(Se.Language.Plugins.UpdateAllXAvailable, UpdateAvailableCount)
            : string.Empty;
    }

    partial void OnUpdateAvailableCountChanged(int value)
    {
        OnPropertyChanged(nameof(HasUpdates));
        OnPropertyChanged(nameof(CanUpdateAll));
    }

    partial void OnIsUpdatingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanUpdateAll));
    }

    private static void OnPluginEnabledChanged(PluginDisplayItem item)
    {
        var disabled = Se.Settings.Plugins.DisabledPluginNames;
        if (item.IsEnabled)
        {
            disabled.Remove(item.Name);
        }
        else if (!disabled.Contains(item.Name))
        {
            disabled.Add(item.Name);
        }

        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task OpenPluginsFolder()
    {
        if (Window == null)
        {
            return;
        }

        Directory.CreateDirectory(Se.PluginsFolder);
        await _folderHelper.OpenFolder(Window, Se.PluginsFolder);
    }

    [RelayCommand]
    private async Task RemovePlugin()
    {
        if (Window == null || SelectedPlugin == null)
        {
            return;
        }

        var item = SelectedPlugin;
        var answer = await MessageBox.Show(
            Window,
            Se.Language.General.Remove,
            string.Format(Se.Language.Plugins.RemovePluginXConfirm, item.Name, Environment.NewLine + item.FolderPath),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            Directory.Delete(item.FolderPath, recursive: true);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Failed to remove plugin: " + item.Name);
            await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                string.Format(Se.Language.Plugins.CouldNotRemovePluginX, item.Name, exception.Message),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        Se.Settings.Plugins.DisabledPluginNames.Remove(item.Name);
        Se.Settings.Plugins.Settings.Remove(item.Name);
        Se.Settings.Plugins.SettingsVersions.Remove(item.Name);
        Se.SaveSettings();

        Plugins.Remove(item);
        SelectedPlugin = Plugins.FirstOrDefault();
    }

    [RelayCommand]
    private async Task GetPluginsOnline()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<GetPluginsWindow, GetPluginsViewModel>(
            Window, vm => vm.Initialize());

        if (result.InstalledAnything)
        {
            LoadPlugins();
        }
    }

    [RelayCommand]
    private void Close()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
