using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsResetWindow : Window
{
    public SettingsResetWindow(SettingsResetViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.ResetSettingsTitle;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        vm.Window = this;
        DataContext = vm;

        var checkBoxResetAll = new CheckBox
        {
            Content = Se.Language.Options.Settings.ResetAllSettings,
            Margin = new Thickness(0, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ResetAll)) { Mode = BindingMode.TwoWay },
        };

        var checkBoxResetRules = new CheckBox
        {
            Content = Se.Language.Options.Settings.ResetRules,
            Margin = new Thickness(20, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ResetRules)) { Mode = BindingMode.TwoWay },
        }.WithBindEnabled(nameof(vm.ResetAll), new InverseBooleanConverter());

        var checkBoxResetShortcuts = new CheckBox
        {
            Content = Se.Language.Options.Settings.ResetShortcuts,
            Margin = new Thickness(20, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ResetShortcuts)) { Mode = BindingMode.TwoWay },
        }.WithBindEnabled(nameof(vm.ResetAll), new InverseBooleanConverter());

        var checkBoxResetRecentFiles = new CheckBox
        {
            Content = Se.Language.Options.Settings.ResetRecentFiles,
            Margin = new Thickness(20, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ResetRecentFiles)) { Mode = BindingMode.TwoWay },
        }.WithBindEnabled(nameof(vm.ResetAll), new InverseBooleanConverter());

        var checkBoxResetAutoTranslate = new CheckBox
        {
            Content = Se.Language.Options.Settings.ResetAutoTranslate,
            Margin = new Thickness(20, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ResetAutoTranslate)) { Mode = BindingMode.TwoWay },
        }.WithBindEnabled(nameof(vm.ResetAll), new InverseBooleanConverter());

        var checkBoxResetAppearance = new CheckBox
        {
            Content = Se.Language.Options.Settings.ResetAppearance,
            Margin = new Thickness(20, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ResetAppearance)) { Mode = BindingMode.TwoWay },
        }.WithBindEnabled(nameof(vm.ResetAll), new InverseBooleanConverter());

        var checkBoxResetWaveform = new CheckBox
        {
            Content = Se.Language.Options.Settings.ResetWaveform,
            Margin = new Thickness(20, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ResetWaveform)) { Mode = BindingMode.TwoWay },
        }.WithBindEnabled(nameof(vm.ResetAll), new InverseBooleanConverter());

        var checkBoxResetSyntaxColoring = new CheckBox
        {
            Content = Se.Language.Options.Settings.ResetSyntaxColoring,
            Margin = new Thickness(20, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ResetSyntaxColoring)) { Mode = BindingMode.TwoWay },
        }.WithBindEnabled(nameof(vm.ResetAll), new InverseBooleanConverter());

        var checkBoxResetWindowPositionAndSize = new CheckBox
        {
            Content = Se.Language.Options.Settings.ResetWindowPositionAndSize,
            Margin = new Thickness(20, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ResetWindowPositionAndSize)) { Mode = BindingMode.TwoWay },
        }.WithBindEnabled(nameof(vm.ResetAll), new InverseBooleanConverter());


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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(checkBoxResetAll, 0);
        grid.Add(checkBoxResetRules, 1);
        grid.Add(checkBoxResetAppearance, 2);
        grid.Add(checkBoxResetWaveform, 3);
        grid.Add(checkBoxResetWindowPositionAndSize, 4);
        grid.Add(checkBoxResetSyntaxColoring, 5);
        grid.Add(checkBoxResetShortcuts, 6);
        grid.Add(checkBoxResetRecentFiles, 7);
        grid.Add(checkBoxResetAutoTranslate, 8);
        grid.Add(panelButtons, 9);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }
}
