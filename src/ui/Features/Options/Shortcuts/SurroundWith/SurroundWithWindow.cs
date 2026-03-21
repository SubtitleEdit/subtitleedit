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
        vm.Window = this;
        DataContext = vm;

        var labelBefore = UiUtil.MakeLabel(Se.Language.General.Before);
        var textBoxBefore = UiUtil.MakeTextBox(200, vm, nameof(vm.Before)); 

        var labelAfter = UiUtil.MakeLabel(Se.Language.General.After);   
        var textBoxAfter = UiUtil.MakeTextBox(200, vm, nameof(vm.After));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelBefore, 0);
        grid.Add(textBoxBefore, 0, 1);
        grid.Add(labelAfter, 1);
        grid.Add(textBoxAfter, 1, 1);
        grid.Add(buttonPanel, 2, 0, 1, 2);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (s, e) => vm.OnKeyDown(e);   
    }
}
