using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;

public partial class PartsSavedViewModel : ObservableObject
{
    [ObservableProperty] private string _xPartsSavedToFolder;
    [ObservableProperty] private string _folderName;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private IFolderHelper _folderHelper;

    public PartsSavedViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;

        OkPressed = false;
        XPartsSavedToFolder = string.Empty; ;
        FolderName = string.Empty;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void OpenFolder()
    {
        if (Window == null || string.IsNullOrWhiteSpace(FolderName))
        {
            return;
        }

        _folderHelper.OpenFolder(Window, FolderName);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Initialize(string outputFolder, int count, Core.SubtitleFormats.SubtitleFormat subtitleFormat)
    {
        XPartsSavedToFolder = string.Format(Se.Language.Tools.SplitSubtitle.XPartsSavedInFormatYToFolder, count, subtitleFormat.Name);
        FolderName = outputFolder;
    }
}