using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class BurnInLogoWindow : Window
{
    private readonly BurnInLogoViewModel _vm;

    public BurnInLogoWindow(BurnInLogoViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Logo;
        CanResize = true;
        MinWidth = 600;
        MinHeight = 500;
        Width = 1000;
        Height = 700;

        _vm = vm;
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

        // Top panel with button to pick logo and position label
        var topPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
        };

        var buttonPickLogo = UiUtil.MakeButton(Se.Language.General.OpenImageFile + "...", vm.PickLogoCommand);
        var labelLogoPosition = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.LogoPositionText));

        // Alpha slider
        var labelAlpha = UiUtil.MakeLabel("Alpha:");
        labelAlpha.VerticalAlignment = VerticalAlignment.Center;
        labelAlpha.Margin = new Thickness(20, 0, 5, 0);

        var sliderAlpha = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            Width = 150,
            VerticalAlignment = VerticalAlignment.Center,
            [!Slider.ValueProperty] = new Binding("BurnInLogo.Alpha"),
        };
        sliderAlpha.ValueChanged += (_, _) => vm.UpdateOverlayOpacity();

        var labelAlphaValue = UiUtil.MakeLabel();
        labelAlphaValue.VerticalAlignment = VerticalAlignment.Center;
        labelAlphaValue.Margin = new Thickness(5, 0, 0, 0);
        labelAlphaValue.MinWidth = 35;
        labelAlphaValue[!TextBlock.TextProperty] = new Binding("BurnInLogo.Alpha") { StringFormat = "{0}%" };

        // Logo size slider
        var labelSize = UiUtil.MakeLabel("Size:");
        labelSize.VerticalAlignment = VerticalAlignment.Center;
        labelSize.Margin = new Thickness(20, 0, 5, 0);

        var sliderSize = new Slider
        {
            Minimum = 10,
            Maximum = 200,
            Width = 150,
            VerticalAlignment = VerticalAlignment.Center,
            [!Slider.ValueProperty] = new Binding("BurnInLogo.Size"),
        };
        sliderSize.ValueChanged += (_, _) => vm.UpdateLogoSize();

        var labelSizeValue = UiUtil.MakeLabel();
        labelSizeValue.VerticalAlignment = VerticalAlignment.Center;
        labelSizeValue.Margin = new Thickness(5, 0, 0, 0);
        labelSizeValue.MinWidth = 35;
        labelSizeValue[!TextBlock.TextProperty] = new Binding("BurnInLogo.Size") { StringFormat = "{0}%" };

        topPanel.Children.Add(buttonPickLogo);
        topPanel.Children.Add(labelLogoPosition);
        topPanel.Children.Add(labelAlpha);
        topPanel.Children.Add(sliderAlpha);
        topPanel.Children.Add(labelAlphaValue);
        topPanel.Children.Add(labelSize);
        topPanel.Children.Add(sliderSize);
        topPanel.Children.Add(labelSizeValue);

        // Create video grid with canvas overlay for draggable logo
        var videoGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        // Video player control
        var videoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        videoPlayerControl.HorizontalAlignment = HorizontalAlignment.Stretch;
        videoPlayerControl.VerticalAlignment = VerticalAlignment.Stretch;
        vm.VideoPlayerControl = videoPlayerControl;
        videoGrid.Children.Add(videoPlayerControl);

        // Create a green rectangle border to show the actual video content area
        var videoContentBorder = new Border
        {
            BorderBrush = Brushes.Green,
            BorderThickness = new Thickness(2),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            IsHitTestVisible = false,
        };
        videoGrid.Children.Add(videoContentBorder);
        vm.VideoContentBorder = videoContentBorder;

        // Canvas overlay for logo
        var logoCanvas = new Canvas
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        // Logo image overlay
        var logoImage = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.LogoOverlay)),
            [!Image.IsVisibleProperty] = new Binding(nameof(vm.HasLogo)),
            Cursor = new Cursor(StandardCursorType.Hand),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };

        logoCanvas.Children.Add(logoImage);
        videoGrid.Children.Add(logoCanvas);

        vm.LogoImage = logoImage;
        vm.VideoGrid = videoGrid;

        // Update overlay position when video player size changes
        videoPlayerControl.SizeChanged += (_, _) =>
        {
            vm.UpdateBorder();
            vm.UpdateLogoPosition();
        };

        // Implement mouse dragging for logo image
        Point? dragStartPoint = null;
        int originalX = 0;
        int originalY = 0;

        logoImage.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(logoImage).Properties.IsLeftButtonPressed)
            {
                dragStartPoint = e.GetPosition(videoPlayerControl);
                originalX = vm.BurnInLogo.X;
                originalY = vm.BurnInLogo.Y;
                e.Handled = true;
            }
        };

        logoImage.PointerMoved += (_, e) =>
        {
            if (dragStartPoint.HasValue && vm.LogoOverlay != null && vm.VideoContentBorder != null)
            {
                var currentPoint = e.GetPosition(videoPlayerControl);
                var delta = currentPoint - dragStartPoint.Value;

                // Get border position and size
                var borderLeft = vm.VideoContentBorder.Margin.Left;
                var borderTop = vm.VideoContentBorder.Margin.Top;
                var borderWidth = vm.VideoContentBorder.Width;

                // Convert screen delta to video coordinate delta (inverse of scale)
                if (borderWidth > 0 && vm.VideoWidth > 0)
                {
                    var scale = borderWidth / vm.VideoWidth;
                    var deltaX = (int)(delta.X / scale);
                    var deltaY = (int)(delta.Y / scale);

                    vm.BurnInLogo.X = originalX + deltaX;
                    vm.BurnInLogo.Y = originalY + deltaY;

                    vm.UpdateOverlayPosition();
                }

                e.Handled = true;
            }
        };

        logoImage.PointerReleased += (_, e) =>
        {
            if (dragStartPoint.HasValue)
            {
                dragStartPoint = null;
                e.Handled = true;
            }
        };

        // Instructions label
        var instructionsLabel = UiUtil.MakeLabel("Pick a PNG image and drag it to position it on the video.");

        // Buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        grid.Add(topPanel, 0);
        grid.Add(videoGrid, 1);
        grid.Add(instructionsLabel, 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.VideoPlayerControl?.Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}