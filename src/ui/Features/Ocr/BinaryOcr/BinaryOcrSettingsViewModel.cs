using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

public partial class BinaryOcrSettingsViewModel : ObservableObject
{
    public Window? Window { get; set; }

    [ObservableProperty] private string _actionText;

    public bool EditPressed { get; set; }
    public bool DeletePressed { get; set; }
    public bool NewPressed { get; set; }
    public bool RenamePressed { get; set; }
    public string BinaryOcrDatabaseName { get; set; }

    private List<string> _binaryOcrDatabases;
    private BinaryOcrDb? _binaryOcrDb;


    private readonly IWindowService _windowService;

    public BinaryOcrSettingsViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        ActionText = string.Empty;
        _binaryOcrDatabases = new List<string>();
        BinaryOcrDatabaseName = string.Empty;
    }

    public void Initialize(string binaryOcrDatabase)
    {
        _binaryOcrDatabases = BinaryOcrDb.GetDatabases();
        BinaryOcrDatabaseName = binaryOcrDatabase;
        _binaryOcrDb = new  BinaryOcrDb(binaryOcrDatabase);
        var name = Path.GetFileNameWithoutExtension(BinaryOcrDatabaseName);
        ActionText = string.Format("Select action to perform on \"Binary image compare\" database \"{0}\"", name);
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
        var name = Path.GetFileNameWithoutExtension(BinaryOcrDatabaseName);
        var totalItemsCount = 1; // TODO: fix to get actual count from database
        var answer = await MessageBox.Show(
           Window!,
           "Delete Binary Image Compare database?",
           string.Format("Do you want to delete the current \"Binary image compare\" database \"{0}\" with {1:#,###,##0} items?", name, totalItemsCount),   
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
    }
}
