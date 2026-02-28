using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class TextEditorBindingHelper
{
    private readonly MainViewModel _vm;
    private readonly TextEditor _textEditor;
    private readonly TextEditorWrapper _wrapper;
    private readonly Border _textEditorBorder;
    private readonly IBrush _defaultBorderBrush;
    private readonly IBrush _focusedBorderBrush;
    private readonly bool _isOriginal;
    private bool _isUpdatingFromViewModel;
    private bool _isUpdatingFromEditor;


    public TextEditorBindingHelper(
        MainViewModel vm,
        TextEditor textEditor,
        TextEditorWrapper wrapper,
        Border textEditorBorder,
        IBrush defaultBorderBrush,
        IBrush focusedBorderBrush,
        bool isOriginal)
    {
        _vm = vm;
        _textEditor = textEditor;
        _wrapper = wrapper;
        _textEditorBorder = textEditorBorder;
        _defaultBorderBrush = defaultBorderBrush;
        _focusedBorderBrush = focusedBorderBrush;
        _isOriginal = isOriginal;
    }

    public void Initialize()
    {
        _textEditor.Loaded += OnTextEditorLoaded;
        _textEditor.TextChanged += OnTextEditorTextChanged;
        _textEditor.Tapped += OnTextEditorTapped;

        UpdateEditorFromViewModel();
    }

    public void DeInitialize()
    {
        _textEditor.Loaded -= OnTextEditorLoaded;
        _textEditor.TextChanged -= OnTextEditorTextChanged;
        _textEditor.Tapped -= OnTextEditorTapped;

        var textArea = _textEditor.TextArea;
        if (textArea != null)
        {
            textArea.GotFocus -= OnTextAreaGotFocus;
            textArea.LostFocus -= OnTextAreaLostFocus;
        }
    }

    public void OnSelectedSubtitleChanged()
    {
        UpdateEditorFromViewModel();
    }

    public void OnSubtitleTextPropertyChanged()
    {
        UpdateEditorFromViewModel();
    }

    private void OnTextEditorLoaded(object? sender, RoutedEventArgs e)
    {
        var textArea = _textEditor.TextArea;
        if (textArea != null)
        {
            textArea.GotFocus += OnTextAreaGotFocus;
            textArea.LostFocus += OnTextAreaLostFocus;
        }
    }

    private void OnTextAreaGotFocus(object? sender, RoutedEventArgs e)
    {
        _textEditorBorder.BorderBrush = _focusedBorderBrush;
        _wrapper.HasFocus = true;
    }

    private void OnTextAreaLostFocus(object? sender, RoutedEventArgs e)
    {
        _textEditorBorder.BorderBrush = _defaultBorderBrush;
        if (_isOriginal)
        {
            _wrapper.HasFocus = false;
        }
    }

    private void OnTextEditorTextChanged(object? sender, System.EventArgs e)
    {
        UpdateViewModelFromEditor();
        var routedEvent = RoutedEvent.Register<TextEditor, TextChangedEventArgs>(
            _isOriginal ? "OriginalTextChanged" : "TextChanged",
            RoutingStrategies.Bubble);
        _vm.SubtitleTextChanged(sender, new TextChangedEventArgs(routedEvent));
    }

    private void OnTextEditorTapped(object? sender, RoutedEventArgs e)
    {
        _vm.SubtitleTextBoxGotFocus();
    }

    private void UpdateEditorFromViewModel()
    {
        if (_isUpdatingFromEditor)
        {
            return;
        }

        _isUpdatingFromViewModel = true;
        try
        {
            var text = _isOriginal
                ? _vm.SelectedSubtitle?.OriginalText ?? string.Empty
                : _vm.SelectedSubtitle?.Text ?? string.Empty;

            if (_textEditor.Text != text)
            {
                _textEditor.Text = text;
            }
        }
        finally
        {
            _isUpdatingFromViewModel = false;
        }
    }

    private void UpdateViewModelFromEditor()
    {
        if (_isUpdatingFromViewModel)
        {
            return;
        }

        _isUpdatingFromEditor = true;
        try
        {
            if (_vm.SelectedSubtitle != null)
            {
                if (_isOriginal)
                {
                    if (_vm.SelectedSubtitle.OriginalText != _textEditor.Text)
                    {
                        _vm.SelectedSubtitle.OriginalText = _textEditor.Text;
                    }
                }
                else
                {
                    if (_vm.SelectedSubtitle.Text != _textEditor.Text)
                    {
                        _vm.SelectedSubtitle.Text = _textEditor.Text;
                    }
                }
            }
        }
        finally
        {
            _isUpdatingFromEditor = false;
        }
    }
}
