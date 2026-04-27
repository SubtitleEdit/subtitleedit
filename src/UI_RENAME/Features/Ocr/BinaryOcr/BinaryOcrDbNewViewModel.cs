using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

public partial class BinaryOcrDbNewViewModel : ObservableObject
{
    [ObservableProperty] private string _databaseName;
    [ObservableProperty] private string _title;

    public Window? Window { get; set; }
    public bool OkPressed { get; set; }

    public BinaryOcrDbNewViewModel()
    {
        DatabaseName = string.Empty;
        Title = string.Empty;
    }

    [RelayCommand]
    private void Ok()
    {
        if (string.IsNullOrWhiteSpace(DatabaseName))
        {
            return;
        }

        OkPressed = true;
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

    internal void TextBoxDatabaseNameKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Ok();
        }
    }

    internal void Initialize(string title, string databaseName)
    {
        Title = title;
        DatabaseName = databaseName;
    }
}
