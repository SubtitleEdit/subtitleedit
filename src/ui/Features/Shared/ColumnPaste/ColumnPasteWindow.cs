using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.ColumnPaste;

/// <summary>
/// Column paste options, in the editor look (see <see cref="EditorChrome"/>): the column and the
/// overwrite/shift choices side by side, Cancel and OK at the bottom.
/// </summary>
public class ColumnPasteWindow : Window
{
    public ColumnPasteWindow(ColumnPasteViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Main.ColumnPaste;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var sections = EditorChrome.MakeSideBySide(MakeChooseColumnView(vm, out var radioButtonColumnsAll, out var radioButtonColumnsTextOnly), MakeOverwriteView(vm));

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        // Plain text has no time codes, so the only enabled column choice is "text only"
        var initialFocusControl = vm.IsTextOnlySource ? radioButtonColumnsTextOnly : radioButtonColumnsAll;
        UiUtil.FocusOnFirstActivation(this, initialFocusControl); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    private static Border MakeChooseColumnView(ColumnPasteViewModel vm, out RadioButton radioButtonAll, out RadioButton radioButtonTextOnly)
    {
        radioButtonAll = UiUtil.MakeRadioButton(Se.Language.General.All, vm, nameof(vm.ColumnsAll), "column");
        var radioButtonTimeCodesOnly = UiUtil.MakeRadioButton(Se.Language.Main.TimeCodesOnly, vm, nameof(vm.ColumnsTimeCodesOnly), "column");
        radioButtonTextOnly = UiUtil.MakeRadioButton(Se.Language.Main.TextOnly, vm, nameof(vm.ColumnsTextOnly), "column");

        if (vm.IsTextOnlySource)
        {
            // clipboard was plain text - there are no time codes to paste
            radioButtonAll.IsEnabled = false;
            radioButtonTimeCodesOnly.IsEnabled = false;
        }

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                radioButtonAll,
                radioButtonTimeCodesOnly,
                radioButtonTextOnly,
            }
        };

        return EditorChrome.MakeEditorSection(Se.Language.Main.ChooseColumn, stackPanel, first: true);
    }

    private static Border MakeOverwriteView(ColumnPasteViewModel vm)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                UiUtil.MakeRadioButton(Se.Language.Main.OverwriteExistingCells, vm, nameof(vm.ModeOverwrite), "overwrite"),
                UiUtil.MakeRadioButton(Se.Language.Main.ShiftTextCellsDown, vm, nameof(vm.ModeTextDown), "overwrite"),
            }
        };

        return EditorChrome.MakeEditorSection(Se.Language.Main.OverwriteOrShiftCellsDown, stackPanel, first: true);
    }
}
