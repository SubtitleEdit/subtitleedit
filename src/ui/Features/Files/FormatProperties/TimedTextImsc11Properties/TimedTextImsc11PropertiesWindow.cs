using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.TimedTextImsc11Properties;

public class TimedTextImsc11PropertiesWindow : Window
{
    public TimedTextImsc11PropertiesWindow(TimedTextImsc11PropertiesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.File.XProperties, "Timed Text IMSC 1.1");
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 450;
        MinHeight = 200;
        vm.Window = this;
        DataContext = vm;

        var labelWidth = 200;

        var labelTitle = UiUtil.MakeLabel("Title").WithMinWidth(labelWidth);
        var textBoxTitle = UiUtil.MakeTextBox(263, vm, nameof(vm.Title));
        var panelTitle = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { labelTitle, textBoxTitle },
        };

        var labelTimeBase = UiUtil.MakeLabel("Time base").WithMinWidth(labelWidth);
        var comboBoxTimeBase = UiUtil.MakeComboBox<string>(vm.TimeBases, vm, nameof(vm.SelectedTimeBase)).WithMinWidth(263);
        var panelTimeBase = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { labelTimeBase, comboBoxTimeBase },
        };

        var labelFrameRate = UiUtil.MakeLabel("Frame rate").WithMinWidth(labelWidth);
        var textBoxFrameRate = UiUtil.MakeTextBox(263, vm, nameof(vm.SelectedFrameRate));
        var panelFrameRate = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { labelFrameRate, textBoxFrameRate },
        };

        var labelFrameRateMultiplier = UiUtil.MakeLabel("Frame rate multiplier").WithMinWidth(labelWidth);
        var textBoxFrameRateMultiplier = UiUtil.MakeTextBox(263, vm, nameof(vm.SelectedFrameRateMultiplier));
        var panelFrameRateMultiplier = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { labelFrameRateMultiplier, textBoxFrameRateMultiplier },
        };

        var labelDropMode = UiUtil.MakeLabel("Drop mode").WithMinWidth(labelWidth);
        var comboBoxDropMode = UiUtil.MakeComboBox<string>(vm.DropModes, vm, nameof(vm.SelectedDropMode)).WithMinWidth(263);
        var panelDropMode = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { labelDropMode, comboBoxDropMode },
        };

        var labelDefaultStyle = UiUtil.MakeLabel("Default style").WithMinWidth(labelWidth);
        var comboBoxDefaultStyle = UiUtil.MakeComboBox<string>(vm.DefaultStyles, vm, nameof(vm.SelectedDefaultStyle)).WithMinWidth(263);
        var panelDefaultStyle = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { labelDefaultStyle, comboBoxDefaultStyle },
        };

        var labelDefaultRegion = UiUtil.MakeLabel("Default region").WithMinWidth(labelWidth);
        var comboBoxDefaultRegion = UiUtil.MakeComboBox<string>(vm.DefaultRegions, vm, nameof(vm.SelectedDefaultRegion)).WithMinWidth(263);
        var panelDefaultRegion = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { labelDefaultRegion, comboBoxDefaultRegion },
        };

        var labelTimeCodeFormat = UiUtil.MakeLabel("Time code format").WithMinWidth(labelWidth);
        var comboBoxTimeCodeFormat = UiUtil.MakeComboBox<string>(vm.TimeCodeFormats, vm, nameof(vm.SelectedTimeCodeFormat)).WithMinWidth(263);
        var panelTimeCodeFormat = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { labelTimeCodeFormat, comboBoxTimeCodeFormat },
        };

        var labelFileExtension = UiUtil.MakeLabel("File extension").WithMinWidth(labelWidth);
        var comboBoxFileExtension = UiUtil.MakeComboBox<string>(vm.FileExtensions, vm, nameof(vm.SelectedFileExtension)).WithMinWidth(263);
        var panelFileExtension = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { labelFileExtension, comboBoxFileExtension },
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

        grid.Add(panelTitle, 0);
        grid.Add(panelTimeBase, 1);
        grid.Add(panelFrameRate, 2);
        grid.Add(panelFrameRateMultiplier, 3);
        grid.Add(panelDropMode, 4);
        grid.Add(panelDefaultStyle, 5);
        grid.Add(panelDefaultRegion, 6);
        grid.Add(panelTimeCodeFormat, 7);
        grid.Add(panelFileExtension, 8);
        grid.Add(buttonPanel, 9);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
