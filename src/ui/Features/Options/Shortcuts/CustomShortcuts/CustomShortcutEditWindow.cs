using Avalonia;
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
        vm.Window = this;
        DataContext = vm;

        var labelName = UiUtil.MakeLabel(Se.Language.General.Name);
        var textBoxName = UiUtil.MakeTextBox(300, vm, nameof(vm.Name));
        var labelActiveIn = UiUtil.MakeLabel(language.ActiveIn);
        var comboBoxActiveIn = UiUtil.MakeComboBox(vm.ActiveInChoices, vm, nameof(vm.SelectedActiveIn));
        comboBoxActiveIn.MinWidth = 200;
        var labelTextBoxKeyHint = new TextBlock
        {
            Text = language.CustomShortcutTextBoxKeyHint,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.7,
            [!IsVisibleProperty] = new Binding(nameof(vm.IsTextBoxKeyHintVisible)),
        };
        var panelName = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnSpacing = 10,
            RowSpacing = 6,
        };
        panelName.Add(labelName, 0, 0);
        panelName.Add(textBoxName, 0, 1);
        panelName.Add(labelActiveIn, 1, 0);
        panelName.Add(comboBoxActiveIn, 1, 1);
        panelName.Add(labelTextBoxKeyHint, 2, 1);

        var labelSteps = UiUtil.MakeLabel(language.CustomShortcutSteps);
        var listBoxSteps = new ListBox
        {
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Steps)) { Mode = BindingMode.OneWay },
            [!Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty] = new Binding(nameof(vm.SelectedStep)) { Mode = BindingMode.TwoWay },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        listBoxSteps.DoubleTapped += (_, _) => vm.EditStepCommand.Execute(null);
        var listBorder = UiUtil.MakeBorderForControlNoPadding(listBoxSteps);

        var buttonAdd = UiUtil.MakeButton(vm.AddStepCommand, IconNames.Plus, Se.Language.General.Add);
        var buttonEdit = UiUtil.MakeButton(vm.EditStepCommand, IconNames.Pencil, Se.Language.General.Edit)
            .WithBindIsEnabled(nameof(vm.IsStepSelected));
        var buttonRemove = UiUtil.MakeButton(vm.RemoveStepCommand, IconNames.Trash, Se.Language.General.Remove)
            .WithBindIsEnabled(nameof(vm.IsStepSelected));
        var buttonUp = UiUtil.MakeButton(vm.MoveStepUpCommand, IconNames.ArrowUpThin, Se.Language.General.MoveUp)
            .WithBindIsEnabled(nameof(vm.IsStepSelected));
        var buttonDown = UiUtil.MakeButton(vm.MoveStepDownCommand, IconNames.ArrowDownThin, Se.Language.General.MoveDown)
            .WithBindIsEnabled(nameof(vm.IsStepSelected));
        var panelStepButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Margin = new Thickness(8, 0, 0, 0),
            Children = { buttonAdd, buttonEdit, buttonRemove, buttonUp, buttonDown },
        };

        var hint = new TextBlock
        {
            Text = language.CustomShortcutStepsHint,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.7,
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 8,
        };

        grid.Add(panelName, 0, 0, 1, 2);
        grid.Add(labelSteps, 1, 0, 1, 2);
        grid.Add(listBorder, 2, 0);
        grid.Add(panelStepButtons, 2, 1);
        grid.Add(hint, 3, 0, 1, 2);
        grid.Add(buttonPanel, 4, 0, 1, 2);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxName); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }
}
