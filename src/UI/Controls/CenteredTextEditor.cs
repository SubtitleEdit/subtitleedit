using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// A wrapper around TextEditor that supports text centering by applying padding.
/// </summary>
public class CenteredTextEditor : Decorator
{
    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<CenteredTextEditor, TextAlignment>(nameof(TextAlignment), TextAlignment.Left);

    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    private TextEditor? _textEditor;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ChildProperty)
        {
            _textEditor = Child as TextEditor;
            UpdateAlignment();
        }
        else if (change.Property == TextAlignmentProperty)
        {
            UpdateAlignment();
        }
    }

    private void UpdateAlignment()
    {
        if (_textEditor == null)
        {
            return;
        }

        // Apply centering through horizontal alignment
        if (TextAlignment == TextAlignment.Center)
        {
            _textEditor.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        }
        else if (TextAlignment == TextAlignment.Right)
        {
            _textEditor.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
        }
        else
        {
            _textEditor.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        }
    }
}

