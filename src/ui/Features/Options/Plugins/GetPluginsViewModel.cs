using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Plugins;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Plugins;

public partial class GetPluginsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<GetPluginsDisplayItem> _plugins;
    [ObservableProperty] private GetPluginsDisplayItem? _selectedPlugin;
    [ObservableProperty] private string _statusMessage;
    [ObservableProperty] private bool _isLoading;

    public Window? Window { get; set; }

    /// <summary>True when at least one plugin was installed/updated during this session.</summary>
    public bool InstalledAnything { get; private set; }

    private readonly IPluginDownloadService _downloadService;
    private readonly IPluginCatalog _pluginCatalog;
    private CancellationTokenSource? _cancellationTokenSource;

    public GetPluginsViewModel(IPluginDownloadService downloadService, IPluginCatalog pluginCatalog)
    {
        _downloadService = downloadService;
        _pluginCatalog = pluginCatalog;
        _plugins = new ObservableCollection<GetPluginsDisplayItem>();
        _statusMessage = string.Empty;
    }

    public void Initialize()
    {
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = Se.Language.Plugins.LoadingPluginList;
        Plugins.Clear();

        try
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var index = await _downloadService.GetIndexAsync(_cancellationTokenSource.Token);
            var installed = _pluginCatalog.GetPlugins();

            foreach (var entry in index.Plugins.OrderBy(p => p.Name))
            {
                var match = installed.FirstOrDefault(p =>
                    p.Manifest.Name.Equals(entry.Name, StringComparison.OrdinalIgnoreCase));
                Plugins.Add(new GetPluginsDisplayItem(entry, match?.Manifest.Version));
            }

            SelectedPlugin = Plugins.FirstOrDefault();
            StatusMessage = Plugins.Count == 0
                ? Se.Language.Plugins.NoPluginsAvailable
                : string.Format(Se.Language.Plugins.XPluginsAvailable, Plugins.Count);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Failed to load plugin index");
            StatusMessage = string.Format(Se.Language.Plugins.CouldNotLoadPluginList, exception.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Install(GetPluginsDisplayItem? item)
    {
        if (item == null || item.IsBusy)
        {
            return;
        }

        item.IsBusy = true;
        item.DownloadProgress = 0;
        var previousStatus = item.StatusText;
        var progress = new Progress<float>(p =>
        {
            item.DownloadProgress = p * 100;
            item.StatusText = string.Format(Se.Language.Plugins.DownloadingXPercent, (int)Math.Round(p * 100));
        });

        try
        {
            _cancellationTokenSource = new CancellationTokenSource();
            await _downloadService.InstallAsync(item.Entry, progress, _cancellationTokenSource.Token);
            InstalledAnything = true;

            var match = _pluginCatalog.GetPlugins().FirstOrDefault(p =>
                p.Manifest.Name.Equals(item.Entry.Name, StringComparison.OrdinalIgnoreCase));
            item.Refresh(match?.Manifest.Version);
            StatusMessage = string.Format(Se.Language.Plugins.PluginXInstalled, item.Name);
        }
        catch (OperationCanceledException)
        {
            item.StatusText = previousStatus;
            StatusMessage = Se.Language.General.Cancelled;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Failed to install plugin: " + item.Name);
            item.StatusText = previousStatus;
            StatusMessage = string.Format(Se.Language.Plugins.CouldNotInstallPluginX, item.Name, exception.Message);
        }
        finally
        {
            item.IsBusy = false;
        }
    }

    [RelayCommand]
    private void CancelDownload()
    {
        _cancellationTokenSource?.Cancel();
    }

    [RelayCommand]
    private void Close()
    {
        _cancellationTokenSource?.Cancel();
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}
