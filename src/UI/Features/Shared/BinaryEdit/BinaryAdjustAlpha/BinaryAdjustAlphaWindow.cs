using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustAlpha;

public class BinaryAdjustAlphaWindow : Window
{
    public BinaryAdjustAlphaWindow(BinaryAdjustAlphaViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.AdjustAlpha;
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

    private static StackPanel MakeControlsPanel(BinaryAdjustAlphaViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
        };

        // Alpha adjustment slider
        var alphaLabel = new TextBlock
        {
            Text = Se.Language.General.AlphaAdjustment,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        panel.Children.Add(alphaLabel);

        var alphaSlider = new Slider
        {
            Minimum = -255,
            Maximum = 255,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TickFrequency = 10,
            IsSnapToTickEnabled = false,
            [!Slider.ValueProperty] = new Binding(nameof(vm.AlphaAdjustment))
            {
                Mode = BindingMode.TwoWay,
            },
        };
        panel.Children.Add(alphaSlider);

        var alphaValueLabel = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.AlphaAdjustmentDisplay)),
        };
        panel.Children.Add(alphaValueLabel);

        // Threshold slider
        var thresholdLabel = new TextBlock
        {
            Text = Se.Language.General.AphaThreshold,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 20, 0, 5),
        };
        panel.Children.Add(thresholdLabel);

        var thresholdSlider = new Slider
        {
            Minimum = 0,
            Maximum = 255,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TickFrequency = 10,
            IsSnapToTickEnabled = false,
            [!Slider.ValueProperty] = new Binding(nameof(vm.TransparencyThreshold))
            {
                Mode = BindingMode.TwoWay,
            },
        };
        panel.Children.Add(thresholdSlider);

        var thresholdValueLabel = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.TransparencyThresholdDisplay)),
        };
        panel.Children.Add(thresholdValueLabel);

        var thresholdInfo = new TextBlock
        {
            Text = "Pixels with alpha below threshold become fully transparent",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = 10,
            Foreground = Avalonia.Media.Brushes.Gray,
            Margin = new Thickness(0, 2, 0, 0),
        };
        panel.Children.Add(thresholdInfo);

        // Reset button
        var resetButton = UiUtil.MakeButton("Reset to defaults", vm.ResetCommand);
        resetButton.Margin = new Thickness(0, 20, 0, 0);
        resetButton.HorizontalAlignment = HorizontalAlignment.Stretch;
        panel.Children.Add(resetButton);

        // Info text
        var infoText = new TextBlock
        {
            Text = "Alpha adjustment: Add/subtract from alpha channel.\n" +
                   "Preview updates automatically and shows with checkered background to visualize transparency.",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(0, 20, 0, 0),
            FontSize = 11,
            Foreground = Avalonia.Media.Brushes.Gray,
        };
        panel.Children.Add(infoText);

        return panel;
    }

    private static Border MakePreviewPanel(BinaryAdjustAlphaViewModel vm)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        // Canvas with checkered background and image overlay
        var canvas = new Canvas
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var backgroundImage = new Image
        {
            Stretch = Avalonia.Media.Stretch.None,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            [!Image.SourceProperty] = new Binding(nameof(vm.CheckeredBackgroundBitmap)),
        };
        canvas.Children.Add(backgroundImage);

        var previewImage = new Image
        {
            Stretch = Avalonia.Media.Stretch.None,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewBitmap)),
        };
        canvas.Children.Add(previewImage);

        scrollViewer.Content = canvas;

        vm.PreviewImage = previewImage;
        vm.BackgroundImage = backgroundImage;

        return UiUtil.MakeBorderForControl(scrollViewer);
    }
}
