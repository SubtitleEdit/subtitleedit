using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

/// <summary>
/// Place a logo on a video frame, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
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

        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // toolbar
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // video
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
        };

        // Toolbar: pick the logo, its position, and the alpha and size sliders
        var topPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var buttonPickLogo = EditorChrome.MakeActionButton("fa-solid fa-image", Se.Language.General.OpenImageFile + "...", vm.PickLogoCommand);
        var labelLogoPosition = EditorChrome.MakeBoundHint(nameof(vm.LogoPositionText));
        labelLogoPosition.FontSize = EditorChrome.FieldFontSize;
        labelLogoPosition.MinWidth = 110;
        labelLogoPosition.Margin = new Thickness(8, 0, 0, 0);

        // Alpha slider
        var labelAlpha = EditorChrome.MakeDimLabel(Se.Language.General.Alpha, new Thickness(0, 0, 8, 0));

        var sliderAlpha = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            Width = 140,
            VerticalAlignment = VerticalAlignment.Center,
            [!Slider.ValueProperty] = new Binding($"{nameof(vm.BurnInLogo)}.{nameof(vm.BurnInLogo.Alpha)}"),
        };
        Avalonia.Automation.AutomationProperties.SetName(sliderAlpha, Se.Language.General.Alpha);
        sliderAlpha.ValueChanged += (_, _) => vm.UpdateOverlayOpacity();

        // TextBlock, not UiUtil.MakeLabel: a Label is a ContentControl, so a TextBlock.Text binding on it never shows
        var labelAlphaValue = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(6, 0, 0, 0),
            MinWidth = 40,
            FontSize = EditorChrome.FieldFontSize,
            [!TextBlock.TextProperty] = new Binding($"{nameof(vm.BurnInLogo)}.{nameof(vm.BurnInLogo.Alpha)}") { StringFormat = "{0}%" },
        };

        // Logo size slider
        var labelSize = EditorChrome.MakeDimLabel(Se.Language.General.Size, new Thickness(0, 0, 8, 0));

        var sliderSize = new Slider
        {
            Minimum = 10,
            Maximum = 200,
            Width = 140,
            VerticalAlignment = VerticalAlignment.Center,
            [!Slider.ValueProperty] = new Binding($"{nameof(vm.BurnInLogo)}.{nameof(vm.BurnInLogo.Size)}"),
        };
        Avalonia.Automation.AutomationProperties.SetName(sliderSize, Se.Language.General.Size);
        sliderSize.ValueChanged += (_, _) => vm.UpdateLogoSize();

        // TextBlock, not UiUtil.MakeLabel: a Label is a ContentControl, so a TextBlock.Text binding on it never shows
        var labelSizeValue = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(6, 0, 0, 0),
            MinWidth = 40,
            FontSize = EditorChrome.FieldFontSize,
            [!TextBlock.TextProperty] = new Binding($"{nameof(vm.BurnInLogo)}.{nameof(vm.BurnInLogo.Size)}") { StringFormat = "{0}%" },
        };

        topPanel.Children.Add(buttonPickLogo);
        topPanel.Children.Add(labelLogoPosition);
        topPanel.Children.Add(EditorChrome.MakeToolbarSeparator());
        topPanel.Children.Add(labelAlpha);
        topPanel.Children.Add(sliderAlpha);
        topPanel.Children.Add(labelAlphaValue);
        topPanel.Children.Add(EditorChrome.MakeToolbarSeparator());
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
        var videoPlayerControl = InitVideoPlayer.MakeVideoPlayerPreferNonNative();
        videoPlayerControl.HorizontalAlignment = HorizontalAlignment.Stretch;
        videoPlayerControl.VerticalAlignment = VerticalAlignment.Stretch;
        // The player's fullscreen button had no command, so it did nothing. A real fullscreen
        // player would lose the draggable logo overlay - maximizing this window keeps it.
        videoPlayerControl.FullScreenCommand = new RelayCommand(ToggleMaximized);
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

        // Instructions in the button bar
        var instructionsLabel = new TextBlock
        {
            Text = Se.Language.Video.BurnIn.LogoInfo,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
        };

        // Buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = EditorChrome.MakeButtonBar(instructionsLabel, buttonCancel, buttonOk);

        // The player in a plain border - no corner radius or clipping, the macOS native video
        // surface disappears inside either
        grid.Add(EditorChrome.MakeToolbar(topPanel), 0);
        grid.Add(new Border
        {
            Child = EditorChrome.MakeVideoFrame(videoGrid),
            Padding = new Thickness(12),
        }, 1);
        grid.Add(panelButtons, 2);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, sliderAlpha); // initial focus on an input, not an action button - a focused button clicks on bare Space
    }

    private void ToggleMaximized()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.VideoPlayerControl?.CloseAndDisposePlayer();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}