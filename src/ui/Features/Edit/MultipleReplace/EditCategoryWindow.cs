using Avalonia;
using Avalonia.Automation;
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var labelCategoryName = EditorChrome.MakeDimLabel(Se.Language.Edit.MultipleReplace.CategoryName, new Thickness(0));
        var textBoxCategoryName = UiUtil.MakeTextBox(340, vm, nameof(vm.CategoryName));
        textBoxCategoryName.Classes.Add("text");
        AutomationProperties.SetName(textBoxCategoryName, Se.Language.Edit.MultipleReplace.CategoryName);
        textBoxCategoryName.KeyDown += (sender, args) =>
        {
            if (args.Key == Key.Enter)
            {
                vm.OkCommand.Execute(null);
                args.Handled = true;
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        var form = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(16, 14, 16, 16),
            Children =
            {
                labelCategoryName,
                textBoxCategoryName,
            },
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(EditorChrome.MakePanel(form), 0, 0);
        grid.Add(buttonPanel, 1, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, textBoxCategoryName); // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;
        Loaded += (_, _) => { Title = vm.Title; };
    }
}
