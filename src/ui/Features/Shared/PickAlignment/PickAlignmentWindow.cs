using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.PickAlignment;

/// <summary>
/// Choose alignment, in the editor look (see <see cref="EditorChrome"/>): a video frame with the
/// nine positions as subtitles, each placed where the text would go, the selected line's position
/// marked. Picking one closes the window.
/// </summary>
public class PickAlignmentWindow : Window
{
    private static readonly IBrush SubtitleBoxBrush = new SolidColorBrush(Color.FromArgb(0xb8, 0, 0, 0));
    private static readonly IBrush FrameBrush = new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops =
        {
            new GradientStop(Color.Parse("#2b3a4d"), 0),
            new GradientStop(Color.Parse("#141a22"), 1),
        },
    };

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
            RowDefinitions = new RowDefinitions("*,*,*"),
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
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

        // The video frame the positions are on: 16:9 and dark in every theme, like a video
        var screen = new Border
        {
            Child = grid,
            Width = 480,
            Height = 270,
            Background = FrameBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
        };
        screen.Styles.Add(new Style(x => x.OfType<Button>().Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters = { new Setter(ContentPresenter.BackgroundProperty, Brushes.Transparent) },
        });

        Content = new Border
        {
            Background = EditorChrome.PanelBrush,
            Child = screen,
            Padding = new Thickness(16),
        };

        Button[] buttons = [buttonAn1, buttonAn2, buttonAn3, buttonAn4, buttonAn5, buttonAn6, buttonAn7, buttonAn8, buttonAn9];

        // On the current position, so Enter keeps it; otherwise on the default
        UiUtil.FocusOnFirstActivation(this, () => (buttons.FirstOrDefault(b => Equals(b.CommandParameter, vm.CurrentAlignment)) ?? buttonAn2).Focus()); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// A position on the frame: its name as a subtitle on the video, where the text would go. It
    /// turns to the accent while hovered or focused, and the line's current position stays in the
    /// accent with a check.
    /// </summary>
    private static Button MakeAnchorButton(string text, PickAlignmentViewModel vm, string alignment, HorizontalAlignment horizontal, VerticalAlignment vertical)
    {
        var check = new Optris.Icons.Avalonia.Icon
        {
            Value = IconNames.Check,
            FontSize = 12,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            IsVisible = false,
        };
        var pill = new Border
        {
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 3),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Children =
                {
                    check,
                    new TextBlock
                    {
                        Text = text,
                        FontSize = EditorChrome.FieldFontSize,
                        Foreground = Brushes.White,
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                },
            },
        };

        var button = new Button
        {
            Content = pill,
            Command = vm.SetAlignmentCommand,
            CommandParameter = alignment,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Padding = new Thickness(12, 10),
            Background = Brushes.Transparent,
            HorizontalContentAlignment = horizontal,
            VerticalContentAlignment = vertical,
            Cursor = new Cursor(StandardCursorType.Hand),
        };

        void UpdateLook()
        {
            var isCurrent = vm.CurrentAlignment == alignment;
            check.IsVisible = isCurrent;
            pill.Background = isCurrent || button.IsPointerOver || button.IsFocused ? EditorChrome.AccentBrush : SubtitleBoxBrush;
        }

        button.PropertyChanged += (_, e) =>
        {
            if (e.Property == IsPointerOverProperty || e.Property == IsFocusedProperty)
            {
                UpdateLook();
            }
        };
        UpdateLook();

        // Panel content has no name of its own for a screen reader (#12087)
        AutomationProperties.SetName(button, text);
        return button;
    }
}
