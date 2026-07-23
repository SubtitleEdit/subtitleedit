using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustColor;

public class BinaryAdjustColorWindow : Window
{
    public BinaryAdjustColorWindow(BinaryAdjustColorViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.ImageBasedEdit.AdjustColor;
        Width = 700;
        Height = 400;
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

        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(260)),
                new ColumnDefinition(GridLength.Star),
            },
            ColumnSpacing = 10,
        };

        var leftPanel = MakeControlsPanel(vm);
        contentGrid.Add(leftPanel, 0, 0);

        var rightPanel = MakePreviewPanel(vm);
        contentGrid.Add(rightPanel, 0, 1);

        mainGrid.Add(contentGrid, 0, 0);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        mainGrid.Add(buttonPanel, 1, 0);

        Content = mainGrid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static StackPanel MakeControlsPanel(BinaryAdjustColorViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
        };

        var swatchButton = new Button
        {
            Height = 60,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            Padding = new Thickness(0),
            Command = vm.ChooseColorCommand,
            Content = new Border
            {
                CornerRadius = new CornerRadius(4),
                [!Border.BackgroundProperty] = new Binding(nameof(vm.ColorSwatchBrush)),
            },
        };
        panel.Children.Add(swatchButton);

        var infoText = new TextBlock
        {
            Text = Se.Language.Tools.ImageBasedEdit.ColorAdjustmentInfo,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 0),
            FontSize = 11,
            Foreground = Brushes.Gray,
        };
        panel.Children.Add(infoText);

        return panel;
    }

    private static Border MakePreviewPanel(BinaryAdjustColorViewModel vm)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var image = new Image
        {
            Stretch = Stretch.None,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewBitmap)),
        };

        scrollViewer.Content = image;

        // Image based subtitles are light or dark text on a transparent background, both
        // invisible on a matching flat backdrop - use the mid-gray checkerboard (issue #12692).
        var border = UiUtil.MakeBorderForControl(scrollViewer);
        border.Background = UiUtil.GetCheckerboardBrush();
        return border;
    }
}
