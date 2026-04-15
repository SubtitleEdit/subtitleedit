using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Options.Settings.WaveformThemes;

public class WaveformThemesWindow : Window
{
    public WaveformThemesWindow(WaveformThemesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.WaveformColorThemesDotDotDot;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var themeComboBox = new ComboBox
        {
            MinWidth = 200,
            DataContext = vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Themes)),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedTheme))
            {
                Mode = BindingMode.TwoWay,
            },
        };

        var themeRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Options.Settings.WaveformColorTheme),
                themeComboBox,
            },
        };

        var colorGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,8,Auto,30,Auto,8,Auto"),
            RowSpacing = 6,
        };

        var leftColors = new (string label, string propName)[]
        {
            (Se.Language.Options.Settings.WaveformTextColor, nameof(vm.TextColor)),
            (Se.Language.Options.Settings.WaveformColor, nameof(vm.WaveformColor)),
            (Se.Language.Options.Settings.WaveformBackgroundColor, nameof(vm.BackgroundColor)),
            (Se.Language.Options.Settings.WaveformSelectedColor, nameof(vm.SelectedColor)),
            (Se.Language.Options.Settings.WaveformCursorColor, nameof(vm.CursorColor)),
            (Se.Language.Options.Settings.WaveformShotChangeColor, nameof(vm.ShotChangeColor)),
        };

        var rightColors = new (string label, string propName)[]
        {
            (Se.Language.Options.Settings.WaveformParagraphBackgroundColor, nameof(vm.ParagraphBackgroundColor)),
            (Se.Language.Options.Settings.WaveformParagraphSelectedBackgroundColor, nameof(vm.ParagraphSelectedBackgroundColor)),
            (Se.Language.Options.Settings.WaveformParagraphLeftColor, nameof(vm.ParagraphLeftColor)),
            (Se.Language.Options.Settings.WaveformParagraphRightColor, nameof(vm.ParagraphRightColor)),
            (Se.Language.Options.Settings.WaveformFancyHighColor, nameof(vm.FancyHighColor)),
        };

        var maxRows = Math.Max(leftColors.Length, rightColors.Length);
        for (var i = 0; i < maxRows; i++)
        {
            colorGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        for (var i = 0; i < leftColors.Length; i++)
        {
            var (label, propName) = leftColors[i];
            colorGrid.Add(UiUtil.MakeLabel(label), i, 0);
            colorGrid.Add(MakeColorButton(vm, propName), i, 2);
        }

        for (var i = 0; i < rightColors.Length; i++)
        {
            var (label, propName) = rightColors[i];
            colorGrid.Add(UiUtil.MakeLabel(label), i, 4);
            colorGrid.Add(MakeColorButton(vm, propName), i, 6);
        }

        var preview = new WaveformPreviewControl(vm)
        {
            Width = 500,
            Height = 90,
        };

        var buttonLoad = UiUtil.MakeButton(Se.Language.Options.Settings.WaveformLoadThemeDotDotDot, vm.LoadThemeCommand);
        var buttonExport = UiUtil.MakeButton(Se.Language.Options.Settings.WaveformExportThemeDotDotDot, vm.ExportThemeCommand);
        var buttonSaveAs = UiUtil.MakeButton(Se.Language.Options.Settings.WaveformSaveAsCustomTheme, vm.SaveAsCustomThemeCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonLoad, buttonExport, buttonSaveAs, buttonOk, buttonCancel);

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 15,
        };

        mainGrid.Add(themeRow, 0);
        mainGrid.Add(preview, 1);
        mainGrid.Add(colorGrid, 2);
        mainGrid.Add(panelButtons, 3);

        Content = mainGrid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                vm.CancelCommand.Execute(null);
            }
        };
    }

    private static Button MakeColorButton(WaveformThemesViewModel vm, string propertyName)
    {
        var swatch = new Border
        {
            Width = 30,
            Height = 20,
            CornerRadius = new CornerRadius(3),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Gray),
            VerticalAlignment = VerticalAlignment.Center,
        };

        swatch.Bind(Border.BackgroundProperty, new Binding(propertyName)
        {
            Source = vm,
            Converter = new ColorToBrushConverter(),
        });

        var button = new Button
        {
            Content = swatch,
            Padding = new Thickness(4, 2),
            VerticalAlignment = VerticalAlignment.Center,
        };

        button.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(button) is not Window window) return;

            var propInfo = typeof(WaveformThemesViewModel).GetProperty(propertyName);
            var currentColor = propInfo?.GetValue(vm) is Color c ? c : Colors.White;

            var pickerVm = new ColorPickerViewModel();
            pickerVm.Initialize(currentColor);
            var pickerWindow = new ColorPickerWindow(pickerVm);
            await pickerWindow.ShowDialog(window);

            if (pickerVm.OkPressed)
            {
                propInfo?.SetValue(vm, pickerVm.SelectedColor);
            }
        };

        return button;
    }
}
