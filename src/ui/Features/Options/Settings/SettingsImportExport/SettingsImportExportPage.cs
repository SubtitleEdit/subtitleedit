using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
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

        ToggleSwitch MakeSwitch(string property, string? isEnabledProperty = null)
        {
            var toggle = EditorChrome.MakeSwitch(property, null, vm);
            if (isEnabledProperty != null)
            {
                toggle[!IsEnabledProperty] = new Binding(isEnabledProperty);
            }

            return toggle;
        }

        var switchExportImportAll = MakeSwitch(nameof(vm.ExportImportAll));

        var switchExportImportSyntaxColoring = MakeSwitch(nameof(vm.ExportImportSyntaxColoring));
        switchExportImportSyntaxColoring[!IsEnabledProperty] = new Binding(nameof(vm.ExportImportAll)) { Converter = InverseBooleanConverter.Instance };

        var parts = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(20, 0, 0, 0),
            Children =
            {
                SettingsChrome.MakeSwitchRow(MakeSwitch(nameof(vm.ExportImportRules), nameof(vm.CanEditRules)), Se.Language.General.Rules),
                SettingsChrome.MakeSwitchRow(MakeSwitch(nameof(vm.ExportImportAppearance), nameof(vm.CanEditAppearance)), Se.Language.General.Appearance),
                SettingsChrome.MakeSwitchRow(MakeSwitch(nameof(vm.ExportImportWaveform), nameof(vm.CanEditWaveform)), Se.Language.General.WaveformSpectrogram),
                SettingsChrome.MakeSwitchRow(switchExportImportSyntaxColoring, Se.Language.General.SyntaxColoring),
                SettingsChrome.MakeSwitchRow(MakeSwitch(nameof(vm.ExportImportShortcuts), nameof(vm.CanEditShortcuts)), Se.Language.General.Shortcuts),
                SettingsChrome.MakeSwitchRow(MakeSwitch(nameof(vm.ExportImportAutoTranslate), nameof(vm.CanEditAutoTranslate)), Se.Language.General.AutoTranslate),
            }
        };

        var content = new StackPanel
        {
            Spacing = 10,
            MinWidth = 300,
            Children =
            {
                SettingsChrome.MakeSwitchRow(switchExportImportAll, Se.Language.Options.Settings.AllSettings),
                parts,
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(_vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(_vm.CancelCommand);

        // "Export..." / "Import..." as the section header, without the dots
        var title = (vm.TitleText ?? string.Empty).TrimEnd('.', '…', ' ');

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
        };
        grid.Add(EditorChrome.MakePanel(EditorChrome.MakeEditorSection(title, content, first: true)), 0);
        grid.Add(SettingsChrome.MakeButtonBar(null, buttonCancel, buttonOk), 1);

        Content = grid;
    }
}
