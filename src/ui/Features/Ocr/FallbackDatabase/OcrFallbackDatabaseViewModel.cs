using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Ocr.FallbackDatabase;

public partial class OcrFallbackDatabaseViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _engineName;
    [ObservableProperty] private string _label;
    [ObservableProperty] private ObservableCollection<string> _databases;
    [ObservableProperty] private string? _selectedDatabase;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public OcrFallbackDatabaseViewModel()
    {
        Title = Se.Language.Ocr.PickFallbackDatabase;
        EngineName = string.Empty;
        Label = Se.Language.Ocr.FallbackOcrDatabase;
        Databases = new ObservableCollection<string>();
    }

    public void Initialize(string engineName, string label, IEnumerable<string> databases, string? selected)
    {
        EngineName = engineName;
        Label = label;
        Databases = new ObservableCollection<string>(databases);
        SelectedDatabase = !string.IsNullOrEmpty(selected) && Databases.Contains(selected)
            ? selected
            : Databases.Count > 0 ? Databases[0] : null;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Ok();
        }
    }
}
