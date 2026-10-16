using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts.CustomShortcuts;

public class CustomShortcutEditWindow : Window
{
    public CustomShortcutEditWindow(CustomShortcutEditViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        var language = Se.Language.Options.Shortcuts;
        Title = string.IsNullOrEmpty(vm.Title) ? language.EditCustomShortcut : vm.Title;
        Width = 640;
        Height = 480;
        MinWidth = 500;
        MinHeight = 360;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        const double labelWidth = 90;

        var textBoxName = EditorChrome.MakeTextField(nameof(vm.Name), Se.Language.General.Name, 300);
        var comboBoxActiveIn = UiUtil.MakeComboBox(vm.ActiveInChoices, vm, nameof(vm.SelectedActiveIn));
        comboBoxActiveIn.MinWidth = 200;
        comboBoxActiveIn.HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(comboBoxActiveIn, language.ActiveIn);
        var labelTextBoxKeyHint = EditorChrome.MakeHint(language.CustomShortcutTextBoxKeyHint);
        labelTextBoxKeyHint[!IsVisibleProperty] = new Binding(nameof(vm.IsTextBoxKeyHintVisible));

        var panelName = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.Name, textBoxName, labelWidth, spacing: 6),
                EditorChrome.MakeLabeledRow(language.ActiveIn, comboBoxActiveIn, labelWidth, spacing: 6),
                EditorChrome.MakeLabeledRow(string.Empty, labelTextBoxKeyHint, labelWidth, spacing: 6),
            },
        };
        var nameSection = EditorChrome.MakeEditorSection(Se.Language.General.General, panelName, first: true);

        var listBoxSteps = new ListBox
        {
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Steps)) { Mode = BindingMode.OneWay },
            [!Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedStep)) { Mode = BindingMode.TwoWay },
            Background = Brushes.Transparent, // the frame's field color shows through
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        listBoxSteps.DoubleTapped += (_, _) => vm.EditStepCommand.Execute(null);
        AutomationProperties.SetName(listBoxSteps, language.CustomShortcutSteps);
        var listBorder = EditorChrome.MakeListFrame(listBoxSteps);

        var buttonAdd = EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.Add, vm.AddStepCommand, 26);
        var buttonEdit = EditorChrome.CreateToolButton("fa-solid fa-pen", Se.Language.General.Edit, vm.EditStepCommand, 26)
            .WithBindIsEnabled(nameof(vm.IsStepSelected));
        var buttonRemove = EditorChrome.CreateToolButton("fa-solid fa-trash-can", Se.Language.General.Remove, vm.RemoveStepCommand, 26)
            .WithBindIsEnabled(nameof(vm.IsStepSelected));
        var buttonUp = EditorChrome.CreateToolButton("fa-solid fa-arrow-up", Se.Language.General.MoveUp, vm.MoveStepUpCommand, 26)
            .WithBindIsEnabled(nameof(vm.IsStepSelected));
        var buttonDown = EditorChrome.CreateToolButton("fa-solid fa-arrow-down", Se.Language.General.MoveDown, vm.MoveStepDownCommand, 26)
            .WithBindIsEnabled(nameof(vm.IsStepSelected));
        var panelStepButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonAdd, buttonEdit, buttonRemove, EditorChrome.MakeToolbarSeparator(), buttonUp, buttonDown },
        };

        var stepsHeader = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(panelStepButtons, Dock.Right);
        stepsHeader.Children.Add(panelStepButtons);
        stepsHeader.Children.Add(EditorChrome.MakeSectionHeader(language.CustomShortcutSteps));

        var hint = EditorChrome.MakeHint(language.CustomShortcutStepsHint);
        hint.Margin = new Thickness(0, 8, 0, 0);

        // The steps fill the window: header, list (shrinks/grows with the window), hint.
        var stepsPanel = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            Margin = new Thickness(14, 12, 14, 14),
        };
        stepsPanel.Add(stepsHeader, 0);
        stepsPanel.Add(listBorder, 1);
        stepsPanel.Add(hint, 2);

        var stepsSection = new Border
        {
            Child = stepsPanel,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
        };
        sections.Add(nameSection, 0);
        sections.Add(stepsSection, 1);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
        };

        grid.Add(EditorChrome.MakePanel(sections), 0);
        grid.Add(buttonPanel, 1);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxName); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }
}
