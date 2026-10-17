using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaImageColorPicker;

/// <summary>
/// Picks colors from the video frame - styled like ASSA draw: the frame on top, under it the color
/// under the pointer (with a magnifier), the picked color with copy buttons, and the history.
/// </summary>
public class AssaImageColorPickerWindow : Window
{
    public AssaImageColorPickerWindow(AssaImageColorPickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.ImageColorPicker;
        CanResize = true;
        MinWidth = 700;
        MinHeight = 500;
        Width = 1050;
        Height = 780;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);

        vm.Window = this;
        DataContext = vm;

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
        };
        mainGrid.Add(MakeFrame(vm), 0, 0);
        mainGrid.Add(MakeColorsPanel(vm), 1, 0);
        mainGrid.Add(MakeButtonBar(vm, out var buttonDone), 2, 0);
        Content = mainGrid;

        UiUtil.FocusOnFirstActivation(this, buttonDone);
        KeyDown += vm.KeyDown;
        Closed += (_, _) => vm.OnClosed();
    }

    private static Control MakeFrame(AssaImageColorPickerViewModel vm)
    {
        var videoGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        var bitmapImage = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.Screenshot)),
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = new Cursor(StandardCursorType.Cross),
        };
        bitmapImage.PointerMoved += vm.OnImagePointerMoved;
        bitmapImage.PointerPressed += vm.OnImagePointerPressed;
        AutomationProperties.SetName(bitmapImage, Se.Language.Assa.ImageColorPickerHint);
        videoGrid.Children.Add(bitmapImage);

        vm.ScreenshotImage = bitmapImage;
        vm.VideoGrid = videoGrid;
        bitmapImage.SizeChanged += (_, _) => vm.UpdateOverlayPosition();

        // Pointer position, or how to pick before the pointer has been over the frame
        var info = new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            Margin = new Thickness(2, 8, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ScreenshotOverlayPosiion))
            {
                Converter = new Avalonia.Data.Converters.FuncValueConverter<string, string>(position =>
                    string.IsNullOrEmpty(position) ? Se.Language.Assa.ImageColorPickerHint : position + "   ·   " + Se.Language.Assa.ImageColorPickerHint),
            },
        };

        var panel = new Grid { Margin = new Thickness(12), RowDefinitions = { new RowDefinition(new GridLength(1, GridUnitType.Star)), new RowDefinition(GridLength.Auto) } };
        panel.Add(videoGrid, 0, 0);
        panel.Add(info, 1, 0);
        return panel;
    }

    private static Border Swatch(string property, double width, double height)
    {
        return new Border
        {
            Width = width,
            Height = height,
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.MenuBorderBrush,
            [!Border.BackgroundProperty] = new Binding(property),
        };
    }

    private static TextBlock Mono(string property, IBrush? foreground = null) => new()
    {
        FontFamily = new FontFamily("Menlo, Consolas, Cascadia Mono, DejaVu Sans Mono, monospace"),
        FontSize = 12.5,
        Foreground = foreground ?? EditorChrome.StrongTextBrush,
        [!TextBlock.TextProperty] = new Binding(property),
    };

    private static Button TextButton(string text, string accessibleName, System.Windows.Input.ICommand command)
    {
        var button = new Button
        {
            Content = new TextBlock { Text = text, FontSize = 12, Margin = new Thickness(9, 0) },
            Height = 26,
            Command = command,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        button.Classes.Add("tool");
        AutomationProperties.SetName(button, accessibleName);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(button, accessibleName);
        }

        return button;
    }

    private static Border MakeColorsPanel(AssaImageColorPickerViewModel vm)
    {
        // Under the pointer: magnifier, swatch and hex
        var magnifier = new Border
        {
            Width = 88,
            Height = 88,
            CornerRadius = new CornerRadius(6),
            ClipToBounds = true,
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.FieldBorderBrush,
            Background = EditorChrome.FieldBrush,
            Child = new Image { [!Image.SourceProperty] = new Binding(nameof(vm.Magnifier)), Stretch = Stretch.Fill },
        };
        var pointerInfo = new StackPanel { Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        pointerInfo.Children.Add(Swatch(nameof(vm.CurrentMouseColor), 56, 32));
        pointerInfo.Children.Add(Mono(nameof(vm.CurrentMouseColorRgbHex)));
        var pointer = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        pointer.Children.Add(magnifier);
        pointer.Children.Add(pointerInfo);

        // Picked: swatch, hex and ASSA code, copy as hex / ASSA / \c tag
        var codes = new StackPanel { Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
        codes.Children.Add(Mono(nameof(vm.ClickedColorRgbHex)));
        codes.Children.Add(Mono(nameof(vm.ClickedColorAssa), EditorChrome.DimTextBrush));
        var pickedTop = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        pickedTop.Children.Add(Swatch(nameof(vm.ClickedColor), 72, 44));
        pickedTop.Children.Add(codes);

        var copyButtons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        copyButtons.Children.Add(TextButton("Hex", Se.Language.Assa.CopyColorAsHextoClipboard, vm.CopyHexCommand));
        copyButtons.Children.Add(TextButton("ASSA", Se.Language.Assa.CopyColorAsAssaToClipboard, vm.CopyAssaCommand));
        copyButtons.Children.Add(TextButton("\\c tag", Se.Language.Assa.CopyColorAsAssaTagToClipboard, vm.CopyAssaTagCommand));
        copyButtons.Bind(IsEnabledProperty, new Binding(nameof(vm.HasPickedColor)));

        var picked = new StackPanel { Spacing = 8 };
        picked.Children.Add(pickedTop);
        picked.Children.Add(copyButtons);
        picked.Children.Add(new TextBlock
        {
            FontSize = 11.5,
            Foreground = EditorChrome.DimTextBrush,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.CopiedText)),
        });

        // History: click a swatch to pick it again
        var history = new ItemsControl
        {
            ItemsSource = vm.History,
            HorizontalAlignment = HorizontalAlignment.Left,
            ItemsPanel = new FuncTemplate<Panel?>(() => new WrapPanel { MaxWidth = 230 }),
            ItemTemplate = new FuncDataTemplate<PickedColorItem>((item, _) =>
            {
                var button = new Button
                {
                    Width = 28,
                    Height = 28,
                    Margin = new Thickness(0, 0, 4, 4),
                    Padding = new Thickness(3),
                    Command = vm.PickFromHistoryCommand,
                    CommandParameter = item,
                    Content = new Border
                    {
                        CornerRadius = new CornerRadius(4),
                        BorderThickness = new Thickness(1),
                        BorderBrush = EditorChrome.MenuBorderBrush,
                        Background = item?.Brush,
                    },
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                };
                button.Classes.Add("tool");
                AutomationProperties.SetName(button, item?.Hex ?? string.Empty);
                if (Se.Settings.Appearance.ShowHints)
                {
                    ToolTip.SetTip(button, item?.Hex);
                }

                return button;
            }),
        };

        Border Column(string title, Control content, bool first) => new()
        {
            Child = new StackPanel { Spacing = 10, Children = { EditorChrome.MakeSectionHeader(title), content } },
            Padding = new Thickness(16, 12, 16, 14),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(first ? 0 : 1, 0, 0, 0),
        };

        var columns = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        columns.Add(Column(Se.Language.Assa.MouseOverColor, pointer, first: true), 0, 0);
        columns.Add(Column(Se.Language.Assa.ClickedColor, picked, first: false), 0, 1);
        columns.Add(Column(Se.Language.General.History, history, first: false), 0, 2);

        return new Border
        {
            Child = columns,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
        };
    }

    private static Border MakeButtonBar(AssaImageColorPickerViewModel vm, out Button buttonDone)
    {
        buttonDone = vm.IsColorSelection ? UiUtil.MakeButtonOk(vm.OkCommand) : UiUtil.MakeButtonDone(vm.OkCommand);
        buttonDone.Classes.Add("primary");
        buttonDone.HorizontalAlignment = HorizontalAlignment.Right;
        if (vm.IsColorSelection)
        {
            buttonDone.Bind(IsEnabledProperty, new Binding(nameof(vm.HasPickedColor)));
        }

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
        };
        if (vm.IsColorSelection)
        {
            buttons.Children.Add(UiUtil.MakeButtonCancel(vm.CancelCommand));
        }

        buttons.Children.Add(buttonDone);
        return new Border
        {
            Child = buttons,
            Background = EditorChrome.ButtonBarBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 10),
        };
    }
}
