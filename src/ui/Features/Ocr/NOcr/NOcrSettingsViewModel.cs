using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class NOcrSettingsViewModel : ObservableObject
{
    public Window? Window { get; set; }

    [ObservableProperty] private string _actionText;

    public bool EditPressed { get; set; }
    public bool DeletePressed { get; set; }
    public bool NewPressed { get; set; }
    public bool RenamePressed { get; set; }
    public bool TrainPressed { get; set; }
    public bool OptimizePressed { get; set; }
    private NOcrDb _nOcrDb;

    public NOcrSettingsViewModel()
    {
        ActionText = string.Empty;
        _nOcrDb = new NOcrDb(string.Empty);
    }

    public void Initialize(NOcrDb nOcrDb)
    {
        _nOcrDb = nOcrDb;
        var name = Path.GetFileNameWithoutExtension(nOcrDb.FileName);
        ActionText = string.Format(Se.Language.Ocr.NOcrSelectAction, name);
    }

    [RelayCommand]
    private void Edit()
    {
        EditPressed = true;
        Close();
    }

    [RelayCommand]
    private async Task Delete()
    {
        var name = Path.GetFileNameWithoutExtension(_nOcrDb.FileName);
        var totalItemsCount = _nOcrDb.TotalCharacterCount;
        var answer = await MessageBox.Show(
           Window!,
           Se.Language.Ocr.NOcrDeleteDatabase,
           string.Format(Se.Language.Ocr.NOcrDeleteDatabaseDetails, name, totalItemsCount),   
           MessageBoxButtons.YesNoCancel,
           MessageBoxIcon.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        DeletePressed = true;
        Close();
    }

    [RelayCommand]
    private void New()
    {
        NewPressed = true;
        Close();
    }

    [RelayCommand]
    private void Rename()
    {
        RenamePressed = true;
        Close();
    }

    [RelayCommand]
    private void Train()
    {
        TrainPressed = true;
        Close();
    }

    [RelayCommand]
    private void Optimize()
    {
        OptimizePressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/ocr", "nocr-nikse-ocr");
        }
    }
}
