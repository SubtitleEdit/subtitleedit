using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.Renumber;

public class RenumberWindow : Window
{
    public RenumberWindow(RenumberViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.Renumber.Title;
        CanResize = false;
        Width = 350;
        Height = 180;
        vm.Window = this;
        DataContext = vm;

        var label = UiUtil.MakeLabel(Se.Language.Tools.Renumber.StartFromNumber);
        var numericUpDown = UiUtil.MakeNumericUpDownInt(0, 99999, 1, 150, vm, nameof(vm.StartNumber));

        var panelInput = UiUtil.MakeVerticalPanel(label, numericUpDown);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelInput, 0);
        grid.Add(panelButtons, 2);

        Content = grid;

        Loaded += delegate { numericUpDown.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
