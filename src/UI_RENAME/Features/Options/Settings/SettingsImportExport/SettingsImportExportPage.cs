using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Options.Settings.SettingsImportExport;

public class SettingsImportExportPage : UserControl
{
    private readonly SettingsImportExportViewModel _vm;

    public SettingsImportExportPage(SettingsImportExportViewModel vm)
    {
        _vm = vm;

        var checkBoxExportImportAll = new CheckBox
        {
            Content = Se.Language.Options.Settings.AllSettings,
            Margin = new Thickness(0, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ExportImportAll)) { Mode = BindingMode.TwoWay },
        };

        var checkBoxExportImportRules = new CheckBox
        {
            Content = Se.Language.General.Rules,
            Margin = new Thickness(20, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ExportImportRules)) { Mode = BindingMode.TwoWay },
            [!CheckBox.IsEnabledProperty] = new Binding(nameof(vm.CanEditRules))
        };

        var checkBoxExportImportAppearance = new CheckBox
        {
            Content = Se.Language.General.Appearance,
            Margin = new Thickness(20, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ExportImportAppearance)) { Mode = BindingMode.TwoWay },
            [!CheckBox.IsEnabledProperty] = new Binding(nameof(vm.CanEditAppearance))
        };

        var checkBoxExportImportWaveform = new CheckBox
        {
            Content = Se.Language.General.WaveformSpectrogram,
            Margin = new Thickness(20, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ExportImportWaveform)) { Mode = BindingMode.TwoWay },
            [!CheckBox.IsEnabledProperty] = new Binding(nameof(vm.CanEditWaveform))
        };

        var checkBoxExportImportSyntaxColoring = new CheckBox
        {
            Content = Se.Language.Options.Settings.SyntaxColoring,
            Margin = new Thickness(20, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ExportImportSyntaxColoring)) { Mode = BindingMode.TwoWay },
        }.WithBindEnabled(nameof(vm.ExportImportAll), new InverseBooleanConverter());

        var checkBoxExportImportShortcuts = new CheckBox
        {
            Content = Se.Language.General.Shortcuts,
            Margin = new Thickness(20, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ExportImportShortcuts)) { Mode = BindingMode.TwoWay },
            [!CheckBox.IsEnabledProperty] = new Binding(nameof(vm.CanEditShortcuts))
        };

        var checkBoxExportImportAutoTranslate = new CheckBox
        {
            Content = Se.Language.General.AutoTranslate,
            Margin = new Thickness(20, 0, 55, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.ExportImportAutoTranslate)) { Mode = BindingMode.TwoWay },
            [!CheckBox.IsEnabledProperty] = new Binding(nameof(vm.CanEditAutoTranslate))
        };

        var checkBoxStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,
            Children =
            {
                checkBoxExportImportAll,
                checkBoxExportImportRules,
                checkBoxExportImportAppearance,
                checkBoxExportImportWaveform,
                checkBoxExportImportSyntaxColoring,
                checkBoxExportImportShortcuts,
                checkBoxExportImportAutoTranslate,
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(_vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(_vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var mainStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(15),
            Spacing = 5,
            Children =
            {
                checkBoxStack,
                buttonBar
            }
        };

        Content = mainStack;
    }
}
