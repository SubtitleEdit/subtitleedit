using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.GoToLineNumber;

public partial class GoToLineNumberViewModel : ObservableObject
{
    [ObservableProperty] private int? _lineNumber;
    [ObservableProperty] private int _maxLineNumber;
    [ObservableProperty] private string _totalText;
    [ObservableProperty] private string _previewTime;
    [ObservableProperty] private string _previewText;
    [ObservableProperty] private bool _isPreviewVisible;
    
    public Window? Window { get; set; }
    public NumericUpDown UpDown { get; set; }

    public bool OkPressed { get; private set; }

    private Func<int, (string Time, string Text)?>? _getPreview;

    public GoToLineNumberViewModel()
    {
        LineNumber = 1;
        MaxLineNumber = 100;
        UpDown = new NumericUpDown();   
        _totalText = string.Empty;
        _previewTime = string.Empty;
        _previewText = string.Empty;
    }

    /// <param name="getPreview">
    /// The time and text of a line, or null when it has none, shown under the field so the line
    /// can be recognized before going there. Leave it out for lists without time codes.
    /// </param>
    public void Initialize(int currentLineNumber, int maxLineNumber, Func<int, (string Time, string Text)?>? getPreview = null)
    {
        _getPreview = getPreview;
        MaxLineNumber = maxLineNumber;
        TotalText = string.Format(Se.Language.General.OfX, maxLineNumber);
        LineNumber = currentLineNumber > 0 && currentLineNumber <= maxLineNumber ? currentLineNumber : 1;
        UpdatePreview();
    }

    partial void OnLineNumberChanged(int? value) => UpdatePreview();

    private void UpdatePreview()
    {
        var preview = LineNumber is { } number ? _getPreview?.Invoke(number) : null;
        PreviewTime = preview?.Time ?? string.Empty;
        PreviewText = preview?.Text ?? string.Empty;
        IsPreviewVisible = preview != null;
    }

    [RelayCommand]
    private void Ok()
    {
        if (LineNumber > MaxLineNumber)
        {
            LineNumber = MaxLineNumber;
        }
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
            e.Handled = true; // the OK button is IsDefault and would run OK again on the same Enter
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