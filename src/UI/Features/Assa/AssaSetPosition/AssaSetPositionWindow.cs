using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssaSetPosition;

public class AssaSetPositionWindow : Window
{
    public AssaSetPositionWindow(AssaSetPositionViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.SetPosition;
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
            RowSpacing = 15,
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
        };

        // Add background image to video grid
        videoGrid.Children.Add(bitmapImage);

        // subtitle bitmap overlay
        var bitmapTextImage = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.ScreenshotOverlayText)),
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
        };

        // Add overlay to video grid
        videoGrid.Children.Add(bitmapTextImage);

        // Store references
        vm.ScreenshotImage = bitmapImage;
        vm.ScreenshotOverlayImage = bitmapTextImage;
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

        // Implement mouse dragging for overlay image
        Point? dragStartPoint = null;
        int originalX = 0;
        int originalY = 0;

        bitmapTextImage.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(bitmapTextImage).Properties.IsLeftButtonPressed)
            {
                dragStartPoint = e.GetPosition(videoGrid);
                originalX = vm.ScreenshotX;
                originalY = vm.ScreenshotY;
                e.Handled = true;
            }
        };

        bitmapTextImage.PointerMoved += (_, e) =>
        {
            if (dragStartPoint.HasValue && vm.ScreenshotOverlayText != null)
            {
                var currentPoint = e.GetPosition(videoGrid);
                var delta = currentPoint - dragStartPoint.Value;

                // Convert screen delta to subtitle coordinate delta (inverse of scale)
                var screenshotImageWidth = bitmapImage.Bounds.Width;
                if (screenshotImageWidth > 0 && vm.TargetWidth > 0)
                {
                    var scale = screenshotImageWidth / vm.TargetWidth;
                    var deltaX = (int)(delta.X / scale);
                    var deltaY = (int)(delta.Y / scale);

                    vm.ScreenshotX = originalX + deltaX;
                    vm.ScreenshotY = originalY + deltaY;

                    vm.UpdateOverlayPosition();
                }

                e.Handled = true;
            }
        };

        bitmapTextImage.PointerReleased += (_, e) =>
        {
            if (dragStartPoint.HasValue)
            {
                dragStartPoint = null;
                e.Handled = true;
            }
        };

        // Position buttons
        var buttonCenterHorizontally = UiUtil.MakeButton(Se.Language.General.CenterHorizontally, vm.CenterHorizontallyCommand);
        var buttonCenterVertically = UiUtil.MakeButton(Se.Language.General.CenterVertically, vm.CenterVerticallyCommand);
        var panelPositionButtons = UiUtil.MakeButtonBar(buttonCenterHorizontally, buttonCenterVertically).WithAlignmentLeft();

        // Buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);


        grid.Add(label, 0);
        grid.Add(videoGrid, 1);
        grid.Add(panelPositionButtons, 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += vm.KeyDown;
    }
}
