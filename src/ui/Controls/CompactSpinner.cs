using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// Gives TimeCodeUpDown and SecondsUpDown the editor field look (as in the ASSA editors): a soft
/// field background with a subtle border, and a narrow stacked up/down pair with no divider
/// instead of the two wide side-by-side buttons. The pair only shows while the pointer is over
/// the field or it has keyboard focus (it keeps its space, so the field never changes width);
/// Up/Down and the mouse wheel step the value either way.
/// The theme sets the panel orientation and button sizes inside its template, where they beat
/// any style, so they are changed here once the template is applied.
/// </summary>
internal static class CompactSpinner
{
    private const double ButtonWidth = 22;

    internal static void Attach(ButtonSpinner spinner)
    {
        spinner.Background = EditorChrome.FieldBrush;
        spinner.BorderBrush = EditorChrome.FieldBorderBrush;
        spinner.MinHeight = 28;
        spinner.TemplateApplied -= OnTemplateApplied;
        spinner.TemplateApplied += OnTemplateApplied;
    }

    /// <summary>
    /// Same look for a plain <see cref="NumericUpDown"/> (e.g. the ASSA layer field in the main
    /// window), so it matches the time controls stacked above it.
    /// </summary>
    internal static void Attach(NumericUpDown upDown)
    {
        upDown.MinHeight = 28;
        upDown.CornerRadius = new Avalonia.CornerRadius(6);
        upDown.TemplateApplied -= OnNumericUpDownTemplateApplied;
        upDown.TemplateApplied += OnNumericUpDownTemplateApplied;
    }

    private static void OnNumericUpDownTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (e.NameScope.Find<ButtonSpinner>("PART_Spinner") is { } spinner)
        {
            spinner.IsTabStop = false;
            spinner.CornerRadius = new Avalonia.CornerRadius(6);
            Attach(spinner);
        }

        if (e.NameScope.Find<TextBox>("PART_TextBox") is { } textBox)
        {
            textBox.Padding = new Avalonia.Thickness(10, 2, 4, 2);
            textBox.Margin = new Avalonia.Thickness(0);
            textBox.VerticalContentAlignment = VerticalAlignment.Center;
            textBox.HorizontalContentAlignment = HorizontalAlignment.Left;
            textBox.BorderBrush = Brushes.Transparent;
            textBox.BorderThickness = new Avalonia.Thickness(0);
            textBox.Background = Brushes.Transparent;
        }
    }

    private static void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (e.NameScope.Find<StackPanel>("PART_SpinnerPanel") is { } panel)
        {
            panel.Orientation = Orientation.Vertical;
            panel.VerticalAlignment = VerticalAlignment.Center;

            if (sender is ButtonSpinner spinner)
            {
                void Update()
                {
                    var active = spinner.IsPointerOver || spinner.IsKeyboardFocusWithin;
                    panel.Opacity = active ? 1 : 0;
                    spinner.BorderBrush = active ? EditorChrome.SeparatorBrush : EditorChrome.FieldBorderBrush;
                }

                Update();
                spinner.PropertyChanged += (_, change) =>
                {
                    if (change.Property == InputElement.IsPointerOverProperty ||
                        change.Property == InputElement.IsKeyboardFocusWithinProperty)
                    {
                        Update();
                    }
                };
            }
        }

        foreach (var name in new[] { "PART_IncreaseButton", "PART_DecreaseButton" })
        {
            if (e.NameScope.Find<RepeatButton>(name) is { } button)
            {
                button.Width = ButtonWidth;
                button.MinWidth = 0;
                button.Height = 13;
                button.MinHeight = 0;
                button.Padding = new Avalonia.Thickness(0);
                button.BorderThickness = new Avalonia.Thickness(0);
                button.Background = Brushes.Transparent;
                button.Foreground = EditorChrome.IconBrush;
                button.RenderTransform = new ScaleTransform(0.8, 0.8);
            }
        }
    }
}
