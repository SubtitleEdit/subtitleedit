using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class EditRuleWindow : Window
{
    public EditRuleWindow(EditRuleViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var labelFindWhat = UiUtil.MakeLabel("Find what");
        var textBoxFindWhat = UiUtil.MakeTextBox(300, vm, nameof(vm.FindWhat));

        var labelReplaceWith = UiUtil.MakeLabel("Replace with");
        var textBoxReplaceWith = UiUtil.MakeTextBox(300, vm, nameof(vm.ReplaceWith));

        var labelDescription = UiUtil.MakeLabel("Description (optional)");
        var textBoxDescription = UiUtil.MakeTextBox(300, vm, nameof(vm.Description));

        var radioButtonRegularExpression = UiUtil.MakeRadioButton("Regular expression", vm, nameof(vm.IsRegularExpression));
        var radioButtonCaseSensitive = UiUtil.MakeRadioButton("Case sensitive", vm, nameof(vm.IsCaseSensitive));
        var radioButtonCaseInsensitive = UiUtil.MakeRadioButton("Case insensitive", vm, nameof(vm.IsCaseInsensitive));
        var panelType = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                radioButtonRegularExpression,
                radioButtonCaseSensitive,
                radioButtonCaseInsensitive
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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

        grid.Add(labelFindWhat, 0, 0);
        grid.Add(textBoxFindWhat, 0, 1);

        grid.Add(labelReplaceWith, 1, 0);
        grid.Add(textBoxReplaceWith, 1, 1);

        grid.Add(labelDescription, 2, 0);
        grid.Add(textBoxDescription, 2, 1);

        grid.Add(panelType, 3, 1);

        grid.Add(buttonPanel, 4, 0, 1, 2);

        Content = grid;
        
        Activated += delegate { textBoxFindWhat.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;
        Loaded += (s, e) => { Title = vm.Title; };
    }
}
