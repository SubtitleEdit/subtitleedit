using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public partial class BurnInSettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _useSourceFolder;
    [ObservableProperty] private bool _useOutputFolder;
    [ObservableProperty] private string _outputFolder;
    [ObservableProperty] private string _suffix;
    [ObservableProperty] private string _suffixExample;
    
    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IFolderHelper _folderHelper;

    public BurnInSettingsViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;

        OutputFolder = string.Empty;
        Suffix = string.Empty;
        SuffixExample = string.Empty;
        LoadSettings();
    }

    private void LoadSettings()
    {
        UseSourceFolder = !Se.Settings.Video.BurnIn.UseOutputFolder; 
        UseOutputFolder = Se.Settings.Video.BurnIn.UseOutputFolder;
        OutputFolder = Se.Settings.Video.BurnIn.OutputFolder;
        Suffix = Se.Settings.Video.BurnIn.BurnInSuffix ?? string.Empty;
    }

    partial void OnSuffixChanged(string value)
    {
        var ext = string.IsNullOrEmpty(Se.Settings.Video.BurnIn.OutputExtension) ? ".mkv" : Se.Settings.Video.BurnIn.OutputExtension;
        SuffixExample = string.Format(Se.Language.Video.OutputFileNameExampleX, "video" + value + ext);
    }

    private void SaveSettings()
    {
        Se.Settings.Video.BurnIn.UseOutputFolder = UseOutputFolder;
        Se.Settings.Video.BurnIn.OutputFolder = OutputFolder;
        Se.Settings.Video.BurnIn.BurnInSuffix = Suffix ?? string.Empty;
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

        if (!string.IsNullOrEmpty(Suffix) && Suffix.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            await MessageBox.Show(Window!, Se.Language.General.Error,
                Se.Language.Video.OutputFileNameSuffixInvalid, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    internal void OnKeyDown(KeyEventArgs e) => UiUtil.CloseOnEscapeOrShowHelp(e, Window, "features/burn-in", "video-settings");
}