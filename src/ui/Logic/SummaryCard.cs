using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;
using System.Windows.Input;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// A clickable "count + label" summary card with a colored accent bar, used as a
/// filter toggle above a result table (speech-to-text quality report, list errors).
/// <see cref="Key"/> is whatever the owner uses to identify the filter; the
/// active card shows its accent color as the border.
/// </summary>
public partial class SummaryCard : ObservableObject
{
    [ObservableProperty] private bool _isActive;
    [ObservableProperty] private IBrush _borderBrush = UiUtil.GetTextColor(0.25);

    partial void OnIsActiveChanged(bool value)
    {
        BorderBrush = value ? Brush : UiUtil.GetTextColor(0.25);
    }

    public object? Key { get; init; }
    public string Label { get; init; } = string.Empty;
    public string Hint { get; init; } = string.Empty;
    public int Count { get; init; }
    public IBrush Brush { get; init; } = Brushes.Gray;

    /// <summary>
    /// Build the visual for a card. A bordered Button rather than a ToggleButton: the
    /// Fluent checked state floods the card with the accent color and drowns the
    /// colored count.
    /// </summary>
    public static Control MakeControl(SummaryCard? card, ICommand command)
    {
        if (card == null)
        {
            return new Border();
        }

        var accent = new Border
        {
            Width = 5,
            CornerRadius = new CornerRadius(3),
            Background = card.Brush,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        var count = new TextBlock
        {
            Text = card.Count.ToString(),
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = card.Brush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var label = new TextBlock
        {
            Text = card.Label,
            FontSize = 12,
            Opacity = 0.85,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var texts = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { count, label },
        };
        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { accent, texts },
        };

        var border = new Border
        {
            Child = content,
            Padding = new Thickness(12, 8),
            MinWidth = 120,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(UiUtil.CornerRadius + 2),
            DataContext = card,
            [!Border.BorderBrushProperty] = new Binding(nameof(BorderBrush)),
        };

        var button = new Button
        {
            Content = border,
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(UiUtil.CornerRadius + 2),
            Command = command,
            CommandParameter = card,
            Opacity = card.Count == 0 && card.Key != null ? 0.5 : 1.0,
        };
        AutomationProperties.SetName(button, $"{card.Label}: {card.Count}");
        if (Se.Settings.Appearance.ShowHints && !string.IsNullOrEmpty(card.Hint))
        {
            ToolTip.SetTip(button, card.Hint);
        }

        return button;
    }

    /// <summary>An ItemsControl that lays cards out in a wrapping row.</summary>
    public static ItemsControl MakeCardsPanel(System.Collections.IEnumerable cards, ICommand command)
    {
        return new ItemsControl
        {
            ItemsSource = cards,
            Margin = new Thickness(0, 0, 0, 12),
            ItemsPanel = new Avalonia.Controls.Templates.FuncTemplate<Panel?>(() => new WrapPanel { Orientation = Orientation.Horizontal, ItemSpacing = 8, LineSpacing = 8 }),
            ItemTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<SummaryCard>((card, _) => MakeControl(card, command)),
        };
    }
}
