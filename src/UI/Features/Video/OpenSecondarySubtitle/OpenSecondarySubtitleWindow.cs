using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

public class OpenSecondarySubtitleWindow : Window
{
    public OpenSecondarySubtitleWindow(OpenSecondarySubtitleViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Secondary Subtitle Appearance";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var labelWidth = 100;

        // Color row
        var labelColor = UiUtil.MakeLabel("Color").WithMinWidth(labelWidth);
        var buttonColor = UiUtil.MakeButton("Choose color...", vm.ChooseColorCommand).WithMinWidth(120);
        var colorPreview = new Border
        {
            Width = 30,
            Height = 20,
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Black),
        };
        colorPreview.Bind(Border.BackgroundProperty, new Binding(nameof(vm.SubtitleColor))
        {
            Source = vm,
            Mode = BindingMode.OneWay,
            Converter = new ColorToBrushConverter(),
        });
        var panelColor = UiUtil.MakeHorizontalPanel(labelColor, buttonColor, colorPreview);

        // Font size row
        var labelFontSize = UiUtil.MakeLabel("Font size").WithMinWidth(labelWidth);
        var numericFontSize = UiUtil.MakeNumericUpDownInt(6, 200, 20, 120, vm, nameof(vm.FontSize));
        var panelFontSize = UiUtil.MakeHorizontalPanel(labelFontSize, numericFontSize);

        // Border style row
        var labelBorderStyle = UiUtil.MakeLabel("Border style").WithMinWidth(labelWidth);
        var comboBoxBorderStyle = UiUtil.MakeComboBox(vm.BorderStyles, vm, nameof(vm.SelectedBorderStyle)).WithMinWidth(160);
        var panelBorderStyle = UiUtil.MakeHorizontalPanel(labelBorderStyle, comboBoxBorderStyle);

        // Alignment grid (an7 an8 an9 / an4 an5 an6 / an1 an2 an3)
        var alignmentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto"),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto"),
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        alignmentGrid.Add(UiUtil.MakeLabel("Position"), 0, 0, 1, 3);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn7), "align"), 1, 0);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn8), "align"), 1, 1);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn9), "align"), 1, 2);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn4), "align"), 2, 0);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn5), "align"), 2, 1);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn6), "align"), 2, 2);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn1), "align"), 3, 0);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn2), "align"), 3, 1);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn3), "align"), 3, 2);
        var alignmentBorder = UiUtil.MakeBorderForControl(alignmentGrid);

        var buttonPanel = UiUtil.MakeButtonBar(UiUtil.MakeButtonOk(vm.OkCommand), UiUtil.MakeButtonCancel(vm.CancelCommand));

        var mainStack = new StackPanel
        {
            Margin = UiUtil.MakeWindowMargin(),
            Spacing = 10,
            Children =
            {
                panelColor,
                panelFontSize,
                panelBorderStyle,
                alignmentBorder,
                buttonPanel,
            },
        };

        Content = mainStack;

        Activated += delegate { buttonPanel.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
