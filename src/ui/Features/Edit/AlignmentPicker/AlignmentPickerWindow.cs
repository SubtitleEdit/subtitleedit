using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.AlignmentPicker;

/// <summary>
/// Choose alignment, in the editor look (see <see cref="EditorChrome"/>): a "screen" with the nine
/// positions as anchors, each placed where the text would go. Picking one closes the window.
/// </summary>
public class AlignmentPickerWindow: Window
{
    public AlignmentPickerWindow(AlignmentPickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.ChooseAlignment;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);
        vm.Window = this;
        DataContext = vm;

        var buttonAn1 = MakeAnchorButton(Se.Language.General.TopLeft, vm.An1Command, HorizontalAlignment.Left, VerticalAlignment.Top);
        var buttonAn2 = MakeAnchorButton(Se.Language.General.TopCenter, vm.An2Command, HorizontalAlignment.Center, VerticalAlignment.Top);
        var buttonAn3 = MakeAnchorButton(Se.Language.General.TopRight, vm.An3Command, HorizontalAlignment.Right, VerticalAlignment.Top);
        var buttonAn4 = MakeAnchorButton(Se.Language.General.MiddleLeft, vm.An4Command, HorizontalAlignment.Left, VerticalAlignment.Center);
        var buttonAn5 = MakeAnchorButton(Se.Language.General.MiddleCenter, vm.An5Command, HorizontalAlignment.Center, VerticalAlignment.Center);
        var buttonAn6 = MakeAnchorButton(Se.Language.General.MiddleRight, vm.An6Command, HorizontalAlignment.Right, VerticalAlignment.Center);
        var buttonAn7 = MakeAnchorButton(Se.Language.General.BottomLeft, vm.An7Command, HorizontalAlignment.Left, VerticalAlignment.Bottom);
        var buttonAn8 = MakeAnchorButton(Se.Language.General.BottomCenter, vm.An8Command, HorizontalAlignment.Center, VerticalAlignment.Bottom);
        var buttonAn9 = MakeAnchorButton(Se.Language.General.BottomRight, vm.An9Command, HorizontalAlignment.Right, VerticalAlignment.Bottom);

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

        grid.Add(buttonAn1, 0, 0);
        grid.Add(buttonAn2, 0, 1);
        grid.Add(buttonAn3, 0, 2);
        grid.Add(buttonAn4, 1, 0);
        grid.Add(buttonAn5, 1, 1);
        grid.Add(buttonAn6, 1, 2);
        grid.Add(buttonAn7, 2, 0);
        grid.Add(buttonAn8, 2, 1);
        grid.Add(buttonAn9, 2, 2);

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
        KeyDown += vm.KeyDown;
    }

    /// <summary>
    /// A flat button with an anchor dot and the position name, both placed at that position in
    /// the cell - so the grid reads like a frame with the text placements on it.
    /// </summary>
    private static Button MakeAnchorButton(string text, System.Windows.Input.ICommand command, HorizontalAlignment horizontal, VerticalAlignment vertical)
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
            FontSize = EditorChrome.FieldFontSize,
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
            Command = command,
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
