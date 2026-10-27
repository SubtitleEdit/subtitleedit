using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts.SurroundWith;

public class SurroundWithWindow : Window
{
    public SurroundWithWindow(SurroundWithViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 400;
        CanResize = false;
        Title = Se.Language.Options.Shortcuts.SurroundWith;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        const double labelWidth = 120;

        var textBoxBefore = EditorChrome.MakeTextField(nameof(vm.Before), Se.Language.General.Before, 200);
        var textBoxAfter = EditorChrome.MakeTextField(nameof(vm.After), Se.Language.General.After, 200);

        var comboBoxBehavior = UiUtil.MakeComboBox(vm.Behaviors, vm, nameof(vm.SelectedBehavior));
        comboBoxBehavior.Width = 300;
        comboBoxBehavior.HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(comboBoxBehavior, Se.Language.Options.Shortcuts.SurroundWithBehavior);

        var comboBoxScope = UiUtil.MakeComboBox(vm.Scopes, vm, nameof(vm.SelectedScope));
        comboBoxScope.Width = 300;
        comboBoxScope.HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(comboBoxScope, Se.Language.Options.Shortcuts.SurroundWithWorksOn);

        var textRows = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.Before, textBoxBefore, labelWidth, spacing: 6),
                EditorChrome.MakeLabeledRow(Se.Language.General.After, textBoxAfter, labelWidth, spacing: 6),
            },
        };
        var optionRows = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.Options.Shortcuts.SurroundWithBehavior, comboBoxBehavior, labelWidth, spacing: 6),
                EditorChrome.MakeLabeledRow(Se.Language.Options.Shortcuts.SurroundWithWorksOn, comboBoxScope, labelWidth, spacing: 6),
            },
        };

        var sections = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.General.Text, textRows, first: true),
                EditorChrome.MakeEditorSection(Se.Language.General.Settings, optionRows),
            },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(EditorChrome.MakePanel(sections), 0);
        grid.Add(buttonPanel, 1);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxBefore); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}
