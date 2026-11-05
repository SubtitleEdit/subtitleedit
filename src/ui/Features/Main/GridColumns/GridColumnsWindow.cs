using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.GridColumns;

/// <summary>
/// The subtitle grid's columns in the editor look (see <see cref="EditorChrome"/>): the column list
/// in one section with move/reset icon actions in its header, Cancel and OK in the button bar.
/// </summary>
public class GridColumnsWindow : Window
{
    public GridColumnsWindow(GridColumnsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Columns;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);
        vm.Window = this;
        DataContext = vm;

        var listBox = new ListBox
        {
            Width = 320,
            Height = 420,
        };
        AutomationProperties.SetName(listBox, Se.Language.General.Columns);
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding(nameof(GridColumnsViewModel.Columns)));
        listBox.Bind(ListBox.SelectedItemProperty, new Binding(nameof(GridColumnsViewModel.SelectedColumn)) { Mode = BindingMode.TwoWay });
        listBox.ItemTemplate = new FuncDataTemplate<GridColumnDisplay>((_, _) =>
        {
            // The always-shown/content-driven columns (Number, Text, teletext, original)
            // still take part in ordering, but their visibility is not the user's to set.
            var checkBox = new CheckBox { MinHeight = 0, VerticalAlignment = VerticalAlignment.Center };
            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(GridColumnDisplay.IsVisible)) { Mode = BindingMode.TwoWay });
            checkBox.Bind(InputElement.IsEnabledProperty, new Binding(nameof(GridColumnDisplay.CanToggle)));
            // The column name is in a separate text block - name the check box too, or a screen
            // reader announces a bare "check box" (#12087).
            checkBox.Bind(AutomationProperties.NameProperty, new Binding(nameof(GridColumnDisplay.Name)));

            var textBlock = new TextBlock { Margin = new Thickness(2, 0, 0, 0), FontSize = EditorChrome.FieldFontSize, VerticalAlignment = VerticalAlignment.Center };
            textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(GridColumnDisplay.Name)));

            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { checkBox, textBlock },
            };
        }, true);

        var buttonMoveUp = EditorChrome.CreateToolButton("fa-solid fa-arrow-up", Se.Language.General.MoveUp, vm.MoveUpCommand, 28);
        var buttonMoveDown = EditorChrome.CreateToolButton("fa-solid fa-arrow-down", Se.Language.General.MoveDown, vm.MoveDownCommand, 28);
        var buttonReset = EditorChrome.CreateToolButton("fa-solid fa-rotate-left", Se.Language.General.Reset, vm.ResetCommand, 28);
        var panelActions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Children = { buttonMoveUp, buttonMoveDown, EditorChrome.MakeToolbarSeparator(), buttonReset },
        };

        var section = EditorChrome.MakeEditorSection(Se.Language.General.Columns, EditorChrome.MakeCompactListFrame(listBox), panelActions, first: true);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, listBox); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
