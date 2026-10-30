using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
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
        SettingsChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var switchResetAll = EditorChrome.MakeSwitch(nameof(vm.ResetAll), null, vm);

        // The parts can only be picked when not resetting everything
        ToggleSwitch MakePartSwitch(string property)
        {
            var toggle = EditorChrome.MakeSwitch(property, null, vm);
            toggle[!IsEnabledProperty] = new Binding(nameof(vm.ResetAll)) { Converter = InverseBooleanConverter.Instance };
            return toggle;
        }

        var parts = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(20, 0, 0, 0),
            Children =
            {
                SettingsChrome.MakeSwitchRow(MakePartSwitch(nameof(vm.ResetRules)), Se.Language.Options.Settings.ResetRules),
                SettingsChrome.MakeSwitchRow(MakePartSwitch(nameof(vm.ResetAppearance)), Se.Language.Options.Settings.ResetAppearance),
                SettingsChrome.MakeSwitchRow(MakePartSwitch(nameof(vm.ResetWaveform)), Se.Language.Options.Settings.ResetWaveform),
                SettingsChrome.MakeSwitchRow(MakePartSwitch(nameof(vm.ResetWindowPositionAndSize)), Se.Language.Options.Settings.ResetWindowPositionAndSize),
                SettingsChrome.MakeSwitchRow(MakePartSwitch(nameof(vm.ResetSyntaxColoring)), Se.Language.Options.Settings.ResetSyntaxColoring),
                SettingsChrome.MakeSwitchRow(MakePartSwitch(nameof(vm.ResetShortcuts)), Se.Language.Options.Settings.ResetShortcuts),
                SettingsChrome.MakeSwitchRow(MakePartSwitch(nameof(vm.ResetRecentFiles)), Se.Language.Options.Settings.ResetRecentFiles),
                SettingsChrome.MakeSwitchRow(MakePartSwitch(nameof(vm.ResetAutoTranslate)), Se.Language.Options.Settings.ResetAutoTranslate),
            }
        };

        var content = new StackPanel
        {
            Spacing = 10,
            MinWidth = 340,
            Children =
            {
                SettingsChrome.MakeSwitchRow(switchResetAll, Se.Language.Options.Settings.ResetAllSettings),
                parts,
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
        };
        grid.Add(EditorChrome.MakePanel(EditorChrome.MakeEditorSection(Se.Language.Options.Settings.ResetSettingsTitle, content, first: true)), 0);
        grid.Add(SettingsChrome.MakeButtonBar(null, buttonCancel, buttonOk), 1);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, switchResetAll); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }
}
