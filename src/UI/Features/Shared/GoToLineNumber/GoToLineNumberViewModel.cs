using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Shared.GoToLineNumber;

public partial class GoToLineNumberViewModel : ObservableObject
{
    [ObservableProperty] private int? _lineNumber;
    [ObservableProperty] private int _maxLineNumber;
    
    public Window? Window { get; set; }
    public NumericUpDown UpDown { get; set; }

    public bool OkPressed { get; private set; }

    public GoToLineNumberViewModel()
    {
        LineNumber = 1;
        MaxLineNumber = 100;
        UpDown = new NumericUpDown();   
    }

    public void Initialize(int currentLineNumber, int maxLineNumber)
    {
        LineNumber = currentLineNumber > 0 && currentLineNumber <= maxLineNumber ? currentLineNumber : 1;
        MaxLineNumber = maxLineNumber;
    }

    [RelayCommand]                   
    private void Ok() 
    {
        OkPressed = LineNumber != null;
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void Cancel() 
    {
        Window?.Close();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
        else if (e.Key == Key.Enter)
        {
            Ok();
        }
    }

    public void Activated()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var textBox = UpDown.GetVisualDescendants()
                .OfType<TextBox>()
                .FirstOrDefault();
            textBox?.SelectAll();
            UpDown.Focus(); 
        });
    }
}