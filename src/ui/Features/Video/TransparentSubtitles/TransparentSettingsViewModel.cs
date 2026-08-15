using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TransparentSubtitles;

public partial class TransparentSettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _useSourceFolder;
    [ObservableProperty] private bool _useOutputFolder;
    [ObservableProperty] private string _outputFolder;
    
    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IFolderHelper _folderHelper;

    public TransparentSettingsViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;

        OutputFolder = string.Empty;
        LoadSettings();
    }

    private void LoadSettings()
    {
        UseSourceFolder = !Se.Settings.Video.Transparent.UseOutputFolder; 
        UseOutputFolder = Se.Settings.Video.Transparent.UseOutputFolder;
        // This dialog's own folder - it saves to Video.Transparent.OutputFolder below, so loading
        // burn-in's showed the wrong folder back and hid the fact that the saved one was unused.
        OutputFolder = Se.Settings.Video.Transparent.OutputFolder;
    }

    private void SaveSettings()
    {
        Se.Settings.Video.Transparent.UseOutputFolder = UseOutputFolder;
        Se.Settings.Video.Transparent.OutputFolder = OutputFolder;
        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (UseOutputFolder && string.IsNullOrWhiteSpace(OutputFolder))
        {
            await MessageBox.Show(Window!, Se.Language.General.Error,
                Se.Language.General.PleaseSelectOutputFolder, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task BrowseOutputFolder()
    {
        var folder = await _folderHelper.PickFolderAsync(Window!, Se.Language.General.PickOutputFolder);
        if (!string.IsNullOrEmpty(folder))
        {
            OutputFolder = folder;
            UseOutputFolder = true;
            UseSourceFolder = false;
        }
    }

    [RelayCommand]
    private void Cancel()
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