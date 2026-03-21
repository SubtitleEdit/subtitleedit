using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaImageColorPicker;

public class AssaImageColorPickerWindow : Window
{
    public AssaImageColorPickerWindow(AssaImageColorPickerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.ImageColorPicker;
        CanResize = true;
        MinWidth = 450;
        MinHeight = 300;
        Width = 1000;
        Height = 700;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        // label
        var label = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.ScreenshotOverlayPosiion));

        // Create a grid to hold the video/screenshot and overlay
        var videoGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        // bitmap
        var bitmapImage = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.Screenshot)),
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = new Cursor(StandardCursorType.Cross),
        };

        // Add mouse event handlers for color picking
        bitmapImage.PointerMoved += vm.OnImagePointerMoved;
        bitmapImage.PointerPressed += vm.OnImagePointerPressed;

        // Add background image to video grid
        videoGrid.Children.Add(bitmapImage);

        // subtitle bitmap overlay


        // Store references
        vm.ScreenshotImage = bitmapImage;
        vm.VideoGrid = videoGrid;

        // Update position when screenshot image size changes
        bitmapImage.SizeChanged += (_, _) => vm.UpdateOverlayPosition();

        // Call UpdateOverlayPosition after UI setup to apply initial position
        bitmapImage.LayoutUpdated += (_, _) =>
        {
            vm.UpdateOverlayPosition();
            // Unsubscribe after first call to avoid repeated updates
            bitmapImage.LayoutUpdated -= (_, _) => { };
        };

        // Color display panel
        var colorPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
        };

        var mouseColorBox = new Border
        {
            Width = 60,
            Height = 60,
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            [!Border.BackgroundProperty] = new Binding(nameof(vm.CurrentMouseColor)),
        };
        ToolTip.SetTip(mouseColorBox, "Mouse-over color");

        var mouseColorInfo = new StackPanel
        {
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                UiUtil.MakeLabel("Mouse-over color"),
                new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding(nameof(vm.CurrentMouseColorHex)),
                    FontSize = 14,
                }
            }
        };

        var clickedColorBox = new Border
        {
            Width = 60,
            Height = 60,
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            [!Border.BackgroundProperty] = new Binding(nameof(vm.ClickedColor)),
            Margin = new Thickness(40, 0, 0, 0),
        };
        ToolTip.SetTip(clickedColorBox, "Clicked color");

        var copyButton = new Button
        {
            [!Button.ContentProperty] = new Binding(nameof(vm.CopyButtonContent)),
            Command = vm.CopyColorToClipboardCommand,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0, 0, 0),
        };

        var clickedColorInfo = new StackPanel
        {
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                UiUtil.MakeLabel("Clicked color"),
                new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding(nameof(vm.ClickedColorHex)),
                    FontSize = 14,
                }
            }
        };

        colorPanel.Children.Add(mouseColorBox);
        colorPanel.Children.Add(mouseColorInfo);
        colorPanel.Children.Add(clickedColorBox);
        colorPanel.Children.Add(clickedColorInfo);
        colorPanel.Children.Add(copyButton);

        // Buttons
        var buttonOk = UiUtil.MakeButtonDone(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk);


        grid.Add(label, 0);
        grid.Add(videoGrid, 1);
        grid.Add(colorPanel, 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += vm.KeyDown;
    }
}
