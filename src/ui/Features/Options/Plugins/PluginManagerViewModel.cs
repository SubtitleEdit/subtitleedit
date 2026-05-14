using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.Plugins;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Plugins;

public partial class PluginManagerViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<PluginDisplayItem> _plugins;
    [ObservableProperty] private PluginDisplayItem? _selectedPlugin;

    public Window? Window { get; set; }

    private readonly IPluginCatalog _pluginCatalog;
    private readonly IFolderHelper _folderHelper;
    private readonly IWindowService _windowService;

    public PluginManagerViewModel(IPluginCatalog pluginCatalog, IFolderHelper folderHelper, IWindowService windowService)
    {
        _pluginCatalog = pluginCatalog;
        _folderHelper = folderHelper;
        _windowService = windowService;
        _plugins = new ObservableCollection<PluginDisplayItem>();
    }

    public void Initialize()
    {
        LoadPlugins();
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
