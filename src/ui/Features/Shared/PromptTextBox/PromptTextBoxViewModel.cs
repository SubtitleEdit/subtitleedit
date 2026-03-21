using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Shared.PromptTextBox;

public partial class PromptTextBoxViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _text;
    [ObservableProperty] private int _textBoxWidth;
    [ObservableProperty] private int _textBoxHeight;
    [ObservableProperty] private bool _isReadOnly;

    private bool _returnSubmits;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public PromptTextBoxViewModel()
    {
        Title = string.Empty;
        Text = string.Empty;
    }

    internal void Initialize(string title, string text, int textBoxWidth, int textBoxHeight, bool returnSubmits = false, bool isReadOnly = false)
    {
        Title = title;
        Text = text;
        TextBoxWidth = textBoxWidth;
        TextBoxHeight = textBoxHeight;
        IsReadOnly = isReadOnly;
        _returnSubmits = returnSubmits;
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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Enter && _returnSubmits)
        {
            e.Handled = true;
            Ok();
        }
    }
}