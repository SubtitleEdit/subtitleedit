using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ChatterboxTtsSettings;

public partial class ChatterboxTtsSettingsViewModel : ObservableObject
{
    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;

    [ObservableProperty] private string _engineLabel = string.Empty;
    [ObservableProperty] private IBrush _engineBrush = Brushes.Gray;
    [ObservableProperty] private string _baseModelLabel = string.Empty;
    [ObservableProperty] private IBrush _baseModelBrush = Brushes.Gray;
    [ObservableProperty] private string _turboModelLabel = string.Empty;
    [ObservableProperty] private IBrush _turboModelBrush = Brushes.Gray;
    [ObservableProperty] private string _modelsFolder = string.Empty;
    [ObservableProperty] private string _voicesFolder = string.Empty;
    [ObservableProperty] private bool _isEngineInstalled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public ChatterboxTtsSettingsViewModel(IWindowService windowService, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _folderHelper = folderHelper;
    }

    public void Initialize()
    {
        ModelsFolder = ChatterboxTtsCpp.GetSetModelsFolder();
        VoicesFolder = ChatterboxTtsCpp.GetSetVoicesFolder();
        Refresh();
    }

    private void Refresh()
    {
        IsEngineInstalled = File.Exists(ChatterboxTtsCpp.GetCrispAsrExecutable());
        if (!IsEngineInstalled)
        {
            EngineLabel = "CrispASR not installed — install via Audio to text first";
            EngineBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)); // red
        }
        else if (!ChatterboxTtsCpp.IsCrispAsrChatterboxCapable())
        {
            EngineLabel = "CrispASR too old — update via Audio to text → Engine settings";
            EngineBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)); // amber
        }
        else
        {
            EngineLabel = "CrispASR (Chatterbox-capable)";
            EngineBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)); // green
        }

        ApplyModelStatus(
            ChatterboxTtsCpp.AreModelsInstalled(ChatterboxTtsCpp.ModelKeyBase),
            label => BaseModelLabel = label,
            brush => BaseModelBrush = brush);

        ApplyModelStatus(
            ChatterboxTtsCpp.AreModelsInstalled(ChatterboxTtsCpp.ModelKeyTurbo),
            label => TurboModelLabel = label,
            brush => TurboModelBrush = brush);
    }

    private static void ApplyModelStatus(bool installed, Action<string> setLabel, Action<IBrush> setBrush)
    {
        if (installed)
        {
            setLabel("Installed");
            setBrush(new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50))); // green
        }
        else
        {
            setLabel("Not installed");
            setBrush(new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E))); // grey
        }
    }

    [RelayCommand]
    private async Task RedownloadBaseModels()
    {
        await RedownloadModelsCore(ChatterboxTtsCpp.ModelKeyBase, "~990 MB");
    }

    [RelayCommand]
    private async Task RedownloadTurboModels()
    {
        await RedownloadModelsCore(ChatterboxTtsCpp.ModelKeyTurbo, "~1 GB");
    }

    private async Task RedownloadModelsCore(string modelKey, string sizeText)
    {
        if (Window == null)
        {
            return;
        }

        var answer = await MessageBox.Show(
            Window,
            "Re-download Chatterbox TTS models",
            $"{Environment.NewLine}Download the Chatterbox TTS \"{modelKey}\" models ({sizeText}) now?",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        await _windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(Window, vm => vm.StartDownloadChatterboxModels(modelKey));
        Refresh();
    }

    [RelayCommand]
    private async Task OpenModelsFolder()
    {
        if (Window == null || string.IsNullOrEmpty(ModelsFolder))
        {
            return;
        }

        try
        {
            await _folderHelper.OpenFolder(Window, ModelsFolder);
        }
        catch
        {
            // ignore - best-effort UX
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
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
