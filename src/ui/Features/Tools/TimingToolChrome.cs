using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools;

/// <summary>
/// Small building blocks shared by the timing tools of the Tools menu (adjust durations, apply
/// duration limits, apply min. gap, bridge gaps, beautify time codes) in the editor look (see
/// <see cref="EditorChrome"/>). The window chrome itself comes from Sync's SyncTimingChrome.
/// </summary>
internal static class TimingToolChrome
{
    /// <summary>
    /// A compact number field (no spinner) with a fixed width and a screen reader name.
    /// </summary>
    internal static NumericUpDown Field(NumericUpDown field, string accessibleName, double width = 90)
    {
        field.Classes.Add("field");
        field.Width = width;
        field.VerticalAlignment = VerticalAlignment.Center;
        AutomationProperties.SetName(field, accessibleName);
        return field;
    }

    /// <summary>
    /// A dim label, a field and the controls after it on one line.
    /// </summary>
    internal static StackPanel LabeledField(string label, Control field, params Control[] after)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(EditorChrome.MakeDimLabel(label, new Thickness(0)));
        panel.Children.Add(field);
        foreach (var control in after)
        {
            panel.Children.Add(control);
        }

        return panel;
    }

    /// <summary>
    /// A compact on/off switch with its title beside it.
    /// </summary>
    internal static StackPanel MakeSwitch(string title, string binding, out ToggleSwitch toggle)
    {
        toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(binding) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggle, title);

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        row.Children.Add(toggle);
        row.Children.Add(new TextBlock { Text = title, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center });
        return row;
    }

    /// <summary>
    /// A list header whose text comes from the view model (counts, "No changes needed", ...).
    /// </summary>
    internal static TextBlock MakeBoundHeader(string binding)
    {
        return new TextBlock
        {
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(binding),
        };
    }

    /// <summary>
    /// Dim status text for the left side of the button bar.
    /// </summary>
    internal static TextBlock MakeStatus(string binding)
    {
        var label = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0));
        label.TextTrimming = TextTrimming.CharacterEllipsis;
        label[!TextBlock.TextProperty] = new Binding(binding);
        return label;
    }

    /// <summary>
    /// A rounded field-colored frame around a list.
    /// </summary>
    internal static Border MakeListFrame(Control list)
    {
        return new Border
        {
            Child = list,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
        };
    }

    /// <summary>
    /// A list section: header (title or bound text, optional actions on the right) above a framed
    /// list that takes the remaining height.
    /// </summary>
    internal static Border MakeListSection(Control header, Control list, bool first = false)
    {
        var grid = new Grid
        {
            RowSpacing = 8,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        grid.Add(header, 0);
        grid.Add(MakeListFrame(list), 1);
        return new Border
        {
            Child = grid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, first ? 0 : 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }
}
