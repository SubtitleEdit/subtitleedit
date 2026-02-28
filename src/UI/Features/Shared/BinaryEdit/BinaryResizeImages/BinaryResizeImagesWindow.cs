using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryResizeImages;

public class BinaryResizeImagesWindow : Window
{
    public BinaryResizeImagesWindow(BinaryResizeImagesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Resize images";
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

    private static StackPanel MakeControlsPanel(BinaryResizeImagesViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
        };

        // Percentage
        var percentageLabel = new TextBlock
        {
            Text = "Percentage:",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        panel.Children.Add(percentageLabel);

        var percentageUpDown = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 1000,
            Increment = 10,
            FormatString = "0",
            Width = double.NaN,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.Percentage))
            {
                Mode = BindingMode.TwoWay,
                Converter = new NullableIntConverter(),
            },
        };
        panel.Children.Add(percentageUpDown);

        // Image size display
        var imageSizeLabel = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ImageSizeText)),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(0, 20, 0, 0),
            FontSize = 12,
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
        };
        panel.Children.Add(imageSizeLabel);

        // Info text
        var infoText = new TextBlock
        {
            Text = "Enter the percentage to resize images.\n" +
                   "Preview updates automatically.",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(0, 20, 0, 0),
            FontSize = 11,
            Foreground = Avalonia.Media.Brushes.Gray,
        };
        panel.Children.Add(infoText);

        return panel;
    }

    private static Border MakePreviewPanel(BinaryResizeImagesViewModel vm)
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
