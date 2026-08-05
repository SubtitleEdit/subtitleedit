using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.ColumnPaste;

public class ColumnPasteWindow : Window
{
    public ColumnPasteWindow(ColumnPasteViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Main.ColumnPaste;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        grid.Add(MakeChooseColumnView(vm, out var radioButtonColumnsAll, out var radioButtonColumnsTextOnly), 0);
        grid.Add(MakeOverwriteView(vm), 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        // Plain text has no time codes, so the only enabled column choice is "text only"
        var initialFocusControl = vm.IsTextOnlySource ? radioButtonColumnsTextOnly : radioButtonColumnsAll;
        Activated += delegate { initialFocusControl.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
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
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Main.ChooseColumn),
                radioButtonAll,
                radioButtonTimeCodesOnly,
                radioButtonTextOnly,
            }
        };

        return UiUtil.MakeBorderForControl(stackPanel);
    }

    private static Border MakeOverwriteView(ColumnPasteViewModel vm)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Main.OverwriteOrShiftCellsDown),
                UiUtil.MakeRadioButton(Se.Language.Main.OverwriteExistingCells, vm, nameof(vm.ModeOverwrite), "overwrite"),
                UiUtil.MakeRadioButton(Se.Language.Main.ShiftTextCellsDown, vm, nameof(vm.ModeTextDown), "overwrite"),

            }
        };

        return UiUtil.MakeBorderForControl(stackPanel);
    }
}
