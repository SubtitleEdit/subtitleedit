using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConvertTsSettingsWindow : Window
{
    public BatchConvertTsSettingsWindow(BatchConvertTsSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.BatchConvert.TransportStreamSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var labelInfo = UiUtil.MakeLabel(Se.Language.Tools.BatchConvert.TransportStreamSettingsInfo).WithOpacity(0.7);
        labelInfo.MaxWidth = 520;

        // X position
        var checkBoxOverrideX = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.TransportStreamOverrideXPosition, vm, nameof(vm.OverrideXPosition));
        var labelAlignment = UiUtil.MakeLabel(Se.Language.General.Alignment).WithMarginLeft(25);
        var comboBoxAlignment = UiUtil.MakeComboBox(vm.HorizontalAlignments, vm, nameof(vm.SelectedHorizontalAlignment))
            .WithBindEnabled(nameof(vm.OverrideXPosition));
        var labelHMargin = UiUtil.MakeLabel(Se.Language.File.Export.LeftRightMargin).WithMarginLeft(15);
        var numericHMargin = UiUtil.MakeNumericUpDownInt(0, 100, 5, 110, vm, nameof(vm.HorizontalMarginPercent))
            .WithBindEnabled(nameof(vm.OverrideXPosition));
        var panelX = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { labelAlignment, comboBoxAlignment, labelHMargin, numericHMargin, UiUtil.MakeLabel("%") }
        };

        // Y position
        var checkBoxOverrideY = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.TransportStreamOverrideYPosition, vm, nameof(vm.OverrideYPosition));
        var labelBottomMargin = UiUtil.MakeLabel(Se.Language.Tools.BatchConvert.TransportStreamBottomMargin).WithMarginLeft(25);
        var numericBottomMargin = UiUtil.MakeNumericUpDownInt(0, 100, 5, 110, vm, nameof(vm.BottomMarginPercent))
            .WithBindEnabled(nameof(vm.OverrideYPosition));
        var panelY = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { labelBottomMargin, numericBottomMargin, UiUtil.MakeLabel("%") }
        };

        // Video size
        var checkBoxOverrideSize = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.TransportStreamOverrideVideoSize, vm, nameof(vm.OverrideVideoSize));
        var labelWidth = UiUtil.MakeLabel(Se.Language.General.Width).WithMarginLeft(25);
        var numericWidth = UiUtil.MakeNumericUpDownInt(1, 9999, 1920, 120, vm, nameof(vm.VideoWidth))
            .WithBindEnabled(nameof(vm.OverrideVideoSize));
        var labelHeight = UiUtil.MakeLabel(Se.Language.General.Height).WithMarginLeft(15);
        var numericHeight = UiUtil.MakeNumericUpDownInt(1, 9999, 1080, 120, vm, nameof(vm.VideoHeight))
            .WithBindEnabled(nameof(vm.OverrideVideoSize));
        var buttonGetSizeFromVideo = UiUtil.MakeButton(Se.Language.Tools.BatchConvert.TransportStreamGetSizeFromVideo, vm.GetSizeFromVideoCommand)
            .WithMarginLeft(15);
        var panelSize = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { labelWidth, numericWidth, labelHeight, numericHeight, buttonGetSizeFromVideo }
        };

        // File name ending
        var labelFileNameEnding = UiUtil.MakeLabel(Se.Language.Tools.BatchConvert.TransportStreamFileNameEnding);
        var textBoxFileNameEnding = UiUtil.MakeTextBox(260, vm, nameof(vm.FileNameEnding));
        vm.GetFileNameEndingCaretIndex = () => textBoxFileNameEnding.CaretIndex;
        vm.SetFileNameEndingCaretIndex = index =>
        {
            textBoxFileNameEnding.CaretIndex = index;
            textBoxFileNameEnding.Focus();
        };
        var buttonInsertPlaceholder = new Button
        {
            Content = "{ }",
            VerticalAlignment = VerticalAlignment.Center,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    MakePlaceholderMenuItem(vm, Se.Language.General.TwoLetterLanguageCode, TransportStreamExportSettings.PlaceholderTwoLetter),
                    MakePlaceholderMenuItem(vm, Se.Language.Tools.BatchConvert.TwoLetterLanguageCodeUppercase, TransportStreamExportSettings.PlaceholderTwoLetterUppercase),
                    MakePlaceholderMenuItem(vm, Se.Language.General.ThreeLetterLanguageCode, TransportStreamExportSettings.PlaceholderThreeLetter),
                    MakePlaceholderMenuItem(vm, Se.Language.Tools.BatchConvert.ThreeLetterLanguageCodeUppercase, TransportStreamExportSettings.PlaceholderThreeLetterUppercase),
                }
            },
        }.WithMarginLeft(5);
        ToolTip.SetTip(buttonInsertPlaceholder, Se.Language.Tools.BatchConvert.TransportStreamFileNameEndingInfo);
        var labelSample = UiUtil.MakeLabel(string.Empty).WithMarginLeft(10).WithOpacity(0.7);
        labelSample.Bind(ContentControl.ContentProperty, new Avalonia.Data.Binding(nameof(vm.FileNameEndingSample)) { Source = vm });
        var panelFileNameEnding = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { labelFileNameEnding, textBoxFileNameEnding, buttonInsertPlaceholder, labelSample }
        };

        var checkBoxOnlyTeletext = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.TransportStreamOnlyTeletext, vm, nameof(vm.OnlyTeletext));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 8,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelInfo, 0, 0);
        grid.Add(checkBoxOverrideX, 1, 0);
        grid.Add(panelX, 2, 0);
        grid.Add(checkBoxOverrideY, 3, 0);
        grid.Add(panelY, 4, 0);
        grid.Add(checkBoxOverrideSize, 5, 0);
        grid.Add(panelSize, 6, 0);
        grid.Add(panelFileNameEnding, 7, 0);
        grid.Add(checkBoxOnlyTeletext, 8, 0);
        grid.Add(panelButtons, 9, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, checkBoxOverrideX); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static MenuItem MakePlaceholderMenuItem(BatchConvertTsSettingsViewModel vm, string header, string placeholder)
    {
        return new MenuItem
        {
            Header = header,
            Command = vm.InsertPlaceholderCommand,
            CommandParameter = placeholder,
        };
    }
}
