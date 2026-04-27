using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.TmpegEncXmlProperties;

public class TmpegEncXmlPropertiesWindow : Window
{
    public TmpegEncXmlPropertiesWindow(TmpegEncXmlProperties.TmpegEncXmlPropertiesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.File.XProperties, new TmpegEncXml().Name);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        MinHeight = 200;
        vm.Window = this;
        DataContext = vm;

        var labelWidth = 200;

        var labelFontName = UiUtil.MakeLabel(Se.Language.General.Language).WithMinWidth(labelWidth);
        var comboBoxFontName = UiUtil.MakeComboBox(vm.FontNames, vm, nameof(vm.SelectedFontName));
        var panelFontName = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelFontName,
                comboBoxFontName,
            }
        };

        var labelFontHeight = UiUtil.MakeLabel(Se.Language.General.FontHeight).WithMinWidth(labelWidth);
        var numericUpDownFontHeight = UiUtil.MakeNumericUpDownThreeDecimals(0.04m, 1.0m, 150, vm, nameof(vm.FontHeight));
        numericUpDownFontHeight.Increment = 0.001m;
        var panelFontHeight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelFontHeight,
                numericUpDownFontHeight,
            }
        };

        var labelOffsetX = UiUtil.MakeLabel(Se.Language.General.OffsetX).WithMinWidth(labelWidth);
        var numericUpDownOffsetX = UiUtil.MakeNumericUpDownThreeDecimals(-100.0m, 100.0m, 150, vm, nameof(vm.OffsetX));
        numericUpDownOffsetX.Increment = 0.001m;
        var panelOffsetX = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelOffsetX,
                numericUpDownOffsetX,
            }
        };

        var labelOffsetY = UiUtil.MakeLabel(Se.Language.General.OffsetY).WithMinWidth(labelWidth);
        var numericUpDownOffsetY = UiUtil.MakeNumericUpDownThreeDecimals(-100.0m, 100.0m, 150, vm, nameof(vm.OffsetY));
        numericUpDownOffsetY.Increment = 0.001m;
        var panelOffsetY = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelOffsetY,
                numericUpDownOffsetY,
            }
        };

        var labelCheckBoxIsBold = UiUtil.MakeLabel().WithMinWidth(labelWidth);
        var checkBoxIsBold = UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, nameof(vm.IsFontBold));
        var panelCheckBoxIsBold = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelCheckBoxIsBold,
                checkBoxIsBold,
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelFontName, 0);
        grid.Add(panelFontHeight, 1);
        grid.Add(panelOffsetX, 2);
        grid.Add(panelOffsetY, 3);
        grid.Add(panelCheckBoxIsBold, 4);
        grid.Add(buttonPanel, 5);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
