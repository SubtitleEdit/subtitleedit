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

        grid.Add(MakeChooseColumnView(vm), 0);
        grid.Add(MakeOverwriteView(vm), 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Border MakeChooseColumnView(ColumnPasteViewModel vm)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Main.ChooseColumn),
                UiUtil.MakeRadioButton(Se.Language.General.All, vm,  nameof(vm.ColumnsAll), "column"),
                UiUtil.MakeRadioButton(Se.Language.Main.TimeCodesOnly, vm, nameof(vm.ColumnsTimeCodesOnly), "column"),
                UiUtil.MakeRadioButton(Se.Language.Main.TextOnly, vm, nameof(vm.ColumnsTextOnly), "column"),
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
