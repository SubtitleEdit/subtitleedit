using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustBrightness;

public class BinaryAdjustBrightnessWindow : Window
{
    public BinaryAdjustBrightnessWindow(BinaryAdjustBrightnessViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Adjust brightness";
        Width = 800;
        Height = 600;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
            },
            Margin = UiUtil.MakeWindowMargin(),
        };

        // Content area with controls on left and preview on right
        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(300)),
                new ColumnDefinition(GridLength.Star),
            },
            ColumnSpacing = 10,
        };

        // Left side - controls
        var leftPanel = MakeControlsPanel(vm);
        contentGrid.Add(leftPanel, 0, 0);

        // Right side - preview
        var rightPanel = MakePreviewPanel(vm);
        contentGrid.Add(rightPanel, 0, 1);

        mainGrid.Add(contentGrid, 0, 0);

        // Button panel
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        mainGrid.Add(buttonPanel, 1, 0);

        Content = mainGrid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static StackPanel MakeControlsPanel(BinaryAdjustBrightnessViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
        };

        // Brightness slider
        var brightnessLabel = new TextBlock
        {
            Text = "Brightness:",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        panel.Children.Add(brightnessLabel);

        var brightnessSlider = new Slider
        {
            Minimum = -100,
            Maximum = 100,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TickFrequency = 10,
            IsSnapToTickEnabled = false,
            [!Slider.ValueProperty] = new Binding(nameof(vm.Brightness))
            {
                Mode = BindingMode.TwoWay,
            },
        };
        panel.Children.Add(brightnessSlider);

        var brightnessValueLabel = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.BrightnessDisplay)),
        };
        panel.Children.Add(brightnessValueLabel);

        // Contrast slider
        var contrastLabel = new TextBlock
        {
            Text = "Contrast:",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 20, 0, 5),
        };
        panel.Children.Add(contrastLabel);

        var contrastSlider = new Slider
        {
            Minimum = -100,
            Maximum = 100,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TickFrequency = 10,
            IsSnapToTickEnabled = false,
            [!Slider.ValueProperty] = new Binding(nameof(vm.Contrast))
            {
                Mode = BindingMode.TwoWay,
            },
        };
        panel.Children.Add(contrastSlider);

        var contrastValueLabel = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ContrastDisplay)),
        };
        panel.Children.Add(contrastValueLabel);

        // Gamma slider
        var gammaLabel = new TextBlock
        {
            Text = "Gamma:",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 20, 0, 5),
        };
        panel.Children.Add(gammaLabel);

        var gammaSlider = new Slider
        {
            Minimum = 1,
            Maximum = 300,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TickFrequency = 10,
            IsSnapToTickEnabled = false,
            [!Slider.ValueProperty] = new Binding(nameof(vm.Gamma))
            {
                Mode = BindingMode.TwoWay,
            },
        };
        panel.Children.Add(gammaSlider);

        var gammaValueLabel = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.GammaDisplay)),
        };
        panel.Children.Add(gammaValueLabel);

        // Reset button
        var resetButton = UiUtil.MakeButton("Reset to defaults",vm.ResetCommand);
        resetButton.Margin = new Thickness(0, 20, 0, 0);
        resetButton.HorizontalAlignment = HorizontalAlignment.Stretch;
        panel.Children.Add(resetButton);

        // Info text
        var infoText = new TextBlock
        {
            Text = "Adjust the sliders to modify brightness, contrast, and gamma. " +
                   "Preview updates automatically and shows the first selected subtitle.",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(0, 20, 0, 0),
            FontSize = 11,
            Foreground = Avalonia.Media.Brushes.Gray,
        };
        panel.Children.Add(infoText);

        return panel;
    }

    private static Border MakePreviewPanel(BinaryAdjustBrightnessViewModel vm)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var image = new Image
        {
            Stretch = Avalonia.Media.Stretch.None,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewBitmap)),
        };

        scrollViewer.Content = image;

        vm.PreviewImage = image;

        return UiUtil.MakeBorderForControl(scrollViewer);
    }
}
