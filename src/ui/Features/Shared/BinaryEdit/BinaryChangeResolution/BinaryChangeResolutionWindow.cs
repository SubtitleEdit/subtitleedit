using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryChangeResolution;

public class BinaryChangeResolutionWindow : Window
{
    public BinaryChangeResolutionWindow(BinaryChangeResolutionViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.VideoResolution;
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

        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(300)),
                new ColumnDefinition(GridLength.Star),
            },
            ColumnSpacing = 10,
        };

        contentGrid.Add(MakeControlsPanel(vm, out var presetComboBox), 0, 0);
        contentGrid.Add(MakePreviewPanel(vm), 0, 1);
        mainGrid.Add(contentGrid, 0, 0);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        mainGrid.Add(UiUtil.MakeButtonBar(buttonOk, buttonCancel), 1, 0);

        Content = mainGrid;

        UiUtil.FocusOnFirstActivation(this, presetComboBox); // an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static StackPanel MakeControlsPanel(BinaryChangeResolutionViewModel vm, out ComboBox presetComboBox)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
        };

        panel.Children.Add(new TextBlock
        {
            Text = Se.Language.General.Resolution,
            FontWeight = Avalonia.Media.FontWeight.Bold,
        });

        presetComboBox = UiUtil.MakeComboBox(vm.Resolutions, vm, nameof(vm.SelectedResolution), null);
        presetComboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        panel.Children.Add(presetComboBox);

        panel.Children.Add(new TextBlock
        {
            Text = Se.Language.General.Width,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 10, 0, 0),
        });
        panel.Children.Add(MakeSizeInput(vm, nameof(vm.NewWidth)));

        panel.Children.Add(new TextBlock
        {
            Text = Se.Language.General.Height,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 10, 0, 0),
        });
        panel.Children.Add(MakeSizeInput(vm, nameof(vm.NewHeight)));

        panel.Children.Add(new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.SizeText)),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(0, 20, 0, 0),
            FontSize = 12,
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
        });

        panel.Children.Add(new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ScaleText)),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = 12,
        });

        return panel;
    }

    private static NumericUpDown MakeSizeInput(BinaryChangeResolutionViewModel vm, string propertyName)
    {
        return new NumericUpDown
        {
            Minimum = 1,
            Maximum = 16384,
            Increment = 2,
            FormatString = "0",
            Width = double.NaN,
            [!NumericUpDown.ValueProperty] = new Binding(propertyName)
            {
                Mode = BindingMode.TwoWay,
                Converter = new NullableIntConverter(),
            },
        };
    }

    private static Border MakePreviewPanel(BinaryChangeResolutionViewModel vm)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = new Image
            {
                Stretch = Avalonia.Media.Stretch.None,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                [!Image.SourceProperty] = new Binding(nameof(vm.PreviewBitmap)),
            },
        };

        // Light or dark text on transparency is invisible on a flat backdrop - checkerboard (issue #12692).
        var border = UiUtil.MakeBorderForControl(scrollViewer);
        border.Background = UiUtil.GetCheckerboardBrush();
        return border;
    }
}
