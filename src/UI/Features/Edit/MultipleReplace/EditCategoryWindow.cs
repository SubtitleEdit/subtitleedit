using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class EditCategoryWindow : Window
{
    public EditCategoryWindow(EditCategoryViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var labelCategoryName = UiUtil.MakeLabel(Se.Language.Edit.MultipleReplace.CategoryName);
        var textBoxCategoryName = UiUtil.MakeTextBox(300, vm, nameof(vm.CategoryName));
        textBoxCategoryName.KeyDown += (sender, args) =>
        {
            if (args.Key == Key.Enter)
            {
                vm.OkCommand.Execute(null);
                args.Handled = true;
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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

        grid.Add(labelCategoryName, 0, 0);
        grid.Add(textBoxCategoryName, 0, 1);

        grid.Add(buttonPanel, 4, 0, 1, 2);

        Content = grid;

        Activated += delegate { textBoxCategoryName.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;
        Loaded += (_, _) => { Title = vm.Title; };
    }
}
