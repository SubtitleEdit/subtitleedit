using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Media;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;

public partial class PromptFileSavedViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _text;
    [ObservableProperty] private bool _isShowInFolderVisible;
    [ObservableProperty] private bool _isShowFileVisible;

    public Window? Window { get; set; }

    private string _fileName;
    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;

    public PromptFileSavedViewModel(IFileHelper fileHelper, IFolderHelper folderHelper)
    {
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;

        _fileName = string.Empty;
        Title = string.Empty;
        Text = string.Empty;
    }

    internal void Initialize(string title, string text, string fileName, bool isShowInFolderVisible, bool isShowFileVisible)
    {
        Title = title;
        Text = text;
        _fileName = fileName;
        IsShowInFolderVisible = isShowInFolderVisible;
        IsShowFileVisible = isShowFileVisible;
    }

    [RelayCommand]
    private async Task ShowInFolder()
    {
        if (Window == null)
        {

            return;
        }

        await _folderHelper.OpenFolderWithFileSelected(Window, _fileName);
    }

    [RelayCommand]
    private void ShowFile()
    {
        if (Window == null)
        {
            return;
        }

        FileHelper.OpenFileWithDefaultProgram(_fileName);
    }

    [RelayCommand]
    private void Ok()
    {
        Window?.Close();
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