using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.RosettaProperties;

public class RosettaPropertiesWindow : Window
{
    public RosettaPropertiesWindow(RosettaPropertiesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.RosettaProperties;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        MinHeight = 200;
        vm.Window = this;
        DataContext = vm;

        var labelWidth = 200;
        
        var labelLanguage = UiUtil.MakeLabel(Se.Language.General.Language).WithMinWidth(labelWidth);
        var comboBoxLanguage = UiUtil.MakeComboBox<string>(vm.Languages, vm, nameof(vm.SelectedLanguage)).WithMinWidth(100);
        var panelLangyage = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelLanguage,
                comboBoxLanguage,
            }
        };

        var labelLineHeight = UiUtil.MakeLabel(Se.Language.General.LineHeigth).WithMinWidth(labelWidth);
        var textBoxLineHeight = UiUtil.MakeTextBox(100, vm, nameof(vm.SelectedLineHeight));
        var panelLineHeight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelLineHeight,
                textBoxLineHeight,
            }
        };

        var labelFontSize = UiUtil.MakeLabel(Se.Language.File.RosettaFontSize).WithMinWidth(labelWidth);
        var numericUpDownFontSize = UiUtil.MakeNumericUpDownOneDecimal(1, 100, 120, vm, nameof(vm.SelectedFontSize));
        var panelFontSize = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelFontSize,
                numericUpDownFontSize,
            }
        };

        var buttonImport = UiUtil.MakeButton(Se.Language.General.ImportDotDotDot, vm.ImportCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonImport, buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
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

        grid.Add(panelLangyage, 0);
        grid.Add(panelLineHeight, 1);
        grid.Add(panelFontSize, 2);
        grid.Add(buttonPanel, 5);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
