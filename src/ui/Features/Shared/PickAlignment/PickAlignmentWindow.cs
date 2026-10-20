using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickAlignment;

/// <summary>
/// Choose alignment, in the editor look (see <see cref="EditorChrome"/>): a "screen" with the
/// nine positions as anchors, each placed where the text would go - like the Edit menu's alignment
/// picker. Picking one closes the window.
/// </summary>
public class PickAlignmentWindow : Window
{
    public PickAlignmentWindow(PickAlignmentViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.ChooseAlignment;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        vm.Window = this;
        DataContext = vm;

        var buttonAn7 = MakeAnchorButton(Se.Language.General.TopLeft, vm, "an7", HorizontalAlignment.Left, VerticalAlignment.Top);
        var buttonAn8 = MakeAnchorButton(Se.Language.General.TopCenter, vm, "an8", HorizontalAlignment.Center, VerticalAlignment.Top);
        var buttonAn9 = MakeAnchorButton(Se.Language.General.TopRight, vm, "an9", HorizontalAlignment.Right, VerticalAlignment.Top);
        var buttonAn4 = MakeAnchorButton(Se.Language.General.MiddleLeft, vm, "an4", HorizontalAlignment.Left, VerticalAlignment.Center);
        var buttonAn5 = MakeAnchorButton(Se.Language.General.MiddleCenter, vm, "an5", HorizontalAlignment.Center, VerticalAlignment.Center);
        var buttonAn6 = MakeAnchorButton(Se.Language.General.MiddleRight, vm, "an6", HorizontalAlignment.Right, VerticalAlignment.Center);
        var buttonAn1 = MakeAnchorButton(Se.Language.General.BottomLeft, vm, "an1", HorizontalAlignment.Left, VerticalAlignment.Bottom);
        var buttonAn2 = MakeAnchorButton(Se.Language.General.BottomCenter, vm, "an2", HorizontalAlignment.Center, VerticalAlignment.Bottom);
        var buttonAn3 = MakeAnchorButton(Se.Language.General.BottomRight, vm, "an3", HorizontalAlignment.Right, VerticalAlignment.Bottom);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 4,
            RowSpacing = 4,
        };

        grid.Add(buttonAn7, 0);
        grid.Add(buttonAn8, 0, 1);
        grid.Add(buttonAn9, 0, 2);
        grid.Add(buttonAn4, 1);
        grid.Add(buttonAn5, 1, 1);
        grid.Add(buttonAn6, 1, 2);
        grid.Add(buttonAn1, 2);
        grid.Add(buttonAn2, 2, 1);
        grid.Add(buttonAn3, 2, 2);

        // The video frame the positions are on
        var screen = new Border
        {
            Child = grid,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(6),
        };

        Content = new Border
        {
            Background = EditorChrome.PanelBrush,
            Child = screen,
            Padding = new Thickness(16),
        };

        UiUtil.FocusOnFirstActivation(this, buttonAn2); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// A flat button with an anchor dot and the position name, both placed at that position in
    /// the cell - so the grid reads like a frame with the text placements on it.
    /// </summary>
    private static Button MakeAnchorButton(string text, PickAlignmentViewModel vm, string alignment, HorizontalAlignment horizontal, VerticalAlignment vertical)
    {
        var dot = new Ellipse
        {
            Width = 7,
            Height = 7,
            Fill = EditorChrome.AccentBrush,
            HorizontalAlignment = horizontal,
        };
        var label = new TextBlock
        {
            Text = text,
            FontSize = 12,
            HorizontalAlignment = horizontal,
            TextAlignment = horizontal switch
            {
                HorizontalAlignment.Left => TextAlignment.Left,
                HorizontalAlignment.Right => TextAlignment.Right,
                _ => TextAlignment.Center,
            },
        };
        var content = new StackPanel { Spacing = 5 };
        if (vertical == VerticalAlignment.Bottom)
        {
            content.Children.Add(label);
            content.Children.Add(dot);
        }
        else
        {
            content.Children.Add(dot);
            content.Children.Add(label);
        }

        var button = new Button
        {
            Content = content,
            Command = vm.SetAlignmentCommand,
            CommandParameter = alignment,
            MinWidth = 116,
            MinHeight = 64,
            Padding = new Thickness(10, 8),
            HorizontalContentAlignment = horizontal,
            VerticalContentAlignment = vertical,
        };
        button.Classes.Add("tool");
        button.Foreground = EditorChrome.StrongTextBrush;

        // Panel content has no name of its own for a screen reader (#12087)
        AutomationProperties.SetName(button, text);
        return button;
    }
}
