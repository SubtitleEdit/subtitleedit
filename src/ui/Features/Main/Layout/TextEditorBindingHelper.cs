using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class TextEditorBindingHelper
{
    private static readonly RoutedEvent<TextChangedEventArgs> TextChangedEvent =
        RoutedEvent.Register<TextEditor, TextChangedEventArgs>("TextChanged", RoutingStrategies.Bubble);
    private static readonly RoutedEvent<TextChangedEventArgs> OriginalTextChangedEvent =
        RoutedEvent.Register<TextEditor, TextChangedEventArgs>("OriginalTextChanged", RoutingStrategies.Bubble);

    private static readonly Thickness DefaultBorderThickness = new(1);
    private static readonly Thickness FocusedBorderThickness = new(2);
    private static readonly Thickness DefaultBorderPadding = new(1);
    private static readonly Thickness FocusedBorderPadding = new(0);

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
            textArea.RemoveHandler(InputElement.KeyDownEvent, OnTextAreaKeyDownTunnel);
        }
    }

    /// <summary>
    /// Makes Tab / Shift+Tab move keyboard focus out of the AvaloniaEdit text editor instead of
    /// inserting indentation, so keyboard-only and screen-reader users are not trapped in the editor
    /// (issue #11745). Other Tab modifier combos (e.g. Ctrl+Tab) are left to their normal handling.
    /// </summary>
    private void OnTextAreaKeyDownTunnel(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Tab || e.KeyModifiers is not (KeyModifiers.None or KeyModifiers.Shift))
        {
            return;
        }

        var focusManager = TopLevel.GetTopLevel(_textEditor)?.FocusManager;
        if (focusManager == null)
        {
            return;
        }

        var direction = e.KeyModifiers == KeyModifiers.Shift
            ? NavigationDirection.Previous
            : NavigationDirection.Next;

        // TryMoveFocus walks the tab order from the currently focused element (the TextArea), the same
        // path the Tab key normally takes - so focus lands on the next/previous control outside the editor.
        if (focusManager.TryMoveFocus(direction))
        {
            e.Handled = true;
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
        _textEditor.FocusAdorner = null;
        var textArea = _textEditor.TextArea;
        if (textArea != null)
        {
            textArea.FocusAdorner = null;
            textArea.GotFocus += OnTextAreaGotFocus;
            textArea.LostFocus += OnTextAreaLostFocus;

            // AvaloniaEdit's TextArea consumes Tab to insert indentation, which traps keyboard and
            // screen-reader users inside the editor: once focus lands here they can never Tab out to
            // the grid, waveform, or video controls (issue #11745). Subtitle text never needs a literal
            // tab, so intercept Tab/Shift+Tab on the tunnel - before AvaloniaEdit sees it - and move
            // focus like every other field instead (matches SE4/WinForms behavior).
            textArea.RemoveHandler(InputElement.KeyDownEvent, OnTextAreaKeyDownTunnel);
            textArea.AddHandler(InputElement.KeyDownEvent, OnTextAreaKeyDownTunnel, RoutingStrategies.Tunnel);
        }

        _textEditorBorder.BorderThickness = DefaultBorderThickness;
        _textEditorBorder.Padding = DefaultBorderPadding;
    }

    private void OnTextAreaGotFocus(object? sender, RoutedEventArgs e)
    {
        _textEditorBorder.BorderBrush = _focusedBorderBrush;
        _textEditorBorder.BorderThickness = FocusedBorderThickness;
        _textEditorBorder.Padding = FocusedBorderPadding;
        _wrapper.HasFocus = true;
    }

    private void OnTextAreaLostFocus(object? sender, RoutedEventArgs e)
    {
        _textEditorBorder.BorderBrush = _defaultBorderBrush;
        _textEditorBorder.BorderThickness = DefaultBorderThickness;
        _textEditorBorder.Padding = DefaultBorderPadding;
        _wrapper.HasFocus = false;
    }

    private void OnTextEditorTextChanged(object? sender, System.EventArgs e)
    {
        UpdateViewModelFromEditor();
        var routedEvent = _isOriginal ? OriginalTextChangedEvent : TextChangedEvent;
        _vm.SubtitleTextChanged(sender, new TextChangedEventArgs(routedEvent));

        // Follow the content: an original subtitle in a left to right language
        // stays readable next to a right to left working language and vice versa.
        // On the TextView, not the TextArea - see RightToLeftHelper.SetFlowDirectionRecursive.
        var requestedDirection = Se.Settings.Appearance.RightToLeft
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
        _textEditor.TextArea.TextView.FlowDirection = MainHelpers.RightToLeftHelper
            .GetContentDirection(_textEditor.Text, requestedDirection);
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
